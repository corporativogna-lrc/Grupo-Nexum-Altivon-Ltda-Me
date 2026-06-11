#!/bin/bash
# ============================================
# Script de Restore — MySQL Nexum Altivon
# Grupo Nexum Altivon ME | www.nexumaltivon.com
# ============================================

if [ -z "$1" ]; then
    echo "Uso: ./restore-mysql.sh <arquivo_backup.sql.gz>"
    echo "Backups disponíveis:"
    ls -la /backups/mysql/*.sql.gz 2>/dev/null || echo "Nenhum backup encontrado"
    exit 1
fi

BACKUP_FILE=$1
DB_NAME="nexum_altivon"
DB_HOST="192.168.1.72"
DB_PORT="3309"
DB_USER="root"

echo "Restaurando backup: $BACKUP_FILE"
echo "AVISO: Isso irá sobrescrever o banco $DB_NAME. Confirme? (yes/no)"
read CONFIRM

if [ "$CONFIRM" != "yes" ]; then
    echo "Restore cancelado."
    exit 0
fi

gunzip -c $BACKUP_FILE | mysql -h $DB_HOST -P $DB_PORT -u $DB_USER -p$DB_PASS $DB_NAME

echo "✅ Restore concluído em $(date)"
