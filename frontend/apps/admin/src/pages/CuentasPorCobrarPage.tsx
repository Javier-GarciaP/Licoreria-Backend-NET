import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Wallet } from 'lucide-react';
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
  PageHeader,
  Select,
  StatusBadge,
} from '@licoreria/ui';
import type { CuentaPorCobrar } from '@licoreria/types';
import { clientesApi, cuentasPorCobrarApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatUSD } from '../lib/format';

const esquemaCrear = z.object({
  clienteId: z.string().min(1, 'Selecciona el cliente'),
  montoUSD: z.coerce.number().positive('Debe ser mayor que 0'),
  vencimiento: z.string().min(1, 'Selecciona el vencimiento'),
});

const esquemaPago = z.object({
  monto: z.coerce.number().positive('Debe ser mayor que 0'),
});

export function CuentasPorCobrarPage() {
  const queryClient = useQueryClient();
  const [creando, setCreando] = useState(false);
  const [pagando, setPagando] = useState<CuentaPorCobrar | null>(null);

  const cuentas = useQuery({
    queryKey: ['cuentas-por-cobrar'],
    queryFn: () => cuentasPorCobrarApi.listar({ soloPendientes: true, pageSize: 100 }),
  });
  const clientes = useQuery({ queryKey: ['clientes'], queryFn: () => clientesApi.listar({ pageSize: 100 }) });

  const crearForm = useForm({ resolver: zodResolver(esquemaCrear), defaultValues: { clienteId: '', montoUSD: 0, vencimiento: '' } });
  const pagoForm = useForm({ resolver: zodResolver(esquemaPago), defaultValues: { monto: 0 } });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['cuentas-por-cobrar'] });

  const crear = useMutation({
    mutationFn: (datos: z.infer<typeof esquemaCrear>) => cuentasPorCobrarApi.crear(datos),
    onSuccess: () => {
      toast.success('Cuenta por cobrar registrada');
      setCreando(false);
      crearForm.reset();
      invalidar();
    },
    onError: (error) => toast.error('No se pudo registrar', { description: mensajeDeError(error) }),
  });

  const pagar = useMutation({
    mutationFn: ({ id, monto }: { id: string; monto: number }) => cuentasPorCobrarApi.registrarPago(id, { monto }),
    onSuccess: () => {
      toast.success('Pago registrado');
      setPagando(null);
      pagoForm.reset();
      invalidar();
    },
    onError: (error) => toast.error('No se pudo registrar el pago', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Cuentas por cobrar"
        subtitle="Créditos de clientes pendientes de cobro."
        actions={<Button onClick={() => setCreando(true)}>Nueva cuenta</Button>}
      />

      <Card>
        <CardHeader>
          <CardTitle>Pendientes</CardTitle>
          <span className="text-xs text-muted-foreground">{cuentas.data?.items.length ?? 0} cuentas</span>
        </CardHeader>
        <CardBody>
          <DataTable<CuentaPorCobrar>
            rows={cuentas.data?.items ?? []}
            loading={cuentas.isLoading}
            rowKey={(fila) => fila.id}
            empty="Sin cuentas por cobrar."
            columns={[
              { key: 'cliente', header: 'Cliente', render: (fila) => fila.clienteNombre },
              { key: 'monto', header: 'Monto', align: 'right', render: (fila) => formatUSD(fila.montoUSD) },
              { key: 'saldo', header: 'Saldo', align: 'right', render: (fila) => formatUSD(fila.saldoUSD) },
              { key: 'vencimiento', header: 'Vencimiento', render: (fila) => fila.vencimiento.slice(0, 10) },
              { key: 'estado', header: 'Estado', render: (fila) => <StatusBadge status={fila.estado} /> },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (fila) => (
                  <ActionMenu
                    label={`Acciones de ${fila.clienteNombre}`}
                    options={[
                      { label: 'Registrar pago', icon: <Wallet size={15} />, onClick: () => setPagando(fila) },
                    ]}
                  />
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={creando}
        onClose={() => setCreando(false)}
        title="Nueva cuenta por cobrar"
        footer={
          <>
            <Button variant="ghost" onClick={() => setCreando(false)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-cxc" loading={crear.isPending}>
              Registrar
            </Button>
          </>
        }
      >
        <form id="form-cxc" className="flex flex-col gap-3" onSubmit={crearForm.handleSubmit((d) => crear.mutate(d))} noValidate>
          <Select label="Cliente" error={crearForm.formState.errors.clienteId?.message} {...crearForm.register('clienteId')}>
            <option value="">Selecciona…</option>
            {clientes.data?.items.map((cliente) => (
              <option key={cliente.id} value={cliente.id}>
                {cliente.nombre}
              </option>
            ))}
          </Select>
          <Input label="Monto USD" type="number" step="0.01" error={crearForm.formState.errors.montoUSD?.message} {...crearForm.register('montoUSD')} />
          <Input label="Vencimiento" type="date" error={crearForm.formState.errors.vencimiento?.message} {...crearForm.register('vencimiento')} />
        </form>
      </Modal>

      <Modal
        open={Boolean(pagando)}
        onClose={() => setPagando(null)}
        title={`Pago · ${pagando?.clienteNombre ?? ''}`}
        footer={
          <>
            <Button variant="ghost" onClick={() => setPagando(null)}>
              Cancelar
            </Button>
            <Button
              type="submit"
              form="form-pago-cxc"
              loading={pagar.isPending}
              disabled={!pagando}
            >
              Registrar pago
            </Button>
          </>
        }
      >
        <form
          id="form-pago-cxc"
          className="flex flex-col gap-3"
          onSubmit={pagoForm.handleSubmit((d) => pagando && pagar.mutate({ id: pagando.id, monto: d.monto }))}
          noValidate
        >
          <p className="text-sm text-muted-foreground">
            Saldo pendiente <span className="num text-foreground">{formatUSD(pagando?.saldoUSD ?? 0)}</span>
          </p>
          <Input label="Monto" type="number" step="0.01" error={pagoForm.formState.errors.monto?.message} {...pagoForm.register('monto')} />
        </form>
      </Modal>
    </div>
  );
}
