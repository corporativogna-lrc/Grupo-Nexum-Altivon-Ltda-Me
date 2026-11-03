/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Sistema de gestão: GenesisGest.Net
 * Versão: 1.1.5.7255 | Data: 30/10/2026
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class SaldosProdutoWindow : Window, INotifyPropertyChanged
{
    private readonly TerminalProfile _profile;
    private readonly DesktopApiClient _apiClient = new();
    private string _status = "A consulta ainda não foi executada.";

    public SaldosProdutoWindow(TerminalProfile profile)
    {
        _profile = profile;
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await LoadStockAsync();
    }

    public ObservableCollection<DesktopGenesisSaldoEstoque> Saldos { get; } = new();

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

    private async void AtualizarSaldo_Click(object sender, RoutedEventArgs e) => await LoadStockAsync();

    private async void BuscaBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        await LoadStockAsync();
    }

    private async Task LoadStockAsync()
    {
        if (string.IsNullOrWhiteSpace(_profile.DesktopAccessToken))
        {
            Status = "Sessão ausente. Entre no Desktop com um usuário autorizado para consultar o estoque.";
            Saldos.Clear();
            return;
        }

        Status = "Consultando o saldo da empresa vinculada ao tenant...";
        try
        {
            var result = await _apiClient.GetGenesisPhysicalStockAsync(
                _profile, _profile.DesktopAccessToken, BuscaBox.Text);
            if (!result.Sucesso || result.Dados is null)
            {
                Saldos.Clear();
                Status = result.Mensagem;
                return;
            }

            Saldos.Clear();
            foreach (var saldo in result.Dados)
            {
                Saldos.Add(saldo);
            }

            Status = $"{result.Mensagem} {Saldos.Count} item(ns) carregado(s).";
        }
        catch (Exception exception)
        {
            Saldos.Clear();
            Status = $"Falha ao consultar o estoque: {exception.Message}";
        }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e) => Close();

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
