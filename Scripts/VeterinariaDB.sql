-- Script de base de datos: Clínica Veterinaria (SEMANA 13)
IF DB_ID('VeterinariaDB') IS NOT NULL
BEGIN
    ALTER DATABASE VeterinariaDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE VeterinariaDB;
END
GO
CREATE DATABASE VeterinariaDB;
GO
USE VeterinariaDB;
GO

CREATE TABLE Propietarios (
    Id           INT IDENTITY(1,1) PRIMARY KEY,
    Nombre       NVARCHAR(100) NOT NULL,
    Documento    NVARCHAR(20)  NOT NULL UNIQUE,
    Telefono     NVARCHAR(20)  NULL,
    Email        NVARCHAR(100) NULL
);

CREATE TABLE Mascotas (
    Id              INT IDENTITY(1,1) PRIMARY KEY,
    Nombre          NVARCHAR(80)  NOT NULL,
    Especie         NVARCHAR(40)  NOT NULL,
    Raza            NVARCHAR(60)  NULL,
    Edad            INT           NOT NULL CHECK (Edad BETWEEN 0 AND 40),
    Peso            DECIMAL(6,2)  NOT NULL CHECK (Peso > 0),
    FechaRegistro   DATE          NOT NULL DEFAULT (CAST(GETDATE() AS DATE)),
    PropietarioId   INT           NOT NULL,
    CONSTRAINT FK_Mascotas_Propietarios FOREIGN KEY (PropietarioId) REFERENCES Propietarios(Id)
);
GO

INSERT INTO Propietarios (Nombre, Documento, Telefono, Email) VALUES
(N'Carlos Méndez',  N'04567890-1', N'7777-1111', N'carlos@mail.com'),
(N'Lucía Hernández',N'01234567-8', N'7777-2222', N'lucia@mail.com'),
(N'Roberto Pineda', N'09876543-2', N'7777-3333', NULL);

INSERT INTO Mascotas (Nombre, Especie, Raza, Edad, Peso, PropietarioId) VALUES
(N'Max',   N'Perro', N'Labrador',  4, 28.50, 1),
(N'Luna',  N'Gato',  N'Siamés',    2,  4.20, 1),
(N'Rocky', N'Perro', N'Bulldog',   6, 22.00, 2),
(N'Nemo',  N'Pez',   NULL,         1,  0.05, 3);
GO
