/* =====================================================================
   09_plantillas_csat_votos.sql : Respuestas enlatadas, CSAT y votos KB.
   Idempotente: puede ejecutarse sobre una base existente.
   ===================================================================== */

USE SoporteTecnico;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- PlantillaRespuesta ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PlantillaRespuesta')
BEGIN
    CREATE TABLE dbo.PlantillaRespuesta (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Titulo          NVARCHAR(100)  NOT NULL,
        Texto           NVARCHAR(4000) NOT NULL,
        CreadoPorId     INT            NOT NULL REFERENCES dbo.Usuario(Id),
        FechaCreacion   DATETIME2(0)   NOT NULL DEFAULT SYSUTCDATETIME(),
        Activa          BIT            NOT NULL DEFAULT 1
    );
END
GO

/* ---------- CalificacionCsat ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'CalificacionCsat')
BEGIN
    CREATE TABLE dbo.CalificacionCsat (
        Id                  INT IDENTITY(1,1) PRIMARY KEY,
        TicketId            INT       NOT NULL REFERENCES dbo.Ticket(Id),
        ClienteId           INT       NOT NULL REFERENCES dbo.Usuario(Id),
        Calificacion        TINYINT   NOT NULL CHECK (Calificacion BETWEEN 1 AND 5),
        Comentario          NVARCHAR(500) NULL,
        FechaCalificacion   DATETIME2(0)  NOT NULL DEFAULT SYSUTCDATETIME(),
        UNIQUE (TicketId, ClienteId)
    );
END
GO

/* ---------- VotoKb ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'VotoKb')
BEGIN
    CREATE TABLE dbo.VotoKb (
        ArticuloKbId    INT  NOT NULL REFERENCES dbo.ArticuloKb(Id),
        UsuarioId       INT  NOT NULL REFERENCES dbo.Usuario(Id),
        EsUtil          BIT  NOT NULL,
        FechaVoto       DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        PRIMARY KEY (ArticuloKbId, UsuarioId)
    );
END
GO

/* ---------- Seed plantillas de ejemplo ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.PlantillaRespuesta)
BEGIN
    INSERT INTO dbo.PlantillaRespuesta (Titulo, Texto, CreadoPorId) VALUES
        (N'Cierre por falta de respuesta',
         N'Hola, cerramos este ticket por falta de respuesta. Si el problema persiste, no dudes en abrir uno nuevo.',
         1),
        (N'Solicitud de información adicional',
         N'Para continuar necesitamos información adicional. ¿Podrías indicarnos [detalle]? Quedamos atentos.',
         1),
        (N'Ticket escalado a nivel 2',
         N'Tu ticket ha sido escalado a nuestro equipo especializado. Te contactaremos en las próximas 24 horas hábiles.',
         1),
        (N'Resolución confirmada',
         N'El problema ha sido resuelto. Si experimentas algún inconveniente adicional, no dudes en contactarnos.',
         1);
END
GO
