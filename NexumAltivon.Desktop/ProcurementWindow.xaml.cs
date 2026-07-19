/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7185
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class ProcurementWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly TerminalProfile _terminal;
    private readonly CancellationTokenSource _lifetime = new();
    private string _codigo = string.Empty;
    private string _tipo;
    private string _origemAquisicao = "EstoqueFisico";
    private string _finalidade = "Reposicao/operacao";
    private string _prioridade = "Normal";
    private string _produtoNome = string.Empty;
    private string _quantidade = "1";
    private string _custoUnitario = "0,00";
    private string _prazoEntregaDias = "7";
    private DateTime? _previsaoEntrega = DateTime.Today.AddDays(7);
    private DateTime? _dataVencimento = DateTime.Today.AddDays(7);
    private string _meioPagamento = "A definir";
    private string _numeroDocumento = string.Empty;
    private string _chaveNfeEntrada = string.Empty;
    private string _recebidoPor;
    private string _observacoes = string.Empty;
    private string _status = "Carregue o painel para iniciar uma operacao de compras.";
    private bool _isBusy;
    private DesktopComprasKpi? _kpis;
    private DesktopCompraFornecedor? _selectedFornecedor;
    private DesktopCompraProdutoReposicao? _selectedProduto;
    private DesktopCompraSolicitacao? _selectedSolicitacao;
    private DesktopCompraCotacao? _selectedCotacao;
    private DesktopCompraPedido? _selectedPedido;
    private DesktopCompraEntrada? _selectedEntrada;

    public ProcurementWindow(string operationType, TerminalProfile terminal)
    {
        _tipo = IsSupportedOperation(operationType) ? operationType : "Solicitação de compra";
        _terminal = terminal;
        _recebidoPor = terminal.OperatorName;
        OperationTypes = new ObservableCollection<string>
        {
            "Solicitação de compra",
            "Cotação com fornecedores",
            "Pedido de compra",
            "Entrada de mercadoria"
        };
        OrigensAquisicao = new ObservableCollection<string> { "EstoqueFisico", "Encomenda", "Dropshipping", "Parceria" };
        Prioridades = new ObservableCollection<string> { "Baixa", "Normal", "Alta", "Urgente" };

        InitializeComponent();
        DataContext = this;
        NovoDocumento();
    }

    public ObservableCollection<string> OperationTypes { get; }
    public ObservableCollection<string> OrigensAquisicao { get; }
    public ObservableCollection<string> Prioridades { get; }
    public ObservableCollection<DesktopCompraFornecedor> Fornecedores { get; } = [];
    public ObservableCollection<DesktopCompraProdutoReposicao> Produtos { get; } = [];
    public ObservableCollection<DesktopCompraSolicitacao> Solicitacoes { get; } = [];
    public ObservableCollection<DesktopCompraCotacao> Cotacoes { get; } = [];
    public ObservableCollection<DesktopCompraPedido> Pedidos { get; } = [];
    public ObservableCollection<DesktopCompraEntrada> Entradas { get; } = [];
    public ObservableCollection<DesktopCompraEntradaItemEdicao> ItensEntrada { get; } = [];

    public string WindowTitle => $"GenesisGest.Net - {Tipo}";
    public string TenantLabel => $"{_terminal.StoreCode} | {_terminal.StoreName}";

    public string Codigo
    {
        get => _codigo;
        private set => SetField(ref _codigo, value);
    }

    public string Tipo
    {
        get => _tipo;
        set
        {
            if (SetField(ref _tipo, value))
            {
                OnPropertyChanged(nameof(WindowTitle));
                Status = $"Fluxo selecionado: {value}.";
            }
        }
    }

    public string OrigemAquisicao
    {
        get => _origemAquisicao;
        set => SetField(ref _origemAquisicao, value);
    }

    public string Finalidade
    {
        get => _finalidade;
        set => SetField(ref _finalidade, value);
    }

    public string Prioridade
    {
        get => _prioridade;
        set => SetField(ref _prioridade, value);
    }

    public string ProdutoNome
    {
        get => _produtoNome;
        set => SetField(ref _produtoNome, value);
    }

    public string Quantidade
    {
        get => _quantidade;
        set
        {
            if (SetField(ref _quantidade, value))
            {
                OnPropertyChanged(nameof(TotalEstimadoLabel));
            }
        }
    }

    public string CustoUnitario
    {
        get => _custoUnitario;
        set
        {
            if (SetField(ref _custoUnitario, value))
            {
                OnPropertyChanged(nameof(TotalEstimadoLabel));
            }
        }
    }

    public string PrazoEntregaDias
    {
        get => _prazoEntregaDias;
        set => SetField(ref _prazoEntregaDias, value);
    }

    public DateTime? PrevisaoEntrega
    {
        get => _previsaoEntrega;
        set => SetField(ref _previsaoEntrega, value);
    }

    public DateTime? DataVencimento
    {
        get => _dataVencimento;
        set => SetField(ref _dataVencimento, value);
    }

    public string MeioPagamento
    {
        get => _meioPagamento;
        set => SetField(ref _meioPagamento, value);
    }

    public string NumeroDocumento
    {
        get => _numeroDocumento;
        set => SetField(ref _numeroDocumento, value);
    }

    public string ChaveNfeEntrada
    {
        get => _chaveNfeEntrada;
        set => SetField(ref _chaveNfeEntrada, value);
    }

    public string RecebidoPor
    {
        get => _recebidoPor;
        set => SetField(ref _recebidoPor, value);
    }

    public string Observacoes
    {
        get => _observacoes;
        set => SetField(ref _observacoes, value);
    }

    public string Status
    {
        get => _status;
        private set => SetField(ref _status, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set => SetField(ref _isBusy, value);
    }

    public DesktopCompraFornecedor? SelectedFornecedor
    {
        get => _selectedFornecedor;
        set
        {
            if (SetField(ref _selectedFornecedor, value) && value is not null)
            {
                PrazoEntregaDias = value.PrazoEntregaDias.ToString(CultureInfo.InvariantCulture);
                PrevisaoEntrega = DateTime.Today.AddDays(Math.Max(0, value.PrazoEntregaDias));
            }
        }
    }

    public DesktopCompraProdutoReposicao? SelectedProduto
    {
        get => _selectedProduto;
        set
        {
            if (SetField(ref _selectedProduto, value) && value is not null)
            {
                ProdutoNome = value.ProdutoNome;
                CustoUnitario = value.CustoAtual.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
                if (value.FornecedorId.HasValue)
                {
                    SelectedFornecedor = Fornecedores.FirstOrDefault(item => item.Id == value.FornecedorId.Value);
                }
            }
        }
    }

    public DesktopCompraSolicitacao? SelectedSolicitacao
    {
        get => _selectedSolicitacao;
        set
        {
            if (SetField(ref _selectedSolicitacao, value) && value is not null)
            {
                Codigo = $"SOL-{value.Id:D6}";
                ProdutoNome = value.ProdutoNome;
                Quantidade = value.Quantidade.ToString(CultureInfo.InvariantCulture);
                OrigemAquisicao = value.Origem;
                Finalidade = value.Finalidade;
                Prioridade = value.Prioridade;
                SelectedProduto = value.ProdutoId.HasValue
                    ? Produtos.FirstOrDefault(item => item.ProdutoId == value.ProdutoId.Value)
                    : null;
                Status = $"Solicitacao {value.Id} carregada com status {value.Status}.";
            }
        }
    }

    public DesktopCompraCotacao? SelectedCotacao
    {
        get => _selectedCotacao;
        set
        {
            if (SetField(ref _selectedCotacao, value) && value is not null)
            {
                Codigo = $"COT-{value.Id:D6}";
                SelectedSolicitacao = Solicitacoes.FirstOrDefault(item => item.Id == value.SolicitacaoId);
                SelectedFornecedor = Fornecedores.FirstOrDefault(item => item.Id == value.FornecedorId);
                ProdutoNome = value.ProdutoNome;
                Quantidade = value.Quantidade.ToString(CultureInfo.InvariantCulture);
                CustoUnitario = value.CustoUnitario.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
                PrazoEntregaDias = value.PrazoEntregaDias.ToString(CultureInfo.InvariantCulture);
                Codigo = $"COT-{value.Id:D6}";
                Status = $"Cotacao {value.Id} carregada e confirmada no servidor.";
            }
        }
    }

    public DesktopCompraPedido? SelectedPedido
    {
        get => _selectedPedido;
        set
        {
            if (SetField(ref _selectedPedido, value))
            {
                ItensEntrada.Clear();
                if (value is null)
                {
                    return;
                }

                Codigo = value.Numero;
                OrigemAquisicao = value.Origem;
                Finalidade = value.Finalidade;
                PrevisaoEntrega = value.DataPrevistaEntrega;
                var firstItem = value.Itens.FirstOrDefault();
                if (firstItem is not null)
                {
                    SelectedProduto = firstItem.ProdutoId.HasValue
                        ? Produtos.FirstOrDefault(item => item.ProdutoId == firstItem.ProdutoId.Value)
                        : null;
                    ProdutoNome = firstItem.ProdutoNome;
                    Quantidade = firstItem.Quantidade.ToString(CultureInfo.InvariantCulture);
                    CustoUnitario = firstItem.CustoUnitario.ToString("N2", CultureInfo.GetCultureInfo("pt-BR"));
                }

                SelectedFornecedor = Fornecedores.FirstOrDefault(item => item.Id == value.FornecedorId);

                foreach (var item in value.Itens.Where(item => item.QuantidadePendente > 0))
                {
                    ItensEntrada.Add(new DesktopCompraEntradaItemEdicao(
                        item.Id,
                        item.ProdutoNome,
                        item.Quantidade,
                        item.QuantidadeRecebida,
                        item.QuantidadePendente));
                }

                Status = $"Pedido {value.Numero} carregado com {ItensEntrada.Count} item(ns) pendente(s).";
            }
        }
    }

    public DesktopCompraEntrada? SelectedEntrada
    {
        get => _selectedEntrada;
        set
        {
            if (SetField(ref _selectedEntrada, value) && value is not null)
            {
                Codigo = $"ENT-{value.Id:D6}";
                SelectedPedido = Pedidos.FirstOrDefault(item => item.Id == value.CompraPedidoId);
                NumeroDocumento = value.NumeroDocumento ?? string.Empty;
                ChaveNfeEntrada = value.ChaveNfeEntrada ?? string.Empty;
                Codigo = $"ENT-{value.Id:D6}";
                Status = $"Entrada {value.Id} relida do servidor com status fiscal {value.StatusFiscal}.";
            }
        }
    }

    public string TotalEstimadoLabel => $"R$ {ParseInt(Quantidade) * ParseDecimal(CustoUnitario):N2}";
    public string KpiSolicitacoes => (_kpis?.SolicitacoesAbertas ?? 0).ToString(CultureInfo.InvariantCulture);
    public string KpiPedidos => (_kpis?.PedidosAbertos ?? 0).ToString(CultureInfo.InvariantCulture);
    public string KpiEntradas => (_kpis?.EntradasMes ?? 0).ToString(CultureInfo.InvariantCulture);
    public string KpiValor => $"R$ {_kpis?.ValorComprasAbertas ?? 0m:N2}";
    public string AlertasTexto { get; private set; } = "Nenhum alerta carregado.";

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        await CarregarPainelAsync();
    }

    private async void Recarregar_Click(object sender, RoutedEventArgs e)
    {
        await CarregarPainelAsync();
    }

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        await ExecutarGravacaoAsync();
    }

    private async void EnviarAprovacao_Click(object sender, RoutedEventArgs e)
    {
        await AtualizarStatusSelecionadoAsync("EmAprovacao");
    }

    private async void Aprovar_Click(object sender, RoutedEventArgs e)
    {
        await AtualizarStatusSelecionadoAsync("Aprovado");
    }

    private async void Reprovar_Click(object sender, RoutedEventArgs e)
    {
        await AtualizarStatusSelecionadoAsync("Reprovado");
    }

    private async void GerarPedido_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedSolicitacao is null || !SelectedSolicitacao.Status.Equals("Aprovada", StringComparison.OrdinalIgnoreCase))
        {
            Status = "Selecione uma solicitacao aprovada antes de gerar o pedido de compra.";
            return;
        }

        Tipo = "Pedido de compra";
        await ExecutarGravacaoAsync();
    }

    private async void RegistrarEntrada_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedPedido is null)
        {
            Status = "Selecione um pedido com quantidade pendente antes de registrar a entrada.";
            return;
        }

        Tipo = "Entrada de mercadoria";
        await ExecutarGravacaoAsync();
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        NovoDocumento();
    }

    private void Fechar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected override void OnClosed(EventArgs e)
    {
        _lifetime.Cancel();
        _lifetime.Dispose();
        base.OnClosed(e);
    }

    private async Task CarregarPainelAsync()
    {
        if (!EnsureSession() || IsBusy)
        {
            return;
        }

        IsBusy = true;
        Status = "Carregando dados reais de compras...";
        try
        {
            var result = await _api.GetComprasPainelAsync(
                _terminal,
                _terminal.DesktopAccessToken,
                _lifetime.Token);
            if (!result.Success || result.Data is null)
            {
                Status = $"Falha ao carregar compras: {result.Detail}";
                return;
            }

            ApplyPainel(result.Data);
            Status = $"Painel relido da API: {Solicitacoes.Count} solicitacoes, {Cotacoes.Count} cotacoes, {Pedidos.Count} pedidos e {Entradas.Count} entradas.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Status = $"Falha inesperada ao carregar compras: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ExecutarGravacaoAsync()
    {
        if (!EnsureSession() || IsBusy)
        {
            return;
        }

        IsBusy = true;
        try
        {
            switch (Tipo)
            {
                case "Solicitação de compra":
                    await RegistrarSolicitacaoAsync();
                    break;
                case "Cotação com fornecedores":
                    await RegistrarCotacaoAsync();
                    break;
                case "Pedido de compra":
                    await RegistrarPedidoAsync();
                    break;
                case "Entrada de mercadoria":
                    await RegistrarEntradaAsync();
                    break;
                default:
                    Status = $"Operacao {Tipo} bloqueada: nao existe contrato de dominio persistente para esta janela.";
                    break;
            }
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Status = $"Falha inesperada na operacao de compras: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task RegistrarSolicitacaoAsync()
    {
        if (!TryReadBaseValues(false, out var quantidade, out _))
        {
            return;
        }

        var previousIds = Solicitacoes.Select(item => item.Id).ToHashSet();
        Status = "Registrando solicitacao no banco oficial...";
        var result = await _api.CreateCompraSolicitacaoAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            new DesktopCompraSolicitacaoRequest(
                SelectedProduto?.ProdutoId,
                ProdutoNome.Trim(),
                quantidade,
                OrigemAquisicao,
                Finalidade.Trim(),
                Prioridade,
                NormalizeOptional(Observacoes)),
            _lifetime.Token);
        var reread = await RereadAfterWriteAsync(result);
        var created = reread?.Solicitacoes.FirstOrDefault(item => !previousIds.Contains(item.Id));
        if (created is null)
        {
            Status = BuildUnconfirmedWriteMessage(result, "solicitacao");
            return;
        }

        ApplyPainel(reread!);
        SelectedSolicitacao = Solicitacoes.First(item => item.Id == created.Id);
        Status = $"Solicitacao {created.Id} persistida e confirmada por releitura da API.";
    }

    private async Task RegistrarCotacaoAsync()
    {
        if (!TryReadBaseValues(true, out var quantidade, out var custo) || SelectedFornecedor is null)
        {
            if (SelectedFornecedor is null)
            {
                Status = "Selecione um fornecedor ativo para registrar a cotacao.";
            }
            return;
        }

        if (!int.TryParse(PrazoEntregaDias, NumberStyles.Integer, CultureInfo.InvariantCulture, out var prazo) || prazo < 0)
        {
            Status = "Prazo de entrega nao pode ser negativo.";
            return;
        }

        var previousIds = Cotacoes.Select(item => item.Id).ToHashSet();
        Status = "Registrando cotacao vinculada ao fornecedor...";
        var result = await _api.CreateCompraCotacaoAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            new DesktopCompraCotacaoRequest(
                SelectedFornecedor.Id,
                SelectedSolicitacao?.Id,
                SelectedProduto?.ProdutoId,
                ProdutoNome.Trim(),
                quantidade,
                custo,
                OrigemAquisicao,
                Finalidade.Trim(),
                Prioridade,
                prazo,
                NormalizeOptional(Observacoes)),
            _lifetime.Token);
        var reread = await RereadAfterWriteAsync(result);
        var created = reread?.Cotacoes.FirstOrDefault(item => !previousIds.Contains(item.Id));
        if (created is null)
        {
            Status = BuildUnconfirmedWriteMessage(result, "cotacao");
            return;
        }

        ApplyPainel(reread!);
        SelectedCotacao = Cotacoes.First(item => item.Id == created.Id);
        Status = $"Cotacao {created.Id} persistida e confirmada por releitura da API.";
    }

    private async Task RegistrarPedidoAsync()
    {
        if (!TryReadBaseValues(true, out var quantidade, out var custo) || SelectedFornecedor is null)
        {
            if (SelectedFornecedor is null)
            {
                Status = "Selecione um fornecedor ativo para gerar o pedido de compra.";
            }
            return;
        }

        var previousIds = Pedidos.Select(item => item.Id).ToHashSet();
        Status = "Gerando pedido, despesa e conta a pagar...";
        var result = await _api.CreateCompraPedidoAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            new DesktopCompraPedidoRequest(
                SelectedFornecedor.Id,
                SelectedSolicitacao?.Id,
                OrigemAquisicao,
                Finalidade.Trim(),
                PrevisaoEntrega,
                DataVencimento,
                NormalizeOptional(MeioPagamento),
                NormalizeOptional(Observacoes),
                [new DesktopCompraPedidoItemRequest(
                    SelectedProduto?.ProdutoId,
                    ProdutoNome.Trim(),
                    SelectedProduto?.Sku,
                    quantidade,
                    custo)]),
            _lifetime.Token);
        var reread = await RereadAfterWriteAsync(result);
        var created = reread?.Pedidos.FirstOrDefault(item => !previousIds.Contains(item.Id));
        if (created is null)
        {
            Status = BuildUnconfirmedWriteMessage(result, "pedido de compra");
            return;
        }

        ApplyPainel(reread!);
        SelectedPedido = Pedidos.First(item => item.Id == created.Id);
        Status = $"Pedido {created.Numero} persistido e confirmado por releitura da API. {result.Detail}";
    }

    private async Task RegistrarEntradaAsync()
    {
        if (SelectedPedido is null)
        {
            Status = "Selecione o pedido que recebera a entrada.";
            return;
        }

        var documento = NormalizeOptional(NumeroDocumento);
        var chave = OnlyDigits(ChaveNfeEntrada);
        if (documento is null && string.IsNullOrWhiteSpace(chave))
        {
            Status = "Informe o numero do documento ou a chave da NF-e de entrada.";
            return;
        }

        if (!string.IsNullOrWhiteSpace(chave) && chave.Length != 44)
        {
            Status = "A chave da NF-e de entrada deve conter 44 digitos.";
            return;
        }

        var invalidItem = ItensEntrada.FirstOrDefault(item => item.QuantidadeReceber < 0 || item.QuantidadeReceber > item.QuantidadePendente);
        if (invalidItem is not null)
        {
            Status = $"Quantidade invalida para {invalidItem.ProdutoNome}; maximo pendente: {invalidItem.QuantidadePendente}.";
            return;
        }

        var items = ItensEntrada
            .Where(item => item.QuantidadeReceber > 0)
            .Select(item => new DesktopCompraEntradaItemRequest(item.ItemId, item.QuantidadeReceber))
            .ToList();
        if (items.Count == 0)
        {
            Status = "Informe ao menos uma quantidade efetivamente recebida.";
            return;
        }

        var previousIds = Entradas.Select(item => item.Id).ToHashSet();
        Status = "Registrando entrada e atualizando estoque...";
        var result = await _api.CreateCompraEntradaAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            SelectedPedido.Id,
            new DesktopCompraEntradaRequest(
                documento,
                string.IsNullOrWhiteSpace(chave) ? null : chave,
                OrigemAquisicao,
                NormalizeOptional(RecebidoPor),
                NormalizeOptional(Observacoes),
                items),
            _lifetime.Token);
        var reread = await RereadAfterWriteAsync(result);
        var created = reread?.Entradas.FirstOrDefault(item => !previousIds.Contains(item.Id));
        if (created is null)
        {
            Status = BuildUnconfirmedWriteMessage(result, "entrada de mercadoria");
            return;
        }

        ApplyPainel(reread!);
        SelectedEntrada = Entradas.First(item => item.Id == created.Id);
        Status = $"Entrada {created.Id} persistida; saldo e pedido confirmados por releitura da API.";
    }

    private async Task AtualizarStatusSelecionadoAsync(string requestedStatus)
    {
        if (!EnsureSession() || IsBusy)
        {
            return;
        }

        var useOrder = Tipo is "Pedido de compra" or "Entrada de mercadoria";
        if (useOrder && SelectedPedido is null)
        {
            Status = "Selecione um pedido antes de alterar o status.";
            return;
        }
        if (!useOrder && SelectedSolicitacao is null)
        {
            Status = "Selecione uma solicitacao antes de alterar o status.";
            return;
        }

        IsBusy = true;
        try
        {
            var result = useOrder
                ? await _api.UpdateCompraPedidoStatusAsync(
                    _terminal,
                    _terminal.DesktopAccessToken,
                    SelectedPedido!.Id,
                    new DesktopCompraStatusRequest(requestedStatus, NormalizeOptional(Observacoes)),
                    _lifetime.Token)
                : await _api.UpdateCompraSolicitacaoStatusAsync(
                    _terminal,
                    _terminal.DesktopAccessToken,
                    SelectedSolicitacao!.Id,
                    new DesktopCompraStatusRequest(requestedStatus, NormalizeOptional(Observacoes)),
                    _lifetime.Token);

            var reread = await RereadAfterWriteAsync(result);
            if (reread is null)
            {
                Status = BuildUnconfirmedWriteMessage(result, "alteracao de status");
                return;
            }

            var expected = NormalizeExpectedStatus(requestedStatus, useOrder);
            var confirmed = useOrder
                ? reread.Pedidos.Any(item => item.Id == SelectedPedido!.Id && item.Status.Equals(expected, StringComparison.OrdinalIgnoreCase))
                : reread.Solicitacoes.Any(item => item.Id == SelectedSolicitacao!.Id && item.Status.Equals(expected, StringComparison.OrdinalIgnoreCase));
            if (!confirmed)
            {
                Status = $"A API respondeu, mas a releitura nao confirmou o status {expected}.";
                return;
            }

            var selectedOrderId = SelectedPedido?.Id;
            var selectedRequestId = SelectedSolicitacao?.Id;
            ApplyPainel(reread);
            if (useOrder && selectedOrderId.HasValue)
            {
                SelectedPedido = Pedidos.First(item => item.Id == selectedOrderId.Value);
            }
            else if (selectedRequestId.HasValue)
            {
                SelectedSolicitacao = Solicitacoes.First(item => item.Id == selectedRequestId.Value);
            }
            Status = $"Status {expected} persistido e confirmado por releitura da API.";
        }
        catch (OperationCanceledException) when (_lifetime.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            Status = $"Falha inesperada ao alterar status: {exception.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<DesktopComprasPainel?> RereadAfterWriteAsync(DesktopApiDataResult<DesktopComprasPainel> writeResult)
    {
        if (!writeResult.Success || writeResult.Data is null)
        {
            Status = $"A API recusou a operacao: {writeResult.Detail}";
            return null;
        }

        var reread = await _api.GetComprasPainelAsync(
            _terminal,
            _terminal.DesktopAccessToken,
            _lifetime.Token);
        if (!reread.Success || reread.Data is null)
        {
            Status = $"A gravacao respondeu, mas a releitura independente falhou: {reread.Detail}";
            return null;
        }

        return reread.Data;
    }

    private void ApplyPainel(DesktopComprasPainel painel)
    {
        Replace(Fornecedores, painel.Fornecedores);
        Replace(Produtos, painel.ProdutosReposicao);
        Replace(Solicitacoes, painel.Solicitacoes);
        Replace(Cotacoes, painel.Cotacoes);
        Replace(Pedidos, painel.Pedidos);
        Replace(Entradas, painel.Entradas);
        _kpis = painel.Kpis;
        AlertasTexto = painel.Alertas.Count == 0 ? "Nenhum alerta operacional." : string.Join(" | ", painel.Alertas);
        OnPropertyChanged(nameof(KpiSolicitacoes));
        OnPropertyChanged(nameof(KpiPedidos));
        OnPropertyChanged(nameof(KpiEntradas));
        OnPropertyChanged(nameof(KpiValor));
        OnPropertyChanged(nameof(AlertasTexto));
    }

    private bool TryReadBaseValues(bool requireCost, out int quantidade, out decimal custo)
    {
        quantidade = ParseInt(Quantidade);
        custo = ParseDecimal(CustoUnitario);
        if (string.IsNullOrWhiteSpace(ProdutoNome))
        {
            Status = "Selecione um produto ou informe a descricao do item.";
            return false;
        }
        if (quantidade <= 0)
        {
            Status = "Quantidade deve ser um numero inteiro maior que zero.";
            return false;
        }
        if (requireCost && custo <= 0)
        {
            Status = "Custo unitario deve ser maior que zero.";
            return false;
        }
        if (string.IsNullOrWhiteSpace(Finalidade))
        {
            Status = "Informe a finalidade operacional da compra.";
            return false;
        }

        return true;
    }

    private bool EnsureSession()
    {
        if (!string.IsNullOrWhiteSpace(_terminal.DesktopAccessToken))
        {
            return true;
        }

        Status = "Sessao administrativa ausente. Autentique o Desktop em Sistema > Rede e endpoints.";
        return false;
    }

    private void NovoDocumento()
    {
        Codigo = $"NOVO-{DateTime.Now:yyyyMMddHHmmss}";
        SelectedSolicitacao = null;
        SelectedCotacao = null;
        SelectedPedido = null;
        SelectedEntrada = null;
        SelectedProduto = null;
        SelectedFornecedor = null;
        ProdutoNome = string.Empty;
        Quantidade = "1";
        CustoUnitario = "0,00";
        PrazoEntregaDias = "7";
        PrevisaoEntrega = DateTime.Today.AddDays(7);
        DataVencimento = DateTime.Today.AddDays(7);
        MeioPagamento = "A definir";
        NumeroDocumento = string.Empty;
        ChaveNfeEntrada = string.Empty;
        RecebidoPor = _terminal.OperatorName;
        Observacoes = string.Empty;
        ItensEntrada.Clear();
        Status = $"Novo documento preparado para {Tipo}. Nenhum dado foi gravado.";
    }

    private static string NormalizeExpectedStatus(string requestedStatus, bool order)
        => requestedStatus switch
        {
            "EmAprovacao" => "EmAprovacao",
            "Aprovado" => order ? "Aprovado" : "Aprovada",
            "Reprovado" => order ? "Reprovado" : "Reprovada",
            _ => requestedStatus
        };

    private static string BuildUnconfirmedWriteMessage(
        DesktopApiDataResult<DesktopComprasPainel> result,
        string operation)
        => result.Success
            ? $"A API respondeu, mas a releitura nao identificou a nova {operation}; nenhuma confirmacao de sucesso foi emitida."
            : $"Falha ao registrar {operation}: {result.Detail}";

    private static bool IsSupportedOperation(string value)
        => value is "Solicitação de compra" or "Cotação com fornecedores" or "Pedido de compra" or "Entrada de mercadoria";

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private static int ParseInt(string value)
        => int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0;

    private static decimal ParseDecimal(string value)
    {
        if (decimal.TryParse(value, NumberStyles.Number, CultureInfo.GetCultureInfo("pt-BR"), out var pt))
        {
            return pt;
        }

        return decimal.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out var invariant)
            ? invariant
            : 0m;
    }

    private static string OnlyDigits(string value)
        => new((value ?? string.Empty).Where(char.IsDigit).ToArray());

    private static string? NormalizeOptional(string value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

public sealed class DesktopCompraEntradaItemEdicao
{
    public DesktopCompraEntradaItemEdicao(
        int itemId,
        string produtoNome,
        int quantidadePedido,
        int quantidadeRecebida,
        int quantidadePendente)
    {
        ItemId = itemId;
        ProdutoNome = produtoNome;
        QuantidadePedido = quantidadePedido;
        QuantidadeRecebida = quantidadeRecebida;
        QuantidadePendente = quantidadePendente;
        QuantidadeReceber = quantidadePendente;
    }

    public int ItemId { get; }
    public string ProdutoNome { get; }
    public int QuantidadePedido { get; }
    public int QuantidadeRecebida { get; }
    public int QuantidadePendente { get; }
    public int QuantidadeReceber { get; set; }
}
