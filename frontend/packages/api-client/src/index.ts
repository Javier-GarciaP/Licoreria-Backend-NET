import type {
  AuthResponse,
  Categoria,
  Cliente,
  Cuenta,
  CuentaPorPagar,
  Denominacion,
  DiagnosticoInventario,
  Dashboard,
  Horario,
  LoginRequest,
  LocalInfo,
  Lote,
  Marca,
  MediaAsset,
  MenuDigital,
  Mesa,
  MetodoPago,
  Merma,
  MovimientoKardex,
  OrdenCompra,
  Pagina,
  PedidoAnticipado,
  Plano,
  Producto,
  Promocion,
  Proveedor,
  QrMenu,
  Recepcion,
  ReporteHeatmap,
  ReporteMermasVsVentas,
  RegistrarVentaRequest,
  Reserva,
  ResultadoPaginado,
  SesionCaja,
  StockItem,
  TasaCambio,
  TomaFisica,
  UnidadMedida,
  Usuario,
  UsuarioActual,
  Venta,
  Evento,
  Zona,
} from '@licoreria/types';
import { apiFetch, ApiError } from './http';

export { apiFetch, configureApi, ApiError } from './http';
export type { ApiConfig, RequestOptions } from './http';

export interface PaginaQuery {
  page?: number;
  pageSize?: number;
}

export const authApi = {
  login: (body: LoginRequest) => apiFetch<AuthResponse>('/api/auth/login', { method: 'POST', body }),
  logout: (refreshToken: string) =>
    apiFetch<void>('/api/auth/logout', { method: 'POST', body: { refreshToken } }),
  refresh: (refreshToken: string) =>
    apiFetch<AuthResponse>('/api/auth/refresh', { method: 'POST', body: { refreshToken } }),
  me: () => apiFetch<UsuarioActual>('/api/auth/me'),
};

export const catalogoApi = {
  productos: (query: PaginaQuery & { busqueda?: string; categoriaId?: string; activo?: boolean } = {}) =>
    apiFetch<ResultadoPaginado<Producto>>('/api/v1/productos', { query }),
  producto: (id: string) => apiFetch<Producto>(`/api/v1/productos/${id}`),
  crear: (body: unknown) => apiFetch<Producto>('/api/v1/productos', { method: 'POST', body }),
  actualizar: (id: string, body: unknown) =>
    apiFetch<Producto>(`/api/v1/productos/${id}`, { method: 'PUT', body }),
  eliminar: (id: string) => apiFetch<void>(`/api/v1/productos/${id}`, { method: 'DELETE' }),
  categorias: () => apiFetch<Categoria[]>('/api/v1/categorias'),
  crearCategoria: (body: unknown) => apiFetch<Categoria>('/api/v1/categorias', { method: 'POST', body }),
  actualizarCategoria: (id: string, body: unknown) =>
    apiFetch<Categoria>(`/api/v1/categorias/${id}`, { method: 'PUT', body }),
  eliminarCategoria: (id: string) => apiFetch<void>(`/api/v1/categorias/${id}`, { method: 'DELETE' }),
  marcas: () => apiFetch<Marca[]>('/api/v1/marcas'),
  crearMarca: (body: unknown) => apiFetch<Marca>('/api/v1/marcas', { method: 'POST', body }),
  actualizarMarca: (id: string, body: unknown) => apiFetch<Marca>(`/api/v1/marcas/${id}`, { method: 'PUT', body }),
  eliminarMarca: (id: string) => apiFetch<void>(`/api/v1/marcas/${id}`, { method: 'DELETE' }),
  unidades: () => apiFetch<UnidadMedida[]>('/api/v1/unidades-medida'),
};

