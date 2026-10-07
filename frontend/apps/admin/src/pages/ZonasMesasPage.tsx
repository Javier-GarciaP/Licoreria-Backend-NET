import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  Input,
  Modal,
  PageHeader,
  Pill,
  Select,
} from '@licoreria/ui';
import type { Mesa, Zona } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { SalonTabs } from '../components/SalonTabs';
import { mensajeDeError } from '../lib/api';

const TIPOS_ZONA = ['Barra', 'Mesas', 'Juegos', 'Pista', 'Vip'] as const;

const esquemaZona = z.object({
  nombre: z.string().min(2, 'Ingresa el nombre'),
  tipo: z.enum(TIPOS_ZONA),
  activo: z.boolean(),
});

const esquemaMesa = z.object({
  zonaId: z.string().min(1, 'Selecciona la zona'),
  numero: z.string().min(1, 'Ingresa el número'),
  capacidad: z.coerce.number().int().min(1, 'Al menos 1'),
  forma: z.enum(['redonda', 'cuadrada', 'rectangular']),
  posX: z.coerce.number().min(0),
  posY: z.coerce.number().min(0),
  ancho: z.coerce.number().min(0.5),
  alto: z.coerce.number().min(0.5),
  activa: z.boolean(),
});

type FormularioZona = z.infer<typeof esquemaZona>;
type FormularioMesa = z.infer<typeof esquemaMesa>;

const ZONA_VACIA: FormularioZona = { nombre: '', tipo: 'Mesas', activo: true };
const MESA_VACIA: FormularioMesa = {
  zonaId: '',
  numero: '',
  capacidad: 4,
  forma: 'redonda',
  posX: 0,
  posY: 0,
  ancho: 1,
  alto: 1,
  activa: true,
};

