import type { ReactNode } from 'react';
import { lazy, Suspense, useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import {
  ArrowUpRight,
  CalendarDays,
  Clock,
  Facebook,
  Instagram,
  MapPin,
  MessageCircle,
  Wine,
} from 'lucide-react';
import { publicApi } from '@licoreria/api-client';
import type { Evento, MenuSeccion, Mesa, MetodoPago, Reserva } from '@licoreria/types';
import { BotanicalWatermark } from './components/BotanicalWatermark';
import { formatBS, formatFecha, formatUSD } from './lib/format';

const Bottle3D = lazy(() => import('./components/Bottle3D').then((m) => ({ default: m.Bottle3D })));

const NAV = [
  { href: '#carta', label: 'CARTA' },
  { href: '#agenda', label: 'AGENDA' },
  { href: '#hoy', label: 'TASA' },
  { href: '#reserva', label: 'RESERVA' },
];

function parseContacto(data?: Record<string, unknown>): { whatsapp: string | null; direccion: string | null } {
  if (!data) return { whatsapp: null, direccion: null };
  const w = data.whatsapp ?? data.whatsApp ?? data.telefono;
  const d = data.direccion ?? data.domicilio ?? data.ubicacion;
  return {
    whatsapp: typeof w === 'string' && w.length > 0 ? w : null,
    direccion: typeof d === 'string' && d.length > 0 ? d : null,
  };
}

function DottedRule() {
  return (
    <div className="mx-auto max-w-poster px-5 sm:px-8 lg:px-12">
      <hr className="rule-dotted" />
    </div>
  );
}

function Eyebrow({ children }: { children: ReactNode }) {
  return <span className="eyebrow">{children}</span>;
}

function NavBar({ whatsapp }: { whatsapp: string | null }) {
  return (
    <header className="sticky top-0 z-40 border-b border-cardamom-brown/50 bg-ember-rust/85 backdrop-blur-sm">
      <nav className="mx-auto flex max-w-poster items-center justify-between gap-4 px-5 py-3 sm:px-8 lg:px-12">
        <a href="#inicio" className="flex items-center gap-2">
          <span className="flex h-9 w-9 items-center justify-center rounded-button border border-tiger-gold text-tiger-gold">
            <Wine size={18} strokeWidth={1.5} />
          </span>
          <span className="font-display text-lg font-bold uppercase leading-none tracking-[0.02em] text-tiger-gold">
            Licorería
            <span className="block text-[11px] font-medium tracking-[0.32em] text-tiger-gold/70">DISCOTECA</span>
          </span>
        </a>

        <div className="hidden items-center gap-2 md:flex">
          {NAV.map((item) => (
            <a key={item.href} href={item.href} className="btn-ghost">
              {item.label}
            </a>
          ))}
        </div>

        <div className="flex items-center gap-2">
          {whatsapp && (
            <a
              href={`https://wa.me/${whatsapp.replace(/\D/g, '')}`}
              target="_blank"
              rel="noreferrer"
              aria-label="Escríbenos por WhatsApp"
              className="hidden h-10 w-10 items-center justify-center rounded-button border border-tiger-gold/70 text-tiger-gold transition-colors hover:bg-tiger-gold/10 sm:flex"
            >
              <MessageCircle size={18} strokeWidth={1.5} />
            </a>
          )}
          <a href="#reserva" className="btn-fill">
            RESERVAR
          </a>
        </div>
      </nav>
    </header>
  );
}

function Hero({ tasa }: { tasa: number | null }) {
  return (
    <section id="inicio" className="relative isolate overflow-hidden">
      <BotanicalWatermark variant="hero" />
      <div className="relative z-10 mx-auto flex min-h-[calc(100svh-64px)] max-w-poster flex-col px-5 pb-12 pt-10 sm:px-8 lg:px-12">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <Eyebrow>DESTILADOS DE AUTOR · FIESTA SIN FIN</Eyebrow>
          <span className="font-display text-[13px] font-medium uppercase tracking-[0.14em] text-tiger-gold/80">
            {tasa ? `Tasa del día · Bs ${tasa.toFixed(2)}` : 'Tasa del día · consulta en barra'}
          </span>
        </div>

        <div className="grid flex-1 items-center gap-6 py-8 lg:grid-cols-[1.12fr_0.88fr]">
          <div>
            <h1 className="display-type text-[clamp(3.75rem,12.5vw,12.1875rem)]">
              LA NOCHE
              <br />
              TIENE
              <br />
              SABOR
            </h1>
            <p className="mt-7 max-w-md text-[15px] leading-relaxed text-tiger-gold/75">
              La mejor selección de licores y la mejor fiesta de la ciudad, en un solo lugar. Carta viva, eventos y
              reservas a un toque.
            </p>
            <div className="mt-8 flex flex-wrap items-center gap-3">
              <a href="#carta" className="btn-fill">
                VER CARTA <ArrowUpRight size={15} strokeWidth={2} />
              </a>
              <a href="#reserva" className="btn-ghost">
                RESERVAR MESA
              </a>
            </div>
          </div>

          <div className="relative flex h-full min-h-[340px] items-center justify-center">
            <div
              className="pointer-events-none absolute inset-0"
              style={{
                background: 'radial-gradient(50% 50% at 50% 55%, rgba(40,16,6,0.85) 0%, rgba(130,53,19,0) 72%)',
              }}
            />
            <Suspense
              fallback={
                <div className="relative flex h-[46vh] min-h-[320px] w-full items-center justify-center lg:h-[68vh]">
                  <span className="eyebrow animate-pulse">Cargando botella…</span>
                </div>
              }
            >
              <Bottle3D className="relative h-[46vh] min-h-[320px] w-full lg:h-[68vh]" />
            </Suspense>
            <div className="pointer-events-none absolute bottom-0 left-1/2 -translate-x-1/2 text-center">
              <span className="tag-outline">VINO TINTO · CASTAÑO COLECCIÓN</span>
            </div>
          </div>
        </div>

        <div className="flex items-center gap-3 text-[11px] uppercase tracking-[0.28em] text-tiger-gold/60">
          <span className="h-px w-10 bg-tiger-gold/60" /> Desliza para descubrir
        </div>
      </div>
    </section>
  );
}

function Statement() {
  return (
    <section className="relative isolate overflow-hidden py-20 sm:py-24">
      <BotanicalWatermark variant="left" />
      <div className="relative z-10 mx-auto grid max-w-poster gap-10 px-5 sm:px-8 lg:grid-cols-[1.25fr_0.75fr] lg:px-12">
        <h2 className="display-type text-[clamp(2.5rem,7vw,6.3125rem)]">
          UN SORBO AL DÍA,
          <br />
          UNA NOCHE A LA VEZ
        </h2>
        <div className="flex flex-col justify-end gap-6">
          <p className="max-w-md text-subheading text-tiger-gold/80">
            Nacimos como licorería y crecimos como pista de baile. Curamos botellas para el coleccionista y cocteles
            para el que solo quiere una buena noche.
          </p>
          <div className="flex flex-wrap gap-2.5">
            <span className="tag-outline">DESTILADOS</span>
            <span className="tag-outline">COCTELERÍA</span>
            <span className="tag-outline">VINOS</span>
            <span className="tag-outline">CERVEZAS</span>
            <span className="tag-heat">PICANTE</span>
          </div>
        </div>
      </div>
    </section>
  );
}

function CartaSection({ secciones, cargando, error }: { secciones: MenuSeccion[]; cargando: boolean; error: boolean }) {
  return (
    <section id="carta" className="relative isolate py-20 sm:py-24">
      <div className="mx-auto max-w-poster px-5 sm:px-8 lg:px-12">
        <div className="flex flex-wrap items-end justify-between gap-6">
          <div>
            <Eyebrow>CARTA · MENÚ DIGITAL</Eyebrow>
            <h2 className="display-type mt-3 text-[clamp(2.75rem,8vw,7.5rem)]">LA CARTA</h2>
          </div>
          <p className="max-w-sm text-[13px] leading-relaxed text-tiger-gold/70">
            Precios en dólares con su equivalente en bolívares según la tasa del día. Disponibilidad sujeta a
            inventario.
          </p>
        </div>

        <div className="mt-12 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
          {cargando &&
            Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="h-64 animate-pulse rounded-card bg-dark-spice" />
            ))}

          {!cargando &&
            secciones.map((seccion) => (
              <article key={seccion.categoriaId} className="rounded-card bg-dark-spice p-5">
                <div className="flex items-baseline justify-between gap-3 border-b border-dotted border-cardamom-brown pb-3">
                  <h3 className="font-display text-2xl font-semibold uppercase tracking-[0.02em] text-tiger-gold">
                    {seccion.nombre}
                  </h3>
                  <span className="text-[11px] uppercase tracking-[0.2em] text-tiger-gold/50">
                    {seccion.items.length} ítems
                  </span>
                </div>
                <ul className="mt-2 flex flex-col">
                  {seccion.items.map((item, index) => (
                    <li
                      key={item.varianteId}
                      className={`flex items-center justify-between gap-4 py-2.5 ${
                        index < seccion.items.length - 1 ? 'border-b border-dotted border-cardamom-brown/60' : ''
                      }`}
                    >
                      <span className="text-[13px] text-tiger-gold">{item.nombre}</span>
                      <span className="shrink-0 text-right">
                        <span className="block font-display text-base font-semibold text-tiger-gold">
                          {formatUSD(item.precioUSD)}
                        </span>
                        <span className="block text-[11px] text-tiger-gold/60">{formatBS(item.precioBS)}</span>
                      </span>
                    </li>
                  ))}
                </ul>
              </article>
            ))}

          {!cargando && !error && secciones.length === 0 && (
            <p className="text-subheading text-tiger-gold/70">La carta se está actualizando. Vuelve en un momento.</p>
          )}

          {!cargando && error && (
            <p className="text-subheading text-tiger-gold/70">No pudimos cargar la carta. Intenta de nuevo más tarde.</p>
          )}
        </div>
      </div>
    </section>
  );
}

