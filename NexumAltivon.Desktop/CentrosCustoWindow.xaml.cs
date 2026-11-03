/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7216
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class CentrosCustoWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopCentroCusto? _selectedCentro;
    private string _status = "Aguardando carregamento da API oficial.";
    private string _selectedDetails = "Nenhum centro de custo selecionado.";
    private bool _updatingForm;

    public CentrosCustoWindow(TerminalProfile terminal)
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

    public ObservableCollection<DesktopCentroCusto> Centros { get; } = [];

    public DesktopCentroCusto? SelectedCentro
    {
        get => _selectedCentro;
        private set
        {
            if (!SetField(ref _selectedCentro, value))
            {
                return;
            }

            if (value is null)
            {
                SelectedDetails = "Novo centro de custo. Nenhum dado foi gravado.";
                return;
            }

            SelectedDetails = $"ID: {value.Id}\nCódigo: {value.Codigo}\nNome: {value.Nome}\nTipo: {value.Tipo}\nStatus: {value.Status}\nPai: {value.PaiId?.ToString() ?? "sem pai"}";
            PreencherFormulario(value);
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
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de gerir centros de custo.";
            Centros.Clear();
            return false;
        }

        Status = "Consultando centros de custo da API oficial...";
        var result = await _api.GetCentrosCustoAsync(_terminal, _terminal.DesktopAccessToken, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Centros de custo não carregados: {result.Detail}";
            return false;
        }

        var selectedId = _selectedCentro?.Id;
        Centros.Clear();
        foreach (var centro in result.Data)
        {
            Centros.Add(centro);
        }

        SelectedCentro = selectedId.HasValue
            ? Centros.FirstOrDefault(item => item.Id == selectedId.Value)
            : null;
        Status = $"{Centros.Count} centro(s) de custo carregado(s) da API e do banco oficial.";
        return true;
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private void CentrosGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingForm)
        {
            SelectedCentro = CentrosGrid.SelectedItem as DesktopCentroCusto;
        }
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        CentrosGrid.SelectedItem = null;
        SelectedCentro = null;
        LimparFormulario();
        Status = "Novo centro de custo pronto para preenchimento. Nenhum dado foi gravado.";
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarFormulario(out var request))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Status = "Sessão JWT ausente. Nenhum dado foi enviado.";
            MessageBox.Show(Status, "Sessão necessária", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var selectedId = SelectedCentro?.Id;
        Status = selectedId.HasValue
            ? "Atualizando centro de custo na API oficial..."
            : "Cadastrando centro de custo na API oficial...";
        var result = selectedId.HasValue
            ? await _api.UpdateCentroCustoAsync(_terminal, _terminal.DesktopAccessToken, selectedId.Value, request, _lifetime.Token)
            : await _api.CreateCentroCustoAsync(_terminal, _terminal.DesktopAccessToken, request, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Centro de custo não persistido: {result.Detail}";
            MessageBox.Show(Status, "Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var persistedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura oficial falhou. A operação não foi apresentada como concluída.";
            return;
        }

        var persisted = Centros.FirstOrDefault(item => item.Id == persistedId);
        if (persisted is null)
        {
            Status = "A API respondeu, mas o centro não apareceu na releitura oficial. Persistência não confirmada.";
            MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        CentrosGrid.SelectedItem = persisted;
        SelectedCentro = persisted;
        Status = $"Centro de custo {persisted.Codigo} persistido e relido no banco oficial.";
    }

    private async void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedCentro is null)
        {
            Status = "Selecione um centro de custo válido antes de excluir.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            $"Excluir logicamente o centro {SelectedCentro.Codigo}? A API marcará ccu_data_exclusao e o registro deixará de ser listado.",
            "Confirmar exclusão lógica",
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

        var deletedId = SelectedCentro.Id;
        Status = "Executando exclusão lógica na API oficial...";
        var result = await _api.DeleteCentroCustoAsync(_terminal, _terminal.DesktopAccessToken, deletedId, _lifetime.Token);
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

        if (Centros.Any(item => item.Id == deletedId))
        {
            Status = "A API respondeu, mas o centro continua na releitura oficial. A exclusão não foi confirmada.";
            MessageBox.Show(Status, "Exclusão não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Novo_Click(sender, e);
        Status = "Centro de custo excluído logicamente e removido da releitura oficial.";
    }

    private bool ValidarFormulario(out DesktopCentroCustoRequest request)
    {
        request = default!;
        var codigo = CodigoBox.Text.Trim();
        var nome = NomeBox.Text.Trim();
        if (codigo.Length == 0 || nome.Length == 0)
        {
            Status = "Código e nome são obrigatórios.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        int? paiId = PaiBox.SelectedValue is int selectedPaiId ? selectedPaiId : null;
        int? responsavelId = null;
        if (!string.IsNullOrWhiteSpace(ResponsavelBox.Text))
        {
            if (!int.TryParse(ResponsavelBox.Text.Trim(), out var parsedResponsavelId) || parsedResponsavelId <= 0)
            {
                Status = "O responsável deve ser um ID inteiro positivo existente no cadastro de usuários.";
                MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            responsavelId = parsedResponsavelId;
        }

        request = new DesktopCentroCustoRequest(
            codigo,
            nome,
            NullIfWhiteSpace(DescricaoBox.Text),
            NullIfWhiteSpace(ObservacoesBox.Text),
            ReadComboValue(TipoBox) ?? "CUSTO",
            paiId,
            responsavelId,
            ReadComboValue(StatusBox) ?? "A");
        return true;
    }

    private void PreencherFormulario(DesktopCentroCusto centro)
    {
        _updatingForm = true;
        try
        {
            CodigoBox.Text = centro.Codigo;
            NomeBox.Text = centro.Nome;
            DescricaoBox.Text = centro.Descricao ?? string.Empty;
            ObservacoesBox.Text = string.Empty;
            TipoBox.SelectedItem = TipoBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Content?.ToString(), centro.Tipo, StringComparison.OrdinalIgnoreCase));
            StatusBox.SelectedItem = StatusBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(item => string.Equals(item.Content?.ToString(), centro.Status, StringComparison.OrdinalIgnoreCase));
            PaiBox.SelectedValue = centro.PaiId;
            ResponsavelBox.Text = centro.ResponsavelUsuarioId?.ToString() ?? string.Empty;
        }
        finally
        {
            _updatingForm = false;
        }
    }

    private void LimparFormulario()
    {
        _updatingForm = true;
        try
        {
            CodigoBox.Text = string.Empty;
            NomeBox.Text = string.Empty;
            DescricaoBox.Text = string.Empty;
            ObservacoesBox.Text = string.Empty;
            TipoBox.SelectedIndex = 0;
            StatusBox.SelectedIndex = 0;
            PaiBox.SelectedIndex = -1;
            ResponsavelBox.Text = string.Empty;
        }
        finally
        {
            _updatingForm = false;
        }
    }

    private static string? ReadComboValue(ComboBox comboBox) =>
        (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

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
