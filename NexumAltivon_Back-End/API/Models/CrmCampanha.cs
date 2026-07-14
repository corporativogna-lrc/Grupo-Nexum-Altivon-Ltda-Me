/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoCampanha
{
    Email,
    WhatsApp,
    Sms,
    RedeSocial,
    GoogleAds,
    Display
}

public enum StatusCampanha
{
    Rascunho,
    Agendada,
    EmAndamento,
    Pausada,
    Concluida,
    Cancelada
}

[Table("crm_campanhas")]
public sealed class CrmCampanha : Sys_AuditableEntity
{
    [Required]
    [MaxLength(200)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(1000)]
    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("tipo")]
    public TipoCampanha Tipo { get; set; }

    [Column("status")]
    public StatusCampanha Status { get; set; } = StatusCampanha.Rascunho;

    [Column("segmento_id")]
    public Guid? SegmentoId { get; set; }

    [Column("data_inicio")]
    public DateTime DataInicio { get; set; }

    [Column("data_fim")]
    public DateTime? DataFim { get; set; }

    [Column("orcamento", TypeName = "decimal(15,2)")]
    public decimal Orcamento { get; set; }

    [Column("custo_atual", TypeName = "decimal(15,2)")]
    public decimal CustoAtual { get; set; }

    [Column("alcance")]
    public int Alcance { get; set; }

    [Column("cliques")]
    public int Cliques { get; set; }

    [Column("leads_gerados")]
    public int LeadsGerados { get; set; }

    [Column("oportunidades_geradas")]
    public int OportunidadesGeradas { get; set; }

    [Column("vendas_geradas")]
    public int VendasGeradas { get; set; }

    [Column("receita_gerada", TypeName = "decimal(15,2)")]
    public decimal ReceitaGerada { get; set; }

    [MaxLength(1000)]
    [Column("publico_alvo")]
    public string? PublicoAlvo { get; set; }

    [Column("conteudo", TypeName = "longtext")]
    public string? Conteudo { get; set; }

    public CrmSegmento? Segmento { get; set; }
}
