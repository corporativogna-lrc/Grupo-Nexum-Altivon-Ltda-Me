/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7202
 * Origem funcional: adaptação auditada do LURCO432 para o Desktop WPF
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class LeadsQualificacaoWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly TerminalProfile? _terminal;
    private List<DesktopLead> _leads = [];
    private string _busca = string.Empty;
    private string _statusFiltro = "Todos";
    private string _novoStatus = "Novo";
    private string _status = "Aguardando consulta da API oficial.";
    private DesktopLead? _leadSelecionado;

    public LeadsQualificacaoWindow(TerminalProfile? terminal)
    {
        _terminal = terminal;
        StatusFiltroOpcoes = ["Todos", "Novo", "Em atendimento", "Qualificado", "Convertido", "Perdido", "Arquivado"];
        StatusOpcoes = StatusFiltroOpcoes.Skip(1).ToList();
        InitializeComponent();
        DataContext = this;
        Loaded += Window_Loaded;
    }

    public ObservableCollection<DesktopLead> LeadsVisiveis { get; } = [];
    public IReadOnlyList<string> StatusFiltroOpcoes { get; }
    public IReadOnlyList<string> StatusOpcoes { get; }

    public string Busca
    {
        get => _busca;
        set
        {
            if (SetField(ref _busca, value))
            {
                AplicarFiltros();
            }
        }
    }

    public string StatusFiltro
    {
        get => _statusFiltro;
        set
        {
            if (SetField(ref _statusFiltro, value))
            {
                AplicarFiltros();
            }
        }
    }

    public string NovoStatus
    {
        get => _novoStatus;
        set => SetField(ref _novoStatus, value);
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string Resumo => $"{LeadsVisiveis.Count} de {_leads.Count} lead(s) exibido(s)";

    public DesktopLead? LeadSelecionado
    {
        get => _leadSelecionado;
        set
        {
            if (SetField(ref _leadSelecionado, value) && value is not null)
            {
                NovoStatus = StatusOpcoes.FirstOrDefault(item => NormalizarStatus(item) == NormalizarStatus(value.Status)) ?? "Novo";
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Loaded -= Window_Loaded;
        await CarregarAsync();
    }

    private async void Atualizar_Click(object sender, RoutedEventArgs e)
        => await CarregarAsync();

    private void Limpar_Click(object sender, RoutedEventArgs e)
    {
        Busca = string.Empty;
        StatusFiltro = "Todos";
        LeadSelecionado = null;
        NovoStatus = "Novo";
        Status = "Filtros limpos. Consulte a API para atualizar os dados.";
    }

    private async void GravarStatus_Click(object sender, RoutedEventArgs e)
    {
        if (_terminal is null)
        {
            Status = "Terminal não inicializado. A atualização não foi enviada.";
            return;
        }

        if (LeadSelecionado is null)
        {
            Status = "Selecione um lead antes de gravar o status.";
            return;
        }

        if (string.IsNullOrWhiteSpace(NovoStatus))
        {
            Status = "Selecione um status válido.";
            return;
        }

        var leadId = LeadSelecionado.Id;
        Status = $"Gravando status do lead {leadId}...";
        try
        {
            var result = await _api.UpdateLeadStatusAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                leadId,
                new DesktopLeadStatusUpdateRequest(NovoStatus, null),
                _lifetime.Token);

            if (!result.Success || result.Data is null)
            {
                Status = $"A API não confirmou a alteração: {result.Detail}";
                return;
            }

            await CarregarAsync(leadId);
            Status = $"Status do lead {leadId} gravado e relido pela API oficial.";
        }
        catch (OperationCanceledException)
        {
            Status = "Atualização cancelada pelo fechamento da janela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha ao gravar status: {ex.Message}";
        }
    }

    private async Task CarregarAsync(int? selecionarId = null)
    {
        if (_terminal is null)
        {
            Status = "Terminal não inicializado. A consulta não foi enviada.";
            return;
        }

        Status = "Consultando /api/crm/leads...";
        try
        {
            var result = await _api.GetLeadsAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"A API não confirmou a consulta: {result.Detail}";
                return;
            }

            _leads = result.Data.OrderByDescending(item => item.CreatedAt).ToList();
            AplicarFiltros();
            LeadSelecionado = selecionarId.HasValue
                ? LeadsVisiveis.FirstOrDefault(item => item.Id == selecionarId.Value)
                : LeadsVisiveis.FirstOrDefault();
            Status = $"Consulta confirmada pela API oficial. {_leads.Count} lead(s) do tenant atual.";
        }
        catch (OperationCanceledException)
        {
            Status = "Consulta cancelada pelo fechamento da janela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha na consulta: {ex.Message}";
        }
    }

    private void AplicarFiltros()
    {
        var busca = Busca.Trim();
        var status = NormalizarStatus(StatusFiltro);
        var filtrados = _leads.Where(item =>
            (status == "todos" || NormalizarStatus(item.Status) == status)
            && (string.IsNullOrWhiteSpace(busca)
                || item.Id.ToString().Contains(busca, StringComparison.OrdinalIgnoreCase)
                || item.Nome.Contains(busca, StringComparison.OrdinalIgnoreCase)
                || item.Email.Contains(busca, StringComparison.OrdinalIgnoreCase)
                || (item.Empresa?.Contains(busca, StringComparison.OrdinalIgnoreCase) ?? false))).ToList();

        LeadsVisiveis.Clear();
        foreach (var lead in filtrados)
        {
            LeadsVisiveis.Add(lead);
        }

        OnPropertyChanged(nameof(Resumo));
    }

    private static string NormalizarStatus(string value)
        => value.Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("-", string.Empty, StringComparison.Ordinal)
            .Replace("_", string.Empty, StringComparison.Ordinal)
            .ToLowerInvariant();

    protected override void OnClosed(EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        base.OnClosed(e);
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
