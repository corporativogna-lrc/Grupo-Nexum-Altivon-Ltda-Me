/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7210
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class TenantManagementWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopTenant? _selectedTenant;
    private string _status = "Aguardando carregamento da API oficial.";
    private string _selectedDetails = "Nenhum tenant selecionado.";
    private bool _updatingForm;

    public TenantManagementWindow(TerminalProfile terminal)
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

    public ObservableCollection<DesktopTenant> Tenants { get; } = [];

    public DesktopTenant? SelectedTenant
    {
        get => _selectedTenant;
        private set
        {
            if (!SetField(ref _selectedTenant, value))
            {
                return;
            }

            _updatingForm = true;
            if (value is null)
            {
                CodigoBox.Text = string.Empty;
                NomeBox.Text = string.Empty;
                DocumentoBox.Text = string.Empty;
                AtivoBox.IsChecked = true;
                SelectedDetails = "Novo tenant. Nenhum dado foi gravado.";
            }
            else
            {
                CodigoBox.Text = value.Codigo;
                NomeBox.Text = value.Nome;
                DocumentoBox.Text = value.Documento ?? string.Empty;
                AtivoBox.IsChecked = value.Ativo;
                SelectedDetails = $"GUID: {value.Id}\nCriado: {value.CriadoEm:dd/MM/yyyy HH:mm:ss} UTC\nAtualizado: {value.AtualizadoEm:dd/MM/yyyy HH:mm:ss} UTC";
            }

            _updatingForm = false;
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
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de gerir tenants.";
            Tenants.Clear();
            return false;
        }

        Status = "Consultando tenants da API oficial...";
        var result = await _api.GetTenantsAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Tenants não carregados: {result.Detail}";
            return false;
        }

        var selectedId = _selectedTenant?.Id;
        Tenants.Clear();
        foreach (var tenant in result.Data)
        {
            Tenants.Add(tenant);
        }

        SelectedTenant = selectedId is null
            ? null
            : Tenants.FirstOrDefault(item => item.Id == selectedId.Value);
        Status = $"{Tenants.Count} tenant(s) carregado(s) da API e do banco oficial.";
        return true;
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private void TenantsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingForm)
        {
            SelectedTenant = TenantsGrid.SelectedItem as DesktopTenant;
        }
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        TenantsGrid.SelectedItem = null;
        SelectedTenant = null;
        Status = "Novo tenant pronto para preenchimento. Nenhum dado foi gravado.";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        var codigo = CodigoBox.Text.Trim();
        var nome = NomeBox.Text.Trim();
        var documento = NullIfWhiteSpace(DocumentoBox.Text);
        if (codigo.Length == 0 || nome.Length == 0)
        {
            Status = "Código e nome são obrigatórios.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Guid? id = null;
        if (SelectedTenant is not null)
        {
            id = SelectedTenant.Id;
        }

        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhum dado foi enviado.";
            MessageBox.Show(Status, "Sessão necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var request = new DesktopTenantRequest(codigo, nome, documento, AtivoBox.IsChecked == true);
        Status = id.HasValue ? "Atualizando tenant na API oficial..." : "Criando tenant na API oficial...";
        var result = await _api.SaveTenantAsync(_terminal, _terminal.DesktopAccessToken, id, request, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Operação não confirmada: {result.Detail}";
            MessageBox.Show(Status, "Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var persistedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura oficial falhou. O resultado não foi apresentado como concluído.";
            return;
        }

        var persisted = Tenants.FirstOrDefault(item => item.Id == persistedId);
        if (persisted is null)
        {
            Status = "A API respondeu, mas o tenant não apareceu na releitura oficial. A operação requer investigação.";
            MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        TenantsGrid.SelectedItem = persisted;
        SelectedTenant = persisted;
        Status = $"Tenant {persisted.Codigo} persistido e relido no banco oficial.";
    }

    private async void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedTenant is null)
        {
            Status = "Selecione um tenant válido antes de excluir.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var id = SelectedTenant.Id;

        var confirmation = MessageBox.Show(
            $"Excluir o tenant {SelectedTenant.Codigo}? A API executará exclusão lógica e o registro deixará de ser listado.",
            "Confirmar exclusão",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhum dado foi enviado.";
            MessageBox.Show(Status, "Sessão necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Status = "Executando exclusão lógica na API oficial...";
        var result = await _api.DeleteTenantAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
        if (!result.Success)
        {
            Status = $"Exclusão não confirmada: {result.Detail}";
            MessageBox.Show(Status, "Falha ao excluir", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura oficial falhou. A exclusão não foi apresentada como concluída.";
            return;
        }

        if (Tenants.Any(item => item.Id == id))
        {
            Status = "A API respondeu, mas o tenant continua na releitura oficial. A exclusão não foi confirmada.";
            MessageBox.Show(Status, "Exclusão não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Novo_Click(sender, e);
        Status = "Tenant excluído logicamente e removido da releitura oficial.";
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

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
