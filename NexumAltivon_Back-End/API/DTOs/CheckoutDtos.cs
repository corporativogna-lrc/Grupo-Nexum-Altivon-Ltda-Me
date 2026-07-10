/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.API.DTOs
{
    public class IniciarCheckoutRequest
    {
        [Required]
        public int EnderecoId { get; set; }
        public int? CupomId { get; set; }
        public string Observacoes { get; set; }
    }

    public class CheckoutDto
    {
        public int CheckoutId { get; set; }
        public string NumeroPedido { get; set; }
        public int ClienteId { get; set; }
        public string ClienteNome { get; set; }
        public EnderecoDto EnderecoEntrega { get; set; }
        public List<ItemCarrinhoDto> Itens { get; set; } = new();
        public ResumoCheckoutDto Resumo { get; set; }
        public List<OpcaoFreteDto> OpcoesFrete { get; set; } = new();
        public string Status { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class ResumoCheckoutDto
    {
        public decimal Subtotal { get; set; }
        public decimal Desconto { get; set; }
        public decimal Frete { get; set; }
        public decimal Total { get; set; }
        public int TotalItens { get; set; }
        public string CupomAplicado { get; set; }
    }

    public class OpcaoFreteDto
    {
        public string Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public int PrazoDias { get; set; }
        public bool SeguroIncluso { get; set; }
    }

    public class SelecionarFreteRequest
    {
        [Required]
        public string CodigoFrete { get; set; }
    }

    public class FinalizarCheckoutRequest
    {
        [Required]
        public int CheckoutId { get; set; }
        [Required]
        public string MetodoPagamento { get; set; } // PIX, CartaoCredito, Boleto
        public DadosCartaoDto DadosCartao { get; set; }
        public int? Parcelas { get; set; } = 1;
        public bool UsarSaldoCashback { get; set; } = false;
    }

    public class DadosCartaoDto
    {
        [Required]
        public string Numero { get; set; }
        [Required]
        public string NomeTitular { get; set; }
        [Required]
        public string Validade { get; set; }
        [Required]
        public string Cvv { get; set; }
        public string CpfTitular { get; set; }
    }

    public class CheckoutResponseDto
    {
        public bool Sucesso { get; set; }
        public string NumeroPedido { get; set; }
        public int PedidoId { get; set; }
        public string StatusPagamento { get; set; }
        public string UrlPagamento { get; set; }
        public string QrCodeBase64 { get; set; } // Para PIX
        public string QrCodeTexto { get; set; } // Para PIX
        public string LinhaDigitavel { get; set; } // Para Boleto
        public string Mensagem { get; set; }
        public List<string> Erros { get; set; } = new();
    }
}
