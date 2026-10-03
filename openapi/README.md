# Contrato OpenAPI

Contrato de la API. Es la fuente compartida entre el backend (Swagger) y el
frontend (`frontend/packages/api-client`).

## Archivo

- [`licoreria.yaml`](licoreria.yaml) — especificación OpenAPI 3.0 **generada desde el
  código** (Swagger) y versionada. Contiene todos los endpoints de la API v1.

> Para regenerarla: ejecutar la API en desarrollo y descargar
> `http://localhost:5190/swagger/v1/swagger.json` (o exportar desde Swagger UI).

## Uso

- **Backend:** Swagger UI disponible en desarrollo en `/swagger`.
- **Frontend:** genera el cliente tipado a partir de este contrato
  (`openapi-generator` o `orval`).
- **Documentación:** el sitio Astro muestra la referencia de API.

## Validación

```bash
npx @stoplight/spectral-cli lint openapi/licoreria.yaml
```

## Convenciones

Ver [`docs/dev/04-api/convenciones.md`](../docs/dev/04-api/convenciones.md) y el manejo
de errores en [`docs/dev/04-api/errores-rfc7807.md`](../docs/dev/04-api/errores-rfc7807.md).
