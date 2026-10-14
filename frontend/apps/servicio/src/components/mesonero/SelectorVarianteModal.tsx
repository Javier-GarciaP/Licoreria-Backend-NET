import { Button, Modal, cn } from '@licoreria/ui';
import type { Producto, ProductoVariante } from '@licoreria/types';
import { formatUSD } from '../../lib/format';
import { estadoExistencia, variantesVendibles } from '../../lib/existencia';

/**
 * Permite al mesonero elegir la presentación (1 L, 2 L, lata, trago…) de un producto con varias
 * variantes. Muestra el precio y la existencia disponible de cada una; las agotadas no se pueden pedir.
 */
export function SelectorVarianteModal({
  producto,
  onCerrar,
  onElegir,
}: {
  producto: Producto | null;
  onCerrar: () => void;
  onElegir: (producto: Producto, variante: ProductoVariante) => void;
}) {
  if (!producto) return null;
  const variantes = variantesVendibles(producto);

  return (
    <Modal
      open
      onClose={onCerrar}
      title={producto.nombre}
      className="max-w-md"
      footer={
        <Button variant="ghost" onClick={onCerrar}>
          Cancelar
        </Button>
      }
    >
      <p className="mb-3 text-xs text-muted-foreground">Elige la presentación que pidió el cliente.</p>
      <div className="flex flex-col gap-2">
        {variantes.map((variante) => {
          const { etiqueta: existencia, bloqueada, aviso } = estadoExistencia(producto, variante);
          return (
            <button
              key={variante.id}
              type="button"
              disabled={bloqueada}
              onClick={() => {
                onElegir(producto, variante);
                onCerrar();
              }}
              className={cn(
                'flex items-center justify-between gap-3 rounded-inner border border-border bg-card px-3 py-3 text-left transition',
                bloqueada ? 'cursor-not-allowed opacity-50' : 'hover:border-primary hover:bg-primary/10',
              )}
            >
              <span className="flex flex-col">
                <span className="text-sm font-medium text-foreground">{variante.nombre}</span>
                {existencia && (
                  <span className={cn('num text-[11px]', bloqueada ? 'text-destructive-fg' : 'text-muted-foreground')}>
                    {existencia}
                  </span>
                )}
                {aviso && <span className="text-[11px] text-muted-foreground">{aviso}</span>}
              </span>
              <span className="num text-sm text-foreground">{formatUSD(variante.precioVentaUSD)}</span>
            </button>
          );
        })}
      </div>
    </Modal>
  );
}
