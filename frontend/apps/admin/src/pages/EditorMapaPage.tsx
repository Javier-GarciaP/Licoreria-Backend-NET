import { useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { toast } from 'sonner';
import {
  ArrowLeft,
  Check,
  Grid3x3,
  Hand,
  Magnet,
  Maximize,
  MousePointer2,
  Redo2,
  Save,
  Undo2,
  ZoomIn,
  ZoomOut,
} from 'lucide-react';
import { Button, cn, Skeleton } from '@licoreria/ui';
import type { Plano, PlanoElemento, Zona } from '@licoreria/types';
import { clubApi } from '@licoreria/api-client';
import { dibujarElemento, ELEMENTOS, ELEMENTOS_POR_FORMA, UNIT } from '../components/mapa/elementos';
import { MapaView } from '../components/mapa/MapaView';
import { EditorOverlay } from '../components/mapa/EditorOverlay';
import { mensajeDeError } from '../lib/api';
import { planoAPayload } from '../lib/plano';

const OFFSET = 16;

interface Borrador {
  nombre: string;
  activo: boolean;
  anchoFondo: number;
  altoFondo: number;
  rejilla: number;
  piso: string;
  elementos: PlanoElemento[];
}

interface Gesto {
  tipo: 'mover' | 'pan' | 'resize' | 'rotate';
  x0: number;
  y0: number;
  base: Borrador;
  id?: string;
  movido?: boolean;
  centroX: number;
  centroY: number;
  rot: number;
  baseEl?: PlanoElemento;
  domNode?: SVGGElement | null;
  ultimo?: { x: number; y: number };
}

type Herramienta = 'seleccionar' | 'mano';

const PISOS = ['madera', 'cemento', 'neon', 'claro'];
let contador = 0;
const nuevoId = () => `nuevo-${Date.now()}-${contador++}`;
const snap = (valor: number, paso: number) => Math.round(valor / paso) * paso;

function desdePlano(plano: Plano): Borrador {
  return {
    nombre: plano.nombre,
    activo: plano.activo,
    anchoFondo: plano.anchoFondo,
    altoFondo: plano.altoFondo,
    rejilla: plano.rejilla,
    piso: plano.piso,
    elementos: plano.elementos.map((e) => ({ ...e })),
  };
}

function Miniatura({ forma, color }: { forma: string; color: string }) {
  return (
    <svg width={26} height={26} viewBox="0 0 26 26" aria-hidden>
      {dibujarElemento(forma, 26, 26, color)}
    </svg>
  );
}

export function EditorMapaPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [draft, setDraft] = useState<Borrador | null>(null);
  const [seleccion, setSeleccion] = useState<string | null>(null);
  const [inspectorAbierto, setInspectorAbierto] = useState(false);
  const [zoom, setZoom] = useState(1);
  const [pan, setPan] = useState({ x: 0, y: 0 });
  const [past, setPast] = useState<Borrador[]>([]);
  const [future, setFuture] = useState<Borrador[]>([]);
  const [herramienta, setHerramienta] = useState<Herramienta>('seleccionar');
  const [colocandoForma, setColocandoForma] = useState<string | null>(null);
  const [rejillaActiva, setRejillaActiva] = useState(true);
  const [snapActivo, setSnapActivo] = useState(true);
  const [coords, setCoords] = useState({ x: 0, y: 0 });
  const [dirty, setDirty] = useState(false);

  const gesto = useRef<Gesto | null>(null);
  const wrapper = useRef<HTMLDivElement>(null);
  const overlayRef = useRef<HTMLDivElement>(null);
  const frame = useRef<number | null>(null);
  const frameCoords = useRef<number | null>(null);
  const pendiente = useRef<Borrador | null>(null);

  const planos = useQuery({ queryKey: ['planos'], queryFn: clubApi.planos });
  const zonas = useQuery({ queryKey: ['zonas'], queryFn: clubApi.zonas });
  const mesas = useQuery({ queryKey: ['mesas'], queryFn: () => clubApi.mesas() });

  const planoBase = planos.data?.find((p) => p.id === id);

  useEffect(() => {
    if (planoBase) setDraft(desdePlano(planoBase));
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [planoBase?.id]);

  const commit = (siguiente: Borrador) => {
    setDraft((actual) => {
      if (actual) setPast((p) => [...p.slice(-49), actual]);
      return siguiente;
    });
    setFuture([]);
    setDirty(true);
  };

  const undo = () =>
    setPast((p) => {
      if (p.length === 0) return p;
      const prev = p[p.length - 1];
      setDraft((actual) => {
        if (actual) setFuture((f) => [actual, ...f.slice(0, 49)]);
        return prev;
      });
      setDirty(true);
      return p.slice(0, -1);
    });

  const redo = () =>
    setFuture((f) => {
      if (f.length === 0) return f;
      const next = f[0];
      setDraft((actual) => {
        if (actual) setPast((p) => [...p, actual]);
        return next;
      });
      setDirty(true);
      return f.slice(1);
    });

  const guardar = useMutation({
    mutationFn: () => {
      if (!draft || !id) throw new Error('Nada que guardar');
      return clubApi.actualizarPlano(id, planoAPayload({ ...draft, id }));
    },
    onSuccess: (plano: Plano) => {
      toast.success('Mapa guardado');
      if (plano.elementos.length > 0) setDraft(desdePlano(plano));
      setPast([]);
      setFuture([]);
      setSeleccion(null);
      setDirty(false);
      queryClient.invalidateQueries({ queryKey: ['planos'] });
    },
    onError: (error) => toast.error('No se pudo guardar', { description: mensajeDeError(error) }),
  });

  // ===== Elementos =====

  const agregar = (forma: string, posicion?: { x: number; y: number }) => {
    if (!draft) return;
    const def = ELEMENTOS_POR_FORMA[forma];
    const elId = nuevoId();
    const px = posicion ? posicion.x - def.w / 2 : draft.anchoFondo / 2 - def.w / 2;
    const py = posicion ? posicion.y - def.h / 2 : draft.altoFondo / 2 - def.h / 2;
    const elemento: PlanoElemento = {
      id: elId,
      zonaId: null,
      mesaId: null,
      tipo: forma.startsWith('mesa') ? 'mesa' : forma,
      forma,
      color: null,
      etiqueta: null,
      z: draft.elementos.length,
      posX: Math.max(0, snapActivo ? snap(px, draft.rejilla) : px),
      posY: Math.max(0, snapActivo ? snap(py, draft.rejilla) : py),
      ancho: def.w,
      alto: def.h,
      rotacion: 0,
    };
    commit({ ...draft, elementos: [...draft.elementos, elemento] });
    setSeleccion(elId);
  };

  const actualizarElemento = (elId: string, cambios: Partial<PlanoElemento>, historial = true) => {
    if (!draft) return;
    const siguiente = { ...draft, elementos: draft.elementos.map((e) => (e.id === elId ? { ...e, ...cambios } : e)) };
    if (historial) commit(siguiente);
    else {
      setDraft(siguiente);
      setDirty(true);
    }
  };

  const eliminarElemento = (elId: string) => {
    if (!draft) return;
    commit({ ...draft, elementos: draft.elementos.filter((e) => e.id !== elId) });
    setSeleccion(null);
    setInspectorAbierto(false);
  };

  const duplicarElemento = (elId: string) => {
    if (!draft) return;
    const original = draft.elementos.find((e) => e.id === elId);
    if (!original) return;
    const copia: PlanoElemento = { ...original, id: nuevoId(), posX: original.posX + 1, posY: original.posY + 1, z: draft.elementos.length };
    commit({ ...draft, elementos: [...draft.elementos, copia] });
    setSeleccion(copia.id);
  };

  const cambiarZ = (elId: string, delta: number) => {
    if (!draft) return;
    const actual = draft.elementos.find((e) => e.id === elId);
    if (!actual) return;
    const nuevo = Math.max(0, Math.min(draft.elementos.length - 1, actual.z + delta));
    commit({ ...draft, elementos: draft.elementos.map((e) => (e.id === elId ? { ...e, z: nuevo } : e)) });
  };

  // ===== Gestos =====

  const aGrid = (clientX: number, clientY: number) => {
    const rect = wrapper.current?.getBoundingClientRect();
    const x = (clientX - (rect?.left ?? 0) - OFFSET - pan.x) / (UNIT * zoom);
    const y = (clientY - (rect?.top ?? 0) - OFFSET - pan.y) / (UNIT * zoom);
    return { x, y };
  };

  const sn = (valor: number) => (snapActivo && draft ? snap(valor, draft.rejilla) : valor);

  const aplicar = (borrador: Borrador) => {
    pendiente.current = borrador;
    if (frame.current != null) return;
    frame.current = requestAnimationFrame(() => {
      frame.current = null;
      const valor = pendiente.current;
      pendiente.current = null;
      if (valor) {
        setDraft(valor);
        setDirty(true);
      }
    });
  };

  const iniciarMover = (evento: React.PointerEvent, elemento: PlanoElemento) => {
    if (!draft) return;
    evento.stopPropagation();
    setSeleccion(elemento.id);
    const domNode = wrapper.current?.querySelector<SVGGElement>(`g[data-el="${elemento.id}"]`) ?? null;
    gesto.current = {
      tipo: 'mover',
      x0: evento.clientX,
      y0: evento.clientY,
      base: draft,
      id: elemento.id,
      movido: false,
      centroX: 0,
      centroY: 0,
      rot: 0,
      baseEl: elemento,
      domNode,
      ultimo: { x: elemento.posX, y: elemento.posY },
    };
    wrapper.current?.setPointerCapture(evento.pointerId);
  };
  const iniciarMoverRef = useRef(iniciarMover);
  iniciarMoverRef.current = iniciarMover;
  const alPointerElemento = useCallback(
    (evento: React.PointerEvent, elemento: PlanoElemento) => iniciarMoverRef.current(evento, elemento),
    [],
  );

  const iniciarResize = (evento: React.PointerEvent) => {
    if (!draft || !seleccion) return;
    evento.stopPropagation();
    const el = draft.elementos.find((e) => e.id === seleccion);
    if (!el) return;
    gesto.current = {
      tipo: 'resize',
      x0: evento.clientX,
      y0: evento.clientY,
      base: draft,
      id: el.id,
      movido: false,
      centroX: el.posX + el.ancho / 2,
      centroY: el.posY + el.alto / 2,
      rot: el.rotacion,
      baseEl: el,
    };
    wrapper.current?.setPointerCapture(evento.pointerId);
  };

  const iniciarRotate = (evento: React.PointerEvent) => {
    if (!draft || !seleccion) return;
    evento.stopPropagation();
    const el = draft.elementos.find((e) => e.id === seleccion);
    if (!el) return;
    gesto.current = {
      tipo: 'rotate',
      x0: evento.clientX,
      y0: evento.clientY,
      base: draft,
      id: el.id,
      movido: false,
      centroX: el.posX + el.ancho / 2,
      centroY: el.posY + el.alto / 2,
      rot: el.rotacion,
      baseEl: el,
    };
    wrapper.current?.setPointerCapture(evento.pointerId);
  };

  const iniciarPan = (evento: React.PointerEvent) => {
    setSeleccion(null);
    setInspectorAbierto(false);
    gesto.current = { tipo: 'pan', x0: evento.clientX, y0: evento.clientY, base: draft ?? ({} as Borrador), centroX: 0, centroY: 0, rot: 0 };
    wrapper.current?.setPointerCapture(evento.pointerId);
  };

  const alFondo = (evento: React.PointerEvent) => {
    if (colocandoForma) {
      const punto = aGrid(evento.clientX, evento.clientY);
      agregar(colocandoForma, punto);
      return;
    }
    iniciarPan(evento);
  };

  const mover = (evento: React.PointerEvent) => {
    const g = gesto.current;
    if (!g || !draft) return;
    const dx = evento.clientX - g.x0;
    const dy = evento.clientY - g.y0;

    if (g.tipo === 'pan') {
      setPan((p) => ({ x: p.x + dx, y: p.y + dy }));
      g.x0 = evento.clientX;
      g.y0 = evento.clientY;
      return;
    }

    if (!g.id || !g.baseEl) return;
    const baseEl = g.baseEl;

    if (g.tipo === 'mover') {
      if (Math.abs(dx) + Math.abs(dy) < 2) return;
      g.movido = true;
      const nuevaX = Math.max(0, sn(baseEl.posX + dx / (UNIT * zoom)));
      const nuevaY = Math.max(0, sn(baseEl.posY + dy / (UNIT * zoom)));
      g.ultimo = { x: nuevaX, y: nuevaY };
      g.domNode?.setAttribute('transform', `translate(${nuevaX * UNIT} ${nuevaY * UNIT})`);
      if (overlayRef.current) {
        overlayRef.current.style.transform = `translate(${(nuevaX - baseEl.posX) * UNIT * zoom}px, ${(nuevaY - baseEl.posY) * UNIT * zoom}px)`;
      }
      return;
    }

    const punto = aGrid(evento.clientX, evento.clientY);
    const relX = punto.x - g.centroX;
    const relY = punto.y - g.centroY;
    const rad = (-g.rot * Math.PI) / 180;
    const lx = relX * Math.cos(rad) - relY * Math.sin(rad);
    const ly = relX * Math.sin(rad) + relY * Math.cos(rad);

    if (g.tipo === 'resize') {
      const paso = draft.rejilla;
      const nuevoW = Math.max(paso, snapActivo ? snap(Math.abs(lx) * 2, paso) : Math.abs(lx) * 2);
      const nuevoH = Math.max(paso, snapActivo ? snap(Math.abs(ly) * 2, paso) : Math.abs(ly) * 2);
      g.movido = true;
      aplicar({
        ...draft,
        elementos: draft.elementos.map((e) =>
          e.id === g.id ? { ...e, ancho: nuevoW, alto: nuevoH, posX: g.centroX - nuevoW / 2, posY: g.centroY - nuevoH / 2 } : e,
        ),
      });
      return;
    }

    if (g.tipo === 'rotate') {
      g.movido = true;
      let angulo = (Math.atan2(relY, relX) * 180) / Math.PI + 90;
      if (!evento.shiftKey) angulo = Math.round(angulo / 15) * 15;
      angulo = ((angulo % 360) + 360) % 360;
      aplicar({ ...draft, elementos: draft.elementos.map((e) => (e.id === g.id ? { ...e, rotacion: angulo } : e)) });
    }
  };

  const actualizarCoords = (evento: React.PointerEvent) => {
    if (frameCoords.current != null) return;
    const { clientX, clientY } = evento;
    frameCoords.current = requestAnimationFrame(() => {
      frameCoords.current = null;
      const punto = aGrid(clientX, clientY);
      setCoords({ x: +punto.x.toFixed(1), y: +punto.y.toFixed(1) });
    });
  };

  const terminar = (evento: React.PointerEvent) => {
    const g = gesto.current;
    gesto.current = null;
    wrapper.current?.releasePointerCapture?.(evento.pointerId);
    if (frame.current != null) {
      cancelAnimationFrame(frame.current);
      frame.current = null;
    }
    if (!g || g.tipo === 'pan') {
      pendiente.current = null;
      return;
    }
    if (g.tipo === 'mover' && g.movido && g.ultimo && g.id) {
      const { x, y } = g.ultimo;
      setDraft({ ...g.base, elementos: g.base.elementos.map((e) => (e.id === g.id ? { ...e, posX: x, posY: y } : e)) });
    } else if (pendiente.current) {
      setDraft(pendiente.current);
    }
    pendiente.current = null;
    if (overlayRef.current) overlayRef.current.style.transform = '';
    if (g.movido) {
      setPast((p) => [...p.slice(-49), g.base]);
      setFuture([]);
      setDirty(true);
    }
  };

  const ajustar = () => {
    if (!draft) return;
    const w = wrapper.current?.clientWidth ?? 0;
    const h = wrapper.current?.clientHeight ?? 0;
    const anchoMapa = draft.anchoFondo * UNIT;
    const altoMapa = draft.altoFondo * UNIT;
    const z = Math.min(2, Math.max(0.4, Math.min((w - 40) / anchoMapa, (h - 40) / altoMapa)));
    setZoom(+z.toFixed(2));
    setPan({ x: 0, y: 0 });
  };

  useEffect(() => {
    const alTeclear = (evento: KeyboardEvent) => {
      const meta = evento.metaKey || evento.ctrlKey;
      const target = evento.target as HTMLElement;
      const escribiendo = target.tagName === 'INPUT' || target.tagName === 'SELECT' || target.tagName === 'TEXTAREA';
      if (meta && evento.key.toLowerCase() === 'z') {
        evento.preventDefault();
        if (evento.shiftKey) redo();
        else undo();
      } else if (meta && evento.key.toLowerCase() === 's') {
        evento.preventDefault();
        guardar.mutate();
      } else if (escribiendo) {
        return;
      } else if (evento.key === 'Delete' || evento.key === 'Backspace') {
        if (seleccion) {
          evento.preventDefault();
          eliminarElemento(seleccion);
        }
      } else if (evento.key === 'v' || evento.key === 'V') {
        setHerramienta('seleccionar');
        setColocandoForma(null);
      } else if (evento.key === 'h' || evento.key === 'H') {
        setHerramienta('mano');
        setColocandoForma(null);
      } else if (evento.key === 'Escape') {
        setColocandoForma(null);
        setSeleccion(null);
        setInspectorAbierto(false);
      }
    };
    document.addEventListener('keydown', alTeclear);
    return () => document.removeEventListener('keydown', alTeclear);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [seleccion, past, future, draft, colocandoForma]);

  const salir = () => {
    if (dirty && !window.confirm('Tienes cambios sin guardar en el mapa. ¿Salir de todos modos?')) return;
    navigate('/salon');
  };

  useEffect(() => {
    const alSalir = (evento: BeforeUnloadEvent) => {
      if (!dirty) return;
      evento.preventDefault();
      evento.returnValue = '';
    };
    window.addEventListener('beforeunload', alSalir);
    return () => window.removeEventListener('beforeunload', alSalir);
  }, [dirty]);

  const elemento = draft?.elementos.find((e) => e.id === seleccion) ?? null;

  if (planos.isLoading) {
    return (
      <div className="absolute inset-0 flex items-center justify-center">
        <Skeleton className="h-64 w-full max-w-2xl" />
      </div>
    );
  }

  if (!planoBase || !draft) {
    return (
      <div className="absolute inset-0 flex flex-col items-center justify-center gap-4 text-center">
        <p className="text-sm text-muted-foreground">Este mapa no existe o fue eliminado.</p>
        <Button variant="ghost" onClick={() => navigate('/salon')}>
          Volver a planos
        </Button>
      </div>
    );
  }

  const cursorLienzo = colocandoForma ? 'crosshair' : herramienta === 'mano' ? 'grab' : 'default';

  return (
    <div className="absolute inset-0 flex min-h-0 flex-col overflow-hidden p-3 pb-24 lg:p-4 lg:pb-4">
      {/* ===== Barra de opciones (top) ===== */}
      <div className="flex flex-wrap items-center gap-2 border-b border-border pb-3">
        <Button variant="ghost" size="sm" onClick={salir} aria-label="Volver">
          <ArrowLeft size={16} />
        </Button>
        <input
          value={draft.nombre}
          onChange={(e) => setDraft({ ...draft, nombre: e.target.value })}
          className="min-w-[10rem] flex-1 rounded-control bg-transparent px-2 py-1.5 text-sm font-medium text-foreground hover:bg-accent/10 focus:bg-muted focus:outline-none"
          aria-label="Nombre del mapa"
        />
        <label className="flex items-center gap-1.5 text-xs text-muted-foreground">
          <input type="checkbox" checked={draft.activo} onChange={(e) => { setDraft({ ...draft, activo: e.target.checked }); setDirty(true); }} />
          Activo
        </label>

        <div className="mx-1 h-6 w-px bg-hairline" />

        <HerramientaBtn activo={herramienta === 'seleccionar' && !colocandoForma} onClick={() => { setHerramienta('seleccionar'); setColocandoForma(null); }} label="Seleccionar (V)">
          <MousePointer2 size={16} />
        </HerramientaBtn>
        <HerramientaBtn activo={herramienta === 'mano' && !colocandoForma} onClick={() => { setHerramienta('mano'); setColocandoForma(null); }} label="Mano (H)">
          <Hand size={16} />
        </HerramientaBtn>
        <HerramientaBtn onClick={undo} label="Deshacer (⌘Z)" disabled={past.length === 0}>
          <Undo2 size={16} />
        </HerramientaBtn>
        <HerramientaBtn onClick={redo} label="Rehacer (⇧⌘Z)" disabled={future.length === 0}>
          <Redo2 size={16} />
        </HerramientaBtn>
        <HerramientaBtn activo={rejillaActiva} onClick={() => setRejillaActiva((v) => !v)} label="Rejilla">
          <Grid3x3 size={16} />
        </HerramientaBtn>
        <HerramientaBtn activo={snapActivo} onClick={() => setSnapActivo((v) => !v)} label="Ajustar a rejilla">
          <Magnet size={16} />
        </HerramientaBtn>

        <div className="ml-auto flex items-center gap-1">
          <HerramientaBtn onClick={() => setZoom((z) => Math.max(0.4, +(z - 0.1).toFixed(2)))} label="Alejar">
            <ZoomOut size={16} />
          </HerramientaBtn>
          <span className="num w-12 text-center text-xs text-muted-foreground">{Math.round(zoom * 100)}%</span>
          <HerramientaBtn onClick={() => setZoom((z) => Math.min(2.4, +(z + 0.1).toFixed(2)))} label="Acercar">
            <ZoomIn size={16} />
          </HerramientaBtn>
          <HerramientaBtn onClick={ajustar} label="Ajustar a pantalla">
            <Maximize size={16} />
          </HerramientaBtn>
          <select
            value={draft.piso}
            onChange={(e) => { setDraft({ ...draft, piso: e.target.value }); setDirty(true); }}
            className="h-9 rounded-control border border-border bg-card px-2 text-xs text-foreground"
            aria-label="Piso"
          >
            {PISOS.map((piso) => (
              <option key={piso} value={piso}>
                {piso}
              </option>
            ))}
          </select>
          <Button size="sm" loading={guardar.isPending} onClick={() => guardar.mutate()}>
            {dirty ? <Save size={15} /> : <Check size={15} />}
            Guardar
          </Button>
        </div>
      </div>

      {/* ===== Cuerpo: rail + lienzo ===== */}
      <div className="flex min-h-0 flex-1 gap-3 pt-3">
        {/* Rail de herramientas */}
        <div className="app-scroll flex w-14 shrink-0 flex-col items-center gap-1 overflow-y-auto rounded-lg border border-border bg-card/50 p-1.5">
          <RailBtn activa={herramienta === 'seleccionar' && !colocandoForma} onClick={() => { setHerramienta('seleccionar'); setColocandoForma(null); }} label="Seleccionar">
            <MousePointer2 size={18} />
          </RailBtn>
          <RailBtn activa={herramienta === 'mano' && !colocandoForma} onClick={() => { setHerramienta('mano'); setColocandoForma(null); }} label="Mano">
            <Hand size={18} />
          </RailBtn>
          <div className="my-1 h-px w-8 bg-hairline" />
          {ELEMENTOS.map((def) => (
            <RailBtn key={def.forma} activa={colocandoForma === def.forma} onClick={() => setColocandoForma(def.forma)} label={def.label}>
              <Miniatura forma={def.forma} color={def.color} />
            </RailBtn>
          ))}
        </div>

        {/* Lienzo */}
        <div className="relative min-h-0 min-w-0 flex-1 overflow-hidden rounded-lg border border-border bg-muted/30">
          <div
            ref={wrapper}
            className="relative h-full w-full touch-none overflow-hidden"
            style={{ cursor: cursorLienzo }}
            onPointerDown={alFondo}
            onPointerMove={(e) => {
              mover(e);
              actualizarCoords(e);
            }}
            onPointerUp={terminar}
            onPointerCancel={terminar}
          >
            <div
              style={{ transform: `translate(${pan.x}px, ${pan.y}px) scale(${zoom})`, transformOrigin: '0 0' }}
              className="absolute left-4 top-4"
            >
              <MapaView
                plano={{ ...planoBase, ...draft }}
                zonas={zonas.data ?? []}
                modo="editor"
                rejilla={rejillaActiva}
                onElementoPointerDown={herramienta === 'mano' ? undefined : alPointerElemento}
              />
            </div>
            <EditorOverlay
              elemento={elemento}
              zoom={zoom}
              pan={pan}
              inspectorAbierto={inspectorAbierto}
              onToggleInspector={() => setInspectorAbierto((v) => !v)}
              onRotateStart={iniciarRotate}
              onResizeStart={iniciarResize}
              onDuplicate={() => elemento && duplicarElemento(elemento.id)}
              onDelete={() => elemento && eliminarElemento(elemento.id)}
              onLayer={(delta) => elemento && cambiarZ(elemento.id, delta)}
              onUpdate={(cambios) => elemento && actualizarElemento(elemento.id, cambios)}
              zonas={(zonas.data ?? []) as Zona[]}
              mesas={mesas.data ?? []}
              rootRef={overlayRef}
            />
          </div>
        </div>
      </div>

      {/* ===== Barra de estado ===== */}
      <div className="mt-3 flex items-center gap-3 border-t border-border pt-2 text-[11px] text-muted-foreground">
        <span className="num">{Math.round(zoom * 100)}%</span>
        <span className="num">x {coords.x} · y {coords.y}</span>
        <span>{draft.elementos.length} elementos</span>
        {colocandoForma && <span className="text-foreground">Colocando: {ELEMENTOS_POR_FORMA[colocandoForma]?.label}</span>}
        <span className="ml-auto flex items-center gap-1.5">
          <span className={cn('h-2 w-2 rounded-full', dirty ? 'bg-warning' : 'bg-success')} />
          {dirty ? 'Cambios sin guardar' : 'Guardado'}
        </span>
      </div>
    </div>
  );
}

function HerramientaBtn({
  activo,
  onClick,
  label,
  disabled,
  children,
}: {
  activo?: boolean;
  onClick: () => void;
  label: string;
  disabled?: boolean;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      title={label}
      aria-label={label}
      disabled={disabled}
      onClick={onClick}
      className={cn(
        'flex h-9 w-9 items-center justify-center rounded-xl transition disabled:opacity-40',
        activo ? 'bg-primary/25 text-foreground' : 'text-muted-foreground hover:bg-accent/10 hover:text-foreground',
      )}
    >
      {children}
    </button>
  );
}

function RailBtn({
  activa,
  onClick,
  label,
  children,
}: {
  activa?: boolean;
  onClick: () => void;
  label: string;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      title={label}
      aria-label={label}
      onClick={onClick}
      className={cn(
        'flex h-10 w-10 shrink-0 items-center justify-center rounded-xl border transition',
        activa ? 'border-primary bg-primary/20' : 'border-transparent hover:border-border hover:bg-accent/10',
      )}
    >
      {children}
    </button>
  );
}
