/* =====================================================================
   02_catalogos.sql : datos de referencia OBLIGATORIOS para que el sistema funcione.
   Los Id de Rol, EstadoTicket y Prioridad deben coincidir con los enums de
   Soporte.Abstracciones.Enumeraciones.
   ===================================================================== */

USE SoporteTecnico;
GO

-- Requeridos por los índices filtrados y guardados junto a cada procedimiento;
-- el sqlcmd clásico los trae apagados por defecto.
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

SET NOCOUNT ON;

INSERT INTO dbo.Rol (Id, Nombre) VALUES
    (1, N'Administrador'),
    (2, N'Agente'),
    (3, N'Cliente');

INSERT INTO dbo.EstadoTicket (Id, Nombre) VALUES
    (1, N'Abierto'),
    (2, N'En Progreso'),
    (3, N'Resuelto'),
    (4, N'Cerrado');

INSERT INTO dbo.Prioridad (Id, Nombre, HorasSla) VALUES
    (1, N'Baja',    72),
    (2, N'Media',   48),
    (3, N'Alta',    24),
    (4, N'Urgente',  8);

/* ---------- Matriz de permisos de solo lectura (HU-16) ---------- */
INSERT INTO dbo.Permiso (Codigo, Descripcion, Grupo) VALUES
    (N'VER_TODOS_TICKETS',      N'Ver todos los tickets del sistema',                 N'Visibilidad'),
    (N'VER_TICKETS_ASIGNADOS',  N'Ver tickets asignados a sí mismo y sin asignar',    N'Visibilidad'),
    (N'VER_TICKETS_PROPIOS',    N'Ver únicamente los tickets que creó',               N'Visibilidad'),
    (N'VER_DASHBOARD_GENERAL',  N'Ver métricas generales de todo el equipo',          N'Visibilidad'),
    (N'VER_DASHBOARD_PROPIO',   N'Ver métricas de su propio ámbito',                  N'Visibilidad'),
    (N'CREAR_TICKET',           N'Crear tickets de soporte',                          N'Gestión de tickets'),
    (N'REGISTRAR_PARA_CLIENTE', N'Registrar tickets a nombre de un cliente',          N'Gestión de tickets'),
    (N'TOMAR_TICKET',           N'Asignarse un ticket sin asignar',                   N'Gestión de tickets'),
    (N'CAMBIAR_ESTADO',         N'Cambiar estado, prioridad y categoría de un ticket', N'Gestión de tickets'),
    (N'REASIGNAR_TICKET',       N'Reasignar un ticket a otro agente',                 N'Gestión de tickets'),
    (N'REABRIR_TICKET_CERRADO', N'Reabrir un ticket en estado Cerrado',               N'Gestión de tickets'),
    (N'RESPONDER_TICKET',       N'Agregar respuestas públicas en un ticket',          N'Comunicación interna'),
    (N'ADJUNTAR_ARCHIVOS',      N'Adjuntar archivos a tickets y comentarios',         N'Comunicación interna'),
    (N'NOTA_INTERNA',           N'Agregar notas internas visibles solo al equipo',    N'Comunicación interna'),
    (N'VER_NOTAS_INTERNAS',     N'Ver las notas internas de los tickets',             N'Comunicación interna'),
    (N'VER_USUARIOS',           N'Ver la lista de usuarios y sus roles',              N'Administración'),
    (N'CAMBIAR_ROL',            N'Cambiar el rol de un usuario',                      N'Administración'),
    (N'VER_MATRIZ_PERMISOS',    N'Consultar la matriz de permisos por rol',           N'Administración');

INSERT INTO dbo.RolPermiso (RolId, PermisoId)
SELECT a.RolId, p.Id
FROM (VALUES
    -- Administrador
    (1, N'VER_TODOS_TICKETS'), (1, N'VER_DASHBOARD_GENERAL'), (1, N'CREAR_TICKET'),
    (1, N'REGISTRAR_PARA_CLIENTE'), (1, N'CAMBIAR_ESTADO'),
    (1, N'REASIGNAR_TICKET'), (1, N'REABRIR_TICKET_CERRADO'), (1, N'RESPONDER_TICKET'),
    (1, N'ADJUNTAR_ARCHIVOS'), (1, N'NOTA_INTERNA'), (1, N'VER_NOTAS_INTERNAS'),
    (1, N'VER_USUARIOS'), (1, N'CAMBIAR_ROL'), (1, N'VER_MATRIZ_PERMISOS'),
    -- Agente
    (2, N'VER_TICKETS_ASIGNADOS'), (2, N'VER_DASHBOARD_PROPIO'), (2, N'CREAR_TICKET'),
    (2, N'REGISTRAR_PARA_CLIENTE'), (2, N'TOMAR_TICKET'),
    (2, N'CAMBIAR_ESTADO'), (2, N'RESPONDER_TICKET'), (2, N'ADJUNTAR_ARCHIVOS'),
    (2, N'NOTA_INTERNA'), (2, N'VER_NOTAS_INTERNAS'),
    -- Cliente
    (3, N'VER_TICKETS_PROPIOS'), (3, N'CREAR_TICKET'), (3, N'RESPONDER_TICKET'),
    (3, N'ADJUNTAR_ARCHIVOS')
) AS a (RolId, Codigo)
JOIN dbo.Permiso p ON p.Codigo = a.Codigo;
GO
