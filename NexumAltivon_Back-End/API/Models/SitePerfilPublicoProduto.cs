/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

[Table("site_perfis_publicos_produtos")]
public sealed class SitePerfilPublicoProduto : Sys_AuditableEntity
{
    [Column("perfil_publico_id")]
    public Guid PerfilPublicoId { get; set; }

    [Column("produto_id")]
    public int ProdutoId { get; set; }

    [Column("publicado")]
    public bool Publicado { get; set; } = true;

    [Column("ordem_exibicao")]
    public int OrdemExibicao { get; set; }

    public SitePerfilPublico PerfilPublico { get; set; } = null!;

    public Produto Produto { get; set; } = null!;
}
