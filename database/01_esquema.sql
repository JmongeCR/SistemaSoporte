/* =====================================================================
   Sistema de Tickets de Soporte Técnico — SC-701
   01_esquema.sql : base de datos, tablas, restricciones e índices (DDL)

   Orden de ejecución: 01_esquema → 02_catalogos → 03_procedimientos → 04_seed_pruebas
                       → 05_seguimiento_notificaciones (migración, también válida sobre una base existente)
   ATENCIÓN: este script elimina y recrea todas las tablas.
   ===================================================================== */

IF DB_ID(N'SoporteTecnico') IS NULL
    CREATE DATABASE SoporteTecnico;
GO

USE SoporteTecnico;
GO

-- Requeridos por los índices filtrados y guardados junto a cada procedimiento;
-- el sqlcmd clásico los trae apagados por defecto.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- Limpieza (orden inverso a las dependencias) ---------- */
DROP TABLE IF EXISTS dbo.Notificacion;   -- creada en 05_seguimiento_notificaciones.sql
DROP TABLE IF EXISTS dbo.LecturaComentario;
DROP TABLE IF EXISTS dbo.Adjunto;
DROP TABLE IF EXISTS dbo.Comentario;
DROP TABLE IF EXISTS dbo.HistorialTicket;
DROP TABLE IF EXISTS dbo.Ticket;
DROP TABLE IF EXISTS dbo.TokenRecuperacion;
DROP TABLE IF EXISTS dbo.RolPermiso;
DROP TABLE IF EXISTS dbo.Permiso;
DROP TABLE IF EXISTS dbo.Usuario;
DROP TABLE IF EXISTS dbo.Categoria;
DROP TABLE IF EXISTS dbo.Prioridad;
DROP TABLE IF EXISTS dbo.EstadoTicket;
DROP TABLE IF EXISTS dbo.Rol;
GO

/* =====================================================================
   Catálogos
   Rol, EstadoTicket y Prioridad usan Id fijo (sin IDENTITY) porque el
   código los referencia mediante los enums RolTipo, EstadoTicketTipo y
   PrioridadTipo.
   ===================================================================== */

CREATE TABLE dbo.Rol
(
    Id      INT          NOT NULL,
    Nombre  NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_Rol PRIMARY KEY (Id),
    CONSTRAINT UQ_Rol_Nombre UNIQUE (Nombre)
);

CREATE TABLE dbo.EstadoTicket
(
    Id      INT          NOT NULL,
    Nombre  NVARCHAR(30) NOT NULL,
    CONSTRAINT PK_EstadoTicket PRIMARY KEY (Id),
    CONSTRAINT UQ_EstadoTicket_Nombre UNIQUE (Nombre)
);

CREATE TABLE dbo.Prioridad
(
    Id       INT          NOT NULL,
    Nombre   NVARCHAR(20) NOT NULL,
    HorasSla INT          NOT NULL,   -- plazo de atención para la métrica "SLA vencido" (HU-12)
    CONSTRAINT PK_Prioridad PRIMARY KEY (Id),
    CONSTRAINT UQ_Prioridad_Nombre UNIQUE (Nombre),
    CONSTRAINT CK_Prioridad_HorasSla CHECK (HorasSla > 0)
);

CREATE TABLE dbo.Categoria
(
    Id      INT IDENTITY(1,1) NOT NULL,
    Nombre  NVARCHAR(50)      NOT NULL,
    Activa  BIT               NOT NULL CONSTRAINT DF_Categoria_Activa DEFAULT (1),
    CONSTRAINT PK_Categoria PRIMARY KEY (Id),
    CONSTRAINT UQ_Categoria_Nombre UNIQUE (Nombre)
);

/* =====================================================================
   Usuarios, permisos y recuperación de contraseña (HU-01, HU-02, HU-14..16)
   ===================================================================== */

CREATE TABLE dbo.Usuario
(
    Id                 INT IDENTITY(1,1) NOT NULL,
    Nombre             NVARCHAR(100)     NOT NULL,
    Correo             NVARCHAR(150)     NOT NULL,
    PasswordHash       NVARCHAR(255)     NOT NULL,
    RolId              INT               NOT NULL,
    Activo             BIT               NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT (1),
    FechaCreacion      DATETIME2         NOT NULL CONSTRAINT DF_Usuario_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    ContrasenaVersion  INT               NOT NULL CONSTRAINT DF_Usuario_ContrasenaVersion DEFAULT (0),
    CONSTRAINT PK_Usuario PRIMARY KEY (Id),
    CONSTRAINT UQ_Usuario_Correo UNIQUE (Correo),
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (RolId) REFERENCES dbo.Rol (Id)
);
CREATE INDEX IX_Usuario_RolId ON dbo.Usuario (RolId);

