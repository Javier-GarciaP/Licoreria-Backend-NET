import { useRef, useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Package } from 'lucide-react';
import { Button, cn, Input, Modal, Skeleton } from '@licoreria/ui';
import type { Categoria, Producto, ProductoVariante } from '@licoreria/types';
import { catalogoApi, clubApi } from '@licoreria/api-client';
import { mensajeDeError } from '../../lib/api';
import { urlDeImagen } from '../../lib/imagenProducto';
import { formatUSD } from '../../lib/format';

const rangoPrecio = (producto: Producto) => {
  const precios = producto.variantes.map((variante) => variante.precioVentaUSD).filter((valor) => valor > 0);
  if (precios.length === 0) return '—';
  const minimo = Math.min(...precios);
  const maximo = Math.max(...precios);
  return minimo === maximo ? formatUSD(minimo) : `${formatUSD(minimo)} – ${formatUSD(maximo)}`;
};

/** Picker visual de productos: cada clic agrega el producto directo al pedido anticipado. */
export function PanelAgregarPedido({
  reservaId,
  onEnviado,
}: {
  reservaId: string;
  onEnviado: () => void;
}) {
  const [busqueda, setBusqueda] = useState('');
  const [categoriaId, setCategoriaId] = useState<string | null>(null);
  const [varianteDe, setVarianteDe] = useState<Producto | null>(null);
  const busquedaRef = useRef<HTMLInputElement>(null);

  const productos = useQuery({
    queryKey: ['reserva', 'productos', busqueda, categoriaId],
    queryFn: () => catalogoApi.productos({ busqueda, categoriaId: categoriaId ?? undefined, pageSize: 50, activo: true }),
  });
  const categorias = useQuery({ queryKey: ['reserva', 'categorias'], queryFn: catalogoApi.categorias });

  const agregarDirecto = useMutation({
    mutationFn: (varianteId: string) => clubApi.agregarPedido(reservaId, { varianteId, cantidad: 1 }),
    onSuccess: () => {
      toast.success('Pedido agregado');
      onEnviado();
    },
    onError: (error) => toast.error('No se pudo agregar el pedido', { description: mensajeDeError(error) }),
  });

  const seleccionarProducto = (producto: Producto) => {
    if (producto.variantes.length === 1) {
      agregarDirecto.mutate(producto.variantes[0].id);
      return;
    }
    setVarianteDe(producto);
  };

  const seleccionarVariante = (variante: ProductoVariante) => {
    agregarDirecto.mutate(variante.id);
    setVarianteDe(null);
  };

  return (
    <div className="flex flex-col gap-3">
      <Input ref={busquedaRef} placeholder="Buscar producto…" value={busqueda} onChange={(evento) => setBusqueda(evento.target.value)} />

      <div className="app-scroll -mx-1 flex gap-1.5 overflow-x-auto px-1 pb-1">
        <button
          type="button"
          onClick={() => setCategoriaId(null)}
          className={cn(
            'flex h-7 shrink-0 items-center whitespace-nowrap rounded-full border px-2.5 text-[11px] transition',
            categoriaId === null ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
          )}
        >
          Todas
        </button>
        {(categorias.data ?? []).map((categoria: Categoria) => (
          <button
            key={categoria.id}
            type="button"
            onClick={() => setCategoriaId(categoria.id)}
            className={cn(
              'flex h-7 shrink-0 items-center whitespace-nowrap rounded-full border px-2.5 text-[11px] transition',
              categoriaId === categoria.id ? 'border-primary bg-primary/15 text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
            )}
          >
            {categoria.nombre}
          </button>
        ))}
      </div>

      <div className="grid max-h-[26rem] grid-cols-2 gap-2 overflow-y-auto sm:grid-cols-3">
        {productos.isLoading ? (
          Array.from({ length: 6 }).map((_, indice) => <Skeleton key={indice} className="h-32 w-full" />)
        ) : (productos.data?.items ?? []).length === 0 ? (
          <p className="col-span-full py-8 text-center text-sm text-muted-foreground">Sin productos para agregar.</p>
        ) : (
          (productos.data?.items ?? []).map((producto: Producto) => {
            const imagen = urlDeImagen(producto.imagenUrl);
            return (
              <button
                key={producto.id}
                type="button"
                onClick={() => seleccionarProducto(producto)}
                className="flex flex-col overflow-hidden rounded-xl border border-border text-left transition hover:border-primary/50"
              >
                {imagen ? (
                  <img src={imagen} alt={producto.nombre} className="aspect-[4/3] w-full object-cover" />
                ) : (
                  <div className="flex aspect-[4/3] w-full items-center justify-center bg-muted/40">
                    <Package size={20} className="text-muted-foreground" />
                  </div>
                )}
                <div className="flex flex-col gap-0.5 p-2.5">
                  <span className="line-clamp-2 text-sm text-foreground">{producto.nombre}</span>
                  <span className="num text-[11px] text-muted-foreground">{rangoPrecio(producto)}</span>
                </div>
              </button>
            );
          })
        )}
      </div>

      <Modal
        open={Boolean(varianteDe)}
        onClose={() => setVarianteDe(null)}
        title={varianteDe?.nombre ?? 'Seleccionar variante'}
        size="sm"
        footer={
          <Button variant="ghost" onClick={() => setVarianteDe(null)}>
            Cancelar
          </Button>
        }
      >
        <div className="flex flex-col gap-2">
          {varianteDe?.variantes.map((variante) => (
            <button
              key={variante.id}
              type="button"
              onClick={() => seleccionarVariante(variante)}
              className="flex items-center justify-between rounded-xl border border-border px-3 py-2 text-left text-sm transition hover:border-primary/50"
            >
              <span className="text-foreground">{variante.nombre}</span>
              <span className="num text-muted-foreground">{formatUSD(variante.precioVentaUSD)}</span>
            </button>
          ))}
        </div>
      </Modal>
    </div>
  );
}