import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { ArrowLeft, Save } from 'lucide-react';
import { Button, cn, Input } from '@licoreria/ui';
import type { Evento } from '@licoreria/types';
import { PanelImagenEvento } from './PanelImagenEvento';

const esquemaEvento = z.object({
  titulo: z.string().min(2, 'Ingresa el título'),
  descripcion: z.string().min(2, 'Ingresa la descripción'),
  fechaInicio: z.string().min(1, 'Selecciona la fecha de inicio'),
  fechaFin: z.string().optional(),
  imagenUrl: z.string().optional(),
  publicado: z.boolean(),
  activo: z.boolean(),
});

export type FormularioEventoForm = z.infer<typeof esquemaEvento>;

const VACIO: FormularioEventoForm = {
  titulo: '',
  descripcion: '',
  fechaInicio: '',
  fechaFin: '',
  imagenUrl: '',
  publicado: false,
  activo: true,
};

function desdeEvento(evento: Evento): FormularioEventoForm {
  return {
    titulo: evento.titulo,
    descripcion: evento.descripcion,
    fechaInicio: evento.fechaInicio.slice(0, 16),
    fechaFin: evento.fechaFin ? evento.fechaFin.slice(0, 16) : '',
    imagenUrl: evento.imagenUrl ?? '',
    publicado: evento.publicado,
    activo: evento.activo,
  };
}

/** Editor de evento a página completa (patrón de editor de productos). */
export function FormularioEvento({
  evento,
  guardando,
  onCancelar,
  onGuardar,
}: {
  evento?: Evento | null;
  guardando: boolean;
  onCancelar: () => void;
  onGuardar: (datos: FormularioEventoForm) => void;
}) {
  const eventoForm = useForm<FormularioEventoForm>({
    resolver: zodResolver(esquemaEvento),
    defaultValues: evento ? desdeEvento(evento) : VACIO,
  });
  const imagenUrl = eventoForm.watch('imagenUrl');

  return (
    <form className="flex h-full min-h-0 flex-col gap-4" onSubmit={eventoForm.handleSubmit(onGuardar)} noValidate>
      <div className="flex flex-wrap items-center gap-2 border-b border-border pb-2">
        <Button variant="ghost" size="sm" type="button" onClick={onCancelar} aria-label="Volver">
          <ArrowLeft size={16} />
        </Button>
        <div>
          <p className="text-base font-medium tracking-tighter2 text-foreground">
            {evento ? `Editar · ${evento.titulo}` : 'Nuevo evento'}
          </p>
          <p className="text-xs text-muted-foreground">
            Define el evento, sus fechas y la imagen que se mostrará en la web pública.
          </p>
        </div>
        <Button type="submit" className="ml-auto" leftIcon={<Save size={15} />} loading={guardando}>
          {evento ? 'Guardar cambios' : 'Crear evento'}
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-4 lg:h-full lg:grid-cols-[35%_1fr]">
        <aside className="lg:self-start">
          <PanelImagenEvento imagenUrl={imagenUrl ?? null} onImagen={(url) => eventoForm.setValue('imagenUrl', url ?? '')} />
        </aside>

        <div className="flex min-w-0 flex-col gap-4">
          <section className="flex flex-col gap-2">
            <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Datos generales</p>
            <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
              <Input label="Título" error={eventoForm.formState.errors.titulo?.message} {...eventoForm.register('titulo')} />
              <label className="flex flex-col gap-1.5">
                <span className="text-sm font-medium text-foreground">Descripción</span>
                <textarea
                  rows={4}
                  placeholder="Describe el evento…"
                  className="w-full resize-y rounded-md border border-input bg-background px-3 py-2 text-sm shadow-sm transition-colors placeholder:text-muted-foreground focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
                  {...eventoForm.register('descripcion')}
                />
                {eventoForm.formState.errors.descripcion?.message && (
                  <span className="text-sm text-destructive-fg">{eventoForm.formState.errors.descripcion.message}</span>
                )}
              </label>
            </div>
          </section>

          <section className="flex flex-col gap-2">
            <p className="text-xs font-medium uppercase tracking-tighter2 text-muted-foreground">Fechas y publicación</p>
            <div className="grid grid-cols-1 gap-2 sm:grid-cols-2">
              <Input label="Inicio" type="datetime-local" error={eventoForm.formState.errors.fechaInicio?.message} {...eventoForm.register('fechaInicio')} />
              <Input label="Fin (opcional)" type="datetime-local" {...eventoForm.register('fechaFin')} />
            </div>
            <label className={cn('flex items-center gap-2 text-sm text-muted-foreground')}>
              <input type="checkbox" {...eventoForm.register('publicado')} />
              Publicado en la web pública
            </label>
            {evento && (
              <label className={cn('flex items-center gap-2 text-sm text-muted-foreground')}>
                <input type="checkbox" {...eventoForm.register('activo')} />
                Activo
              </label>
            )}
          </section>
        </div>
      </div>
    </form>
  );
}