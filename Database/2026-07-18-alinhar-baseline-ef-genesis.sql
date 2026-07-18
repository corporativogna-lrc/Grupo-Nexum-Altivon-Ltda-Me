/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7182
 */

SET NAMES utf8mb4;

CREATE TABLE IF NOT EXISTS erp_boletos (
    id INT NOT NULL AUTO_INCREMENT,
    conta_receber_id INT NOT NULL,
    nosso_numero VARCHAR(100) NULL,
    linha_digitavel VARCHAR(255) NULL,
    codigo_barras VARCHAR(255) NULL,
    banco VARCHAR(100) NULL,
    vencimento DATETIME(6) NOT NULL,
    valor DECIMAL(65,30) NOT NULL,
    status VARCHAR(30) NULL,
    url_boleto VARCHAR(500) NULL,
    pdf_url VARCHAR(500) NULL,
    criado_em DATETIME(6) NOT NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS erp_financeiro_referencias (
    id INT NOT NULL AUTO_INCREMENT,
    tipo VARCHAR(40) NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    descricao VARCHAR(150) NOT NULL,
    ordem INT NOT NULL,
    ativo TINYINT(1) NOT NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS erp_rh_colaboradores (
    id INT NOT NULL AUTO_INCREMENT,
    nome VARCHAR(150) NOT NULL,
    cargo VARCHAR(120) NULL,
    departamento VARCHAR(120) NULL,
    status VARCHAR(50) NULL,
    data_admissao DATETIME(6) NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb4;

CREATE TABLE IF NOT EXISTS erp_rh_referencias (
    id INT NOT NULL AUTO_INCREMENT,
    tipo VARCHAR(40) NOT NULL,
    codigo VARCHAR(50) NOT NULL,
    descricao VARCHAR(120) NOT NULL,
    ordem INT NOT NULL,
    ativo TINYINT(1) NOT NULL,
    PRIMARY KEY (id)
) ENGINE=InnoDB DEFAULT CHARACTER SET=utf8mb4;

ALTER TABLE erp_contas_pagar
    MODIFY numero_documento VARCHAR(40) NOT NULL,
    MODIFY descricao VARCHAR(200) NOT NULL,
    MODIFY tenant_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
    MODIFY row_version BINARY(16) NOT NULL,
    MODIFY created_at DATETIME(6) NOT NULL,
    MODIFY created_by_user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    MODIFY updated_at DATETIME(6) NULL,
    MODIFY updated_by_user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    MODIFY is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    MODIFY deleted_at DATETIME(6) NULL;

ALTER TABLE erp_contas_receber
    MODIFY numero_documento VARCHAR(40) NOT NULL,
    MODIFY descricao VARCHAR(200) NOT NULL,
    MODIFY tenant_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
    MODIFY row_version BINARY(16) NOT NULL,
    MODIFY created_at DATETIME(6) NOT NULL,
    MODIFY created_by_user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    MODIFY updated_at DATETIME(6) NULL,
    MODIFY updated_by_user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    MODIFY is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    MODIFY deleted_at DATETIME(6) NULL;

ALTER TABLE erp_fluxo_caixa
    MODIFY tenant_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
    MODIFY row_version BINARY(16) NOT NULL,
    MODIFY created_at DATETIME(6) NOT NULL,
    MODIFY created_by_user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    MODIFY updated_at DATETIME(6) NULL,
    MODIFY updated_by_user_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NULL,
    MODIFY is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    MODIFY deleted_at DATETIME(6) NULL;

CREATE TABLE IF NOT EXISTS `__EFMigrationsHistory` (
    `MigrationId` VARCHAR(150) CHARACTER SET utf8mb4 NOT NULL,
    `ProductVersion` VARCHAR(32) CHARACTER SET utf8mb4 NOT NULL,
    CONSTRAINT `PK___EFMigrationsHistory` PRIMARY KEY (`MigrationId`)
) CHARACTER SET=utf8mb4;

SELECT COUNT(*) INTO @genesis_table_count
FROM information_schema.TABLES
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN (
      'erp_boletos',
      'erp_contas_pagar',
      'erp_contas_receber',
      'erp_financeiro_referencias',
      'erp_fluxo_caixa',
      'erp_rh_colaboradores',
      'erp_rh_referencias'
  );
SET @genesis_assert_sql = IF(
    @genesis_table_count = 7,
    'DO 0',
    'SELECT * FROM baseline_genesis_recusado_tabelas_ef_incompletas'
);
PREPARE genesis_assert FROM @genesis_assert_sql;
EXECUTE genesis_assert;
DEALLOCATE PREPARE genesis_assert;

SELECT COUNT(*) INTO @genesis_audit_column_count
FROM information_schema.COLUMNS
WHERE TABLE_SCHEMA = DATABASE()
  AND TABLE_NAME IN ('erp_contas_pagar', 'erp_contas_receber', 'erp_fluxo_caixa')
  AND COLUMN_NAME IN (
      'tenant_id', 'row_version', 'created_at', 'created_by_user_id',
      'updated_at', 'updated_by_user_id', 'is_deleted', 'deleted_at'
  );
SET @genesis_assert_sql = IF(
    @genesis_audit_column_count = 24,
    'DO 0',
    'SELECT * FROM baseline_genesis_recusado_colunas_auditoria_incompletas'
);
PREPARE genesis_assert FROM @genesis_assert_sql;
EXECUTE genesis_assert;
DEALLOCATE PREPARE genesis_assert;

SELECT COUNT(*) INTO @genesis_tenant_index_count
FROM (
    SELECT TABLE_NAME
    FROM information_schema.STATISTICS
    WHERE TABLE_SCHEMA = DATABASE()
      AND TABLE_NAME IN ('erp_contas_pagar', 'erp_contas_receber', 'erp_fluxo_caixa')
    GROUP BY TABLE_NAME, INDEX_NAME
    HAVING GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX) = 'tenant_id,is_deleted'
) AS tenant_indexes;
SET @genesis_assert_sql = IF(
    @genesis_tenant_index_count = 3,
    'DO 0',
    'SELECT * FROM baseline_genesis_recusado_indices_tenant_incompletos'
);
PREPARE genesis_assert FROM @genesis_assert_sql;
EXECUTE genesis_assert;
DEALLOCATE PREPARE genesis_assert;

SELECT
    (SELECT COUNT(*) FROM erp_contas_pagar
     WHERE tenant_id IS NULL OR tenant_id = '' OR row_version IS NULL
        OR OCTET_LENGTH(row_version) <> 16 OR created_at IS NULL
        OR numero_documento IS NULL OR TRIM(numero_documento) = '')
  + (SELECT COUNT(*) FROM erp_contas_receber
     WHERE tenant_id IS NULL OR tenant_id = '' OR row_version IS NULL
        OR OCTET_LENGTH(row_version) <> 16 OR created_at IS NULL
        OR numero_documento IS NULL OR TRIM(numero_documento) = '')
  + (SELECT COUNT(*) FROM erp_fluxo_caixa
     WHERE tenant_id IS NULL OR tenant_id = '' OR row_version IS NULL
        OR OCTET_LENGTH(row_version) <> 16 OR created_at IS NULL)
INTO @genesis_invalid_row_count;
SET @genesis_assert_sql = IF(
    @genesis_invalid_row_count = 0,
    'DO 0',
    'SELECT * FROM baseline_genesis_recusado_registros_sem_tenant_ou_auditoria'
);
PREPARE genesis_assert FROM @genesis_assert_sql;
EXECUTE genesis_assert;
DEALLOCATE PREPARE genesis_assert;

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260705114438_InitialGenesis', '8.0.5')
ON DUPLICATE KEY UPDATE `ProductVersion` = VALUES(`ProductVersion`);

INSERT INTO `__EFMigrationsHistory` (`MigrationId`, `ProductVersion`)
VALUES ('20260718072948_HardenGenesisFinanceTenancy', '8.0.29')
ON DUPLICATE KEY UPDATE `ProductVersion` = VALUES(`ProductVersion`);

SELECT `MigrationId`, `ProductVersion`
FROM `__EFMigrationsHistory`
ORDER BY `MigrationId`;
