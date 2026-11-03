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

public partial class OpsProducaoWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private DesktopOpsProducaoApontamento? _selectedApontamento;
    private string _status = "Aguardando carregamento da API oficial.";

    public OpsProducaoWindow(TerminalProfile terminal)
    {
        _terminal = terminal ?? throw new ArgumentNullException(nameof(terminal));
        InitializeComponent();
        DataContext = this;
        Closed += (_, _) => _lifetime.Cancel();
    }

    public ObservableCollection<DesktopOpsProducaoApontamento> Apontamentos { get; } = new();

    public ObservableCollection<OpsInsumoInput> Insumos { get; } = new();

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public string SelectedDetails => _selectedApontamento is null
        ? "Nenhum apontamento selecionado."
        : $"Apontamento #{_selectedApontamento.Id} | registrado em {_selectedApontamento.DataApontamento:dd/MM/yyyy HH:mm} | operador {_selectedApontamento.OperadorUserId?.ToString() ?? "não informado"}";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        InicioPicker.SelectedDate = DateTime.Today.AddDays(-30);
        FimPicker.SelectedDate = DateTime.Today;
        DataApontamentoPicker.SelectedDate = DateTime.Today;
        await CarregarAsync();
    }

    private async void Carregar_Click(object sender, RoutedEventArgs e) => await CarregarAsync();

    private void ApontamentosGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedApontamento = ApontamentosGrid.SelectedItem as DesktopOpsProducaoApontamento;
        if (_selectedApontamento is not null)
        {
            OrdemServicoBox.Text = _selectedApontamento.OrdemServicoId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            ProdutoIdBox.Text = _selectedApontamento.ProdutoId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            ProdutoCodigoBox.Text = _selectedApontamento.ProdutoCodigo ?? string.Empty;
            ProdutoNomeBox.Text = _selectedApontamento.ProdutoNome ?? string.Empty;
            QuantidadeProduzidaBox.Text = _selectedApontamento.QuantidadeProduzida.ToString(CultureInfo.InvariantCulture);
            QuantidadeRefugoBox.Text = _selectedApontamento.QuantidadeRefugo.ToString(CultureInfo.InvariantCulture);
            TempoMinutosBox.Text = _selectedApontamento.TempoMinutos?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            OperadorUserIdBox.Text = _selectedApontamento.OperadorUserId?.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
            DataApontamentoPicker.SelectedDate = _selectedApontamento.DataApontamento.ToLocalTime().Date;
            ObservacoesBox.Text = _selectedApontamento.Observacoes ?? string.Empty;
            CarregarInsumosPersistidos(_selectedApontamento.InsumosJson);
        }

        NotifyContext();
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        _selectedApontamento = null;
        OrdemServicoBox.Clear();
        ProdutoIdBox.Clear();
        ProdutoCodigoBox.Clear();
        ProdutoNomeBox.Clear();
        QuantidadeProduzidaBox.Text = "0";
        QuantidadeRefugoBox.Text = "0";
        TempoMinutosBox.Clear();
        OperadorUserIdBox.Clear();
        DataApontamentoPicker.SelectedDate = DateTime.Today;
        ObservacoesBox.Clear();
        Insumos.Clear();
        ApontamentosGrid.SelectedItem = null;
        Status = "Novo apontamento pronto. Nenhuma alteração foi gravada.";
        NotifyContext();
    }

    private void AdicionarInsumo_Click(object sender, RoutedEventArgs e)
    {
        Insumos.Add(new OpsInsumoInput());
        InsumosGrid.SelectedIndex = Insumos.Count - 1;
        InsumosGrid.ScrollIntoView(InsumosGrid.SelectedItem);
    }

    private void RemoverInsumo_Click(object sender, RoutedEventArgs e)
    {
        if (InsumosGrid.SelectedItem is OpsInsumoInput insumo)
        {
            Insumos.Remove(insumo);
            Status = "Insumo removido da edição local. Confirme em Registrar apontamento para persistir a composição.";
        }
    }

    private async void Registrar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ProdutoNomeBox.Text))
        {
            Status = "O nome do produto é obrigatório.";
            return;
        }

        if (!TryReadNullableInt(OrdemServicoBox.Text, "ordem de serviço", out var ordemServicoId)
            || !TryReadNullableInt(ProdutoIdBox.Text, "produto", out var produtoId)
            || !TryReadNullableInt(TempoMinutosBox.Text, "tempo", out var tempoMinutos)
            || !TryReadNullableInt(OperadorUserIdBox.Text, "operador", out var operadorUserId)
            || !TryReadDecimal(QuantidadeProduzidaBox.Text, "quantidade produzida", out var quantidadeProduzida)
            || !TryReadDecimal(QuantidadeRefugoBox.Text, "quantidade de refugo", out var quantidadeRefugo))
        {
            return;
        }

        if (quantidadeProduzida <= 0 || quantidadeRefugo < 0)
        {
            Status = "A quantidade produzida deve ser maior que zero e o refugo não pode ser negativo.";
            return;
        }

        if (!TryBuildInsumos(out var insumos))
        {
            return;
        }

        var request = new DesktopOpsProducaoApontamentoRequest(
            ordemServicoId,
            produtoId,
            NullIfWhiteSpace(ProdutoCodigoBox.Text),
            ProdutoNomeBox.Text.Trim(),
            quantidadeProduzida,
            quantidadeRefugo,
            tempoMinutos,
            operadorUserId,
            DataApontamentoPicker.SelectedDate?.ToUniversalTime(),
            insumos,
            NullIfWhiteSpace(ObservacoesBox.Text));

        Status = "Registrando apontamento na API oficial...";
        var result = await _api.CreateOpsProducaoApontamentoAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            request,
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Apontamento não persistido: {result.Detail}";
            return;
        }

        var persistedId = result.Data.Id;
        if (!await CarregarAsync())
        {
            Status = "A API respondeu, mas a releitura dos apontamentos falhou. A operação não será tratada como concluída.";
            return;
        }

        _selectedApontamento = Apontamentos.FirstOrDefault(item => item.Id == persistedId);
        ApontamentosGrid.SelectedItem = _selectedApontamento;
        Status = _selectedApontamento is null
            ? $"Apontamento #{persistedId} foi respondido, mas não pôde ser relido na consulta filtrada."
            : $"Apontamento #{persistedId} persistido e relido do banco oficial.";
        NotifyContext();
    }

    private async Task<bool> CarregarAsync()
    {
        if (string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            Apontamentos.Clear();
            Status = "Sessão JWT ausente. Autentique o terminal antes de registrar produção.";
            return false;
        }

        var inicio = InicioPicker.SelectedDate?.ToUniversalTime();
        var fim = FimPicker.SelectedDate?.Date.AddDays(1).AddTicks(-1).ToUniversalTime();
        Status = "Consultando apontamentos na API oficial...";
        var result = await _api.GetOpsProducaoApontamentosAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            inicio,
            fim,
            _lifetime.Token);
        if (!result.Success || result.Data is null)
        {
            Status = $"Apontamentos não carregados: {result.Detail}";
            return false;
        }

        Apontamentos.Clear();
        foreach (var item in result.Data)
        {
            Apontamentos.Add(item);
        }

        Status = $"{Apontamentos.Count} apontamento(s) carregado(s) da API oficial.";
        return true;
    }

    private bool TryBuildInsumos(out List<DesktopOpsProducaoInsumoRequest> insumos)
    {
        insumos = new List<DesktopOpsProducaoInsumoRequest>();
        foreach (var insumo in Insumos)
        {
            if (string.IsNullOrWhiteSpace(insumo.Descricao))
            {
                Status = "A descrição de cada insumo é obrigatória.";
                return false;
            }

            if (!TryReadDecimal(insumo.Quantidade, "quantidade do insumo", out var quantidade) || quantidade <= 0)
            {
                if (quantidade <= 0)
                {
                    Status = "A quantidade de cada insumo deve ser maior que zero.";
                }

                return false;
            }

            insumos.Add(new DesktopOpsProducaoInsumoRequest(
                NullIfWhiteSpace(insumo.Codigo),
                insumo.Descricao.Trim(),
                quantidade,
                NullIfWhiteSpace(insumo.Unidade) ?? "UN"));
        }

        return true;
    }

    private void CarregarInsumosPersistidos(string? insumosJson)
    {
        Insumos.Clear();
        if (string.IsNullOrWhiteSpace(insumosJson))
        {
            return;
        }

        try
        {
            var itens = System.Text.Json.JsonSerializer.Deserialize<List<DesktopOpsProducaoInsumoRequest>>(insumosJson);
            if (itens is null)
            {
                return;
            }

            foreach (var item in itens)
            {
                Insumos.Add(new OpsInsumoInput
                {
                    Codigo = item.Codigo ?? string.Empty,
                    Descricao = item.Descricao,
                    Quantidade = item.Quantidade.ToString(CultureInfo.InvariantCulture),
                    Unidade = item.Unidade ?? "UN"
                });
            }
        }
        catch (System.Text.Json.JsonException)
        {
            Status = "Os insumos persistidos possuem formato inválido e não foram convertidos para edição.";
        }
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

    public sealed class OpsInsumoInput : INotifyPropertyChanged
    {
        private string _codigo = string.Empty;
        private string _descricao = string.Empty;
        private string _quantidade = "1";
        private string _unidade = "UN";

        public string Codigo { get => _codigo; set => SetField(ref _codigo, value); }
        public string Descricao { get => _descricao; set => SetField(ref _descricao, value); }
        public string Quantidade { get => _quantidade; set => SetField(ref _quantidade, value); }
        public string Unidade { get => _unidade; set => SetField(ref _unidade, value); }

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
