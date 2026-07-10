/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Net.Http;
using System.Net.Http.Json;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;

namespace NexumAltivon.Desktop;

public partial class ManualNfeWindow : Window, INotifyPropertyChanged
{
    private const string ApiBaseUrl = "http://192.168.1.72:5010";

    private string _empresaEmissora = "Grupo Nexum Altivon Ltda. Me.";
    private string _cnpjEmissor = "";
    private string _clienteDestinatario = "";
    private string _documentoDestinatario = "";
    private string _naturezaOperacao = "Venda de mercadoria";
    private string _cfop = "5102";
    private string _subtotal = "";
    private string _frete = "";
    private string _impostosEstimados = "";
    private string _margemMinima = "";
    private string _observacoes = "";
    private string _previewResumo = "Aguardando preparação da emissão.";

    public string EmpresaEmissora { get => _empresaEmissora; set => SetField(ref _empresaEmissora, value); }
    public string CnpjEmissor { get => _cnpjEmissor; set => SetField(ref _cnpjEmissor, value); }
    public string ClienteDestinatario { get => _clienteDestinatario; set => SetField(ref _clienteDestinatario, value); }
    public string DocumentoDestinatario { get => _documentoDestinatario; set => SetField(ref _documentoDestinatario, value); }
    public string NaturezaOperacao { get => _naturezaOperacao; set => SetField(ref _naturezaOperacao, value); }
    public string Cfop { get => _cfop; set => SetField(ref _cfop, value); }
    public string Subtotal { get => _subtotal; set => SetField(ref _subtotal, value); }
    public string Frete { get => _frete; set => SetField(ref _frete, value); }
    public string ImpostosEstimados { get => _impostosEstimados; set => SetField(ref _impostosEstimados, value); }
    public string MargemMinima { get => _margemMinima; set => SetField(ref _margemMinima, value); }
    public string Observacoes { get => _observacoes; set => SetField(ref _observacoes, value); }
    public string PreviewResumo { get => _previewResumo; set => SetField(ref _previewResumo, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ManualNfeWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void SalvarRascunho_Click(object sender, RoutedEventArgs e)
    {
        _ = SalvarRascunhoAsync();
    }

    private async void PrepararEmissao_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
            var request = BuildRequest();

            using var response = await http.PostAsJsonAsync("/api/fiscal/preparar-emissao-manual", request);
            if (!response.IsSuccessStatusCode)
            {
                PreviewResumo = $"API retornou {(int)response.StatusCode} ao preparar a emissão manual.";
                return;
            }

            var payload = await response.Content.ReadFromJsonAsync<ApiResponse<FiscalManualEmissaoResponseDto>>();
            if (payload?.Success == true && payload.Data is not null)
            {
                var dto = payload.Data;
                PreviewResumo = $"{dto.CertificadoStatus} | {dto.RoteamentoResumo} | Selecionada: {dto.RazaoSocialSelecionada ?? "nenhuma"}";
                if (dto.Pendencias.Count > 0)
                {
                    Observacoes = string.Join(" | ", dto.Pendencias);
                }
            }
            else
            {
                PreviewResumo = payload?.Message ?? "A API não retornou o retorno esperado para a emissão manual.";
            }
        }
        catch (Exception ex)
        {
            PreviewResumo = $"Falha ao consultar a API: {ex.Message}";
        }
    }

    private async Task SalvarRascunhoAsync()
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
            var request = BuildRequest();
            using var response = await http.PostAsJsonAsync("/api/fiscal/rascunho-manual", request);

            if (!response.IsSuccessStatusCode)
            {
                PreviewResumo = $"Falha ao salvar rascunho: {(int)response.StatusCode}.";
                return;
            }

            PreviewResumo = "Rascunho salvo no servidor com sucesso.";
        }
        catch (Exception ex)
        {
            PreviewResumo = $"Falha ao salvar rascunho: {ex.Message}";
        }
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private static decimal ParseDecimal(string? value)
    {
        return decimal.TryParse(value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var result)
            ? result
            : 0m;
    }

    private FiscalManualEmissaoRequest BuildRequest() =>
        new(
            EmpresaEmissora,
            CnpjEmissor,
            ClienteDestinatario,
            DocumentoDestinatario,
            NaturezaOperacao,
            Cfop,
            ParseDecimal(Subtotal),
            ParseDecimal(Frete),
            ParseDecimal(ImpostosEstimados),
            ParseDecimal(MargemMinima),
            Observacoes,
            "VendaInterna",
            "SP",
            InferState(DocumentoDestinatario),
            null,
            null,
            false,
            false,
            true,
            false);

    private static string InferState(string? value) => "SP";

    private sealed record ApiResponse<T>(bool Sucesso, string? Mensagem, T? Dados)
    {
        public T? Data => Dados;
        public bool Success => Sucesso;
        public string? Message => Mensagem;
    }

    private sealed record FiscalManualEmissaoRequest(
        string EmpresaEmissora,
        string CnpjEmissor,
        string ClienteDestinatario,
        string DocumentoDestinatario,
        string NaturezaOperacao,
        string Cfop,
        decimal Subtotal,
        decimal Frete,
        decimal ImpostosEstimados,
        decimal MargemMinima,
        string? Observacoes,
        string TipoOperacao,
        string EstadoOrigem,
        string EstadoDestino,
        string? CategoriaFiscal,
        string? SubcategoriaFiscal,
        bool ExigeMarketplace,
        bool ExigeDropshipping,
        bool RequerSaidaNfe,
        bool RequerEntradaNfe);

    private sealed record FiscalManualEmissaoResponseDto(
        bool CertificadoOperacional,
        string CertificadoStatus,
        string? CertificadoReferencia,
        string RoteamentoResumo,
        string? CodigoEmpresaSelecionada,
        string? RazaoSocialSelecionada,
        string? CnpjSelecionado,
        string? EstadoSelecionado,
        List<string> Pendencias,
        DateTime GeradoEm);
}
