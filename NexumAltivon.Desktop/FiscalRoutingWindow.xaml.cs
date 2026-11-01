/* Operação fiscal somente por simulação autenticada na API oficial. */
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class FiscalRoutingWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private string _status = "Carregue as empresas fiscais ativas para iniciar.";

    public FiscalRoutingWindow(string operationType, TerminalProfile terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        InitializeComponent();
        _terminal = terminal;
        Title = $"GenesisGest.Net - {operationType}";
        DataContext = this;
        Loaded += async (_, _) => await LoadCompaniesAsync();
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public string Status
    {
        get => _status;
        private set { _status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadCompaniesAsync()
    {
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            Status = "Sessão administrativa ausente. Entre no Desktop para consultar os dados fiscais.";
            EmpresaOrigemBox.ItemsSource = null;
            return;
        }

        Status = "Consultando empresas fiscais persistidas…";
        var result = await _api.GetFiscalCompaniesAsync(_terminal, token, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            EmpresaOrigemBox.ItemsSource = null;
            Status = $"Falha ao consultar empresas fiscais: {result.Detail}";
            return;
        }

        var eligible = result.Data
            .Where(company => company.Ativa
                && !string.IsNullOrWhiteSpace(company.RazaoSocial)
                && !string.IsNullOrWhiteSpace(company.Cnpj)
                && !string.IsNullOrWhiteSpace(company.Estado))
            .OrderBy(company => company.RazaoSocial, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
        EmpresaOrigemBox.ItemsSource = eligible;
        EmpresaOrigemBox.SelectedIndex = eligible.Count == 0 ? -1 : 0;
        SimularButton.IsEnabled = eligible.Count > 0;
        Status = eligible.Count == 0
            ? "Nenhuma empresa ativa com CNPJ e UF cadastrados. Complete o cadastro legal antes da simulação."
            : $"{eligible.Count} empresa(s) fiscal(is) real(is) elegível(is).";
    }

    private async void CarregarEmpresas_Click(object sender, RoutedEventArgs e) => await LoadCompaniesAsync();

    private void EmpresaOrigem_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (EmpresaOrigemBox.SelectedItem is DesktopFiscalEmpresa company
            && string.IsNullOrWhiteSpace(UfDestinoBox.Text))
        {
            UfDestinoBox.Text = company.Estado ?? string.Empty;
        }
    }

    private async void Simular_Click(object sender, RoutedEventArgs e)
    {
        if (EmpresaOrigemBox.SelectedItem is not DesktopFiscalEmpresa origin)
        {
            Status = "Selecione uma empresa fiscal ativa carregada da API.";
            return;
        }
        if (!TryParseMoney(ValorProdutosBox.Text, out var productValue) || productValue <= 0)
        {
            Status = "Informe um valor de produtos maior que zero.";
            return;
        }
        if (!TryParseMoney(FreteBox.Text, out var freight) || freight < 0)
        {
            Status = "Informe um frete válido, igual ou maior que zero.";
            return;
        }

        var destination = UfDestinoBox.Text.Trim().ToUpperInvariant();
        if (destination.Length != 2 || destination.Any(character => !char.IsAsciiLetter(character)))
        {
            Status = "Informe uma UF de destino válida com duas letras.";
            return;
        }
        if (TipoOperacaoBox.SelectedItem is not ComboBoxItem { Tag: string typeText }
            || !int.TryParse(typeText, NumberStyles.None, CultureInfo.InvariantCulture, out var operationType))
        {
            Status = "Selecione um tipo de operação fiscal.";
            return;
        }

        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            Status = "Sessão administrativa ausente.";
            return;
        }

        SimularButton.IsEnabled = false;
        try
        {
            Status = "Calculando no motor fiscal da API…";
            var request = new DesktopFiscalRoutingRequest(
                operationType,
                productValue,
                freight,
                origin.Estado!,
                destination,
                origin.CategoriaFiscal,
                origin.SubcategoriaFiscal,
                origin.NaturezaOperacaoPadrao ?? "Venda de mercadoria",
                ExigeMarketplaceCheck.IsChecked == true || operationType == 2,
                ExigeDropshippingCheck.IsChecked == true || operationType == 3,
                operationType != 4,
                RequerEntradaNfeCheck.IsChecked == true || operationType == 4);

            var result = await _api.SimulateFiscalRoutingAsync(_terminal, token, request, _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                RankingGrid.ItemsSource = null;
                ResultadoText.Text = "A API não concluiu a simulação.";
                Status = $"Falha na simulação fiscal: {result.Detail}";
                return;
            }

            RankingGrid.ItemsSource = result.Data.Ranking;
            ResultadoText.Text = result.Data.Sucesso
                ? $"{result.Data.Resumo}\nEmitente sugerida: {result.Data.RazaoSocialSelecionada} · CNPJ {result.Data.CnpjSelecionado} · UF {result.Data.EstadoSelecionado}."
                : result.Data.Resumo;
            Status = result.Data.Sucesso
                ? $"Simulação concluída pela API. {result.Data.Ranking.Count} empresa(s) classificadas. Nenhum documento foi emitido."
                : "A API não encontrou emitente elegível; revise cadastro e parâmetros fiscais.";
        }
        catch (OperationCanceledException)
        {
            Status = "Operação cancelada ao fechar a janela.";
        }
        finally
        {
            SimularButton.IsEnabled = EmpresaOrigemBox.Items.Count > 0;
        }
    }

    private static bool TryParseMoney(string? value, out decimal amount) =>
        decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out amount)
        || decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
}
