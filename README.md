<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 -->
# NEXUM ALTIVON COMMERCE PLATFORM
## FASE 1 — Estrutura Base, Banco, API e Autenticação

### Grupo Nexum Altivon ME — www.nexumaltivon.com

---

## Estrutura do Projeto

```
DIAGRAMA GERAL

Sistema Nexum Altivon
│
├── Frontend React
│   ├── Interface
│   ├── Componentes
│   └── Consome API
│
├── Backend API
│   ├── Controllers
│   ├── Services
│   ├── DTOs
│   ├── Models
│   ├── Data
│   └── Banco SQL
│
├── ERP
│   ├── Models
│   ├── Services
│   └── Data
│
├── Desktop WPF
│   ├── Interface
│   └── Integrações
│
├── Docker
│   ├── API
│   ├── Frontend
│   └── Nginx
│
└── Documentação

ARVORE COMPLETA DO PROJETO

├── .github
│   └── workflows
│       ├── ci-cd.yml
│       ├── dotnet-desktop.yml
│       ├── npm-publish-github-packages.yml
│       └── Pages.yml
├── .snakeflow
│   ├── trigger-precommit
│   ├── trigger-precommit.cmd
│   ├── trigger-quality
│   └── trigger-quality.cmd
├── .vscode
│   ├── extensions.json
│   └── settings.json
├── Arquivos_Mortos
│   └── frontend-legado-20260629
│       ├── admin-index-mock-legado.html
│       ├── frontend-index-legado.html
│       ├── landing-raiz-legado.html
│       ├── public-landing-legado.html
│       └── README.md
├── Configurations
│   ├── ErpMappingProfile.cs
│   ├── HangfireAuthorizationFilter.cs
│   └── ServiceExtensions.cs
├── Controllers
│   ├── CrmController.cs
│   ├── ErpDashboardController.cs
│   ├── EstoqueController.cs
│   ├── FinanceiroController.cs
│   ├── FiscalController.cs
│   ├── FornecedoresController.cs
│   ├── RelatoriosController.cs
│   └── SyncController.cs
├── Data
│   ├── ErpDbContext.cs
│   ├── NexumDbContext.cs
│   └── NexumDbContext_ERP.cs
├── Database
│   ├── 2026-06-19-corrigir-clientes-confirmacao-email.sql
│   ├── 2026-06-19-normalizar-enums-operacionais.sql
│   ├── 2026-06-19-sincronizar-6-lojas-operacao.sql
│   ├── erp_schema_update.sql
│   └── erp_update_schema.sql
├── docker
│   ├── nginx
│   │   ├── frontend.conf
│   │   └── nginx.conf
│   ├── scripts
│   │   ├── backup-mysql.sh
│   │   └── restore-mysql.sh
│   ├── .env.example
│   ├── docker-compose.prod.yml
│   ├── docker-compose.yml
│   ├── Dockerfile.api
│   └── Dockerfile.frontend
├── docs
│   ├── 2026-06-16-checklist-prontidao-deploy.md
│   ├── CHECKLIST_OPERACIONAL_TABELAS_2026-06-29.md
│   ├── CRONOGRAMA_OPERACIONAL_2026-06-29.md
│   └── HANDOFF-PRODUCAO-PRIVADA-INTEGRACOES.md
├── DTOs
│   ├── CrmDtos.cs
│   ├── ErpDtos.cs
│   ├── FinanceiroDtos.cs
│   └── OperacionalDtos.cs
├── Models
│   ├── CrmFornecedorModels.cs
│   ├── CrmModels.cs
│   ├── EstoqueModels.cs
│   ├── FinanceiroModels.cs
│   ├── FiscalEstoqueModels.cs
│   ├── FiscalModels.cs
│   ├── SharedModels.cs
│   └── SyncModels.cs
├── NexumAltivon.Desktop
│   ├── Models
│   │   ├── DesktopModule.cs
│   │   └── OrganizationNode.cs
│   ├── App.xaml
│   ├── App.xaml.cs
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   ├── ManualNfeWindow.xaml
│   ├── ManualNfeWindow.xaml.cs
│   └── NexumAltivon.Desktop.csproj
├── NexumAltivon_Back-End
│   ├── API
│   │   ├── Configurations
│   │   │   ├── HangfireAuthorizationFilter.cs
│   │   │   ├── IntegrationExtensions.cs
│   │   │   └── ServiceExtensions.cs
│   │   ├── Controllers
│   │   │   ├── Admin
│   │   │   │   └── DashboardController.cs
│   │   │   ├── ERP
│   │   │   │   └── ErpDashboardController.cs
│   │   │   ├── AuthController.cs
│   │   │   ├── CarrinhoController.cs
│   │   │   ├── CheckoutController.cs
│   │   │   ├── ClientesController.cs
│   │   │   ├── CrmController.cs
│   │   │   ├── FinanceiroController.cs
│   │   │   ├── IntegracoesController.cs
│   │   │   ├── LojasController.cs
│   │   │   ├── PagamentoController.cs
│   │   │   ├── PedidosController.cs
│   │   │   ├── ProdutosController.cs
│   │   │   └── WebhookController.cs
│   │   ├── Data
│   │   │   └── NexumDbContext.cs
│   │   ├── Database
│   │   │   └── 2026-06-29-genesisgest-original-schema.sql
│   │   ├── DTOs
│   │   │   ├── Admin
│   │   │   │   └── DashboardDtos.cs
│   │   │   ├── ApiResponse.cs
│   │   │   ├── AuthDtos.cs
│   │   │   ├── CarrinhoDtos.cs
│   │   │   ├── CheckoutDtos.cs
│   │   │   ├── ClienteDtos.cs
│   │   │   ├── CrmDtos.cs
│   │   │   ├── DropshippingDtos.cs
│   │   │   ├── ErpSyncDtos.cs
│   │   │   ├── LogisticaDtos.cs
│   │   │   ├── LojaDtos.cs
│   │   │   ├── MarketplaceDtos.cs
│   │   │   ├── PagamentoDtos.cs
│   │   │   ├── PedidoDtos.cs
│   │   │   └── ProdutoDtos.cs
│   │   ├── ERP
│   │   │   ├── FiscalRouting
│   │   │   │   ├── FiscalRoutingEngine.cs
│   │   │   │   └── FiscalRoutingModels.cs
│   │   │   └── SharedData
│   │   │       ├── GenesisDbContext.cs
│   │   │       ├── GenesisFinanceDtos.cs
│   │   │       ├── GenesisFinanceModels.cs
│   │   │       ├── GenesisFinanceReferenciaDtos.cs
│   │   │       ├── GenesisFinanceService.cs
│   │   │       ├── GenesisFinanceWriteDtos.cs
│   │   │       ├── GenesisRhDtos.cs
│   │   │       ├── GenesisRhService.cs
│   │   │       └── GenesisRhWriteDtos.cs
│   │   ├── Mappings
│   │   │   └── MappingProfile.cs
│   │   ├── Middleware
│   │   │   ├── ApiHealthCheck.cs
│   │   │   ├── AuditoriaMiddleware.cs
│   │   │   ├── AuthValidators.cs
│   │   │   ├── ExceptionMiddleware.cs
│   │   │   └── RateLimitingMiddleware.cs
│   │   ├── Models
│   │   │   ├── Carrinho.cs
│   │   │   ├── CarrinhoCheckoutPagamento.cs
│   │   │   ├── Categoria.cs
│   │   │   ├── Cliente.cs
│   │   │   ├── ConfiguracaoSistema.cs
│   │   │   ├── CrmAtendimento.cs
│   │   │   ├── CrmLead.cs
│   │   │   ├── Cupom.cs
│   │   │   ├── DropshippingConfig.cs
│   │   │   ├── EmpresaGrupo.cs
│   │   │   ├── Endereco.cs
│   │   │   ├── Envio.cs
│   │   │   ├── Financeiro.cs
│   │   │   ├── Fiscal.cs
│   │   │   ├── Fornecedor.cs
│   │   │   ├── IntegracoesModels.cs
│   │   │   ├── LogAuditoria.cs
│   │   │   ├── Loja.cs
│   │   │   ├── Marketplace.cs
│   │   │   ├── Notificacao.cs
│   │   │   ├── Pagamento.cs
│   │   │   ├── Pedido.cs
│   │   │   ├── PedidoItem.cs
│   │   │   ├── Produto.cs
│   │   │   ├── Transportadora.cs
│   │   │   └── Usuario.cs
│   │   ├── Services
│   │   │   ├── Admin
│   │   │   │   └── AdminDashboardService.cs
│   │   │   ├── AuthService.cs
│   │   │   ├── CarrinhoService.cs
│   │   │   ├── CheckoutService.cs
│   │   │   ├── ClienteService.cs
│   │   │   ├── ConfiguracaoService.cs
│   │   │   ├── CrmService.cs
│   │   │   ├── DropshippingService.cs
│   │   │   ├── ErpDashboardService.cs
│   │   │   ├── ErpSyncService.cs
│   │   │   ├── FinanceiroService.cs
│   │   │   ├── FreteService.cs
│   │   │   ├── IErpDashboardService.cs
│   │   │   ├── Interfaces.cs
│   │   │   ├── LogAuditoriaService.cs
│   │   │   ├── LogisticaService.cs
│   │   │   ├── LojaService.cs
│   │   │   ├── MarketplaceHubService.cs
│   │   │   ├── MarketplaceSyncService.cs
│   │   │   ├── MelhorEnvioService.cs
│   │   │   ├── MercadoLivreService.cs
│   │   │   ├── MercadoPagoService.cs
│   │   │   ├── NotificacaoService.cs
│   │   │   ├── PedidoService.cs
│   │   │   └── ProdutoService.cs
│   │   ├── appsettings.json
│   │   ├── appsettings.PrivateProduction.template.json
│   │   └── Program.cs
│   ├── Database
│   │   └── nexum_altivon_schema.sql
│   ├── appsettings.ERP.json
│   ├── NexumAltivon.API.csproj
│   └── Program_ERP.cs
├── NexumAltivon_ERP
│   ├── Data
│   │   └── GenesisDbContext.cs
│   ├── DTOs
│   │   ├── CRM
│   │   │   ├── AtividadeDtos.cs
│   │   │   ├── CampanhaDtos.cs
│   │   │   └── PipelineDtos.cs
│   │   ├── Estoque
│   │   │   └── MovimentacaoEstoqueDtos.cs
│   │   ├── Financeiro
│   │   │   ├── BancoDtos.cs
│   │   │   ├── ContaPagarDtos.cs
│   │   │   ├── ContaReceberDtos.cs
│   │   │   ├── DREDtos.cs
│   │   │   ├── FluxoCaixaDtos.cs
│   │   │   └── PlanoContasDtos.cs
│   │   └── Fiscal
│   │       ├── ConfiguracaoFiscalDtos.cs
│   │       └── NFeDtos.cs
│   ├── Models
│   │   ├── CRM
│   │   │   ├── Atividade.cs
│   │   │   ├── Campanha.cs
│   │   │   ├── InteracaoTicket.cs
│   │   │   ├── LeadCRM.cs
│   │   │   ├── Oportunidade.cs
│   │   │   ├── Pipeline.cs
│   │   │   ├── SegmentoCliente.cs
│   │   │   └── TicketSuporte.cs
│   │   ├── Estoque
│   │   │   ├── AlertaEstoque.cs
│   │   │   ├── Inventario.cs
│   │   │   ├── ItemInventario.cs
│   │   │   ├── Kardex.cs
│   │   │   ├── LocalEstoque.cs
│   │   │   ├── MovimentacaoEstoque.cs
│   │   │   ├── ProdutoFornecedor.cs
│   │   │   └── TransferenciaEstoque.cs
│   │   ├── Financeiro
│   │   │   ├── Banco.cs
│   │   │   ├── CentroCusto.cs
│   │   │   ├── ConciliacaoBancaria.cs
│   │   │   ├── ContaBancaria.cs
│   │   │   ├── ContaPagar.cs
│   │   │   ├── ContaReceber.cs
│   │   │   ├── DRE.cs
│   │   │   ├── FluxoCaixa.cs
│   │   │   ├── MovimentacaoBancaria.cs
│   │   │   └── PlanoContas.cs
│   │   └── Fiscal
│   │       ├── CFOP.cs
│   │       ├── ConfiguracaoFiscal.cs
│   │       ├── Imposto.cs
│   │       ├── ItemNFe.cs
│   │       ├── NFCe.cs
│   │       ├── NFe.cs
│   │       ├── Sintegra.cs
│   │       └── SPED.cs
│   └── Services
│       └── Financeiro
│           ├── BancoService.cs
│           ├── ContaPagarService.cs
│           ├── ContaReceberService.cs
│           ├── DREService.cs
│           ├── FluxoCaixaService.cs
│           └── PlanoContasService.cs
├── NexumAltivon_Front-End
│   ├── public
│   │   ├── area-cliente
│   │   │   └── index.html
│   │   ├── contato
│   │   │   └── index.html
│   │   ├── dashboard
│   │   │   ├── cadastros
│   │   │   │   ├── categorias
│   │   │   │   │   └── index.html
│   │   │   │   ├── clientes
│   │   │   │   │   └── index.html
│   │   │   │   ├── fornecedores
│   │   │   │   │   └── index.html
│   │   │   │   └── produtos
│   │   │   │       └── index.html
│   │   │   ├── erp-compras
│   │   │   │   └── index.html
│   │   │   ├── erp-financeiro
│   │   │   │   └── index.html
│   │   │   ├── erp-fiscal
│   │   │   │   └── index.html
│   │   │   ├── erp-logistica
│   │   │   │   └── index.html
│   │   │   ├── erp-vendas
│   │   │   │   └── index.html
│   │   │   ├── pedidos
│   │   │   │   └── index.html
│   │   │   └── index.html
│   │   ├── imagens
│   │   │   └── homepage
│   │   │       ├── banner-atendimento.svg
│   │   │       ├── banner-ecommerce.svg
│   │   │       ├── banner-marcas.svg
│   │   │       ├── Logo-2.png
│   │   │       ├── logo-grupo-nexum-altivon.svg
│   │   │       ├── loja-chronos.svg
│   │   │       ├── loja-estruturaline.svg
│   │   │       ├── loja-geracao-top.svg
│   │   │       ├── loja-gran-festas.svg
│   │   │       ├── loja-gran-tur.svg
│   │   │       └── loja-moda-mim.svg
│   │   ├── institucional
│   │   │   └── index.html
│   │   ├── lojas
│   │   │   └── index.html
│   │   ├── politica-privacidade
│   │   │   └── index.html
│   │   ├── politica-reembolso
│   │   │   └── index.html
│   │   ├── .htaccess
│   │   ├── 404.html
│   │   ├── admin-painel.html
│   │   ├── api-runtime.json
│   │   ├── CNAME
│   │   ├── confirmar-cadastro.html
│   │   ├── index.html
│   │   ├── nexum-admin-integration.js
│   │   ├── nexum-integration.js
│   │   └── web.config
│   ├── scripts
│   │   └── postbuild-pages.js
│   ├── src
│   │   ├── components
│   │   │   ├── ui
│   │   │   │   ├── accordion.jsx
│   │   │   │   ├── alert-dialog.jsx
│   │   │   │   ├── alert.jsx
│   │   │   │   ├── aspect-ratio.jsx
│   │   │   │   ├── avatar.jsx
│   │   │   │   ├── badge.jsx
│   │   │   │   ├── breadcrumb.jsx
│   │   │   │   ├── button.jsx
│   │   │   │   ├── calendar.jsx
│   │   │   │   ├── card.jsx
│   │   │   │   ├── carousel.jsx
│   │   │   │   ├── checkbox.jsx
│   │   │   │   ├── collapsible.jsx
│   │   │   │   ├── command.jsx
│   │   │   │   ├── context-menu.jsx
│   │   │   │   ├── dialog.jsx
│   │   │   │   ├── drawer.jsx
│   │   │   │   ├── dropdown-menu.jsx
│   │   │   │   ├── form.jsx
│   │   │   │   ├── hover-card.jsx
│   │   │   │   ├── input-otp.jsx
│   │   │   │   ├── input.jsx
│   │   │   │   ├── label.jsx
│   │   │   │   ├── menubar.jsx
│   │   │   │   ├── navigation-menu.jsx
│   │   │   │   ├── pagination.jsx
│   │   │   │   ├── popover.jsx
│   │   │   │   ├── progress.jsx
│   │   │   │   ├── radio-group.jsx
│   │   │   │   ├── resizable.jsx
│   │   │   │   ├── scroll-area.jsx
│   │   │   │   ├── select.jsx
│   │   │   │   ├── separator.jsx
│   │   │   │   ├── sheet.jsx
│   │   │   │   ├── skeleton.jsx
│   │   │   │   ├── slider.jsx
│   │   │   │   ├── sonner.jsx
│   │   │   │   ├── switch.jsx
│   │   │   │   ├── table.jsx
│   │   │   │   ├── tabs.jsx
│   │   │   │   ├── textarea.jsx
│   │   │   │   ├── toast.jsx
│   │   │   │   ├── toaster.jsx
│   │   │   │   ├── toggle-group.jsx
│   │   │   │   ├── toggle.jsx
│   │   │   │   └── tooltip.jsx
│   │   │   ├── CheckoutSteps.js
│   │   │   ├── Footer.js
│   │   │   ├── GlobalActions.js
│   │   │   ├── Navbar.js
│   │   │   └── ProductCard.js
│   │   ├── context
│   │   │   ├── AuthContext.js
│   │   │   └── CartContext.js
│   │   ├── hooks
│   │   │   └── use-toast.js
│   │   ├── lib
│   │   │   └── utils.js
│   │   ├── pages
│   │   │   ├── AcompanharPedido.js
│   │   │   ├── AreaCliente.js
│   │   │   ├── Carrinho.js
│   │   │   ├── Checkout.js
│   │   │   ├── Contato.js
│   │   │   ├── Dashboard.js
│   │   │   ├── Home.js
│   │   │   ├── Institucional.js
│   │   │   ├── Login.js
│   │   │   ├── Lojas.js
│   │   │   ├── PoliticaPrivacidade.js
│   │   │   ├── PoliticaReembolso.js
│   │   │   ├── ProdutoDetalhe.js
│   │   │   └── Produtos.js
│   │   ├── services
│   │   │   └── api.js
│   │   ├── utils
│   │   │   ├── formatters.js
│   │   │   └── validation.js
│   │   ├── App.css
│   │   ├── App.js
│   │   ├── constants.js
│   │   ├── index.css
│   │   └── index.js
│   ├── .env.example
│   ├── .npmrc
│   ├── components.json
│   ├── craco.config.js
│   ├── jsconfig.json
│   ├── package-lock.json
│   ├── package.json
│   ├── postcss.config.js
│   └── tailwind.config.js
├── Properties
│   └── launchSettings.json
├── scripts
│   ├── server
│   │   ├── APLICAR-API-5012-SERVIDOR.cmd
│   │   ├── INSTALAR-BOOT-SERVIDOR-NEXUM.cmd
│   │   ├── NEXUM-GUARDIAN-CMD-5012.cmd
│   │   └── START-SERVICE-TUNNEL-CLOUDFLARE.cmd
│   └── INSTALAR-GUARDIAN-API-5012-SERVIDOR.cmd
├── Services
│   ├── CrmService.cs
│   ├── EstoqueService.cs
│   ├── FinanceiroService.cs
│   ├── FiscalService.cs
│   ├── FornecedorService.cs
│   ├── RelatorioService.cs
│   └── SyncErpService.cs
├── static
│   ├── css
│   │   ├── main.b044b9e2.css
│   │   └── main.b044b9e2.css.map
│   └── js
│       ├── main.b42d8c62.js
│       ├── main.b42d8c62.js.LICENSE.txt
│       └── main.b42d8c62.js.map
├── .dockerignore
├── .gitignore
├── .htaccess
├── .nojekyll
├── 404.html
├── _github-pages-challenge-corporativogna-lrc
├── ABRIR-ERP-DESKTOP.cmd
├── admin-painel.html
├── api-runtime.json
├── API_24H_SERVIDOR.md
├── appsettings.json
├── asset-manifest.json
├── CNAME
├── DEPLOY.md
├── DOCUMENTACAO_CONEXOES.md
├── eb5a5c7d1a81dc9e18390a5f8146f3c2187e34c7
├── github-pages-challenge-corporativogna-lrc
├── IIS_DEPLOY.md
├── index.fallback.html
├── index.html
├── INSTALAR-ATALHO-ERP-DESKTOP.cmd
├── nexum-admin-integration.js
├── nexum-integration.js
├── NexumAltivon.ERP.csproj
├── NexumAltivon.ERP.sln
├── NuGet.Config
├── PROD_DNS_IIS_RUNBOOK.md
├── PROD_SAFE_UPDATE_CHECKLIST.md
├── PRODUCAO_STATUS_ATUAL.md
├── Program.cs
├── README.md
├── README_Fase2.md
├── README_Fase3.md
├── README_Fase4.md
├── README_Fase5.md
├── README_Fase6.md
├── README_FINAL.md
├── settings.VisualStudio.json
├── VERSAO.md
└── web.config

```

