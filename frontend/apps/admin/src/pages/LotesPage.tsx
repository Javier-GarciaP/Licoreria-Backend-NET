import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  Input,
  Modal,
  PageHeader,
  Pill,
  Select,
} from '@licoreria/ui';
import type { Lote } from '@licoreria/types';
import { inventarioApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { InventarioTabs } from '../components/InventarioTabs';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatNumber } from '../lib/format';

const esquema = z.object({
  varianteId: z.string().min(1, 'Selecciona la variante'),
  codigo: z.string().min(1, 'Ingresa el código del lote'),
  cantidad: z.coerce.number({ invalid_type_error: 'Cantidad inválida' }).min(0, 'No puede ser negativa'),
  fechaVencimiento: z.string().optional(),
  activo: z.boolean(),
});

type Formulario = z.infer<typeof esquema>;

const VACIO: Formulario = { varianteId: '', codigo: '', cantidad: 0, fechaVencimiento: '', activo: true };

function estadoVencimiento(lote: Lote): { texto: string; tone: 'success' | 'warning' | 'danger' | 'neutral' } {
  if (!lote.fechaVencimiento) return { texto: 'Sin vencimiento', tone: 'neutral' };
  const dias = Math.ceil((new Date(lote.fechaVencimiento).getTime() - Date.now()) / 86_400_000);
  if (dias < 0) return { texto: `Vencido (${Math.abs(dias)} d)`, tone: 'danger' };
  if (dias <= 30) return { texto: `Vence en ${dias} d`, tone: 'warning' };
  return { texto: 'Vigente', tone: 'success' };
}

export function LotesPage() {
  const [editando, setEditando] = useState<Lote | null>(null);
  const [creando, setCreando] = useState(false);
  const [porEliminar, setPorEliminar] = useState<Lote | null>(null);
  const queryClient = useQueryClient();

  const loteForm = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: VACIO });

  const lotes = useQuery({ queryKey: ['lotes'], queryFn: () => inventarioApi.lotes() });
  const stock = useQuery({ queryKey: ['stock'], queryFn: () => inventarioApi.stock() });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['lotes'] });
    queryClient.invalidateQueries({ queryKey: ['stock'] });
  };

  const guardar = useMutation({
    mutationFn: (datos: Formulario) =>
      editando
        ? inventarioApi.editarLote(editando.id, {
            id: editando.id,
            codigo: datos.codigo,
            cantidad: datos.cantidad,
            fechaVencimiento: datos.fechaVencimiento || null,
            activo: datos.activo,
          })
        : inventarioApi.crearLote({
            varianteId: datos.varianteId,
            codigo: datos.codigo,
            cantidad: datos.cantidad,
            fechaVencimiento: datos.fechaVencimiento || null,
          }),
    onSuccess: () => {
      toast.success(editando ? 'Lote actualizado' : 'Lote creado');
      setCreando(false);
      setEditando(null);
      loteForm.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => inventarioApi.eliminarLote(id),
    onSuccess: () => {
      toast.success('Lote eliminado');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const abrirCrear = () => {
    loteForm.reset(VACIO);
    setCreando(true);
  };

  const abrirEditar = (lote: Lote) => {
    loteForm.reset({
      varianteId: lote.varianteId,
      codigo: lote.codigo,
      cantidad: lote.cantidad,
      fechaVencimiento: lote.fechaVencimiento ? lote.fechaVencimiento.slice(0, 10) : '',
      activo: lote.activo,
    });
    setEditando(lote);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Inventario"
        subtitle="Lotes y fechas de vencimiento."
        actions={<Button onClick={abrirCrear}>Nuevo lote</Button>}
      />
      <InventarioTabs />

      <Card>
        <CardHeader>
          <CardTitle>Lotes</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<Lote>
            rows={lotes.data ?? []}
            loading={lotes.isLoading}
            rowKey={(lote) => lote.id}
            empty="Sin lotes registrados."
            columns={[
              { key: 'codigo', header: 'Código', render: (lote) => <span className="text-ink">{lote.codigo}</span> },
              { key: 'sku', header: 'SKU', render: (lote) => lote.sku },
              { key: 'cantidad', header: 'Cantidad', align: 'right', render: (lote) => formatNumber(lote.cantidad) },
              {
                key: 'vencimiento',
                header: 'Vencimiento',
                render: (lote) => (lote.fechaVencimiento ? formatDateTime(lote.fechaVencimiento) : '—'),
              },
              {
                key: 'estado',
                header: 'Estado',
                render: (lote) => {
                  const info = estadoVencimiento(lote);
                  return <Pill tone={info.tone}>{info.texto}</Pill>;
                },
              },
              {
                key: 'activo',
                header: 'Activo',
                render: (lote) => <Pill tone={lote.activo ? 'success' : 'danger'}>{lote.activo ? 'Sí' : 'No'}</Pill>,
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (lote) => (
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="ghost" onClick={() => abrirEditar(lote)}>
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setPorEliminar(lote)}>
                      Eliminar
                    </Button>
                  </div>
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={creando || Boolean(editando)}
        onClose={() => {
          setCreando(false);
          setEditando(null);
        }}
        title={editando ? 'Editar lote' : 'Nuevo lote'}
        footer={
          <>
            <Button
              variant="ghost"
              onClick={() => {
                setCreando(false);
                setEditando(null);
              }}
            >
              Cancelar
            </Button>
            <Button type="submit" form="form-lote" loading={guardar.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-lote" className="flex flex-col gap-3" onSubmit={loteForm.handleSubmit((d) => guardar.mutate(d))} noValidate>
          {!editando && (
            <Select label="Variante" error={loteForm.formState.errors.varianteId?.message} {...loteForm.register('varianteId')}>
              <option value="">Selecciona…</option>
              {stock.data?.items.map((item) => (
                <option key={item.varianteId} value={item.varianteId}>
                  {item.productoNombre} · {item.sku}
                </option>
              ))}
            </Select>
          )}
          <Input label="Código de lote" error={loteForm.formState.errors.codigo?.message} {...loteForm.register('codigo')} />
          <div className="grid grid-cols-2 gap-3">
            <Input label="Cantidad" type="number" error={loteForm.formState.errors.cantidad?.message} {...loteForm.register('cantidad')} />
            <Input label="Vencimiento" type="date" {...loteForm.register('fechaVencimiento')} />
          </div>
          {editando && (
            <label className="flex items-center gap-2 text-sm text-muted">
              <input type="checkbox" {...loteForm.register('activo')} />
              Activo
            </label>
          )}
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar lote"
        description={`¿Seguro que deseas eliminar el lote "${porEliminar?.codigo ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}
