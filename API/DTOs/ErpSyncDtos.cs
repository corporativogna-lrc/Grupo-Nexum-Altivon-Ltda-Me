using System;
using System.Collections.Generic;

namespace NexumAltivon.API.DTOs
{
    public class ErpSyncRequest
    {
        public string Entidade { get; set; } // PRODUTOS, CLIENTES, PEDIDOS, ESTOQUE, FINANCEIRO
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool ForcaCompleta { get; set; } = false;
    }

    public class ErpSyncResultDto
    {
        public bool Sucesso { get; set; }
        public string Entidade { get; set; }
        public int RegistrosEnviados { get; set; }
        public int RegistrosRecebidos { get; set; }
        public int RegistrosComErro { get; set; }
        public string Mensagem { get; set; }
        public DateTime ExecutadoEm { get; set; }
        public List<string> Erros { get; set; } = new();
    }

    public class ErpProdutoDto
    {
        public int ProdutoId { get; set; }
        public string Nome { get; set; }
        public string Sku { get; set; }
        public string CodigoErp { get; set; }
        public decimal PrecoCusto { get; set; }
        public decimal PrecoVenda { get; set; }
        public int Estoque { get; set; }
        public string Unidade { get; set; }
        public string Ncm { get; set; }
        public string Cest { get; set; }
        public decimal PesoKg { get; set; }
        public bool Ativo { get; set; }
    }

    public class ErpClienteDto
    {
        public int ClienteId { get; set; }
        public string Nome { get; set; }
        public string CpfCnpj { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string EnderecoCompleto { get; set; }
        public string Cep { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
        public string CodigoErp { get; set; }
    }

    public class ErpPedidoDto
    {
        public string NumeroPedido { get; set; }
        public string CodigoErp { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public string CpfCnpj { get; set; }
        public DateTime DataEmissao { get; set; }
        public decimal Total { get; set; }
        public string Status { get; set; }
        public List<ErpPedidoItemDto> Itens { get; set; } = new();
    }

    public class ErpPedidoItemDto
    {
        public string Sku { get; set; }
        public string Descricao { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal Subtotal { get; set; }
        public string Ncm { get; set; }
    }

    public class ErpConfiguracaoDto
    {
        public string UrlBase { get; set; }
        public string Usuario { get; set; }
        public string Senha { get; set; }
        public string TokenApi { get; set; }
        public int IntervaloSyncMinutos { get; set; } = 60;
        public bool SyncAutomatico { get; set; } = true;
        public List<string> EntidadesAtivas { get; set; } = new();
    }

    public class ErpStatusConexaoDto
    {
        public bool Conectado { get; set; }
        public string VersaoErp { get; set; }
        public DateTime? UltimoSync { get; set; }
        public string Mensagem { get; set; }
        public int LatenciaMs { get; set; }
    }
}
