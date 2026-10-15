import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  FiltroDropdown,
  LimpiarFiltros,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  Input,
  Modal,
  Pill,
  Select,
} from '@licoreria/ui';
import type { Promocion } from '@licoreria/types';
import { promocionesApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatUSD } from '../lib/format';
import { contiene } from '../lib/filtros';
import { useAuth } from '../context/AuthContext';

const esquema = z.object({
  nombre: z.string().min(1, 'Ingresa el nombre'),
  tipo: z.enum(['Porcentaje', 'MontoFijo']),
  valor: z.coerce.number().positive('Debe ser mayor que 0'),
  activo: z.boolean().default(true),
  fechaInicio: z.string().optional(),
  fechaFin: z.string().optional(),
});

type Formulario = z.infer<typeof esquema>;

export function PromocionesPage() {
  const queryClient = useQueryClient();
  const { esAdmin, tienePermiso } = useAuth() as { esAdmin: boolean; tienePermiso: (clave: string) => boolean };
  const puedeGestionar = esAdmin || tienePermiso('promotions:manage');
  const [editando, setEditando] = useState<Promocion | null>(null);
  const [creando, setCreando] = useState(false);
  const [busqueda, setBusqueda] = useState('');
  const [estado, setEstado] = useState('');
  const [tipo, setTipo] = useState('');
  const [eliminar, setEliminar] = useState<Promocion | null>(null);

  const promociones = useQuery({ queryKey: ['promociones'], queryFn: promocionesApi.listar });

  const filas = (promociones.data ?? []).filter((fila) => {
    if (!contiene(fila.nombre, busqueda)) return false;
    if (estado === 'activas' && !fila.activo) return false;
    if (estado === 'inactivas' && fila.activo) return false;
    if (tipo && fila.tipo !== tipo) return false;
    return true;
  });
  const hayFiltros = Boolean(busqueda || estado || tipo);
  const limpiarFiltros = () => {
    setBusqueda('');
    setEstado('');
    setTipo('');
  };
  const form = useForm<Formulario>({
    resolver: zodResolver(esquema),
    defaultValues: { nombre: '', tipo: 'Porcentaje', valor: 0, activo: true, fechaInicio: '', fechaFin: '' },
  });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['promociones'] });

  const guardar = useMutation({
    mutationFn: (datos: Formulario) => {
      const body = {
        nombre: datos.nombre,
        tipo: datos.tipo,
        valor: datos.valor,
        activo: datos.activo,
        fechaInicio: datos.fechaInicio || null,
        fechaFin: datos.fechaFin || null,
      };
      return editando ? promocionesApi.actualizar(editando.id, { ...body, id: editando.id }) : promocionesApi.crear(body);
    },
    onSuccess: () => {
      toast.success(editando ? 'Promoción actualizada' : 'Promoción creada');
      cerrar();
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const borrar = useMutation({
    mutationFn: (id: string) => promocionesApi.eliminar(id),
    onSuccess: () => {
      toast.success('Promoción eliminada');
      setEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const cerrar = () => {
    setCreando(false);
    setEditando(null);
    form.reset({ nombre: '', tipo: 'Porcentaje', valor: 0, activo: true, fechaInicio: '', fechaFin: '' });
  };

  const abrirCrear = () => {
    form.reset({ nombre: '', tipo: 'Porcentaje', valor: 0, activo: true, fechaInicio: '', fechaFin: '' });
    setEditando(null);
    setCreando(true);
  };

  const abrirEditar = (promocion: Promocion) => {
    form.reset({
      nombre: promocion.nombre,
      tipo: promocion.tipo === 'MontoFijo' ? 'MontoFijo' : 'Porcentaje',
      valor: promocion.valor,
      activo: promocion.activo,
      fechaInicio: promocion.fechaInicio?.slice(0, 10) ?? '',
      fechaFin: promocion.fechaFin?.slice(0, 10) ?? '',
    });
    setEditando(promocion);
    setCreando(true);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador placeholder="Buscar promoción…" value={busqueda} onCambio={setBusqueda} />
            <div className="ml-auto">
              {puedeGestionar ? (
                <Button leftIcon={<Plus size={15} />} onClick={abrirCrear}>
                  Nueva promoción
                </Button>
              ) : null}
            </div>
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroDropdown
              label="Estado"
              opciones={[
                { valor: 'activas', etiqueta: 'Activas' },
                { valor: 'inactivas', etiqueta: 'Inactivas' },
              ]}
              valor={estado}
              onChange={setEstado}
            />
            <FiltroDropdown
              label="Tipo"
              opciones={[
                { valor: 'Porcentaje', etiqueta: 'Porcentaje' },
                { valor: 'MontoFijo', etiqueta: 'Monto fijo' },
              ]}
              valor={tipo}
              onChange={setTipo}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Promocion>
            rows={filas}
            loading={promociones.isLoading}
            rowKey={(fila) => fila.id}
            empty={hayFiltros ? 'Sin promociones con esos filtros.' : 'No hay promociones.'}
            columns={[
              { key: 'nombre', header: 'Nombre', render: (fila) => fila.nombre },
              { key: 'tipo', header: 'Tipo', render: (fila) => (fila.tipo === 'MontoFijo' ? 'Monto fijo' : 'Porcentaje') },
              {
                key: 'valor',
                header: 'Valor',
                align: 'right',
                render: (fila) => (fila.tipo === 'MontoFijo' ? formatUSD(fila.valor) : `${fila.valor}%`),
              },
              {
                key: 'estado',
                header: 'Estado',
                render: (fila) => <Pill tone={fila.activo ? 'success' : 'neutral'}>{fila.activo ? 'Activa' : 'Inactiva'}</Pill>,
              },
              ...(puedeGestionar
                ? [
                    {
                      key: 'acciones',
                      header: '',
                      align: 'right' as const,
                      render: (fila: Promocion) => (
                        <ActionMenu
                          label={`Acciones de ${fila.nombre}`}
                          options={[
                            { label: 'Editar', icon: <Pencil size={15} />, onClick: () => abrirEditar(fila) },
                            { label: 'Eliminar', icon: <Trash2 size={15} />, danger: true, onClick: () => setEliminar(fila) },
                          ]}
                        />
                      ),
                    },
                  ]
                : []),
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={creando}
        onClose={cerrar}
        title={editando ? 'Editar promoción' : 'Nueva promoción'}
        footer={
          <>
            <Button variant="ghost" onClick={cerrar}>
              Cancelar
            </Button>
            <Button type="submit" form="form-promocion" loading={guardar.isPending}>
              {editando ? 'Guardar cambios' : 'Crear promoción'}
            </Button>
          </>
        }
      >
        <form
          id="form-promocion"
          className="flex flex-col gap-3"
          onSubmit={form.handleSubmit((datos) => guardar.mutate(datos))}
          noValidate
        >
          <Input label="Nombre" error={form.formState.errors.nombre?.message} {...form.register('nombre')} />
          <div className="grid grid-cols-2 gap-3">
            <Select label="Tipo" {...form.register('tipo')}>
              <option value="Porcentaje">Porcentaje</option>
              <option value="MontoFijo">Monto fijo</option>
            </Select>
            <Input
              label="Valor"
              type="number"
              step="0.01"
              error={form.formState.errors.valor?.message}
              {...form.register('valor')}
            />
          </div>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Desde" type="date" {...form.register('fechaInicio')} />
            <Input label="Hasta" type="date" {...form.register('fechaFin')} />
          </div>
          <label className="flex items-center gap-2 text-sm text-foreground">
            <input type="checkbox" className="h-4 w-4 rounded-control border-border" {...form.register('activo')} />
            Activa
          </label>
        </form>
      </Modal>

      <Modal
        open={Boolean(eliminar)}
        onClose={() => setEliminar(null)}
        title="Eliminar promoción"
        footer={
          <>
            <Button variant="ghost" onClick={() => setEliminar(null)}>
              Cancelar
            </Button>
            <Button variant="danger" loading={borrar.isPending} onClick={() => eliminar && borrar.mutate(eliminar.id)}>
              Eliminar
            </Button>
          </>
        }
      >
        <p className="text-sm text-muted-foreground">
          ¿Eliminar la promoción <span className="text-foreground">{eliminar?.nombre}</span>? Esta acción no se puede deshacer.
        </p>
      </Modal>
    </div>
  );
}
