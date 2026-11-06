-- HU-16 ext: permisos explícitos por usuario (overrides sobre los defaults del rol).
-- Solo se almacenan las diferencias respecto al rol; si no hay fila, el permiso se hereda del rol.
USE SoporteTecnico;
GO

IF OBJECT_ID(N'dbo.UsuarioPermiso', N'U') IS NULL
CREATE TABLE dbo.UsuarioPermiso (
    UsuarioId  INT NOT NULL,
    PermisoId  INT NOT NULL,
    Concedido  BIT NOT NULL DEFAULT 1,
    CONSTRAINT PK_UsuarioPermiso PRIMARY KEY (UsuarioId, PermisoId),
    CONSTRAINT FK_UsuarioPermiso_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario(Id),
    CONSTRAINT FK_UsuarioPermiso_Permiso FOREIGN KEY (PermisoId) REFERENCES dbo.Permiso(Id)
);
