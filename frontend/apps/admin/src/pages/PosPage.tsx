import { useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { useMutation, useQuery } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Keyboard, Search } from 'lucide-react';
import { Button, Card, Input, PageHeader, Pill } from '@licoreria/ui';
import type { Producto, ProductoVariante, TasaCambio, Venta } from '@licoreria/types';
import { catalogoApi, finanzasApi, promocionesApi, ventasApi } from '@licoreria/api-client';
import { mensajeDeError } from '../lib/api';
import { formatBS } from '../lib/format';
import { precioUnitario, totalConPropina, usePos } from '../hooks/usePos';
import { usePosHotkeys, type HotkeyMap } from '../hooks/usePosHotkeys';
import { AyudaAtajos } from '../components/pos/AyudaAtajos';
import { CategoriaBar } from '../components/pos/CategoriaBar';
import { OrdenPanel } from '../components/pos/OrdenPanel';
import { ProductoGrid, type ProductoGridHandle } from '../components/pos/ProductoGrid';
import { SelectorProductoModal, type SeleccionProducto } from '../components/pos/SelectorProductoModal';
import { TicketVenta } from '../components/pos/TicketVenta';

export function PosPage() {
  const [params] = useSearchParams();
  const [busqueda, setBusqueda] = useState(params.get('busqueda') ?? '');
  const [busquedaDebounced, setBusquedaDebounced] = useState(busqueda.trim());
  const [categoriaId, setCategoriaId] = useState<string | null>(null);
  const [productoModal, setProductoModal] = useState<Producto | null>(null);
  const [ayudaAbierta, setAyudaAbierta] = useState(false);
  const [ventaTicket, setVentaTicket] = useState<Venta | null>(null);

  const busquedaRef = useRef<HTMLInputElement>(null);
  const descuentoRef = useRef<HTMLInputElement>(null);
  const gridRef = useRef<ProductoGridHandle>(null);

  const pos = usePos();
  const activa = pos.activa;

  useEffect(() => {
    const temporizador = setTimeout(() => setBusquedaDebounced(busqueda.trim()), 250);
    return () => clearTimeout(temporizador);
  }, [busqueda]);

  const categorias = useQuery({ queryKey: ['pos', 'categorias'], queryFn: catalogoApi.categorias });
  const productos = useQuery({
    queryKey: ['pos', 'productos', categoriaId, busquedaDebounced],
    queryFn: () =>
      catalogoApi.productos({
        busqueda: busquedaDebounced,
        categoriaId: categoriaId ?? undefined,
        pageSize: 100,
        activo: true,
      }),
  });
  const metodos = useQuery({ queryKey: ['pos', 'metodos-pago'], queryFn: ventasApi.metodosPago });
  const promociones = useQuery({ queryKey: ['pos', 'promociones'], queryFn: promocionesApi.listar });
  const tasa = useQuery({ queryKey: ['pos', 'tasa'], queryFn: () => finanzasApi.tasaActual('Paralelo') });

  const valorTasa = (tasa.data as TasaCambio | undefined)?.valor ?? 0;
  const listaProductos = productos.data?.items ?? [];
  const listaCategorias = useMemo(
    () => [null as string | null, ...(categorias.data?.map((categoria) => categoria.id) ?? [])],
    [categorias.data],
  );

  const totalCobrar = totalConPropina(activa);
  const metodoPredeterminado = metodos.data?.[0] ?? null;

  const registrar = useMutation({
    mutationFn: () =>
      ventasApi.registrar({
        items: activa.lineas.map((linea) => ({
          varianteId: linea.varianteId,
          cantidad: linea.cantidad,
          precioUnitarioUSD: precioUnitario(linea),
        })),
        descuentoUSD: activa.descuentoUSD,
        promocionId: activa.promocionId || null,
        pagos: metodoPredeterminado
          ? [
              {
                metodoPagoId: metodoPredeterminado.id,
                monto: totalCobrar,
                moneda: 'USD',
                propina: activa.propinaUSD,
              },
            ]
          : [],
      }),
    onSuccess: (venta) => {
      setVentaTicket(venta);
      pos.reiniciar();
    },
    onError: (error) => toast.error('No se pudo registrar la venta', { description: mensajeDeError(error) }),
  });

  const agregarVariante = (producto: Producto, variante: ProductoVariante) =>
    pos.agregarLinea({
      varianteId: variante.id,
      productoId: producto.id,
      nombre: producto.nombre,
      varianteNombre: variante.nombre,
      sku: variante.sku,
      precioBaseUSD: variante.precioVentaUSD,
      modificadores: [],
      cantidad: 1,
    });

  const seleccionarProducto = (producto: Producto) => {
    if (producto.variantes.length === 1 && producto.tipo !== 'Preparado') {
      agregarVariante(producto, producto.variantes[0]);
      return;
    }
    setProductoModal(producto);
  };

  const confirmarSeleccion = (seleccion: SeleccionProducto) => {
    if (!productoModal) return;
    pos.agregarLinea({
      varianteId: seleccion.variante.id,
      productoId: productoModal.id,
      nombre: productoModal.nombre,
      varianteNombre: seleccion.variante.nombre,
      sku: seleccion.variante.sku,
      precioBaseUSD: seleccion.variante.precioVentaUSD,
      modificadores: seleccion.modificadores,
      cantidad: seleccion.cantidad,
    });
    setProductoModal(null);
    busquedaRef.current?.focus();
  };

  const cobrar = () => {
    if (activa.lineas.length === 0) {
      toast.error('Agrega al menos un producto');
      return;
    }
    if (!metodoPredeterminado) {
      toast.error('No hay métodos de pago configurados');
      return;
    }
    registrar.mutate();
  };

  const moverCategoria = (delta: number) =>
    setCategoriaId((actual) => {
      const indice = Math.max(0, listaCategorias.indexOf(actual));
      const siguiente = Math.min(Math.max(0, indice + delta), listaCategorias.length - 1);
      return listaCategorias[siguiente];
    });

  const { ordenes, activar } = pos;
  const ordenHotkeys = useMemo<HotkeyMap>(() => {
    const mapa: HotkeyMap = {};
    ordenes.slice(0, 9).forEach((orden, indice) => {
      mapa[`alt+${indice + 1}`] = () => activar(orden.id);
    });
    return mapa;
  }, [ordenes, activar]);

  useEffect(() => {
    if (productoModal || ayudaAbierta || ventaTicket) return;
    const alEscribir = (evento: KeyboardEvent) => {
      if (evento.ctrlKey || evento.metaKey || evento.altKey) return;
      const objetivo = evento.target as HTMLElement | null;
      if (
        objetivo &&
        (objetivo.tagName === 'INPUT' ||
          objetivo.tagName === 'TEXTAREA' ||
          objetivo.tagName === 'SELECT' ||
          objetivo.isContentEditable)
      ) {
        return;
      }
      if (evento.key.length !== 1 || /[+\-\s]/.test(evento.key)) return;
      evento.preventDefault();
      busquedaRef.current?.focus();
      setBusqueda(evento.key);
    };
    window.addEventListener('keydown', alEscribir);
    return () => window.removeEventListener('keydown', alEscribir);
  }, [productoModal, ayudaAbierta, ventaTicket]);

  const lineaSeleccionada = pos.lineaSeleccionada;

  usePosHotkeys(
    {
      F1: () => setAyudaAbierta(true),
      F2: () => busquedaRef.current?.focus(),
      '/': () => busquedaRef.current?.focus(),
      'ctrl+f': () => busquedaRef.current?.focus(),
      F4: () => cobrar(),
      F8: () => descuentoRef.current?.focus(),
      'ctrl+n': () => pos.nueva(`Venta ${pos.ordenes.length + 1}`),
      '+': () => lineaSeleccionada && pos.cantidad(lineaSeleccionada.key, 1),
      '-': () => lineaSeleccionada && pos.cantidad(lineaSeleccionada.key, -1),
      Delete: () => lineaSeleccionada && pos.quitarLinea(lineaSeleccionada.key),
      'ctrl+ArrowDown': () => pos.moverSeleccion(1),
      'ctrl+ArrowUp': () => pos.moverSeleccion(-1),
      ArrowLeft: () => moverCategoria(-1),
      ArrowRight: () => moverCategoria(1),
      'alt+ArrowLeft': () => moverCategoria(-1),
      'alt+ArrowRight': () => moverCategoria(1),
      Escape: () => {
        if (busqueda) setBusqueda('');
      },
      ...ordenHotkeys,
    },
    !productoModal && !ayudaAbierta && !ventaTicket,
  );

  return (
    <div className="mx-auto flex max-w-page flex-col gap-4 lg:h-[calc(100dvh-9rem)] lg:min-h-0">
      <PageHeader
        title="POS"
        actions={
          <>
            <Pill tone="accent">Tasa {valorTasa > 0 ? formatBS(valorTasa) : '—'}</Pill>
            <Button variant="ghost" size="sm" leftIcon={<Keyboard size={15} />} onClick={() => setAyudaAbierta(true)}>
              Atajos (F1)
            </Button>
          </>
        }
      />

      <div className="grid grid-cols-1 items-start gap-4 lg:min-h-0 lg:flex-1 lg:items-stretch lg:grid-cols-[minmax(0,1fr)_380px]">
        <div className="flex min-w-0 flex-col gap-4 lg:min-h-0">
          <CategoriaBar categorias={categorias.data ?? []} activa={categoriaId} onCambiar={setCategoriaId} />

          <Input
            ref={busquedaRef}
            aria-label="Buscar producto"
            placeholder="Buscar por nombre o SKU… (F2)"
            value={busqueda}
            onChange={(evento) => setBusqueda(evento.target.value)}
            onKeyDown={(evento) => {
              if (evento.key === 'Enter') {
                evento.preventDefault();
                if (listaProductos[0]) seleccionarProducto(listaProductos[0]);
              } else if (evento.key === 'ArrowDown') {
                evento.preventDefault();
                gridRef.current?.enfocar(0);
              } else if (evento.key === 'Escape') {
                setBusqueda('');
              }
            }}
            rightSlot={<Search size={16} className="text-muted" />}
          />

          <div className="app-scroll lg:min-h-0 lg:flex-1 lg:overflow-y-auto lg:pr-1">
            <ProductoGrid
              ref={gridRef}
              productos={listaProductos}
              cargando={productos.isLoading}
              onSeleccionar={seleccionarProducto}
            />
          </div>
        </div>

        <Card className="flex min-h-0 flex-col p-4 lg:h-full">
          <OrdenPanel
            pos={pos}
            promociones={promociones.data ?? []}
            registrando={registrar.isPending}
            onCobrar={cobrar}
            descuentoRef={descuentoRef}
          />
        </Card>
      </div>

      <SelectorProductoModal
        producto={productoModal}
        onCerrar={() => setProductoModal(null)}
        onConfirmar={confirmarSeleccion}
      />
      <AyudaAtajos open={ayudaAbierta} onClose={() => setAyudaAbierta(false)} />
      <TicketVenta venta={ventaTicket} onCerrar={() => setVentaTicket(null)} />
    </div>
  );
}
