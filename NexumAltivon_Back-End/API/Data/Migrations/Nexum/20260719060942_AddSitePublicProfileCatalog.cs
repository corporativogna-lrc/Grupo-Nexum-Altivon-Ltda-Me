/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexumAltivon.API.API.Data.Migrations.Nexum
{
    /// <inheritdoc />
    public partial class AddSitePublicProfileCatalog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cor_fundo",
                table: "site_perfis_publicos",
                type: "varchar(7)",
                maxLength: 7,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "cor_primaria",
                table: "site_perfis_publicos",
                type: "varchar(7)",
                maxLength: 7,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "cor_secundaria",
                table: "site_perfis_publicos",
                type: "varchar(7)",
                maxLength: 7,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "cor_texto",
                table: "site_perfis_publicos",
                type: "varchar(7)",
                maxLength: 7,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "email_publico",
                table: "site_perfis_publicos",
                type: "varchar(254)",
                maxLength: 254,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "endereco_publico",
                table: "site_perfis_publicos",
                type: "varchar(300)",
                maxLength: 300,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "site_url",
                table: "site_perfis_publicos",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "telefone_publico",
                table: "site_perfis_publicos",
                type: "varchar(30)",
                maxLength: 30,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "site_perfis_publicos_produtos",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    perfil_publico_id = table.Column<Guid>(type: "char(36)", nullable: false, collation: "ascii_general_ci"),
                    produto_id = table.Column<int>(type: "int", nullable: false),
                    publicado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ordem_exibicao = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_site_perfis_publicos_produtos", x => x.id);
                    table.ForeignKey(
                        name: "FK_site_perfis_publicos_produtos_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_site_perfis_publicos_produtos_site_perfis_publicos_perfil_pu~",
                        column: x => x.perfil_publico_id,
                        principalTable: "site_perfis_publicos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_site_perfis_publicos_produtos_perfil_publico_id",
                table: "site_perfis_publicos_produtos",
                column: "perfil_publico_id");

            migrationBuilder.CreateIndex(
                name: "IX_site_perfis_publicos_produtos_produto_id",
                table: "site_perfis_publicos_produtos",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_site_perfis_publicos_produtos_tenant_deleted",
                table: "site_perfis_publicos_produtos",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_site_perfis_publicos_produtos_tenant_id_perfil_publico_id_pr~",
                table: "site_perfis_publicos_produtos",
                columns: new[] { "tenant_id", "perfil_publico_id", "produto_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_site_perfis_publicos_produtos_tenant_id_perfil_publico_id_pu~",
                table: "site_perfis_publicos_produtos",
                columns: new[] { "tenant_id", "perfil_publico_id", "publicado", "ordem_exibicao" });

            migrationBuilder.Sql(
                """
                UPDATE site_perfis_publicos
                SET banner_url = CASE slug
                    WHEN 'chronos' THEN '/uploads/Imagens/Gemini_Generated_Image_shv10ashv10ashv1.png'
                    WHEN 'grann-tur' THEN '/uploads/Imagens/Gemini_Generated_Image_cnu682cnu682cnu6.jfif'
                    WHEN 'gran-tur' THEN '/uploads/Imagens/Gemini_Generated_Image_cnu682cnu682cnu6.jfif'
                    WHEN 'moda-mim' THEN '/uploads/Imagens/Gemini_Generated_Image_lbd948lbd948lbd9.jfif'
                    WHEN 'geracao-top' THEN '/uploads/Imagens/Gemini_Generated_Image_aowvz5aowvz5aowv.jfif'
                    WHEN 'estruturaline' THEN '/uploads/Imagens/Gemini_Generated_Image_s9ijb8s9ijb8s9ij.jfif'
                    WHEN 'gran-fest' THEN '/uploads/Imagens/Gemini_Generated_Image_miqkz6miqkz6miqk.jfif'
                    WHEN 'gran-festas' THEN '/uploads/Imagens/Gemini_Generated_Image_miqkz6miqkz6miqk.jfif'
                    ELSE banner_url
                END
                WHERE tipo_perfil = 'Loja'
                  AND is_deleted = 0
                  AND slug IN ('chronos', 'grann-tur', 'gran-tur', 'moda-mim', 'geracao-top', 'estruturaline', 'gran-fest', 'gran-festas')
                  AND (
                      banner_url IS NULL
                      OR TRIM(banner_url) = ''
                      OR banner_url IN (
                          '/imagens/homepage/loja-gran-tur.svg',
                          '/imagens/homepage/loja-chronos.svg',
                          '/imagens/homepage/loja-moda-mim.svg',
                          '/imagens/homepage/loja-geracao-top.svg',
                          '/imagens/homepage/loja-estruturaline.svg',
                          '/imagens/homepage/loja-gran-festas.svg'
                      )
                  );

                UPDATE configuracoes_sistema
                SET valor = REPLACE(
                    REPLACE(
                        REPLACE(
                            REPLACE(
                                REPLACE(
                                    REPLACE(
                                        valor,
                                        '/imagens/homepage/loja-gran-tur.svg',
                                        '/uploads/Imagens/Gemini_Generated_Image_cnu682cnu682cnu6.jfif'),
                                    '/imagens/homepage/loja-chronos.svg',
                                    '/uploads/Imagens/Gemini_Generated_Image_shv10ashv10ashv1.png'),
                                '/imagens/homepage/loja-moda-mim.svg',
                                '/uploads/Imagens/Gemini_Generated_Image_lbd948lbd948lbd9.jfif'),
                            '/imagens/homepage/loja-geracao-top.svg',
                            '/uploads/Imagens/Gemini_Generated_Image_aowvz5aowvz5aowv.jfif'),
                        '/imagens/homepage/loja-estruturaline.svg',
                        '/uploads/Imagens/Gemini_Generated_Image_s9ijb8s9ijb8s9ij.jfif'),
                    '/imagens/homepage/loja-gran-festas.svg',
                    '/uploads/Imagens/Gemini_Generated_Image_miqkz6miqkz6miqk.jfif')
                WHERE chave = 'home_lojas_cards'
                  AND valor IS NOT NULL
                  AND valor LIKE '%/imagens/homepage/loja-%';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "site_perfis_publicos_produtos");

            migrationBuilder.DropColumn(
                name: "cor_fundo",
                table: "site_perfis_publicos");

            migrationBuilder.DropColumn(
                name: "cor_primaria",
                table: "site_perfis_publicos");

            migrationBuilder.DropColumn(
                name: "cor_secundaria",
                table: "site_perfis_publicos");

            migrationBuilder.DropColumn(
                name: "cor_texto",
                table: "site_perfis_publicos");

            migrationBuilder.DropColumn(
                name: "email_publico",
                table: "site_perfis_publicos");

            migrationBuilder.DropColumn(
                name: "endereco_publico",
                table: "site_perfis_publicos");

            migrationBuilder.DropColumn(
                name: "site_url",
                table: "site_perfis_publicos");

            migrationBuilder.DropColumn(
                name: "telefone_publico",
                table: "site_perfis_publicos");
        }
    }
}
