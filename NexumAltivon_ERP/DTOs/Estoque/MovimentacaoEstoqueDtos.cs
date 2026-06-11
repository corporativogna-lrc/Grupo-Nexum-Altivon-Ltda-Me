using System;

namespace NexumAltivon_ERP.DTOs.Estoque
{
    public class MovimentacaoEstoqueCreateDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoSKU { get; set; } = string.Empty;
        public int LocalEstoqueId { get; set; }
        public string Tipo { get; set; } = "Entrada";
        public string Motivo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal CustoUnitario { get; set; }
        public int? PedidoId { get; set; }
        public int? NFeId { get; set; }
        public int? FornecedorId { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public int? LocalDestinoId { get; set; }
        public string Observacoes { get; set; } = string.Empty;
        public DateTime DataMovimentacao { get; set; } = DateTime.Now;
    }

    public class MovimentacaoEstoqueResponseDto
    {
        public int Id { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoSKU { get; set; } = string.Empty;
        public string LocalEstoqueNome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal CustoUnitario { get; set; }
        public decimal CustoTotal { get; set; }
        public decimal EstoqueAnterior { get; set; }
        public decimal EstoqueAtual { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public DateTime DataMovimentacao { get; set; }
        public string CriadoPor { get; set; } = string.Empty;
    }

    public class MovimentacaoEstoqueFiltroDto
    {
        public int? ProdutoId { get; set; }
        public int? LocalEstoqueId { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    public class InventarioCreateDto
    {
        public int LocalEstoqueId { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }

    public class InventarioResponseDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string LocalEstoqueNome { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int TotalProdutos { get; set; }
        public int ProdutosDivergentes { get; set; }
        public decimal ValorDivergencia { get; set; }
    }

    public class ItemInventarioUpdateDto
    {
        public int Id { get; set; }
        public decimal EstoqueFisico { get; set; }
        public string Observacoes { get; set; } = string.Empty;
        public string ContadoPor { get; set; } = string.Empty;
    }

    public class TransferenciaEstoqueCreateDto
    {
        public int OrigemId { get; set; }
        public int DestinoId { get; set; }
        public List<ItemTransferenciaDto> Itens { get; set; } = new List<ItemTransferenciaDto>();
        public string Observacoes { get; set; } = string.Empty;
    }

    public class ItemTransferenciaDto
    {
        public int ProdutoId { get; set; }
        public decimal Quantidade { get; set; }
    }

    public class TransferenciaEstoqueResponseDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string OrigemNome { get; set; } = string.Empty;
        public string DestinoNome { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataSolicitacao { get; set; }
        public DateTime? DataEnvio { get; set; }
        public DateTime? DataRecebimento { get; set; }
        public int TotalProdutos { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class LocalEstoqueDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Endereco { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    public class ProdutoFornecedorCreateDto
    {
        public int ProdutoId { get; set; }
        public int FornecedorId { get; set; }
        public string CodigoFornecedor { get; set; } = string.Empty;
        public decimal PrecoCusto { get; set; }
        public decimal PrecoTabela { get; set; }
        public int PrazoEntregaDias { get; set; }
        public decimal QuantidadeMinima { get; set; }
        public bool FornecedorPrincipal { get; set; } = false;
    }

    public class ProdutoFornecedorResponseDto
    {
        public int Id { get; set; }
        public string FornecedorNome { get; set; } = string.Empty;
        public string CodigoFornecedor { get; set; } = string.Empty;
        public decimal PrecoCusto { get; set; }
        public decimal PrecoTabela { get; set; }
        public int PrazoEntregaDias { get; set; }
        public bool FornecedorPrincipal { get; set; }
        public bool Ativo { get; set; }
    }

    public class AlertaEstoqueResponseDto
    {
        public int Id { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoSKU { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal EstoqueAtual { get; set; }
        public decimal EstoqueMinimo { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public bool Notificado { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class KardexResponseDto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Documento { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal SaldoQuantidade { get; set; }
        public decimal SaldoValor { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }

    public class KardexFiltroDto
    {
        public int ProdutoId { get; set; }
        public int? LocalEstoqueId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
    }

    public class EstoqueAtualDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoSKU { get; set; } = string.Empty;
        public int LocalEstoqueId { get; set; }
        public string LocalEstoqueNome { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal QuantidadeReservada { get; set; }
        public decimal QuantidadeDisponivel { get; set; }
        public decimal CustoMedio { get; set; }
        public decimal ValorTotal { get; set; }
    }
}