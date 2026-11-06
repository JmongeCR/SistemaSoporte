/* =====================================================================
   08_base_conocimiento.sql : Base de conocimiento (KB) + vinculación a tickets.
   ===================================================================== */

USE SoporteTecnico;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- ArticuloKb ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ArticuloKb')
BEGIN
    CREATE TABLE dbo.ArticuloKb (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Titulo          NVARCHAR(200)  NOT NULL,
        Contenido       NVARCHAR(MAX)  NOT NULL,
        Slug            NVARCHAR(220)  NOT NULL,
        CategoriaId     INT            NOT NULL REFERENCES dbo.Categoria(Id),
        CreadoPorId     INT            NOT NULL REFERENCES dbo.Usuario(Id),
        FechaCreacion   DATETIME2(0)   NOT NULL DEFAULT SYSUTCDATETIME(),
        FechaActualizacion DATETIME2(0) NULL,
        Publicado       BIT            NOT NULL DEFAULT 1,
        Vistas          INT            NOT NULL DEFAULT 0
    );
    CREATE UNIQUE INDEX UX_ArticuloKb_Slug ON dbo.ArticuloKb(Slug);
END
GO

/* ---------- TicketArticuloKb (vinculación) ---------- */
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TicketArticuloKb')
BEGIN
    CREATE TABLE dbo.TicketArticuloKb (
        TicketId        INT NOT NULL REFERENCES dbo.Ticket(Id)      ON DELETE CASCADE,
        ArticuloKbId    INT NOT NULL REFERENCES dbo.ArticuloKb(Id)  ON DELETE CASCADE,
        VinculadoPorId  INT NOT NULL REFERENCES dbo.Usuario(Id),
        FechaVinculo    DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        PRIMARY KEY (TicketId, ArticuloKbId)
    );
END
GO
