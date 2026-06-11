using System;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.ERP.DTOs
{
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
        public string? Observacoes { get; set; }
        public string? Empresa { get; set; }
        public string? Cargo { get; set; }
        public string? Cnpj { get; set; }
        public string? Cpf { get; set; }
        public string? ResponsavelNome { get; set; }
        public decimal? ValorEstimado { get; set; }
        public int? Probabilidade { get; set; }
        public DateTime? DataPrevisaoFechamento { get; set; }
        public DateTime? DataUltimoContato { get; set; }
        public DateTime? DataConversao { get; set; }
        public int DiasDesdeCriacao => (DateTime.Now - CriadoEm).Days;
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

        [StringLength(500)]
        public string? Observacoes { get; set; }

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
    }

    public class AtualizarStatusLeadDto
    {
        [Required]
        public int LeadId { get; set; }

        [Required, StringLength(20)]
        public string NovoStatus { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Motivo { get; set; }
    }

    public class InteracaoCRMDto
    {
        public int Id { get; set; }
        public int LeadId { get; set; }
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
        public string? Descricao { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? LeadId { get; set; }
        public DateTime DataVencimento { get; set; }
        public DateTime? DataConclusao { get; set; }
        public string? Responsavel { get; set; }
        public bool Atrasada => Status != "Concluida" && DateTime.Now > DataVencimento;
        public DateTime CriadoEm { get; set; }
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
        public decimal ValorMedio => Quantidade > 0 ? ValorTotal / Quantidade : 0;
        public int ProbabilidadeMedia { get; set; }
    }

    public class FornecedorDto
    {
        public int Id { get; set; }
        public string RazaoSocial { get; set; } = string.Empty;
        public string? NomeFantasia { get; set; }
        public string Cnpj { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Telefone { get; set; }
        public string? Celular { get; set; }
        public string? Cidade { get; set; }
        public string? Uf { get; set; }
        public string? Segmento { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal? LimiteCredito { get; set; }
        public int? PrazoPagamentoDias { get; set; }
        public bool Dropshipping { get; set; }
        public decimal? ComissaoDropshipping { get; set; }
        public double? MediaAvaliacao { get; set; }
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
