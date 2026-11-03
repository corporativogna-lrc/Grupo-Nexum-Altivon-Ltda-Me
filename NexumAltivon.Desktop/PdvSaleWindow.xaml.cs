/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7256 Data: 30/10/2026
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class PdvSaleWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly LocalOutboxService _outbox = new();
    private readonly TerminalProfile _terminal;
    private PdvCartItem? _itemSelecionado;
    private DesktopPdvCatalogProduct? _produtoCatalogoSelecionado;
    private DesktopCliente? _clientePdvSelecionado;
    private string _codigoVenda = string.Empty;
    private string _clienteNome = string.Empty;
    private string _clienteDocumento = string.Empty;
    private string _buscaCliente = string.Empty;
    private string _empresaEmissora = string.Empty;
    private string _novoItemCodigo = string.Empty;
    private string _buscaCatalogo = string.Empty;
    private string _novoItemDescricao = string.Empty;
    private string _novoItemQuantidade = "1";
    private string _novoItemValorUnitario = "0,00";
    private string _novoItemCustoEstimado = "0,00";
    private string _novoItemDesconto = "0,00";
    private string _novoItemOrigem = "ECOM";
    private string _tipoEntrega = "Retirada na loja";
    private string _formaPagamento = "PIX";
    private string _valorPagamento = "0,00";
    private string _autorizacaoPagamento = string.Empty;
    private string _statusOperacional = "Venda em atendimento. Nenhum item registrado.";
    private string _ultimoArquivoLocal = "Fila local ainda vazia.";

    public ObservableCollection<PdvCartItem> Itens { get; } = new();
    public ObservableCollection<PdvPaymentLine> Pagamentos { get; } = new();
    public ObservableCollection<DesktopPdvCatalogProduct> ProdutosCatalogo { get; } = new();
    public ObservableCollection<DesktopCliente> ClientesPdv { get; } = new();

    public string BuscaCliente
    {
        get => _buscaCliente;
        set => SetField(ref _buscaCliente, value);
    }

    public DesktopCliente? ClientePdvSelecionado
    {
        get => _clientePdvSelecionado;
        set
        {
            SetField(ref _clientePdvSelecionado, value);
            if (value is null)
            {
                ClienteNome = string.Empty;
                ClienteDocumento = string.Empty;
                AtualizarStatusCiclo();
                return;
            }
            ClienteNome = value.Nome;
            ClienteDocumento = value.Cpf ?? string.Empty;
            AtualizarStatusCiclo();
            StatusOperacional = string.IsNullOrWhiteSpace(ClienteDocumento)
                ? "O cliente selecionado não possui CPF/CNPJ cadastrado; não poderá finalizar uma venda PDV."
                : $"Cliente {value.Nome} selecionado. A API validará o documento e o vínculo ativo no Genesis ao finalizar.";
        }
    }

    public string BuscaCatalogo
    {
        get => _buscaCatalogo;
        set => SetField(ref _buscaCatalogo, value);
    }

    public DesktopPdvCatalogProduct? ProdutoCatalogoSelecionado
    {
        get => _produtoCatalogoSelecionado;
        set
        {
            SetField(ref _produtoCatalogoSelecionado, value);
            if (value is null) return;
            NovoItemCodigo = value.Sku;
            NovoItemDescricao = value.Nome;
            NovoItemValorUnitario = value.PrecoAtual.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
            NovoItemCustoEstimado = value.Custo.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
            StatusOperacional = $"{value.Nome}: {value.Estoque} unidade(s) disponível(is) no catálogo consultado.";
        }
    }

    public string CodigoVenda
    {
        get => _codigoVenda;
        set => SetField(ref _codigoVenda, value);
    }

    public string ContextoOperacional => $"{_terminal.StoreName} | {_terminal.TerminalCode} | {_terminal.OperatorName}";

    public string ClienteNome
    {
        get => _clienteNome;
        set => SetField(ref _clienteNome, value);
    }

    public string ClienteDocumento
    {
        get => _clienteDocumento;
        set
        {
            SetField(ref _clienteDocumento, value);
            AtualizarStatusCiclo();
        }
    }

    public string EmpresaEmissora
    {
        get => _empresaEmissora;
        set
        {
            SetField(ref _empresaEmissora, NormalizeComboValue(value));
            AtualizarStatusCiclo();
        }
    }

    public string NovoItemCodigo
    {
        get => _novoItemCodigo;
        set => SetField(ref _novoItemCodigo, value);
    }

    public string NovoItemDescricao
    {
        get => _novoItemDescricao;
        set => SetField(ref _novoItemDescricao, value);
    }

    public string NovoItemQuantidade
    {
        get => _novoItemQuantidade;
        set => SetField(ref _novoItemQuantidade, value);
    }

    public string NovoItemValorUnitario
    {
        get => _novoItemValorUnitario;
        set => SetField(ref _novoItemValorUnitario, value);
    }

    public string NovoItemCustoEstimado
    {
        get => _novoItemCustoEstimado;
        set => SetField(ref _novoItemCustoEstimado, value);
    }

    public string NovoItemDesconto
    {
        get => _novoItemDesconto;
        set => SetField(ref _novoItemDesconto, value);
    }

    public string NovoItemOrigem
    {
        get => _novoItemOrigem;
        set => SetField(ref _novoItemOrigem, NormalizeComboValue(value));
    }

    public string TipoEntrega
    {
        get => _tipoEntrega;
        set
        {
            SetField(ref _tipoEntrega, NormalizeComboValue(value));
            AtualizarStatusCiclo();
        }
    }

    public string FormaPagamento
    {
        get => _formaPagamento;
        set => SetField(ref _formaPagamento, NormalizeComboValue(value));
    }

    public string ValorPagamento
    {
        get => _valorPagamento;
        set => SetField(ref _valorPagamento, value);
    }

    public string AutorizacaoPagamento
    {
        get => _autorizacaoPagamento;
        set => SetField(ref _autorizacaoPagamento, value);
    }

    public PdvCartItem? ItemSelecionado
    {
        get => _itemSelecionado;
        set => SetField(ref _itemSelecionado, value);
    }

    public string StatusOperacional
    {
        get => _statusOperacional;
        set => SetField(ref _statusOperacional, value);
    }

    public string UltimoArquivoLocal
    {
        get => _ultimoArquivoLocal;
        set => SetField(ref _ultimoArquivoLocal, value);
    }

    public string SubtotalLabel => $"Subtotal: R$ {Subtotal:N2}";
    public string DescontoLabel => $"Descontos: R$ {Desconto:N2}";
    public string TotalLabel => $"Total: R$ {Total:N2}";
    public string PagoLabel => $"Pago: R$ {Pago:N2}";
    public string TrocoLabel => $"Troco: R$ {Troco:N2}";
    public string MargemLabel => $"Margem estimada: R$ {MargemEstimada:N2}";
    public bool DocumentoClienteValido => string.IsNullOrWhiteSpace(ClienteDocumento) || IsCpfOrCnpjValid(ClienteDocumento);
    public string DocumentoClienteLabel => DocumentoClienteValido
        ? "Documento aceito para venda."
        : "CPF/CNPJ inválido. Corrija antes de finalizar.";
    public string FiscalLabel => $"Fiscal: {StatusFiscal}";
    public string FinanceiroLabel => $"Financeiro: {StatusFinanceiro}";
    public string LogisticaLabel => $"Logística: {StatusLogistico}";
    private string StatusFiscal => string.IsNullOrWhiteSpace(EmpresaEmissora)
        ? "codigo/CNPJ/nome da empresa emissora obrigatorio"
        : $"empresa emissora explicitamente informada: {EmpresaEmissora}";
    private string StatusFinanceiro => Pago >= Total && Total > 0m
        ? "pagamento suficiente para baixa e contas a receber"
        : $"aguardando pagamento de R$ {Math.Max(0m, Total - Pago):N2}";
    private string StatusLogistico => TipoEntrega switch
    {
        "Retirada na loja" => "retirada imediata no balcão",
        "Entrega local" => "separação e rota local pendentes",
        "Transportadora" => "coleta por transportadora pendente",
        "Dropshipping/parceiro" => "pedido será vinculado ao parceiro/dropshipping",
        _ => "logística pendente"
    };

    public decimal Subtotal => Itens.Sum(item => item.Quantidade * item.ValorUnitario);
    public decimal Desconto => Itens.Sum(item => item.Desconto);
    public decimal Total => Itens.Sum(item => item.Total);
    public decimal Pago => Pagamentos.Sum(payment => payment.Valor);
    public decimal Troco => Math.Max(0m, Pago - Total);
    public decimal MargemEstimada => Itens.Sum(item => item.MargemEstimada);

    public event PropertyChangedEventHandler? PropertyChanged;

    public PdvSaleWindow(TerminalProfile terminal)
    {
        _terminal = terminal;
        InitializeComponent();
        DataContext = this;
        EmpresaEmissora = terminal.CompanyCode;
        NovaVenda();
    }

    private void NovaVenda_Click(object sender, RoutedEventArgs e)
    {
        NovaVenda();
    }

    private async void BuscarCatalogo_Click(object sender, RoutedEventArgs e) => await BuscarCatalogoAsync();

    private async void BuscaCatalogo_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        await BuscarCatalogoAsync();
    }

    private async Task BuscarCatalogoAsync()
    {
        ProdutoCatalogoSelecionado = null;
        StatusOperacional = "Consultando nome, SKU, código de barras ou QR no catálogo oficial...";
        var result = await _api.SearchPdvCatalogAsync(_terminal, BuscaCatalogo);
        ProdutosCatalogo.Clear();
        if (!result.Success || result.Data is null)
        {
            ProdutoCatalogoSelecionado = null;
            StatusOperacional = $"Catálogo não consultado: {result.Detail}";
            return;
        }

        foreach (var product in result.Data.Where(product => !string.IsNullOrWhiteSpace(product.Sku)))
            ProdutosCatalogo.Add(product);

        var term = BuscaCatalogo.Trim();
        var termDigits = NormalizeIdentifierDigits(term);
        var exactMatches = ProdutosCatalogo.Where(product =>
                string.Equals(product.Sku, term, StringComparison.OrdinalIgnoreCase)
                || (!string.IsNullOrWhiteSpace(product.QrCode)
                    && string.Equals(product.QrCode.Trim(), term, StringComparison.OrdinalIgnoreCase))
                || (termDigits.Length > 0
                    && NormalizeIdentifierDigits(product.CodigoBarras) == termDigits))
            .Take(2)
            .ToArray();
        if (exactMatches.Length == 1)
        {
            ProdutoCatalogoSelecionado = exactMatches[0];
            StatusOperacional = "Identificador exato localizado. Confirme o produto e adicione-o à venda.";
            return;
        }
        if (exactMatches.Length > 1)
        {
            StatusOperacional = "O identificador corresponde a mais de um produto. Selecione manualmente o item correto e corrija a duplicidade do cadastro.";
            return;
        }

        StatusOperacional = ProdutosCatalogo.Count == 0
            ? "Nenhum produto publicado encontrado. Confira o termo e a disponibilidade no catálogo."
            : $"{ProdutosCatalogo.Count} produto(s) retornado(s) pela API. Selecione o item correto.";
    }

    private async void CarregarClientes_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            StatusOperacional = "Sessão de gerente ausente. Autentique o Desktop antes de carregar clientes.";
            return;
        }

        StatusOperacional = "Consultando clientes cadastrados na API oficial...";
        var result = await _api.SearchClientesAsync(_terminal, _terminal.DesktopAccessToken, BuscaCliente);
        ClientesPdv.Clear();
        ClientePdvSelecionado = null;
        if (!result.Success || result.Data is null)
        {
            StatusOperacional = $"Clientes não carregados: {result.Detail}";
            return;
        }

        var term = BuscaCliente.Trim();
        if (!string.IsNullOrEmpty(term)
            && !string.Equals(result.Detail, "Clientes filtrados no banco oficial.", StringComparison.Ordinal))
        {
            StatusOperacional = "A API conectada não confirmou a busca completa por clientes. Atualize a API oficial antes de usar um filtro; nenhum resultado parcial será apresentado.";
            return;
        }

        var matches = result.Data.Where(cliente =>
                string.Equals(cliente.Status, "Ativo", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(cliente.Cpf)
                && (string.IsNullOrWhiteSpace(term)
                    || cliente.Nome.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || cliente.Email.Contains(term, StringComparison.OrdinalIgnoreCase)
                    || (cliente.Cpf?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)))
            .ToList();

        foreach (var customer in matches)
            ClientesPdv.Add(customer);

        StatusOperacional = ClientesPdv.Count == 0
            ? string.IsNullOrEmpty(term)
                ? "Nenhum cliente ativo com documento foi encontrado entre os 500 registros recentes retornados pela API. Confira o cadastro e a situação do cliente."
                : "Nenhum cliente ativo com documento corresponde ao filtro consultado no banco oficial. Confira o cadastro e a situação do cliente."
            : $"{ClientesPdv.Count} cliente(s) ativo(s) com documento no retorno oficial. Selecione o comprador cadastrado.";
    }

    private void AdicionarItem_Click(object sender, RoutedEventArgs e)
    {
        if (ProdutoCatalogoSelecionado is null
            || string.IsNullOrWhiteSpace(NovoItemCodigo) || string.IsNullOrWhiteSpace(NovoItemDescricao))
        {
            StatusOperacional = "Pesquise e selecione um produto publicado no catálogo oficial antes de adicioná-lo.";
            return;
        }

        if (!TryParseDecimal(NovoItemQuantidade, out var quantidade) || quantidade <= 0m
            || !TryParseDecimal(NovoItemValorUnitario, out var valorUnitario) || valorUnitario < 0m
            || !TryParseDecimal(NovoItemCustoEstimado, out var custoEstimado) || custoEstimado < 0m
            || !TryParseDecimal(NovoItemDesconto, out var desconto) || desconto < 0m)
        {
            StatusOperacional = "Quantidade, preço, custo e desconto devem ser valores numéricos válidos; quantidade deve ser maior que zero.";
            return;
        }

        var codigo = NovoItemCodigo.Trim();
        var descricao = NovoItemDescricao.Trim();
        var quantidadeNoCarrinho = Itens
            .Where(item => string.Equals(item.Codigo, ProdutoCatalogoSelecionado.Sku, StringComparison.OrdinalIgnoreCase))
            .Sum(item => item.Quantidade);
        if (quantidade + quantidadeNoCarrinho > ProdutoCatalogoSelecionado.Estoque)
        {
            var disponivel = Math.Max(0m, ProdutoCatalogoSelecionado.Estoque - quantidadeNoCarrinho);
            StatusOperacional = $"Estoque insuficiente no catálogo: {disponivel} unidade(s) restante(s) para este SKU. A API fará a validação final.";
            return;
        }

        var item = new PdvCartItem
        {
            Codigo = codigo,
            Descricao = descricao,
            EmpresaDestino = _terminal.StoreCode,
            OrigemAquisicao = NovoItemOrigem,
            Quantidade = quantidade,
            ValorUnitario = valorUnitario,
            CustoEstimado = custoEstimado,
            Desconto = desconto
        };

        item.PropertyChanged += (_, _) => AtualizarTotais();
        Itens.Add(item);
        ItemSelecionado = item;
        StatusOperacional = $"Item {item.Codigo} incluído. Total atual R$ {Total:N2}.";

        NovoItemCodigo = string.Empty;
        NovoItemDescricao = string.Empty;
        ProdutoCatalogoSelecionado = null;
        NovoItemQuantidade = "1";
        NovoItemValorUnitario = "0,00";
        NovoItemCustoEstimado = "0,00";
        NovoItemDesconto = "0,00";
        AtualizarTotais();
    }

    private void RemoverItem_Click(object sender, RoutedEventArgs e)
    {
        if (ItemSelecionado is null)
        {
            StatusOperacional = "Selecione um item para remover.";
            return;
        }

        Itens.Remove(ItemSelecionado);
        ItemSelecionado = null;
        StatusOperacional = $"Item removido. Total atual R$ {Total:N2}.";
        AtualizarTotais();
    }

    private void RegistrarPagamento_Click(object sender, RoutedEventArgs e)
    {
        var valor = ParseDecimal(ValorPagamento, Total - Pago);
        if (valor <= 0)
        {
            StatusOperacional = "Informe um valor de pagamento maior que zero.";
            return;
        }

        Pagamentos.Add(new PdvPaymentLine
        {
            Forma = FormaPagamento,
            Valor = valor,
            Autorizacao = AutorizacaoPagamento.Trim()
        });

        ValorPagamento = "0,00";
        AutorizacaoPagamento = string.Empty;
        StatusOperacional = Pago >= Total
            ? "Pagamento suficiente para finalizar a venda."
            : $"Pagamento parcial registrado. Falta R$ {Math.Max(0m, Total - Pago):N2}.";
        AtualizarTotais();
    }

    private async void SalvarLocal_Click(object sender, RoutedEventArgs e)
    {
        await SalvarRascunhoLocalAsync();
    }

    private async void FinalizarVenda_Click(object sender, RoutedEventArgs e)
    {
        if (!FinalizarVendaButton.IsEnabled) return;
        if (Itens.Count == 0)
        {
            StatusOperacional = "Adicione ao menos um produto cadastrado no catálogo antes de finalizar.";
            return;
        }
        if (ClientePdvSelecionado is null
            || string.IsNullOrWhiteSpace(ClienteNome) || string.IsNullOrWhiteSpace(ClienteDocumento)
            || !DocumentoClienteValido)
        {
            StatusOperacional = "Informe nome e CPF/CNPJ válido de um cliente previamente cadastrado.";
            return;
        }
        if (string.IsNullOrWhiteSpace(EmpresaEmissora))
        {
            StatusOperacional = "Configure NEXUM_DESKTOP_COMPANY_CODE com o CNPJ ou nome exato da empresa emissora.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            StatusOperacional = "Sessão de gerente ausente. Autentique o Desktop antes de finalizar a venda.";
            return;
        }
        if (Total <= 0m || Pago < Total)
        {
            StatusOperacional = $"Pagamento insuficiente. Falta R$ {Math.Max(0m, Total - Pago):N2}.";
            return;
        }

        FinalizarVendaButton.IsEnabled = false;
        StatusOperacional = "Validando produtos, cliente, empresa, estoque e gravação nos sistemas Genesis/Nexum...";
        try
        {
            var request = new DesktopPdvSaleRequest(
                CodigoVenda,
                EmpresaEmissora.Trim(),
                null,
                ClienteNome.Trim(),
                ClienteDocumento.Trim(),
                ClientePdvSelecionado.Email,
                ClientePdvSelecionado.Id,
                _terminal.TerminalCode,
                _terminal.TerminalCode,
                _terminal.OperatorName,
                Desconto,
                0m,
                $"Origem=PDV; Entrega={TipoEntrega}; PerfilLoja={_terminal.StoreCode}.",
                Itens.Select(item => new DesktopPdvSaleItemRequest(
                    null,
                    item.Codigo,
                    item.Codigo,
                    item.Descricao,
                    item.Quantidade,
                    item.ValorUnitario,
                    item.CustoEstimado,
                    0m,
                    item.OrigemAquisicao)).ToList(),
                Pagamentos.Select(payment => new DesktopPdvPaymentRequest(
                    payment.Forma,
                    payment.Valor,
                    1,
                    payment.Autorizacao,
                    null,
                    null)).ToList());

            var result = await _api.RegisterPdvSaleAsync(_terminal, _terminal.DesktopAccessToken, request);
            if (!result.Success || result.Data is null)
            {
                StatusOperacional = $"Venda não confirmada pela API: {result.Detail}";
                return;
            }

            UltimoArquivoLocal = $"Venda {result.Data.Numero} | Genesis {result.Data.Id}";
            if (result.Data.StatusSincronizacao.StartsWith("ERRO_NEXUM:", StringComparison.OrdinalIgnoreCase))
            {
                StatusOperacional = $"Venda {result.Data.Numero} gravada no Genesis, mas o espelho Nexum falhou: {result.Data.StatusSincronizacao[11..]}. Não repita a venda; encaminhe para conciliação.";
                return;
            }

            StatusOperacional = $"Venda {result.Data.Numero} gravada e sincronizada. Total recebido: R$ {result.Data.ValorPago:N2}.";
        }
        catch (Exception ex)
        {
            StatusOperacional = $"Venda não confirmada. Consulte a API pelo número {CodigoVenda} antes de tentar novamente. Detalhe: {ex.Message}";
        }
        finally
        {
            FinalizarVendaButton.IsEnabled = true;
        }
    }

    private void NovaVenda()
    {
        Itens.Clear();
        Pagamentos.Clear();
        CodigoVenda = $"PDV-{_terminal.StoreCode}-{DateTime.Now:yyyyMMdd-HHmmss-fff}";
        ClienteNome = string.Empty;
        ClienteDocumento = string.Empty;
        ClientePdvSelecionado = null;
        EmpresaEmissora = _terminal.CompanyCode;
        StatusOperacional = "Venda em atendimento. Adicione itens para totalizar.";
        UltimoArquivoLocal = "Fila local ainda vazia.";
        AtualizarTotais();
    }

    private async Task SalvarRascunhoLocalAsync()
    {
        if (Itens.Count == 0)
        {
            StatusOperacional = "Não é possível salvar venda sem itens.";
            return;
        }

        var sale = new PdvSaleDraft
        {
            CodigoVenda = CodigoVenda,
            Loja = _terminal.StoreName,
            Terminal = _terminal.TerminalCode,
            Operador = _terminal.OperatorName,
            ClienteNome = ClienteNome,
            ClienteDocumento = ClienteDocumento,
            ClienteDocumentoValido = DocumentoClienteValido,
            EmpresaEmissora = EmpresaEmissora,
            Status = "Rascunho local em atendimento",
            TipoEntrega = TipoEntrega,
            StatusPedido = "Rascunho local em atendimento",
            StatusFiscal = StatusFiscal,
            StatusFinanceiro = StatusFinanceiro,
            StatusLogistico = StatusLogistico,
            DecisaoEmpresaEmissora = StatusFiscal,
            Subtotal = Subtotal,
            Desconto = Desconto,
            Total = Total,
            Pago = Pago,
            Troco = Troco,
            MargemEstimada = MargemEstimada,
            Itens = Itens.Select(CloneItem).ToList(),
            Pagamentos = Pagamentos.ToList()
        };

        if (!_terminal.OfflineContingencyEnabled)
        {
            UltimoArquivoLocal = "Contingência local desativada.";
            StatusOperacional = "Rascunho não salvo: contingência local desativada. Nenhum dado foi enviado ao servidor.";
            return;
        }

        var filePath = await _outbox.SaveSaleAsync(sale);
        UltimoArquivoLocal = $"Contingência local: {filePath}";
        StatusOperacional = "Rascunho salvo localmente. Não é uma venda finalizada nem será sincronizado automaticamente.";
    }

    private static PdvCartItem CloneItem(PdvCartItem item) => new()
    {
        Codigo = item.Codigo,
        Descricao = item.Descricao,
        EmpresaDestino = item.EmpresaDestino,
        OrigemAquisicao = item.OrigemAquisicao,
        Quantidade = item.Quantidade,
        ValorUnitario = item.ValorUnitario,
        Desconto = item.Desconto,
        CustoEstimado = item.CustoEstimado
    };

    private static string NormalizeComboValue(object? value)
    {
        if (value is System.Windows.Controls.ComboBoxItem item)
        {
            return item.Content?.ToString() ?? string.Empty;
        }

        return value?.ToString() ?? string.Empty;
    }

    private static string NormalizeIdentifierDigits(string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    private static decimal ParseDecimal(string? value, decimal fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        var normalized = value.Trim().Replace("R$", "", StringComparison.OrdinalIgnoreCase).Trim();
        var culture = CultureInfo.GetCultureInfo("pt-BR");

        if (decimal.TryParse(normalized, NumberStyles.Any, culture, out var result))
        {
            return result;
        }

        if (decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result))
        {
            return result;
        }

        return fallback;
    }

    private static bool TryParseDecimal(string? value, out decimal result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = 0m;
            return false;
        }

        var normalized = value.Trim().Replace("R$", "", StringComparison.OrdinalIgnoreCase).Trim();
        return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.GetCultureInfo("pt-BR"), out result)
            || decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
    }

    private void AtualizarTotais()
    {
        OnPropertyChanged(nameof(SubtotalLabel));
        OnPropertyChanged(nameof(DescontoLabel));
        OnPropertyChanged(nameof(TotalLabel));
        OnPropertyChanged(nameof(PagoLabel));
        OnPropertyChanged(nameof(TrocoLabel));
        OnPropertyChanged(nameof(MargemLabel));
        AtualizarStatusCiclo();
    }

    private void AtualizarStatusCiclo()
    {
        OnPropertyChanged(nameof(DocumentoClienteValido));
        OnPropertyChanged(nameof(DocumentoClienteLabel));
        OnPropertyChanged(nameof(FiscalLabel));
        OnPropertyChanged(nameof(FinanceiroLabel));
        OnPropertyChanged(nameof(LogisticaLabel));
    }

    private static bool IsCpfOrCnpjValid(string document)
    {
        var digits = new string((document ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length switch
        {
            11 => IsCpfValid(digits),
            14 => IsCnpjValid(digits),
            _ => false
        };
    }

    private static bool IsCpfValid(string cpf)
    {
        if (cpf.Distinct().Count() == 1)
        {
            return false;
        }

        var sum = 0;
        for (var i = 0; i < 9; i++)
        {
            sum += (cpf[i] - '0') * (10 - i);
        }

        var digit = sum % 11;
        var first = digit < 2 ? 0 : 11 - digit;
        if (first != cpf[9] - '0')
        {
            return false;
        }

        sum = 0;
        for (var i = 0; i < 10; i++)
        {
            sum += (cpf[i] - '0') * (11 - i);
        }

        digit = sum % 11;
        var second = digit < 2 ? 0 : 11 - digit;
        return second == cpf[10] - '0';
    }

    private static bool IsCnpjValid(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1)
        {
            return false;
        }

        int[] firstWeights = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] secondWeights = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            sum += (cnpj[i] - '0') * firstWeights[i];
        }

        var remainder = sum % 11;
        var first = remainder < 2 ? 0 : 11 - remainder;
        if (first != cnpj[12] - '0')
        {
            return false;
        }

        sum = 0;
        for (var i = 0; i < 13; i++)
        {
            sum += (cnpj[i] - '0') * secondWeights[i];
        }

        remainder = sum % 11;
        var second = remainder < 2 ? 0 : 11 - remainder;
        return second == cnpj[13] - '0';
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
