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
        1, 2, 3, 5, 3, DATEADD(HOUR, -20, @Ahora), @ResueltoEn),
    -- 6: Abierto sin asignar, Baja, pantalla
    (6, N'Monitor de recepción parpadea intermitentemente',
        N'El monitor del puesto de recepción lleva dos días parpadeando. No ocurre con otros equipos en la misma regleta.',
        1, 1, 1, 6, NULL, DATEADD(HOUR, -48, @Ahora), NULL),
    -- 7: En Progreso con María, correo duplicado
    (7, N'El correo aparece duplicado en Outlook',
        N'Desde ayer cada mensaje nuevo aparece dos veces en la bandeja de entrada. Ya intenté compactar los archivos PST.',
        2, 2, 2, 5, 4, DATEADD(HOUR, -10, @Ahora), DATEADD(HOUR, -8, @Ahora)),
    -- 8: Resuelto por Luis, WiFi
    (8, N'WiFi corporativo se desconecta cada 30 minutos',
        N'En el área de contabilidad la señal WiFi cae cada media hora aproximadamente. El cable de red funciona bien.',
        3, 3, 3, 6, 3, DATEADD(DAY, -3, @Ahora), DATEADD(DAY, -2, @Ahora)),
    -- 9: Cerrado por María, software de diseño
    (9, N'Solicitud de instalación de Adobe Illustrator',
        N'Necesito Adobe Illustrator para preparar materiales de marketing. Adjunto la aprobación del jefe de área.',
        2, 1, 4, 5, 4, DATEADD(DAY, -7, @Ahora), DATEADD(DAY, -5, @Ahora)),
    -- 10: En Progreso con Luis, error en aplicativo contable
    (10, N'No se puede exportar a PDF desde el sistema contable',
        N'Al pulsar "Exportar PDF" en el módulo de balances el programa muestra un error y se cierra. Ocurre con todos los usuarios.',
        2, 3, 2, 6, 3, DATEADD(HOUR, -15, @Ahora), DATEADD(HOUR, -12, @Ahora)),
    -- 11: Abierto urgente sin asignar, servidor
    (11, N'Servidor de archivos inaccesible desde todas las estaciones',
        N'Nadie en la empresa puede acceder al servidor de archivos compartidos. El área de producción está detenida.',
        3, 4, 1, 6, NULL, DATEADD(HOUR, -1, @Ahora), NULL),
    -- 12: Resuelto por María, contraseña
    (12, N'Contraseña de acceso al ERP expiró y no permite cambiarla',
        N'El sistema indica que la contraseña venció hace 90 días, pero al intentar cambiarla dice "Política no cumplida" sin más detalles.',
        4, 2, 3, 6, 4, DATEADD(DAY, -5, @Ahora), DATEADD(DAY, -4, @Ahora)),
    -- 13: Cerrado por Luis, teclado
    (13, N'Teclado inalámbrico no es detectado al encender el equipo',
        N'Cada vez que reinicio el equipo el teclado inalámbrico tarda varios minutos en conectarse o no conecta nunca.',
        1, 1, 4, 5, 3, DATEADD(DAY, -10, @Ahora), DATEADD(DAY, -8, @Ahora)),
    -- 14: Abierto sin asignar, intranet
    (14, N'La intranet no carga en el navegador corporativo',
        N'Al ingresar la URL de la intranet aparece "ERR_CONNECTION_REFUSED". Funciona desde el celular con WiFi de la empresa.',
        3, 3, 1, 6, NULL, DATEADD(HOUR, -6, @Ahora), NULL),
    -- 15: En Progreso con María, portal de RRHH
    (15, N'Error 403 al ingresar al portal de Recursos Humanos',
        N'Al intentar acceder al portal de RRHH aparece "Acceso denegado 403". Otros compañeros del mismo departamento sí pueden ingresar.',
        4, 4, 2, 5, 4, DATEADD(HOUR, -3, @Ahora), DATEADD(HOUR, -2, @Ahora));
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
    (5, 3, 2,    3, @ResueltoEn),
    -- Ticket 6: solo apertura
    (6, 6, NULL, 1, DATEADD(HOUR, -48, @Ahora)),
    -- Ticket 7: apertura y asignación a María
    (7, 5, NULL, 1, DATEADD(HOUR, -10, @Ahora)),
    (7, 2, 1,    2, DATEADD(HOUR, -8, @Ahora)),
    -- Ticket 8: apertura, en progreso y resolución
    (8, 6, NULL, 1, DATEADD(DAY, -3, @Ahora)),
    (8, 3, 1,    2, DATEADD(DAY, -3, DATEADD(HOUR, 2, @Ahora))),
    (8, 3, 2,    3, DATEADD(DAY, -2, @Ahora)),
    -- Ticket 9: apertura, en progreso, resuelto y cerrado
    (9, 5, NULL, 1, DATEADD(DAY, -7, @Ahora)),
    (9, 4, 1,    2, DATEADD(DAY, -6, @Ahora)),
    (9, 4, 2,    3, DATEADD(DAY, -6, DATEADD(HOUR, 4, @Ahora))),
    (9, 2, 3,    4, DATEADD(DAY, -5, @Ahora)),
    -- Ticket 10: apertura y en progreso con Luis
    (10, 6, NULL, 1, DATEADD(HOUR, -15, @Ahora)),
    (10, 3, 1,   2, DATEADD(HOUR, -12, @Ahora)),
    -- Ticket 11: solo apertura urgente
    (11, 6, NULL, 1, DATEADD(HOUR, -1, @Ahora)),
    -- Ticket 12: apertura, en progreso, resuelto
    (12, 6, NULL, 1, DATEADD(DAY, -5, @Ahora)),
    (12, 4, 1,   2, DATEADD(DAY, -5, DATEADD(HOUR, 3, @Ahora))),
    (12, 4, 2,   3, DATEADD(DAY, -4, @Ahora)),
    -- Ticket 13: apertura, en progreso, resuelto, cerrado
    (13, 5, NULL, 1, DATEADD(DAY, -10, @Ahora)),
    (13, 3, 1,   2, DATEADD(DAY, -9, @Ahora)),
    (13, 3, 2,   3, DATEADD(DAY, -9, DATEADD(HOUR, 5, @Ahora))),
    (13, 1, 3,   4, DATEADD(DAY, -8, @Ahora)),
    -- Ticket 14: solo apertura
    (14, 6, NULL, 1, DATEADD(HOUR, -6, @Ahora)),
    -- Ticket 15: apertura y en progreso con María
    (15, 5, NULL, 1, DATEADD(HOUR, -3, @Ahora)),
    (15, 4, 1,   2, DATEADD(HOUR, -2, @Ahora));

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
        0, DATEADD(MINUTE, -5, @ResueltoEn)),
    -- Ticket 7: María en progreso
    (5, 7, 4, N'Revisé la cuenta y encontré que hay una regla de bandeja de entrada duplicando mensajes. La voy a corregir ahora.',
        0, DATEADD(HOUR, -7, @Ahora)),
    (6, 7, 5, N'Perfecto, gracias. ¿Cuándo quedará listo?', 0, DATEADD(HOUR, -6, @Ahora)),
    -- Ticket 8: Luis resuelve WiFi
    (7, 8, 3, N'Identifiqué un canal WiFi saturado en el área. Cambié el canal del access point y actualicé el firmware.',
        0, DATEADD(DAY, -2, DATEADD(HOUR, 1, @Ahora))),
    (8, 8, 6, N'Lleva más de una hora sin desconectarse. ¡Gracias!', 0, DATEADD(DAY, -2, DATEADD(HOUR, 3, @Ahora))),
    -- Ticket 10: Luis investigando
    (9, 10, 3, N'El error está relacionado con el driver de PDF de la versión instalada. Estoy descargando el parche del proveedor.',
        0, DATEADD(HOUR, -11, @Ahora)),
    -- Nota interna ticket 10
    (10, 10, 3, N'El proveedor tiene un parche liberado el mes pasado; no se aplicó en la última actualización del sistema. Coordinar con IT para el despliegue.',
        1, DATEADD(HOUR, -10, @Ahora)),
    -- Ticket 11: urgente, anotación inicial
    (11, 11, 6, N'Estamos completamente parados. Producción, contabilidad y ventas no pueden trabajar. Es crítico.',
        0, DATEADD(MINUTE, -50, @Ahora)),
    -- Ticket 12: María resuelve contraseña
    (12, 12, 4, N'El problema era que la contraseña nueva no cumplía la política de longitud. Restablecí el acceso y le envié la nueva contraseña por mensaje seguro.',
        0, DATEADD(DAY, -4, DATEADD(HOUR, 1, @Ahora))),
    (13, 12, 6, N'Ya pude ingresar. Muchas gracias.', 0, DATEADD(DAY, -4, DATEADD(HOUR, 2, @Ahora))),
    -- Ticket 15: María investigando acceso RRHH
    (14, 15, 4, N'Revisé los permisos del Active Directory. Parece que tu usuario fue removido del grupo de RRHH por error en una migración reciente.',
        0, DATEADD(HOUR, -1, @Ahora));
SET IDENTITY_INSERT dbo.Comentario OFF;

/* ---------- Lecturas (HU-09 esc. 3) ----------
   El comentario 4 queda SIN leer por Sofía, para mostrar el indicador de "no leído". */
INSERT INTO dbo.LecturaComentario (ComentarioId, UsuarioId, FechaLectura) VALUES
    (1, 3, DATEADD(MINUTE, -240, @Ahora)),   -- Luis leyó el comentario de Sofía
    (2, 5, DATEADD(MINUTE, -200, @Ahora)),   -- Sofía leyó la respuesta de Luis
    (5, 5, DATEADD(HOUR, -6, @Ahora)),       -- Sofía leyó respuesta de María (ticket 7)
    (7, 6, DATEADD(DAY, -2, DATEADD(HOUR, 4, @Ahora))), -- Diego leyó resolución de Luis (ticket 8)
    (9, 6, DATEADD(HOUR, -10, @Ahora)),      -- Diego leyó actualización de Luis (ticket 10)
    (12, 6, DATEADD(DAY, -4, DATEADD(HOUR, 3, @Ahora))); -- Diego leyó resolución de María (ticket 12)

COMMIT TRANSACTION;
GO