---




### Tabelas Criadas (22)

| Tabela | Descricao |
|---|---|
| usuarios | Usuarios do sistema (admin/ERP) |
| lojas | 6 lojas do Grupo Nexum Altivon |
| categorias | Categorias de produtos |
| produtos | Catalogo de produtos |
| fornecedores | Fornecedores e distribuidores |
| clientes | Clientes finais |
| enderecos | Enderecos dos clientes |
| pedidos | Pedidos de compra |
| pedido_itens | Itens de cada pedido |
| carrinho | Carrinho de compras |
| cupons | Cupons de desconto |
| pagamentos | Transacoes de pagamento |
| transportadoras | Transportadoras integradas |
| envios | Envios e rastreamento |
| marketplaces | Configuracoes de marketplaces |
| dropshipping_config | Configuracoes de dropshipping |
| crm_leads | Leads do CRM |
| crm_atendimentos | Atendimentos do CRM |
| financeiro | Lancamentos financeiros |
| fiscal | Notas fiscais |
| notificacoes | Notificacoes do sistema |
| logs_auditoria | Logs de auditoria |
| configuracoes_sistema | Configuracoes globais |

### Views (5)
- v_resumo_pedidos_status
- v_estoque_baixo
- v_vendas_loja
- v_clientes_vip
- v_crm_leads_status

