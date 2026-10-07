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

  const abrirCrear = () => {
    usuarioForm.reset(VACIO);
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
    setEditando(usuario);
  };

  const formularioAbierto = creando || Boolean(editando);

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Usuarios"
        subtitle="Personal del sistema, roles y estado."
        actions={<Button onClick={abrirCrear}>Nuevo usuario</Button>}
      />
      <Card>
        <CardHeader>
          <CardTitle>Cuentas</CardTitle>
          <div className="w-56">
            <Input
              placeholder="Buscar…"
              value={busqueda}
              onChange={(evento) => {
                setBusqueda(evento.target.value);
                setPage(1);
              }}
            />
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Usuario>
            rows={usuarios.data?.items ?? []}
            loading={usuarios.isLoading}
            rowKey={(usuario) => usuario.id}
            empty="No hay usuarios."
            columns={[
              {
                key: 'nombre',
                header: 'Nombre',
                render: (usuario) => (
                  <div>
                    <p className="text-ink">{usuario.nombreCompleto}</p>
                    <p className="text-xs text-muted">{usuario.email}</p>
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
                  <div className="flex justify-end gap-2">
                    <Button size="sm" variant="ghost" onClick={() => abrirEditar(usuario)}>
                      Editar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setCambiandoPassword(usuario)}>
                      Contraseña
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => revocar.mutate(usuario.id)}>
                      Revocar
                    </Button>
                    <Button size="sm" variant="ghost" onClick={() => setPorEliminar(usuario)}>
                      Eliminar
                    </Button>
                  </div>
                ),
              },
            ]}
          />
          <Pagination page={page} totalPages={usuarios.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>

      <Modal
        open={formularioAbierto}
        onClose={() => {
          setCreando(false);
          setEditando(null);
        }}
        title={editando ? 'Editar usuario' : 'Nuevo usuario'}
        footer={
          <>
            <Button
              variant="ghost"
              onClick={() => {
                setCreando(false);
                setEditando(null);
              }}
            >
              Cancelar
            </Button>
            <Button type="submit" form="form-usuario" loading={guardar.isPending}>
              Guardar
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
          <label className="flex items-center gap-2 text-sm text-muted">
            <input type="checkbox" {...usuarioForm.register('activo')} />
            Activo
          </label>
        </form>
      </Modal>

      <Modal
        open={Boolean(cambiandoPassword)}
        onClose={() => setCambiandoPassword(null)}
        title={`Cambiar contraseña · ${cambiandoPassword?.nombreCompleto ?? ''}`}
        footer={
          <>
            <Button variant="ghost" onClick={() => setCambiandoPassword(null)}>
              Cancelar
            </Button>
            <Button
              type="submit"
              form="form-password"
              loading={cambiarPassword.isPending}
            >
              Actualizar
            </Button>
          </>
        }
      >
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
