import { Link } from 'react-router-dom';
import { Button, EmptyState } from '@licoreria/ui';

export function NotFoundPage() {
  return (
    <div className="mx-auto flex max-w-page items-center justify-center py-20">
      <EmptyState
        title="Página no encontrada"
        description="La ruta que buscas no existe o no tienes acceso."
        action={
          <Link to="/">
            <Button>Volver al dashboard</Button>
          </Link>
        }
      />
    </div>
  );
}
