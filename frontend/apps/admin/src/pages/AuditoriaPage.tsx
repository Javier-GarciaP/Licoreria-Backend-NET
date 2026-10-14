import { useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Card,
  CardBody,
  CardHeader,
  DataTable,
  FiltroDropdown,
  FiltroFechas,
  LimpiarFiltros,
  Modal,
  Pagination,
  Pill,
} from '@licoreria/ui';
import type { AuditLog, TipoAuditoria } from '@licoreria/types';
import { auditoriaApi, usuariosApi } from '@licoreria/api-client';
import { formatDateTime } from '../lib/format';
import {
  ACCIONES_CAMBIO,
  ACCIONES_EVENTO,
  filasDeDetalle,
  formatearValor,
  interpretarDatos,
  rangoIso,
} from '../lib/auditoria';

const PAGE_SIZE = 20;
const CLAVES_FILTRO = ['usuario', 'entidad', 'accion', 'tipo', 'desde', 'hasta'] as const;

export function AuditoriaPage() {
  // El estado de los filtros vive en la URL: se puede compartir y sobrevive a recargar.
  const [params, setParams] = useSearchParams();
  const [seleccion, setSeleccion] = useState<AuditLog | null>(null);

  const usuarioId = params.get('usuario') ?? '';
  const entidad = params.get('entidad') ?? '';
  const accion = params.get('accion') ?? '';
  const tipo = (params.get('tipo') ?? '') as TipoAuditoria | '';
  const desde = params.get('desde') ?? '';
  const hasta = params.get('hasta') ?? '';
  const page = Math.max(1, Number(params.get('page') ?? '1') || 1);

  const hayFiltros = Boolean(usuarioId || entidad || accion || tipo || desde || hasta);

  const cambiar = (cambios: Record<string, string>, conservarPagina = false) => {
    setParams(
      (actuales) => {
        const siguientes = new URLSearchParams(actuales);
        for (const [clave, valor] of Object.entries(cambios)) {
          if (valor) siguientes.set(clave, valor);
          else siguientes.delete(clave);
        }
        if (!conservarPagina) siguientes.delete('page');
        return siguientes;
      },
      { replace: true },
    );
  };

  const limpiarFiltros = () =>
    setParams(
      (actuales) => {
        const siguientes = new URLSearchParams(actuales);
        for (const clave of CLAVES_FILTRO) siguientes.delete(clave);
        siguientes.delete('page');
        return siguientes;
      },
      { replace: true },
    );

  const usuarios = useQuery({
    queryKey: ['auditoria-usuarios'],
    queryFn: () => usuariosApi.listar({ page: 1, pageSize: 100 }),
  });
  const entidades = useQuery({ queryKey: ['auditoria-entidades'], queryFn: () => auditoriaApi.entidades() });

  const auditoria = useQuery({
    queryKey: ['auditoria', page, usuarioId, entidad, accion, tipo, desde, hasta],
    queryFn: () =>
      auditoriaApi.listar({
        page,
        pageSize: PAGE_SIZE,
        usuarioId: usuarioId || undefined,
        entidad: entidad || undefined,
        accion: accion || undefined,
        tipo: tipo || undefined,
        ...rangoIso(desde, hasta),
      }),
  });

  const opcionesAccion = useMemo(
    () => [...ACCIONES_CAMBIO, ...ACCIONES_EVENTO].map((valor) => ({ valor, etiqueta: valor })),
    [],
  );

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <Card>
        <CardHeader className="flex flex-col items-stretch gap-3">
          <div className="flex flex-wrap items-end gap-2">
            <FiltroDropdown
              label="Usuario"
              opciones={(usuarios.data?.items ?? []).map((u) => ({ valor: u.id, etiqueta: u.nombreCompleto }))}
              valor={usuarioId}
              onChange={(valor) => cambiar({ usuario: valor })}
            />
            <FiltroDropdown
              label="Entidad"
              opciones={(entidades.data ?? []).map((valor) => ({ valor, etiqueta: valor }))}
              valor={entidad}
              onChange={(valor) => cambiar({ entidad: valor })}
            />
            <FiltroDropdown
              label="Acción"
              opciones={opcionesAccion}
              valor={accion}
              onChange={(valor) => cambiar({ accion: valor })}
            />
            <FiltroDropdown
              label="Tipo"
              opciones={[
                { valor: 'Cambio', etiqueta: 'Cambios de datos' },
                { valor: 'Evento', etiqueta: 'Eventos' },
              ]}
              valor={tipo}
              onChange={(valor) => cambiar({ tipo: valor })}
            />
            <FiltroFechas
              desde={desde}
              hasta={hasta}
              onDesde={(valor) => cambiar({ desde: valor })}
              onHasta={(valor) => cambiar({ hasta: valor })}
            />
            <div className="ml-auto">
              <LimpiarFiltros activo={hayFiltros} onClick={limpiarFiltros} />
            </div>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<AuditLog>
            rows={auditoria.data?.items ?? []}
            loading={auditoria.isLoading}
            rowKey={(fila) => fila.id}
            onRowClick={setSeleccion}
            empty={
              hayFiltros ? (
                <div className="flex flex-col items-center gap-2">
                  <span>Ningún registro cumple los filtros.</span>
                  <LimpiarFiltros activo onClick={limpiarFiltros} />
                </div>
              ) : (
                'Sin registros de auditoría.'
              )
            }
            columns={[
              { key: 'fecha', header: 'Fecha', render: (fila) => formatDateTime(fila.fecha) },
              { key: 'usuario', header: 'Usuario', render: (fila) => fila.usuario ?? '—' },
              { key: 'accion', header: 'Acción', render: (fila) => fila.accion },
              { key: 'entidad', header: 'Entidad', render: (fila) => fila.entidad },
              {
                key: 'tipo',
                header: 'Tipo',
                render: (fila) => <Pill tone={fila.tipo === 'Evento' ? 'info' : 'neutral'}>{fila.tipo}</Pill>,
              },
              { key: 'ip', header: 'IP', render: (fila) => fila.ip ?? '—' },
            ]}
          />
          <Pagination
            page={page}
            totalPages={auditoria.data?.totalPages ?? 1}
            onPageChange={(siguiente) => cambiar({ page: String(siguiente) }, true)}
          />
        </CardBody>
      </Card>

      <DetalleAuditoria entrada={seleccion} onClose={() => setSeleccion(null)} />
    </div>
  );
}