### Procedures (6)
- sp_gerar_numero_pedido
- sp_atualizar_estoque_pedido
- sp_calcular_total_pedido
- sp_registrar_auditoria
- sp_limpar_carrinho_abandonado
- sp_relatorio_vendas_dia

### Triggers (2)
- trg_pedido_before_update
- trg_cliente_after_update

---

## Seguranca

- JWT Authentication com refresh token
- BCrypt para hash de senhas
- Rate Limiting (100 req/min por IP)
- CORS configurado para dominios oficiais
- Auditoria automatica de todas as operacoes
- Policies de autorizacao por perfil

---

## Endpoints da API

| Endpoint | Metodo | Acesso | Descricao |
|---|---|---|---|
| /api/auth/login | POST | Publico | Login com JWT |
| /api/auth/refresh | POST | Publico | Renova token |
| /api/auth/registrar | POST | SuperAdmin | Novo usuario |
| /api/lojas | GET | Publico | Lista 6 lojas |
| /api/lojas/{id} | GET | Publico | Detalhes da loja |
| /api/produtos | GET | Publico | Lista produtos |
| /api/produtos/destaques | GET | Publico | Produtos em destaque |
| /api/clientes | GET | Vendedor | Lista clientes |
| /api/clientes | POST | Publico | Cadastro cliente |
| /api/carrinho | GET | Publico | Ver carrinho |
| /api/carrinho/adicionar | POST | Publico | Adicionar item |
| /api/pedidos | GET | Vendedor | Lista pedidos |
| /api/pedidos | POST | Publico | Criar pedido |
| /api/crm/leads | GET | Vendedor | Lista leads |
| /api/crm/leads | POST | Publico | Novo lead |
| /api/financeiro/faturamento | GET | Financeiro | Faturamento por periodo |
| /health | GET | Publico | Health check |
| /swagger | GET | Dev/Staging | Documentacao API |

---
```

### 2. API
```bash
cd NexumAltivon_Back-End
dotnet restore
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```

### 3. Front-End
Fonte oficial: `NexumAltivon_Front-End/src` com build publicado pelo workflow `Nexum 2026-06-28 - Deploy Operacional Oficial .com.br`.

Ambientes oficiais:
- Site: https://nexumaltivon.com.br
- API: https://api.nexumaltivon.com.br

Arquivos HTML antigos que continham informacoes incompletas ou dados fixos foram movidos para `Arquivos_Mortos/frontend-legado-20260629` e nao devem voltar para a publicacao.

---

## Proximas Fases

| Fase | Entregaveis |
|---|---|
| FASE 2 | Painel Admin, Dashboard, Gestao Completa |
| FASE 3 | Carrinho Funcional, Checkout, Gateway Pagamento |
| FASE 4 | Logistica, Marketplaces, Dropshipping |
| FASE 5 | CRM Avancado, Automacoes, IA, Analytics |

---

(c) 2026 Grupo Nexum Altivon ME — Todos os direitos reservados
