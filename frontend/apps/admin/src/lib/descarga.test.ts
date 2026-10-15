import { http, HttpResponse } from 'msw';
import { afterAll, beforeAll, describe, expect, it } from 'vitest';
import {
  ApiError,
  apiDownload,
  configureApi,
  nombreDeContentDisposition,
  reportesApi,
} from '@licoreria/api-client';
import { server } from '../test/server';

const BASE = 'http://localhost:5190';
const XLSX = 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet';

describe('nombreDeContentDisposition', () => {
  it('lee el nombre entre comillas', () => {
    expect(nombreDeContentDisposition('attachment; filename="reporte-ventas.xlsx"')).toBe('reporte-ventas.xlsx');
  });

  it('prefiere filename* y lo decodifica', () => {
    expect(
      nombreDeContentDisposition("attachment; filename=reporte.xlsx; filename*=UTF-8''reporte%20mes.xlsx"),
    ).toBe('reporte mes.xlsx');
  });

  it('lee el nombre sin comillas', () => {
    expect(nombreDeContentDisposition('attachment; filename=reporte-caja.pdf')).toBe('reporte-caja.pdf');
  });

  it('devuelve null sin cabecera o sin nombre', () => {
    expect(nombreDeContentDisposition(null)).toBeNull();
    expect(nombreDeContentDisposition('inline')).toBeNull();
  });
});

describe('descarga de reportes', () => {
  beforeAll(() => configureApi({ baseUrl: BASE }));
  afterAll(() => configureApi({ baseUrl: '', getAccessToken: () => null }));

  it('devuelve el Blob y el nombre indicado por el servidor', async () => {
    server.use(
      http.get(`${BASE}/api/v1/reportes/ventas/exportar`, () =>
        new HttpResponse(new Uint8Array([80, 75, 3, 4]), {
          headers: {
            'Content-Type': XLSX,
            'Content-Disposition': 'attachment; filename=reporte-ventas-2026-10-01_2026-10-13.xlsx',
          },
        }),
      ),
    );

    const archivo = await reportesApi.exportar('ventas', 'xlsx', { desde: '2026-10-01', hasta: '2026-10-13' });

    expect(archivo.nombre).toBe('reporte-ventas-2026-10-01_2026-10-13.xlsx');
    expect(archivo.blob.size).toBe(4);
    expect(archivo.blob.type).toBe(XLSX);
  });

  it('envía el formato y los filtros en la consulta', async () => {
    let consulta = '';
    server.use(
      http.get(`${BASE}/api/v1/reportes/productos/exportar`, ({ request }) => {
        consulta = new URL(request.url).search;
        return new HttpResponse(new Uint8Array([1]), { headers: { 'Content-Disposition': 'attachment; filename=x.pdf' } });
      }),
    );

    await reportesApi.exportar('productos', 'pdf', { desde: '2026-10-01', hasta: '2026-10-13', orden: 'Ingresos', limite: 5 });

    const params = new URLSearchParams(consulta);
    expect(params.get('formato')).toBe('pdf');
    expect(params.get('desde')).toBe('2026-10-01');
    expect(params.get('orden')).toBe('Ingresos');
    expect(params.get('limite')).toBe('5');
  });

  it('manda el token de la sesión en la cabecera Authorization', async () => {
    let autorizacion: string | null = null;
    server.use(
      http.get(`${BASE}/api/v1/reportes/caja/exportar`, ({ request }) => {
        autorizacion = request.headers.get('Authorization');
        return new HttpResponse(new Uint8Array([1]));
      }),
    );
    configureApi({ getAccessToken: () => 'token-de-prueba' });

    try {
      await apiDownload('/api/v1/reportes/caja/exportar', { formato: 'pdf' });
    } finally {
      configureApi({ getAccessToken: () => null });
    }

    expect(autorizacion).toBe('Bearer token-de-prueba');
  });

  it('un 403 lanza ApiError con el estado', async () => {
    server.use(
      http.get(`${BASE}/api/v1/reportes/compras/exportar`, () =>
        HttpResponse.json({ title: 'Prohibido', status: 403 }, { status: 403 }),
      ),
    );

    const error = await reportesApi.exportar('compras', 'xlsx').catch((e: unknown) => e);

    expect(error).toBeInstanceOf(ApiError);
    expect((error as ApiError).status).toBe(403);
  });

  it('un 422 conserva el mensaje que sugiere acotar el periodo', async () => {
    server.use(
      http.get(`${BASE}/api/v1/reportes/propinas/exportar`, () =>
        HttpResponse.json(
          { title: 'Regla de Negocio Incumplida', status: 422, detail: 'Acote el periodo o exporte en Excel.' },
          { status: 422 },
        ),
      ),
    );

    const error = await reportesApi.exportar('propinas', 'pdf').catch((e: unknown) => e);

    expect((error as ApiError).status).toBe(422);
    expect((error as ApiError).message).toMatch(/Acote el periodo/);
  });
});
