/** Guarda un archivo en el equipo del usuario mediante un enlace temporal (el JWT impide enlazar directo al API). */
export function guardarArchivo(blob: Blob, nombre: string): void {
  const url = URL.createObjectURL(blob);
  const enlace = document.createElement('a');
  enlace.href = url;
  enlace.download = nombre;
  enlace.style.display = 'none';
  document.body.appendChild(enlace);
  enlace.click();
  enlace.remove();
  // Se libera después del clic para que el navegador alcance a iniciar la descarga.
  setTimeout(() => URL.revokeObjectURL(url), 1000);
}
