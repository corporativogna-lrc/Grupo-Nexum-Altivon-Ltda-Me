using System;
using System.Collections.Generic;

namespace NexumAltivon_ERP.DTOs.Fiscal
{
    public class NFeCreateDto
    {
        public string Serie { get; set; } = "1";
        public string TipoOperacao { get; set; } = "1";
        public int CFOPId { get; set; }
        public int DestinatarioId { get; set; }
        public string DestinatarioNome { get; set; } = string.Empty;
        public string DestinatarioCNPJ { get; set; } = string.Empty;
        public string DestinatarioIE { get; set; } = string.Empty;
        public string DestinatarioEndereco { get; set; } = string.Empty;
        public string DestinatarioUF { get; set; } = string.Empty;
        public DateTime? DataSaida { get; set; }
        public decimal ValorTotalFrete { get; set; }
        public decimal ValorTotalSeguro { get; set; }
        public decimal ValorTotalDesconto { get; set; }
        public string InformacoesAdicionais { get; set; } = string.Empty;
        public int? PedidoId { get; set; }
        public int? LojaId { get; set; }
        public List<ItemNFeCreateDto> Itens { get; set; } = new List<ItemNFeCreateDto>();
    }

    public class ItemNFeCreateDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; } = string.Empty;
        public string ProdutoSKU { get; set; } = string.Empty;
        public string ProdutoEAN { get; set; } = string.Empty;
        public string NCM { get; set; } = string.Empty;
        public string CFOP { get; set; } = string.Empty;
        public string Unidade { get; set; } = "UN";
        public decimal Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorDesconto { get; set; }
        public decimal ValorFrete { get; set; }
        public decimal ValorSeguro { get; set; }
        public decimal AliquotaICMS { get; set; }
        public decimal AliquotaIPI { get; set; }
        public string CSTICMS { get; set; } = string.Empty;
        public string CSTIPI { get; set; } = string.Empty;
        public string CSTPIS { get; set; } = string.Empty;
        public string CSTCOFINS { get; set; } = string.Empty;
    }

    public class NFeResponseDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string ChaveAcesso { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public DateTime? DataAutorizacao { get; set; }
        public string ProtocoloAutorizacao { get; set; } = string.Empty;
        public string DestinatarioNome { get; set; } = string.Empty;
        public string DestinatarioCNPJ { get; set; } = string.Empty;
        public decimal ValorTotalNF { get; set; }
        public int? PedidoId { get; set; }
        public int? LojaId { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class NFeCancelamentoDto
    {
        public int Id { get; set; }
        public string Motivo { get; set; } = string.Empty;
    }

    public class NFeFiltroDto
    {
        public string Status { get; set; } = string.Empty;
        public string Numero { get; set; } = string.Empty;
        public string DestinatarioNome { get; set; } = string.Empty;
        public string DestinatarioCNPJ { get; set; } = string.Empty;
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? LojaId { get; set; }
        public int Pagina { get; set; } = 1;
        public int TamanhoPagina { get; set; } = 20;
    }

    public class NFCeCreateDto
    {
        public string Serie { get; set; } = "1";
        public decimal ValorTotal { get; set; }
        public decimal ValorDesconto { get; set; }
        public string CPFConsumidor { get; set; } = string.Empty;
        public string NomeConsumidor { get; set; } = string.Empty;
        public int? LojaId { get; set; }
    }

    public class NFCeResponseDto
    {
        public int Id { get; set; }
        public string Numero { get; set; } = string.Empty;
        public string Serie { get; set; } = string.Empty;
        public string ChaveAcesso { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime DataEmissao { get; set; }
        public decimal ValorTotal { get; set; }
        public string ProtocoloAutorizacao { get; set; } = string.Empty;
    }
}
