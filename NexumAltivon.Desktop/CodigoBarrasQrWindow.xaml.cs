/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA ChatGPT/Codex Sophia
 * Sistema de gestão: GenesisGest.Net
 * Versão: 1.1.5.7195
 */
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    private DesktopProdutoIdentificador? _selected;
    private string _status = "Aguardando sessão autenticada.";
    private int _page = 1;
    private int _pageCount = 1;

    public CodigoBarrasQrWindow(TerminalProfile terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        InitializeComponent();
        _terminal = terminal;
        DataContext = this;
        Loaded += async (_, _) => await LoadAsync(resetPage: true);
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopProdutoIdentificador> Produtos { get; } = [];
    public string Status
    {
        get => _status;
        private set { _status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadAsync(bool resetPage = false)
    {
        if (resetPage) _page = 1;
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            Produtos.Clear();
            Status = "Sessão ausente. Entre no Desktop antes de consultar os identificadores.";
            return;
        }

        Status = "Consultando catálogo administrativo…";
        var result = await _api.GetProductIdentifiersAsync(_terminal, token, PesquisaBox.Text, _page, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Produtos.Clear();
            Status = $"Falha ao consultar produtos: {result.Detail}";
            return;
        }

        var selectedId = _selected?.Id;
        Produtos.Clear();
        foreach (var item in result.Data.Itens) Produtos.Add(item);
        _page = result.Data.Pagina;
        _pageCount = Math.Max(1, result.Data.TotalPaginas);
        PaginaText.Text = $"Página {_page} de {_pageCount} · {result.Data.Total} produtos";
        AnteriorButton.IsEnabled = _page > 1;
        ProximaButton.IsEnabled = _page < _pageCount;
        Status = $"{Produtos.Count} produto(s) nesta página.";
        if (selectedId.HasValue)
            ProdutosGrid.SelectedItem = Produtos.FirstOrDefault(item => item.Id == selectedId.Value);
    }

    private async void Buscar_Click(object sender, RoutedEventArgs e) => await LoadAsync(resetPage: true);
    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void Anterior_Click(object sender, RoutedEventArgs e)
    {
        if (_page <= 1) return;
        _page--;
        await LoadAsync();
    }
    private async void Proxima_Click(object sender, RoutedEventArgs e)
    {
        if (_page >= _pageCount) return;
        _page++;
        await LoadAsync();
    }

    private void ProdutosGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = ProdutosGrid.SelectedItem as DesktopProdutoIdentificador;
        ProdutoText.Text = _selected is null
            ? "Selecione um produto."
            : $"{_selected.Nome}\nSKU: {_selected.Sku} · Loja: {_selected.LojaId} · ID: {_selected.Id}";
        CodigoBarrasBox.Text = _selected?.CodigoBarras ?? string.Empty;
        QrCodeBox.Text = _selected?.QrCode ?? string.Empty;
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_selected is null)
        {
            Status = "Selecione um produto antes de salvar.";
            return;
        }
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão ausente. Nenhuma alteração foi enviada.";
            return;
        }

        var barcode = string.IsNullOrWhiteSpace(CodigoBarrasBox.Text) ? null : CodigoBarrasBox.Text.Trim();
        var qr = string.IsNullOrWhiteSpace(QrCodeBox.Text) ? null : QrCodeBox.Text.Trim();
        Status = "Salvando identificadores…";
        var result = await _api.UpdateProductIdentifiersAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            _selected.Id,
            new DesktopProdutoIdentificadorUpdateRequest(barcode, qr, _selected.RowVersion),
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Falha ao salvar: {result.Detail}";
            return;
        }

        var updated = result.Data;
        var index = Produtos.IndexOf(_selected);
        if (index >= 0) Produtos[index] = updated;
        _selected = updated;
        ProdutosGrid.SelectedItem = updated;
        Status = "Alteração confirmada pela API; versão atualizada.";
    }
}
