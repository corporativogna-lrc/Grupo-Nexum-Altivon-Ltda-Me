#
# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5
#

#!/usr/bin/env bash
set -Eeuo pipefail

usage() {
    echo "Uso: bash docker/scripts/restore-mysql.sh <arquivo_backup.sql.gz>"
}

BACKUP_FILE="${1:-}"
DB_NAME="${DB_NAME:-${MYSQL_DATABASE:-nexum_altivon}}"
DB_HOST="${DB_HOST:-${MYSQL_HOST:-192.168.1.72}}"
DB_PORT="${DB_PORT:-${MYSQL_PORT:-3309}}"
DB_USER="${DB_USER:-${MYSQL_USER:-root}}"
RESTORE_CONFIRM="${RESTORE_CONFIRM:-}"
RESTORE_DRY_RUN="${RESTORE_DRY_RUN:-false}"
DB_PASSWORD="${DB_PASS:-${MYSQL_PASSWORD:-${MYSQL_PWD:-}}}"

if [ -z "$BACKUP_FILE" ]; then
    usage >&2
    echo "Backups disponiveis em /backups/mysql:" >&2
    if ! ls -la /backups/mysql/*.sql.gz 2>/dev/null; then
        echo "Nenhum backup encontrado em /backups/mysql." >&2
    fi
    exit 2
fi

if [ ! -f "$BACKUP_FILE" ]; then
    echo "Arquivo de backup nao encontrado: $BACKUP_FILE" >&2
    exit 2
fi

command -v gzip >/dev/null 2>&1 || {
    echo "gzip nao encontrado no ambiente." >&2
    exit 2
}

gzip -t "$BACKUP_FILE"

if [ "$RESTORE_DRY_RUN" = "true" ]; then
    echo "Dry-run restore MySQL: arquivo=$BACKUP_FILE host=$DB_HOST port=$DB_PORT database=$DB_NAME user=$DB_USER"
    exit 0
fi

if [ -z "$DB_PASSWORD" ]; then
    echo "Defina DB_PASS, MYSQL_PASSWORD ou MYSQL_PWD antes do restore." >&2
    exit 2
fi

if [ "$RESTORE_CONFIRM" != "yes" ]; then
    if [ -t 0 ]; then
        echo "Restore sobrescrevera o banco $DB_NAME em $DB_HOST:$DB_PORT. Digite yes para confirmar:"
        read -r RESTORE_CONFIRM
    fi

    if [ "$RESTORE_CONFIRM" != "yes" ]; then
        echo "Restore cancelado. Defina RESTORE_CONFIRM=yes para execucao automatizada controlada."
        exit 1
    fi
fi

command -v mysql >/dev/null 2>&1 || {
    echo "mysql client nao encontrado no ambiente." >&2
    exit 2
}

export MYSQL_PWD="$DB_PASSWORD"
gzip -dc "$BACKUP_FILE" | mysql \
    --host="$DB_HOST" \
    --port="$DB_PORT" \
    --user="$DB_USER" \
    --protocol=TCP \
    "$DB_NAME"

echo "Restore concluido em $(date -u +%Y-%m-%dT%H:%M:%SZ)"
