import { useMemo, useState, type ReactNode } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, CardHeader, CardTitle, cn, DataTable, PageHeader, Pill, Skeleton } from '@licoreria/ui';
import { reportesApi } from '@licoreria/api-client';
import type { OrdenProductos, ProductoVendido, ReporteExportableId } from '@licoreria/types';
import { BotonesExportar } from '../components/BotonesExportar';
import { SelectorPeriodo } from '../components/SelectorPeriodo';
import { useAuth } from '../context/AuthContext';
import { usePeriodo } from '../hooks/usePeriodo';
import { formatDateTime, formatNumber, formatPercent, formatUSD } from '../lib/format';

type Pestana =
  | 'ventas'
  | 'productos'
  | 'inventario'
  | 'compras'
  | 'propinas'
  | 'mesoneros'
  | 'caja'
  | 'ocupacion'
  | 'mermas';

interface DefinicionPestana {
  id: Pestana;
  label: string;
  /** Reporte que se descarga desde la pestaña. */
  exporta: ReporteExportableId;
  /** El reporte no depende del periodo (se oculta el selector). */
  sinPeriodo?: boolean;
  permiso?: string;
  /** Solo el administrador ve esta pestaña. */
  admin?: boolean;
}

const PESTANAS: DefinicionPestana[] = [
  { id: 'ventas', label: 'Ventas', exporta: 'ventas', permiso: 'sales:read' },
  { id: 'productos', label: 'Productos', exporta: 'productos', permiso: 'sales:read' },
  { id: 'inventario', label: 'Inventario', exporta: 'inventario', sinPeriodo: true, permiso: 'inventory:read' },
  { id: 'compras', label: 'Compras', exporta: 'compras', permiso: 'purchasing:read' },
  { id: 'propinas', label: 'Propinas', exporta: 'propinas', permiso: 'sales:read' },
  { id: 'mesoneros', label: 'Mesoneros', exporta: 'mesoneros', admin: true },
  { id: 'caja', label: 'Caja', exporta: 'caja', permiso: 'cash:close' },
  { id: 'ocupacion', label: 'Ocupación', exporta: 'ocupacion', permiso: 'club:read' },
  { id: 'mermas', label: 'Mermas', exporta: 'mermas-vs-ventas', permiso: 'inventory:read' },
];

const LIMITE_TOP = 10;

function Metrica({ label, value, hint }: { label: string; value: ReactNode; hint?: string }) {
  return (
    <div className="rounded-xl border border-border p-4">
      <p className="text-xs text-muted-foreground">{label}</p>
      <p className="num mt-1 text-xl font-medium text-foreground">{value}</p>
      {hint && <p className="mt-1 text-xs text-muted-foreground">{hint}</p>}
    </div>
  );
}

const REJILLA = 'grid grid-cols-2 gap-4 lg:grid-cols-4';

function formatMinutos(minutos: number | null): string {
  if (minutos === null) return '—';
  if (minutos < 60) return `${formatNumber(Math.round(minutos))} min`;
  const horas = Math.floor(minutos / 60);
  return `${horas} h ${Math.round(minutos % 60)} min`;
}

function formatSegundos(segundos: number | null): string {
  if (segundos === null) return '—';
  const minutos = Math.floor(segundos / 60);
  const resto = Math.round(segundos % 60);
  return minutos > 0 ? `${minutos} min ${resto} s` : `${resto} s`;
}

const ROTACION: Record<ProductoVendido['estadoRotacion'], string> = {
  Calculada: '',
  NoCalculable: 'No calculable',
  SinMovimiento: 'Sin movimiento',
};

