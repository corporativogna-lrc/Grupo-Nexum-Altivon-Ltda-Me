namespace NexumAltivon.API.DTOs;

public class LojaDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Segmento { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Logo { get; set; }
    public string? Banner { get; set; }
    public string CorPrimaria { get; set; } = "#C9A227";
    public string CorSecundaria { get; set; } = "#1E3A5F";
    public string? Dominio { get; set; }
    public bool Ativa { get; set; }
    public int OrdemExibicao { get; set; }
}

public class CriarLojaDto
{
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Segmento { get; set; } = string.Empty;
    public string? Descricao { get; set; }
    public string? Logo { get; set; }
    public string? Banner { get; set; }
    public string CorPrimaria { get; set; } = "#C9A227";
    public string CorSecundaria { get; set; } = "#1E3A5F";
    public string? Dominio { get; set; }
    public int OrdemExibicao { get; set; } = 0;
}
