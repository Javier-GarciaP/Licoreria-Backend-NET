import { useEffect, useRef } from 'react';

const FOCUSABLES =
  'a[href], button:not([disabled]), textarea, input, select, [tabindex]:not([tabindex="-1"])';

/**
 * Atrapa el foco dentro de un diálogo mientras está abierto:
 * foco inicial, Tab cíclico, cierre con Escape y restauración del foco previo.
 * Devuelve la ref que debe ir en el contenedor del diálogo (con tabIndex={-1}).
 */
export function useFocusTrap<T extends HTMLElement = HTMLElement>(open: boolean, onClose: () => void) {
  const ref = useRef<T>(null);

  useEffect(() => {
    if (!open) return;

    const contenedor = ref.current;
    const elementoPrevio = document.activeElement as HTMLElement | null;

    const alPresionar = (evento: KeyboardEvent) => {
      if (evento.key === 'Escape') {
        onClose();
        return;
      }

      if (evento.key !== 'Tab' || !contenedor) return;

      const focusables = contenedor.querySelectorAll<HTMLElement>(FOCUSABLES);
      if (focusables.length === 0) {
        evento.preventDefault();
        return;
      }

      const primero = focusables[0];
      const ultimo = focusables[focusables.length - 1];

      if (evento.shiftKey && document.activeElement === primero) {
        evento.preventDefault();
        ultimo.focus();
      } else if (!evento.shiftKey && document.activeElement === ultimo) {
        evento.preventDefault();
        primero.focus();
      }
    };

    document.addEventListener('keydown', alPresionar);
    contenedor?.focus();

    return () => {
      document.removeEventListener('keydown', alPresionar);
      elementoPrevio?.focus?.();
    };
  }, [open, onClose]);

  return ref;
}
