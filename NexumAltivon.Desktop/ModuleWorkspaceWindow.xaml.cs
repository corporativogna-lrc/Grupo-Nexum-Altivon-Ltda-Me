/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class ModuleWorkspaceWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _apiClient = new();
    private readonly LocalOutboxService _outbox = new();
    private readonly TerminalProfile _profile;
    private string _status;

    public ModuleWorkspaceWindow(string moduleTitle, string area, string detail, TerminalProfile profile)
    {
        _profile = profile;
        ModuleTitle = moduleTitle;
        Area = area;
        Detail = detail;
        WindowTitle = $"GenesisGest.Net - {moduleTitle}";
        ReferenceCode = BuildReferenceCode(area, moduleTitle);
        Source = "Menu operacional desktop";
        ServerDatabase = $"{profile.ServerAddress}:{profile.DatabasePort}";
        ExecutionPolicy = "Toda confirmação exige resposta persistida da API. Se a API falhar e a contingência estiver habilitada, o registro local será identificado explicitamente como pendente de sincronização.";
        _status = "Nenhuma operação foi registrada nesta janela.";

        InitializeComponent();
        DataContext = this;
    }

    public string WindowTitle { get; }
    public string ModuleTitle { get; }
    public string Area { get; }
    public string Detail { get; }
    public string ReferenceCode { get; }
    public string Source { get; }
    public string ExecutionPolicy { get; }
    public string ServerDatabase { get; }

    public string Status
    {
        get => _status;
        set
        {
            if (_status == value)
            {
                return;
            }

            _status = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void ConfirmarOperacao_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement control)
        {
            return;
        }

        control.IsEnabled = false;
        Status = $"Enviando {ReferenceCode} para a API oficial...";

        var payload = new WorkspaceOperationPayload(
            ModuleTitle,
            Area,
            Detail,
            ReferenceCode,
            _profile.StoreCode,
            _profile.TerminalCode,
            _profile.OperatorName,
            DateTime.UtcNow);

        try
        {
            var result = await _apiClient.SubmitOperationAsync(
                _profile,
                NormalizeCode(Area),
                ReferenceCode,
                payload);

            if (result.Success && result.PersistedOnServer && !string.IsNullOrWhiteSpace(result.ServerReference))
            {
                Status = $"Persistência confirmada em {result.Endpoint}. {result.ServerReference}.";
                return;
            }

            if (!_profile.OfflineContingencyEnabled)
            {
                Status = $"Falha sem persistência: {result.Detail}";
                return;
            }

            var outboxPath = await _outbox.SaveOperationAsync(NormalizeCode(Area), ReferenceCode, payload);
            Status = $"API não confirmou persistência. Registro salvo somente na contingência local e pendente de sincronização: {outboxPath}. Motivo: {result.Detail}";
        }
        catch (Exception ex)
        {
            Status = $"Falha operacional não confirmada: {ex.Message}";
        }
        finally
        {
            control.IsEnabled = true;
        }
    }

    private async void AtualizarVinculo_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement control)
        {
            return;
        }

        control.IsEnabled = false;
        Status = "Validando endpoints local e público...";
        try
        {
            var health = await _apiClient.CheckHealthAsync(_profile);
            Status = $"{health.Status}. {health.Detail} Verificado em {health.CheckedAt:dd/MM/yyyy HH:mm:ss}.";
        }
        catch (Exception ex)
        {
            Status = $"Falha ao validar vínculo: {ex.Message}";
        }
        finally
        {
            control.IsEnabled = true;
        }
    }

    private void Fechar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private static string BuildReferenceCode(string area, string moduleTitle)
    {
        var areaPrefix = NormalizeCode(area);
        var modulePrefix = NormalizeCode(moduleTitle);
        return $"{areaPrefix}-{modulePrefix}-{DateTime.Now:yyyyMMddHHmmss}";
    }

    private static string NormalizeCode(string value)
    {
        var allowed = value
            .ToUpperInvariant()
            .Where(char.IsLetterOrDigit)
            .Take(8)
            .ToArray();

        return allowed.Length == 0 ? "GENESIS" : new string(allowed);
    }

    private sealed record WorkspaceOperationPayload(
        string Modulo,
        string Area,
        string Detalhe,
        string Codigo,
        string Loja,
        string Terminal,
        string Operador,
        DateTime CriadoEmUtc);
}