function TablaProductos({ filas, vacio }: { filas: ProductoVendido[]; vacio: string }) {
  return (
    <DataTable
      rows={filas}
      rowKey={(fila) => fila.varianteId}
      empty={vacio}
      columns={[
        {
          key: 'producto',
          header: 'Producto',
          render: (fila) => (
            <span className="text-foreground">
              {fila.productoNombre} <span className="text-xs text-muted-foreground">{fila.varianteNombre}</span>
            </span>
          ),
        },
        { key: 'unidades', header: 'Unidades', align: 'right', render: (fila) => formatNumber(fila.unidades) },
        { key: 'ingreso', header: 'Ingreso', align: 'right', render: (fila) => formatUSD(fila.ingresoUSD) },
        { key: 'pct', header: '% unidades', align: 'right', render: (fila) => formatPercent(fila.porcentajeUnidades) },
        { key: 'existencia', header: 'Existencia', align: 'right', render: (fila) => formatNumber(fila.existencia) },
        {
          key: 'rotacion',
          header: 'Rotación',
          align: 'right',
          render: (fila) => (fila.rotacion === null ? ROTACION[fila.estadoRotacion] : formatNumber(fila.rotacion)),
        },
      ]}
    />
  );
}

function Cargando({ alto = 'h-64' }: { alto?: string }) {
  return <Skeleton className={cn(alto, 'w-full')} />;
}

