/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7217
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

public partial class ItensServicoWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopItemServico? _selectedItem;
    private string _status = "Aguardando carregamento da API oficial.";
    private string _selectedDetails = "Nenhum item ou serviço selecionado.";
    private bool _updatingForm;

    public ItensServicoWindow(TerminalProfile terminal)
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

    public ObservableCollection<DesktopItemServico> Itens { get; } = [];

    public DesktopItemServico? SelectedItem
    {
        get => _selectedItem;
        private set
        {
            if (!SetField(ref _selectedItem, value))
            {
                return;
            }

            if (value is null)
            {
                SelectedDetails = "Novo item ou serviço. Nenhum dado foi gravado.";
                return;
            }

            SelectedDetails = $"ID: {value.Id}\nCódigo: {value.Codigo}\nDescrição: {value.Descricao}\nTipo: {value.Tipo}\nUnidade: {value.Unidade}\nEstado: {(value.Ativo ? "ativo" : "inativo")}";
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
            Status = "Sessão JWT ausente. Autentique o terminal em Configurações antes de gerir produtos e serviços.";
            Itens.Clear();
            return false;
        }

        Status = "Consultando itens e serviços da API oficial...";
        var result = await _api.GetItensServicoAsync(_terminal, _terminal.DesktopAccessToken, FiltroBox.Text, null, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Itens e serviços não carregados: {result.Detail}";
            return false;
        }

        var selectedId = _selectedItem?.Id;
        Itens.Clear();
        foreach (var item in result.Data)
        {
            Itens.Add(item);
        }

        SelectedItem = selectedId.HasValue
            ? Itens.FirstOrDefault(item => item.Id == selectedId.Value)
            : null;
        Status = $"{Itens.Count} item(ns)/serviço(s) carregado(s) da API e do banco oficial.";
        return true;
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private async void Pesquisar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarAsync();
    }

    private void ItensGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_updatingForm)
        {
            SelectedItem = ItensGrid.SelectedItem as DesktopItemServico;
        }
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        ItensGrid.SelectedItem = null;
        SelectedItem = null;
        LimparFormulario();
        Status = "Novo item ou serviço pronto para preenchimento. Nenhum dado foi gravado.";
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

        var selectedId = SelectedItem?.Id;
        Status = selectedId.HasValue
            ? "Atualizando item/serviço na API oficial..."
            : "Cadastrando item/serviço na API oficial...";
        var result = await _api.SaveItemServicoAsync(_terminal, _terminal.DesktopAccessToken, selectedId, request, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Item/serviço não persistido: {result.Detail}";
            MessageBox.Show(Status, "Falha ao salvar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var persistedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura oficial falhou. A operação não foi apresentada como concluída.";
            return;
        }

        var persisted = Itens.FirstOrDefault(item => item.Id == persistedId);
        if (persisted is null)
        {
            Status = "A API respondeu, mas o item não apareceu na releitura oficial. Persistência não confirmada.";
            MessageBox.Show(Status, "Persistência não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        ItensGrid.SelectedItem = persisted;
        SelectedItem = persisted;
        Status = $"Item/serviço {persisted.Codigo} persistido e relido no banco oficial.";
    }

    private async void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedItem is null)
        {
            Status = "Selecione um item ou serviço válido antes de desativar.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var confirmation = MessageBox.Show(
            $"Desativar o item {SelectedItem.Codigo}? A API marcará itm_ativo = 0 e o registro deixará de ser listado.",
            "Confirmar desativação",
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

        var deletedId = SelectedItem.Id;
        Status = "Executando desativação na API oficial...";
        var result = await _api.DeleteItemServicoAsync(_terminal, _terminal.DesktopAccessToken, deletedId, _lifetime.Token);
        if (!result.Success)
        {
            Status = $"Desativação não confirmada: {result.Detail}";
            MessageBox.Show(Status, "Falha ao desativar", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura oficial falhou. A desativação não foi apresentada como concluída.";
            return;
        }

        if (Itens.Any(item => item.Id == deletedId))
        {
            Status = "A API respondeu, mas o item continua na releitura oficial. A desativação não foi confirmada.";
            MessageBox.Show(Status, "Desativação não confirmada", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        Novo_Click(sender, e);
        Status = "Item/serviço desativado e removido da releitura oficial.";
    }

    private bool ValidarFormulario(out DesktopItemServicoRequest request)
    {
        request = default!;
        if (!int.TryParse(EmpresaIdBox.Text.Trim(), out var empresaId) || empresaId <= 0)
        {
            Status = "Empresa (ID) deve ser um número inteiro positivo existente no cadastro oficial.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        var codigo = CodigoBox.Text.Trim();
        var tipo = ReadComboValue(TipoBox);
        var descricao = DescricaoBox.Text.Trim();
        if (codigo.Length == 0 || tipo is null || descricao.Length == 0)
        {
            Status = "Código, tipo e descrição são obrigatórios.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (!TryDecimal(PesoBrutoBox.Text, out var pesoBruto) ||
            !TryDecimal(PesoLiquidoBox.Text, out var pesoLiquido) ||
            !TryDecimal(AlturaBox.Text, out var altura) ||
            !TryDecimal(LarguraBox.Text, out var largura) ||
            !TryDecimal(ProfundidadeBox.Text, out var profundidade))
        {
            Status = "Peso e dimensões devem estar vazios ou conter números válidos.";
            MessageBox.Show(Status, "Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        request = new DesktopItemServicoRequest(
            empresaId,
            codigo,
            tipo,
            descricao,
            NullIfWhiteSpace(DescricaoDetalhadaBox.Text),
            NullIfWhiteSpace(UnidadeBox.Text) ?? "UN",
            NullIfWhiteSpace(NcmBox.Text),
            NullIfWhiteSpace(CestBox.Text),
            pesoBruto,
            pesoLiquido,
            altura,
            largura,
            profundidade,
            ControlaEstoqueBox.IsChecked == true,
            ControlaLoteBox.IsChecked == true,
            ControlaSerieBox.IsChecked == true,
            AtivoBox.IsChecked == true);
        return true;
    }

    private void PreencherFormulario(DesktopItemServico item)
    {
        _updatingForm = true;
        try
        {
            EmpresaIdBox.Text = item.EmpresaId.ToString(CultureInfo.InvariantCulture);
            CodigoBox.Text = item.Codigo;
            TipoBox.SelectedItem = TipoBox.Items
                .OfType<ComboBoxItem>()
                .FirstOrDefault(option => string.Equals(option.Content?.ToString(), item.Tipo, StringComparison.OrdinalIgnoreCase));
            DescricaoBox.Text = item.Descricao;
            DescricaoDetalhadaBox.Text = item.DescricaoDetalhada ?? string.Empty;
            UnidadeBox.Text = item.Unidade;
            NcmBox.Text = item.Ncm ?? string.Empty;
            CestBox.Text = item.Cest ?? string.Empty;
            ControlaEstoqueBox.IsChecked = item.ControlaEstoque;
            ControlaLoteBox.IsChecked = item.ControlaLote;
            ControlaSerieBox.IsChecked = item.ControlaSerie;
            AtivoBox.IsChecked = item.Ativo;
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
            EmpresaIdBox.Text = "1";
            CodigoBox.Text = string.Empty;
            TipoBox.SelectedIndex = 0;
            DescricaoBox.Text = string.Empty;
            DescricaoDetalhadaBox.Text = string.Empty;
            UnidadeBox.Text = "UN";
            NcmBox.Text = string.Empty;
            CestBox.Text = string.Empty;
            PesoBrutoBox.Text = string.Empty;
            PesoLiquidoBox.Text = string.Empty;
            AlturaBox.Text = string.Empty;
            LarguraBox.Text = string.Empty;
            ProfundidadeBox.Text = string.Empty;
            ControlaEstoqueBox.IsChecked = false;
            ControlaLoteBox.IsChecked = false;
            ControlaSerieBox.IsChecked = false;
            AtivoBox.IsChecked = true;
        }
        finally
        {
            _updatingForm = false;
        }
    }

    private static bool TryDecimal(string value, out decimal? result)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            result = null;
            return true;
        }

        if (decimal.TryParse(value.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out var parsed) && parsed >= 0)
        {
            result = parsed;
            return true;
        }

        result = null;
        return false;
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
