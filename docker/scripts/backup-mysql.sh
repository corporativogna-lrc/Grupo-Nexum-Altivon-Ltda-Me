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
DATE_UTC="$(date -u +%Y%m%d_%H%M%S)"
DB_NAME="${DB_NAME:-${MYSQL_DATABASE:-nexum_altivon}}"
DB_HOST="${DB_HOST:-${MYSQL_HOST:-192.168.1.72}}"
DB_PORT="${DB_PORT:-${MYSQL_PORT:-3309}}"
DB_USER="${DB_USER:-${MYSQL_USER:-root}}"
RETENTION_DAYS="${RETENTION_DAYS:-30}"
BACKUP_DRY_RUN="${BACKUP_DRY_RUN:-false}"
DB_PASSWORD="${DB_PASS:-${MYSQL_PASSWORD:-${MYSQL_PWD:-}}}"

case "$RETENTION_DAYS" in
    ''|*[!0-9]*)
        echo "RETENTION_DAYS deve ser numerico." >&2
        exit 2
        ;;
esac

if [ -z "$DB_PASSWORD" ]; then
    echo "Defina DB_PASS, MYSQL_PASSWORD ou MYSQL_PWD antes do backup." >&2
    exit 2
fi

if [ "$BACKUP_DRY_RUN" = "true" ]; then
    echo "Dry-run backup MySQL: host=$DB_HOST port=$DB_PORT database=$DB_NAME user=$DB_USER dir=$BACKUP_DIR retention=$RETENTION_DAYS"
    exit 0
fi

command -v mysqldump >/dev/null 2>&1 || {
    echo "mysqldump nao encontrado no ambiente." >&2
    exit 2
}

command -v gzip >/dev/null 2>&1 || {
    echo "gzip nao encontrado no ambiente." >&2
    exit 2
}

mkdir -p "$BACKUP_DIR"

BACKUP_SQL="$BACKUP_DIR/${DB_NAME}_${DATE_UTC}.sql"
BACKUP_GZ="$BACKUP_SQL.gz"
LOG_FILE="$BACKUP_DIR/backup.log"

cleanup_uncompressed() {
    rm -f "$BACKUP_SQL"
}
trap cleanup_uncompressed EXIT

export MYSQL_PWD="$DB_PASSWORD"
mysqldump \
    --host="$DB_HOST" \
    --port="$DB_PORT" \
    --user="$DB_USER" \
    --protocol=TCP \
    --single-transaction \
    --routines \
    --triggers \
    --events \
    "$DB_NAME" > "$BACKUP_SQL"

gzip -f "$BACKUP_SQL"
find "$BACKUP_DIR" -type f -name "${DB_NAME}_*.sql.gz" -mtime +"$RETENTION_DAYS" -delete

echo "[$(date -u +%Y-%m-%dT%H:%M:%SZ)] Backup concluido: $BACKUP_GZ" >> "$LOG_FILE"
echo "$BACKUP_GZ"
