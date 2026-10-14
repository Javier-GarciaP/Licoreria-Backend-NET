import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useLocation, useNavigate, useParams } from 'react-router-dom';
import { toast } from 'sonner';
import { motion, LayoutGroup } from 'framer-motion';
import { CalendarDays, Pencil, Trash2 } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  Button,
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  LimpiarFiltros,
  Pagination,
  Pill,
  Skeleton,
} from '@licoreria/ui';
import type { Evento } from '@licoreria/types';
import { contenidoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { FormularioEvento, type FormularioEventoForm } from '../components/eventos/FormularioEvento';
import { PanelDetalleEvento } from '../components/eventos/PanelDetalleEvento';
import { EstadoEvento, vigenciaDe } from '../components/eventos/estado';
import { mensajeDeError } from '../lib/api';
import { urlDeImagen } from '../lib/imagenProducto';
import { formatDateTime } from '../lib/format';
import { contiene, paginarEnMemoria } from '../lib/filtros';

const OPCIONES_ESTADO = [
  { valor: '', etiqueta: 'Todos' },
  { valor: 'Publicados', etiqueta: 'Publicados' },
  { valor: 'Proximos', etiqueta: 'Próximos' },
  { valor: 'Pasados', etiqueta: 'Pasados' },
];

const imagenIdDe = (evento: Evento) => `ev-img-${evento.id}`;

function MiniaturaEvento({ evento }: { evento: Evento }) {
  const imagen = urlDeImagen(evento.imagenUrl);
  const layoutId = imagenIdDe(evento);
  if (imagen) {
    return (
      <motion.img
        layoutId={layoutId}
        layout
        src={imagen}
        alt=""
        className="h-10 w-10 shrink-0 rounded-lg border border-border object-cover"
      />
    );
  }
  return (
    <motion.div
      layoutId={layoutId}
      layout
      className="flex h-10 w-10 shrink-0 items-center justify-center rounded-lg border border-dashed border-border bg-muted/30 text-muted-foreground"
    >
      <CalendarDays size={16} />
    </motion.div>
  );
}

export function EventosPage() {
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const { id } = useParams();
  const queryClient = useQueryClient();

  const esCrear = pathname === '/eventos/nuevo';
  const esEditar = /^\/eventos\/[^/]+\/editar$/.test(pathname);
  const esEditor = esCrear || esEditar;

  const [page, setPage] = useState(1);
  const [busqueda, setBusqueda] = useState('');
  const [filtro, setFiltro] = useState('');
  const [expandido, setExpandido] = useState<Evento | null>(null);
  const [porEliminar, setPorEliminar] = useState<Evento | null>(null);

  useEffect(() => {
    setExpandido(null);
  }, [pathname]);

  const eventos = useQuery({ queryKey: ['eventos-todos'], queryFn: contenidoApi.eventos });
  const detalle = useQuery({
    queryKey: ['evento', id],
    queryFn: () => contenidoApi.eventos().then((todos) => todos.find((evento) => evento.id === id) ?? null),
    enabled: esEditar,
  });

  const invalidar = () => {
    queryClient.invalidateQueries({ queryKey: ['eventos-todos'] });
    queryClient.invalidateQueries({ queryKey: ['eventos'] });
  };

  const editando = esEditar ? detalle.data : null;

  const guardar = useMutation({
    mutationFn: (datos: FormularioEventoForm) => {
      const body = {
        titulo: datos.titulo,
        descripcion: datos.descripcion,
        fechaInicio: new Date(datos.fechaInicio).toISOString(),
        fechaFin: datos.fechaFin ? new Date(datos.fechaFin).toISOString() : null,
        imagenUrl: datos.imagenUrl || null,
        publicado: datos.publicado,
      };
      return editando
        ? contenidoApi.actualizarEvento(editando.id, { id: editando.id, activo: datos.activo, ...body })
        : contenidoApi.crearEvento(body);
    },
    onSuccess: () => {
      toast.success(editando ? 'Evento actualizado' : 'Evento creado');
      invalidar();
      navigate('/eventos');
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (eventoId: string) => contenidoApi.eliminarEvento(eventoId),
    onSuccess: () => {
      toast.success('Evento eliminado');
      setPorEliminar(null);
      invalidar();
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  const hayFiltros = Boolean(busqueda || filtro);
  const limpiarFiltros = () => {
    setBusqueda('');
    setFiltro('');
    setPage(1);
  };

  const { items: filas, totalPages } = useMemo(() => {
    const filtradas = (eventos.data ?? []).filter((evento) => {
      if (!contiene(`${evento.titulo} ${evento.descripcion}`, busqueda)) return false;
      if (filtro === 'Publicados' && !evento.publicado) return false;
      if (filtro === 'Proximos' && vigenciaDe(evento) === 'Pasado') return false;
      if (filtro === 'Pasados' && vigenciaDe(evento) !== 'Pasado') return false;
      return true;
    });
    return paginarEnMemoria(filtradas, page, 10);
  }, [eventos.data, busqueda, filtro, page]);

  if (esEditor) {
    if (esEditar && detalle.isLoading) {
      return (
        <div className="absolute inset-0 flex items-center justify-center">
          <Skeleton className="h-64 w-full max-w-2xl" />
        </div>
      );
    }
    if (esEditar && !detalle.data) {
      return (
        <div className="absolute inset-0 flex flex-col items-center justify-center gap-4 text-center">
          <p className="text-sm text-muted-foreground">Este evento no existe o fue eliminado.</p>
          <Button variant="ghost" onClick={() => navigate('/eventos')}>
            Volver a eventos
          </Button>
        </div>
      );
    }
    return (
      <div className="absolute inset-0 flex min-h-0 flex-col overflow-hidden p-3 lg:p-4">
        <FormularioEvento
          evento={editando}
          guardando={guardar.isPending}
          onCancelar={() => navigate('/eventos')}
          onGuardar={(datos) => guardar.mutate(datos)}
        />
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador
              placeholder="Buscar título o descripción…"
              value={busqueda}
              onCambio={(valor) => {
                setBusqueda(valor);
                setPage(1);
              }}
            />
            <FiltroDropdown
              label="Estado"
              opciones={OPCIONES_ESTADO}
              valor={filtro}
              onChange={(valor) => {
                setFiltro(valor);
                setPage(1);
              }}
            />
            <div className="ml-auto">
              <Button size="sm" leftIcon={<CalendarDays size={15} />} onClick={() => navigate('/eventos/nuevo')}>
                Nuevo evento
              </Button>
            </div>
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <LayoutGroup>
            <DataTable<Evento>
              rows={filas}
              loading={eventos.isLoading}
              rowKey={(evento) => evento.id}
              onRowClick={(evento) => setExpandido((actual) => (actual?.id === evento.id ? null : evento))}
              expandirKey={expandido?.id ?? null}
              expandedRow={
                expandido
                  ? (evento) =>
                      evento.id === expandido.id ? (
                        <PanelDetalleEvento
                          evento={evento}
                          imagenId={imagenIdDe(evento)}
                          onCerrar={() => setExpandido(null)}
                        />
                      ) : null
                  : undefined
              }
              empty="No hay eventos."
              columns={[
                {
                  key: 'titulo',
                  header: 'Evento',
                  render: (evento) => (
                    <div className="flex items-center gap-3">
                      <MiniaturaEvento evento={evento} />
                      <div>
                        <p className="text-foreground">{evento.titulo}</p>
                        <p className="text-xs text-muted-foreground">{formatDateTime(evento.fechaInicio)}</p>
                      </div>
                    </div>
                  ),
                },
                { key: 'estado', header: 'Estado', render: (evento) => <EstadoEvento evento={evento} /> },
                {
                  key: 'activo',
                  header: 'Activo',
                  render: (evento) => <Pill tone={evento.activo ? 'success' : 'danger'}>{evento.activo ? 'Sí' : 'No'}</Pill>,
                },
                {
                  key: 'acciones',
                  header: '',
                  align: 'right',
                  render: (evento) => (
                    <ActionMenu
                      label={`Acciones de ${evento.titulo}`}
                      options={[
                        { label: 'Editar', icon: <Pencil size={15} />, onClick: () => navigate(`/eventos/${evento.id}/editar`) },
                        {
                          label: 'Eliminar',
                          icon: <Trash2 size={15} />,
                          danger: true,
                          onClick: () => setPorEliminar(evento),
                        },
                      ]}
                    />
                  ),
                },
              ]}
            />
          </LayoutGroup>
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </CardBody>
      </Card>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar evento"
        description={`¿Seguro que deseas eliminar "${porEliminar?.titulo ?? ''}"?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}