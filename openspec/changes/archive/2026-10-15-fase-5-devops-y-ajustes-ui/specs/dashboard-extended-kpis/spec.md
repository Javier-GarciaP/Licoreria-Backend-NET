# dashboard-extended-kpis Specification

## Purpose

Amplía el Dashboard con indicadores de ventas, inventario, compras y salón, y permite llevarlos a PDF.

## Requirements

### Requirement: Indicadores adicionales
El Dashboard SHALL mostrar promedio diario, mejor día, ventas en Bs, propinas, costo y margen potencial del inventario, compras, por pagar, clientes, productos activos, permanencia en mesa y reservas, además de paneles por método de pago, top por ingresos, usuarios, mesoneros y zonas.

#### Scenario: Rol sin acceso a mesoneros
- **WHEN** un usuario no administrador abre el Dashboard
- **THEN** el panel de mesoneros indica que solo está disponible para administradores y el resto sigue funcionando

### Requirement: Exportación a PDF
El Dashboard SHALL ofrecer un botón que genere un PDF con los datos del periodo elegido y omita las secciones sin datos.

#### Scenario: Periodo válido con ventas cargadas
- **WHEN** el usuario pulsa Exportar PDF
- **THEN** se descarga `dashboard-<desde>_<hasta>.pdf`

#### Scenario: Periodo inválido
- **WHEN** el periodo no es válido o las ventas no han cargado
- **THEN** el botón está deshabilitado
