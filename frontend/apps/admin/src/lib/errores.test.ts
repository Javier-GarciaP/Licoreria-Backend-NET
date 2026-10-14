import { http, HttpResponse } from 'msw';
import { beforeEach, describe, expect, it, vi } from 'vitest';
import { toast } from 'sonner';
import { ApiError, avisarError, mensajeDeError } from '@licoreria/auth';
import { cuentasApi } from '@licoreria/api-client';
import { server } from '../test/server';

vi.mock('sonner', () => ({
  toast: Object.assign(vi.fn(), { warning: vi.fn(), error: vi.fn(), success: vi.fn(), info: vi.fn() }),
  Toaster: () => null,
}));

const BASE = 'http://localhost:5190';

async function capturar(respuesta: () => Response): Promise<ApiError> {
  server.use(http.post(`${BASE}/api/v1/cuentas/c1/comandas`, () => respuesta()));
  try {
    await cuentasApi.agregarComanda('c1', {});
  } catch (error) {
    return error as ApiError;
  }
  throw new Error('Se esperaba un error');
}

describe('errores de concurrencia del cliente HTTP', () => {
  it('reconoce un 409 como conflicto recuperable', async () => {
    const error = await capturar(() =>
      HttpResponse.json(
        { title: 'Conflicto', status: 409, detail: 'La mesa ya está ocupada.', codigo: 'conflicto' },
        { status: 409 },
      ),
    );

    expect(error).toBeInstanceOf(ApiError);
    expect(error.esConflicto).toBe(true);
    expect(error.esExistenciaAgotada).toBe(false);
    expect(mensajeDeError(error)).toMatch(/al mismo tiempo/);
  });

  it('distingue el producto agotado (422) de una carrera (409)', async () => {
    const error = await capturar(() =>
      HttpResponse.json(
        {
          title: 'Regla de Negocio Incumplida',
          status: 422,
          detail: "Stock insuficiente para 'Coca-Cola'",
          codigo: 'existencia-agotada',
          producto: 'Coca-Cola',
          requerido: 2,
          disponible: 1,
        },
        { status: 422 },
      ),
    );

    expect(error.esExistenciaAgotada).toBe(true);
    expect(error.esConflicto).toBe(false);
    expect(mensajeDeError(error)).toBe('Se agotó Coca-Cola: se piden 2 y quedan 1.');
  });

  it('un 422 de otra regla no se confunde con producto agotado', async () => {
    const error = await capturar(() =>
      HttpResponse.json({ title: 'Regla', status: 422, detail: 'No hay un turno abierto.' }, { status: 422 }),
    );

    expect(error.esExistenciaAgotada).toBe(false);
    expect(mensajeDeError(error)).toBe('No hay un turno abierto.');
  });

  it('un 500 conserva el mensaje original', async () => {
    const error = await capturar(() =>
      HttpResponse.json({ title: 'Error interno', status: 500, detail: 'Fallo inesperado.' }, { status: 500 }),
    );

    expect(error.esConflicto).toBe(false);
    expect(mensajeDeError(error)).toBe('Fallo inesperado.');
  });
});

describe('avisarError', () => {
  beforeEach(() => vi.clearAllMocks());

  it('ante un 409 avisa como advertencia y pide refrescar los datos', async () => {
    const error = await capturar(() =>
      HttpResponse.json({ title: 'Conflicto', status: 409, codigo: 'conflicto' }, { status: 409 }),
    );
    const refrescar = vi.fn();

    avisarError('No se pudo enviar', error, refrescar);

    expect(toast.warning).toHaveBeenCalledWith(
      'Los datos cambiaron mientras trabajabas',
      expect.objectContaining({ description: expect.stringMatching(/al mismo tiempo/) }),
    );
    expect(toast.error).not.toHaveBeenCalled();
    expect(refrescar).toHaveBeenCalledTimes(1);
  });

  it('ante un producto agotado avisa con el producto y las cantidades', async () => {
    const error = await capturar(() =>
      HttpResponse.json(
        { title: 'Regla', status: 422, codigo: 'existencia-agotada', producto: 'Coca-Cola', requerido: 1, disponible: 0 },
        { status: 422 },
      ),
    );

    avisarError('No se pudo enviar', error, vi.fn());

    expect(toast.warning).toHaveBeenCalledWith(
      'Producto agotado',
      expect.objectContaining({ description: 'Se agotó Coca-Cola: se piden 1 y quedan 0.' }),
    );
  });

  it('ante porciones sin abrir avisa que falta destapar una botella y pide refrescar', async () => {
    const error = await capturar(() =>
      HttpResponse.json(
        {
          title: 'Regla',
          status: 422,
          codigo: 'porciones-sin-abrir',
          producto: 'Vaso de Coca-Cola',
          requerido: 1,
          disponible: 0,
          envasesCerrados: 2,
        },
        { status: 422 },
      ),
    );
    const refrescar = vi.fn();

    expect(error.esPorcionesSinAbrir).toBe(true);
    expect(error.esExistenciaAgotada).toBe(false);
    avisarError('No se pudo enviar', error, refrescar);

    expect(toast.warning).toHaveBeenCalledWith(
      'Falta destapar una botella',
      expect.objectContaining({
        description: 'No hay porciones abiertas de Vaso de Coca-Cola: pide a barra que destape una botella. Hay 2 botellas cerradas.',
      }),
    );
    expect(toast.error).not.toHaveBeenCalled();
    expect(refrescar).toHaveBeenCalledTimes(1);
  });

  it('con porciones abiertas insuficientes explica cuántas quedan y pide otra botella', async () => {
    const error = await capturar(() =>
      HttpResponse.json(
        {
          title: 'Regla',
          status: 422,
          codigo: 'porciones-sin-abrir',
          producto: 'Coca-Cola · Vaso',
          requerido: 2,
          disponible: 1,
          envasesCerrados: 9,
        },
        { status: 422 },
      ),
    );

    expect(mensajeDeError(error)).toBe(
      'Solo queda 1 porción abierta de Coca-Cola · Vaso y se piden 2: pide a barra que destape otra botella. Hay 9 botellas cerradas.',
    );
  });

  it('un error cualquiera sigue siendo un error y no refresca', async () => {
    const error = await capturar(() =>
      HttpResponse.json({ title: 'Regla', status: 422, detail: 'No hay un turno abierto.' }, { status: 422 }),
    );
    const refrescar = vi.fn();

    avisarError('No se pudo enviar', error, refrescar);

    expect(toast.error).toHaveBeenCalledWith('No se pudo enviar', { description: 'No hay un turno abierto.' });
    expect(toast.warning).not.toHaveBeenCalled();
    expect(refrescar).not.toHaveBeenCalled();
  });
});
