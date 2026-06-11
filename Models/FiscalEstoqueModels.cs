using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.ERP.Models
{
    // ==================== FISCAL ====================

    [Table("erp_notas_fiscais")]
    public class NotaFiscal
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Serie { get; set; } = "1";

        [Required, StringLength(10)]
        public string Tipo { get; set; } = string.Empty; // Entrada, Saida

        [Required, StringLength(20)]
        public string NaturezaOperacao { get; set; } = string.Empty;

        [Required]
        public int EmitenteId { get; set; }

        [Required]
        public int DestinatarioId { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
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

        [Required]
        public DateTime DataEmissao { get; set; }

        public DateTime? DataSaidaEntrada { get; set; }

        [Required, StringLength(50)]
        public string Status { get; set; } = "Emitida"; // Emitida, Cancelada, Denegada, Inutilizada

        [StringLength(44)]
        public string? ChaveAcesso { get; set; }

        [StringLength(500)]
        public string? XmlAutorizacao { get; set; }

        [StringLength(500)]
        public string? ProtocoloAutorizacao { get; set; }

        [StringLength(1000)]
        public string? Observacoes { get; set; }

        public int? PedidoId { get; set; }
        public int? LojaId { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
        [StringLength(100)]
        public string? CriadoPor { get; set; }

        public ICollection<ItemNotaFiscal> Itens { get; set; } = new List<ItemNotaFiscal>();
    }

    [Table("erp_itens_nota_fiscal")]
    public class ItemNotaFiscal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int NotaFiscalId { get; set; }

        [ForeignKey("NotaFiscalId")]
        public virtual NotaFiscal? NotaFiscal { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [Required, StringLength(120)]
        public string Descricao { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string Cfop { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Ncm { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string CstIcms { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string CstPis { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string CstCofins { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(18,3)")]
        public decimal Quantidade { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal ValorUnitario { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal ValorTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ValorIcms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AliquotaIcms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal BaseCalculoIcms { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }

    [Table("erp_impostos_config")]
    public class ConfiguracaoImposto
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Descricao { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Ncm { get; set; } = string.Empty;

        [Required, StringLength(10)]
        public string Cfop { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal AliquotaIcms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AliquotaIpi { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AliquotaPis { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal AliquotaCofins { get; set; }

        [StringLength(10)]
        public string? CstIcms { get; set; }

        [StringLength(10)]
        public string? CstPis { get; set; }

        [StringLength(10)]
        public string? CstCofins { get; set; }

        [StringLength(2)]
        public string? UfOrigem { get; set; }

        [StringLength(2)]
        public string? UfDestino { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }

    // ==================== ESTOQUE AVANÇADO ====================

    [Table("erp_movimentacoes_estoque")]
    public class MovimentacaoEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [ForeignKey("ProdutoId")]
        public virtual Produto? Produto { get; set; }

        [Required, StringLength(20)]
        public string Tipo { get; set; } = string.Empty; // Entrada, Saida, Transferencia, Ajuste, Inventario

        [Required, Column(TypeName = "decimal(18,3)")]
        public decimal Quantidade { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustoTotal { get; set; }

        [Required, StringLength(50)]
        public string Motivo { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public int? OrigemLojaId { get; set; }
        public int? DestinoLojaId { get; set; }

        public int? PedidoId { get; set; }
        public int? NotaFiscalId { get; set; }
        public int? FornecedorId { get; set; }

        [Required, StringLength(100)]
        public string DocumentoReferencia { get; set; } = string.Empty;

        public DateTime DataMovimentacao { get; set; } = DateTime.Now;
        public DateTime CriadoEm { get; set; } = DateTime.Now;
        [StringLength(100)]
        public string? CriadoPor { get; set; }

    }

    [Table("erp_inventarios")]
    public class Inventario
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;

        public int? LojaId { get; set; }

        [Required, StringLength(20)]
        public string Status { get; set; } = "Aberto"; // Aberto, EmAndamento, Finalizado, Cancelado

        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        [StringLength(100)]
        public string? CriadoPor { get; set; }

        public ICollection<ItemInventario> Itens { get; set; } = new List<ItemInventario>();
    }

    [Table("erp_itens_inventario")]
    public class ItemInventario
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int InventarioId { get; set; }

        [ForeignKey("InventarioId")]
        public virtual Inventario? Inventario { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [Required, Column(TypeName = "decimal(18,3)")]
        public decimal QuantidadeSistema { get; set; }

        [Required, Column(TypeName = "decimal(18,3)")]
        public decimal QuantidadeContada { get; set; }

        [Column(TypeName = "decimal(18,3)")]
        public decimal Diferenca => QuantidadeContada - QuantidadeSistema;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorDiferenca { get; set; }

        [StringLength(200)]
        public string? Observacoes { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }

    [Table("erp_kardex")]
    public class Kardex
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProdutoId { get; set; }

        [Required]
        public DateTime Data { get; set; }

        [Required, StringLength(20)]
        public string Tipo { get; set; } = string.Empty; // Entrada, Saida

        [Required, Column(TypeName = "decimal(18,3)")]
        public decimal Quantidade { get; set; }

        [Required, Column(TypeName = "decimal(18,3)")]
        public decimal Saldo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustoUnitario { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? CustoMedio { get; set; }

        [Required, StringLength(100)]
        public string Documento { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }

    [Table("erp_locais_estoque")]
    public class LocalEstoque
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Codigo { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Descricao { get; set; }

        public int? LojaId { get; set; }

        [StringLength(100)]
        public string? Setor { get; set; }

        [StringLength(100)]
        public string? Prateleira { get; set; }

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}
