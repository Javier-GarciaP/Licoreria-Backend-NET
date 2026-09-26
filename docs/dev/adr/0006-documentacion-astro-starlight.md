# 0006 · Documentación con Astro Starlight

- **Estado:** Aceptada
- **Fecha:** 2026-09-26
- **Decisores:** Equipo de desarrollo

## Contexto

Se necesita documentación técnica para desarrolladores y manuales para el cliente
final, publicados en la web y versionados junto al código.

## Decisión

Usar **Astro Starlight** para el sitio de documentación, con **dos secciones**
(*Desarrolladores* y *Cliente*), publicado en **GitHub Pages**. La documentación
técnica canónica vive en `docs/dev` (GitHub) y el sitio la consume; la sección de
cliente se redacta directamente en el sitio.

## Consecuencias

- **Positivas:** búsqueda integrada, soporte de Mermaid y OpenAPI, navegación clara,
  hosting gratuito en GitHub Pages.
- **Negativas:** requiere un paso de sincronización de contenido.
- **Neutrales:** el contenido se mantiene en español.

## Alternativas consideradas

- GitHub Wiki: poca personalización y sin sección de cliente.
- Docusaurus: válido, pero Astro es más ligero y moderno.