export const ventasApi = {
  listar: (query: PaginaQuery & { desde?: string; hasta?: string } = {}) =>
    apiFetch<ResultadoPaginado<Venta>>('/api/v1/ventas', { query }),
  registrar: (body: RegistrarVentaRequest) => apiFetch<Venta>('/api/v1/ventas', { method: 'POST', body }),
  metodosPago: () => apiFetch<MetodoPago[]>('/api/v1/metodos-pago'),
  devolver: (id: string, body: unknown) =>
    apiFetch<unknown>(`/api/v1/ventas/${id}/devoluciones`, { method: 'POST', body }),
};

export const promocionesApi = {
  listar: () => apiFetch<Promocion[]>('/api/v1/promociones'),
};

export const cuentasApi = {
  listar: (query: PaginaQuery & { estado?: string } = {}) =>
    apiFetch<ResultadoPaginado<Cuenta>>('/api/v1/cuentas', { query }),
  obtener: (id: string) => apiFetch<Cuenta>(`/api/v1/cuentas/${id}`),
  abrir: (body: { nombreMesa: string; mesaId?: string }) =>
    apiFetch<Cuenta>('/api/v1/cuentas', { method: 'POST', body }),
  agregarComanda: (id: string, body: unknown) =>
    apiFetch<Cuenta>(`/api/v1/cuentas/${id}/comandas`, { method: 'POST', body }),
  cambiarEstadoItem: (
    id: string,
    comandaId: string,
    detalleId: string,
    estado: string,
  ) =>
    apiFetch<Cuenta>(`/api/v1/cuentas/${id}/comandas/${comandaId}/detalles/${detalleId}/estado`, {
      method: 'PUT',
      body: { estado },
    }),
  abonar: (id: string, body: unknown) =>
    apiFetch<Cuenta>(`/api/v1/cuentas/${id}/abonos`, { method: 'POST', body }),
  dividir: (id: string, body: { partes?: number; montos?: number[] }) =>
    apiFetch<unknown>(`/api/v1/cuentas/${id}/dividir`, { method: 'POST', body }),
  cerrar: (id: string, body: unknown) =>
    apiFetch<Venta>(`/api/v1/cuentas/${id}/cerrar`, { method: 'POST', body }),
};

export const clubApi = {
  zonas: () => apiFetch<Zona[]>('/api/v1/zonas'),
  mesas: (zonaId?: string) => apiFetch<Mesa[]>('/api/v1/mesas', { query: { zonaId } }),
  desalojar: (mesaId: string) => apiFetch<void>(`/api/v1/mesas/${mesaId}/desalojar`, { method: 'POST' }),
  planos: () => apiFetch<Plano[]>('/api/v1/planos'),
  plano: (id: string) => apiFetch<Plano>(`/api/v1/planos/${id}`),
  reservas: (query: PaginaQuery & { estado?: string } = {}) =>
    apiFetch<ResultadoPaginado<Reserva>>('/api/v1/reservas', { query }),
  reserva: (id: string) => apiFetch<Reserva>(`/api/v1/reservas/${id}`),
  crearReserva: (body: unknown) => apiFetch<Reserva>('/api/v1/reservas', { method: 'POST', body }),
  cambiarEstadoReserva: (id: string, estado: string) =>
    apiFetch<Reserva>(`/api/v1/reservas/${id}/estado`, { method: 'PUT', body: { estado } }),
  registrarPagoReserva: (id: string, body: unknown) =>
    apiFetch<Reserva>(`/api/v1/reservas/${id}/pagos`, { method: 'POST', body }),
  validarPagoReserva: (id: string, pagoId: string, aprobar: boolean) =>
    apiFetch<Reserva>(`/api/v1/reservas/${id}/pagos/${pagoId}/validar`, { method: 'POST', body: { aprobar } }),
  pedidos: (id: string) => apiFetch<PedidoAnticipado[]>(`/api/v1/reservas/${id}/pedidos`),
  agregarPedido: (id: string, body: unknown) =>
    apiFetch<PedidoAnticipado>(`/api/v1/reservas/${id}/pedidos`, { method: 'POST', body }),
  eliminarPedido: (id: string, pedidoId: string) =>
    apiFetch<void>(`/api/v1/reservas/${id}/pedidos/${pedidoId}`, { method: 'DELETE' }),
};

