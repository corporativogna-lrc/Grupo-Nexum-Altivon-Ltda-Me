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
    public interface ICheckoutService
    {
        Task<CheckoutDto> IniciarCheckoutAsync(int clienteId, IniciarCheckoutRequest request);
        Task<CheckoutDto> SelecionarFreteAsync(int checkoutId, string codigoFrete);
        Task<CheckoutResponseDto> FinalizarAsync(int clienteId, FinalizarCheckoutRequest request);
        Task<CheckoutDto> ObterCheckoutAsync(int checkoutId);
    }

    public class CheckoutService : ICheckoutService
    {
        private readonly NexumDbContext _context;
        private readonly IFreteService _frete;
        private readonly IMercadoPagoService _mp;
        private readonly INotificacaoService _notificacao;
        private readonly ILogAuditoriaService _auditoria;
        private readonly IPedidoService _pedidoService;

        public CheckoutService(
            NexumDbContext context,
            IFreteService frete,
            IMercadoPagoService mp,
            INotificacaoService notificacao,
            ILogAuditoriaService auditoria,
            IPedidoService pedidoService)
        {
            _context = context;
            _frete = frete;
            _mp = mp;
            _notificacao = notificacao;
            _auditoria = auditoria;
            _pedidoService = pedidoService;
        }

        public async Task<CheckoutDto> IniciarCheckoutAsync(int clienteId, IniciarCheckoutRequest request)
        {
            var cliente = await _context.Clientes.FindAsync(clienteId);
            if (cliente == null) throw new ArgumentException("Cliente não encontrado.");

            var endereco = await _context.Enderecos.FirstOrDefaultAsync(e => e.EnderecoId == request.EnderecoId && e.ClienteId == clienteId);
            if (endereco == null) throw new ArgumentException("Endereço não encontrado.");

            var carrinho = await _context.Carrinhos
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

            if (carrinho == null || !carrinho.Itens.Any())
                throw new InvalidOperationException("Carrinho vazio.");

            // Validar estoque
            foreach (var item in carrinho.Itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                if (produto.Estoque < item.Quantidade)
                    throw new InvalidOperationException($"Estoque insuficiente para '{item.ProdutoNome}'. Disponível: {produto.Estoque}");
            }

            var checkout = new Checkout
            {
                ClienteId = clienteId,
                EnderecoId = endereco.EnderecoId,
                CupomId = request.CupomId,
                Observacoes = request.Observacoes,
                Status = "ABERTO",
                CriadoEm = DateTime.UtcNow
            };

            foreach (var item in carrinho.Itens)
            {
                checkout.Itens.Add(new CheckoutItem
                {
                    ProdutoId = item.ProdutoId,
                    ProdutoNome = item.ProdutoNome,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    Subtotal = item.Subtotal,
                    LojaId = item.LojaId
                });
            }

            // Calcular desconto do cupom
            decimal desconto = 0;
            if (request.CupomId.HasValue)
            {
                var cupom = await _context.Cupons.FindAsync(request.CupomId.Value);
                var subtotal = checkout.Itens.Sum(i => i.Subtotal);
                if (cupom != null && cupom.Ativo && cupom.DataInicio <= DateTime.UtcNow && cupom.DataFim >= DateTime.UtcNow)
                {
                    desconto = cupom.Tipo == "PERCENTUAL" ? subtotal * (cupom.Valor / 100) : cupom.Valor;
                    if (desconto > subtotal) desconto = subtotal;
                    checkout.CupomCodigo = cupom.Codigo;
                }
            }
            checkout.Desconto = desconto;
            checkout.Subtotal = checkout.Itens.Sum(i => i.Subtotal);

            // Calcular frete
            var opcoesFrete = await _frete.CalcularFreteAsync(endereco.Cep, checkout.Itens);
            checkout.OpcoesFrete = opcoesFrete;
            checkout.Frete = opcoesFrete.FirstOrDefault()?.Valor ?? 0;
            checkout.CodigoFreteSelecionado = opcoesFrete.FirstOrDefault()?.Codigo;

            checkout.Total = checkout.Subtotal - checkout.Desconto + checkout.Frete;

            _context.Checkouts.Add(checkout);
            await _context.SaveChangesAsync();

            await _auditoria.RegistrarAsync("CHECKOUT", $"Checkout {checkout.CheckoutId} iniciado pelo cliente {clienteId}", clienteId.ToString());

            return MapearCheckoutDto(checkout, endereco, opcoesFrete);
        }

        public async Task<CheckoutDto> SelecionarFreteAsync(int checkoutId, string codigoFrete)
        {
            var checkout = await _context.Checkouts
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.CheckoutId == checkoutId);

            if (checkout == null) throw new ArgumentException("Checkout não encontrado.");
            if (checkout.Status != "ABERTO") throw new InvalidOperationException("Checkout não pode mais ser alterado.");

            var opcao = checkout.OpcoesFrete?.FirstOrDefault(o => o.Codigo == codigoFrete);
            if (opcao == null)
            {
                // Recalcular se necessário
                var endereco = await _context.Enderecos.FindAsync(checkout.EnderecoId);
                var opcoes = await _frete.CalcularFreteAsync(endereco.Cep, checkout.Itens);
                opcao = opcoes.FirstOrDefault(o => o.Codigo == codigoFrete);
                if (opcao == null) throw new ArgumentException("Opção de frete inválida.");
            }

            checkout.CodigoFreteSelecionado = opcao.Codigo;
            checkout.Frete = opcao.Valor;
            checkout.Total = checkout.Subtotal - checkout.Desconto + checkout.Frete;
            checkout.AtualizadoEm = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return MapearCheckoutDto(checkout, await _context.Enderecos.FindAsync(checkout.EnderecoId), checkout.OpcoesFrete);
        }

        public async Task<CheckoutResponseDto> FinalizarAsync(int clienteId, FinalizarCheckoutRequest request)
        {
            var checkout = await _context.Checkouts
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.CheckoutId == request.CheckoutId && c.ClienteId == clienteId);

            if (checkout == null) throw new ArgumentException("Checkout não encontrado.");
            if (checkout.Status != "ABERTO") throw new InvalidOperationException("Checkout já finalizado ou cancelado.");

            // Validar estoque novamente
            foreach (var item in checkout.Itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                if (produto.Estoque < item.Quantidade)
                    throw new InvalidOperationException($"Estoque insuficiente para '{item.ProdutoNome}'.");
            }

            // Gerar pedido
            var pedido = await _pedidoService.GerarPedidoDoCheckoutAsync(checkout);

            // Processar pagamento
            PagamentoResultado resultadoPagamento = null;
            switch (request.MetodoPagamento.ToUpper())
            {
                case "PIX":
                    resultadoPagamento = await _mp.GerarPagamentoPixAsync(pedido);
                    break;
                case "CARTAOCREDITO":
                    if (request.DadosCartao == null)
                        throw new ArgumentException("Dados do cartão obrigatórios.");
                    resultadoPagamento = await _mp.GerarPagamentoCartaoAsync(pedido, request.DadosCartao, request.Parcelas ?? 1);
                    break;
                case "BOLETO":
                    resultadoPagamento = await _mp.GerarPagamentoBoletoAsync(pedido);
                    break;
                default:
                    throw new ArgumentException("Método de pagamento não suportado.");
            }

            if (resultadoPagamento == null || !resultadoPagamento.Sucesso)
            {
                pedido.Status = "PAGAMENTO_FALHOU";
                await _context.SaveChangesAsync();
                return new CheckoutResponseDto
                {
                    Sucesso = false,
                    Mensagem = resultadoPagamento?.Mensagem ?? "Falha ao processar pagamento.",
                    NumeroPedido = pedido.NumeroPedido,
                    PedidoId = pedido.PedidoId
                };
            }

            // Atualizar pedido com dados do pagamento
            pedido.Status = "AGUARDANDO_PAGAMENTO";
            pedido.MetodoPagamento = request.MetodoPagamento;
            pedido.TransacaoGatewayId = resultadoPagamento.TransacaoId;
            checkout.Status = "FINALIZADO";
            checkout.AtualizadoEm = DateTime.UtcNow;

            // Deduzir estoque
            foreach (var item in checkout.Itens)
            {
                var produto = await _context.Produtos.FindAsync(item.ProdutoId);
                produto.Estoque -= item.Quantidade;
                if (produto.Estoque <= produto.EstoqueMinimo)
                    await _notificacao.EnviarAlertaEstoqueBaixoAsync(produto);
            }

            // Limpar carrinho
            var carrinho = await _context.Carrinhos.FirstOrDefaultAsync(c => c.ClienteId == clienteId);
            if (carrinho != null)
            {
                carrinho.Itens.Clear();
                carrinho.Desconto = 0;
                carrinho.CupomId = null;
                carrinho.CupomCodigo = null;
            }

            // Incrementar uso do cupom
            if (checkout.CupomId.HasValue)
            {
                var cupom = await _context.Cupons.FindAsync(checkout.CupomId.Value);
                if (cupom != null) cupom.QuantidadeUsada++;
            }

            await _context.SaveChangesAsync();

            // Notificações
            var cliente = await _context.Clientes.FindAsync(clienteId);
            await _notificacao.EnviarConfirmacaoPedidoAsync(cliente, pedido);
            await _notificacao.EnviarNotificacaoWhatsAppAsync(cliente.Telefone,
                $"Ola {cliente.Nome}, seu pedido {pedido.NumeroPedido} foi recebido! Total: R$ {pedido.Total:N2}. Aguardando pagamento.");

            await _auditoria.RegistrarAsync("CHECKOUT", $"Pedido {pedido.NumeroPedido} finalizado. Pagamento: {request.MetodoPagamento}", clienteId.ToString());

            return new CheckoutResponseDto
            {
                Sucesso = true,
                NumeroPedido = pedido.NumeroPedido,
                PedidoId = pedido.PedidoId,
                StatusPagamento = pedido.Status,
                UrlPagamento = resultadoPagamento.UrlPagamento,
                QrCodeBase64 = resultadoPagamento.QrCodeBase64,
                QrCodeTexto = resultadoPagamento.QrCodeTexto,
                LinhaDigitavel = resultadoPagamento.LinhaDigitavel,
                Mensagem = "Pedido gerado com sucesso. Aguardando confirmação de pagamento."
            };
        }

        public async Task<CheckoutDto> ObterCheckoutAsync(int checkoutId)
        {
            var checkout = await _context.Checkouts
                .Include(c => c.Itens)
                .FirstOrDefaultAsync(c => c.CheckoutId == checkoutId);

            if (checkout == null) return null;
            var endereco = await _context.Enderecos.FindAsync(checkout.EnderecoId);
            return MapearCheckoutDto(checkout, endereco, checkout.OpcoesFrete);
        }

        private CheckoutDto MapearCheckoutDto(Checkout c, Endereco end, List<OpcaoFreteDto> freteOpts)
        {
            return new CheckoutDto
            {
                CheckoutId = c.CheckoutId,
                NumeroPedido = c.NumeroPedido,
                ClienteId = c.ClienteId,
                ClienteNome = c.Cliente?.Nome,
                EnderecoEntrega = end == null ? null : new EnderecoDto
                {
                    EnderecoId = end.EnderecoId,
                    Apelido = end.Apelido,
                    Destinatario = end.Destinatario,
                    Cep = end.Cep,
                    Logradouro = end.Logradouro,
                    Numero = end.Numero,
                    Complemento = end.Complemento,
                    Bairro = end.Bairro,
                    Cidade = end.Cidade,
                    Estado = end.Estado,
                    Telefone = end.Telefone,
                    Principal = end.Principal
                },
                Itens = c.Itens.Select(i => new ItemCarrinhoDto
                {
                    ProdutoId = i.ProdutoId,
                    ProdutoNome = i.ProdutoNome,
                    Quantidade = i.Quantidade,
                    PrecoUnitario = i.PrecoUnitario,
                    Subtotal = i.Subtotal,
                    LojaId = i.LojaId
                }).ToList(),
                Resumo = new ResumoCheckoutDto
                {
                    Subtotal = c.Subtotal,
                    Desconto = c.Desconto,
                    Frete = c.Frete,
                    Total = c.Total,
                    TotalItens = c.Itens.Sum(i => i.Quantidade),
                    CupomAplicado = c.CupomCodigo
                },
                OpcoesFrete = freteOpts ?? new List<OpcaoFreteDto>(),
                Status = c.Status,
                CriadoEm = c.CriadoEm
            };
        }
    }
}
