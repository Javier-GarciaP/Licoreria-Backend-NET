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
  permisos: string[];
  accessToken: string;
  refreshToken: string;
  expiraEn: string;
}

export interface UsuarioActual {
  usuarioId: string;
  nombreCompleto: string;
  email: string;
  rol: string;
  permisos: string[];
}

/* ===================== Catálogo ===================== */

export type TipoProducto = 'Simple' | 'Preparado';

export interface PrecioVariante {
  id: string;
  listaPrecioId: string;
  listaPrecioNombre: string;
  moneda: Moneda;
  precio: number;
}

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
  precios: PrecioVariante[];
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
export type EstadoItemComanda = 'Recibido' | 'Preparado' | 'Entregado' | 'Cancelado';
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
}

/* ===================== Club ===================== */

export type TipoZona = 'Barra' | 'Mesas' | 'Juegos' | 'Pista' | 'Vip';

export interface Zona {
  id: string;
  nombre: string;
  tipo: TipoZona;
  activo: boolean;
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
  tipo: string;
  etiqueta: string | null;
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
