/*
 * Sistema de gestao: GenesisGest.Net (Grupo Nexum Altivon)
 * Modulo: Atualizacao oficial do Desktop via GitHub Releases
 */

using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class AtualizacaoDesktopWindow : Window, INotifyPropertyChanged
{
    private readonly CancellationTokenSource _lifetime = new();
    private string _currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString(4) ?? "Indisponível";
    private string _latestVersion = "Ainda não consultada";
    private string _status = "Consulte as releases oficiais para verificar se há uma versão mais recente.";
    private string _downloadedFile = "Nenhum instalador baixado.";
    private string? _packagePath;
    private bool _busy;

    public AtualizacaoDesktopWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string CurrentVersion { get => _currentVersion; private set => SetField(ref _currentVersion, value); }
    public string LatestVersion { get => _latestVersion; private set => SetField(ref _latestVersion, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }
    public string DownloadedFile { get => _downloadedFile; private set => SetField(ref _downloadedFile, value); }
    public bool CanCheck => !_busy;
    public bool CanInstall => !_busy && !string.IsNullOrWhiteSpace(_packagePath) && File.Exists(_packagePath);

    private async void CheckAndDownload_Click(object sender, RoutedEventArgs e) => await CheckAndDownloadAsync();

    private async Task CheckAndDownloadAsync()
    {
        if (_busy)
        {
            return;
        }

        _busy = true;
        NotifyState();
        Status = "Consultando a release oficial e verificando o pacote Desktop...";
        try
        {
            var result = await DesktopAutoUpdateService.CheckAndDownloadLatestAsync(_lifetime.Token);
            CurrentVersion = result.CurrentVersion;
            LatestVersion = result.LatestVersion ?? "Não encontrada";
            Status = result.Message;
            _packagePath = result.DownloadedFile;
            DownloadedFile = _packagePath is null ? "Nenhum instalador baixado." : Path.GetFullPath(_packagePath);
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Status = "Consulta cancelada ao fechar a janela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha ao consultar ou baixar a release: {ex.Message}";
        }
        finally
        {
            _busy = false;
            NotifyState();
        }
    }

    private void Install_Click(object sender, RoutedEventArgs e)
    {
        if (!CanInstall || _packagePath is null)
        {
            Status = "Não existe um instalador local válido para iniciar.";
            NotifyState();
            return;
        }

        var confirmation = MessageBox.Show(
            $"Iniciar o instalador baixado?{Environment.NewLine}{_packagePath}",
            "Confirmar atualização",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            DesktopAutoUpdateService.InstallDownloadedPackage(_packagePath);
            Status = "Instalador iniciado. Conclua as etapas exibidas pelo Windows; esta janela não confirma a instalação.";
        }
        catch (Exception ex)
        {
            Status = $"Falha ao iniciar o instalador: {ex.Message}";
        }
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private void NotifyState()
    {
        OnPropertyChanged(nameof(CanCheck));
        OnPropertyChanged(nameof(CanInstall));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
