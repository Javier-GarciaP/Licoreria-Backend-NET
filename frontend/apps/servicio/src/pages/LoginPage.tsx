import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'sonner';
import { Button, Card, cn, Input } from '@licoreria/ui';
import { useAuth } from '../context/AuthContext';
import { mensajeDeError } from '../lib/api';
import { CUENTAS_DEMO, esRolServicio, inicioDeRol } from '../lib/roles';

export function LoginPage() {
  const { login, logout } = useAuth();
  const navigate = useNavigate();
  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [cargando, setCargando] = useState(false);

  const autenticar = async (usuario: string, clave: string) => {
    setCargando(true);
    try {
      const respuesta = await login(usuario, clave);
      if (!esRolServicio(respuesta.rolDominio)) {
        toast.error('Este sistema es para el personal de servicio', {
          description: 'Meseros, Barra y Cocina usan este panel. El resto entra por el panel de administración.',
        });
        await logout();
        setCargando(false);
        return;
      }
      toast.success('Sesión iniciada');
      navigate(inicioDeRol(respuesta.rolDominio), { replace: true });
    } catch (error) {
      toast.error('No se pudo iniciar sesión', { description: mensajeDeError(error) });
      setCargando(false);
    }
  };

  const enviar = (evento: React.FormEvent) => {
    evento.preventDefault();
    if (username.trim().length === 0 || password.length === 0) return;
    void autenticar(username.trim(), password);
  };

  const usarCuenta = (cuenta: (typeof CUENTAS_DEMO)[number]) => void autenticar(cuenta.email, cuenta.password);

  return (
    <div className="flex min-h-dvh items-center justify-center bg-background px-4 py-10">
      <div className="w-full max-w-sm">
        <div className="mb-8 text-center">
          <div className="mx-auto mb-4 flex h-14 w-14 items-center justify-center rounded-xl bg-primary text-2xl text-primary-foreground">
            &#127863;
          </div>
          <h1 className="text-xl font-medium tracking-tightest text-foreground">Servicio</h1>
          <p className="mt-1 text-sm text-muted-foreground">Mesoneros · Barra · Cocina</p>
        </div>

        <Card className="p-6">
          <form className="flex flex-col gap-4" onSubmit={enviar} noValidate>
            <Input label="Correo" type="email" placeholder="mesero1@licoreria.com" autoComplete="username" value={username} onChange={(e) => setUsername(e.target.value)} />
            <Input label="Contraseña" type="password" placeholder="••••••••" autoComplete="current-password" value={password} onChange={(e) => setPassword(e.target.value)} />
            <Button type="submit" loading={cargando} className="mt-2 w-full">
              Entrar
            </Button>
          </form>
        </Card>

        <div className="mt-6">
          <p className="mb-2 text-center text-xs text-muted-foreground">Entrar como</p>
          <div className="flex flex-wrap justify-center gap-1.5">
            {CUENTAS_DEMO.map((cuenta) => (
              <button
                key={cuenta.email}
                type="button"
                onClick={() => usarCuenta(cuenta)}
                className={cn(
                  'rounded-full border border-border px-3 py-1.5 text-xs text-muted-foreground transition',
                  'hover:border-primary hover:bg-primary/15 hover:text-foreground',
                )}
              >
                {cuenta.rol}
              </button>
            ))}
          </div>
          <p className="mt-3 text-center text-[11px] text-muted-foreground">
            Contraseña demo: <span className="num text-foreground">demo123</span>
          </p>
        </div>
      </div>
    </div>
  );
}
