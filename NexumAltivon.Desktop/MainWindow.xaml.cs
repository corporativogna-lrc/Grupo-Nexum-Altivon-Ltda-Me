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
using NexumAltivon.Desktop.Models;
using NexumAltivon.Desktop.Services;

namespace NexumAltivon.Desktop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private const string PortalUrl = "https://nexumaltivon.com.br/dashboard";
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
    private string _serverDatabaseLabel = "Banco interno 192.168.1.72:3309";

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
            Status = "Pronto para implementação"
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
            Status = "Aguardando gateways"
        });
        PdvActions.Add(new WorkstationAction
        {
            Title = "Fiscal local",
            Detail = "NFC-e/SAT/MFe, contingência e emissão manual quando necessário.",
            Route = "pdv-fiscal",
            Accent = "#F59E0B",
            Status = "Preparado para certificado"
        });
        PdvActions.Add(new WorkstationAction
        {
            Title = "Troca/devolução",
            Detail = "Localiza pedido, cliente, item vendido, estoque e financeiro reverso.",
            Route = "pdv-devolucao",
            Accent = "#FB7185",
            Status = "Fila de desenvolvimento"
        });
        PdvActions.Add(new WorkstationAction
        {
            Title = "Fechar caixa",
            Detail = "Concilia recebimentos, sangrias, suprimentos, fiscal e relatório local.",
            Route = "pdv-caixa-fechamento",
            Accent = "#EAB308",
            Status = "Fila de desenvolvimento"
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

    private void OpenPortal_Click(object sender, RoutedEventArgs e)
    {
        OpenUrl(PortalUrl);
    }

    private void OpenManualNfe_Click(object sender, RoutedEventArgs e)
    {
        var window = new ManualNfeWindow
        {
            Owner = this
        };

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
        SelectedModuleDetail = $"{action.Detail} Rota interna: {action.Route}.";

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
            OpenManualNfe_Click(sender, e);
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

    private static void OpenUrl(string url)
    {
        try
        {
            var chromePaths = new[]
            {
                Environment.GetEnvironmentVariable("ProgramFiles") is string programFiles
                    ? Path.Combine(programFiles, "Google", "Chrome", "Application", "chrome.exe")
                    : null,
                Environment.GetEnvironmentVariable("ProgramFiles(x86)") is string programFilesX86
                    ? Path.Combine(programFilesX86, "Google", "Chrome", "Application", "chrome.exe")
                    : null
            };

            var chromePath = chromePaths.FirstOrDefault(File.Exists);

            if (!string.IsNullOrWhiteSpace(chromePath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = chromePath,
                    Arguments = $"\"{url}\"",
                    UseShellExecute = false
                });
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }
        catch
        {
            // A interface mantém o status operacional mesmo se o navegador local falhar.
        }
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
