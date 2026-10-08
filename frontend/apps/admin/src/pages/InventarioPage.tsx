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
  Pagination,
  Pill,
} from '@licoreria/ui';
import type { StockItem } from '@licoreria/types';
import { inventarioApi } from '@licoreria/api-client';
import { InventarioTabs } from '../components/InventarioTabs';
import { FolderPanel } from '../components/FolderTabs';
import { InlineForm } from '../components/InlineForm';
import { Can } from '../components/Rbac';
import { mensajeDeError } from '../lib/api';
import { formatNumber } from '../lib/format';

const esquemaAjuste = z.object({
  cantidad: z.coerce
    .number({ invalid_type_error: 'Ingresa una cantidad' })
    .refine((valor) => valor !== 0, 'La cantidad no puede ser cero'),
  motivo: z.string().min(3, 'Describe el motivo'),
});

type FormularioAjuste = z.infer<typeof esquemaAjuste>;

function FormularioAjuste({
  item,
  guardando,
  onGuardar,
}: {
  item: StockItem;
  guardando: boolean;
  onGuardar: (datos: FormularioAjuste) => void;
}) {
  const form = useForm<FormularioAjuste>({
    resolver: zodResolver(esquemaAjuste),
    defaultValues: { cantidad: 0, motivo: '' },
  });

  return (
    <form className="flex flex-col gap-3" onSubmit={form.handleSubmit(onGuardar)} noValidate>
      <p className="text-xs text-muted-foreground">
        Disponible actual: <span className="num text-foreground">{formatNumber(item.cantidad)}</span>. Usa valores negativos
        para descontar.
      </p>
      <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
        <Input
          label="Cantidad (+/-)"
          type="number"
          error={form.formState.errors.cantidad?.message}
          {...form.register('cantidad')}
        />
        <Input label="Motivo" error={form.formState.errors.motivo?.message} {...form.register('motivo')} />
      </div>
      <div className="flex justify-end">
        <Button type="submit" loading={guardando}>
          Registrar ajuste
        </Button>
      </div>
    </form>
  );
}

export function InventarioPage() {
  const [page, setPage] = useState(1);
  const [busqueda, setBusqueda] = useState('');
  const [soloBajoMinimo, setSoloBajoMinimo] = useState(false);
  const [ajustando, setAjustando] = useState<StockItem | null>(null);
  const queryClient = useQueryClient();

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
      queryClient.invalidateQueries({ queryKey: ['stock'] });
      queryClient.invalidateQueries({ queryKey: ['kardex'] });
    },
    onError: (error) => toast.error('No se pudo ajustar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <InventarioTabs />

      <div className="flex flex-col">
      <FolderPanel className="flex flex-col gap-4">
        <Card className="border-0 bg-transparent shadow-none">
          <CardHeader>
            <CardTitle>Existencias</CardTitle>
            <div className="flex flex-wrap items-center gap-3">
              <label className="flex items-center gap-2 text-xs text-muted-foreground">
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
              expandirKey={ajustando?.varianteId ?? null}
              expandedRow={
                ajustando
                  ? (item) =>
                      item.varianteId === ajustando.varianteId ? (
                        <InlineForm title={`Ajustar · ${item.productoNombre}`} onCancel={() => setAjustando(null)}>
                          <FormularioAjuste
                            item={item}
                            guardando={ajustar.isPending}
                            onGuardar={(datos) => ajustar.mutate(datos)}
                          />
                        </InlineForm>
                      ) : null
                  : undefined
              }
              columns={[
                {
                  key: 'producto',
                  header: 'Producto',
                  render: (item) => (
                    <div>
                      <p className="text-foreground">{item.productoNombre}</p>
                      <p className="text-xs text-muted-foreground">
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
                    <Can permiso="inventory:write">
                      <Button size="sm" variant="ghost" onClick={() => setAjustando(item)}>
                        Ajustar
                      </Button>
                    </Can>
                  ),
                },
              ]}
            />
            <Pagination page={page} totalPages={stock.data?.totalPages ?? 1} onPageChange={setPage} />
          </CardBody>
        </Card>
      </FolderPanel>
      </div>
    </div>
  );
}