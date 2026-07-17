/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexumAltivon.API.Data.Migrations.Nexum
{
    /// <inheritdoc />
    public partial class HardenPlatformSso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "mfa_secret",
                table: "usuarios",
                type: "varchar(1024)",
                maxLength: 1024,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(64)",
                oldMaxLength: 64,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<long>(
                name: "mfa_ultimo_passo",
                table: "usuarios",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "token_refresh_expira_em",
                table: "usuarios",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "token_refresh",
                table: "clientes",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "token_refresh_expira_em",
                table: "clientes",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mfa_ultimo_passo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "token_refresh_expira_em",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "token_refresh",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "token_refresh_expira_em",
                table: "clientes");

            migrationBuilder.AlterColumn<string>(
                name: "mfa_secret",
                table: "usuarios",
                type: "varchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(1024)",
                oldMaxLength: 1024,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