function EventosSection({ eventos, cargando }: { eventos: Evento[]; cargando: boolean }) {
  return (
    <section id="agenda" className="relative isolate overflow-hidden py-20 sm:py-24">
      <BotanicalWatermark variant="right" />
      <div className="relative z-10 mx-auto max-w-poster px-5 sm:px-8 lg:px-12">
        <div className="flex flex-wrap items-end justify-between gap-6">
          <div>
            <Eyebrow>AGENDA · PRÓXIMAS NOCHES</Eyebrow>
            <h2 className="display-type mt-3 text-[clamp(2.75rem,8vw,7.5rem)]">
              LA AGENDA
            </h2>
          </div>
          <span className="tag-outline">
            <CalendarDays size={13} className="mr-2" strokeWidth={1.8} /> {eventos.length}{' '}
            {eventos.length === 1 ? 'EVENTO' : 'EVENTOS'}
          </span>
        </div>

        <div className="mt-12 grid gap-5 md:grid-cols-2 xl:grid-cols-3">
          {cargando &&
            Array.from({ length: 3 }).map((_, i) => (
              <div key={i} className="h-72 animate-pulse rounded-card bg-dark-spice" />
            ))}

          {!cargando &&
            eventos.map((evento) => (
              <article key={evento.id} className="overflow-hidden rounded-card bg-dark-spice">
                {evento.imagenUrl ? (
                  <img src={evento.imagenUrl} alt={evento.titulo} className="h-44 w-full object-cover" loading="lazy" />
                ) : (
                  <div className="flex h-44 w-full items-center justify-center bg-charred-clove">
                    <span className="display-type text-[64px] leading-none">{evento.titulo.charAt(0)}</span>
                  </div>
                )}
                <div className="flex flex-col gap-3 p-5">
                  <span className="eyebrow">{formatFecha(evento.fechaInicio)}</span>
                  <h3 className="font-display text-2xl font-semibold uppercase leading-tight tracking-[0.01em] text-tiger-gold">
                    {evento.titulo}
                  </h3>
                  <p className="line-clamp-3 text-[13px] leading-relaxed text-tiger-gold/70">{evento.descripcion}</p>
                </div>
              </article>
            ))}

          {!cargando && eventos.length === 0 && (
            <p className="text-subheading text-tiger-gold/70">Aún no hay eventos publicados. Vuelve pronto.</p>
          )}
        </div>
      </div>
    </section>
  );
}

