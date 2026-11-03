/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7226 Data: 16/08/2026
 */

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace NexumAltivon.Desktop;

public sealed class DevelopmentMapNode
{
    public DevelopmentMapNode(string name, string status, Brush statusBrush)
    {
        Name = name;
        Status = status;
        StatusBrush = statusBrush;
    }

    public string Name { get; }
    public string Status { get; }
    public Brush StatusBrush { get; }
    public ObservableCollection<DevelopmentMapNode> Children { get; } = new();
}

public partial class MapaDesenvolvimentoWindow : Window
{
    public MapaDesenvolvimentoWindow()
    {
        ProductVersion = typeof(MainWindow).Assembly.GetName().Version?.ToString(4)
            ?? "Versão do assembly indisponível";
        Evidence = "Fonte oficial: D:\\Nexum Altivon\\NexumAltivon.com. As 40 consultas do lote possuem rota GET oficial e permissão declarada; elas não são contabilizadas como CRUD. O aceite de produção permanece bloqueado até superar 260 tabelas com escrita, auditoria, releitura e validação ponta a ponta.";
        BuildNodes();
        InitializeComponent();
        DataContext = this;
    }

    public string ProductVersion { get; }
    public string Evidence { get; }
    public ObservableCollection<DevelopmentMapNode> Nodes { get; } = new();

    private void BuildNodes()
    {
        var active = new SolidColorBrush(Color.FromRgb(47, 158, 68));
        var readOnly = new SolidColorBrush(Color.FromRgb(215, 181, 40));
        var blocked = new SolidColorBrush(Color.FromRgb(217, 92, 75));

        var operation = Add("Operação / PDV", "janela própria");
        operation.Children.Add(new DevelopmentMapNode("PDV / Venda de balcão", "janela própria", active));
        operation.Children.Add(new DevelopmentMapNode("Caixa / pagamento / contingência", "janela própria", active));

        var finance = Add("Financeiro", "telas separadas");
        finance.Children.Add(new DevelopmentMapNode("Contas a pagar", "CRUD + PDF", active));
        finance.Children.Add(new DevelopmentMapNode("Contas a receber", "CRUD + PDF", active));
        finance.Children.Add(new DevelopmentMapNode("Baixas financeiras", "operação separada", active));
        finance.Children.Add(new DevelopmentMapNode("Lançamentos contábeis", "persistência + releitura", active));
        finance.Children.Add(new DevelopmentMapNode("Fechamento contábil", "persistência + releitura", active));
        finance.Children.Add(new DevelopmentMapNode("Consultas financeiras do lote", "41 rotas GET", readOnly));

        var governance = Add("Governança", "RBAC e auditoria");
        governance.Children.Add(new DevelopmentMapNode("Usuários, perfis e permissões", "API oficial", active));
        governance.Children.Add(new DevelopmentMapNode("Tenants / workflow / auditoria", "API oficial", active));
        governance.Children.Add(new DevelopmentMapNode("Aceite de produção", "bloqueado: meta >260", blocked));

        var master = Add("Master Data / Site", "janelas próprias");
        master.Children.Add(new DevelopmentMapNode("Clientes, fornecedores, categorias e itens", "CRUD separado", active));
        master.Children.Add(new DevelopmentMapNode("Lojas, mídias e perfis públicos", "CRUD separado", active));
        master.Children.Add(new DevelopmentMapNode("Consultas de catálogo e site", "41 rotas GET", readOnly));

        var ops = Add("MES / OPS", "janelas próprias");
        ops.Children.Add(new DevelopmentMapNode("Ativos, ordens, produção e manutenção", "API oficial", active));

        var source = Add("LURCO432", "fonte de avaliação");
        source.Children.Add(new DevelopmentMapNode("Arquivos Utilizados", "somente código validado", active));
        source.Children.Add(new DevelopmentMapNode("Arquivos Descartados", "não é dependência de runtime", readOnly));
    }

    private DevelopmentMapNode Add(string name, string status)
    {
        var node = new DevelopmentMapNode(name, status, new SolidColorBrush(Color.FromRgb(215, 181, 40)));
        Nodes.Add(node);
        return node;
    }
}
