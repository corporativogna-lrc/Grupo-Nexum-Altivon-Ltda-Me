using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class RhReferenceManagementWindow : Window
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopGenesisRhReference? _selected;
    private bool _busy;

    public RhReferenceManagementWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopGenesisRhReference> Referencias { get; } = [];
    public DesktopGenesisRhReference? SelectedReferencia
    {
        get => _selected;
        set
        {
            _selected = value;
            if (value is null) return;
            FormTitle.Text = $"Editar referência · #{value.Id}";
            CodigoBox.Text = value.Codigo;
            CodigoBox.IsEnabled = false;
            DescricaoBox.Text = value.Descricao;
            OrdemBox.Text = value.Ordem.ToString(CultureInfo.InvariantCulture);
        }
    }

    private string SelectedType => (TipoCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "CARGO";

    private async Task<bool> LoadAsync()
    {
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token)) { StatusText.Text = "Sessão Gerente ausente. Nenhum dado foi enviado."; return false; }
        SetBusy(true);
        try
        {
            var response = await _api.GetGenesisRhReferencesAsync(_terminal, token, SelectedType, _lifetime.Token);
            if (!response.Success || response.Data is null) { StatusText.Text = $"Consulta não concluída: {response.Detail}"; return false; }
            var query = BuscaBox.Text.Trim();
            var rows = string.IsNullOrEmpty(query) ? response.Data : response.Data.Where(item =>
                item.Codigo.Contains(query, StringComparison.OrdinalIgnoreCase)
                || item.Descricao.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            Referencias.Clear();
            foreach (var item in rows) Referencias.Add(item);
            ReferenciasGrid.SelectedItem = _selected is null ? null : Referencias.FirstOrDefault(item => item.Id == _selected.Id);
            StatusText.Text = $"{Referencias.Count} referência(s) {SelectedType.ToLowerInvariant()} retornada(s) pela API Genesis.";
            return true;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return false; }
        catch (Exception ex) { StatusText.Text = $"Falha ao consultar referências: {ex.Message}"; return false; }
        finally { SetBusy(false); }
    }

    private async void Atualizar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void Pesquisar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void BuscaBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) await LoadAsync(); }
    private async void TipoCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded) { _selected = null; await LoadAsync(); }
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ReferenciasGrid.SelectedItem = null;
        FormTitle.Text = $"Nova referência · {SelectedType}";
        CodigoBox.IsEnabled = true;
        CodigoBox.Clear(); DescricaoBox.Clear(); OrdemBox.Text = "0";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token)) { StatusText.Text = "Sessão Gerente ausente. Nenhum dado foi enviado."; return; }
        var code = CodigoBox.Text.Trim();
        var description = DescricaoBox.Text.Trim();
        if (code.Length is < 1 or > 50 || description.Length is < 1 or > 120)
        {
            StatusText.Text = "Código (1–50 caracteres) e descrição (1–120 caracteres) são obrigatórios.";
            return;
        }
        if (!int.TryParse(OrdemBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var order) || order < 0)
        {
            StatusText.Text = "Informe uma ordem inteira igual ou maior que zero.";
            OrdemBox.Focus();
            return;
        }
        SetBusy(true);
        try
        {
            var result = await _api.CreateGenesisRhReferenceAsync(_terminal, token,
                new DesktopGenesisRhReferenceRequest(SelectedType, code, description, order), _lifetime.Token);
            if (!result.Success || result.Data is null) { StatusText.Text = $"A API não confirmou a gravação: {result.Detail}"; return; }
            _selected = result.Data;
            await LoadAsync();
            StatusText.Text = $"Referência {result.Data.Codigo} persistida e relida da API Genesis.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex) { StatusText.Text = $"Falha ao salvar referência: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();
    private void SetBusy(bool busy) { _busy = busy; IsEnabled = !busy; }
}
