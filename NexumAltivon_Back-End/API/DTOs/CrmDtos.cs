namespace NexumAltivon.API.DTOs;

public class CrmLeadDto
{
    public int Id { get; set; }
    public string Origem { get; set; } = "Site";
    public string Tipo { get; set; } = "ClienteVIP";
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public string? Empresa { get; set; }
    public string? Cnpj { get; set; }
    public string? Segmento { get; set; }
    public string? Proposta { get; set; }
    public string? Experiencia { get; set; }
    public string Status { get; set; } = "Novo";
    public string Prioridade { get; set; } = "Media";
    public DateTime CreatedAt { get; set; }
}

public class CriarLeadDto
{
    public string Origem { get; set; } = "Site";
    public string Tipo { get; set; } = "ClienteVIP";
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public string? Empresa { get; set; }
    public string? Cnpj { get; set; }
    public string? Segmento { get; set; }
    public string? Proposta { get; set; }
    public string? Experiencia { get; set; }
    public string Prioridade { get; set; } = "Media";
}

public class AtualizarLeadDto
{
    public string Status { get; set; } = string.Empty;
    public string? Prioridade { get; set; }
    public int? ResponsavelId { get; set; }
    public string? Anotacoes { get; set; }
}
