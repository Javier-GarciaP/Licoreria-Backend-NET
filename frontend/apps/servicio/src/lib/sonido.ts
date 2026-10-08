/** Sonido corto de notificación (Web Audio API, sin assets). */
export function sonidoNotificacion() {
  try {
    const contexto = new AudioContext();
    const oscilador = contexto.createOscillator();
    const ganancia = contexto.createGain();
    oscilador.connect(ganancia);
    ganancia.connect(contexto.destination);
    oscilador.frequency.value = 880;
    ganancia.gain.value = 0.12;
    oscilador.start();
    ganancia.gain.exponentialRampToValueAtTime(0.0001, contexto.currentTime + 0.35);
    oscilador.stop(contexto.currentTime + 0.4);
  } catch {
    /* El audio no está disponible; la notificación visual basta. */
  }
}
