using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoCupom
{
    Percentual,
    ValorFixo,
    FreteGratis
}

[Table("cupons")]
public class Cupom
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("codigo")]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Column("tipo")]
    public TipoCupom Tipo { get; set; } = TipoCupom.Percentual;

    [Required]
    [Column("valor", TypeName = "decimal(10,2)")]
    public decimal Valor { get; set; } = 0.00m;

    [Column("valor_minimo_pedido", TypeName = "decimal(10,2)")]
    public decimal ValorMinimoPedido { get; set; } = 0.00m;

    [Column("valor_maximo_desconto", TypeName = "decimal(10,2)")]
    public decimal? ValorMaximoDesconto { get; set; }

    [Column("quantidade_usos")]
    public int? QuantidadeUsos { get; set; }

    [Column("usos_atuais")]
    public int UsosAtuais { get; set; } = 0;

    [Column("quantidade_por_cliente")]
    public int QuantidadePorCliente { get; set; } = 1;

    [Column("valido_de")]
    public DateTime? ValidoDe { get; set; }

    [Column("valido_ate")]
    public DateTime? ValidoAte { get; set; }

    [Column("lojas_aplicaveis")]
    public string? LojasAplicaveis { get; set; } // JSON

    [Column("categorias_aplicaveis")]
    public string? CategoriasAplicaveis { get; set; } // JSON

    [Column("produtos_aplicaveis")]
    public string? ProdutosAplicaveis { get; set; } // JSON

    [Column("clientes_aplicaveis")]
    public string? ClientesAplicaveis { get; set; } // JSON

    [Column("primeiro_compra_only")]
    public bool PrimeiroCompraOnly { get; set; } = false;

    [Column("ativo")]
    public bool Ativo { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
