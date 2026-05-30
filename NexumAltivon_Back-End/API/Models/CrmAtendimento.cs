using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoAtendimento
{
    Ligacao,
    Email,
    WhatsApp,
    Chat,
    Reuniao,
    Visita,
    Outro
}

public enum StatusAtendimento
{
    Aberto,
    EmAndamento,
    Aguardando,
    Resolvido,
    Cancelado
}

[Table("crm_atendimentos")]
public class CrmAtendimento
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("lead_id")]
    public int? LeadId { get; set; }

    [Column("cliente_id")]
    public int? ClienteId { get; set; }

    [Column("tipo")]
    public TipoAtendimento Tipo { get; set; } = TipoAtendimento.WhatsApp;

    [Column("assunto")]
    [MaxLength(200)]
    public string? Assunto { get; set; }

    [Column("descricao")]
    public string? Descricao { get; set; }

    [Column("status")]
    public StatusAtendimento Status { get; set; } = StatusAtendimento.Aberto;

    [Column("responsavel_id")]
    public int? ResponsavelId { get; set; }

    [Column("data_agendamento")]
    public DateTime? DataAgendamento { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("LeadId")]
    public CrmLead? Lead { get; set; }

    [ForeignKey("ClienteId")]
    public Cliente? Cliente { get; set; }

    [ForeignKey("ResponsavelId")]
    public Usuario? Responsavel { get; set; }
}
