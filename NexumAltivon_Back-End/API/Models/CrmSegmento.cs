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

[Table("crm_segmentos")]
public sealed class CrmSegmento : Sys_AuditableEntity
{
    [Required]
    [MaxLength(100)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [MaxLength(500)]
    [Column("descricao")]
    public string? Descricao { get; set; }

    [Required]
    [MaxLength(20)]
    [Column("cor")]
    public string Cor { get; set; } = "#C9A227";

    [Column("prioridade")]
    public int Prioridade { get; set; } = 1;

    [Column("ticket_medio_minimo", TypeName = "decimal(15,2)")]
    public decimal? TicketMedioMinimo { get; set; }

    [Column("ticket_medio_maximo", TypeName = "decimal(15,2)")]
    public decimal? TicketMedioMaximo { get; set; }

    [Column("frequencia_minima_dias")]
    public int? FrequenciaMinimaDias { get; set; }

    [Column("frequencia_maxima_dias")]
    public int? FrequenciaMaximaDias { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;

    public ICollection<CrmCampanha> Campanhas { get; set; } = new List<CrmCampanha>();
}
