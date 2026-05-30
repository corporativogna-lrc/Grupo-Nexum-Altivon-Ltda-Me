using System;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.ERP.DTOs
{
    // ==================== FINANCEIRO DTOs ====================

    public class ContaPagarDto
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public int FornecedorId { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorOriginal { get; set; }
        public decimal ValorPago { get; set; }
        public decimal SaldoDevedor => ValorOriginal - ValorPago + ValorMulta + ValorJuros - ValorDesconto;
        public decimal ValorMulta { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorDesconto { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataPagamento { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DiasAtraso => DataVencimento < DateTime.Now && Status != "Pago" ? (DateTime.Now - DataVencimento).Days : 0;
        public string? FormaPagamento { get; set; }
        public string? NumeroBoleto { get; set; }
        public int? LojaId { get; set; }
        public string? LojaNome { get; set; }
        public int CentroCustoId { get; set; }
        public string? CentroCustoNome { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class CriarContaPagarDto
    {
        [Required, StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;
        [Required]
        public int FornecedorId { get; set; }
        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;
        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorOriginal { get; set; }
        [Required]
        public DateTime DataVencimento { get; set; }
        public int? LojaId { get; set; }
        [Required]
        public int CentroCustoId { get; set; }
        public string? Observacoes { get; set; }
    }

    public class BaixarContaPagarDto
    {
        [Required]
        public int ContaPagarId { get; set; }
        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorPago { get; set; }
        public decimal ValorMulta { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorDesconto { get; set; }
        [Required]
        public DateTime DataPagamento { get; set; }
        [Required, StringLength(50)]
        public string FormaPagamento { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
    }

    public class ContaReceberDto
    {
        public int Id { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal ValorOriginal { get; set; }
        public decimal ValorRecebido { get; set; }
        public decimal SaldoDevedor => ValorOriginal - ValorRecebido + ValorMulta + ValorJuros - ValorDesconto;
        public decimal ValorMulta { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorDesconto { get; set; }
        public DateTime DataEmissao { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataRecebimento { get; set; }
        public string Status { get; set; } = string.Empty;
        public int DiasAtraso => DataVencimento < DateTime.Now && Status != "Recebido" ? (DateTime.Now - DataVencimento).Days : 0;
        public string? FormaRecebimento { get; set; }
        public string? NumeroPedidoReferencia { get; set; }
        public int? LojaId { get; set; }
        public int CentroCustoId { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class CriarContaReceberDto
    {
        [Required, StringLength(20)]
        public string NumeroDocumento { get; set; } = string.Empty;
        [Required]
        public int ClienteId { get; set; }
        [Required, StringLength(200)]
        public string Descricao { get; set; } = string.Empty;
        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorOriginal { get; set; }
        [Required]
        public DateTime DataVencimento { get; set; }
        public int? LojaId { get; set; }
        [Required]
        public int CentroCustoId { get; set; }
        public string? NumeroPedidoReferencia { get; set; }
        public string? Observacoes { get; set; }
    }

    public class BaixarContaReceberDto
    {
        [Required]
        public int ContaReceberId { get; set; }
        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorRecebido { get; set; }
        public decimal ValorMulta { get; set; }
        public decimal ValorJuros { get; set; }
        public decimal ValorDesconto { get; set; }
        [Required]
        public DateTime DataRecebimento { get; set; }
        [Required, StringLength(50)]
        public string FormaRecebimento { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
    }

    public class FluxoCaixaDto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string? Categoria { get; set; }
        public string? FormaPagamento { get; set; }
        public string? ContaBancaria { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class ResumoFinanceiroDto
    {
        public decimal TotalContasPagarPendentes { get; set; }
        public decimal TotalContasPagarVencidas { get; set; }
        public decimal TotalContasReceberPendentes { get; set; }
        public decimal TotalContasReceberVencidas { get; set; }
        public decimal SaldoProjetado30Dias { get; set; }
        public decimal SaldoProjetado60Dias { get; set; }
        public decimal SaldoProjetado90Dias { get; set; }
        public int QtdContasPagarVencidas { get; set; }
        public int QtdContasReceberVencidas { get; set; }
        public List<FluxoCaixaDto> UltimasMovimentacoes { get; set; } = new();
    }

    // ==================== FISCAL DTOs ====================

    public class NotaFiscalDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string NaturezaOperacao { get; set; } = string.Empty;
        public decimal ValorTotal { get; set; }
        public decimal ValorIcms { get; set; }
        public decimal ValorIpi { get; set; }
        public decimal ValorPis { get; set; }
        public decimal ValorCofins { get; set; }
        public DateTime DataEmissao { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ChaveAcesso { get; set; }
        public int? PedidoId { get; set; }
        public int? LojaId { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class CriarNotaFiscalDto
    {
        [Required, StringLength(20)]
        public string Numero { get; set; } = string.Empty;
        [StringLength(10)]
        public string Serie { get; set; } = "1";
        [Required, StringLength(10)]
        public string Tipo { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string NaturezaOperacao { get; set; } = string.Empty;
        [Required]
        public int EmitenteId { get; set; }
        [Required]
        public int DestinatarioId { get; set; }
        public int? PedidoId { get; set; }
        public int? LojaId { get; set; }
        public List<CriarItemNotaFiscalDto> Itens { get; set; } = new();
    }

    public class CriarItemNotaFiscalDto
    {
        [Required]
        public int ProdutoId { get; set; }
        [Required, StringLength(120)]
        public string Descricao { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Cfop { get; set; } = string.Empty;
        [Required, StringLength(10)]
        public string Ncm { get; set; } = string.Empty;
        [Required, Range(0.001, double.MaxValue)]
        public decimal Quantidade { get; set; }
        [Required, Range(0.01, double.MaxValue)]
        public decimal ValorUnitario { get; set; }
        public decimal AliquotaIcms { get; set; }
    }

    // ==================== ESTOQUE DTOs ====================

    public class MovimentacaoEstoqueDto
    {
        public int Id { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal? CustoUnitario { get; set; }
        public string Motivo { get; set; } = string.Empty;
        public string DocumentoReferencia { get; set; } = string.Empty;
        public DateTime DataMovimentacao { get; set; }
        public string? CriadoPor { get; set; }
    }

    public class CriarMovimentacaoEstoqueDto
    {
        [Required]
        public int ProdutoId { get; set; }
        [Required, StringLength(20)]
        public string Tipo { get; set; } = string.Empty;
        [Required, Range(0.001, double.MaxValue)]
        public decimal Quantidade { get; set; }
        public decimal? CustoUnitario { get; set; }
        [Required, StringLength(50)]
        public string Motivo { get; set; } = string.Empty;
        public int? OrigemLojaId { get; set; }
        public int? DestinoLojaId { get; set; }
        public int? PedidoId { get; set; }
        public int? FornecedorId { get; set; }
        [Required, StringLength(100)]
        public string DocumentoReferencia { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
    }

    public class InventarioDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int TotalItens { get; set; }
        public int TotalDivergencias { get; set; }
        public decimal? ValorTotalDivergencia { get; set; }
    }

    public class KardexDto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal Saldo { get; set; }
        public decimal? CustoUnitario { get; set; }
        public decimal? CustoMedio { get; set; }
        public string Documento { get; set; } = string.Empty;
    }

    // ==================== CRM DTOs ====================

    public class LeadCRMDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? WhatsApp { get; set; }
        public string Origem { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public string? Empresa { get; set; }
        public string? ResponsavelNome { get; set; }
        public decimal? ValorEstimado { get; set; }
        public int? Probabilidade { get; set; }
        public DateTime? DataPrevisaoFechamento { get; set; }
        public DateTime? DataUltimoContato { get; set; }
        public int DiasSemContato => DataUltimoContato.HasValue ? (DateTime.Now - DataUltimoContato.Value).Days : (DateTime.Now - CriadoEm).Days;
        public DateTime CriadoEm { get; set; }
    }

    public class CriarLeadCRMDto
    {
        [Required, StringLength(200)]
        public string Nome { get; set; } = string.Empty;
        [StringLength(200)]
        public string? Email { get; set; }
        [StringLength(20)]
        public string? Telefone { get; set; }
        [StringLength(20)]
        public string? WhatsApp { get; set; }
        [Required, StringLength(50)]
        public string Origem { get; set; } = string.Empty;
        [StringLength(50)]
        public string? Tipo { get; set; }
        [StringLength(200)]
        public string? Empresa { get; set; }
        [StringLength(100)]
        public string? Cargo { get; set; }
        [StringLength(20)]
        public string? Cnpj { get; set; }
        [StringLength(20)]
        public string? Cpf { get; set; }
        public decimal? ValorEstimado { get; set; }
        public int? Probabilidade { get; set; }
        public DateTime? DataPrevisaoFechamento { get; set; }
        public string? Observacoes { get; set; }
    }

    public class AtualizarStatusLeadDto
    {
        [Required]
        public int LeadId { get; set; }
        [Required, StringLength(50)]
        public string NovoStatus { get; set; } = string.Empty;
        [StringLength(500)]
        public string? Observacoes { get; set; }
    }

    public class InteracaoCRMDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public DateTime DataInteracao { get; set; }
        public string? Responsavel { get; set; }
    }

    public class CriarInteracaoCRMDto
    {
        [Required]
        public int LeadId { get; set; }
        [Required, StringLength(50)]
        public string Tipo { get; set; } = string.Empty;
        [Required, StringLength(1000)]
        public string Descricao { get; set; } = string.Empty;
        [StringLength(100)]
        public string? Responsavel { get; set; }
        [StringLength(500)]
        public string? Anotacoes { get; set; }
    }

    public class TarefaCRMDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataVencimento { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? Responsavel { get; set; }
        public bool Atrasada => Status != "Concluida" && DataVencimento < DateTime.Now;
        public int DiasRestantes => (DataVencimento - DateTime.Now).Days;
    }

    public class CriarTarefaCRMDto
    {
        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;
        [StringLength(1000)]
        public string? Descricao { get; set; }
        [Required, StringLength(50)]
        public string Tipo { get; set; } = string.Empty;
        [Required, StringLength(20)]
        public string Prioridade { get; set; } = "Media";
        public int? LeadId { get; set; }
        public int? ClienteId { get; set; }
        [Required]
        public DateTime DataVencimento { get; set; }
        [StringLength(100)]
        public string? Responsavel { get; set; }
    }

    public class PipelineCRMDto
    {
        public string Status { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal ValorMedio { get; set; }
        public List<LeadCRMDto> Leads { get; set; } = new();
    }

    // ==================== FORNECEDOR DTOs ====================

    public class FornecedorDto
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
        public string? Segmento { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? LimiteCredito { get; set; }
        public int? PrazoPagamentoDias { get; set; }
        public bool Dropshipping { get; set; }
        public decimal? ComissaoDropshipping { get; set; }
        public decimal? NotaMedia { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class CriarFornecedorDto
    {
        [Required, StringLength(200)]
        public string RazaoSocial { get; set; } = string.Empty;
        [StringLength(200)]
        public string? NomeFantasia { get; set; }
        [Required, StringLength(20)]
        public string Cnpj { get; set; } = string.Empty;
        [StringLength(20)]
        public string? InscricaoEstadual { get; set; }
        [StringLength(200)]
        public string? Email { get; set; }
        [StringLength(20)]
        public string? Telefone { get; set; }
        [StringLength(20)]
        public string? Celular { get; set; }
        [StringLength(200)]
        public string? Endereco { get; set; }
        [StringLength(100)]
        public string? Cidade { get; set; }
        [StringLength(2)]
        public string? Uf { get; set; }
        [StringLength(10)]
        public string? Cep { get; set; }
        [StringLength(50)]
        public string? Segmento { get; set; }
        public decimal? LimiteCredito { get; set; }
        public int? PrazoPagamentoDias { get; set; }
        public bool Dropshipping { get; set; }
        public decimal? ComissaoDropshipping { get; set; }
        public string? Observacoes { get; set; }
    }
}
