import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Pencil, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  Input,
  Modal,
  Pill,
  Select,
} from '@licoreria/ui';
import type { Mesa, Plano } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';
import { etiquetaForma, FORMAS_MESA, formaElementoDeMesa, planosDeMesa } from '../lib/salon';

const esquemaMesa = z.object({
  numero: z.string().min(1, 'Ingresa el número'),
  capacidad: z.coerce.number().int().min(1, 'Al menos 1'),
  forma: z.enum(FORMAS_MESA),
  activa: z.boolean(),
});

type FormularioMesa = z.infer<typeof esquemaMesa>;

const MESA_VACIA: FormularioMesa = { numero: '', capacidad: 4, forma: 'redonda', activa: true };

/** Gestión de mesas: se listan y editan; se crean desde el editor de planos. */
export function SalonMesasPage() {
  const [mesaEditando, setMesaEditando] = useState<Mesa | null>(null);
  const [mesaEliminar, setMesaEliminar] = useState<Mesa | null>(null);
  const queryClient = useQueryClient();

  const mesaForm = useForm<FormularioMesa>({ resolver: zodResolver(esquemaMesa), defaultValues: MESA_VACIA });

  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas() });
  const planos = useQuery({ queryKey: ['planos'], queryFn: clubApi.planos });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['mesas'] });
    queryClient.invalidateQueries({ queryKey: ['planos'] });
  };

  const syncFormaEnPlanos = (mesa: Mesa) => {
    if (!mesaEditando || mesa.forma === mesaEditando.forma) return;
    const planosMesa = planosDeMesa(planos.data ?? [], mesa.id);
    if (planosMesa.length === 0) return;

    const forma = formaElementoDeMesa(mesa.forma);
    const pendientes = planosMesa.map((plano) => {
      const elementos = plano.elementos.map((e) => (e.mesaId === mesa.id ? { ...e, forma, tipo: 'mesa' } : e));
      return clubApi.actualizarPlano(plano.id, {
        id: plano.id,
        nombre: plano.nombre,
        activo: plano.activo,
        anchoFondo: plano.anchoFondo,
        altoFondo: plano.altoFondo,
        rejilla: plano.rejilla,
        piso: plano.piso,
        elementos,
      });
    });
    Promise.allSettled(pendientes);
  };

  const guardarMesa = useMutation({
    mutationFn: (datos: FormularioMesa) => {
      if (!mesaEditando) throw new Error('Nada que guardar');
      return clubApi.actualizarMesa(mesaEditando.id, {
        id: mesaEditando.id,
        zonaId: mesaEditando.zonaId,
        numero: datos.numero,
        capacidad: datos.capacidad,
        forma: datos.forma,
        posX: mesaEditando.posX,
        posY: mesaEditando.posY,
        ancho: mesaEditando.ancho,
        alto: mesaEditando.alto,
        activa: datos.activa,
      });
    },
    onSuccess: (mesa) => {
      toast.success('Mesa actualizada');
      syncFormaEnPlanos(mesa);
      setMesaEditando(null);
      mesaForm.reset(MESA_VACIA);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminarMesa = useMutation({
    mutationFn: (id: string) => clubApi.eliminarMesa(id),
    onSuccess: () => {
      toast.success('Mesa eliminada');
      setMesaEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const planosPorMesa = useMemo(() => {
    const mapa = new Map<string, Plano[]>();
    for (const mesa of mesas.data ?? []) {
      const relacionados = planosDeMesa(planos.data ?? [], mesa.id);
      if (relacionados.length > 0) mapa.set(mesa.id, relacionados);
    }
    return mapa;
  }, [mesas.data, planos.data]);

  return (
    <div className="flex flex-col gap-6">
      <Card>
        <CardHeader>
          <CardTitle>Mesas</CardTitle>
        </CardHeader>
        <CardBody>
          <p className="mb-3 text-xs text-muted-foreground">
            Las mesas se crean desde el editor de planos: se coloca la figura, se asigna el número y la capacidad. Aquí se edita su información básica.
          </p>
          <DataTable<Mesa>
            rows={mesas.data ?? []}
            loading={mesas.isLoading}
            rowKey={(mesa) => mesa.id}
            empty="No hay mesas. Créalas desde el editor de planos."
            columns={[
              { key: 'numero', header: 'Número', render: (mesa) => <span className="text-foreground">{mesa.numero}</span> },
              {
                key: 'capacidad',
                header: 'Capacidad',
                align: 'center',
                render: (mesa) => <span className="num">{mesa.capacidad}</span>,
              },
              {
                key: 'forma',
                header: 'Forma',
                render: (mesa) => <Pill tone="accent">{etiquetaForma(mesa.forma)}</Pill>,
              },
              {
                key: 'plano',
                header: 'En plano',
                render: (mesa) => {
                  const relacionados = planosPorMesa.get(mesa.id);
                  if (!relacionados) return <span className="text-muted-foreground">Sin ubicar</span>;
                  return <span className="text-xs text-muted-foreground">{relacionados.map((p) => p.nombre).join(', ')}</span>;
                },
              },
              {
                key: 'activa',
                header: 'Estado',
                render: (mesa) => <Pill tone={mesa.activa ? 'success' : 'danger'}>{mesa.activa ? 'Activa' : 'Inactiva'}</Pill>,
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (mesa) => (
                  <ActionMenu
                    label={`Acciones de mesa ${mesa.numero}`}
                    options={[
                      {
                        label: 'Editar',
                        icon: <Pencil size={15} />,
                        onClick: () => {
                          mesaForm.reset({
                            numero: mesa.numero,
                            capacidad: mesa.capacidad,
                            forma: mesa.forma as FormularioMesa['forma'],
                            activa: mesa.activa,
                          });
                          setMesaEditando(mesa);
                        },
                      },
                      { label: 'Eliminar', icon: <Trash2 size={15} />, danger: true, onClick: () => setMesaEliminar(mesa) },
                    ]}
                  />
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={Boolean(mesaEditando)}
        onClose={() => setMesaEditando(null)}
        title="Editar mesa"
        footer={
          <>
            <Button variant="ghost" onClick={() => setMesaEditando(null)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-mesa" loading={guardarMesa.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-mesa" className="flex flex-col gap-3" onSubmit={mesaForm.handleSubmit((d) => guardarMesa.mutate(d))} noValidate>
          <Input label="Número" error={mesaForm.formState.errors.numero?.message} {...mesaForm.register('numero')} />
          <div className="grid grid-cols-2 gap-3">
            <Input label="Capacidad" type="number" error={mesaForm.formState.errors.capacidad?.message} {...mesaForm.register('capacidad')} />
            <Select label="Forma" {...mesaForm.register('forma')}>
              {FORMAS_MESA.map((forma) => (
                <option key={forma} value={forma}>
                  {etiquetaForma(forma)}
                </option>
              ))}
            </Select>
          </div>
          <label className="flex items-center gap-2 text-sm text-muted-foreground">
            <input type="checkbox" {...mesaForm.register('activa')} />
            Activa
          </label>
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(mesaEliminar)}
        title="Eliminar mesa"
        description={`¿Seguro que deseas eliminar la mesa "${mesaEliminar?.numero ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminarMesa.isPending}
        onClose={() => setMesaEliminar(null)}
        onConfirm={() => mesaEliminar && eliminarMesa.mutate(mesaEliminar.id)}
      />
    </div>
  );
}