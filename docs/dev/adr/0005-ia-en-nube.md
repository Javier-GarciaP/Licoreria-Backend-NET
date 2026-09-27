# 0005 · Ejecutar la IA en la nube

- **Estado:** Aceptada
- **Fecha:** 2026-09-26
- **Decisores:** Equipo de desarrollo

## Contexto

El módulo de IA requiere visión por computador (digitalizar planos desde papel),
generación de texto/imágenes (secciones y posters) y modelos de pronóstico. Se debe
elegir dónde ejecutar estos modelos.

## Decisión

Consumir **modelos de IA en la nube mediante API**, a través de una capa de abstracción
en `Application`, con ejecución asíncrona de trabajos y registro en `AiGeneracion`.

## Consecuencias

- **Positivas:** integración rápida, sin infraestructura de GPU, modelos de última
  generación.
- **Negativas:** costo por uso y dependencia de proveedor; se mitiga con la abstracción.
- **Neutrales:** las imágenes generadas se almacenan en el storage propio.

## Alternativas consideradas

- Modelos autoalojados: mayor control y privacidad, pero alto costo operativo.
- Híbrido: se puede adoptar después gracias a la capa de abstracción.
