/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Diagnostics;
using System.Windows;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        _ = CheckDesktopUpdateAsync();
    }

    private static async Task CheckDesktopUpdateAsync()
    {
        var result = await DesktopAutoUpdateService.CheckDownloadAndApplyAsync();
        Debug.WriteLine($"GenesisGest.Net updater: {result.Message}");
    }
}
