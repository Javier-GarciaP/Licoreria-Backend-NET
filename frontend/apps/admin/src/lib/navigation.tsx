import type { LucideIcon } from 'lucide-react';
import {
  Activity,
  Armchair,
  BarChart3,
  Boxes,
  CalendarClock,
  CalendarDays,
  ChefHat,
  CircleUser,
  ClipboardCheck,
  ClipboardList,
  Clock,
  Coins,
  FileClock,
  FileText,
  Gauge,
  Grid2x2,
  HandCoins,
  Image as ImageIcon,
  Landmark,
  LayoutGrid,
  Map as MapIcon,
  Package,
  PackageCheck,
  PackageOpen,
  Percent,
  Receipt,
  ReceiptText,
  ScrollText,
  Settings,
  ShoppingBag,
  ShoppingCart,
  Star,
  Tags,
  Ticket,
  Trash2,
  Truck,
  Users,
  Wallet,
  Warehouse,
} from 'lucide-react';

export interface NavItem {
  to: string;
  label: string;
  icon: LucideIcon;
  /** Etiqueta corta para la barra inferior móvil. */
  corto?: string;
  permiso?: string;
  admin?: boolean;
  /** Coincidencia exacta de ruta (evita que un padre quede activo en sus hijos). */
  end?: boolean;
  /** Aparece en la barra inferior móvil. */
  primario?: boolean;
  /** Roles de dominio que ven este ítem (vacío = todos). */
  roles?: string[];
  /** Fuera del núcleo mínimo: no aparece en la nav ni es accesible. Reversible. */
  oculto?: boolean;
}

export interface NavGroup {
  id: string;
  label: string;
  icon: LucideIcon;
  /** Roles de dominio que ven este grupo (vacío = todos). */
  roles?: string[];
  /** Fuera del núcleo mínimo. Reversible. */
  oculto?: boolean;
  items: NavItem[];
}

const ADMIN = ['Administrador'];
const OP = ['Administrador', 'Cajero', 'Mesero', 'Barra', 'Cocina'];
const CAJA = ['Administrador', 'Cajero'];
const SALON = ['Administrador', 'Mesero', 'Host'];
const CONTENIDO = ['Administrador', 'EditorContenido'];

/** Fuente única de la navegación del admin. Alimenta sidebar, sheet móvil y bottom-nav. */
export const GRUPOS: NavGroup[] = [
  {
    id: 'operacion',
    label: 'Operación',
    icon: Gauge,
    roles: OP,
    items: [
      { to: '/', label: 'Dashboard', icon: BarChart3, permiso: 'sales:read', end: true, primario: true, corto: 'Inicio', roles: CAJA },
      { to: '/pos', label: 'POS', icon: ShoppingCart, permiso: 'sales:write', primario: true, roles: CAJA },
      { to: '/plano', label: 'Mesas', icon: Grid2x2, permiso: 'club:read', primario: true, roles: ['Administrador'] },
      { to: '/cuentas', label: 'Cuentas', icon: LayoutGrid, permiso: 'sales:read', roles: ['Administrador', 'Cajero', 'Mesero'] },
      { to: '/ventas', label: 'Ventas', icon: Receipt, permiso: 'sales:read', roles: CAJA },
      { to: '/promociones', label: 'Promociones', icon: Percent, permiso: 'sales:read', roles: CAJA, oculto: true },
    ],
  },
  {
    id: 'salon',
    label: 'Salón',
    icon: Armchair,
    roles: SALON,
    items: [
      { to: '/reservas', label: 'Reservas', icon: CalendarClock, permiso: 'reservation:manage', roles: ['Administrador', 'Host'] },
      { to: '/salon', label: 'Salón', icon: MapIcon, permiso: 'club:manage', end: true, roles: ['Administrador', 'Host'] },
      { to: '/entradas', label: 'Entradas', icon: Ticket, permiso: 'club:manage', roles: ['Administrador', 'Host'], oculto: true },
      { to: '/vip', label: 'Lista VIP', icon: Star, permiso: 'club:read', roles: ['Administrador', 'Host'], oculto: true },
    ],
  },
  {
    id: 'catalogo',
    label: 'Catálogo y almacén',
    icon: Boxes,
    roles: CAJA,
    items: [
      { to: '/productos', label: 'Catálogo', icon: Package, permiso: 'catalog:read' },
      { to: '/inventario', label: 'Existencias', icon: Warehouse, permiso: 'inventory:read', end: true },
      { to: '/inventario/kardex', label: 'Kardex', icon: ScrollText, permiso: 'inventory:read' },
      { to: '/inventario/lotes', label: 'Lotes', icon: PackageOpen, permiso: 'inventory:read', oculto: true },
      { to: '/inventario/tomas', label: 'Tomas físicas', icon: ClipboardCheck, permiso: 'inventory:read', oculto: true },
      { to: '/mermas', label: 'Mermas y cortesías', icon: Trash2, permiso: 'inventory:merma', roles: ['Administrador', 'Cajero', 'Mesero'] },
    ],
  },
  {
    id: 'dinero',
    label: 'Dinero',
    icon: Coins,
    roles: CAJA,
    items: [
      { to: '/caja', label: 'Caja', icon: Wallet, permiso: 'cash:movement' },
      { to: '/finanzas', label: 'Tasas de cambio', icon: Coins, permiso: 'finance:read', end: true },
    ],
  },
  {
    id: 'compras',
    label: 'Compras',
    icon: ShoppingBag,
    roles: ADMIN,
    items: [
      { to: '/compras', label: 'Órdenes', icon: ClipboardList, permiso: 'purchasing:read', end: true },
      { to: '/compras/proveedores', label: 'Proveedores', icon: Truck, permiso: 'purchasing:read' },
      { to: '/compras/recepciones', label: 'Recepciones', icon: PackageCheck, permiso: 'purchasing:read' },
      { to: '/compras/cuentas', label: 'Cuentas por pagar', icon: ReceiptText, permiso: 'purchasing:read' },
    ],
  },
  {
    id: 'clientes',
    label: 'Clientes',
    icon: Users,
    roles: CAJA,
    oculto: true,
    items: [
      { to: '/clientes', label: 'Clientes', icon: Users, permiso: 'crm:read' },
      { to: '/cxc', label: 'Cuentas por cobrar', icon: HandCoins, permiso: 'crm:read' },
    ],
  },
  {
    id: 'contenido',
    label: 'Marca y contenido',
    icon: FileText,
    roles: CONTENIDO,
    oculto: true,
    items: [
      { to: '/contenido', label: 'Páginas', icon: FileText, permiso: 'content:read', end: true },
      { to: '/contenido/eventos', label: 'Eventos', icon: CalendarDays, permiso: 'content:read' },
      { to: '/contenido/local', label: 'Horarios y local', icon: Clock, permiso: 'content:read' },
      { to: '/contenido/menu', label: 'Menú y archivos', icon: ImageIcon, permiso: 'content:read' },
    ],
  },
  {
    id: 'analitica',
    label: 'Analítica',
    icon: Activity,
    roles: ADMIN,
    oculto: true,
    items: [
      { to: '/reportes', label: 'Reportes', icon: BarChart3, oculto: true },
      { to: '/auditoria', label: 'Auditoría', icon: FileClock, admin: true, oculto: true },
    ],
  },
  {
    id: 'sistema',
    label: 'Sistema',
    icon: Settings,
    roles: ADMIN,
    items: [{ to: '/usuarios', label: 'Usuarios', icon: CircleUser, admin: true }],
  },
];

