using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.ERP.Models;

public class MovimentacaoEstoque
{
    [Key]
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int LocalEstoqueId { get; set; }
    public LocalEstoque LocalEstoque { get; set; } = null!;
    public string Tipo { get; set; } = "Entrada"; // Entrada, Saida, Transferencia, Ajuste, Inventario
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantidade { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal CustoUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal CustoTotal { get; set; }
    public string? DocumentoReferencia { get; set; } // NF, Pedido, Compra, Inventario
    public int? DocumentoId { get; set; }
    public string? Observacao { get; set; }
    public int? UsuarioId { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class Inventario
{
    [Key]
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public int LocalEstoqueId { get; set; }
    public LocalEstoque LocalEstoque { get; set; } = null!;
    public string Status { get; set; } = "EmAndamento"; // EmAndamento, Finalizado, Cancelado
    public DateTime DataInicio { get; set; } = DateTime.Now;
    public DateTime? DataFim { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? DiferencaValor { get; set; }
    public string? Observacao { get; set; }
    public int? UsuarioResponsavelId { get; set; }
    public ICollection<InventarioItem> Itens { get; set; } = new List<InventarioItem>();
}

public class InventarioItem
{
    [Key]
    public int Id { get; set; }
    public int InventarioId { get; set; }
    public Inventario Inventario { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    [Column(TypeName = "decimal(18,3)")]
    public decimal QuantidadeSistema { get; set; }
    [Column(TypeName = "decimal(18,3)")]
    public decimal QuantidadeContada { get; set; }
    [Column(TypeName = "decimal(18,3)")]
    public decimal Diferenca { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal CustoUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal DiferencaValor { get; set; }
    public string? Observacao { get; set; }
}

public class LocalEstoque
{
    [Key]
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = "CD"; // CD, Loja, Deposito, Terceiro
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? UF { get; set; }
    public bool Ativo { get; set; } = true;
}

public class Fornecedor
{
    [Key]
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string? NomeFantasia { get; set; }
    public string Cnpj { get; set; } = string.Empty;
    public string? InscricaoEstadual { get; set; }
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Cidade { get; set; }
    public string? UF { get; set; }
    public string? Cep { get; set; }
    public string? ContatoNome { get; set; }
    public string? ContatoTelefone { get; set; }
    public string? Segmento { get; set; }
    public string Status { get; set; } = "Ativo"; // Ativo, Inativo, Bloqueado
    public int? PrazoPagamentoDias { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? LimiteCredito { get; set; }
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class Compra
{
    [Key]
    public int Id { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public int FornecedorId { get; set; }
    public Fornecedor Fornecedor { get; set; } = null!;
    public string Status { get; set; } = "Pendente"; // Pendente, Aprovado, Recebido, Cancelado
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorDesconto { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorFrete { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.Now;
    public DateTime? DataPrevistaRecebimento { get; set; }
    public DateTime? DataRecebimento { get; set; }
    public string? Observacao { get; set; }
    public int? LojaId { get; set; }
    public int? UsuarioId { get; set; }
    public ICollection<CompraItem> Itens { get; set; } = new List<CompraItem>();
}

public class CompraItem
{
    [Key]
    public int Id { get; set; }
    public int CompraId { get; set; }
    public Compra Compra { get; set; } = null!;
    public int? ProdutoId { get; set; }
    public string Descricao { get; set; } = string.Empty;
    [Column(TypeName = "decimal(18,3)")]
    public decimal Quantidade { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? QuantidadeRecebida { get; set; }
}
