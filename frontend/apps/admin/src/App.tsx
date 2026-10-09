import { lazy, Suspense } from 'react';
import { Navigate, Outlet, Route, Routes, useLocation } from 'react-router-dom';
import { Spinner } from '@licoreria/ui';
import { AppShell } from './components/AppShell';
import { ProtectedRoute } from './components/Rbac';
import { useAuth } from './context/AuthContext';
import { itemDeRuta, puedeAcceder, filtrarGrupos } from './lib/navigation';
import { LoginPage } from './pages/LoginPage';

const DashboardPage = lazy(() => import('./pages/DashboardPage').then((m) => ({ default: m.DashboardPage })));
const PosPage = lazy(() => import('./pages/PosPage').then((m) => ({ default: m.PosPage })));
const MesasPage = lazy(() => import('./pages/MesasPage').then((m) => ({ default: m.MesasPage })));
const CuentasPage = lazy(() => import('./pages/CuentasPage').then((m) => ({ default: m.CuentasPage })));
const CuentaPage = lazy(() => import('./pages/CuentaPage').then((m) => ({ default: m.CuentaPage })));
const VentasPage = lazy(() => import('./pages/VentasPage').then((m) => ({ default: m.VentasPage })));
const ReservasPage = lazy(() => import('./pages/ReservasPage').then((m) => ({ default: m.ReservasPage })));
const ReservaPage = lazy(() => import('./pages/ReservaPage').then((m) => ({ default: m.ReservaPage })));
const MermasPage = lazy(() => import('./pages/MermasPage').then((m) => ({ default: m.MermasPage })));
const InventarioPage = lazy(() => import('./pages/InventarioPage').then((m) => ({ default: m.InventarioPage })));
const KardexPage = lazy(() => import('./pages/KardexPage').then((m) => ({ default: m.KardexPage })));
const LotesPage = lazy(() => import('./pages/LotesPage').then((m) => ({ default: m.LotesPage })));
const TomasFisicasPage = lazy(() => import('./pages/TomasFisicasPage').then((m) => ({ default: m.TomasFisicasPage })));
const ComprasPage = lazy(() => import('./pages/ComprasPage').then((m) => ({ default: m.ComprasPage })));
const ProveedoresPage = lazy(() => import('./pages/ProveedoresPage').then((m) => ({ default: m.ProveedoresPage })));
const RecepcionesPage = lazy(() => import('./pages/RecepcionesPage').then((m) => ({ default: m.RecepcionesPage })));
const CuentasPorPagarPage = lazy(() => import('./pages/CuentasPorPagarPage').then((m) => ({ default: m.CuentasPorPagarPage })));
const PaginasPage = lazy(() => import('./pages/PaginasPage').then((m) => ({ default: m.PaginasPage })));
const EventosPage = lazy(() => import('./pages/EventosPage').then((m) => ({ default: m.EventosPage })));
const LocalPage = lazy(() => import('./pages/LocalPage').then((m) => ({ default: m.LocalPage })));
const MenuMediaPage = lazy(() => import('./pages/MenuMediaPage').then((m) => ({ default: m.MenuMediaPage })));
const TasasPage = lazy(() => import('./pages/TasasPage').then((m) => ({ default: m.TasasPage })));
const PlanosPage = lazy(() => import('./pages/PlanosPage').then((m) => ({ default: m.PlanosPage })));
const SalonMesasPage = lazy(() => import('./pages/SalonMesasPage').then((m) => ({ default: m.SalonMesasPage })));
const SalonLayout = lazy(() => import('./pages/SalonLayout').then((m) => ({ default: m.SalonLayout })));
const EditorMapaPage = lazy(() => import('./pages/EditorMapaPage').then((m) => ({ default: m.EditorMapaPage })));
const ReportesPage = lazy(() => import('./pages/ReportesPage').then((m) => ({ default: m.ReportesPage })));
const AuditoriaPage = lazy(() => import('./pages/AuditoriaPage').then((m) => ({ default: m.AuditoriaPage })));
const PromocionesPage = lazy(() => import('./pages/PromocionesPage').then((m) => ({ default: m.PromocionesPage })));
const CuentasPorCobrarPage = lazy(() =>
  import('./pages/CuentasPorCobrarPage').then((m) => ({ default: m.CuentasPorCobrarPage })),
);
const EntradasPage = lazy(() => import('./pages/EntradasPage').then((m) => ({ default: m.EntradasPage })));
const ListaVipPage = lazy(() => import('./pages/ListaVipPage').then((m) => ({ default: m.ListaVipPage })));
const ProductosPage = lazy(() => import('./pages/ProductosPage').then((m) => ({ default: m.ProductosPage })));
const CatalogosPage = lazy(() => import('./pages/CatalogosPage').then((m) => ({ default: m.CatalogosPage })));
const CatalogoAvanzadoPage = lazy(() =>
  import('./pages/CatalogoAvanzadoPage').then((m) => ({ default: m.CatalogoAvanzadoPage })),
);
const ClientesPage = lazy(() => import('./pages/ClientesPage').then((m) => ({ default: m.ClientesPage })));
const CajaPage = lazy(() => import('./pages/CajaPage').then((m) => ({ default: m.CajaPage })));
const UsuariosPage = lazy(() => import('./pages/UsuariosPage').then((m) => ({ default: m.UsuariosPage })));
const NotFoundPage = lazy(() => import('./pages/NotFoundPage').then((m) => ({ default: m.NotFoundPage })));

