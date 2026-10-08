/* DTOs compartidos (espejo del backend .NET). Los enums viajan como texto. */

export type Moneda = 'USD' | 'BS';
export type RolSeguridad = 'Admin' | 'Employee';

export interface ResultadoPaginado<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  errors?: Record<string, string[]>;
}

/* ===================== Auth ===================== */

export interface LoginRequest {
  username: string;
  password: string;
}

export interface AuthResponse {
  usuarioId: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  rolDominio: string;
  permisos: string[];
  accessToken: string;
  refreshToken: string;
  expiraEn: string;
}

export interface UsuarioActual {
  id: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  rolDominio: string;
  permisos: string[];
}

/* ===================== Catálogo ===================== */

export type TipoProducto = 'Simple' | 'Preparado';

export interface ProductoVariante {
  id: string;
  nombre: string;
  sku: string;
  precioCompraUSD: number;
  precioVentaUSD: number;
  unidadMedidaId: string;
  unidadMedidaNombre: string;
  activo: boolean;
  codigosBarras: string[];
}

export interface Producto {
  id: string;
  nombre: string;
  descripcion: string | null;
  categoriaId: string;
  categoriaNombre: string;
  marcaId: string | null;
  marcaNombre: string | null;
  impuestoId: string | null;
  tipo: TipoProducto;
  gradoAlcoholico: number | null;
  imagenUrl: string | null;
  activo: boolean;
  variantes: ProductoVariante[];
}

export interface Categoria {
  id: string;
  nombre: string;
  descripcion: string | null;
  categoriaPadreId: string | null;
  activo: boolean;
}

export interface Marca {
  id: string;
  nombre: string;
  descripcion: string | null;
  activo: boolean;
}

export interface UnidadMedida {
  id: string;
  nombre: string;
  abreviatura: string;
}

export interface StockItem {
  varianteId: string;
  sku: string;
  productoNombre: string;
  varianteNombre: string;
  cantidad: number;
  cantidadReservada: number;
  stockMinimo: number;
  stockMaximo: number;
  bajoMinimo: boolean;
}

export type TipoMovimientoInventario =
  | 'Compra'
  | 'Venta'
  | 'Ajuste'
  | 'Merma'
  | 'Cortesia'
  | 'ConsumoInterno';

export interface MovimientoKardex {
  id: string;
  varianteId: string;
  sku: string;
  tipo: TipoMovimientoInventario;
  cantidad: number;
  costoUnitario: number | null;
  referenciaTipo: string | null;
  referenciaId: string | null;
  motivo: string | null;
  fecha: string;
}

export interface AjusteInventario {
  varianteId: string;
  cantidad: number;
  motivo: string;
}

export interface Lote {
  id: string;
  varianteId: string;
  sku: string;
  codigo: string;
  fechaVencimiento: string | null;
  cantidad: number;
  activo: boolean;
}

export interface TomaFisicaDetalle {
  varianteId: string;
  sku: string;
  cantidadSistema: number;
  cantidadContada: number;
  diferencia: number;
}

export interface TomaFisica {
  id: string;
  fecha: string;
  estado: string;
  observaciones: string | null;
  detalles: TomaFisicaDetalle[];
}

export interface Merma {
  id: string;
  movimientoId: string;
  varianteId: string;
  sku: string;
  cantidad: number;
  motivo: string;
  repuesto: boolean;
  fecha: string;
}

/* ===================== Compras ===================== */

export interface Proveedor {
  id: string;
  nombre: string;
  rif: string | null;
  contacto: string | null;
  telefono: string | null;
  email: string | null;
  direccion: string | null;
  diasCredito: number;
  activo: boolean;
}

export type EstadoOrdenCompra =
  | 'Borrador'
  | 'Aprobada'
  | 'Enviada'
  | 'RecibidaParcial'
  | 'Recibida'
  | 'Cancelada';

export interface OrdenCompraDetalle {
  id: string;
  varianteId: string;
  sku: string;
  nombre: string;
  cantidad: number;
  costoUnitarioUSD: number;
  cantidadRecibida: number;
  subtotalUSD: number;
}

export interface OrdenCompra {
  id: string;
  numero: string;
  proveedorId: string;
  proveedorNombre: string;
  fecha: string;
  estado: EstadoOrdenCompra;
  observaciones: string | null;
  totalUSD: number;
  detalles: OrdenCompraDetalle[];
}

export interface RecepcionDetalle {
  id: string;
  ordenCompraDetalleId: string;
  varianteId: string;
  sku: string;
  cantidad: number;
  costoUnitarioUSD: number;
}

export interface Recepcion {
  id: string;
  ordenCompraId: string;
  numeroOrden: string;
  fecha: string;
  totalUSD: number;
  observaciones: string | null;
  detalles: RecepcionDetalle[];
}