export function ZonasMesasPage() {
  const [zonaEditando, setZonaEditando] = useState<Zona | null>(null);
  const [zonaCreando, setZonaCreando] = useState(false);
  const [zonaEliminar, setZonaEliminar] = useState<Zona | null>(null);
  const [mesaEditando, setMesaEditando] = useState<Mesa | null>(null);
  const [mesaCreando, setMesaCreando] = useState(false);
  const [mesaEliminar, setMesaEliminar] = useState<Mesa | null>(null);
  const queryClient = useQueryClient();

  const zonaForm = useForm<FormularioZona>({ resolver: zodResolver(esquemaZona), defaultValues: ZONA_VACIA });
  const mesaForm = useForm<FormularioMesa>({ resolver: zodResolver(esquemaMesa), defaultValues: MESA_VACIA });

  const zonas = useQuery({ queryKey: ['zonas'], queryFn: clubApi.zonas });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas() });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['zonas'] });
    queryClient.invalidateQueries({ queryKey: ['mesas'] });
  };

  const guardarZona = useMutation({
    mutationFn: (datos: FormularioZona) =>
      zonaEditando
        ? clubApi.actualizarZona(zonaEditando.id, { id: zonaEditando.id, ...datos })
        : clubApi.crearZona(datos),
    onSuccess: () => {
      toast.success(zonaEditando ? 'Zona actualizada' : 'Zona creada');
      setZonaCreando(false);
      setZonaEditando(null);
      zonaForm.reset(ZONA_VACIA);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminarZona = useMutation({
    mutationFn: (id: string) => clubApi.eliminarZona(id),
    onSuccess: () => {
      toast.success('Zona eliminada');
      setZonaEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const guardarMesa = useMutation({
    mutationFn: (datos: FormularioMesa) =>
      mesaEditando
        ? clubApi.actualizarMesa(mesaEditando.id, { id: mesaEditando.id, ...datos })
        : clubApi.crearMesa(datos),
    onSuccess: () => {
      toast.success(mesaEditando ? 'Mesa actualizada' : 'Mesa creada');
      setMesaCreando(false);
      setMesaEditando(null);
      mesaForm.reset(MESA_VACIA);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminarMesa = useMutation({
    mutationFn: (id: string) => clubApi.eliminarMesa(id),
    onSuccess: () => {
      toast.success('Mesa eliminada');
      setMesaEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Salón" subtitle="Zonas y mesas del local." />
      <SalonTabs />

      <Card>
        <CardHeader>
          <CardTitle>Zonas</CardTitle>
          <Button
            size="sm"
            onClick={() => {
              zonaForm.reset(ZONA_VACIA);
              setZonaCreando(true);
            }}
          >
            Nueva zona
          </Button>
        </CardHeader>
        <CardBody>
          <DataTable<Zona>
            rows={zonas.data ?? []}
            loading={zonas.isLoading}
            rowKey={(zona) => zona.id}
            empty="No hay zonas."
            columns={[
              { key: 'nombre', header: 'Nombre', render: (zona) => <span className="text-ink">{zona.nombre}</span> },
              { key: 'tipo', header: 'Tipo', render: (zona) => <Pill tone="accent">{zona.tipo}</Pill> },
              {
                key: 'activo',
                header: 'Estado',
                render: (zona) => <Pill tone={zona.activo ? 'success' : 'danger'}>{zona.activo ? 'Activa' : 'Inactiva'}</Pill>,
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (zona) => (
                  <div className="flex justify-end gap-2">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => {
                        zonaForm.reset({ nombre: zona.nombre, tipo: zona.tipo, activo: zona.activo });
                        setZonaEditando(zona);
                      }}
                    >
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setZonaEliminar(zona)}>
                      Eliminar
                    </Button>
                  </div>
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Mesas</CardTitle>
          <Button
            size="sm"
            onClick={() => {
              mesaForm.reset(MESA_VACIA);
              setMesaCreando(true);
            }}
          >
            Nueva mesa
          </Button>
        </CardHeader>
        <CardBody>
          <DataTable<Mesa>
            rows={mesas.data ?? []}
            loading={mesas.isLoading}
            rowKey={(mesa) => mesa.id}
            empty="No hay mesas."
            columns={[
              { key: 'numero', header: 'Número', render: (mesa) => <span className="text-ink">{mesa.numero}</span> },
              { key: 'zona', header: 'Zona', render: (mesa) => mesa.zonaNombre },
              { key: 'capacidad', header: 'Capacidad', align: 'center', render: (mesa) => mesa.capacidad },
              { key: 'forma', header: 'Forma', render: (mesa) => mesa.forma },
              {
                key: 'pos',
                header: 'Posición',
                render: (mesa) => `${mesa.posX} , ${mesa.posY}`,
              },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (mesa) => (
                  <div className="flex justify-end gap-2">
                    <Button
                      size="sm"
                      variant="ghost"
                      onClick={() => {
                        mesaForm.reset({
                          zonaId: mesa.zonaId,
                          numero: mesa.numero,
                          capacidad: mesa.capacidad,
                          forma: mesa.forma as FormularioMesa['forma'],
                          posX: mesa.posX,
                          posY: mesa.posY,
                          ancho: mesa.ancho,
                          alto: mesa.alto,
                          activa: mesa.activa,
                        });
                        setMesaEditando(mesa);
                      }}
                    >
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setMesaEliminar(mesa)}>
                      Eliminar
                    </Button>
                  </div>
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={zonaCreando || Boolean(zonaEditando)}
        onClose={() => {
          setZonaCreando(false);
          setZonaEditando(null);
        }}
        title={zonaEditando ? 'Editar zona' : 'Nueva zona'}
        footer={
          <>
            <Button variant="ghost" onClick={() => { setZonaCreando(false); setZonaEditando(null); }}>
              Cancelar
            </Button>
            <Button type="submit" form="form-zona" loading={guardarZona.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-zona" className="flex flex-col gap-3" onSubmit={zonaForm.handleSubmit((d) => guardarZona.mutate(d))} noValidate>
          <Input label="Nombre" error={zonaForm.formState.errors.nombre?.message} {...zonaForm.register('nombre')} />
          <Select label="Tipo" {...zonaForm.register('tipo')}>
            {TIPOS_ZONA.map((tipo) => (
              <option key={tipo} value={tipo}>
                {tipo}
              </option>
            ))}
          </Select>
          <label className="flex items-center gap-2 text-sm text-muted">
            <input type="checkbox" {...zonaForm.register('activo')} />
            Activa
          </label>
        </form>
      </Modal>

      <Modal
        open={mesaCreando || Boolean(mesaEditando)}
        onClose={() => {
          setMesaCreando(false);
          setMesaEditando(null);
        }}
        title={mesaEditando ? 'Editar mesa' : 'Nueva mesa'}
        footer={
          <>
            <Button variant="ghost" onClick={() => { setMesaCreando(false); setMesaEditando(null); }}>
              Cancelar
            </Button>
            <Button type="submit" form="form-mesa" loading={guardarMesa.isPending}>
              Guardar
            </Button>
          </>
        }
      >
        <form id="form-mesa" className="flex flex-col gap-3" onSubmit={mesaForm.handleSubmit((d) => guardarMesa.mutate(d))} noValidate>
          <div className="grid grid-cols-2 gap-3">
            <Select label="Zona" error={mesaForm.formState.errors.zonaId?.message} {...mesaForm.register('zonaId')}>
              <option value="">Selecciona…</option>
              {zonas.data?.map((zona) => (
                <option key={zona.id} value={zona.id}>
                  {zona.nombre}
                </option>
              ))}
            </Select>
            <Input label="Número" error={mesaForm.formState.errors.numero?.message} {...mesaForm.register('numero')} />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Capacidad" type="number" error={mesaForm.formState.errors.capacidad?.message} {...mesaForm.register('capacidad')} />
            <Select label="Forma" {...mesaForm.register('forma')}>
              <option value="redonda">Redonda</option>
              <option value="cuadrada">Cuadrada</option>
              <option value="rectangular">Rectangular</option>
            </Select>
          </div>
          <div className="grid grid-cols-4 gap-3">
            <Input label="Pos X" type="number" step="0.1" {...mesaForm.register('posX')} />
            <Input label="Pos Y" type="number" step="0.1" {...mesaForm.register('posY')} />
            <Input label="Ancho" type="number" step="0.1" {...mesaForm.register('ancho')} />
            <Input label="Alto" type="number" step="0.1" {...mesaForm.register('alto')} />
          </div>
          <label className="flex items-center gap-2 text-sm text-muted">
            <input type="checkbox" {...mesaForm.register('activa')} />
            Activa
          </label>
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(zonaEliminar)}
        title="Eliminar zona"
        description={`¿Seguro que deseas eliminar "${zonaEliminar?.nombre ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminarZona.isPending}
        onClose={() => setZonaEliminar(null)}
        onConfirm={() => zonaEliminar && eliminarZona.mutate(zonaEliminar.id)}
      />
      <ConfirmDialog
        open={Boolean(mesaEliminar)}
        title="Eliminar mesa"
        description={`¿Seguro que deseas eliminar la mesa "${mesaEliminar?.numero ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminarMesa.isPending}
        onClose={() => setMesaEliminar(null)}
        onConfirm={() => mesaEliminar && eliminarMesa.mutate(mesaEliminar.id)}
      />
    </div>
  );
}
