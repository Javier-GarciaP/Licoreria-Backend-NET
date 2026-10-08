import { useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Armchair, ArrowRight, Plus, User } from 'lucide-react';
import { Button, Input, Modal } from '@licoreria/ui';
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

      <div className="app-scroll min-h-0 flex-1 overflow-y-auto pr-1">
        {susCuentas.isLoading || mesas.isLoading ? (
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

            {/* Mesas libres */}
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
