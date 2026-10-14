import { toast } from 'sonner';
import { describirConcurrencia, mensajeDeError } from './api';

/**
 * Muestra el error de una operación. Si es una carrera de concurrencia (409) o un producto agotado (422),
 * avisa como advertencia y llama a `alConflicto` para que la pantalla refresque sus datos; lo que el
 * usuario estaba armando (el pedido, el cobro) se conserva porque solo se limpia al tener éxito.
 */
export function avisarError(titulo: string, error: unknown, alConflicto?: () => void): void {
  const aviso = describirConcurrencia(error);
  if (aviso) {
    toast.warning(aviso.titulo, { description: mensajeDeError(error) });
    alConflicto?.();
    return;
  }
  toast.error(titulo, { description: mensajeDeError(error) });
}
