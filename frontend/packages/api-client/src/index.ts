import type {
  AuthResponse,
  Categoria,
  Cliente,
  Cuenta,
  Denominacion,
  DiagnosticoInventario,
  Dashboard,
  LoginRequest,
  Marca,
  MenuDigital,
  Mesa,
  MetodoPago,
  PedidoAnticipado,
  Plano,
  Producto,
  Promocion,
  ReporteHeatmap,
  ReporteMermasVsVentas,
  RegistrarVentaRequest,
  Reserva,
  ResultadoPaginado,
  SesionCaja,
  StockItem,
  TasaCambio,
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
  stock: (query: PaginaQuery = {}) =>
    apiFetch<ResultadoPaginado<StockItem>>('/api/v1/stock', { query: { pageSize: 100, ...query } }),
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
  mermas: (query: { desde?: string; hasta?: string } = {}) =>
    apiFetch<unknown>('/api/v1/mermas', { query }),
  registrarMerma: (body: unknown) => apiFetch<unknown>('/api/v1/mermas', { method: 'POST', body }),
  ajustar: (body: unknown) => apiFetch<void>('/api/v1/ajustes-inventario', { method: 'POST', body }),
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