export const inventarioApi = {
  stock: (query: PaginaQuery & { soloBajoMinimo?: boolean; busqueda?: string } = {}) =>
    apiFetch<ResultadoPaginado<StockItem>>('/api/v1/stock', { query: { pageSize: 100, ...query } }),
  kardex: (query: PaginaQuery & { varianteId?: string; tipo?: string; desde?: string; hasta?: string } = {}) =>
    apiFetch<ResultadoPaginado<MovimientoKardex>>('/api/v1/movimientos-inventario', { query }),
  ajustar: (body: unknown) => apiFetch<MovimientoKardex>('/api/v1/ajustes-inventario', { method: 'POST', body }),
  lotes: (varianteId?: string) => apiFetch<Lote[]>('/api/v1/lotes', { query: { varianteId } }),
  crearLote: (body: unknown) => apiFetch<Lote>('/api/v1/lotes', { method: 'POST', body }),
  editarLote: (id: string, body: unknown) => apiFetch<Lote>(`/api/v1/lotes/${id}`, { method: 'PUT', body }),
  eliminarLote: (id: string) => apiFetch<void>(`/api/v1/lotes/${id}`, { method: 'DELETE' }),
  tomas: (query: PaginaQuery = {}) =>
    apiFetch<ResultadoPaginado<TomaFisica>>('/api/v1/tomas-fisicas', { query }),
  toma: (id: string) => apiFetch<TomaFisica>(`/api/v1/tomas-fisicas/${id}`),
  registrarToma: (body: unknown) => apiFetch<TomaFisica>('/api/v1/tomas-fisicas', { method: 'POST', body }),
  mermas: (query: PaginaQuery & { desde?: string; hasta?: string } = {}) =>
    apiFetch<ResultadoPaginado<Merma>>('/api/v1/mermas', { query }),
  registrarMerma: (body: unknown) => apiFetch<Merma>('/api/v1/mermas', { method: 'POST', body }),
};

export const finanzasApi = {
  tasaActual: (tipo: 'BCV' | 'Paralelo' = 'Paralelo') =>
    apiFetch<TasaCambio>('/api/v1/tasas-cambio/actual', { query: { tipo } }),
};

export const reportesApi = {
  dashboard: () => apiFetch<Dashboard>('/api/v1/reportes/dashboard'),
  heatmap: (desde?: string, hasta?: string) =>
    apiFetch<ReporteHeatmap>('/api/v1/reportes/heatmap', { query: { desde, hasta } }),
  inventarioSalud: () => apiFetch<DiagnosticoInventario>('/api/v1/reportes/inventario-salud'),
  mermasVsVentas: (desde?: string, hasta?: string) =>
    apiFetch<ReporteMermasVsVentas>('/api/v1/reportes/mermas-vs-ventas', { query: { desde, hasta } }),
};

export const usuariosApi = {
  listar: (query: PaginaQuery & { busqueda?: string } = {}) =>
    apiFetch<ResultadoPaginado<Usuario>>('/api/v1/usuarios', { query }),
  crear: (body: unknown) => apiFetch<Usuario>('/api/v1/usuarios', { method: 'POST', body }),
  actualizar: (id: string, body: unknown) => apiFetch<Usuario>(`/api/v1/usuarios/${id}`, { method: 'PUT', body }),
  cambiarPassword: (id: string, passwordNueva: string) =>
    apiFetch<void>(`/api/v1/usuarios/${id}/password`, { method: 'POST', body: { passwordNueva } }),
  revocarSesiones: (id: string) => apiFetch<void>(`/api/v1/usuarios/${id}/revocar-sesiones`, { method: 'POST' }),
  eliminar: (id: string) => apiFetch<void>(`/api/v1/usuarios/${id}`, { method: 'DELETE' }),
  roles: () => apiFetch<{ nombre: string; descripcion: string; permisos: string[] }[]>('/api/v1/roles'),
};

