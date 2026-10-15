# ci-pipeline Specification

## Purpose

Valida cada cambio y publica artefactos solo cuando todo pasa.

## Requirements

### Requirement: Disparadores y pruebas
El workflow `ci.yml` SHALL ejecutarse en push y pull_request a `main` y `master`, restaurar, compilar y ejecutar las pruebas xUnit/Moq del backend y las pruebas y el build del frontend.

#### Scenario: Pruebas fallidas
- **WHEN** una prueba del backend o del frontend falla
- **THEN** no se publica el artefacto ni se construyen las imágenes

### Requirement: Publicación condicionada
Los jobs de publicación del artefacto de la API y de construcción de imágenes SHALL depender de los jobs de backend y frontend.

#### Scenario: Todo en verde
- **WHEN** backend y frontend pasan
- **THEN** se publica el artefacto `api-publish` y se construyen las imágenes
