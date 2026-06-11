using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.ERP.Data;
using NexumAltivon.ERP.DTOs;
using NexumAltivon.ERP.Models;

namespace NexumAltivon.ERP.Services
{
    /// <summary>
    /// Serviço de estoque avançado — movimentações, inventário, kardex e locais
    /// </summary>
    public interface IEstoqueService
    {
        Task<MovimentacaoEstoque> RegistrarEntradaAsync(int produtoId, decimal quantidade, decimal custoUnitario, int? fornecedorId, string documento, string motivo, int? lojaId, string usuario);
        Task<MovimentacaoEstoque> RegistrarSaidaAsync(int produtoId, decimal quantidade, int? pedidoId, string documento, string motivo, int? lojaId, string usuario);
        Task<MovimentacaoEstoque> RegistrarMovimentacaoAsync(CriarMovimentacaoEstoqueDto dto, string usuario);
        Task<MovimentacaoEstoque> RegistrarTransferenciaAsync(int produtoId, decimal quantidade, int origemLojaId, int destinoLojaId, string documento, string usuario);
        Task<MovimentacaoEstoque> RegistrarAjusteAsync(int produtoId, decimal quantidadeSistema, decimal quantidadeContada, string motivo, int? lojaId, string usuario);
        Task<IEnumerable<MovimentacaoEstoque>> ListarMovimentacoesAsync(int? produtoId = null, string? tipo = null, DateTime? inicio = null, DateTime? fim = null);
        Task<IEnumerable<Kardex>> ObterKardexAsync(int produtoId, DateTime? inicio = null, DateTime? fim = null);
        Task<Inventario> CriarInventarioAsync(string codigo, string descricao, int? lojaId, string usuario);
        Task<Inventario> CriarInventarioAsync(string descricao, int? lojaId, string usuario);
        Task<ItemInventario> RegistrarItemInventarioAsync(int inventarioId, int produtoId, decimal quantidadeSistema, decimal quantidadeContada, string? observacoes);
        Task<bool> RegistrarContagemInventarioAsync(int inventarioId, int produtoId, decimal quantidadeContada, string? observacoes);
        Task<Inventario> FinalizarInventarioAsync(int inventarioId, string usuario);
        Task AjustarEstoquePorInventarioAsync(int inventarioId, string usuario);
        Task<IEnumerable<Inventario>> ListarInventariosAsync(string? status = null);
    }

    public class EstoqueService : IEstoqueService
    {
        private readonly NexumDbContext _context;

        public EstoqueService(NexumDbContext context)
        {
            _context = context;
        }

