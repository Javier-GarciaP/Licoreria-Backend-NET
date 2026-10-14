import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Armchair, ArrowRight, HandCoins, Plus, RotateCcw, User } from 'lucide-react';
import { Button, Input, Modal } from '@licoreria/ui';
import type { Cuenta, Mesa } from '@licoreria/types';
import { clubApi, cuentasApi } from '@licoreria/api-client';
import { useAuth } from '../../context/AuthContext';
import { useTurno } from '../../hooks/useTurno';
import { estadoDeMesa } from '../../lib/estadoMesa';
import { formatUSD, haceCuanto } from '../../lib/format';
import { mensajeDeError } from '../../lib/api';

interface AbrirBody {
  nombreMesa: string;
  mesaId?: string;
  cliente?: string;
  notas?: string;
}

export function AtenderView({ onEntrarCarta }: { onEntrarCarta: (cuenta: Cuenta) => void }) {
  const queryClient = useQueryClient();
  const { usuario } = useAuth();
  const { turnoAbierto, cargando: cargandoTurno } = useTurno();
  const [abriendo, setAbriendo] = useState<Mesa | null>(null);
  const [cliente, setCliente] = useState('');
  const [notas, setNotas] = useState('');

  const susCuentas = useQuery({
    queryKey: ['mis-cuentas'],
    queryFn: () => cuentasApi.listar({ estado: 'Abierta', usuarioId: usuario?.usuarioId, pageSize: 100 }),
  });
  const porCobrar = useQuery({
    queryKey: ['mis-cuentas', 'por-cobrar'],
    queryFn: () => cuentasApi.listar({ estado: 'PorCobrar', usuarioId: usuario?.usuarioId, pageSize: 100 }),
  });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas() });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['mis-cuentas'] });
    queryClient.invalidateQueries({ queryKey: ['mesas'] });
  };

  const abrir = useMutation({
    mutationFn: (body: AbrirBody) => cuentasApi.abrir(body),
    onSuccess: (cuenta) => {
      toast.success('Mesa abierta');
      setAbriendo(null);
      setCliente('');
      setNotas('');
      invalidar();
      onEntrarCarta(cuenta);
    },
    onError: (error) => toast.error('No se pudo abrir la mesa', { description: mensajeDeError(error) }),
  });

  const reabrir = useMutation({
    mutationFn: (cuentaId: string) => cuentasApi.reabrir(cuentaId),
    onSuccess: (cuenta) => {
      toast.success('Cuenta retomada');
      invalidar();
      onEntrarCarta(cuenta);
    },
    onError: (error) => toast.error('No se pudo retomar la cuenta', { description: mensajeDeError(error) }),
  });

  const cuentas = useMemo(() => susCuentas.data?.items ?? [], [susCuentas.data]);
  const cuentasPorCobrar = useMemo(() => porCobrar.data?.items ?? [], [porCobrar.data]);

  const mesasOcupadasPorMi = useMemo(() => {
    return (mesas.data ?? []).filter((m) => m.cuentaId && cuentas.some((c) => c.id === m.cuentaId));
  }, [mesas.data, cuentas]);

  const mesasLibres = useMemo(
    () => (mesas.data ?? []).filter((m) => estadoDeMesa(m) === 'Libre'),
    [mesas.data],
  );

  return (
    <div className="flex h-full min-h-0 flex-col gap-4">
      <div>
        <h2 className="text-lg font-medium text-foreground">Atender</h2>
        <p className="text-xs text-muted-foreground">Tus mesas en curso y las disponibles para abrir.</p>
      </div>

      {cargandoTurno ? null : turnoAbierto ? (
        <p className="inline-flex items-center gap-1.5 self-start rounded-full border border-success/30 bg-success/10 px-3 py-1 text-[11px] text-success-fg">
          <span className="h-2 w-2 rounded-full bg-success" />
          Turno abierto
        </p>
      ) : (
        <div className="rounded-xl border border-warning/40 bg-warning/10 px-4 py-3 text-xs text-warning-fg">
          Sin turno abierto. Pide al cajero que abra la caja para poder asignar mesas, enviar comandas y cobrar.
        </div>
      )}

      <div className="app-scroll min-h-0 flex-1 overflow-y-auto pr-1">
        {susCuentas.isLoading || mesas.isLoading || cargandoTurno ? (
          <p className="py-10 text-center text-sm text-muted-foreground">Cargando…</p>
        ) : (
          <div className="flex flex-col gap-6">
            {/* Mesas en curso (solo las que maneja este mesonero) */}
            <section className="flex flex-col gap-2">
              <p className="text-[11px] font-medium uppercase tracking-tighter2 text-muted-foreground">
                En curso ({cuentas.length})
              </p>
              {cuentas.length === 0 ? (
                <div className="rounded-xl border border-dashed border-border px-4 py-8 text-center text-xs text-muted-foreground">
                  No tienes mesas asignadas en este momento.
                </div>
              ) : (
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-3">
                  {cuentas.map((cuenta) => {
                    const mesa = mesasOcupadasPorMi.find((m) => m.cuentaId === cuenta.id);
                    return (
                      <button
                        key={cuenta.id}
                        type="button"
                        onClick={() => onEntrarCarta(cuenta)}
                        className="group flex flex-col gap-2 rounded-xl border border-border bg-card/60 p-4 text-left transition hover:border-primary hover:bg-primary/10"
                      >
                        <div className="flex items-start justify-between gap-2">
                          <div>
                            <p className="text-base font-medium text-foreground">{cuenta.nombreMesa}</p>
                            <p className="text-xs text-muted-foreground">
                              {mesa?.zonaNombre ?? ''} · {haceCuanto(cuenta.abiertaEn)}
                            </p>
                          </div>
                          <span className="flex h-9 w-9 items-center justify-center rounded-full bg-primary/20 text-foreground">
                            <Armchair size={16} />
                          </span>
                        </div>
                        {cuenta.cliente && (
                          <p className="inline-flex items-center gap-1.5 text-xs text-foreground">
                            <User size={13} className="text-muted-foreground" /> {cuenta.cliente}
                          </p>
                        )}
                        <div className="mt-auto flex items-center justify-between pt-1">
                          <span className="num text-sm font-medium text-foreground">{formatUSD(cuenta.saldo)}</span>
                          <ArrowRight size={15} className="text-muted-foreground transition group-hover:text-foreground" />
                        </div>
                      </button>
                    );
                  })}
                </div>
              )}
            </section>

            {/* Cuentas por cobrar retomables */}
            <section className="flex flex-col gap-2">
              <p className="text-[11px] font-medium uppercase tracking-tighter2 text-muted-foreground">
                Por cobrar ({cuentasPorCobrar.length})
              </p>
              {cuentasPorCobrar.length === 0 ? (
                <p className="rounded-xl border border-dashed border-border px-4 py-8 text-center text-xs text-muted-foreground">
                  No tienes cuentas pendientes de cobro.
                </p>
              ) : (
                <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 xl:grid-cols-3">
                  {cuentasPorCobrar.map((cuenta) => (
                    <div
                      key={cuenta.id}
                      className="flex flex-col gap-2 rounded-xl border border-warning/30 bg-warning/5 p-4"
                    >
                      <div className="flex items-start justify-between gap-2">
                        <div className="min-w-0">
                          <p className="truncate text-base font-medium text-foreground">{cuenta.nombreMesa}</p>
                          <p className="text-xs text-muted-foreground">Quedó por cobrar · {haceCuanto(cuenta.abiertaEn)}</p>
                        </div>
                        <span className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-warning/20 text-warning-fg">
                          <HandCoins size={16} />
                        </span>
                      </div>
                      {cuenta.cliente && (
                        <p className="inline-flex items-center gap-1.5 text-xs text-foreground">
                          <User size={13} className="text-muted-foreground" /> {cuenta.cliente}
                        </p>
                      )}
                      <div className="mt-auto flex items-center justify-between gap-2 pt-1">
                        <span className="num text-sm font-medium text-foreground">{formatUSD(cuenta.saldo)}</span>
                        <Button
                          size="sm"
                          loading={reabrir.isPending}
                          disabled={!turnoAbierto}
                          onClick={() => reabrir.mutate(cuenta.id)}
                        >
                          <RotateCcw size={14} /> Retomar
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </section>

            {/* Mesas libres (solo con turno abierto) */}
            {turnoAbierto && (
              <section className="flex flex-col gap-2">
                <p className="text-[11px] font-medium uppercase tracking-tighter2 text-muted-foreground">
                  Mesas libres ({mesasLibres.length})
                </p>
                {mesasLibres.length === 0 ? (
                  <p className="rounded-xl border border-dashed border-border px-4 py-8 text-center text-xs text-muted-foreground">
                    No hay mesas libres.
                  </p>
                ) : (
                  <div className="flex flex-wrap gap-2">
                    {mesasLibres.map((mesa) => (
                      <button
                        key={mesa.id}
                        type="button"
                        onClick={() => {
                          setCliente('');
                          setNotas('');
                          setAbriendo(mesa);
                        }}
                        className="inline-flex items-center gap-2 rounded-full border border-border bg-card/60 px-3 py-2 text-sm text-foreground transition hover:border-primary hover:bg-primary/10"
                      >
                        <Plus size={14} className="text-foreground" />
                        Mesa {mesa.numero}
                        <span className="text-[11px] text-muted-foreground">{mesa.zonaNombre}</span>
                      </button>
                    ))}
                  </div>
                )}
              </section>
            )}
          </div>
        )}
      </div>

      <Modal
        open={Boolean(abriendo)}
        onClose={() => setAbriendo(null)}
        title={`Abrir mesa ${abriendo?.numero ?? ''}`}
        footer={
          <>
            <Button variant="ghost" onClick={() => setAbriendo(null)}>
              Cancelar
            </Button>
            <Button
              loading={abrir.isPending}
              onClick={() =>
                abrir.mutate({
                  nombreMesa: abriendo ? `Mesa ${abriendo.numero}` : '',
                  mesaId: abriendo?.id,
                  cliente: cliente.trim() || undefined,
                  notas: notas.trim() || undefined,
                })
              }
            >
              Abrir y atender
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-3">
          <Input label="Cliente" placeholder="Nombre del cliente (opcional)" value={cliente} onChange={(e) => setCliente(e.target.value)} />
          <label className="flex flex-col gap-1 text-xs text-muted-foreground">
            Anotaciones
            <textarea
              rows={3}
              placeholder="Preferencias, pedidos especiales…"
              value={notas}
              onChange={(e) => setNotas(e.target.value)}
              className="rounded-control border border-border bg-card px-3 py-2 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
            />
          </label>
        </div>
      </Modal>
    </div>
  );
}