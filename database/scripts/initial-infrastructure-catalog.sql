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
VALUES ('20000000-0000-0000-0000-000000000001', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'admin@licoreria.com', FALSE, NULL, 'Administrador Principal', 'admin123_hash', 'Administrador');
INSERT INTO usuarios ("Id", "Activo", "CreatedAt", "Email", "IsDeleted", "LastModifiedAt", "NombreCompleto", "PasswordHash", "Rol")
VALUES ('20000000-0000-0000-0000-000000000002', TRUE, TIMESTAMPTZ '2026-09-23T00:00:00Z', 'cajero1@licoreria.com', FALSE, NULL, 'Cajero Turno Mañana', 'cajero123_hash', 'Cajero');

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

