/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class NetworkSettingsWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _apiClient = new();
    private string _resultMessage = "Ajuste a grade quando trocar terminal, unidade, rota local ou token de acesso.";
    private string _summary = "Matriz usa servidor local primeiro; unidades externas usam API pública segura e podem receber rota dedicada quando houver túnel próprio.";
    private string _yaraModel = "gpt-5-mini";
    private string _sophiaModel = "gpt-5-mini";
    private string _yaraStatus = "Status ainda nao consultado";
    private string _sophiaStatus = "Status ainda nao consultado";
    private readonly Dictionary<string, DesktopSiteConfiguracao> _portalConfigurations = new(StringComparer.OrdinalIgnoreCase);
    private string _portalPrimaryColor = "#C9A227";
    private string _portalSecondaryColor = "#F59E0B";
    private string _portalBackgroundColor = "#080A0F";
    private string _portalSurfaceColor = "#11151D";
    private string _portalTextColor = "#F8FAFC";
    private string _portalMutedTextColor = "#A1A1AA";

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

    public string YaraModel
    {
        get => _yaraModel;
        set => SetField(ref _yaraModel, value);
    }

    public string SophiaModel
    {
        get => _sophiaModel;
        set => SetField(ref _sophiaModel, value);
    }

    public string YaraStatus
    {
        get => _yaraStatus;
        set => SetField(ref _yaraStatus, value);
    }

    public string SophiaStatus
    {
        get => _sophiaStatus;
        set => SetField(ref _sophiaStatus, value);
    }

    public string PortalPrimaryColor { get => _portalPrimaryColor; set => SetField(ref _portalPrimaryColor, value); }
    public string PortalSecondaryColor { get => _portalSecondaryColor; set => SetField(ref _portalSecondaryColor, value); }
    public string PortalBackgroundColor { get => _portalBackgroundColor; set => SetField(ref _portalBackgroundColor, value); }
    public string PortalSurfaceColor { get => _portalSurfaceColor; set => SetField(ref _portalSurfaceColor, value); }
    public string PortalTextColor { get => _portalTextColor; set => SetField(ref _portalTextColor, value); }
    public string PortalMutedTextColor { get => _portalMutedTextColor; set => SetField(ref _portalMutedTextColor, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    public NetworkSettingsWindow(TerminalProfile terminal)
    {
        Terminal = terminal;
        InitializeComponent();
        DataContext = this;
        TokenBox.Password = Terminal.DesktopAccessToken;
        LoadEndpoints();
        Loaded += async (_, _) =>
        {
            await RefreshAssistantsAsync();
            await RefreshPortalThemeAsync();
        };
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

        ResultMessage = "Configuracao do terminal salva. O JWT administrativo permanece somente na memoria desta execucao.";
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

    private async void AuthenticateAdmin_Click(object sender, RoutedEventArgs e)
    {
        ResultMessage = "Autenticando sessao administrativa na API oficial...";
        var login = await _apiClient.AuthenticateAsync(
            Terminal,
            AdminEmailBox.Text,
            AdminPasswordBox.Password,
            AdminMfaBox.Text);
        if (!login.Success || string.IsNullOrWhiteSpace(login.Token))
        {
            ResultMessage = $"Autenticacao administrativa recusada: {login.Detail}";
            return;
        }

        Terminal.DesktopAccessToken = login.Token;
        TokenBox.Password = login.Token;
        AdminPasswordBox.Clear();
        AdminMfaBox.Clear();
        ResultMessage = $"{login.Detail} O JWT sera mantido somente na memoria ate o Desktop ser fechado.";
        await RefreshAssistantsAsync();
        await RefreshPortalThemeAsync();
    }

    private async void RefreshAssistants_Click(object sender, RoutedEventArgs e)
    {
        Terminal.DesktopAccessToken = TokenBox.Password;
        await RefreshAssistantsAsync();
    }

    private async void SaveAssistants_Click(object sender, RoutedEventArgs e)
    {
        Terminal.DesktopAccessToken = TokenBox.Password;
        var yaraModel = YaraModel.Trim();
        var sophiaModel = SophiaModel.Trim();
        if (string.IsNullOrWhiteSpace(yaraModel) || string.IsNullOrWhiteSpace(sophiaModel))
        {
            ResultMessage = "Informe os modelos da Yara e da Sophia antes de salvar.";
            return;
        }

        ResultMessage = "Validando as duas credenciais diretamente na OpenAI...";
        var result = await _apiClient.SaveOpenAiAssistantsAsync(
            Terminal,
            Terminal.DesktopAccessToken,
            new DesktopOpenAiAssistentesConfiguracaoRequest(
                string.IsNullOrWhiteSpace(YaraApiKeyBox.Password) ? null : YaraApiKeyBox.Password,
                yaraModel,
                string.IsNullOrWhiteSpace(SophiaApiKeyBox.Password) ? null : SophiaApiKeyBox.Password,
                sophiaModel));

        if (!result.Success || result.Data is null)
        {
            ResultMessage = $"As credenciais nao foram salvas: {result.Detail}";
            return;
        }

        ApplyAssistantStatus(result.Data);
        YaraApiKeyBox.Clear();
        SophiaApiKeyBox.Clear();
        ResultMessage = "Credenciais validadas, criptografadas e relidas do banco oficial. Os campos de chave foram limpos.";
    }

    private async Task RefreshAssistantsAsync()
    {
        Terminal.DesktopAccessToken = TokenBox.Password;
        var result = await _apiClient.GetOpenAiAssistantsStatusAsync(Terminal, Terminal.DesktopAccessToken);
        if (!result.Success || result.Data is null)
        {
            YaraStatus = "Consulta indisponivel";
            SophiaStatus = "Consulta indisponivel";
            ResultMessage = $"Nao foi possivel consultar as credenciais: {result.Detail}";
            return;
        }

        ApplyAssistantStatus(result.Data);
        ResultMessage = "Status das credenciais consultado na API oficial.";
    }

    private void ApplyAssistantStatus(DesktopOpenAiAssistentesStatus status)
    {
        YaraStatus = BuildAssistantStatus(status.Yara);
        SophiaStatus = BuildAssistantStatus(status.Sophia);
        if (!string.IsNullOrWhiteSpace(status.Yara.Modelo))
        {
            YaraModel = status.Yara.Modelo;
        }
        if (!string.IsNullOrWhiteSpace(status.Sophia.Modelo))
        {
            SophiaModel = status.Sophia.Modelo;
        }
    }

    private static string BuildAssistantStatus(DesktopOpenAiAssistenteStatus status)
    {
        return status.Configurada
            ? $"Configurada | {status.Modelo} | {status.Origem}"
            : "Nao configurada";
    }

    private async void RefreshPortalTheme_Click(object sender, RoutedEventArgs e)
    {
        Terminal.DesktopAccessToken = TokenBox.Password;
        await RefreshPortalThemeAsync();
    }

    private async void SavePortalTheme_Click(object sender, RoutedEventArgs e)
    {
        Terminal.DesktopAccessToken = TokenBox.Password;
        var requested = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["site_cor_primaria"] = PortalPrimaryColor.Trim(),
            ["site_cor_secundaria"] = PortalSecondaryColor.Trim(),
            ["site_cor_fundo"] = PortalBackgroundColor.Trim(),
            ["site_cor_superficie"] = PortalSurfaceColor.Trim(),
            ["site_cor_texto"] = PortalTextColor.Trim(),
            ["site_cor_texto_suave"] = PortalMutedTextColor.Trim()
        };

        var invalid = requested.FirstOrDefault(item => !Regex.IsMatch(item.Value, "^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant));
        if (!string.IsNullOrEmpty(invalid.Key))
        {
            ResultMessage = $"A configuração {invalid.Key} deve usar uma cor hexadecimal no formato #RRGGBB.";
            return;
        }

        if (requested.Keys.Any(key => !_portalConfigurations.ContainsKey(key)))
        {
            ResultMessage = "As seis configurações de identidade não foram carregadas da API oficial. Recarregue antes de salvar.";
            return;
        }

        var items = requested.Select(item =>
        {
            var current = _portalConfigurations[item.Key];
            return new DesktopSiteConfiguracaoUpdate(
                item.Key,
                item.Value.ToUpperInvariant(),
                "Cor",
                current.Descricao,
                current.Grupo,
                current.Editavel);
        }).ToList();

        ResultMessage = "Gravando a identidade visual no banco oficial...";
        var result = await _apiClient.SaveSiteConfigurationsAsync(
            Terminal,
            Terminal.DesktopAccessToken,
            new DesktopSiteConfiguracaoUpdateRequest(items));
        if (!result.Success || result.Data is null)
        {
            ResultMessage = $"A identidade visual não foi salva: {result.Detail}";
            return;
        }

        var persisted = result.Data.ToDictionary(item => item.Chave, StringComparer.OrdinalIgnoreCase);
        var mismatch = requested.FirstOrDefault(item =>
            !persisted.TryGetValue(item.Key, out var saved)
            || !string.Equals(saved.Valor, item.Value, StringComparison.OrdinalIgnoreCase));
        if (!string.IsNullOrEmpty(mismatch.Key))
        {
            ResultMessage = $"A API não confirmou a releitura da configuração {mismatch.Key}.";
            return;
        }

        ApplyPortalConfigurations(result.Data);
        ResultMessage = "Identidade visual gravada e relida do banco oficial.";
    }

    private async Task RefreshPortalThemeAsync()
    {
        Terminal.DesktopAccessToken = TokenBox.Password;
        var result = await _apiClient.GetSiteConfigurationsAsync(Terminal, Terminal.DesktopAccessToken);
        if (!result.Success || result.Data is null)
        {
            ResultMessage = $"Não foi possível carregar a identidade visual: {result.Detail}";
            return;
        }

        ApplyPortalConfigurations(result.Data);
        ResultMessage = "Identidade visual carregada da API oficial.";
    }

    private void ApplyPortalConfigurations(IEnumerable<DesktopSiteConfiguracao> configurations)
    {
        _portalConfigurations.Clear();
        foreach (var configuration in configurations)
        {
            _portalConfigurations[configuration.Chave] = configuration;
        }

        PortalPrimaryColor = GetPortalColor("site_cor_primaria", PortalPrimaryColor);
        PortalSecondaryColor = GetPortalColor("site_cor_secundaria", PortalSecondaryColor);
        PortalBackgroundColor = GetPortalColor("site_cor_fundo", PortalBackgroundColor);
        PortalSurfaceColor = GetPortalColor("site_cor_superficie", PortalSurfaceColor);
        PortalTextColor = GetPortalColor("site_cor_texto", PortalTextColor);
        PortalMutedTextColor = GetPortalColor("site_cor_texto_suave", PortalMutedTextColor);
    }

    private string GetPortalColor(string key, string currentValue) =>
        _portalConfigurations.TryGetValue(key, out var configuration)
        && Regex.IsMatch(configuration.Valor ?? string.Empty, "^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant)
            ? configuration.Valor!.ToUpperInvariant()
            : currentValue;

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
