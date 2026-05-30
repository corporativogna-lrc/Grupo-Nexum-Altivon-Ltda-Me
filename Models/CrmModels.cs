using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.ERP.Models;

public class PipelineEtapa
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public int Ordem { get; set; }
    public string Cor { get; set; } = "#C9A227";
    public int Probabilidade { get; set; } = 0; // 0 a 100
}

public class Oportunidade
{
    [Key]
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public int? LeadId { get; set; }
    public int PipelineEtapaId { get; set; }
    public PipelineEtapa PipelineEtapa { get; set; } = null!;
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorEstimado { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ValorFechado { get; set; }
    public int Probabilidade { get; set; } = 10;
    public DateTime? DataPrevistaFechamento { get; set; }
    public DateTime? DataFechamento { get; set; }
    public string Status { get; set; } = "Aberta"; // Aberta, Ganha, Perdida
    public string? MotivoPerda { get; set; }
    public int? ResponsavelId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class Atendimento
{
    [Key]
    public int Id { get; set; }
    public string Protocolo { get; set; } = string.Empty;
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }
    public string Canal { get; set; } = "WhatsApp"; // WhatsApp, Email, Telefone, Chat, Reclamacao
    public string Assunto { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Status { get; set; } = "Aberto"; // Aberto, EmAtendimento, Aguardando, Resolvido, Fechado
    public string? Resolucao { get; set; }
    public int? ResponsavelId { get; set; }
    public int? OportunidadeId { get; set; }
    public DateTime? DataResolucao { get; set; }
    public int? TempoAtendimentoMinutos { get; set; }
    public int? NotaSatisfacao { get; set; } // 1 a 5
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class CampanhaMarketing
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string Canal { get; set; } = "Email"; // Email, WhatsApp, SMS, RedesSociais, GoogleAds
    public string Segmento { get; set; } = string.Empty; // Todos, ClientesVIP, Leads, Inativos
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Orcamento { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? CustoReal { get; set; }
    public int? Envios { get; set; }
    public int? Aberturas { get; set; }
    public int? Cliques { get; set; }
    public int? Conversoes { get; set; }
    public string Status { get; set; } = "Planejada"; // Planejada, EmExecucao, Pausada, Finalizada
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class LeadScore
{
    [Key]
    public int Id { get; set; }
    public int? ClienteId { get; set; }
    public string? Email { get; set; }
    public int Pontuacao { get; set; } = 0;
    public string? Origem { get; set; }
    public string? Interesses { get; set; }
    public DateTime? UltimaInteracao { get; set; }
    public string Classificacao { get; set; } = "Frio"; // Frio, Morno, Quente
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
