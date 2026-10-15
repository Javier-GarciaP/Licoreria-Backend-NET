import { useEffect, useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import { Search, UserPlus, X } from 'lucide-react';
import { Button } from '@licoreria/ui';
import type { Cliente } from '@licoreria/types';
import { clientesApi } from '@licoreria/api-client';
import { mensajeDeError } from '../../lib/api';

const campo =
  'h-9 w-full rounded-control border border-border bg-card px-3 text-sm text-foreground focus:outline-none focus:ring-2 focus:ring-ring/50';

/**
 * Selector de cliente para una venta: busca por nombre, documento o teléfono, permite crear uno con
 * los datos mínimos y muestra el nombre y los puntos del elegido.
 */
export function ClienteVentaPicker({
  cliente,
  onChange,
}: {
  cliente: Cliente | null;
  onChange: (cliente: Cliente | null) => void;
}) {
  const queryClient = useQueryClient();
  const [texto, setTexto] = useState('');
  const [busqueda, setBusqueda] = useState('');
  const [creando, setCreando] = useState(false);
  const [nombre, setNombre] = useState('');
  const [telefono, setTelefono] = useState('');

  useEffect(() => {
    const temporizador = setTimeout(() => setBusqueda(texto.trim()), 250);
    return () => clearTimeout(temporizador);
  }, [texto]);

  const resultados = useQuery({
    queryKey: ['pos', 'clientes', busqueda],
    queryFn: () => clientesApi.listar({ busqueda, pageSize: 6 }),
    enabled: !cliente && busqueda.length >= 2,
  });

  const crear = useMutation({
    mutationFn: () => clientesApi.crear({ nombre: nombre.trim(), telefono: telefono.trim() || null }),
    onSuccess: (nuevo) => {
      queryClient.invalidateQueries({ queryKey: ['pos', 'clientes'] });
      queryClient.invalidateQueries({ queryKey: ['clientes'] });
      elegir(nuevo);
      toast.success('Cliente creado');
    },
    onError: (error) => toast.error('No se pudo crear el cliente', { description: mensajeDeError(error) }),
  });

  const elegir = (elegido: Cliente | null) => {
    onChange(elegido);
    setTexto('');
    setBusqueda('');
    setCreando(false);
    setNombre('');
    setTelefono('');
  };

  if (cliente) {
    return (
      <div className="flex items-center justify-between gap-2 rounded-control border border-border bg-muted/40 px-3 py-2">
        <div className="min-w-0">
          <p className="truncate text-sm font-medium text-foreground">{cliente.nombre}</p>
          <p className="num text-xs text-muted-foreground">{cliente.puntos} puntos</p>
        </div>
        <button
          type="button"
          aria-label="Quitar cliente"
          onClick={() => elegir(null)}
          className="flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-muted-foreground transition hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
        >
          <X size={15} />
        </button>
      </div>
    );
  }

  if (creando) {
    return (
      <form
        className="flex flex-col gap-2 rounded-control border border-border p-2"
        onSubmit={(evento) => {
          evento.preventDefault();
          if (nombre.trim()) crear.mutate();
        }}
      >
        <input
          aria-label="Nombre del cliente"
          placeholder="Nombre"
          value={nombre}
          onChange={(evento) => setNombre(evento.target.value)}
          className={campo}
          autoFocus
        />
        <input
          aria-label="Teléfono del cliente"
          placeholder="Teléfono (opcional)"
          value={telefono}
          onChange={(evento) => setTelefono(evento.target.value)}
          className={campo}
        />
        <div className="flex justify-end gap-2">
          <Button type="button" variant="ghost" size="sm" onClick={() => setCreando(false)}>
            Cancelar
          </Button>
          <Button type="submit" size="sm" loading={crear.isPending} disabled={!nombre.trim()}>
            Crear cliente
          </Button>
        </div>
      </form>
    );
  }

  const lista = resultados.data?.items ?? [];

  return (
    <div className="flex flex-col gap-1.5">
      <div className="flex items-center gap-2">
        <div className="relative min-w-0 flex-1">
          <Search size={14} className="pointer-events-none absolute left-2.5 top-1/2 -translate-y-1/2 text-muted-foreground" />
          <input
            aria-label="Buscar cliente"
            placeholder="Cliente (nombre, documento o teléfono)"
            value={texto}
            onChange={(evento) => setTexto(evento.target.value)}
            className={`${campo} pl-8`}
          />
        </div>
        <button
          type="button"
          aria-label="Nuevo cliente"
          title="Nuevo cliente"
          onClick={() => {
            setNombre(texto.trim());
            setCreando(true);
          }}
          className="flex h-9 w-9 shrink-0 items-center justify-center rounded-control border border-border text-muted-foreground transition hover:text-foreground focus:outline-none focus-visible:ring-2 focus-visible:ring-ring/50"
        >
          <UserPlus size={15} />
        </button>
      </div>

      {busqueda.length >= 2 && !resultados.isLoading && (
        <ul role="listbox" aria-label="Clientes encontrados" className="max-h-40 overflow-y-auto rounded-control border border-border bg-card">
          {lista.length === 0 && <li className="px-3 py-2 text-xs text-muted-foreground">Sin coincidencias</li>}
          {lista.map((encontrado) => (
            <li key={encontrado.id} role="option" aria-selected={false}>
              <button
                type="button"
                onClick={() => elegir(encontrado)}
                disabled={!encontrado.activo}
                className="flex w-full items-center justify-between gap-2 px-3 py-2 text-left text-sm transition hover:bg-muted/60 disabled:cursor-not-allowed disabled:opacity-50"
              >
                <span className="min-w-0 truncate">{encontrado.nombre}</span>
                <span className="num shrink-0 text-xs text-muted-foreground">
                  {encontrado.activo ? `${encontrado.puntos} pts` : 'Inactivo'}
                </span>
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
