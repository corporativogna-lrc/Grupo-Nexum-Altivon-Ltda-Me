/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.ERP.SharedData;

public static class GenesisPdvService
{
    public static async Task<List<GenesisPdvVendaDto>> ListarVendasRecentesAsync(GenesisDbContext genesisDb, int limite, CancellationToken ct)
    {
        var connection = genesisDb.Database.GetDbConnection();
        var closeAfterUse = connection.State != ConnectionState.Open;
        if (closeAfterUse)
        {
            await connection.OpenAsync(ct);
        }

        try
        {
            var vendas = new List<GenesisPdvVendaSnapshot>();
            await using (var command = CreateCommand(
                connection,
                null,
                """
                SELECT
                    p.pdv_id,
                    p.pdv_numero,
                    p.pdv_emp_id,
                    p.pdv_pes_id,
                    COALESCE(pe.pes_nome_razao, 'Cliente nao identificado') AS cliente_nome,
                    pe.pes_cpf_cnpj,
                    pe.pes_email,
                    p.pdv_valor_produtos,
                    p.pdv_valor_desconto,
                    p.pdv_valor_frete,
                    p.pdv_valor_total,
                    p.pdv_forma_pagamento,
                    p.pdv_status,
                    p.pdv_observacoes,
                    p.pdv_data_cadastro
                FROM vnd_pedidos p
                LEFT JOIN adm_pessoas_empresas pe ON pe.pes_id = p.pdv_pes_id
                ORDER BY p.pdv_data_cadastro DESC, p.pdv_id DESC
                LIMIT @limite;
                """,
                ("@limite", Math.Clamp(limite, 1, 100))))
            {
                await using var reader = await command.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    vendas.Add(ReadVenda(reader));
                }
            }

            foreach (var venda in vendas)
            {
                var itens = await ListarItensAsync(connection, null, venda.Id, ct);
                var pagamentos = new List<GenesisPdvPagamentoSnapshot>
                {
                    new(1, venda.FormaPagamento ?? "PDV", venda.Total, 1, null, null, null)
                };
                venda.Itens.AddRange(itens);
                venda.Pagamentos.AddRange(pagamentos);
            }

            return vendas.Select(ToDto).ToList();
        }
        finally
        {
            if (closeAfterUse)
            {
                await connection.CloseAsync();
            }
        }
    }

    public static async Task<GenesisPdvVendaDto> RegistrarVendaAsync(
        GenesisDbContext genesisDb,
        NexumDbContext nexumDb,
        GenesisPdvVendaRequest request,
        CancellationToken ct)
    {
        ValidarVenda(request);

        var subtotal = request.Itens.Sum(item => Math.Round(item.Quantidade * item.PrecoUnitario, 2, MidpointRounding.AwayFromZero));
        var desconto = Math.Max(0m, request.Desconto);
        var frete = Math.Max(0m, request.Frete);
        var total = Math.Max(0m, subtotal - desconto + frete);
        var valorPago = request.Pagamentos.Sum(item => item.Valor);
        var troco = Math.Max(0m, valorPago - total);
        var now = DateTime.UtcNow;
        var numero = TrimOrNull(request.Numero) ?? $"PDV{now:yyMMddHHmmssfff}";

        var connection = genesisDb.Database.GetDbConnection();
        var closeAfterUse = connection.State != ConnectionState.Open;
        if (closeAfterUse)
        {
            await connection.OpenAsync(ct);
        }

        GenesisPdvVendaSnapshot venda;
        await using var transaction = await connection.BeginTransactionAsync(ct);
        try
        {
            var empresaId = await ResolverEmpresaIdAsync(connection, transaction, request.EmpresaCodigo, ct);
            var usuarioId = await ResolverUsuarioIdAsync(connection, transaction, request.Operador, empresaId, ct);
            var clienteId = await ResolverClienteGenesisAsync(connection, transaction, request, ct);
            var tabelaPrecoId = await ResolverTabelaPrecoAsync(connection, transaction, empresaId, ct);
            var contaReceitaId = await ResolverContaReceitaAsync(connection, transaction, ct);
            var depositoId = await ResolverDepositoAsync(connection, transaction, empresaId, ct);
            var pedidoId = await NextIdAsync(connection, transaction, "vnd_pedidos", "pdv_id", ct);

            await ExecuteAsync(
                connection,
                transaction,
                """
                INSERT INTO vnd_pedidos
                    (pdv_id, pdv_emp_id, pdv_numero, pdv_pes_id, pdv_tpr_id, pdv_data_pedido,
                     pdv_data_entrega, pdv_valor_produtos, pdv_valor_desconto, pdv_valor_frete,
                     pdv_valor_total, pdv_tipo_frete, pdv_forma_pagamento, pdv_condicao_pagamento,
                     pdv_status, pdv_observacoes, pdv_usr_vendedor, pdv_usr_cadastro)
                VALUES
                    (@id, @empresaId, @numero, @clienteId, @tabelaPrecoId, @dataPedido,
                     @dataEntrega, @subtotal, @desconto, @frete,
                     @total, 'CIF', @formaPagamento, @condicaoPagamento,
                     @status, @observacoes, @usuarioId, @usuarioId);
                """,
                ct,
                ("@id", pedidoId),
                ("@empresaId", empresaId),
                ("@numero", numero),
                ("@clienteId", clienteId),
                ("@tabelaPrecoId", tabelaPrecoId),
                ("@dataPedido", now.Date),
                ("@dataEntrega", now.Date),
                ("@subtotal", subtotal),
                ("@desconto", desconto),
                ("@frete", frete),
                ("@total", total),
                ("@formaPagamento", request.Pagamentos.First().Forma.Trim()),
                ("@condicaoPagamento", $"{Math.Max(1, request.Pagamentos.Max(item => item.Parcelas))} parcela(s)"),
                ("@status", valorPago >= total ? "APROVADO" : "ORCAMENTO"),
                ("@observacoes", BuildObservacoes(request)),
                ("@usuarioId", usuarioId));

            var itens = new List<GenesisPdvVendaItemSnapshot>();
            var sequencia = 1;
            foreach (var item in request.Itens)
            {
                var itemId = await ResolverItemGenesisAsync(connection, transaction, item, empresaId, sequencia, ct);
                var itemTotal = Math.Round(item.Quantidade * item.PrecoUnitario - Math.Max(0m, item.Desconto), 2, MidpointRounding.AwayFromZero);
                var pedidoItemId = await NextIdAsync(connection, transaction, "vnd_pedido_itens", "pdi_id", ct);

                await ExecuteAsync(
                    connection,
                    transaction,
                    """
                    INSERT INTO vnd_pedido_itens
                        (pdi_id, pdi_pdv_id, pdi_itm_id, pdi_sequencia, pdi_descricao,
                         pdi_quantidade, pdi_preco_unitario, pdi_desconto_percentual,
                         pdi_desconto_valor, pdi_valor_total, pdi_pct_id, pdi_ccu_id)
                    VALUES
                        (@id, @pedidoId, @itemId, @sequencia, @descricao,
                         @quantidade, @precoUnitario, 0,
                         @desconto, @total, @contaReceitaId, NULL);
                    """,
                    ct,
                    ("@id", pedidoItemId),
                    ("@pedidoId", pedidoId),
                    ("@itemId", itemId),
                    ("@sequencia", sequencia),
                    ("@descricao", item.Descricao.Trim()),
                    ("@quantidade", item.Quantidade),
                    ("@precoUnitario", item.PrecoUnitario),
                    ("@desconto", Math.Max(0m, item.Desconto)),
                    ("@total", itemTotal),
                    ("@contaReceitaId", contaReceitaId));

                if (depositoId.HasValue)
                {
                    await RegistrarSaidaEstoqueAsync(connection, transaction, empresaId, itemId, depositoId.Value, pedidoId, item, itemTotal, usuarioId, ct);
                }

                itens.Add(new GenesisPdvVendaItemSnapshot(
                    pedidoItemId,
                    sequencia,
                    item.ProdutoNexumId,
                    TrimOrNull(item.ProdutoCodigo),
                    TrimOrNull(item.Sku),
                    item.Descricao.Trim(),
                    item.Quantidade,
                    item.PrecoUnitario,
                    Math.Max(0m, item.CustoUnitario),
                    Math.Max(0m, item.Desconto),
                    itemTotal,
                    TrimOrNull(item.OrigemAquisicao)));

                sequencia++;
            }

            var tituloId = await NextIdAsync(connection, transaction, "fin_titulos_receber", "trc_id", ct);
            await ExecuteAsync(
                connection,
                transaction,
                """
                INSERT INTO fin_titulos_receber
                    (trc_id, trc_emp_id, trc_numero, trc_tipo, trc_pes_id, trc_documento,
                     trc_parcela, trc_data_emissao, trc_data_vencimento, trc_data_recebimento,
                     trc_valor_original, trc_valor_juros, trc_valor_multa, trc_valor_desconto,
                     trc_valor_recebido, trc_pct_id, trc_cba_id, trc_status,
                     trc_observacoes, trc_usr_cadastro)
                VALUES
                    (@id, @empresaId, @numero, 'VENDA', @clienteId, @documento,
                     '01/01', @emissao, @vencimento, @recebimento,
                     @valorOriginal, 0, 0, @desconto,
                     @valorRecebido, @contaReceitaId, NULL, @status,
                     @observacoes, @usuarioId);
                """,
                ct,
                ("@id", tituloId),
                ("@empresaId", empresaId),
                ("@numero", numero),
                ("@clienteId", clienteId),
                ("@documento", numero),
                ("@emissao", now.Date),
                ("@vencimento", now.Date),
                ("@recebimento", valorPago >= total ? now.Date : null),
                ("@valorOriginal", total),
                ("@desconto", desconto),
                ("@valorRecebido", Math.Min(total, valorPago)),
                ("@contaReceitaId", contaReceitaId),
                ("@status", valorPago >= total ? "RECEBIDO" : "ABERTO"),
                ("@observacoes", "Titulo gerado automaticamente pela venda PDV Genesis/Nexum."),
                ("@usuarioId", usuarioId));

            await transaction.CommitAsync(ct);

            var pagamentos = request.Pagamentos
                .Select((pagamento, index) => new GenesisPdvPagamentoSnapshot(
                    index + 1,
                    pagamento.Forma.Trim(),
                    pagamento.Valor,
                    Math.Max(1, pagamento.Parcelas),
                    TrimOrNull(pagamento.Autorizacao),
                    TrimOrNull(pagamento.Nsu),
                    TrimOrNull(pagamento.Bandeira)))
                .ToList();

            venda = new GenesisPdvVendaSnapshot(
                pedidoId,
                numero,
                empresaId.ToString(),
                request.EmpresaNexumId,
                request.ClienteNome.Trim(),
                OnlyDigits(request.ClienteDocumento),
                TrimOrNull(request.ClienteEmail)?.ToLowerInvariant(),
                request.ClienteNexumId,
                null,
                null,
                TrimOrNull(request.Terminal),
                TrimOrNull(request.CaixaCodigo),
                TrimOrNull(request.Operador),
                subtotal,
                desconto,
                frete,
                total,
                valorPago,
                troco,
                valorPago >= total ? "FINALIZADA" : "PENDENTE",
                "GENESIS_REGISTRADO",
                now,
                request.Pagamentos.First().Forma.Trim(),
                BuildObservacoes(request));
            venda.Itens.AddRange(itens);
            venda.Pagamentos.AddRange(pagamentos);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
        finally
        {
            if (closeAfterUse)
            {
                await connection.CloseAsync();
            }
        }

        await EspelharVendaNoNexumAsync(nexumDb, venda, ct);
        return ToDto(venda);
    }

    private static async Task EspelharVendaNoNexumAsync(
        NexumDbContext nexumDb,
        GenesisPdvVendaSnapshot venda,
        CancellationToken ct)
    {
        try
        {
            var cliente = await ObterOuCriarClienteNexumAsync(nexumDb, venda, ct);
            var pedido = new Pedido
            {
                NumeroPedido = venda.Numero.Length > 20 ? venda.Numero[..20] : venda.Numero,
                ClienteId = cliente.Id,
                LojaId = null,
                Status = venda.ValorPago >= venda.Total ? StatusPedido.Pago : StatusPedido.Pendente,
                StatusPagamento = venda.ValorPago >= venda.Total ? StatusPagamento.Aprovado : StatusPagamento.Aguardando,
                MeioPagamento = venda.Pagamentos.FirstOrDefault()?.Forma ?? "PDV",
                GatewayPagamento = "GenesisGest.PDV",
                GatewayTransacaoId = $"GENESIS-PDV-{venda.Id}",
                Subtotal = venda.Subtotal,
                Desconto = venda.Desconto,
                FreteValor = venda.Frete,
                Total = venda.Total,
                Parcelas = venda.Pagamentos.Max(item => (int?)item.Parcelas) ?? 1,
                ObservacoesInternas = $"Venda originada no GenesisGest.Net. PedidoGenesisId={venda.Id}; Terminal={venda.Terminal}; Caixa={venda.CaixaCodigo}",
                Origem = OrigemPedido.API,
                MarketplaceOrigem = "GenesisGest.PDV",
                MarketplacePedidoId = venda.Numero,
                DataPagamento = venda.ValorPago >= venda.Total ? venda.CriadoEm : null,
                CreatedAt = venda.CriadoEm,
                UpdatedAt = DateTime.UtcNow
            };

            nexumDb.Pedidos.Add(pedido);
            await nexumDb.SaveChangesAsync(ct);

            foreach (var item in venda.Itens)
            {
                var produto = await ResolverProdutoNexumAsync(nexumDb, item, ct);
                var quantidade = Math.Max(1, (int)Math.Ceiling(item.Quantidade));
                if (produto is not null)
                {
                    produto.EstoqueAtual = Math.Max(0, produto.EstoqueAtual - quantidade);
                    produto.EstoqueReservado = Math.Max(0, produto.EstoqueReservado - quantidade);
                    produto.UpdatedAt = DateTime.UtcNow;
                }

                nexumDb.PedidoItens.Add(new PedidoItem
                {
                    PedidoId = pedido.Id,
                    ProdutoId = produto?.Id,
                    NomeProduto = item.Descricao,
                    SkuProduto = item.Sku ?? item.ProdutoCodigo ?? produto?.Sku,
                    ImagemProduto = produto?.ImagemPrincipal,
                    Quantidade = quantidade,
                    PrecoUnitario = item.PrecoUnitario,
                    PrecoTotal = item.Total,
                    DescontoItem = item.Desconto,
                    TipoFulfillment = ParseFulfillment(item.OrigemAquisicao),
                    StatusItem = StatusItemPedido.Separado,
                    CreatedAt = venda.CriadoEm
                });
            }

            foreach (var pagamento in venda.Pagamentos)
            {
                nexumDb.Pagamentos.Add(new Pagamento
                {
                    PedidoId = pedido.Id,
                    Gateway = "GenesisGest.PDV",
                    GatewayTransacaoId = $"GENESIS-PDV-{venda.Id}-{pagamento.Id}",
                    Metodo = ParseMetodoPagamento(pagamento.Forma),
                    Status = venda.ValorPago >= venda.Total ? StatusPagamentoDetalhado.Aprovado : StatusPagamentoDetalhado.Pendente,
                    Valor = pagamento.Valor,
                    ValorLiquido = pagamento.Valor,
                    TaxaGateway = 0m,
                    Parcelas = Math.Max(1, pagamento.Parcelas),
                    Bandeira = pagamento.Bandeira,
                    Nsu = pagamento.Nsu,
                    AutorizacaoCodigo = pagamento.Autorizacao,
                    DataProcessamento = venda.ValorPago >= venda.Total ? venda.CriadoEm : null,
                    CreatedAt = venda.CriadoEm,
                    UpdatedAt = DateTime.UtcNow
                });
            }

            nexumDb.Financeiros.Add(new Financeiro
            {
                Tipo = TipoLancamento.Receita,
                Categoria = "Venda PDV Genesis",
                Descricao = $"Venda PDV {venda.Numero} - {venda.ClienteNome}",
                Valor = venda.Total,
                DataVencimento = venda.CriadoEm,
                DataPagamento = venda.ValorPago >= venda.Total ? venda.CriadoEm : null,
                Status = venda.ValorPago >= venda.Total ? StatusLancamento.Pago : StatusLancamento.Pendente,
                MeioPagamento = venda.Pagamentos.FirstOrDefault()?.Forma ?? "PDV",
                Observacoes = $"GenesisPdvId={venda.Id}; PedidoNexum={pedido.NumeroPedido}",
                CreatedAt = venda.CriadoEm,
                UpdatedAt = DateTime.UtcNow
            });

            await nexumDb.SaveChangesAsync(ct);

            venda.ClienteNexumId = cliente.Id;
            venda.PedidoNexumId = pedido.Id;
            venda.PedidoNexumNumero = pedido.NumeroPedido;
            venda.StatusSincronizacao = "SINCRONIZADO_NEXUM";
        }
        catch (Exception ex)
        {
            venda.StatusSincronizacao = $"ERRO_NEXUM: {ex.Message[..Math.Min(180, ex.Message.Length)]}";
        }
    }

    private static async Task<Cliente> ObterOuCriarClienteNexumAsync(NexumDbContext nexumDb, GenesisPdvVendaSnapshot venda, CancellationToken ct)
    {
        if (venda.ClienteNexumId.HasValue)
        {
            var clientePorId = await nexumDb.Clientes.FirstOrDefaultAsync(item => item.Id == venda.ClienteNexumId.Value, ct);
            if (clientePorId is not null) return clientePorId;
        }

        var documento = OnlyDigits(venda.ClienteDocumento);
        if (!string.IsNullOrWhiteSpace(documento))
        {
            var clientePorDocumento = await nexumDb.Clientes.FirstOrDefaultAsync(item => item.CpfCnpj == documento, ct);
            if (clientePorDocumento is not null) return clientePorDocumento;
        }

        var email = TrimOrNull(venda.ClienteEmail);
        if (!string.IsNullOrWhiteSpace(email))
        {
            var clientePorEmail = await nexumDb.Clientes.FirstOrDefaultAsync(item => item.Email == email, ct);
            if (clientePorEmail is not null) return clientePorEmail;
        }

        email ??= !string.IsNullOrWhiteSpace(documento)
            ? $"pdv.{documento}@nexumaltivon.local"
            : $"pdv.{Guid.NewGuid():N}@nexumaltivon.local";

        var cliente = new Cliente
        {
            Tipo = string.IsNullOrWhiteSpace(documento) || documento.Length <= 11 ? TipoCliente.PF : TipoCliente.PJ,
            Nome = venda.ClienteNome,
            Email = email,
            CpfCnpj = documento,
            Newsletter = false,
            Vip = false,
            Status = StatusCliente.Ativo,
            ConfirmadoEm = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        nexumDb.Clientes.Add(cliente);
        await nexumDb.SaveChangesAsync(ct);
        return cliente;
    }

    private static async Task<Produto?> ResolverProdutoNexumAsync(NexumDbContext nexumDb, GenesisPdvVendaItemSnapshot item, CancellationToken ct)
    {
        if (item.ProdutoNexumId.HasValue)
        {
            var produtoPorId = await nexumDb.Produtos.FirstOrDefaultAsync(produto => produto.Id == item.ProdutoNexumId.Value, ct);
            if (produtoPorId is not null) return produtoPorId;
        }

        var sku = TrimOrNull(item.Sku) ?? TrimOrNull(item.ProdutoCodigo);
        return string.IsNullOrWhiteSpace(sku)
            ? null
            : await nexumDb.Produtos.FirstOrDefaultAsync(produto => produto.Sku == sku || produto.CodigoBarras == sku, ct);
    }

    private static async Task<int> ResolverEmpresaIdAsync(DbConnection connection, DbTransaction transaction, string? empresaCodigo, CancellationToken ct)
    {
        var code = TrimOrNull(empresaCodigo);
        if (!string.IsNullOrWhiteSpace(code))
        {
            var found = await ScalarIntOrNullAsync(
                connection,
                transaction,
                """
                SELECT emp_id
                FROM adm_empresas
                WHERE emp_ativo = 1
                  AND (emp_cnpj = @codigo OR emp_nome_fantasia = @codigo OR emp_razao_social = @codigo)
                ORDER BY emp_matriz DESC, emp_id
                LIMIT 1;
                """,
                ct,
                ("@codigo", code));
            if (found.HasValue) return found.Value;
        }

        return await ScalarIntOrNullAsync(
            connection,
            transaction,
            "SELECT emp_id FROM adm_empresas WHERE emp_ativo = 1 ORDER BY emp_matriz DESC, emp_id LIMIT 1;",
            ct) ?? 1;
    }

    private static async Task<int> ResolverUsuarioIdAsync(DbConnection connection, DbTransaction transaction, string? operador, int empresaId, CancellationToken ct)
    {
        var login = TrimOrNull(operador);
        if (!string.IsNullOrWhiteSpace(login))
        {
            var found = await ScalarIntOrNullAsync(
                connection,
                transaction,
                """
                SELECT usr_id
                FROM adm_usuarios
                WHERE usr_ativo = 1
                  AND (usr_login = @login OR usr_email = @login OR usr_nome = @login)
                ORDER BY CASE WHEN usr_emp_id = @empresaId THEN 0 ELSE 1 END, usr_id
                LIMIT 1;
                """,
                ct,
                ("@login", login),
                ("@empresaId", empresaId));
            if (found.HasValue) return found.Value;
        }

        return await ScalarIntOrNullAsync(
            connection,
            transaction,
            "SELECT usr_id FROM adm_usuarios WHERE usr_ativo = 1 ORDER BY CASE WHEN usr_emp_id = @empresaId THEN 0 ELSE 1 END, usr_id LIMIT 1;",
            ct,
            ("@empresaId", empresaId)) ?? 1;
    }

    private static async Task<int> ResolverClienteGenesisAsync(DbConnection connection, DbTransaction transaction, GenesisPdvVendaRequest request, CancellationToken ct)
    {
        var documento = OnlyDigits(request.ClienteDocumento);
        if (!string.IsNullOrWhiteSpace(documento))
        {
            var found = await ScalarIntOrNullAsync(
                connection,
                transaction,
                "SELECT pes_id FROM adm_pessoas_empresas WHERE pes_cpf_cnpj = @documento LIMIT 1;",
                ct,
                ("@documento", documento));
            if (found.HasValue) return found.Value;
        }

        var email = TrimOrNull(request.ClienteEmail)?.ToLowerInvariant();
        if (!string.IsNullOrWhiteSpace(email))
        {
            var found = await ScalarIntOrNullAsync(
                connection,
                transaction,
                "SELECT pes_id FROM adm_pessoas_empresas WHERE pes_email = @email LIMIT 1;",
                ct,
                ("@email", email));
            if (found.HasValue) return found.Value;
        }

        var id = await NextIdAsync(connection, transaction, "adm_pessoas_empresas", "pes_id", ct);
        await ExecuteAsync(
            connection,
            transaction,
            """
            INSERT INTO adm_pessoas_empresas
                (pes_id, pes_tipo, pes_nome_razao, pes_nome_fantasia, pes_cpf_cnpj, pes_rg_ie,
                 pes_cliente, pes_fornecedor, pes_colaborador, pes_transportadora,
                 pes_email, pes_observacoes, pes_ativo)
            VALUES
                (@id, @tipo, @nome, @fantasia, @documento, NULL,
                 1, 0, 0, 0,
                 @email, @observacoes, 1);
            """,
            ct,
            ("@id", id),
            ("@tipo", string.IsNullOrWhiteSpace(documento) || documento.Length <= 11 ? "FISICA" : "JURIDICA"),
            ("@nome", request.ClienteNome.Trim()),
            ("@fantasia", request.ClienteNome.Trim()),
            ("@documento", documento),
            ("@email", email),
            ("@observacoes", "Cliente criado automaticamente por venda PDV/online."));

        return id;
    }

    private static async Task<int> ResolverTabelaPrecoAsync(DbConnection connection, DbTransaction transaction, int empresaId, CancellationToken ct) =>
        await ScalarIntOrNullAsync(
            connection,
            transaction,
            """
            SELECT tpr_id
            FROM vnd_tabelas_preco
            WHERE tpr_ativo = 1
            ORDER BY CASE WHEN tpr_emp_id = @empresaId THEN 0 ELSE 1 END, tpr_padrao DESC, tpr_id
            LIMIT 1;
            """,
            ct,
            ("@empresaId", empresaId)) ?? 1;

    private static async Task<int> ResolverContaReceitaAsync(DbConnection connection, DbTransaction transaction, CancellationToken ct) =>
        await ScalarIntOrNullAsync(
            connection,
            transaction,
            """
            SELECT pct_id
            FROM fin_plano_contas
            WHERE pct_ativo = 1
            ORDER BY CASE WHEN pct_classe = 'RECEITA' THEN 0 ELSE 1 END, pct_id
            LIMIT 1;
            """,
            ct) ?? 1;

    private static async Task<int?> ResolverDepositoAsync(DbConnection connection, DbTransaction transaction, int empresaId, CancellationToken ct) =>
        await ScalarIntOrNullAsync(
            connection,
            transaction,
            """
            SELECT dep_id
            FROM est_depositos
            WHERE dep_ativo = 1
            ORDER BY CASE WHEN dep_emp_id = @empresaId THEN 0 ELSE 1 END, dep_id
            LIMIT 1;
            """,
            ct,
            ("@empresaId", empresaId));

    private static async Task<int> ResolverItemGenesisAsync(DbConnection connection, DbTransaction transaction, GenesisPdvVendaItemRequest item, int empresaId, int sequencia, CancellationToken ct)
    {
        var codigo = TrimOrNull(item.Sku) ?? TrimOrNull(item.ProdutoCodigo);
        if (!string.IsNullOrWhiteSpace(codigo))
        {
            var found = await ScalarIntOrNullAsync(
                connection,
                transaction,
                "SELECT itm_id FROM vnd_itens WHERE itm_codigo = @codigo LIMIT 1;",
                ct,
                ("@codigo", codigo));
            if (found.HasValue) return found.Value;
        }

        codigo ??= $"PDV-{DateTime.UtcNow:yyMMddHHmmss}-{sequencia:00}";
        var id = await NextIdAsync(connection, transaction, "vnd_itens", "itm_id", ct);
        await ExecuteAsync(
            connection,
            transaction,
            """
            INSERT INTO vnd_itens
                (itm_id, itm_emp_id, itm_codigo, itm_tipo, itm_descricao, itm_descricao_detalhada,
                 itm_unidade, itm_ncm, itm_cest, itm_peso_bruto, itm_peso_liquido,
                 itm_altura, itm_largura, itm_profundidade, itm_controla_estoque,
                 itm_controla_lote, itm_controla_serie, itm_ativo)
            VALUES
                (@id, @empresaId, @codigo, 'PRODUTO', @descricao, @detalhes,
                 'UN', NULL, NULL, NULL, NULL,
                 NULL, NULL, NULL, 1,
                 0, 0, 1);
            """,
            ct,
            ("@id", id),
            ("@empresaId", empresaId),
            ("@codigo", codigo),
            ("@descricao", item.Descricao.Trim()),
            ("@detalhes", $"Item criado no Genesis por venda PDV/Nexum. Origem: {TrimOrNull(item.OrigemAquisicao) ?? "nao informada"}."));
        return id;
    }

    private static async Task RegistrarSaidaEstoqueAsync(
        DbConnection connection,
        DbTransaction transaction,
        int empresaId,
        int itemId,
        int depositoId,
        int pedidoId,
        GenesisPdvVendaItemRequest item,
        decimal total,
        int usuarioId,
        CancellationToken ct)
    {
        var id = await NextIdAsync(connection, transaction, "est_movimentacoes", "mov_id", ct);
        var custoUnitario = Math.Max(0m, item.CustoUnitario);
        await ExecuteAsync(
            connection,
            transaction,
            """
            INSERT INTO est_movimentacoes
                (mov_id, mov_emp_id, mov_tipo, mov_itm_id, mov_dep_origem_id,
                 mov_dep_destino_id, mov_end_origem_id, mov_end_destino_id,
                 mov_lote, mov_serie, mov_quantidade, mov_custo_unitario, mov_custo_total,
                 mov_documento_tipo, mov_documento_id, mov_observacoes, mov_usr_id)
            VALUES
                (@id, @empresaId, 'SAIDA', @itemId, @depositoId,
                 NULL, NULL, NULL,
                 NULL, NULL, @quantidade, @custoUnitario, @custoTotal,
                 'PDV', @pedidoId, @observacoes, @usuarioId);
            """,
            ct,
            ("@id", id),
            ("@empresaId", empresaId),
            ("@itemId", itemId),
            ("@depositoId", depositoId),
            ("@quantidade", item.Quantidade),
            ("@custoUnitario", custoUnitario),
            ("@custoTotal", custoUnitario > 0 ? Math.Round(custoUnitario * item.Quantidade, 2, MidpointRounding.AwayFromZero) : total),
            ("@pedidoId", pedidoId),
            ("@observacoes", $"Saida automatica por venda PDV. Produto: {item.Descricao.Trim()}"),
            ("@usuarioId", usuarioId));
    }

    private static async Task<List<GenesisPdvVendaItemSnapshot>> ListarItensAsync(DbConnection connection, DbTransaction? transaction, int pedidoId, CancellationToken ct)
    {
        var itens = new List<GenesisPdvVendaItemSnapshot>();
        await using var command = CreateCommand(
            connection,
            transaction,
            """
            SELECT
                i.pdi_id,
                i.pdi_sequencia,
                it.itm_codigo,
                i.pdi_descricao,
                i.pdi_quantidade,
                i.pdi_preco_unitario,
                i.pdi_desconto_valor,
                i.pdi_valor_total
            FROM vnd_pedido_itens i
            LEFT JOIN vnd_itens it ON it.itm_id = i.pdi_itm_id
            WHERE i.pdi_pdv_id = @pedidoId
            ORDER BY i.pdi_sequencia, i.pdi_id;
            """,
            ("@pedidoId", pedidoId));

        await using var reader = await command.ExecuteReaderAsync(ct);
        while (await reader.ReadAsync(ct))
        {
            itens.Add(new GenesisPdvVendaItemSnapshot(
                GetInt(reader, "pdi_id"),
                GetInt(reader, "pdi_sequencia"),
                null,
                GetString(reader, "itm_codigo"),
                GetString(reader, "itm_codigo"),
                GetString(reader, "pdi_descricao") ?? "Item Genesis",
                GetDecimal(reader, "pdi_quantidade"),
                GetDecimal(reader, "pdi_preco_unitario"),
                0m,
                GetDecimal(reader, "pdi_desconto_valor"),
                GetDecimal(reader, "pdi_valor_total"),
                "GENESIS"));
        }

        return itens;
    }

    private static async Task<int> NextIdAsync(DbConnection connection, DbTransaction transaction, string table, string column, CancellationToken ct)
    {
        await using var command = CreateCommand(connection, transaction, $"SELECT COALESCE(MAX({column}), 0) + 1 FROM {table};");
        var result = await command.ExecuteScalarAsync(ct);
        return Convert.ToInt32(result);
    }

    private static async Task<int?> ScalarIntOrNullAsync(
        DbConnection connection,
        DbTransaction transaction,
        string sql,
        CancellationToken ct,
        params (string Name, object? Value)[] parameters)
    {
        await using var command = CreateCommand(connection, transaction, sql, parameters);
        var result = await command.ExecuteScalarAsync(ct);
        return result is null or DBNull ? null : Convert.ToInt32(result);
    }

    private static async Task ExecuteAsync(
        DbConnection connection,
        DbTransaction transaction,
        string sql,
        CancellationToken ct,
        params (string Name, object? Value)[] parameters)
    {
        await using var command = CreateCommand(connection, transaction, sql, parameters);
        await command.ExecuteNonQueryAsync(ct);
    }

    private static DbCommand CreateCommand(
        DbConnection connection,
        DbTransaction? transaction,
        string sql,
        params (string Name, object? Value)[] parameters)
    {
        var command = connection.CreateCommand();
        command.CommandText = sql;
        if (transaction is not null)
        {
            command.Transaction = transaction;
        }

        foreach (var parameter in parameters)
        {
            var dbParameter = command.CreateParameter();
            dbParameter.ParameterName = parameter.Name;
            dbParameter.Value = parameter.Value ?? DBNull.Value;
            command.Parameters.Add(dbParameter);
        }

        return command;
    }

    private static GenesisPdvVendaSnapshot ReadVenda(DbDataReader reader)
    {
        var total = GetDecimal(reader, "pdv_valor_total");
        return new GenesisPdvVendaSnapshot(
            GetInt(reader, "pdv_id"),
            GetString(reader, "pdv_numero") ?? string.Empty,
            GetInt(reader, "pdv_emp_id").ToString(),
            null,
            GetString(reader, "cliente_nome") ?? "Cliente nao identificado",
            GetString(reader, "pes_cpf_cnpj"),
            GetString(reader, "pes_email"),
            null,
            null,
            null,
            null,
            null,
            null,
            GetDecimal(reader, "pdv_valor_produtos"),
            GetDecimal(reader, "pdv_valor_desconto"),
            GetDecimal(reader, "pdv_valor_frete"),
            total,
            total,
            0m,
            GetString(reader, "pdv_status") ?? "APROVADO",
            "GENESIS_ORIGINAL",
            GetDateTime(reader, "pdv_data_cadastro"),
            GetString(reader, "pdv_forma_pagamento"),
            GetString(reader, "pdv_observacoes"));
    }

    private static void ValidarVenda(GenesisPdvVendaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClienteNome))
        {
            throw new ArgumentException("Cliente obrigatorio para venda PDV.");
        }

        if (request.Itens is null || request.Itens.Count == 0)
        {
            throw new ArgumentException("Venda PDV precisa de ao menos um item.");
        }

        if (request.Itens.Any(item => string.IsNullOrWhiteSpace(item.Descricao) || item.Quantidade <= 0 || item.PrecoUnitario < 0))
        {
            throw new ArgumentException("Itens PDV precisam de descricao, quantidade e preco validos.");
        }

        if (request.Pagamentos is null || request.Pagamentos.Count == 0)
        {
            throw new ArgumentException("Venda PDV precisa de pagamento informado.");
        }

        if (request.Pagamentos.Any(item => string.IsNullOrWhiteSpace(item.Forma) || item.Valor <= 0))
        {
            throw new ArgumentException("Pagamentos PDV precisam de forma e valor validos.");
        }
    }

    private static GenesisPdvVendaDto ToDto(GenesisPdvVendaSnapshot venda) =>
        new(
            venda.Id,
            venda.Numero,
            venda.EmpresaCodigo,
            venda.EmpresaNexumId,
            venda.ClienteNome,
            venda.ClienteDocumento,
            venda.ClienteEmail,
            venda.ClienteNexumId,
            venda.PedidoNexumId,
            venda.PedidoNexumNumero,
            venda.Terminal,
            venda.CaixaCodigo,
            venda.Operador,
            venda.Subtotal,
            venda.Desconto,
            venda.Frete,
            venda.Total,
            venda.ValorPago,
            venda.Troco,
            venda.Status,
            venda.StatusSincronizacao,
            venda.CriadoEm,
            venda.Itens.Select(item => new GenesisPdvVendaItemDto(
                item.Id,
                item.Sequencia,
                item.ProdutoNexumId,
                item.ProdutoCodigo,
                item.Sku,
                item.Descricao,
                item.Quantidade,
                item.PrecoUnitario,
                item.CustoUnitario,
                item.Desconto,
                item.Total,
                item.OrigemAquisicao)).ToList(),
            venda.Pagamentos.Select(item => new GenesisPdvPagamentoDto(
                item.Id,
                item.Forma,
                item.Valor,
                item.Parcelas,
                item.Autorizacao,
                item.Nsu,
                item.Bandeira)).ToList());

    private static string BuildObservacoes(GenesisPdvVendaRequest request)
    {
        var parts = new List<string>
        {
            "Venda registrada por GenesisGest.Net com espelho Nexum.",
            $"Terminal: {TrimOrNull(request.Terminal) ?? "nao informado"}",
            $"Caixa: {TrimOrNull(request.CaixaCodigo) ?? "nao informado"}"
        };

        var obs = TrimOrNull(request.Observacoes);
        if (!string.IsNullOrWhiteSpace(obs))
        {
            parts.Add(obs);
        }

        return string.Join(" | ", parts);
    }

    private static MetodoPagamento ParseMetodoPagamento(string? forma)
    {
        var normalized = (forma ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "pix" => MetodoPagamento.PIX,
            "dinheiro" or "cash" => MetodoPagamento.Outro,
            "credito" or "crédito" or "cartao credito" or "cartão crédito" or "cartao_credito" => MetodoPagamento.CartaoCredito,
            "debito" or "débito" or "cartao debito" or "cartão débito" or "cartao_debito" => MetodoPagamento.CartaoDebito,
            "boleto" => MetodoPagamento.Boleto,
            "transferencia" or "transferência" => MetodoPagamento.Transferencia,
            "wallet" or "carteira" => MetodoPagamento.Wallet,
            _ => MetodoPagamento.Outro
        };
    }

    private static TipoFulfillment ParseFulfillment(string? origem)
    {
        var normalized = (origem ?? string.Empty).Trim().ToLowerInvariant();
        return normalized switch
        {
            "drop" or "dropshipping" or "dropship" => TipoFulfillment.Dropshipping,
            "marketplace" or "ecommerce" or "e-commerce" => TipoFulfillment.Marketplace,
            _ => TipoFulfillment.Proprio
        };
    }

    private static int GetInt(DbDataReader reader, string name) =>
        reader[name] is DBNull ? 0 : Convert.ToInt32(reader[name]);

    private static decimal GetDecimal(DbDataReader reader, string name) =>
        reader[name] is DBNull ? 0m : Convert.ToDecimal(reader[name]);

    private static string? GetString(DbDataReader reader, string name) =>
        reader[name] is DBNull ? null : Convert.ToString(reader[name]);

    private static DateTime GetDateTime(DbDataReader reader, string name) =>
        reader[name] is DBNull ? DateTime.UtcNow : Convert.ToDateTime(reader[name]);

    private static string? TrimOrNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? OnlyDigits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return string.IsNullOrWhiteSpace(digits) ? null : digits;
    }

    private sealed class GenesisPdvVendaSnapshot
    {
        public GenesisPdvVendaSnapshot(
            int id,
            string numero,
            string? empresaCodigo,
            int? empresaNexumId,
            string clienteNome,
            string? clienteDocumento,
            string? clienteEmail,
            int? clienteNexumId,
            int? pedidoNexumId,
            string? pedidoNexumNumero,
            string? terminal,
            string? caixaCodigo,
            string? operador,
            decimal subtotal,
            decimal desconto,
            decimal frete,
            decimal total,
            decimal valorPago,
            decimal troco,
            string status,
            string statusSincronizacao,
            DateTime criadoEm,
            string? formaPagamento,
            string? observacoes)
        {
            Id = id;
            Numero = numero;
            EmpresaCodigo = empresaCodigo;
            EmpresaNexumId = empresaNexumId;
            ClienteNome = clienteNome;
            ClienteDocumento = clienteDocumento;
            ClienteEmail = clienteEmail;
            ClienteNexumId = clienteNexumId;
            PedidoNexumId = pedidoNexumId;
            PedidoNexumNumero = pedidoNexumNumero;
            Terminal = terminal;
            CaixaCodigo = caixaCodigo;
            Operador = operador;
            Subtotal = subtotal;
            Desconto = desconto;
            Frete = frete;
            Total = total;
            ValorPago = valorPago;
            Troco = troco;
            Status = status;
            StatusSincronizacao = statusSincronizacao;
            CriadoEm = criadoEm;
            FormaPagamento = formaPagamento;
            Observacoes = observacoes;
        }

        public int Id { get; }
        public string Numero { get; }
        public string? EmpresaCodigo { get; }
        public int? EmpresaNexumId { get; }
        public string ClienteNome { get; }
        public string? ClienteDocumento { get; }
        public string? ClienteEmail { get; }
        public int? ClienteNexumId { get; set; }
        public int? PedidoNexumId { get; set; }
        public string? PedidoNexumNumero { get; set; }
        public string? Terminal { get; }
        public string? CaixaCodigo { get; }
        public string? Operador { get; }
        public decimal Subtotal { get; }
        public decimal Desconto { get; }
        public decimal Frete { get; }
        public decimal Total { get; }
        public decimal ValorPago { get; }
        public decimal Troco { get; }
        public string Status { get; }
        public string StatusSincronizacao { get; set; }
        public DateTime CriadoEm { get; }
        public string? FormaPagamento { get; }
        public string? Observacoes { get; }
        public List<GenesisPdvVendaItemSnapshot> Itens { get; } = [];
        public List<GenesisPdvPagamentoSnapshot> Pagamentos { get; } = [];
    }

    private sealed record GenesisPdvVendaItemSnapshot(
        int Id,
        int Sequencia,
        int? ProdutoNexumId,
        string? ProdutoCodigo,
        string? Sku,
        string Descricao,
        decimal Quantidade,
        decimal PrecoUnitario,
        decimal CustoUnitario,
        decimal Desconto,
        decimal Total,
        string? OrigemAquisicao);

    private sealed record GenesisPdvPagamentoSnapshot(
        int Id,
        string Forma,
        decimal Valor,
        int Parcelas,
        string? Autorizacao,
        string? Nsu,
        string? Bandeira);
}
