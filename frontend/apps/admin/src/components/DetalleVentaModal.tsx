import { Printer } from 'lucide-react';
import { Button, Modal, ModalSection } from '@licoreria/ui';
import type { Venta } from '@licoreria/types';
import { formatBS, formatDateTime, formatUSD } from '../lib/format';

function Fila({ etiqueta, valor, fuerte = false }: { etiqueta: string; valor: string; fuerte?: boolean }) {
  return (
    <div className={`flex items-baseline justify-between gap-3 ${fuerte ? 'text-sm font-medium text-foreground' : 'text-sm text-muted-foreground'}`}>
      <span>{etiqueta}</span>
      <span className="num shrink-0 text-right text-foreground">{valor}</span>
    </div>
  );
}

/** Detalle de venta ordenado en secciones (patrón del ticket), dentro de un modal espacioso. */
export function DetalleVentaModal({
  venta,
  onCerrar,
  onImprimir,
}: {
  venta: Venta | null;
  onCerrar: () => void;
  onImprimir: () => void;
}) {
  if (!venta) return null;

  const propina = venta.pagos.reduce((total, pago) => total + (pago.propina ?? 0), 0);
  const unidades = venta.detalles.reduce((total, detalle) => total + detalle.cantidad, 0);
  const numero = venta.numeroComprobante ?? venta.id.slice(0, 8);

  return (
    <Modal
      open={Boolean(venta)}
      onClose={onCerrar}
      title={`Venta ${numero}`}
      size="lg"
      backdrop="none"
      footer={
        <>
          <Button variant="ghost" onClick={onCerrar}>
            Cerrar
          </Button>
          <Button onClick={onImprimir} leftIcon={<Printer size={15} />}>
            Imprimir ticket
          </Button>
        </>
      }
    >
      <div className="flex flex-col gap-6">
        <div className="flex flex-wrap gap-x-8 gap-y-2 rounded-inner border border-border bg-muted/40 p-4">
          <Fila etiqueta="Fecha" valor={formatDateTime(venta.fecha)} />
          <Fila etiqueta="Estado" valor={venta.estado} />
          <Fila etiqueta="Tasa" valor={formatBS(venta.tasaCambio)} />
          <Fila etiqueta="Artículos" valor={String(unidades)} />
        </div>

        <ModalSection title="Detalle">
          <div className="overflow-x-auto rounded-inner border border-border">
            <table className="w-full min-w-[420px] text-sm">
              <thead>
                <tr className="bg-muted/60 text-left text-xs uppercase tracking-tighter2 text-muted-foreground">
                  <th scope="col" className="px-4 py-2.5">Producto</th>
                  <th scope="col" className="px-4 py-2.5 text-right">Precio</th>
                  <th scope="col" className="px-4 py-2.5 text-right">Subtotal</th>
                </tr>
              </thead>
              <tbody>
                {venta.detalles.map((linea) => (
                  <tr key={linea.id} className="border-t border-border">
                    <td className="px-4 py-2.5">
                      <span className="text-foreground">
                        {linea.cantidad} × {linea.nombre}
                      </span>
                      {linea.esCortesia && <span className="ml-2 text-warning-fg">(cortesía)</span>}
                      {linea.sku && <span className="ml-1 text-xs text-muted-foreground">· {linea.sku}</span>}
                    </td>
                    <td className="num px-4 py-2.5 text-right text-muted-foreground">{formatUSD(linea.precioUnitarioUSD)}</td>
                    <td className="num px-4 py-2.5 text-right text-foreground">{formatUSD(linea.subtotalUSD)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </ModalSection>

        <ModalSection title="Totales">
          <div className="flex flex-col gap-1.5">
            <Fila etiqueta="Subtotal" valor={formatUSD(venta.subtotalUSD)} />
            {venta.descuentoUSD > 0 && <Fila etiqueta="Descuento" valor={`−${formatUSD(venta.descuentoUSD)}`} />}
            {propina > 0 && <Fila etiqueta="Propina" valor={formatUSD(propina)} />}
            <Fila etiqueta="Total USD" valor={formatUSD(venta.totalUSD + propina)} fuerte />
            <Fila etiqueta="Total Bs" valor={formatBS(venta.totalBS + propina * venta.tasaCambio)} />
          </div>
        </ModalSection>

        <ModalSection title="Pagos">
          {venta.pagos.length === 0 ? (
            <p className="text-sm text-muted-foreground">Sin pagos registrados</p>
          ) : (
            <div className="flex flex-col gap-1.5">
              {venta.pagos.map((pago) => (
                <Fila
                  key={pago.id}
                  etiqueta={pago.metodoPago}
                  valor={pago.moneda === 'USD' ? formatUSD(pago.monto) : formatBS(pago.monto)}
                />
              ))}
            </div>
          )}
        </ModalSection>
      </div>
    </Modal>
  );
}