function HoySection({
  tasa,
  actualizada,
  cargando,
  contacto,
}: {
  tasa: number | null;
  actualizada: string | null;
  cargando: boolean;
  contacto: ReturnType<typeof parseContacto>;
}) {
  return (
    <section id="hoy" className="relative isolate border-y border-cardamom-brown/50 py-20 sm:py-24">
      <BotanicalWatermark variant="band" />
      <div className="relative z-10 mx-auto grid max-w-poster gap-10 px-5 sm:px-8 lg:grid-cols-[1fr_1fr] lg:px-12">
        <div>
          <Eyebrow>TASA DEL DÍA · PARALELO</Eyebrow>
          {cargando ? (
            <div className="mt-4 h-24 w-64 animate-pulse rounded-card bg-dark-spice" />
          ) : (
            <p className="display-type mt-3 pb-5 text-[clamp(3.5rem,10vw,9rem)]">{formatBS(tasa ?? 0)}</p>
          )}
          <p className="mt-3 flex items-center gap-2 text-[12px] uppercase tracking-[0.18em] text-tiger-gold/60">
            <Clock size={14} strokeWidth={1.6} />
            {actualizada ? `Actualizada ${actualizada}` : 'Sin actualizar'}
          </p>
        </div>

        <div className="flex flex-col justify-center gap-5">
          <p className="max-w-md text-subheading text-tiger-gold/80">
            Una sola sucursal, una sola obsesión: que la noche salga perfecta. Escríbenos para reservar, pedir la carta
            o apartar tu botella.
          </p>
          <div className="flex flex-wrap gap-3">
            {contacto.whatsapp && (
              <a
                href={`https://wa.me/${contacto.whatsapp.replace(/\D/g, '')}`}
                target="_blank"
                rel="noreferrer"
                className="btn-fill"
              >
                <MessageCircle size={15} strokeWidth={1.8} /> WHATSAPP
              </a>
            )}
            {contacto.direccion && (
              <span className="btn-ghost">
                <MapPin size={15} strokeWidth={1.8} /> {contacto.direccion}
              </span>
            )}
          </div>
        </div>
      </div>
    </section>
  );
}

