import { Navigate, Route, Routes, useLocation } from 'react-router-dom';
import { useAuth } from './context/AuthContext';
import { LoginPage } from './pages/LoginPage';
import { MesoneroPage } from './pages/MesoneroPage';
import { KdsPage } from './pages/KdsPage';
import { esRolServicio } from './lib/roles';

/** Guard: exige sesión y rol de servicio (Mesero/Barra/Cocina). */
function Protegido({ children }: { children: React.ReactNode }) {
  const { autenticado, inicializando, rolDominio } = useAuth();
  const location = useLocation();

  if (inicializando) {
    return (
      <div className="flex min-h-dvh items-center justify-center bg-background">
        <p className="text-sm text-muted-foreground">Cargando…</p>
      </div>
    );
  }

  if (!autenticado) return <Navigate to="/login" state={{ from: location }} replace />;
  if (!esRolServicio(rolDominio)) return <Navigate to="/login" replace />;

  return <>{children}</>;
}

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route
        path="/mesonero"
        element={
          <Protegido>
            <MesoneroPage />
          </Protegido>
        }
      />
      <Route
        path="/kds"
        element={
          <Protegido>
            <KdsPage />
          </Protegido>
        }
      />
      <Route path="*" element={<Navigate to="/login" replace />} />
    </Routes>
  );
}
