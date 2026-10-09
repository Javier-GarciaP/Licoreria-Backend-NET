import { cn } from '@licoreria/ui';

/** Genera un patrón determinista de barras a partir de un texto (p. ej. SKU). */
function gradientDe(valor: string): string {
  const semilla = Array.from(valor).reduce((acc, c) => acc + c.charCodeAt(0), 0) || 42;
  let estado = semilla;
  const pasos: string[] = [];
  let posicion = 0;
  while (posicion < 100) {
    estado = (estado * 1103515245 + 12345) & 0x7fffffff;
    const barra = 1 + (estado % 3);
    const espacio = 1 + ((estado >> 4) % 2);
    pasos.push(`#000 0 ${barra}px`, `transparent ${barra}px ${barra + espacio}px`);
    posicion += barra + espacio;
  }
  return `repeating-linear-gradient(90deg, ${pasos.join(',')})`;
}

/** Código de barras decorativo derivado de un valor (SKU). */
export function CodigoBarras({ valor, className }: { valor?: string | null; className?: string }) {
  if (!valor) {
    return (
      <div className={cn('flex flex-col items-center gap-1', className)}>
        <div className="h-9 w-40 rounded-inner border border-dashed border-border" />
        <span className="text-[11px] text-muted-foreground">Sin código de barras</span>
      </div>
    );
  }
  return (
    <div className={cn('flex flex-col items-center gap-1', className)}>
      <div className="h-9 w-40 max-w-full" style={{ backgroundImage: gradientDe(valor) }} role="img" aria-label={`Código de barras ${valor}`} />
      <span className="num text-[11px] tracking-tight text-muted-foreground">{valor}</span>
    </div>
  );
}
