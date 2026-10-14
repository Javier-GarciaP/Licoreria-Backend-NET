import { useCallback, useMemo, useReducer, useState } from 'react';

export interface ModificadorSeleccionado {
  productoModificadorId: string;
  modificadorId: string;
  nombre: string;
  precioAdicional: number;
}

export interface LineaOrden {
  key: string;
  varianteId: string;
  productoId: string;
  nombre: string;
  varianteNombre: string;
  sku: string;
  precioBaseUSD: number;
  modificadores: ModificadorSeleccionado[];
  cantidad: number;
}

export interface Orden {
  id: string;
  nombre: string;
  lineas: LineaOrden[];
  descuentoUSD: number;
  propinaUSD: number;
  promocionId: string;
}

let secuencia = 0;
const nuevoId = (prefijo: string) => {
  secuencia += 1;
  const azar = Math.random().toString(36).slice(2, 8);
  return `${prefijo}-${Date.now().toString(36)}-${secuencia}-${azar}`;
};

export const precioUnitario = (linea: LineaOrden) =>
  linea.precioBaseUSD + linea.modificadores.reduce((total, mod) => total + mod.precioAdicional, 0);

export const subtotalOrden = (orden: Orden) =>
  orden.lineas.reduce((total, linea) => total + precioUnitario(linea) * linea.cantidad, 0);

export const totalOrden = (orden: Orden) =>
  Math.max(0, subtotalOrden(orden) - orden.descuentoUSD);

export const totalConPropina = (orden: Orden) => totalOrden(orden) + Math.max(0, orden.propinaUSD);

export function crearOrden(nombre?: string): Orden {
  return {
    id: nuevoId('orden'),
    nombre: nombre ?? 'Venta',
    lineas: [],
    descuentoUSD: 0,
    propinaUSD: 0,
    promocionId: '',
  };
}

interface Estado {
  ordenes: Orden[];
  activaId: string;
}

type Accion =
  | { tipo: 'nueva'; nombre?: string }
  | { tipo: 'activar'; id: string }
  | { tipo: 'cerrar'; id: string }
  | { tipo: 'renombrar'; id: string; nombre: string }
  | { tipo: 'agregarLinea'; linea: Omit<LineaOrden, 'key'> }
  | { tipo: 'cantidad'; key: string; delta: number }
  | { tipo: 'fijarCantidad'; key: string; cantidad: number }
  | { tipo: 'quitarLinea'; key: string }
  | { tipo: 'vaciar' }
  | { tipo: 'descuento'; valor: number }
  | { tipo: 'propina'; valor: number }
  | { tipo: 'promocion'; id: string }
  | { tipo: 'reiniciar' };

const actualizarActiva = (estado: Estado, cambio: (orden: Orden) => Orden): Estado => ({
  ...estado,
  ordenes: estado.ordenes.map((orden) => (orden.id === estado.activaId ? cambio(orden) : orden)),
});

const firma = (linea: Omit<LineaOrden, 'key'>) =>
  `${linea.varianteId}|${linea.modificadores.map((m) => m.productoModificadorId).sort().join(',')}`;

