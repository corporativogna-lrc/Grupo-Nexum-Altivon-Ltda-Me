using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NexumAltivon.API.DTOs
{
    public class CarrinhoDto
    {
        public Guid CarrinhoId { get; set; }
        public int? ClienteId { get; set; }
        public string SessaoId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal Desconto { get; set; }
        public decimal Total { get; set; }
        public int QuantidadeItens { get; set; }
        public List<ItemCarrinhoDto> Itens { get; set; } = new();
        public DateTime CriadoEm { get; set; }
        public DateTime AtualizadoEm { get; set; }
    }

    public class ItemCarrinhoDto
    {
        public int ItemId { get; set; }
        public int ProdutoId { get; set; }
        public string ProdutoNome { get; set; }
        public string ProdutoImagem { get; set; }
        public string Sku { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal PrecoOriginal { get; set; }
        public decimal Subtotal { get; set; }
        public int LojaId { get; set; }
        public string LojaNome { get; set; }
        public int EstoqueDisponivel { get; set; }
    }

    public class AdicionarItemCarrinhoRequest
    {
        [Required(ErrorMessage = "Produto obrigatório")]
        public int ProdutoId { get; set; }

        [Range(1, 100, ErrorMessage = "Quantidade deve ser entre 1 e 100")]
        public int Quantidade { get; set; } = 1;
    }

    public class AtualizarQuantidadeRequest
    {
        [Range(1, 100, ErrorMessage = "Quantidade deve ser entre 1 e 100")]
        public int Quantidade { get; set; }
    }

    public class MigrarCarrinhoRequest
    {
        [Required]
        public string SessaoId { get; set; }
        [Required]
        public int ClienteId { get; set; }
    }

    public class AplicarCupomRequest
    {
        [Required(ErrorMessage = "Código do cupom obrigatório")]
        [StringLength(20)]
        public string CodigoCupom { get; set; }
    }

    public class ResumoCarrinhoDto
    {
        public decimal Subtotal { get; set; }
        public decimal DescontoCupom { get; set; }
        public decimal Frete { get; set; }
        public decimal Total { get; set; }
        public string CupomAplicado { get; set; }
        public int TotalItens { get; set; }
    }
}
