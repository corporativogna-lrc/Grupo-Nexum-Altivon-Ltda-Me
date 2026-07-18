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

public partial class ContasPagarWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private DesktopContaPagar? _selectedTitulo;
    private string _numeroDocumento = string.Empty;
    private string _fornecedorId = string.Empty;
    private string _descricao = string.Empty;
    private string _valorOriginal = string.Empty;
    private DateTime _dataEmissao = DateTime.Today;
    private DateTime _dataVencimento = DateTime.Today;
    private string _formaPagamento = string.Empty;
    private string _numeroBoleto = string.Empty;
    private string _valorBaixa = string.Empty;
    private DateTime _dataBaixa = DateTime.Today;
    private string _formaBaixa = string.Empty;
    private string _observacoesBaixa = string.Empty;
    private string _status = "Aguardando carregamento da API oficial.";
    private bool _busy;

    public ContasPagarWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DesktopContaPagar> Titulos { get; } = new();
    public ObservableCollection<DesktopAuditoriaOperacional> Auditoria { get; } = new();
    public string TenantLabel => $"Loja {_terminal.StoreCode}";
    public bool PodeOperar => !_busy;
    public string CodigoContexto => SelectedTitulo is null ? "NOVO CP" : $"CP-{SelectedTitulo.Id}";
    public string EstadoContexto => SelectedTitulo?.Status ?? "NOVO";
    public string QuantidadeLabel => $"{Titulos.Count} título(s)";
    public string TotalOriginalLabel => $"R$ {Titulos.Sum(item => item.ValorOriginal):N2}";
    public string TotalAbertoLabel => $"R$ {Titulos.Sum(item => item.ValorAberto):N2}";
    public string TituloSelecionadoLabel => SelectedTitulo is null
        ? "Selecione um título na grade"
        : $"{SelectedTitulo.NumeroDocumento} | R$ {SelectedTitulo.ValorAberto:N2} em aberto";
    public string TituloSelecionadoDetalhe => SelectedTitulo is null
        ? string.Empty
        : $"{SelectedTitulo.Descricao} | vencimento {SelectedTitulo.DataVencimento:dd/MM/yyyy} | status {SelectedTitulo.Status}";
    public string ValidacaoCadastro => BuildValidationMessage();

    public DesktopContaPagar? SelectedTitulo
    {
        get => _selectedTitulo;
        set
        {
            if (!SetField(ref _selectedTitulo, value))
            {
                return;
            }

            if (value is not null)
            {
                ValorBaixa = value.ValorAberto.ToString("N2", CultureInfo.CurrentCulture);
                FormaBaixa = value.FormaPagamento ?? string.Empty;
            }

            NotifyContext();
        }
    }

    public string NumeroDocumento { get => _numeroDocumento; set => SetFormField(ref _numeroDocumento, value); }
    public string FornecedorId { get => _fornecedorId; set => SetFormField(ref _fornecedorId, value); }
    public string Descricao { get => _descricao; set => SetFormField(ref _descricao, value); }
    public string ValorOriginal { get => _valorOriginal; set => SetFormField(ref _valorOriginal, value); }
    public DateTime DataEmissao { get => _dataEmissao; set => SetFormField(ref _dataEmissao, value); }
    public DateTime DataVencimento { get => _dataVencimento; set => SetFormField(ref _dataVencimento, value); }
    public string FormaPagamento { get => _formaPagamento; set => SetFormField(ref _formaPagamento, value); }
    public string NumeroBoleto { get => _numeroBoleto; set => SetFormField(ref _numeroBoleto, value); }
    public string ValorBaixa { get => _valorBaixa; set => SetField(ref _valorBaixa, value); }
    public DateTime DataBaixa { get => _dataBaixa; set => SetField(ref _dataBaixa, value); }
    public string FormaBaixa { get => _formaBaixa; set => SetField(ref _formaBaixa, value); }
    public string ObservacoesBaixa { get => _observacoesBaixa; set => SetField(ref _observacoesBaixa, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private void Novo_Click(object sender, RoutedEventArgs e) => NovoTitulo();

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildCreateRequest(out var request, out var error))
        {
            Status = error;
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _api.CreateContaPagarAsync(_terminal, _terminal.DesktopAccessToken, request!);
            if (!result.Success || result.Data is null)
            {
                Status = $"Conta a pagar não gravada. {result.Detail}";
                return;
            }

            var persistedId = result.Data.Id;
            await CarregarAsync(false);
            SelectedTitulo = Titulos.FirstOrDefault(item => item.Id == persistedId);
            Status = $"Conta a pagar CP-{persistedId} confirmada no banco oficial.";
            LimparCadastro();
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void Baixar_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTitulo is null)
        {
            Status = "Selecione uma conta a pagar antes de registrar a baixa.";
            return;
        }

        if (!TryParsePositiveDecimal(ValorBaixa, out var valor))
        {
            Status = "Informe um valor de baixa maior que zero.";
            return;
        }

        SetBusy(true);
        try
        {
            var request = new DesktopBaixaPagarRequest(
                valor,
                DataBaixa,
                NullIfWhiteSpace(FormaBaixa),
                NullIfWhiteSpace(ObservacoesBaixa));
            var result = await _api.BaixarContaPagarAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                SelectedTitulo.Id,
                request);
            if (!result.Success || result.Data is null)
            {
                Status = $"Baixa não gravada. {result.Detail}";
                return;
            }

            var persistedId = result.Data.Id;
            await CarregarAsync(false);
            SelectedTitulo = Titulos.FirstOrDefault(item => item.Id == persistedId);
            ObservacoesBaixa = string.Empty;
            Status = $"Baixa da conta CP-{persistedId} confirmada no banco e no fluxo de caixa.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task CarregarAsync(bool updateStatus = true)
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de operar o financeiro.";
            return;
        }

        SetBusy(true);
        try
        {
            var titlesResult = await _api.GetContasPagarAsync(_terminal, _terminal.DesktopAccessToken);
            if (!titlesResult.Success || titlesResult.Data is null)
            {
                Status = $"Falha ao carregar contas a pagar. {titlesResult.Detail}";
                return;
            }

            Titulos.Clear();
            foreach (var item in titlesResult.Data)
            {
                Titulos.Add(item);
            }

            Auditoria.Clear();
            var auditResult = await _api.GetAuditoriaAsync(_terminal, _terminal.DesktopAccessToken, "erp_contas_pagar");
            if (auditResult.Success && auditResult.Data is not null)
            {
                foreach (var item in auditResult.Data)
                {
                    Auditoria.Add(item);
                }
            }

            NotifyTotals();
            if (updateStatus)
            {
                Status = auditResult.Success
                    ? $"{Titulos.Count} conta(s) a pagar e {Auditoria.Count} evento(s) de auditoria carregados da API oficial."
                    : $"{Titulos.Count} conta(s) a pagar carregadas. Auditoria não autorizada: {auditResult.Detail}";
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool TryBuildCreateRequest(out DesktopContaPagarCreateRequest? request, out string error)
    {
        request = null;
        error = BuildValidationMessage();
        if (!string.IsNullOrEmpty(error))
        {
            return false;
        }

        int? fornecedorId = null;
        if (!string.IsNullOrWhiteSpace(FornecedorId))
        {
            fornecedorId = int.Parse(FornecedorId, CultureInfo.InvariantCulture);
        }

        TryParsePositiveDecimal(ValorOriginal, out var valor);
        request = new DesktopContaPagarCreateRequest(
            NumeroDocumento.Trim(),
            fornecedorId,
            Descricao.Trim(),
            valor,
            DataEmissao,
            DataVencimento,
            NullIfWhiteSpace(FormaPagamento),
            NullIfWhiteSpace(NumeroBoleto));
        return true;
    }

    private string BuildValidationMessage()
    {
        if (string.IsNullOrWhiteSpace(NumeroDocumento)) return "Informe o número do documento.";
        if (string.IsNullOrWhiteSpace(Descricao)) return "Informe a descrição do título.";
        if (!TryParsePositiveDecimal(ValorOriginal, out _)) return "Informe um valor original maior que zero.";
        if (DataVencimento.Date < DataEmissao.Date) return "O vencimento não pode ser anterior à emissão.";
        if (!string.IsNullOrWhiteSpace(FornecedorId)
            && (!int.TryParse(FornecedorId, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id <= 0))
        {
            return "Fornecedor ID deve ser um número positivo quando informado.";
        }

        return string.Empty;
    }

    private void NovoTitulo()
    {
        SelectedTitulo = null;
        LimparCadastro();
        Status = "Novo título de conta a pagar pronto para preenchimento.";
    }

    private void LimparCadastro()
    {
        NumeroDocumento = string.Empty;
        FornecedorId = string.Empty;
        Descricao = string.Empty;
        ValorOriginal = string.Empty;
        DataEmissao = DateTime.Today;
        DataVencimento = DateTime.Today;
        FormaPagamento = string.Empty;
        NumeroBoleto = string.Empty;
    }

    private void SetBusy(bool value)
    {
        _busy = value;
        OnPropertyChanged(nameof(PodeOperar));
    }

    private void NotifyContext()
    {
        OnPropertyChanged(nameof(CodigoContexto));
        OnPropertyChanged(nameof(EstadoContexto));
        OnPropertyChanged(nameof(TituloSelecionadoLabel));
        OnPropertyChanged(nameof(TituloSelecionadoDetalhe));
    }

    private void NotifyTotals()
    {
        OnPropertyChanged(nameof(QuantidadeLabel));
        OnPropertyChanged(nameof(TotalOriginalLabel));
        OnPropertyChanged(nameof(TotalAbertoLabel));
    }

    private bool SetFormField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        var changed = SetField(ref field, value, propertyName);
        if (changed)
        {
            OnPropertyChanged(nameof(ValidacaoCadastro));
        }

        return changed;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static bool TryParsePositiveDecimal(string? value, out decimal result)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) && result > 0m)
        {
            return true;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out result) && result > 0m;
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
