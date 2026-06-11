using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.DTOs;
using NexumAltivon.API.Models;
using NexumAltivon.API.Services;

namespace NexumAltivon.API.Services
{
    public interface ICarrinhoService
    {
        Task<CarrinhoDto> ObterCarrinhoAsync(string sessaoId, int? clienteId = null);
        Task<CarrinhoDto> AdicionarItemAsync(string sessaoId, int? clienteId, AdicionarItemCarrinhoRequest request);
        Task<CarrinhoDto> RemoverItemAsync(string sessaoId, int itemId);
        Task<CarrinhoDto> AtualizarQuantidadeAsync(string sessaoId, int itemId, int quantidade);
        Task<CarrinhoDto> AplicarCupomAsync(string sessaoId, string codigoCupom);
        Task<CarrinhoDto> RemoverCupomAsync(string sessaoId);
        Task<bool> MigrarCarrinhoSessaoParaClienteAsync(string sessaoId, int clienteId);
        Task LimparCarrinhoAsync(string sessaoId);
        Task<ResumoCarrinhoDto> ObterResumoAsync(string sessaoId);
    }

    public class CarrinhoService : ICarrinhoService
    {
        private readonly NexumDbContext _context;
        private readonly ILogAuditoriaService _auditoria;

        public CarrinhoService(NexumDbContext context, ILogAuditoriaService auditoria)
        {
            _context = context;
            _auditoria = auditoria;
        }

        public async Task<CarrinhoDto> ObterCarrinhoAsync(string sessaoId, int? clienteId = null)
        {
            var carrinho = await BuscarOuCriarCarrinho(sessaoId, clienteId);
            return MapearParaDto(carrinho);
        }

        public async Task<CarrinhoDto> AdicionarItemAsync(string sessaoId, int? clienteId, AdicionarItemCarrinhoRequest request)
        {
            var carrinho = await BuscarOuCriarCarrinho(sessaoId, clienteId);
            var produto = await _context.Produtos
                .Include(p => p.Loja)
                .FirstOrDefaultAsync(p => p.ProdutoId == request.ProdutoId && p.Ativo);

            if (produto == null)
                throw new ArgumentException("Produto não encontrado ou inativo.");

            if (produto.Estoque < request.Quantidade)
                throw new InvalidOperationException($"Estoque insuficiente. Disponível: {produto.Estoque}");

            var itemExistente = carrinho.Itens.FirstOrDefault(i => i.ProdutoId == request.ProdutoId);
            if (itemExistente != null)
            {
                var novaQtd = itemExistente.Quantidade + request.Quantidade;
                if (produto.Estoque < novaQtd)
                    throw new InvalidOperationException($"Estoque insuficiente para a quantidade total. Disponível: {produto.Estoque}");
                itemExistente.Quantidade = novaQtd;
                itemExistente.Subtotal = itemExistente.Quantidade * itemExistente.PrecoUnitario;
            }
            else
            {
                carrinho.Itens.Add(new ItemCarrinho
                {
                    ProdutoId = produto.ProdutoId,
                    ProdutoNome = produto.Nome,
                    ProdutoImagem = produto.ImagemPrincipal,
                    Sku = produto.Sku,
                    Quantidade = request.Quantidade,
                    PrecoUnitario = produto.PrecoPromocional > 0 ? produto.PrecoPromocional : produto.Preco,
                    PrecoOriginal = produto.Preco,
                    Subtotal = request.Quantidade * (produto.PrecoPromocional > 0 ? produto.PrecoPromocional : produto.Preco),
                    LojaId = produto.LojaId,
                    LojaNome = produto.Loja?.Nome,
                    EstoqueDisponivel = produto.Estoque
                });
            }

            carrinho.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("CARRINHO", $"Item adicionado: Produto {produto.ProdutoId}, Qtd {request.Quantidade}", clienteId?.ToString() ?? sessaoId);

            return MapearParaDto(carrinho);
        }

