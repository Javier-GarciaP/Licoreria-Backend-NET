import { useState, type ReactNode } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useLocation, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Eye, EyeOff, Lock, Mail } from 'lucide-react';
import { Button, Card, CardBody, Input } from '@licoreria/ui';
import { mensajeDeError } from './api';
import { useAuth } from './AuthContext';
import { esRolServicio, inicioDeRol, urlAdmin, urlServicio } from './roles';

const esquema = z.object({
  username: z.string().min(3, 'Ingresa tu correo o usuario'),
  password: z.string().min(1, 'Ingresa tu contraseña'),
});

type Formulario = z.infer<typeof esquema>;

/** Aplicación que está mostrando el login; define la redirección cruzada por rol. */
export type AplicacionLogin = 'admin' | 'servicio';

export interface LoginPageProps {
  app: AplicacionLogin;
  titulo: string;
  subtitulo: string;
  logo: ReactNode;
}

/** Bolas pastel en deriva del fondo del login (fuera de la card). */
const BOLAS = [
  { size: 320, pos: { top: '-70px', left: '-80px' }, a: '--primary', b: '--info', dur: '26s', ret: '0s' },
  { size: 260, pos: { top: '40px', right: '-70px' }, a: '--success', b: '--butter', dur: '22s', ret: '-6s' },
  { size: 300, pos: { bottom: '-90px', left: '60px' }, a: '--warning', b: '--destructive', dur: '28s', ret: '-3s' },
  { size: 220, pos: { bottom: '30px', right: '40px' }, a: '--info', b: '--primary', dur: '20s', ret: '-9s' },
  { size: 180, pos: { top: '44%', left: '8%' }, a: '--butter', b: '--success', dur: '24s', ret: '-12s' },
  { size: 200, pos: { top: '30%', right: '14%' }, a: '--destructive', b: '--primary', dur: '19s', ret: '-4s' },
] as const;

/**
 * Pantalla de login compartida por los apps de administración y servicio.
 * Card centrada con fondo translúcido sobre un lienzo de bolas pastel en
 * deriva; el icono de la marca queda fuera y encima de la card.
 * Si el rol pertenece a la otra aplicación, cierra la sesión local y redirige
 * a su app (cada origen maneja su propio JWT en localStorage).
 */
export function LoginPage({ app, titulo, subtitulo, logo }: LoginPageProps) {
  const { login, logout } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const destino = (location.state as { from?: { pathname: string } } | null)?.from?.pathname;

  const [verClave, setVerClave] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: { username: '', password: '' } });

  const autenticar = async (username: string, password: string) => {
    try {
      const respuesta = await login(username, password);

      if (esRolServicio(respuesta.rolDominio)) {
        if (app === 'servicio') {
          toast.success('Sesión iniciada');
          navigate(inicioDeRol(respuesta.rolDominio), { replace: true });
          return;
        }
        toast.info('Redirigiendo al panel de Servicio…');
        await logout();
        window.location.assign(urlServicio());
        return;
      }

      if (app === 'servicio') {
        toast.error('Este sistema es para el personal de servicio', {
          description: 'Meseros, Barra y Cocina usan este panel. El resto entra por el panel de administración.',
        });
        await logout();
        window.location.assign(urlAdmin());
        return;
      }

      toast.success('Sesión iniciada');
      navigate(destino ?? inicioDeRol(respuesta.rolDominio), { replace: true });
    } catch (error) {
      toast.error('No se pudo iniciar sesión', { description: mensajeDeError(error) });
    }
  };

  const enviar = (datos: Formulario) => autenticar(datos.username, datos.password);

  return (
    <div className="relative flex min-h-dvh items-center justify-center overflow-hidden bg-background px-4 py-14">
      {/* Lienzo: bolas pastel degradadas en deriva */}
      <div aria-hidden="true" className="absolute inset-0 overflow-hidden">
        {BOLAS.map((bola) => (
          <span
            key={`${bola.a}-${bola.size}`}
            className="absolute animate-deriva-bola rounded-full blur-[18px] motion-reduce:animate-none"
            style={{
              width: bola.size,
              height: bola.size,
              ...bola.pos,
              animationDuration: bola.dur,
              animationDelay: bola.ret,
              background: `radial-gradient(circle at 32% 30%, rgb(var(${bola.a}) / 0.75), rgb(var(${bola.b}) / 0.35) 62%, rgb(var(${bola.b}) / 0) 78%)`,
            }}
          />
        ))}
      </div>

      {/* Acceso */}
      <div className="relative z-10 w-full max-w-sm">
        <div className="absolute left-1/2 top-0 z-10 -translate-x-1/2 -translate-y-1/2">{logo}</div>

        <Card className="border-border/70 bg-card/70 shadow-lg backdrop-blur-xl">
          <CardBody className="pt-14">
            <header className="mb-6 text-center">
              <h1 className="text-xl font-medium tracking-tightest text-foreground">{titulo}</h1>
              <p className="mt-1 text-sm text-muted-foreground">{subtitulo}</p>
            </header>

            <form className="flex flex-col gap-4" onSubmit={handleSubmit(enviar)} noValidate>
              <Input
                label="Correo"
                type="email"
                placeholder="tu@licoreria.com"
                autoComplete="username"
                leftSlot={<Mail className="h-4 w-4" />}
                error={errors.username?.message}
                className="h-11"
                {...register('username')}
              />
              <Input
                label="Contraseña"
                type={verClave ? 'text' : 'password'}
                placeholder="••••••••"
                autoComplete="current-password"
                leftSlot={<Lock className="h-4 w-4" />}
                rightSlot={
                  <button
                    type="button"
                    onClick={() => setVerClave((actual) => !actual)}
                    aria-label={verClave ? 'Ocultar contraseña' : 'Mostrar contraseña'}
                    className="flex h-9 w-9 items-center justify-center rounded-md text-muted-foreground transition-colors hover:bg-accent/10 hover:text-foreground focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring"
                  >
                    {verClave ? <EyeOff className="h-4 w-4" /> : <Eye className="h-4 w-4" />}
                  </button>
                }
                error={errors.password?.message}
                className="h-11"
                {...register('password')}
              />
              <Button type="submit" size="lg" loading={isSubmitting} className="mt-1 h-11 w-full">
                Entrar
              </Button>
            </form>
          </CardBody>
        </Card>

        <p className="mt-6 text-center text-xs text-muted-foreground">
          Credenciales de prueba documentadas en el README del proyecto.
        </p>
      </div>
    </div>
  );
}
