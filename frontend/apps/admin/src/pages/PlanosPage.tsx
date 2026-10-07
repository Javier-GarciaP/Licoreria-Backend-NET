import { useEffect, useState } from 'react';
import { DndContext, useDraggable, type DragEndEvent } from '@dnd-kit/core';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  Input,
  Modal,
  PageHeader,
  Select,
  Skeleton,
} from '@licoreria/ui';
import type { Plano, PlanoElemento, Zona } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { SalonTabs } from '../components/SalonTabs';
import { mensajeDeError } from '../lib/api';

const ESCALA = 56;
const TIPOS = ['mesa', 'barra', 'bano', 'pista', 'vip', 'puerta'];

type Elemento = PlanoElemento;

let contador = 0;
const nuevoId = () => `nuevo-${Date.now()}-${contador++}`;

function ElementoCanvas({
  elemento,
  seleccionado,
  onSelect,
}: {
  elemento: Elemento;
  seleccionado: boolean;
  onSelect: () => void;
}) {
  const { attributes, listeners, setNodeRef, transform } = useDraggable({ id: elemento.id });
  return (
    <button
      ref={setNodeRef}
      type="button"
      {...listeners}
      {...attributes}
      onClick={onSelect}
      style={{
        left: elemento.posX * ESCALA,
        top: elemento.posY * ESCALA,
        width: Math.max(24, elemento.ancho * ESCALA - 8),
        height: Math.max(24, elemento.alto * ESCALA - 8),
        transform: transform ? `translate3d(${transform.x}px, ${transform.y}px, 0)` : undefined,
      }}
      className={`absolute flex touch-none items-center justify-center rounded-2xl border text-xs transition ${
        seleccionado ? 'border-accent bg-accent/20 text-ink' : 'border-hairline bg-elevated/60 text-muted'
      }`}
    >
      {elemento.etiqueta ?? elemento.tipo}
    </button>
  );
}

