using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class CostCenterManagementWindow : Window
{
    private const string ReferenceType = "CENTRO_CUSTO";
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopGenesisFinanceReference? _selected;
    private bool _busy;

    public CostCenterManagementWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopGenesisFinanceReference> Centros { get; } = [];
    public DesktopGenesisFinanceReference? SelectedCentro
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

    private async Task<bool> LoadAsync()
    {
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            StatusText.Text = "Sessão Gerente ausente. Entre no Desktop antes de consultar centros de custo.";
            return false;
        }
        SetBusy(true);
        try
        {
            var response = await _api.GetGenesisFinanceReferencesAsync(_terminal, token, ReferenceType, _lifetime.Token);
            if (!response.Success || response.Data is null)
            {
                StatusText.Text = $"Consulta financeira não concluída: {response.Detail}";
                return false;
            }
            var query = BuscaBox.Text.Trim();
            var rows = string.IsNullOrEmpty(query) ? response.Data : response.Data.Where(item =>
                item.Codigo.Contains(query, StringComparison.OrdinalIgnoreCase)
                || item.Descricao.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();
            Centros.Clear();
            foreach (var item in rows) Centros.Add(item);
            CentrosGrid.SelectedItem = _selected is null ? null : Centros.FirstOrDefault(item => item.Id == _selected.Id);
            StatusText.Text = $"{Centros.Count} centro(s) de custo retornados pela API Genesis.";
            return true;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return false; }
        catch (Exception ex) { StatusText.Text = $"Falha ao consultar centros de custo: {ex.Message}"; return false; }
        finally { SetBusy(false); }
    }

    private async void Atualizar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void Pesquisar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void BuscaBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) await LoadAsync(); }
    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        CentrosGrid.SelectedItem = null;
        FormTitle.Text = "Novo centro de custo";
        CodigoBox.IsEnabled = true;
        CodigoBox.Clear(); DescricaoBox.Clear(); OrdemBox.Text = "0";
        StatusText.Text = "Preencha a referência e salve pela API Genesis.";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token)) { StatusText.Text = "Sessão Gerente ausente. Nenhum dado foi enviado."; return; }
        var code = CodigoBox.Text.Trim();
        var description = DescricaoBox.Text.Trim();
        if (code.Length is < 1 or > 50 || description.Length is < 1 or > 150)
        {
            StatusText.Text = "Código (1–50 caracteres) e descrição (1–150 caracteres) são obrigatórios.";
            return;
        }
        if (!int.TryParse(OrdemBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var order) || order < 0)
        {
            StatusText.Text = "Informe uma ordem de exibição inteira igual ou maior que zero.";
            OrdemBox.Focus();
            return;
        }
        SetBusy(true);
        try
        {
            var result = await _api.CreateGenesisFinanceReferenceAsync(_terminal, token,
                new DesktopGenesisFinanceReferenceRequest(ReferenceType, code, description, order), _lifetime.Token);
            if (!result.Success || result.Data is null) { StatusText.Text = $"A API não confirmou a gravação: {result.Detail}"; return; }
            _selected = result.Data;
            await LoadAsync();
            StatusText.Text = $"Centro de custo {result.Data.Codigo} persistido e relido da API Genesis.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex) { StatusText.Text = $"Falha ao salvar centro de custo: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();
    private void SetBusy(bool busy) { _busy = busy; IsEnabled = !busy; }
}