export function ReportesPage() {
  const { esAdmin, tienePermiso } = useAuth() as { esAdmin: boolean; tienePermiso: (clave: string) => boolean };
  const periodo = usePeriodo();
  const [params, setParams] = useSearchParams();
  const [orden, setOrden] = useState<OrdenProductos>('Unidades');

  const visibles = useMemo(
    () => PESTANAS.filter((p) => (p.admin ? esAdmin : !p.permiso || esAdmin || tienePermiso(p.permiso))),
    [esAdmin, tienePermiso],
  );
  const solicitada = params.get('reporte');
  const actual = visibles.find((p) => p.id === solicitada) ?? visibles[0];
  const pestana = actual?.id;

  const { desde, hasta, valido } = periodo;
  const consulta = (id: Pestana) => pestana === id && (valido || Boolean(actual?.sinPeriodo));

  const ventas = useQuery({
    queryKey: ['reportes', 'ventas', desde, hasta],
    queryFn: () => reportesApi.ventas(desde, hasta),
    enabled: consulta('ventas'),
  });
  const productos = useQuery({
    queryKey: ['reportes', 'productos', desde, hasta, orden, LIMITE_TOP],
    queryFn: () => reportesApi.productos(desde, hasta, orden, LIMITE_TOP),
    enabled: consulta('productos'),
  });
  const inventario = useQuery({
    queryKey: ['reportes', 'inventario'],
    queryFn: () => reportesApi.inventario(),
    enabled: consulta('inventario'),
  });
  const compras = useQuery({
    queryKey: ['reportes', 'compras', desde, hasta],
    queryFn: () => reportesApi.compras(desde, hasta),
    enabled: consulta('compras'),
  });
  const propinas = useQuery({
    queryKey: ['reportes', 'propinas', desde, hasta],
    queryFn: () => reportesApi.propinas(desde, hasta),
    enabled: consulta('propinas'),
  });
  const mesoneros = useQuery({
    queryKey: ['reportes', 'mesoneros', desde, hasta],
    queryFn: () => reportesApi.mesoneros(desde, hasta),
    enabled: consulta('mesoneros'),
  });
  const caja = useQuery({
    queryKey: ['reportes', 'caja', desde, hasta],
    queryFn: () => reportesApi.caja(desde, hasta),
    enabled: consulta('caja'),
  });
  const ocupacion = useQuery({
    queryKey: ['reportes', 'ocupacion', desde, hasta],
    queryFn: () => reportesApi.ocupacion(desde, hasta),
    enabled: consulta('ocupacion'),
  });
  const asistencia = useQuery({
    queryKey: ['reportes', 'reservas-asistencia', desde, hasta],
    queryFn: () => reportesApi.reservasAsistencia(desde, hasta),
    enabled: consulta('ocupacion'),
  });
  const mermas = useQuery({
    queryKey: ['reportes', 'mermas-vs-ventas', desde, hasta],
    queryFn: () => reportesApi.mermasVsVentas(desde, hasta),
    enabled: consulta('mermas'),
  });

  const elegirPestana = (id: Pestana) =>
    setParams(
      (previo) => {
        const siguiente = new URLSearchParams(previo);
        siguiente.set('reporte', id);
        return siguiente;
      },
      { replace: true },
    );

  if (!actual) {
    return (
      <div className="mx-auto flex max-w-page flex-col gap-6">
        <PageHeader title="Reportes" subtitle="No tienes permiso para consultar reportes." />
      </div>
    );
  }

  const filtrosExportar = actual.sinPeriodo
    ? {}
    : { desde, hasta, ...(actual.id === 'productos' ? { orden, limite: LIMITE_TOP } : {}) };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Reportes"
        subtitle="Ventas, productos, inventario, compras, propinas, mesoneros, caja, ocupación y mermas."
        actions={
          <BotonesExportar
            key={actual.id}
            reporte={actual.exporta}
            filtros={filtrosExportar}
            deshabilitado={!actual.sinPeriodo && !valido}
          />
        }
      />

      {!actual.sinPeriodo && (
        <SelectorPeriodo periodo={periodo} onElegir={periodo.elegir} onRango={periodo.fijarRango} />
      )}

      <nav aria-label="Secciones de reportes" className="flex flex-wrap gap-2">
        {visibles.map((item) => (
          <button
            key={item.id}
            type="button"
            aria-current={pestana === item.id ? 'page' : undefined}
            onClick={() => elegirPestana(item.id)}
            className={cn(
              'rounded-full border px-4 py-1.5 text-sm transition',
              pestana === item.id
                ? 'border-primary text-foreground'
                : 'border-border text-muted-foreground hover:text-foreground',
            )}
          >
            {item.label}
          </button>
        ))}
      </nav>

      {pestana === 'ventas' && (
        <Card>
          <CardHeader>
            <CardTitle>Ventas del período</CardTitle>
            {ventas.data && <Pill tone="success">{ventas.data.cantidad} ventas</Pill>}
          </CardHeader>
          <CardBody className="flex flex-col gap-5">
            {ventas.isLoading || !ventas.data ? (
              <Cargando />
            ) : (
              <>
                <div className={REJILLA}>
                  <Metrica label="Total USD" value={formatUSD(ventas.data.totalUSD)} />
                  <Metrica label="Total Bs" value={formatNumber(ventas.data.totalBS)} />
                  <Metrica label="Ticket promedio" value={formatUSD(ventas.data.ticketPromedioUSD)} />
                  <Metrica label="Ventas" value={formatNumber(ventas.data.cantidad)} />
                </div>

                <div>
                  <p className="mb-2 text-sm font-medium text-foreground">Por día</p>
                  <DataTable
                    rows={ventas.data.porDia}
                    rowKey={(fila) => fila.fecha}
                    columns={[
                      { key: 'fecha', header: 'Fecha', render: (fila) => formatDateTime(fila.fecha) },
                      { key: 'cantidad', header: 'Ventas', align: 'right', render: (fila) => formatNumber(fila.cantidad) },
                      { key: 'total', header: 'Total', align: 'right', render: (fila) => formatUSD(fila.totalUSD) },
                    ]}
                  />
                </div>

                <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
                  <div>
                    <p className="mb-2 text-sm font-medium text-foreground">Por usuario</p>
                    <DataTable
                      rows={ventas.data.porUsuario}
                      rowKey={(fila) => fila.usuarioId}
                      empty="Sin datos."
                      columns={[
                        { key: 'usuario', header: 'Usuario', render: (fila) => fila.usuarioNombre },
                        { key: 'cantidad', header: 'Ventas', align: 'right', render: (fila) => formatNumber(fila.cantidad) },
                        { key: 'total', header: 'Total', align: 'right', render: (fila) => formatUSD(fila.totalUSD) },
                      ]}
                    />
                  </div>
                  <div>
                    <p className="mb-2 text-sm font-medium text-foreground">Por método de pago</p>
                    <DataTable
                      rows={ventas.data.porMetodoPago}
                      rowKey={(fila) => `${fila.metodoPagoId}-${fila.moneda}`}
                      empty="Sin datos."
                      columns={[
                        { key: 'metodo', header: 'Método', render: (fila) => fila.metodoPagoNombre },
                        { key: 'moneda', header: 'Moneda', render: (fila) => fila.moneda },
                        { key: 'monto', header: 'Monto', align: 'right', render: (fila) => formatNumber(fila.monto) },
                      ]}
                    />
                  </div>
                </div>
              </>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'productos' && (
        <Card>
          <CardHeader>
            <CardTitle>Productos más vendidos</CardTitle>
            <div className="flex items-center gap-2" role="group" aria-label="Orden del ranking">
              {(['Unidades', 'Ingresos'] as const).map((opcion) => (
                <button
                  key={opcion}
                  type="button"
                  aria-pressed={orden === opcion}
                  onClick={() => setOrden(opcion)}
                  className={cn(
                    'rounded-full border px-3 py-1 text-xs transition',
                    orden === opcion
                      ? 'border-primary text-foreground'
                      : 'border-border text-muted-foreground hover:text-foreground',
                  )}
                >
                  Por {opcion.toLowerCase()}
                </button>
              ))}
            </div>
          </CardHeader>
          <CardBody className="flex flex-col gap-5">
            {productos.isLoading || !productos.data ? (
              <Cargando />
            ) : (
              <>
                <div className={REJILLA}>
                  <Metrica label="Unidades vendidas" value={formatNumber(productos.data.totalUnidades)} />
                  <Metrica label="Ingreso" value={formatUSD(productos.data.totalIngresoUSD)} />
                </div>
                <div>
                  <p className="mb-2 text-sm font-medium text-foreground">Top {productos.data.limite}</p>
                  <TablaProductos filas={productos.data.top} vacio="Sin ventas en el periodo." />
                </div>
                <div>
                  <p className="mb-2 text-sm font-medium text-foreground">Con existencia y sin movimiento</p>
                  <TablaProductos filas={productos.data.sinMovimiento} vacio="Todos los productos con existencia tuvieron ventas." />
                </div>
              </>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'inventario' && (
        <Card>
          <CardHeader>
            <CardTitle>Inventario valorizado</CardTitle>
            {inventario.data && <Pill tone="accent">{inventario.data.variantes} variantes</Pill>}
          </CardHeader>
          <CardBody className="flex flex-col gap-5">
            {inventario.isLoading || !inventario.data ? (
              <Cargando />
            ) : (
              <>
                <div className={REJILLA}>
                  <Metrica label="Unidades" value={formatNumber(inventario.data.unidadesTotales)} />
                  <Metrica label="Costo total" value={formatUSD(inventario.data.costoTotalUSD)} />
                  <Metrica label="Valor de venta" value={formatUSD(inventario.data.valorVentaTotalUSD)} />
                  <Metrica label="Bajo mínimo" value={formatNumber(inventario.data.bajoMinimo)} />
                </div>
                <DataTable
                  rows={inventario.data.items}
                  rowKey={(fila) => fila.varianteId}
                  columns={[
                    {
                      key: 'producto',
                      header: 'Producto',
                      render: (fila) => (
                        <span className="text-foreground">
                          {fila.productoNombre} <span className="text-xs text-muted-foreground">{fila.sku}</span>
                        </span>
                      ),
                    },
                    { key: 'cantidad', header: 'Cantidad', align: 'right', render: (fila) => formatNumber(fila.cantidad) },
                    { key: 'costo', header: 'Costo total', align: 'right', render: (fila) => formatUSD(fila.costoTotalUSD) },
                    { key: 'venta', header: 'Valor venta', align: 'right', render: (fila) => formatUSD(fila.valorVentaUSD) },
                  ]}
                />
              </>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'compras' && (
        <Card>
          <CardHeader>
            <CardTitle>Compras del período</CardTitle>
            {compras.data && <Pill tone="accent">{compras.data.recepciones} recepciones</Pill>}
          </CardHeader>
          <CardBody>
            {compras.isLoading || !compras.data ? (
              <Cargando alto="h-40" />
            ) : (
              <div className={REJILLA}>
                <Metrica label="Comprado" value={formatUSD(compras.data.totalCompradoUSD)} />
                <Metrica label="Por pagar" value={formatUSD(compras.data.totalPorPagarUSD)} />
                <Metrica label="Pagado" value={formatUSD(compras.data.totalPagadoUSD)} />
                <Metrica label="Cuentas pendientes" value={formatNumber(compras.data.cuentasPendientes)} />
              </div>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'propinas' && (
        <Card>
          <CardHeader>
            <CardTitle>Propinas del período</CardTitle>
            {propinas.data && <Pill tone="success">{formatUSD(propinas.data.totalPropinaUSD)}</Pill>}
          </CardHeader>
          <CardBody>
            {propinas.isLoading || !propinas.data ? (
              <Cargando alto="h-40" />
            ) : (
              <DataTable
                rows={propinas.data.porUsuario}
                rowKey={(fila) => fila.usuarioId}
                empty="Sin propinas registradas."
                columns={[
                  { key: 'usuario', header: 'Usuario', render: (fila) => fila.usuarioNombre },
                  { key: 'ventas', header: 'Ventas', align: 'right', render: (fila) => formatNumber(fila.ventas) },
                  { key: 'total', header: 'Propina', align: 'right', render: (fila) => formatUSD(fila.totalPropinaUSD) },
                ]}
              />
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'mesoneros' && (
        <Card>
          <CardHeader>
            <CardTitle>Desempeño por mesonero</CardTitle>
            {mesoneros.data && <Pill tone="success">{formatUSD(mesoneros.data.totalVentasUSD)}</Pill>}
          </CardHeader>
          <CardBody>
            {mesoneros.isLoading || !mesoneros.data ? (
              <Cargando alto="h-40" />
            ) : (
              <DataTable
                rows={mesoneros.data.mesoneros}
                rowKey={(fila) => fila.usuarioId ?? 'sin-asignar'}
                empty="Sin comandas en el periodo."
                columns={[
                  { key: 'mesonero', header: 'Mesonero', render: (fila) => fila.nombre },
                  { key: 'comandas', header: 'Comandas', align: 'right', render: (fila) => formatNumber(fila.comandas) },
                  { key: 'items', header: 'Ítems servidos', align: 'right', render: (fila) => formatNumber(fila.itemsServidos) },
                  { key: 'ventas', header: 'Ventas generadas', align: 'right', render: (fila) => formatUSD(fila.ventasGeneradasUSD) },
                  { key: 'cuentas', header: 'Cuentas', align: 'right', render: (fila) => formatNumber(fila.cuentas) },
                  { key: 'ticket', header: 'Ticket por cuenta', align: 'right', render: (fila) => formatUSD(fila.ticketPorCuentaUSD) },
                  { key: 'cancelacion', header: 'Cancelación', align: 'right', render: (fila) => formatPercent(fila.tasaCancelacion) },
                  { key: 'servicio', header: 'Servicio promedio', align: 'right', render: (fila) => formatSegundos(fila.tiempoServicioPromedioSegundos) },
                ]}
              />
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'caja' && (
        <Card>
          <CardHeader>
            <CardTitle>Descuadre de caja por turno</CardTitle>
            {caja.data && <Pill tone="accent">{caja.data.turnos} turnos cerrados</Pill>}
          </CardHeader>
          <CardBody className="flex flex-col gap-5">
            {caja.isLoading || !caja.data ? (
              <Cargando />
            ) : (
              <>
                <div className={REJILLA}>
                  <Metrica label="Descuadre total" value={formatUSD(caja.data.descuadreTotal)} />
                  <Metrica label="Descuadre promedio" value={formatUSD(caja.data.descuadrePromedio)} />
                  <Metrica label="Mayor descuadre" value={formatUSD(caja.data.mayorDescuadre)} />
                  <Metrica label="Turnos cerrados" value={formatNumber(caja.data.turnos)} />
                </div>
                <DataTable
                  rows={caja.data.items}
                  rowKey={(fila) => fila.sesionId}
                  empty="Sin turnos cerrados en el periodo."
                  columns={[
                    { key: 'cajero', header: 'Cajero', render: (fila) => fila.usuarioNombre },
                    { key: 'cierre', header: 'Cierre', render: (fila) => formatDateTime(fila.cerradaEn) },
                    { key: 'esperado', header: 'Esperado', align: 'right', render: (fila) => formatUSD(fila.montoEsperado) },
                    { key: 'contado', header: 'Contado', align: 'right', render: (fila) => formatUSD(fila.montoContado) },
                    {
                      key: 'descuadre',
                      header: 'Descuadre',
                      align: 'right',
                      render: (fila) => (
                        <span className={fila.descuadre < 0 ? 'text-danger' : undefined}>{formatUSD(fila.descuadre)}</span>
                      ),
                    },
                  ]}
                />
              </>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'ocupacion' && (
        <Card>
          <CardHeader>
            <CardTitle>Ocupación de mesas y reservas</CardTitle>
            {ocupacion.data && (
              <Pill tone="accent">
                {ocupacion.data.mesasOcupadas} de {ocupacion.data.mesasActivas} mesas
              </Pill>
            )}
          </CardHeader>
          <CardBody className="flex flex-col gap-5">
            {ocupacion.isLoading || !ocupacion.data ? (
              <Cargando />
            ) : (
              <>
                <div className={REJILLA}>
                  <Metrica label="Ocupación actual" value={formatPercent(ocupacion.data.porcentajeOcupacion, 0)} />
                  <Metrica label="Permanencia promedio" value={formatMinutos(ocupacion.data.permanenciaPromedioMinutos)} />
                  <Metrica
                    label="Sesiones por mesa"
                    value={ocupacion.data.sesionesPorMesa === null ? '—' : formatNumber(ocupacion.data.sesionesPorMesa)}
                  />
                  <Metrica
                    label="No asistencia a reservas"
                    value={
                      asistencia.data?.tasaNoAsistencia == null ? 'Sin datos' : formatPercent(asistencia.data.tasaNoAsistencia)
                    }
                    hint={
                      asistencia.data
                        ? `${asistencia.data.asistieron} asistieron · ${asistencia.data.noAsistieron} no · ${asistencia.data.canceladas} canceladas`
                        : undefined
                    }
                  />
                </div>
                <DataTable
                  rows={ocupacion.data.porZona}
                  rowKey={(fila) => fila.zonaId ?? 'sin-zona'}
                  empty="Sin zonas."
                  columns={[
                    { key: 'zona', header: 'Zona', render: (fila) => fila.zona },
                    {
                      key: 'mesas',
                      header: 'Ocupadas',
                      align: 'right',
                      render: (fila) => `${fila.mesasOcupadas} de ${fila.mesasActivas}`,
                    },
                    { key: 'pct', header: '% ocupación', align: 'right', render: (fila) => formatPercent(fila.porcentajeOcupacion, 0) },
                    { key: 'sesiones', header: 'Sesiones', align: 'right', render: (fila) => formatNumber(fila.sesiones) },
                    {
                      key: 'permanencia',
                      header: 'Permanencia',
                      align: 'right',
                      render: (fila) => formatMinutos(fila.permanenciaPromedioMinutos),
                    },
                  ]}
                />
              </>
            )}
          </CardBody>
        </Card>
      )}

      {pestana === 'mermas' && (
        <Card>
          <CardHeader>
            <CardTitle>Mermas frente a ventas</CardTitle>
            {mermas.data && <Pill tone="warning">{formatPercent(mermas.data.porcentaje)} de las ventas</Pill>}
          </CardHeader>
          <CardBody className="flex flex-col gap-5">
            {mermas.isLoading || !mermas.data ? (
              <Cargando />
            ) : (
              <>
                <div className={REJILLA}>
                  <Metrica label="Ventas" value={formatUSD(mermas.data.ventasUSD)} />
                  <Metrica label="Merma valorizada" value={formatUSD(mermas.data.mermaValorizadaUSD)} />
                </div>
                <DataTable
                  rows={mermas.data.porMotivo}
                  rowKey={(fila) => fila.motivo}
                  empty="Sin mermas registradas."
                  columns={[
                    { key: 'motivo', header: 'Motivo', render: (fila) => <span className="capitalize">{fila.motivo}</span> },
                    { key: 'registros', header: 'Registros', align: 'right', render: (fila) => formatNumber(fila.registros) },
                    { key: 'unidades', header: 'Unidades', align: 'right', render: (fila) => formatNumber(fila.unidades) },
                    { key: 'valor', header: 'Costo estimado', align: 'right', render: (fila) => formatUSD(fila.valorUSD) },
                    { key: 'pct', header: '% de ventas', align: 'right', render: (fila) => formatPercent(fila.porcentajeVentas, 2) },
                  ]}
                />
              </>
            )}
          </CardBody>
        </Card>
      )}
    </div>
  );
}
