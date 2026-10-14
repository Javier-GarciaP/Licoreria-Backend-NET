import { useEffect, useRef } from 'react';

export type HotkeyMap = Record<string, (evento: KeyboardEvent) => void>;

const esEditable = (target: EventTarget | null) => {
  const elemento = target as HTMLElement | null;
  if (!elemento) return false;
  return (
    elemento.tagName === 'INPUT' ||
    elemento.tagName === 'TEXTAREA' ||
    elemento.tagName === 'SELECT' ||
    elemento.isContentEditable
  );
};

const normalizar = (evento: KeyboardEvent) => {
  const partes: string[] = [];
  if (evento.ctrlKey || evento.metaKey) partes.push('ctrl');
  if (evento.altKey) partes.push('alt');
  partes.push(evento.key.length === 1 ? evento.key.toLowerCase() : evento.key);
  return partes.join('+');
};

/**
 * Registra atajos de teclado a nivel de ventana. Los atajos sin modificador se
 * ignoran cuando el foco está en un campo editable (salvo teclas de función y Escape).
 */
export function usePosHotkeys(mapa: HotkeyMap, activo = true) {
  const ref = useRef(mapa);
  ref.current = mapa;

  useEffect(() => {
    if (!activo) return;
    const alPresionar = (evento: KeyboardEvent) => {
      const combo = normalizar(evento);
      const esFuncion = /^f\d+$/i.test(evento.key);
      const conModificador = evento.ctrlKey || evento.metaKey || evento.altKey;
      if (esEditable(evento.target) && !esFuncion && !conModificador && evento.key !== 'Escape') return;

      const handler = ref.current[combo] ?? ref.current[evento.key];
      if (!handler) return;
      evento.preventDefault();
      handler(evento);
    };
    window.addEventListener('keydown', alPresionar);
    return () => window.removeEventListener('keydown', alPresionar);
  }, [activo]);
}
