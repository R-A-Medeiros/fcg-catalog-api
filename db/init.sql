CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260901171312_InitialCreate') THEN
    CREATE TABLE games (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(2000) NOT NULL,
        "Price" numeric(10,2) NOT NULL,
        "Genre" character varying(100) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_games" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260901171312_InitialCreate') THEN
    CREATE TABLE library_items (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "GameId" uuid NOT NULL,
        "GameTitle" character varying(200) NOT NULL,
        "PricePaid" numeric(10,2) NOT NULL,
        "AddedAt" timestamp with time zone NOT NULL,
        CONSTRAINT "PK_library_items" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260901171312_InitialCreate') THEN
    CREATE TABLE orders (
        "Id" uuid NOT NULL,
        "UserId" uuid NOT NULL,
        "GameId" uuid NOT NULL,
        "GameTitle" character varying(200) NOT NULL,
        "Price" numeric(10,2) NOT NULL,
        "Status" character varying(20) NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "ResolvedAt" timestamp with time zone,
        CONSTRAINT "PK_orders" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260901171312_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_library_items_UserId_GameId" ON library_items ("UserId", "GameId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260901171312_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260901171312_InitialCreate', '10.0.11');
    END IF;
END $EF$;
COMMIT;

