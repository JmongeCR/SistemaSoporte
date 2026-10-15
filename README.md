# Sistema de Tickets de Soporte Técnico

Sistema web de gestión de tickets de soporte (SC-701 Programación Avanzada Web).  
Arquitectura: ASP.NET Core 8 · Razor Pages (frontend) · Web API (backend) · SQL Server 2022.

---

## Requisitos

| Herramienta | Versión mínima |
|-------------|---------------|
| .NET SDK    | 8.0           |
| Docker Desktop | 4.x (para SQL Server) |
| `sqlcmd` (dentro del contenedor) | incluido en la imagen de SQL Server |

---

## Configuración inicial

### 1. Variables de entorno

```bash
cp .env.example .env
```

Edite `.env` y defina una contraseña segura:

```
SQLSERVER_SA_PASSWORD=CambieEsta_Contrasena1!
SQLSERVER_PUERTO=14330
```

> `.env` está en `.gitignore` y nunca debe subirse al repositorio.

### 2. Secretos de usuario (Jwt y cadena de conexión)

La API no almacena claves en `appsettings.json`. Use `dotnet user-secrets`:

```bash
dotnet user-secrets set "Jwt:Clave" "$(openssl rand -base64 32)" \
  --project src/Soporte.API

dotnet user-secrets set "ConnectionStrings:SoporteDb" \
  "Server=localhost,14330;Database=SoporteTecnico;User Id=sa;Password=<su_contraseña>;TrustServerCertificate=True" \
  --project src/Soporte.API
```

---

## Base de datos

### Levantar SQL Server

```bash
docker compose up -d
```

### Inicializar esquema y datos de prueba

> ⚠️ **ADVERTENCIA**: `database/inicializar.sh` ejecuta `01_esquema.sql`, que **elimina todas las tablas y sus datos** antes de recrearlas. No lo ejecute sobre una base con datos que quiera conservar.

```bash
chmod +x database/inicializar.sh
./database/inicializar.sh
```

El script ejecuta, en orden:

| Archivo | Propósito |
|---------|-----------|
| `01_esquema.sql` | Crea tablas, índices y restricciones (borra y recrea todo) |
| `02_catalogos.sql` | Inserta datos de catálogo (roles, estados, prioridades) |
| `03_procedimientos.sql` | Stored procedures |
| `04_seed_pruebas.sql` | Usuarios y tickets de prueba |
| `05_seguimiento_notificaciones.sql` | Tabla de notificaciones (migración) |

### Migración sobre base existente

Si ya tiene la base de datos y quiere agregar columnas nuevas sin recrear todo:

```bash
docker exec sc701-soporte-sqlserver /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "$SQLSERVER_SA_PASSWORD" -C -b \
  -i "/scripts/06_contrasena_version.sql"
```

---

## Ejecución en desarrollo

Abra dos terminales:

**Terminal 1 — API:**
```bash
dotnet run --project src/Soporte.API
```
Swagger disponible en `http://localhost:5049/swagger`

**Terminal 2 — Web:**
```bash
dotnet run --project src/Soporte.Web
```
Aplicación disponible en `http://localhost:5122`

### Usuarios de prueba

| Correo | Contraseña | Rol |
|--------|-----------|-----|
| `admin@soporte.test` | `Admin_2025!` | Administrador |
| `agente1@soporte.test` | `Agente_2025!` | Agente |
| `cliente1@soporte.test` | `Cliente_2025!` | Cliente |

Los correos de recuperación de contraseña se guardan como archivos HTML en `src/Soporte.API/App_Data/correos/`.

---

## Pruebas

```bash
dotnet test
```

---

## Estructura del proyecto

```
src/
  Soporte.API/           API REST (autenticación, tickets, etc.)
  Soporte.Web/           Interfaz Razor Pages
  Soporte.Flujo/         Lógica de negocio (casos de uso)
  Soporte.Reglas/        Reglas de dominio
  Soporte.DA/            Acceso a datos (EF Core + SQL Server)
  Soporte.Abstracciones/ Interfaces, DTOs y modelos compartidos
tests/
  Soporte.Pruebas/       Pruebas unitarias (xUnit)
database/
  01_esquema.sql         DDL completo
  06_contrasena_version.sql  Migración incremental
```
