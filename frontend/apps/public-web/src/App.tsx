import { useQuery } from '@tanstack/react-query';
import { CalendarDays, MapPin, MessageCircle, Wine } from 'lucide-react';
import { Card, CardBody, CardHeader, CardTitle, Pill, Skeleton } from '@licoreria/ui';
import { publicApi } from '@licoreria/api-client';
import { formatBS, formatFecha, formatUSD } from './lib/format';

function parseWhatsapp(data?: Record<string, unknown>): string | null {
  if (!data) return null;
  const valor = data.whatsapp ?? data.whatsApp ?? data.telefono;
  return typeof valor === 'string' && valor.length > 0 ? valor : null;
}

export default function App() {
  const tasa = useQuery({ queryKey: ['tasa'], queryFn: () => publicApi.tasaActual('Paralelo') });
  const menu = useQuery({ queryKey: ['menu-digital'], queryFn: publicApi.menuDigital });
  const eventos = useQuery({ queryKey: ['eventos'], queryFn: publicApi.eventos });
  const local = useQuery({ queryKey: ['local-info'], queryFn: publicApi.localInfo });

  const whatsapp = parseWhatsapp(local.data);

  return (
    <div className="min-h-dvh bg-canvas text-ink">
      <header className="mx-auto flex max-w-page items-center justify-between px-4 py-5">
        <div className="flex items-center gap-2">
          <span className="flex h-10 w-10 items-center justify-center rounded-full bg-accent text-white shadow-glow">
            <Wine size={20} />
          </span>
          <div>
            <p className="text-sm font-semibold tracking-tighter2">Licorería · Discoteca</p>
            <p className="text-[11px] text-muted">Menú, eventos y reservas</p>
          </div>
        </div>
        {whatsapp && (
          <a
            href={`https://wa.me/${whatsapp.replace(/\D/g, '')}`}
            target="_blank"
            rel="noreferrer"
            className="inline-flex items-center gap-2 rounded-pill border border-hairline px-4 py-2 text-sm text-ink transition hover:border-accent/60"
          >
            <MessageCircle size={16} /> WhatsApp
          </a>
        )}
      </header>

      <main className="mx-auto flex max-w-page flex-col gap-16 px-4 pb-24">
        <section className="grid grid-cols-1 gap-6 lg:grid-cols-[1.4fr_1fr]">
          <div className="flex flex-col justify-center gap-4">
            <Pill tone="accent">Bienvenido</Pill>
            <h1 className="text-3xl font-semibold tracking-tightest sm:text-4xl">
              La mejor selección de licores y la mejor noche de la ciudad.
            </h1>
            <p className="max-w-xl text-sm text-muted">
              Consulta nuestro menú digital actualizado, la tasa del día y los próximos eventos. Reserva tu mesa y
              vive la experiencia.
            </p>
            <div className="flex flex-wrap gap-3">
              <span className="inline-flex items-center gap-2 rounded-pill bg-elevated px-4 py-2 text-sm">
                <CalendarDays size={16} /> {eventos.data?.length ?? 0} eventos próximos
              </span>
              {whatsapp && (
                <span className="inline-flex items-center gap-2 rounded-pill bg-elevated px-4 py-2 text-sm">
                  <MapPin size={16} /> Una sola sucursal
                </span>
              )}
            </div>
          </div>

          <Card className="p-6">
            <p className="text-xs uppercase tracking-tighter2 text-muted">Tasa del día · Paralelo</p>
            {tasa.isLoading ? (
              <Skeleton className="mt-3 h-10 w-40" />
            ) : (
              <>
                <p className="mt-2 text-3xl font-semibold tracking-tightest">{formatBS(tasa.data?.valor ?? 0)}</p>
                <p className="mt-1 text-xs text-muted">
                  {tasa.data?.fecha ? `Actualizada ${formatFecha(tasa.data.fecha)}` : 'Sin actualizar'}
                </p>
              </>
            )}
          </Card>
        </section>

        <section className="flex flex-col gap-6">
          <h2 className="text-xl font-semibold tracking-tightest">Menú digital</h2>
          {menu.isLoading ? (
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <Skeleton className="h-64 w-full" />
              <Skeleton className="h-64 w-full" />
            </div>
          ) : (
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2 xl:grid-cols-3">
              {menu.data?.secciones.map((seccion) => (
                <Card key={seccion.categoriaId}>
                  <CardHeader>
                    <CardTitle>{seccion.nombre}</CardTitle>
                  </CardHeader>
                  <CardBody className="flex flex-col gap-2">
                    {seccion.items.map((item) => (
                      <div key={item.varianteId} className="flex items-center justify-between gap-3 text-sm">
                        <span className="text-ink">{item.nombre}</span>
                        <span className="text-right text-muted">
                          {formatUSD(item.precioUSD)}
                          <span className="block text-[11px]">{formatBS(item.precioBS)}</span>
                        </span>
                      </div>
                    ))}
                  </CardBody>
                </Card>
              ))}
              {menu.data?.secciones.length === 0 && <p className="text-sm text-muted">Menú no disponible.</p>}
            </div>
          )}
        </section>

        <section className="flex flex-col gap-6">
          <h2 className="text-xl font-semibold tracking-tightest">Próximos eventos</h2>
          <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
            {eventos.data?.map((evento) => (
              <Card key={evento.id} className="overflow-hidden">
                {evento.imagenUrl && (
                  <img src={evento.imagenUrl} alt={evento.titulo} className="h-40 w-full object-cover" />
                )}
                <CardBody>
                  <p className="text-sm font-semibold text-ink">{evento.titulo}</p>
                  <p className="mt-1 text-xs text-muted">{formatFecha(evento.fechaInicio)}</p>
                  <p className="mt-3 line-clamp-3 text-sm text-muted">{evento.descripcion}</p>
                </CardBody>
              </Card>
            ))}
            {eventos.data?.length === 0 && <p className="text-sm text-muted">No hay eventos publicados.</p>}
          </div>
        </section>
      </main>

      <footer className="border-t border-hairline py-8 text-center text-xs text-muted">
        Licorería · Discoteca — Plataforma web
      </footer>
    </div>
  );
}
