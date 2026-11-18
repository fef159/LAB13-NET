-- Semana 14: tabla Usuarios para la autenticación JWT (ejecutar sobre VeterinariaDB)
USE VeterinariaDB;
GO

IF OBJECT_ID('Usuarios') IS NOT NULL DROP TABLE Usuarios;
GO

CREATE TABLE Usuarios (
    Id         INT IDENTITY(1,1) PRIMARY KEY,
    Correo     NVARCHAR(100) NOT NULL UNIQUE,
    ClaveHash  NVARCHAR(100) NOT NULL,   -- hash BCrypt, nunca la clave en texto
    Rol        NVARCHAR(30)  NOT NULL    -- Recepcionista | Veterinario
);
GO

-- Un usuario por rol. Clave de prueba de ambos: Clave1234!
-- (los hashes se obtuvieron registrando los usuarios por la API y cambiando el rol con UPDATE)
INSERT INTO Usuarios (Correo, ClaveHash, Rol) VALUES
(N'recepcion@vet.com',    N'$2a$11$mm7krZPmakup1fDwIpSin.20FPzHmjMJ62PjZax/x98MHLnAvlv/m', N'Recepcionista'),
(N'veterinario@vet.com',  N'$2a$11$vl1ufmHRFxUeO73i9weWSOt9Z/c2JkTPKU3N5t7TaYy/lJeUPE23e', N'Veterinario');
GO
