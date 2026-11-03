/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7247 Data: 30/10/2026
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public sealed record OperationalQueryDefinition(
    string Key,
    string Title,
    string Sector,
    string TableName,
    string Permission,
    string Endpoint);

public partial class OperationalQueryWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _apiClient = new();
    private readonly TerminalProfile _terminal;
    private string _status = "Nenhuma consulta executada.";
    private string _resultJson = "A consulta ainda não foi executada.";
    private string _orderNumber = string.Empty;
    private string _customerIdentity = string.Empty;

    public OperationalQueryWindow(OperationalQueryDefinition definition, TerminalProfile terminal)
    {
        Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        WindowTitle = $"GenesisGest.Net - {definition.Title}";
        Sector = definition.Sector;
        TableName = definition.TableName;
        Permission = $"Permissão requerida: {definition.Permission}";
        Endpoint = definition.Endpoint;
        IsOrderTracking = string.Equals(definition.Key, "pedido-status", StringComparison.OrdinalIgnoreCase);
        InitializeComponent();
        DataContext = this;
    }

    public OperationalQueryDefinition Definition { get; }
    public string WindowTitle { get; }
    public string Sector { get; }
    public string TableName { get; }
    public string Permission { get; }
    public string Endpoint { get; }
    public bool IsOrderTracking { get; }
    public Visibility StandardQueryVisibility => IsOrderTracking ? Visibility.Collapsed : Visibility.Visible;
    public Visibility OrderTrackingVisibility => IsOrderTracking ? Visibility.Visible : Visibility.Collapsed;
    public string Description => IsOrderTracking
        ? "Consulta pública protegida por número do pedido e e-mail ou CPF/CNPJ do comprador. Não altera dados."
        : "Consulta individual da fonte oficial do módulo. Esta tela não grava dados.";
    public string OrderNumber { get => _orderNumber; set => SetField(ref _orderNumber, value); }
    public string CustomerIdentity { get => _customerIdentity; set => SetField(ref _customerIdentity, value); }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string ResultJson
    {
        get => _resultJson;
        private set => SetField(ref _resultJson, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Consultar_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement control)
        {
            control.IsEnabled = false;
        }

        Status = IsOrderTracking
            ? "Validando identificação e consultando o pedido na API oficial..."
            : $"Consultando {Endpoint} na API oficial...";
        ResultJson = string.Empty;
        try
        {
            var result = IsOrderTracking
                ? await _apiClient.GetPublicOrderTrackingAsync(_terminal, OrderNumber, CustomerIdentity)
                : await _apiClient.GetOperationalJsonAsync(_terminal, _terminal.DesktopAccessToken, Endpoint);

            if (!result.Success)
            {
                Status = $"Consulta não confirmada: {result.Detail}";
                ResultJson = "A API não confirmou dados para esta consulta.";
                return;
            }

            ResultJson = JsonSerializer.Serialize(
                result.Data,
                new JsonSerializerOptions { WriteIndented = true });
            Status = $"Consulta confirmada pela API oficial em {DateTime.Now:dd/MM/yyyy HH:mm:ss}. Nenhum dado foi alterado.";
        }
        catch (Exception ex)
        {
            Status = $"Falha rastreável na consulta: {ex.Message}";
            ResultJson = "A resposta não foi confirmada.";
        }
        finally
        {
            if (sender is FrameworkElement enabledControl)
            {
                enabledControl.IsEnabled = true;
            }
        }
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
