import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Card, CardBody, CardHeader, CardTitle, DataTable, Input, PageHeader, Pagination } from '@licoreria/ui';
import type { AuditLog } from '@licoreria/types';
import { auditoriaApi } from '@licoreria/api-client';
import { formatDateTime } from '../lib/format';

export function AuditoriaPage() {
  const [page, setPage] = useState(1);
  const [entidad, setEntidad] = useState('');

  const auditoria = useQuery({
    queryKey: ['auditoria', page, entidad],
    queryFn: () => auditoriaApi.listar({ page, pageSize: 20, entidad: entidad || undefined }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Auditoría" subtitle="Acciones sensibles registradas por el sistema." />

      <Card>
        <CardHeader>
          <CardTitle>Bitácora</CardTitle>
          <Input
            aria-label="Filtrar por entidad"
            placeholder="Filtrar por entidad…"
            value={entidad}
            onChange={(evento) => {
              setEntidad(evento.target.value);
              setPage(1);
            }}
            className="w-56"
          />
        </CardHeader>
        <CardBody>
          <DataTable<AuditLog>
            rows={auditoria.data?.items ?? []}
            loading={auditoria.isLoading}
            rowKey={(fila) => fila.id}
            empty="Sin registros de auditoría."
            columns={[
              { key: 'fecha', header: 'Fecha', render: (fila) => formatDateTime(fila.fecha) },
              { key: 'usuario', header: 'Usuario', render: (fila) => fila.usuario ?? '—' },
              { key: 'accion', header: 'Acción', render: (fila) => fila.accion },
              { key: 'entidad', header: 'Entidad', render: (fila) => fila.entidad },
              { key: 'ip', header: 'IP', render: (fila) => fila.ip ?? '—' },
            ]}
          />
          <Pagination page={page} totalPages={auditoria.data?.totalPages ?? 1} onPageChange={setPage} />
        </CardBody>
      </Card>
    </div>
  );
}
