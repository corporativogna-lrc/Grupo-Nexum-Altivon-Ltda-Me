using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class ProductManagementWindow : Window
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private int _page = 1;
    private int _totalPages;
    private bool _busy;

    public ProductManagementWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => _lifetime.Cancel();
    }

    public ObservableCollection<DesktopProdutoAdmin> Produtos { get; } = [];
    public DesktopProdutoAdmin? ProdutoSelecionado { get; set; }

    private async void Pesquisar_Click(object sender, RoutedEventArgs e)
    {
        _page = 1;
        await LoadAsync(BuscaBox.Text);
    }

    private async void Atualizar_Click(object sender, RoutedEventArgs e) => await LoadAsync(BuscaBox.Text);

    private async void BuscaBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            _page = 1;
            await LoadAsync(BuscaBox.Text);
        }
    }

    private async void Anterior_Click(object sender, RoutedEventArgs e)
    {
        if (_page > 1) { _page--; await LoadAsync(BuscaBox.Text); }
    }

    private async void Proxima_Click(object sender, RoutedEventArgs e)
    {
        if (_page < _totalPages) { _page++; await LoadAsync(BuscaBox.Text); }
    }

    private void ProdutosGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (ProdutosGrid.SelectedItem is DesktopProdutoAdmin item) PopulateForm(item);
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        var item = ProdutosGrid.SelectedItem as DesktopProdutoAdmin;
        if (item is null)
        {
            StatusText.Text = "Selecione um produto persistido antes de salvar.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            StatusText.Text = "Sessão administrativa ausente. Autentique o terminal antes de editar produtos.";
            return;
        }
        if (string.IsNullOrWhiteSpace(NomeBox.Text) || string.IsNullOrWhiteSpace(SkuBox.Text)
            || !TryDecimal(PrecoBox.Text, out var preco) || preco < 0
            || !TryOptionalDecimal(PrecoPromocionalBox.Text, out var promocional)
            || !TryDecimal(CustoBox.Text, out var custo) || custo < 0
            || !int.TryParse(EstoqueBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var estoque) || estoque < 0
            || !int.TryParse(EstoqueMinimoBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var estoqueMinimo) || estoqueMinimo < 0)
        {
            StatusText.Text = "Revise nome, SKU, valores e quantidades. Preços e estoque não podem ser negativos.";
            return;
        }

        var request = new DesktopProdutoUpdateRequest(
            item.Slug, NomeBox.Text.Trim(), DescricaoBox.Text.Trim(), DescricaoBox.Text.Trim(),
            preco, promocional, item.ImagemUrl, estoque, estoqueMinimo, item.EstoqueReservado,
            DestaqueCheck.IsChecked == true, SkuBox.Text.Trim(), CategoriaBox.Text.Trim(), null,
            null, custo, item.Peso, item.Altura, item.Largura, item.Comprimento, item.TipoProduto,
            item.FornecedorId, EmptyToNull(MarcaBox.Text), EmptyToNull(TagsBox.Text), item.SeoTitulo,
            item.SeoDescricao, item.SeoKeywords, item.ImagensGaleria);

        SetBusy(true, "Salvando edição pela API…");
        try
        {
            var saved = await _api.UpdateProductAsync(_terminal, _terminal.DesktopAccessToken, item.Slug, request, _lifetime.Token);
            if (!saved.Success || saved.Data is null)
            {
                StatusText.Text = $"API não confirmou a edição: {saved.Detail}";
                return;
            }

            var reread = await _api.GetAdminProductsAsync(_terminal, _terminal.DesktopAccessToken, item.Slug, 1, _lifetime.Token);
            var persisted = reread.Success ? reread.Data?.Itens.FirstOrDefault(product => product.Id == item.Id) : null;
            if (persisted is null || persisted.Nome != request.Nome || persisted.Sku != request.Sku
                || persisted.Preco != request.Preco || persisted.Estoque != request.Estoque)
            {
                StatusText.Text = "A API aceitou a gravação, mas a releitura não confirmou os campos enviados. Confira o registro antes de repetir.";
                return;
            }

            StatusText.Text = $"Produto #{persisted.Id} relido e confirmado. Imagem principal preservada.";
            SetBusy(false);
            await LoadAsync(BuscaBox.Text);
            var refreshed = Produtos.FirstOrDefault(product => product.Id == persisted.Id);
            if (refreshed is not null) ProdutosGrid.SelectedItem = refreshed;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex)
        {
            StatusText.Text = $"Falha ao editar produto: {ex.Message}";
        }
        finally { SetBusy(false); }
    }

    private async Task LoadAsync(string? search = null)
    {
        if (_busy) return;
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            StatusText.Text = "Sessão administrativa ausente. Autentique o terminal para consultar o catálogo.";
            return;
        }
        SetBusy(true, "Consultando produtos na API…");
        try
        {
            var result = await _api.GetAdminProductsAsync(_terminal, _terminal.DesktopAccessToken,
                search, _page, _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                StatusText.Text = $"Falha na consulta administrativa: {result.Detail}";
                return;
            }
            Produtos.Clear();
            foreach (var product in result.Data.Itens) Produtos.Add(product);
            _totalPages = result.Data.TotalPaginas;
            StatusText.Text = $"{result.Data.Total} produto(s) · página {result.Data.Pagina} de {result.Data.TotalPaginas}.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { }
        catch (Exception ex) { StatusText.Text = $"Falha ao consultar produtos: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private void PopulateForm(DesktopProdutoAdmin item)
    {
        NomeBox.Text = item.Nome;
        SkuBox.Text = item.Sku;
        PrecoBox.Text = item.Preco.ToString("0.00", CultureInfo.CurrentCulture);
        PrecoPromocionalBox.Text = item.PrecoPromocional?.ToString("0.00", CultureInfo.CurrentCulture) ?? string.Empty;
        EstoqueBox.Text = item.Estoque.ToString(CultureInfo.CurrentCulture);
        EstoqueMinimoBox.Text = item.EstoqueMinimo.ToString(CultureInfo.CurrentCulture);
        CustoBox.Text = item.Custo.ToString("0.00", CultureInfo.CurrentCulture);
        CategoriaBox.Text = item.CategoriaId ?? string.Empty;
        MarcaBox.Text = item.Marca ?? string.Empty;
        TagsBox.Text = item.Tags ?? string.Empty;
        DescricaoBox.Text = item.Descricao ?? item.DescricaoCurta ?? string.Empty;
        DestaqueCheck.IsChecked = item.Destaque;
        ImagemAtualText.Text = string.IsNullOrWhiteSpace(item.ImagemUrl) ? "Sem imagem principal" : item.ImagemUrl;
    }

    private void SetBusy(bool busy, string? message = null)
    {
        _busy = busy;
        IsEnabled = !busy;
        if (message is not null) StatusText.Text = message;
    }

    private static bool TryDecimal(string raw, out decimal value) =>
        decimal.TryParse(raw, NumberStyles.Number, CultureInfo.CurrentCulture, out value)
        || decimal.TryParse(raw, NumberStyles.Number, CultureInfo.InvariantCulture, out value);

    private static bool TryOptionalDecimal(string raw, out decimal? value)
    {
        if (string.IsNullOrWhiteSpace(raw)) { value = null; return true; }
        if (TryDecimal(raw, out var parsed)) { value = parsed; return parsed >= 0; }
        value = null;
        return false;
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
