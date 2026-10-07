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
  Select,
} from '@licoreria/ui';
import type { MovimientoTesoreria } from '@licoreria/types';
import { finanzasApi } from '@licoreria/api-client';
import { FinanzasTabs } from '../components/FinanzasTabs';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

const esquema = z.object({
  tipo: z.enum(['Ingreso', 'Egreso']),
  monto: z.coerce.number({ invalid_type_error: 'Ingresa el monto' }).positive('Debe ser mayor que 0'),
  moneda: z.enum(['USD', 'BS']),
  motivo: z.string().min(3, 'Describe el motivo'),
});

type Formulario = z.infer<typeof esquema>;

export function TesoreriaPage() {
  const [page, setPage] = useState(1);
  const [tipo, setTipo] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');
  const [registrando, setRegistrando] = useState(false);
  const queryClient = useQueryClient();

  const form = useForm<Formulario>({
    resolver: zodResolver(esquema),
    defaultValues: { tipo: 'Ingreso', monto: 0, moneda: 'USD', motivo: '' },
  });

  const movimientos = useQuery({
    queryKey: ['tesoreria', page, tipo, desde, hasta],
    queryFn: () =>
      finanzasApi.movimientos({
        page,
        pageSize: 15,
        tipo: tipo || undefined,
        desde: desde || undefined,
        hasta: hasta || undefined,
      }),
  });

  const registrar = useMutation({
    mutationFn: (datos: Formulario) => finanzasApi.registrarMovimiento(datos),
    onSuccess: () => {
      toast.success('Movimiento registrado');
      setRegistrando(false);
      form.reset({ tipo: 'Ingreso', monto: 0, moneda: 'USD', motivo: '' });
      queryClient.invalidateQueries({ queryKey: ['tesoreria'] });
    },
    onError: (error) => toast.error('No se pudo registrar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Finanzas"
        subtitle="Movimientos de tesorería."
        actions={<Button onClick={() => setRegistrando(true)}>Nuevo movimiento</Button>}
      />
      <FinanzasTabs />

      <Card>
        <CardHeader>
          <CardTitle>Movimientos</CardTitle>
          <div className="flex flex-wrap items-end gap-2">
            <Select aria-label="Tipo" value={tipo} onChange={(evento) => { setTipo(evento.target.value); setPage(1); }}>
              <option value="">Todos</option>
              <option value="Ingreso">Ingreso</option>
              <option value="Egreso">Egreso</option>
            </Select>
            <div className="w-40">
              <Input label="Desde" type="date" value={desde} onChange={(evento) => { setDesde(evento.target.value); setPage(1); }} />
            </div>
            <div className="w-40">
              <Input label="Hasta" type="date" value={hasta} onChange={(evento) => { setHasta(evento.target.value); setPage(1); }} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<MovimientoTesoreria>
            rows={movimientos.data?.items ?? []}
            loading={movimientos.isLoading}
            rowKey={(movimiento) => movimiento.id}
            empty="Sin movimientos."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (movimiento) => formatDateTime(movimiento.fecha) },
              {
                key: 'tipo',
                header: 'Tipo',
                render: (movimiento) => (
                  <Pill tone={movimiento.tipo === 'Ingreso' ? 'success' : 'danger'}>{movimiento.tipo}</Pill>
                ),
              },
              { key: 'motivo', header: 'Motivo', render: (movimiento) => movimiento.motivo },
              {
                key: 'monto',
                header: 'Monto',
                align: 'right',
                render: (movimiento) =>
                  movimiento.moneda === 'USD' ? formatUSD(movimiento.monto) : `Bs ${movimiento.monto.toFixed(2)}`,
              },
            ]}
          />
          <Pagination page={page} totalPages={movimientos.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={registrando}
        onClose={() => setRegistrando(false)}
        title="Nuevo movimiento de tesorería"
        footer={
          <>
            <Button variant="ghost" onClick={() => setRegistrando(false)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-tesoreria" loading={registrar.isPending}>
              Registrar
            </Button>
          </>
        }
      >
        <form id="form-tesoreria" className="flex flex-col gap-3" onSubmit={form.handleSubmit((d) => registrar.mutate(d))} noValidate>
          <div className="grid grid-cols-2 gap-3">
            <Select label="Tipo" {...form.register('tipo')}>
              <option value="Ingreso">Ingreso</option>
              <option value="Egreso">Egreso</option>
            </Select>
            <Select label="Moneda" {...form.register('moneda')}>
              <option value="USD">USD</option>
              <option value="BS">Bs</option>
            </Select>
          </div>
          <Input label="Monto" type="number" error={form.formState.errors.monto?.message} {...form.register('monto')} />
          <Input label="Motivo" error={form.formState.errors.motivo?.message} {...form.register('motivo')} />
        </form>
      </Modal>
    </div>
  );
}
