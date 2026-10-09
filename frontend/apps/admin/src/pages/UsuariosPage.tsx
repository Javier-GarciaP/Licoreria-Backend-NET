import { useMemo, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { KeyRound, Pencil, Plus, ShieldCheck, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  Input,
  LimpiarFiltros,
  Modal,
  Pagination,
  Pill,
  Select,
} from '@licoreria/ui';
import type { Usuario } from '@licoreria/types';
import { usuariosApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';
import { formatDateTime } from '../lib/format';

const ROLES = ['Administrador', 'Cajero', 'Mesero', 'Barra', 'Cocina', 'Host', 'EditorContenido'] as const;

const esquemaUsuario = z.object({
  nombreCompleto: z.string().min(2, 'Ingresa el nombre'),
  email: z.string().email('Correo inválido'),
  rol: z.enum(ROLES),
  password: z.string().min(6, 'Mínimo 6 caracteres').or(z.literal('')),
  activo: z.boolean(),
});

const esquemaPassword = z.object({
  passwordNueva: z.string().min(6, 'Mínimo 6 caracteres'),
});

type FormularioUsuario = z.infer<typeof esquemaUsuario>;
type FormularioPassword = z.infer<typeof esquemaPassword>;

const VACIO: FormularioUsuario = {
  nombreCompleto: '',
  email: '',
  rol: 'Cajero',
  password: '',
  activo: true,
};

export function UsuariosPage() {
  const [page, setPage] = useState(1);
  const [busqueda, setBusqueda] = useState('');
  const [rol, setRol] = useState('');
  const [estado, setEstado] = useState('');
  const [editando, setEditando] = useState<Usuario | null>(null);
  const [creando, setCreando] = useState(false);
  const [cambiandoPassword, setCambiandoPassword] = useState<Usuario | null>(null);
  const [porEliminar, setPorEliminar] = useState<Usuario | null>(null);
  const queryClient = useQueryClient();

  const usuarioForm = useForm<FormularioUsuario>({ resolver: zodResolver(esquemaUsuario), defaultValues: VACIO });
  const passwordForm = useForm<FormularioPassword>({
    resolver: zodResolver(esquemaPassword),
    defaultValues: { passwordNueva: '' },
  });

  const usuarios = useQuery({
    queryKey: ['usuarios', page, busqueda],
    queryFn: () => usuariosApi.listar({ page, pageSize: 15, busqueda }),
  });

  const filas = useMemo(
    () =>
      (usuarios.data?.items ?? []).filter((usuario) => {
        if (rol && usuario.rol !== rol) return false;
        if (estado === 'activos' && !usuario.activo) return false;
        if (estado === 'inactivos' && usuario.activo) return false;
        return true;
      }),
    [usuarios.data, rol, estado],
  );

  const hayFiltros = Boolean(busqueda || rol || estado);

  const limpiarFiltros = () => {
    setBusqueda('');
    setRol('');
    setEstado('');
    setPage(1);
  };

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['usuarios'] });

  const guardar = useMutation({
    mutationFn: (datos: FormularioUsuario) =>
      editando
        ? usuariosApi.actualizar(editando.id, {
            id: editando.id,
            nombreCompleto: datos.nombreCompleto,
            email: datos.email,
            rol: datos.rol,
            activo: datos.activo,
          })
        : usuariosApi.crear({
            nombreCompleto: datos.nombreCompleto,
            email: datos.email,
            password: datos.password,
            rol: datos.rol,
            activo: datos.activo,
          }),
    onSuccess: () => {
      toast.success(editando ? 'Usuario actualizado' : 'Usuario creado');
      setCreando(false);
      setEditando(null);
      usuarioForm.reset(VACIO);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const cambiarPassword = useMutation({
    mutationFn: ({ id, password }: { id: string; password: string }) => usuariosApi.cambiarPassword(id, password),
    onSuccess: () => {
      toast.success('Contraseña actualizada');
      setCambiandoPassword(null);
      passwordForm.reset({ passwordNueva: '' });
    },
    onError: (error) => toast.error('No se pudo cambiar la contraseña', { description: mensajeDeError(error) }),
  });

  const revocar = useMutation({
    mutationFn: (id: string) => usuariosApi.revocarSesiones(id),
    onSuccess: () => toast.success('Sesiones revocadas'),
    onError: (error) => toast.error('No se pudo revocar', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => usuariosApi.eliminar(id),
    onSuccess: () => {
      toast.success('Usuario eliminado');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const cerrarFormulario = () => {
    setCreando(false);
    setEditando(null);
    usuarioForm.reset(VACIO);
  };

  const abrirCrear = () => {
    usuarioForm.reset(VACIO);
    setEditando(null);
    setCreando(true);
  };

  const abrirEditar = (usuario: Usuario) => {
    usuarioForm.reset({
      nombreCompleto: usuario.nombreCompleto,
      email: usuario.email,
      rol: usuario.rol as (typeof ROLES)[number],
      password: '',
      activo: usuario.activo,
    });
    setCreando(false);
    setEditando(usuario);
  };

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar nombre o correo…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <div className="ml-auto">
              <Button size="sm" leftIcon={<Plus size={15} />} onClick={abrirCrear}>
                Nuevo usuario
              </Button>
            </div>
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroDropdown
              label="Rol"
              opciones={ROLES.map((valor) => ({ valor, etiqueta: valor }))}
              valor={rol}
              onChange={setRol}
            />
            <FiltroDropdown
              label="Estado"
              opciones={[
                { valor: 'activos', etiqueta: 'Activos' },
                { valor: 'inactivos', etiqueta: 'Inactivos' },
              ]}
              valor={estado}
              onChange={setEstado}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Usuario>
            rows={filas}
            loading={usuarios.isLoading}
            rowKey={(usuario) => usuario.id}
            empty="No hay usuarios."
            columns={[
              {
                key: 'nombre',
                header: 'Nombre',
                render: (usuario) => (
                  <div>
                    <p className="text-foreground">{usuario.nombreCompleto}</p>
                    <p className="text-xs text-muted-foreground">{usuario.email}</p>
                  </div>
                ),
              },
              { key: 'rol', header: 'Rol', render: (usuario) => <Pill tone="accent">{usuario.rol}</Pill> },
              {
                key: 'activo',
                header: 'Estado',
                render: (usuario) => (
                  <Pill tone={usuario.activo ? 'success' : 'danger'}>{usuario.activo ? 'Activo' : 'Inactivo'}</Pill>
                ),
              },
              { key: 'creado', header: 'Creado', align: 'right', render: (usuario) => formatDateTime(usuario.createdAt) },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (usuario) => (
                  <ActionMenu
                    label={`Acciones de ${usuario.nombreCompleto}`}
                    options={[
                      { label: 'Editar', icon: <Pencil size={15} />, onClick: () => abrirEditar(usuario) },
                      {
                        label: 'Cambiar contraseña',
                        icon: <KeyRound size={15} />,
                        onClick: () => setCambiandoPassword(usuario),
                      },
                      { label: 'Revocar sesiones', icon: <ShieldCheck size={15} />, onClick: () => revocar.mutate(usuario.id) },
                      {
                        label: 'Eliminar',
                        icon: <Trash2 size={15} />,
                        danger: true,
                        onClick: () => setPorEliminar(usuario),
                      },
                    ]}
                  />
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={usuarios.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={creando || Boolean(editando)}
        onClose={cerrarFormulario}
        title={editando ? `Editar · ${editando.nombreCompleto}` : 'Nuevo usuario'}
        footer={
          <>
            <Button variant="ghost" onClick={cerrarFormulario}>
              Cancelar
            </Button>
            <Button type="submit" form="form-usuario" loading={guardar.isPending}>
              {editando ? 'Guardar cambios' : 'Crear usuario'}
            </Button>
          </>
        }
      >
        <form
          id="form-usuario"
          className="flex flex-col gap-3"
          onSubmit={usuarioForm.handleSubmit((datos) => guardar.mutate(datos))}
          noValidate
        >
          <Input label="Nombre completo" error={usuarioForm.formState.errors.nombreCompleto?.message} {...usuarioForm.register('nombreCompleto')} />
          <Input label="Correo" type="email" error={usuarioForm.formState.errors.email?.message} {...usuarioForm.register('email')} />
          <Select label="Rol" error={usuarioForm.formState.errors.rol?.message} {...usuarioForm.register('rol')}>
            {ROLES.map((rol) => (
              <option key={rol} value={rol}>
                {rol}
              </option>
            ))}
          </Select>
          {!editando && (
            <Input
              label="Contraseña"
              type="password"
              error={usuarioForm.formState.errors.password?.message}
              {...usuarioForm.register('password')}
            />
          )}
          <label className="flex items-center gap-2 text-sm text-muted-foreground">
            <input type="checkbox" {...usuarioForm.register('activo')} />
            Activo
          </label>
        </form>
      </Modal>

      <Modal
        open={Boolean(cambiandoPassword)}
        onClose={() => setCambiandoPassword(null)}
        title={`Cambiar contraseña`}
        size="sm"
        backdrop="none"
        footer={
          <>
            <Button variant="ghost" onClick={() => setCambiandoPassword(null)}>
              Cancelar
            </Button>
            <Button type="submit" form="form-password" loading={cambiarPassword.isPending}>
              Actualizar
            </Button>
          </>
        }
      >
        <p className="mb-3 text-sm text-muted-foreground">{cambiandoPassword?.nombreCompleto}</p>
        <form
          id="form-password"
          onSubmit={passwordForm.handleSubmit((datos) =>
            cambiandoPassword && cambiarPassword.mutate({ id: cambiandoPassword.id, password: datos.passwordNueva }),
          )}
          noValidate
        >
          <Input
            label="Nueva contraseña"
            type="password"
            error={passwordForm.formState.errors.passwordNueva?.message}
            {...passwordForm.register('passwordNueva')}
          />
        </form>
      </Modal>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar usuario"
        description={`¿Seguro que deseas eliminar a ${porEliminar?.nombreCompleto ?? ''}? Esta acción es de baja lógica.`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}