using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.ERP.Models;

public class SyncQueue
{
    [Key]
    public int Id { get; set; }
    public string Entidade { get; set; } = string.Empty; // Produto, Cliente, Pedido, Estoque
    public string Acao { get; set; } = string.Empty; // Create, Update, Delete
    public string PayloadJson { get; set; } = string.Empty;
    public string Direcao { get; set; } = "EcommerceParaErp"; // EcommerceParaErp, ErpParaEcommerce
    public string Status { get; set; } = "Pendente"; // Pendente, Processando, Concluido, Erro
    public int Tentativas { get; set; } = 0;
    public string? Erro { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public DateTime? ProcessadoEm { get; set; }
}

public class SyncLog
{
    [Key]
    public int Id { get; set; }
    public string Entidade { get; set; } = string.Empty;
    public string Acao { get; set; } = string.Empty;
    public string Direcao { get; set; } = string.Empty;
    public bool Sucesso { get; set; }
    public string? Erro { get; set; }
    public string? PayloadJson { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class RelatorioGerado
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = "PDF"; // PDF, Excel, CSV
    public string Modulo { get; set; } = string.Empty; // Financeiro, Fiscal, Estoque, CRM
    public string? ParametrosJson { get; set; }
    public string? CaminhoArquivo { get; set; }
    public int? TamanhoBytes { get; set; }
    public int? UsuarioId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class AuditoriaErp
{
    [Key]
    public int Id { get; set; }
    public string Tabela { get; set; } = string.Empty;
    public int? RegistroId { get; set; }
    public string Acao { get; set; } = string.Empty; // Insert, Update, Delete
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNovos { get; set; }
    public int? UsuarioId { get; set; }
    public string? IpOrigem { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class ErpLog
{
    [Key]
    public int Id { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public string? Properties { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
