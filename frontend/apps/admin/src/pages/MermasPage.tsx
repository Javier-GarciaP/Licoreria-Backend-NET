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
  PageHeader,
  Pagination,
  Select,
} from '@licoreria/ui';
import type { Merma } from '@licoreria/types';
import { inventarioApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatNumber } from '../lib/format';

const MOTIVOS = ['Danado', 'Partido', 'Vencido'] as const;

const esquema = z.object({
  varianteId: z.string().min(1, 'Selecciona un producto'),
  cantidad: z.coerce.number({ invalid_type_error: 'Ingresa una cantidad' }).positive('Debe ser mayor que 0'),
  motivo: z.enum(MOTIVOS),
  reponerSinCobro: z.boolean(),
});

type Formulario = z.infer<typeof esquema>;

export function MermasPage() {
  const [page, setPage] = useState(1);
  const queryClient = useQueryClient();
  const stock = useQuery({ queryKey: ['stock'], queryFn: () => inventarioApi.stock() });
  const mermas = useQuery({ queryKey: ['mermas', page], queryFn: () => inventarioApi.mermas({ page, pageSize: 10 }) });

  const {
    register,
    handleSubmit,
    reset,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({
    resolver: zodResolver(esquema),
    defaultValues: { varianteId: '', cantidad: 1, motivo: 'Danado', reponerSinCobro: false },
  });

  const registrar = useMutation({
    mutationFn: (datos: Formulario) => inventarioApi.registrarMerma(datos),
    onSuccess: () => {
      toast.success('Merma registrada');
      reset({ varianteId: '', cantidad: 1, motivo: 'Danado', reponerSinCobro: false });
      queryClient.invalidateQueries({ queryKey: ['mermas'] });
      queryClient.invalidateQueries({ queryKey: ['stock'] });
      queryClient.invalidateQueries({ queryKey: ['kardex'] });
    },
    onError: (error) => toast.error('No se pudo registrar la merma', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Mermas y cortesías" subtitle="Registra producto dañado, partido o vencido." />
      <Card>
        <CardHeader>
          <CardTitle>Nuevo registro</CardTitle>
        </CardHeader>
        <CardBody>
          <form className="flex flex-col gap-4" onSubmit={handleSubmit((datos) => registrar.mutate(datos))} noValidate>
            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <Select label="Producto / variante" error={errors.varianteId?.message} {...register('varianteId')}>
                <option value="">Selecciona…</option>
                {stock.data?.items.map((item) => (
                  <option key={item.varianteId} value={item.varianteId}>
                    {item.productoNombre} · {item.sku} ({item.cantidad} disp.)
                  </option>
                ))}
              </Select>

              <Select label="Motivo" error={errors.motivo?.message} {...register('motivo')}>
                {MOTIVOS.map((valor) => (
                  <option key={valor} value={valor}>
                    {valor}
                  </option>
                ))}
              </Select>
            </div>

            <Input
              label="Cantidad"
              type="number"
              error={errors.cantidad?.message}
              {...register('cantidad')}
            />

            <label className="flex items-center gap-2 text-sm text-muted">
              <input type="checkbox" {...register('reponerSinCobro')} />
              Reponer sin cobro (cortesía)
            </label>

            <Button type="submit" loading={isSubmitting || registrar.isPending}>
              Registrar merma
            </Button>
          </form>
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Mermas recientes</CardTitle>
        </CardHeader>
        <CardBody>
          <DataTable<Merma>
            rows={mermas.data?.items ?? []}
            loading={mermas.isLoading}
            rowKey={(merma) => merma.id}
            empty="Sin mermas registradas."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (merma) => formatDateTime(merma.fecha) },
              { key: 'sku', header: 'SKU', render: (merma) => merma.sku },
              { key: 'cantidad', header: 'Cantidad', align: 'right', render: (merma) => formatNumber(merma.cantidad) },
              { key: 'motivo', header: 'Motivo', render: (merma) => merma.motivo },
              {
                key: 'repuesto',
                header: 'Cortesía',
                render: (merma) => (merma.repuesto ? 'Sí' : 'No'),
              },
            ]}
          />
          <Pagination page={page} totalPages={mermas.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>
    </div>
  );
}
