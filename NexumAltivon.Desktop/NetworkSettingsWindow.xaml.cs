/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class NetworkSettingsWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _apiClient = new();
    private string _resultMessage = "Ajuste a grade quando trocar terminal, unidade, rota local ou token de acesso.";
    private string _summary = "Matriz usa servidor local primeiro; unidades externas usam API pública segura e podem receber rota dedicada quando houver túnel próprio.";

    public TerminalProfile Terminal { get; }
    public ObservableCollection<NetworkEndpointSetting> Endpoints { get; } = new();

    public string ResultMessage
    {
        get => _resultMessage;
        set => SetField(ref _resultMessage, value);
    }

    public string Summary
    {
        get => _summary;
        set => SetField(ref _summary, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public NetworkSettingsWindow(TerminalProfile terminal)
    {
        Terminal = terminal;
        InitializeComponent();
        DataContext = this;
        TokenBox.Password = Terminal.DesktopAccessToken;
        LoadEndpoints();
    }

    private void LoadEndpoints()
    {
        Endpoints.Clear();
        Endpoints.Add(CreateEndpoint("API local matriz", "NEXUM_DESKTOP_API_BASE", Terminal.ApiBaseUrl, "Terminal"));
        Endpoints.Add(CreateEndpoint("API pública", "NEXUM_DESKTOP_PUBLIC_API", Terminal.PublicApiUrl, "Terminal"));
        Endpoints.Add(CreateEndpoint("Servidor interno", "NEXUM_DESKTOP_SERVER_ADDRESS", Terminal.ServerAddress, "Terminal"));
        Endpoints.Add(CreateEndpoint("Porta do banco", "NEXUM_DESKTOP_DATABASE_PORT", Terminal.DatabasePort, "Terminal"));
        Endpoints.Add(CreateEndpoint("Loja", "NEXUM_DESKTOP_STORE_CODE", Terminal.StoreCode, "Terminal"));
        Endpoints.Add(CreateEndpoint("Nome da loja", "NEXUM_DESKTOP_STORE_NAME", Terminal.StoreName, "Terminal"));
        Endpoints.Add(CreateEndpoint("Terminal", "NEXUM_DESKTOP_TERMINAL_CODE", Terminal.TerminalCode, "Terminal"));
        Endpoints.Add(CreateEndpoint("Operador", "NEXUM_DESKTOP_OPERATOR", Terminal.OperatorName, "Terminal"));
        Endpoints.Add(CreateEndpoint("Impressora local", "NEXUM_DESKTOP_LOCAL_PRINTER", Terminal.LocalPrinter, "Terminal"));
        Endpoints.Add(CreateEndpoint("Contingência", "NEXUM_DESKTOP_CONTINGENCY", Terminal.OfflineContingencyEnabled ? "true" : "false", "Terminal"));
    }

    private async void TestConnections_Click(object sender, RoutedEventArgs e)
    {
        ApplyGridToTerminal();
        ResultMessage = "Testando API local e pública...";

        var result = await _apiClient.CheckHealthAsync(Terminal);
        UpdateStatus("NEXUM_DESKTOP_API_BASE", result.LocalHealthy ? "Saudável" : "Sem resposta");
        UpdateStatus("NEXUM_DESKTOP_PUBLIC_API", result.PublicHealthy ? "Saudável" : "Sem resposta");
        UpdateStatus("NEXUM_DESKTOP_TOKEN", string.IsNullOrWhiteSpace(Terminal.DesktopAccessToken) ? "Não informado" : "Informado");

        ResultMessage = $"{result.Status}. {result.Detail}";
    }

    private void SaveEnvironment_Click(object sender, RoutedEventArgs e)
    {
        ApplyGridToTerminal();

        SetUserEnvironment("NEXUM_DESKTOP_API_BASE", Terminal.ApiBaseUrl);
        SetUserEnvironment("NEXUM_DESKTOP_PUBLIC_API", Terminal.PublicApiUrl);
        SetUserEnvironment("NEXUM_DESKTOP_SERVER_ADDRESS", Terminal.ServerAddress);
        SetUserEnvironment("NEXUM_DESKTOP_DATABASE_PORT", Terminal.DatabasePort);
        SetUserEnvironment("NEXUM_DESKTOP_STORE_CODE", Terminal.StoreCode);
        SetUserEnvironment("NEXUM_DESKTOP_STORE_NAME", Terminal.StoreName);
        SetUserEnvironment("NEXUM_DESKTOP_TERMINAL_CODE", Terminal.TerminalCode);
        SetUserEnvironment("NEXUM_DESKTOP_OPERATOR", Terminal.OperatorName);
        SetUserEnvironment("NEXUM_DESKTOP_LOCAL_PRINTER", Terminal.LocalPrinter);
        SetUserEnvironment("NEXUM_DESKTOP_CONTINGENCY", Terminal.OfflineContingencyEnabled ? "true" : "false");
        SetUserEnvironment("NEXUM_DESKTOP_TOKEN", Terminal.DesktopAccessToken);

        ResultMessage = "Configuração salva nas variáveis do usuário do Windows. Feche e abra o GenesisGest.Net para recarregar tudo limpo.";
        foreach (var endpoint in Endpoints)
        {
            endpoint.Status = "Salvo";
        }
        RefreshGrid();
    }

    private void TokenBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        Terminal.DesktopAccessToken = TokenBox.Password;
    }

    private void ApplyGridToTerminal()
    {
        foreach (var endpoint in Endpoints)
        {
            switch (endpoint.EnvironmentVariable)
            {
                case "NEXUM_DESKTOP_API_BASE":
                    Terminal.ApiBaseUrl = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_PUBLIC_API":
                    Terminal.PublicApiUrl = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_SERVER_ADDRESS":
                    Terminal.ServerAddress = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_DATABASE_PORT":
                    Terminal.DatabasePort = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_STORE_CODE":
                    Terminal.StoreCode = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_STORE_NAME":
                    Terminal.StoreName = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_TERMINAL_CODE":
                    Terminal.TerminalCode = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_OPERATOR":
                    Terminal.OperatorName = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_LOCAL_PRINTER":
                    Terminal.LocalPrinter = endpoint.Value.Trim();
                    break;
                case "NEXUM_DESKTOP_CONTINGENCY":
                    Terminal.OfflineContingencyEnabled = endpoint.Value.Equals("true", StringComparison.OrdinalIgnoreCase);
                    break;
            }
        }
    }

    private void UpdateStatus(string environmentVariable, string status)
    {
        var endpoint = Endpoints.FirstOrDefault(item => item.EnvironmentVariable == environmentVariable);
        if (endpoint is not null)
        {
            endpoint.Status = status;
        }
        RefreshGrid();
    }

    private void RefreshGrid()
    {
        var snapshot = Endpoints.ToArray();
        Endpoints.Clear();
        foreach (var endpoint in snapshot)
        {
            Endpoints.Add(endpoint);
        }
    }

    private static NetworkEndpointSetting CreateEndpoint(string name, string variable, string value, string source)
    {
        return new NetworkEndpointSetting
        {
            Name = name,
            EnvironmentVariable = variable,
            Value = value,
            Source = source,
            Status = string.IsNullOrWhiteSpace(value) ? "Pendente" : "Carregado"
        };
    }

    private static void SetUserEnvironment(string name, string value)
    {
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.User);
        Environment.SetEnvironmentVariable(name, value, EnvironmentVariableTarget.Process);
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
