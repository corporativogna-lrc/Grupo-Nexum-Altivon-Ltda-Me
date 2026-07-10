/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace NexumAltivon.API.Data.Migrations.Nexum
{
    /// <inheritdoc />
    public partial class InitialNexumOperational : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    senha_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cpf_cnpj = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    rg_ie = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_nascimento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    whatsapp = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    newsletter = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    vip = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    pontos_fidelidade = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ultimo_acesso = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    token_reset_senha = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_confirmacao_email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    confirmado_em = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    token_expira_em = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "configuracoes_sistema",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    chave = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valor = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    grupo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    editavel = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_configuracoes_sistema", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "cupons",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    valor_minimo_pedido = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    valor_maximo_desconto = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    quantidade_usos = table.Column<int>(type: "int", nullable: true),
                    usos_atuais = table.Column<int>(type: "int", nullable: false),
                    quantidade_por_cliente = table.Column<int>(type: "int", nullable: false),
                    valido_de = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    valido_ate = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    lojas_aplicaveis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    categorias_aplicaveis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    produtos_aplicaveis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    clientes_aplicaveis = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    primeiro_compra_only = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cupons", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "dropshipping_config",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slug = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    api_endpoint = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    api_key = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    api_secret = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_dropshipping_config", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "erp_empresas_grupo",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tipo_cadastro = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    razao_social = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nome_fantasia = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnpj = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    inscricao_estadual = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    inscricao_municipal = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    matriz_filial = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    codigo_empresa = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    regime_tributario = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    crt = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnae_principal = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnaes_secundarios = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    categoria_fiscal = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subcategoria_fiscal = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ncm_padrao = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    natureza_operacao_padrao = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    responsavel_legal = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    responsavel_fiscal = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_fiscal = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email_comercial = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefone = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    whatsapp = table.Column<string>(type: "varchar(25)", maxLength: 25, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cep = table.Column<string>(type: "varchar(12)", maxLength: 12, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logradouro = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    complemento = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bairro = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cidade = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pais = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ambiente_nfe = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    serie_nfe = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    serie_nfce = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    modelo_documento_pdv = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ambiente_nfce = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    proxima_nfce_numero = table.Column<int>(type: "int", nullable: true),
                    nfce_csc = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nfce_csc_id_token = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pdv_serie_sat = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pdv_impressora_fiscal = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pdv_nome_caixa_padrao = table.Column<string>(type: "varchar(80)", maxLength: 80, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pdv_contingencia_offline = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    proxima_nfe_numero = table.Column<int>(type: "int", nullable: true),
                    cfop_padrao_interno = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cfop_padrao_interestadual = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    aliquota_icms_interna = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    aliquota_icms_interestadual = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    aliquota_pis = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    aliquota_cofins = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    aliquota_iss = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    aliquota_ipi = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    carga_tributaria_percentual = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    perfil_tributacao = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usa_st_legado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    destaca_icms_st_separado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    custo_operacional_percentual = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    margem_minima_percentual = table.Column<decimal>(type: "decimal(10,4)", precision: 10, scale: 4, nullable: true),
                    prioridade_fiscal = table.Column<int>(type: "int", nullable: false),
                    permite_nfe_entrada = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    permite_nfe_saida = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    permite_dropshipping = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    permite_marketplace = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    emitente_preferencial = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ativa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    beneficios_estrategicos = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contrato_resumo = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_erp_empresas_grupo", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "logs_auditoria",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tabela = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    registro_id = table.Column<int>(type: "int", nullable: false),
                    acao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    usuario_id = table.Column<int>(type: "int", nullable: true),
                    usuario_tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_address = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dados_anteriores = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dados_novos = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    endpoint = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_logs_auditoria", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "lojas",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slug = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    segmento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logo = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    banner = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cor_primaria = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cor_secundaria = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    dominio = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ativa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ordem_exibicao = table.Column<int>(type: "int", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_lojas", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "notificacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mensagem = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    destinatario_tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    destinatario_id = table.Column<int>(type: "int", nullable: true),
                    lida = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    data_leitura = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    link_acao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacoes", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "transportadoras",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slug = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    api_endpoint = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    api_token = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    api_sandbox = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ativa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transportadoras", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    senha_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    perfil = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    avatar = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ultimo_login = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    token_refresh = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mfa_habilitado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    mfa_secret = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    mfa_confirmado_em = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "enderecos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    apelido = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cep = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    logradouro = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numero = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    complemento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    bairro = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pais = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    padrao = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_enderecos", x => x.id);
                    table.ForeignKey(
                        name: "FK_enderecos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    loja_id = table.Column<int>(type: "int", nullable: false),
                    nome = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slug = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    imagem = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    categoria_pai_id = table.Column<int>(type: "int", nullable: true),
                    ordem = table.Column<int>(type: "int", nullable: false),
                    ativa = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                    table.ForeignKey(
                        name: "FK_categorias_categorias_categoria_pai_id",
                        column: x => x.categoria_pai_id,
                        principalTable: "categorias",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_categorias_lojas_loja_id",
                        column: x => x.loja_id,
                        principalTable: "lojas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fornecedores",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    razao_social = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nome_fantasia = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnpj = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ie = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    whatsapp = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    endereco = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cidade = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estado = table.Column<string>(type: "varchar(2)", maxLength: 2, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cep = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    segmento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    loja_vinculada_id = table.Column<int>(type: "int", nullable: true),
                    comissao_percentual = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    prazo_entrega_dias = table.Column<int>(type: "int", nullable: false),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fornecedores", x => x.id);
                    table.ForeignKey(
                        name: "FK_fornecedores_lojas_loja_vinculada_id",
                        column: x => x.loja_vinculada_id,
                        principalTable: "lojas",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "marketplaces",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nome = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slug = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    app_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    app_secret = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    access_token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    refresh_token = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    token_expira_em = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    loja_vinculada_id = table.Column<int>(type: "int", nullable: true),
                    seller_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sandbox = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ultima_sincronizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_marketplaces", x => x.id);
                    table.ForeignKey(
                        name: "FK_marketplaces_lojas_loja_vinculada_id",
                        column: x => x.loja_vinculada_id,
                        principalTable: "lojas",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "crm_leads",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    origem = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telefone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    whatsapp = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    empresa = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnpj = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    segmento = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    proposta = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    experiencia = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    responsavel_id = table.Column<int>(type: "int", nullable: true),
                    prioridade = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    anotacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_leads", x => x.id);
                    table.ForeignKey(
                        name: "FK_crm_leads_usuarios_responsavel_id",
                        column: x => x.responsavel_id,
                        principalTable: "usuarios",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    numero_pedido = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cliente_id = table.Column<int>(type: "int", nullable: false),
                    endereco_entrega_id = table.Column<int>(type: "int", nullable: true),
                    loja_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_pagamento = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    meio_pagamento = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gateway_pagamento = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gateway_transacao_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    subtotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    desconto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    frete_valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    frete_metodo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    frete_codigo_rastreio = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    frete_transportadora = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    frete_prazo_dias = table.Column<int>(type: "int", nullable: false),
                    total = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    parcelas = table.Column<int>(type: "int", nullable: false),
                    juros = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    cupom_codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cupom_desconto = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    observacoes_cliente = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observacoes_internas = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ip_cliente = table.Column<string>(type: "varchar(45)", maxLength: 45, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_agent = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    origem = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    marketplace_origem = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    marketplace_pedido_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_pagamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    data_envio = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    data_entrega = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.id);
                    table.ForeignKey(
                        name: "FK_pedidos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_pedidos_enderecos_endereco_entrega_id",
                        column: x => x.endereco_entrega_id,
                        principalTable: "enderecos",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_pedidos_lojas_loja_id",
                        column: x => x.loja_id,
                        principalTable: "lojas",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "produtos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    loja_id = table.Column<int>(type: "int", nullable: false),
                    categoria_id = table.Column<int>(type: "int", nullable: true),
                    sku = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nome = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    slug = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao_curta = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao_longa = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    preco = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    preco_promocional = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    custo = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    peso = table.Column<decimal>(type: "decimal(8,3)", nullable: false),
                    altura = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    largura = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    comprimento = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    imagem_principal = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    imagens_galeria = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    estoque_minimo = table.Column<int>(type: "int", nullable: false),
                    estoque_atual = table.Column<int>(type: "int", nullable: false),
                    estoque_reservado = table.Column<int>(type: "int", nullable: false),
                    tipo_produto = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fornecedor_id = table.Column<int>(type: "int", nullable: true),
                    marca = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tags = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    seo_titulo = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    seo_descricao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    seo_keywords = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    codigo_barras = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    qr_code = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    identificacao_estoque = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    destaque = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ativo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_produtos", x => x.id);
                    table.ForeignKey(
                        name: "FK_produtos_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_produtos_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_produtos_lojas_loja_id",
                        column: x => x.loja_id,
                        principalTable: "lojas",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "crm_atendimentos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    lead_id = table.Column<int>(type: "int", nullable: true),
                    cliente_id = table.Column<int>(type: "int", nullable: true),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    assunto = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    responsavel_id = table.Column<int>(type: "int", nullable: true),
                    data_agendamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crm_atendimentos", x => x.id);
                    table.ForeignKey(
                        name: "FK_crm_atendimentos_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_crm_atendimentos_crm_leads_lead_id",
                        column: x => x.lead_id,
                        principalTable: "crm_leads",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_crm_atendimentos_usuarios_responsavel_id",
                        column: x => x.responsavel_id,
                        principalTable: "usuarios",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "envios",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    transportadora_id = table.Column<int>(type: "int", nullable: true),
                    codigo_rastreio = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    etiqueta_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    etiqueta_pdf = table.Column<byte[]>(type: "longblob", nullable: true),
                    status_envio = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    preco = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    prazo_dias = table.Column<int>(type: "int", nullable: false),
                    data_postagem = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    data_entrega_estimada = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    data_entrega_real = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    eventos_rastreamento = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_envios", x => x.id);
                    table.ForeignKey(
                        name: "FK_envios_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_envios_transportadoras_transportadora_id",
                        column: x => x.transportadora_id,
                        principalTable: "transportadoras",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "financeiro",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: true),
                    tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    categoria = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    descricao = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    data_vencimento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    data_pagamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    meio_pagamento = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    conta_bancaria = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    comprovante_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_financeiro", x => x.id);
                    table.ForeignKey(
                        name: "FK_financeiro_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "fiscal",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    empresa_grupo_id = table.Column<int>(type: "int", nullable: true),
                    empresa_emitente = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    codigo_empresa_emitente = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    cnpj_emitente = table.Column<string>(type: "varchar(18)", maxLength: 18, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    numero_nfe = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    serie = table.Column<string>(type: "varchar(5)", maxLength: 5, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    chave_acesso = table.Column<string>(type: "varchar(44)", maxLength: 44, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    xml_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    danfe_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_nfe = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valor_total = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    cfop = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    natureza_operacao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ambiente_documento = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    modelo_documento = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_automacao = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resumo_roteamento = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    payload_operacao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_emissao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    data_autorizacao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    email_cliente_notificado_em = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    protocolo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    motivo_cancelamento = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscal", x => x.id);
                    table.ForeignKey(
                        name: "FK_fiscal_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pagamentos",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    gateway = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    gateway_transacao_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    metodo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    valor = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    valor_liquido = table.Column<decimal>(type: "decimal(10,2)", nullable: true),
                    taxa_gateway = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    parcelas = table.Column<int>(type: "int", nullable: false),
                    bandeira = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ultimos_digitos = table.Column<string>(type: "varchar(4)", maxLength: 4, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    nsu = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    autorizacao_codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pix_qrcode = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pix_expiracao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    boleto_url = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    boleto_codigo_barras = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    boleto_vencimento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    webhook_payload = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    data_processamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagamentos", x => x.id);
                    table.ForeignKey(
                        name: "FK_pagamentos_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "carrinho",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    cliente_id = table.Column<int>(type: "int", nullable: true),
                    sessao_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    produto_id = table.Column<int>(type: "int", nullable: false),
                    quantidade = table.Column<int>(type: "int", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    variacao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_carrinho", x => x.id);
                    table.ForeignKey(
                        name: "FK_carrinho_clientes_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "clientes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_carrinho_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "pedido_itens",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    pedido_id = table.Column<int>(type: "int", nullable: false),
                    produto_id = table.Column<int>(type: "int", nullable: true),
                    nome_produto = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    sku_produto = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    imagem_produto = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    quantidade = table.Column<int>(type: "int", nullable: false),
                    preco_unitario = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    preco_total = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    desconto_item = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    fornecedor_id = table.Column<int>(type: "int", nullable: true),
                    comissao_fornecedor = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    tipo_fulfillment = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status_item = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci"),
                    deleted_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    is_deleted = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: false),
                    row_version = table.Column<byte[]>(type: "blob", nullable: true),
                    tenant_id = table.Column<Guid>(type: "char(36)", nullable: false, defaultValue: new Guid("00000000-0000-0000-0000-000000000001"), collation: "ascii_general_ci"),
                    updated_by_user_id = table.Column<Guid>(type: "char(36)", nullable: true, collation: "ascii_general_ci")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedido_itens", x => x.id);
                    table.ForeignKey(
                        name: "FK_pedido_itens_fornecedores_fornecedor_id",
                        column: x => x.fornecedor_id,
                        principalTable: "fornecedores",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_pedido_itens_pedidos_pedido_id",
                        column: x => x.pedido_id,
                        principalTable: "pedidos",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pedido_itens_produtos_produto_id",
                        column: x => x.produto_id,
                        principalTable: "produtos",
                        principalColumn: "id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "dropshipping_config",
                columns: new[] { "id", "api_endpoint", "api_key", "api_secret", "ativo", "created_at", "created_by_user_id", "deleted_at", "nome", "slug", "tipo", "updated_at", "updated_by_user_id" },
                values: new object[,]
                {
                    { 1, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6857), null, null, "AliExpress", "aliexpress", "AliExpress", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6858), null },
                    { 2, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6863), null, null, "CJ Dropshipping", "cj-dropshipping", "CJDropshipping", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6863), null },
                    { 3, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6866), null, null, "Dropi", "dropi", "Dropi", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6867), null },
                    { 4, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6869), null, null, "Cartpanda HUB", "cartpanda", "Cartpanda", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6870), null },
                    { 5, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6872), null, null, "Nuvemshop HUB", "nuvemshop", "Nuvemshop", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6872), null }
                });

            migrationBuilder.InsertData(
                table: "lojas",
                columns: new[] { "id", "ativa", "banner", "cor_primaria", "cor_secundaria", "created_at", "created_by_user_id", "deleted_at", "descricao", "dominio", "logo", "nome", "ordem_exibicao", "segmento", "slug", "updated_at", "updated_by_user_id" },
                values: new object[,]
                {
                    { 1, true, null, "#C9A227", "#1E3A5F", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5764), null, null, "Mochilas, malas, acessórios de viagem", "grann-tur.nexumaltivon.com", null, "Grann-Tur", 1, "Viagens & Turismo", "grann-tur", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5769), null },
                    { 2, true, null, "#C9A227", "#2E5A8F", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5777), null, null, "Relógios que marcam estilo", "chronos.nexumaltivon.com", null, "Chronos", 2, "Relógios & Acessórios", "chronos", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5778), null },
                    { 3, true, null, "#C9A227", "#8B1E3F", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5786), null, null, "Tendências que vestem a sua personalidade", "moda-mim.nexumaltivon.com", null, "Moda Mim", 3, "Moda & Vestuário", "moda-mim", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5787), null },
                    { 4, true, null, "#C9A227", "#0F4C3A", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5793), null, null, "Tecnologia de ponta ao alcance de todos", "geracao-top.nexumaltivon.com", null, "Geração Top+", 4, "Tecnologia & Gadgets", "geracao-top", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5795), null },
                    { 5, true, null, "#C9A227", "#4A3728", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5802), null, null, "Ferramentas e materiais de construção", "estruturaline.nexumaltivon.com", null, "Estruturaline", 5, "Construção & Estruturas", "estruturaline", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5804), null },
                    { 6, true, null, "#C9A227", "#6B2D5C", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5810), null, null, "Decorações e utensílios para festas", "gran-fest.nexumaltivon.com", null, "Gran-fest-festas", 6, "Festas & Eventos", "gran-fest", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(5811), null }
                });

            migrationBuilder.InsertData(
                table: "marketplaces",
                columns: new[] { "id", "access_token", "app_id", "app_secret", "ativo", "created_at", "created_by_user_id", "deleted_at", "loja_vinculada_id", "nome", "refresh_token", "sandbox", "seller_id", "slug", "tipo", "token_expira_em", "ultima_sincronizacao", "updated_at", "updated_by_user_id" },
                values: new object[,]
                {
                    { 1, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6718), null, null, null, "Mercado Livre", null, true, null, "mercado-livre", "MercadoLivre", null, null, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6719), null },
                    { 2, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6725), null, null, null, "Shopee", null, true, null, "shopee", "Shopee", null, null, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6727), null },
                    { 3, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6731), null, null, null, "Amazon", null, true, null, "amazon", "Amazon", null, null, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6733), null },
                    { 4, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6737), null, null, null, "Magalu", null, true, null, "magalu", "Magalu", null, null, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6739), null },
                    { 5, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6743), null, null, null, "Americanas", null, true, null, "americanas", "B2W", null, null, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6745), null },
                    { 6, null, null, null, false, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6749), null, null, null, "Via Varejo", null, true, null, "via-varejo", "B2W", null, null, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6750), null }
                });

            migrationBuilder.InsertData(
                table: "transportadoras",
                columns: new[] { "id", "api_endpoint", "api_sandbox", "api_token", "ativa", "created_at", "created_by_user_id", "deleted_at", "nome", "slug", "tipo", "updated_at", "updated_by_user_id" },
                values: new object[,]
                {
                    { 1, null, true, null, true, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6512), null, null, "Correios", "correios", "Correios", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6514), null },
                    { 2, null, true, null, true, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6519), null, null, "Melhor Envio", "melhor-envio", "Hub", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6520), null },
                    { 3, null, true, null, true, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6525), null, null, "Jadlog", "jadlog", "Transportadora", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6527), null },
                    { 4, null, true, null, true, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6531), null, null, "Loggi", "loggi", "Logistica", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6532), null },
                    { 5, null, true, null, true, new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6536), null, null, "Kangu", "kangu", "Hub", new DateTime(2026, 7, 5, 11, 43, 18, 561, DateTimeKind.Utc).AddTicks(6538), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_carrinho_cliente_id",
                table: "carrinho",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_carrinho_produto_id",
                table: "carrinho",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_carrinho_tenant_deleted",
                table: "carrinho",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_categoria_pai_id",
                table: "categorias",
                column: "categoria_pai_id");

            migrationBuilder.CreateIndex(
                name: "IX_categorias_loja_id",
                table: "categorias",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "ix_categorias_tenant_deleted",
                table: "categorias",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_clientes_cpf_cnpj",
                table: "clientes",
                column: "cpf_cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clientes_email",
                table: "clientes",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_clientes_tenant_deleted",
                table: "clientes",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "ix_configuracoes_sistema_tenant_deleted",
                table: "configuracoes_sistema",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_crm_atendimentos_cliente_id",
                table: "crm_atendimentos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_crm_atendimentos_lead_id",
                table: "crm_atendimentos",
                column: "lead_id");

            migrationBuilder.CreateIndex(
                name: "IX_crm_atendimentos_responsavel_id",
                table: "crm_atendimentos",
                column: "responsavel_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_atendimentos_tenant_deleted",
                table: "crm_atendimentos",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_crm_leads_responsavel_id",
                table: "crm_leads",
                column: "responsavel_id");

            migrationBuilder.CreateIndex(
                name: "ix_crm_leads_tenant_deleted",
                table: "crm_leads",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_cupons_codigo",
                table: "cupons",
                column: "codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_cupons_tenant_deleted",
                table: "cupons",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_dropshipping_config_slug",
                table: "dropshipping_config",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_dropshipping_config_tenant_deleted",
                table: "dropshipping_config",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_enderecos_cliente_id",
                table: "enderecos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "ix_enderecos_tenant_deleted",
                table: "enderecos",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_envios_pedido_id",
                table: "envios",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_envios_tenant_deleted",
                table: "envios",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_envios_transportadora_id",
                table: "envios",
                column: "transportadora_id");

            migrationBuilder.CreateIndex(
                name: "IX_erp_empresas_grupo_cnpj",
                table: "erp_empresas_grupo",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_erp_empresas_grupo_codigo_empresa",
                table: "erp_empresas_grupo",
                column: "codigo_empresa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_erp_empresas_grupo_tenant_deleted",
                table: "erp_empresas_grupo",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_financeiro_pedido_id",
                table: "financeiro",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_financeiro_tenant_deleted",
                table: "financeiro",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_fiscal_pedido_id",
                table: "fiscal",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_fiscal_tenant_deleted",
                table: "fiscal",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_fornecedores_cnpj",
                table: "fornecedores",
                column: "cnpj",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fornecedores_loja_vinculada_id",
                table: "fornecedores",
                column: "loja_vinculada_id");

            migrationBuilder.CreateIndex(
                name: "ix_fornecedores_tenant_deleted",
                table: "fornecedores",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "ix_logs_auditoria_tenant_deleted",
                table: "logs_auditoria",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_lojas_slug",
                table: "lojas",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lojas_tenant_deleted",
                table: "lojas",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_marketplaces_loja_vinculada_id",
                table: "marketplaces",
                column: "loja_vinculada_id");

            migrationBuilder.CreateIndex(
                name: "IX_marketplaces_slug",
                table: "marketplaces",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_marketplaces_tenant_deleted",
                table: "marketplaces",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "ix_notificacoes_tenant_deleted",
                table: "notificacoes",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_pagamentos_pedido_id",
                table: "pagamentos",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "ix_pagamentos_tenant_deleted",
                table: "pagamentos",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_pedido_itens_fornecedor_id",
                table: "pedido_itens",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_itens_pedido_id",
                table: "pedido_itens",
                column: "pedido_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedido_itens_produto_id",
                table: "pedido_itens",
                column: "produto_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedido_itens_tenant_deleted",
                table: "pedido_itens",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_cliente_id",
                table: "pedidos",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_endereco_entrega_id",
                table: "pedidos",
                column: "endereco_entrega_id");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_loja_id",
                table: "pedidos",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "ix_pedidos_tenant_deleted",
                table: "pedidos",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_produtos_categoria_id",
                table: "produtos",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_fornecedor_id",
                table: "produtos",
                column: "fornecedor_id");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_loja_id",
                table: "produtos",
                column: "loja_id");

            migrationBuilder.CreateIndex(
                name: "IX_produtos_sku",
                table: "produtos",
                column: "sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_produtos_tenant_deleted",
                table: "produtos",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_transportadoras_slug",
                table: "transportadoras",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_transportadoras_tenant_deleted",
                table: "transportadoras",
                columns: new[] { "tenant_id", "is_deleted" });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_email",
                table: "usuarios",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_usuarios_tenant_deleted",
                table: "usuarios",
                columns: new[] { "tenant_id", "is_deleted" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "carrinho");

            migrationBuilder.DropTable(
                name: "configuracoes_sistema");

            migrationBuilder.DropTable(
                name: "crm_atendimentos");

            migrationBuilder.DropTable(
                name: "cupons");

            migrationBuilder.DropTable(
                name: "dropshipping_config");

            migrationBuilder.DropTable(
                name: "envios");

            migrationBuilder.DropTable(
                name: "erp_empresas_grupo");

            migrationBuilder.DropTable(
                name: "financeiro");

            migrationBuilder.DropTable(
                name: "fiscal");

            migrationBuilder.DropTable(
                name: "logs_auditoria");

            migrationBuilder.DropTable(
                name: "marketplaces");

            migrationBuilder.DropTable(
                name: "notificacoes");

            migrationBuilder.DropTable(
                name: "pagamentos");

            migrationBuilder.DropTable(
                name: "pedido_itens");

            migrationBuilder.DropTable(
                name: "crm_leads");

            migrationBuilder.DropTable(
                name: "transportadoras");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "produtos");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "enderecos");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "fornecedores");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "lojas");
        }
    }
}
