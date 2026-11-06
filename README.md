# Sistema de Tickets de Soporte Técnico

Sistema web para gestión de tickets de soporte (SC-701).  
**Stack:** .NET 8 · Razor Pages · Web API · SQL Server 2022 · Docker

---

## Requisitos

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

---

## Puesta en marcha

**1. Copiar variables de entorno**
```bash
cp .env.example .env
# Editar .env y cambiar SQLSERVER_SA_PASSWORD
```

**2. Levantar la base de datos**
```bash
docker compose up -d
./database/inicializar.sh
```
> ⚠️ `inicializar.sh` **elimina y recrea** todas las tablas. No lo ejecute sobre datos que quiera conservar.

**3. Configurar secretos de la API**
```bash
dotnet user-secrets set "Jwt:Clave" "$(openssl rand -base64 32)" --project src/Soporte.API
dotnet user-secrets set "ConnectionStrings:SoporteDb" "Server=localhost,14330;Database=SoporteTecnico;User Id=sa;Password=<contraseña>;TrustServerCertificate=True" --project src/Soporte.API
```

**4. Ejecutar**
```bash
dotnet run --project src/Soporte.API   # http://localhost:5049/swagger
dotnet run --project src/Soporte.Web   # http://localhost:5122
```

---

## Usuarios de prueba

| Correo | Contraseña | Rol |
|--------|-----------|-----|
| `admin@soporte.test` | `Admin_2025!` | Administrador |
| `agente1@soporte.test` | `Agente_2025!` | Agente |
| `cliente1@soporte.test` | `Cliente_2025!` | Cliente |

Los correos (recuperación de contraseña) se guardan en `src/Soporte.API/App_Data/correos/`.

---

## Pruebas

```bash
dotnet test
```
