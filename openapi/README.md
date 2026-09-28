# Contrato OpenAPI

Contrato **contract-first** de la API. Es la fuente compartida entre el backend (Swagger)
y el frontend (`frontend/packages/api-client`).

## Archivo

- [`licoreria.yaml`](licoreria.yaml) — especificación OpenAPI 3.0.3.

## Uso

- **Backend:** sirve como guía de implementación y para Swagger UI.
- **Frontend:** genera el cliente tipado a partir de este contrato.
- **Documentación:** el sitio Astro muestra la referencia de API (`starlight-openapi`).

## Validación

```bash
npx @stoplight/spectral-cli lint openapi/licoreria.yaml
```

## Convenciones

Ver [`docs/dev/04-api/convenciones.md`](../docs/dev/04-api/convenciones.md) y el manejo
de errores en [`docs/dev/04-api/errores-rfc7807.md`](../docs/dev/04-api/errores-rfc7807.md).
