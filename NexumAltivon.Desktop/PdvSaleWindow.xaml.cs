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
    private string _novoItemOrigem = "ECOM";
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
        set => SetField(ref _clienteDocumento, value);
    }

    public string EmpresaEmissora
    {
        get => _empresaEmissora;
        set => SetField(ref _empresaEmissora, NormalizeComboValue(value));
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

    public string NovoItemOrigem
    {
        get => _novoItemOrigem;
        set => SetField(ref _novoItemOrigem, NormalizeComboValue(value));
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
            CustoEstimado = ParseDecimal(NovoItemCustoEstimado, 0m)
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

        var sale = new PdvSaleDraft
        {
            CodigoVenda = CodigoVenda,
            Loja = _terminal.StoreName,
            Terminal = _terminal.TerminalCode,
            Operador = _terminal.OperatorName,
            ClienteNome = ClienteNome,
            ClienteDocumento = ClienteDocumento,
            EmpresaEmissora = EmpresaEmissora,
            Status = finalizar ? "Finalizada localmente" : "Rascunho local",
            Subtotal = Subtotal,
            Desconto = Desconto,
            Total = Total,
            Pago = Pago,
            Troco = Troco,
            MargemEstimada = MargemEstimada,
            Itens = Itens.Select(CloneItem).ToList(),
            Pagamentos = Pagamentos.ToList()
        };

        var filePath = await _outbox.SaveSaleAsync(sale);
        UltimoArquivoLocal = $"Arquivo local: {filePath}";
        StatusOperacional = finalizar
            ? "Venda finalizada localmente e pronta para sincronização com servidor, estoque, fiscal e financeiro."
            : "Rascunho salvo localmente para retomada/sincronização.";
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