function visiblePorRol(roles: string[] | undefined, esAdmin: boolean, rolDominio?: string): boolean {
  if (esAdmin || !roles || roles.length === 0) return true;
  return Boolean(rolDominio && roles.includes(rolDominio));
}

/**
 * Muestra cada grupo/ítem según RBAC y rol de dominio; excluye lo marcado como
 * fuera del núcleo mínimo y los grupos vacíos. `esAdmin` omite el filtro de rol.
 */
export function filtrarGrupos(
  esAdmin: boolean,
  tienePermiso: (clave: string) => boolean,
  rolDominio?: string,
): NavGroup[] {
  return GRUPOS.filter((grupo) => !grupo.oculto && visiblePorRol(grupo.roles, esAdmin, rolDominio))
    .map((grupo) => ({
      ...grupo,
      items: grupo.items.filter((item) => {
        if (item.oculto) return false;
        if (!visiblePorRol(item.roles, esAdmin, rolDominio)) return false;
        if (item.admin) return esAdmin;
        return !item.permiso || esAdmin || tienePermiso(item.permiso);
      }),
    }))
    .filter((grupo) => grupo.items.length > 0);
}

/** ¿La ruta actual corresponde a este ítem? */
export function esRutaActiva(pathname: string, item: NavItem): boolean {
  if (item.to === '/' || item.end) return pathname === item.to;
  return pathname === item.to || pathname.startsWith(`${item.to}/`);
}

/** Ítem de navegación que corresponde a una ruta (exacto y luego por prefijo). */
export function itemDeRuta(pathname: string): NavItem | undefined {
  const items = GRUPOS.flatMap((grupo) => grupo.items);
  const exacto = items.find((item) => item.to === pathname);
  if (exacto) return exacto;
  return items.find((item) => item.to !== '/' && pathname.startsWith(`${item.to}/`));
}

/** Grupo de navegación que contiene un ítem. */
export function grupoDeItem(item?: NavItem): NavGroup | undefined {
  if (!item) return undefined;
  return GRUPOS.find((grupo) => grupo.items.some((i) => i.to === item.to));
}

/** ¿El usuario (rol + permisos) puede acceder a la ruta de un ítem? */
export function puedeAcceder(
  item: NavItem | undefined,
  esAdmin: boolean,
  tienePermiso: (clave: string) => boolean,
  rolDominio?: string,
): boolean {
  if (!item) return true; // ruta sin ítem de nav (p. ej. tabs): se permite
  if (esAdmin) return true;
  const grupo = grupoDeItem(item);
  if (grupo?.oculto || item.oculto) return false;
  if (!visiblePorRol(grupo?.roles, esAdmin, rolDominio)) return false;
  if (!visiblePorRol(item.roles, esAdmin, rolDominio)) return false;
  if (item.admin) return esAdmin;
  return !item.permiso || tienePermiso(item.permiso);
}
