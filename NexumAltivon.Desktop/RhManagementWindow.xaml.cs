using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class RhManagementWindow : Window
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private readonly string? _statusFilter;
    private readonly bool _newAdmissionMode;
    private DesktopGenesisRhColaborador? _selected;
    private bool _busy;

    public RhManagementWindow(TerminalProfile terminal, string? operation = null)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        _statusFilter = string.Equals(operation, "Demissões", StringComparison.OrdinalIgnoreCase) ? "DESLIGADO" : null;
        _newAdmissionMode = string.Equals(operation, "Contratações", StringComparison.OrdinalIgnoreCase);
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) =>
        {
            await LoadAsync();
            if (_newAdmissionMode)
            {
                Novo_Click(this, new RoutedEventArgs());
                FormTitle.Text = "Admitir colaborador";
                DataAdmissaoPicker.SelectedDate = DateTime.Today;
            }
            else if (_statusFilter is not null)
            {
                StatusText.Text = "Exibindo colaboradores desligados; selecione um registro para revisar ou atualizar.";
            }
        };
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopGenesisRhColaborador> Colaboradores { get; } = [];
    public DesktopGenesisRhColaborador? SelectedColaborador
    {
        get => _selected;
        set
        {
            _selected = value;
            if (value is null) return;
            FormTitle.Text = $"Editar colaborador · #{value.Id}";
            NomeBox.Text = value.Nome;
            CargoBox.Text = value.Cargo ?? string.Empty;
            DepartamentoBox.Text = value.Departamento ?? string.Empty;
            var status = value.Status ?? "ATIVO";
            StatusCombo.SelectedItem = StatusCombo.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Content?.ToString(), status, StringComparison.OrdinalIgnoreCase));
            DataAdmissaoPicker.SelectedDate = value.DataAdmissao;
        }
    }

    private async Task<bool> LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            StatusText.Text = "Sessão Gerente ausente. Entre no Desktop antes de consultar o cadastro de RH.";
            return false;
        }
        SetBusy(true);
        try
        {
            var response = await _api.GetGenesisRhColaboradoresAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
            if (!response.Success || response.Data is null)
            {
                StatusText.Text = $"Consulta de RH não concluída: {response.Detail}";
                return false;
            }

            var search = BuscaBox.Text.Trim();
            var rows = string.IsNullOrEmpty(search)
                ? response.Data
                : response.Data.Where(item => item.Nome.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || (item.Cargo?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (item.Departamento?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();
            if (_statusFilter is not null)
                rows = rows.Where(item => string.Equals(item.Status, _statusFilter, StringComparison.OrdinalIgnoreCase)).ToList();
            Colaboradores.Clear();
            foreach (var item in rows) Colaboradores.Add(item);
            ColaboradoresGrid.SelectedItem = _selected is null ? null : Colaboradores.FirstOrDefault(item => item.Id == _selected.Id);
            StatusText.Text = $"{Colaboradores.Count} colaborador(es) retornados pela API Genesis{(_statusFilter is null ? string.Empty : $" com status {_statusFilter}")}.";
            return true;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return false; }
        catch (Exception ex)
        {
            StatusText.Text = $"Falha ao consultar RH: {ex.Message}";
            return false;
        }
        finally { SetBusy(false); }
    }

    private async void Atualizar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void Pesquisar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void BuscaBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) await LoadAsync();
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        ColaboradoresGrid.SelectedItem = null;
        FormTitle.Text = "Novo colaborador";
        NomeBox.Clear(); CargoBox.Clear(); DepartamentoBox.Clear();
        StatusCombo.SelectedIndex = 0;
        DataAdmissaoPicker.SelectedDate = null;
        StatusText.Text = "Preencha os campos operacionais e salve pela API Genesis.";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            StatusText.Text = "Sessão Gerente ausente. Nenhum dado foi enviado.";
            return;
        }
        var nome = NomeBox.Text.Trim();
        if (nome.Length is < 2 or > 180)
        {
            StatusText.Text = "Informe um nome completo com 2 a 180 caracteres.";
            NomeBox.Focus();
            return;
        }
        var status = (StatusCombo.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "ATIVO";
        var request = new DesktopGenesisRhColaboradorRequest(nome, NullIfBlank(CargoBox.Text),
            NullIfBlank(DepartamentoBox.Text), status, DataAdmissaoPicker.SelectedDate);
        SetBusy(true);
        try
        {
            var result = _selected is null
                ? await _api.CreateGenesisRhColaboradorAsync(_terminal, token, request, _lifetime.Token)
                : await _api.UpdateGenesisRhColaboradorAsync(_terminal, token, _selected.Id, request, _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                StatusText.Text = $"A API não confirmou a gravação: {result.Detail}";
                return;
            }

            _selected = result.Data;
            await LoadAsync();
            StatusText.Text = $"Colaborador #{result.Data.Id} persistido e relido da API Genesis.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex) { StatusText.Text = $"Falha ao salvar colaborador: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();
    private void SetBusy(bool busy)
    {
        _busy = busy;
        IsEnabled = !busy;
        if (!busy && string.IsNullOrWhiteSpace(StatusText.Text)) StatusText.Text = "Pronto.";
    }
    private static string? NullIfBlank(string value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
