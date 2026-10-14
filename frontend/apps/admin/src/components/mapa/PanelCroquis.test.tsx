import { fireEvent, render, screen, waitFor } from '@testing-library/react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { http, HttpResponse } from 'msw';
import { describe, expect, it, vi } from 'vitest';
import '@licoreria/auth'; // configura la URL base del cliente HTTP
import { server } from '../../test/server';
import { PanelCroquis } from './PanelCroquis';

const BASE = 'http://localhost:5190';

function renderPanel(props: Partial<React.ComponentProps<typeof PanelCroquis>> = {}) {
  const onOpacidad = vi.fn();
  const onAjuste = vi.fn();
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  render(
    <QueryClientProvider client={queryClient}>
      <PanelCroquis
        planoId="p-1"
        tieneCroquis={false}
        opacidad={0.5}
        ajuste="contener"
        visible
        onOpacidad={onOpacidad}
        onAjuste={onAjuste}
        onVisible={() => undefined}
        {...props}
      />
    </QueryClientProvider>,
  );
  return { onOpacidad, onAjuste };
}

const archivo = (nombre: string, tipo: string, bytes = 10) => new File([new Uint8Array(bytes)], nombre, { type: tipo });

describe('PanelCroquis', () => {
  it('sube la imagen elegida al endpoint del fondo', async () => {
    let tipoContenido: string | null = null;
    server.use(
      http.post(`${BASE}/api/v1/planos/p-1/fondo`, ({ request }) => {
        tipoContenido = request.headers.get('content-type');
        return HttpResponse.json({ id: 'p-1', fondoImagenUrl: '/uploads/planos/a.png' });
      }),
    );
    renderPanel();

    fireEvent.change(screen.getByLabelText('Archivo del croquis'), { target: { files: [archivo('croquis.png', 'image/png')] } });

    await waitFor(() => expect(tipoContenido).not.toBeNull());
    expect(tipoContenido).toContain('multipart/form-data');
  });

  it('rechaza en el cliente una imagen de más de 5 MB sin llamar al servidor', async () => {
    let llamadas = 0;
    server.use(
      http.post(`${BASE}/api/v1/planos/p-1/fondo`, () => {
        llamadas += 1;
        return HttpResponse.json({});
      }),
    );
    renderPanel();

    fireEvent.change(screen.getByLabelText('Archivo del croquis'), {
      target: { files: [archivo('grande.png', 'image/png', 6 * 1024 * 1024)] },
    });

    expect(await screen.findByRole('alert')).toHaveTextContent('5 MB');
    expect(llamadas).toBe(0);
  });

  it('rechaza un tipo de archivo no permitido', async () => {
    renderPanel();

    fireEvent.change(screen.getByLabelText('Archivo del croquis'), { target: { files: [archivo('a.gif', 'image/gif')] } });

    expect(await screen.findByRole('alert')).toHaveTextContent('PNG, JPG o WebP');
  });

  it('con croquis permite cambiar opacidad y ajuste, y quitarlo', async () => {
    let quitado = false;
    server.use(
      http.delete(`${BASE}/api/v1/planos/p-1/fondo`, () => {
        quitado = true;
        return HttpResponse.json({ id: 'p-1', fondoImagenUrl: null });
      }),
    );
    const { onOpacidad, onAjuste } = renderPanel({ tieneCroquis: true });

    fireEvent.change(screen.getByLabelText('Opacidad del croquis'), { target: { value: '0.3' } });
    fireEvent.change(screen.getByLabelText('Ajuste del croquis'), { target: { value: 'cubrir' } });
    fireEvent.click(screen.getByRole('button', { name: /quitar/i }));

    expect(onOpacidad).toHaveBeenCalledWith(0.3);
    expect(onAjuste).toHaveBeenCalledWith('cubrir');
    await waitFor(() => expect(quitado).toBe(true));
  });
});
