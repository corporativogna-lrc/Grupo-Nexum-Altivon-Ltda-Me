<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7258 Data: 30/10/2026
 -->

# Arvore do menu WPF/PDV

Data da fotografia do menu: 2026-08-16  
Última reconciliação com o código: 2026-11-02  
Checkout oficial: D:\Nexum Altivon\NexumAltivon.com  
API oficial local: porta 5010  
Fonte de versao: Directory.Build.props

## Regra de aceite

O progresso de cada bloco e comunicado ao proprietario. O aceite externo e a divulgacao de marco permanecem bloqueados ate superar 260 tabelas operacionais. Uma tabela somente entra no numerador quando possui contrato real, permissao, persistencia, auditoria, releitura e formulario ou consulta correspondente ao processo.

Arquivos de patch, telas estaticas e janelas genericas nao entram no numerador por coincidencia de nome. Cada ramo abaixo precisa ser reconciliado com tabela, endpoint, permissao e evidencia de build ou execucao.

## Arvore real de menu

~~~text
GenesisGest.Net - Central Desktop ERP/PDV
|-- Operacao
|   |-- Abrir PDV / Venda de balcao -> PdvSaleWindow
|   |-- Abrir caixa -> PdvSaleWindow / contrato de caixa a homologar
|   |-- Fechar caixa -> PdvSaleWindow / contrato de caixa a homologar
|   |-- Troca e devolucao -> PdvSaleWindow / regra de devolucao a homologar
|   +-- Consulta de pedido -> OperationalQueryWindow -> GET /api/pedidos (Gerente)
|-- Cadastros
|   |-- Clientes -> CustomerManagementWindow
|   |-- Fornecedores -> SupplierManagementWindow
|   |-- Produtos e servicos -> ItensServicoWindow
|   |-- Categorias e subcategorias -> CategoriasWindow
|   |-- Empresas do grupo -> EmpresasGrupoWindow
|   |-- Centros de custo -> CentrosCustoWindow
|   +-- Usuarios, perfis e permissoes -> AccessManagementWindow
|-- Compras
|   |-- Solicitacao de compra -> ProcurementWindow / contrato por processo
|   |-- Cotacao com fornecedores -> ProcurementWindow / contrato por processo
|   |-- Pedido de compra -> ProcurementWindow / contrato por processo
|   |-- Entrada de mercadoria -> ProcurementWindow / contrato por processo
|   |-- Dropshipping e parcerias -> DropshippingWindow
|   +-- Devolucao a fornecedor -> bloqueada ate contrato proprio de estorno
|-- Estoque
|   |-- Saldo fisico -> SaldosProdutoWindow -> GET /api/erp/estoque-fisico (Gerente; leitura)
|   |-- Saldo fiscal -> pendente de contrato fiscal reconciliado; nao inferir de est_saldos
|   |-- Transferencia entre lojas -> contrato WMS a homologar
|   |-- Inventario -> contrato WMS a homologar
|   |-- Codigo de barras e QR Code -> CodigoBarrasQrWindow / API administrativa autenticada
|   +-- Rastreabilidade por origem -> StockTraceabilityWindow -> GET /api/erp/estoque-movimentacoes (Gerente; leitura)
|-- Retaguarda
|   |-- Painel Genesis local -> ModuleWorkspaceWindow
|   |-- Cockpit executivo -> ModuleWorkspaceWindow / indicadores a homologar
|   |-- Comercial e CRM -> ModuleWorkspaceWindow
|   |-- Leads e qualificacao -> LeadsQualificacaoWindow
|   |-- Cupons e promocoes -> CuponsPromocoesWindow
|   |-- Biblioteca de midias -> SiteMidiasWindow
|   |-- Perfis publicos de lojas e parceiros -> SitePerfisPublicosWindow
|   |-- Relatorios gerenciais -> consulta por contrato do relatorio
|   +-- Auditoria operacional -> AuditoriaOperacionalWindow
|-- Financeiro
|   |-- Contas a receber -> ContasReceberWindow
|   |-- Contas a pagar -> ContasPagarWindow
|   |-- Baixas financeiras -> BaixasPagamentosWindow
|   |-- Lancamentos contabeis -> LancamentoContabilWindow
|   |-- Fechamento contabil -> FechamentoPeriodoWindow
|   |-- Caixa e conciliacao -> contrato proprio pendente
|   |-- Tesouraria -> contrato proprio pendente
|   |-- Alcadas de aprovacao -> WorkflowAprovacoesWindow
|   |-- Conciliacao bancaria -> ConciliacaoFinanceiraWindow
|   |-- Ativo imobilizado -> contrato proprio pendente
|   +-- DRE e indicadores -> DreIndicadoresWindow
|-- Fiscal
|   |-- Emissao fiscal manual -> ManualNfeWindow
|   |-- Contingencia fiscal -> ManualNfeWindow
|   |-- Empresa emissora automatica -> FiscalRoutingWindow
|   |-- Regras fiscais do grupo -> FiscalRoutingWindow
|   |-- NF-e / NFC-e / SAT -> ManualNfeWindow
|   |-- SPED / EFD / REINF -> contrato fiscal por arquivo
|   +-- Tributos e retencoes -> contrato fiscal por tributo
|-- Logistica
|   |-- Coleta -> LogisticsWindow
|   |-- Entrega -> LogisticsWindow
|   |-- Rastreamento -> LogisticsWindow / provedor externo pendente
|   |-- Transportadoras -> LogisticsWindow
|   |-- Status do pedido -> OperationalQueryWindow -> GET /api/pedidos/acompanhar (número + e-mail/documento)
|   +-- Comunicacao ao cliente -> contrato de notificacao
|-- MES / OPS
|   |-- Ativos operacionais -> OpsAtivosWindow
|   |-- Ordens de servico -> OpsOrdensServicoWindow
|   +-- Apontamentos de producao -> OpsProducaoWindow
|-- Integracoes
|   |-- API local do servidor -> health local e tarefa oficial
|   |-- API publica Cloudflare -> health publico e tunnel
|   |-- E-commerce e dropshipping -> DropshippingWindow
|   |-- Logistica e rastreio -> LogisticsWindow
|   |-- Marketplaces -> credenciais oficiais pendentes
|   |-- Open Finance / bancos -> contrato externo pendente
|   |-- WhatsApp e e-mail -> contrato de notificacao pendente
|   +-- Chaves OpenAI - Yara e Sophia -> NetworkSettingsWindow
|-- Governanca
|   |-- RBAC e perfis -> AccessManagementWindow
|   |-- Trilha de auditoria -> AuditoriaOperacionalWindow
|   |-- LGPD -> contrato de governanca pendente
|   |-- Workflow e aprovacoes -> WorkflowAprovacoesWindow
|   +-- Multiempresa e tenant -> TenantManagementWindow
|-- HCM / RH
|   |-- Colaboradores / RH / folha -> contrato RH por processo
|   |-- Contratacoes -> contrato RH por processo
|   |-- Demissoes -> contrato RH por processo
|   |-- Cargos e departamentos -> contrato RH por processo
|   +-- Ponto e jornada -> contrato RH por processo
+-- Sistema
    |-- Atualizar conexoes -> health local/publico
    |-- Grade de rede e endpoints -> NetworkSettingsWindow
    |-- Configurar terminal -> configuracao local do terminal
    |-- Mapa de desenvolvimento WPF/PDV -> MapaDesenvolvimentoWindow
    |-- Atualizacao do Desktop -> AtualizacaoDesktopWindow (consulta Releases; download; instalacao confirmada)
    +-- Abrir pasta de contingencia -> LocalOutboxService
