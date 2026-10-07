import { useEffect } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Button, Card, CardBody, CardHeader, CardTitle, Input, PageHeader, Skeleton } from '@licoreria/ui';
import type { Horario } from '@licoreria/types';
import { contenidoApi } from '@licoreria/api-client';
import { ContenidoTabs } from '../components/ContenidoTabs';
import { mensajeDeError } from '../lib/api';

const DIAS = ['Domingo', 'Lunes', 'Martes', 'Miércoles', 'Jueves', 'Viernes', 'Sábado'];

const esquemaLocal = z.object({
  nombre: z.string().min(2, 'Ingresa el nombre'),
  descripcion: z.string().min(2, 'Ingresa la descripción'),
  direccion: z.string().min(2, 'Ingresa la dirección'),
  telefono: z.string().min(3, 'Ingresa el teléfono'),
  whatsapp: z.string().min(3, 'Ingresa el WhatsApp'),
  email: z.string().email('Correo inválido'),
  instagram: z.string().optional(),
  facebook: z.string().optional(),
  mapaUrl: z.string().optional(),
  logoUrl: z.string().optional(),
});

type FormularioLocal = z.infer<typeof esquemaLocal>;

function aTime(valor: string | null): string {
  return valor ? valor.slice(0, 5) : '';
}

function HorarioRow({ horario, onSave, guardando }: { horario: Horario; onSave: (body: unknown) => void; guardando: boolean }) {
  const form = useForm({
    defaultValues: {
      abierto: horario.abierto,
      apertura: aTime(horario.horaApertura),
      cierre: aTime(horario.horaCierre),
    },
  });

  return (
    <form
      className="grid grid-cols-2 items-center gap-2 rounded-2xl bg-elevated/30 px-3 py-2 sm:grid-cols-4"
      onSubmit={form.handleSubmit((datos) =>
        onSave({
          diaSemana: horario.diaSemana,
          abierto: datos.abierto,
          horaApertura: datos.apertura ? `${datos.apertura}:00` : null,
          horaCierre: datos.cierre ? `${datos.cierre}:00` : null,
        }),
      )}
    >
      <span className="text-sm text-ink">{DIAS[horario.diaSemana] ?? `Día ${horario.diaSemana}`}</span>
      <label className="flex items-center gap-2 text-xs text-muted">
        <input type="checkbox" {...form.register('abierto')} />
        Abierto
      </label>
      <input
        type="time"
        aria-label="Apertura"
        className="h-9 rounded-control border border-hairline bg-surface px-3 text-sm text-ink"
        {...form.register('apertura')}
      />
      <div className="flex items-center gap-2">
        <input
          type="time"
          aria-label="Cierre"
          className="h-9 w-full rounded-control border border-hairline bg-surface px-3 text-sm text-ink"
          {...form.register('cierre')}
        />
        <Button type="submit" size="sm" variant="ghost" loading={guardando}>
          Guardar
        </Button>
      </div>
    </form>
  );
}

export function LocalPage() {
  const queryClient = useQueryClient();
  const local = useQuery({ queryKey: ['local-info'], queryFn: contenidoApi.localInfo });
  const horarios = useQuery({ queryKey: ['horarios'], queryFn: contenidoApi.horarios });

  const form = useForm<FormularioLocal>({
    resolver: zodResolver(esquemaLocal),
    defaultValues: {
      nombre: '',
      descripcion: '',
      direccion: '',
      telefono: '',
      whatsapp: '',
      email: '',
      instagram: '',
      facebook: '',
      mapaUrl: '',
      logoUrl: '',
    },
  });

  useEffect(() => {
    if (local.data) {
      form.reset({
        nombre: local.data.nombre,
        descripcion: local.data.descripcion,
        direccion: local.data.direccion,
        telefono: local.data.telefono,
        whatsapp: local.data.whatsapp,
        email: local.data.email,
        instagram: local.data.instagram ?? '',
        facebook: local.data.facebook ?? '',
        mapaUrl: local.data.mapaUrl ?? '',
        logoUrl: local.data.logoUrl ?? '',
      });
    }
  }, [local.data, form]);

  const guardarLocal = useMutation({
    mutationFn: (datos: FormularioLocal) =>
      contenidoApi.actualizarLocalInfo({
        ...datos,
        instagram: datos.instagram || null,
        facebook: datos.facebook || null,
        mapaUrl: datos.mapaUrl || null,
        logoUrl: datos.logoUrl || null,
      }),
    onSuccess: () => {
      toast.success('Información actualizada');
      queryClient.invalidateQueries({ queryKey: ['local-info'] });
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const guardarHorario = useMutation({
    mutationFn: (body: unknown) => contenidoApi.guardarHorario(body),
    onSuccess: () => {
      toast.success('Horario actualizado');
      queryClient.invalidateQueries({ queryKey: ['horarios'] });
    },
    onError: (error) => toast.error('No se pudo guardar el horario', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Contenido" subtitle="Información del local y horarios de atención." />
      <ContenidoTabs />

      <Card>
        <CardHeader>
          <CardTitle>Información del local</CardTitle>
        </CardHeader>
        <CardBody>
          {local.isLoading ? (
            <Skeleton className="h-40 w-full" />
          ) : (
            <form className="flex flex-col gap-3" onSubmit={form.handleSubmit((d) => guardarLocal.mutate(d))} noValidate>
              <Input label="Nombre" error={form.formState.errors.nombre?.message} {...form.register('nombre')} />
              <Input label="Descripción" error={form.formState.errors.descripcion?.message} {...form.register('descripcion')} />
              <div className="grid grid-cols-2 gap-3">
                <Input label="Dirección" error={form.formState.errors.direccion?.message} {...form.register('direccion')} />
                <Input label="Teléfono" error={form.formState.errors.telefono?.message} {...form.register('telefono')} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <Input label="WhatsApp" error={form.formState.errors.whatsapp?.message} {...form.register('whatsapp')} />
                <Input label="Correo" type="email" error={form.formState.errors.email?.message} {...form.register('email')} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <Input label="Instagram" {...form.register('instagram')} />
                <Input label="Facebook" {...form.register('facebook')} />
              </div>
              <div className="grid grid-cols-2 gap-3">
                <Input label="Mapa (URL)" {...form.register('mapaUrl')} />
                <Input label="Logo (URL)" {...form.register('logoUrl')} />
              </div>
              <Button type="submit" loading={guardarLocal.isPending} className="self-start">
                Guardar información
              </Button>
            </form>
          )}
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Horarios de atención</CardTitle>
        </CardHeader>
        <CardBody className="flex flex-col gap-2">
          {horarios.isLoading ? (
            <Skeleton className="h-40 w-full" />
          ) : (
            (horarios.data ?? [])
              .slice()
              .sort((a, b) => a.diaSemana - b.diaSemana)
              .map((horario) => (
                <HorarioRow
                  key={horario.id}
                  horario={horario}
                  guardando={guardarHorario.isPending}
                  onSave={(body) => guardarHorario.mutate(body)}
                />
              ))
          )}
        </CardBody>
      </Card>
    </div>
  );
}
