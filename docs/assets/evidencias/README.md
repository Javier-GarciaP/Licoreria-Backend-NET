# Evidencias por Fase

Índice de las evidencias (capturas y colecciones) de cada fase.

## Fase 1 · Fundamentos y resiliencia REST

- Capturas RFC 7807: [`fase-1/postman1.jpeg`](fase-1/postman1.jpeg) · [`fase-1/postman2.jpeg`](fase-1/postman2.jpeg)
- Colección Postman: [`Licoreria_Fase1_Postman_Collection.json`](Licoreria_Fase1_Postman_Collection.json)
- Documentación detallada: [Fase 1](../dev/10-fases/fase-1.md)

## Fase 2 · Persistencia PostgreSQL 15

- Tablas creadas: [`fase-2/tablas.png`](fase-2/tablas.png)
- DDL con restricciones (UUID, `numeric(18,2)`, PK/FK, único): [`fase-2/genera-sql-productos.png`](fase-2/genera-sql-productos.png)
- Datos sembrados: [`fase-2/siembra-datos.png`](fase-2/siembra-datos.png)
- Script DDL: [`../../database/scripts/initial-infrastructure-catalog.sql`](../../database/scripts/initial-infrastructure-catalog.sql)
- Documentación detallada: [Fase 2](../dev/10-fases/fase-2.md)

## Fase 3 · Seguridad JWT, RBAC y validación

- Login Admin: [`fase-3/login-admin.png`](fase-3/login-admin.png)
- Login Employee: [`fase-3/login-empleado.png`](fase-3/login-empleado.png)
- Listar productos (Employee): [`fase-3/fase3-listrarproductos.png`](fase-3/fase3-listrarproductos.png)
- Sin token (401): [`fase-3/fase3-401.png`](fase-3/fase3-401.png)
- Eliminar con Employee (403): [`fase-3/fase3-403.png`](fase-3/fase3-403.png)
- Precio negativo (400): [`fase-3/fase3-400.png`](fase-3/fase3-400.png)
- Colección Postman: [`Licoreria_Fase3_Postman_Collection.json`](Licoreria_Fase3_Postman_Collection.json)
- Documentación detallada: [Fase 3](../dev/10-fases/fase-3.md)
