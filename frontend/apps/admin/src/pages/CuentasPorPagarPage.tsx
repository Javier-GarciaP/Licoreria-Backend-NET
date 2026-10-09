import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Wallet } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  FiltroRango,
  Input,
  LimpiarFiltros,
  Pagination,
  Pill,
  Select,
  StatusBadge,
} from '@licoreria/ui';
import type { CuentaPorPagar } from '@licoreria/types';
import { comprasApi, ventasApi } from '@licoreria/api-client';
import { InlineForm } from '../components/InlineForm';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';
import { contiene, paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

const esquemaPago = z.object({
  monto: z.coerce.number({ invalid_type_error: 'Ingresa el monto' }).positive('Debe ser mayor que 0'),
  moneda: z.enum(['USD', 'BS']),
  metodoPagoId: z.string(),
  referencia: z.string().optional(),
});

type FormularioPago = z.infer<typeof esquemaPago>;

export function CuentasPorPagarPage() {
  const [page, setPage] = useState(1);
  const [soloPendientes, setSoloPendientes] = useState('');
  const [proveedor, setProveedor] = useState('');
  const [busqueda, setBusqueda] = useState('');
  const [saldoMin, setSaldoMin] = useState('');
  const [saldoMax, setSaldoMax] = useState('');
  const [pagando, setPagando] = useState<CuentaPorPagar | null>(null);
  const queryClient = useQueryClient();

  const pagoForm = useForm<FormularioPago>({
    resolver: zodResolver(esquemaPago),
    defaultValues: { monto: 0, moneda: 'USD', metodoPagoId: '', referencia: '' },
  });

  const soloPendientesSS = soloPendientes === '' ? true : soloPendientes === 'si';
  const hayFiltroLocal = Boolean(busqueda || proveedor || saldoMin || saldoMax);
  const hayFiltros = hayFiltroLocal || Boolean(soloPendientes);

  const limpiarFiltros = () => {
    setSoloPendientes('');
    setProveedor('');
    setBusqueda('');
    setSaldoMin('');
    setSaldoMax('');
    setPage(1);
  };

  const cuentas = useQuery({
    queryKey: ['cuentas-pagar', page, soloPendientesSS, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15],
    queryFn: () =>
      comprasApi.cuentasPorPagar({
        page: hayFiltroLocal ? 1 : page,
        pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15,
        soloPendientes: soloPendientesSS,
      }),
  });
  const metodos = useQuery({ queryKey: ['metodos-pago'], queryFn: ventasApi.metodosPago, enabled: Boolean(pagando) });

  const { items: filas, totalPages } = useMemo(() => {
    const saldoMinN = Number(saldoMin);
    const saldoMaxN = Number(saldoMax);
    const filtradas = (cuentas.data?.items ?? []).filter((cuenta) => {
      if (!contiene(cuenta.proveedorNombre, busqueda)) return false;
      if (proveedor && cuenta.proveedorId !== proveedor) return false;
      if (saldoMinN && cuenta.saldoUSD < saldoMinN) return false;
      if (saldoMaxN && cuenta.saldoUSD > saldoMaxN) return false;
      return true;
    });
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 15)
      : { items: filtradas, totalPages: cuentas.data?.totalPages ?? 1 };
  }, [cuentas.data, busqueda, proveedor, saldoMin, saldoMax, hayFiltroLocal, page]);

  const opcionesProveedor = useMemo(() => {
    const vistos = new Set<string>();
    const opciones: { valor: string; etiqueta: string }[] = [];
    for (const cuenta of cuentas.data?.items ?? []) {
      if (!vistos.has(cuenta.proveedorId)) {
        vistos.add(cuenta.proveedorId);
        opciones.push({ valor: cuenta.proveedorId, etiqueta: cuenta.proveedorNombre });
      }
    }
    return opciones.sort((a, b) => a.etiqueta.localeCompare(b.etiqueta));
  }, [cuentas.data]);

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
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar proveedor…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <FiltroDropdown
              label="Estado"
              opciones={[
                { valor: 'si', etiqueta: 'Solo pendientes' },
                { valor: 'no', etiqueta: 'Todas' },
              ]}
              valor={soloPendientes}
              onChange={(valor) => {
                setSoloPendientes(valor);
                setPage(1);
              }}
            />
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroDropdown
              label="Proveedor"
              opciones={opcionesProveedor}
              valor={proveedor}
              onChange={(valor) => {
                setProveedor(valor);
                setPage(1);
              }}
            />
            <FiltroRango
              label="Saldo USD"
              minimo={saldoMin}
              maximo={saldoMax}
              onMinimo={(valor) => {
                setSaldoMin(valor);
                setPage(1);
              }}
              onMaximo={(valor) => {
                setSaldoMax(valor);
                setPage(1);
              }}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<CuentaPorPagar>
            rows={filas}
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
                          <p className="text-xs text-muted-foreground">
                            Saldo pendiente: <span className="num text-foreground">{formatUSD(cuenta.saldoUSD)}</span>
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
              { key: 'proveedor', header: 'Proveedor', render: (cuenta) => <span className="text-foreground">{cuenta.proveedorNombre}</span> },
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
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </CardBody>
      </Card>
    </div>
  );
}