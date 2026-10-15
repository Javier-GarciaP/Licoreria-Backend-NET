import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Plus, XCircle } from 'lucide-react';
import {
  ActionMenu,
  Buscador,
  FiltroDropdown,
  LimpiarFiltros,
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  Input,
  Modal,
  Select,
  StatusBadge,
} from '@licoreria/ui';
import type { Entrada } from '@licoreria/types';
import { contenidoApi, entradasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { contiene } from '../lib/filtros';
import { formatDateTime, formatUSD } from '../lib/format';

export function EntradasPage() {
  const queryClient = useQueryClient();
  const [emitiendo, setEmitiendo] = useState(false);
  const [precio, setPrecio] = useState('0');
  const [moneda, setMoneda] = useState<'USD' | 'BS'>('USD');
  const [eventoId, setEventoId] = useState('');
  const [codigo, setCodigo] = useState('');
  const [busqueda, setBusqueda] = useState('');
  const [estado, setEstado] = useState('');

  const entradas = useQuery({ queryKey: ['entradas'], queryFn: () => entradasApi.listar() });
  const filas = (entradas.data?.items ?? []).filter(
    (fila) => contiene(`${fila.codigo} ${fila.eventoTitulo ?? ''}`, busqueda) && (!estado || fila.estado === estado),
  );
  const estadosDisponibles = Array.from(new Set((entradas.data?.items ?? []).map((fila) => fila.estado)));
  const hayFiltros = Boolean(busqueda || estado);
  const limpiarFiltros = () => {
    setBusqueda('');
    setEstado('');
  };

  const eventos = useQuery({ queryKey: ['eventos', 'todos'], queryFn: contenidoApi.eventos });

  const invalidar = () => queryClient.invalidateQueries({ queryKey: ['entradas'] });

  const emitir = useMutation({
    mutationFn: () => entradasApi.emitir({ precio: Number(precio), moneda, eventoId: eventoId || null }),
    onSuccess: () => {
      toast.success('Entrada emitida');
      setEmitiendo(false);
      setPrecio('0');
      setEventoId('');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo emitir', { description: mensajeDeError(error) }),
  });

  const validar = useMutation({
    mutationFn: (valor: string) => entradasApi.validar(valor),
    onSuccess: () => {
      toast.success('Entrada validada');
      setCodigo('');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo validar', { description: mensajeDeError(error) }),
  });

  const cancelar = useMutation({
    mutationFn: (id: string) => entradasApi.cancelar(id),
    onSuccess: () => {
      toast.success('Entrada cancelada');
      invalidar();
    },
    onError: (error) => toast.error('No se pudo cancelar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <Card>
        <CardHeader>
          <CardTitle>Validar por código</CardTitle>
        </CardHeader>
        <CardBody>
          <form
            className="flex flex-wrap items-end gap-2"
            onSubmit={(evento) => {
              evento.preventDefault();
              if (codigo.trim()) validar.mutate(codigo.trim());
            }}
          >
            <Input
              aria-label="Código de entrada"
              placeholder="Código…"
              value={codigo}
              onChange={(evento) => setCodigo(evento.target.value)}
              className="w-64"
            />
            <Button type="submit" loading={validar.isPending}>
              Validar
            </Button>
          </form>
        </CardBody>
      </Card>

      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-center gap-2">
            <Buscador placeholder="Buscar código o evento…" value={busqueda} onCambio={setBusqueda} />
            <div className="ml-auto">
              <Button leftIcon={<Plus size={15} />} onClick={() => setEmitiendo(true)}>
                Emitir entrada
              </Button>
            </div>
          </div>
          <div className="border-b border-border" />
          <div className="flex flex-wrap items-center gap-2">
            <FiltroDropdown
              label="Estado"
              opciones={estadosDisponibles.map((valor) => ({ valor, etiqueta: valor }))}
              valor={estado}
              onChange={setEstado}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<Entrada>
            rows={filas}
            loading={entradas.isLoading}
            rowKey={(fila) => fila.id}
            empty={hayFiltros ? 'Sin entradas con esos filtros.' : 'Sin entradas emitidas.'}
            columns={[
              { key: 'codigo', header: 'Código', render: (fila) => <span className="num">{fila.codigo}</span> },
              { key: 'evento', header: 'Evento', render: (fila) => fila.eventoTitulo ?? '—' },
              { key: 'precio', header: 'Precio', align: 'right', render: (fila) => `${formatUSD(fila.precio)} ${fila.moneda}` },
              { key: 'estado', header: 'Estado', render: (fila) => <StatusBadge status={fila.estado} /> },
              { key: 'emitida', header: 'Emitida', render: (fila) => formatDateTime(fila.emitidaEn) },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (fila) =>
                  fila.estado === 'Emitida' || fila.estado === 'Valida' ? (
                    <ActionMenu
                      label={`Acciones de entrada ${fila.codigo}`}
                      options={[
                        { label: 'Cancelar', icon: <XCircle size={15} />, onClick: () => cancelar.mutate(fila.id) },
                      ]}
                    />
                  ) : null,
              },
            ]}
          />
        </CardBody>
      </Card>

      <Modal
        open={emitiendo}
        onClose={() => setEmitiendo(false)}
        title="Emitir entrada"
        footer={
          <>
            <Button variant="ghost" onClick={() => setEmitiendo(false)}>
              Cancelar
            </Button>
            <Button loading={emitir.isPending} onClick={() => emitir.mutate()}>
              Emitir
            </Button>
          </>
        }
      >
        <div className="flex flex-col gap-3">
          <Select label="Evento (opcional)" value={eventoId} onChange={(evento) => setEventoId(evento.target.value)}>
            <option value="">Sin evento</option>
            {eventos.data?.map((evento) => (
              <option key={evento.id} value={evento.id}>
                {evento.titulo}
              </option>
            ))}
          </Select>
          <div className="grid grid-cols-2 gap-3">
            <Input label="Precio" type="number" step="0.01" value={precio} onChange={(evento) => setPrecio(evento.target.value)} />
            <Select label="Moneda" value={moneda} onChange={(evento) => setMoneda(evento.target.value as 'USD' | 'BS')}>
              <option value="USD">USD</option>
              <option value="BS">Bs</option>
            </Select>
          </div>
        </div>
      </Modal>
    </div>
  );
}