        public async Task<MovimentacaoEstoque> RegistrarEntradaAsync(int produtoId, decimal quantidade, decimal custoUnitario, int? fornecedorId, string documento, string motivo, int? lojaId, string usuario)
        {
            var produto = await _context.Produtos.FindAsync(produtoId)
                ?? throw new Exception("Produto não encontrado.");

            var movimentacao = new MovimentacaoEstoque
            {
                ProdutoId = produtoId,
                Tipo = "Entrada",
                Quantidade = quantidade,
                CustoUnitario = custoUnitario,
                CustoTotal = quantidade * custoUnitario,
                Motivo = motivo,
                DocumentoReferencia = documento,
                FornecedorId = fornecedorId,
                DestinoLojaId = lojaId,
                DataMovimentacao = DateTime.Now,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            // Atualiza estoque do produto
            produto.EstoqueAtual += quantidade;
            produto.CustoMedio = produto.EstoqueAtual > 0
                ? ((produto.CustoMedio * (produto.EstoqueAtual - quantidade)) + (quantidade * custoUnitario)) / produto.EstoqueAtual
                : custoUnitario;
            produto.UltimaAtualizacao = DateTime.Now;

            _context.MovimentacoesEstoque.Add(movimentacao);
            await _context.SaveChangesAsync();

            // Registra no kardex
            await RegistrarKardexAsync(produtoId, "Entrada", quantidade, produto.EstoqueAtual, custoUnitario, produto.CustoMedio, documento, usuario);

            return movimentacao;
        }

        public async Task<MovimentacaoEstoque> RegistrarSaidaAsync(int produtoId, decimal quantidade, int? pedidoId, string documento, string motivo, int? lojaId, string usuario)
        {
            var produto = await _context.Produtos.FindAsync(produtoId)
                ?? throw new Exception("Produto não encontrado.");

            if (produto.EstoqueAtual < quantidade)
                throw new Exception($"Estoque insuficiente. Disponível: {produto.EstoqueAtual}, Solicitado: {quantidade}");

            var movimentacao = new MovimentacaoEstoque
            {
                ProdutoId = produtoId,
                Tipo = "Saida",
                Quantidade = quantidade,
                CustoUnitario = produto.CustoMedio,
                CustoTotal = quantidade * produto.CustoMedio,
                Motivo = motivo,
                DocumentoReferencia = documento,
                PedidoId = pedidoId,
                OrigemLojaId = lojaId,
                DataMovimentacao = DateTime.Now,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            produto.EstoqueAtual -= quantidade;
            produto.UltimaAtualizacao = DateTime.Now;

            _context.MovimentacoesEstoque.Add(movimentacao);
            await _context.SaveChangesAsync();

            await RegistrarKardexAsync(produtoId, "Saida", quantidade, produto.EstoqueAtual, produto.CustoMedio, produto.CustoMedio, documento, usuario);

            return movimentacao;
        }

        public Task<MovimentacaoEstoque> RegistrarMovimentacaoAsync(CriarMovimentacaoEstoqueDto dto, string usuario)
        {
            return dto.Tipo.Equals("Saida", StringComparison.OrdinalIgnoreCase)
                ? RegistrarSaidaAsync(dto.ProdutoId, dto.Quantidade, dto.PedidoId, dto.DocumentoReferencia, dto.Motivo, dto.OrigemLojaId, usuario)
                : RegistrarEntradaAsync(dto.ProdutoId, dto.Quantidade, dto.CustoUnitario ?? 0, dto.FornecedorId, dto.DocumentoReferencia, dto.Motivo, dto.DestinoLojaId, usuario);
        }

        public async Task<MovimentacaoEstoque> RegistrarTransferenciaAsync(int produtoId, decimal quantidade, int origemLojaId, int destinoLojaId, string documento, string usuario)
        {
            // Saída da origem
            var saida = await RegistrarSaidaAsync(produtoId, quantidade, null, documento, "Transferência entre lojas", origemLojaId, usuario);

            // Entrada no destino
            var produto = await _context.Produtos.FindAsync(produtoId);
            var entrada = await RegistrarEntradaAsync(produtoId, quantidade, produto?.CustoMedio ?? 0, null, documento, "Transferência entre lojas", destinoLojaId, usuario);

            // Cria movimentação de transferência
            var transferencia = new MovimentacaoEstoque
            {
                ProdutoId = produtoId,
                Tipo = "Transferencia",
                Quantidade = quantidade,
                Motivo = "Transferência entre lojas",
                DocumentoReferencia = documento,
                OrigemLojaId = origemLojaId,
                DestinoLojaId = destinoLojaId,
                DataMovimentacao = DateTime.Now,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            _context.MovimentacoesEstoque.Add(transferencia);
            await _context.SaveChangesAsync();

            return transferencia;
        }

        public async Task<MovimentacaoEstoque> RegistrarAjusteAsync(int produtoId, decimal quantidadeSistema, decimal quantidadeContada, string motivo, int? lojaId, string usuario)
        {
            var produto = await _context.Produtos.FindAsync(produtoId)
                ?? throw new Exception("Produto não encontrado.");

            var diferenca = quantidadeContada - quantidadeSistema;
            var tipo = diferenca >= 0 ? "Entrada" : "Saida";

            var movimentacao = new MovimentacaoEstoque
            {
                ProdutoId = produtoId,
                Tipo = "Ajuste",
                Quantidade = Math.Abs(diferenca),
                CustoUnitario = produto.CustoMedio,
                CustoTotal = Math.Abs(diferenca) * produto.CustoMedio,
                Motivo = $"Ajuste de inventário: {motivo}",
                DocumentoReferencia = "AJUSTE-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                OrigemLojaId = lojaId,
                DataMovimentacao = DateTime.Now,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            produto.EstoqueAtual = quantidadeContada;
            produto.UltimaAtualizacao = DateTime.Now;

            _context.MovimentacoesEstoque.Add(movimentacao);
            await _context.SaveChangesAsync();

            await RegistrarKardexAsync(produtoId, tipo, Math.Abs(diferenca), produto.EstoqueAtual, produto.CustoMedio, produto.CustoMedio, movimentacao.DocumentoReferencia, usuario);

            return movimentacao;
        }

        public async Task<IEnumerable<MovimentacaoEstoque>> ListarMovimentacoesAsync(int? produtoId = null, string? tipo = null, DateTime? inicio = null, DateTime? fim = null)
        {
            var query = _context.MovimentacoesEstoque
                .Include(m => m.Produto)
                .AsQueryable();

            if (produtoId.HasValue)
                query = query.Where(m => m.ProdutoId == produtoId.Value);

            if (!string.IsNullOrEmpty(tipo))
                query = query.Where(m => m.Tipo == tipo);

            if (inicio.HasValue)
                query = query.Where(m => m.DataMovimentacao >= inicio.Value);

            if (fim.HasValue)
                query = query.Where(m => m.DataMovimentacao <= fim.Value);

            return await query.OrderByDescending(m => m.DataMovimentacao).ToListAsync();
        }

        public async Task<IEnumerable<Kardex>> ObterKardexAsync(int produtoId, DateTime? inicio = null, DateTime? fim = null)
        {
            var query = _context.Kardex.Where(k => k.ProdutoId == produtoId).AsQueryable();

            if (inicio.HasValue)
                query = query.Where(k => k.Data >= inicio.Value);

            if (fim.HasValue)
                query = query.Where(k => k.Data <= fim.Value);

            return await query.OrderByDescending(k => k.Data).ToListAsync();
        }

        public async Task<Inventario> CriarInventarioAsync(string codigo, string descricao, int? lojaId, string usuario)
        {
            var inventario = new Inventario
            {
                Codigo = codigo,
                Descricao = descricao,
                LojaId = lojaId,
                Status = "Aberto",
                DataInicio = DateTime.Now,
                CriadoEm = DateTime.Now,
                CriadoPor = usuario
            };

            _context.Inventarios.Add(inventario);
            await _context.SaveChangesAsync();

            return inventario;
        }

        public Task<Inventario> CriarInventarioAsync(string descricao, int? lojaId, string usuario)
        {
            var codigo = "INV-" + DateTime.Now.ToString("yyyyMMddHHmmss");
            return CriarInventarioAsync(codigo, descricao, lojaId, usuario);
        }

        public async Task<ItemInventario> RegistrarItemInventarioAsync(int inventarioId, int produtoId, decimal quantidadeSistema, decimal quantidadeContada, string? observacoes)
        {
            var produto = await _context.Produtos.FindAsync(produtoId)
                ?? throw new Exception("Produto não encontrado.");

            var diferenca = quantidadeContada - quantidadeSistema;
            var valorDiferenca = diferenca * produto.CustoMedio;

            var item = new ItemInventario
            {
                InventarioId = inventarioId,
                ProdutoId = produtoId,
                QuantidadeSistema = quantidadeSistema,
                QuantidadeContada = quantidadeContada,
                CustoUnitario = produto.CustoMedio,
                ValorDiferenca = valorDiferenca,
                Observacoes = observacoes,
                CriadoEm = DateTime.Now
            };

            _context.ItensInventario.Add(item);
            await _context.SaveChangesAsync();

            return item;
        }

        public async Task<bool> RegistrarContagemInventarioAsync(int inventarioId, int produtoId, decimal quantidadeContada, string? observacoes)
        {
            var produto = await _context.Produtos.FindAsync(produtoId);
            if (produto == null) return false;

            await RegistrarItemInventarioAsync(inventarioId, produtoId, produto.EstoqueAtual, quantidadeContada, observacoes);
            return true;
        }

        public async Task<Inventario> FinalizarInventarioAsync(int inventarioId, string usuario)
        {
            var inventario = await _context.Inventarios
                .Include(i => i.Itens)
                .FirstOrDefaultAsync(i => i.Id == inventarioId)
                ?? throw new Exception("Inventário não encontrado.");

            if (inventario.Status != "Aberto" && inventario.Status != "EmAndamento")
                throw new Exception("Inventário não pode ser finalizado.");

            // Aplica ajustes automáticos para itens com diferença
            foreach (var item in inventario.Itens ?? new List<ItemInventario>())
            {
                if (item.Diferenca != 0)
                {
                    await RegistrarAjusteAsync(
                        item.ProdutoId,
                        item.QuantidadeSistema,
                        item.QuantidadeContada,
                        $"Ajuste via inventário {inventario.Codigo}",
                        inventario.LojaId,
                        usuario
                    );
                }
            }

            inventario.Status = "Finalizado";
            inventario.DataFim = DateTime.Now;

            await _context.SaveChangesAsync();
            return inventario;
        }

        public async Task AjustarEstoquePorInventarioAsync(int inventarioId, string usuario)
        {
            await FinalizarInventarioAsync(inventarioId, usuario);
        }

        public async Task<IEnumerable<Inventario>> ListarInventariosAsync(string? status = null)
        {
            var query = _context.Inventarios.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(i => i.Status == status);

            return await query.OrderByDescending(i => i.CriadoEm).ToListAsync();
        }

        private async Task RegistrarKardexAsync(int produtoId, string tipo, decimal quantidade, decimal saldo, decimal? custoUnitario, decimal? custoMedio, string documento, string usuario)
        {
            var kardex = new Kardex
            {
                ProdutoId = produtoId,
                Data = DateTime.Now,
                Tipo = tipo,
                Quantidade = quantidade,
                Saldo = saldo,
                CustoUnitario = custoUnitario,
                CustoMedio = custoMedio,
                Documento = documento,
                Observacoes = $"Registrado por {usuario}",
                CriadoEm = DateTime.Now
            };

            _context.Kardex.Add(kardex);
            await _context.SaveChangesAsync();
        }
    }
}
