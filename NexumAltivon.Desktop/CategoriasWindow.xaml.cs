/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7214
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class CategoriasWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopCategoria? _selectedCategoria;
    private string _status = "Aguardando carregamento da API oficial.";
    private string _selectedDetails = "Nenhuma categoria selecionada.";

    public CategoriasWindow(TerminalProfile terminal)
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

    public ObservableCollection<DesktopCategoria> Categorias { get; } = [];

    public DesktopCategoria? SelectedCategoria
    {
        get => _selectedCategoria;
        private set
        {
            if (!SetField(ref _selectedCategoria, value))
            {
                return;
            }

            SelectedDetails = value is null
                ? "Nova categoria. Nenhum dado foi gravado."
                : $"Slug: {value.Id}\nNome: {value.Nome}\nCaminho: {value.Caminho ?? value.Nome}\nEstado: {(value.Ativa ? "ativa" : "inativa")}";
        }
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string SelectedDetails
    {
        get => _selectedDetails;
        private set => SetField(ref _selectedDetails, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task<bool> CarregarAsync()
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de gerir categorias.";
            Categorias.Clear();
            return false;
        }

        Status = "Consultando categorias da API oficial...";
        var result = await _api.GetCategoriasAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Categorias não carregadas: {result.Detail}";
            return false;
        }

        var selectedId = _selectedCategoria?.Id;
        Categorias.Clear();
        foreach (var categoria in result.Data)
        {
            Categorias.Add(categoria);
        }

        SelectedCategoria = selectedId is null
            ? null
            : Categorias.FirstOrDefault(item => string.Equals(item.Id, selectedId, StringComparison.OrdinalIgnoreCase));
        Status = $"{Categorias.Count} categoria(s) carregada(s) da API e do banco oficial.";
        return true;
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private void CategoriasGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        SelectedCategoria = CategoriasGrid.SelectedItem as DesktopCategoria;
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        CategoriasGrid.SelectedItem = null;
        SelectedCategoria = null;
        CodigoBox.Text = string.Empty;
        NomeBox.Text = string.Empty;
        DescricaoBox.Text = string.Empty;
        CategoriaPaiBox.SelectedIndex = -1;
        OrdemBox.Text = "0";
        Status = "Nova categoria pronta para preenchimento. Nenhum dado foi gravado.";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        var codigo = CodigoBox.Text.Trim();
        var nome = NomeBox.Text.Trim();
        if (codigo.Length == 0 && nome.Length == 0)
        {
            Status = "Informe o código/slug ou o nome da categoria.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (!int.TryParse(OrdemBox.Text.Trim(), out var ordem) || ordem < 0)
        {
            Status = "A ordem deve ser um número inteiro maior ou igual a zero.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhum dado foi enviado.";
            MessageBox.Show(Status, "Sessão necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var parentSlug = CategoriaPaiBox.SelectedValue as string;
        var request = new DesktopCategoriaRequest(
            codigo,
            nome,
            DescricaoBox.Text.Trim(),
            parentSlug,
            string.IsNullOrWhiteSpace(parentSlug) ? 1 : 2,
            null,
            ordem,
            true);
        Status = "Cadastrando categoria na API oficial...";
        var result = await _api.CreateCategoriaAsync(_terminal, _terminal.DesktopAccessToken, request, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Categoria não persistida: {result.Detail}";
            MessageBox.Show(Status, "Falha ao cadastrar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura oficial falhou. A operação não foi apresentada como concluída.";
            return;
        }

        var persisted = Categorias.FirstOrDefault(item => string.Equals(item.Id, result.Data.Id, StringComparison.OrdinalIgnoreCase));
        if (persisted is null)
        {
            Status = "A API respondeu, mas a categoria não apareceu na releitura oficial. Persistência não confirmada.";
            MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CategoriasGrid.SelectedItem = persisted;
        SelectedCategoria = persisted;
        Status = $"Categoria {persisted.Id} persistida e relida no banco oficial.";
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
