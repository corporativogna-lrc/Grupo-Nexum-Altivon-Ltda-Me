/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7182
 */

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexumAltivon.API.Data.Migrations.Nexum
{
    /// <inheritdoc />
    public partial class AddLogisticsTrackingAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "logistica_rastreamento_consultas",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    codigo_rastreio = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    provedor = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    configurada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    operacional = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    http_status_code = table.Column<int>(type: "int", nullable: true),
                    status_externo = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantidade_eventos = table.Column<int>(type: "int", nullable: false),
                    eventos_json = table.Column<string>(type: "mediumtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pendencias_json = table.Column<string>(type: "text", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resposta_sha256 = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    consultado_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    row_version = table.Column<byte[]>(type: "blob", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logistica_rastreamento_consultas", x => x.id);
                    table.ForeignKey(
                        name: "FK_logistica_rastreamento_consultas_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_logistica_rastreamento_consultas_codigo_rastreio_consultado_~",
                table: "logistica_rastreamento_consultas",
                columns: new[] { "codigo_rastreio", "consultado_at" });

            migrationBuilder.CreateIndex(
                name: "IX_logistica_rastreamento_consultas_pedido_id_consultado_at",
                table: "logistica_rastreamento_consultas",
                columns: new[] { "pedido_id", "consultado_at" });

            migrationBuilder.CreateIndex(
                name: "ix_logistica_rastreamento_consultas_tenant_deleted",
                table: "logistica_rastreamento_consultas",
                columns: new[] { "tenant_id", "is_deleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "logistica_rastreamento_consultas");
        }
    }
}
