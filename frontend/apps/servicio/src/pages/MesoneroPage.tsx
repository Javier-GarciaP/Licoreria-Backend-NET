import { useCallback, useEffect, useRef, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Clock3, LogOut, UserRound } from 'lucide-react';
import type { Cuenta } from '@licoreria/types';
import { cuentasApi } from '@licoreria/api-client';
import { useAuth } from '../context/AuthContext';
import { useRealtime } from '../hooks/useRealtime';
import { Chat } from '../components/Chat';
import { AtenderView } from '../components/mesonero/AtenderView';
import { PosCarta, type Linea } from '../components/mesonero/PosCarta';
import { Notificaciones } from '../components/mesonero/Notificaciones';
import { cargarNotificaciones, guardarNotificaciones, type Notificacion } from '../lib/notificaciones';
import { etiquetaRol } from '../lib/roles';
import { sonidoNotificacion } from '../lib/sonido';

let secuencia = 0;

function horaActual(): string {
  return new Date().toLocaleTimeString('es-VE', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

export function MesoneroPage() {
  const queryClient = useQueryClient();
  const { usuario, logout } = useAuth();
  const [modo, setModo] = useState<'atender' | 'carta'>('atender');
  const [cuentaActiva, setCuentaActiva] = useState<Cuenta | null>(null);
  const [lineasPorCuenta, setLineasPorCuenta] = useState<Record<string, Linea[]>>({});
  const [hora, setHora] = useState(horaActual);
  const [notificaciones, setNotificaciones] = useState<Notificacion[]>(() => cargarNotificaciones(usuario?.usuarioId ?? ''));
  const misCuentasRef = useRef<string[]>([]);
  const nombresMesaRef = useRef<Record<string, string>>({});
  const notificadasRef = useRef<Set<string>>(new Set());

  const susCuentas = useQuery({
    queryKey: ['mis-cuentas'],
    queryFn: () => cuentasApi.listar({ estado: 'Abierta', usuarioId: usuario?.usuarioId, pageSize: 100 }),
  });

  // Persistencia de notificaciones por mesonero: se cargan al entrar (estado inicial
  // perezoso desde localStorage) y se guardan en cada cambio, de modo que sobreviven
  // a refrescar la página o reabrir la app.
  const usuarioId = usuario?.usuarioId;

  useEffect(() => {
    if (!usuarioId) return;
    setNotificaciones(cargarNotificaciones(usuarioId));
    notificadasRef.current.clear();
  }, [usuarioId]);

  useEffect(() => {
    if (!usuarioId) return;
    guardarNotificaciones(usuarioId, notificaciones);
  }, [notificaciones, usuarioId]);

  useEffect(() => {
    const items = susCuentas.data?.items ?? [];
    misCuentasRef.current = items.map((c) => c.id);
    nombresMesaRef.current = Object.fromEntries(items.map((c) => [c.id, c.nombreMesa]));
  }, [susCuentas.data]);

  useEffect(() => {
    const intervalo = setInterval(() => setHora(horaActual), 1000);
    return () => clearInterval(intervalo);
  }, []);

  const invalidar = useCallback(() => {
    queryClient.invalidateQueries({ queryKey: ['mis-cuentas'] });
    queryClient.invalidateQueries({ queryKey: ['mesas'] });
    queryClient.invalidateQueries({ queryKey: ['carta-cuenta'] });
  }, [queryClient]);

  const notificar = useCallback((texto: string, tipo: Notificacion['tipo'] = 'info') => {
    setNotificaciones((prev) =>
      [{ id: `n-${Date.now()}-${secuencia++}`, texto, cuando: Date.now(), leida: false, tipo }, ...prev].slice(0, 50),
    );
    toast.info(texto);
    sonidoNotificacion(tipo === 'exito' ? 'listo' : 'recibido');
  }, []);

  const marcarLeidas = useCallback(() => {
    setNotificaciones((prev) => prev.map((n) => (n.leida ? n : { ...n, leida: true })));
  }, []);

  const descartar = useCallback((id: string) => {
    setNotificaciones((prev) => prev.filter((n) => n.id !== id));
  }, []);

  const limpiar = useCallback(() => setNotificaciones([]), []);

  useRealtime('meseros', {
    'mesa:actualizada': invalidar,
    'turno:abierto': () => queryClient.invalidateQueries({ queryKey: ['turno'] }),
    'turno:cerrado': () => {
      queryClient.invalidateQueries({ queryKey: ['turno'] });
      queryClient.invalidateQueries({ queryKey: ['mesas'] });
      queryClient.invalidateQueries({ queryKey: ['mis-cuentas'] });
    },
    'comanda:creada': invalidar,
    'comanda:actualizada': invalidar,
    'item:actualizado': (payload) => {
      const p = payload as { cuentaId?: string; detalleId?: string; estado?: string; area?: string };
      const cuentaId = p.cuentaId;
      if (cuentaId && misCuentasRef.current.includes(cuentaId)) {
        const clave = `${p.detalleId}-${p.estado}`;
        if (p.estado === 'EnProceso' && !notificadasRef.current.has(clave)) {
          notificadasRef.current.add(clave);
          notificar(`En preparación (${p.area ?? ''}) — mesa ${nombresMesaRef.current[cuentaId] ?? ''}`, 'info');
        } else if (p.estado === 'Preparado' && !notificadasRef.current.has(clave)) {
          notificadasRef.current.add(clave);
          notificar(`Tu pedido está listo (${p.area ?? ''}) — mesa ${nombresMesaRef.current[cuentaId] ?? ''}`, 'exito');
        }
      }
      invalidar();
    },
  });

  const entrarCarta = (cuenta: Cuenta) => {
    setCuentaActiva(cuenta);
    setModo('carta');
  };

  const volver = () => {
    setModo('atender');
    setCuentaActiva(null);
    invalidar();
  };

  const lineasActivas = cuentaActiva ? (lineasPorCuenta[cuentaActiva.id] ?? []) : [];

  const setLineasActivas = (lineas: Linea[]) => {
    if (!cuentaActiva) return;
    setLineasPorCuenta((prev) => ({ ...prev, [cuentaActiva.id]: lineas }));
  };

  return (
    <div className="flex h-dvh flex-col overflow-hidden bg-background p-4 lg:p-6">
      {/* Header */}
      <header className="flex items-center gap-3 border-b border-border pb-4">
        <span className="flex h-10 w-10 items-center justify-center rounded-full bg-primary/20 text-foreground">
          <UserRound size={18} />
        </span>
        <div className="min-w-0">
          <p className="truncate text-sm font-medium text-foreground">{usuario?.nombreCompleto}</p>
          <p className="text-[11px] text-muted-foreground">{etiquetaRol(usuario?.rolDominio)}</p>
        </div>

        <div className="ml-auto flex items-center gap-2">
          <span className="inline-flex items-center gap-1.5 rounded-full border border-border bg-card/60 px-3 py-1.5 num text-xs text-foreground">
            <Clock3 size={14} className="text-foreground" /> {hora}
          </span>

          <Notificaciones
            notificaciones={notificaciones}
            onMarcarLeidas={marcarLeidas}
            onDescartar={descartar}
            onLimpiar={limpiar}
          />

          <button
            type="button"
            aria-label="Salir"
            onClick={() => void logout()}
            className="flex h-10 w-10 items-center justify-center rounded-full border border-border text-muted-foreground transition hover:border-destructive/40 hover:text-destructive-fg"
          >
            <LogOut size={17} />
          </button>
        </div>
      </header>

      {/* Contenido */}
      <div className="min-h-0 flex-1 pt-4">
        {modo === 'atender' ? (
          <AtenderView onEntrarCarta={entrarCarta} />
        ) : (
          cuentaActiva && (
            <PosCarta
              cuenta={cuentaActiva}
              lineas={lineasActivas}
              onLineasChange={setLineasActivas}
              onActualizar={setCuentaActiva}
              onVolver={volver}
            />
          )
        )}
      </div>

      <Chat />
    </div>
  );
}