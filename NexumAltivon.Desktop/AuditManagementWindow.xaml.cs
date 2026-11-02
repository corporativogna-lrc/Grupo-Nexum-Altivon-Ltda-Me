using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class AuditManagementWindow : Window
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private List<DesktopAuditoriaOperacional> _source = [];
    private bool _busy;

    public AuditManagementWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await ConsultarAsync();
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopAuditoriaOperacional> Registros { get; } = [];

    private async void TabelaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) await ConsultarAsync();
    }

    private async void Consultar_Click(object sender, RoutedEventArgs e) => await ConsultarAsync();
    private async void BuscaBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) await ConsultarAsync();
    }

    private async Task ConsultarAsync()
    {
        if (_busy) return;
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            StatusText.Text = "Sessão Gerente ausente. Entre no Desktop antes de consultar a auditoria.";
            return;
        }
        var tabela = (TabelaCombo.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (string.IsNullOrWhiteSpace(tabela))
        {
            StatusText.Text = "Selecione uma origem de auditoria.";
            return;
        }

        SetBusy(true);
        try
        {
            var response = await _api.GetAuditoriaAsync(_terminal, token, tabela, _lifetime.Token);
            if (!response.Success || response.Data is null)
            {
                _source = [];
                AplicarBusca();
                StatusText.Text = $"A consulta não foi confirmada: {response.Detail}";
                return;
            }
            _source = response.Data;
            AplicarBusca();
            StatusText.Text = $"{Registros.Count} evento(s) de {tabela} retornados pela API, entre {response.Data.Count} registros lidos (limite da API: 1.000).";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _source = [];
            AplicarBusca();
            StatusText.Text = $"Falha ao consultar auditoria: {ex.Message}";
        }
        finally { SetBusy(false); }
    }

    private void AplicarBusca()
    {
        var query = BuscaBox.Text.Trim();
        var rows = string.IsNullOrEmpty(query) ? _source : _source.Where(item =>
            item.Acao.Contains(query, StringComparison.OrdinalIgnoreCase)
            || item.UsuarioTipo.Contains(query, StringComparison.OrdinalIgnoreCase)
            || (item.Endpoint?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)
            || item.RegistroId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase)
            || (item.UsuarioId?.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
        Registros.Clear();
        foreach (var item in rows) Registros.Add(item);
    }

    private void SetBusy(bool value)
    {
        _busy = value;
        IsEnabled = !value;
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();
}
