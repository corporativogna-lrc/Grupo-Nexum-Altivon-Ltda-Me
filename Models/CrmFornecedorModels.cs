using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.ERP.Models
{
    // ==================== CRM ====================

    [Table("erp_leads_crm")]
    public class LeadCRM
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Nome { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(20)]
        public string? WhatsApp { get; set; }

        [Required, StringLength(50)]
        public string Origem { get; set; } = string.Empty; // Site, WhatsApp, Instagram, Feira, Indicacao

        [Required, StringLength(50)]
        public string Status { get; set; } = "Novo"; // Novo, EmAtendimento, Qualificado, Proposta, Convertido, Perdido, Frio

        [StringLength(50)]
        public string? Tipo { get; set; } // Cliente, Fornecedor, Parceiro, Dropshipping

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

        public int? ResponsavelId { get; set; }

        [StringLength(100)]
        public string? ResponsavelNome { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ValorEstimado { get; set; }

        public int? Probabilidade { get; set; } // 0 a 100

        public DateTime? DataPrevisaoFechamento { get; set; }
        public DateTime? DataUltimoContato { get; set; }
        public DateTime? DataConversao { get; set; }

        public int? ClienteConvertidoId { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
        [StringLength(100)]
        public string? CriadoPor { get; set; }
    }

    [Table("erp_interacoes_crm")]
    public class InteracaoCRM
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LeadId { get; set; }

        [ForeignKey("LeadId")]
        public virtual LeadCRM? Lead { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = string.Empty; // Ligacao, Email, WhatsApp, Reuniao, Visita, Nota

        [Required, StringLength(1000)]
        public string Descricao { get; set; } = string.Empty;

        public DateTime DataInteracao { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? Responsavel { get; set; }

        [StringLength(500)]
        public string? Anotacoes { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }

    [Table("erp_tarefas_crm")]
    public class TarefaCRM
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string Titulo { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Descricao { get; set; }

        [Required, StringLength(50)]
        public string Tipo { get; set; } = string.Empty; // Ligacao, Email, Proposta, Reuniao, FollowUp

        [Required, StringLength(20)]
        public string Prioridade { get; set; } = "Media"; // Baixa, Media, Alta, Urgente

        [Required, StringLength(20)]
        public string Status { get; set; } = "Pendente"; // Pendente, EmAndamento, Concluida, Cancelada

        public int? LeadId { get; set; }
        public int? ClienteId { get; set; }

        [Required]
        public DateTime DataVencimento { get; set; }

        public DateTime? DataConclusao { get; set; }

        [StringLength(100)]
        public string? Responsavel { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
    }

    // ==================== FORNECEDORES ====================

    [Table("erp_fornecedores")]
    public class Fornecedor
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(200)]
        public string RazaoSocial { get; set; } = string.Empty;

        [StringLength(200)]
        public string? NomeFantasia { get; set; }

        [Required, StringLength(20)]
        public string Cnpj { get; set; } = string.Empty;

        [StringLength(20)]
        public string? InscricaoEstadual { get; set; }

        [StringLength(20)]
        public string? InscricaoMunicipal { get; set; }

        [StringLength(200)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [StringLength(20)]
        public string? Celular { get; set; }

        [StringLength(200)]
        public string? Endereco { get; set; }

        [StringLength(20)]
        public string? Numero { get; set; }

        [StringLength(100)]
        public string? Complemento { get; set; }

        [StringLength(100)]
        public string? Bairro { get; set; }

        [StringLength(100)]
        public string? Cidade { get; set; }

        [StringLength(2)]
        public string? Uf { get; set; }

        [StringLength(10)]
        public string? Cep { get; set; }

        [StringLength(50)]
        public string? Segmento { get; set; }

        [StringLength(50)]
        public string? Status { get; set; } = "Ativo"; // Ativo, Inativo, Suspenso, Bloqueado

        [Column(TypeName = "decimal(18,2)")]
        public decimal? LimiteCredito { get; set; }

        public int? PrazoPagamentoDias { get; set; }

        [StringLength(50)]
        public string? FormaPagamentoPreferida { get; set; }

        [StringLength(500)]
        public string? Observacoes { get; set; }

        public bool Dropshipping { get; set; } = false;

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ComissaoDropshipping { get; set; }

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        public DateTime? AtualizadoEm { get; set; }
        [StringLength(100)]
        public string? CriadoPor { get; set; }
    }

    [Table("erp_avaliacoes_fornecedor")]
    public class AvaliacaoFornecedor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int FornecedorId { get; set; }

        [ForeignKey("FornecedorId")]
        public virtual Fornecedor? Fornecedor { get; set; }

        [Required]
        public int Nota { get; set; } // 1 a 5

        [StringLength(500)]
        public string? Comentario { get; set; }

        [StringLength(50)]
        public string? CategoriaAvaliacao { get; set; } // Qualidade, Prazo, Atendimento, Preco

        public DateTime CriadoEm { get; set; } = DateTime.Now;
        [StringLength(100)]
        public string? CriadoPor { get; set; }
    }
}
