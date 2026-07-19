/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoPerfilPublico
{
    Loja,
    Fornecedor,
    ParceiroVenda,
    ParceiroCompra,
    Marketplace
}

public enum OrigemPerfilPublico
{
    Loja,
    Fornecedor,
    Marketplace,
    Parceiro
}

[Table("site_perfis_publicos")]
public sealed class SitePerfilPublico : Sys_AuditableEntity
{
    [Required]
    [Column("tipo_perfil")]
    public TipoPerfilPublico TipoPerfil { get; set; }

    [Required]
    [Column("origem_tipo")]
    public OrigemPerfilPublico OrigemTipo { get; set; }

    [Column("origem_id")]
    public int? OrigemId { get; set; }

    [Required]
    [MaxLength(160)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    [Column("slug")]
    public string Slug { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    [Column("segmento")]
    public string Segmento { get; set; } = string.Empty;

    [Required]
    [MaxLength(240)]
    [Column("atividade")]
    public string Atividade { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    [Column("descricao")]
    public string Descricao { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("logo_url")]
    public string? LogoUrl { get; set; }

    [MaxLength(500)]
    [Column("banner_url")]
    public string? BannerUrl { get; set; }

    [MaxLength(80)]
    [Column("icone")]
    public string? Icone { get; set; }

    [MaxLength(80)]
    [Column("cta_texto")]
    public string? CtaTexto { get; set; }

    [MaxLength(500)]
    [Column("cta_url")]
    public string? CtaUrl { get; set; }

    [MaxLength(500)]
    [Column("site_url")]
    public string? SiteUrl { get; set; }

    [MaxLength(254)]
    [Column("email_publico")]
    public string? EmailPublico { get; set; }

    [MaxLength(30)]
    [Column("telefone_publico")]
    public string? TelefonePublico { get; set; }

    [MaxLength(300)]
    [Column("endereco_publico")]
    public string? EnderecoPublico { get; set; }

    [MaxLength(7)]
    [Column("cor_primaria")]
    public string? CorPrimaria { get; set; }

    [MaxLength(7)]
    [Column("cor_secundaria")]
    public string? CorSecundaria { get; set; }

    [MaxLength(7)]
    [Column("cor_fundo")]
    public string? CorFundo { get; set; }

    [MaxLength(7)]
    [Column("cor_texto")]
    public string? CorTexto { get; set; }

    [Column("publicado")]
    public bool Publicado { get; set; }

    [Column("ordem_exibicao")]
    public int OrdemExibicao { get; set; }

    public ICollection<SitePerfilPublicoProduto> ProdutosPublicados { get; set; } = [];
}
