/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        // Toda nova janela do shell recebe Mica / Acrylic automaticamente,
        // replicando o efeito cristal/translúcido do Windows 11.
        EventManager.RegisterClassHandler(
            typeof(Window),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler((s, _) =>
            {
                if (s is Window w)
                {
                    Win32Helper.ApplyMica(w);
                }
            }));

        ShutdownMode = ShutdownMode.OnExplicitShutdown;
        var terminal = new TerminalProfile { DesktopAccessToken = string.Empty };
        var loginWindow = new DesktopLoginWindow(terminal);
        if (loginWindow.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        var mainWindow = new MainWindow(terminal);
        MainWindow = mainWindow;
        ShutdownMode = ShutdownMode.OnMainWindowClose;
        mainWindow.Show();
        _ = CheckDesktopUpdateAsync();
    }

    private static async Task CheckDesktopUpdateAsync()
    {
        var result = await DesktopAutoUpdateService.CheckAndDownloadLatestAsync();
        Debug.WriteLine($"GenesisGest.Net updater: {result.Message}");
    }
}
