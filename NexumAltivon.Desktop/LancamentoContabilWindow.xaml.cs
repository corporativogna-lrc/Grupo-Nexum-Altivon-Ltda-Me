/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7224 Data: 16/08/2026
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class LancamentoContabilWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private ContabilPartidaEditor? _selectedPartida;
    private string _empresaIdText = string.Empty;
    private string _lote = string.Empty;
    private string _sublote = string.Empty;
    private DateTime _dataLancamento = DateTime.Today;
    private string _valorLancamento = string.Empty;
    private string _tipoLancamento = "MANUAL";
    private string _historicoPadrao = string.Empty;
    private string _complemento = string.Empty;
    private string _origemModulo = string.Empty;
    private string _origemIdText = string.Empty;
    private string _status = "Aguardando carregamento da API oficial.";
    private bool _busy;

    public LancamentoContabilWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        PrepararNovoLancamento();
    }

    public ObservableCollection<ContabilPartidaEditor> Partidas { get; } = new();
    public ObservableCollection<DesktopContabilLancamento> Lancamentos { get; } = new();
    public ObservableCollection<DesktopAuditoriaOperacional> Auditoria { get; } = new();
    public string TenantLabel => $"Loja {_terminal.StoreCode}";
    public bool PodeOperar => !_busy && !string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken);
    public string CodigoContexto => Lancamentos.Count == 0 ? "LCT-NOVO" : $"LCT-{Lancamentos[0].Id}";
    public string EstadoContexto => _busy ? "PROCESSANDO" : "PRONTO";
    public string PartidasLabel => $"{Partidas.Count} partida(s)";
    public string LancamentosLabel => $"{Lancamentos.Count} lançamento(s)";
    public string TotalDebitosLabel => $"R$ {Partidas.Where(item => item.TipoNormalizado == "DEBITO").Sum(item => item.ValorDecimal):N2}";
    public string TotalCreditosLabel => $"R$ {Partidas.Where(item => item.TipoNormalizado == "CREDITO").Sum(item => item.ValorDecimal):N2}";
    public string Validacao => BuildValidationMessage();
    public string EmpresaIdText { get => _empresaIdText; set => SetFormField(ref _empresaIdText, value, nameof(Validacao)); }
    public string Lote { get => _lote; set => SetFormField(ref _lote, value, nameof(Validacao)); }
    public string Sublote { get => _sublote; set => SetField(ref _sublote, value); }
    public DateTime DataLancamento { get => _dataLancamento; set => SetField(ref _dataLancamento, value); }
    public string ValorLancamento { get => _valorLancamento; set => SetFormField(ref _valorLancamento, value, nameof(Validacao)); }
    public string TipoLancamento { get => _tipoLancamento; set => SetField(ref _tipoLancamento, value); }
    public string HistoricoPadrao { get => _historicoPadrao; set => SetField(ref _historicoPadrao, value); }
    public string Complemento { get => _complemento; set => SetField(ref _complemento, value); }
    public string OrigemModulo { get => _origemModulo; set => SetField(ref _origemModulo, value); }
    public string OrigemIdText { get => _origemIdText; set => SetFormField(ref _origemIdText, value, nameof(Validacao)); }
    public string Status { get => _status; private set => SetField(ref _status, value); }

    public ContabilPartidaEditor? SelectedPartida
    {
        get => _selectedPartida;
        set => SetField(ref _selectedPartida, value);
    }

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
        PrepararNovoLancamento();
        Status = "Novo lançamento contábil preparado. Nenhum dado foi gravado.";
    }

    private void AdicionarPartida_Click(object sender, RoutedEventArgs e)
    {
        Partidas.Add(new ContabilPartidaEditor
        {
            Tipo = "DEBITO"
        });
        NotifyCollections();
    }

    private void RemoverPartida_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPartida is null)
        {
            Status = "Selecione uma partida antes de remover.";
            return;
        }

        Partidas.Remove(SelectedPartida);
        SelectedPartida = null;
        NotifyCollections();
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildRequest(out var request, out var error))
        {
            Status = error;
            NotifyCollections();
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _api.CreateContabilLancamentoAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                request,
                _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Lançamento não gravado. {result.Detail}";
                return;
            }

            var persistedId = result.Data.Id;
            await CarregarAsync(false);
            var persisted = Lancamentos.FirstOrDefault(item => item.Id == persistedId);
            if (persisted is null)
            {
                Status = $"A API confirmou o lançamento LCT-{persistedId}, mas a releitura não o encontrou. Operação não considerada concluída.";
                return;
            }

            PrepararNovoLancamento();
            Status = $"Lançamento LCT-{persistedId} gravado e confirmado por releitura da API e do banco oficial.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Status = "Operação cancelada pelo encerramento da tela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha ao gravar lançamento contábil: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task CarregarAsync(bool updateStatus = true)
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de operar a contabilidade.";
            return;
        }

        SetBusy(true);
        try
        {
            var lancamentosTask = _api.GetContabilLancamentosAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                null,
                null,
                null,
                null,
                _lifetime.Token);
            var auditoriaTask = _api.GetAuditoriaAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                "cnt_lancamentos",
                _lifetime.Token);
            await Task.WhenAll(lancamentosTask, auditoriaTask);
            var lancamentos = await lancamentosTask;
            var auditoria = await auditoriaTask;
            if (!lancamentos.Success || lancamentos.Data is null || !auditoria.Success || auditoria.Data is null)
            {
                Status = $"Falha ao carregar contabilidade. Lançamentos: {lancamentos.Detail}; Auditoria: {auditoria.Detail}";
                return;
            }

            Lancamentos.Clear();
            foreach (var item in lancamentos.Data)
            {
                Lancamentos.Add(item);
            }

            Auditoria.Clear();
            foreach (var item in auditoria.Data)
            {
                Auditoria.Add(item);
            }

            NotifyCollections();
            if (updateStatus)
            {
                Status = $"{Lancamentos.Count} lançamento(s) e {Auditoria.Count} evento(s) de auditoria carregados da API oficial.";
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            Status = "Carregamento cancelado pelo encerramento da tela.";
        }
        catch (Exception ex)
        {
            Status = $"Falha ao carregar contabilidade: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool TryBuildRequest(out DesktopContabilLancamentoRequest request, out string error)
    {
        request = default!;
        error = BuildValidationMessage();
        if (error.Length > 0)
        {
            return false;
        }

        if (!TryParsePositiveInt(EmpresaIdText, out var empresaId))
        {
            error = "Empresa deve ser um ID inteiro positivo.";
            return false;
        }

        if (!TryParsePositiveDecimal(ValorLancamento, out var valorLancamento))
        {
            error = "Valor do lançamento deve ser positivo.";
            return false;
        }

        var partidas = new List<DesktopContabilPartidaRequest>(Partidas.Count);
        foreach (var partida in Partidas)
        {
            var tipo = partida.TipoNormalizado;
            if (tipo is not ("DEBITO" or "CREDITO"))
            {
                error = "Cada partida deve informar Tipo DEBITO ou CREDITO.";
                return false;
            }

            if (!TryParsePositiveInt(partida.PlanoContaIdText, out var planoContaId))
            {
                error = "Cada partida deve informar um plano de conta válido.";
                return false;
            }

            int? centroCustoId = null;
            if (!string.IsNullOrWhiteSpace(partida.CentroCustoIdText))
            {
                if (!TryParsePositiveInt(partida.CentroCustoIdText, out var centro))
                {
                    error = "Centro de custo deve ser um ID inteiro positivo quando informado.";
                    return false;
                }

                centroCustoId = centro;
            }

            if (!TryParsePositiveDecimal(partida.ValorText, out var valor))
            {
                error = "Cada partida deve informar um valor positivo.";
                return false;
            }

            partidas.Add(new DesktopContabilPartidaRequest(
                tipo,
                planoContaId,
                centroCustoId,
                valor,
                NullIfWhiteSpace(partida.Historico)));
        }

        var debitos = partidas.Where(item => item.Tipo == "DEBITO").Sum(item => item.Valor);
        var creditos = partidas.Where(item => item.Tipo == "CREDITO").Sum(item => item.Valor);
        if (debitos <= 0m || creditos <= 0m || Math.Abs(debitos - creditos) > 0.01m || Math.Abs(debitos - valorLancamento) > 0.01m)
        {
            error = "O lançamento exige débitos e créditos positivos, iguais entre si e iguais ao valor do cabeçalho.";
            return false;
        }

        int? origemId = null;
        if (!string.IsNullOrWhiteSpace(OrigemIdText))
        {
            if (!TryParsePositiveInt(OrigemIdText, out var parsedOrigemId))
            {
                error = "ID de origem deve ser um inteiro positivo quando informado.";
                return false;
            }

            origemId = parsedOrigemId;
        }

        request = new DesktopContabilLancamentoRequest(
            empresaId,
            NullIfWhiteSpace(Lote),
            NullIfWhiteSpace(Sublote),
            DataLancamento.Date,
            NullIfWhiteSpace(HistoricoPadrao),
            NullIfWhiteSpace(Complemento),
            valorLancamento,
            NullIfWhiteSpace(TipoLancamento),
            NullIfWhiteSpace(OrigemModulo),
            origemId,
            partidas);
        return true;
    }

    private string BuildValidationMessage()
    {
        if (!TryParsePositiveInt(EmpresaIdText, out _))
        {
            return "Informe o ID inteiro positivo da empresa.";
        }

        if (!TryParsePositiveDecimal(ValorLancamento, out var valorLancamento))
        {
            return "Informe um valor de lançamento positivo.";
        }

        if (Partidas.Count < 2)
        {
            return "Inclua pelo menos duas partidas.";
        }

        var debitos = Partidas.Where(item => item.TipoNormalizado == "DEBITO").Sum(item => item.ValorDecimal);
        var creditos = Partidas.Where(item => item.TipoNormalizado == "CREDITO").Sum(item => item.ValorDecimal);
        if (debitos <= 0m || creditos <= 0m || Math.Abs(debitos - creditos) > 0.01m || Math.Abs(debitos - valorLancamento) > 0.01m)
        {
            return "Débitos e créditos devem fechar entre si e com o valor do cabeçalho.";
        }

        return string.Empty;
    }

    private void PrepararNovoLancamento()
    {
        Lote = string.Empty;
        Sublote = string.Empty;
        DataLancamento = DateTime.Today;
        ValorLancamento = string.Empty;
        TipoLancamento = "MANUAL";
        HistoricoPadrao = string.Empty;
        Complemento = string.Empty;
        OrigemModulo = string.Empty;
        OrigemIdText = string.Empty;
        Partidas.Clear();
        Partidas.Add(new ContabilPartidaEditor { Tipo = "DEBITO" });
        Partidas.Add(new ContabilPartidaEditor { Tipo = "CREDITO" });
        SelectedPartida = null;
        NotifyCollections();
    }

    private void SetBusy(bool value)
    {
        _busy = value;
        Notify(nameof(PodeOperar));
        Notify(nameof(EstadoContexto));
    }

    private void NotifyCollections()
    {
        Notify(nameof(CodigoContexto));
        Notify(nameof(PartidasLabel));
        Notify(nameof(LancamentosLabel));
        Notify(nameof(TotalDebitosLabel));
        Notify(nameof(TotalCreditosLabel));
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

    private static bool TryParsePositiveInt(string? value, out int result) =>
        int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out result) && result > 0
        || int.TryParse(value, NumberStyles.Integer, CultureInfo.CurrentCulture, out result) && result > 0;

    private static bool TryParsePositiveDecimal(string? value, out decimal result)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out result) && result > 0m)
        {
            return true;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out result) && result > 0m;
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}

public sealed class ContabilPartidaEditor : INotifyPropertyChanged
{
    private string _tipo = "DEBITO";
    private string _planoContaIdText = string.Empty;
    private string _centroCustoIdText = string.Empty;
    private string _valorText = string.Empty;
    private string _historico = string.Empty;

    public string Tipo { get => _tipo; set => SetField(ref _tipo, value); }
    public string PlanoContaIdText { get => _planoContaIdText; set => SetField(ref _planoContaIdText, value); }
    public string CentroCustoIdText { get => _centroCustoIdText; set => SetField(ref _centroCustoIdText, value); }
    public string ValorText { get => _valorText; set => SetField(ref _valorText, value); }
    public string Historico { get => _historico; set => SetField(ref _historico, value); }
    public string TipoNormalizado => (Tipo ?? string.Empty).Trim().ToUpperInvariant();
    public decimal ValorDecimal => decimal.TryParse(ValorText, NumberStyles.Number, CultureInfo.CurrentCulture, out var value) ? value : 0m;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        if (propertyName is nameof(Tipo) or nameof(ValorText))
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TipoNormalizado)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValorDecimal)));
        }
    }
}
