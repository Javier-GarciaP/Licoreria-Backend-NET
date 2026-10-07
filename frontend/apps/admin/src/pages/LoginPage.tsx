import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useLocation, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Button, Card, Input } from '@licoreria/ui';
import { useAuth } from '../context/AuthContext';
import { mensajeDeError } from '../lib/api';

const esquema = z.object({
  username: z.string().min(3, 'Ingresa tu correo o usuario'),
  password: z.string().min(1, 'Ingresa tu contraseña'),
});

type Formulario = z.infer<typeof esquema>;

export function LoginPage() {
  const { login } = useAuth() as { login: (u: string, p: string) => Promise<unknown> };
  const navigate = useNavigate();
  const location = useLocation();
  const destino = (location.state as { from?: { pathname: string } } | null)?.from?.pathname ?? '/';

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: { username: '', password: '' } });

  const enviar = async (datos: Formulario) => {
    try {
      await login(datos.username, datos.password);
      toast.success('Sesión iniciada');
      navigate(destino, { replace: true });
    } catch (error) {
      toast.error('No se pudo iniciar sesión', { description: mensajeDeError(error) });
    }
  };

  return (
    <div className="flex min-h-dvh items-center justify-center bg-canvas px-4">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-card bg-accent text-2xl text-white shadow-glow">
            &#127863;
          </div>
          <h1 className="text-xl font-semibold tracking-tightest text-ink">Panel interno</h1>
          <p className="mt-1 text-sm text-muted">Licorería · Discoteca</p>
        </div>

        <Card className="p-6">
          <form className="flex flex-col gap-4" onSubmit={handleSubmit(enviar)} noValidate>
            <Input
              label="Correo"
              type="email"
              placeholder="admin@licoreria.com"
              autoComplete="username"
              error={errors.username?.message}
              {...register('username')}
            />
            <Input
              label="Contraseña"
              type="password"
              placeholder="••••••••"
              autoComplete="current-password"
              error={errors.password?.message}
              {...register('password')}
            />
            <Button type="submit" loading={isSubmitting} className="mt-2 w-full">
              Entrar
            </Button>
          </form>
        </Card>

        <p className="mt-6 text-center text-xs text-muted">
          Demo: <span className="text-ink">admin@licoreria.com</span> / <span className="text-ink">admin123</span>
        </p>
      </div>
    </div>
  );
}