export interface CuentaPorPagar {
  id: string;
  proveedorId: string;
  proveedorNombre: string;
  ordenCompraId: string | null;
  montoUSD: number;
  saldoUSD: number;
  vencimiento: string;
  estado: string;
}

export interface PagoProveedor {
  id: string;
  cuentaPorPagarId: string;
  monto: number;
  moneda: Moneda;
  referencia: string | null;
  fecha: string;
}

/* ===================== Contenido ===================== */

export interface Bloque {
  id: string;
  tipo: string;
  orden: number;
  contenido: string;
  activo: boolean;
}

export interface Seccion {
  id: string;
  titulo: string;
  tipo: string;
  orden: number;
  activa: boolean;
  bloques: Bloque[];
}

export interface Pagina {
  id: string;
  titulo: string;
  slug: string;
  publicada: boolean;
  activo: boolean;
  secciones: Seccion[];
}

export interface Horario {
  id: string;
  diaSemana: number;
  abierto: boolean;
  horaApertura: string | null;
  horaCierre: string | null;
}

export interface LocalInfo {
  id: string;
  nombre: string;
  descripcion: string;
  direccion: string;
  telefono: string;
  whatsapp: string;
  email: string;
  instagram: string | null;
  facebook: string | null;
  mapaUrl: string | null;
  logoUrl: string | null;
}

export interface MediaAsset {
  id: string;
  nombre: string;
  url: string;
  tipo: string;
  tamano: number;
}

export interface QrMenu {
  url: string;
  contenido: string;
}

/* ===================== Finanzas ===================== */

/* ===================== Ventas / Pagos ===================== */

export interface MetodoPago {
  id: string;
  codigo: string;
  nombre: string;
}

export interface VentaItem {
  varianteId: string;
  cantidad: number;
  precioUnitarioUSD?: number | null;
  descuentoUSD?: number;
  esCortesia?: boolean;
}

export interface VentaPago {
  metodoPagoId: string;
  monto: number;
  moneda?: Moneda;
  propina?: number;
}

export interface RegistrarVentaRequest {
  items: VentaItem[];
  pagos: VentaPago[];
  descuentoUSD?: number;
  cuentaId?: string | null;
  promocionId?: string | null;
}

export interface VentaDetalle {
  id: string;
  varianteId: string;
  sku: string;
  nombre: string;
  cantidad: number;
  precioUnitarioUSD: number;
  descuentoUSD: number;
  esCortesia: boolean;
  subtotalUSD: number;
}

export interface VentaPagoRegistrado {
  id: string;
  metodoPagoId: string;
  metodoPago: string;
  monto: number;
  moneda: Moneda;
  propina: number;
}

export interface Venta {
  id: string;
  fecha: string;
  tasaCambio: number;
  subtotalUSD: number;
  descuentoUSD: number;
  totalUSD: number;
  totalBS: number;
  estado: string;
  usuarioId: string;
  numeroComprobante: string | null;
  detalles: VentaDetalle[];
  pagos: VentaPagoRegistrado[];
}

/* ===================== Cuentas / Comandas ===================== */

export type AreaDestino = 'Barra' | 'Cocina';
export type EstadoItemComanda = 'Recibido' | 'Preparado' | 'Entregado' | 'Cancelado' | 'EnProceso';
export type EstadoCuenta = 'Abierta' | 'PorCobrar' | 'Cerrada';

export interface ComandaDetalle {
  id: string;
  varianteId: string;
  sku: string;
  nombre: string;
  cantidad: number;
  precioUnitarioUSD: number;
  areaDestino: AreaDestino;
  estado: EstadoItemComanda;
  esCortesia: boolean;
  subtotalUSD: number;
  /** Monto ya cubierto por abonos (pago por consumo). Opcional hasta que el backend lo exponga. */
  pagadoUSD?: number;
  /** Saldo pendiente del consumo (`subtotalUSD - pagadoUSD`). */
  saldoUSD?: number;
}

/** Ítem que un abono cubre, para el pago por consumo. */
export interface AbonoItem {
  comandaDetalleId: string;
  monto: number;
}

export interface Comanda {
  id: string;
  area: AreaDestino;
  estado: string;
  fecha: string;
  detalles: ComandaDetalle[];
}

export interface Abono {
  id: string;
  metodoPagoId: string;
  metodoPago: string;
  monto: number;
  moneda: Moneda;
  fecha: string;
}

export interface CuentaDivision {
  id: string;
  indice: number;
  monto: number;
  pagada: boolean;
}

export interface Cuenta {
  id: string;
  sesionMesaId: string;
  nombreMesa: string;
  estado: EstadoCuenta;
  total: number;
  totalAbonado: number;
  saldo: number;
  abiertaEn: string;
  comandas: Comanda[];
  abonos: Abono[];
  divisiones: CuentaDivision[];
  abiertaPorId: string | null;
  cliente: string | null;
}

