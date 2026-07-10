/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using Microsoft.Win32;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class ProcurementWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _api = new();
    private readonly LocalOutboxService _outbox = new();
    private readonly TerminalProfile _terminal = new();
    private string _codigo = string.Empty;
    private string _tipo;
    private string _empresaDestino = "NEXUM";
    private string _origemAquisicao = "DIST";
    private string _fornecedorParceiro = string.Empty;
    private string _itemDescricao = string.Empty;
    private string _categoria = string.Empty;
    private string _unidadeComercial = "UN";
    private string _ncm = string.Empty;
    private string _cest = string.Empty;
    private string _cfopSugerido = "5102";
    private string _origemFiscalItem = "0 - Nacional";
    private string _gtinCodigoBarras = string.Empty;
    private string _pesoDimensoes = string.Empty;
    private string _quantidade = "1";
    private string _custoUnitario = "0,00";
    private string _freteEstimado = "0,00";
    private string _impostosEstimados = "0,00";
    private string _centroCusto = "Estoque / Comercial";
    private string _nivelAprovacao = "Gerencial";
    private string _aprovadorResponsavel = string.Empty;
    private string _statusAprovacao = "Rascunho";
    private string _numeroDocumentoFornecedor = string.Empty;
    private string _serieDocumentoFornecedor = string.Empty;
    private DateTime? _dataEmissaoDocumento;
    private DateTime? _dataEntradaMercadoria = DateTime.Today;
    private DateTime? _previsaoEntrega = DateTime.Today.AddDays(7);
    private string _condicaoPagamento = "À vista";
    private string _xmlImportadoPath = string.Empty;
    private string _xmlChaveAcesso = string.Empty;
    private string _xmlFornecedor = string.Empty;
    private string _xmlValorTotal = "0,00";
    private string _observacoes = string.Empty;
    private string _status = "Pronto para registrar aquisição local.";

    public ProcurementWindow(string operationType)
    {
        _tipo = operationType;
        OperationTypes = new ObservableCollection<string>
        {
            "Solicitação de compra",
            "Cotação com fornecedores",
            "Pedido de compra",
            "Entrada de mercadoria",
            "Dropshipping e parcerias",
            "Devolução a fornecedor"
        };
        EmpresasDestino = new ObservableCollection<string> { "NEXUM", "GTOP", "MODA", "CHRON", "GRANF", "ESTR" };
        OrigensAquisicao = new ObservableCollection<string> { "ECOM", "DROP", "DIST", "PARC", "ESTQ" };
        NiveisAprovacao = new ObservableCollection<string> { "Operacional", "Gerencial", "Diretoria", "Dupla diretoria" };
        StatusAprovacaoOptions = new ObservableCollection<string>
        {
            "Rascunho",
            "Em aprovação",
            "Aprovado",
            "Reprovado",
            "Pedido gerado",
            "Entrada registrada"
        };
        OrigensFiscais = new ObservableCollection<string>
        {
            "0 - Nacional",
            "1 - Estrangeira importação direta",
            "2 - Estrangeira adquirida no mercado interno",
            "3 - Nacional com conteúdo importado superior a 40%",
            "4 - Nacional conforme processos produtivos básicos",
            "5 - Nacional com conteúdo importado inferior ou igual a 40%",
            "6 - Estrangeira importação direta sem similar nacional",
            "7 - Estrangeira adquirida internamente sem similar nacional",
            "8 - Nacional com conteúdo importado superior a 70%"
        };

        NovoDocumento();
        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<string> OperationTypes { get; }
    public ObservableCollection<string> EmpresasDestino { get; }
    public ObservableCollection<string> OrigensAquisicao { get; }
    public ObservableCollection<string> NiveisAprovacao { get; }
    public ObservableCollection<string> StatusAprovacaoOptions { get; }
    public ObservableCollection<string> OrigensFiscais { get; }
    public string WindowTitle => $"GenesisGest.Net - {Tipo}";

    public string Codigo
    {
        get => _codigo;
        set => SetField(ref _codigo, value);
    }

    public string Tipo
    {
        get => _tipo;
        set
        {
            if (SetField(ref _tipo, value))
            {
                NovoDocumento(false);
                OnPropertyChanged(nameof(WindowTitle));
            }
        }
    }

    public string EmpresaDestino
    {
        get => _empresaDestino;
        set
        {
            if (SetField(ref _empresaDestino, value))
            {
                NovoDocumento(false);
                AtualizarResumo();
            }
        }
    }

    public string OrigemAquisicao
    {
        get => _origemAquisicao;
        set
        {
            if (SetField(ref _origemAquisicao, value))
            {
                NovoDocumento(false);
                AtualizarResumo();
            }
        }
    }

    public string FornecedorParceiro
    {
        get => _fornecedorParceiro;
        set => SetField(ref _fornecedorParceiro, value);
    }

    public string ItemDescricao
    {
        get => _itemDescricao;
        set => SetField(ref _itemDescricao, value);
    }

    public string Categoria
    {
        get => _categoria;
        set => SetField(ref _categoria, value);
    }

    public string UnidadeComercial
    {
        get => _unidadeComercial;
        set
        {
            if (SetField(ref _unidadeComercial, value.ToUpperInvariant()))
            {
                AtualizarResumo();
            }
        }
    }

    public string Ncm
    {
        get => _ncm;
        set
        {
            if (SetField(ref _ncm, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string Cest
    {
        get => _cest;
        set => SetField(ref _cest, value);
    }

    public string CfopSugerido
    {
        get => _cfopSugerido;
        set
        {
            if (SetField(ref _cfopSugerido, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string OrigemFiscalItem
    {
        get => _origemFiscalItem;
        set
        {
            if (SetField(ref _origemFiscalItem, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string GtinCodigoBarras
    {
        get => _gtinCodigoBarras;
        set => SetField(ref _gtinCodigoBarras, value);
    }

    public string PesoDimensoes
    {
        get => _pesoDimensoes;
        set => SetField(ref _pesoDimensoes, value);
    }

    public string Quantidade
    {
        get => _quantidade;
        set
        {
            if (SetField(ref _quantidade, value))
            {
                AtualizarResumo();
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
                AtualizarResumo();
            }
        }
    }

    public string FreteEstimado
    {
        get => _freteEstimado;
        set
        {
            if (SetField(ref _freteEstimado, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string ImpostosEstimados
    {
        get => _impostosEstimados;
        set
        {
            if (SetField(ref _impostosEstimados, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string CentroCusto
    {
        get => _centroCusto;
        set => SetField(ref _centroCusto, value);
    }

    public string NivelAprovacao
    {
        get => _nivelAprovacao;
        set
        {
            if (SetField(ref _nivelAprovacao, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string AprovadorResponsavel
    {
        get => _aprovadorResponsavel;
        set
        {
            if (SetField(ref _aprovadorResponsavel, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string StatusAprovacao
    {
        get => _statusAprovacao;
        set
        {
            if (SetField(ref _statusAprovacao, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string NumeroDocumentoFornecedor
    {
        get => _numeroDocumentoFornecedor;
        set => SetField(ref _numeroDocumentoFornecedor, value);
    }

    public string SerieDocumentoFornecedor
    {
        get => _serieDocumentoFornecedor;
        set => SetField(ref _serieDocumentoFornecedor, value);
    }

    public DateTime? DataEmissaoDocumento
    {
        get => _dataEmissaoDocumento;
        set => SetField(ref _dataEmissaoDocumento, value);
    }

    public DateTime? DataEntradaMercadoria
    {
        get => _dataEntradaMercadoria;
        set => SetField(ref _dataEntradaMercadoria, value);
    }

    public DateTime? PrevisaoEntrega
    {
        get => _previsaoEntrega;
        set => SetField(ref _previsaoEntrega, value);
    }

    public string CondicaoPagamento
    {
        get => _condicaoPagamento;
        set => SetField(ref _condicaoPagamento, value);
    }

    public string XmlImportadoPath
    {
        get => _xmlImportadoPath;
        set => SetField(ref _xmlImportadoPath, value);
    }

    public string XmlChaveAcesso
    {
        get => _xmlChaveAcesso;
        set => SetField(ref _xmlChaveAcesso, value);
    }

    public string XmlFornecedor
    {
        get => _xmlFornecedor;
        set => SetField(ref _xmlFornecedor, value);
    }

    public string XmlValorTotal
    {
        get => _xmlValorTotal;
        set
        {
            if (SetField(ref _xmlValorTotal, value))
            {
                AtualizarResumo();
            }
        }
    }

    public string Observacoes
    {
        get => _observacoes;
        set => SetField(ref _observacoes, value);
    }

    public string Status
    {
        get => _status;
        set => SetField(ref _status, value);
    }

    public string TotalEstimadoLabel => $"R$ {TotalEstimado:N2}";
    public string CodigoProdutoSugerido => $"Código sugerido: {EmpresaDestino}-{OrigemAquisicao}-{DateTime.Now:yyyyMMddHHmm}";
    public bool ProdutoFiscalmenteCompleto => !string.IsNullOrWhiteSpace(ItemDescricao)
        && !string.IsNullOrWhiteSpace(Categoria)
        && !string.IsNullOrWhiteSpace(UnidadeComercial)
        && OnlyDigits(Ncm).Length == 8
        && !string.IsNullOrWhiteSpace(CfopSugerido)
        && !string.IsNullOrWhiteSpace(OrigemFiscalItem);
    public string StatusCadastroProduto => ProdutoFiscalmenteCompleto
        ? "Produto apto para compra, entrada, estoque e emissão fiscal."
        : "Produto incompleto: preencha descrição, categoria, unidade, NCM de 8 dígitos, CFOP e origem fiscal.";
    public bool PodeGerarPedido => !Tipo.Contains("Cotação", StringComparison.OrdinalIgnoreCase)
        || !string.IsNullOrWhiteSpace(AprovadorResponsavel);
    public string FluxoAprovacao => PodeGerarPedido
        ? $"Aprovado para avançar por {(string.IsNullOrWhiteSpace(AprovadorResponsavel) ? "responsável operacional" : AprovadorResponsavel.Trim())}."
        : "Cotação bloqueada: informe aprovador gerencial antes de gerar pedido.";
    public string XmlResumo => string.IsNullOrWhiteSpace(XmlImportadoPath)
        ? "Entrada manual. Para entrada por NF-e, importe o XML da nota."
        : $"XML vinculado: {XmlFornecedor} | Chave {XmlChaveAcesso} | Total R$ {ParseDecimal(XmlValorTotal):N2}";
    public string ControleEntrada => Tipo.Contains("Entrada", StringComparison.OrdinalIgnoreCase)
        ? "Entrada alimenta estoque físico, fiscal, QR Code e código de barras."
        : "Documento prepara aquisição e mantém vínculo para entrada posterior.";
    public string EtapaOperacional => $"{StatusAprovacao} | {NivelAprovacao} | {CondicaoPagamento}";

    private decimal TotalEstimado => ParseDecimal(Quantidade) * ParseDecimal(CustoUnitario) + ParseDecimal(FreteEstimado) + ParseDecimal(ImpostosEstimados);

    public event PropertyChangedEventHandler? PropertyChanged;

    private async void Salvar_Click(object sender, RoutedEventArgs e)
    {
        await SalvarOperacaoAsync(StatusAprovacao);
    }

    private async void EnviarAprovacao_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarBase("enviar para aprovação"))
        {
            return;
        }

        StatusAprovacao = "Em aprovação";
        await SalvarOperacaoAsync(StatusAprovacao);
    }

    private async void Aprovar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AprovadorResponsavel))
        {
            Status = "Informe o aprovador responsável antes de aprovar.";
            return;
        }

        if (!ValidarBase("aprovar"))
        {
            return;
        }

        StatusAprovacao = "Aprovado";
        await SalvarOperacaoAsync(StatusAprovacao);
    }

    private async void Reprovar_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(AprovadorResponsavel))
        {
            Status = "Informe o aprovador responsável antes de reprovar.";
            return;
        }

        StatusAprovacao = "Reprovado";
        await SalvarOperacaoAsync(StatusAprovacao);
    }

    private async void GerarPedido_Click(object sender, RoutedEventArgs e)
    {
        if (StatusAprovacao != "Aprovado")
        {
            Status = "Pedido bloqueado: a cotação/solicitação precisa estar aprovada.";
            return;
        }

        if (!ValidarBase("gerar pedido"))
        {
            return;
        }

        Tipo = "Pedido de compra";
        StatusAprovacao = "Pedido gerado";
        await SalvarOperacaoAsync(StatusAprovacao);
    }

    private async void RegistrarEntrada_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidarEntrada())
        {
            return;
        }

        Tipo = "Entrada de mercadoria";
        StatusAprovacao = "Entrada registrada";
        await SalvarOperacaoAsync(StatusAprovacao);
    }

    private async Task SalvarOperacaoAsync(string statusOperacional)
    {
        var draft = new ProcurementDraft
        {
            Codigo = Codigo,
            Tipo = Tipo,
            StatusAprovacao = StatusAprovacao,
            EmpresaDestino = EmpresaDestino,
            OrigemAquisicao = OrigemAquisicao,
            FornecedorParceiro = FornecedorParceiro.Trim(),
            ItemDescricao = ItemDescricao.Trim(),
            Categoria = Categoria.Trim(),
            UnidadeComercial = UnidadeComercial.Trim(),
            Ncm = OnlyDigits(Ncm),
            Cest = OnlyDigits(Cest),
            CfopSugerido = CfopSugerido.Trim(),
            OrigemFiscalItem = OrigemFiscalItem.Trim(),
            GtinCodigoBarras = GtinCodigoBarras.Trim(),
            PesoDimensoes = PesoDimensoes.Trim(),
            ProdutoFiscalmenteCompleto = ProdutoFiscalmenteCompleto,
            NumeroDocumentoFornecedor = NumeroDocumentoFornecedor.Trim(),
            SerieDocumentoFornecedor = SerieDocumentoFornecedor.Trim(),
            DataEmissaoDocumento = DataEmissaoDocumento,
            DataEntradaMercadoria = DataEntradaMercadoria,
            PrevisaoEntrega = PrevisaoEntrega,
            CondicaoPagamento = CondicaoPagamento.Trim(),
            Quantidade = ParseDecimal(Quantidade),
            CustoUnitario = ParseDecimal(CustoUnitario),
            FreteEstimado = ParseDecimal(FreteEstimado),
            ImpostosEstimados = ParseDecimal(ImpostosEstimados),
            TotalEstimado = TotalEstimado,
            CentroCusto = CentroCusto.Trim(),
            NivelAprovacao = NivelAprovacao.Trim(),
            AprovadorResponsavel = AprovadorResponsavel.Trim(),
            PodeGerarPedido = PodeGerarPedido,
            XmlImportadoPath = XmlImportadoPath.Trim(),
            XmlChaveAcesso = XmlChaveAcesso.Trim(),
            XmlFornecedor = XmlFornecedor.Trim(),
            XmlValorTotal = ParseDecimal(XmlValorTotal),
            Observacoes = Observacoes.Trim(),
            Status = statusOperacional
        };

        var submit = await _api.SubmitOperationAsync(_terminal, "compras-estoque", Codigo, draft);
        if (submit.Success)
        {
            Status = $"Aquisição gravada no servidor: {submit.ServerReference ?? submit.Detail}";
            return;
        }

        var path = await _outbox.SaveOperationAsync("compras-estoque", Codigo, draft);
        Status = $"API indisponível. Aquisição salva em contingência local: {path}. Motivo: {submit.Detail}";
    }

    private bool ValidarBase(string acao)
    {
        if (string.IsNullOrWhiteSpace(FornecedorParceiro)
            || string.IsNullOrWhiteSpace(ItemDescricao)
            || string.IsNullOrWhiteSpace(Categoria)
            || ParseDecimal(Quantidade) <= 0
            || ParseDecimal(CustoUnitario) <= 0)
        {
            Status = $"Não é possível {acao}: informe fornecedor, item, categoria, quantidade e custo unitário.";
            return false;
        }

        if (!ProdutoFiscalmenteCompleto)
        {
            Status = $"Não é possível {acao}: complete o cadastro fiscal do produto.";
            return false;
        }

        return true;
    }

    private bool ValidarEntrada()
    {
        if (!ValidarBase("registrar entrada"))
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(NumeroDocumentoFornecedor) && string.IsNullOrWhiteSpace(XmlChaveAcesso))
        {
            Status = "Entrada bloqueada: informe número do documento do fornecedor ou importe o XML da NF-e.";
            return false;
        }

        if (DataEntradaMercadoria is null)
        {
            Status = "Entrada bloqueada: informe a data de entrada da mercadoria.";
            return false;
        }

        return true;
    }

    private void ImportarXml_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Selecionar XML de NF-e para entrada de mercadoria",
            Filter = "XML de NF-e (*.xml)|*.xml|Todos os arquivos (*.*)|*.*",
            Multiselect = false
        };

        if (dialog.ShowDialog(this) != true)
        {
            return;
        }

        try
        {
            var doc = XDocument.Load(dialog.FileName);
            XNamespace ns = doc.Root?.GetDefaultNamespace() ?? XNamespace.None;
            var infNfe = doc.Descendants(ns + "infNFe").FirstOrDefault();
            var emit = doc.Descendants(ns + "emit").FirstOrDefault();
            var total = doc.Descendants(ns + "ICMSTot").FirstOrDefault();
            var det = doc.Descendants(ns + "det").FirstOrDefault();

            XmlImportadoPath = dialog.FileName;
            XmlChaveAcesso = infNfe?.Attribute("Id")?.Value.Replace("NFe", string.Empty, StringComparison.OrdinalIgnoreCase) ?? string.Empty;
            XmlFornecedor = emit?.Element(ns + "xNome")?.Value ?? emit?.Element(ns + "CNPJ")?.Value ?? string.Empty;
            XmlValorTotal = total?.Element(ns + "vNF")?.Value ?? "0,00";

            if (det is not null)
            {
                ItemDescricao = det.Descendants(ns + "xProd").FirstOrDefault()?.Value ?? ItemDescricao;
                Ncm = det.Descendants(ns + "NCM").FirstOrDefault()?.Value ?? Ncm;
                Cest = det.Descendants(ns + "CEST").FirstOrDefault()?.Value ?? Cest;
                CfopSugerido = det.Descendants(ns + "CFOP").FirstOrDefault()?.Value ?? CfopSugerido;
                UnidadeComercial = det.Descendants(ns + "uCom").FirstOrDefault()?.Value ?? UnidadeComercial;
                GtinCodigoBarras = det.Descendants(ns + "cEAN").FirstOrDefault()?.Value ?? GtinCodigoBarras;
                Quantidade = det.Descendants(ns + "qCom").FirstOrDefault()?.Value ?? Quantidade;
                CustoUnitario = det.Descendants(ns + "vUnCom").FirstOrDefault()?.Value ?? CustoUnitario;
            }

            Tipo = "Entrada de mercadoria";
            Status = "XML importado e vinculado à entrada local.";
            AtualizarResumo();
        }
        catch (Exception ex)
        {
            Status = $"Não foi possível importar o XML: {ex.Message}";
        }
    }

    private void Novo_Click(object sender, RoutedEventArgs e)
    {
        NovoDocumento();
    }

    private void Fechar_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void NovoDocumento(bool clearFields = true)
    {
        Codigo = $"{EmpresaDestino}-{OrigemAquisicao}-{NormalizeCode(Tipo)}-{DateTime.Now:yyyyMMdd-HHmmss}";

        if (clearFields)
        {
            FornecedorParceiro = string.Empty;
            ItemDescricao = string.Empty;
            Categoria = string.Empty;
            UnidadeComercial = "UN";
            Ncm = string.Empty;
            Cest = string.Empty;
            CfopSugerido = "5102";
            OrigemFiscalItem = "0 - Nacional";
            GtinCodigoBarras = string.Empty;
            PesoDimensoes = string.Empty;
            Quantidade = "1";
            CustoUnitario = "0,00";
            FreteEstimado = "0,00";
            ImpostosEstimados = "0,00";
            AprovadorResponsavel = string.Empty;
            XmlImportadoPath = string.Empty;
            XmlChaveAcesso = string.Empty;
            XmlFornecedor = string.Empty;
            XmlValorTotal = "0,00";
            StatusAprovacao = "Rascunho";
            NumeroDocumentoFornecedor = string.Empty;
            SerieDocumentoFornecedor = string.Empty;
            DataEmissaoDocumento = null;
            DataEntradaMercadoria = DateTime.Today;
            PrevisaoEntrega = DateTime.Today.AddDays(7);
            CondicaoPagamento = "À vista";
            Observacoes = string.Empty;
        }

        Status = "Novo documento aquisitivo pronto para preenchimento.";
        AtualizarResumo();
    }

    private void AtualizarResumo()
    {
        OnPropertyChanged(nameof(TotalEstimadoLabel));
        OnPropertyChanged(nameof(CodigoProdutoSugerido));
        OnPropertyChanged(nameof(ControleEntrada));
        OnPropertyChanged(nameof(PodeGerarPedido));
        OnPropertyChanged(nameof(FluxoAprovacao));
        OnPropertyChanged(nameof(XmlResumo));
        OnPropertyChanged(nameof(ProdutoFiscalmenteCompleto));
        OnPropertyChanged(nameof(StatusCadastroProduto));
        OnPropertyChanged(nameof(EtapaOperacional));
    }

    private static string OnlyDigits(string value)
    {
        return new string((value ?? string.Empty).Where(char.IsDigit).ToArray());
    }

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

    private static string NormalizeCode(string value)
    {
        var chars = value.ToUpperInvariant().Where(char.IsLetterOrDigit).Take(10).ToArray();
        return chars.Length == 0 ? "DOC" : new string(chars);
    }

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