function reducer(estado: Estado, accion: Accion): Estado {
  switch (accion.tipo) {
    case 'nueva': {
      const orden = crearOrden(accion.nombre);
      return { ordenes: [...estado.ordenes, orden], activaId: orden.id };
    }
    case 'activar':
      return { ...estado, activaId: accion.id };
    case 'cerrar': {
      if (estado.ordenes.length === 1) {
        const limpia = crearOrden();
        return { ordenes: [limpia], activaId: limpia.id };
      }
      const ordenes = estado.ordenes.filter((orden) => orden.id !== accion.id);
      const activaId = estado.activaId === accion.id ? ordenes[0].id : estado.activaId;
      return { ordenes, activaId };
    }
    case 'renombrar':
      return {
        ...estado,
        ordenes: estado.ordenes.map((orden) =>
          orden.id === accion.id ? { ...orden, nombre: accion.nombre } : orden,
        ),
      };
    case 'agregarLinea':
      return actualizarActiva(estado, (orden) => {
        const clave = firma(accion.linea);
        const existente = orden.lineas.find((linea) => firma(linea) === clave);
        if (existente) {
          return {
            ...orden,
            lineas: orden.lineas.map((linea) =>
              linea.key === existente.key ? { ...linea, cantidad: linea.cantidad + accion.linea.cantidad } : linea,
            ),
          };
        }
        return { ...orden, lineas: [...orden.lineas, { ...accion.linea, key: nuevoId('linea') }] };
      });
    case 'cantidad':
      return actualizarActiva(estado, (orden) => ({
        ...orden,
        lineas: orden.lineas
          .map((linea) => (linea.key === accion.key ? { ...linea, cantidad: linea.cantidad + accion.delta } : linea))
          .filter((linea) => linea.cantidad > 0),
      }));
    case 'fijarCantidad':
      return actualizarActiva(estado, (orden) => ({
        ...orden,
        lineas: orden.lineas
          .map((linea) =>
            linea.key === accion.key ? { ...linea, cantidad: Math.max(0, Math.floor(accion.cantidad)) } : linea,
          )
          .filter((linea) => linea.cantidad > 0),
      }));
    case 'quitarLinea':
      return actualizarActiva(estado, (orden) => ({
        ...orden,
        lineas: orden.lineas.filter((linea) => linea.key !== accion.key),
      }));
    case 'vaciar':
      return actualizarActiva(estado, (orden) => ({ ...orden, lineas: [], descuentoUSD: 0, propinaUSD: 0, promocionId: '' }));
    case 'descuento':
      return actualizarActiva(estado, (orden) => ({ ...orden, descuentoUSD: Math.max(0, accion.valor) }));
    case 'propina':
      return actualizarActiva(estado, (orden) => ({ ...orden, propinaUSD: Math.max(0, accion.valor) }));
    case 'promocion':
      return actualizarActiva(estado, (orden) => ({ ...orden, promocionId: accion.id }));
    case 'reiniciar': {
      const orden = crearOrden();
      const resto = estado.ordenes.filter((item) => item.id !== estado.activaId);
      return { ordenes: [...resto, orden], activaId: orden.id };
    }
    default:
      return estado;
  }
}

export function usePos() {
  const [estado, dispatch] = useReducer(reducer, undefined, () => {
    const orden = crearOrden();
    return { ordenes: [orden], activaId: orden.id };
  });

  const activa = useMemo(
    () => estado.ordenes.find((orden) => orden.id === estado.activaId) ?? estado.ordenes[0],
    [estado],
  );

  const acciones = useMemo(
    () => ({
      nueva: (nombre?: string) => dispatch({ tipo: 'nueva', nombre }),
      activar: (id: string) => dispatch({ tipo: 'activar', id }),
      cerrar: (id: string) => dispatch({ tipo: 'cerrar', id }),
      renombrar: (id: string, nombre: string) => dispatch({ tipo: 'renombrar', id, nombre }),
      agregarLinea: (linea: Omit<LineaOrden, 'key'>) => dispatch({ tipo: 'agregarLinea', linea }),
      cantidad: (key: string, delta: number) => dispatch({ tipo: 'cantidad', key, delta }),
      fijarCantidad: (key: string, cantidad: number) => dispatch({ tipo: 'fijarCantidad', key, cantidad }),
      quitarLinea: (key: string) => dispatch({ tipo: 'quitarLinea', key }),
      vaciar: () => dispatch({ tipo: 'vaciar' }),
      descuento: (valor: number) => dispatch({ tipo: 'descuento', valor }),
      propina: (valor: number) => dispatch({ tipo: 'propina', valor }),
      promocion: (id: string) => dispatch({ tipo: 'promocion', id }),
      reiniciar: () => dispatch({ tipo: 'reiniciar' }),
    }),
    [],
  );

  const [lineaActiva, setLineaActiva] = useState(0);

  const indiceSeguro = Math.min(lineaActiva, Math.max(0, activa.lineas.length - 1));
  const lineaSeleccionada = activa.lineas[indiceSeguro] ?? null;

  const moverSeleccion = useCallback(
    (delta: number) => {
      setLineaActiva(Math.min(Math.max(0, indiceSeguro + delta), Math.max(0, activa.lineas.length - 1)));
    },
    [indiceSeguro, activa.lineas.length],
  );

  return {
    ordenes: estado.ordenes,
    activa,
    lineaSeleccionada,
    indiceLinea: indiceSeguro,
    setIndiceLinea: setLineaActiva,
    moverSeleccion,
    ...acciones,
  };
}

export type PosApi = ReturnType<typeof usePos>;
