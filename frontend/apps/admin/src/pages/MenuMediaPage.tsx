import { useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  Button,
  Card,
  CardBody,
  CardHeader,
  CardTitle,
  DataTable,
  PageHeader,
  Pill,
  Skeleton,
} from '@licoreria/ui';
import type { MediaAsset } from '@licoreria/types';
import { contenidoApi } from '@licoreria/api-client';
import { ConfirmDialog } from '../components/ConfirmDialog';
import { ContenidoTabs } from '../components/ContenidoTabs';
import { mensajeDeError } from '../lib/api';
import { formatNumber, formatUSD } from '../lib/format';

function peso(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function MenuMediaPage() {
  const inputRef = useRef<HTMLInputElement>(null);
  const [porEliminar, setPorEliminar] = useState<MediaAsset | null>(null);
  const queryClient = useQueryClient();

  const menu = useQuery({ queryKey: ['menu-digital'], queryFn: contenidoApi.menuDigital });
  const qr = useQuery({ queryKey: ['menu-qr'], queryFn: () => contenidoApi.qrMenu() });
  const media = useQuery({ queryKey: ['media'], queryFn: contenidoApi.media });

  const subir = useMutation({
    mutationFn: (file: File) => contenidoApi.subirArchivo(file),
    onSuccess: () => {
      toast.success('Archivo subido');
      queryClient.invalidateQueries({ queryKey: ['media'] });
      if (inputRef.current) inputRef.current.value = '';
    },
    onError: (error) => toast.error('No se pudo subir el archivo', { description: mensajeDeError(error) }),
  });

  const eliminar = useMutation({
    mutationFn: (id: string) => contenidoApi.eliminarMedia(id),
    onSuccess: () => {
      toast.success('Archivo eliminado');
      setPorEliminar(null);
      queryClient.invalidateQueries({ queryKey: ['media'] });
    },
    onError: (error) => toast.error('No se pudo eliminar', { description: mensajeDeError(error) }),
  });

  return (
    <div className="mx-auto flex max-w-page flex-col gap-6">
      <PageHeader title="Contenido" subtitle="Menú digital, código QR y archivos." />
      <ContenidoTabs />

      <Card>
        <CardHeader>
          <CardTitle>Menú digital</CardTitle>
          {menu.data && <Pill tone="accent">Tasa {formatUSD(menu.data.tasa)}</Pill>}
        </CardHeader>
        <CardBody>
          {menu.isLoading ? (
            <Skeleton className="h-40 w-full" />
          ) : (
            <div className="flex flex-col gap-4">
              <div className="rounded-lg border border-border p-3">
                <p className="text-xs text-muted-foreground">Enlace / contenido del QR</p>
                <p className="mt-1 break-all text-sm text-foreground">{qr.data?.contenido ?? qr.data?.url ?? '—'}</p>
              </div>
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2 xl:grid-cols-3">
                {menu.data?.secciones.map((seccion) => (
                  <div key={seccion.categoriaId} className="rounded-lg border border-border p-3">
                    <p className="text-sm font-medium text-foreground">{seccion.nombre}</p>
                    <div className="mt-2 flex flex-col gap-1">
                      {seccion.items.map((item) => (
                        <div key={item.varianteId} className="flex justify-between text-sm">
                          <span className="text-muted-foreground">{item.nombre}</span>
                          <span className="text-foreground">{formatUSD(item.precioUSD)}</span>
                        </div>
                      ))}
                    </div>
                  </div>
                ))}
                {menu.data?.secciones.length === 0 && <p className="text-sm text-muted-foreground">El menú no tiene secciones.</p>}
              </div>
            </div>
          )}
        </CardBody>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>Archivos</CardTitle>
          <div className="flex items-center gap-2">
            <input
              ref={inputRef}
              type="file"
              className="text-xs text-muted-foreground"
              onChange={(evento) => {
                const file = evento.target.files?.[0];
                if (file) subir.mutate(file);
              }}
            />
            <Button size="sm" loading={subir.isPending} onClick={() => inputRef.current?.click()}>
              Subir
            </Button>
          </div>
        </CardHeader>
        <CardBody>
          <DataTable<MediaAsset>
            rows={media.data ?? []}
            loading={media.isLoading}
            rowKey={(asset) => asset.id}
            empty="No hay archivos."
            columns={[
              {
                key: 'nombre',
                header: 'Archivo',
                render: (asset) => (
                  <a href={asset.url} target="_blank" rel="noreferrer" className="text-foreground hover:text-foreground">
                    {asset.nombre}
                  </a>
                ),
              },
              { key: 'tipo', header: 'Tipo', render: (asset) => asset.tipo },
              { key: 'tamano', header: 'Tamaño', align: 'right', render: (asset) => peso(asset.tamano) },
              { key: 'id', header: 'ID', render: (asset) => <span className="text-xs text-muted-foreground">{asset.id.slice(0, 8)}</span> },
              {
                key: 'acciones',
                header: '',
                align: 'right',
                render: (asset) => (
                  <Button size="sm" variant="ghost" onClick={() => setPorEliminar(asset)}>
                    Eliminar
                  </Button>
                ),
              },
            ]}
          />
        </CardBody>
      </Card>

      <ConfirmDialog
        open={Boolean(porEliminar)}
        title="Eliminar archivo"
        description={`¿Seguro que deseas eliminar "${porEliminar?.nombre ?? ''}" (${formatNumber(porEliminar?.tamano ?? 0)} bytes)?`}
        confirmLabel="Eliminar"
        loading={eliminar.isPending}
        onClose={() => setPorEliminar(null)}
        onConfirm={() => porEliminar && eliminar.mutate(porEliminar.id)}
      />
    </div>
  );
}