        public async Task<CarrinhoDto> RemoverItemAsync(string sessaoId, int itemId)
        {
            var carrinho = await BuscarCarrinho(sessaoId);
            if (carrinho == null) throw new ArgumentException("Carrinho não encontrado.");

            var item = carrinho.Itens.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null) throw new ArgumentException("Item não encontrado no carrinho.");

            carrinho.Itens.Remove(item);
            carrinho.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapearParaDto(carrinho);
        }

        public async Task<CarrinhoDto> AtualizarQuantidadeAsync(string sessaoId, int itemId, int quantidade)
        {
            var carrinho = await BuscarCarrinho(sessaoId);
            if (carrinho == null) throw new ArgumentException("Carrinho não encontrado.");

            var item = carrinho.Itens.FirstOrDefault(i => i.ItemId == itemId);
            if (item == null) throw new ArgumentException("Item não encontrado.");

            var produto = await _context.Produtos.FindAsync(item.ProdutoId);
            if (produto.Estoque < quantidade)
                throw new InvalidOperationException($"Estoque insuficiente. Disponível: {produto.Estoque}");

            item.Quantidade = quantidade;
            item.Subtotal = quantidade * item.PrecoUnitario;
            carrinho.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapearParaDto(carrinho);
        }

        public async Task<CarrinhoDto> AplicarCupomAsync(string sessaoId, string codigoCupom)
        {
            var carrinho = await BuscarCarrinho(sessaoId);
            if (carrinho == null) throw new ArgumentException("Carrinho não encontrado.");

            var cupom = await _context.Cupons.FirstOrDefaultAsync(c =>
                c.Codigo == codigoCupom.ToUpper() &&
                c.Ativo &&
                c.DataInicio <= DateTime.UtcNow &&
                c.DataFim >= DateTime.UtcNow &&
                (c.QuantidadeMaxima == null || c.QuantidadeUsada < c.QuantidadeMaxima));

            if (cupom == null)
                throw new ArgumentException("Cupom inválido, expirado ou esgotado.");

            var subtotal = carrinho.Itens.Sum(i => i.Subtotal);
            if (cupom.ValorMinimoPedido > 0 && subtotal < cupom.ValorMinimoPedido)
                throw new InvalidOperationException($"Valor mínimo para este cupom: R$ {cupom.ValorMinimoPedido:N2}");

            carrinho.CupomId = cupom.CupomId;
            carrinho.CupomCodigo = cupom.Codigo;
            carrinho.Desconto = cupom.Tipo == "PERCENTUAL"
                ? subtotal * (cupom.Valor / 100)
                : cupom.Valor;

            if (carrinho.Desconto > subtotal) carrinho.Desconto = subtotal;
            carrinho.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapearParaDto(carrinho);
        }

        public async Task<CarrinhoDto> RemoverCupomAsync(string sessaoId)
        {
            var carrinho = await BuscarCarrinho(sessaoId);
            if (carrinho == null) throw new ArgumentException("Carrinho não encontrado.");

            carrinho.CupomId = null;
            carrinho.CupomCodigo = null;
            carrinho.Desconto = 0;
            carrinho.AtualizadoEm = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapearParaDto(carrinho);
        }

        public async Task<bool> MigrarCarrinhoSessaoParaClienteAsync(string sessaoId, int clienteId)
        {
            var carrinhoSessao = await BuscarCarrinho(sessaoId);
            if (carrinhoSessao == null || !carrinhoSessao.Itens.Any()) return false;

            var carrinhoCliente = await _context.Carrinhos
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

            if (carrinhoCliente == null)
            {
                carrinhoSessao.ClienteId = clienteId;
                carrinhoSessao.SessaoId = null;
            }
            else
            {
                foreach (var item in carrinhoSessao.Itens)
                {
                    var existente = carrinhoCliente.Itens.FirstOrDefault(i => i.ProdutoId == item.ProdutoId);
                    if (existente != null)
                    {
                        existente.Quantidade += item.Quantidade;
                        existente.Subtotal = existente.Quantidade * existente.PrecoUnitario;
                    }
                    else
                    {
                        carrinhoCliente.Itens.Add(item);
                    }
                }
                carrinhoCliente.AtualizadoEm = DateTime.UtcNow;
                _context.Carrinhos.Remove(carrinhoSessao);
            }

            await _context.SaveChangesAsync();
            await _auditoria.RegistrarAsync("CARRINHO", $"Carrinho migrado sessão→cliente {clienteId}", clienteId.ToString());
            return true;
        }

