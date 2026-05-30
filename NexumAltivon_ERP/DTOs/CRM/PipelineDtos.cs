using System.Collections.Generic;

namespace NexumAltivon_ERP.DTOs.CRM
{
    public class PipelineDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Ordem { get; set; }
        public string Cor { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public int TotalOportunidades { get; set; }
        public decimal ValorTotal { get; set; }
    }

    public class OportunidadeCreateDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int PipelineId { get; set; }
        public string Etapa { get; set; } = "Lead";
        public int? ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string ClienteTelefone { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public int? LeadId { get; set; }
        public decimal ValorEstimado { get; set; }
        public decimal Probabilidade { get; set; } = 0;
        public DateTime? DataPrevisaoFechamento { get; set; }
        public string Responsavel { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public int? CampanhaId { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }

    public class OportunidadeUpdateDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Etapa { get; set; } = string.Empty;
        public decimal ValorEstimado { get; set; }
        public decimal? ValorFechado { get; set; }
        public decimal Probabilidade { get; set; }
        public DateTime? DataPrevisaoFechamento { get; set; }
        public string Responsavel { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
    }

    public class OportunidadeFechamentoDto
    {
        public int Id { get; set; }
        public bool Ganho { get; set; }
        public decimal? ValorFechado { get; set; }
        public DateTime DataFechamento { get; set; } = DateTime.Now;
        public string MotivoPerda { get; set; } = string.Empty;
    }

    public class OportunidadeResponseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Etapa { get; set; } = string.Empty;
        public string PipelineNome { get; set; } = string.Empty;
        public string ClienteNome { get; set; } = string.Empty;
        public string ClienteTelefone { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public decimal ValorEstimado { get; set; }
        public decimal? ValorFechado { get; set; }
        public decimal Probabilidade { get; set; }
        public decimal ValorPonderado => ValorEstimado * (Probabilidade / 100);
        public DateTime? DataPrevisaoFechamento { get; set; }
        public DateTime? DataFechamento { get; set; }
        public string Responsavel { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public int DiasNoPipeline { get; set; }
    }

    public class OportunidadeFiltroDto
    {
        public int? PipelineId { get; set; }
        public string Etapa { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string Busca { get; set; } = string.Empty;
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    public class OportunidadeResumoDto
    {
        public int TotalOportunidades { get; set; }
        public decimal ValorTotalEstimado { get; set; }
        public decimal ValorTotalPonderado { get; set; }
        public decimal ValorTotalFechado { get; set; }
        public int TotalGanhas { get; set; }
        public int TotalPerdidas { get; set; }
        public int TotalEmAberto { get; set; }
        public decimal TaxaConversao { get; set; }
        public decimal TicketMedio { get; set; }
    }

    public class FunilVendasDto
    {
        public string Etapa { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal PercentualConversao { get; set; }
    }
}