function ReservaSection({ whatsapp }: { whatsapp: string | null }) {
  const mesas = useQuery({ queryKey: ['reservar-mesas'], queryFn: publicApi.mesas });
  const metodos = useQuery({ queryKey: ['reservar-metodos'], queryFn: publicApi.metodosPago });

  const [fecha, setFecha] = useState('');
  const [personas, setPersonas] = useState('2');
  const [seleccion, setSeleccion] = useState<string[]>([]);
  const [nombre, setNombre] = useState('');
  const [telefono, setTelefono] = useState('');
  const [notas, setNotas] = useState('');
  const [conSena, setConSena] = useState(false);
  const [metodoPagoId, setMetodoPagoId] = useState('');
  const [monto, setMonto] = useState('');
  const [creada, setCreada] = useState<Reserva | null>(null);

  const toggleMesa = (id: string) =>
    setSeleccion((actuales) => (actuales.includes(id) ? actuales.filter((x) => x !== id) : [...actuales, id]));

  const porZona = (mesas.data ?? []).reduce<Record<string, Mesa[]>>((acc, mesa) => {
    (acc[mesa.zonaNombre] ??= []).push(mesa);
    return acc;
  }, {});

  const reservar = useMutation({
    mutationFn: async () => {
      const reserva = await publicApi.crearReserva({
        fechaHora: new Date(fecha).toISOString(),
        personas: Number(personas),
        mesas: seleccion,
        nombreContacto: nombre,
        telefono,
        origen: 'Web',
        notas: notas || null,
      });
      if (conSena && metodoPagoId && Number(monto) > 0) {
        await publicApi.registrarPagoReserva(reserva.id, {
          metodoPagoId,
          monto: Number(monto),
          moneda: 'USD',
          comprobanteUrl: null,
        });
      }
      return reserva;
    },
    onSuccess: (reserva) => setCreada(reserva),
  });

  const puedeEnviar = fecha && seleccion.length > 0 && nombre.trim().length >= 2 && telefono.trim().length >= 6;

  return (
    <section id="reserva" className="relative isolate overflow-hidden py-24 sm:py-32">
      <BotanicalWatermark variant="left" />
      <div className="relative z-10 mx-auto max-w-poster px-5 sm:px-8 lg:px-12">
        <div className="text-center">
          <Eyebrow>MESAS VIP · CUMPLEAÑOS · EVENTOS PRIVADOS</Eyebrow>
          <h2 className="display-type mx-auto mt-4 text-[clamp(2.75rem,9vw,8.75rem)]">
            RESERVA
            <br />
            TU MESA
          </h2>
          <p className="mx-auto mt-6 max-w-xl text-[15px] leading-relaxed text-tiger-gold/75">
            Elige tus mesas en el plano, confirma con seña y llega directo a la mejor ubicación.
          </p>
        </div>

        {creada ? (
          <div className="mx-auto mt-12 max-w-xl rounded-card bg-dark-spice p-8 text-center">
            <p className="display-type text-[clamp(2rem,6vw,3.5rem)]">¡RESERVADA!</p>
            <p className="mt-4 text-[14px] text-tiger-gold/80">
              Te esperamos el {formatFecha(creada.fechaHora)} para {creada.personas} personas.
            </p>
            <p className="mt-2 text-[13px] text-tiger-gold/60">
              Mesas: {creada.mesas.map((mesa) => mesa.numero).join(', ') || 'por asignar'}
            </p>
            {whatsapp && (
              <a
                href={`https://wa.me/${whatsapp.replace(/\D/g, '')}?text=${encodeURIComponent(
                  `Hola, reservé a nombre de ${creada.nombreContacto}. Quisiera coordinar la seña.`,
                )}`}
                target="_blank"
                rel="noreferrer"
                className="btn-fill mt-6 inline-flex"
              >
                CONFIRMAR POR WHATSAPP <ArrowUpRight size={15} strokeWidth={2} />
              </a>
            )}
          </div>
        ) : (
          <div className="mx-auto mt-12 flex max-w-3xl flex-col gap-5 rounded-card bg-dark-spice p-6 sm:p-8">
            <div className="grid gap-4 sm:grid-cols-2">
              <label className="flex flex-col gap-1.5">
                <span className="eyebrow">Fecha y hora</span>
                <input
                  type="datetime-local"
                  value={fecha}
                  onChange={(evento) => setFecha(evento.target.value)}
                  className="rounded-button border border-cardamom-brown bg-charred-clove px-4 py-2 text-[14px] text-tiger-gold"
                />
              </label>
              <label className="flex flex-col gap-1.5">
                <span className="eyebrow">Personas</span>
                <input
                  type="number"
                  min={1}
                  value={personas}
                  onChange={(evento) => setPersonas(evento.target.value)}
                  className="rounded-button border border-cardamom-brown bg-charred-clove px-4 py-2 text-[14px] text-tiger-gold"
                />
              </label>
            </div>

            <div>
              <span className="eyebrow">Mesas disponibles</span>
              <div className="mt-2 flex flex-col gap-3">
                {Object.entries(porZona).map(([zona, mesasZona]) => (
                  <div key={zona}>
                    <p className="text-[11px] uppercase tracking-[0.2em] text-tiger-gold/50">{zona}</p>
                    <div className="mt-1.5 flex flex-wrap gap-2">
                      {mesasZona.map((mesa) => {
                        const activa = seleccion.includes(mesa.id);
                        return (
                          <button
                            key={mesa.id}
                            type="button"
                            onClick={() => toggleMesa(mesa.id)}
                            aria-pressed={activa}
                            className={`rounded-button border px-3 py-1.5 text-[12px] uppercase tracking-[0.1em] transition ${
                              activa
                                ? 'border-tiger-gold bg-tiger-gold text-charred-clove'
                                : 'border-tiger-gold/50 text-tiger-gold/80 hover:border-tiger-gold'
                            }`}
                          >
                            {mesa.numero} · {mesa.capacidad}p
                          </button>
                        );
                      })}
                    </div>
                  </div>
                ))}
                {(mesas.data?.length ?? 0) === 0 && (
                  <p className="text-[13px] text-tiger-gold/60">Consultamos disponibilidad por WhatsApp.</p>
                )}
              </div>
            </div>

            <div className="grid gap-4 sm:grid-cols-2">
              <label className="flex flex-col gap-1.5">
                <span className="eyebrow">Nombre</span>
                <input
                  value={nombre}
                  onChange={(evento) => setNombre(evento.target.value)}
                  className="rounded-button border border-cardamom-brown bg-charred-clove px-4 py-2 text-[14px] text-tiger-gold"
                />
              </label>
              <label className="flex flex-col gap-1.5">
                <span className="eyebrow">Teléfono</span>
                <input
                  value={telefono}
                  onChange={(evento) => setTelefono(evento.target.value)}
                  className="rounded-button border border-cardamom-brown bg-charred-clove px-4 py-2 text-[14px] text-tiger-gold"
                />
              </label>
            </div>

            <label className="flex flex-col gap-1.5">
              <span className="eyebrow">Notas (opcional)</span>
              <input
                value={notas}
                onChange={(evento) => setNotas(evento.target.value)}
                className="rounded-button border border-cardamom-brown bg-charred-clove px-4 py-2 text-[14px] text-tiger-gold"
              />
            </label>

            <div className="rounded-card border border-dotted border-cardamom-brown p-4">
              <label className="flex items-center gap-2 text-[13px] text-tiger-gold/80">
                <input type="checkbox" checked={conSena} onChange={(evento) => setConSena(evento.target.checked)} />
                Pagar seña ahora
              </label>
              {conSena && (
                <div className="mt-3 grid gap-3 sm:grid-cols-2">
                  <label className="flex flex-col gap-1.5">
                    <span className="eyebrow">Método</span>
                    <select
                      value={metodoPagoId}
                      onChange={(evento) => setMetodoPagoId(evento.target.value)}
                      className="rounded-button border border-cardamom-brown bg-charred-clove px-4 py-2 text-[14px] text-tiger-gold"
                    >
                      <option value="">Selecciona…</option>
                      {metodos.data?.map((metodo: MetodoPago) => (
                        <option key={metodo.id} value={metodo.id}>
                          {metodo.nombre}
                        </option>
                      ))}
                    </select>
                  </label>
                  <label className="flex flex-col gap-1.5">
                    <span className="eyebrow">Monto (USD)</span>
                    <input
                      type="number"
                      min={0}
                      value={monto}
                      onChange={(evento) => setMonto(evento.target.value)}
                      className="rounded-button border border-cardamom-brown bg-charred-clove px-4 py-2 text-[14px] text-tiger-gold"
                    />
                  </label>
                </div>
              )}
            </div>

            {reservar.isError && (
              <p className="text-[13px] text-chili-red">
                No pudimos registrar la reserva. Verifica los datos o escríbenos por WhatsApp.
              </p>
            )}

            <button
              type="button"
              disabled={!puedeEnviar || reservar.isPending}
              onClick={() => reservar.mutate()}
              className="btn-fill self-start disabled:opacity-40"
            >
              {reservar.isPending ? 'ENVIANDO…' : 'RESERVAR MESA'} <ArrowUpRight size={15} strokeWidth={2} />
            </button>
          </div>
        )}
      </div>
    </section>
  );
}

