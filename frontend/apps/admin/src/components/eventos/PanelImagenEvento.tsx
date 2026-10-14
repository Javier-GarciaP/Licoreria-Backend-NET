import { useRef, useState } from 'react';
import { useMutation } from '@tanstack/react-query';
import { toast } from 'sonner';
import { ImagePlus, Loader2, RefreshCw, Trash2, Upload } from 'lucide-react';
import { Button, cn } from '@licoreria/ui';
import { contenidoApi } from '@licoreria/api-client';
import { mensajeDeError } from '../../lib/api';
import { urlDeImagen } from '../../lib/imagenProducto';

const MAX_IMAGEN = 2 * 1024 * 1024;

/** Sube y previsualiza la imagen de un evento (drag & drop o clic). */
export function PanelImagenEvento({
  imagenUrl,
  onImagen,
}: {
  imagenUrl: string | null;
  onImagen: (url: string | null) => void;
}) {
  const inputRef = useRef<HTMLInputElement>(null);
  const [arrastrando, setArrastrando] = useState(false);

  const subir = useMutation({
    mutationFn: (file: File) => contenidoApi.subirArchivo(file, 'eventos'),
    onSuccess: (asset) => {
      toast.success('Imagen subida');
      onImagen(asset.url);
    },
    onError: (error) => toast.error('No se pudo subir la imagen', { description: mensajeDeError(error) }),
  });

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

  return (
    <div className="flex flex-col gap-4">
      <div
        role="button"
        tabIndex={0}
        aria-label={imagen ? 'Cambiar imagen del evento' : 'Subir imagen del evento'}
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
          'flex aspect-video w-full cursor-pointer items-center justify-center overflow-hidden rounded-xl border-2 border-dashed transition',
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
          <img src={imagen} alt="Vista previa del evento" className="h-full w-full object-cover" />
        ) : (
          <div className="flex flex-col items-center gap-2 p-6 text-center text-sm text-muted-foreground">
            <Upload className="h-8 w-8" />
            Arrastra una imagen o haz clic para subirla
          </div>
        )}
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
          JPG, PNG o WEBP · máx 2 MB · se mostrará en la web pública
        </p>
      )}
    </div>
  );
}