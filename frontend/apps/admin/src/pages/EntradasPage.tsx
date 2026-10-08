import { useState } from 'react';
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
  Select,
  StatusBadge,
} from '@licoreria/ui';
import type { Entrada } from '@licoreria/types';
import { contenidoApi, entradasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatDateTime, formatUSD } from '../lib/format';

export function EntradasPage() {
  const queryClient = useQueryClient();
  const [emitiendo, setEmitiendo] = useState(false);
  const [precio, setPrecio] = useState('0');
  const [moneda, setMoneda] = useState<'USD' | 'BS'>('USD');
  const [eventoId, setEventoId] = useState('');
  const [codigo, setCodigo] = useState('');

  const entradas = useQuery({ queryKey: ['entradas'], queryFn: () => entradasApi.listar() });
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
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader
        title="Entradas"
        subtitle="Emisión y control de acceso a eventos."
        actions={<Button onClick={() => setEmitiendo(true)}>Emitir entrada</Button>}
      />

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
        <CardHeader>
          <CardTitle>Entradas</CardTitle>
          <span className="text-xs text-muted-foreground">{entradas.data?.length ?? 0} emitidas</span>
        </CardHeader>
        <CardBody>
          <DataTable<Entrada>
            rows={entradas.data ?? []}
            loading={entradas.isLoading}
            rowKey={(fila) => fila.id}
            empty="Sin entradas emitidas."
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
                    <Button size="sm" variant="ghost" onClick={() => cancelar.mutate(fila.id)}>
                      Cancelar
                    </Button>
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
