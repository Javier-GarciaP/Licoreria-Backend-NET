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
  PageHeader,
  Pagination,
  Pill,
  Select,
  StatusBadge,
} from '@licoreria/ui';
import type { CuentaPorPagar } from '@licoreria/types';
import { comprasApi, ventasApi } from '@licoreria/api-client';
import { ComprasTabs } from '../components/ComprasTabs';
import { FolderPanel } from '../components/FolderTabs';
import { InlineForm } from '../components/InlineForm';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

const esquemaPago = z.object({
  monto: z.coerce.number({ invalid_type_error: 'Ingresa el monto' }).positive('Debe ser mayor que 0'),
  moneda: z.enum(['USD', 'BS']),
  metodoPagoId: z.string(),
  referencia: z.string().optional(),
});

type FormularioPago = z.infer<typeof esquemaPago>;

export function CuentasPorPagarPage() {
  const [page, setPage] = useState(1);
  const [soloPendientes, setSoloPendientes] = useState(true);
  const [pagando, setPagando] = useState<CuentaPorPagar | null>(null);
  const queryClient = useQueryClient();

  const pagoForm = useForm<FormularioPago>({
    resolver: zodResolver(esquemaPago),
    defaultValues: { monto: 0, moneda: 'USD', metodoPagoId: '', referencia: '' },
  });

  const cuentas = useQuery({
    queryKey: ['cuentas-pagar', page, soloPendientes],
    queryFn: () => comprasApi.cuentasPorPagar({ page, pageSize: 15, soloPendientes }),
  });
  const metodos = useQuery({ queryKey: ['metodos-pago'], queryFn: ventasApi.metodosPago, enabled: Boolean(pagando) });

  const pagar = useMutation({
    mutationFn: (datos: FormularioPago) =>
      comprasApi.registrarPago(pagando!.id, {
        monto: datos.monto,
        moneda: datos.moneda,
        metodoPagoId: datos.metodoPagoId || null,
        referencia: datos.referencia || null,
      }),
    onSuccess: () => {
      toast.success('Pago registrado');
      setPagando(null);
      pagoForm.reset({ monto: 0, moneda: 'USD', metodoPagoId: '', referencia: '' });
      queryClient.invalidateQueries({ queryKey: ['cuentas-pagar'] });
    },
    onError: (error) => toast.error('No se pudo registrar el pago', { description: mensajeDeError(error) }),
  });

  const abrirPago = (cuenta: CuentaPorPagar) => {
    pagoForm.reset({ monto: cuenta.saldoUSD, moneda: 'USD', metodoPagoId: '', referencia: '' });
    setPagando(cuenta);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Compras" subtitle="Cuentas por pagar a proveedores." />
      <ComprasTabs />

      <FolderPanel className="flex flex-col gap-4">
        <Card className="border-0 bg-transparent shadow-none">
          <CardHeader>
            <CardTitle>Cuentas por pagar</CardTitle>
            <label className="flex items-center gap-2 text-xs text-muted">
              <input
                type="checkbox"
                checked={soloPendientes}
                onChange={(evento) => {
                  setSoloPendientes(evento.target.checked);
                  setPage(1);
                }}
              />
              Solo pendientes
            </label>
          </CardHeader>
          <CardBody>
            <DataTable<CuentaPorPagar>
              rows={cuentas.data?.items ?? []}
              loading={cuentas.isLoading}
              rowKey={(cuenta) => cuenta.id}
              empty="No hay cuentas por pagar."
              expandirKey={pagando?.id ?? null}
              expandedRow={
                pagando
                  ? (cuenta) =>
                      cuenta.id === pagando.id ? (
                        <InlineForm title={`Pago · ${cuenta.proveedorNombre}`} onCancel={() => setPagando(null)}>
                          <form
                            className="flex flex-col gap-3"
                            onSubmit={pagoForm.handleSubmit((d) => pagar.mutate(d))}
                            noValidate
                          >
                            <p className="text-xs text-muted">
                              Saldo pendiente: <span className="num text-ink">{formatUSD(cuenta.saldoUSD)}</span>
                            </p>
                            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                              <Input label="Monto" type="number" error={pagoForm.formState.errors.monto?.message} {...pagoForm.register('monto')} />
                              <Select label="Moneda" {...pagoForm.register('moneda')}>
                                <option value="USD">USD</option>
                                <option value="BS">Bs</option>
                              </Select>
                            </div>
                            <Select label="Método de pago" {...pagoForm.register('metodoPagoId')}>
                              <option value="">Sin especificar</option>
                              {metodos.data?.map((metodo) => (
                                <option key={metodo.id} value={metodo.id}>
                                  {metodo.nombre}
                                </option>
                              ))}
                            </Select>
                            <Input label="Referencia" {...pagoForm.register('referencia')} />
                            <div className="flex justify-end gap-2">
                              <Button variant="ghost" onClick={() => setPagando(null)}>
                                Cancelar
                              </Button>
                              <Button type="submit" loading={pagar.isPending}>
                                Registrar pago
                              </Button>
                            </div>
                          </form>
                        </InlineForm>
                      ) : null
                  : undefined
              }
              columns={[
                { key: 'proveedor', header: 'Proveedor', render: (cuenta) => <span className="text-ink">{cuenta.proveedorNombre}</span> },
                { key: 'estado', header: 'Estado', render: (cuenta) => <StatusBadge status={cuenta.estado} /> },
                { key: 'vencimiento', header: 'Vencimiento', render: (cuenta) => formatDateTime(cuenta.vencimiento) },
                { key: 'monto', header: 'Monto', align: 'right', render: (cuenta) => formatUSD(cuenta.montoUSD) },
                {
                  key: 'saldo',
                  header: 'Saldo',
                  align: 'right',
                  render: (cuenta) => <Pill tone={cuenta.saldoUSD > 0 ? 'warning' : 'success'}>{formatUSD(cuenta.saldoUSD)}</Pill>,
                },
                {
                  key: 'acciones',
                  header: '',
                  align: 'right',
                  render: (cuenta) => (
                    <ActionMenu
                      label={`Acciones de ${cuenta.proveedorNombre}`}
                      options={[
                        {
                          label: 'Pagar',
                          icon: <Wallet size={15} />,
                          disabled: cuenta.saldoUSD <= 0,
                          onClick: () => abrirPago(cuenta),
                        },
                      ]}
                    />
                  ),
                },
              ]}
            />
            <Pagination page={page} totalPages={cuentas.data?.totalPages ?? 1} onPageChange={setPage} />
          </CardBody>
        </Card>
      </FolderPanel>
    </div>
  );
}