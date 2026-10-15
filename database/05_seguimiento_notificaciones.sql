/* =====================================================================
   05_seguimiento_notificaciones.sql : migración NO destructiva (idempotente).
   Se puede ejecutar sobre una base con datos: solo agrega columnas, tablas,
   índices y actualiza un procedimiento. Nunca borra información.

     1. Comentario.SolicitaInformacion  → el equipo pide información adicional al cliente
     2. Tabla Notificacion              → notificaciones dentro de la plataforma
     3. sp_TomarTicket                  → devuelve el Id del historial generado
   ===================================================================== */

USE SoporteTecnico;
GO

SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------- 1. Solicitud de información en comentarios ---------- */
IF COL_LENGTH(N'dbo.Comentario', N'SolicitaInformacion') IS NULL
BEGIN
    ALTER TABLE dbo.Comentario
        ADD SolicitaInformacion BIT NOT NULL
            CONSTRAINT DF_Comentario_SolicitaInformacion DEFAULT (0);
END
GO

-- Una solicitud de información siempre es pública (el cliente tiene que verla).
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints WHERE name = N'CK_Comentario_SolicitudPublica')
    ALTER TABLE dbo.Comentario
        ADD CONSTRAINT CK_Comentario_SolicitudPublica CHECK (NOT (SolicitaInformacion = 1 AND EsNotaInterna = 1));
GO

/* ---------- 2. Notificaciones ---------- */
IF OBJECT_ID(N'dbo.Notificacion', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Notificacion
    (
        Id            INT IDENTITY(1,1) NOT NULL,
        UsuarioId     INT               NOT NULL,   -- destinatario
        TicketId      INT               NOT NULL,
        Tipo          TINYINT           NOT NULL,   -- ver enum TipoNotificacion
        Mensaje       NVARCHAR(300)     NOT NULL,   -- texto tal como se generó en el momento del evento
        ClaveEvento   NVARCHAR(100)     NOT NULL,   -- evento de origen, ej. "historial:15"; evita duplicados
        FechaCreacion DATETIME2         NOT NULL CONSTRAINT DF_Notificacion_FechaCreacion DEFAULT (SYSUTCDATETIME()),
        FechaLectura  DATETIME2         NULL,       -- NULL = no leída
        CONSTRAINT PK_Notificacion PRIMARY KEY (Id),
        CONSTRAINT FK_Notificacion_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario (Id),
        CONSTRAINT FK_Notificacion_Ticket  FOREIGN KEY (TicketId)  REFERENCES dbo.Ticket (Id),
        CONSTRAINT CK_Notificacion_Tipo CHECK (Tipo BETWEEN 1 AND 7),
        -- Un mismo evento notifica una sola vez a cada destinatario.
        CONSTRAINT UQ_Notificacion_Usuario_Evento UNIQUE (UsuarioId, ClaveEvento)
    );

    CREATE INDEX IX_Notificacion_Usuario_Fecha ON dbo.Notificacion (UsuarioId, FechaCreacion DESC);
    CREATE INDEX IX_Notificacion_NoLeidas ON dbo.Notificacion (UsuarioId) WHERE FechaLectura IS NULL;
END
GO

/* ---------- 3. sp_TomarTicket con @HistorialId ---------- */
CREATE OR ALTER PROCEDURE dbo.sp_TomarTicket
    @TicketId    INT,
    @AgenteId    INT,
    @Asignado    BIT OUTPUT,
    @HistorialId INT = NULL OUTPUT   -- Id del cambio de estado registrado (NULL si el estado no cambió)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Asignado = 0;
    SET @HistorialId = NULL;

    IF NOT EXISTS (SELECT 1 FROM dbo.Usuario WHERE Id = @AgenteId AND RolId = 2 AND Activo = 1)
        THROW 50001, N'Solo un agente activo puede tomar tickets.', 1;

    DECLARE @Cambio TABLE (EstadoAnteriorId INT, EstadoNuevoId INT);

    BEGIN TRANSACTION;

    UPDATE dbo.Ticket
       SET AgenteAsignadoId   = @AgenteId,
           EstadoId           = CASE WHEN EstadoId = 1 THEN 2 ELSE EstadoId END,
           FechaActualizacion = SYSUTCDATETIME()
    OUTPUT deleted.EstadoId, inserted.EstadoId INTO @Cambio (EstadoAnteriorId, EstadoNuevoId)
     WHERE Id = @TicketId
       AND AgenteAsignadoId IS NULL
       AND EstadoId IN (1, 2);

    IF @@ROWCOUNT = 1
    BEGIN
        SET @Asignado = 1;

        INSERT INTO dbo.HistorialTicket (TicketId, UsuarioId, EstadoAnteriorId, EstadoNuevoId)
        SELECT @TicketId, @AgenteId, c.EstadoAnteriorId, c.EstadoNuevoId
        FROM @Cambio c
        WHERE c.EstadoAnteriorId <> c.EstadoNuevoId;

        IF @@ROWCOUNT = 1
            SET @HistorialId = CAST(SCOPE_IDENTITY() AS INT);
    END

    COMMIT TRANSACTION;
END
GO

/* ---------- 4. Permisos nuevos en la matriz (HU-16) ---------- */
SET NOCOUNT ON;

MERGE dbo.Permiso AS destino
USING (VALUES
    (N'VER_NOTIFICACIONES',    N'Recibir y consultar notificaciones de sus tickets', N'Visibilidad'),
    (N'SOLICITAR_INFORMACION', N'Pedir información adicional al cliente',          N'Comunicación interna')
) AS origen (Codigo, Descripcion, Grupo)
ON destino.Codigo = origen.Codigo
WHEN NOT MATCHED THEN
    INSERT (Codigo, Descripcion, Grupo) VALUES (origen.Codigo, origen.Descripcion, origen.Grupo);

INSERT INTO dbo.RolPermiso (RolId, PermisoId)
SELECT a.RolId, p.Id
FROM (VALUES
    (1, N'VER_NOTIFICACIONES'), (2, N'VER_NOTIFICACIONES'), (3, N'VER_NOTIFICACIONES'),
    (1, N'SOLICITAR_INFORMACION'), (2, N'SOLICITAR_INFORMACION')
) AS a (RolId, Codigo)
JOIN dbo.Permiso p ON p.Codigo = a.Codigo
WHERE NOT EXISTS (SELECT 1 FROM dbo.RolPermiso rp WHERE rp.RolId = a.RolId AND rp.PermisoId = p.Id);
GO
