import { useAuth } from '../context/AuthContext';
import { etiquetaRol } from '../lib/roles';

export function MesoneroPage() {
  const { usuario, logout } = useAuth();
  return (
    <div className="flex min-h-dvh flex-col bg-canvas p-6">
      <header className="flex items-center justify-between border-b border-hairline pb-4">
        <div>
          <p className="text-sm font-medium text-ink">Hola, {usuario?.nombreCompleto}</p>
          <p className="text-xs text-muted">{etiquetaRol(usuario?.rolDominio)} · Modo mesonero</p>
        </div>
        <button type="button" onClick={() => void logout()} className="rounded-pill border border-hairline px-3 py-1.5 text-xs text-muted hover:text-ink">
          Salir
        </button>
      </header>
      <div className="flex flex-1 items-center justify-center">
        <p className="text-sm text-muted">El módulo del mesonero se construye en la siguiente fase.</p>
      </div>
    </div>
  );
}
