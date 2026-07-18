/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7182
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

[Table("logistica_rastreamento_consultas")]
public sealed class LogisticaRastreamentoConsulta : Sys_AuditableEntity
{
    [Column("pedido_id")]
    public int PedidoId { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("codigo_rastreio")]
    public string CodigoRastreio { get; set; } = string.Empty;

    [Required]
    [MaxLength(120)]
    [Column("provedor")]
    public string Provedor { get; set; } = string.Empty;

    [Column("configurada")]
    public bool Configurada { get; set; }

    [Column("operacional")]
    public bool Operacional { get; set; }

    [Column("http_status_code")]
    public int? HttpStatusCode { get; set; }

    [MaxLength(160)]
    [Column("status_externo")]
    public string? StatusExterno { get; set; }

    [Column("quantidade_eventos")]
    public int QuantidadeEventos { get; set; }

    [Column("eventos_json", TypeName = "mediumtext")]
    public string EventosJson { get; set; } = "[]";

    [Column("pendencias_json", TypeName = "text")]
    public string PendenciasJson { get; set; } = "[]";

    [MaxLength(64)]
    [Column("resposta_sha256")]
    public string? RespostaSha256 { get; set; }

    [Column("consultado_at")]
    public DateTime ConsultadoAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(PedidoId))]
    public Pedido? Pedido { get; set; }
}
