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
  Pagination,
  Pill,
} from '@licoreria/ui';
import type { StockItem } from '@licoreria/types';
import { inventarioApi } from '@licoreria/api-client';
import { InventarioTabs } from '../components/InventarioTabs';
import { mensajeDeError } from '../lib/api';
import { formatNumber } from '../lib/format';

const esquemaAjuste = z.object({
  cantidad: z.coerce
    .number({ invalid_type_error: 'Ingresa una cantidad' })
    .refine((valor) => valor !== 0, 'La cantidad no puede ser cero'),
  motivo: z.string().min(3, 'Describe el motivo'),
});

type FormularioAjuste = z.infer<typeof esquemaAjuste>;

export function InventarioPage() {
  const [page, setPage] = useState(1);
  const [busqueda, setBusqueda] = useState('');
  const [soloBajoMinimo, setSoloBajoMinimo] = useState(false);
  const [ajustando, setAjustando] = useState<StockItem | null>(null);
  const queryClient = useQueryClient();

  const ajusteForm = useForm<FormularioAjuste>({
    resolver: zodResolver(esquemaAjuste),
    defaultValues: { cantidad: 0, motivo: '' },
  });

  const stock = useQuery({
    queryKey: ['stock', page, busqueda, soloBajoMinimo],
    queryFn: () => inventarioApi.stock({ page, pageSize: 15, busqueda, soloBajoMinimo }),
  });

  const ajustar = useMutation({
    mutationFn: (datos: FormularioAjuste) =>
      inventarioApi.ajustar({ varianteId: ajustando!.varianteId, cantidad: datos.cantidad, motivo: datos.motivo }),
    onSuccess: () => {
      toast.success('Ajuste registrado');
      setAjustando(null);
      ajusteForm.reset({ cantidad: 0, motivo: '' });
      queryClient.invalidateQueries({ queryKey: ['stock'] });
      queryClient.invalidateQueries({ queryKey: ['kardex'] });
    },
    onError: (error) => toast.error('No se pudo ajustar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Inventario" subtitle="Existencias por variante y ajustes autorizados." />
      <InventarioTabs />

      <Card>
        <CardHeader>
          <CardTitle>Existencias</CardTitle>
          <div className="flex flex-wrap items-center gap-3">
            <label className="flex items-center gap-2 text-xs text-muted">
              <input
                type="checkbox"
                checked={soloBajoMinimo}
                onChange={(evento) => {
                  setSoloBajoMinimo(evento.target.checked);
                  setPage(1);
                }}
              />
              Solo bajo mínimo
            </label>
            <div className="w-56">
              <Input
                placeholder="Buscar producto o SKU…"
                value={busqueda}
                onChange={(evento) => {
                  setBusqueda(evento.target.value);
                  setPage(1);
                }}
              />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<StockItem>
            rows={stock.data?.items ?? []}
            loading={stock.isLoading}
            rowKey={(item) => item.varianteId}
            empty="Sin existencias."
            columns={[
              {
                key: 'producto',
                header: 'Producto',
                render: (item) => (
                  <div>
                    <p className="text-ink">{item.productoNombre}</p>
                    <p className="text-xs text-muted">
                      {item.varianteNombre} · {item.sku}
                    </p>
                  </div>
                ),
              },
              { key: 'cantidad', header: 'Disponible', align: 'right', render: (item) => formatNumber(item.cantidad) },
              { key: 'reservada', header: 'Reservada', align: 'right', render: (item) => formatNumber(item.cantidadReservada) },
              { key: 'minimo', header: 'Mín', align: 'right', render: (item) => formatNumber(item.stockMinimo) },
              { key: 'maximo', header: 'Máx', align: 'right', render: (item) => formatNumber(item.stockMaximo) },
              {
                key: 'estado',
                header: 'Estado',
                render: (item) =>
                  item.bajoMinimo ? <Pill tone="danger">Bajo mínimo</Pill> : <Pill tone="success">OK</Pill>,
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (item) => (
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={() => {
                      ajusteForm.reset({ cantidad: 0, motivo: '' });
                      setAjustando(item);
                    }}
                  >
                    Ajustar
                  </Button>
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={stock.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={Boolean(ajustando)}
        onClose={() => setAjustando(null)}
        title={`Ajustar · ${ajustando?.productoNombre ?? ''}`}
        footer={
          <>
            <Button variant="ghost" onClick={() => setAjustando(null)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-ajuste" loading={ajustar.isPending}>
              Registrar ajuste
            </Button>
          </>
        }
      >
        <form
          id="form-ajuste"
          className="flex flex-col gap-3"
          onSubmit={ajusteForm.handleSubmit((datos) => ajustar.mutate(datos))}
          noValidate
        >
          <p className="text-xs text-muted">
            Disponible actual: <span className="text-ink">{formatNumber(ajustando?.cantidad ?? 0)}</span>. Usa valores
            negativos para descontar.
          </p>
          <Input
            label="Cantidad (+/-)"
            type="number"
            error={ajusteForm.formState.errors.cantidad?.message}
            {...ajusteForm.register('cantidad')}
          />
          <Input label="Motivo" error={ajusteForm.formState.errors.motivo?.message} {...ajusteForm.register('motivo')} />
        </form>
      </Modal>
    </div>
  );
}
