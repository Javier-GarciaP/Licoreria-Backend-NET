CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;
CREATE TABLE categorias (
    "Id" uuid NOT NULL,
    "Nombre" character varying(50) NOT NULL,
    "Descripcion" character varying(200) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_categorias" PRIMARY KEY ("Id")
);

CREATE TABLE marcas (
    "Id" uuid NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Descripcion" character varying(200),
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_marcas" PRIMARY KEY ("Id")
);

CREATE TABLE unidades_medida (
    "Id" uuid NOT NULL,
    "Nombre" character varying(50) NOT NULL,
    "Abreviatura" character varying(10) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_unidades_medida" PRIMARY KEY ("Id")
);

CREATE TABLE usuarios (
    "Id" uuid NOT NULL,
    "NombreCompleto" character varying(100) NOT NULL,
    "Email" character varying(100) NOT NULL,
    "PasswordHash" text NOT NULL,
    "Rol" character varying(30) NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_usuarios" PRIMARY KEY ("Id")
);

CREATE TABLE productos (
    "Id" uuid NOT NULL,
    "Nombre" character varying(100) NOT NULL,
    "Descripcion" character varying(300),
    "Sku" character varying(20) NOT NULL,
    "CodigoBarras" character varying(50) NOT NULL,
    "PrecioCompraUSD" numeric(18,2) NOT NULL,
    "PrecioVentaUSD" numeric(18,2) NOT NULL,
    "Stock" integer NOT NULL,
    "StockMinimo" integer NOT NULL,
    "StockMaximo" integer NOT NULL DEFAULT 0,
    "ImagenUrl" character varying(500),
    "Activo" boolean NOT NULL,
    "CategoriaId" uuid NOT NULL,
    "MarcaId" uuid NOT NULL,
    "UnidadMedidaId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_productos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_productos_categorias_CategoriaId" FOREIGN KEY ("CategoriaId") REFERENCES categorias ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_productos_marcas_MarcaId" FOREIGN KEY ("MarcaId") REFERENCES marcas ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_productos_unidades_medida_UnidadMedidaId" FOREIGN KEY ("UnidadMedidaId") REFERENCES unidades_medida ("Id") ON DELETE RESTRICT
);

CREATE TABLE ventas (
    "Id" uuid NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "TasaCambio" numeric(18,4) NOT NULL,
    "TotalUSD" numeric(18,2) NOT NULL,
    "TotalBS" numeric(18,2) NOT NULL,
    "MetodoPago" character varying(50) NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_ventas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ventas_usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuarios ("Id") ON DELETE RESTRICT
);

CREATE TABLE detalles_venta (
    "Id" uuid NOT NULL,
    "VentaId" uuid NOT NULL,
    "ProductoId" uuid NOT NULL,
    "Cantidad" integer NOT NULL,
    "PrecioUnitarioUSD" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    CONSTRAINT "PK_detalles_venta" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_detalles_venta_productos_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES productos ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_detalles_venta_ventas_VentaId" FOREIGN KEY ("VentaId") REFERENCES ventas ("Id") ON DELETE CASCADE
);

