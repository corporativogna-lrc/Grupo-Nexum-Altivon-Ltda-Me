/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7225 Data: 16/08/2026
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class FechamentoPeriodoWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private string _empresaIdText = string.Empty;
    private DateTime _periodo = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private bool _bloqueado = true;
    private string _observacoes = string.Empty;
    private string _status = "Aguardando carregamento da API oficial.";
    private bool _busy;

    public FechamentoPeriodoWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<DesktopFechamentoContabil> Fechamentos { get; } = new();
    public ObservableCollection<DesktopAuditoriaOperacional> Auditoria { get; } = new();
    public string TenantLabel => $"Loja {_terminal.StoreCode}";
    public bool PodeOperar => !_busy && !string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken);
    public string CodigoContexto => Fechamentos.Count == 0 ? "FEC-NOVO" : $"FEC-{Fechamentos[0].Id}";
    public string EstadoContexto => _busy ? "PROCESSANDO" : "PRONTO";
    public string FechamentosLabel => $"{Fechamentos.Count} fechamento(s)";
    public string Validacao => BuildValidationMessage();
    public string EmpresaIdText { get => _empresaIdText; set => SetFormField(ref _empresaIdText, value, nameof(Validacao)); }
    public DateTime Periodo { get => _periodo; set => SetFormField(ref _periodo, value, nameof(Validacao)); }
    public bool Bloqueado { get => _bloqueado; set => SetField(ref _bloqueado, value); }
    public string Observacoes { get => _observacoes; set => SetField(ref _observacoes, value); }
    public string Status { get => _status; private set => SetField(ref _status, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await CarregarAsync();

    private void Window_Closed(object? sender, EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
    }

    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        EmpresaIdText = string.Empty;
        Periodo = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        Bloqueado = true;
        Observacoes = string.Empty;
        Status = "Novo fechamento contábil preparado. Nenhum dado foi gravado.";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildRequest(out var request, out var error))
        {
            Status = error;
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _api.SaveFechamentoContabilAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                request,
                _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Fechamento não gravado. {result.Detail}";
                return;
            }

            var persistedId = result.Data.Id;
            await CarregarAsync(false, request.EmpresaId, request.Periodo);
            if (Fechamentos.All(item => item.Id != persistedId))
            {
                Status = $"A API confirmou FEC-{persistedId}, mas a releitura não o encontrou. Operação não considerada concluída.";
                return;
            }

            Status = $"Fechamento FEC-{persistedId} gravado e confirmado por releitura da API e do banco oficial.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Status = "Operação cancelada pelo encerramento da tela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha ao registrar fechamento contábil: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task CarregarAsync(bool updateStatus = true, int? empresaId = null, DateTime? periodo = null)
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de operar o fechamento.";
            return;
        }

        SetBusy(true);
        try
        {
            var fechamentosTask = _api.GetFechamentosContabeisAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                empresaId,
                periodo,
                _lifetime.Token);
            var auditoriaTask = _api.GetAuditoriaAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                "cnt_fechamentos",
                _lifetime.Token);
            await Task.WhenAll(fechamentosTask, auditoriaTask);
            var fechamentos = await fechamentosTask;
            var auditoria = await auditoriaTask;
            if (!fechamentos.Success || fechamentos.Data is null || !auditoria.Success || auditoria.Data is null)
            {
                Status = $"Falha ao carregar fechamento. Registros: {fechamentos.Detail}; Auditoria: {auditoria.Detail}";
                return;
            }

            Fechamentos.Clear();
            foreach (var item in fechamentos.Data)
            {
                Fechamentos.Add(item);
            }

            Auditoria.Clear();
            foreach (var item in auditoria.Data)
            {
                Auditoria.Add(item);
            }

            NotifyCollections();
            if (updateStatus)
            {
                Status = $"{Fechamentos.Count} fechamento(s) e {Auditoria.Count} evento(s) de auditoria carregados da API oficial.";
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Status = "Carregamento cancelado pelo encerramento da tela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha ao carregar fechamento contábil: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool TryBuildRequest(out DesktopFechamentoContabilRequest request, out string error)
    {
        request = default!;
        error = BuildValidationMessage();
        if (error.Length > 0)
        {
            return false;
        }

        request = new DesktopFechamentoContabilRequest(
            int.Parse(EmpresaIdText, CultureInfo.CurrentCulture),
            new DateTime(Periodo.Year, Periodo.Month, 1),
            Bloqueado,
            NullIfWhiteSpace(Observacoes));
        return true;
    }

    private string BuildValidationMessage() =>
        int.TryParse(EmpresaIdText, NumberStyles.Integer, CultureInfo.CurrentCulture, out var empresaId) && empresaId > 0
            ? string.Empty
            : "Informe o ID inteiro positivo da empresa.";

    private void SetBusy(bool value)
    {
        _busy = value;
        Notify(nameof(PodeOperar));
        Notify(nameof(EstadoContexto));
    }

    private void NotifyCollections()
    {
        Notify(nameof(CodigoContexto));
        Notify(nameof(FechamentosLabel));
        Notify(nameof(Validacao));
    }

    private void SetFormField<T>(ref T field, T value, string validationProperty, [CallerMemberName] string? propertyName = null)
    {
        if (SetField(ref field, value, propertyName))
        {
            Notify(validationProperty);
        }
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        Notify(propertyName);
        return true;
    }

    private void Notify([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
