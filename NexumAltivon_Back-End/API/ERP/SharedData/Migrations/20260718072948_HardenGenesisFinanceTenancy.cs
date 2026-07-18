/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7181
 */

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexumAltivon.API.ERP.SharedData.Migrations
{
    /// <inheritdoc />
    public partial class HardenGenesisFinanceTenancy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "erp_fluxo_caixa",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                table: "erp_fluxo_caixa",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "erp_fluxo_caixa",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "erp_fluxo_caixa",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "erp_fluxo_caixa",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "erp_fluxo_caixa",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "erp_fluxo_caixa",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                table: "erp_fluxo_caixa",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "numero_documento",
                table: "erp_contas_receber",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "erp_contas_receber",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                table: "erp_contas_receber",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "erp_contas_receber",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "erp_contas_receber",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "erp_contas_receber",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "erp_contas_receber",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "erp_contas_receber",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                table: "erp_contas_receber",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AlterColumn<string>(
                name: "numero_documento",
                table: "erp_contas_pagar",
                type: "varchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "created_at",
                table: "erp_contas_pagar",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "created_by_user_id",
                table: "erp_contas_pagar",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "deleted_at",
                table: "erp_contas_pagar",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "is_deleted",
                table: "erp_contas_pagar",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "row_version",
                table: "erp_contas_pagar",
                type: "binary(16)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tenant_id",
                table: "erp_contas_pagar",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.AddColumn<DateTime>(
                name: "updated_at",
                table: "erp_contas_pagar",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "updated_by_user_id",
                table: "erp_contas_pagar",
                type: "char(36)",
                nullable: true,
                collation: "ascii_general_ci");

            migrationBuilder.Sql(
                """
                UPDATE erp_contas_pagar
                SET tenant_id = '00000000-0000-0000-0000-000000000001'
                WHERE tenant_id IS NULL OR tenant_id = '' OR tenant_id = '00000000-0000-0000-0000-000000000000';
                UPDATE erp_contas_pagar
                SET created_at = COALESCE(data_emissao, UTC_TIMESTAMP(6))
                WHERE created_at IS NULL;
                UPDATE erp_contas_pagar
                SET row_version = UNHEX(REPLACE(UUID(), '-', ''))
                WHERE row_version IS NULL OR OCTET_LENGTH(row_version) <> 16;

                UPDATE erp_contas_receber
                SET tenant_id = '00000000-0000-0000-0000-000000000001'
                WHERE tenant_id IS NULL OR tenant_id = '' OR tenant_id = '00000000-0000-0000-0000-000000000000';
                UPDATE erp_contas_receber
                SET created_at = COALESCE(data_emissao, UTC_TIMESTAMP(6))
                WHERE created_at IS NULL;
                UPDATE erp_contas_receber
                SET row_version = UNHEX(REPLACE(UUID(), '-', ''))
                WHERE row_version IS NULL OR OCTET_LENGTH(row_version) <> 16;

                UPDATE erp_fluxo_caixa
                SET tenant_id = '00000000-0000-0000-0000-000000000001'
                WHERE tenant_id IS NULL OR tenant_id = '' OR tenant_id = '00000000-0000-0000-0000-000000000000';
                UPDATE erp_fluxo_caixa
                SET created_at = COALESCE(criado_em, data, UTC_TIMESTAMP(6))
                WHERE created_at IS NULL;
                UPDATE erp_fluxo_caixa
                SET row_version = UNHEX(REPLACE(UUID(), '-', ''))
                WHERE row_version IS NULL OR OCTET_LENGTH(row_version) <> 16;

                ALTER TABLE erp_contas_pagar
                    MODIFY tenant_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    MODIFY row_version BINARY(16) NOT NULL,
                    MODIFY created_at DATETIME(6) NOT NULL;
                ALTER TABLE erp_contas_receber
                    MODIFY tenant_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    MODIFY row_version BINARY(16) NOT NULL,
                    MODIFY created_at DATETIME(6) NOT NULL;
                ALTER TABLE erp_fluxo_caixa
                    MODIFY tenant_id CHAR(36) CHARACTER SET ascii COLLATE ascii_general_ci NOT NULL,
                    MODIFY row_version BINARY(16) NOT NULL,
                    MODIFY created_at DATETIME(6) NOT NULL;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_erp_fluxo_caixa_tenant_id_is_deleted",
                table: "erp_fluxo_caixa",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_contas_receber_tenant_id_is_deleted",
                table: "erp_contas_receber",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_erp_contas_pagar_tenant_id_is_deleted",
                table: "erp_contas_pagar",
                columns: new[] { "tenant_id", "is_deleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_erp_fluxo_caixa_tenant_id_is_deleted",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropIndex(
                name: "IX_erp_contas_receber_tenant_id_is_deleted",
                table: "erp_contas_receber");

            migrationBuilder.DropIndex(
                name: "IX_erp_contas_pagar_tenant_id_is_deleted",
                table: "erp_contas_pagar");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "erp_fluxo_caixa");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "erp_contas_receber");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "erp_contas_receber");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "erp_contas_receber");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "erp_contas_receber");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "erp_contas_receber");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "erp_contas_receber");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "erp_contas_receber");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "erp_contas_receber");

            migrationBuilder.DropColumn(
                name: "created_at",
                table: "erp_contas_pagar");

            migrationBuilder.DropColumn(
                name: "created_by_user_id",
                table: "erp_contas_pagar");

            migrationBuilder.DropColumn(
                name: "deleted_at",
                table: "erp_contas_pagar");

            migrationBuilder.DropColumn(
                name: "is_deleted",
                table: "erp_contas_pagar");

            migrationBuilder.DropColumn(
                name: "row_version",
                table: "erp_contas_pagar");

            migrationBuilder.DropColumn(
                name: "tenant_id",
                table: "erp_contas_pagar");

            migrationBuilder.DropColumn(
                name: "updated_at",
                table: "erp_contas_pagar");

            migrationBuilder.DropColumn(
                name: "updated_by_user_id",
                table: "erp_contas_pagar");

            migrationBuilder.AlterColumn<string>(
                name: "numero_documento",
                table: "erp_contas_receber",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "numero_documento",
                table: "erp_contas_pagar",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(40)",
                oldMaxLength: 40)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
