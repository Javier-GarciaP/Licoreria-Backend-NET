import { LoginPage as LoginCompartido } from '@licoreria/auth';
import { LogoCorcho } from '../components/Logo';

export function LoginPage() {
  return (
    <LoginCompartido
      app="admin"
      titulo="Bienvenido a CORCHO"
      subtitulo="Panel interno · Licorería &amp; Discoteca"
      logo={
        <span className="flex h-14 w-14 items-center justify-center rounded-xl bg-primary text-primary-foreground shadow-md">
          <LogoCorcho className="h-8 w-8" />
        </span>
      }
    />
  );
}
