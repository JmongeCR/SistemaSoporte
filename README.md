# Sistema de Tickets de Soporte Técnico

Proyecto universitario — curso **SC-701 Programación Avanzada Web**.

Aplicación web para gestión de tickets de soporte con roles Administrador, Agente y Cliente.

**Stack:** ASP.NET Core 8 · Razor Pages · Web API · Entity Framework Core · SQL Server 2022 (Docker)

---

## Requisitos previos

| Herramienta | Versión mínima |
|---|---|
| [.NET SDK](https://dotnet.microsoft.com/download) | 8.0 |
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) | 4.x |
| Git | cualquiera |

---

## Instalación paso a paso

### 1. Clonar el repositorio

```bash
git clone https://github.com/JmongeCR/SistemaSoporte.git
cd SistemaSoporte
```

### 2. Crear el archivo `.env`

```bash
cp .env.example .env
```

El `.env` define la contraseña del SQL Server. Puedes dejarla como está para desarrollo local o cambiarla (mínimo 8 caracteres con mayúscula, minúscula, número y símbolo).

### 3. Levantar SQL Server con Docker

```bash
docker compose up -d
```

Espera unos 20–30 segundos a que el contenedor esté `healthy`:

```bash
docker ps   # columna STATUS debe mostrar (healthy)
```

### 4. Inicializar la base de datos

```bash
bash database/inicializar.sh
```

Este script crea el esquema completo y carga los datos de prueba (usuarios, categorías, etc.).

### 5. Configurar los secretos de la API

Los valores sensibles (cadena de conexión y clave JWT) se guardan con **User Secrets** de .NET y nunca se suben al repositorio.

```bash
cd src/Soporte.API

dotnet user-secrets set "ConnectionStrings:SoporteDb" \
  "Server=localhost,14330;Database=SoporteTecnico;User Id=sa;Password=<TU_PASSWORD_DEL_.env>;TrustServerCertificate=True"

dotnet user-secrets set "Jwt:Clave" \
  "7e216db2a02e89415c1b85928f7a8f65ee533d045a20a9208a227cf149b8b568"

cd ../..
```

> Reemplaza `<TU_PASSWORD_DEL_.env>` por el valor de `SQLSERVER_SA_PASSWORD` que pusiste en el paso 2.

### 6. Correr la aplicación

Abre **dos terminales** desde la raíz del proyecto:

**Terminal 1 — API (puerto 5029)**
```bash
cd src/Soporte.API
dotnet run
```

**Terminal 2 — Web (puerto 5122)**
```bash
cd src/Soporte.Web
dotnet run
```

Luego abre el navegador en: **http://localhost:5122**

---

## Usuarios de prueba

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | admin1@soporte.test | Soporte123! |
| Administrador | admin2@soporte.test | Soporte123! |
| Agente | agente1@soporte.test | Soporte123! |
| Agente | agente2@soporte.test | Soporte123! |
| Cliente | cliente1@soporte.test | Soporte123! |
| Cliente | cliente2@soporte.test | Soporte123! |

---

## Actualizar la base de datos (pull con cambios de esquema)

Si ya tenías la BD corriendo y alguien del equipo agregó un script nuevo (`05` en adelante), ejecuta:

```bash
bash database/migrar.sh
```

Este script aplica solo los scripts no destructivos (`05_*.sql` al `09_*.sql`) sin borrar datos.

> Si los cambios incluyen el `01_esquema.sql` (rediseño completo), usa `inicializar.sh` en su lugar — **borra y recrea todo**.

---

## Estructura del proyecto

```
SistemaSoporte/
├── database/               Scripts SQL ordenados (01→09) + scripts de shell
├── src/
│   ├── Soporte.Abstracciones/   Modelos, DTOs, interfaces
│   ├── Soporte.DA/              Repositorios y EF Core
│   ├── Soporte.Flujo/           Lógica de negocio
│   ├── Soporte.API/             Web API (puerto 5029)
│   └── Soporte.Web/             Razor Pages (puerto 5122)
└── tests/
    └── Soporte.Pruebas/         Pruebas unitarias
```