CREATE TABLE dbo.Permiso
(
    Id          INT IDENTITY(1,1) NOT NULL,
    Codigo      NVARCHAR(50)      NOT NULL,
    Descripcion NVARCHAR(200)     NOT NULL,
    Grupo       NVARCHAR(50)      NOT NULL,
    CONSTRAINT PK_Permiso PRIMARY KEY (Id),
    CONSTRAINT UQ_Permiso_Codigo UNIQUE (Codigo)
);

CREATE TABLE dbo.RolPermiso
(
    RolId     INT NOT NULL,
    PermisoId INT NOT NULL,
    CONSTRAINT PK_RolPermiso PRIMARY KEY (RolId, PermisoId),
    CONSTRAINT FK_RolPermiso_Rol     FOREIGN KEY (RolId)     REFERENCES dbo.Rol (Id),
    CONSTRAINT FK_RolPermiso_Permiso FOREIGN KEY (PermisoId) REFERENCES dbo.Permiso (Id)
);

CREATE TABLE dbo.TokenRecuperacion
(
    Id              INT IDENTITY(1,1) NOT NULL,
    UsuarioId       INT               NOT NULL,
    TokenHash       NVARCHAR(128)     NOT NULL,   -- SHA-256 del token; el token en claro solo viaja en el enlace
    FechaCreacion   DATETIME2         NOT NULL CONSTRAINT DF_TokenRecuperacion_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    FechaExpiracion DATETIME2         NOT NULL,
    Usado           BIT               NOT NULL CONSTRAINT DF_TokenRecuperacion_Usado DEFAULT (0),
    CONSTRAINT PK_TokenRecuperacion PRIMARY KEY (Id),
    CONSTRAINT UQ_TokenRecuperacion_TokenHash UNIQUE (TokenHash),
    CONSTRAINT FK_TokenRecuperacion_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario (Id),
    CONSTRAINT CK_TokenRecuperacion_Expiracion CHECK (FechaExpiracion > FechaCreacion)
);

/* =====================================================================
   Tickets (HU-04..08)
   ===================================================================== */

CREATE TABLE dbo.Ticket
(
    Id                 INT IDENTITY(1,1) NOT NULL,
    Asunto             NVARCHAR(150)     NOT NULL,
    Descripcion        NVARCHAR(4000)    NOT NULL,
    CategoriaId        INT               NOT NULL,
    PrioridadId        INT               NOT NULL,
    EstadoId           INT               NOT NULL CONSTRAINT DF_Ticket_EstadoId DEFAULT (1),   -- Abierto
    ClienteId          INT               NOT NULL,
    AgenteAsignadoId   INT               NULL,       -- NULL = sin asignar
    FechaCreacion      DATETIME2         NOT NULL CONSTRAINT DF_Ticket_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    FechaActualizacion DATETIME2         NULL,
    RowVersion         ROWVERSION        NOT NULL,   -- concurrencia optimista (HU-06, HU-07)
    CONSTRAINT PK_Ticket PRIMARY KEY (Id),
    CONSTRAINT FK_Ticket_Categoria      FOREIGN KEY (CategoriaId)      REFERENCES dbo.Categoria (Id),
    CONSTRAINT FK_Ticket_Prioridad      FOREIGN KEY (PrioridadId)      REFERENCES dbo.Prioridad (Id),
    CONSTRAINT FK_Ticket_Estado         FOREIGN KEY (EstadoId)         REFERENCES dbo.EstadoTicket (Id),
    CONSTRAINT FK_Ticket_Cliente        FOREIGN KEY (ClienteId)        REFERENCES dbo.Usuario (Id),
    CONSTRAINT FK_Ticket_AgenteAsignado FOREIGN KEY (AgenteAsignadoId) REFERENCES dbo.Usuario (Id),
    CONSTRAINT CK_Ticket_Asunto      CHECK (LEN(Asunto) >= 5),
    CONSTRAINT CK_Ticket_Descripcion CHECK (LEN(Descripcion) >= 10)
);
CREATE INDEX IX_Ticket_EstadoId         ON dbo.Ticket (EstadoId);
CREATE INDEX IX_Ticket_AgenteAsignadoId ON dbo.Ticket (AgenteAsignadoId);
CREATE INDEX IX_Ticket_ClienteId        ON dbo.Ticket (ClienteId);
CREATE INDEX IX_Ticket_PrioridadId      ON dbo.Ticket (PrioridadId);
CREATE INDEX IX_Ticket_CategoriaId      ON dbo.Ticket (CategoriaId);

