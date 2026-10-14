import { useMemo, useState, type ReactNode } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Pencil, Plus, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  Input,
  Modal,
} from '@licoreria/ui';
import { unidadesApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { mensajeDeError } from '../lib/api';
import { contiene } from '../lib/filtros';

type Campo = { name: string; label: string; tipo?: 'texto' | 'numero' | 'check'; step?: string };

function Seccion<T extends { id: string }>({
  titulo,
  items,
  cargando,
  columnas,
  campos,
  valorInicial,
  aValores,
  aBody,
  api,
  invalidarKeys,
}: {
  titulo: string;
  items: T[];
  cargando: boolean;
  columnas: { key: string; header: string; align?: 'left' | 'right' | 'center'; render: (fila: T) => ReactNode }[];
  campos: Campo[];
  valorInicial: Record<string, unknown>;
  aValores: (item: T) => Record<string, unknown>;
  aBody: (valores: Record<string, unknown>, id?: string) => unknown;
  api: {
    crear: (body: unknown) => Promise<unknown>;
    actualizar: (id: string, body: unknown) => Promise<unknown>;
    eliminar: (id: string) => Promise<unknown>;
  };
  invalidarKeys: string[];
}) {
  const queryClient = useQueryClient();
  const [valores, setValores] = useState<Record<string, unknown>>(valorInicial);
  const [editando, setEditando] = useState<{ id: string } | null>(null);
  const [abierto, setAbierto] = useState(false);
  const [confirmar, setConfirmar] = useState<{ id: string } | null>(null);
  const [busqueda, setBusqueda] = useState('');

  const invalidar = () => invalidarKeys.forEach((key) => queryClient.invalidateQueries({ queryKey: [key] }));

  const filas = useMemo(
    () =>
      items.filter((item) =>
        Object.values(item).some((valor) => contiene(String(valor ?? ''), busqueda)),
      ),
    [items, busqueda],
  );

  const guardar = useMutation({
    mutationFn: () => (editando ? api.actualizar(editando.id, aBody(valores, editando.id)) : api.crear(aBody(valores))),
    onSuccess: () => {
      toast.success(editando ? `${titulo}: actualizado` : `${titulo}: creado`);
      cerrar();
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const borrar = useMutation({
    mutationFn: (id: string) => api.eliminar(id),
    onSuccess: () => {
      toast.success(`${titulo}: eliminado`);
      setConfirmar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const cerrar = () => {
    setAbierto(false);
    setEditando(null);
    setValores(valorInicial);
  };

  const abrirCrear = () => {
    setValores(valorInicial);
    setEditando(null);
    setAbierto(true);
  };

  const abrirEditar = (item: T) => {
    setValores(aValores(item));
    setEditando({ id: item.id });
    setAbierto(true);
  };

  return (
    <Card>
      <CardHeader className="flex flex-wrap items-center justify-between gap-3">
        <Buscador
          placeholder={`Buscar ${titulo.toLowerCase()}…`}
          value={busqueda}
          onCambio={setBusqueda}
        />
        <Button size="sm" leftIcon={<Plus size={15} />} onClick={abrirCrear}>
          Nuevo
        </Button>
      </CardHeader>
      <CardBody>
        <DataTable<T>
          rows={filas}
          loading={cargando}
          rowKey={(fila) => fila.id}
          empty={`Sin ${titulo.toLowerCase()}.`}
          columns={[
            ...columnas,
            {
              key: 'acciones',
              header: '',
              align: 'right',
              render: (fila) => (
                <ActionMenu
                  label={`Acciones de ${fila.id}`}
                  options={[
                    { label: 'Editar', icon: <Pencil size={15} />, onClick: () => abrirEditar(fila) },
                    { label: 'Eliminar', icon: <Trash2 size={15} />, danger: true, onClick: () => setConfirmar({ id: fila.id }) },
                  ]}
                />
              ),
            },
          ]}
        />
      </CardBody>

      <Modal
        open={abierto}
        onClose={cerrar}
        title={editando ? `Editar ${titulo.toLowerCase()}` : `Nuevo ${titulo.toLowerCase()}`}
        footer={
          <>
            <Button variant="ghost" onClick={cerrar}>
              Cancelar
            </Button>
            <Button loading={guardar.isPending} onClick={() => guardar.mutate()}>
              Guardar
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-3">
          {campos.map((campo) =>
            campo.tipo === 'check' ? (
              <label key={campo.name} className="flex items-center gap-2 text-sm text-foreground">
                <input
                  type="checkbox"
                  className="h-4 w-4 rounded-control border-border"
                  checked={Boolean(valores[campo.name])}
                  onChange={(evento) => setValores((v) => ({ ...v, [campo.name]: evento.target.checked }))}
                />
                {campo.label}
              </label>
            ) : (
              <Input
                key={campo.name}
                label={campo.label}
                type={campo.tipo === 'numero' ? 'number' : 'text'}
                step={campo.step}
                value={String(valores[campo.name] ?? '')}
                onChange={(evento) =>
                  setValores((v) => ({
                    ...v,
                    [campo.name]: campo.tipo === 'numero' ? Number(evento.target.value) : evento.target.value,
                  }))
                }
              />
            ),
          )}
        </div>
      </Modal>

      <ConfirmDialog
        open={Boolean(confirmar)}
        title={`Eliminar ${titulo.toLowerCase()}`}
        description={`¿Confirmas eliminar este registro de ${titulo.toLowerCase()}?`}
        confirmLabel="Eliminar"
        loading={borrar.isPending}
        onClose={() => setConfirmar(null)}
        onConfirm={() => confirmar && borrar.mutate(confirmar.id)}
      />
    </Card>
  );
}

export function CatalogoAvanzadoPage() {
  const unidades = useQuery({ queryKey: ['unidades'], queryFn: unidadesApi.listar });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Seccion
        titulo="Unidad de medida"
        items={unidades.data ?? []}
        cargando={unidades.isLoading}
        campos={[
          { name: 'nombre', label: 'Nombre' },
          { name: 'abreviatura', label: 'Abreviatura' },
        ]}
        valorInicial={{ nombre: '', abreviatura: '' }}
        aValores={(item) => ({ nombre: item.nombre, abreviatura: item.abreviatura })}
        aBody={(valores) => valores}
        api={unidadesApi}
        invalidarKeys={['unidades']}
        columnas={[
          { key: 'nombre', header: 'Nombre', render: (fila) => String(fila.nombre) },
          { key: 'abreviatura', header: 'Abreviatura', render: (fila) => String(fila.abreviatura) },
        ]}
      />
    </div>
  );
}