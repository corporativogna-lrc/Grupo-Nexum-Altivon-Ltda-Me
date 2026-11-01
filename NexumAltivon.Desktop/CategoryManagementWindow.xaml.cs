using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class CategoryManagementWindow : Window
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopCategoriaAdmin? _selected;
    private bool _busy;

    public CategoryManagementWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopCategoriaAdmin> Categorias { get; } = [];

    private async Task<bool> LoadAsync()
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            StatusText.Text = "Sessão administrativa ausente. Entre no Desktop para manter categorias.";
            return false;
        }

        SetBusy(true, "Consultando categorias e lojas reais na API…");
        try
        {
            var storesTask = _api.GetEmpresasGrupoLojasAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
            var categoriesTask = _api.GetAdminCategoriesAsync(_terminal, _terminal.DesktopAccessToken, BuscaBox.Text, _lifetime.Token);
            await Task.WhenAll(storesTask, categoriesTask);
            var stores = await storesTask;
            var categories = await categoriesTask;
            if (!stores.Success || stores.Data is null || !categories.Success || categories.Data is null)
            {
                StatusText.Text = $"Falha de leitura: {stores.Detail} {categories.Detail}".Trim();
                return false;
            }

            var previousStoreId = LojaCombo.SelectedValue is int id ? id : _selected?.LojaId;
            LojaCombo.ItemsSource = stores.Data.Where(store => store.LojaAtiva)
                .Select(store => new CategoryStoreOption(store.LojaId, store.NomeLoja, store.Segmento)).ToList();
            if (previousStoreId.HasValue && LojaCombo.Items.Cast<CategoryStoreOption>().Any(option => option.LojaId == previousStoreId.Value))
                LojaCombo.SelectedValue = previousStoreId.Value;
            else if (LojaCombo.Items.Count > 0)
                LojaCombo.SelectedIndex = 0;

            Categorias.Clear();
            foreach (var category in categories.Data) Categorias.Add(category);
            RefreshParents();
            StatusText.Text = $"{Categorias.Count} categoria(s) carregada(s) da API.";
            if (_selected is not null)
                CategoriasGrid.SelectedItem = Categorias.FirstOrDefault(item => item.Id == _selected.Id);
            return true;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested) { return false; }
        catch (Exception ex)
        {
            StatusText.Text = $"Falha ao consultar categorias: {ex.Message}";
            return false;
        }
        finally { SetBusy(false); }
    }

    private async void Atualizar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void Pesquisar_Click(object sender, RoutedEventArgs e) => await LoadAsync();
    private async void BuscaBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) await LoadAsync();
    }

    private void CategoriasGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selected = CategoriasGrid.SelectedItem as DesktopCategoriaAdmin;
        if (_selected is null) return;
        FormTitle.Text = $"Editar categoria · {_selected.LojaNome}";
        LojaCombo.SelectedValue = _selected.LojaId;
        SlugBox.Text = _selected.Slug;
        SlugBox.IsEnabled = false;
        NomeBox.Text = _selected.Nome;
        DescricaoBox.Text = _selected.Descricao ?? string.Empty;
        OrdemBox.Text = _selected.Ordem.ToString(CultureInfo.InvariantCulture);
        AtivaCheck.IsChecked = _selected.Ativa;
        RefreshParents(_selected.CategoriaPaiId);
        SalvarButton.Content = "Salvar alteração";
    }

    private void RefreshParents(int? selectedParent = null)
    {
        var storeId = LojaCombo.SelectedValue is int id ? id : 0;
        var roots = Categorias.Where(item => item.LojaId == storeId && item.Ativa
            && item.CategoriaPaiId is null && item.Id != _selected?.Id).ToList();
        PaiCombo.ItemsSource = roots;
        PaiCombo.SelectedValue = selectedParent;
    }

    private void LojaCombo_SelectionChanged(object sender, SelectionChangedEventArgs e) => RefreshParents();

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        _selected = null;
        CategoriasGrid.SelectedItem = null;
        FormTitle.Text = "Nova categoria";
        SlugBox.Clear(); SlugBox.IsEnabled = true;
        NomeBox.Clear(); DescricaoBox.Clear(); OrdemBox.Text = "0";
        AtivaCheck.IsChecked = true;
        SalvarButton.Content = "Criar categoria";
        RefreshParents();
        StatusText.Text = "Novo cadastro pronto. Escolha a loja e a categoria pai, se necessário.";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_busy) return;
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            StatusText.Text = "Sessão administrativa ausente.";
            return;
        }
        if (LojaCombo.SelectedValue is not int storeId || string.IsNullOrWhiteSpace(NomeBox.Text)
            || !int.TryParse(OrdemBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var order) || order < 0)
        {
            StatusText.Text = "Informe loja ativa, nome e ordem inteira não negativa.";
            return;
        }
        int? parentId = PaiCombo.SelectedValue is int parent ? parent : null;
        SetBusy(true, "Salvando categoria pela API…");
        try
        {
            DesktopApiDataResult<DesktopCategoriaAdmin> result;
            if (_selected is null)
            {
                result = await _api.CreateAdminCategoryAsync(_terminal, _terminal.DesktopAccessToken,
                    new DesktopCategoriaCreateRequest(storeId, SlugBox.Text.Trim(), NomeBox.Text.Trim(),
                        EmptyToNull(DescricaoBox.Text), parentId, order), _lifetime.Token);
            }
            else
            {
                result = await _api.UpdateAdminCategoryAsync(_terminal, _terminal.DesktopAccessToken, _selected.Id,
                    new DesktopCategoriaUpdateRequest(NomeBox.Text.Trim(), EmptyToNull(DescricaoBox.Text),
                        parentId, order, AtivaCheck.IsChecked == true, _selected.RowVersion), _lifetime.Token);
            }

            if (!result.Success || result.Data is null)
            {
                StatusText.Text = $"API recusou a gravação: {result.Detail}";
                return;
            }

            var savedId = result.Data.Id;
            SetBusy(false);
            if (!await LoadAsync()) return;
            var persisted = Categorias.FirstOrDefault(item => item.Id == savedId);
            if (persisted is null || persisted.Nome != NomeBox.Text.Trim()
                || persisted.CategoriaPaiId != parentId || persisted.Ordem != order
                || persisted.Ativa != (AtivaCheck.IsChecked == true))
            {
                StatusText.Text = "A API respondeu, mas a releitura não confirmou os valores persistidos.";
                return;
            }
            StatusText.Text = $"Categoria #{savedId} salva e confirmada após releitura.";
            _selected = persisted;
            CategoriasGrid.SelectedItem = persisted;
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
            StatusText.Text = "Operação cancelada ao fechar a janela.";
        }
        catch (Exception ex) { StatusText.Text = $"Falha ao salvar categoria: {ex.Message}"; }
        finally { SetBusy(false); }
    }

    private void SetBusy(bool busy, string? message = null)
    {
        _busy = busy;
        if (message is not null) StatusText.Text = message;
        SalvarButton.IsEnabled = !busy;
    }

    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed record CategoryStoreOption(int LojaId, string NomeLoja, string? Segmento);
}
