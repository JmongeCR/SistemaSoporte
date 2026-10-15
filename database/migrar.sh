#!/usr/bin/env bash
# Aplica las migraciones NO destructivas (05 en adelante) sobre la base existente, sin borrar datos.
# Para una base nueva desde cero use inicializar.sh.
set -euo pipefail

CONTENEDOR="sc701-soporte-sqlserver"
cd "$(dirname "$0")/.."
set -a; source .env; set +a

for script in database/05_*.sql; do
  echo "→ $(basename "$script")"
  docker exec "$CONTENEDOR" /opt/mssql-tools18/bin/sqlcmd \
    -S localhost -U sa -P "$SQLSERVER_SA_PASSWORD" -C -b -i "/scripts/$(basename "$script")" > /dev/null
done
echo "Migraciones aplicadas."