/* ===================== Club ===================== */

export type TipoZona = 'Barra' | 'Mesas' | 'Juegos' | 'Pista' | 'Vip';

export interface Zona {
  id: string;
  nombre: string;
  tipo: TipoZona;
  activo: boolean;
  color: string | null;
  posX: number;
  posY: number;
  ancho: number;
  alto: number;
}

export interface Mesa {
  id: string;
  zonaId: string;
  zonaNombre: string;
  numero: string;
  capacidad: number;
  forma: string;
  posX: number;
  posY: number;
  ancho: number;
  alto: number;
  activa: boolean;
  disponible: boolean;
  cuentaId: string | null;
  reservada: boolean;
}

export interface PlanoElemento {
  id: string;
  zonaId: string | null;
  mesaId: string | null;
  tipo: string;
  forma: string | null;
  color: string | null;
  etiqueta: string | null;
  z: number;
  posX: number;
  posY: number;
  ancho: number;
  alto: number;
  rotacion: number;
}

export interface Plano {
  id: string;
  nombre: string;
  version: number;
  activo: boolean;
  anchoFondo: number;
  altoFondo: number;
  rejilla: number;
  piso: string;
  elementos: PlanoElemento[];
}

export interface Reserva {
  id: string;
  fechaHora: string;
  personas: number;
  estado: string;
  origen: string;
  nombreContacto: string;
  telefono: string;
  notas: string | null;
  mesas: { mesaId: string; numero: string; zona: string }[];
  pagos: {
    id: string;
    metodoPagoId: string;
    metodoPago: string;
    monto: number;
    moneda: Moneda;
    comprobanteUrl: string | null;
    estado: string;
  }[];
}

/* ===================== Mermas ===================== */

export type MotivoMerma = string;

export interface MermaRegistro {
  varianteId: string;
  cantidad: number;
  motivo: string;
  reponerSinCobro?: boolean;
}

/* ===================== Reportes ===================== */

export interface Dashboard {
  ventasHoyUSD: number;
  ventasHoyCantidad: number;
  ventasMesUSD: number;
  ventasMesCantidad: number;
  ticketPromedioUSD: number;
  mermasMesCantidad: number;
  mermasMesUnidades: number;
  cuentasAbiertas: number;
  productosStockBajo: number;
  reservasProximas: number;
  cajaAbierta: boolean;
  cajaFondoInicial: number;
  clientes: number;
  productosActivos: number;
  generadoEn: string;
}

export interface HeatmapFranja {
  diaSemana: number;
  hora: number;
  totalUSD: number;
  cantidad: number;
}

export interface ReporteHeatmap {
  desde: string;
  hasta: string;
  franjas: HeatmapFranja[];
}

export interface DiagnosticoInventarioItem {
  varianteId: string;
  sku: string;
  productoNombre: string;
  varianteNombre: string;
  cantidad: number;
  stockMinimo: number;
  stockMaximo: number;
  estado: string;
  mensajeDiagnostico: string;
  unidadesCompraSugeridas: number;
}

export interface DiagnosticoInventario {
  total: number;
  sinStock: number;
  riesgoCritico: number;
  subabastecido: number;
  optimo: number;
  sobreabastecido: number;
  excesivo: number;
  items: DiagnosticoInventarioItem[];
}

export interface ComparativoMerma {
  motivo: string;
  registros: number;
  unidades: number;
  valorUSD: number;
}

export interface ReporteMermasVsVentas {
  desde: string;
  hasta: string;
  ventasUSD: number;
  mermaValorizadaUSD: number;
  porcentaje: number;
  porMotivo: ComparativoMerma[];
}

export interface TasaCambio {
  id: string;
  fecha: string;
  tipo: 'BCV' | 'Paralelo';
  valor: number;
}

export interface Usuario {
  id: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  activo: boolean;
  createdAt: string;
}

export type RolUsuario =
  | 'Administrador'
  | 'Cajero'
  | 'Mesero'
  | 'Barra'
  | 'Cocina'
  | 'Host'
  | 'EditorContenido';

export interface Cliente {
  id: string;
  nombre: string;
  rif: string | null;
  ci: string | null;
  email: string | null;
  telefono: string | null;
  direccion: string | null;
  puntos: number;
  activo: boolean;
}

export interface Denominacion {
  id: string;
  moneda: Moneda;
  tipo: 'Billete' | 'Moneda' | string;
  valor: number;
}

export interface ChatMensaje {
  id: string;
  autorId: string;
  autorNombre: string;
  rol: string;
  mensaje: string;
  creadoEn: string;
}

export interface MovimientoCaja {
  id: string;
  tipo: 'Ingreso' | 'Egreso' | string;
  monto: number;
  moneda: Moneda;
  motivo: string;
  fecha: string;
}