export const clientesApi = {
  listar: (query: PaginaQuery & { busqueda?: string } = {}) =>
    apiFetch<ResultadoPaginado<Cliente>>('/api/v1/clientes', { query }),
  crear: (body: unknown) => apiFetch<Cliente>('/api/v1/clientes', { method: 'POST', body }),
  actualizar: (id: string, body: unknown) => apiFetch<Cliente>(`/api/v1/clientes/${id}`, { method: 'PUT', body }),
  eliminar: (id: string) => apiFetch<void>(`/api/v1/clientes/${id}`, { method: 'DELETE' }),
  acumularPuntos: (id: string, body: { puntos: number; motivo: string }) =>
    apiFetch<Cliente>(`/api/v1/clientes/${id}/puntos/acumular`, { method: 'POST', body }),
  canjearPuntos: (id: string, body: { puntos: number; motivo: string }) =>
    apiFetch<Cliente>(`/api/v1/clientes/${id}/puntos/canjear`, { method: 'POST', body }),
};

export const cajaApi = {
  sesiones: (query: PaginaQuery = {}) =>
    apiFetch<ResultadoPaginado<SesionCaja>>('/api/v1/sesiones-caja', { query }),
  activa: async (): Promise<SesionCaja | null> => {
    try {
      return await apiFetch<SesionCaja>('/api/v1/sesiones-caja/activa');
    } catch (error) {
      if (error instanceof ApiError && error.status === 404) return null;
      throw error;
    }
  },
  abrir: (fondoInicial: number) =>
    apiFetch<SesionCaja>('/api/v1/sesiones-caja', { method: 'POST', body: { fondoInicial } }),
  movimiento: (id: string, body: unknown) =>
    apiFetch<SesionCaja>(`/api/v1/sesiones-caja/${id}/movimientos`, { method: 'POST', body }),
  cerrar: (id: string, arqueo: { denominacionId: string; cantidad: number }[]) =>
    apiFetch<SesionCaja>(`/api/v1/sesiones-caja/${id}/cerrar`, { method: 'POST', body: { arqueo } }),
  denominaciones: () => apiFetch<Denominacion[]>('/api/v1/denominaciones'),
};

/** Endpoints públicos (sin token) para la web de clientes. */
export const publicApi = {
  menuDigital: () => apiFetch<MenuDigital>('/api/v1/menu-digital'),
  eventos: () => apiFetch<Evento[]>('/api/v1/eventos'),
  localInfo: () => apiFetch<Record<string, unknown>>('/api/v1/local-info'),
  horarios: () => apiFetch<Record<string, unknown>>('/api/v1/horarios'),
  tasaActual: (tipo: 'BCV' | 'Paralelo' = 'Paralelo') =>
    apiFetch<TasaCambio>('/api/v1/tasas-cambio/actual', { query: { tipo } }),
};

/* ===================== Compras ===================== */

export const proveedoresApi = {
  listar: (busqueda?: string) => apiFetch<Proveedor[]>('/api/v1/proveedores', { query: { busqueda } }),
  crear: (body: unknown) => apiFetch<Proveedor>('/api/v1/proveedores', { method: 'POST', body }),
  actualizar: (id: string, body: unknown) => apiFetch<Proveedor>(`/api/v1/proveedores/${id}`, { method: 'PUT', body }),
  eliminar: (id: string) => apiFetch<void>(`/api/v1/proveedores/${id}`, { method: 'DELETE' }),
};