export function PlanosPage() {
  const [seleccionadoId, setSeleccionadoId] = useState<string | null>(null);
  const [elementos, setElementos] = useState<Elemento[]>([]);
  const [nombre, setNombre] = useState('');
  const [activo, setActivo] = useState(true);
  const [elementoSeleccionado, setElementoSeleccionado] = useState<string | null>(null);
  const [creando, setCreando] = useState(false);
  const [planosEliminar, setPlanosEliminar] = useState<Plano | null>(null);
  const queryClient = useQueryClient();

  const planos = useQuery({ queryKey: ['planos'], queryFn: clubApi.planos });
  const zonas = useQuery({ queryKey: ['zonas'], queryFn: clubApi.zonas });

  useEffect(() => {
    if (seleccionadoId || !planos.data || planos.data.length === 0) return;
    const plano = planos.data[0];
    setSeleccionadoId(plano.id);
    setNombre(plano.nombre);
    setActivo(plano.activo);
    setElementos(plano.elementos.map((elemento) => ({ ...elemento })));
  }, [planos.data, seleccionadoId]);

  const seleccionar = (plano: Plano) => {
    setSeleccionadoId(plano.id);
    setNombre(plano.nombre);
    setActivo(plano.activo);
    setElementos(plano.elementos.map((elemento) => ({ ...elemento })));
    setElementoSeleccionado(null);
  };

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['planos'] });

  const guardar = useMutation({
    mutationFn: () =>
      clubApi.actualizarPlano(seleccionadoId!, {
        id: seleccionadoId,
        nombre,
        activo,
        elementos: elementos.map((elemento) => ({
          zonaId: elemento.zonaId,
          tipo: elemento.tipo,
          etiqueta: elemento.etiqueta,
          posX: elemento.posX,
          posY: elemento.posY,
          ancho: elemento.ancho,
          alto: elemento.alto,
          rotacion: elemento.rotacion,
        })),
      }),
    onSuccess: () => {
      toast.success('Plano guardado');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo guardar el plano', { description: mensajeDeError(error) }),
  });

  const crear = useMutation({
    mutationFn: (nombrePlano: string) => clubApi.crearPlano({ nombre: nombrePlano, elementos: [] }),
    onSuccess: (plano) => {
      toast.success('Plano creado');
      setCreando(false);
      invalidar();
      seleccionar(plano);
    },
    onError: (error) => toast.error('No se pudo crear el plano', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => clubApi.eliminarPlano(id),
    onSuccess: () => {
      toast.success('Plano eliminado');
      setPlanosEliminar(null);
      setSeleccionadoId(null);
      setElementos([]);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const onDragEnd = (evento: DragEndEvent) => {
    const { active, delta } = evento;
    setElementos((actuales) =>
      actuales.map((elemento) =>
        elemento.id === active.id
          ? {
              ...elemento,
              posX: Math.max(0, Number((elemento.posX + delta.x / ESCALA).toFixed(2))),
              posY: Math.max(0, Number((elemento.posY + delta.y / ESCALA).toFixed(2))),
            }
          : elemento,
      ),
    );
  };

  const agregarElemento = () => {
    const id = nuevoId();
    setElementos((actuales) => [
      ...actuales,
      { id, zonaId: null, tipo: 'mesa', etiqueta: 'M', posX: 1, posY: 1, ancho: 1, alto: 1, rotacion: 0 },
    ]);
    setElementoSeleccionado(id);
  };

  const actualizarElemento = (id: string, cambios: Partial<Elemento>) =>
    setElementos((actuales) => actuales.map((elemento) => (elemento.id === id ? { ...elemento, ...cambios } : elemento)));

  const eliminarElemento = (id: string) => {
    setElementos((actuales) => actuales.filter((elemento) => elemento.id !== id));
    setElementoSeleccionado(null);
  };

  const elemento = elementos.find((item) => item.id === elementoSeleccionado) ?? null;
  const anchoCanvas = Math.max(12, ...elementos.map((item) => item.posX + item.ancho + 1)) * ESCALA;
  const altoCanvas = Math.max(8, ...elementos.map((item) => item.posY + item.alto + 1)) * ESCALA;

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Salón"
        subtitle="Editor de planos del local."
        actions={<Button onClick={() => setCreando(true)}>Nuevo plano</Button>}
      />
      <SalonTabs />

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-[260px_1fr]">
        <Card>
          <CardHeader>
            <CardTitle>Planos</CardTitle>
          </CardHeader>
          <CardBody className="flex flex-col gap-2">
            {planos.isLoading ? (
              <Skeleton className="h-24 w-full" />
            ) : (
              (planos.data ?? []).map((plano) => (
                <div key={plano.id} className="flex items-center justify-between gap-2">
                  <button
                    onClick={() => seleccionar(plano)}
                    className={`flex-1 rounded-2xl px-3 py-2 text-left text-sm transition ${
                      seleccionadoId === plano.id ? 'bg-elevated text-ink' : 'text-muted hover:text-ink'
                    }`}
                  >
                    {plano.nombre}
                    <span className="ml-2 text-xs text-muted">v{plano.version}</span>
                  </button>
                  <button className="text-danger text-xs" onClick={() => setPlanosEliminar(plano)}>
                    Eliminar
                  </button>
                </div>
              ))
            )}
            {(planos.data?.length ?? 0) === 0 && <p className="text-sm text-muted">Sin planos.</p>}
          </CardBody>
        </Card>

        <div className="flex flex-col gap-4">
          {seleccionadoId ? (
            <>
              <Card>
                <CardHeader>
                  <div className="flex flex-wrap items-center gap-2">
                    <Input placeholder="Nombre del plano" value={nombre} onChange={(evento) => setNombre(evento.target.value)} />
                    <label className="flex items-center gap-2 text-sm text-muted">
                      <input type="checkbox" checked={activo} onChange={(evento) => setActivo(evento.target.checked)} />
                      Activo
                    </label>
                  </div>
                  <div className="flex gap-2">
                    <Button size="sm" variant="ghost" onClick={agregarElemento}>
                      Agregar elemento
                    </Button>
                    <Button size="sm" loading={guardar.isPending} onClick={() => guardar.mutate()}>
                      Guardar
                    </Button>
                  </div>
                </CardHeader>
                <CardBody>
                  <div className="overflow-x-auto">
                    <DndContext onDragEnd={onDragEnd}>
                      <div
                        className="relative rounded-card border border-hairline bg-elevated/20"
                        style={{ width: anchoCanvas, height: altoCanvas, minWidth: '100%' }}
                      >
                        {elementos.map((item) => (
                          <ElementoCanvas
                            key={item.id}
                            elemento={item}
                            seleccionado={elementoSeleccionado === item.id}
                            onSelect={() => setElementoSeleccionado(item.id)}
                          />
                        ))}
                      </div>
                    </DndContext>
                  </div>
                </CardBody>
              </Card>

              {elemento && (
                <Card>
                  <CardHeader>
                    <CardTitle>Elemento</CardTitle>
                    <Button size="sm" variant="ghost" onClick={() => eliminarElemento(elemento.id)}>
                      Quitar
                    </Button>
                  </CardHeader>
                  <CardBody className="grid grid-cols-2 gap-3 sm:grid-cols-4">
                    <Select
                      label="Tipo"
                      value={elemento.tipo}
                      onChange={(evento) => actualizarElemento(elemento.id, { tipo: evento.target.value })}
                    >
                      {TIPOS.map((tipo) => (
                        <option key={tipo} value={tipo}>
                          {tipo}
                        </option>
                      ))}
                    </Select>
                    <Input
                      label="Etiqueta"
                      value={elemento.etiqueta ?? ''}
                      onChange={(evento) => actualizarElemento(elemento.id, { etiqueta: evento.target.value })}
                    />
                    <Select
                      label="Zona"
                      value={elemento.zonaId ?? ''}
                      onChange={(evento) => actualizarElemento(elemento.id, { zonaId: evento.target.value || null })}
                    >
                      <option value="">Sin zona</option>
                      {zonas.data?.map((zona: Zona) => (
                        <option key={zona.id} value={zona.id}>
                          {zona.nombre}
                        </option>
                      ))}
                    </Select>
                    <Input
                      label="Rotación"
                      type="number"
                      value={elemento.rotacion}
                      onChange={(evento) => actualizarElemento(elemento.id, { rotacion: Number(evento.target.value) })}
                    />
                    <Input
                      label="Ancho"
                      type="number"
                      step="0.1"
                      value={elemento.ancho}
                      onChange={(evento) => actualizarElemento(elemento.id, { ancho: Number(evento.target.value) })}
                    />
                    <Input
                      label="Alto"
                      type="number"
                      step="0.1"
                      value={elemento.alto}
                      onChange={(evento) => actualizarElemento(elemento.id, { alto: Number(evento.target.value) })}
                    />
                    <Input label="Pos X" type="number" step="0.1" value={elemento.posX} readOnly />
                    <Input label="Pos Y" type="number" step="0.1" value={elemento.posY} readOnly />
                  </CardBody>
                </Card>
              )}
            </>
          ) : (
            <Card>
              <CardBody>
                <p className="py-16 text-center text-sm text-muted">
                  Crea un plano o selecciona uno para editarlo.
                </p>
              </CardBody>
            </Card>
          )}
        </div>
      </div>

      <ModalCrearPlano open={creando} onClose={() => setCreando(false)} onCrear={(valor) => crear.mutate(valor)} cargando={crear.isPending} />

      <ConfirmDialog
        open={Boolean(planosEliminar)}
        title="Eliminar plano"
        description={`¿Seguro que deseas eliminar "${planosEliminar?.nombre ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPlanosEliminar(null)}
        onConfirm={() => planosEliminar && eliminar.mutate(planosEliminar.id)}
      />
    </div>
  );
}

function ModalCrearPlano({
  open,
  onClose,
  onCrear,
  cargando,
}: {
  open: boolean;
  onClose: () => void;
  onCrear: (nombre: string) => void;
  cargando: boolean;
}) {
  const [valor, setValor] = useState('');
  return (
    <Modal
      open={open}
      onClose={onClose}
      title="Nuevo plano"
      footer={
        <>
          <Button variant="ghost" onClick={onClose}>
            Cancelar
          </Button>
          <Button disabled={valor.trim().length < 2} loading={cargando} onClick={() => onCrear(valor.trim())}>
            Crear
          </Button>
        </>
      }
    >
      <Input label="Nombre del plano" value={valor} onChange={(evento) => setValor(evento.target.value)} />
    </Modal>
  );
}
