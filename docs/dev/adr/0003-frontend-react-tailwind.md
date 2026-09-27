# 0003 · Frontend con React + Tailwind CSS

- **Estado:** Aceptada
- **Fecha:** 2026-09-26
- **Decisores:** Equipo de desarrollo

## Contexto

Se requieren dos aplicaciones web: una pública para clientes y una interna para el
personal. Se busca un stack moderno, con buena experiencia de desarrollo y control
total del diseño.

## Decisión

Implementar ambas aplicaciones con **React + Vite + TypeScript** y **Tailwind CSS**,
en un monorepo con workspaces y paquetes compartidos (UI, cliente de API, tipos).

## Consecuencias

- **Positivas:** ecosistema amplio, tipado fuerte, estilos utilitarios rápidos de
  mantener, componentes reutilizables entre apps.
- **Negativas:** la web pública requiere trabajo adicional de SEO (mitigable con
  prerenderizado).
- **Neutrales:** el backend permanece en .NET; el contrato se comparte por OpenAPI.

## Alternativas consideradas

- Blazor: unifica el stack en C#, pero el equipo prefiere React.
- Vue/Nuxt: válido, pero React es la elección del equipo.
