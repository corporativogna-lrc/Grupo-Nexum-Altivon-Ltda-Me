/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7201
 * Origem funcional: adaptação auditada do LURCO432
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class DreIndicadoresWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly CancellationTokenSource _lifetime = new();
    private readonly TerminalProfile? _terminal;
    private string _empresaIdTexto = string.Empty;
    private string _competencia = string.Empty;
    private string _status = "Informe os filtros e consulte a API oficial.";

    public DreIndicadoresWindow(TerminalProfile? terminal = null)
    {
        _terminal = terminal;
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DesktopDreGerencial> Registros { get; } = new();

    public string EmpresaIdTexto
    {
        get => _empresaIdTexto;
        set => SetField(ref _empresaIdTexto, value);
    }

    public string Competencia
    {
        get => _competencia;
        set => SetField(ref _competencia, value);
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string TotalDebito => Registros.Sum(item => item.ValorDebito ?? 0m).ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
    public string TotalCredito => Registros.Sum(item => item.ValorCredito ?? 0m).ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
    public string TotalSaldo => Registros.Sum(item => item.SaldoConta ?? 0m).ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Consultar_Click(object sender, RoutedEventArgs e)
    {
        if (_terminal is null)
        {
            Status = "Terminal não inicializado. A consulta não foi enviada.";
            return;
        }

        int? empresaId = null;
        if (!string.IsNullOrWhiteSpace(EmpresaIdTexto)
            && (!int.TryParse(EmpresaIdTexto, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedEmpresaId)
                || parsedEmpresaId <= 0))
        {
            Status = "O código da empresa deve ser um número inteiro positivo.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(EmpresaIdTexto))
        {
            empresaId = int.Parse(EmpresaIdTexto, CultureInfo.InvariantCulture);
        }

        Status = "Consultando /api/financeiro/dre...";
        try
        {
            var result = await _api.GetDreAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                empresaId,
                Competencia,
                _lifetime.Token);

            Registros.Clear();
            if (!result.Success || result.Data is null)
            {
                AtualizarTotais();
                Status = $"A API não confirmou a consulta: {result.Detail}";
                return;
            }

            foreach (var registro in result.Data)
            {
                Registros.Add(registro);
            }

            AtualizarTotais();
            Status = $"Consulta confirmada pela API oficial. {Registros.Count} registro(s) recebido(s).";
        }
        catch (OperationCanceledException)
        {
            Status = "Consulta cancelada pelo fechamento da janela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha na consulta: {ex.Message}";
        }
    }

    private void Limpar_Click(object sender, RoutedEventArgs e)
    {
        EmpresaIdTexto = string.Empty;
        Competencia = string.Empty;
        Registros.Clear();
        AtualizarTotais();
        Status = "Filtros e resultados limpos.";
    }

    protected override void OnClosed(EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        base.OnClosed(e);
    }

    private void AtualizarTotais()
    {
        OnPropertyChanged(nameof(TotalDebito));
        OnPropertyChanged(nameof(TotalCredito));
        OnPropertyChanged(nameof(TotalSaldo));
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

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