export interface ArqueoLinea {
  denominacionId: string;
  moneda: Moneda;
  valor: number;
  cantidad: number;
  subtotal: number;
}

export interface SesionCaja {
  id: string;
  estado: 'Abierta' | 'Cerrada' | string;
  fondoInicial: number;
  montoEsperado: number;
  montoContado: number;
  descuadre: number;
  abiertaEn: string;
  cerradaEn: string | null;
  movimientos: MovimientoCaja[];
  arqueo: ArqueoLinea[];
}

/* ===================== Web pública ===================== */

export interface MenuItem {
  varianteId: string;
  nombre: string;
  sku: string;
  precioUSD: number;
  precioBS: number;
}

export interface MenuSeccion {
  categoriaId: string;
  nombre: string;
  items: MenuItem[];
}

export interface MenuDigital {
  nombre: string;
  tasa: number;
  generadoEn: string;
  secciones: MenuSeccion[];
}

export interface Evento {
  id: string;
  titulo: string;
  descripcion: string;
  fechaInicio: string;
  fechaFin: string | null;
  imagenUrl: string | null;
  publicado: boolean;
  activo: boolean;
}

export interface Promocion {
  id: string;
  nombre: string;
  tipo: 'Porcentaje' | 'Monto' | string;
  valor: number;
  activo: boolean;
  fechaInicio: string | null;
  fechaFin: string | null;
}

export interface PedidoAnticipado {
  id: string;
  varianteId: string;
  sku: string;
  nombre: string;
  cantidad: number;
  precioUnitarioUSD: number;
  subtotalUSD: number;
}

/* ===================== Gerencia (R0) ===================== */

export interface AuditLog {
  id: string;
  usuarioId: string | null;
  usuario: string | null;
  accion: string;
  entidad: string;
  entidadId: string | null;
  datos: string | null;
  ip: string | null;
  fecha: string;
}

export interface CuentaPorCobrar {
  id: string;
  clienteId: string;
  clienteNombre: string;
  montoUSD: number;
  saldoUSD: number;
  vencimiento: string;
  estado: string;
}

export interface ListaVip {
  id: string;
  clienteId: string | null;
  nombre: string;
  documento: string | null;
  telefono: string | null;
  notas: string | null;
  activo: boolean;
}

export interface Entrada {
  id: string;
  codigo: string;
  eventoId: string | null;
  eventoTitulo: string | null;
  reservaId: string | null;
  clienteId: string | null;
  precio: number;
  moneda: Moneda;
  estado: string;
  emitidaEn: string;
  usadaEn: string | null;
}

export interface ReporteVentas {
  desde: string;
  hasta: string;
  totalUSD: number;
  totalBS: number;
  cantidad: number;
  ticketPromedioUSD: number;
  porDia: { fecha: string; totalUSD: number; cantidad: number }[];
  porUsuario: { usuarioId: string; usuarioNombre: string; totalUSD: number; cantidad: number }[];
  porMetodoPago: { metodoPagoId: string; metodoPagoNombre: string; moneda: Moneda; monto: number; pagos: number }[];
}

export interface ReportePropinas {
  desde: string;
  hasta: string;
  totalPropinaUSD: number;
  porUsuario: { usuarioId: string; usuarioNombre: string; ventas: number; totalPropinaUSD: number }[];
}

export interface InventarioValorizado {
  variantes: number;
  unidadesTotales: number;
  costoTotalUSD: number;
  valorVentaTotalUSD: number;
  bajoMinimo: number;
  items: {
    varianteId: string;
    sku: string;
    productoNombre: string;
    varianteNombre: string;
    cantidad: number;
    costoUnitarioUSD: number;
    costoTotalUSD: number;
    valorVentaUSD: number;
    bajoMinimo: boolean;
  }[];
}

export interface ReporteCompras {
  desde: string;
  hasta: string;
  recepciones: number;
  totalCompradoUSD: number;
  totalPorPagarUSD: number;
  totalPagadoUSD: number;
  cuentasPendientes: number;
}

export interface Impuesto {
  id: string;
  nombre: string;
  porcentaje: number;
  activo: boolean;
}

export interface ListaPrecio {
  id: string;
  nombre: string;
  descripcion: string | null;
  esPredeterminada: boolean;
  activo: boolean;
}

export interface Modificador {
  id: string;
  nombre: string;
  precioAdicional: number;
  activo: boolean;
}

/** Modificador asignado a un producto, con sus límites de selección en el POS. */
export interface ProductoModificador {
  id: string;
  modificadorId: string;
  modificadorNombre: string;
  precioAdicional: number;
  minimo: number;
  maximo: number;
  requerido: boolean;
}

/** Insumo de la receta de un producto preparado. */
export interface Receta {
  id: string;
  varianteInsumoId: string;
  varianteInsumoNombre: string;
  sku: string;
  cantidad: number;
}
