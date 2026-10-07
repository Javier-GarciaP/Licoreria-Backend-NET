import { Component, type ErrorInfo, type ReactNode } from 'react';
import { Button, Card } from '@licoreria/ui';

interface Props {
  children: ReactNode;
}

interface State {
  error: Error | null;
}

/** Captura errores de render y evita que la app quede en blanco. */
export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  componentDidCatch(error: Error, info: ErrorInfo) {
    // En un entorno real esto se enviaría a un colector de errores.
    console.error('ErrorBoundary', error, info.componentStack);
  }

  render() {
    if (this.state.error) {
      return (
        <div className="flex min-h-dvh items-center justify-center bg-canvas px-4">
          <Card className="w-full max-w-md p-6 text-center">
            <h1 className="text-lg font-medium text-ink">Algo salió mal</h1>
            <p className="mt-2 text-sm text-muted">{this.state.error.message}</p>
            <Button className="mt-5" onClick={() => window.location.reload()}>
              Recargar
            </Button>
          </Card>
        </div>
      );
    }

    return this.props.children;
  }
}
