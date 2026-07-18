/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7184
 */

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexumAltivon.API.API.Data.Migrations.Nexum
{
    /// <inheritdoc />
    public partial class HardenProcurementTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string defaultTenantId = "00000000-0000-0000-0000-000000000001";
            var tables = new[]
            {
                "compras_solicitacoes",
                "compras_cotacoes",
                "compras_pedidos",
                "compras_pedido_itens",
                "compras_entradas",
                "compras_entrada_itens",
                "estoque_movimentos"
            };

            foreach (var table in tables)
            {
                migrationBuilder.Sql(
                    $"""
                    ALTER TABLE `{table}`
                        ADD COLUMN IF NOT EXISTS `tenant_id` CHAR(36) NOT NULL DEFAULT '{defaultTenantId}',
                        ADD COLUMN IF NOT EXISTS `row_version` BLOB NULL,
                        ADD COLUMN IF NOT EXISTS `created_by_user_id` CHAR(36) NULL,
                        ADD COLUMN IF NOT EXISTS `updated_by_user_id` CHAR(36) NULL,
                        ADD COLUMN IF NOT EXISTS `is_deleted` TINYINT(1) NOT NULL DEFAULT 0,
                        ADD COLUMN IF NOT EXISTS `deleted_at` DATETIME NULL,
                        ADD COLUMN IF NOT EXISTS `updated_at` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP;

                    UPDATE `{table}`
                    SET `tenant_id` = '{defaultTenantId}'
                    WHERE `tenant_id` IS NULL OR `tenant_id` = '';

                    UPDATE `{table}`
                    SET `row_version` = UNHEX(REPLACE(UUID(), '-', ''))
                    WHERE `row_version` IS NULL OR OCTET_LENGTH(`row_version`) = 0;
                    """);

                var index = $"ix_{table}_tenant_deleted";
                migrationBuilder.Sql(
                    $"""
                    SET @procurement_index_sql = (
                        SELECT IF(
                            COUNT(*) = 0,
                            'CREATE INDEX `{index}` ON `{table}` (`tenant_id`, `is_deleted`)',
                            'SELECT 1')
                        FROM information_schema.statistics
                        WHERE table_schema = DATABASE()
                          AND table_name = '{table}'
                          AND index_name = '{index}'
                    );
                    PREPARE procurement_index_statement FROM @procurement_index_sql;
                    EXECUTE procurement_index_statement;
                    DEALLOCATE PREPARE procurement_index_statement;
                    """);
            }

            migrationBuilder.Sql(
                """
                SET @procurement_order_drop_sql = (
                    SELECT IF(
                        COUNT(*) = 0,
                        'SELECT 1',
                        'ALTER TABLE `compras_pedidos` DROP INDEX `ux_compras_pedidos_numero`')
                    FROM information_schema.statistics
                    WHERE table_schema = DATABASE()
                      AND table_name = 'compras_pedidos'
                      AND index_name = 'ux_compras_pedidos_numero'
                );
                PREPARE procurement_order_drop_statement FROM @procurement_order_drop_sql;
                EXECUTE procurement_order_drop_statement;
                DEALLOCATE PREPARE procurement_order_drop_statement;

                SET @procurement_order_add_sql = (
                    SELECT IF(
                        COUNT(*) = 0,
                        'CREATE UNIQUE INDEX `ux_compras_pedidos_tenant_numero` ON `compras_pedidos` (`tenant_id`, `numero`)',
                        'SELECT 1')
                    FROM information_schema.statistics
                    WHERE table_schema = DATABASE()
                      AND table_name = 'compras_pedidos'
                      AND index_name = 'ux_compras_pedidos_tenant_numero'
                );
                PREPARE procurement_order_add_statement FROM @procurement_order_add_sql;
                EXECUTE procurement_order_add_statement;
                DEALLOCATE PREPARE procurement_order_add_statement;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var tables = new[]
            {
                "compras_solicitacoes",
                "compras_cotacoes",
                "compras_pedidos",
                "compras_pedido_itens",
                "compras_entradas",
                "compras_entrada_itens",
                "estoque_movimentos"
            };

            foreach (var table in tables)
            {
                var index = $"ix_{table}_tenant_deleted";
                migrationBuilder.Sql(
                    $"""
                    SET @procurement_index_drop_sql = (
                        SELECT IF(
                            COUNT(*) = 0,
                            'SELECT 1',
                            'DROP INDEX `{index}` ON `{table}`')
                        FROM information_schema.statistics
                        WHERE table_schema = DATABASE()
                          AND table_name = '{table}'
                          AND index_name = '{index}'
                    );
                    PREPARE procurement_index_drop_statement FROM @procurement_index_drop_sql;
                    EXECUTE procurement_index_drop_statement;
                    DEALLOCATE PREPARE procurement_index_drop_statement;
                    """);
            }

            migrationBuilder.Sql(
                """
                SET @procurement_order_drop_tenant_sql = (
                    SELECT IF(
                        COUNT(*) = 0,
                        'SELECT 1',
                        'DROP INDEX `ux_compras_pedidos_tenant_numero` ON `compras_pedidos`')
                    FROM information_schema.statistics
                    WHERE table_schema = DATABASE()
                      AND table_name = 'compras_pedidos'
                      AND index_name = 'ux_compras_pedidos_tenant_numero'
                );
                PREPARE procurement_order_drop_tenant_statement FROM @procurement_order_drop_tenant_sql;
                EXECUTE procurement_order_drop_tenant_statement;
                DEALLOCATE PREPARE procurement_order_drop_tenant_statement;

                SET @procurement_order_restore_sql = (
                    SELECT IF(
                        COUNT(*) = 0,
                        'CREATE UNIQUE INDEX `ux_compras_pedidos_numero` ON `compras_pedidos` (`numero`)',
                        'SELECT 1')
                    FROM information_schema.statistics
                    WHERE table_schema = DATABASE()
                      AND table_name = 'compras_pedidos'
                      AND index_name = 'ux_compras_pedidos_numero'
                );
                PREPARE procurement_order_restore_statement FROM @procurement_order_restore_sql;
                EXECUTE procurement_order_restore_statement;
                DEALLOCATE PREPARE procurement_order_restore_statement;
                """);

        }
    }
}