function Cargando() {
  return (
    <div className="flex h-96 items-center justify-center">
      <Spinner />
    </div>
  );
}

/** Comprueba que el rol tenga permiso para la ruta actual; si no, va a su inicio. */
function ContenidoProtegido() {
  const { esAdmin, tienePermiso, inicio, rolDominio } = useAuth() as {
    esAdmin: boolean;
    tienePermiso: (clave: string) => boolean;
    inicio: string;
    rolDominio?: string;
  };
  const location = useLocation();
  const item = itemDeRuta(location.pathname);

  const permitido = puedeAcceder(item, esAdmin, tienePermiso, rolDominio);

  if (!permitido) {
    // Redirige al primer destino permitido (evita bucles si el inicio no aplica).
    const destino = filtrarGrupos(esAdmin, tienePermiso, rolDominio)[0]?.items[0]?.to ?? inicio;
    if (destino !== location.pathname) return <Navigate to={destino} replace />;
    return <div className="p-8 text-center text-sm text-muted">No tienes acceso a esta sección.</div>;
  }

  return (
    <Suspense fallback={<Cargando />}>
      <Outlet />
    </Suspense>
  );
}

function LayoutProtegido() {
  return (
    <ProtectedRoute>
      <AppShell>
        <ContenidoProtegido />
      </AppShell>
    </ProtectedRoute>
  );
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<LayoutProtegido />}>
        <Route index element={<DashboardPage />} />
        <Route path="/pos" element={<PosPage />} />
        <Route path="/plano" element={<MesasPage />} />
        <Route path="/cuentas" element={<CuentasPage />} />
        <Route path="/cuentas/:id" element={<CuentaPage />} />
        <Route path="/ventas" element={<VentasPage />} />
        <Route path="/reservas" element={<ReservasPage />} />
        <Route path="/eventos" element={<EventosPage />} />
        <Route path="/eventos/nuevo" element={<EventosPage />} />
        <Route path="/eventos/:id/editar" element={<EventosPage />} />
        <Route path="/reservas/:id" element={<ReservaPage />} />
        <Route path="/mermas" element={<MermasPage />} />
        <Route path="/inventario" element={<InventarioPage />} />
        <Route path="/inventario/kardex" element={<KardexPage />} />
        <Route path="/inventario/lotes" element={<LotesPage />} />
        <Route path="/inventario/tomas" element={<TomasFisicasPage />} />
        <Route path="/compras" element={<ComprasPage />} />
        <Route path="/compras/ordenes/nueva" element={<ComprasPage />} />
        <Route path="/compras/proveedores" element={<ProveedoresPage />} />
        <Route path="/compras/recepciones" element={<RecepcionesPage />} />
        <Route path="/compras/cuentas" element={<CuentasPorPagarPage />} />
        <Route path="/contenido" element={<PaginasPage />} />
        <Route path="/contenido/local" element={<LocalPage />} />
        <Route path="/contenido/menu" element={<MenuMediaPage />} />
        <Route path="/finanzas" element={<TasasPage />} />
        <Route path="/salon" element={<SalonLayout />}>
          <Route index element={<PlanosPage />} />
          <Route path="mesas" element={<SalonMesasPage />} />
        </Route>
        <Route path="/salon/planos/:id" element={<EditorMapaPage />} />
        <Route path="/entradas" element={<EntradasPage />} />
        <Route path="/vip" element={<ListaVipPage />} />
        <Route path="/productos" element={<ProductosPage />} />
        <Route path="/productos/nuevo" element={<ProductosPage />} />
        <Route path="/productos/:id/editar" element={<ProductosPage />} />
        <Route path="/catalogos" element={<CatalogosPage />} />
        <Route path="/catalogos-avanzado" element={<CatalogoAvanzadoPage />} />
        <Route path="/clientes" element={<ClientesPage />} />
        <Route path="/cxc" element={<CuentasPorCobrarPage />} />
        <Route path="/promociones" element={<PromocionesPage />} />
        <Route path="/reportes" element={<ReportesPage />} />
        <Route path="/auditoria" element={<AuditoriaPage />} />
        <Route path="/caja" element={<CajaPage />} />
        <Route path="/usuarios" element={<UsuariosPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
