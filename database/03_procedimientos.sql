/* =====================================================================
   03_procedimientos.sql : procedimientos almacenados
     1. sp_ObtenerMetricasDashboard  (HU-12, HU-13)
     2. sp_TicketsPorAgente          (HU-12, carga de trabajo del equipo)
     3. sp_TomarTicket               (HU-06, asignación atómica)
   ===================================================================== */

USE SoporteTecnico;
GO

-- Requeridos por los índices filtrados y guardados junto a cada procedimiento;
-- el sqlcmd clásico los trae apagados por defecto.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

/* ---------------------------------------------------------------------
   sp_ObtenerMetricasDashboard
   @AgenteId NULL      → métricas generales (Administrador, HU-12)
   @AgenteId con valor → tickets del agente + sin asignar (HU-13)
   Siempre devuelve exactamente una fila, en cero si no hay tickets (HU-12 esc. 2).
   SLA vencido = ticket Abierto o En Progreso cuya antigüedad supera Prioridad.HorasSla.
   Las fechas se guardan en UTC; "hoy" se calcula según @ZonaHoraria (Costa Rica = UTC-6).
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_ObtenerMetricasDashboard
    @AgenteId    INT          = NULL,
    @ZonaHoraria NVARCHAR(64) = N'Central America Standard Time'
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Ahora DATETIME2 = SYSUTCDATETIME();
    -- Medianoche local de hoy, expresada en UTC.
    DECLARE @InicioHoy DATETIME2 = CONVERT(DATETIME2,
        CAST(CAST(@Ahora AT TIME ZONE 'UTC' AT TIME ZONE @ZonaHoraria AS DATE) AS DATETIME2)
            AT TIME ZONE @ZonaHoraria AT TIME ZONE 'UTC');

    ;WITH TicketsAlcance AS
    (
        SELECT t.Id, t.EstadoId, t.FechaCreacion, p.HorasSla,
               -- Resuelto hoy: el ticket sigue Resuelto y la transición a Resuelto ocurrió hoy.
               ResueltoHoy = CASE WHEN t.EstadoId = 3 AND EXISTS (
                                      SELECT 1 FROM dbo.HistorialTicket h
                                      WHERE h.TicketId = t.Id
                                        AND h.EstadoNuevoId = 3
                                        AND h.FechaCambio >= @InicioHoy)
                                  THEN 1 ELSE 0 END
        FROM dbo.Ticket t
        JOIN dbo.Prioridad p ON p.Id = t.PrioridadId
        WHERE @AgenteId IS NULL
           OR t.AgenteAsignadoId = @AgenteId
           OR t.AgenteAsignadoId IS NULL
    )
    SELECT
        Abiertos     = COUNT(CASE WHEN ta.EstadoId = 1 THEN 1 END),
        EnProgreso   = COUNT(CASE WHEN ta.EstadoId = 2 THEN 1 END),
        ResueltosHoy = ISNULL(SUM(ta.ResueltoHoy), 0),
        SlaVencido   = COUNT(CASE WHEN ta.EstadoId IN (1, 2)
                                   AND DATEADD(HOUR, ta.HorasSla, ta.FechaCreacion) < @Ahora
                                  THEN 1 END)
    FROM TicketsAlcance ta;
END
GO

/* ---------------------------------------------------------------------
   sp_TicketsPorAgente
   Carga de trabajo por agente activo, incluidos los agentes sin tickets.
   Parámetros opcionales para acotar por fecha de creación del ticket.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_TicketsPorAgente
    @FechaDesde DATETIME2 = NULL,
    @FechaHasta DATETIME2 = NULL
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        AgenteId   = u.Id,
        Agente     = u.Nombre,
        Abiertos   = COUNT(CASE WHEN t.EstadoId = 1 THEN 1 END),
        EnProgreso = COUNT(CASE WHEN t.EstadoId = 2 THEN 1 END),
        Resueltos  = COUNT(CASE WHEN t.EstadoId = 3 THEN 1 END),
        Cerrados   = COUNT(CASE WHEN t.EstadoId = 4 THEN 1 END)
    FROM dbo.Usuario u
    LEFT JOIN dbo.Ticket t
           ON t.AgenteAsignadoId = u.Id
          AND (@FechaDesde IS NULL OR t.FechaCreacion >= @FechaDesde)
          AND (@FechaHasta IS NULL OR t.FechaCreacion <  @FechaHasta)
    WHERE u.RolId = 2          -- Agente
      AND u.Activo = 1
    GROUP BY u.Id, u.Nombre
    ORDER BY (COUNT(CASE WHEN t.EstadoId IN (1, 2) THEN 1 END)) DESC, u.Nombre;
END
GO

/* ---------------------------------------------------------------------
   sp_TomarTicket (HU-06)
   Asigna el ticket al agente solo si sigue sin asignar. El UPDATE con la
   condición "AgenteAsignadoId IS NULL" es atómico: si dos agentes lo toman
   a la vez, solo uno obtiene @Asignado = 1 (esc. 2).
   Si el ticket estaba Abierto pasa a En Progreso y se registra en el historial.
   --------------------------------------------------------------------- */
CREATE OR ALTER PROCEDURE dbo.sp_TomarTicket
    @TicketId INT,
    @AgenteId INT,
    @Asignado BIT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SET @Asignado = 0;

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
       AND EstadoId IN (1, 2);          -- no se toman tickets Resueltos ni Cerrados

    IF @@ROWCOUNT = 1
    BEGIN
        SET @Asignado = 1;

        INSERT INTO dbo.HistorialTicket (TicketId, UsuarioId, EstadoAnteriorId, EstadoNuevoId)
        SELECT @TicketId, @AgenteId, c.EstadoAnteriorId, c.EstadoNuevoId
        FROM @Cambio c
        WHERE c.EstadoAnteriorId <> c.EstadoNuevoId;
    END

    COMMIT TRANSACTION;
END
GO
