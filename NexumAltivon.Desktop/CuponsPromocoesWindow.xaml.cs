/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7222 Data: 16/08/2026
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class CuponsPromocoesWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopCupomAdmin? _selectedCupom;
    private string _status = "Aguardando carregamento da API oficial.";
    private string _resumo = "Nenhum cupom carregado.";
    private string _selectedDetails = "Nenhum cupom selecionado.";

    public CuponsPromocoesWindow(TerminalProfile terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        InitializeComponent();
        _terminal = terminal;
        DataContext = this;
        Loaded += async (_, _) => await CarregarAsync();
        Closed += (_, _) =>
        {
            _lifetime.Cancel();
            _lifetime.Dispose();
        };
    }

    public ObservableCollection<DesktopCupomAdmin> Cupons { get; } = new();

    public DesktopCupomAdmin? SelectedCupom
    {
        get => _selectedCupom;
        private set
        {
            if (!SetField(ref _selectedCupom, value))
            {
                return;
            }

            SelectedDetails = value is null
                ? "Novo cupom. Nenhum dado foi gravado."
                : $"ID: {value.Id}\nCódigo: {value.Codigo}\nUsos: {value.UsosAtuais}{(value.QuantidadeUsos.HasValue ? $"/{value.QuantidadeUsos.Value}" : string.Empty)}\nEstado: {(value.Ativo ? "ativo" : "inativo")}\nAtualizado em: {value.UpdatedAt:dd/MM/yyyy HH:mm}";
        }
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string Resumo
    {
        get => _resumo;
        private set => SetField(ref _resumo, value);
    }

    public string SelectedDetails
    {
        get => _selectedDetails;
        private set => SetField(ref _selectedDetails, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task<bool> CarregarAsync(int? selecionarId = null)
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de gerir cupons.";
            Cupons.Clear();
            Resumo = "A consulta não foi enviada porque não existe sessão administrativa.";
            return false;
        }

        Status = "Consultando cupons da API oficial...";
        var result = await _api.GetCuponsAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Cupons não carregados: {result.Detail}";
            Resumo = "A API não confirmou a leitura dos registros.";
            return false;
        }

        Cupons.Clear();
        foreach (var cupom in result.Data.OrderBy(item => item.Codigo, StringComparer.OrdinalIgnoreCase))
        {
            Cupons.Add(cupom);
        }

        Resumo = $"{Cupons.Count} cupom(ns) lido(s) do banco oficial. Ativos: {Cupons.Count(item => item.Ativo)}.";
        var id = selecionarId ?? _selectedCupom?.Id;
        SelectedCupom = id.HasValue ? Cupons.FirstOrDefault(item => item.Id == id.Value) : null;
        if (SelectedCupom is not null)
        {
            CuponsGrid.SelectedItem = SelectedCupom;
        }

        Status = "Consulta concluída com dados retornados pela API oficial.";
        return true;
    }

    private void CuponsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedCupom = CuponsGrid.SelectedItem as DesktopCupomAdmin;
        if (SelectedCupom is not null)
        {
            PreencherFormulario(SelectedCupom);
        }
    }

    private void PreencherFormulario(DesktopCupomAdmin cupom)
    {
        CodigoBox.Text = cupom.Codigo;
        TipoBox.SelectedItem = TipoBox.Items
            .OfType<ComboBoxItem>()
            .FirstOrDefault(item => string.Equals(item.Content?.ToString(), cupom.Tipo, StringComparison.OrdinalIgnoreCase));
        ValorBox.Text = cupom.Valor.ToString("N2", CultureInfo.CurrentCulture);
        ValorMinimoBox.Text = cupom.ValorMinimoPedido.ToString("N2", CultureInfo.CurrentCulture);
        ValorMaximoBox.Text = cupom.ValorMaximoDesconto?.ToString("N2", CultureInfo.CurrentCulture) ?? string.Empty;
        QuantidadeUsosBox.Text = cupom.QuantidadeUsos?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        QuantidadeClienteBox.Text = cupom.QuantidadePorCliente.ToString(CultureInfo.InvariantCulture);
        ValidoDeBox.Text = cupom.ValidoDe?.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture) ?? string.Empty;
        ValidoAteBox.Text = cupom.ValidoAte?.ToString("dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture) ?? string.Empty;
        PrimeiroCompraBox.IsChecked = cupom.PrimeiroCompraOnly;
        AtivoBox.IsChecked = cupom.Ativo;
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        CuponsGrid.SelectedItem = null;
        SelectedCupom = null;
        CodigoBox.Text = string.Empty;
        TipoBox.SelectedIndex = 0;
        ValorBox.Text = string.Empty;
        ValorMinimoBox.Text = "0";
        ValorMaximoBox.Text = string.Empty;
        QuantidadeUsosBox.Text = string.Empty;
        QuantidadeClienteBox.Text = "0";
        ValidoDeBox.Text = string.Empty;
        ValidoAteBox.Text = string.Empty;
        PrimeiroCompraBox.IsChecked = false;
        AtivoBox.IsChecked = true;
        Status = "Novo cupom pronto para preenchimento. Nenhum dado foi gravado.";
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (!TryBuildRequest(out var request, out var validationError))
        {
            Status = validationError;
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhum dado foi enviado.";
            MessageBox.Show(Status, "Sessão necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Status = SelectedCupom is null
            ? "Cadastrando cupom na API oficial..."
            : $"Atualizando cupom {SelectedCupom.Id} na API oficial...";
        var result = await _api.SaveCupomAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            SelectedCupom?.Id,
            request,
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Cupom não persistido: {result.Detail}";
            MessageBox.Show(Status, "Falha na persistência", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!await CarregarAsync(result.Data.Id))
        {
            Status = "A API respondeu, mas a releitura oficial falhou. Persistência não confirmada.";
            return;
        }

        var persisted = Cupons.FirstOrDefault(item => item.Id == result.Data.Id);
        if (persisted is null)
        {
            Status = "A API respondeu, mas o cupom não apareceu na releitura oficial. Persistência não confirmada.";
            MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CuponsGrid.SelectedItem = persisted;
        SelectedCupom = persisted;
        PreencherFormulario(persisted);
        Status = $"Cupom {persisted.Codigo} persistido e relido no banco oficial.";
    }

    private async void Desativar_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedCupom is null)
        {
            MessageBox.Show("Selecione um cupom ativo na grade.", "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!SelectedCupom.Ativo)
        {
            Status = "O cupom selecionado já está inativo.";
            return;
        }

        if (MessageBox.Show(
                $"Desativar o cupom {SelectedCupom.Codigo}? A operação altera o registro oficial.",
                "Confirmar desativação",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhum dado foi enviado.";
            return;
        }

        var id = SelectedCupom.Id;
        Status = $"Desativando cupom {id} na API oficial...";
        var result = await _api.DeactivateCupomAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
        if (!result.Success)
        {
            Status = $"Cupom não desativado: {result.Detail}";
            MessageBox.Show(Status, "Falha na persistência", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!await CarregarAsync(id))
        {
            Status = "A API respondeu, mas a releitura oficial falhou. Desativação não confirmada.";
            return;
        }

        var persisted = Cupons.FirstOrDefault(item => item.Id == id);
        if (persisted is null || persisted.Ativo)
        {
            Status = "A API respondeu, mas a releitura não confirmou o estado inativo.";
            MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CuponsGrid.SelectedItem = persisted;
        SelectedCupom = persisted;
        PreencherFormulario(persisted);
        Status = $"Cupom {persisted.Codigo} desativado e relido no banco oficial.";
    }

    private bool TryBuildRequest(out DesktopCupomRequest request, out string error)
    {
        request = default!;
        error = string.Empty;
        var codigo = CodigoBox.Text.Trim().ToUpperInvariant();
        if (codigo.Length is < 3 or > 50 || codigo.Any(character => !char.IsLetterOrDigit(character) && character is not '-' and not '_'))
        {
            error = "O código deve ter entre 3 e 50 caracteres e aceitar apenas letras, números, hífen e sublinhado.";
            return false;
        }

        var tipo = (TipoBox.SelectedItem as ComboBoxItem)?.Content?.ToString();
        if (string.IsNullOrWhiteSpace(tipo))
        {
            error = "Selecione o tipo do cupom.";
            return false;
        }

        if (!TryParseDecimal(ValorBox.Text, out var valor) || valor < 0m)
        {
            error = "Informe um valor numérico válido para o cupom.";
            return false;
        }

        if (!TryParseDecimal(ValorMinimoBox.Text, out var valorMinimo) || valorMinimo < 0m)
        {
            error = "Informe um valor mínimo de pedido maior ou igual a zero.";
            return false;
        }

        decimal? valorMaximo = null;
        if (!string.IsNullOrWhiteSpace(ValorMaximoBox.Text))
        {
            if (!TryParseDecimal(ValorMaximoBox.Text, out var parsedMaximo) || parsedMaximo <= 0m)
            {
                error = "O desconto máximo deve ser vazio ou maior que zero.";
                return false;
            }

            valorMaximo = parsedMaximo;
        }

        int? quantidadeUsos = null;
        if (!string.IsNullOrWhiteSpace(QuantidadeUsosBox.Text))
        {
            if (!int.TryParse(QuantidadeUsosBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedQuantidade) || parsedQuantidade <= 0)
            {
                error = "A quantidade total de usos deve ser vazia ou um inteiro maior que zero.";
                return false;
            }

            quantidadeUsos = parsedQuantidade;
        }

        if (!int.TryParse(QuantidadeClienteBox.Text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var quantidadeCliente) || quantidadeCliente < 0)
        {
            error = "Usos por cliente deve ser um inteiro maior ou igual a zero.";
            return false;
        }

        if (!TryParseDate(ValidoDeBox.Text, out var validoDe) || !TryParseDate(ValidoAteBox.Text, out var validoAte))
        {
            error = "As datas devem estar vazias ou no formato dd/MM/yyyy HH:mm.";
            return false;
        }

        if (validoDe.HasValue && validoAte.HasValue && validoAte.Value < validoDe.Value)
        {
            error = "A data final da vigência deve ser igual ou posterior à data inicial.";
            return false;
        }

        if (string.Equals(tipo, "Percentual", StringComparison.OrdinalIgnoreCase) && (valor <= 0m || valor > 100m))
        {
            error = "Cupom percentual deve ter valor maior que zero e menor ou igual a 100.";
            return false;
        }

        if (string.Equals(tipo, "ValorFixo", StringComparison.OrdinalIgnoreCase) && valor <= 0m)
        {
            error = "Cupom de valor fixo deve ter valor maior que zero.";
            return false;
        }

        if (string.Equals(tipo, "FreteGratis", StringComparison.OrdinalIgnoreCase) && valor != 0m)
        {
            error = "Cupom de frete grátis deve ter valor zero.";
            return false;
        }

        request = new DesktopCupomRequest(
            codigo,
            tipo,
            valor,
            valorMinimo,
            valorMaximo,
            quantidadeUsos,
            quantidadeCliente,
            validoDe,
            validoAte,
            PrimeiroCompraBox.IsChecked == true,
            AtivoBox.IsChecked == true);
        return true;
    }

    private static bool TryParseDecimal(string text, out decimal value)
    {
        return decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value)
            || decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static bool TryParseDate(string text, out DateTime? value)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            value = null;
            return true;
        }

        if (DateTime.TryParseExact(text.Trim(), "dd/MM/yyyy HH:mm", CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out var parsed)
            || DateTime.TryParse(text.Trim(), CultureInfo.CurrentCulture, DateTimeStyles.AllowWhiteSpaces, out parsed))
        {
            value = parsed;
            return true;
        }

        value = null;
        return false;
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        return true;
    }
}
