/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7209
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

public partial class OpsOrdensServicoWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopOpsOrdemServico? _selectedOrdem;
    private string _status = "Aguardando carregamento da API oficial.";
    private int? _selectedAtivoId;
    private bool _detailLoaded;

    public OpsOrdensServicoWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        Closed += (_, _) => _lifetime.Cancel();
    }

    public ObservableCollection<DesktopOpsOrdemServico> Ordens { get; } = new();

    public ObservableCollection<DesktopOpsAtivo> Ativos { get; } = new();

    public ObservableCollection<OpsItemInput> Itens { get; } = new();

    public int? SelectedAtivoId
    {
        get => _selectedAtivoId;
        set => SetField(ref _selectedAtivoId, value);
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string SelectedDetails => _selectedOrdem is null
        ? "Nenhuma ordem selecionada."
        : $"OS #{_selectedOrdem.Id} | {_selectedOrdem.Numero} | criada em {_selectedOrdem.CriadoEm:dd/MM/yyyy HH:mm} | atualizada em {_selectedOrdem.AtualizadoEm:dd/MM/yyyy HH:mm}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void Carregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private async void StatusFiltro_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded)
        {
            await CarregarAsync();
        }
    }

    private async void OrdensGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (OrdensGrid.SelectedItem is not DesktopOpsOrdemServico ordem)
        {
            return;
        }

        await CarregarDetalheAsync(ordem.Id);
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        _selectedOrdem = null;
        _detailLoaded = false;
        NumeroBox.Clear();
        SelectedAtivoId = null;
        AtivoBox.SelectedItem = null;
        TituloBox.Clear();
        DescricaoBox.Clear();
        SelectComboItem(StatusBox, "ABERTA");
        SelectComboItem(PrioridadeBox, "NORMAL");
        DataAberturaPicker.SelectedDate = DateTime.Today;
        DataPrevistaPicker.SelectedDate = null;
        DataConclusaoPicker.SelectedDate = null;
        TempoEstimadoBox.Clear();
        TempoRealBox.Clear();
        ResponsavelBox.Clear();
        CustoPrevistoBox.Clear();
        CustoRealBox.Clear();
        ObservacoesBox.Clear();
        Itens.Clear();
        OrdensGrid.SelectedItem = null;
        Status = "Nova ordem pronta. Nenhuma alteração foi gravada.";
        NotifyContext();
    }

    private void AdicionarItem_Click(object sender, RoutedEventArgs e)
    {
        Itens.Add(new OpsItemInput());
        ItensGrid.SelectedIndex = Itens.Count - 1;
        ItensGrid.ScrollIntoView(ItensGrid.SelectedItem);
    }

    private void RemoverItem_Click(object sender, RoutedEventArgs e)
    {
        if (ItensGrid.SelectedItem is OpsItemInput item)
        {
            Itens.Remove(item);
            Status = "Item removido da edição local. Confirme em Salvar ordem para persistir a composição.";
        }
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedOrdem is not null && !_detailLoaded)
        {
            Status = "O detalhe da ordem não foi carregado; nenhum dado será alterado.";
            return;
        }

        if (string.IsNullOrWhiteSpace(TituloBox.Text))
        {
            Status = "O título da ordem é obrigatório.";
            return;
        }

        if (!TryReadNullableInt(ResponsavelBox.Text, "responsável", out var responsavelUserId)
            || !TryReadNullableInt(TempoEstimadoBox.Text, "tempo estimado", out var tempoEstimado)
            || !TryReadNullableInt(TempoRealBox.Text, "tempo real", out var tempoReal)
            || !TryReadNullableDecimal(CustoPrevistoBox.Text, "custo previsto", out var custoPrevisto)
            || !TryReadNullableDecimal(CustoRealBox.Text, "custo real", out var custoReal))
        {
            return;
        }

        if (!TryBuildItems(out var itemRequests))
        {
            return;
        }

        var request = new DesktopOpsOrdemServicoRequest(
            NullIfWhiteSpace(NumeroBox.Text),
            SelectedAtivoId,
            TituloBox.Text.Trim(),
            NullIfWhiteSpace(DescricaoBox.Text),
            SelectedComboValue(StatusBox) ?? "ABERTA",
            SelectedComboValue(PrioridadeBox) ?? "NORMAL",
            responsavelUserId,
            DataAberturaPicker.SelectedDate?.ToUniversalTime(),
            DataPrevistaPicker.SelectedDate?.ToUniversalTime(),
            DataConclusaoPicker.SelectedDate?.ToUniversalTime(),
            tempoEstimado,
            tempoReal,
            custoPrevisto,
            custoReal,
            NullIfWhiteSpace(ObservacoesBox.Text),
            itemRequests);

        var operation = _selectedOrdem is null ? "Criando" : "Atualizando";
        Status = $"{operation} ordem na API oficial...";
        var result = await _api.SaveOpsOrdemServicoAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            _selectedOrdem?.Id,
            request,
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Ordem não persistida: {result.Detail}";
            return;
        }

        var persistedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura das ordens falhou. A operação não será tratada como concluída.";
            return;
        }

        var persisted = Ordens.FirstOrDefault(item => item.Id == persistedId);
        OrdensGrid.SelectedItem = persisted;
        if (persisted is null || !await CarregarDetalheAsync(persistedId))
        {
            Status = "A ordem foi respondida pela API, mas não pôde ser relida com os itens persistidos.";
            return;
        }

        Status = $"Ordem #{persistedId} persistida e relida do banco oficial com {Itens.Count} item(ns).";
    }

    private async void Excluir_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedOrdem is null)
        {
            Status = "Selecione uma ordem antes de excluir.";
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            $"Excluir a ordem {_selectedOrdem.Numero} por soft-delete?",
            "Confirmar exclusão",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            return;
        }

        var id = _selectedOrdem.Id;
        Status = "Aplicando soft-delete na API oficial...";
        var result = await _api.DeleteOpsOrdemServicoAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
        if (!result.Success)
        {
            Status = $"Ordem não excluída: {result.Detail}";
            return;
        }

        if (!await CarregarAsync())
        {
            Status = "A API confirmou a exclusão, mas a releitura falhou. O resultado não será tratado como concluído.";
            return;
        }

        Novo_Click(sender, e);
        Status = $"Ordem #{id} excluída por soft-delete e ausente na releitura.";
    }

    private async Task<bool> CarregarAsync()
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Ordens.Clear();
            Ativos.Clear();
            Status = "Sessão JWT ausente. Autentique o terminal antes de gerir ordens.";
            return false;
        }

        var selectedStatus = SelectedComboValue(StatusFiltroBox);
        if (string.Equals(selectedStatus, "Todos", StringComparison.OrdinalIgnoreCase))
        {
            selectedStatus = null;
        }

        Status = "Consultando ordens e ativos na API oficial...";
        var ordensTask = _api.GetOpsOrdensServicoAsync(_terminal, _terminal.DesktopAccessToken, selectedStatus, _lifetime.Token);
        var ativosTask = _api.GetOpsAtivosAsync(_terminal, _terminal.DesktopAccessToken, null, _lifetime.Token);
        await Task.WhenAll(ordensTask, ativosTask);
        var ordensResult = await ordensTask;
        var ativosResult = await ativosTask;
        if (!ordensResult.Success || ordensResult.Data is null)
        {
            Status = $"Ordens não carregadas: {ordensResult.Detail}";
            return false;
        }

        if (!ativosResult.Success || ativosResult.Data is null)
        {
            Status = $"Ativos vinculáveis não carregados: {ativosResult.Detail}";
            return false;
        }

        Ordens.Clear();
        foreach (var ordem in ordensResult.Data)
        {
            Ordens.Add(ordem);
        }

        Ativos.Clear();
        foreach (var ativo in ativosResult.Data)
        {
            Ativos.Add(ativo);
        }

        Status = $"{Ordens.Count} ordem(ns) e {Ativos.Count} ativo(s) carregado(s) da API oficial.";
        return true;
    }

    private async Task<bool> CarregarDetalheAsync(int id)
    {
        _detailLoaded = false;
        Status = $"Consultando detalhe da ordem #{id} na API oficial...";
        var result = await _api.GetOpsOrdemServicoAsync(_terminal, _terminal.DesktopAccessToken, id, _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Detalhe da ordem não carregado: {result.Detail}";
            return false;
        }

        var ordem = result.Data.Ordem;
        _selectedOrdem = ordem;
        NumeroBox.Text = ordem.Numero;
        SelectedAtivoId = ordem.AtivoId;
        AtivoBox.SelectedValue = ordem.AtivoId;
        TituloBox.Text = ordem.Titulo;
        DescricaoBox.Text = ordem.Descricao ?? string.Empty;
        SelectComboItem(StatusBox, ordem.Status);
        SelectComboItem(PrioridadeBox, ordem.Prioridade);
        DataAberturaPicker.SelectedDate = ordem.DataAbertura.ToLocalTime().Date;
        DataPrevistaPicker.SelectedDate = ordem.DataPrevista?.ToLocalTime().Date;
        DataConclusaoPicker.SelectedDate = ordem.DataConclusao?.ToLocalTime().Date;
        TempoEstimadoBox.Text = ordem.TempoEstimadoMinutos?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        TempoRealBox.Text = ordem.TempoRealMinutos?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        ResponsavelBox.Text = ordem.ResponsavelUserId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
        CustoPrevistoBox.Text = ordem.CustoPrevisto?.ToString("N2", CultureInfo.CurrentCulture) ?? string.Empty;
        CustoRealBox.Text = ordem.CustoReal?.ToString("N2", CultureInfo.CurrentCulture) ?? string.Empty;
        ObservacoesBox.Text = ordem.Observacoes ?? string.Empty;

        Itens.Clear();
        foreach (var item in result.Data.Itens)
        {
            Itens.Add(new OpsItemInput
            {
                Tipo = item.Tipo,
                Codigo = item.Codigo ?? string.Empty,
                Descricao = item.Descricao,
                Quantidade = item.Quantidade.ToString(CultureInfo.InvariantCulture),
                Unidade = item.Unidade,
                CustoUnitario = item.CustoUnitario?.ToString(CultureInfo.InvariantCulture) ?? string.Empty
            });
        }

        _detailLoaded = true;
        NotifyContext();
        Status = $"Ordem #{id} carregada com {Itens.Count} item(ns) persistido(s).";
        return true;
    }

    private bool TryBuildItems(out List<DesktopOpsOrdemServicoItemRequest> items)
    {
        items = new List<DesktopOpsOrdemServicoItemRequest>();
        foreach (var item in Itens)
        {
            if (string.IsNullOrWhiteSpace(item.Descricao))
            {
                Status = "A descrição de cada item é obrigatória.";
                return false;
            }

            if (!TryReadDecimal(item.Quantidade, "quantidade do item", out var quantidade) || quantidade <= 0)
            {
                if (quantidade <= 0)
                {
                    Status = "A quantidade de cada item deve ser maior que zero.";
                }

                return false;
            }

            if (!TryReadNullableDecimal(item.CustoUnitario, "custo unitário do item", out var custoUnitario))
            {
                return false;
            }

            items.Add(new DesktopOpsOrdemServicoItemRequest(
                NullIfWhiteSpace(item.Tipo) ?? "SERVICO",
                NullIfWhiteSpace(item.Codigo),
                item.Descricao.Trim(),
                quantidade,
                NullIfWhiteSpace(item.Unidade) ?? "UN",
                custoUnitario));
        }

        return true;
    }

    private bool TryReadNullableInt(string text, string field, out int? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (int.TryParse(text.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed))
        {
            value = parsed;
            return true;
        }

        Status = $"O campo {field} deve conter um número inteiro válido.";
        return false;
    }

    private bool TryReadNullableDecimal(string text, string field, out decimal? value)
    {
        value = null;
        if (string.IsNullOrWhiteSpace(text))
        {
            return true;
        }

        if (TryReadDecimal(text, field, out var parsed))
        {
            value = parsed;
            return true;
        }

        return false;
    }

    private bool TryReadDecimal(string text, string field, out decimal value)
    {
        if (decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out value)
            || decimal.TryParse(text.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out value))
        {
            return true;
        }

        Status = $"O campo {field} deve conter um número decimal válido.";
        return false;
    }

    private void NotifyContext()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedDetails)));
    }

    private static string? NullIfWhiteSpace(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? SelectedComboValue(ComboBox comboBox) =>
        (comboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

    private static void SelectComboItem(ComboBox comboBox, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            comboBox.SelectedIndex = -1;
            return;
        }

        comboBox.SelectedItem = comboBox.Items.OfType<ComboBoxItem>()
            .FirstOrDefault(item => string.Equals(item.Content?.ToString(), value, StringComparison.OrdinalIgnoreCase));
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

    public sealed class OpsItemInput : INotifyPropertyChanged
    {
        private string _tipo = "SERVICO";
        private string _codigo = string.Empty;
        private string _descricao = string.Empty;
        private string _quantidade = "1";
        private string _unidade = "UN";
        private string _custoUnitario = string.Empty;

        public string Tipo { get => _tipo; set => SetField(ref _tipo, value); }
        public string Codigo { get => _codigo; set => SetField(ref _codigo, value); }
        public string Descricao { get => _descricao; set => SetField(ref _descricao, value); }
        public string Quantidade { get => _quantidade; set => SetField(ref _quantidade, value); }
        public string Unidade { get => _unidade; set => SetField(ref _unidade, value); }
        public string CustoUnitario { get => _custoUnitario; set => SetField(ref _custoUnitario, value); }

        public event PropertyChangedEventHandler? PropertyChanged;

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
}
