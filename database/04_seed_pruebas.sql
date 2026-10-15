/* =====================================================================
   04_seed_pruebas.sql : DATOS DE PRUEBA para demos (no ejecutar en producción).

   Contraseña de todos los usuarios de prueba: Soporte123!
   (hash PBKDF2-SHA256 generado con Soporte.Reglas.Reglas.SeguridadContrasenas)

   Usuarios:
     1 Ana Rodríguez   admin1@soporte.test    Administrador
     2 Carlos Mora     admin2@soporte.test    Administrador  ← 2 admins para probar HU-15
     3 Luis Vargas     agente1@soporte.test   Agente
     4 María Jiménez   agente2@soporte.test   Agente
     5 Sofía Castro    cliente1@soporte.test  Cliente
     6 Diego Solís     cliente2@soporte.test  Cliente

   Las fechas son relativas al momento de ejecución, así que el dashboard
   siempre muestra datos. Resultado esperado de sp_ObtenerMetricasDashboard:
     General (NULL)   → Abiertos 2, EnProgreso 2, ResueltosHoy 1, SlaVencido 1
     Luis   (@3)      → Abiertos 2, EnProgreso 1, ResueltosHoy 1, SlaVencido 1
     María  (@4)      → Abiertos 2, EnProgreso 1, ResueltosHoy 0, SlaVencido 1
   ===================================================================== */

USE SoporteTecnico;
GO

-- Requeridos por los índices filtrados y guardados junto a cada procedimiento;
-- el sqlcmd clásico los trae apagados por defecto.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM dbo.Usuario) OR EXISTS (SELECT 1 FROM dbo.Categoria)
    THROW 50000, N'La base ya tiene datos. Ejecute 01_esquema.sql y 02_catalogos.sql antes de este script.', 1;

-- Todas las fechas se guardan en UTC. @InicioHoy = medianoche de hoy en Costa Rica, expresada en UTC.
DECLARE @Ahora     DATETIME2 = SYSUTCDATETIME();
DECLARE @InicioHoy DATETIME2 = CONVERT(DATETIME2,
    CAST(CAST(@Ahora AT TIME ZONE 'UTC' AT TIME ZONE 'Central America Standard Time' AS DATE) AS DATETIME2)
        AT TIME ZONE 'Central America Standard Time' AT TIME ZONE 'UTC');
-- Hora de resolución del ticket 5: hace 1 hora, sin cruzar la medianoche local (para que cuente como "hoy").
DECLARE @ResueltoEn DATETIME2 = CASE WHEN DATEADD(HOUR, -1, @Ahora) >= @InicioHoy
                                     THEN DATEADD(HOUR, -1, @Ahora) ELSE @Ahora END;

BEGIN TRANSACTION;

/* ---------- Usuarios ---------- */
SET IDENTITY_INSERT dbo.Usuario ON;
INSERT INTO dbo.Usuario (Id, Nombre, Correo, PasswordHash, RolId, FechaCreacion) VALUES
    (1, N'Ana Rodríguez', N'admin1@soporte.test',   N'PBKDF2-SHA256$100000$5S1d3A5Xpi92q+RX+R8tvg==$2vRkUAvnohmkrtBdF+ZqHqa0L1CtQtgrmHL7H+uuX8o=', 1, DATEADD(DAY, -30, @Ahora)),
    (2, N'Carlos Mora',   N'admin2@soporte.test',   N'PBKDF2-SHA256$100000$vMTJtu9CmU2da6KBnJEElA==$2HG9yemfeH2/t3DbkDDljvluWoSP/+MB+rYOwcnuN8U=', 1, DATEADD(DAY, -30, @Ahora)),
    (3, N'Luis Vargas',   N'agente1@soporte.test',  N'PBKDF2-SHA256$100000$KSIt5r5iq/He8UPPIjxjiQ==$DyuYfG7d4u55q+h+clowvvEUhkYaHi1dJrVeUgjsnLc=', 2, DATEADD(DAY, -20, @Ahora)),
    (4, N'María Jiménez', N'agente2@soporte.test',  N'PBKDF2-SHA256$100000$ALSuMoKMmr61psuTOx0GuA==$rSQe3wE2x5BLFScVZlql2oQaqqsdlXSxivYxdHFhXBw=', 2, DATEADD(DAY, -20, @Ahora)),
    (5, N'Sofía Castro',  N'cliente1@soporte.test', N'PBKDF2-SHA256$100000$ory2ES4Bhpj69yxDcajPag==$fhoqaOhMDB9x+BHY+SxG9MRuGK9nUTPKQOhIFjopNqE=', 3, DATEADD(DAY, -10, @Ahora)),
    (6, N'Diego Solís',   N'cliente2@soporte.test', N'PBKDF2-SHA256$100000$y5LowFZnNFTRfFdX1G8t+Q==$CdiKKHKMzv12aQ5Q2r8RN86eSg7f15H7hCaBaty404Y=', 3, DATEADD(DAY, -10, @Ahora));
SET IDENTITY_INSERT dbo.Usuario OFF;

/* ---------- Categorías ---------- */
SET IDENTITY_INSERT dbo.Categoria ON;
INSERT INTO dbo.Categoria (Id, Nombre) VALUES
    (1, N'Hardware'),
    (2, N'Software'),
    (3, N'Redes y conectividad'),
    (4, N'Cuentas y accesos');
