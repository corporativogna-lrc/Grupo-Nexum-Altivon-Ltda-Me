using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.ERP.Models;

// Entidades compartilhadas com o E-Commerce (mapeamento readonly / sync)

public class Loja
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Segmento { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public bool Ativo { get; set; } = true;
}

public class Produto
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Sku { get; set; }
    public string? Descricao { get; set; }
    public string? Ncm { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal Preco { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecoPromocional { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecoCusto { get; set; }
    public decimal EstoqueAtual { get; set; }
    public decimal? EstoqueMinimo { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal CustoMedio { get; set; }
    public DateTime? UltimaAtualizacao { get; set; }
    public int LojaId { get; set; }
    public Loja Loja { get; set; } = null!;
    public bool Ativo { get; set; } = true;
}

public class Cliente
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string? CpfCnpj { get; set; }
    [NotMapped]
    public string? Cpf
    {
        get => CpfCnpj;
        set => CpfCnpj = value;
    }
    public string? Endereco { get; set; }
    public string? Celular { get; set; }
    public string? Cidade { get; set; }
    public string? UF { get; set; }
    [NotMapped]
    public string? Uf
    {
        get => UF;
        set => UF = value;
    }
    public string? Cep { get; set; }
    public bool Vip { get; set; } = false;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}

public class Pedido
{
    [Key]
    public int Id { get; set; }
    public string NumeroPedido { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;
    public int LojaId { get; set; }
    public Loja? Loja { get; set; }
    public string Status { get; set; } = "Pendente";
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ValorFrete { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ValorDesconto { get; set; }
    public DateTime DataPedido { get; set; } = DateTime.Now;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
    public ICollection<PedidoItem> Itens { get; set; } = new List<PedidoItem>();

    [NotMapped]
    public string Numero
    {
        get => NumeroPedido;
        set => NumeroPedido = value;
    }
}

public class PedidoItem
{
    [Key]
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;
    public int Quantidade { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorUnitario { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal ValorTotal { get; set; }
}

public class Usuario
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Perfil { get; set; } = "Vendedor"; // SuperAdmin, Admin, Gerente, Vendedor, Financeiro, Fiscal, Operacional, Suporte
    public bool Ativo { get; set; } = true;
    public DateTime CriadoEm { get; set; } = DateTime.Now;
}
