/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NexumAltivon.API.API.Data.Migrations.Nexum
{
    /// <inheritdoc />
    public partial class RepairPublicSiteMediaReferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE configuracoes_sistema
                SET valor = '/uploads/produtos/Logo-2.png',
                    updated_at = UTC_TIMESTAMP()
                WHERE chave = 'site_logo'
                  AND (
                      valor IS NULL
                      OR TRIM(valor) = ''
                      OR valor IN (
                          '/imagens/homepage/Logo-2.png',
                          '/uploads/site-bbe7b8af15f44e40bee4832882ae2623.png',
                          'https://api.nexumaltivon.com.br/uploads/site-bbe7b8af15f44e40bee4832882ae2623.png'
                      )
                  );

                UPDATE configuracoes_sistema
                SET valor = REPLACE(
                    REPLACE(
                        REPLACE(
                            valor,
                            '/imagens/homepage/banner-ecommerce.svg',
                            '/uploads/Imagens/Gemini_Generated_Image_aowvz5aowvz5aowv.jfif'),
                        '/imagens/homepage/banner-marcas.svg',
                        '/uploads/Imagens/Gemini_Generated_Image_miqkz6miqkz6miqk.jfif'),
                    '/imagens/homepage/banner-atendimento.svg',
                    '/uploads/Imagens/Gemini_Generated_Image_19y5zi19y5zi19y5.jfif'),
                    updated_at = UTC_TIMESTAMP()
                WHERE chave = 'home_hero_slides'
                  AND valor IS NOT NULL
                  AND (
                      valor LIKE '%/imagens/homepage/banner-ecommerce.svg%'
                      OR valor LIKE '%/imagens/homepage/banner-marcas.svg%'
                      OR valor LIKE '%/imagens/homepage/banner-atendimento.svg%'
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
