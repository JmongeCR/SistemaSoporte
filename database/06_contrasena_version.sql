/* =====================================================================
   Sistema de Tickets de Soporte Técnico — SC-701
   06_contrasena_version.sql : agrega ContrasenaVersion a dbo.Usuario

   Migración incremental: segura sobre una base existente (ALTER TABLE).
   Al terminar, todos los usuarios quedan en versión 0.
   ===================================================================== */

USE SoporteTecnico;
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID(N'dbo.Usuario') AND name = N'ContrasenaVersion'
)
BEGIN
    ALTER TABLE dbo.Usuario
        ADD ContrasenaVersion INT NOT NULL
            CONSTRAINT DF_Usuario_ContrasenaVersion DEFAULT (0);
END
GO
