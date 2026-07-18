/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7183
 */

using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class ManualNfeWindow : Window, INotifyPropertyChanged
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private static readonly CultureInfo BrazilianCulture = CultureInfo.GetCultureInfo("pt-BR");
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private string _empresaEmissora = "Grupo Nexum Altivon Ltda. Me.";
    private string _cnpjEmissor = string.Empty;
    private string _clienteDestinatario = string.Empty;
    private string _documentoDestinatario = string.Empty;
    private string _naturezaOperacao = "Venda de mercadoria";
    private string _cfop = "5102";
    private string _estadoOrigem = "SP";
    private string _estadoDestino = "SP";
    private string _subtotal = string.Empty;
    private string _frete = "0,00";
    private string _impostosEstimados = "0,00";
    private string _margemMinima = "0,00";
    private string _observacoes = string.Empty;
    private string _previewResumo = "Aguardando leitura do rascunho fiscal persistido.";
    private bool _busy;

    public ManualNfeWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
    }

    public string EmpresaEmissora { get => _empresaEmissora; set => SetField(ref _empresaEmissora, value); }
    public string CnpjEmissor { get => _cnpjEmissor; set => SetField(ref _cnpjEmissor, value); }
    public string ClienteDestinatario { get => _clienteDestinatario; set => SetField(ref _clienteDestinatario, value); }
    public string DocumentoDestinatario { get => _documentoDestinatario; set => SetField(ref _documentoDestinatario, value); }
    public string NaturezaOperacao { get => _naturezaOperacao; set => SetField(ref _naturezaOperacao, value); }
    public string Cfop { get => _cfop; set => SetField(ref _cfop, value); }
    public string EstadoOrigem { get => _estadoOrigem; set => SetField(ref _estadoOrigem, (value ?? string.Empty).ToUpperInvariant()); }
    public string EstadoDestino { get => _estadoDestino; set => SetField(ref _estadoDestino, (value ?? string.Empty).ToUpperInvariant()); }
    public string Subtotal { get => _subtotal; set => SetField(ref _subtotal, value); }
    public string Frete { get => _frete; set => SetField(ref _frete, value); }
    public string ImpostosEstimados { get => _impostosEstimados; set => SetField(ref _impostosEstimados, value); }
    public string MargemMinima { get => _margemMinima; set => SetField(ref _margemMinima, value); }
    public string Observacoes { get => _observacoes; set => SetField(ref _observacoes, value); }
    public string PreviewResumo { get => _previewResumo; private set => SetField(ref _previewResumo, value); }
    public bool PodeOperar => !_busy;

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await CarregarRascunhoAsync();

    private async void SalvarRascunho_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildRequest(out var request, out var validationError))
        {
            PreviewResumo = validationError;
            return;
        }

        if (!HasAdministrativeSession())
        {
            return;
        }

        SetBusy(true);
        try
        {
            var saved = await _api.SaveManualNfeDraftAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                request);
            if (!saved.Success || saved.Data is null || !saved.Data.Salvo)
            {
                PreviewResumo = $"Rascunho não gravado. {saved.Detail}";
                return;
            }

            var reread = await _api.GetManualNfeDraftAsync(_terminal, _terminal.DesktopAccessToken);
            if (!reread.Success || reread.Data is null || !reread.Data.Existe || string.IsNullOrWhiteSpace(reread.Data.Valor))
            {
                PreviewResumo = $"A API informou gravação, mas a releitura não confirmou o rascunho. {reread.Detail}";
                return;
            }

            var persisted = JsonSerializer.Deserialize<DesktopFiscalManualEmissaoRequest>(reread.Data.Valor, JsonOptions);
            if (persisted is null || persisted != request)
            {
                PreviewResumo = "A releitura divergiu do conteúdo enviado; o salvamento não foi confirmado.";
                return;
            }

            ApplyDraft(persisted);
            PreviewResumo = $"Rascunho fiscal confirmado no banco em {saved.Data.SalvoEm.ToLocalTime():dd/MM/yyyy HH:mm:ss}.";
        }
        catch (JsonException ex)
        {
            PreviewResumo = $"Rascunho gravado com conteúdo inválido para releitura: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void PrepararEmissao_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildRequest(out var request, out var validationError))
        {
            PreviewResumo = validationError;
            return;
        }

        if (!HasAdministrativeSession())
        {
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _api.PrepareManualNfeAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                request);
            if (!result.Success || result.Data is null)
            {
                PreviewResumo = $"Preparação fiscal não confirmada. {result.Detail}";
                return;
            }

            var dto = result.Data;
            var selectedCompany = string.IsNullOrWhiteSpace(dto.RazaoSocialSelecionada)
                ? "nenhuma empresa elegível"
                : dto.RazaoSocialSelecionada;
            var pending = dto.Pendencias.Count == 0
                ? "sem pendências de preparação"
                : string.Join(" | ", dto.Pendencias);
            PreviewResumo = $"{dto.CertificadoStatus} | {dto.RoteamentoResumo} | Empresa: {selectedCompany} | {pending}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async Task CarregarRascunhoAsync()
    {
        if (!HasAdministrativeSession())
        {
            return;
        }

        SetBusy(true);
        try
        {
            var result = await _api.GetManualNfeDraftAsync(_terminal, _terminal.DesktopAccessToken);
            if (!result.Success || result.Data is null)
            {
                PreviewResumo = $"Rascunho não carregado. {result.Detail}";
                return;
            }

            if (!result.Data.Existe || string.IsNullOrWhiteSpace(result.Data.Valor))
            {
                PreviewResumo = "Nenhum rascunho fiscal persistido para este ambiente.";
                return;
            }

            var draft = JsonSerializer.Deserialize<DesktopFiscalManualEmissaoRequest>(result.Data.Valor, JsonOptions);
            if (draft is null)
            {
                PreviewResumo = "A API retornou um rascunho sem conteúdo fiscal reconhecível.";
                return;
            }

            ApplyDraft(draft);
            PreviewResumo = result.Data.UpdatedAt.HasValue
                ? $"Rascunho fiscal relido do banco em {result.Data.UpdatedAt.Value.ToLocalTime():dd/MM/yyyy HH:mm:ss}."
                : "Rascunho fiscal relido do banco; horário de atualização ausente.";
        }
        catch (JsonException ex)
        {
            PreviewResumo = $"O rascunho persistido não possui JSON fiscal válido: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private bool TryBuildRequest(out DesktopFiscalManualEmissaoRequest request, out string error)
    {
        request = default!;
        error = string.Empty;
        var emitterDocument = OnlyDigits(CnpjEmissor);
        var recipientDocument = OnlyDigits(DocumentoDestinatario);
        var cfop = OnlyDigits(Cfop);
        var origin = NormalizeState(EstadoOrigem);
        var destination = NormalizeState(EstadoDestino);

        if (string.IsNullOrWhiteSpace(EmpresaEmissora) || emitterDocument.Length != 14)
        {
            error = "Informe a empresa emissora e um CNPJ de 14 dígitos.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(ClienteDestinatario) || recipientDocument.Length is not (11 or 14))
        {
            error = "Informe o destinatário e um CPF/CNPJ válido em quantidade de dígitos.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(NaturezaOperacao) || cfop.Length != 4)
        {
            error = "Natureza da operação e CFOP de quatro dígitos são obrigatórios.";
            return false;
        }

        if (origin.Length != 2 || destination.Length != 2)
        {
            error = "UF de origem e destino devem conter duas letras.";
            return false;
        }

        if (!TryParseDecimal(Subtotal, out var subtotal) || subtotal <= 0)
        {
            error = "Subtotal deve ser um valor monetário maior que zero.";
            return false;
        }

        if (!TryParseNonNegative(Frete, out var freight)
            || !TryParseNonNegative(ImpostosEstimados, out var taxes)
            || !TryParseNonNegative(MargemMinima, out var minimumMargin))
        {
            error = "Frete, impostos e margem devem ser números válidos e não negativos.";
            return false;
        }

        if (minimumMargin > 100)
        {
            error = "Margem mínima deve ficar entre zero e cem por cento.";
            return false;
        }

        request = new DesktopFiscalManualEmissaoRequest(
            EmpresaEmissora.Trim(),
            emitterDocument,
            ClienteDestinatario.Trim(),
            recipientDocument,
            NaturezaOperacao.Trim(),
            cfop,
            subtotal,
            freight,
            taxes,
            minimumMargin,
            TrimOrNull(Observacoes),
            "VendaInterna",
            origin,
            destination,
            null,
            null,
            false,
            false,
            true,
            false);
        return true;
    }

    private bool HasAdministrativeSession()
    {
        if (!string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            return true;
        }

        PreviewResumo = "Sessão JWT ausente. Autentique o terminal em Configurações antes de operar o fiscal.";
        return false;
    }

    private void ApplyDraft(DesktopFiscalManualEmissaoRequest draft)
    {
        EmpresaEmissora = draft.EmpresaEmissora;
        CnpjEmissor = draft.CnpjEmissor;
        ClienteDestinatario = draft.ClienteDestinatario;
        DocumentoDestinatario = draft.DocumentoDestinatario;
        NaturezaOperacao = draft.NaturezaOperacao;
        Cfop = draft.Cfop;
        EstadoOrigem = draft.EstadoOrigem;
        EstadoDestino = draft.EstadoDestino;
        Subtotal = FormatDecimal(draft.Subtotal);
        Frete = FormatDecimal(draft.Frete);
        ImpostosEstimados = FormatDecimal(draft.ImpostosEstimados);
        MargemMinima = FormatDecimal(draft.MargemMinima);
        Observacoes = draft.Observacoes ?? string.Empty;
    }

    private void SetBusy(bool value)
    {
        _busy = value;
        OnPropertyChanged(nameof(PodeOperar));
    }

    private static bool TryParseNonNegative(string? value, out decimal result) =>
        TryParseDecimal(value, out result) && result >= 0;

    private static bool TryParseDecimal(string? value, out decimal result)
    {
        var text = value?.Trim();
        return decimal.TryParse(text, NumberStyles.Number, BrazilianCulture, out result)
            || decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out result);
    }

    private static string FormatDecimal(decimal value) => value.ToString("N2", BrazilianCulture);
    private static string OnlyDigits(string? value) => new((value ?? string.Empty).Where(char.IsDigit).ToArray());
    private static string NormalizeState(string? value) =>
        new string((value ?? string.Empty).Where(char.IsLetter).ToArray()).ToUpperInvariant();
    private static string? TrimOrNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        OnPropertyChanged(propertyName);
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
