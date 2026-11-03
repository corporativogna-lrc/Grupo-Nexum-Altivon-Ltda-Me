/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Sistema: GenesisGest.Net / Nexum Altivon
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class StockTraceabilityWindow : Window, INotifyPropertyChanged
{
    private readonly TerminalProfile _profile;
    private readonly DesktopApiClient _apiClient = new();
    private string _status = "A consulta ainda não foi executada.";
    private bool _busy;

    public StockTraceabilityWindow(TerminalProfile profile)
    {
        _profile = profile;
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await LoadAsync();
    }

    public ObservableCollection<DesktopGenesisMovimentacaoEstoque> Movimentos { get; } = [];

    public string Status
    {
        get => _status;
        private set
        {
            if (_status == value) return;
            _status = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Pesquisar_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private async void BuscaBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        if (_busy) return;
        if (string.IsNullOrWhiteSpace(_profile.DesktopAccessToken))
        {
            Movimentos.Clear();
            Status = "Sessão ausente. Entre no Desktop com um usuário autorizado para consultar a rastreabilidade.";
            return;
        }

        _busy = true;
        Status = "Consultando eventos Genesis da empresa vinculada ao tenant...";
        try
        {
            var result = await _apiClient.GetGenesisStockMovementsAsync(
                _profile, _profile.DesktopAccessToken, BuscaBox.Text);
            if (!result.Sucesso || result.Dados is null)
            {
                Movimentos.Clear();
                Status = result.Mensagem;
                return;
            }

            Movimentos.Clear();
            foreach (var movement in result.Dados) Movimentos.Add(movement);
            Status = $"{result.Mensagem} {Movimentos.Count} evento(s) carregado(s). Nenhum registro foi alterado.";
        }
        catch (Exception ex)
        {
            Movimentos.Clear();
            Status = $"Falha ao consultar rastreabilidade: {ex.Message}";
        }
        finally { _busy = false; }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
