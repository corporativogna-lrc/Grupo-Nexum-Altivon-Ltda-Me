using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Net.Http;
using System.Windows;
using NexumAltivon.Desktop.Models;

namespace NexumAltivon.Desktop;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private const string PortalUrl = "http://localhost:3000";
    private const string ApiBaseUrl = "http://localhost:5000";

    private string _environmentStatus = "Aguardando conexao";
    private string _environmentDetail = "Os blocos de gestao local e integrações estao preparados para receber as chaves.";
    private string _selectedModuleTitle = "ERP local pronto";
    private string _selectedModuleDetail = "Selecione um modulo para ver a proposta operacional do desktop.";
    private string _lastUpdatedLabel = "Atualizado agora";
    private string _salesMetricValue = "01";
    private string _fiscalMetricValue = "04";
    private string _groupMetricValue = "Grupo";
    private string _integrationMetricValue = "08";

    public ObservableCollection<DesktopModule> Modules { get; } = new();
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

    public event PropertyChangedEventHandler? PropertyChanged;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;

        Modules.Add(new DesktopModule
        {
            Title = "Operacao Comercial",
            Detail = "Fluxo para vendas, pedidos, checkout e atendimento ao cliente com foco em conversao.",
            Status = "Pronto para operar",
            Accent = "#38BDF8",
            ActionText = "Abrir vendas"
        });
        Modules.Add(new DesktopModule
        {
            Title = "Gestao Fiscal",
            Detail = "NFe, emissoes, pendencias, roteamento tributario e acompanhamento de documentos.",
            Status = "Homologacao guiada",
            Accent = "#F59E0B",
            ActionText = "Abrir fiscal"
        });
        Modules.Add(new DesktopModule
        {
            Title = "Grupo e Empresas",
            Detail = "Visao societaria, unidades, centros de custo e controle local da empresa.",
            Status = "Governanca ativa",
            Accent = "#22C55E",
            ActionText = "Abrir grupo"
        });
        Modules.Add(new DesktopModule
        {
            Title = "Integracoes",
            Detail = "Ambientes prontos para receber chaves, tokens, webhooks e autorizações externas.",
            Status = "Aguardando chaves",
            Accent = "#8B5CF6",
            ActionText = "Abrir integracoes"
        });

        var holding = new OrganizationNode
        {
            Name = "Grupo Nexum Altivon Ltda. Me.",
            Domain = "www.nexumaltivon.com",
            Role = "Matriz / Holding / Gestora Corporativa",
            TaxSituation = "Situação tributária: centralizadora, fiscalizadora e emitente principal sob análise de custo.",
            EmailPattern = "5 caixas-padrao: Corporativo, Vendas, Compras, Contabilidade e RecursosHumanos"
        };

        holding.Children.Add(CreateFilial(
            "Geracao Top Mais MEI",
            "www.geracaotopmais.com.br",
            "Filial - Informatica e Tecnologia / Loja-01"));
        holding.Children.Add(CreateFilial(
            "Moda Mim MEI",
            "www.modamim.com.br",
            "Filial - Vestuario Feminino / Loja-02"));
        holding.Children.Add(CreateFilial(
            "Ghrann Tur MEI",
            "www.ghranntur.com.br",
            "Filial - Turismo e Viagens / Loja-03"));
        holding.Children.Add(CreateFilial(
            "Chornos Relojoaria MEI",
            "www.chornosrelojoaria.com.br",
            "Filial - Relojoaria / Loja-04"));
        holding.Children.Add(CreateFilial(
            "Estrutural Line MEI",
            "www.estruturaline.com.br",
            "Filial - Construcao / Loja-05"));
        holding.Children.Add(CreateFilial(
            "Ghrann Fest Festas MEI",
            "www.ghrannfestfestas.com.br",
            "Filial - Festas e Eventos / Loja-06"));

        OrganizationUnits.Add(holding);

        SelectedModuleTitle = Modules[0].Title;
        SelectedModuleDetail = Modules[0].Detail;
        Loaded += async (_, _) => await RefreshStatusAsync();
    }

    private async Task RefreshStatusAsync()
    {
        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(ApiBaseUrl) };
            using var response = await http.GetAsync("/health");

            if (response.IsSuccessStatusCode)
            {
                EnvironmentStatus = "Conexao local ok";
                EnvironmentDetail = "API acessivel e pronta para alimentar o desktop com dados operacionais.";
            }
            else
            {
                EnvironmentStatus = "API responde com alerta";
                EnvironmentDetail = $"A API respondeu com status {(int)response.StatusCode}. O desktop segue carregado para a operacao.";
            }
        }
        catch
        {
            EnvironmentStatus = "Aguardando API";
            EnvironmentDetail = "Nao foi possivel confirmar a API local agora. O painel permanece pronto para o uso corporativo.";
        }

        LastUpdatedLabel = $"Atualizado em {DateTime.Now:dd/MM/yyyy HH:mm}";
    }

    private async void RefreshStatus_Click(object sender, RoutedEventArgs e)
    {
        await RefreshStatusAsync();
    }

    private void OpenPortal_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = PortalUrl,
                UseShellExecute = true
            });
        }
        catch
        {
            EnvironmentStatus = "Portal indisponivel";
            EnvironmentDetail = "Nao foi possivel abrir o portal no navegador padrao.";
        }
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

    private static OrganizationNode CreateFilial(string name, string domain, string role)
    {
        var filial = new OrganizationNode
        {
            Name = name,
            Domain = domain,
            Role = role,
            TaxSituation = "Situação tributária: parametrização fiscal pendente de regime, CFOP, ST e emitente preferencial.",
            EmailPattern = $"5 caixas por dominio: Financeiro@{domain}, Vendas@{domain}, Compras@{domain}, Contabilidade@{domain}, RecursosHumanos@{domain}"
        };

        filial.Children.Add(new OrganizationNode
        {
            Name = "Parceiros de negocios",
            Domain = domain,
            Role = "Vinculacao direta ao dominio da filial",
            TaxSituation = "Situação tributária: depende do cadastro de cada parceiro e do vínculo com a filial responsável.",
            EmailPattern = "Cada parceiro pode receber ate 5 contas por dominio registrado"
        });

        return filial;
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
