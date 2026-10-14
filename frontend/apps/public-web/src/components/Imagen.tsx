import { useState, type ReactNode } from 'react';
import { Wine } from 'lucide-react';
import { urlDeImagen } from '../lib/imagenes';

interface ImagenProps {
  src?: string | null;
  alt: string;
  /** Clases del contenedor (tamaño, redondeo, borde). La imagen ocupa 100% del mismo. */
  className?: string;
  /** Nodo a mostrar si no hay src o si la imagen falla al cargar. */
  fallback?: ReactNode;
  /** Carga prioritaria (above-the-fold); por defecto `lazy`. */
  prioritaria?: boolean;
}

/**
 * Imagen con las mejoras visuales de la web pública:
 * placeholder mientras carga, fade-in al aparecer, `decoding="async"`,
 * zoom suave al pasar el cursor y fallback si la URL falla (sin íconos rotos).
 */
export function Imagen({ src, alt, className = '', fallback, prioritaria }: ImagenProps) {
  const url = urlDeImagen(src);
  const [estado, setEstado] = useState<'cargando' | 'ok' | 'error'>(url ? 'cargando' : 'error');

  if (!url || estado === 'error') {
    return (
      <span className={`flex items-center justify-center overflow-hidden ${className}`}>
        {fallback ?? (
          <Wine size={20} strokeWidth={1.5} className="text-tiger-gold/50" />
        )}
      </span>
    );
  }

  return (
    <span className={`group/imagen relative block overflow-hidden ${className}`}>
      {estado === 'cargando' && (
        <span className="absolute inset-0 animate-pulse bg-charred-clove" aria-hidden="true" />
      )}
      <img
        src={url}
        alt={alt}
        loading={prioritaria ? 'eager' : 'lazy'}
        decoding="async"
        onLoad={() => setEstado('ok')}
        onError={() => setEstado('error')}
        className={`h-full w-full object-cover transition-[opacity,transform] duration-500 ease-out group-hover/imagen:scale-[1.04] ${
          estado === 'ok' ? 'opacity-100' : 'opacity-0'
        }`}
      />
    </span>
  );
}
