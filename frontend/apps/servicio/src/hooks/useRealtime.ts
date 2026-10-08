import { useEffect, useRef } from 'react';
import * as signalR from '@microsoft/signalr';
import { tokens } from '../lib/api';

const hubUrl = (import.meta.env.VITE_HUB_URL as string | undefined) ?? 'http://localhost:5190/hubs/comandas';

/**
 * Conexión en tiempo real al hub /hubs/comandas. Se une al área indicada
 * (barra, cocina, meseros o staff) y despacha los eventos al handler.
 */
export function useRealtime(area: string, handlers: Record<string, (payload: unknown) => void>) {
  const handlersRef = useRef(handlers);
  handlersRef.current = handlers;

  useEffect(() => {
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, { accessTokenFactory: () => tokens.access() ?? '' })
      .withAutomaticReconnect()
      .build();

    for (const event of Object.keys(handlersRef.current)) {
      connection.on(event, (payload: unknown) => handlersRef.current[event]?.(payload));
    }

    const unirse = () => connection.invoke('UnirseArea', area).catch(() => undefined);
    connection.onreconnected(unirse);

    connection
      .start()
      .then(unirse)
      .catch(() => undefined);

    return () => {
      void connection.stop();
    };
  }, [area]);
}
