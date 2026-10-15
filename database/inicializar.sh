#!/usr/bin/env bash
# Crea (o recrea desde cero) la base SoporteTecnico dentro del contenedor de docker-compose.
# ATENCIÓN: 01_esquema.sql borra todas las tablas y sus datos.
set -euo pipefail

CONTENEDOR="sc701-soporte-sqlserver"
cd "$(dirname "$0")/.."

if [[ ! -f .env ]]; then
  echo "Falta el archivo .env (copie .env.example como .env)." >&2
  exit 1
fi
set -a; source .env; set +a

echo "Esperando a que SQL Server esté listo..."
for _ in $(seq 1 40); do
  estado=$(docker inspect -f '{{.State.Health.Status}}' "$CONTENEDOR" 2>/dev/null || echo "sin-contenedor")
  [[ "$estado" == "healthy" ]] && break
  if [[ "$estado" == "sin-contenedor" ]]; then
    echo "El contenedor $CONTENEDOR no existe. Ejecute primero: docker compose up -d" >&2
    exit 1
  fi
  sleep 3
done
[[ "$estado" == "healthy" ]] || { echo "SQL Server no respondió a tiempo." >&2; exit 1; }

for script in 01_esquema.sql 02_catalogos.sql 03_procedimientos.sql 04_seed_pruebas.sql 05_seguimiento_notificaciones.sql; do
  echo "→ $script"
  docker exec "$CONTENEDOR" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SQLSERVER_SA_PASSWORD" -C -b -i "/scripts/$script" > /dev/null
done

echo "Base SoporteTecnico lista (usuarios de prueba en database/04_seed_pruebas.sql)."
