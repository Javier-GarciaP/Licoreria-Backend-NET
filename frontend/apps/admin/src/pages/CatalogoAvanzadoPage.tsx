import { useState, type ReactNode } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  cn,
  DataTable,
  Input,
  Modal,
  PageHeader,
  Pill,
} from '@licoreria/ui';
import { impuestosApi, listasPrecioApi, modificadoresApi, unidadesApi } from '@licoreria/api-client';
import { CatalogoTabs } from '../components/CatalogoTabs';
import { mensajeDeError } from '../lib/api';
import { formatUSD } from '../lib/format';

type Pestana = 'unidades' | 'impuestos' | 'listas' | 'modificadores';
type Campo = { name: string; label: string; tipo?: 'texto' | 'numero' | 'check'; step?: string };

const PESTANAS: { id: Pestana; label: string }[] = [
  { id: 'unidades', label: 'Unidades de medida' },
  { id: 'impuestos', label: 'Impuestos' },
  { id: 'listas', label: 'Listas de precio' },
  { id: 'modificadores', label: 'Modificadores' },
];

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

  const invalidar = () => invalidarKeys.forEach((key) => queryClient.invalidateQueries({ queryKey: [key] }));

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
      <CardHeader>
        <CardTitle>{titulo}</CardTitle>
        <Button size="sm" onClick={abrirCrear}>
          Nuevo
        </Button>
      </CardHeader>
      <CardBody>
        <DataTable<T>
          rows={items}
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
                <div className="flex justify-end gap-2">
                  <Button size="sm" variant="ghost" onClick={() => abrirEditar(fila)}>
                    Editar
                  </Button>
                  <Button size="sm" variant="ghost" onClick={() => setConfirmar({ id: fila.id })}>
                    Eliminar
                  </Button>
                </div>
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
              <label key={campo.name} className="flex items-center gap-2 text-sm text-ink">
                <input
                  type="checkbox"
                  className="h-4 w-4 rounded-control border-hairline"
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

      <Modal
        open={Boolean(confirmar)}
        onClose={() => setConfirmar(null)}
        title={`Eliminar ${titulo.toLowerCase()}`}
        footer={
          <>
            <Button variant="ghost" onClick={() => setConfirmar(null)}>
              Cancelar
            </Button>
            <Button variant="danger" loading={borrar.isPending} onClick={() => confirmar && borrar.mutate(confirmar.id)}>
              Eliminar
            </Button>
          </>
        }
      >
        <p className="text-sm text-muted">¿Confirmas eliminar este registro?</p>
      </Modal>
    </Card>
  );
}

