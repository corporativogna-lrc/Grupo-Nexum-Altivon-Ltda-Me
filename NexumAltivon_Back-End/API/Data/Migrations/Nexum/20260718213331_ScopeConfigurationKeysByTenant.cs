/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7183
 */

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexumAltivon.API.API.Data.Migrations.Nexum
{
    /// <inheritdoc />
    public partial class ScopeConfigurationKeysByTenant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                SET @config_key_indexes = (
                    SELECT GROUP_CONCAT(CONCAT('DROP INDEX `', indexes_by_name.INDEX_NAME, '`') SEPARATOR ', ')
                    FROM (
                        SELECT INDEX_NAME
                        FROM information_schema.statistics
                        WHERE TABLE_SCHEMA = DATABASE()
                          AND TABLE_NAME = 'configuracoes_sistema'
                          AND INDEX_NAME <> 'PRIMARY'
                        GROUP BY INDEX_NAME
                        HAVING GROUP_CONCAT(COLUMN_NAME ORDER BY SEQ_IN_INDEX) = 'chave'
                    ) AS indexes_by_name
                );
                SET @config_key_sql = IF(
                    @config_key_indexes IS NULL,
                    'SELECT 1',
                    CONCAT('ALTER TABLE `configuracoes_sistema` ', @config_key_indexes)
                );
                PREPARE config_key_statement FROM @config_key_sql;
                EXECUTE config_key_statement;
                DEALLOCATE PREPARE config_key_statement;
                """);

            migrationBuilder.CreateIndex(
                name: "ux_configuracoes_sistema_tenant_chave",
                table: "configuracoes_sistema",
                columns: new[] { "tenant_id", "chave" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_configuracoes_sistema_tenant_chave",
                table: "configuracoes_sistema");
        }
    }
}