INSERT INTO categorias ("Id", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('11111111-1111-1111-1111-111111111111', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Rones, Whiskeys, Anises y Vodka', FALSE, NULL, 'Licores');
INSERT INTO categorias ("Id", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('22222222-2222-2222-2222-222222222222', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Refrescos y bebidas carbonatadas', FALSE, NULL, 'Gaseosas');
INSERT INTO categorias ("Id", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('33333333-3333-3333-3333-333333333333', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Bebidas para rendimiento y energía', FALSE, NULL, 'Energizantes');
INSERT INTO categorias ("Id", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('44444444-4444-4444-4444-444444444444', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Snacks, papitas y frutos secos', FALSE, NULL, 'Pasabocas');

INSERT INTO marcas ("Id", "Activo", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('55555555-5555-5555-5555-555555555551', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Ron venezolano', FALSE, NULL, 'Cacique');
INSERT INTO marcas ("Id", "Activo", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('55555555-5555-5555-5555-555555555552', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Whisky escocés', FALSE, NULL, 'Old Parr');
INSERT INTO marcas ("Id", "Activo", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('55555555-5555-5555-5555-555555555553', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Bebidas gaseosas', FALSE, NULL, 'Coca-Cola');
INSERT INTO marcas ("Id", "Activo", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('55555555-5555-5555-5555-555555555554', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Bebidas energizantes', FALSE, NULL, 'Red Bull');
INSERT INTO marcas ("Id", "Activo", "CreatedAt", "Descripcion", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('55555555-5555-5555-5555-555555555555', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Snacks y pasabocas', FALSE, NULL, 'Frito-Lay');

INSERT INTO unidades_medida ("Id", "Abreviatura", "CreatedAt", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('66666666-6666-6666-6666-666666666661', 'BOT', TIMESTAMPTZ '2026-09-23T00:00:00Z', FALSE, NULL, 'Botella');
INSERT INTO unidades_medida ("Id", "Abreviatura", "CreatedAt", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('66666666-6666-6666-6666-666666666662', 'UND', TIMESTAMPTZ '2026-09-23T00:00:00Z', FALSE, NULL, 'Unidad');
INSERT INTO unidades_medida ("Id", "Abreviatura", "CreatedAt", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('66666666-6666-6666-6666-666666666663', 'TOB', TIMESTAMPTZ '2026-09-23T00:00:00Z', FALSE, NULL, 'Tobo');
INSERT INTO unidades_medida ("Id", "Abreviatura", "CreatedAt", "IsDeleted", "LastModifiedAt", "Nombre")
VALUES ('66666666-6666-6666-6666-666666666664', 'PLA', TIMESTAMPTZ '2026-09-23T00:00:00Z', FALSE, NULL, 'Plato');

INSERT INTO usuarios ("Id", "Activo", "CreatedAt", "Email", "IsDeleted", "LastModifiedAt", "NombreCompleto", "PasswordHash", "Rol")
VALUES ('20000000-0000-0000-0000-000000000001', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'admin@licoreria.com', FALSE, NULL, 'Administrador Principal', '100000.FAjfLZhxDdPKpSSBsWnxhA==.oEa34uWpndtwYkEbsIYWwybInWw7MjWldrQtjwI46xI=', 'Administrador');
INSERT INTO usuarios ("Id", "Activo", "CreatedAt", "Email", "IsDeleted", "LastModifiedAt", "NombreCompleto", "PasswordHash", "Rol")
VALUES ('20000000-0000-0000-0000-000000000002', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'cajero1@licoreria.com', FALSE, NULL, 'Cajero Turno Mañana', '100000./aCU6rte+XCUv0r4ENjLuw==.1WuFy7dAWv4p3TZOWu05sIRNYmAQy25tmq7a2FxGY50=', 'Cajero');

INSERT INTO productos ("Id", "Activo", "CategoriaId", "CodigoBarras", "CreatedAt", "Descripcion", "ImagenUrl", "IsDeleted", "LastModifiedAt", "MarcaId", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId")
VALUES ('10000000-0000-0000-0000-000000000001', TRUE, '11111111-1111-1111-1111-111111111111', '759100100101', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Ron añejo venezolano de 0.75 litros.', NULL, FALSE, NULL, '55555555-5555-5555-5555-555555555551', 'Ron Cacique Añejo 0.75L', 8.5, 12.0, 'LIC-RON-0001', 30, 60, 5, '66666666-6666-6666-6666-666666666661');
INSERT INTO productos ("Id", "Activo", "CategoriaId", "CodigoBarras", "CreatedAt", "Descripcion", "ImagenUrl", "IsDeleted", "LastModifiedAt", "MarcaId", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId")
VALUES ('10000000-0000-0000-0000-000000000002', TRUE, '11111111-1111-1111-1111-111111111111', '500028100202', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Whisky escocés de 12 años, 0.75 litros.', NULL, FALSE, NULL, '55555555-5555-5555-5555-555555555552', 'Whisky Old Parr 12 Años 0.75L', 28.0, 38.0, 'LIC-WHI-0002', 12, 24, 3, '66666666-6666-6666-6666-666666666661');
INSERT INTO productos ("Id", "Activo", "CategoriaId", "CodigoBarras", "CreatedAt", "Descripcion", "ImagenUrl", "IsDeleted", "LastModifiedAt", "MarcaId", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId")
VALUES ('10000000-0000-0000-0000-000000000003', TRUE, '22222222-2222-2222-2222-222222222222', '759100200303', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Refresco de cola de 2 litros.', NULL, FALSE, NULL, '55555555-5555-5555-5555-555555555553', 'Coca-Cola 2 Litros', 1.6, 2.5, 'GAS-COC-0003', 50, 100, 10, '66666666-6666-6666-6666-666666666661');
INSERT INTO productos ("Id", "Activo", "CategoriaId", "CodigoBarras", "CreatedAt", "Descripcion", "ImagenUrl", "IsDeleted", "LastModifiedAt", "MarcaId", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId")
VALUES ('10000000-0000-0000-0000-000000000004', TRUE, '33333333-3333-3333-3333-333333333333', '900249010001', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Bebida energizante de 250 ml.', NULL, FALSE, NULL, '55555555-5555-5555-5555-555555555554', 'Red Bull 250ml', 1.8, 3.0, 'ENE-RED-0004', 40, 80, 8, '66666666-6666-6666-6666-666666666662');
INSERT INTO productos ("Id", "Activo", "CategoriaId", "CodigoBarras", "CreatedAt", "Descripcion", "ImagenUrl", "IsDeleted", "LastModifiedAt", "MarcaId", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "Sku", "Stock", "StockMaximo", "StockMinimo", "UnidadMedidaId")
VALUES ('10000000-0000-0000-0000-000000000005', TRUE, '44444444-4444-4444-4444-444444444444', '759100400505', TIMESTAMPTZ '2026-09-23T00:00:00Z', 'Snack de maíz sabor queso, 150 g.', NULL, FALSE, NULL, '55555555-5555-5555-5555-555555555555', 'Doritos Queso Atrevido 150g', 1.2, 2.0, 'PAS-DOR-0005', 25, 50, 5, '66666666-6666-6666-6666-666666666662');

CREATE INDEX "IX_detalles_venta_ProductoId" ON detalles_venta ("ProductoId");

CREATE INDEX "IX_detalles_venta_VentaId" ON detalles_venta ("VentaId");

CREATE INDEX "IX_productos_CategoriaId" ON productos ("CategoriaId");

CREATE INDEX "IX_productos_MarcaId" ON productos ("MarcaId");

CREATE UNIQUE INDEX "IX_productos_Sku" ON productos ("Sku");

CREATE INDEX "IX_productos_UnidadMedidaId" ON productos ("UnidadMedidaId");

CREATE UNIQUE INDEX "IX_usuarios_Email" ON usuarios ("Email");

CREATE INDEX "IX_ventas_UsuarioId" ON ventas ("UsuarioId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260928120124_InitialInfrastructureCatalog', '10.0.12');

COMMIT;

START TRANSACTION;
UPDATE usuarios SET "PasswordHash" = '100000.bGljb3JlcmlhLWFkbWluIQ==.YwgspkL29TkDaFw34dp94bJtoYuLiheB1jTHjHoI0/A='
WHERE "Id" = '20000000-0000-0000-0000-000000000001';

UPDATE usuarios SET "PasswordHash" = '100000.bGljb3JlcmlhLWNhamVybw==.vBfTiTnA2NkFbJLRMHRR/Cp00Fm6VwefpzHLwFJcVY0='
WHERE "Id" = '20000000-0000-0000-0000-000000000002';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260928121214_SeedUsuariosConPasswordHash', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE ventas ADD "CreatedBy" uuid;

ALTER TABLE ventas ADD "UpdatedBy" uuid;

ALTER TABLE usuarios ADD "CreatedBy" uuid;

ALTER TABLE usuarios ADD "UpdatedBy" uuid;

ALTER TABLE unidades_medida ADD "CreatedBy" uuid;

ALTER TABLE unidades_medida ADD "UpdatedBy" uuid;

ALTER TABLE productos ADD "CreatedBy" uuid;

ALTER TABLE productos ADD "UpdatedBy" uuid;

ALTER TABLE marcas ADD "CreatedBy" uuid;

ALTER TABLE marcas ADD "UpdatedBy" uuid;

ALTER TABLE detalles_venta ADD "CreatedBy" uuid;

ALTER TABLE detalles_venta ADD "UpdatedBy" uuid;

ALTER TABLE categorias ADD "CreatedBy" uuid;

ALTER TABLE categorias ADD "UpdatedBy" uuid;

UPDATE categorias SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '11111111-1111-1111-1111-111111111111';

UPDATE categorias SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '22222222-2222-2222-2222-222222222222';

UPDATE categorias SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '33333333-3333-3333-3333-333333333333';

UPDATE categorias SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '44444444-4444-4444-4444-444444444444';

UPDATE marcas SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '55555555-5555-5555-5555-555555555551';

UPDATE marcas SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '55555555-5555-5555-5555-555555555552';

UPDATE marcas SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '55555555-5555-5555-5555-555555555553';

UPDATE marcas SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '55555555-5555-5555-5555-555555555554';

UPDATE marcas SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '55555555-5555-5555-5555-555555555555';

UPDATE productos SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '10000000-0000-0000-0000-000000000001';

UPDATE productos SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '10000000-0000-0000-0000-000000000002';

UPDATE productos SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '10000000-0000-0000-0000-000000000003';

UPDATE productos SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '10000000-0000-0000-0000-000000000004';

UPDATE productos SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '10000000-0000-0000-0000-000000000005';

UPDATE unidades_medida SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '66666666-6666-6666-6666-666666666661';

UPDATE unidades_medida SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '66666666-6666-6666-6666-666666666662';

UPDATE unidades_medida SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '66666666-6666-6666-6666-666666666663';

UPDATE unidades_medida SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '66666666-6666-6666-6666-666666666664';

UPDATE usuarios SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '20000000-0000-0000-0000-000000000001';

UPDATE usuarios SET "CreatedBy" = NULL, "UpdatedBy" = NULL
WHERE "Id" = '20000000-0000-0000-0000-000000000002';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003154414_AuditoriaCampos', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE refresh_tokens (
    "Id" uuid NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "Token" character varying(200) NOT NULL,
    "ExpiraEn" timestamp with time zone NOT NULL,
    "Revocado" boolean NOT NULL,
    "RevocadoEn" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_refresh_tokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_refresh_tokens_usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuarios ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_refresh_tokens_Token" ON refresh_tokens ("Token");

CREATE INDEX "IX_refresh_tokens_UsuarioId" ON refresh_tokens ("UsuarioId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003155348_RefreshTokens', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE productos DROP CONSTRAINT "FK_productos_unidades_medida_UnidadMedidaId";

DROP INDEX "IX_productos_Sku";

DROP INDEX "IX_productos_UnidadMedidaId";

ALTER TABLE productos DROP COLUMN "CodigoBarras";

ALTER TABLE productos DROP COLUMN "PrecioCompraUSD";

ALTER TABLE productos DROP COLUMN "PrecioVentaUSD";

ALTER TABLE productos DROP COLUMN "Stock";

ALTER TABLE productos DROP COLUMN "StockMaximo";

ALTER TABLE productos DROP COLUMN "StockMinimo";

ALTER TABLE productos DROP COLUMN "UnidadMedidaId";

ALTER TABLE productos RENAME COLUMN "Sku" TO "Tipo";

ALTER TABLE productos ALTER COLUMN "Nombre" TYPE character varying(120);

ALTER TABLE productos ALTER COLUMN "MarcaId" DROP NOT NULL;

ALTER TABLE productos ADD "GradoAlcoholico" numeric(5,2);

ALTER TABLE productos ADD "ImpuestoId" uuid;

ALTER TABLE categorias ADD "Activo" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE categorias ADD "CategoriaPadreId" uuid;

CREATE TABLE impuestos (
    "Id" uuid NOT NULL,
    "Nombre" character varying(50) NOT NULL,
    "Porcentaje" numeric(5,2) NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_impuestos" PRIMARY KEY ("Id")
);

CREATE TABLE listas_precio (
    "Id" uuid NOT NULL,
    "Nombre" character varying(50) NOT NULL,
    "Descripcion" character varying(200),
    "EsPredeterminada" boolean NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_listas_precio" PRIMARY KEY ("Id")
);

CREATE TABLE producto_variantes (
    "Id" uuid NOT NULL,
    "ProductoId" uuid NOT NULL,
    "Nombre" character varying(120) NOT NULL,
    "Sku" character varying(50) NOT NULL,
    "PrecioCompraUSD" numeric(18,2) NOT NULL,
    "PrecioVentaUSD" numeric(18,2) NOT NULL,
    "Activo" boolean NOT NULL,
    "UnidadMedidaId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_producto_variantes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_producto_variantes_productos_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES productos ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_producto_variantes_unidades_medida_UnidadMedidaId" FOREIGN KEY ("UnidadMedidaId") REFERENCES unidades_medida ("Id") ON DELETE RESTRICT
);

CREATE TABLE codigos_barras (
    "Id" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Codigo" character varying(50) NOT NULL,
    "Principal" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_codigos_barras" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_codigos_barras_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE CASCADE
);

CREATE TABLE precios_producto (
    "Id" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "ListaPrecioId" uuid NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "Precio" numeric(18,2) NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_precios_producto" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_precios_producto_listas_precio_ListaPrecioId" FOREIGN KEY ("ListaPrecioId") REFERENCES listas_precio ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_precios_producto_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE CASCADE
);

CREATE TABLE recetas (
    "Id" uuid NOT NULL,
    "ProductoId" uuid NOT NULL,
    "VarianteInsumoId" uuid NOT NULL,
    "Cantidad" numeric(14,3) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_recetas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_recetas_producto_variantes_VarianteInsumoId" FOREIGN KEY ("VarianteInsumoId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_recetas_productos_ProductoId" FOREIGN KEY ("ProductoId") REFERENCES productos ("Id") ON DELETE CASCADE
);

UPDATE categorias SET "Activo" = TRUE, "CategoriaPadreId" = NULL
WHERE "Id" = '11111111-1111-1111-1111-111111111111';

UPDATE categorias SET "Activo" = TRUE, "CategoriaPadreId" = NULL
WHERE "Id" = '22222222-2222-2222-2222-222222222222';

UPDATE categorias SET "Activo" = TRUE, "CategoriaPadreId" = NULL
WHERE "Id" = '33333333-3333-3333-3333-333333333333';

UPDATE categorias SET "Activo" = TRUE, "CategoriaPadreId" = NULL
WHERE "Id" = '44444444-4444-4444-4444-444444444444';

INSERT INTO impuestos ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "Porcentaje", "UpdatedBy")
VALUES ('77777777-7777-7777-7777-777777777771', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'IVA', 16.0, NULL);
INSERT INTO impuestos ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "Porcentaje", "UpdatedBy")
VALUES ('77777777-7777-7777-7777-777777777772', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'IGTF', 3.0, NULL);

INSERT INTO listas_precio ("Id", "Activo", "CreatedAt", "CreatedBy", "Descripcion", "EsPredeterminada", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy")
VALUES ('88888888-8888-8888-8888-888888888881', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, 'Precio de venta al público.', TRUE, FALSE, NULL, 'Detal', NULL);
INSERT INTO listas_precio ("Id", "Activo", "CreatedAt", "CreatedBy", "Descripcion", "EsPredeterminada", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy")
VALUES ('88888888-8888-8888-8888-888888888882', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, 'Precio para compras al mayor.', FALSE, FALSE, NULL, 'Mayorista', NULL);

INSERT INTO producto_variantes ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "ProductoId", "Sku", "UnidadMedidaId", "UpdatedBy")
VALUES ('30000000-0000-0000-0000-000000000001', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Botella 0.75L', 8.5, 12.0, '10000000-0000-0000-0000-000000000001', 'LIC-RON-0001', '66666666-6666-6666-6666-666666666661', NULL);
INSERT INTO producto_variantes ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "ProductoId", "Sku", "UnidadMedidaId", "UpdatedBy")
VALUES ('30000000-0000-0000-0000-000000000002', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Botella 0.75L', 28.0, 38.0, '10000000-0000-0000-0000-000000000002', 'LIC-WHI-0002', '66666666-6666-6666-6666-666666666661', NULL);
INSERT INTO producto_variantes ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "ProductoId", "Sku", "UnidadMedidaId", "UpdatedBy")
VALUES ('30000000-0000-0000-0000-000000000003', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Botella 2L', 1.6, 2.5, '10000000-0000-0000-0000-000000000003', 'GAS-COC-0003', '66666666-6666-6666-6666-666666666661', NULL);
INSERT INTO producto_variantes ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "ProductoId", "Sku", "UnidadMedidaId", "UpdatedBy")
VALUES ('30000000-0000-0000-0000-000000000004', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Lata 250ml', 1.8, 3.0, '10000000-0000-0000-0000-000000000004', 'ENE-RED-0004', '66666666-6666-6666-6666-666666666662', NULL);
INSERT INTO producto_variantes ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "PrecioCompraUSD", "PrecioVentaUSD", "ProductoId", "Sku", "UnidadMedidaId", "UpdatedBy")
VALUES ('30000000-0000-0000-0000-000000000005', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Paquete 150g', 1.2, 2.0, '10000000-0000-0000-0000-000000000005', 'PAS-DOR-0005', '66666666-6666-6666-6666-666666666662', NULL);

UPDATE productos SET "Descripcion" = 'Ron añejo venezolano.', "GradoAlcoholico" = 40.0, "ImpuestoId" = '77777777-7777-7777-7777-777777777771', "Nombre" = 'Ron Cacique Añejo', "Tipo" = 'Simple'
WHERE "Id" = '10000000-0000-0000-0000-000000000001';

UPDATE productos SET "Descripcion" = 'Whisky escocés de 12 años.', "GradoAlcoholico" = 40.0, "ImpuestoId" = '77777777-7777-7777-7777-777777777771', "Nombre" = 'Whisky Old Parr 12 Años', "Tipo" = 'Simple'
WHERE "Id" = '10000000-0000-0000-0000-000000000002';

UPDATE productos SET "Descripcion" = 'Refresco de cola.', "GradoAlcoholico" = NULL, "ImpuestoId" = '77777777-7777-7777-7777-777777777771', "Nombre" = 'Coca-Cola', "Tipo" = 'Simple'
WHERE "Id" = '10000000-0000-0000-0000-000000000003';

UPDATE productos SET "Descripcion" = 'Bebida energizante.', "GradoAlcoholico" = NULL, "ImpuestoId" = '77777777-7777-7777-7777-777777777771', "Nombre" = 'Red Bull', "Tipo" = 'Simple'
WHERE "Id" = '10000000-0000-0000-0000-000000000004';

UPDATE productos SET "Descripcion" = 'Snack de maíz sabor queso.', "GradoAlcoholico" = NULL, "ImpuestoId" = '77777777-7777-7777-7777-777777777771', "Nombre" = 'Doritos Queso Atrevido', "Tipo" = 'Simple'
WHERE "Id" = '10000000-0000-0000-0000-000000000005';

INSERT INTO codigos_barras ("Id", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Principal", "UpdatedBy", "VarianteId")
VALUES ('40000000-0000-0000-0000-000000000001', '759100100101', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, TRUE, NULL, '30000000-0000-0000-0000-000000000001');
INSERT INTO codigos_barras ("Id", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Principal", "UpdatedBy", "VarianteId")
VALUES ('40000000-0000-0000-0000-000000000002', '500028100202', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, TRUE, NULL, '30000000-0000-0000-0000-000000000002');
INSERT INTO codigos_barras ("Id", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Principal", "UpdatedBy", "VarianteId")
VALUES ('40000000-0000-0000-0000-000000000003', '759100200303', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, TRUE, NULL, '30000000-0000-0000-0000-000000000003');
INSERT INTO codigos_barras ("Id", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Principal", "UpdatedBy", "VarianteId")
VALUES ('40000000-0000-0000-0000-000000000004', '900249010001', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, TRUE, NULL, '30000000-0000-0000-0000-000000000004');
INSERT INTO codigos_barras ("Id", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Principal", "UpdatedBy", "VarianteId")
VALUES ('40000000-0000-0000-0000-000000000005', '759100400505', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, TRUE, NULL, '30000000-0000-0000-0000-000000000005');

INSERT INTO precios_producto ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "ListaPrecioId", "Moneda", "Precio", "UpdatedBy", "VarianteId")
VALUES ('90000000-0000-0000-0000-000000000001', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, '88888888-8888-8888-8888-888888888881', 'USD', 12.0, NULL, '30000000-0000-0000-0000-000000000001');
INSERT INTO precios_producto ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "ListaPrecioId", "Moneda", "Precio", "UpdatedBy", "VarianteId")
VALUES ('90000000-0000-0000-0000-000000000002', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, '88888888-8888-8888-8888-888888888881', 'USD', 38.0, NULL, '30000000-0000-0000-0000-000000000002');
INSERT INTO precios_producto ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "ListaPrecioId", "Moneda", "Precio", "UpdatedBy", "VarianteId")
VALUES ('90000000-0000-0000-0000-000000000003', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, '88888888-8888-8888-8888-888888888881', 'USD', 2.5, NULL, '30000000-0000-0000-0000-000000000003');
INSERT INTO precios_producto ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "ListaPrecioId", "Moneda", "Precio", "UpdatedBy", "VarianteId")
VALUES ('90000000-0000-0000-0000-000000000004', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, '88888888-8888-8888-8888-888888888881', 'USD', 3.0, NULL, '30000000-0000-0000-0000-000000000004');
INSERT INTO precios_producto ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "ListaPrecioId", "Moneda", "Precio", "UpdatedBy", "VarianteId")
VALUES ('90000000-0000-0000-0000-000000000005', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, '88888888-8888-8888-8888-888888888881', 'USD', 2.0, NULL, '30000000-0000-0000-0000-000000000005');

CREATE INDEX "IX_productos_ImpuestoId" ON productos ("ImpuestoId");

CREATE INDEX "IX_categorias_CategoriaPadreId" ON categorias ("CategoriaPadreId");

CREATE UNIQUE INDEX "IX_codigos_barras_Codigo" ON codigos_barras ("Codigo");

CREATE INDEX "IX_codigos_barras_VarianteId" ON codigos_barras ("VarianteId");

CREATE INDEX "IX_precios_producto_ListaPrecioId" ON precios_producto ("ListaPrecioId");

CREATE UNIQUE INDEX "IX_precios_producto_VarianteId_ListaPrecioId_Moneda" ON precios_producto ("VarianteId", "ListaPrecioId", "Moneda");

CREATE INDEX "IX_producto_variantes_ProductoId" ON producto_variantes ("ProductoId");

CREATE UNIQUE INDEX "IX_producto_variantes_Sku" ON producto_variantes ("Sku");

CREATE INDEX "IX_producto_variantes_UnidadMedidaId" ON producto_variantes ("UnidadMedidaId");

CREATE UNIQUE INDEX "IX_recetas_ProductoId_VarianteInsumoId" ON recetas ("ProductoId", "VarianteInsumoId");

CREATE INDEX "IX_recetas_VarianteInsumoId" ON recetas ("VarianteInsumoId");

ALTER TABLE categorias ADD CONSTRAINT "FK_categorias_categorias_CategoriaPadreId" FOREIGN KEY ("CategoriaPadreId") REFERENCES categorias ("Id") ON DELETE RESTRICT;

ALTER TABLE productos ADD CONSTRAINT "FK_productos_impuestos_ImpuestoId" FOREIGN KEY ("ImpuestoId") REFERENCES impuestos ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003155950_CatalogoVariantes', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE movimiento_inventario (
    "Id" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Tipo" character varying(20) NOT NULL,
    "Cantidad" numeric(14,3) NOT NULL,
    "CostoUnitario" numeric(18,2),
    "ReferenciaTipo" character varying(40),
    "ReferenciaId" uuid,
    "Motivo" character varying(200),
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_movimiento_inventario" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_movimiento_inventario_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT
);

CREATE TABLE stock_producto (
    "Id" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Cantidad" numeric(14,3) NOT NULL,
    "CantidadReservada" numeric(14,3) NOT NULL,
    "StockMinimo" numeric(14,3) NOT NULL,
    "StockMaximo" numeric(14,3) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_stock_producto" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_stock_producto_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT
);

CREATE TABLE mermas (
    "Id" uuid NOT NULL,
    "MovimientoId" uuid NOT NULL,
    "Motivo" character varying(20) NOT NULL,
    "Repuesto" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_mermas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_mermas_movimiento_inventario_MovimientoId" FOREIGN KEY ("MovimientoId") REFERENCES movimiento_inventario ("Id") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_mermas_MovimientoId" ON mermas ("MovimientoId");

CREATE INDEX "IX_movimiento_inventario_VarianteId" ON movimiento_inventario ("VarianteId");

CREATE UNIQUE INDEX "IX_stock_producto_VarianteId" ON stock_producto ("VarianteId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003160314_InventarioKardex', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE movimientos_tesoreria (
    "Id" uuid NOT NULL,
    "Tipo" character varying(20) NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "Motivo" character varying(200) NOT NULL,
    "ReferenciaTipo" character varying(40),
    "ReferenciaId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_movimientos_tesoreria" PRIMARY KEY ("Id")
);

CREATE TABLE tasas_cambio (
    "Id" uuid NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "Tipo" character varying(20) NOT NULL,
    "Valor" numeric(18,4) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_tasas_cambio" PRIMARY KEY ("Id")
);

INSERT INTO tasas_cambio ("Id", "CreatedAt", "CreatedBy", "Fecha", "IsDeleted", "LastModifiedAt", "Tipo", "UpdatedBy", "Valor")
VALUES ('aaaa0000-0000-0000-0000-000000000001', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, TIMESTAMPTZ '2026-09-23T00:00:00Z', FALSE, NULL, 'BCV', NULL, 36.5);
INSERT INTO tasas_cambio ("Id", "CreatedAt", "CreatedBy", "Fecha", "IsDeleted", "LastModifiedAt", "Tipo", "UpdatedBy", "Valor")
VALUES ('aaaa0000-0000-0000-0000-000000000002', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, TIMESTAMPTZ '2026-09-23T00:00:00Z', FALSE, NULL, 'Paralelo', NULL, 40.0);

CREATE UNIQUE INDEX "IX_tasas_cambio_Fecha_Tipo" ON tasas_cambio ("Fecha", "Tipo");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003160631_FinanzasTasasTesoreria', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE detalles_venta DROP CONSTRAINT "FK_detalles_venta_productos_ProductoId";

ALTER TABLE ventas DROP COLUMN "MetodoPago";

ALTER TABLE detalles_venta RENAME COLUMN "ProductoId" TO "VarianteId";

ALTER INDEX "IX_detalles_venta_ProductoId" RENAME TO "IX_detalles_venta_VarianteId";

ALTER TABLE ventas ADD "CuentaId" uuid;

ALTER TABLE ventas ADD "DescuentoUSD" numeric(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE ventas ADD "Estado" character varying(20) NOT NULL DEFAULT '';

ALTER TABLE ventas ADD "SubtotalUSD" numeric(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE detalles_venta ALTER COLUMN "Cantidad" TYPE numeric(14,3);

ALTER TABLE detalles_venta ADD "DescuentoUSD" numeric(18,2) NOT NULL DEFAULT 0.0;

ALTER TABLE detalles_venta ADD "EsCortesia" boolean NOT NULL DEFAULT FALSE;

CREATE TABLE comprobantes_fiscales (
    "Id" uuid NOT NULL,
    "VentaId" uuid NOT NULL,
    "Numero" character varying(20) NOT NULL,
    "NumeroControl" character varying(20) NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "TotalUSD" numeric(18,2) NOT NULL,
    "TotalBS" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_comprobantes_fiscales" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_comprobantes_fiscales_ventas_VentaId" FOREIGN KEY ("VentaId") REFERENCES ventas ("Id") ON DELETE CASCADE
);

CREATE TABLE devoluciones (
    "Id" uuid NOT NULL,
    "VentaId" uuid NOT NULL,
    "Motivo" character varying(200) NOT NULL,
    "MontoUSD" numeric(18,2) NOT NULL,
    "ReintegrarInventario" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_devoluciones" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_devoluciones_ventas_VentaId" FOREIGN KEY ("VentaId") REFERENCES ventas ("Id") ON DELETE CASCADE
);

CREATE TABLE metodos_pago (
    "Id" uuid NOT NULL,
    "Codigo" character varying(30) NOT NULL,
    "Nombre" character varying(60) NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_metodos_pago" PRIMARY KEY ("Id")
);

CREATE TABLE devolucion_detalles (
    "Id" uuid NOT NULL,
    "DevolucionId" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Cantidad" numeric(14,3) NOT NULL,
    "PrecioUnitarioUSD" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_devolucion_detalles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_devolucion_detalles_devoluciones_DevolucionId" FOREIGN KEY ("DevolucionId") REFERENCES devoluciones ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_devolucion_detalles_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT
);

CREATE TABLE pagos (
    "Id" uuid NOT NULL,
    "VentaId" uuid NOT NULL,
    "MetodoPagoId" uuid NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "Propina" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_pagos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_pagos_metodos_pago_MetodoPagoId" FOREIGN KEY ("MetodoPagoId") REFERENCES metodos_pago ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_pagos_ventas_VentaId" FOREIGN KEY ("VentaId") REFERENCES ventas ("Id") ON DELETE CASCADE
);

INSERT INTO metodos_pago ("Id", "Activo", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy")
VALUES ('bbbb0000-0000-0000-0000-000000000001', TRUE, 'EfectivoUSD', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Efectivo en dólares', NULL);
INSERT INTO metodos_pago ("Id", "Activo", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy")
VALUES ('bbbb0000-0000-0000-0000-000000000002', TRUE, 'EfectivoBS', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Efectivo en bolívares', NULL);
INSERT INTO metodos_pago ("Id", "Activo", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy")
VALUES ('bbbb0000-0000-0000-0000-000000000003', TRUE, 'PagoMovil', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Pago móvil', NULL);
INSERT INTO metodos_pago ("Id", "Activo", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy")
VALUES ('bbbb0000-0000-0000-0000-000000000004', TRUE, 'Zelle', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Zelle', NULL);
INSERT INTO metodos_pago ("Id", "Activo", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy")
VALUES ('bbbb0000-0000-0000-0000-000000000005', TRUE, 'Punto', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Punto de venta', NULL);
INSERT INTO metodos_pago ("Id", "Activo", "Codigo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Nombre", "UpdatedBy")
VALUES ('bbbb0000-0000-0000-0000-000000000006', TRUE, 'Credito', TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'Crédito', NULL);

CREATE INDEX "IX_ventas_Fecha" ON ventas ("Fecha");

CREATE UNIQUE INDEX "IX_comprobantes_fiscales_VentaId" ON comprobantes_fiscales ("VentaId");

CREATE INDEX "IX_devolucion_detalles_DevolucionId" ON devolucion_detalles ("DevolucionId");

CREATE INDEX "IX_devolucion_detalles_VarianteId" ON devolucion_detalles ("VarianteId");

CREATE INDEX "IX_devoluciones_VentaId" ON devoluciones ("VentaId");

CREATE UNIQUE INDEX "IX_metodos_pago_Codigo" ON metodos_pago ("Codigo");

CREATE INDEX "IX_pagos_MetodoPagoId" ON pagos ("MetodoPagoId");

CREATE INDEX "IX_pagos_VentaId" ON pagos ("VentaId");

ALTER TABLE detalles_venta ADD CONSTRAINT "FK_detalles_venta_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003161003_VentasPagosComprobante', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE sesiones_mesa (
    "Id" uuid NOT NULL,
    "NombreMesa" character varying(60) NOT NULL,
    "MesaId" uuid,
    "AbiertaEn" timestamp with time zone NOT NULL,
    "CerradaEn" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_sesiones_mesa" PRIMARY KEY ("Id")
);

CREATE TABLE cuentas (
    "Id" uuid NOT NULL,
    "SesionMesaId" uuid NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "Total" numeric(18,2) NOT NULL,
    "TotalAbonado" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_cuentas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_cuentas_sesiones_mesa_SesionMesaId" FOREIGN KEY ("SesionMesaId") REFERENCES sesiones_mesa ("Id") ON DELETE CASCADE
);

CREATE TABLE abonos (
    "Id" uuid NOT NULL,
    "CuentaId" uuid NOT NULL,
    "MetodoPagoId" uuid NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_abonos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_abonos_cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES cuentas ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_abonos_metodos_pago_MetodoPagoId" FOREIGN KEY ("MetodoPagoId") REFERENCES metodos_pago ("Id") ON DELETE RESTRICT
);

CREATE TABLE comandas (
    "Id" uuid NOT NULL,
    "CuentaId" uuid NOT NULL,
    "Area" character varying(20) NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_comandas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_comandas_cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES cuentas ("Id") ON DELETE CASCADE
);

CREATE TABLE comanda_detalles (
    "Id" uuid NOT NULL,
    "ComandaId" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Cantidad" numeric(14,3) NOT NULL,
    "PrecioUnitarioUSD" numeric(18,2) NOT NULL,
    "AreaDestino" character varying(20) NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "EsCortesia" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_comanda_detalles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_comanda_detalles_comandas_ComandaId" FOREIGN KEY ("ComandaId") REFERENCES comandas ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_comanda_detalles_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_abonos_CuentaId" ON abonos ("CuentaId");

CREATE INDEX "IX_abonos_MetodoPagoId" ON abonos ("MetodoPagoId");

CREATE INDEX "IX_comanda_detalles_ComandaId" ON comanda_detalles ("ComandaId");

CREATE INDEX "IX_comanda_detalles_VarianteId" ON comanda_detalles ("VarianteId");

CREATE INDEX "IX_comandas_CuentaId" ON comandas ("CuentaId");

CREATE UNIQUE INDEX "IX_cuentas_SesionMesaId" ON cuentas ("SesionMesaId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003161450_CuentasComandas', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE denominaciones (
    "Id" uuid NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "Tipo" character varying(10) NOT NULL,
    "Valor" numeric(18,2) NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_denominaciones" PRIMARY KEY ("Id")
);

CREATE TABLE sesiones_caja (
    "Id" uuid NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "FondoInicial" numeric(18,2) NOT NULL,
    "MontoEsperado" numeric(18,2) NOT NULL,
    "MontoContado" numeric(18,2) NOT NULL,
    "Descuadre" numeric(18,2) NOT NULL,
    "AbiertaEn" timestamp with time zone NOT NULL,
    "CerradaEn" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_sesiones_caja" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_sesiones_caja_usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuarios ("Id") ON DELETE RESTRICT
);

CREATE TABLE arqueos_denominacion (
    "Id" uuid NOT NULL,
    "SesionCajaId" uuid NOT NULL,
    "DenominacionId" uuid NOT NULL,
    "Cantidad" integer NOT NULL,
    "Subtotal" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_arqueos_denominacion" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_arqueos_denominacion_denominaciones_DenominacionId" FOREIGN KEY ("DenominacionId") REFERENCES denominaciones ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_arqueos_denominacion_sesiones_caja_SesionCajaId" FOREIGN KEY ("SesionCajaId") REFERENCES sesiones_caja ("Id") ON DELETE CASCADE
);

CREATE TABLE movimientos_caja (
    "Id" uuid NOT NULL,
    "SesionCajaId" uuid NOT NULL,
    "Tipo" character varying(20) NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "Motivo" character varying(200) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_movimientos_caja" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_movimientos_caja_sesiones_caja_SesionCajaId" FOREIGN KEY ("SesionCajaId") REFERENCES sesiones_caja ("Id") ON DELETE CASCADE
);

INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000001', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'USD', 'Billete', NULL, 1.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000002', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'USD', 'Billete', NULL, 5.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000003', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'USD', 'Billete', NULL, 10.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000004', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'USD', 'Billete', NULL, 20.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000005', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'USD', 'Billete', NULL, 50.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000006', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'USD', 'Billete', NULL, 100.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000007', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'BS', 'Billete', NULL, 20.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000008', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'BS', 'Billete', NULL, 50.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000009', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'BS', 'Billete', NULL, 100.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000010', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'BS', 'Billete', NULL, 500.0);
INSERT INTO denominaciones ("Id", "Activo", "CreatedAt", "CreatedBy", "IsDeleted", "LastModifiedAt", "Moneda", "Tipo", "UpdatedBy", "Valor")
VALUES ('cccc0000-0000-0000-0000-000000000011', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', NULL, FALSE, NULL, 'BS', 'Billete', NULL, 1000.0);

CREATE INDEX "IX_arqueos_denominacion_DenominacionId" ON arqueos_denominacion ("DenominacionId");

CREATE INDEX "IX_arqueos_denominacion_SesionCajaId" ON arqueos_denominacion ("SesionCajaId");

CREATE INDEX "IX_movimientos_caja_SesionCajaId" ON movimientos_caja ("SesionCajaId");

CREATE INDEX "IX_sesiones_caja_UsuarioId" ON sesiones_caja ("UsuarioId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003162015_CajaSesiones', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE eventos (
    "Id" uuid NOT NULL,
    "Titulo" character varying(120) NOT NULL,
    "Descripcion" character varying(1000) NOT NULL,
    "FechaInicio" timestamp with time zone NOT NULL,
    "FechaFin" timestamp with time zone,
    "ImagenUrl" character varying(500),
    "Publicado" boolean NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_eventos" PRIMARY KEY ("Id")
);

CREATE TABLE planos (
    "Id" uuid NOT NULL,
    "Nombre" character varying(60) NOT NULL,
    "Version" integer NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_planos" PRIMARY KEY ("Id")
);

CREATE TABLE reservas (
    "Id" uuid NOT NULL,
    "FechaHora" timestamp with time zone NOT NULL,
    "Personas" integer NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "Origen" character varying(20) NOT NULL,
    "NombreContacto" character varying(120) NOT NULL,
    "Telefono" character varying(30) NOT NULL,
    "Notas" character varying(300),
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_reservas" PRIMARY KEY ("Id")
);

CREATE TABLE zonas (
    "Id" uuid NOT NULL,
    "Nombre" character varying(60) NOT NULL,
    "Tipo" character varying(20) NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_zonas" PRIMARY KEY ("Id")
);

CREATE TABLE reserva_pagos (
    "Id" uuid NOT NULL,
    "ReservaId" uuid NOT NULL,
    "MetodoPagoId" uuid NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "ComprobanteUrl" character varying(500),
    "Estado" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_reserva_pagos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_reserva_pagos_metodos_pago_MetodoPagoId" FOREIGN KEY ("MetodoPagoId") REFERENCES metodos_pago ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_reserva_pagos_reservas_ReservaId" FOREIGN KEY ("ReservaId") REFERENCES reservas ("Id") ON DELETE CASCADE
);

CREATE TABLE mesas (
    "Id" uuid NOT NULL,
    "ZonaId" uuid NOT NULL,
    "Numero" character varying(20) NOT NULL,
    "Capacidad" integer NOT NULL,
    "Forma" character varying(20) NOT NULL,
    "PosX" numeric(10,2) NOT NULL,
    "PosY" numeric(10,2) NOT NULL,
    "Ancho" numeric(10,2) NOT NULL,
    "Alto" numeric(10,2) NOT NULL,
    "Activa" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_mesas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_mesas_zonas_ZonaId" FOREIGN KEY ("ZonaId") REFERENCES zonas ("Id") ON DELETE RESTRICT
);

CREATE TABLE plano_elementos (
    "Id" uuid NOT NULL,
    "PlanoId" uuid NOT NULL,
    "ZonaId" uuid,
    "Tipo" character varying(30) NOT NULL,
    "Etiqueta" character varying(60),
    "PosX" numeric(10,2) NOT NULL,
    "PosY" numeric(10,2) NOT NULL,
    "Ancho" numeric(10,2) NOT NULL,
    "Alto" numeric(10,2) NOT NULL,
    "Rotacion" numeric(10,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_plano_elementos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_plano_elementos_planos_PlanoId" FOREIGN KEY ("PlanoId") REFERENCES planos ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_plano_elementos_zonas_ZonaId" FOREIGN KEY ("ZonaId") REFERENCES zonas ("Id") ON DELETE SET NULL
);

CREATE TABLE reserva_mesas (
    "Id" uuid NOT NULL,
    "ReservaId" uuid NOT NULL,
    "MesaId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_reserva_mesas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_reserva_mesas_mesas_MesaId" FOREIGN KEY ("MesaId") REFERENCES mesas ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_reserva_mesas_reservas_ReservaId" FOREIGN KEY ("ReservaId") REFERENCES reservas ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_eventos_FechaInicio" ON eventos ("FechaInicio");

CREATE INDEX "IX_mesas_ZonaId" ON mesas ("ZonaId");

CREATE INDEX "IX_plano_elementos_PlanoId" ON plano_elementos ("PlanoId");

CREATE INDEX "IX_plano_elementos_ZonaId" ON plano_elementos ("ZonaId");

CREATE INDEX "IX_reserva_mesas_MesaId" ON reserva_mesas ("MesaId");

CREATE UNIQUE INDEX "IX_reserva_mesas_ReservaId_MesaId" ON reserva_mesas ("ReservaId", "MesaId");

CREATE INDEX "IX_reserva_pagos_MetodoPagoId" ON reserva_pagos ("MetodoPagoId");

CREATE INDEX "IX_reserva_pagos_ReservaId" ON reserva_pagos ("ReservaId");

CREATE INDEX "IX_reservas_FechaHora" ON reservas ("FechaHora");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003162330_ClubReservasEventos', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE clientes (
    "Id" uuid NOT NULL,
    "Nombre" character varying(120) NOT NULL,
    "Rif" character varying(20),
    "Ci" character varying(20),
    "Email" character varying(120),
    "Telefono" character varying(30),
    "Direccion" character varying(300),
    "Puntos" integer NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_clientes" PRIMARY KEY ("Id")
);

CREATE TABLE cuentas_por_cobrar (
    "Id" uuid NOT NULL,
    "ClienteId" uuid NOT NULL,
    "MontoUSD" numeric(18,2) NOT NULL,
    "SaldoUSD" numeric(18,2) NOT NULL,
    "Vencimiento" timestamp with time zone NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_cuentas_por_cobrar" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_cuentas_por_cobrar_clientes_ClienteId" FOREIGN KEY ("ClienteId") REFERENCES clientes ("Id") ON DELETE RESTRICT
);

CREATE TABLE puntos_movimiento (
    "Id" uuid NOT NULL,
    "ClienteId" uuid NOT NULL,
    "Tipo" character varying(20) NOT NULL,
    "Puntos" integer NOT NULL,
    "Motivo" character varying(200) NOT NULL,
    "ReferenciaTipo" character varying(40),
    "ReferenciaId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_puntos_movimiento" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_puntos_movimiento_clientes_ClienteId" FOREIGN KEY ("ClienteId") REFERENCES clientes ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_clientes_Rif" ON clientes ("Rif");

CREATE INDEX "IX_cuentas_por_cobrar_ClienteId" ON cuentas_por_cobrar ("ClienteId");

CREATE INDEX "IX_puntos_movimiento_ClienteId" ON puntos_movimiento ("ClienteId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003162545_CrmClientes', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE horarios_atencion (
    "Id" uuid NOT NULL,
    "DiaSemana" integer NOT NULL,
    "Abierto" boolean NOT NULL,
    "HoraApertura" time without time zone,
    "HoraCierre" time without time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_horarios_atencion" PRIMARY KEY ("Id")
);

CREATE TABLE local_info (
    "Id" uuid NOT NULL,
    "Nombre" character varying(120) NOT NULL,
    "Descripcion" character varying(1000) NOT NULL,
    "Direccion" character varying(300) NOT NULL,
    "Telefono" character varying(30) NOT NULL,
    "Whatsapp" character varying(30) NOT NULL,
    "Email" character varying(120) NOT NULL,
    "Instagram" character varying(200),
    "Facebook" character varying(200),
    "MapaUrl" character varying(500),
    "LogoUrl" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_local_info" PRIMARY KEY ("Id")
);

CREATE TABLE media_assets (
    "Id" uuid NOT NULL,
    "Nombre" character varying(200) NOT NULL,
    "RutaRelativa" character varying(500) NOT NULL,
    "Url" character varying(500) NOT NULL,
    "Tipo" character varying(40) NOT NULL,
    "Tamano" bigint NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_media_assets" PRIMARY KEY ("Id")
);

CREATE TABLE paginas (
    "Id" uuid NOT NULL,
    "Titulo" character varying(120) NOT NULL,
    "Slug" character varying(120) NOT NULL,
    "Publicada" boolean NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_paginas" PRIMARY KEY ("Id")
);

CREATE TABLE secciones (
    "Id" uuid NOT NULL,
    "PaginaId" uuid NOT NULL,
    "Titulo" character varying(120) NOT NULL,
    "Tipo" character varying(40) NOT NULL,
    "Orden" integer NOT NULL,
    "Activa" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_secciones" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_secciones_paginas_PaginaId" FOREIGN KEY ("PaginaId") REFERENCES paginas ("Id") ON DELETE CASCADE
);

CREATE TABLE bloques_contenido (
    "Id" uuid NOT NULL,
    "SeccionId" uuid NOT NULL,
    "Tipo" character varying(40) NOT NULL,
    "Orden" integer NOT NULL,
    "Contenido" jsonb NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_bloques_contenido" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_bloques_contenido_secciones_SeccionId" FOREIGN KEY ("SeccionId") REFERENCES secciones ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_bloques_contenido_SeccionId" ON bloques_contenido ("SeccionId");

CREATE UNIQUE INDEX "IX_paginas_Slug" ON paginas ("Slug");

CREATE INDEX "IX_secciones_PaginaId" ON secciones ("PaginaId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003162839_ContenidoWeb', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE ai_generaciones (
    "Id" uuid NOT NULL,
    "Tipo" character varying(40) NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "Entrada" jsonb NOT NULL,
    "Salida" jsonb,
    "Modelo" character varying(80),
    "Costo" numeric(10,4) NOT NULL,
    "Aprobado" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_ai_generaciones" PRIMARY KEY ("Id")
);

CREATE TABLE planos_generados (
    "Id" uuid NOT NULL,
    "AiGeneracionId" uuid NOT NULL,
    "Resultado" jsonb NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_planos_generados" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_planos_generados_ai_generaciones_AiGeneracionId" FOREIGN KEY ("AiGeneracionId") REFERENCES ai_generaciones ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_planos_generados_AiGeneracionId" ON planos_generados ("AiGeneracionId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261003163220_IaSimulada', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE proveedores (
    "Id" uuid NOT NULL,
    "Nombre" character varying(120) NOT NULL,
    "Rif" character varying(20),
    "Contacto" character varying(120),
    "Telefono" character varying(30),
    "Email" character varying(120),
    "Direccion" character varying(300),
    "DiasCredito" integer NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_proveedores" PRIMARY KEY ("Id")
);

CREATE TABLE ordenes_compra (
    "Id" uuid NOT NULL,
    "Numero" character varying(20) NOT NULL,
    "ProveedorId" uuid NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "Observaciones" character varying(300),
    "TotalUSD" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_ordenes_compra" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ordenes_compra_proveedores_ProveedorId" FOREIGN KEY ("ProveedorId") REFERENCES proveedores ("Id") ON DELETE RESTRICT
);

CREATE TABLE orden_compra_detalles (
    "Id" uuid NOT NULL,
    "OrdenCompraId" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Cantidad" numeric(14,3) NOT NULL,
    "CostoUnitarioUSD" numeric(18,2) NOT NULL,
    "CantidadRecibida" numeric(14,3) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_orden_compra_detalles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_orden_compra_detalles_ordenes_compra_OrdenCompraId" FOREIGN KEY ("OrdenCompraId") REFERENCES ordenes_compra ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_orden_compra_detalles_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT
);

CREATE INDEX "IX_orden_compra_detalles_OrdenCompraId" ON orden_compra_detalles ("OrdenCompraId");

CREATE INDEX "IX_orden_compra_detalles_VarianteId" ON orden_compra_detalles ("VarianteId");

CREATE INDEX "IX_ordenes_compra_Fecha" ON ordenes_compra ("Fecha");

CREATE UNIQUE INDEX "IX_ordenes_compra_Numero" ON ordenes_compra ("Numero");

CREATE INDEX "IX_ordenes_compra_ProveedorId" ON ordenes_compra ("ProveedorId");

CREATE INDEX "IX_proveedores_Rif" ON proveedores ("Rif");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004145033_ComprasProveedores', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE recepciones (
    "Id" uuid NOT NULL,
    "OrdenCompraId" uuid NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "Observaciones" character varying(300),
    "TotalUSD" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_recepciones" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_recepciones_ordenes_compra_OrdenCompraId" FOREIGN KEY ("OrdenCompraId") REFERENCES ordenes_compra ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_recepciones_usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuarios ("Id") ON DELETE RESTRICT
);

CREATE TABLE recepcion_detalles (
    "Id" uuid NOT NULL,
    "RecepcionId" uuid NOT NULL,
    "OrdenCompraDetalleId" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Cantidad" numeric(14,3) NOT NULL,
    "CostoUnitarioUSD" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_recepcion_detalles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_recepcion_detalles_orden_compra_detalles_OrdenCompraDetalle~" FOREIGN KEY ("OrdenCompraDetalleId") REFERENCES orden_compra_detalles ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_recepcion_detalles_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_recepcion_detalles_recepciones_RecepcionId" FOREIGN KEY ("RecepcionId") REFERENCES recepciones ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_recepcion_detalles_OrdenCompraDetalleId" ON recepcion_detalles ("OrdenCompraDetalleId");

CREATE INDEX "IX_recepcion_detalles_RecepcionId" ON recepcion_detalles ("RecepcionId");

CREATE INDEX "IX_recepcion_detalles_VarianteId" ON recepcion_detalles ("VarianteId");

CREATE INDEX "IX_recepciones_Fecha" ON recepciones ("Fecha");

CREATE INDEX "IX_recepciones_OrdenCompraId" ON recepciones ("OrdenCompraId");

CREATE INDEX "IX_recepciones_UsuarioId" ON recepciones ("UsuarioId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004145441_RecepcionesCompra', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE cuentas_por_pagar (
    "Id" uuid NOT NULL,
    "ProveedorId" uuid NOT NULL,
    "OrdenCompraId" uuid,
    "RecepcionId" uuid,
    "MontoUSD" numeric(18,2) NOT NULL,
    "SaldoUSD" numeric(18,2) NOT NULL,
    "Vencimiento" timestamp with time zone NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_cuentas_por_pagar" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_cuentas_por_pagar_proveedores_ProveedorId" FOREIGN KEY ("ProveedorId") REFERENCES proveedores ("Id") ON DELETE RESTRICT
);

CREATE TABLE pagos_proveedor (
    "Id" uuid NOT NULL,
    "CuentaPorPagarId" uuid NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "MetodoPagoId" uuid,
    "Referencia" character varying(120),
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_pagos_proveedor" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_pagos_proveedor_cuentas_por_pagar_CuentaPorPagarId" FOREIGN KEY ("CuentaPorPagarId") REFERENCES cuentas_por_pagar ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_cuentas_por_pagar_ProveedorId" ON cuentas_por_pagar ("ProveedorId");

CREATE INDEX "IX_cuentas_por_pagar_Vencimiento" ON cuentas_por_pagar ("Vencimiento");

CREATE INDEX "IX_pagos_proveedor_CuentaPorPagarId" ON pagos_proveedor ("CuentaPorPagarId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004150015_CuentasPorPagarProveedor', '10.0.12');

COMMIT;

START TRANSACTION;
ALTER TABLE ventas ADD "PromocionId" uuid;

CREATE TABLE cuenta_divisiones (
    "Id" uuid NOT NULL,
    "CuentaId" uuid NOT NULL,
    "Indice" integer NOT NULL,
    "Monto" numeric(18,2) NOT NULL,
    "Pagada" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_cuenta_divisiones" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_cuenta_divisiones_cuentas_CuentaId" FOREIGN KEY ("CuentaId") REFERENCES cuentas ("Id") ON DELETE CASCADE
);

CREATE TABLE promociones (
    "Id" uuid NOT NULL,
    "Nombre" character varying(120) NOT NULL,
    "Tipo" character varying(20) NOT NULL,
    "Valor" numeric(18,2) NOT NULL,
    "Activo" boolean NOT NULL,
    "FechaInicio" timestamp with time zone,
    "FechaFin" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_promociones" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_cuenta_divisiones_CuentaId" ON cuenta_divisiones ("CuentaId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004152341_DivisionesPromociones', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE lotes (
    "Id" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Codigo" character varying(50) NOT NULL,
    "FechaVencimiento" timestamp with time zone,
    "Cantidad" numeric(14,3) NOT NULL,
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_lotes" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_lotes_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT
);

CREATE TABLE tomas_fisicas (
    "Id" uuid NOT NULL,
    "UsuarioId" uuid NOT NULL,
    "Fecha" timestamp with time zone NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "Observaciones" character varying(300),
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_tomas_fisicas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_tomas_fisicas_usuarios_UsuarioId" FOREIGN KEY ("UsuarioId") REFERENCES usuarios ("Id") ON DELETE RESTRICT
);

CREATE TABLE toma_fisica_detalles (
    "Id" uuid NOT NULL,
    "TomaFisicaId" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "CantidadSistema" numeric(14,3) NOT NULL,
    "CantidadContada" numeric(14,3) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_toma_fisica_detalles" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_toma_fisica_detalles_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_toma_fisica_detalles_tomas_fisicas_TomaFisicaId" FOREIGN KEY ("TomaFisicaId") REFERENCES tomas_fisicas ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_lotes_FechaVencimiento" ON lotes ("FechaVencimiento");

CREATE INDEX "IX_lotes_VarianteId" ON lotes ("VarianteId");

CREATE INDEX "IX_toma_fisica_detalles_TomaFisicaId" ON toma_fisica_detalles ("TomaFisicaId");

CREATE INDEX "IX_toma_fisica_detalles_VarianteId" ON toma_fisica_detalles ("VarianteId");

CREATE INDEX "IX_tomas_fisicas_UsuarioId" ON tomas_fisicas ("UsuarioId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004153236_LotesTomasFisicas', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE entradas (
    "Id" uuid NOT NULL,
    "Codigo" character varying(40) NOT NULL,
    "EventoId" uuid,
    "ReservaId" uuid,
    "ClienteId" uuid,
    "Precio" numeric(18,2) NOT NULL,
    "Moneda" character varying(10) NOT NULL,
    "Estado" character varying(20) NOT NULL,
    "EmitidaEn" timestamp with time zone NOT NULL,
    "UsadaEn" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_entradas" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_entradas_clientes_ClienteId" FOREIGN KEY ("ClienteId") REFERENCES clientes ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_entradas_eventos_EventoId" FOREIGN KEY ("EventoId") REFERENCES eventos ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_entradas_reservas_ReservaId" FOREIGN KEY ("ReservaId") REFERENCES reservas ("Id") ON DELETE SET NULL
);

CREATE TABLE lista_vip (
    "Id" uuid NOT NULL,
    "ClienteId" uuid,
    "Nombre" character varying(120) NOT NULL,
    "Documento" character varying(30),
    "Telefono" character varying(30),
    "Notas" character varying(300),
    "Activo" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_lista_vip" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_lista_vip_clientes_ClienteId" FOREIGN KEY ("ClienteId") REFERENCES clientes ("Id") ON DELETE SET NULL
);

CREATE TABLE pedidos_anticipados (
    "Id" uuid NOT NULL,
    "ReservaId" uuid NOT NULL,
    "VarianteId" uuid NOT NULL,
    "Cantidad" numeric(14,3) NOT NULL,
    "PrecioUnitarioUSD" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_pedidos_anticipados" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_pedidos_anticipados_producto_variantes_VarianteId" FOREIGN KEY ("VarianteId") REFERENCES producto_variantes ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_pedidos_anticipados_reservas_ReservaId" FOREIGN KEY ("ReservaId") REFERENCES reservas ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_entradas_ClienteId" ON entradas ("ClienteId");

CREATE UNIQUE INDEX "IX_entradas_Codigo" ON entradas ("Codigo");

CREATE INDEX "IX_entradas_EventoId" ON entradas ("EventoId");

CREATE INDEX "IX_entradas_ReservaId" ON entradas ("ReservaId");

CREATE INDEX "IX_lista_vip_ClienteId" ON lista_vip ("ClienteId");

CREATE INDEX "IX_pedidos_anticipados_ReservaId" ON pedidos_anticipados ("ReservaId");

CREATE INDEX "IX_pedidos_anticipados_VarianteId" ON pedidos_anticipados ("VarianteId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004153546_ClubVipEntradasPedidos', '10.0.12');

COMMIT;

START TRANSACTION;
CREATE TABLE audit_log (
    "Id" uuid NOT NULL,
    "UsuarioId" uuid,
    "Usuario" character varying(120),
    "Accion" character varying(80) NOT NULL,
    "Entidad" character varying(60) NOT NULL,
    "EntidadId" uuid,
    "Datos" jsonb,
    "Ip" character varying(60),
    "CreatedAt" timestamp with time zone NOT NULL,
    "LastModifiedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    CONSTRAINT "PK_audit_log" PRIMARY KEY ("Id")
);

CREATE INDEX "IX_audit_log_CreatedAt" ON audit_log ("CreatedAt");

CREATE INDEX "IX_audit_log_Entidad" ON audit_log ("Entidad");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004154111_Auditoria', '10.0.12');

COMMIT;

START TRANSACTION;
INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20261004163315_ConcurrenciaXmin', '10.0.12');

COMMIT;

