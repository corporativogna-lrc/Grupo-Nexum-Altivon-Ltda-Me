/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class ConciliacaoFinanceiraWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private DesktopConciliacaoFinanceira? _selectedLancamento;
    private DateTime? _filtroInicio = DateTime.Today.AddDays(-90);
    private DateTime? _filtroFim = DateTime.Today;
    private string _filtroStatus = "TODOS";
    private string _statusConciliacao = "PENDENTE";
    private string _referenciaBancaria = string.Empty;
    private string _observacoes = string.Empty;
    private string _status = "Aguardando carregamento da API oficial.";
    private bool _busy;

    public ConciliacaoFinanceiraWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DesktopConciliacaoFinanceira> Lancamentos { get; } = new();
    public ObservableCollection<DesktopAuditoriaOperacional> Auditoria { get; } = new();
    public IReadOnlyList<string> FiltroStatusOptions { get; } = ["TODOS", "PENDENTE", "CONCILIADO", "DIVERGENTE", "IGNORADO"];
    public IReadOnlyList<string> StatusOptions { get; } = ["PENDENTE", "CONCILIADO", "DIVERGENTE", "IGNORADO"];

    public string TenantLabel => $"Loja {_terminal.StoreCode}";
    public bool PodeOperar => !_busy;
    public bool PodeConciliar => !_busy && SelectedLancamento?.DataPagamento.HasValue == true;
    public string CodigoContexto => SelectedLancamento is null ? "CONCILIAÇÃO" : $"FIN-{SelectedLancamento.LancamentoFinanceiroId}";
    public string EstadoContexto => SelectedLancamento?.Status ?? "CONSULTA";
    public string QuantidadeLabel => $"{Lancamentos.Count} lançamento(s)";
    public string ConciliadosLabel => Lancamentos.Count(item => item.Status == "CONCILIADO").ToString();
    public string DivergentesLabel => Lancamentos.Count(item => item.Status == "DIVERGENTE").ToString();
    public string PendentesLabel => Lancamentos.Count(item => item.Status == "PENDENTE").ToString();
    public string LancamentoSelecionadoLabel => SelectedLancamento is null
        ? "Selecione um lançamento pago"
        : $"FIN-{SelectedLancamento.LancamentoFinanceiroId} | R$ {SelectedLancamento.Valor:N2}";
    public string LancamentoSelecionadoDetalhe => SelectedLancamento is null
        ? string.Empty
        : $"{SelectedLancamento.Descricao} | pagamento {SelectedLancamento.DataPagamento:dd/MM/yyyy} | {SelectedLancamento.ContaBancaria}";
    public string Validacao => BuildValidationMessage();

    public DesktopConciliacaoFinanceira? SelectedLancamento
    {
        get => _selectedLancamento;
        set
        {
            if (!SetField(ref _selectedLancamento, value))
            {
                return;
            }

            StatusConciliacao = value?.Status is "PENDENTE" or "CONCILIADO" or "DIVERGENTE" or "IGNORADO"
                ? value.Status
                : "PENDENTE";
            ReferenciaBancaria = value?.ReferenciaBancaria ?? string.Empty;
            Observacoes = value?.Observacoes ?? string.Empty;
            NotifySelection();
        }
    }

    public DateTime? FiltroInicio { get => _filtroInicio; set => SetField(ref _filtroInicio, value); }
    public DateTime? FiltroFim { get => _filtroFim; set => SetField(ref _filtroFim, value); }
    public string FiltroStatus { get => _filtroStatus; set => SetField(ref _filtroStatus, value); }
    public string StatusConciliacao { get => _statusConciliacao; set => SetEditorField(ref _statusConciliacao, value); }
    public string ReferenciaBancaria { get => _referenciaBancaria; set => SetEditorField(ref _referenciaBancaria, value); }
    public string Observacoes { get => _observacoes; set => SetEditorField(ref _observacoes, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void AplicarFiltros_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        var validation = BuildValidationMessage();
        if (!string.IsNullOrWhiteSpace(validation) || SelectedLancamento is null)
        {
            Status = validation;
            return;
        }

        SetBusy(true);
        try
        {
            var request = new DesktopConciliacaoFinanceiraRequest(
                SelectedLancamento.LancamentoFinanceiroId,
                StatusConciliacao,
                NullIfWhiteSpace(ReferenciaBancaria),
                NullIfWhiteSpace(Observacoes),
                SelectedLancamento.RowVersion);
            var result = await _api.SaveConciliacaoFinanceiraAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                request);
            if (!result.Success || result.Data is null)
            {
                Status = $"Conciliação não gravada. {result.Detail}";
                return;
            }

            var id = result.Data.LancamentoFinanceiroId;
            await CarregarAsync(false);
            SelectedLancamento = Lancamentos.FirstOrDefault(item => item.LancamentoFinanceiroId == id);
            Status = $"Conciliação FIN-{id} confirmada no banco oficial e na trilha de auditoria.";
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
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de operar a conciliação.";
            return;
        }

        if (FiltroInicio.HasValue && FiltroFim.HasValue && FiltroFim.Value.Date < FiltroInicio.Value.Date)
        {
            Status = "A data final do filtro não pode ser anterior à data inicial.";
            return;
        }

        var selectedId = SelectedLancamento?.LancamentoFinanceiroId;
        SetBusy(true);
        try
        {
            var statusFilter = FiltroStatus == "TODOS" ? null : FiltroStatus;
            var result = await _api.GetConciliacoesFinanceirasAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                FiltroInicio,
                FiltroFim,
                statusFilter);
            if (!result.Success || result.Data is null)
            {
                Status = $"Falha ao carregar conciliações. {result.Detail}";
                return;
            }

            Lancamentos.Clear();
            foreach (var item in result.Data)
            {
                Lancamentos.Add(item);
            }

            Auditoria.Clear();
            var auditResult = await _api.GetAuditoriaAsync(_terminal, _terminal.DesktopAccessToken, "ctb_conciliacoes");
            if (auditResult.Success && auditResult.Data is not null)
            {
                foreach (var item in auditResult.Data)
                {
                    Auditoria.Add(item);
                }
            }

            SelectedLancamento = selectedId.HasValue
                ? Lancamentos.FirstOrDefault(item => item.LancamentoFinanceiroId == selectedId.Value)
                : null;
            NotifyTotals();
            if (updateStatus)
            {
                Status = auditResult.Success
                    ? $"{Lancamentos.Count} lançamento(s) e {Auditoria.Count} evento(s) carregados da API oficial."
                    : $"{Lancamentos.Count} lançamento(s) carregados. Auditoria não autorizada: {auditResult.Detail}";
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    private string BuildValidationMessage()
    {
        if (SelectedLancamento is null) return "Selecione um lançamento financeiro.";
        if (!SelectedLancamento.DataPagamento.HasValue) return "Somente lançamento pago pode ser conciliado.";
        if (!StatusOptions.Contains(StatusConciliacao)) return "Selecione um resultado de conciliação válido.";
        if (StatusConciliacao == "CONCILIADO" && string.IsNullOrWhiteSpace(ReferenciaBancaria))
        {
            return "Informe a referência bancária para concluir a conciliação.";
        }

        return string.Empty;
    }

    private void SetBusy(bool value)
    {
        _busy = value;
        OnPropertyChanged(nameof(PodeOperar));
        OnPropertyChanged(nameof(PodeConciliar));
    }

    private void NotifySelection()
    {
        OnPropertyChanged(nameof(CodigoContexto));
        OnPropertyChanged(nameof(EstadoContexto));
        OnPropertyChanged(nameof(PodeConciliar));
        OnPropertyChanged(nameof(LancamentoSelecionadoLabel));
        OnPropertyChanged(nameof(LancamentoSelecionadoDetalhe));
        OnPropertyChanged(nameof(Validacao));
    }

    private void NotifyTotals()
    {
        OnPropertyChanged(nameof(QuantidadeLabel));
        OnPropertyChanged(nameof(ConciliadosLabel));
        OnPropertyChanged(nameof(DivergentesLabel));
        OnPropertyChanged(nameof(PendentesLabel));
    }

    private void SetEditorField(ref string field, string value, [CallerMemberName] string? propertyName = null)
    {
        if (SetField(ref field, value, propertyName))
        {
            OnPropertyChanged(nameof(Validacao));
        }
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

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
