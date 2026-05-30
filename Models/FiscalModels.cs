using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.ERP.Models;

public class NotaFiscal
{
    [Key]
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Serie { get; set; } = "1";
    public string ChaveAcesso { get; set; } = string.Empty;
    public string Tipo { get; set; } = "Saida"; // Entrada, Saida
    public string Status { get; set; } = "Emitida"; // Emitida, Cancelada, Denegada, Inutilizada
    public int? PedidoId { get; set; }
    public int? ClienteId { get; set; }
    public int? FornecedorId { get; set; }
    public DateTime DataEmissao { get; set; } = DateTime.Now;
    public DateTime? DataSaidaEntrada { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorIcms { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorIpi { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorPis { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorCofins { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorFrete { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorSeguro { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorDesconto { get; set; }
    public string? XmlAutorizacao { get; set; }
    public string? ProtocoloAutorizacao { get; set; }
    public string? MotivoCancelamento { get; set; }
    public int? LojaId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public ICollection<NotaFiscalItem> Itens { get; set; } = new List<NotaFiscalItem>();
}

public class NotaFiscalItem
{
    [Key]
    public int Id { get; set; }
    public int NotaFiscalId { get; set; }
    public NotaFiscal NotaFiscal { get; set; } = null!;
    public int? ProdutoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    public string CodigoProduto { get; set; } = string.Empty;
    public string Ncm { get; set; } = string.Empty;
    public string Cfop { get; set; } = string.Empty;
    public string Unidade { get; set; } = "UN";
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantidade { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal AliquotaIcms { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorIcms { get; set; }
}

public class ConfiguracaoFiscal
{
    [Key]
    public int Id { get; set; }
    public int LojaId { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string InscricaoEstadual { get; set; } = string.Empty;
    public string RazaoSocial { get; set; } = string.Empty;
    public string NomeFantasia { get; set; } = string.Empty;
    public string? CertificadoDigital { get; set; }
    public string? SenhaCertificado { get; set; }
    public string RegimeTributario { get; set; } = "Simples Nacional";
    public string Ambiente { get; set; } = "Homologacao"; // Homologacao, Producao
    public string? CaminhoSchemasNFe { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class ManifestoDestinatario
{
    [Key]
    public int Id { get; set; }
    public string ChaveNFe { get; set; } = string.Empty;
    public string CnpjEmitente { get; set; } = string.Empty;
    public string NomeEmitente { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }
    public DateTime DataEmissao { get; set; }
    public string SituacaoManifesto { get; set; } = "Pendente"; // Pendente, Ciencia, Confirmacao, Desconhecimento, NaoRealizada
    public DateTime? DataManifesto { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
