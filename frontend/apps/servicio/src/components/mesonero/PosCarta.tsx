import { useEffect, useMemo, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  ArrowLeft,
  Banknote,
  Calculator,
  CheckCircle2,
  CircleDollarSign,
  CookingPot,
  Divide,
  HandCoins,
  Minus,
  Percent,
  Plus,
  Search,
  Sparkles,
  UtensilsCrossed,
  Wine,
  X,
} from 'lucide-react';
import { Button, cn, Input, Modal, Select } from '@licoreria/ui';
import type { Cuenta, EstadoItemComanda, Producto, ProductoVariante } from '@licoreria/types';
import { catalogoApi, cuentasApi, ventasApi } from '@licoreria/api-client';
import { formatUSD } from '../../lib/format';
import { mensajeDeError } from '../../lib/api';
import { urlDeImagen } from '../../lib/imagen';

type Area = 'Barra' | 'Cocina';
type AreaAuto = 'Auto' | Area;

const OPCIONES_AREA: AreaAuto[] = ['Auto', 'Barra', 'Cocina'];

export interface Linea {
  key: string;
  varianteId: string;
  nombre: string;
  varianteNombre: string;
  precioUSD: number;
  cantidad: number;
  area: Area;
  esCortesia: boolean;
  grupo: string;
}

let secuencia = 0;
const nuevoKey = () => `lc-${Date.now().toString(36)}-${secuencia++}`;

function grupoDeCategoria(nombre: string): string {
  const n = (nombre ?? '').toLowerCase();
  if (/licor|ron|whisky|vodka|tequila|trago|coctel/i.test(n)) return 'Tragos';
  if (/gaseosa|energizante|agua|jugo|refresco|te|cerveza/i.test(n)) return 'Bebidas';
  if (/pasabocas|snack|entrada/i.test(n)) return 'Entradas';
  return 'Comida';
}

const ESTADO_ITEM: Record<EstadoItemComanda, { etiqueta: string; clase: string }> = {
  Recibido: { etiqueta: 'Recibido', clase: 'bg-muted/60 text-muted-foreground' },
  EnProceso: { etiqueta: 'En proceso', clase: 'bg-warning/15 text-warning-fg' },
  Preparado: { etiqueta: 'Listo', clase: 'bg-success/15 text-success-fg' },
  Entregado: { etiqueta: 'Entregado', clase: 'bg-info/15 text-info-fg' },
  Cancelado: { etiqueta: 'Cancelado', clase: 'bg-destructive/10 text-destructive-fg' },
};

function EstadoItem({ estado }: { estado: EstadoItemComanda }) {
  const info = ESTADO_ITEM[estado] ?? ESTADO_ITEM.Recibido;
  return (
    <span className={cn('inline-flex items-center gap-1 rounded-full px-2 py-0.5 text-[10px] font-medium', info.clase)}>
      {estado === 'Preparado' && <CheckCircle2 size={10} />}
      {estado === 'EnProceso' && <CookingPot size={10} />}
      {info.etiqueta}
    </span>
  );
}

function ImagenProducto({ url, nombre }: { url: string | null; nombre: string }) {
  const [falla, setFalla] = useState(false);
  const src = falla ? null : urlDeImagen(url);

  if (!src) {
    return (
      <div className="flex h-full w-full items-center justify-center bg-muted/60">
        <span className="text-lg font-medium text-muted-foreground/60">{nombre.trim().charAt(0).toUpperCase()}</span>
      </div>
    );
  }

  return (
    <img
      src={src}
      alt={nombre}
      className="h-full w-full object-cover"
      loading="lazy"
      onError={() => setFalla(true)}
    />
  );
}