~~~

## Rotas web que suportam fluxos da arvore

- Cadastro publico de cliente: `Home` -> `POST /api/clientes` -> link HTTPS `/confirmar-cadastro?token=...` -> `ConfirmarCadastro` -> `GET /api/clientes/confirmar` -> `/login`. A rota antiga `/confirmar-cadastro.html` permanece como alias para links já enviados.
- O envio de e-mail precisa ser homologado com destinatário e provedor autorizados; não gravar cadastros fictícios no banco compartilhado.

## Evidencia desta versao

- Lancamentos contabeis: API transacional, tela individual, menu, auditoria, build integrado e 37 testes aprovados no bloco.
- Fechamento contabil: API transacional, tela individual, menu, auditoria, build integrado e 37 testes aprovados no bloco.
- Consultas operacionais: 40 entradas individuais no menu Retaguarda, cada uma ligada a uma rota GET oficial e protegida; não são contabilizadas como formulários CRUD.
- Build integrado desta entrega: API, ERP, Desktop e testes, 0 erros e 0 avisos.
- O mapa e informativo e nao transforma ramos pendentes, bloqueados ou genericos em tabelas operacionais.
- Atualizacao do Desktop foi conectada ao menu Sistema em 2026-10-30; publicacao, instalacao real e rollback continuam sem homologacao.
- PDV em 2026-10-30: venda de balcão usa `POST /api/erp/genesis/pdv/vendas`, exige sessão JWT e identificadores explícitos de empresa, cliente, operador e produtos; há idempotência por número e status de falha do espelhamento Genesis→Nexum. A busca do catálogo inclui SKU, código de barras e QR cadastrado; para códigos numéricos, a API também ignora pontuação comum. Enter em leitor de teclado inicia a busca e seleciona apenas identificador único exato, sem adicionar ou finalizar venda automaticamente. A tela exige seleção de cliente ativo e o POST revalida o documento no Genesis. A busca ainda é limitada ao catálogo publicável e aguarda homologação autenticada com itens reais e leitores. Pagamento é parte da venda. Abertura/fechamento de caixa, emissão fiscal e devolução não possuem contrato transacional completo e permanecem bloqueados.
- Recuperação do servidor em 2026-10-30: endpoints locais/públicos respondem, serviços MySQL/Cloudflared/Schedule estão automáticos. A tarefa oficial da API não foi encontrada nesta sessão e a identidade do processo exige validação elevada; não declarar boot testado. Instalador/reparador foram ajustados para três reinícios de serviço e ainda precisam ser executados como Administrador. Nenhum reinício de host foi feito.
- Estoque físico: a consulta somente leitura `GET /api/erp/estoque-fisico` está ligada ao menu e à janela canônica `SaldosProdutoWindow`, autorizada pela política Gerente. Usa `tenant_id` do token, obtém o CNPJ de `sys_tenants.documento`, valida dígitos verificadores, exige exatamente uma empresa ativa correspondente em `adm_empresas` e agrega `est_saldos` somente para o `emp_id` resolvido. CNPJ ausente/inválido, empresa não encontrada ou duplicada geram HTTP 409; busca maior que 100 caracteres gera HTTP 400. Retorna SKU, descrição e quantidades física, reservada e disponível, com pesquisa e limite de 500. Build API aprovado; build WPF global continua bloqueado pelas demais telas. Publicação da API e homologação autenticada com CNPJ/saldo reais ainda pendentes. O tenant padrão sem CNPJ não lê saldos.
- Rastreabilidade de estoque: `StockTraceabilityWindow` e `GET /api/erp/estoque-movimentacoes` são consulta somente leitura sob Gerente. O endpoint restringe empresa pelo tenant/CNPJ, permite filtro por SKU, descrição, documento, lote e série e limita a resposta a 500 eventos. Build API e 37 testes passaram; build WPF global ainda está bloqueado por handlers ausentes em outras janelas. Nenhuma consulta autenticada em produção foi feita; conforme informado pelo proprietário, movimentos persistidos devem ser tratados como dados de teste até validação. Saldo fiscal, inventário e transferências seguem sem contrato confirmado.
- Confirmação de cadastro web: o emissor de e-mail usa `/confirmar-cadastro` e o cliente também aceita o legado `.html` redirecionado pelo GitHub Pages. Entrega de e-mail e confirmação com conta real permanecem sem homologação.

## Proximo registro

Cada nova janela deve atualizar este mapa, o checklist clinico e a versao central antes da classificacao do material de origem. O relato ao proprietario ocorre em cada bloco; o aceite externo continua condicionado ao marco superior a 260 tabelas operacionais.
