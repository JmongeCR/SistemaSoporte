/* =====================================================================
   08_base_conocimiento.sql : Base de conocimiento (KB) + vinculación a tickets.
   Idempotente: puede ejecutarse sobre una base existente.
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
        TicketId        INT NOT NULL REFERENCES dbo.Ticket(Id),
        ArticuloKbId    INT NOT NULL REFERENCES dbo.ArticuloKb(Id),
        VinculadoPorId  INT NOT NULL REFERENCES dbo.Usuario(Id),
        FechaVinculo    DATETIME2(0) NOT NULL DEFAULT SYSUTCDATETIME(),
        PRIMARY KEY (TicketId, ArticuloKbId)
    );
END
GO

/* ---------- Seed de artículos KB (solo si la tabla está vacía) ---------- */
IF NOT EXISTS (SELECT 1 FROM dbo.ArticuloKb)
BEGIN
    SET IDENTITY_INSERT dbo.ArticuloKb ON;
    INSERT INTO dbo.ArticuloKb (Id, Titulo, Contenido, Slug, CategoriaId, CreadoPorId, FechaCreacion, Publicado) VALUES
        (1, N'¿Qué hacer si la laptop no enciende?',
            N'## Pasos para diagnosticar una laptop que no enciende

1. Verifique que el cargador esté bien conectado y el LED del cargador encienda.
2. Retire la batería (si es removible), espere 30 segundos y vuelva a colocarla.
3. Mantenga presionado el botón de encendido por 15 segundos (descarga del capacitor).
4. Intente encender sin batería conectando solo el cargador.
5. Si tiene pantalla negra pero el equipo sí enciende (luces o ventilador), conecte un monitor externo.

**Si ningún paso funciona**, escale el ticket a soporte de hardware nivel 2.',
            N'laptop-no-enciende', 1, 1, DATEADD(DAY, -25, SYSUTCDATETIME()), 1),

        (2, N'Configurar cliente VPN en Windows 10/11',
            N'## Configuración del cliente VPN corporativo

### Requisitos previos
- Credenciales corporativas activas
- Certificado digital instalado (solicitarlo a soporte)

### Pasos
1. Descargue el cliente VPN desde la intranet: `http://intranet/vpn-client`
2. Ejecute el instalador como administrador
3. Ingrese el servidor VPN: `vpn.empresa.local`
4. Autentíquese con su usuario y contraseña de red

### Problemas comunes
- **"Tiempo de espera agotado"**: Verifique que el puerto 443 no esté bloqueado por el firewall local.
- **"Certificado no válido"**: Reinstale el certificado corporativo raíz.',
            N'configurar-cliente-vpn', 3, 1, DATEADD(DAY, -20, SYSUTCDATETIME()), 1),

        (3, N'Cómo solicitar el restablecimiento de contraseña',
            N'## Procedimiento de restablecimiento de contraseña

### Autoservicio (recomendado)
1. Vaya a `https://soporte.empresa.local/cuenta/restablecer`
2. Ingrese su correo corporativo
3. Revise su bandeja de entrada (también spam)
4. Siga el enlace recibido y establezca una contraseña nueva

### Política de contraseñas
- Mínimo 8 caracteres
- Al menos una mayúscula, una minúscula y un número
- No puede repetir las últimas 5 contraseñas

### Si no recibe el correo
Abra un ticket en la categoría **Cuentas y accesos** con su nombre completo y número de empleado.',
            N'restablecer-contrasena', 4, 2, DATEADD(DAY, -18, SYSUTCDATETIME()), 1),

        (4, N'Solución a problemas de conectividad WiFi',
            N'## Diagnóstico de problemas WiFi

### Verificaciones iniciales
1. Confirme que el WiFi del equipo está activado
2. Olvide la red corporativa y vuelva a conectarse
3. Reinicie el adaptador de red desde el Administrador de dispositivos

### Problemas de canal saturado
Si varias personas del mismo área reportan cortes simultáneos puede ser saturación del access point. Reporte el ticket con:
- Hora exacta del corte
- Área o piso donde ocurre
- Número de personas afectadas

### Cambio de DNS temporal
Si el WiFi conecta pero no hay acceso a internet, cambie el DNS a 8.8.8.8 y 8.8.4.4 como prueba.',
            N'problemas-wifi', 3, 1, DATEADD(DAY, -15, SYSUTCDATETIME()), 1),

        (5, N'Protocolo de escalamiento de tickets urgentes',
            N'## Cuándo y cómo escalar un ticket

### Criterios para escalamiento inmediato
- Más de 10 usuarios afectados simultáneamente
- Sistemas críticos de producción, contabilidad o ventas fuera de servicio
- Pérdida de datos o riesgo de pérdida inminente

### Pasos para escalar
1. Cambie la prioridad del ticket a **Urgente**
2. Asigne al agente senior disponible
3. Notifique al administrador por canal de mensajería inmediata
4. Deje nota interna con evaluación de impacto

### SLA urgente: 8 horas
Los tickets urgentes deben tener primera respuesta en menos de 1 hora y resolución dentro de 8 horas hábiles.',
            N'escalamiento-tickets-urgentes', 2, 2, DATEADD(DAY, -12, SYSUTCDATETIME()), 1),

        (6, N'Instalación de software corporativo aprobado',
            N'## Proceso de solicitud e instalación de software

### Software pre-aprobado (instalación inmediata)
- Suite de ofimática (ya instalada)
- Antivirus corporativo
- Cliente VPN
- Adobe Reader

### Software que requiere aprobación
Para cualquier otro software abra un ticket de categoría **Software** con:
1. Nombre exacto y versión del software
2. Justificación de negocio
3. Aprobación firmada del jefe inmediato (adjuntar imagen o PDF)

### Tiempos estimados
- Aprobación del área de TI: 1-2 días hábiles
- Instalación una vez aprobado: mismo día si hay licencia disponible',
            N'instalacion-software-corporativo', 2, 1, DATEADD(DAY, -10, SYSUTCDATETIME()), 1),

        (7, N'Acceso al portal de Recursos Humanos',
            N'## Problemas de acceso al portal de RRHH

### Error 403 - Acceso denegado
Este error indica que su usuario no tiene los permisos necesarios. Causas frecuentes:
- Cambio de departamento reciente sin actualización de grupos en Active Directory
- Cuenta nueva que no ha sido agregada al grupo correspondiente
- Expiración de acceso temporal

### Solución
Abra un ticket en categoría **Cuentas y accesos** indicando:
1. Su nombre completo y número de empleado
2. Departamento actual
3. Nombre de su jefe inmediato

El equipo de soporte coordinará con RRHH para restablecer el acceso.',
            N'acceso-portal-rrhh', 4, 2, DATEADD(DAY, -8, SYSUTCDATETIME()), 1);
    SET IDENTITY_INSERT dbo.ArticuloKb OFF;

    INSERT INTO dbo.TicketArticuloKb (TicketId, ArticuloKbId, VinculadoPorId, FechaVinculo) VALUES
        (1,  1, 3, DATEADD(HOUR, -28, SYSUTCDATETIME())),
        (2,  2, 1, DATEADD(HOUR,  -1, SYSUTCDATETIME())),
        (4,  3, 4, DATEADD(HOUR, -24, SYSUTCDATETIME())),
        (8,  4, 3, DATEADD(DAY,  -2,  SYSUTCDATETIME())),
        (9,  6, 4, DATEADD(DAY,  -6,  SYSUTCDATETIME())),
        (11, 5, 1, DATEADD(MINUTE, -45, SYSUTCDATETIME())),
        (12, 3, 4, DATEADD(DAY,  -5,  SYSUTCDATETIME())),
        (14, 4, 1, DATEADD(HOUR,  -5,  SYSUTCDATETIME())),
        (15, 7, 4, DATEADD(HOUR,  -2,  SYSUTCDATETIME()));
END
GO