        public async Task LimparCarrinhoAsync(string sessaoId)
        {
            var carrinho = await BuscarCarrinho(sessaoId);
            if (carrinho != null)
            {
                carrinho.Itens.Clear();
                carrinho.Desconto = 0;
                carrinho.CupomId = null;
                carrinho.CupomCodigo = null;
                carrinho.AtualizadoEm = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<ResumoCarrinhoDto> ObterResumoAsync(string sessaoId)
        {
            var carrinho = await BuscarCarrinho(sessaoId);
            if (carrinho == null) return new ResumoCarrinhoDto();

            var subtotal = carrinho.Itens.Sum(i => i.Subtotal);
            return new ResumoCarrinhoDto
            {
                Subtotal = subtotal,
                DescontoCupom = carrinho.Desconto,
                Frete = 0,
                Total = subtotal - carrinho.Desconto,
                CupomAplicado = carrinho.CupomCodigo,
                TotalItens = carrinho.Itens.Sum(i => i.Quantidade)
            };
        }

        // ===== Métodos privados =====
        private async Task<Carrinho> BuscarOuCriarCarrinho(string sessaoId, int? clienteId)
        {
            Carrinho carrinho = null;

            if (clienteId.HasValue)
                carrinho = await _context.Carrinhos
                    .Include(c => c.Itens)
                    .FirstOrDefaultAsync(c => c.ClienteId == clienteId.Value);

            if (carrinho == null && !string.IsNullOrEmpty(sessaoId))
                carrinho = await _context.Carrinhos
                    .Include(c => c.Itens)
                    .FirstOrDefaultAsync(c => c.SessaoId == sessaoId);

            if (carrinho == null)
            {
                carrinho = new Carrinho
                {
                    SessaoId = clienteId.HasValue ? null : sessaoId,
                    ClienteId = clienteId,
                    CriadoEm = DateTime.UtcNow,
                    AtualizadoEm = DateTime.UtcNow
                };
                _context.Carrinhos.Add(carrinho);
                await _context.SaveChangesAsync();
            }

            return carrinho;
        }

        private async Task<Carrinho> BuscarCarrinho(string sessaoId)
        {
            return await _context.Carrinhos
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.SessaoId == sessaoId || (c.ClienteId.HasValue && c.SessaoId == sessaoId));
        }

        private CarrinhoDto MapearParaDto(Carrinho carrinho)
        {
            var subtotal = carrinho.Itens.Sum(i => i.Subtotal);
            return new CarrinhoDto
            {
                CarrinhoId = carrinho.CarrinhoId,
                ClienteId = carrinho.ClienteId,
                SessaoId = carrinho.SessaoId,
                Subtotal = subtotal,
                Desconto = carrinho.Desconto,
                Total = subtotal - carrinho.Desconto,
                QuantidadeItens = carrinho.Itens.Sum(i => i.Quantidade),
                CriadoEm = carrinho.CriadoEm,
                AtualizadoEm = carrinho.AtualizadoEm,
                Itens = carrinho.Itens.Select(i => new ItemCarrinhoDto
                {
                    ItemId = i.ItemId,
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.ProdutoNome,
                    ProdutoImagem = i.ProdutoImagem,
                    Sku = i.Sku,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    PrecoOriginal = i.PrecoOriginal,
                    Subtotal = i.Subtotal,
                    LojaId = i.LojaId,
                    LojaNome = i.LojaNome,
                    EstoqueDisponivel = i.EstoqueDisponivel
                }).ToList()
            };
        }
    }
}
