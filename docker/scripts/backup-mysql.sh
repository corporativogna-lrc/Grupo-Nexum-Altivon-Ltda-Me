#!/bin/bash
# ============================================
# Script de Backup Automático — MySQL Nexum Altivon
# Grupo Nexum Altivon ME | www.nexumaltivon.com
# Fase 6 — Operações e Manutenção
# ============================================

BACKUP_DIR="/backups/mysql"
DATE=$(date +%Y%m%d_%H%M%S)
DB_NAME="nexum_altivon"
DB_HOST="192.168.1.72"
DB_PORT="3309"
DB_USER="root"
RETENTION_DAYS=30

mkdir -p $BACKUP_DIR

mysqldump -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS \
    --single-transaction \
    --routines \
    --triggers \
    --events \
    $DB_NAME > "$BACKUP_DIR/${DB_NAME}_${DATE}.sql"

gzip "$BACKUP_DIR/${DB_NAME}_${DATE}.sql"

find $BACKUP_DIR -name "${DB_NAME}_*.sql.gz" -mtime +$RETENTION_DAYS -delete

echo "[$(date)] Backup concluído: ${DB_NAME}_${DATE}.sql.gz" >> $BACKUP_DIR/backup.log
