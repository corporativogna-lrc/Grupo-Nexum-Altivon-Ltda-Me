#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

#!/usr/bin/env sh
set -eu

SCHEMA_PATH="/schemas/genesisgest-original-schema.sql"

if [ -z "${MYSQL_ROOT_PASSWORD:-}" ]; then
  echo "[init-genesisgest] MYSQL_ROOT_PASSWORD nao configurada."
  exit 1
fi

if [ ! -f "$SCHEMA_PATH" ]; then
  echo "[init-genesisgest] Schema GenesisGest.Net nao encontrado em $SCHEMA_PATH."
  exit 1
fi

mysql -uroot -p"$MYSQL_ROOT_PASSWORD" -e "CREATE DATABASE IF NOT EXISTS \`genesis_bd\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
mysql -uroot -p"$MYSQL_ROOT_PASSWORD" genesis_bd < "$SCHEMA_PATH"

echo "[init-genesisgest] Banco genesis_bd inicializado com schema GenesisGest.Net."
