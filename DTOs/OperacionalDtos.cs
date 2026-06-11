using System;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.ERP.DTOs
{
    public class EmitirNFeDto
    {
        [Required]
        public int PedidoId { get; set; }
    }

    public class CancelarNFeDto
    {
        [Required]
        public int NotaFiscalId { get; set; }

        [Required, StringLength(500)]
        public string Justificativa { get; set; } = string.Empty;
    }

    public class ConfiguracaoFiscalDto
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string InscricaoEstadual { get; set; } = string.Empty;
        public string RazaoSocial { get; set; } = string.Empty;
        public string NomeFantasia { get; set; } = string.Empty;
        public string Ambiente { get; set; } = "Homologacao";
        public bool Ativo { get; set; } = true;
    }

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

        [Required]
        public string Tipo { get; set; } = string.Empty;

        [Required]
        public decimal Quantidade { get; set; }

        public decimal? CustoUnitario { get; set; }
        public int? FornecedorId { get; set; }
        public int? PedidoId { get; set; }
        public int? OrigemLojaId { get; set; }
        public int? DestinoLojaId { get; set; }
        public string DocumentoReferencia { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;
    }

    public class InventarioDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
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

    public class FiltroRelatorioDto
    {
        public string TipoRelatorio { get; set; } = string.Empty;
        public string? Formato { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? LojaId { get; set; }
    }

    public class RelatorioGeradoDto
    {
        public string TipoRelatorio { get; set; } = string.Empty;
        public DateTime GeradoEm { get; set; } = DateTime.Now;
        public object? Dados { get; set; }
    }

    public class ResumoCrmDto
    {
        public int LeadsAtivos { get; set; }
        public int LeadsConvertidos { get; set; }
        public int TarefasPendentes { get; set; }
        public decimal ValorPipeline { get; set; }
    }
}
