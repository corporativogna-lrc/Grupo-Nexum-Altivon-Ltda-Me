/* Consulta administrativa de pedidos via API oficial — versão 1.1.5.7195. */
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class PedidoConsultaWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private List<DesktopPedidoConsulta> _allOrders = [];
    private string _status = "Aguardando sessão autenticada.";

    public PedidoConsultaWindow(TerminalProfile terminal)
    {
        ArgumentNullException.ThrowIfNull(terminal);
        InitializeComponent();
        _terminal = terminal;
        DataContext = this;
        Loaded += async (_, _) => await LoadAsync();
        Closed += (_, _) => { _lifetime.Cancel(); _lifetime.Dispose(); };
    }

    public ObservableCollection<DesktopPedidoConsulta> Pedidos { get; } = [];
    public string Status
    {
        get => _status;
        private set { _status = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status))); }
    }
    public event PropertyChangedEventHandler? PropertyChanged;

    private async Task LoadAsync()
    {
        var token = _terminal.DesktopAccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            _allOrders = [];
            ApplyFilter();
            Status = "Sessão ausente. Entre no Desktop para consultar pedidos.";
            return;
        }

        Status = "Consultando pedidos na API oficial…";
        try
        {
            var result = await _api.GetOrdersAsync(_terminal, token, _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                _allOrders = [];
                ApplyFilter();
                Status = $"Falha ao consultar pedidos: {result.Detail}";
                return;
            }

            _allOrders = result.Data;
            ApplyFilter();
            Status = $"{_allOrders.Count} pedido(s) recente(s) retornado(s) pela API.";
        }
        catch (OperationCanceledException)
        {
            Status = "Consulta cancelada ao fechar a janela.";
        }
    }

    private void ApplyFilter()
    {
        var term = PesquisaBox.Text.Trim();
        var rows = string.IsNullOrWhiteSpace(term)
            ? _allOrders
            : _allOrders.Where(order =>
                order.NumeroPedido.Contains(term, StringComparison.OrdinalIgnoreCase)
                || order.Status.Contains(term, StringComparison.OrdinalIgnoreCase)
                || order.StatusPagamento.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        Pedidos.Clear();
        foreach (var order in rows) Pedidos.Add(order);
        DetalheText.Text = "Selecione um pedido.";
    }

    private async void Recarregar_Click(object sender, RoutedEventArgs e) => await LoadAsync();

    private void Buscar_Click(object sender, RoutedEventArgs e)
    {
        ApplyFilter();
        Status = $"Exibindo {Pedidos.Count} de {_allOrders.Count} pedidos carregados.";
    }

    private void PedidosGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (PedidosGrid.SelectedItem is not DesktopPedidoConsulta order)
        {
            DetalheText.Text = "Selecione um pedido.";
            return;
        }

        DetalheText.Text =
            $"Número: {order.NumeroPedido}\n" +
            $"Cliente: {order.ClienteNome ?? "Não informado"}\n" +
            $"Loja: {order.LojaNome ?? (order.LojaId.HasValue ? $"ID {order.LojaId}" : "Não informada")}\n" +
            $"Status: {order.Status}\n" +
            $"Pagamento: {order.StatusPagamento}\n" +
            $"Meio de pagamento: {order.MeioPagamento ?? "Não informado"}\n" +
            $"Total: R$ {order.Total:N2}\n" +
            $"Frete: R$ {order.FreteValor:N2} · {order.FreteMetodo ?? "método não informado"}\n" +
            $"Criado em (UTC): {order.CreatedAt.ToUniversalTime():dd/MM/yyyy HH:mm}\n" +
            $"Itens:\n{FormatItems(order.Itens)}\n" +
            "Consulta somente leitura; a tela não altera o pedido.";
    }

    private static string FormatItems(List<DesktopPedidoItemResumo>? items) =>
        items is { Count: > 0 }
            ? string.Join(Environment.NewLine, items.Select(item =>
                $"• {item.NomeProduto} · {item.Quantidade} × R$ {item.PrecoUnitario:N2} = R$ {item.PrecoTotal:N2}"))
            : "Nenhum item detalhado retornado pela API.";
}