export function PosCarta({
  cuenta,
  lineas,
  onLineasChange,
  onActualizar,
  onVolver,
}: {
  cuenta: Cuenta;
  lineas: Linea[];
  onLineasChange: (lineas: Linea[]) => void;
  onActualizar: (cuenta: Cuenta) => void;
  onVolver: () => void;
}) {
  const queryClient = useQueryClient();
  const [categoriaId, setCategoriaId] = useState<string | null>(null);
  const [busqueda, setBusqueda] = useState('');
  const [areaActual, setAreaActual] = useState<AreaAuto>('Auto');
  const [tab, setTab] = useState<'comanda' | 'consumos'>('comanda');
  const [panel, setPanel] = useState<'descuento' | 'calculadora' | 'abonar' | 'dividir' | 'cobrar' | null>(null);

  const categorias = useQuery({ queryKey: ['carta-categorias'], queryFn: catalogoApi.categorias });
  const productos = useQuery({
    queryKey: ['carta-productos', categoriaId, busqueda],
    queryFn: () =>
      catalogoApi.productos({ busqueda: busqueda.trim() || undefined, categoriaId: categoriaId ?? undefined, pageSize: 100, activo: true }),
  });
  const metodos = useQuery({ queryKey: ['metodos-pago'], queryFn: ventasApi.metodosPago });

  const refresh = () => {
    queryClient.invalidateQueries({ queryKey: ['mis-cuentas'] });
    queryClient.invalidateQueries({ queryKey: ['carta-cuenta'] });
  };

  const cuentaQuery = useQuery({
    queryKey: ['carta-cuenta', cuenta.id],
    queryFn: () => cuentasApi.obtener(cuenta.id),
  });
  const cuentaViva = cuentaQuery.data ?? cuenta;

  const agregar = (producto: Producto, variante: ProductoVariante) => {
    const categoria = categorias.data?.find((c) => c.id === producto.categoriaId);
    const area = areaActual === 'Auto' ? (producto.areaDestino ?? 'Barra') : areaActual;
    const grupo = grupoDeCategoria(categoria?.nombre ?? producto.categoriaNombre ?? '');
    onLineasChange(
      (() => {
        const existente = lineas.find((l) => l.varianteId === variante.id && l.area === area);
        if (existente) {
          return lineas.map((l) => (l.key === existente.key ? { ...l, cantidad: l.cantidad + 1 } : l));
        }
        return [
          ...lineas,
          {
            key: nuevoKey(),
            varianteId: variante.id,
            nombre: producto.nombre,
            varianteNombre: variante.nombre,
            precioUSD: variante.precioVentaUSD,
            cantidad: 1,
            area,
            esCortesia: false,
            grupo,
          },
        ];
      })(),
    );
  };

  const seleccionarProducto = (producto: Producto) => {
    const variante = producto.variantes[0];
    if (variante) agregar(producto, variante);
  };

  const cambiarCantidad = (key: string, delta: number) =>
    onLineasChange(lineas.map((l) => (l.key === key ? { ...l, cantidad: l.cantidad + delta } : l)).filter((l) => l.cantidad > 0));
  const alternarArea = (key: string) =>
    onLineasChange(lineas.map((l) => (l.key === key ? { ...l, area: l.area === 'Barra' ? 'Cocina' : 'Barra' } : l)));
  const alternarCortesia = (key: string) =>
    onLineasChange(lineas.map((l) => (l.key === key ? { ...l, esCortesia: !l.esCortesia } : l)));
  const quitar = (key: string) => onLineasChange(lineas.filter((l) => l.key !== key));

  const enviar = useMutation({
    mutationFn: ({ area, items }: { area: Area; items: { varianteId: string; cantidad: number; esCortesia: boolean }[] }) =>
      cuentasApi.agregarComanda(cuenta.id, { area, items }),
    onSuccess: (cuentaActualizada, variables) => {
      toast.success(`Comanda enviada a ${variables.area}`);
      onActualizar(cuentaActualizada);
      queryClient.invalidateQueries({ queryKey: ['carta-cuenta', cuenta.id] });
      onLineasChange(lineas.filter((l) => l.area !== variables.area));
      refresh();
    },
    onError: (error) => toast.error('No se pudo enviar', { description: mensajeDeError(error) }),
  });

  const enviarArea = (area: Area) => {
    const pendientes = lineas.filter((l) => l.area === area);
    if (pendientes.length === 0) {
      toast.info(`No hay ítems pendientes para ${area}`);
      return;
    }
    enviar.mutate({ area, items: pendientes.map((l) => ({ varianteId: l.varianteId, cantidad: l.cantidad, esCortesia: l.esCortesia })) });
  };

  const abonar = useMutation({
    mutationFn: (body: { metodoPagoId: string; monto: number }) => cuentasApi.abonar(cuenta.id, { ...body, moneda: 'USD' }),
    onSuccess: (cuentaActualizada) => {
      toast.success('Abono registrado');
      setPanel(null);
      onActualizar(cuentaActualizada);
      queryClient.invalidateQueries({ queryKey: ['carta-cuenta', cuenta.id] });
    },
    onError: (error) => toast.error('No se pudo abonar', { description: mensajeDeError(error) }),
  });

  const dividir = useMutation({
    mutationFn: (partes: number) => cuentasApi.dividir(cuenta.id, { partes }),
    onSuccess: () => {
      toast.success('Cuenta dividida');
      setPanel(null);
      queryClient.invalidateQueries({ queryKey: ['carta-cuenta', cuenta.id] });
    },
    onError: (error) => toast.error('No se pudo dividir', { description: mensajeDeError(error) }),
  });

  const cerrar = useMutation({
    mutationFn: (body: { metodoPagoId: string; monto: number; descuentoUSD?: number }) =>
      cuentasApi.cerrar(cuenta.id, {
        pagos: [{ metodoPagoId: body.metodoPagoId, monto: body.monto, moneda: 'USD' }],
        descuentoUSD: body.descuentoUSD,
      }),
    onSuccess: () => {
      toast.success('Cuenta cobrada');
      setPanel(null);
      onVolver();
    },
    onError: (error) => toast.error('No se pudo cobrar', { description: mensajeDeError(error) }),
  });

  const pendientesBarra = lineas.filter((l) => l.area === 'Barra');
  const pendientesCocina = lineas.filter((l) => l.area === 'Cocina');
  const total = useMemo(
    () => lineas.reduce((acc, l) => acc + l.precioUSD * l.cantidad, 0),
    [lineas],
  );
  const totalArea = (items: Linea[]) => items.reduce((acc, l) => acc + l.precioUSD * l.cantidad, 0);

  const consumos = useMemo(() => (cuentaViva.comandas ?? []).flatMap((c) => c.detalles), [cuentaViva.comandas]);

  return (
    <div className="flex h-full min-h-0 flex-col">
      {/* Header */}
      <header className="flex items-center gap-3 border-b border-border pb-3">
        <button
          type="button"
          aria-label="Volver"
          onClick={onVolver}
          className="flex h-10 w-10 items-center justify-center rounded-full border border-border text-foreground transition hover:bg-accent/10"
        >
          <ArrowLeft size={18} />
        </button>
        <div className="min-w-0">
          <p className="truncate text-base font-medium text-foreground">{cuentaViva.nombreMesa}</p>
          <p className="truncate text-xs text-muted-foreground">
            {cuentaViva.cliente ? `Cliente: ${cuentaViva.cliente}` : 'Sin cliente'} · {consumos.length} consumos
          </p>
        </div>
        <div className="ml-auto text-right">
          <p className="text-[11px] text-muted-foreground">Saldo</p>
          <p className="num text-lg font-medium text-foreground">{formatUSD(cuentaViva.saldo)}</p>
        </div>
      </header>

      {/* Cuerpo: comanda | menú | carta por categorías */}
      <div className="grid min-h-0 flex-1 grid-cols-1 gap-3 pt-3 lg:grid-cols-[22rem_minmax(0,1fr)_15rem] 2xl:grid-cols-[26rem_minmax(0,1fr)_18rem]">
        {/* Izquierda: comanda / consumos */}
        <aside className="flex min-h-0 flex-col rounded-lg border border-border bg-card/40 p-3">
          <div className="flex gap-1.5">
            <button
              type="button"
              onClick={() => setTab('comanda')}
              className={cn(
                'flex h-8 flex-1 items-center justify-center gap-1.5 rounded-full border text-[11px] transition',
                tab === 'comanda' ? 'border-primary bg-primary/15 font-medium text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
              )}
            >
              <UtensilsCrossed size={13} /> Por enviar ({lineas.length})
            </button>
            <button
              type="button"
              onClick={() => setTab('consumos')}
              className={cn(
                'flex h-8 flex-1 items-center justify-center gap-1.5 rounded-full border text-[11px] transition',
                tab === 'consumos' ? 'border-primary bg-primary/15 font-medium text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
              )}
            >
              <CheckCircle2 size={13} /> Consumos ({consumos.length})
            </button>
          </div>

          {tab === 'comanda' ? (
            <>
              <div className="app-scroll mt-2 min-h-0 flex-1 overflow-y-auto pr-1">
                {lineas.length === 0 ? (
                  <p className="py-8 text-center text-xs text-muted-foreground">
                    Agrega productos del menú; cada uno irá a su área (Barra/Cocina) automáticamente.
                  </p>
                ) : (
                  <div className="flex flex-col gap-3">
                    {[pendientesBarra, pendientesCocina].map((items, indice) => {
                      const area = indice === 0 ? 'Barra' : 'Cocina';
                      if (items.length === 0) return null;
                      return (
                        <div key={area}>
                          <div className="flex items-center justify-between">
                            <span className="inline-flex items-center gap-1.5 text-xs font-medium text-foreground">
                              {area === 'Barra' ? <Wine size={13} /> : <UtensilsCrossed size={13} />}
                              {area}
                              <span className="num text-[10px] text-muted-foreground">{formatUSD(totalArea(items))}</span>
                            </span>
                            <button
                              type="button"
                              onClick={() => onLineasChange(lineas.filter((l) => l.area !== area))}
                              className="text-[10px] text-muted-foreground transition hover:text-destructive-fg"
                            >
                              Vaciar
                            </button>
                          </div>
                          <ul className="mt-1.5 flex flex-col gap-1.5">
                            {items.map((l) => (
                              <li key={l.key} className="rounded-inner bg-muted/40 px-2.5 py-2">
                                <div className="flex items-start justify-between gap-2">
                                  <div className="min-w-0">
                                    <p className="truncate text-xs text-foreground">
                                      {l.cantidad} × {l.nombre}
                                      <span className="text-muted-foreground"> · {l.varianteNombre}</span>
                                    </p>
                                    <div className="mt-1 flex items-center gap-2">
                                      <button
                                        type="button"
                                        onClick={() => alternarArea(l.key)}
                                        className={cn(
                                          'rounded-full border px-1.5 py-0.5 text-[10px]',
                                          l.area === 'Barra' ? 'border-primary/40 text-foreground' : 'border-warning/40 text-warning-fg',
                                        )}
                                        title={`Mover a ${l.area === 'Barra' ? 'Cocina' : 'Barra'}`}
                                      >
                                        {l.area === 'Barra' ? '🍸 Barra' : '🍽 Cocina'}
                                      </button>
                                      <button
                                        type="button"
                                        onClick={() => alternarCortesia(l.key)}
                                        className="text-[10px] text-muted-foreground transition hover:text-foreground"
                                      >
                                        {l.esCortesia ? 'cortesía' : 'cortesía?'}
                                      </button>
                                    </div>
                                  </div>
                                  <button
                                    type="button"
                                    aria-label="Quitar"
                                    onClick={() => quitar(l.key)}
                                    className="text-muted-foreground transition hover:text-destructive-fg"
                                  >
                                    <X size={13} />
                                  </button>
                                </div>
                                <div className="mt-1.5 flex items-center justify-end gap-1.5">
                                  <button
                                    type="button"
                                    onClick={() => cambiarCantidad(l.key, -1)}
                                    className="flex h-6 w-6 items-center justify-center rounded-full border border-border text-foreground"
                                  >
                                    <Minus size={11} />
                                  </button>
                                  <span className="num w-5 text-center text-xs text-foreground">{l.cantidad}</span>
                                  <button
                                    type="button"
                                    onClick={() => cambiarCantidad(l.key, 1)}
                                    className="flex h-6 w-6 items-center justify-center rounded-full border border-border text-foreground"
                                  >
                                    <Plus size={11} />
                                  </button>
                                </div>
                              </li>
                            ))}
                          </ul>
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
              <div className="mt-2 flex flex-col gap-2 border-t border-border pt-2">
                <div className="flex items-center justify-between text-xs">
                  <span className="text-muted-foreground">Total por enviar</span>
                  <span className="num text-sm font-medium text-foreground">{formatUSD(total)}</span>
                </div>
                <div className="grid grid-cols-2 gap-2">
                  <Button
                    size="sm"
                    variant="ghost"
                    disabled={enviar.isPending || pendientesBarra.length === 0}
                    onClick={() => enviarArea('Barra')}
                  >
                    <Wine size={14} /> Barra {pendientesBarra.length > 0 && `(${pendientesBarra.length})`}
                  </Button>
                  <Button size="sm" disabled={enviar.isPending || pendientesCocina.length === 0} onClick={() => enviarArea('Cocina')}>
                    <UtensilsCrossed size={14} /> Cocina {pendientesCocina.length > 0 && `(${pendientesCocina.length})`}
                  </Button>
                </div>
              </div>
            </>
          ) : (
            <div className="app-scroll mt-2 min-h-0 flex-1 overflow-y-auto pr-1">
              {consumos.length === 0 ? (
                <p className="py-8 text-center text-xs text-muted-foreground">Todavía no hay consumos en esta mesa.</p>
              ) : (
                <ul className="flex flex-col gap-2">
                  {cuentaViva.comandas.map((comanda) => (
                    <li key={comanda.id} className="rounded-inner bg-muted/40 px-2.5 py-2">
                      <div className="mb-1 flex items-center justify-between">
                        <span className="inline-flex items-center gap-1 text-[10px] font-medium uppercase tracking-tighter2 text-muted-foreground">
                          {comanda.area === 'Barra' ? <Wine size={11} /> : <UtensilsCrossed size={11} />}
                          {comanda.area}
                        </span>
                        <span className="num text-[10px] text-muted-foreground">{formatTime(comanda.fecha)}</span>
                      </div>
                      <ul className="flex flex-col gap-1">
                        {comanda.detalles.map((detalle) => (
                          <li key={detalle.id} className="flex items-center justify-between gap-2">
                            <div className="min-w-0">
                              <p className="truncate text-xs text-foreground">
                                {detalle.cantidad} × {detalle.nombre}
                                {detalle.esCortesia && <span className="text-[10px] text-muted-foreground"> · cortesía</span>}
                              </p>
                              <p className="num text-[10px] text-muted-foreground">{formatUSD(detalle.subtotalUSD)}</p>
                            </div>
                            <EstadoItem estado={detalle.estado} />
                          </li>
                        ))}
                      </ul>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          )}
        </aside>

        {/* Centro: menú */}
        <div className="flex min-h-0 flex-col rounded-lg border border-border bg-card/40 p-3">
          <div className="flex items-center gap-2">
            <label className="relative flex flex-1 items-center">
              <Search size={15} className="absolute left-3 text-muted-foreground" />
              <input
                aria-label="Buscar producto"
                placeholder="Buscar…"
                value={busqueda}
                onChange={(e) => setBusqueda(e.target.value)}
                className="h-9 w-full rounded-control border border-border bg-card pl-9 pr-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50"
              />
            </label>
            <span className="num text-xs text-muted-foreground">{productos.data?.totalItems ?? 0}</span>
          </div>

          <div className="mt-2 flex gap-1.5">
            {OPCIONES_AREA.map((valor) => (
              <button
                key={valor}
                type="button"
                onClick={() => setAreaActual(valor)}
                className={cn(
                  'flex h-8 flex-1 items-center justify-center gap-1.5 rounded-full border px-2 text-[11px] transition',
                  areaActual === valor ? 'border-primary bg-primary/15 font-medium text-foreground' : 'border-border text-muted-foreground hover:text-foreground',
                )}
              >
                {valor === 'Barra' ? <Wine size={13} /> : valor === 'Cocina' ? <UtensilsCrossed size={13} /> : <Sparkles size={13} />}
                {valor}
              </button>
            ))}
          </div>
          <p className="mt-1 text-[10px] text-muted-foreground">
            {areaActual === 'Auto' ? 'Cada producto irá al área registrada (Barra/Cocina).' : <>Lo que agregues irá a {areaActual}.</>}
          </p>

          <div className="app-scroll mt-2 min-h-0 flex-1 overflow-y-auto pr-1">
            {productos.isLoading ? (
              <p className="py-10 text-center text-sm text-muted-foreground">Cargando…</p>
            ) : (productos.data?.items?.length ?? 0) === 0 ? (
              <p className="py-10 text-center text-sm text-muted-foreground">Sin productos en esta categoría.</p>
            ) : (
              <div className="grid grid-cols-2 gap-2 sm:grid-cols-3 xl:grid-cols-4">
                {productos.data?.items?.map((producto) => (
                  <button
                    key={producto.id}
                    type="button"
                    onClick={() => seleccionarProducto(producto)}
                    className="flex flex-col overflow-hidden rounded-inner border border-border bg-card transition hover:border-primary hover:bg-primary/10"
                  >
                    <div className="flex h-20 items-center justify-center overflow-hidden bg-muted/50">
                      <ImagenProducto url={producto.imagenUrl} nombre={producto.nombre} />
                    </div>
                    <div className="flex flex-col gap-0.5 p-2">
                      <p className="line-clamp-1 text-xs font-medium text-foreground">{producto.nombre}</p>
                      <div className="flex items-center justify-between gap-1">
                        <p className="num text-[11px] text-foreground">{formatUSD(producto.variantes[0]?.precioVentaUSD ?? 0)}</p>
                        <span
                          className={cn(
                            'inline-flex items-center gap-1 text-[10px]',
                            producto.areaDestino === 'Cocina' ? 'text-info-fg' : 'text-foreground',
                          )}
                        >
                          {producto.areaDestino === 'Cocina' ? <UtensilsCrossed size={10} /> : <Wine size={10} />}
                          {producto.areaDestino ?? 'Barra'}
                        </span>
                      </div>
                    </div>
                  </button>
                ))}
              </div>
            )}
          </div>
        </div>

        {/* Derecha: carta por categorías */}
        <aside className="app-scroll min-h-0 overflow-y-auto rounded-lg border border-border bg-card/40 p-3">
          <p className="text-[11px] font-medium uppercase tracking-tighter2 text-muted-foreground">Carta</p>
          <div className="mt-2 flex flex-col gap-1">
            <button
              type="button"
              onClick={() => setCategoriaId(null)}
              className={cn(
                'rounded-full px-3 py-1.5 text-left text-sm transition',
                categoriaId === null ? 'bg-primary/20 font-medium text-foreground' : 'text-foreground hover:bg-accent/10',
              )}
            >
              Todo
            </button>
            {(categorias.data ?? []).map((categoria) => (
              <button
                key={categoria.id}
                type="button"
                onClick={() => setCategoriaId(categoria.id)}
                className={cn(
                  'rounded-full px-3 py-1.5 text-left text-sm transition',
                  categoriaId === categoria.id ? 'bg-primary/20 font-medium text-foreground' : 'text-foreground hover:bg-accent/10',
                )}
              >
                {categoria.nombre}
              </button>
            ))}
          </div>
        </aside>
      </div>

      {/* Barra inferior de acciones */}
      <footer className="mt-3 flex items-center gap-2 border-t border-border pt-3">
        <BotonAccion icon={<Percent size={16} />} label="Descuento" onClick={() => setPanel('descuento')} />
        <BotonAccion icon={<Calculator size={16} />} label="Calculadora" onClick={() => setPanel('calculadora')} />
        <BotonAccion icon={<Banknote size={16} />} label="Cajón" onClick={() => setPanel('cobrar')} />
        <BotonAccion icon={<HandCoins size={16} />} label="Abonar" onClick={() => setPanel('abonar')} />
        <BotonAccion icon={<Divide size={16} />} label="Dividir" onClick={() => setPanel('dividir')} />
        <BotonAccion icon={<CircleDollarSign size={16} />} label="Cobrar" onClick={() => setPanel('cobrar')} />
        <div className="ml-auto flex items-center gap-2">
          <span className="text-[11px] text-muted-foreground">Saldo</span>
          <span className="num text-lg font-medium text-foreground">{formatUSD(cuentaViva.saldo)}</span>
        </div>
      </footer>

      <Modales
        panel={panel}
        onCerrar={() => setPanel(null)}
        metodos={metodos.data ?? []}
        saldo={cuentaViva.saldo}
        onAbonar={(metodoPagoId, monto) => abonar.mutate({ metodoPagoId, monto })}
        onDividir={(partes) => dividir.mutate(partes)}
        onCobrar={(metodoPagoId, monto, descuento) => cerrar.mutate({ metodoPagoId, monto, descuentoUSD: descuento })}
      />
    </div>
  );
}

function formatTime(iso: string): string {
  try {
    return new Date(iso).toLocaleTimeString('es-VE', { hour: '2-digit', minute: '2-digit' });
  } catch {
    return '';
  }
}

function BotonAccion({ icon, label, onClick }: { icon: React.ReactNode; label: string; onClick: () => void }) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="flex flex-col items-center gap-1 rounded-xl border border-border bg-card/60 px-3 py-2 text-[11px] text-foreground transition hover:border-primary hover:bg-primary/10"
    >
      <span className="text-foreground">{icon}</span>
      {label}
    </button>
  );
}

function Modales({
  panel,
  onCerrar,
  metodos,
  saldo,
  onAbonar,
  onDividir,
  onCobrar,
}: {
  panel: 'descuento' | 'calculadora' | 'abonar' | 'dividir' | 'cobrar' | null;
  onCerrar: () => void;
  metodos: { id: string; nombre: string }[];
  saldo: number;
  onAbonar: (metodoPagoId: string, monto: number) => void;
  onDividir: (partes: number) => void;
  onCobrar: (metodoPagoId: string, monto: number, descuento?: number) => void;
}) {
  const [monto, setMonto] = useState(saldo);
  const [descuento, setDescuento] = useState(0);
  const [partes, setPartes] = useState(2);
  const [metodo, setMetodo] = useState('');
  const [calc, setCalc] = useState('');

  useEffect(() => {
    if (panel === 'abonar' || panel === 'cobrar') setMonto(saldo);
    if (panel === 'cobrar') setDescuento(0);
    if (metodos.length > 0 && !metodo) setMetodo(metodos[0].id);
    if (panel === 'calculadora') setCalc('');
  }, [panel, saldo, metodos, metodo]);

  const pie = (confirma: string, onClick: () => void, cargando = false) => (
    <>
      <Button variant="ghost" onClick={onCerrar}>
        Cancelar
      </Button>
      <Button onClick={onClick} loading={cargando}>
        {confirma}
      </Button>
    </>
  );

  if (!panel) return null;

  return (
    <>
      {panel === 'descuento' && (
        <Modal open onClose={onCerrar} title="Descuento" footer={pie('Aplicar', () => onCobrar(metodo, saldo - descuento, descuento))}>
          <Input label="Descuento (USD)" type="number" value={descuento} onChange={(e) => setDescuento(Math.max(0, Number(e.target.value)))} />
        </Modal>
      )}

      {panel === 'calculadora' && (
        <Modal open onClose={onCerrar} title="Calculadora" footer={pie('Usar', () => setMonto(Number(calc || 0)))}>
          <input
            readOnly
            aria-label="Resultado"
            value={calc}
            className="num h-12 w-full rounded-control border border-border bg-muted/40 px-3 text-right text-xl text-foreground"
          />
          <div className="mt-3 grid grid-cols-3 gap-2">
            {['7', '8', '9', '4', '5', '6', '1', '2', '3', '0', '.', 'C'].map((tecla) => (
              <button
                key={tecla}
                type="button"
                onClick={() => setCalc((v) => (tecla === 'C' ? '' : v + tecla))}
                className="h-11 rounded-control border border-border bg-card text-sm text-foreground transition hover:bg-accent/10"
              >
                {tecla}
              </button>
            ))}
          </div>
        </Modal>
      )}

      {panel === 'abonar' && (
        <Modal open onClose={onCerrar} title="Abonar" footer={pie('Abonar', () => metodo && onAbonar(metodo, monto), false)}>
          <div className="flex flex-col gap-3">
            <Input label="Monto" type="number" value={monto} onChange={(e) => setMonto(Number(e.target.value))} />
            <Select label="Método" value={metodo} onChange={(e) => setMetodo(e.target.value)}>
              {metodos.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.nombre}
                </option>
              ))}
            </Select>
          </div>
        </Modal>
      )}

      {panel === 'dividir' && (
        <Modal open onClose={onCerrar} title="Dividir cuenta" footer={pie('Dividir', () => onDividir(partes))}>
          <Input label="Partes" type="number" min={2} value={partes} onChange={(e) => setPartes(Math.max(2, Number(e.target.value)))} />
          <p className="mt-2 text-xs text-muted-foreground">
            Cada parte quedará por <span className="num text-foreground">{formatUSD(saldo / Math.max(1, partes))}</span>
          </p>
        </Modal>
      )}

      {panel === 'cobrar' && (
        <Modal open onClose={onCerrar} title="Cobrar" footer={pie('Cobrar', () => metodo && onCobrar(metodo, saldo - descuento, descuento))}>
          <div className="flex flex-col gap-3">
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Saldo</span>
              <span className="num font-medium text-foreground">{formatUSD(saldo)}</span>
            </div>
            <div className="flex items-center justify-between text-sm">
              <span className="text-muted-foreground">Descuento</span>
              <span className="num text-destructive-fg">−{formatUSD(descuento)}</span>
            </div>
            <div className="flex items-center justify-between border-t border-border pt-2 text-sm">
              <span className="font-medium text-foreground">Total a cobrar</span>
              <span className="num text-lg font-medium text-foreground">{formatUSD(saldo - descuento)}</span>
            </div>
            <Select label="Método" value={metodo} onChange={(e) => setMetodo(e.target.value)}>
              {metodos.map((m) => (
                <option key={m.id} value={m.id}>
                  {m.nombre}
                </option>
              ))}
            </Select>
            <label className="flex items-center gap-2 text-xs text-muted-foreground">
              <input type="checkbox" checked={descuento > 0} onChange={(e) => setDescuento(e.target.checked ? Math.min(10, saldo) : 0)} />
              Aplicar descuento
            </label>
            {descuento > 0 && <Input label="Descuento (USD)" type="number" value={descuento} onChange={(e) => setDescuento(Math.max(0, Number(e.target.value)))} />}
          </div>
        </Modal>
      )}
    </>
  );
}