import { useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Card, PageHeader } from '@licoreria/ui';
import type { Cuenta, EstadoItemComanda, Mesa, Venta } from '@licoreria/types';
import { clubApi, cuentasApi, ventasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { useAuth } from '../context/AuthContext';
import { useRealtime } from '../hooks/useRealtime';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { MesasPanel, estadoDeMesa } from '../components/mesonero/MesasPanel';
import { CuentaPanel } from '../components/mesonero/CuentaPanel';
import { ConsumoSheet } from '../components/mesonero/ConsumoSheet';
import { AbonoSheet, type AbonoBody, type AbonoItemSeleccion } from '../components/mesonero/AbonoSheet';
import { DividirSheet } from '../components/mesonero/DividirSheet';
import { CobrarSheet, type CobrarBody } from '../components/mesonero/CobrarSheet';
import { TicketVenta } from '../components/pos/TicketVenta';

export function SalonOperacionPage() {
  const queryClient = useQueryClient();
  const [params, setParams] = useSearchParams();
  const { esAdmin, tienePermiso } = useAuth() as {
    esAdmin: boolean;
    tienePermiso: (clave: string) => boolean;
  };

  const [cuentaActivaId, setCuentaActivaId] = useState<string | null>(params.get('cuenta'));
  const [consumoAbierto, setConsumoAbierto] = useState(false);
  const [abono, setAbono] = useState<{ items?: AbonoItemSeleccion[] } | null>(null);
  const [dividirAbierto, setDividirAbierto] = useState(false);
  const [cobrarAbierto, setCobrarAbierto] = useState(false);
  const [ventaTicket, setVentaTicket] = useState<Venta | null>(null);
  const [mesaLiberar, setMesaLiberar] = useState<Mesa | null>(null);

  const puedeAbonar = esAdmin || tienePermiso('account:abono');
  const puedeCobrar = esAdmin || tienePermiso('account:close');

  const planos = useQuery({ queryKey: ['planos'], queryFn: clubApi.planos });
  const zonas = useQuery({ queryKey: ['zonas'], queryFn: clubApi.zonas });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas() });
  const cuentasAbiertas = useQuery({
    queryKey: ['cuentas', 'abiertas'],
    queryFn: () => cuentasApi.listar({ estado: 'Abierta', pageSize: 100 }),
  });
  const cuentaQuery = useQuery({
    queryKey: ['cuenta', cuentaActivaId],
    queryFn: () => cuentasApi.obtener(cuentaActivaId!),
    enabled: Boolean(cuentaActivaId),
  });
  const metodos = useQuery({ queryKey: ['metodos-pago'], queryFn: ventasApi.metodosPago });

  const invalidarMesas = () => {
    queryClient.invalidateQueries({ queryKey: ['mesas'] });
    queryClient.invalidateQueries({ queryKey: ['cuentas'] });
  };
  const invalidarCuenta = () => {
    queryClient.invalidateQueries({ queryKey: ['cuenta', cuentaActivaId] });
    queryClient.invalidateQueries({ queryKey: ['cuentas'] });
  };

  useRealtime('meseros', {
    'mesa:actualizada': invalidarMesas,
    'comanda:creada': invalidarCuenta,
    'comanda:actualizada': invalidarCuenta,
    'item:actualizado': invalidarCuenta,
  });

  const cuentasPorId = useMemo(() => {
    const mapa: Record<string, Cuenta> = {};
    for (const cuenta of cuentasAbiertas.data?.items ?? []) mapa[cuenta.id] = cuenta;
    if (cuentaQuery.data) mapa[cuentaQuery.data.id] = cuentaQuery.data;
    return mapa;
  }, [cuentasAbiertas.data, cuentaQuery.data]);

  const listaMesas = useMemo(() => mesas.data ?? [], [mesas.data]);
  const mesaActiva = useMemo(
    () => listaMesas.find((mesa) => mesa.cuentaId && mesa.cuentaId === cuentaActivaId),
    [listaMesas, cuentaActivaId],
  );
  const cuentaActiva = cuentaQuery.data ?? null;

  const seleccionarCuenta = (id: string | null) => {
    setCuentaActivaId(id);
    setParams(id ? { cuenta: id } : {}, { replace: true });
  };

  const abrir = useMutation({
    mutationFn: (mesa: Mesa) => cuentasApi.abrir({ nombreMesa: mesa.numero, mesaId: mesa.id }),
    onSuccess: (cuenta) => {
      toast.success('Mesa abierta');
      invalidarMesas();
      seleccionarCuenta(cuenta.id);
    },
    onError: (error) => toast.error('No se pudo abrir la mesa', { description: mensajeDeError(error) }),
  });

  const enviarComanda = useMutation({
    mutationFn: ({ area, items }: { area: 'Barra' | 'Cocina'; items: { varianteId: string; cantidad: number; esCortesia: boolean }[] }) =>
      cuentasApi.agregarComanda(cuentaActivaId!, { area, items }),
    onSuccess: () => {
      toast.success('Comanda enviada');
      setConsumoAbierto(false);
      invalidarCuenta();
    },
    onError: (error) => toast.error('No se pudo enviar la comanda', { description: mensajeDeError(error) }),
  });

  const cambiarEstado = useMutation({
    mutationFn: ({ comandaId, detalleId, estado }: { comandaId: string; detalleId: string; estado: EstadoItemComanda }) =>
      cuentasApi.cambiarEstadoItem(cuentaActivaId!, comandaId, detalleId, estado),
    onSuccess: invalidarCuenta,
    onError: (error) => toast.error('No se pudo actualizar', { description: mensajeDeError(error) }),
  });

  const abonar = useMutation({
    mutationFn: (body: AbonoBody) => cuentasApi.abonar(cuentaActivaId!, body),
    onSuccess: () => {
      toast.success('Abono registrado');
      setAbono(null);
      invalidarCuenta();
    },
    onError: (error) => toast.error('No se pudo abonar', { description: mensajeDeError(error) }),
  });

  const dividir = useMutation({
    mutationFn: (dto: { partes: number } | { montos: number[] }) => cuentasApi.dividir(cuentaActivaId!, dto),
    onSuccess: () => {
      toast.success('Cuenta dividida');
      setDividirAbierto(false);
      invalidarCuenta();
    },
    onError: (error) => toast.error('No se pudo dividir', { description: mensajeDeError(error) }),
  });

  const cerrar = useMutation({
    mutationFn: (body: CobrarBody) => cuentasApi.cerrar(cuentaActivaId!, body),
    onSuccess: (venta) => {
      setCobrarAbierto(false);
      setVentaTicket(venta);
      invalidarMesas();
      seleccionarCuenta(null);
    },
    onError: (error) => toast.error('No se pudo cerrar la cuenta', { description: mensajeDeError(error) }),
  });

  const desalojar = useMutation({
    mutationFn: (mesaId: string) => clubApi.desalojar(mesaId),
    onSuccess: () => {
      toast.success('Mesa liberada');
      setMesaLiberar(null);
      invalidarMesas();
      seleccionarCuenta(null);
    },
    onError: (error) => toast.error('No se pudo liberar', { description: mensajeDeError(error) }),
  });

  const manejarMesa = (mesa: Mesa) => {
    const estado = estadoDeMesa(mesa);
    if (mesa.cuentaId) seleccionarCuenta(mesa.cuentaId);
    else if (estado === 'Libre') abrir.mutate(mesa);
    else toast.info(`Mesa ${mesa.numero} ${estado === 'Reservada' ? 'reservada' : 'en limpieza'}`);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4 lg:h-[calc(100dvh-9rem)] lg:min-h-0">
      <PageHeader
        title="Salón"
        subtitle="Selecciona una mesa para atenderla. Abre, agrega consumo, abona y cobra sin salir."
      />

      <div className="grid grid-cols-1 gap-4 lg:min-h-0 lg:flex-1 lg:items-stretch lg:grid-cols-[minmax(0,1.1fr)_minmax(0,0.9fr)]">
        <Card className="flex min-h-0 flex-col p-4">
          <MesasPanel
            mesas={listaMesas}
            zonas={zonas.data ?? []}
            planos={planos.data ?? []}
            cuentasPorId={cuentasPorId}
            seleccionadaId={mesaActiva?.id ?? null}
            onSeleccionar={manejarMesa}
          />
        </Card>

        <Card className="flex min-h-0 flex-col p-4">
          {cuentaActiva ? (
            <CuentaPanel
              cuenta={cuentaActiva}
              mesa={mesaActiva}
              registrando={cambiarEstado.isPending}
              puedeAbonar={puedeAbonar}
              puedeCobrar={puedeCobrar}
              onAgregarConsumo={() => setConsumoAbierto(true)}
              onAbonar={(detalles) =>
                setAbono({
                  items: detalles.map((detalle) => ({
                    detalleId: detalle.detalle.id,
                    nombre: detalle.detalle.nombre,
                    monto: detalle.pendiente,
                  })),
                })
              }
              onAbonarMonto={() => setAbono({})}
              onDividir={() => setDividirAbierto(true)}
              onCobrar={() => setCobrarAbierto(true)}
              onLiberar={() => mesaActiva && setMesaLiberar(mesaActiva)}
              onCambiarEstado={(comandaId, detalleId, estado) => cambiarEstado.mutate({ comandaId, detalleId, estado })}
              onCancelar={(comandaId, detalleId) => cambiarEstado.mutate({ comandaId, detalleId, estado: 'Cancelado' })}
            />
          ) : (
            <div className="flex h-full flex-col items-center justify-center gap-2 text-center">
              <p className="text-base font-medium text-ink">Selecciona una mesa</p>
              <p className="max-w-xs text-sm text-muted">
                Toca una mesa libre para abrirla, o una ocupada para ver y cobrar su cuenta.
              </p>
            </div>
          )}
        </Card>
      </div>

      {cuentaActiva && (
        <ConsumoSheet
          abierto={consumoAbierto}
          cuentaNombre={mesaActiva ? `Mesa ${mesaActiva.numero}` : cuentaActiva.nombreMesa}
          enviando={enviarComanda.isPending}
          onCerrar={() => setConsumoAbierto(false)}
          onEnviar={(area, items) => enviarComanda.mutate({ area, items })}
        />
      )}

      {cuentaActiva && (
        <AbonoSheet
          abierto={abono !== null}
          cuenta={cuentaActiva}
          items={abono?.items}
          metodos={metodos.data ?? []}
          registrando={abonar.isPending}
          onCerrar={() => setAbono(null)}
          onConfirmar={(body) => abonar.mutate(body)}
        />
      )}

      {cuentaActiva && (
        <DividirSheet
          abierto={dividirAbierto}
          cuenta={cuentaActiva}
          registrando={dividir.isPending}
          onCerrar={() => setDividirAbierto(false)}
          onConfirmar={(dto) => dividir.mutate(dto)}
        />
      )}

      {cuentaActiva && (
        <CobrarSheet
          abierto={cobrarAbierto}
          cuenta={cuentaActiva}
          metodos={metodos.data ?? []}
          registrando={cerrar.isPending}
          onCerrar={() => setCobrarAbierto(false)}
          onConfirmar={(body) => cerrar.mutate(body)}
        />
      )}

      <ConfirmDialog
        open={Boolean(mesaLiberar)}
        title="Liberar mesa"
        description={`¿Liberar la mesa ${mesaLiberar?.numero ?? ''} sin cerrar la cuenta?`}
        confirmLabel="Liberar"
        loading={desalojar.isPending}
        onClose={() => setMesaLiberar(null)}
        onConfirm={() => mesaLiberar && desalojar.mutate(mesaLiberar.id)}
      />

      <TicketVenta venta={ventaTicket} onCerrar={() => setVentaTicket(null)} />
    </div>
  );
}
