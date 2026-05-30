using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum OrigemLead
{
    Site,
    WhatsApp,
    Email,
    Telefone,
    Marketplace,
    Indicacao,
    Campanha,
    Outro
}

public enum TipoLead
{
    ClienteVIP,
    Dropshipping,
    Fornecedor,
    Parceiro,
    Afiliado,
    Outro
}

public enum StatusLead
{
    Novo,
    EmAtendimento,
    Qualificado,
    Convertido,
    Perdido,
    Arquivado
}

public enum PrioridadeLead
{
    Baixa,
    Media,
    Alta,
    Urgente
}

[Table("crm_leads")]
public class CrmLead
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("origem")]
    public OrigemLead Origem { get; set; } = OrigemLead.Site;

    [Column("tipo")]
    public TipoLead Tipo { get; set; } = TipoLead.ClienteVIP;

    [Required]
    [Column("nome")]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Column("email")]
    [MaxLength(150)]
    public string? Email { get; set; }

    [Column("telefone")]
    [MaxLength(20)]
    public string? Telefone { get; set; }

    [Column("whatsapp")]
    [MaxLength(20)]
    public string? Whatsapp { get; set; }

    [Column("empresa")]
    [MaxLength(200)]
    public string? Empresa { get; set; }

    [Column("cnpj")]
    [MaxLength(18)]
    public string? Cnpj { get; set; }

    [Column("segmento")]
    [MaxLength(100)]
    public string? Segmento { get; set; }

    [Column("proposta")]
    public string? Proposta { get; set; }

    [Column("experiencia")]
    [MaxLength(50)]
    public string? Experiencia { get; set; }

    [Column("status")]
    public StatusLead Status { get; set; } = StatusLead.Novo;

    [Column("responsavel_id")]
    public int? ResponsavelId { get; set; }

    [Column("prioridade")]
    public PrioridadeLead Prioridade { get; set; } = PrioridadeLead.Media;

    [Column("anotacoes")]
    public string? Anotacoes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    [ForeignKey("ResponsavelId")]
    public Usuario? Responsavel { get; set; }

    public ICollection<CrmAtendimento>? Atendimentos { get; set; }
}
