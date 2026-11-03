/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7223 Data: 16/08/2026
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class BaixasPagamentosWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopContaPagar? _selectedContaPagar;
    private DesktopContaReceber? _selectedContaReceber;
    private string _valorPagar = string.Empty;
    private DateTime _dataPagar = DateTime.Today;
    private string _formaPagar = string.Empty;
    private string _observacoesPagar = string.Empty;
    private string _valorReceber = string.Empty;
    private DateTime _dataReceber = DateTime.Today;
    private string _formaReceber = string.Empty;
    private string _observacoesReceber = string.Empty;
    private string _status = "Aguardando carregamento da API oficial.";
    private bool _busy;

    public BaixasPagamentosWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DesktopContaPagar> ContasPagar { get; } = new();
    public ObservableCollection<DesktopContaReceber> ContasReceber { get; } = new();
    public ObservableCollection<DesktopAuditoriaOperacional> AuditoriaPagar { get; } = new();
    public ObservableCollection<DesktopAuditoriaOperacional> AuditoriaReceber { get; } = new();
    public string TenantLabel => $"Loja {_terminal.StoreCode}";
    public bool PodeOperar => !_busy && !string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken);
    public string CodigoContexto => _selectedContaPagar is not null
        ? $"BAIXA-CP-{_selectedContaPagar.Id}"
        : _selectedContaReceber is not null
            ? $"BAIXA-CR-{_selectedContaReceber.Id}"
            : "BAIXAS";
    public string EstadoContexto => _busy ? "PROCESSANDO" : "PRONTO";
    public string ContasPagarLabel => $"{ContasPagar.Count} título(s)";
    public string ContasReceberLabel => $"{ContasReceber.Count} título(s)";
    public string TotalPagarLabel => $"R$ {ContasPagar.Sum(item => item.ValorAberto):N2}";
    public string TotalReceberLabel => $"R$ {ContasReceber.Sum(item => item.ValorAberto):N2}";
    public string ContaPagarSelecionadaLabel => _selectedContaPagar is null
        ? "Selecione um título a pagar."
        : $"{_selectedContaPagar.NumeroDocumento} | aberto R$ {_selectedContaPagar.ValorAberto:N2} | {_selectedContaPagar.Status}";
    public string ContaReceberSelecionadaLabel => _selectedContaReceber is null
        ? "Selecione um título a receber."
        : $"{_selectedContaReceber.NumeroDocumento} | aberto R$ {_selectedContaReceber.ValorAberto:N2} | {_selectedContaReceber.Status}";
    public string ValidacaoPagar => BuildValidationMessage(_selectedContaPagar, ValorPagar, "pagar");
    public string ValidacaoReceber => BuildValidationMessage(_selectedContaReceber, ValorReceber, "receber");
    public string ValorPagar { get => _valorPagar; set => SetFormField(ref _valorPagar, value, nameof(ValidacaoPagar)); }
    public DateTime DataPagar { get => _dataPagar; set => SetField(ref _dataPagar, value); }
    public string FormaPagar { get => _formaPagar; set => SetField(ref _formaPagar, value); }
    public string ObservacoesPagar { get => _observacoesPagar; set => SetField(ref _observacoesPagar, value); }
    public string ValorReceber { get => _valorReceber; set => SetFormField(ref _valorReceber, value, nameof(ValidacaoReceber)); }
    public DateTime DataReceber { get => _dataReceber; set => SetField(ref _dataReceber, value); }
    public string FormaReceber { get => _formaReceber; set => SetField(ref _formaReceber, value); }
    public string ObservacoesReceber { get => _observacoesReceber; set => SetField(ref _observacoesReceber, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }

    public DesktopContaPagar? SelectedContaPagar
    {
        get => _selectedContaPagar;
        set
        {
            if (!SetField(ref _selectedContaPagar, value))
            {
                return;
            }

            if (value is not null)
            {
                ValorPagar = value.ValorAberto.ToString("N2", CultureInfo.CurrentCulture);
                FormaPagar = value.FormaPagamento ?? string.Empty;
                DataPagar = DateTime.Today;
            }

            NotifySelection();
        }
    }

    public DesktopContaReceber? SelectedContaReceber
    {
        get => _selectedContaReceber;
        set
        {
            if (!SetField(ref _selectedContaReceber, value))
            {
                return;
            }

            if (value is not null)
            {
                ValorReceber = value.ValorAberto.ToString("N2", CultureInfo.CurrentCulture);
                FormaReceber = value.FormaRecebimento ?? string.Empty;
                DataReceber = DateTime.Today;
            }

            NotifySelection();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await CarregarAsync();

    private void Window_Closed(object? sender, EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void BaixarPagar_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedContaPagar is null)
        {
            Status = "Selecione uma conta a pagar antes de registrar a baixa.";
            return;
        }

        if (!TryParsePositiveDecimal(ValorPagar, out var valor) || valor > _selectedContaPagar.ValorAberto)
        {
            Status = "Informe um valor de baixa maior que zero e não superior ao valor em aberto.";
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _api.BaixarContaPagarAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                _selectedContaPagar.Id,
                new DesktopBaixaPagarRequest(valor, DataPagar, NullIfWhiteSpace(FormaPagar), NullIfWhiteSpace(ObservacoesPagar)),
                _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Baixa a pagar não gravada. {result.Detail}";
                return;
            }

            var persistedId = result.Data.Id;
            await CarregarAsync(false);
            SelectedContaPagar = ContasPagar.FirstOrDefault(item => item.Id == persistedId);
            Status = $"Baixa da conta a pagar CP-{persistedId} confirmada por releitura do banco oficial.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Status = "Operação cancelada pelo encerramento da tela.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void BaixarReceber_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedContaReceber is null)
        {
            Status = "Selecione uma conta a receber antes de registrar o recebimento.";
            return;
        }

        if (!TryParsePositiveDecimal(ValorReceber, out var valor) || valor > _selectedContaReceber.ValorAberto)
        {
            Status = "Informe um valor de recebimento maior que zero e não superior ao valor em aberto.";
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _api.BaixarContaReceberAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                _selectedContaReceber.Id,
                new DesktopBaixaReceberRequest(valor, DataReceber, NullIfWhiteSpace(FormaReceber), NullIfWhiteSpace(ObservacoesReceber)),
                _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Recebimento não gravado. {result.Detail}";
                return;
            }

            var persistedId = result.Data.Id;
            await CarregarAsync(false);
            SelectedContaReceber = ContasReceber.FirstOrDefault(item => item.Id == persistedId);
            Status = $"Recebimento da conta a receber CR-{persistedId} confirmado por releitura do banco oficial.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Status = "Operação cancelada pelo encerramento da tela.";
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
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de operar as baixas.";
            return;
        }

        SetBusy(true);
        try
        {
            var pagarTask = _api.GetContasPagarAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
            var receberTask = _api.GetContasReceberAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
            await Task.WhenAll(pagarTask, receberTask);
            var pagar = await pagarTask;
            var receber = await receberTask;
            if (!pagar.Success || pagar.Data is null || !receber.Success || receber.Data is null)
            {
                Status = $"Falha ao carregar baixas. A pagar: {pagar.Detail}; A receber: {receber.Detail}";
                return;
            }

            ContasPagar.Clear();
            foreach (var item in pagar.Data)
            {
                ContasPagar.Add(item);
            }

            ContasReceber.Clear();
            foreach (var item in receber.Data)
            {
                ContasReceber.Add(item);
            }

            var auditoriaPagarTask = _api.GetAuditoriaAsync(_terminal, _terminal.DesktopAccessToken, "erp_contas_pagar", _lifetime.Token);
            var auditoriaReceberTask = _api.GetAuditoriaAsync(_terminal, _terminal.DesktopAccessToken, "erp_contas_receber", _lifetime.Token);
            await Task.WhenAll(auditoriaPagarTask, auditoriaReceberTask);
            var auditoriaPagar = await auditoriaPagarTask;
            var auditoriaReceber = await auditoriaReceberTask;
            AuditoriaPagar.Clear();
            if (auditoriaPagar.Success && auditoriaPagar.Data is not null)
            {
                foreach (var item in auditoriaPagar.Data)
                {
                    AuditoriaPagar.Add(item);
                }
            }

            AuditoriaReceber.Clear();
            if (auditoriaReceber.Success && auditoriaReceber.Data is not null)
            {
                foreach (var item in auditoriaReceber.Data)
                {
                    AuditoriaReceber.Add(item);
                }
            }

            NotifyCollections();
            if (updateStatus)
            {
                Status = $"{ContasPagar.Count} conta(s) a pagar e {ContasReceber.Count} conta(s) a receber carregadas da API oficial.";
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Status = "Carregamento cancelado pelo encerramento da tela.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool value)
    {
        _busy = value;
        Notify(nameof(PodeOperar));
        Notify(nameof(EstadoContexto));
    }

    private void NotifySelection()
    {
        Notify(nameof(CodigoContexto));
        Notify(nameof(EstadoContexto));
        Notify(nameof(ContaPagarSelecionadaLabel));
        Notify(nameof(ContaReceberSelecionadaLabel));
        Notify(nameof(ValidacaoPagar));
        Notify(nameof(ValidacaoReceber));
    }

    private void NotifyCollections()
    {
        Notify(nameof(ContasPagarLabel));
        Notify(nameof(ContasReceberLabel));
        Notify(nameof(TotalPagarLabel));
        Notify(nameof(TotalReceberLabel));
    }

    private void SetFormField<T>(ref T field, T value, string validationProperty, [CallerMemberName] string? propertyName = null)
    {
        if (SetField(ref field, value, propertyName))
        {
            Notify(validationProperty);
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        Notify(propertyName);
        return true;
    }

    private void Notify([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static string BuildValidationMessage<T>(T? selected, string value, string operation)
        where T : class
    {
        if (selected is null)
        {
            return $"Selecione uma conta a {operation} antes de confirmar.";
        }

        return TryParsePositiveDecimal(value, out _)
            ? string.Empty
            : $"Informe o valor a {operation} maior que zero.";
    }

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