function Footer({ whatsapp }: { whatsapp: string | null }) {
  return (
    <footer className="relative isolate border-t border-cardamom-brown/50 py-14">
      <div className="mx-auto flex max-w-poster flex-col gap-8 px-5 sm:px-8 lg:px-12">
        <div className="flex flex-wrap items-start justify-between gap-8">
          <div className="max-w-xs">
            <p className="font-display text-2xl font-bold uppercase text-tiger-gold">Licorería · Discoteca</p>
            <p className="mt-2 text-[13px] leading-relaxed text-tiger-gold/65">
              Destilados de autor, coctelería y la mejor noche de la ciudad.
            </p>
          </div>
          <nav className="flex flex-col gap-2 text-[13px]">
            {NAV.map((item) => (
              <a key={item.href} href={item.href} className="text-tiger-gold/75 transition hover:text-tiger-gold">
                {item.label}
              </a>
            ))}
          </nav>
          <div className="flex gap-2">
            {[Instagram, Facebook, MessageCircle].map((Icon, i) => {
              const href =
                i === 2 && whatsapp ? `https://wa.me/${whatsapp.replace(/\D/g, '')}` : '#inicio';
              return (
                <a
                  key={i}
                  href={href}
                  target={i === 2 && whatsapp ? '_blank' : undefined}
                  rel="noreferrer"
                  aria-label="Red social"
                  className="flex h-10 w-10 items-center justify-center rounded-button border border-tiger-gold/60 text-tiger-gold transition-colors hover:bg-tiger-gold/10"
                >
                  <Icon size={17} strokeWidth={1.5} />
                </a>
              );
            })}
          </div>
        </div>

        <div className="flex flex-col gap-2 border-t border-dotted border-cardamom-brown pt-6 text-[11px] uppercase tracking-[0.16em] text-tiger-gold/45 md:flex-row md:items-center md:justify-between">
          <span>© {new Date().getFullYear()} Licorería · Discoteca. Todos los derechos reservados.</span>
          <span>Modelo 3D «Vino Tinto Castaño Colección» · anaa_ggarcia · CC BY 4.0</span>
        </div>
      </div>
    </footer>
  );
}

