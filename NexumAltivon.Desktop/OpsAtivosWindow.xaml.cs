/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7209
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class OpsAtivosWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopOpsAtivo? _selectedAtivo;
    private string _status = "Aguardando carregamento da API oficial.";

    public OpsAtivosWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        Closed += (_, _) => _lifetime.Cancel();
    }

    public ObservableCollection<DesktopOpsAtivo> Ativos { get; } = new();

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string SelectedDetails => _selectedAtivo is null
        ? "Nenhum ativo selecionado."
        : $"Ativo #{_selectedAtivo.Id} | {_selectedAtivo.Codigo} | criado em {_selectedAtivo.CriadoEm:dd/MM/yyyy HH:mm} | atualizado em {_selectedAtivo.AtualizadoEm:dd/MM/yyyy HH:mm}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void Carregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void StatusFiltro_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            await CarregarAsync();
        }
    }

    private void AtivosGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedAtivo = AtivosGrid.SelectedItem as DesktopOpsAtivo;
        if (_selectedAtivo is not null)
        {
            CodigoBox.Text = _selectedAtivo.Codigo;
            NomeBox.Text = _selectedAtivo.Nome;
            SelectComboItem(TipoBox, _selectedAtivo.Tipo);
            SelectComboItem(StatusBox, _selectedAtivo.Status);
            LocalizacaoBox.Text = _selectedAtivo.Localizacao ?? string.Empty;
            FabricanteBox.Text = _selectedAtivo.Fabricante ?? string.Empty;
            ModeloBox.Text = _selectedAtivo.Modelo ?? string.Empty;
            NumeroSerieBox.Text = _selectedAtivo.NumeroSerie ?? string.Empty;
            ProximaManutencaoPicker.SelectedDate = _selectedAtivo.ProximaManutencao?.ToLocalTime().Date;
        }

        NotifyContext();
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        _selectedAtivo = null;
        CodigoBox.Clear();
        NomeBox.Clear();
        SelectComboItem(TipoBox, "EQUIPAMENTO");
        SelectComboItem(StatusBox, "ATIVO");
        LocalizacaoBox.Clear();
        FabricanteBox.Clear();
        ModeloBox.Clear();
        NumeroSerieBox.Clear();
        ProximaManutencaoPicker.SelectedDate = null;
        AtivosGrid.SelectedItem = null;
        Status = "Novo ativo pronto. Nenhuma alteração foi gravada.";
        NotifyContext();
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CodigoBox.Text) || string.IsNullOrWhiteSpace(NomeBox.Text))
        {
            Status = "Código e nome do ativo são obrigatórios.";
            return;
        }

        var request = new DesktopOpsAtivoRequest(
            CodigoBox.Text.Trim(),
            NomeBox.Text.Trim(),
            SelectedComboValue(TipoBox) ?? "EQUIPAMENTO",
            NullIfWhiteSpace(LocalizacaoBox.Text),
            SelectedComboValue(StatusBox) ?? "ATIVO",
            NullIfWhiteSpace(FabricanteBox.Text),
            NullIfWhiteSpace(ModeloBox.Text),
            NullIfWhiteSpace(NumeroSerieBox.Text),
            ProximaManutencaoPicker.SelectedDate?.ToUniversalTime());

        Status = _selectedAtivo is null
            ? "Gravando ativo na API oficial..."
            : "Atualizando ativo na API oficial...";
        var result = await _api.SaveOpsAtivoAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            _selectedAtivo?.Id,
            request,
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Ativo não persistido: {result.Detail}";
            return;
        }

        var persistedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura falhou. A operação não será tratada como concluída.";
            return;
        }

        _selectedAtivo = Ativos.FirstOrDefault(item => item.Id == persistedId);
        AtivosGrid.SelectedItem = _selectedAtivo;
        Status = $"Ativo #{persistedId} persistido e relido do banco oficial.";
        NotifyContext();
    }

    private async void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedAtivo is null)
        {
            Status = "Selecione um ativo antes de excluir.";
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            $"Excluir o ativo {_selectedAtivo.Codigo} por soft-delete?",
            "Confirmar exclusão",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        var id = _selectedAtivo.Id;
        Status = "Aplicando soft-delete na API oficial...";
        var result = await _api.DeleteOpsAtivoAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
        if (!result.Success)
        {
            Status = $"Ativo não excluído: {result.Detail}";
            return;
        }

        if (!await CarregarAsync())
        {
            Status = "A API confirmou a exclusão, mas a releitura falhou. O resultado não será tratado como concluído.";
            return;
        }

        Novo_Click(sender, e);
        Status = $"Ativo #{id} excluído por soft-delete e ausente na releitura.";
    }

    private async Task<bool> CarregarAsync()
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Ativos.Clear();
            Status = "Sessão JWT ausente. Autentique o terminal antes de gerir ativos.";
            return false;
        }

        var selectedStatus = SelectedComboValue(StatusFiltroBox);
        Status = "Consultando /api/ops/ativos na API oficial...";
        var result = await _api.GetOpsAtivosAsync(_terminal, _terminal.DesktopAccessToken, selectedStatus, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Ativos não carregados: {result.Detail}";
            return false;
        }

        Ativos.Clear();
        foreach (var item in result.Data)
        {
            Ativos.Add(item);
        }

        Status = $"{Ativos.Count} ativo(s) carregado(s) da API oficial.";
        return true;
    }

    private void NotifyContext()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDetails)));
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? SelectedComboValue(ComboBox comboBox) =>
        (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

    private static void SelectComboItem(ComboBox comboBox, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            comboBox.SelectedIndex = -1;
            return;
        }

        var match = comboBox.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(item => string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase));
        comboBox.SelectedItem = match;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
