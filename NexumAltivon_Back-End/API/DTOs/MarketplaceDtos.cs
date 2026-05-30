using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.API.DTOs
{
    // ========== MERCADO LIVRE ==========
    public class MlPublicarProdutoRequest
    {
        [Required]
        public int ProdutoId { get; set; }
        public string CategoriaMl { get; set; }
        public decimal? PrecoEspecifico { get; set; }
        public int? EstoqueEspecifico { get; set; }
    }

    public class MlProdutoPublicadoDto
    {
        public int ProdutoId { get; set; }
        public string MlItemId { get; set; }
        public string Permalink { get; set; }
        public string Status { get; set; }
        public decimal PrecoMl { get; set; }
        public int EstoqueMl { get; set; }
        public DateTime PublicadoEm { get; set; }
    }

    public class MlPedidoRecebidoDto
    {
        public string MlOrderId { get; set; }
        public string Status { get; set; }
        public DateTime CriadoEm { get; set; }
        public decimal Total { get; set; }
        public List<MlItemPedidoDto> Itens { get; set; } = new();
        public MlCompradorDto Comprador { get; set; }
        public MlEnvioDto Envio { get; set; }
    }

    public class MlItemPedidoDto
    {
        public string MlItemId { get; set; }
        public string Titulo { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
    }

    public class MlCompradorDto
    {
        public string MlUserId { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Telefone { get; set; }
        public string Cpf { get; set; }
    }

    public class MlEnvioDto
    {
        public string Modo { get; set; }
        public string Status { get; set; }
        public string CodigoRastreio { get; set; }
        public MlEnderecoDto Endereco { get; set; }
    }

    public class MlEnderecoDto
    {
        public string Cep { get; set; }
        public string Logradouro { get; set; }
        public string Numero { get; set; }
        public string Bairro { get; set; }
        public string Cidade { get; set; }
        public string Estado { get; set; }
    }

    // ========== SHOPEE ==========
    public class ShopeePublicarProdutoRequest
    {
        [Required]
        public int ProdutoId { get; set; }
        public long? ShopeeCategoryId { get; set; }
        public decimal? PrecoEspecifico { get; set; }
    }

    public class ShopeeProdutoDto
    {
        public int ProdutoId { get; set; }
        public long ShopeeItemId { get; set; }
        public string Status { get; set; }
        public decimal Preco { get; set; }
        public int Estoque { get; set; }
    }

    // ========== AMAZON ==========
    public class AmazonPublicarProdutoRequest
    {
        [Required]
        public int ProdutoId { get; set; }
        public string Asin { get; set; }
        public string SkuAmazon { get; set; }
    }

    // ========== HUB UNIFICADO ==========
    public class SincronizarProdutoRequest
    {
        [Required]
        public int ProdutoId { get; set; }
        public List<string> Canais { get; set; } = new(); // "mercadolivre", "shopee", "amazon"
        public bool ForcarAtualizacao { get; set; } = false;
    }

    public class SyncStatusDto
    {
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public List<CanalSyncDto> Canais { get; set; } = new();
        public DateTime? UltimaSync { get; set; }
    }

    public class CanalSyncDto
    {
        public string Canal { get; set; }
        public string Status { get; set; } // SINCRONIZADO, PENDENTE, ERRO, NAO_PUBLICADO
        public string IdExterno { get; set; }
        public string Url { get; set; }
        public string Erro { get; set; }
        public DateTime? SyncEm { get; set; }
    }

    public class RelatorioSyncDto
    {
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public int TotalProdutos { get; set; }
        public int Sincronizados { get; set; }
        public int ComErro { get; set; }
        public int Pendentes { get; set; }
        public List<SyncStatusDto> Detalhes { get; set; } = new();
    }
}
