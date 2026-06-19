using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;
using System.Text.RegularExpressions;

namespace NexumAltivon.API.Services;

public class ProdutoService : IProdutoService
{
    private readonly NexumDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogAuditoriaService _auditoria;

    public ProdutoService(NexumDbContext context, IMapper mapper, ILogAuditoriaService auditoria)
    {
        _context = context;
        _mapper = mapper;
        _auditoria = auditoria;
    }

    public async Task<ApiResponse<ProdutoDto>> ObterPorIdAsync(int id)
    {
        var produto = await ProdutosPublicaveis()
            .Include(p => p.Loja)
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (produto == null)
            return ApiResponse<ProdutoDto>.Erro("Produto não encontrado.");
        return ApiResponse<ProdutoDto>.Ok(_mapper.Map<ProdutoDto>(produto));
    }

    public async Task<ApiResponse<ProdutoDto>> ObterPorSlugAsync(string slug)
    {
        var produto = await ProdutosPublicaveis()
            .Include(p => p.Loja)
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Slug == slug);
        if (produto == null)
            return ApiResponse<ProdutoDto>.Erro("Produto não encontrado.");
        return ApiResponse<ProdutoDto>.Ok(_mapper.Map<ProdutoDto>(produto));
    }

    public async Task<ApiResponse<List<ProdutoListagemDto>>> ListarAsync(PaginacaoDto paginacao, int? lojaId = null, int? categoriaId = null)
    {
        var query = ProdutosPublicaveis()
            .Include(p => p.Loja)
            .AsQueryable();

        if (lojaId.HasValue)
            query = query.Where(p => p.LojaId == lojaId.Value);
        if (categoriaId.HasValue)
            query = query.Where(p => p.CategoriaId == categoriaId.Value);
        if (!string.IsNullOrWhiteSpace(paginacao.Busca))
            query = query.Where(p => p.Nome.Contains(paginacao.Busca) || p.Sku.Contains(paginacao.Busca));

        var total = await query.CountAsync();
        var produtos = await query
            .OrderByDescending(p => p.Destaque)
            .ThenByDescending(p => p.CreatedAt)
            .Skip((paginacao.Pagina - 1) * paginacao.ItensPorPagina)
            .Take(paginacao.ItensPorPagina)
            .ToListAsync();

        var totalPaginas = (int)Math.Ceiling(total / (double)paginacao.ItensPorPagina);

        return ApiResponse<List<ProdutoListagemDto>>.Ok(
            _mapper.Map<List<ProdutoListagemDto>>(produtos),
            total: total, pagina: paginacao.Pagina, totalPaginas: totalPaginas);
    }

    public async Task<ApiResponse<List<ProdutoListagemDto>>> ListarDestaquesAsync(int? lojaId = null)
    {
        var query = ProdutosPublicaveis()
            .Include(p => p.Loja)
            .Where(p => p.Destaque)
            .AsQueryable();

        if (lojaId.HasValue)
            query = query.Where(p => p.LojaId == lojaId.Value);

        var produtos = await query
            .OrderByDescending(p => p.CreatedAt)
            .Take(12)
            .ToListAsync();

        return ApiResponse<List<ProdutoListagemDto>>.Ok(_mapper.Map<List<ProdutoListagemDto>>(produtos));
    }

    public async Task<ApiResponse<ProdutoDto>> CriarAsync(CriarProdutoDto dto)
    {
        var skuNormalizado = NormalizarSkuInterno(dto);
        if (await _context.Produtos.AnyAsync(p => p.Sku == skuNormalizado))
            return ApiResponse<ProdutoDto>.Erro("SKU já existe.");

        dto.Sku = skuNormalizado;
        var produto = _mapper.Map<Produto>(dto);
        _context.Produtos.Add(produto);
        await _context.SaveChangesAsync();

        await _auditoria.RegistrarAsync("produtos", produto.Id, "INSERT", null, "Usuario",
            null, null, null, $"{{\"nome\":\"{dto.Nome}\",\"sku\":\"{dto.Sku}\"}}", "/api/produtos");

        return ApiResponse<ProdutoDto>.Ok(_mapper.Map<ProdutoDto>(produto), "Produto criado com sucesso.");
    }

    public async Task<ApiResponse<ProdutoDto>> AtualizarAsync(int id, CriarProdutoDto dto)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
            return ApiResponse<ProdutoDto>.Erro("Produto não encontrado.");

        var skuNormalizado = NormalizarSkuInterno(dto);
        if (await _context.Produtos.AnyAsync(p => p.Id != id && p.Sku == skuNormalizado))
            return ApiResponse<ProdutoDto>.Erro("SKU já existe.");

        dto.Sku = skuNormalizado;
        _mapper.Map(dto, produto);
        produto.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<ProdutoDto>.Ok(_mapper.Map<ProdutoDto>(produto), "Produto atualizado com sucesso.");
    }

    public async Task<ApiResponse<bool>> ExcluirAsync(int id)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
            return ApiResponse<bool>.Erro("Produto não encontrado.");

        produto.Ativo = false;
        produto.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, "Produto desativado com sucesso.");
    }

    public async Task<ApiResponse<bool>> AtualizarEstoqueAsync(int id, int quantidade)
    {
        var produto = await _context.Produtos.FindAsync(id);
        if (produto == null)
            return ApiResponse<bool>.Erro("Produto não encontrado.");

        produto.EstoqueAtual += quantidade;
        produto.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Ok(true, $"Estoque atualizado. Novo saldo: {produto.EstoqueAtual}");
    }

    private IQueryable<Produto> ProdutosPublicaveis() =>
        _context.Produtos.Where(produto =>
            produto.Ativo &&
            produto.LojaId > 0 &&
            produto.CategoriaId.HasValue &&
            !string.IsNullOrEmpty(produto.Nome) &&
            !string.IsNullOrEmpty(produto.Sku) &&
            !string.IsNullOrEmpty(produto.Slug) &&
            (!string.IsNullOrEmpty(produto.DescricaoCurta) || !string.IsNullOrEmpty(produto.DescricaoLonga)) &&
            !string.IsNullOrEmpty(produto.ImagemPrincipal) &&
            produto.Preco > 0 &&
            produto.Peso > 0 &&
            produto.Altura > 0 &&
            produto.Largura > 0 &&
            produto.Comprimento > 0);

    private static string NormalizarSkuInterno(CriarProdutoDto dto)
    {
        var prefixo = ObterPrefixoOrigem(dto.TipoProduto, dto.FornecedorId);
        var baseSku = RemoverPrefixoConhecido(dto.Sku);

        if (string.IsNullOrWhiteSpace(baseSku))
        {
            baseSku = dto.Slug;
        }

        if (string.IsNullOrWhiteSpace(baseSku))
        {
            baseSku = dto.Nome;
        }

        var corpo = Regex.Replace(baseSku ?? string.Empty, @"[^a-zA-Z0-9]+", "-")
            .Trim('-')
            .ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(corpo))
        {
            corpo = DateTime.UtcNow.ToString("yyMMddHHmmss");
        }

        return $"{prefixo.ToUpperInvariant()}-{corpo}";
    }

    private static string ObterPrefixoOrigem(string? tipoProduto, int? fornecedorId)
    {
        var valorComparacao = $"{tipoProduto ?? string.Empty} {(fornecedorId.HasValue ? "fornecedor" : string.Empty)}".ToLowerInvariant();

        if (valorComparacao.Contains("drop"))
            return "Ds";

        if (valorComparacao.Contains("marketplace"))
            return "Ec";

        if (fornecedorId.HasValue)
            return "Fo";

        return "Ec";
    }

    private static string? RemoverPrefixoConhecido(string? sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return null;

        var valor = sku.Trim();
        var normalizado = Regex.Replace(valor, @"\s+", "");

        foreach (var prefixo in new[] { "Fo", "Ds", "Ec", "Cl", "Fu", "Lj", "Pr" })
        {
            if (normalizado.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))
            {
                return normalizado[prefixo.Length..].TrimStart('-', '_', ' ');
            }
        }

        return valor;
    }
}
