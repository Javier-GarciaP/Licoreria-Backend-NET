import { useEffect, useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Button, Modal, Spinner, cn } from '@licoreria/ui';
import type { Producto, ProductoVariante } from '@licoreria/types';
import { catalogoApi } from '@licoreria/api-client';
import { formatUSD } from '../../lib/format';
import type { ModificadorSeleccionado } from '../../hooks/usePos';

export interface SeleccionProducto {
  variante: ProductoVariante;
  modificadores: ModificadorSeleccionado[];
  cantidad: number;
}

export function SelectorProductoModal({
  producto,
  onCerrar,
  onConfirmar,
}: {
  producto: Producto | null;
  onCerrar: () => void;
  onConfirmar: (seleccion: SeleccionProducto) => void;
}) {
  const abierto = producto !== null;
  const [indiceVariante, setIndiceVariante] = useState(0);
  const [seleccionados, setSeleccionados] = useState<Set<string>>(new Set());
  const [cantidad, setCantidad] = useState(1);

  const modificadoresQuery = useQuery({
    queryKey: ['pos', 'producto-modificadores', producto?.id],
    queryFn: () => catalogoApi.productoModificadores(producto!.id),
    enabled: abierto,
  });
  const recetasQuery = useQuery({
    queryKey: ['pos', 'producto-recetas', producto?.id],
    queryFn: () => catalogoApi.recetas(producto!.id),
    enabled: abierto && producto?.tipo === 'Preparado',
  });

  useEffect(() => {
    if (!abierto) return;
    setIndiceVariante(0);
    setSeleccionados(new Set());
    setCantidad(1);
  }, [abierto, producto?.id]);

  const variantes = producto?.variantes ?? [];
  const variante = variantes[indiceVariante] ?? null;
  const modificadores = useMemo(() => modificadoresQuery.data ?? [], [modificadoresQuery.data]);

  const requeridosMinimos = useMemo(
    () => modificadores.reduce((total, mod) => total + Math.max(mod.minimo, mod.requerido ? 1 : 0), 0),
    [modificadores],
  );
  const cumpleRequeridos = seleccionados.size >= requeridosMinimos;

  const totalLinea = useMemo(() => {
    if (!variante) return 0;
    const extras = modificadores
      .filter((mod) => seleccionados.has(mod.id))
      .reduce((total, mod) => total + mod.precioAdicional, 0);
    return (variante.precioVentaUSD + extras) * cantidad;
  }, [variante, modificadores, seleccionados, cantidad]);

  const alternarModificador = (id: string) =>
    setSeleccionados((actuales) => {
      const siguiente = new Set(actuales);
      if (siguiente.has(id)) siguiente.delete(id);
      else siguiente.add(id);
      return siguiente;
    });

  const confirmar = () => {
    if (!variante || !cumpleRequeridos) return;
    onConfirmar({
      variante,
      cantidad,
      modificadores: modificadores
        .filter((mod) => seleccionados.has(mod.id))
        .map((mod) => ({
          productoModificadorId: mod.id,
          modificadorId: mod.modificadorId,
          nombre: mod.modificadorNombre,
          precioAdicional: mod.precioAdicional,
        })),
    });
    onCerrar();
  };

  useEffect(() => {
    if (!abierto) return;
    const alPresionar = (evento: KeyboardEvent) => {
      const target = evento.target as HTMLElement | null;
      const enControl = target?.tagName === 'INPUT' || target?.tagName === 'SELECT';
      if ((evento.ctrlKey || evento.metaKey) && evento.key === 'Enter') {
        evento.preventDefault();
        confirmar();
        return;
      }
      if (evento.key === 'F4') {
        evento.preventDefault();
        confirmar();
        return;
      }
      if (!enControl && variantes.length > 1 && /^[1-9]$/.test(evento.key)) {
        const indice = Number(evento.key) - 1;
        if (indice < variantes.length) {
          evento.preventDefault();
          setIndiceVariante(indice);
        }
        return;
      }
      if (!enControl && evento.key === '+') {
        setCantidad((actual) => actual + 1);
      } else if (!enControl && evento.key === '-') {
        setCantidad((actual) => Math.max(1, actual - 1));
      } else if (evento.key === 'Enter') {
        const activo = document.activeElement?.tagName;
        if (activo === 'BUTTON' || activo === 'INPUT') return;
        evento.preventDefault();
        confirmar();
      }
    };
    document.addEventListener('keydown', alPresionar);
    return () => document.removeEventListener('keydown', alPresionar);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [abierto, variante, seleccionados, cantidad, modificadores, cumpleRequeridos]);

  if (!producto) return null;

  return (
    <Modal
      open={abierto}
      onClose={onCerrar}
      title={producto.nombre}
      className="max-w-xl"
      backdrop="none"
      footer={
        <>
          <Button variant="ghost" onClick={onCerrar}>
            Cancelar
          </Button>
          <Button onClick={confirmar} disabled={!cumpleRequeridos || !variante}>
            Agregar {formatUSD(totalLinea)}
          </Button>
        </>
      }
    >
      <div className="flex flex-col gap-5">
        <section className="flex flex-col gap-2">
          <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">Variante</p>
          <div className="flex flex-wrap gap-2">
            {variantes.map((item, indice) => {
              const activa = indice === indiceVariante;
              return (
                <button
                  key={item.id}
                  type="button"
                  onClick={() => setIndiceVariante(indice)}
                  className={cn(
                    'flex h-11 items-center gap-2 rounded-pill border px-4 text-sm transition',
                    activa
                      ? 'border-accent bg-accent/15 text-accent-ink'
                      : 'border-hairline bg-surface/60 text-muted hover:text-ink',
                  )}
                >
                  {variantes.length > 1 && <kbd className="num text-[11px] text-muted">{indice + 1}</kbd>}
                  <span className="text-ink">{item.nombre}</span>
                  <span className="num text-xs text-accent-ink">{formatUSD(item.precioVentaUSD)}</span>
                </button>
              );
            })}
          </div>
        </section>

        <section className="flex flex-col gap-2">
          <div className="flex items-center justify-between">
            <p className="text-xs font-medium uppercase tracking-tighter2 text-muted">Extras</p>
            {requeridosMinimos > 0 && (
              <span className={cn('text-[11px]', cumpleRequeridos ? 'text-muted' : 'text-danger-ink')}>
                Elige al menos {requeridosMinimos}
              </span>
            )}
          </div>
          {modificadoresQuery.isLoading ? (
            <Spinner />
          ) : modificadores.length === 0 ? (
            <p className="text-sm text-muted">Sin extras para este producto.</p>
          ) : (
            <div className="flex flex-wrap gap-2">
              {modificadores.map((mod) => {
                const activo = seleccionados.has(mod.id);
                return (
                  <button
                    key={mod.id}
                    type="button"
                    onClick={() => alternarModificador(mod.id)}
                    aria-pressed={activo}
                    className={cn(
                      'flex h-11 items-center gap-2 rounded-pill border px-4 text-sm transition',
                      activo
                        ? 'border-accent bg-accent/15 text-accent-ink'
                        : 'border-hairline bg-surface/60 text-muted hover:text-ink',
                    )}
                  >
                    <span className="text-ink">{mod.modificadorNombre}</span>
                    {mod.precioAdicional > 0 && (
                      <span className="num text-xs text-accent-ink">+{formatUSD(mod.precioAdicional)}</span>
                    )}
                  </button>
                );
              })}
            </div>
          )}
        </section>

        {producto.tipo === 'Preparado' && (recetasQuery.data?.length ?? 0) > 0 && (
          <section className="rounded-inner border border-hairline bg-elevated/40 p-3">
            <p className="mb-2 text-xs font-medium uppercase tracking-tighter2 text-muted">Receta</p>
            <ul className="flex flex-col gap-1">
              {recetasQuery.data?.map((receta) => (
                <li key={receta.id} className="flex items-center justify-between text-sm text-muted">
                  <span>{receta.varianteInsumoNombre}</span>
                  <span className="num text-ink">{receta.cantidad}</span>
                </li>
              ))}
            </ul>
          </section>
        )}

        <label className="flex items-center justify-between border-t border-hairline pt-4">
          <span className="text-sm text-muted">Cantidad</span>
          <input
            aria-label="Cantidad"
            type="number"
            min={1}
            value={cantidad}
            onChange={(evento) => setCantidad(Math.max(1, Number(evento.target.value) || 1))}
            className="num h-10 w-20 rounded-control border border-hairline bg-surface px-3 text-center text-base text-ink focus:outline-none focus:ring-2 focus:ring-accent/50 [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
          />
        </label>
      </div>
    </Modal>
  );
}
