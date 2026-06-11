using System;

namespace NexumAltivon_ERP.DTOs.CRM
{
    public class AtividadeCreateDto
    {
        public string Titulo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Ligacao";
        public DateTime DataAgendamento { get; set; }
        public int? OportunidadeId { get; set; }
        public int? ClienteId { get; set; }
        public int? LeadId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
        public string Prioridade { get; set; } = "Media";
        public string Observacoes { get; set; } = string.Empty;
    }

    public class AtividadeUpdateDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public DateTime? DataConclusao { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }

    public class AtividadeResponseDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public DateTime DataAgendamento { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public bool Atrasada => Status != "Concluida" && DataAgendamento < DateTime.Now;
        public DateTime CriadoEm { get; set; }
    }

    public class AtividadeFiltroDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool? Atrasadas { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }
}