function FloatingActions({ whatsapp }: { whatsapp: string | null }) {
  return (
    <div className="fixed bottom-5 right-5 z-40 flex flex-col gap-2">
      {whatsapp && (
        <a
          href={`https://wa.me/${whatsapp.replace(/\D/g, '')}`}
          target="_blank"
          rel="noreferrer"
          aria-label="WhatsApp"
          className="flex h-12 w-12 items-center justify-center rounded-button border border-tiger-gold bg-charred-clove text-tiger-gold transition-colors hover:bg-dark-spice"
        >
          <MessageCircle size={20} strokeWidth={1.6} />
        </a>
      )}
      <a
        href="#inicio"
        aria-label="Volver arriba"
        className="flex h-12 w-12 items-center justify-center rounded-button border border-tiger-gold bg-tiger-gold text-charred-clove transition-opacity hover:opacity-85"
      >
        <ArrowUpRight size={20} strokeWidth={2} className="-rotate-45" />
      </a>
    </div>
  );
}

export default function App() {
  const tasa = useQuery({ queryKey: ['tasa'], queryFn: () => publicApi.tasaActual('Paralelo') });
  const menu = useQuery({ queryKey: ['menu-digital'], queryFn: publicApi.menuDigital });
  const eventos = useQuery({ queryKey: ['eventos'], queryFn: publicApi.eventos });
  const local = useQuery({ queryKey: ['local-info'], queryFn: publicApi.localInfo });

  const contacto = parseContacto(local.data);

  return (
    <div className="min-h-dvh bg-ember-rust text-tiger-gold">
      <NavBar whatsapp={contacto.whatsapp} />
      <main>
        <Hero tasa={tasa.data?.valor ?? null} />
        <DottedRule />
        <Statement />
        <DottedRule />
        <CartaSection
          secciones={menu.data?.secciones ?? []}
          cargando={menu.isLoading}
          error={menu.isError}
        />
        <DottedRule />
        <EventosSection eventos={eventos.data ?? []} cargando={eventos.isLoading} />
        <DottedRule />
        <HoySection
          tasa={tasa.data?.valor ?? null}
          actualizada={tasa.data?.fecha ? formatFecha(tasa.data.fecha) : null}
          cargando={tasa.isLoading}
          contacto={contacto}
        />
        <ReservaSection whatsapp={contacto.whatsapp} />
      </main>
      <Footer whatsapp={contacto.whatsapp} />
      <FloatingActions whatsapp={contacto.whatsapp} />
    </div>
  );
}
