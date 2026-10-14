import { LoginPage as LoginCompartido } from '@licoreria/auth';
import { Wine } from 'lucide-react';

export function LoginPage() {
  return (
    <LoginCompartido
      app="servicio"
      titulo="Bienvenido a Servicio"
      subtitulo="Mesoneros · Barra · Cocina"
      logo={
        <span className="flex h-14 w-14 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-md">
          <Wine className="h-7 w-7" strokeWidth={1.8} />
        </span>
      }
    />
  );
}
