import { useRef, useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ImagePlus, Loader2, RefreshCw, Trash2, Upload } from 'lucide-react';
import { Button, cn } from '@licoreria/ui';
import { contenidoApi } from '@licoreria/api-client';
import { mensajeDeError } from '../../lib/api';
import { urlDeImagen } from '../../lib/imagenProducto';
import { CodigoBarras } from './CodigoBarras';

export function PanelImagenProducto({
  imagenUrl,
  variantes,
  onImagen,
}: {
  imagenUrl: string | null;
  variantes: { nombre: string; sku: string; codigoBarras?: string }[];
  onImagen: (url: string | null) => void;
}) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [arrastrando, setArrastrando] = useState(false);
  const [indice, setIndice] = useState(0);

  const subir = useMutation({
    mutationFn: (file: File) => contenidoApi.subirArchivo(file, 'productos'),
    onSuccess: (asset) => {
      toast.success('Imagen subida');
      onImagen(asset.url);
    },
    onError: (error) => toast.error('No se pudo subir la imagen', { description: mensajeDeError(error) }),
  });

  const MAX_IMAGEN = 2 * 1024 * 1024;

  const elegirArchivo = (file?: File | null) => {
    if (!file) return;
    if (file.size > MAX_IMAGEN) {
      toast.error('La imagen supera 2 MB', { description: 'Comprime la imagen o elige otra (JPG, PNG, WEBP).' });
      return;
    }
    if (file.type.startsWith('image/') || file.type === '') {
      subir.mutate(file);
    } else {
      toast.error('Selecciona un archivo de imagen (PNG, JPG, WEBP…)');
    }
  };

  const imagen = urlDeImagen(imagenUrl);
  const segura = variantes.length > 0 ? Math.min(indice, variantes.length - 1) : 0;
  const seleccionada = variantes[segura];
  const valorBarras = seleccionada?.codigoBarras || seleccionada?.sku || null;

  return (
    <div className="flex flex-col gap-4">
      <div className="aspect-square w-full max-h-[min(60dvh,32rem)]">
        <div
          role="button"
          tabIndex={0}
          aria-label={imagen ? 'Cambiar imagen del producto' : 'Subir imagen del producto'}
          onKeyDown={(evento) => {
            if (evento.key === 'Enter' || evento.key === ' ') {
              evento.preventDefault();
              inputRef.current?.click();
            }
          }}
          onClick={() => inputRef.current?.click()}
          onDragOver={(evento) => {
            evento.preventDefault();
            setArrastrando(true);
          }}
          onDragLeave={() => setArrastrando(false)}
          onDrop={(evento) => {
            evento.preventDefault();
            setArrastrando(false);
            elegirArchivo(evento.dataTransfer.files?.[0]);
          }}
          className={cn(
            'flex h-[calc(100%-3px)] w-full cursor-pointer items-center justify-center overflow-hidden rounded-xl border-2 border-dashed transition',
            arrastrando ? 'border-primary bg-primary/10' : 'border-border hover:border-primary/60 hover:bg-muted/30',
          )}
        >
          <input
            ref={inputRef}
            type="file"
            accept="image/*"
            className="hidden"
            onChange={(evento) => {
              elegirArchivo(evento.target.files?.[0]);
              evento.target.value = '';
            }}
          />
          {subir.isPending ? (
            <div className="flex flex-col items-center gap-2 text-sm text-muted-foreground">
              <Loader2 className="h-7 w-7 animate-spin" />
              Subiendo imagen…
            </div>
          ) : imagen ? (
            <img src={imagen} alt="Vista previa del producto" className="h-full w-full object-cover" />
          ) : (
            <div className="flex flex-col items-center gap-2 p-4 text-center text-sm text-muted-foreground">
              <Upload className="h-8 w-8" />
              Arrastra una imagen o haz clic para subirla
            </div>
          )}
        </div>
      </div>

      {imagen && !subir.isPending && (
        <div className="flex gap-2">
          <Button type="button" size="sm" variant="ghost" leftIcon={<RefreshCw size={14} />} onClick={() => inputRef.current?.click()}>
            Cambiar
          </Button>
          <Button type="button" size="sm" variant="ghost" leftIcon={<Trash2 size={14} />} onClick={() => onImagen(null)}>
            Quitar
          </Button>
        </div>
      )}
      {!imagen && !subir.isPending && (
        <p className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <ImagePlus size={13} />
          JPG, PNG o WEBP · máx 2 MB
        </p>
      )}

      {variantes.length > 1 && (
        <div className="flex flex-wrap gap-1.5" role="group" aria-label="Presentación del código de barras">
          {variantes.map((variante, posicion) => (
            <button
              key={posicion}
              type="button"
              onClick={() => setIndice(posicion)}
              className={cn(
                'rounded-full border px-2.5 py-1 text-xs transition',
                posicion === segura
                  ? 'border-primary bg-primary/15 text-foreground'
                  : 'border-border text-muted-foreground hover:text-foreground',
              )}
            >
              {variante.nombre || `Variante ${posicion + 1}`}
            </button>
          ))}
        </div>
      )}

      <CodigoBarras valor={valorBarras} className="border-t border-border pt-4" />
    </div>
  );
}
