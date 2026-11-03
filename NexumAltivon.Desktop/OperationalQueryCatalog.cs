/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7247 Data: 30/10/2026
 */

namespace NexumAltivon.Desktop;

public static class OperationalQueryCatalog
{
    private static readonly IReadOnlyDictionary<string, OperationalQueryDefinition> Definitions =
        new Dictionary<string, OperationalQueryDefinition>(StringComparer.OrdinalIgnoreCase)
        {
            ["compras-painel"] = Definition("compras-painel", "Painel de compras", "Compras", "compras.painel", "Gerente", "/api/compras/painel"),
            ["genesis-contas-pagar"] = Definition("genesis-contas-pagar", "Contas a pagar Genesis", "Financeiro", "ContaPagar", "Financeiro", "/api/erp/genesis/financeiro/contas-pagar"),
            ["genesis-contas-receber"] = Definition("genesis-contas-receber", "Contas a receber Genesis", "Financeiro", "ContaReceber", "Financeiro", "/api/erp/genesis/financeiro/contas-receber"),
            ["genesis-boletos"] = Definition("genesis-boletos", "Boletos financeiros", "Financeiro", "Boleto", "Financeiro", "/api/erp/genesis/financeiro/boletos"),
            ["genesis-referencias-financeiras"] = Definition("genesis-referencias-financeiras", "Referências financeiras", "Financeiro", "FinanceReferencia", "Financeiro", "/api/erp/genesis/financeiro/referencias"),
            ["genesis-vendas-pdv"] = Definition("genesis-vendas-pdv", "Vendas do PDV Genesis", "Operação / PDV", "VendaPDV", "Gerente", "/api/erp/genesis/pdv/vendas?limite=100"),
            ["pedidos"] = Definition("pedidos", "Consulta de pedidos", "Operação / Vendas", "Pedido", "Gerente", "/api/pedidos"),
            ["pedido-status"] = Definition("pedido-status", "Status do pedido", "Logística / Cliente", "Acompanhamento identificado", "E-mail ou CPF/CNPJ do comprador", "/api/pedidos/acompanhar"),
            ["genesis-rh-resumo"] = Definition("genesis-rh-resumo", "Resumo de RH Genesis", "HCM / RH", "RH.resumo", "Gerente", "/api/erp/genesis/rh/resumo"),
            ["genesis-rh-colaboradores"] = Definition("genesis-rh-colaboradores", "Colaboradores Genesis", "HCM / RH", "Colaborador", "Gerente", "/api/erp/genesis/rh/colaboradores"),
            ["genesis-rh-referencias"] = Definition("genesis-rh-referencias", "Referências de RH Genesis", "HCM / RH", "RH.referencias", "Gerente", "/api/erp/genesis/rh/referencias"),
            ["ops-ativos"] = Definition("ops-ativos", "Ativos operacionais", "MES / OPS", "OpsAtivo", "Gerente", "/api/ops/ativos"),
            ["ops-ordens-servico"] = Definition("ops-ordens-servico", "Ordens de serviço", "MES / OPS", "OpsOrdemServico", "Gerente", "/api/ops/ordens-servico"),
            ["ops-apontamentos"] = Definition("ops-apontamentos", "Apontamentos de produção", "MES / OPS", "OpsProducaoApontamento", "Gerente", "/api/ops/producao/apontamentos"),
            ["ops-manutencao"] = Definition("ops-manutencao", "Manutenção operacional", "MES / OPS", "OpsManutencao", "Gerente", "/api/ops/manutencao"),
            ["tenants"] = Definition("tenants", "Empresas e tenants", "Governança", "Tenant", "Admin", "/api/tenants"),
            ["workflow-definicoes"] = Definition("workflow-definicoes", "Definições de workflow", "Governança", "WorkflowDefinicao", "Gerente", "/api/workflows/definicoes"),
            ["workflow-instancias"] = Definition("workflow-instancias", "Instâncias de workflow", "Governança", "WorkflowInstancia", "Gerente", "/api/workflows/instancias"),
            ["admin-usuarios"] = Definition("admin-usuarios", "Usuários administrativos", "Governança", "Usuario", "Admin", "/api/admin/usuarios"),
            ["perfis"] = Definition("perfis", "Perfis de acesso", "Governança", "Perfil", "Admin", "/api/perfis"),
            ["permissoes"] = Definition("permissoes", "Permissões de acesso", "Governança", "Permissao", "Admin", "/api/permissoes"),
            ["auditoria"] = Definition("auditoria", "Trilha de auditoria", "Governança", "LogAuditoria", "Admin", "/api/auditoria"),
            ["sod-regras"] = Definition("sod-regras", "Regras de segregação de funções", "Governança", "SoD.regra", "Admin", "/api/sod/regras"),
            ["pessoas"] = Definition("pessoas", "Pessoas PF/PJ", "Master Data", "Pessoa", "Gerente", "/api/pessoas"),
            ["centros-custo"] = Definition("centros-custo", "Centros de custo", "Master Data", "CentroCusto", "Gerente", "/api/centros-custo"),
            ["itens-servico"] = Definition("itens-servico", "Itens de serviço", "Master Data", "ItemServico", "Gerente", "/api/itens-servico"),
            ["dashboard-completo"] = Definition("dashboard-completo", "Dashboard executivo completo", "Cockpit executivo", "dashboard.completo", "Gerente", "/api/admin/dashboard/completo"),
            ["dashboard-kpis"] = Definition("dashboard-kpis", "KPIs executivos", "Cockpit executivo", "dashboard.kpis", "Gerente", "/api/admin/dashboard/kpis"),
            ["relatorio-vendas"] = Definition("relatorio-vendas", "Relatório de vendas", "Cockpit executivo", "relatorio.vendas", "Gerente", "/api/relatorios/vendas"),
            ["lojas"] = Definition("lojas", "Lojas do grupo", "Master Data", "Loja", "Público", "/api/lojas"),
            ["site-configuracoes-publico"] = Definition("site-configuracoes-publico", "Configurações públicas do site", "Site / conteúdo", "ConfiguracaoSistema", "Público", "/api/site/configuracoes/publico"),
            ["site-lojas"] = Definition("site-lojas", "Páginas públicas das lojas", "Site / conteúdo", "SitePerfilPublico.lojas", "Público", "/api/site/lojas"),
            ["site-parceiros"] = Definition("site-parceiros", "Páginas públicas de parceiros", "Site / conteúdo", "SitePerfilPublico.parceiros", "Público", "/api/site/parceiros"),
            ["site-perfis-publicos"] = Definition("site-perfis-publicos", "Perfis públicos administráveis", "Site / conteúdo", "SitePerfilPublico", "Gerente", "/api/site/perfis-publicos"),
            ["site-midias"] = Definition("site-midias", "Biblioteca de mídias", "Site / conteúdo", "SiteMidia", "Gerente", "/api/site/midias"),
            ["categorias"] = Definition("categorias", "Categorias de catálogo", "Master Data", "Categoria", "Público", "/api/categorias"),
            ["produtos"] = Definition("produtos", "Produtos de catálogo", "Master Data", "Produto", "Público", "/api/produtos"),
            ["clientes"] = Definition("clientes", "Clientes administrativos", "Master Data", "Cliente", "Gerente", "/api/clientes"),
            ["fornecedores"] = Definition("fornecedores", "Fornecedores", "Master Data", "Fornecedor", "Gerente", "/api/fornecedores"),
            ["financeiro-lancamentos"] = Definition("financeiro-lancamentos", "Lançamentos financeiros", "Financeiro", "Financeiro", "Financeiro", "/api/financeiro/lancamentos"),
            ["financeiro-faturamento"] = Definition("financeiro-faturamento", "Faturamento por período", "Financeiro", "Faturamento", "Financeiro", "/api/financeiro/faturamento"),
            ["financeiro-contabil-lancamentos"] = Definition("financeiro-contabil-lancamentos", "Lançamentos contábeis", "Financeiro", "ContabilLancamento", "Financeiro", "/api/financeiro/contabil/lancamentos")
        };

    public static bool TryGet(string key, out OperationalQueryDefinition definition)
        => Definitions.TryGetValue(key, out definition!);

    public static IReadOnlyCollection<OperationalQueryDefinition> All => Definitions.Values.ToArray();

    private static OperationalQueryDefinition Definition(
        string key,
        string title,
        string sector,
        string tableName,
        string permission,
        string endpoint)
        => new(key, title, sector, tableName, permission, endpoint);
}
