import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

export interface SeccionPdf {
  titulo: string;
  /** Indicadores clave: pares etiqueta/valor. */
  kpis?: [string, string][];
  /** Tabla opcional con encabezado y filas ya formateadas. */
  tabla?: { cabecera: string[]; filas: string[][] };
}

/** Genera el PDF del dashboard con todas las secciones disponibles y lo descarga. */
export function exportarDashboardPdf(opciones: {
  local: string;
  periodo: string;
  secciones: SeccionPdf[];
  nombreArchivo: string;
}) {
  const doc = new jsPDF({ unit: 'pt', format: 'a4' });
  const ancho = doc.internal.pageSize.getWidth();

  doc.setFontSize(18);
  doc.text(`${opciones.local} · Dashboard de analíticas`, 40, 48);
  doc.setFontSize(10);
  doc.setTextColor(110);
  doc.text(`Periodo: ${opciones.periodo}`, 40, 66);
  doc.text(`Generado: ${new Date().toLocaleString('es-VE')}`, ancho - 40, 66, { align: 'right' });
  doc.setTextColor(0);

  let y = 90;
  for (const seccion of opciones.secciones) {
    if (!seccion.kpis?.length && !seccion.tabla?.filas.length) continue;
    if (y > 740) {
      doc.addPage();
      y = 48;
    }
    doc.setFontSize(13);
    doc.text(seccion.titulo, 40, y);
    y += 6;
    if (seccion.kpis?.length) {
      autoTable(doc, {
        startY: y,
        body: seccion.kpis,
        theme: 'plain',
        styles: { fontSize: 10, cellPadding: 3 },
        columnStyles: { 0: { textColor: 100, cellWidth: 240 }, 1: { fontStyle: 'bold' } },
        margin: { left: 40, right: 40 },
      });
      y = (doc as unknown as { lastAutoTable: { finalY: number } }).lastAutoTable.finalY + 6;
    }
    if (seccion.tabla?.filas.length) {
      autoTable(doc, {
        startY: y,
        head: [seccion.tabla.cabecera],
        body: seccion.tabla.filas,
        styles: { fontSize: 9 },
        headStyles: { fillColor: [76, 76, 120] },
        margin: { left: 40, right: 40 },
      });
      y = (doc as unknown as { lastAutoTable: { finalY: number } }).lastAutoTable.finalY + 6;
    }
    y += 14;
  }

  const paginas = doc.getNumberOfPages();
  for (let i = 1; i <= paginas; i++) {
    doc.setPage(i);
    doc.setFontSize(8);
    doc.setTextColor(130);
    doc.text(`Página ${i} de ${paginas}`, ancho / 2, 820, { align: 'center' });
  }
  doc.save(opciones.nombreArchivo);
}
