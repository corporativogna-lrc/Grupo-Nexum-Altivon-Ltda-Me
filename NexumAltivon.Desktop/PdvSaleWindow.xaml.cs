/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class PdvSaleWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly LocalOutboxService _outbox = new();
    private readonly TerminalProfile _terminal;
    private PdvCartItem? _itemSelecionado;
    private string _codigoVenda = string.Empty;
    private string _clienteNome = "Consumidor final";
    private string _clienteDocumento = string.Empty;
    private string _empresaEmissora = "Seleção automática - menor custo";
    private string _novoItemCodigo = string.Empty;
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
    private string StatusFiscal => EmpresaEmissora.Contains("automática", StringComparison.OrdinalIgnoreCase)
        ? "emitente será definido pelo menor custo e maior margem"
        : $"emitente selecionado: {EmpresaEmissora}";
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
        NovaVenda();
    }

    private void NovaVenda_Click(object sender, RoutedEventArgs e)
    {
        NovaVenda();
    }

    private void AdicionarItem_Click(object sender, RoutedEventArgs e)
    {
        var codigo = string.IsNullOrWhiteSpace(NovoItemCodigo)
            ? GerarCodigoTemporario()
            : NovoItemCodigo.Trim();

        var descricao = string.IsNullOrWhiteSpace(NovoItemDescricao)
            ? "Item de balcão informado no terminal"
            : NovoItemDescricao.Trim();

        var item = new PdvCartItem
        {
            Codigo = codigo,
            Descricao = descricao,
            EmpresaDestino = _terminal.StoreCode,
            OrigemAquisicao = NovoItemOrigem,
            Quantidade = ParseDecimal(NovoItemQuantidade, 1m),
            ValorUnitario = ParseDecimal(NovoItemValorUnitario, 0m),
            CustoEstimado = ParseDecimal(NovoItemCustoEstimado, 0m),
            Desconto = ParseDecimal(NovoItemDesconto, 0m)
        };

        item.PropertyChanged += (_, _) => AtualizarTotais();
        Itens.Add(item);
        ItemSelecionado = item;
        StatusOperacional = $"Item {item.Codigo} incluído. Total atual R$ {Total:N2}.";

        NovoItemCodigo = string.Empty;
        NovoItemDescricao = string.Empty;
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
        await SalvarVendaLocalAsync(false);
    }

    private async void FinalizarVenda_Click(object sender, RoutedEventArgs e)
    {
        await SalvarVendaLocalAsync(true);
    }

    private void NovaVenda()
    {
        Itens.Clear();
        Pagamentos.Clear();
        CodigoVenda = $"PDV-{_terminal.StoreCode}-{DateTime.Now:yyyyMMdd-HHmmss}";
        ClienteNome = "Consumidor final";
        ClienteDocumento = string.Empty;
        EmpresaEmissora = "Seleção automática - menor custo";
        StatusOperacional = "Venda em atendimento. Adicione itens para totalizar.";
        UltimoArquivoLocal = "Fila local ainda vazia.";
        AtualizarTotais();
    }

    private async Task SalvarVendaLocalAsync(bool finalizar)
    {
        if (Itens.Count == 0)
        {
            StatusOperacional = "Não é possível salvar venda sem itens.";
            return;
        }

        if (finalizar && Pago < Total)
        {
            StatusOperacional = $"Pagamento insuficiente. Falta R$ {Total - Pago:N2}.";
            return;
        }

        if (finalizar && !DocumentoClienteValido)
        {
            StatusOperacional = "CPF/CNPJ inválido. Corrija o documento do cliente antes de finalizar.";
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
            Status = finalizar ? "Finalizada no PDV" : "Rascunho em atendimento",
            TipoEntrega = TipoEntrega,
            StatusPedido = finalizar ? "Finalizado no PDV local" : "Rascunho em atendimento",
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

        var submit = await _api.SubmitSaleAsync(_terminal, sale);
        if (submit.Success)
        {
            UltimoArquivoLocal = $"Servidor: {submit.Endpoint} | {submit.ServerReference ?? submit.Detail}";
            StatusOperacional = finalizar
                ? "Venda gravada no servidor Genesis/Nexum com reflexo em estoque, financeiro e fila fiscal."
                : "Rascunho enviado ao servidor Genesis/Nexum para continuidade operacional.";
            return;
        }

        if (!_terminal.OfflineContingencyEnabled)
        {
            UltimoArquivoLocal = "Contingência local desativada.";
            StatusOperacional = $"API indisponível e contingência desativada. Venda não foi gravada. Motivo: {submit.Detail}";
            return;
        }

        var filePath = await _outbox.SaveSaleAsync(sale);
        UltimoArquivoLocal = $"Contingência local: {filePath}";
        StatusOperacional = $"API indisponível. Venda salva em contingência local e pendente de sincronização. Motivo: {submit.Detail}";
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

    private static string GerarCodigoTemporario() => $"BALCAO-{DateTime.Now:HHmmss}";

    private static string NormalizeComboValue(object? value)
    {
        if (value is System.Windows.Controls.ComboBoxItem item)
        {
            return item.Content?.ToString() ?? string.Empty;
        }

        return value?.ToString() ?? string.Empty;
    }

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
