import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { obtenerAccessTokenVigente } from '../lib/api';

const hubUrl = (import.meta.env.VITE_HUB_URL as string | undefined) ?? 'http://localhost:5190/hubs/comandas';

/** Espera creciente (máx. 30 s) entre reintentos: nunca se rinde mientras la pantalla siga abierta. */
const esperaReintento = (intento: number) => Math.min(30_000, 1_000 * 2 ** Math.min(intento, 5));

/**
 * Conexión en tiempo real al hub /hubs/comandas. Se une al área indicada
 * (barra, cocina, meseros o staff) y despacha los eventos al handler.
 *
 * Se mantiene viva: renueva el token de acceso antes de cada (re)conexión, reintenta sin límite si el
 * arranque o la reconexión fallan (red caída, teléfono en reposo, token vencido) y se reconecta al
 * volver a la pestaña. `alReconectar` permite refrescar lo que se pudo perder mientras estuvo caída.
 */
export function useRealtime(
  area: string,
  handlers: Record<string, (payload: unknown) => void>,
  alReconectar?: () => void,
) {
  const handlersRef = useRef(handlers);
  handlersRef.current = handlers;
  const reconectarRef = useRef(alReconectar);
  reconectarRef.current = alReconectar;

  useEffect(() => {
    let activo = true;
    let temporizador: ReturnType<typeof setTimeout> | undefined;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => obtenerAccessTokenVigente() })
      .withAutomaticReconnect({ nextRetryDelayInMilliseconds: (ctx) => esperaReintento(ctx.previousRetryCount) })
      .build();

    for (const event of Object.keys(handlersRef.current)) {
      connection.on(event, (payload: unknown) => handlersRef.current[event]?.(payload));
    }

    const unirse = () => connection.invoke('UnirseArea', area);

    const conectar = async (intento = 0): Promise<void> => {
      if (!activo || connection.state !== signalR.HubConnectionState.Disconnected) return;
      try {
        await connection.start();
        await unirse();
        if (intento > 0) reconectarRef.current?.();
      } catch {
        if (!activo) return;
        // Si el arranque quedó a medias, se corta antes de reintentar.
        await connection.stop().catch(() => undefined);
        temporizador = setTimeout(() => void conectar(intento + 1), esperaReintento(intento));
      }
    };

    connection.onreconnected(() => {
      void unirse()
        .then(() => reconectarRef.current?.())
        .catch(() => connection.stop().then(() => conectar(1)));
    });
    // Agotada la reconexión automática (o cerrada por el servidor): se vuelve a empezar.
    connection.onclose(() => {
      if (activo) temporizador = setTimeout(() => void conectar(1), esperaReintento(0));
    });

    const alVolver = () => {
      if (document.visibilityState === 'visible') void conectar(1);
    };
    document.addEventListener('visibilitychange', alVolver);

    void conectar();

    return () => {
      activo = false;
      if (temporizador) clearTimeout(temporizador);
      document.removeEventListener('visibilitychange', alVolver);
      void connection.stop();
    };
  }, [area]);
}
