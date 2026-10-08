import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useLocation, useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Button, Card, cn, Input } from '@licoreria/ui';
import { useAuth } from '../context/AuthContext';
import { mensajeDeError } from '../lib/api';
import { CUENTAS_DEMO, esRolServicio, inicioDeRol, urlServicio } from '../lib/roles';

const esquema = z.object({
  username: z.string().min(3, 'Ingresa tu correo o usuario'),
  password: z.string().min(1, 'Ingresa tu contraseña'),
});

type Formulario = z.infer<typeof esquema>;

interface RespuestaLogin {
  rolDominio?: string;
}

export function LoginPage() {
  const { login } = useAuth() as { login: (u: string, p: string) => Promise<RespuestaLogin> };
  const navigate = useNavigate();
  const location = useLocation();
  const destino = (location.state as { from?: { pathname: string } } | null)?.from?.pathname;

  const {
    register,
    handleSubmit,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<Formulario>({ resolver: zodResolver(esquema), defaultValues: { username: '', password: '' } });

  const autenticar = async (username: string, password: string) => {
    try {
      const respuesta = await login(username, password);
      if (esRolServicio(respuesta.rolDominio)) {
        window.location.assign(urlServicio());
        return;
      }
      toast.success('Sesión iniciada');
      navigate(destino ?? inicioDeRol(respuesta.rolDominio), { replace: true });
    } catch (error) {
      toast.error('No se pudo iniciar sesión', { description: mensajeDeError(error) });
    }
  };

  const enviar = (datos: Formulario) => autenticar(datos.username, datos.password);

  const usarCuenta = (cuenta: (typeof CUENTAS_DEMO)[number]) => {
    setValue('username', cuenta.email);
    setValue('password', cuenta.password);
    void autenticar(cuenta.email, cuenta.password);
  };

  return (
    <div className="flex min-h-dvh items-center justify-center bg-canvas px-4 py-10">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-card bg-accent text-2xl text-on-pastel shadow-glow">
            &#127863;
          </div>
          <h1 className="text-xl font-medium tracking-tightest text-ink">Panel interno</h1>
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

        <div className="mt-6">
          <p className="mb-2 text-center text-xs text-muted">Entrar como</p>
          <div className="flex flex-wrap justify-center gap-1.5">
            {CUENTAS_DEMO.map((cuenta) => (
              <button
                key={cuenta.email}
                type="button"
                onClick={() => usarCuenta(cuenta)}
                className={cn(
                  'rounded-pill border border-hairline px-3 py-1.5 text-xs text-muted transition',
                  'hover:border-accent hover:bg-accent/15 hover:text-accent-ink',
                )}
              >
                {cuenta.rol}
              </button>
            ))}
          </div>
          <p className="mt-3 text-center text-[11px] text-muted">Contraseña demo: <span className="num text-ink">demo123</span></p>
        </div>
      </div>
    </div>
  );
}