CREATE TABLE dbo.HistorialTicket
(
    Id               INT IDENTITY(1,1) NOT NULL,
    TicketId         INT               NOT NULL,
    UsuarioId        INT               NOT NULL,
    EstadoAnteriorId INT               NULL,          -- NULL en el registro de creación
    EstadoNuevoId    INT               NOT NULL,
    FechaCambio      DATETIME2         NOT NULL CONSTRAINT DF_HistorialTicket_FechaCambio DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_HistorialTicket PRIMARY KEY (Id),
    CONSTRAINT FK_HistorialTicket_Ticket         FOREIGN KEY (TicketId)         REFERENCES dbo.Ticket (Id),
    CONSTRAINT FK_HistorialTicket_Usuario        FOREIGN KEY (UsuarioId)        REFERENCES dbo.Usuario (Id),
    CONSTRAINT FK_HistorialTicket_EstadoAnterior FOREIGN KEY (EstadoAnteriorId) REFERENCES dbo.EstadoTicket (Id),
    CONSTRAINT FK_HistorialTicket_EstadoNuevo    FOREIGN KEY (EstadoNuevoId)    REFERENCES dbo.EstadoTicket (Id)
);
CREATE INDEX IX_HistorialTicket_TicketId           ON dbo.HistorialTicket (TicketId);
CREATE INDEX IX_HistorialTicket_EstadoNuevo_Fecha  ON dbo.HistorialTicket (EstadoNuevoId, FechaCambio);

/* =====================================================================
   Comunicación (HU-09..11)
   ===================================================================== */

CREATE TABLE dbo.Comentario
(
    Id            INT IDENTITY(1,1) NOT NULL,
    TicketId      INT               NOT NULL,
    UsuarioId     INT               NOT NULL,
    Texto         NVARCHAR(4000)    NOT NULL,
    EsNotaInterna BIT               NOT NULL CONSTRAINT DF_Comentario_EsNotaInterna DEFAULT (0),
    FechaCreacion DATETIME2         NOT NULL CONSTRAINT DF_Comentario_FechaCreacion DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Comentario PRIMARY KEY (Id),
    CONSTRAINT FK_Comentario_Ticket  FOREIGN KEY (TicketId)  REFERENCES dbo.Ticket (Id),
    CONSTRAINT FK_Comentario_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario (Id),
    CONSTRAINT CK_Comentario_Texto CHECK (LEN(LTRIM(RTRIM(Texto))) > 0)
);
CREATE INDEX IX_Comentario_Ticket_Fecha ON dbo.Comentario (TicketId, FechaCreacion);

-- Un adjunto pertenece a un ticket (HU-04) o a un comentario (HU-11), nunca a ambos.
-- Guardar TicketId también en los adjuntos de comentario crearía la dependencia
-- transitiva ComentarioId → TicketId (violación de 3FN).
CREATE TABLE dbo.Adjunto
(
    Id                 INT IDENTITY(1,1) NOT NULL,
    TicketId           INT               NULL,
    ComentarioId       INT               NULL,
    UsuarioId          INT               NOT NULL,
    NombreOriginal     NVARCHAR(255)     NOT NULL,
    TipoContenido      NVARCHAR(100)     NOT NULL,
    TamanoBytes        BIGINT            NOT NULL,
    RutaAlmacenamiento NVARCHAR(500)     NOT NULL,
    FechaSubida        DATETIME2         NOT NULL CONSTRAINT DF_Adjunto_FechaSubida DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_Adjunto PRIMARY KEY (Id),
    CONSTRAINT FK_Adjunto_Ticket     FOREIGN KEY (TicketId)     REFERENCES dbo.Ticket (Id),
    CONSTRAINT FK_Adjunto_Comentario FOREIGN KEY (ComentarioId) REFERENCES dbo.Comentario (Id),
    CONSTRAINT FK_Adjunto_Usuario    FOREIGN KEY (UsuarioId)    REFERENCES dbo.Usuario (Id),
    CONSTRAINT CK_Adjunto_UnPadre CHECK (
        (TicketId IS NOT NULL AND ComentarioId IS NULL) OR
        (TicketId IS NULL AND ComentarioId IS NOT NULL)),
    CONSTRAINT CK_Adjunto_Tamano CHECK (TamanoBytes > 0 AND TamanoBytes <= 10485760)   -- 10 MB (HU-11)
);
CREATE INDEX IX_Adjunto_TicketId     ON dbo.Adjunto (TicketId)     WHERE TicketId IS NOT NULL;
CREATE INDEX IX_Adjunto_ComentarioId ON dbo.Adjunto (ComentarioId) WHERE ComentarioId IS NOT NULL;

-- Sin fila = el usuario no ha leído el comentario (HU-09 esc. 3).
CREATE TABLE dbo.LecturaComentario
(
    ComentarioId INT       NOT NULL,
    UsuarioId    INT       NOT NULL,
    FechaLectura DATETIME2 NOT NULL CONSTRAINT DF_LecturaComentario_FechaLectura DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT PK_LecturaComentario PRIMARY KEY (ComentarioId, UsuarioId),
    CONSTRAINT FK_LecturaComentario_Comentario FOREIGN KEY (ComentarioId) REFERENCES dbo.Comentario (Id),
    CONSTRAINT FK_LecturaComentario_Usuario    FOREIGN KEY (UsuarioId)    REFERENCES dbo.Usuario (Id)
);
GO