export function CatalogoAvanzadoPage() {
  const [pestana, setPestana] = useState<Pestana>('unidades');

  const unidades = useQuery({ queryKey: ['unidades'], queryFn: unidadesApi.listar });
  const impuestos = useQuery({ queryKey: ['impuestos'], queryFn: impuestosApi.listar });
  const listas = useQuery({ queryKey: ['listas-precio'], queryFn: listasPrecioApi.listar });
  const modificadores = useQuery({ queryKey: ['modificadores'], queryFn: modificadoresApi.listar });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Catálogo" subtitle="Productos, categorías, marcas y precios." />
      <CatalogoTabs />

      <nav aria-label="Secciones de catálogo avanzado" className="flex flex-wrap gap-2">
        {PESTANAS.map((item) => (
          <button
            key={item.id}
            type="button"
            onClick={() => setPestana(item.id)}
            className={cn(
              'rounded-pill border px-4 py-1.5 text-sm transition',
              pestana === item.id ? 'border-accent text-accent-ink' : 'border-hairline text-muted hover:text-ink',
            )}
          >
            {item.label}
          </button>
        ))}
      </nav>

      {pestana === 'unidades' && (
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
      )}

      {pestana === 'impuestos' && (
        <Seccion
          titulo="Impuesto"
          items={impuestos.data ?? []}
          cargando={impuestos.isLoading}
          campos={[
            { name: 'nombre', label: 'Nombre' },
            { name: 'porcentaje', label: 'Porcentaje', tipo: 'numero', step: '0.01' },
            { name: 'activo', label: 'Activo', tipo: 'check' },
          ]}
          valorInicial={{ nombre: '', porcentaje: 0, activo: true }}
          aValores={(item) => ({ nombre: item.nombre, porcentaje: item.porcentaje, activo: item.activo })}
          aBody={(valores, id) => (id ? { ...valores, id } : valores)}
          api={impuestosApi}
          invalidarKeys={['impuestos']}
          columnas={[
            { key: 'nombre', header: 'Nombre', render: (fila) => String(fila.nombre) },
            { key: 'porcentaje', header: '%', align: 'right', render: (fila) => `${Number(fila.porcentaje)}%` },
            {
              key: 'activo',
              header: 'Estado',
              render: (fila) => <Pill tone={fila.activo ? 'success' : 'neutral'}>{fila.activo ? 'Activo' : 'Inactivo'}</Pill>,
            },
          ]}
        />
      )}

      {pestana === 'listas' && (
        <Seccion
          titulo="Lista de precio"
          items={listas.data ?? []}
          cargando={listas.isLoading}
          campos={[
            { name: 'nombre', label: 'Nombre' },
            { name: 'descripcion', label: 'Descripción' },
            { name: 'esPredeterminada', label: 'Predeterminada', tipo: 'check' },
            { name: 'activo', label: 'Activa', tipo: 'check' },
          ]}
          valorInicial={{ nombre: '', descripcion: '', esPredeterminada: false, activo: true }}
          aValores={(item) => ({
            nombre: item.nombre,
            descripcion: item.descripcion ?? '',
            esPredeterminada: item.esPredeterminada,
            activo: item.activo,
          })}
          aBody={(valores, id) => (id ? { ...valores, id } : valores)}
          api={listasPrecioApi}
          invalidarKeys={['listas-precio']}
          columnas={[
            { key: 'nombre', header: 'Nombre', render: (fila) => String(fila.nombre) },
            {
              key: 'predeterminada',
              header: 'Predeterminada',
              render: (fila) => (fila.esPredeterminada ? <Pill tone="accent">Sí</Pill> : '—'),
            },
            {
              key: 'activo',
              header: 'Estado',
              render: (fila) => <Pill tone={fila.activo ? 'success' : 'neutral'}>{fila.activo ? 'Activa' : 'Inactiva'}</Pill>,
            },
          ]}
        />
      )}

      {pestana === 'modificadores' && (
        <Seccion
          titulo="Modificador"
          items={modificadores.data ?? []}
          cargando={modificadores.isLoading}
          campos={[
            { name: 'nombre', label: 'Nombre' },
            { name: 'precioAdicional', label: 'Precio adicional USD', tipo: 'numero', step: '0.01' },
            { name: 'activo', label: 'Activo', tipo: 'check' },
          ]}
          valorInicial={{ nombre: '', precioAdicional: 0, activo: true }}
          aValores={(item) => ({ nombre: item.nombre, precioAdicional: item.precioAdicional, activo: item.activo })}
          aBody={(valores, id) => (id ? { ...valores, id } : valores)}
          api={modificadoresApi}
          invalidarKeys={['modificadores']}
          columnas={[
            { key: 'nombre', header: 'Nombre', render: (fila) => String(fila.nombre) },
            {
              key: 'precio',
              header: 'Precio',
              align: 'right',
              render: (fila) => formatUSD(Number(fila.precioAdicional)),
            },
            {
              key: 'activo',
              header: 'Estado',
              render: (fila) => <Pill tone={fila.activo ? 'success' : 'neutral'}>{fila.activo ? 'Activo' : 'Inactivo'}</Pill>,
            },
          ]}
        />
      )}
    </div>
  );
}
