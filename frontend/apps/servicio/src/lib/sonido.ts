/** Sonido corto de notificación (Web Audio API, sin assets). Reutiliza un
 * AudioContext único y lo reanuda si el navegador lo suspende. */
let contexto: AudioContext | null = null;

function obtenerContexto(): AudioContext | null {
  try {
    if (!contexto) {
      const Constructor = window.AudioContext ?? (window as unknown as { webkitAudioContext?: typeof AudioContext }).webkitAudioContext;
      if (!Constructor) return null;
      contexto = new Constructor();
    }
    if (contexto.state === 'suspended') {
      void contexto.resume();
    }
    return contexto;
  } catch {
    return null;
  }
}

/** Dos tonos cortos: uno al recibir (sube) y otro al estar listo (baja). */
export function sonidoNotificacion(tipo: 'recibido' | 'listo' = 'recibido') {
  try {
    const ctx = obtenerContexto();
    if (!ctx) return;

    const tocar = (frecuencia: number, inicio: number, duracion: number) => {
      const oscilador = ctx.createOscillator();
      const ganancia = ctx.createGain();
      oscilador.connect(ganancia);
      ganancia.connect(ctx.destination);
      oscilador.type = 'sine';
      oscilador.frequency.value = frecuencia;
      ganancia.gain.setValueAtTime(0.0001, ctx.currentTime + inicio);
      ganancia.gain.exponentialRampToValueAtTime(0.14, ctx.currentTime + inicio + 0.02);
      ganancia.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + inicio + duracion);
      oscilador.start(ctx.currentTime + inicio);
      oscilador.stop(ctx.currentTime + inicio + duracion + 0.05);
    };

    if (tipo === 'listo') {
      tocar(660, 0, 0.18);
      tocar(990, 0.16, 0.22);
    } else {
      tocar(520, 0, 0.12);
      tocar(780, 0.12, 0.16);
    }
  } catch {
    /* El audio no está disponible; la notificación visual basta. */
  }
}