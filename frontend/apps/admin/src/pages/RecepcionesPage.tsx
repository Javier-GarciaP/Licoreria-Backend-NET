import { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Eye } from 'lucide-react';
import { ActionMenu, Buscador, Card, CardBody, CardHeader, DataTable, FiltroFechas, LimpiarFiltros, Modal, ModalSection, Pagination } from '@licoreria/ui';
import type { Recepcion } from '@licoreria/types';
import { comprasApi } from '@licoreria/api-client';
import { formatDateTime, formatNumber, formatUSD } from '../lib/format';
import { contiene, paginarEnMemoria, PAGE_SIZE_FILTRO_LOCAL } from '../lib/filtros';

export function RecepcionesPage() {
  const [page, setPage] = useState(1);
  const [detalle, setDetalle] = useState<Recepcion | null>(null);
  const [busqueda, setBusqueda] = useState('');
  const [desde, setDesde] = useState('');
  const [hasta, setHasta] = useState('');

  const hayFiltroLocal = Boolean(busqueda || desde || hasta);
  const hayFiltros = hayFiltroLocal;

  const limpiarFiltros = () => {
    setBusqueda('');
    setDesde('');
    setHasta('');
    setPage(1);
  };

  const recepciones = useQuery({
    queryKey: ['recepciones', page, hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15],
    queryFn: () => comprasApi.recepciones({ page: hayFiltroLocal ? 1 : page, pageSize: hayFiltroLocal ? PAGE_SIZE_FILTRO_LOCAL : 15 }),
  });

  const { items: filas, totalPages } = useMemo(() => {
    const filtradas = (recepciones.data?.items ?? []).filter((recepcion) => {
      if (!contiene(recepcion.numeroOrden, busqueda)) return false;
      if (desde && recepcion.fecha.slice(0, 10) < desde) return false;
      if (hasta && recepcion.fecha.slice(0, 10) > hasta) return false;
      return true;
    });
    return hayFiltroLocal
      ? paginarEnMemoria(filtradas, page, 15)
      : { items: filtradas, totalPages: recepciones.data?.totalPages ?? 1 };
  }, [recepciones.data, busqueda, desde, hasta, hayFiltroLocal, page]);

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar número de orden…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <FiltroFechas
              desde={desde}
              hasta={hasta}
              onDesde={(valor) => { setDesde(valor); setPage(1); }}
              onHasta={(valor) => { setHasta(valor); setPage(1); }}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
          <CardBody>
            <DataTable<Recepcion>
              rows={filas}
              loading={recepciones.isLoading}
              rowKey={(recepcion) => recepcion.id}
              empty="No hay recepciones."
              columns={[
                { key: 'orden', header: 'Orden', render: (recepcion) => <span className="text-foreground">{recepcion.numeroOrden}</span> },
                { key: 'fecha', header: 'Fecha', render: (recepcion) => formatDateTime(recepcion.fecha) },
                { key: 'lineas', header: 'Líneas', align: 'center', render: (recepcion) => recepcion.detalles.length },
                { key: 'total', header: 'Total', align: 'right', render: (recepcion) => formatUSD(recepcion.totalUSD) },
                {
                  key: 'acciones',
                  header: '',
                  align: 'right',
                  render: (recepcion) => (
                    <ActionMenu
                      label={`Acciones de recepción ${recepcion.numeroOrden}`}
                      options={[{ label: 'Ver detalle', icon: <Eye size={15} />, onClick: () => setDetalle(recepcion) }]}
                    />
                  ),
                },
              ]}
            />
            <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
          </CardBody>
      </Card>

      <Modal
        open={Boolean(detalle)}
        onClose={() => setDetalle(null)}
        title={`Recepción · ${detalle?.numeroOrden ?? ''}`}
        size="lg"
        backdrop="none"
      >
        {detalle && (
          <div className="flex flex-col gap-5">
            <p className="text-sm text-muted-foreground">{formatDateTime(detalle.fecha)}</p>
            {detalle.observaciones && <p className="text-sm text-muted-foreground">{detalle.observaciones}</p>}
            <ModalSection title="Líneas recibidas">
              <div className="overflow-x-auto rounded-inner border border-border">
                <table className="w-full min-w-[420px] text-sm">
                  <thead>
                    <tr className="bg-muted/60 text-left text-xs uppercase tracking-tighter2 text-muted-foreground">
                      <th scope="col" className="px-4 py-2.5">SKU</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Cantidad</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Costo unit.</th>
                      <th scope="col" className="px-4 py-2.5 text-right">Subtotal</th>
                    </tr>
                  </thead>
                  <tbody>
                    {detalle.detalles.map((item) => (
                      <tr key={item.id} className="border-t border-border">
                        <td className="px-4 py-2.5 text-foreground">{item.sku}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatNumber(item.cantidad)}</td>
                        <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatUSD(item.costoUnitarioUSD)}</td>
                        <td className="num px-4 py-2.5 text-right text-foreground">
                          {formatUSD(Number(item.cantidad) * Number(item.costoUnitarioUSD))}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </ModalSection>
            <div className="flex justify-end border-t border-border pt-3">
              <div className="flex items-baseline gap-3">
                <span className="text-sm text-muted-foreground">Total</span>
                <span className="num text-lg font-medium text-foreground">{formatUSD(detalle.totalUSD)}</span>
              </div>
            </div>
          </div>
        )}
      </Modal>
    </div>
  );
}