export const comprasApi = {
  ordenes: (query: PaginaQuery & { estado?: string; proveedorId?: string } = {}) =>
    apiFetch<ResultadoPaginado<OrdenCompra>>('/api/v1/ordenes-compra', { query }),
  orden: (id: string) => apiFetch<OrdenCompra>(`/api/v1/ordenes-compra/${id}`),
  crear: (body: unknown) => apiFetch<OrdenCompra>('/api/v1/ordenes-compra', { method: 'POST', body }),
  aprobar: (id: string) => apiFetch<OrdenCompra>(`/api/v1/ordenes-compra/${id}/aprobar`, { method: 'POST' }),
  enviar: (id: string) => apiFetch<OrdenCompra>(`/api/v1/ordenes-compra/${id}/enviar`, { method: 'POST' }),
  cancelar: (id: string) => apiFetch<OrdenCompra>(`/api/v1/ordenes-compra/${id}/cancelar`, { method: 'POST' }),
  recepciones: (query: PaginaQuery & { ordenCompraId?: string } = {}) =>
    apiFetch<ResultadoPaginado<Recepcion>>('/api/v1/recepciones', { query }),
  recepcion: (id: string) => apiFetch<Recepcion>(`/api/v1/recepciones/${id}`),
  registrarRecepcion: (body: unknown) => apiFetch<Recepcion>('/api/v1/recepciones', { method: 'POST', body }),
  cuentasPorPagar: (query: PaginaQuery & { proveedorId?: string; soloPendientes?: boolean } = {}) =>
    apiFetch<ResultadoPaginado<CuentaPorPagar>>('/api/v1/cuentas-por-pagar', { query }),
  registrarPago: (id: string, body: unknown) =>
    apiFetch<CuentaPorPagar>(`/api/v1/cuentas-por-pagar/${id}/pagos`, { method: 'POST', body }),
};

/* ===================== Contenido ===================== */

export const contenidoApi = {
  paginas: () => apiFetch<Pagina[]>('/api/v1/paginas/todos'),
  pagina: (id: string) => apiFetch<Pagina>(`/api/v1/paginas/${id}`),
  crearPagina: (body: unknown) => apiFetch<Pagina>('/api/v1/paginas', { method: 'POST', body }),
  actualizarPagina: (id: string, body: unknown) => apiFetch<Pagina>(`/api/v1/paginas/${id}`, { method: 'PUT', body }),
  eliminarPagina: (id: string) => apiFetch<void>(`/api/v1/paginas/${id}`, { method: 'DELETE' }),

  eventos: () => apiFetch<Evento[]>('/api/v1/eventos/todos'),
  crearEvento: (body: unknown) => apiFetch<Evento>('/api/v1/eventos', { method: 'POST', body }),
  actualizarEvento: (id: string, body: unknown) => apiFetch<Evento>(`/api/v1/eventos/${id}`, { method: 'PUT', body }),
  eliminarEvento: (id: string) => apiFetch<void>(`/api/v1/eventos/${id}`, { method: 'DELETE' }),

  horarios: () => apiFetch<Horario[]>('/api/v1/horarios'),
  guardarHorario: (body: unknown) => apiFetch<Horario>('/api/v1/horarios', { method: 'PUT', body }),

  localInfo: () => apiFetch<LocalInfo>('/api/v1/local-info'),
  actualizarLocalInfo: (body: unknown) => apiFetch<LocalInfo>('/api/v1/local-info', { method: 'PUT', body }),

  media: () => apiFetch<MediaAsset[]>('/api/v1/archivos'),
  subirArchivo: (file: File, carpeta = 'media') => {
    const formData = new FormData();
    formData.append('archivo', file);
    return apiFetch<MediaAsset>('/api/v1/archivos', { method: 'POST', formData, query: { carpeta } });
  },
  eliminarMedia: (id: string) => apiFetch<void>(`/api/v1/archivos/${id}`, { method: 'DELETE' }),

  menuDigital: () => apiFetch<MenuDigital>('/api/v1/menu-digital'),
  qrMenu: (baseUrl?: string) => apiFetch<QrMenu>('/api/v1/menu-digital/qr', { query: { baseUrl } }),
};
