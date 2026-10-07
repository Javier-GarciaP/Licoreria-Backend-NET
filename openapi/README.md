# Contrato OpenAPI

Contrato de la API. Es la fuente compartida entre el backend (Swagger) y el
frontend (`frontend/packages/api-client`).

## Archivo

- [`licoreria.yaml`](licoreria.yaml) — especificación OpenAPI **generada desde el
  código** (OpenAPI nativo de ASP.NET Core) y versionada. Contiene todos los endpoints de la API v1.

## Regeneración

El documento se genera durante `dotnet build` (`Microsoft.AspNetCore.OpenApi`,
`OpenApiGenerateDocuments`) como `openapi/generated/Licoreria.WebAPI.json`, y se
exporta a YAML con una tarea:

```bash
task backend:openapi        # genera el JSON y escribe openapi/licoreria.yaml
task backend:openapi:check  # verifica que el YAML esté sincronizado con los controladores
```

> El directorio `openapi/generated/` es un artefacto de build (ignorado por git);
> solo se versiona `licoreria.yaml`.

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
