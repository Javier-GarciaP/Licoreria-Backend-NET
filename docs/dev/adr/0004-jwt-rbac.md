# 0004 · Autenticación JWT con RBAC

- **Estado:** Aceptada
- **Fecha:** 2026-09-26
- **Decisores:** Equipo de desarrollo

## Contexto

El sistema interno es multiusuario y por roles (administrador, cajero, mesero, barra,
cocina, host). Se necesita autenticación y autorización claras y compatibles con la API.

## Decisión

Usar **JWT** (token de acceso + refresco) para autenticación y **RBAC** (control de
acceso basado en roles) para autorización, con permisos granulares por rol.

## Consecuencias

- **Positivas:** API stateless, escalable, compatible con React y SignalR.
- **Negativas:** requiere gestionar expiración y revocación de tokens.
- **Neutrales:** los claims alimentan la auditoría (`CreatedBy`/`UpdatedBy`).

## Alternativas consideradas

- Cookies de sesión con servidor: menos adecuado para API stateless y apps separadas.
- OAuth externo: innecesario para usuarios internos.
