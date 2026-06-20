using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.ERP.SharedData;

[Table("erp_contas_pagar")]
public sealed class GenesisContaPagar
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("numero_documento")]
    [MaxLength(20)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Column("fornecedor_id")]
    public int? FornecedorId { get; set; }

    [Column("descricao")]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Column("valor_original")]
    public decimal ValorOriginal { get; set; }

    [Column("valor_pago")]
    public decimal ValorPago { get; set; }

    [Column("valor_multa")]
    public decimal ValorMulta { get; set; }

    [Column("valor_juros")]
    public decimal ValorJuros { get; set; }

    [Column("valor_desconto")]
    public decimal ValorDesconto { get; set; }

    [Column("data_emissao")]
    public DateTime DataEmissao { get; set; }

    [Column("status")]
    [MaxLength(50)]
    public string? Status { get; set; }

    [Column("data_vencimento")]
    public DateTime DataVencimento { get; set; }

    [Column("data_pagamento")]
    public DateTime? DataPagamento { get; set; }

    [Column("forma_pagamento")]
    [MaxLength(50)]
    public string? FormaPagamento { get; set; }

    [Column("numero_boleto")]
    [MaxLength(100)]
    public string? NumeroBoleto { get; set; }
}

[Table("erp_contas_receber")]
public sealed class GenesisContaReceber
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("numero_documento")]
    [MaxLength(20)]
    public string NumeroDocumento { get; set; } = string.Empty;

    [Column("cliente_id")]
    public int? ClienteId { get; set; }

    [Column("descricao")]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Column("valor_original")]
    public decimal ValorOriginal { get; set; }

    [Column("valor_recebido")]
    public decimal ValorRecebido { get; set; }

    [Column("valor_multa")]
    public decimal ValorMulta { get; set; }

    [Column("valor_juros")]
    public decimal ValorJuros { get; set; }

    [Column("valor_desconto")]
    public decimal ValorDesconto { get; set; }

    [Column("data_emissao")]
    public DateTime DataEmissao { get; set; }

    [Column("status")]
    [MaxLength(50)]
    public string? Status { get; set; }

    [Column("data_vencimento")]
    public DateTime DataVencimento { get; set; }

    [Column("data_recebimento")]
    public DateTime? DataRecebimento { get; set; }

    [Column("forma_recebimento")]
    [MaxLength(50)]
    public string? FormaRecebimento { get; set; }

    [Column("numero_pedido_referencia")]
    [MaxLength(100)]
    public string? NumeroPedidoReferencia { get; set; }
}

[Table("erp_fluxo_caixa")]
public sealed class GenesisFluxoCaixa
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("data")]
    public DateTime Data { get; set; }

    [Column("tipo")]
    [MaxLength(50)]
    public string? Tipo { get; set; }

    [Column("descricao")]
    [MaxLength(100)]
    public string? Descricao { get; set; }

    [Column("valor")]
    public decimal Valor { get; set; }

    [Column("categoria")]
    [MaxLength(50)]
    public string? Categoria { get; set; }

    [Column("conta_pagar_id")]
    public int? ContaPagarId { get; set; }

    [Column("conta_receber_id")]
    public int? ContaReceberId { get; set; }

    [Column("forma_pagamento")]
    [MaxLength(50)]
    public string? FormaPagamento { get; set; }

    [Column("conta_bancaria")]
    [MaxLength(100)]
    public string? ContaBancaria { get; set; }

    [Column("observacoes")]
    public string? Observacoes { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }
}

[Table("erp_boletos")]
public sealed class GenesisBoleto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("conta_receber_id")]
    public int ContaReceberId { get; set; }

    [Column("nosso_numero")]
    [MaxLength(100)]
    public string? NossoNumero { get; set; }

    [Column("linha_digitavel")]
    [MaxLength(255)]
    public string? LinhaDigitavel { get; set; }

    [Column("codigo_barras")]
    [MaxLength(255)]
    public string? CodigoBarras { get; set; }

    [Column("banco")]
    [MaxLength(100)]
    public string? Banco { get; set; }

    [Column("vencimento")]
    public DateTime Vencimento { get; set; }

    [Column("valor")]
    public decimal Valor { get; set; }

    [Column("status")]
    [MaxLength(30)]
    public string? Status { get; set; }

    [Column("url_boleto")]
    [MaxLength(500)]
    public string? UrlBoleto { get; set; }

    [Column("pdf_url")]
    [MaxLength(500)]
    public string? PdfUrl { get; set; }

    [Column("criado_em")]
    public DateTime CriadoEm { get; set; }
}

[Table("erp_financeiro_referencias")]
public sealed class GenesisFinanceReferencia
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("tipo")]
    [MaxLength(40)]
    public string Tipo { get; set; } = string.Empty;

    [Column("codigo")]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Column("descricao")]
    [MaxLength(150)]
    public string Descricao { get; set; } = string.Empty;

    [Column("ordem")]
    public int Ordem { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; }
}

[Table("erp_rh_colaboradores")]
public sealed class GenesisRhColaborador
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nome")]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Column("cargo")]
    [MaxLength(120)]
    public string? Cargo { get; set; }

    [Column("departamento")]
    [MaxLength(120)]
    public string? Departamento { get; set; }

    [Column("status")]
    [MaxLength(50)]
    public string? Status { get; set; }

    [Column("data_admissao")]
    public DateTime? DataAdmissao { get; set; }
}

[Table("erp_rh_referencias")]
public sealed class GenesisRhReferencia
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("tipo")]
    [MaxLength(40)]
    public string Tipo { get; set; } = string.Empty;

    [Column("codigo")]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Column("descricao")]
    [MaxLength(120)]
    public string Descricao { get; set; } = string.Empty;

    [Column("ordem")]
    public int Ordem { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; }
}