function DetalleAuditoria({ entrada, onClose }: { entrada: AuditLog | null; onClose: () => void }) {
  const detalleQuery = useQuery({
    queryKey: ['auditoria-detalle', entrada?.id],
    queryFn: () => auditoriaApi.obtener(entrada!.id),
    enabled: Boolean(entrada),
    initialData: entrada ?? undefined,
  });
  const actual = detalleQuery.data ?? entrada;
  const { detalle, libre } = interpretarDatos(actual?.datos ?? null);
  const filas = detalle ? filasDeDetalle(detalle) : [];

  return (
    <Modal open={Boolean(entrada)} onClose={onClose} title="Detalle de auditoría">
      {actual && (
        <div className="flex flex-col gap-4 text-sm">
          <dl className="grid grid-cols-2 gap-x-4 gap-y-2">
            <Dato etiqueta="Fecha" valor={formatDateTime(actual.fecha)} />
            <Dato etiqueta="Usuario" valor={actual.usuario ?? 'sistema'} />
            <Dato etiqueta="IP" valor={actual.ip ?? '—'} />
            <Dato etiqueta="Acción" valor={actual.accion} />
            <Dato etiqueta="Entidad" valor={actual.entidad} />
            <Dato etiqueta="Identificador" valor={actual.entidadId ?? '—'} />
          </dl>

          {detalle && (
            <table className="w-full text-left" aria-label="Valores antes y después">
              <thead>
                <tr className="border-b border-border text-xs text-muted-foreground">
                  <th className="py-1 pr-3 font-medium">Campo</th>
                  <th className="py-1 pr-3 font-medium">Antes</th>
                  <th className="py-1 font-medium">Después</th>
                </tr>
              </thead>
              <tbody>
                {filas.map((fila) => (
                  <tr key={fila.campo} className="border-b border-border/50">
                    <td className="py-1 pr-3 font-medium">{fila.campo}</td>
                    <td className="py-1 pr-3 text-muted-foreground">{fila.antes}</td>
                    <td className="py-1">{fila.despues}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}

          {libre && (
            <dl className="grid grid-cols-2 gap-x-4 gap-y-2" aria-label="Datos del evento">
              {Object.entries(libre).map(([campo, valor]) => (
                <Dato key={campo} etiqueta={campo} valor={formatearValor(valor)} />
              ))}
            </dl>
          )}

          {!detalle && !libre && <p className="text-muted-foreground">Esta entrada no tiene datos adicionales.</p>}
        </div>
      )}
    </Modal>
  );
}

function Dato({ etiqueta, valor }: { etiqueta: string; valor: string }) {
  return (
    <div>
      <dt className="text-xs text-muted-foreground">{etiqueta}</dt>
      <dd className="break-all font-medium">{valor}</dd>
    </div>
  );
}
