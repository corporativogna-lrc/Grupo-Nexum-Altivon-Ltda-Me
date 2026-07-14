/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly DesktopApiClient _apiClient = new();

    private string _environmentStatus = "Aguardando conexão";
    private string _environmentDetail = "Terminal preparado para validar servidor, Cloudflare e modo de contingência.";
    private string _selectedModuleTitle = "ERP/PDV local pronto";
    private string _selectedModuleDetail = "Selecione uma ação para operar loja física, caixa, fiscal, estoque ou gestão.";
    private string _lastUpdatedLabel = "Atualizado agora";
    private string _salesMetricValue = "PDV";
    private string _fiscalMetricValue = "NFC-e";
    private string _groupMetricValue = "Loja";
    private string _integrationMetricValue = "API";
    private string _connectionModeLabel = "Validando conexão";
    private string _pdvOperationalNote = "Contingência preparada para registrar venda local e sincronizar quando a API estiver disponível.";
    private string _serverDatabaseLabel = "Banco interno local:3309";
    private bool _showSalesDashboard = true;
    private bool _showFinanceDashboard = true;
    private bool _showPurchaseDashboard = true;
    private bool _showFiscalDashboard = true;
    private bool _showLogisticsDashboard = true;

    public TerminalProfile Terminal { get; } = new();
    public ObservableCollection<DesktopModule> Modules { get; } = new();
    public ObservableCollection<WorkstationAction> PdvActions { get; } = new();
    public ObservableCollection<OrganizationNode> OrganizationUnits { get; } = new();

    public string EnvironmentStatus
    {
        get => _environmentStatus;
        set => SetField(ref _environmentStatus, value);
    }

    public string EnvironmentDetail
    {
        get => _environmentDetail;
        set => SetField(ref _environmentDetail, value);
    }

    public string SelectedModuleTitle
    {
        get => _selectedModuleTitle;
        set => SetField(ref _selectedModuleTitle, value);
    }

    public string SelectedModuleDetail
    {
        get => _selectedModuleDetail;
        set => SetField(ref _selectedModuleDetail, value);
    }

    public string LastUpdatedLabel
    {
        get => _lastUpdatedLabel;
        set => SetField(ref _lastUpdatedLabel, value);
    }

    public string SalesMetricValue
    {
        get => _salesMetricValue;
        set => SetField(ref _salesMetricValue, value);
    }

    public string FiscalMetricValue
    {
        get => _fiscalMetricValue;
        set => SetField(ref _fiscalMetricValue, value);
    }

    public string GroupMetricValue
    {
        get => _groupMetricValue;
        set => SetField(ref _groupMetricValue, value);
    }

    public string IntegrationMetricValue
    {
        get => _integrationMetricValue;
        set => SetField(ref _integrationMetricValue, value);
    }

    public string ConnectionModeLabel
    {
        get => _connectionModeLabel;
        set => SetField(ref _connectionModeLabel, value);
    }

    public string PdvOperationalNote
    {
        get => _pdvOperationalNote;
        set => SetField(ref _pdvOperationalNote, value);
    }

    public string ServerDatabaseLabel
    {
        get => _serverDatabaseLabel;
        set => SetField(ref _serverDatabaseLabel, value);
    }

    public bool ShowSalesDashboard
    {
        get => _showSalesDashboard;
        set => SetField(ref _showSalesDashboard, value);
    }

    public bool ShowFinanceDashboard
    {
        get => _showFinanceDashboard;
        set => SetField(ref _showFinanceDashboard, value);
    }

    public bool ShowPurchaseDashboard
    {
        get => _showPurchaseDashboard;
        set => SetField(ref _showPurchaseDashboard, value);
    }

    public bool ShowFiscalDashboard
    {
        get => _showFiscalDashboard;
        set => SetField(ref _showFiscalDashboard, value);
    }

    public bool ShowLogisticsDashboard
    {
        get => _showLogisticsDashboard;
        set => SetField(ref _showLogisticsDashboard, value);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        LoadPdvActions();
        LoadEnterpriseModules();
        LoadOrganizationTree();

        SelectedModuleTitle = PdvActions[0].Title;
        SelectedModuleDetail = PdvActions[0].Detail;
        Loaded += async (_, _) => await RefreshStatusAsync();
    }

    private void LoadPdvActions()
    {
        PdvActions.Add(new WorkstationAction
        {
            Title = "Abrir caixa",
            Detail = "Inicializa operador, terminal, loja, saldo de abertura e vínculo financeiro.",
            Route = "pdv-caixa-abertura",
            Accent = "#22C55E",
            Status = "Operação via PDV local"
        });
        PdvActions.Add(new WorkstationAction
        {
            Title = "Venda balcão",
            Detail = "Busca item por código de barras, QR Code, SKU, nome ou categoria.",
            Route = "pdv-venda",
            Accent = "#38BDF8",
            Status = "Base conectada à API"
        });
        PdvActions.Add(new WorkstationAction
        {
            Title = "Pagamento",
            Detail = "Dinheiro, PIX, cartão, voucher e registro de baixa no financeiro.",
            Route = "pdv-pagamento",
            Accent = "#8B5CF6",
            Status = "Registro na venda local"
        });
        PdvActions.Add(new WorkstationAction
        {
            Title = "Fiscal local",
            Detail = "NFC-e/SAT/MFe, contingência e emissão manual quando necessário.",
            Route = "pdv-fiscal",
            Accent = "#F59E0B",
            Status = "Roteamento fiscal local"
        });
        PdvActions.Add(new WorkstationAction
        {
            Title = "Troca/devolução",
            Detail = "Localiza pedido, cliente, item vendido, estoque e financeiro reverso.",
            Route = "pdv-devolucao",
            Accent = "#FB7185",
            Status = "Bloqueado sem endpoint operacional"
        });
        PdvActions.Add(new WorkstationAction
        {
            Title = "Fechar caixa",
            Detail = "Concilia recebimentos, sangrias, suprimentos, fiscal e relatório local.",
            Route = "pdv-caixa-fechamento",
            Accent = "#EAB308",
            Status = "Conciliação na tela PDV"
        });
    }

    private void LoadEnterpriseModules()
    {
        Modules.Add(new DesktopModule
        {
            Title = "Operação Comercial",
            Detail = "Vendas, pedidos, checkout, cliente, catálogo, loja física e e-commerce integrados.",
            Status = "Conectado ao ciclo comercial",
            Accent = "#38BDF8",
            ActionText = "Abrir vendas"
        });
        Modules.Add(new DesktopModule
        {
            Title = "Financeiro",
            Detail = "Contas a receber, contas a pagar, caixa, conciliação e vínculos com pedido/compra.",
            Status = "Amarração em evolução",
            Accent = "#22C55E",
            ActionText = "Abrir financeiro"
        });
        Modules.Add(new DesktopModule
        {
            Title = "Fiscal",
            Detail = "Roteamento por empresa do grupo, menor custo, maior margem e emissão fiscal.",
            Status = "Motor fiscal conectado",
            Accent = "#F59E0B",
            ActionText = "Abrir fiscal"
        });
        Modules.Add(new DesktopModule
        {
            Title = "Compras e Estoque",
            Detail = "Solicitação, cotação, pedido, entrada, QR Code, código de barras e saldo físico/fiscal.",
            Status = "Módulo base ativo",
            Accent = "#EAB308",
            ActionText = "Abrir compras"
        });
        Modules.Add(new DesktopModule
        {
            Title = "Logística",
            Detail = "Coleta, rastreio, entrega, status do pedido e comunicação ao cliente.",
            Status = "Integração em preparação",
            Accent = "#8B5CF6",
            ActionText = "Abrir logística"
        });
        Modules.Add(new DesktopModule
        {
            Title = "Governança",
            Detail = "Perfis, trilha de auditoria, empresas, usuários e controle multi-empresarial.",
            Status = "Estrutura enterprise",
            Accent = "#FB7185",
            ActionText = "Abrir governança"
        });
    }

    private void LoadOrganizationTree()
    {
        var holding = new OrganizationNode
        {
            Name = "Grupo Nexum Altivon Ltda. Me.",
            Domain = "nexumaltivon.com.br",
            Role = "Matriz / Holding / Gestora Corporativa",
            TaxSituation = "Centralizadora, fiscalizadora e emitente preferencial conforme menor custo operacional.",
            EmailPattern = "Corporativo, vendas, compras, financeiro e fiscal"
        };

        holding.Children.Add(CreateFilial("Geração Top Mais", "geracaotopmais.com.br", "Tecnologia / Loja física e online"));
        holding.Children.Add(CreateFilial("Moda Mim", "modamim.com.br", "Moda e acessórios"));
        holding.Children.Add(CreateFilial("Ghrann Tur", "ghranntur.com.br", "Turismo e serviços"));
        holding.Children.Add(CreateFilial("Chronnus Relojoaria", "chronnusrelojoaria.com.br", "Relojoaria"));
        holding.Children.Add(CreateFilial("Estrutural Line", "estruturaline.com.br", "Construção e estrutura"));
        holding.Children.Add(CreateFilial("Gran Festas", "granfestas.com.br", "Festas e eventos"));

        OrganizationUnits.Add(holding);
    }

    private async Task RefreshStatusAsync()
    {
        ServerDatabaseLabel = $"Banco interno {Terminal.ServerAddress}:{Terminal.DatabasePort}";

        var result = await _apiClient.CheckHealthAsync(Terminal);
        EnvironmentStatus = result.Status;
        EnvironmentDetail = result.Detail;
        LastUpdatedLabel = $"Atualizado em {result.CheckedAt:dd/MM/yyyy HH:mm}";
        ConnectionModeLabel = result.LocalHealthy ? "Conexão local ativa" : result.PublicHealthy ? "Público ativo" : "Contingência";
        IntegrationMetricValue = result.LocalHealthy || result.PublicHealthy ? "OK" : "OFF";
        PdvOperationalNote = result.LocalHealthy
            ? "PDV pode operar online contra o servidor principal. Próxima etapa: tela de venda com carrinho local e envio para a API."
            : "PDV deve permanecer em modo de contingência até a API responder novamente.";
    }

    private async void RefreshStatus_Click(object sender, RoutedEventArgs e)
    {
        await RefreshStatusAsync();
    }

    private void OpenWorkspace_Click(object sender, RoutedEventArgs e)
    {
        var title = sender switch
        {
            MenuItem item => CleanMenuHeader(item.Header),
            Button button => CleanMenuHeader(button.Content),
            _ => "Painel Genesis local"
        };

        var area = sender is MenuItem { Parent: MenuItem parent }
            ? CleanMenuHeader(parent.Header)
            : "GenesisGest.Net";
        var detail = BuildWorkspaceDetail(area, title);

        SelectedModuleTitle = title;
        SelectedModuleDetail = detail;

        if (IsMasterDataWorkspace(title))
        {
            var masterDataWindow = new CorporateMasterDataWindow(title);
            masterDataWindow.ShowDialog();
            return;
        }

        if (IsFinancialWorkspace(title))
        {
            var financialWindow = new FinancialLedgerWindow(title);
            financialWindow.ShowDialog();
            return;
        }

        if (IsProcurementWorkspace(title))
        {
            var procurementWindow = new ProcurementWindow(title);
            procurementWindow.ShowDialog();
            return;
        }

        if (IsFiscalWorkspace(title))
        {
            var fiscalWindow = new FiscalRoutingWindow(title);
            fiscalWindow.ShowDialog();
            return;
        }

        if (IsLogisticsWorkspace(title))
        {
            var logisticsWindow = new LogisticsWindow(title);
            logisticsWindow.ShowDialog();
            return;
        }

        var window = new ModuleWorkspaceWindow(title, area, detail, Terminal);
        window.ShowDialog();
    }

    private void OpenPdvSale_Click(object sender, RoutedEventArgs e)
    {
        SelectedModuleTitle = "PDV / Venda de balcão";
        SelectedModuleDetail = "Janela separada para venda física, caixa, pagamento, itens, cliente e contingência local.";

        var window = new PdvSaleWindow(Terminal);

        window.ShowDialog();
    }

    private void OpenManualNfe_Click(object sender, RoutedEventArgs e)
    {
        var window = new ManualNfeWindow();

        window.ShowDialog();
    }

    private void ModuleCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.Tag is not DesktopModule module)
        {
            return;
        }

        SelectedModuleTitle = module.Title;
        SelectedModuleDetail = $"{module.Detail} ({module.ActionText})";
    }

    private void WorkstationAction_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement element || element.Tag is not WorkstationAction action)
        {
            return;
        }

        SelectedModuleTitle = action.Title;
        SelectedModuleDetail = $"{action.Detail} Código local: {action.Route}.";

        if (action.Route is "pdv-venda" or "pdv-pagamento" or "pdv-caixa-abertura" or "pdv-caixa-fechamento")
        {
            var window = new PdvSaleWindow(Terminal)
            {
                Owner = this
            };

            window.ShowDialog();
            return;
        }

        if (action.Route is "pdv-fiscal")
        {
            var fiscalWindow = new FiscalRoutingWindow("Empresa emissora automática")
            {
                Owner = this
            };
            fiscalWindow.ShowDialog();
        }
    }

    private void FocusTerminal_Click(object sender, RoutedEventArgs e)
    {
        SelectedModuleTitle = "Terminal e conexões";
        SelectedModuleDetail = "Configuração local de loja, operador, API do servidor, API pública, banco 3309, impressora e contingência.";
    }

    private void OpenNetworkSettings_Click(object sender, RoutedEventArgs e)
    {
        var window = new NetworkSettingsWindow(Terminal)
        {
            Owner = this
        };

        window.ShowDialog();
        ServerDatabaseLabel = $"Banco interno {Terminal.ServerAddress}:{Terminal.DatabasePort}";
    }

    private void OpenOutboxFolder_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var outbox = new LocalOutboxService();
            Directory.CreateDirectory(outbox.BaseDirectory);
            Process.Start(new ProcessStartInfo
            {
                FileName = outbox.BaseDirectory,
                UseShellExecute = true
            });
        }
        catch
        {
            SelectedModuleTitle = "Contingência local";
            SelectedModuleDetail = "Não foi possível abrir a pasta local agora. O caminho de contingência permanece registrado no serviço do PDV.";
        }
    }

    private static OrganizationNode CreateFilial(string name, string domain, string role)
    {
        var filial = new OrganizationNode
        {
            Name = name,
            Domain = domain,
            Role = role,
            TaxSituation = "Parametrização fiscal por regime, CFOP, ST e emitente preferencial.",
            EmailPattern = "Financeiro, vendas, compras, fiscal e atendimento"
        };

        filial.Children.Add(new OrganizationNode
        {
            Name = "Terminal físico / PDV",
            Domain = domain,
            Role = "Caixa, balcão, estoque local e atendimento",
            TaxSituation = "Venda local com emissão fiscal e sincronização com servidor principal.",
            EmailPattern = "Operador, gerente, fiscal, estoque e suporte"
        });

        return filial;
    }

    private static string CleanMenuHeader(object? header)
    {
        return (header?.ToString() ?? "GenesisGest.Net").Replace("_", string.Empty).Trim();
    }

    private static string BuildWorkspaceDetail(string area, string title)
    {
        return title switch
        {
            "Contas a pagar" => "Tela financeira para títulos a pagar, aprovação, vencimento, baixa, centro de custo, fornecedor e trilha de auditoria.",
            "Contas a receber" => "Tela financeira para recebíveis, vínculo com venda, cliente, cobrança, baixa, conciliação e status de crédito.",
            "Solicitação de compra" => "Tela de suprimentos para requisição corporativa, centro de custo, aprovação, necessidade e origem da aquisição.",
            "Cotação com fornecedores" => "Tela de compras para comparar fornecedores, prazos, custos, origem e melhor condição de aquisição.",
            "Pedido de compra" => "Tela de compras para gerar pedido, parcelas, entrada prevista, vínculo fiscal e acompanhamento de recebimento.",
            "Entrada de mercadoria" => "Tela de estoque para receber item, alimentar saldo físico/fiscal, lote, custo, código de barras e QR Code.",
            "Empresa emissora automática" => "Tela fiscal para selecionar a melhor empresa emissora conforme custo, margem, regime e regra fiscal do grupo.",
            "Coleta" or "Entrega" or "Rastreamento" => "Tela logística para organizar coleta, rota, transportadora, status do pedido e comunicação com cliente.",
            "RBAC e perfis" => "Tela de governança para configurar usuários, permissões, níveis de acesso e segregação entre Nexum e Genesis.",
            _ => $"Tela operacional de {area} para registrar, consultar e manter dados do módulo {title} dentro do GenesisGest.Net."
        };
    }

    private static bool IsFinancialWorkspace(string title)
    {
        return title is "Contas a pagar"
            or "Contas a receber"
            or "Caixa e conciliação"
            or "Tesouraria"
            or "Alçadas de aprovação"
            or "Conciliação bancária";
    }

    private static bool IsMasterDataWorkspace(string title)
    {
        return title is "Clientes"
            or "Fornecedores"
            or "Produtos e serviços"
            or "Categorias e subcategorias"
            or "Empresas do grupo"
            or "Centros de custo"
            or "Usuários, perfis e permissões"
            or "Colaboradores / RH / folha"
            or "Contratações"
            or "Demissões"
            or "Cargos e departamentos"
            or "Ponto e jornada";
    }

    private static bool IsProcurementWorkspace(string title)
    {
        return title is "Solicitação de compra"
            or "Cotação com fornecedores"
            or "Pedido de compra"
            or "Entrada de mercadoria"
            or "Dropshipping e parcerias"
            or "Devolução a fornecedor";
    }

    private static bool IsFiscalWorkspace(string title)
    {
        return title is "Empresa emissora automática"
            or "Regras fiscais do grupo"
            or "SPED / EFD / REINF"
            or "Tributos e retenções"
            or "Fiscal local";
    }

    private static bool IsLogisticsWorkspace(string title)
    {
        return title is "Coleta"
            or "Entrega"
            or "Rastreamento"
            or "Transportadoras"
            or "Status do pedido"
            or "Comunicação ao cliente";
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
