CREATE SCHEMA IF NOT EXISTS auth;
CREATE SCHEMA IF NOT EXISTS orders;

CREATE TABLE auth."EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK_EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

CREATE TABLE orders."EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK_EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);