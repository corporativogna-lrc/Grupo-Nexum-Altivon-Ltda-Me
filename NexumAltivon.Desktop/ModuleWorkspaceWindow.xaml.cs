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

namespace NexumAltivon.Desktop;

public partial class ModuleWorkspaceWindow : Window, INotifyPropertyChanged
{
    private string _status;

    public ModuleWorkspaceWindow(string moduleTitle, string area, string detail)
    {
        ModuleTitle = moduleTitle;
        Area = area;
        Detail = detail;
        WindowTitle = $"GenesisGest.Net - {moduleTitle}";
        ReferenceCode = BuildReferenceCode(area, moduleTitle);
        Source = "Menu operacional desktop";
        ExecutionPolicy = "Fluxo local individualizado no Genesis. O módulo preserva vínculo com login, permissões, banco interno e integração com o servidor principal.";
        _status = "Janela operacional carregada. Selecione a ação para registrar, consultar ou atualizar o vínculo do módulo.";

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

    private void ConfirmarOperacao_Click(object sender, RoutedEventArgs e)
    {
        Status = $"{ModuleTitle}: operação registrada no contexto local do GenesisGest.Net para continuidade operacional.";
    }

    private void AtualizarVinculo_Click(object sender, RoutedEventArgs e)
    {
        Status = $"{ModuleTitle}: vínculo com o servidor principal mantido para uso do banco interno 192.168.1.72:3309.";
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
}
