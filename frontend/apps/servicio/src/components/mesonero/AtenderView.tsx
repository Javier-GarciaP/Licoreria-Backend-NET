import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Armchair, ArrowRight, Clock, Plus, User, Wallet } from 'lucide-react';
import { Button, cn, Modal, Input } from '@licoreria/ui';
import type { Cuenta, Mesa } from '@licoreria/types';
import { clubApi, cuentasApi } from '@licoreria/api-client';
import { useAuth } from '../../context/AuthContext';
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
  const [abriendo, setAbriendo] = useState<Mesa | null>(null);
  const [cliente, setCliente] = useState('');
  const [notas, setNotas] = useState('');

  const susCuentas = useQuery({
    queryKey: ['mis-cuentas'],
    queryFn: () => cuentasApi.listar({ estado: 'Abierta', usuarioId: usuario?.usuarioId, pageSize: 100 }),
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

  const cuentas = useMemo(() => susCuentas.data?.items ?? [], [susCuentas.data]);

  const mesasOcupadasPorMi = useMemo(() => {
    const ids = new Set(cuentas.map((c) => c.sesionMesaId));
    return (mesas.data ?? []).filter((m) => m.cuentaId && cuentas.some((c) => c.id === m.cuentaId));
  }, [mesas.data, cuentas]);

  const mesasLibres = useMemo(
    () => (mesas.data ?? []).filter((m) => estadoDeMesa(m) === 'Libre'),
    [mesas.data],
  );

  return (
    <div className="flex h-full min-h-0 flex-col gap-4">
      <div>
        <h2 className="text-lg font-medium text-ink">Atender</h2>
        <p className="text-xs text-muted">Tus mesas en curso y las disponibles para abrir.</p>
      </div>

      <div className="app-scroll min-h-0 flex-1 overflow-y-auto pr-1">
        {susCuentas.isLoading || mesas.isLoading ? (
          <p className="py-10 text-center text-sm text-muted">Cargando…</p>
        ) : (
          <div className="flex flex-col gap-6">
            {/* Mesas en curso (solo las que maneja este mesonero) */}
            <section className="flex flex-col gap-2">
              <p className="text-[11px] font-medium uppercase tracking-tighter2 text-muted">
                En curso ({cuentas.length})
              </p>
              {cuentas.length === 0 ? (
                <div className="rounded-card border border-dashed border-hairline px-4 py-8 text-center text-xs text-muted">
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
                        className="group flex flex-col gap-2 rounded-card border border-hairline bg-surface/60 p-4 text-left transition hover:border-accent hover:bg-accent/10"
                      >
                        <div className="flex items-start justify-between gap-2">
                          <div>
                            <p className="text-base font-medium text-ink">{cuenta.nombreMesa}</p>
                            <p className="text-xs text-muted">
                              {mesa?.zonaNombre ?? ''} · {haceCuanto(cuenta.abiertaEn)}
                            </p>
                          </div>
                          <span className="flex h-9 w-9 items-center justify-center rounded-full bg-accent/20 text-accent-ink">
                            <Armchair size={16} />
                          </span>
                        </div>
                        {cuenta.cliente && (
                          <p className="inline-flex items-center gap-1.5 text-xs text-ink">
                            <User size={13} className="text-muted" /> {cuenta.cliente}
                          </p>
                        )}
                        <div className="mt-auto flex items-center justify-between pt-1">
                          <span className="num text-sm font-medium text-accent-ink">{formatUSD(cuenta.saldo)}</span>
                          <ArrowRight size={15} className="text-muted transition group-hover:text-accent-ink" />
                        </div>
                      </button>
                    );
                  })}
                </div>
              )}
            </section>

            {/* Mesas libres */}
            <section className="flex flex-col gap-2">
              <p className="text-[11px] font-medium uppercase tracking-tighter2 text-muted">
                Mesas libres ({mesasLibres.length})
              </p>
              {mesasLibres.length === 0 ? (
                <p className="rounded-card border border-dashed border-hairline px-4 py-8 text-center text-xs text-muted">
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
                      className="inline-flex items-center gap-2 rounded-pill border border-hairline bg-surface/60 px-3 py-2 text-sm text-ink transition hover:border-accent hover:bg-accent/10"
                    >
                      <Plus size={14} className="text-accent-ink" />
                      Mesa {mesa.numero}
                      <span className="text-[11px] text-muted">{mesa.zonaNombre}</span>
                    </button>
                  ))}
                </div>
              )}
            </section>
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
          <label className="flex flex-col gap-1 text-xs text-muted">
            Anotaciones
            <textarea
              rows={3}
              placeholder="Preferencias, pedidos especiales…"
              value={notas}
              onChange={(e) => setNotas(e.target.value)}
              className="rounded-control border border-hairline bg-surface px-3 py-2 text-sm text-ink focus:outline-none focus:ring-2 focus:ring-accent/50"
            />
          </label>
        </div>
      </Modal>
    </div>
  );
}
