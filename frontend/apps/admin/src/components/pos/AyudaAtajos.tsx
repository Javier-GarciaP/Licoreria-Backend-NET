import { Modal } from '@licoreria/ui';

const ATAJOS: { grupo: string; filas: [string, string][] }[] = [
  {
    grupo: 'Navegación',
    filas: [
      ['Letra / número', 'Abrir el buscador y escribir'],
      ['F2 · / · Ctrl+F', 'Buscar producto'],
      ['Flechas', 'Mover entre productos'],
      ['← →', 'Cambiar de categoría'],
      ['Ctrl+↑ / Ctrl+↓', 'Seleccionar línea de la orden'],
    ],
  },
  {
    grupo: 'Venta',
    filas: [
      ['Enter', 'Agregar producto o confirmar'],
      ['+ / -', 'Ajustar cantidad de la línea'],
      ['Supr', 'Quitar línea'],
      ['F8', 'Foco en descuento'],
      ['F4', 'Cobrar el total'],
    ],
  },
  {
    grupo: 'Órdenes',
    filas: [
      ['Ctrl+N', 'Nueva orden'],
      ['Alt+1…9', 'Cambiar de orden'],
      ['Esc', 'Limpiar búsqueda'],
      ['F1', 'Mostrar esta ayuda'],
    ],
  },
];

export function AyudaAtajos({ open, onClose }: { open: boolean; onClose: () => void }) {
  return (
    <Modal open={open} onClose={onClose} title="Atajos de teclado" className="max-w-lg" backdrop="none">
      <div className="grid gap-5 sm:grid-cols-1">
        {ATAJOS.map((bloque) => (
          <section key={bloque.grupo}>
            <p className="mb-2 text-xs font-medium uppercase tracking-tighter2 text-muted">{bloque.grupo}</p>
            <ul className="flex flex-col gap-1.5">
              {bloque.filas.map(([tecla, descripcion]) => (
                <li key={tecla} className="flex items-center justify-between gap-4 text-sm">
                  <kbd className="rounded border border-hairline bg-elevated px-2 py-1 text-xs text-ink">{tecla}</kbd>
                  <span className="text-muted">{descripcion}</span>
                </li>
              ))}
            </ul>
          </section>
        ))}
      </div>
    </Modal>
  );
}
