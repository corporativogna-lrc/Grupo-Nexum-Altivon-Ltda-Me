namespace NexumAltivon.API.DTOs;

public class ProdutoDto
{
    public int Id { get; set; }
    public int LojaId { get; set; }
    public string LojaNome { get; set; } = string.Empty;
    public int? CategoriaId { get; set; }
    public string? CategoriaNome { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? DescricaoCurta { get; set; }
    public string? DescricaoLonga { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }
    public decimal Custo { get; set; }
    public decimal Peso { get; set; }
    public decimal Altura { get; set; }
    public decimal Largura { get; set; }
    public decimal Comprimento { get; set; }
    public string? ImagemPrincipal { get; set; }
    public List<string>? ImagensGaleria { get; set; }
    public int EstoqueAtual { get; set; }
    public int EstoqueMinimo { get; set; }
    public string TipoProduto { get; set; } = "Proprio";
    public string? Marca { get; set; }
    public string? Tags { get; set; }
    public bool Destaque { get; set; }
    public bool Ativo { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CriarProdutoDto
{
    public int LojaId { get; set; }
    public int? CategoriaId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? DescricaoCurta { get; set; }
    public string? DescricaoLonga { get; set; }
    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }
    public decimal Custo { get; set; }
    public decimal Peso { get; set; }
    public decimal Altura { get; set; }
    public decimal Largura { get; set; }
    public decimal Comprimento { get; set; }
    public string? ImagemPrincipal { get; set; }
    public List<string>? ImagensGaleria { get; set; }
    public int EstoqueMinimo { get; set; } = 5;
    public int EstoqueAtual { get; set; } = 0;
    public string TipoProduto { get; set; } = "Proprio";
    public int? FornecedorId { get; set; }
    public string? Marca { get; set; }
    public string? Tags { get; set; }
    public string? SeoTitulo { get; set; }
    public string? SeoDescricao { get; set; }
    public bool Destaque { get; set; } = false;
    public bool Ativo { get; set; } = true;
}

public class ProdutoListagemDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public decimal? PrecoPromocional { get; set; }
    public string? ImagemPrincipal { get; set; }
    public string LojaNome { get; set; } = string.Empty;
    public bool Ativo { get; set; }
}
