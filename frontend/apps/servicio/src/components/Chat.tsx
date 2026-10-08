import { useEffect, useRef, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { MessageCircle, Send, X } from 'lucide-react';
import { cn } from '@licoreria/ui';
import type { ChatMensaje } from '@licoreria/types';
import { chatApi } from '@licoreria/api-client';
import { useAuth } from '../context/AuthContext';
import { useRealtime } from '../hooks/useRealtime';
import { formatTime } from '../lib/format';
import { etiquetaRol } from '../lib/roles';
import { mensajeDeError } from '../lib/api';
import { toast } from 'sonner';

/** Chat básico del personal (mesonero, barra y cocina) en vivo. */
export function Chat() {
  const queryClient = useQueryClient();
  const { usuario } = useAuth();
  const [abierto, setAbierto] = useState(false);
  const [texto, setTexto] = useState('');
  const listaRef = useRef<HTMLDivElement>(null);

  const mensajes = useQuery({ queryKey: ['chat'], queryFn: () => chatApi.listar(50) });

  useRealtime('staff', {
    'chat:recibido': () => queryClient.invalidateQueries({ queryKey: ['chat'] }),
  });

  const enviar = useMutation({
    mutationFn: (mensaje: string) => chatApi.enviar(mensaje),
    onSuccess: () => {
      setTexto('');
      queryClient.invalidateQueries({ queryKey: ['chat'] });
    },
    onError: (error) => toast.error('No se pudo enviar', { description: mensajeDeError(error) }),
  });

  useEffect(() => {
    if (abierto) listaRef.current?.scrollTo({ top: listaRef.current.scrollHeight });
  }, [abierto, mensajes.data]);

  const esMio = (m: ChatMensaje) => m.autorId === usuario?.usuarioId;

  return (
    <>
      <button
        type="button"
        aria-label="Abrir chat"
        onClick={() => setAbierto((v) => !v)}
        className={cn(
          'fixed bottom-5 right-5 z-50 flex h-12 w-12 items-center justify-center rounded-full shadow-card transition',
          abierto ? 'bg-ink text-canvas' : 'bg-accent text-on-pastel hover:bg-accent/90',
        )}
      >
        {abierto ? <X size={20} /> : <MessageCircle size={20} />}
      </button>

      {abierto && (
        <div className="fixed bottom-20 right-5 z-50 flex h-[26rem] w-[20rem] flex-col overflow-hidden rounded-2xl border border-hairline bg-surface/95 shadow-card backdrop-blur">
          <div className="flex items-center gap-2 border-b border-hairline px-4 py-3">
            <span className="flex h-7 w-7 items-center justify-center rounded-full bg-accent/25 text-accent-ink">
              <MessageCircle size={14} />
            </span>
            <div>
              <p className="text-sm font-medium text-ink">Chat del personal</p>
              <p className="text-[10px] text-success-ink">● en vivo</p>
            </div>
          </div>

          <div ref={listaRef} className="app-scroll min-h-0 flex-1 overflow-y-auto px-3 py-3">
            {mensajes.isLoading ? (
              <p className="py-8 text-center text-xs text-muted">Cargando…</p>
            ) : (mensajes.data?.length ?? 0) === 0 ? (
              <p className="py-8 text-center text-xs text-muted">Sin mensajes todavía.</p>
            ) : (
              <ul className="flex flex-col gap-2">
                {mensajes.data?.map((m) => (
                  <li key={m.id} className={cn('flex flex-col', esMio(m) ? 'items-end' : 'items-start')}>
                    <div
                      className={cn(
                        'max-w-[85%] rounded-2xl px-3 py-2',
                        esMio(m) ? 'rounded-br-sm bg-accent/20' : 'rounded-bl-sm bg-elevated/60',
                      )}
                    >
                      <p className="text-xs leading-snug text-ink">{m.mensaje}</p>
                      <p className={cn('mt-0.5 text-[10px]', esMio(m) ? 'text-accent-ink' : 'text-muted')}>
                        {esMio(m) ? 'Tú' : m.autorNombre} · {etiquetaRol(m.rol)} · {formatTime(m.creadoEn)}
                      </p>
                    </div>
                  </li>
                ))}
              </ul>
            )}
          </div>

          <form
            className="flex items-center gap-2 border-t border-hairline p-2"
            onSubmit={(e) => {
              e.preventDefault();
              if (texto.trim().length === 0 || enviar.isPending) return;
              enviar.mutate(texto.trim());
            }}
          >
            <input
              aria-label="Mensaje"
              value={texto}
              onChange={(e) => setTexto(e.target.value)}
              placeholder="Escribe un mensaje…"
              className="h-9 min-w-0 flex-1 rounded-control border border-hairline bg-surface px-3 text-sm text-ink focus:outline-none focus:ring-2 focus:ring-accent/50"
            />
            <button
              type="submit"
              aria-label="Enviar"
              disabled={texto.trim().length === 0 || enviar.isPending}
              className="flex h-9 w-9 items-center justify-center rounded-full bg-accent text-on-pastel transition disabled:opacity-40"
            >
              <Send size={15} />
            </button>
          </form>
        </div>
      )}
    </>
  );
}
