import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Card, CardBody, CardHeader, CardTitle, DataTable, FiltroDropdown, Pill } from '@licoreria/ui';
import type { EnvaseAbierto, EstadoEnvase } from '@licoreria/types';
import { envasesApi } from '@licoreria/api-client';
import { InventarioTabs } from '../components/InventarioTabs';
import { useRealtime } from '../hooks/useRealtime';
import { formatDateTime } from '../lib/format';

const TONO: Record<EstadoEnvase, 'success' | 'neutral' | 'danger'> = {
  Abierto: 'success',
  Agotado: 'neutral',
  Descartado: 'danger',
};

const OPCIONES_ESTADO = [
  { valor: 'Abierto', etiqueta: 'Abiertos' },
  { valor: 'Agotado', etiqueta: 'Agotados' },
  { valor: 'Descartado', etiqueta: 'Descartados' },
];

/** Envases destapados (vasos, tragos) con quién y cuándo, más el historial reciente de aperturas y descartes. */
export function EnvasesAbiertosPage() {
  const queryClient = useQueryClient();
  const [estado, setEstado] = useState('');

  const envases = useQuery({
    queryKey: ['envases', 'historial', estado],
    queryFn: () =>
      envasesApi.listar(estado ? { estado: estado as EstadoEnvase, limite: 100 } : { todos: true, limite: 100 }),
  });

  useRealtime('meseros', {
    'envase:actualizado': () => queryClient.invalidateQueries({ queryKey: ['envases'] }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4">
      <InventarioTabs />

      <Card>
        <CardHeader className="flex flex-wrap items-center justify-between gap-3">
          <CardTitle>Envases abiertos</CardTitle>
          <FiltroDropdown label="Estado" opciones={OPCIONES_ESTADO} valor={estado} onChange={setEstado} />
        </CardHeader>
        <CardBody>
          <DataTable<EnvaseAbierto>
            rows={envases.data ?? []}
            loading={envases.isLoading}
            rowKey={(envase) => envase.id}
            empty="Aún no se ha destapado ninguna botella."
            columns={[
              {
                key: 'porcion',
                header: 'Porción',
                render: (envase) => (
                  <div>
                    <p className="text-foreground">{envase.porcionNombre}</p>
                    <p className="text-xs text-muted-foreground">de {envase.presentacionNombre}</p>
                  </div>
                ),
              },
              {
                key: 'restantes',
                header: 'Restantes',
                align: 'right',
                render: (envase) => `${envase.porcionesRestantes} / ${envase.porcionesIniciales}`,
              },
              {
                key: 'estado',
                header: 'Estado',
                render: (envase) => (
                  <div>
                    <Pill tone={TONO[envase.estado]}>{envase.estado}</Pill>
                    {envase.motivoDescarte && <p className="mt-0.5 text-xs text-muted-foreground">{envase.motivoDescarte}</p>}
                  </div>
                ),
              },
              { key: 'abierto', header: 'Abierto', render: (envase) => formatDateTime(envase.abiertoEn) },
              {
                key: 'cerrado',
                header: 'Cerrado',
                render: (envase) => (envase.cerradoEn ? formatDateTime(envase.cerradoEn) : '—'),
              },
            ]}
          />
        </CardBody>
      </Card>
    </div>
  );
}
