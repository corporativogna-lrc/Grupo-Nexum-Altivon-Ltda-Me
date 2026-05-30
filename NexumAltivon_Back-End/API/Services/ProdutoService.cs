using AutoMapper;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;

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
        var produto = await _context.Produtos
            .Include(p => p.Loja)
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (produto == null)
            return ApiResponse<ProdutoDto>.Erro("Produto não encontrado.");
        return ApiResponse<ProdutoDto>.Ok(_mapper.Map<ProdutoDto>(produto));
    }

    public async Task<ApiResponse<ProdutoDto>> ObterPorSlugAsync(string slug)
    {
        var produto = await _context.Produtos
            .Include(p => p.Loja)
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.Slug == slug);
        if (produto == null)
            return ApiResponse<ProdutoDto>.Erro("Produto não encontrado.");
        return ApiResponse<ProdutoDto>.Ok(_mapper.Map<ProdutoDto>(produto));
    }

    public async Task<ApiResponse<List<ProdutoListagemDto>>> ListarAsync(PaginacaoDto paginacao, int? lojaId = null, int? categoriaId = null)
    {
        var query = _context.Produtos
            .Include(p => p.Loja)
            .Where(p => p.Ativo)
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
        var query = _context.Produtos
            .Include(p => p.Loja)
            .Where(p => p.Ativo && p.Destaque)
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
        if (await _context.Produtos.AnyAsync(p => p.Sku == dto.Sku))
            return ApiResponse<ProdutoDto>.Erro("SKU já existe.");

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
}
