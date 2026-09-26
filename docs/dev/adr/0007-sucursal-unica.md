# 0007 · Modelo para una única sucursal

- **Estado:** Aceptada
- **Fecha:** 2026-09-26
- **Decisores:** Equipo de desarrollo

## Contexto

Se consideró inicialmente un modelo multi-sucursal/multi-tenant, pero el alcance real
del proyecto es un único local de licorería/discoteca.

## Decisión

Diseñar el sistema para **una única sucursal**, sin entidades de `Empresa`, `Sucursal`
ni `Almacen`. El stock es global y existe un único punto de venta para correlativos
fiscales.

## Consecuencias

- **Positivas:** modelo más simple, menos tablas y menor complejidad operativa.
- **Negativas:** si en el futuro se requiere multi-sucursal, habrá que migrar.
- **Neutrales:** se conservan agregados bien delimitados que facilitan esa evolución.

## Alternativas consideradas

- Multi-sucursal desde el inicio: mayor complejidad no justificada por el alcance.
