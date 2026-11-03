/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace NexumAltivon.Desktop.Services;

/// <summary>
/// Helper para ativar backdrop Mica / Acrylic em janelas WPF, replicando o efeito
/// cristal/translúcido do Windows 11. Requer Windows 11 22H2 ou superior.
/// </summary>
public static class Win32Helper
{
    // DWMWINDOWATTRIBUTE (Windows 11 22H2+)
    // 38 = DWMWA_SYSTEMBACKDROP_TYPE
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;

    // Valores aceitos por DWMWA_SYSTEMBACKDROP_TYPE:
    // 0 = Auto     (deixa o SO decidir, segue o tema do sistema)
    // 1 = None     (sem backdrop)
    // 2 = Mica     (recomendado para o shell principal)
    // 3 = Acrylic  (mais translúcido, ideal para menus e painéis)
    // 4 = Tabbed   (Mica em camadas, para tabs)
    private const int DWMSBT_MICA = 2;
    private const int DWMSBT_ACRYLIC = 3;

    // 20 = DWMWA_USE_IMMERSIVE_DARK_MODE
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int attribute,
        ref int pvAttribute,
        int cbAttribute);

    /// <summary>
    /// Aplica Mica (Windows 11) na janela. Falha silenciosamente em Windows 10.
    /// </summary>
    public static void ApplyMica(Window window, bool useAcrylic = false)
    {
        try
        {
            var hwnd = new WindowInteropHelper(window).EnsureHandle();

            // Modo escuro do título da janela (combina com o tema dourado/preto).
            int useDark = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDark, sizeof(int));

            int backdrop = useAcrylic ? DWMSBT_ACRYLIC : DWMSBT_MICA;
            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdrop, sizeof(int));
        }
        catch
        {
            // Em Windows 10 ou se a DWM não responder, segue com o Background translúcido do WPF.
        }
    }

    /// <summary>
    /// Aplica um Background translúcido à janela (efeito cristal compatível com Windows 10/11).
    /// </summary>
    public static void ApplyTranslucentBackground(Window window, double opacity = 0.78)
    {
        window.Background = new SolidColorBrush(Color.FromArgb(
            (byte)(255 * opacity),
            0x05, 0x05, 0x05));
        window.AllowsTransparency = false; // Mica não exige AllowsTransparency
    }
}

/// <summary>
/// Janela base do GenesisGest.Net. Ativa Mica automaticamente quando carregada.
/// </summary>
public class TranslatableWindow : Window
{
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        Win32Helper.ApplyMica(this);
    }
}
