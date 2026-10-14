import type { ProblemDetails } from '@licoreria/types';

export interface ApiConfig {
  baseUrl: string;
  getAccessToken: () => string | null;
  getRefreshToken: () => string | null;
  /** Rota el par de tokens y devuelve el nuevo access token (o null si falla). */
  refresh: () => Promise<string | null>;
  onUnauthorized?: () => void;
}

let config: ApiConfig = {
  baseUrl: '',
  getAccessToken: () => null,
  getRefreshToken: () => null,
  refresh: async () => null,
};

export function configureApi(next: Partial<ApiConfig>): void {
  config = { ...config, ...next };
}

/** Error tipado a partir de una respuesta RFC 7807. */
export class ApiError extends Error {
  readonly status: number;
  readonly problem: ProblemDetails;

  constructor(status: number, problem: ProblemDetails) {
    super(problem.detail || problem.title || `Error ${status}`);
    this.name = 'ApiError';
    this.status = status;
    this.problem = problem;
  }

  /** 409: otra operación modificó los mismos datos (o creó el mismo registro) al mismo tiempo. */
  get esConflicto(): boolean {
    return this.status === 409;
  }

  /** 422 por producto agotado: distinto de una carrera, el stock realmente no alcanza. */
  get esExistenciaAgotada(): boolean {
    return this.status === 422 && this.problem.codigo === 'existencia-agotada';
  }

  /** 422: se pidió una porción (vaso, trago) y hay botellas cerradas pero ningún envase abierto: la barra debe destapar una. */
  get esPorcionesSinAbrir(): boolean {
    return this.status === 422 && this.problem.codigo === 'porciones-sin-abrir';
  }

  /** Mensajes por campo devueltos por FluentValidation (400). */
  get fieldErrors(): Record<string, string[]> {
    return this.problem.errors ?? {};
  }
}

function buildUrl(path: string, query?: object): string {
  const base = config.baseUrl.replace(/\/$/, '');
  const url = `${base}${path.startsWith('/') ? path : `/${path}`}`;
  if (!query) return url;

  const params = new URLSearchParams();
  for (const [key, value] of Object.entries(query as Record<string, unknown>)) {
    if (value === undefined || value === null || value === '') continue;
    params.append(key, String(value));
  }
  const qs = params.toString();
  return qs ? `${url}?${qs}` : url;
}

async function parseProblem(response: Response): Promise<ProblemDetails> {
  try {
    const body = await response.json();
    return body as ProblemDetails;
  } catch {
    return { status: response.status, title: response.statusText };
  }
}

export interface RequestOptions {
  method?: string;
  body?: unknown;
  query?: object;
  formData?: FormData;
  signal?: AbortSignal;
}

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = 'GET', body, query, formData, signal } = options;

  const call = (token: string | null) => {
    const headers: Record<string, string> = { Accept: 'application/json' };
    if (token) headers.Authorization = `Bearer ${token}`;
    if (body !== undefined && !formData) headers['Content-Type'] = 'application/json';

    return fetch(buildUrl(path, query), {
      method,
      headers,
      body: formData ?? (body !== undefined ? JSON.stringify(body) : undefined),
      signal,
    });
  };

  let response = await call(config.getAccessToken());

  if (response.status === 401 && config.getRefreshToken()) {
    const newToken = await config.refresh();
    if (newToken) {
      response = await call(newToken);
    }
  }

  if (!response.ok) {
    const problem = await parseProblem(response);
    // No cerrar sesión ante un 401 del propio flujo de autenticación.
    if (response.status === 401 && !path.startsWith('/api/auth/')) {
      config.onUnauthorized?.();
    }
    throw new ApiError(response.status, problem);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
