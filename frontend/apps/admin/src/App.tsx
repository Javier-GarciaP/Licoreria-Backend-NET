import { lazy, Suspense } from 'react';
import { Outlet, Route, Routes } from 'react-router-dom';
import { Spinner } from '@licoreria/ui';
import { AppShell } from './components/AppShell';
import { ProtectedRoute } from './components/Rbac';
import { LoginPage } from './pages/LoginPage';

const DashboardPage = lazy(() => import('./pages/DashboardPage').then((m) => ({ default: m.DashboardPage })));
const PosPage = lazy(() => import('./pages/PosPage').then((m) => ({ default: m.PosPage })));
const PlanoPage = lazy(() => import('./pages/PlanoPage').then((m) => ({ default: m.PlanoPage })));
const CuentasPage = lazy(() => import('./pages/CuentasPage').then((m) => ({ default: m.CuentasPage })));
const CuentaPage = lazy(() => import('./pages/CuentaPage').then((m) => ({ default: m.CuentaPage })));
const VentasPage = lazy(() => import('./pages/VentasPage').then((m) => ({ default: m.VentasPage })));
const KdsPage = lazy(() => import('./pages/KdsPage').then((m) => ({ default: m.KdsPage })));
const ReservasPage = lazy(() => import('./pages/ReservasPage').then((m) => ({ default: m.ReservasPage })));
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
const TesoreriaPage = lazy(() => import('./pages/TesoreriaPage').then((m) => ({ default: m.TesoreriaPage })));
const PlanosPage = lazy(() => import('./pages/PlanosPage').then((m) => ({ default: m.PlanosPage })));
const ZonasMesasPage = lazy(() => import('./pages/ZonasMesasPage').then((m) => ({ default: m.ZonasMesasPage })));
const ProductosPage = lazy(() => import('./pages/ProductosPage').then((m) => ({ default: m.ProductosPage })));
const CatalogosPage = lazy(() => import('./pages/CatalogosPage').then((m) => ({ default: m.CatalogosPage })));
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

function LayoutProtegido() {
  return (
    <ProtectedRoute>
      <AppShell>
        <Suspense fallback={<Cargando />}>
          <Outlet />
        </Suspense>
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
        <Route path="/plano" element={<PlanoPage />} />
        <Route path="/cuentas" element={<CuentasPage />} />
        <Route path="/cuentas/:id" element={<CuentaPage />} />
        <Route path="/ventas" element={<VentasPage />} />
        <Route path="/kds" element={<KdsPage />} />
        <Route path="/reservas" element={<ReservasPage />} />
        <Route path="/mermas" element={<MermasPage />} />
        <Route path="/inventario" element={<InventarioPage />} />
        <Route path="/inventario/kardex" element={<KardexPage />} />
        <Route path="/inventario/lotes" element={<LotesPage />} />
        <Route path="/inventario/tomas" element={<TomasFisicasPage />} />
        <Route path="/compras" element={<ComprasPage />} />
        <Route path="/compras/proveedores" element={<ProveedoresPage />} />
        <Route path="/compras/recepciones" element={<RecepcionesPage />} />
        <Route path="/compras/cuentas" element={<CuentasPorPagarPage />} />
        <Route path="/contenido" element={<PaginasPage />} />
        <Route path="/contenido/eventos" element={<EventosPage />} />
        <Route path="/contenido/local" element={<LocalPage />} />
        <Route path="/contenido/menu" element={<MenuMediaPage />} />
        <Route path="/finanzas" element={<TasasPage />} />
        <Route path="/finanzas/tesoreria" element={<TesoreriaPage />} />
        <Route path="/salon" element={<PlanosPage />} />
        <Route path="/salon/zonas" element={<ZonasMesasPage />} />
        <Route path="/productos" element={<ProductosPage />} />
        <Route path="/catalogos" element={<CatalogosPage />} />
        <Route path="/clientes" element={<ClientesPage />} />
        <Route path="/caja" element={<CajaPage />} />
        <Route path="/usuarios" element={<UsuariosPage />} />
        <Route path="*" element={<NotFoundPage />} />
      </Route>
    </Routes>
  );
}
