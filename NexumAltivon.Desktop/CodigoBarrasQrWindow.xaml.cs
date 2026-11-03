/* Versão: 1.1.5.7258 | Gestão de códigos de barras e QR Code via API */
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class CodigoBarrasQrWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopProdutoIdentificador? _selectedProduct;
    private string _status = "Aguardando sessão administrativa.";
    private bool _loadingSelection;
    private int _currentPage = 1;
    private int _totalPages = 1;
    private int _totalProducts;

    public CodigoBarrasQrWindow(TerminalProfile terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        InitializeComponent();
        _terminal = terminal;
        DataContext = this;
        Loaded += async (_, _) => await CarregarAsync();
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopProdutoIdentificador> Produtos { get; } = [];
    public string Status { get => _status; private set { _status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); } }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task CarregarAsync(bool resetPage = false)
    {
        if (resetPage) _currentPage = 1;
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Produtos.Clear();
            Status = "Sessão JWT ausente. Autentique o terminal antes de consultar identificadores.";
            return;
        }

        Status = "Consultando produtos do tenant autenticado…";
        var selectedId = _selectedProduct?.Id;
        var result = await _api.GetProductIdentifiersAsync(_terminal, _terminal.DesktopAccessToken, PesquisaBox.Text, _currentPage, _lifetime.Token);
        if (!result.Sucesso || result.Dados is null)
        {
            Produtos.Clear();
            Status = $"Falha ao carregar produtos: {result.Mensagem}";
            return;
        }

        Produtos.Clear();
        foreach (var product in result.Dados) Produtos.Add(product);
        _totalProducts = result.TotalRegistros ?? Produtos.Count;
        _currentPage = result.PaginaAtual ?? _currentPage;
        _totalPages = Math.Max(1, result.TotalPaginas ?? 1);
        PaginacaoText.Text = $"Página {_currentPage} de {_totalPages} · {_totalProducts} produto(s)";
        PaginaAnteriorButton.IsEnabled = _currentPage > 1;
        ProximaPaginaButton.IsEnabled = _currentPage < _totalPages;
        Status = $"{Produtos.Count} produto(s) nesta página; {_totalProducts} no resultado.";
        if (selectedId.HasValue)
            ProdutosGrid.SelectedItem = Produtos.FirstOrDefault(item => item.Id == selectedId.Value);
    }

    private async void Buscar_Click(object sender, RoutedEventArgs e) => await CarregarAsync(resetPage: true);
    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();
    private async void PaginaAnterior_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage <= 1) return;
        _currentPage--;
        await CarregarAsync();
    }

    private async void ProximaPagina_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage >= _totalPages) return;
        _currentPage++;
        await CarregarAsync();
    }

    private void ProdutosGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedProduct = ProdutosGrid.SelectedItem as DesktopProdutoIdentificador;
        _loadingSelection = true;
        ProdutoSelecionadoText.Text = _selectedProduct is null
            ? "Nenhum produto selecionado."
            : $"{_selectedProduct.Nome}\nSKU: {_selectedProduct.Sku} | Loja: {_selectedProduct.LojaId} | ID: {_selectedProduct.Id}";
        CodigoBarrasBox.Text = _selectedProduct?.CodigoBarras ?? string.Empty;
        QrCodeBox.Text = _selectedProduct?.QrCode ?? string.Empty;
        _loadingSelection = false;
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_loadingSelection) return;
        if (_selectedProduct is null)
        {
            Status = "Selecione um produto antes de salvar.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhuma alteração foi enviada.";
            return;
        }

        var barcode = string.IsNullOrWhiteSpace(CodigoBarrasBox.Text) ? null : CodigoBarrasBox.Text.Trim();
        var qrCode = string.IsNullOrWhiteSpace(QrCodeBox.Text) ? null : QrCodeBox.Text.Trim();
        if (barcode is null && qrCode is null)
        {
            Status = "Informe ao menos um identificador; nenhum dado foi gravado.";
            return;
        }

        Status = "Gravando identificadores e validando concorrência…";
        var result = await _api.UpdateProductIdentifiersAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            _selectedProduct.Id,
            new DesktopProdutoIdentificadorUpdateRequest(barcode, qrCode, _selectedProduct.RowVersion),
            _lifetime.Token);
        if (!result.Sucesso || result.Dados is null)
        {
            Status = $"Falha ao salvar: {result.Mensagem}";
            return;
        }

        var updated = result.Dados;
        var index = Produtos.IndexOf(_selectedProduct);
        if (index >= 0) Produtos[index] = updated;
        ProdutosGrid.SelectedItem = updated;
        _selectedProduct = updated;
        Status = "Identificadores salvos pela API; RowVersion atualizada.";
    }
}
