import { useCallback, useEffect, useRef, useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Bell, Clock3, LogOut, UserRound } from 'lucide-react';
import type { Cuenta } from '@licoreria/types';
import { cuentasApi } from '@licoreria/api-client';
import { useAuth } from '../context/AuthContext';
import { useRealtime } from '../hooks/useRealtime';
import { Chat } from '../components/Chat';
import { AtenderView } from '../components/mesonero/AtenderView';
import { PosCarta } from '../components/mesonero/PosCarta';
import { etiquetaRol } from '../lib/roles';
import { sonidoNotificacion } from '../lib/sonido';

interface Notificacion {
  id: string;
  texto: string;
}

let secuencia = 0;

function horaActual(): string {
  return new Date().toLocaleTimeString('es-VE', { hour: '2-digit', minute: '2-digit', second: '2-digit' });
}

export function MesoneroPage() {
  const queryClient = useQueryClient();
  const { usuario, logout } = useAuth();
  const [modo, setModo] = useState<'atender' | 'carta'>('atender');
  const [cuentaActiva, setCuentaActiva] = useState<Cuenta | null>(null);
  const [hora, setHora] = useState(horaActual);
  const [notificaciones, setNotificaciones] = useState<Notificacion[]>([]);
  const misCuentasRef = useRef<string[]>([]);

  const susCuentas = useQuery({
    queryKey: ['mis-cuentas'],
    queryFn: () => cuentasApi.listar({ estado: 'Abierta', usuarioId: usuario?.usuarioId, pageSize: 100 }),
  });

  useEffect(() => {
    misCuentasRef.current = (susCuentas.data?.items ?? []).map((c) => c.id);
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

  const notificar = useCallback((texto: string) => {
    setNotificaciones((prev) => [{ id: `n-${Date.now()}-${secuencia++}`, texto }, ...prev].slice(0, 20));
    toast.info(texto);
    sonidoNotificacion();
  }, []);

  useRealtime('meseros', {
    'mesa:actualizada': invalidar,
    'comanda:creada': invalidar,
    'comanda:actualizada': invalidar,
    'item:actualizado': (payload) => {
      const p = payload as { cuentaId?: string; estado?: string; area?: string };
      if (p.estado === 'Preparado' && p.cuentaId && misCuentasRef.current.includes(p.cuentaId)) {
        notificar(`Tu pedido está listo (${p.area ?? ''})`);
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

  return (
    <div className="flex min-h-dvh flex-col bg-background p-4 lg:p-6">
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

          <button
            type="button"
            aria-label={`Notificaciones (${notificaciones.length})`}
            onClick={() => setNotificaciones([])}
            className="relative flex h-10 w-10 items-center justify-center rounded-full border border-border text-foreground transition hover:bg-accent/10"
          >
            <Bell size={17} />
            {notificaciones.length > 0 && (
              <span className="absolute -right-0.5 -top-0.5 flex h-5 min-w-5 items-center justify-center rounded-full bg-danger px-1 num text-[10px] text-primary-foreground">
                {notificaciones.length}
              </span>
            )}
          </button>

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

      {/* Notificaciones desplegables */}
      {notificaciones.length > 0 && (
        <div className="absolute right-6 top-16 z-40 flex w-72 flex-col gap-1 rounded-lg border border-border bg-card/95 p-2 shadow-card backdrop-blur">
          {notificaciones.map((n) => (
            <p key={n.id} className="rounded-inner bg-primary/10 px-3 py-2 text-xs text-foreground">
              {n.texto}
            </p>
          ))}
        </div>
      )}

      {/* Contenido */}
      <div className="min-h-0 flex-1 pt-4">
        {modo === 'atender' ? (
          <AtenderView onEntrarCarta={entrarCarta} />
        ) : (
          cuentaActiva && <PosCarta cuenta={cuentaActiva} onActualizar={setCuentaActiva} onVolver={volver} />
        )}
      </div>

      <Chat />
    </div>
  );
}
