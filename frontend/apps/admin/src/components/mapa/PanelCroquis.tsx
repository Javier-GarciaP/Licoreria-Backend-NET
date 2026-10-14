import { useRef, useState } from 'react';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Eye, EyeOff, ImagePlus, Loader2, Trash2, Upload } from 'lucide-react';
import { Button } from '@licoreria/ui';
import type { FondoAjuste } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { mensajeDeError } from '../../lib/api';

export const MAX_CROQUIS = 5 * 1024 * 1024;
const TIPOS_CROQUIS = ['image/png', 'image/jpeg', 'image/webp'];

const AJUSTES: { valor: FondoAjuste; etiqueta: string }[] = [
  { valor: 'contener', etiqueta: 'Contener' },
  { valor: 'cubrir', etiqueta: 'Cubrir' },
  { valor: 'estirar', etiqueta: 'Estirar' },
];

/** Valida en el cliente lo mismo que el servidor; devuelve el motivo del rechazo o `null`. */
export function errorDeCroquis(archivo: Pick<File, 'type' | 'size'>): string | null {
  if (!TIPOS_CROQUIS.includes(archivo.type)) return 'Solo se permiten imágenes PNG, JPG o WebP.';
  if (archivo.size > MAX_CROQUIS) return 'La imagen supera el límite de 5 MB.';
  return null;
}

/** Subida, reemplazo y ajuste del croquis que se dibuja detrás del plano. */
export function PanelCroquis({
  planoId,
  tieneCroquis,
  opacidad,
  ajuste,
  visible,
  onOpacidad,
  onAjuste,
  onVisible,
}: {
  planoId: string;
  tieneCroquis: boolean;
  opacidad: number;
  ajuste: FondoAjuste;
  /** Interruptor local: oculta el croquis mientras se edita, sin tocar el plano. */
  visible: boolean;
  onOpacidad: (valor: number) => void;
  onAjuste: (valor: FondoAjuste) => void;
  onVisible: (valor: boolean) => void;
}) {
  const queryClient = useQueryClient();
  const inputRef = useRef<HTMLInputElement>(null);
  const [error, setError] = useState<string | null>(null);

  const alCambiar = () => queryClient.invalidateQueries({ queryKey: ['planos'] });

  const subir = useMutation({
    mutationFn: (archivo: File) => clubApi.subirFondo(planoId, archivo),
    onSuccess: () => {
      toast.success('Croquis cargado');
      void alCambiar();
    },
    onError: (e) => {
      setError(mensajeDeError(e));
      toast.error('No se pudo subir el croquis', { description: mensajeDeError(e) });
    },
  });

  const quitar = useMutation({
    mutationFn: () => clubApi.quitarFondo(planoId),
    onSuccess: () => {
      toast.success('Croquis eliminado');
      void alCambiar();
    },
    onError: (e) => toast.error('No se pudo quitar el croquis', { description: mensajeDeError(e) }),
  });

  const elegir = (archivo?: File | null) => {
    if (!archivo) return;
    const motivo = errorDeCroquis(archivo);
    setError(motivo);
    if (motivo) return;
    subir.mutate(archivo);
  };

  return (
    <div className="flex w-72 flex-col gap-3 rounded-lg border border-border bg-card p-3 shadow-lg" role="group" aria-label="Croquis del plano">
      <input
        ref={inputRef}
        type="file"
        accept="image/png,image/jpeg,image/webp"
        className="hidden"
        aria-label="Archivo del croquis"
        onChange={(evento) => {
          elegir(evento.target.files?.[0]);
          evento.target.value = '';
        }}
      />

      <div className="flex flex-wrap gap-2">
        <Button type="button" size="sm" loading={subir.isPending} leftIcon={tieneCroquis ? <Upload size={14} /> : <ImagePlus size={14} />} onClick={() => inputRef.current?.click()}>
          {tieneCroquis ? 'Reemplazar' : 'Subir croquis'}
        </Button>
        {tieneCroquis && (
          <Button type="button" size="sm" variant="ghost" loading={quitar.isPending} leftIcon={<Trash2 size={14} />} onClick={() => quitar.mutate()}>
            Quitar
          </Button>
        )}
        {subir.isPending && <Loader2 size={16} className="animate-spin text-muted-foreground" />}
      </div>

      {error && (
        <p role="alert" className="text-xs text-destructive-fg">
          {error}
        </p>
      )}
      {!tieneCroquis && !error && <p className="text-xs text-muted-foreground">PNG, JPG o WebP · máx. 5 MB</p>}

      {tieneCroquis && (
        <>
          <label className="flex flex-col gap-1 text-xs text-muted-foreground">
            Opacidad · {Math.round(opacidad * 100)}%
            <input
              type="range"
              min={0}
              max={1}
              step={0.05}
              value={opacidad}
              aria-label="Opacidad del croquis"
              onChange={(evento) => onOpacidad(Number(evento.target.value))}
            />
          </label>
          <label className="flex flex-col gap-1 text-xs text-muted-foreground">
            Ajuste
            <select
              value={ajuste}
              aria-label="Ajuste del croquis"
              onChange={(evento) => onAjuste(evento.target.value as FondoAjuste)}
              className="h-9 rounded-control border border-border bg-card px-2 text-xs text-foreground"
            >
              {AJUSTES.map((a) => (
                <option key={a.valor} value={a.valor}>
                  {a.etiqueta}
                </option>
              ))}
            </select>
          </label>
          <Button type="button" size="sm" variant="ghost" leftIcon={visible ? <EyeOff size={14} /> : <Eye size={14} />} onClick={() => onVisible(!visible)}>
            {visible ? 'Ocultar mientras edito' : 'Mostrar croquis'}
          </Button>
          <p className="text-[11px] text-muted-foreground">La opacidad y el ajuste se guardan con el plano.</p>
        </>
      )}
    </div>
  );
}
