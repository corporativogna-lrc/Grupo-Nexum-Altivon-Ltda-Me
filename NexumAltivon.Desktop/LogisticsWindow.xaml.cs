/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7182
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class LogisticsWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private DesktopLogisticaExpedicao? _selectedExpedicao;
    private string _freteMetodo = string.Empty;
    private string _freteTransportadora = string.Empty;
    private string _freteCodigoRastreio = string.Empty;
    private string _fretePrazoDias = string.Empty;
    private string _status = "Aguardando carregamento da API oficial.";
    private bool _busy;

    public LogisticsWindow(string operationType, TerminalProfile terminal)
    {
        _ = operationType;
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DesktopLogisticaExpedicao> Expedicoes { get; } = new();
    public ObservableCollection<DesktopLogisticaRastreamentoEvento> Eventos { get; } = new();
    public string TenantLabel => $"Loja {_terminal.StoreCode}";
    public bool PodeOperar => !_busy;
    public string QuantidadeLabel => $"{Expedicoes.Count} pedido(s)";
    public string CodigoContexto => SelectedExpedicao is null ? "EXPEDIÇÕES" : $"PEDIDO {SelectedExpedicao.NumeroPedido}";
    public string EstadoContexto => SelectedExpedicao?.Status ?? "AGUARDANDO SELEÇÃO";
    public string PedidoSelecionadoLabel => SelectedExpedicao is null
        ? "Selecione um pedido na grade."
        : $"{SelectedExpedicao.ClienteNome} | atualizado em {SelectedExpedicao.UpdatedAt:dd/MM/yyyy HH:mm}";

    public DesktopLogisticaExpedicao? SelectedExpedicao
    {
        get => _selectedExpedicao;
        set
        {
            if (!SetField(ref _selectedExpedicao, value))
            {
                return;
            }

            FreteMetodo = value?.FreteMetodo ?? string.Empty;
            FreteTransportadora = value?.FreteTransportadora ?? string.Empty;
            FreteCodigoRastreio = value?.FreteCodigoRastreio ?? string.Empty;
            FretePrazoDias = value?.FretePrazoDias > 0
                ? value.FretePrazoDias.ToString(CultureInfo.InvariantCulture)
                : string.Empty;
            Eventos.Clear();
            NotifyContext();
        }
    }

    public string FreteMetodo { get => _freteMetodo; set => SetField(ref _freteMetodo, value); }
    public string FreteTransportadora { get => _freteTransportadora; set => SetField(ref _freteTransportadora, value); }
    public string FreteCodigoRastreio { get => _freteCodigoRastreio; set => SetField(ref _freteCodigoRastreio, value); }
    public string FretePrazoDias { get => _fretePrazoDias; set => SetField(ref _fretePrazoDias, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await CarregarAsync();
    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedExpedicao is null)
        {
            Status = "Selecione um pedido antes de salvar a logística.";
            return;
        }

        if (string.IsNullOrWhiteSpace(FreteMetodo)
            || string.IsNullOrWhiteSpace(FreteTransportadora)
            || string.IsNullOrWhiteSpace(FreteCodigoRastreio))
        {
            Status = "Método, transportadora e código de rastreio real são obrigatórios.";
            return;
        }

        if (!int.TryParse(FretePrazoDias, NumberStyles.None, CultureInfo.InvariantCulture, out var prazoDias)
            || prazoDias is < 1 or > 120)
        {
            Status = "Prazo deve ser um número inteiro entre 1 e 120 dias.";
            return;
        }

        var selectedId = SelectedExpedicao.PedidoId;
        SetBusy(true);
        try
        {
            var request = new DesktopPedidoLogisticaRequest(
                FreteMetodo.Trim(),
                FreteTransportadora.Trim(),
                prazoDias,
                FreteCodigoRastreio.Trim(),
                SelectedExpedicao.RowVersion);
            var result = await _api.UpdatePedidoLogisticaAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                selectedId,
                request);
            if (!result.Success || result.Data is null)
            {
                Status = $"Logística não gravada. {result.Detail}";
                return;
            }

            await CarregarAsync(false, selectedId);
            Status = $"Logística do pedido {result.Data.NumeroPedido} confirmada no banco oficial.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void AvancarFluxo_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedExpedicao is null)
        {
            Status = "Selecione um pedido antes de avançar o fluxo.";
            return;
        }

        var selectedId = SelectedExpedicao.PedidoId;
        SetBusy(true);
        try
        {
            var result = await _api.AdvancePedidoFluxoAsync(_terminal, _terminal.DesktopAccessToken, selectedId);
            if (!result.Success || result.Data is null)
            {
                Status = $"Fluxo não avançado. {result.Detail}";
                return;
            }

            await CarregarAsync(false, selectedId);
            Status = $"Pedido {result.Data.NumeroPedido} avançado para {result.Data.Status} com persistência operacional.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void ConsultarRastreamento_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedExpedicao is null || string.IsNullOrWhiteSpace(FreteCodigoRastreio))
        {
            Status = "Selecione uma expedição com código de rastreio salvo.";
            return;
        }

        SetBusy(true);
        try
        {
            Eventos.Clear();
            var result = await _api.GetLogisticaRastreamentoAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                FreteCodigoRastreio);
            if (!result.Success || result.Data is null)
            {
                await CarregarAsync(false, SelectedExpedicao.PedidoId);
                Status = $"Consulta externa não confirmada; a tentativa foi auditada pela API. {result.Detail}";
                return;
            }

            var selectedId = SelectedExpedicao.PedidoId;
            await CarregarAsync(false, selectedId);
            foreach (var item in result.Data.Eventos)
            {
                Eventos.Add(item);
            }

            Status = $"Consulta {result.Data.ConsultaId} confirmada em {result.Data.FonteExterna}; status {result.Data.StatusExterno ?? "sem status textual"}.";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();

    private async Task CarregarAsync(bool updateStatus = true, int? selectedId = null, bool clearEvents = true)
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de operar a logística.";
            return;
        }

        var previousId = selectedId ?? SelectedExpedicao?.PedidoId;
        SetBusy(true);
        try
        {
            var result = await _api.GetLogisticaExpedicoesAsync(_terminal, _terminal.DesktopAccessToken);
            if (!result.Success || result.Data is null)
            {
                Status = $"Expedições não carregadas. {result.Detail}";
                return;
            }

            Expedicoes.Clear();
            foreach (var item in result.Data)
            {
                Expedicoes.Add(item);
            }

            SelectedExpedicao = previousId.HasValue
                ? Expedicoes.FirstOrDefault(item => item.PedidoId == previousId.Value)
                : Expedicoes.FirstOrDefault();
            if (clearEvents)
            {
                Eventos.Clear();
            }

            OnPropertyChanged(nameof(QuantidadeLabel));
            if (updateStatus)
            {
                Status = $"{Expedicoes.Count} expedição(ões) carregada(s) do banco oficial.";
            }
        }
        finally
        {
            SetBusy(false);
        }
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
        OnPropertyChanged(nameof(PedidoSelecionadoLabel));
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
}