SET IDENTITY_INSERT dbo.Categoria OFF;

/* ---------- Tickets ----------
   Prioridad: 1 Baja (72 h) · 2 Media (48 h) · 3 Alta (24 h) · 4 Urgente (8 h)
   Estado:    1 Abierto · 2 En Progreso · 3 Resuelto · 4 Cerrado            */
SET IDENTITY_INSERT dbo.Ticket ON;
INSERT INTO dbo.Ticket (Id, Asunto, Descripcion, CategoriaId, PrioridadId, EstadoId, ClienteId, AgenteAsignadoId, FechaCreacion, FechaActualizacion) VALUES
    -- 1: Abierto, sin asignar, Alta con 30 h → SLA vencido
    (1, N'Laptop no enciende después de actualización',
        N'Después de instalar las actualizaciones de anoche la laptop no pasa de la pantalla negra. Ya probé con otro cargador.',
        1, 3, 1, 5, NULL, DATEADD(HOUR, -30, @Ahora), NULL),
    -- 2: Abierto, sin asignar, reciente
    (2, N'No puedo conectarme a la VPN desde casa',
        N'El cliente de VPN muestra "tiempo de espera agotado" al conectar. Desde la oficina funciona sin problema.',
        3, 2, 1, 6, NULL, DATEADD(HOUR, -2, @Ahora), NULL),
    -- 3: En Progreso con Luis; tiene la conversación y la nota interna de ejemplo
    (3, N'Error al generar el reporte de ventas en el ERP',
        N'Al filtrar el reporte de ventas por el mes actual aparece "Error 500". Lo necesitamos para el cierre de hoy.',
        2, 4, 2, 5, 3, DATEADD(HOUR, -5, @Ahora), DATEADD(HOUR, -3, @Ahora)),
    -- 4: En Progreso con María
    (4, N'Restablecer acceso al correo corporativo',
        N'Mi cuenta de correo quedó bloqueada después de varios intentos fallidos. Necesito recuperar el acceso.',
        4, 1, 2, 6, 4, DATEADD(HOUR, -26, @Ahora), DATEADD(HOUR, -24, @Ahora)),
    -- 5: Resuelto hoy por Luis
    (5, N'Impresora del segundo piso imprime en blanco',
        N'La impresora del segundo piso saca las hojas en blanco aunque indica que tiene tóner.',
        1, 2, 3, 5, 3, DATEADD(HOUR, -20, @Ahora), @ResueltoEn);
SET IDENTITY_INSERT dbo.Ticket OFF;

/* ---------- Historial de estados (HU-08) ---------- */
INSERT INTO dbo.HistorialTicket (TicketId, UsuarioId, EstadoAnteriorId, EstadoNuevoId, FechaCambio) VALUES
    (1, 5, NULL, 1, DATEADD(HOUR, -30, @Ahora)),
    (2, 6, NULL, 1, DATEADD(HOUR,  -2, @Ahora)),
    (3, 5, NULL, 1, DATEADD(HOUR,  -5, @Ahora)),
    (3, 3, 1,    2, DATEADD(HOUR,  -4, @Ahora)),
    (4, 6, NULL, 1, DATEADD(HOUR, -26, @Ahora)),
    (4, 4, 1,    2, DATEADD(HOUR, -24, @Ahora)),
    (5, 5, NULL, 1, DATEADD(HOUR, -20, @Ahora)),
    (5, 3, 1,    2, DATEADD(HOUR, -18, @Ahora)),
    (5, 3, 2,    3, @ResueltoEn);

/* ---------- Comentarios (HU-09, HU-10) ---------- */
SET IDENTITY_INSERT dbo.Comentario ON;
INSERT INTO dbo.Comentario (Id, TicketId, UsuarioId, Texto, EsNotaInterna, FechaCreacion) VALUES
    (1, 3, 5, N'El error aparece solo con el filtro del mes actual; con meses anteriores el reporte sale bien.',
        0, DATEADD(MINUTE, -270, @Ahora)),
    (2, 3, 3, N'Gracias, Sofía. Ya reproduje el error y estoy revisando el servicio de reportes. Le aviso en cuanto tenga novedades.',
        0, DATEADD(MINUTE, -210, @Ahora)),
    -- Nota interna: el cliente no debe verla (HU-10 esc. 2)
    (3, 3, 3, N'El timeout viene de la consulta de ventas sin índice por fecha. Coordinar con el DBA antes de tocar producción.',
        1, DATEADD(MINUTE, -180, @Ahora)),
    (4, 5, 3, N'Se reemplazó el cartucho y se limpió el cabezal. Por favor confirme si ya imprime correctamente.',
        0, DATEADD(MINUTE, -5, @ResueltoEn));
SET IDENTITY_INSERT dbo.Comentario OFF;

/* ---------- Lecturas (HU-09 esc. 3) ----------
   El comentario 4 queda SIN leer por Sofía, para mostrar el indicador de "no leído". */
INSERT INTO dbo.LecturaComentario (ComentarioId, UsuarioId, FechaLectura) VALUES
    (1, 3, DATEADD(MINUTE, -240, @Ahora)),   -- Luis leyó el comentario de Sofía
    (2, 5, DATEADD(MINUTE, -200, @Ahora));   -- Sofía leyó la respuesta de Luis

COMMIT TRANSACTION;
GO
