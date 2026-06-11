using System;

namespace NexumAltivon_ERP.DTOs.CRM
{
    public class CampanhaCreateDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Email";
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public decimal Orcamento { get; set; }
        public string PublicoAlvo { get; set; } = string.Empty;
        public string Conteudo { get; set; } = string.Empty;
    }

    public class CampanhaUpdateDto
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal CustoAtual { get; set; }
        public int Alcance { get; set; }
        public int Cliques { get; set; }
        public int LeadsGerados { get; set; }
        public int OportunidadesGeradas { get; set; }
        public int VendasGeradas { get; set; }
        public decimal ReceitaGerada { get; set; }
    }

    public class CampanhaResponseDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public decimal Orcamento { get; set; }
        public decimal CustoAtual { get; set; }
        public int Alcance { get; set; }
        public int Cliques { get; set; }
        public int LeadsGerados { get; set; }
        public int OportunidadesGeradas { get; set; }
        public int VendasGeradas { get; set; }
        public decimal ReceitaGerada { get; set; }
        public decimal ROAS { get; set; }
        public decimal CPC => Cliques > 0 ? CustoAtual / Cliques : 0;
        public decimal CPL => LeadsGerados > 0 ? CustoAtual / LeadsGerados : 0;
        public decimal CPA => VendasGeradas > 0 ? CustoAtual / VendasGeradas : 0;
    }

    public class LeadCRMDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public int? Score { get; set; }
        public string Responsavel { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
    }

    public class LeadCRMCreateDto
    {
        public string Nome { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Telefone { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Cliente";
        public string Origem { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
        public string Cargo { get; set; } = string.Empty;
        public string Interesses { get; set; } = string.Empty;
        public string Observacoes { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
    }

    public class SegmentoClienteDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Cor { get; set; } = string.Empty;
        public int Prioridade { get; set; }
        public int TotalClientes { get; set; }
    }

    public class TicketSuporteCreateDto
    {
        public string Assunto { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Prioridade { get; set; } = "Media";
        public int? ClienteId { get; set; }
        public string ClienteNome { get; set; } = string.Empty;
        public string ClienteTelefone { get; set; } = string.Empty;
        public string ClienteEmail { get; set; } = string.Empty;
        public int? PedidoId { get; set; }
    }

    public class TicketSuporteResponseDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Assunto { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Prioridade { get; set; } = string.Empty;
        public string ClienteNome { get; set; } = string.Empty;
        public string Responsavel { get; set; } = string.Empty;
        public DateTime? DataAtribuicao { get; set; }
        public DateTime? DataResolucao { get; set; }
        public int Avaliacao { get; set; }
        public int TempoAtendimentoMinutos { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class InteracaoTicketCreateDto
    {
        public int TicketId { get; set; }
        public string Tipo { get; set; } = "Resposta";
        public string Conteudo { get; set; } = string.Empty;
        public bool Interno { get; set; } = false;
        public string Anexos { get; set; } = string.Empty;
    }

    public class DashboardCrmDto
    {
        public int TotalOportunidadesAtivas { get; set; }
        public decimal ValorPipeline { get; set; }
        public int TotalAtividadesHoje { get; set; }
        public int TotalAtividadesAtrasadas { get; set; }
        public int TotalTicketsAbertos { get; set; }
        public int TotalTicketsUrgentes { get; set; }
        public int TotalLeadsNovos { get; set; }
        public int TotalLeadsConvertidos { get; set; }
        public decimal TaxaConversaoLeads { get; set; }
        public decimal TicketMedioOportunidades { get; set; }
        public List<FunilVendasDto> FunilVendas { get; set; } = new List<FunilVendasDto>();
    }
}