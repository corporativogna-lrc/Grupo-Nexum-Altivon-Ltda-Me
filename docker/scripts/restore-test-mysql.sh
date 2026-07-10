#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

#!/usr/bin/env bash
set -Eeuo pipefail

BACKUP_DIR="${BACKUP_DIR:-/backups/mysql}"
BACKUP_FILE="${1:-}"
DB_HOST="${DB_HOST:-${MYSQL_HOST:-192.168.1.72}}"
DB_PORT="${DB_PORT:-${MYSQL_PORT:-3309}}"
DB_USER="${DB_USER:-${MYSQL_USER:-root}}"
DB_PASSWORD="${DB_PASS:-${MYSQL_PASSWORD:-${MYSQL_PWD:-}}}"
RESTORE_TEST_DRY_RUN="${RESTORE_TEST_DRY_RUN:-false}"
RESTORE_TEST_DATABASE_PREFIX="${RESTORE_TEST_DATABASE_PREFIX:-genesisgest_restore_test}"
RESTORE_TEST_DATABASE="${RESTORE_TEST_DATABASE:-${RESTORE_TEST_DATABASE_PREFIX}_$(date -u +%Y%m%d_%H%M%S)}"

if [[ ! "$RESTORE_TEST_DATABASE" =~ ^[A-Za-z0-9_]+$ ]]; then
    echo "RESTORE_TEST_DATABASE deve conter apenas letras, numeros e underscore." >&2
    exit 2
fi

if [ -z "$BACKUP_FILE" ]; then
    BACKUP_FILE="$(find "$BACKUP_DIR" -type f -name '*.sql.gz' -print 2>/dev/null | sort | tail -n 1)"
fi

if [ -z "$BACKUP_FILE" ] || [ ! -f "$BACKUP_FILE" ]; then
    echo "Nenhum backup .sql.gz encontrado para restore-test em $BACKUP_DIR." >&2
    exit 2
fi

command -v gzip >/dev/null 2>&1 || {
    echo "gzip nao encontrado no ambiente." >&2
    exit 2
}

gzip -t "$BACKUP_FILE"

if [ "$RESTORE_TEST_DRY_RUN" = "true" ]; then
    echo "Dry-run restore-test MySQL: arquivo=$BACKUP_FILE host=$DB_HOST port=$DB_PORT database=$RESTORE_TEST_DATABASE user=$DB_USER"
    exit 0
fi

if [ -z "$DB_PASSWORD" ]; then
    echo "Defina DB_PASS, MYSQL_PASSWORD ou MYSQL_PWD antes do restore-test." >&2
    exit 2
fi

command -v mysql >/dev/null 2>&1 || {
    echo "mysql client nao encontrado no ambiente." >&2
    exit 2
}

export MYSQL_PWD="$DB_PASSWORD"

cleanup_database() {
    if ! mysql \
        --host="$DB_HOST" \
        --port="$DB_PORT" \
        --user="$DB_USER" \
        --protocol=TCP \
        --execute="DROP DATABASE IF EXISTS \`$RESTORE_TEST_DATABASE\`;" >/dev/null 2>&1; then
        echo "Falha ao remover schema temporario $RESTORE_TEST_DATABASE do restore-test." >&2
        return 1
    fi
}
trap cleanup_database EXIT

mysql \
    --host="$DB_HOST" \
    --port="$DB_PORT" \
    --user="$DB_USER" \
    --protocol=TCP \
    --execute="CREATE DATABASE IF NOT EXISTS \`$RESTORE_TEST_DATABASE\` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"

gzip -dc "$BACKUP_FILE" | mysql \
    --host="$DB_HOST" \
    --port="$DB_PORT" \
    --user="$DB_USER" \
    --protocol=TCP \
    "$RESTORE_TEST_DATABASE"

TABLE_COUNT="$(mysql \
    --host="$DB_HOST" \
    --port="$DB_PORT" \
    --user="$DB_USER" \
    --protocol=TCP \
    --batch \
    --skip-column-names \
    --execute="SELECT COUNT(*) FROM information_schema.tables WHERE table_schema = '$RESTORE_TEST_DATABASE';")"

if [ "${TABLE_COUNT:-0}" -le 0 ]; then
    echo "Restore-test falhou: nenhum objeto restaurado em $RESTORE_TEST_DATABASE." >&2
    exit 1
fi

echo "Restore-test concluido em $(date -u +%Y-%m-%dT%H:%M:%SZ): $TABLE_COUNT tabelas verificadas em schema temporario."
