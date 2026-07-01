================================================================================
ANALISE ESTRUTURAL COMPLETA — GRUPO NEXUM ALTIVON (GenesisGest.Net v1.1.5)
================================================================================
Data da analise: 2026-06-30
Repositorio: Y:\Nexum Altivon\NexumAltivon.com (branch main, commit aed084a)
Total de arquivos versionados: 423
Stack oficial: ASP.NET 8 (Minimal API) + MySQL/Pomelo + React 19 (CRA/CRACO) +
               WPF (.NET 8) + Docker + GitHub Actions + GitHub Pages

--------------------------------------------------------------------------------
NOTA DE METODOLOGIA
--------------------------------------------------------------------------------
Esta analise foi feita lendo o codigo-fonte real (nao apenas a arvore de pastas).
Foram inspecionados: Program.cs (10.731 linhas, ~90 endpoints mapeados),
GenesisDbContext, controllers, services, models, .csproj, .sln, workflows CI/CD,
docker-compose, scripts .cmd de servidor e os 7 scripts SQL.
Marcadores [inferred] indicam informacao deduzida porem nao confirmada por teste
de execucao. Nenhuma modificacao foi feita no projeto (somente leitura).

================================================================================
DIAGRAMA 1 — ORGANIZACIONAL EMPRESARIAL / SETORIAL (com arquivos por setor)
================================================================================
Modelagem por SETORES CORPORATIVOS do Grupo Nexum Altivon. Cada ramificacao
lista os arquivos reais que atendem aquele setor, independentemente da linguagem.

GRUPO NEXUM ALTIVON (Holding multiempresa)
|
+-- SETOR 00 — NUCLEO DE PLATAFORMA & SSO (Autenticacao, Identidade, Plataforma)
|   |   [Status: PARCIAL — auth JWT real; MFA/multitenancy/workflow AUSENTES]
|   +-- NexumAltivon_Back-End/API/Program.cs                     (login JWT, /api/auth/*, /api/admin/usuarios, policies RBAC)
|   +-- NexumAltivon_Back-End/API/Controllers/AuthController.cs  (MVC — NAO roteado, ver DIAGRAMA 3)
|   +-- NexumAltivon_Back-End/API/Services/AuthService.cs
|   +-- NexumAltivon_Back-End/API/DTOs/AuthDtos.cs
|   +-- NexumAltivon_Back-End/API/Middleware/AuthValidators.cs
|   +-- NexumAltivon_Back-End/API/Middleware/ExceptionMiddleware.cs
|   +-- NexumAltivon_Back-End/API/Middleware/RateLimitingMiddleware.cs
|   +-- NexumAltivon_Back-End/API/Middleware/AuditoriaMiddleware.cs
|   +-- NexumAltivon_Back-End/API/Models/Usuario.cs
|   +-- NexumAltivon_Back-End/API/Models/LogAuditoria.cs
|   +-- NexumAltivon_Back-End/API/Models/ConfiguracaoSistema.cs
|   +-- NexumAltivon_Back-End/API/Models/EmpresaGrupo.cs         (multiempresa, mas SEM TenantId na base)
|   +-- NexumAltivon_Back-End/API/Services/LogAuditoriaService.cs
|   +-- NexumAltivon_Back-End/API/Services/ConfiguracaoService.cs
|   +-- NexumAltivon_Back-End/API/Configurations/ServiceExtensions.cs
|   +-- NexumAltivon_Back-End/API/Configurations/HangfireAuthorizationFilter.cs
|   +-- Configurations/HangfireAuthorizationFilter.cs            (duplicado raiz)
|   +-- Configurations/ServiceExtensions.cs                      (duplicado raiz)
|
+-- SETOR 01 — COCKPIT EXECUTIVO (Dashboards BI, KPIs, Relatorios)
|   |   [Status: IMPLEMENTADO (somente leitura/agregacao)]
|   +-- NexumAltivon_Back-End/API/Program.cs                     (/api/admin/dashboard/completo, /kpis, /api/dashboard/resumo)
|   +-- NexumAltivon_Back-End/API/Controllers/Admin/DashboardController.cs  (MVC — NAO roteado)
|   +-- NexumAltivon_Back-End/API/Controllers/ERP/ErpDashboardController.cs (MVC — NAO roteado)
|   +-- NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs
|   +-- NexumAltivon_Back-End/API/Services/ErpDashboardService.cs
|   +-- NexumAltivon_Back-End/API/Services/IErpDashboardService.cs
|   +-- NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs
|   +-- Controllers/ErpDashboardController.cs                    (duplicado raiz — NAO roteado)
|   +-- Controllers/RelatoriosController.cs                      (duplicado raiz — NAO roteado)
|   +-- Services/RelatorioService.cs                             (duplicado raiz)
|   +-- DTOs/ErpDtos.cs                                          (duplicado raiz, excluido do build)
|   +-- NexumAltivon_Front-End/src/pages/Dashboard.js
|   +-- NexumAltivon_Front-End/public/dashboard/index.html
|
+-- SETOR 02 — GRC & IAM (Acessos, Perfis RBAC, Trilhas de Auditoria)
|   |   [Status: PARCIAL — policies RBAC no JWT; sem gerenciamento de permissoes fino]
|   +-- NexumAltivon_Back-End/API/Program.cs                     (policies: SuperAdmin/Admin/Gerente/Financeiro/Fiscal/RH; /api/admin/usuarios/*)
|   +-- NexumAltivon_Back-End/API/Middleware/AuditoriaMiddleware.cs
|   +-- NexumAltivon_Back-End/API/Models/LogAuditoria.cs
|   +-- NexumAltivon_Back-End/API/Services/LogAuditoriaService.cs
|   +-- Database/2026-06-19-normalizar-enums-operacionais.sql    (normaliza enums de papel/perfil)
|
+-- SETOR 03 — MASTER DATA (Pessoas, Empresas, Centros de Custo, Itens)
|   |   [Status: IMPLEMENTADO (e-commerce); ERP corporativo PARCIAL]
|   +-- NexumAltivon_Back-End/API/Program.cs                     (/api/erp/empresas, /api/clientes*, /api/fornecedores*, /api/produtos*, /api/categorias*)
|   +-- NexumAltivon_Back-End/API/Controllers/ClientesController.cs  (MVC — NAO roteado)
|   +-- NexumAltivon_Back-End/API/Controllers/FornecedoresController.cs (NAO existe nesta pasta; ver raiz)
|   +-- Controllers/FornecedoresController.cs                    (duplicado raiz — NAO roteado)
|   +-- NexumAltivon_Back-End/API/Services/ClienteService.cs
|   +-- NexumAltivon_Back-End/API/Services/ProdutoService.cs
|   +-- NexumAltivon_Back-End/API/Services/LojaService.cs
|   +-- NexumAltivon_Back-End/API/Models/Cliente.cs / Endereco.cs / Fornecedor.cs / Produto.cs / Categoria.cs / Loja.cs / EmpresaGrupo.cs
|   +-- NexumAltivon_Back-End/API/DTOs/ClienteDtos.cs / ProdutoDtos.cs / LojaDtos.cs
|   +-- NexumAltivon_ERP/Models/Financeiro/CentroCusto.cs        (centro de custo — dominio ERP)
|   +-- NexumAltivon_ERP/Models/Estoque/ProdutoFornecedor.cs
|   +-- NexumAltivon.Desktop/CorporateMasterDataWindow.xaml(.cs) (janela desktop de Master Data corporativo)
|   +-- NexumAltivon_Front-End/public/dashboard/cadastros/{clientes,produtos,categorias,fornecedores}/index.html
|
+-- SETOR 04 — FICO (Financeiro: Contas Pagar/Receber, Tesouraria, Contabilidade)
|   |   [Status: IMPLEMENTADO em duas camadas (API e-commerce + ERP isolado)]
|   +-- NexumAltivon_Back-End/API/Program.cs                     (/api/financeiro/lancamentos*, /api/erp/genesis/financeiro/* — contas pagar/receber, boletos, referencias, baixas)
|   +-- NexumAltivon_Back-End/API/Controllers/FinanceiroController.cs (MVC — NAO roteado)
|   +-- NexumAltivon_Back-End/API/Services/FinanceiroService.cs
|   +-- NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceService.cs
|   +-- NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs / GenesisFinanceDtos.cs / GenesisFinanceWriteDtos.cs / GenesisFinanceReferenciaDtos.cs
|   +-- NexumAltivon_Back-End/API/ERP/SharedData/GenesisDbContext.cs  (DbSet do schema GenesisGest — duplicado do NexumAltivon_ERP)
|   +-- NexumAltivon_ERP/Models/Financeiro/*  (Banco, CentroCusto, ConciliacaoBancaria, ContaBancaria, ContaPagar, ContaReceber, DRE, FluxoCaixa, MovimentacaoBancaria, PlanoContas)
|   +-- NexumAltivon_ERP/DTOs/Financeiro/*    (Banco, ContaPagar, ContaReceber, DRE, FluxoCaixa, PlanoContas)
|   +-- NexumAltivon_ERP/Services/Financeiro/* (BancoService, ContaPagarService, ContaReceberService, DREService, FluxoCaixaService, PlanoContasService)
|   +-- NexumAltivon_ERP/Data/GenesisDbContext.cs (DbSet ERP — NAO referenciado pela API ativa)
|   +-- Controllers/FinanceiroController.cs / Models/FinanceiroModels.cs / DTOs/FinanceiroDtos.cs  (duplicados raiz — NAO roteados)
|   +-- Services/FinanceiroService.cs (duplicado raiz)
|   +-- NexumAltivon.Desktop/FinancialLedgerWindow.xaml(.cs)     (janela desktop do razao/financeiro)
|   +-- NexumAltivon_Front-End/public/dashboard/erp-financeiro/index.html
|   +-- Database/erp_schema_update.sql / erp_update_schema.sql   (DDL financeiro/fiscal)
|
+-- SETOR 05 — SCM & WMS (Suprimentos, Compras, Portaria, Estoques)
|   |   [Status: IMPLEMENTADO (compras + recebimento + WMS avancado no ERP isolado)]
|   +-- NexumAltivon_Back-End/API/Program.cs                     (/api/compras/{painel,solicitacoes,cotacoes,pedidos,entradas}*, status transitions)
|   +-- NexumAltivon_Back-End/API/Controllers/EstoqueController.cs (MVC — NAO roteado)
|   +-- NexumAltivon_Back-End/API/Services/ProdutoService.cs     (estoque derivado de produto)
|   +-- NexumAltivon_ERP/Models/Estoque/*  (AlertaEstoque, Inventario, ItemInventario, Kardex, LocalEstoque, MovimentacaoEstoque, ProdutoFornecedor, TransferenciaEstoque)
|   +-- NexumAltivon_ERP/DTOs/Estoque/MovimentacaoEstoqueDtos.cs
|   +-- Controllers/EstoqueController.cs / Models/EstoqueModels.cs / Models/FiscalEstoqueModels.cs (duplicados raiz — NAO roteados)
|   +-- Services/EstoqueService.cs / Services/FornecedorService.cs (duplicados raiz)
|   +-- NexumAltivon.Desktop/ProcurementWindow.xaml(.cs)         (janela desktop de compras/suprimentos)
|   +-- NexumAltivon.Desktop/LogisticsWindow.xaml(.cs)           (janela desktop de logistica/portaria)
|   +-- NexumAltivon_Front-End/public/dashboard/erp-compras/index.html
|   +-- NexumAltivon_Front-End/public/dashboard/erp-logistica/index.html
|
+-- SETOR 06 — COMERCIAL & CRM (Vendas, Pedidos, Faturamento, Fiscal, CRM)
|   |   [Status: IMPLEMENTADO (e-commerce + PDV + fiscal); CRM basico]
|   +-- NexumAltivon_Back-End/API/Program.cs                     (/api/pedidos*, /api/pdv/cockpit, /api/fiscal/* , /api/crm/leads*, /api/erp/genesis/pdv/vendas*, /api/desktop/genesis/pdv/vendas*)
|   +-- NexumAltivon_Back-End/API/Controllers/{Carrinho,Checkout,Pedidos,Produtos,Pagamento,Crm}Controller.cs (MVC — NAO roteados)
|   +-- NexumAltivon_Back-End/API/Controllers/IntegracoesController.cs / WebhookController.cs (MVC — NAO roteados)
|   +-- NexumAltivon_Back-End/API/Services/{CarrinhoService,CheckoutService,PedidoService,ProdutoService,Pagamento*}.cs
|   +-- NexumAltivon_Back-End/API/Services/CrmService.cs
|   +-- NexumAltivon_Back-End/API/Services/MercadoPagoService.cs / MercadoLivreService.cs / MelhorEnvioService.cs / FreteService.cs / LogisticaService.cs
|   +-- NexumAltivon_Back-End/API/Services/{MarketplaceHubService,MarketplaceSyncService,DropshippingService}.cs
|   +-- NexumAltivon_Back-End/API/ERP/FiscalRouting/{FiscalRoutingEngine.cs,FiscalRoutingModels.cs}  (motor de roteamento fiscal real)
|   +-- NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs / GenesisPdvDtos.cs  (PDV corporativo)
|   +-- NexumAltivon_Back-End/API/Models/{Pedido,PedidoItem,Carrinho,CarrinhoCheckoutPagamento,Pagamento,Fiscal,Cupom,Envio,Transportadora,Marketplace,DropshippingConfig,Notificacao}.cs
|   +-- NexumAltivon_Back-End/API/Models/{CrmLead,CrmAtendimento}.cs
|   +-- NexumAltivon_Back-End/API/DTOs/{Carrinho,Checkout,Pedido,Produto,Pagamento,Logistica,Marketplace,Dropshipping,Crm}Dtos.cs
|   +-- NexumAltivon_ERP/Models/CRM/*  (Atividade, Campanha, InteracaoTicket, LeadCRM, Oportunidade, Pipeline, SegmentoCliente, TicketSuporte)
|   +-- NexumAltivon_ERP/Models/Fiscal/*  (CFOP, ConfiguracaoFiscal, Imposto, ItemNFe, NFCe, NFe, Sintegra, SPED)
|   +-- NexumAltivon_ERP/DTOs/CRM/* / DTOs/Fiscal/*  (Atividade, Campanha, Pipeline, ConfiguracaoFiscal, NFe)
|   +-- Controllers/{Crm,Fiscal}Controller.cs / Models/{CrmModels,CrmFornecedorModels,FiscalModels}.cs / DTOs/{Crm,Operacional}Dtos.cs (duplicados raiz — NAO roteados)
|   +-- Services/{CrmService,FiscalService}Controller.cs (duplicados raiz)
|   +-- NexumAltivon.Desktop/PdvSaleWindow.xaml(.cs)             (PDV touchscreen)
|   +-- NexumAltivon.Desktop/ManualNfeWindow.xaml(.cs)           (emissao NFC-e/NF-e manual)
|   +-- NexumAltivon.Desktop/FiscalRoutingWindow.xaml(.cs)       (roteamento fiscal)
|   +-- NexumAltivon_Front-End/src/pages/{Carrinho,Checkout,Produtos,ProdutoDetalhe,AcompanharPedido}.js
|   +-- NexumAltivon_Front-End/public/dashboard/{erp-vendas,erp-fiscal,pedidos}/index.html
|
+-- SETOR 07 — HCM / RH (Colaboradores, Ponto, Folha, Desempenho)
|   |   [Status: PARCIAL — colaborador + referencias; sem ponto/folha/desempenho]
|   +-- NexumAltivon_Back-End/API/Program.cs                     (/api/erp/genesis/rh/{resumo,colaboradores,referencias}*)
|   +-- NexumAltivon_Back-End/API/ERP/SharedData/GenesisRhService.cs
|   +-- NexumAltivon_Back-End/API/ERP/SharedData/GenesisRhDtos.cs / GenesisRhWriteDtos.cs
|   +-- NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs  (entidades de pessoa/colaborador compartilhadas)
|
+-- SETOR 08 — MES / OPS (Ordens de Servico, Producao, Manutencao)
|   |   [Status: AUSENTE — nao ha endpoints, models nem telas de OS/producao/manutencao]
|   +-- (sem arquivos identificados para este setor)
|
+-- SETOR TRANSVERSAL — INTEGRACOES & WEBHOOKS
|   +-- NexumAltivon_Back-End/API/Program.cs                     (/api/integracoes/{status,diagnostico,credenciais-modelo,testar/*}, /api/webhooks/mercadopago, /api/frete/cotar, /api/logistica/roteamento)
|   +-- NexumAltivon_Back-End/API/Models/IntegracoesModels.cs
|   +-- NexumAltivon_Back-End/API/DTOs/{ErpSyncDtos,LogisticaDtos}.cs
|   +-- NexumAltivon_Back-End/API/Services/ErpSyncService.cs
|   +-- NexumAltivon_Back-End/API/Mappings/MappingProfile.cs     (AutoMapper)
|   +-- Controllers/SyncController.cs / Models/SyncModels.cs / Services/SyncErpService.cs (duplicados raiz — NAO roteados)
|
+-- SETOR TRANSVERSAL — EXPERIENCIA CLIENTE (Site publico + Area Cliente)
|   +-- NexumAltivon_Front-End/src/pages/{Home,Lojas,Institucional,Contato,AreaCliente,Login,PoliticaPrivacidade,PoliticaReembolso}.js
|   +-- NexumAltivon_Front-End/src/components/{Navbar,Footer,CheckoutSteps,GlobalActions,ProductCard}.js
|   +-- NexumAltivon_Front-End/src/context/{AuthContext,CartContext}.js
|   +-- NexumAltivon_Front-End/src/services/api.js
|   +-- NexumAltivon_Front-End/public/{index,admin-painel,confirmar-cadastro,area-cliente,contato,institucional,lojas,politica-*}.html
|   +-- NexumAltivon_Front-End/public/imagens/homepage/*         (logo + 6 lojas: Chronos, EstruturaLine, Geracao Top, Gran Festas, Gran Tur, Moda Mim)
|   +-- NexumAltivon_Front-End/public/nexum-integration.js / nexum-admin-integration.js
|   +-- static/css|js/*                                          (build estatico legacy publicado)
|   +-- index.html / index.fallback.html / 404.html (raiz, publicados no Pages)


================================================================================
DIAGRAMA 2 — INFRAESTRUTURA TECNOLOGICA (ramificacao grafica)
================================================================================

                        +-----------------------------+
                        |   FONTES DE CODIGO (repo)   |
                        |  github.com / main branch   |
                        +--------------+--------------+
                                       |
          +----------------------------+----------------------------+
          |                            |                            |
          v                            v                            v
+-------------------+        +-------------------+        +-------------------+
|  CI: ci-cd.yml    |        | CI: Pages.yml     |        | CI: dotnet-       |
|  (API + Frontend  |        | (Site estatico    |        | desktop.yml       |
|   Docker -> GHCR) |        |   -> GitHub Pages)|        | (build WPF x64)   |
+---------+---------+        +---------+---------+        +---------+---------+
          |                            |                            |
          v                            v                            v
+-------------------+        +-------------------+        +-------------------+
| ghcr.io/.../api   |        | GitHub Pages      |        | Artifact WPF      |
| ghcr.io/.../      |        | nexumaltivon.com  |        | (NexumAltivon.    |
|   frontend        |        |   .br (CNAME)     |        |  Desktop.exe)     |
+---------+---------+        +---------+---------+        +---------+---------+
          |                            |                            |
          |                 +----------+----------+                 |
          |                 |  DNS / CDN Cloudflare|                 |
          |                 |  nexumaltivon.com.br |                 |
          |                 +----------+----------+                 |
          |                            |                            |
          v                            v                            v
+---------------------------------------------------------------------------+
|              SERVIDOR PRINCIPAL PRODUCAO  192.168.1.72                     |
|  (scripts/server/APLICAR-API-5012-SERVIDOR.cmd, NEXUM-GUARDIAN-CMD-5012)   |
|                                                                            |
|   +-------------------+        +-------------------+                       |
|   | API .NET 8 (EXE)  |  <---> |  MySQL 8          |                       |
|   | porta 5012        |        |  192.168.1.72     |                       |
|   | Kestrel           |        |  :3309            |                       |
|   | (NexumAltivon.API |        |  schema: nexum_   |                       |
|   |  .dll)            |        |   altivon +       |                       |
|   |                   |        |   genesis_bd      |                       |
|   | JWT + Swagger +   |        |   (125 tabelas    |                       |
|   | HealthChecks +    |        |    adm_*)         |                       |
|   | Hangfire/Quartz   |        +-------------------+                       |
|   +---------+---------+                                                    |
|             |                                                              |
|             | Cloudflare Tunnel (START-SERVICE-TUNNEL-CLOUDFLARE.cmd)      |
|             v                                                              |
|   https://api.nexumaltivon.com.br  <---- publico                           |
|                                                                            |
|   +-------------------+                                                    |
|   | Watchtower        |  (auto-pull GHCR, compose prod)                    |
|   +-------------------+                                                    |
+---------------------------------------------------------------------------+
          ^                            ^
          |                            |
+---------+---------+        +---------+---------+
| Desktop WPF       |        | Navegadores       |
| (PDV/Fiscal/      |  ---API--->  Site publico  |
|  Compras/Logistica|  offline   Area Cliente    |
|  /MasterData/Fin) |  outbox    Admin Painel    |
| LocalOutboxService|            (React SPA)     |
+-------------------+            +---------------+

  INTEGRACOES EXTERNAS (HttpClient nomeados na API):
  - Mercado Pago   (api.mercadopago.com)      -> pagamento + webhook
  - Melhor Envio   (melhorenvio.com.br)       -> frete/cotacao
  - Mercado Livre  (api.mercadolibre.com)     -> marketplace
  - SendGrid       (Notificacoes)             -> e-mail transacional
  - GitHub Container Registry (ghcr.io)       -> imagens Docker

  DOCKER (docker/docker-compose.{yml,prod.yml}):
  - servicos: api (8080) + frontend (80) + nginx (80:80) + watchtower
  - nginx proxy reverso (docker/nginx/nginx.conf, frontend.conf)
  - backup/restore MySQL: docker/scripts/backup-mysql.sh / restore-mysql.sh
  - observacao: compose.prod NAO sobe MySQL (banco externo no servidor 192.168.1.72)

  ARTEFATOS DE DDL (scripts SQL no repo):
  - NexumAltivon_Back-End/API/Database/2026-06-29-genesisgest-original-schema.sql
  - NexumAltivon_Back-End/Database/nexum_altivon_schema.sql
  - Database/2026-06-19-corrigir-clientes-confirmacao-email.sql
  - Database/2026-06-19-normalizar-enums-operacionais.sql
  - Database/2026-06-19-sincronizar-6-lojas-operacao.sql      (seed das 6 lojas)
  - Database/erp_schema_update.sql / erp_update_schema.sql


================================================================================
ANALISE: O QUE JA ESTA PRONTO  vs  O QUE RESTA (ate os endpoints por setor)
================================================================================

-------------------------------------
A) PRONTO E OPERACIONAL (verificado no codigo)
-------------------------------------
1. Plataforma base .NET 8
   - Program.cs com 10.731 linhas, ~90 endpoints Minimal API reais, JWT Bearer,
     policies RBAC (SuperAdmin/Admin/Gerente/Financeiro/Fiscal/RH), Swagger,
     HealthChecks (/health, /health/db, /health/db/genesis), CORS por ambiente,
     DataProtection, AutoMapper, HttpClient pool.
2. E-commerce completo (Setor 03 + 06)
   - Lojas (seed de 6 unidades), categorias, produtos (CRUD + upload de imagem),
     cupons, carrinho, checkout, pedidos (CRUD + acompanhar + fluxo-operacional +
     logistica), clientes (CRUD + confirmar-cadastro + portal/enderecos),
     fornecedores (CRUD), dashboard admin (KPIs, faturamento, mais-vendidos).
3. Financeiro (Setor 04) — duas camadas
   - API e-commerce: /api/financeiro/lancamentos (GET/POST/PATCH status).
   - GenesisGest: /api/erp/genesis/financeiro/* (contas pagar/receber, baixas,
     boletos, referencias) + servicos ERP isolados (ContaPagar, ContaReceber,
     FluxoCaixa, DRE, PlanoContas, Banco) com logica de negocio real.
4. Compras/SCM (Setor 05)
   - /api/compras/{painel, solicitacoes, cotacoes, pedidos, entradas} com
     maquina de status (Pendente->Cotado->Pedido->Recebido/RecebidoParcial).
5. PDV + Fiscal (Setor 06)
   - /api/pdv/cockpit, /api/erp/genesis/pdv/vendas, /api/desktop/genesis/pdv/vendas,
     FiscalRoutingEngine (motor real de roteamento CFOP/ICMS),
     /api/fiscal/{pdv/configuracoes, pedidos, simular-roteamento,
     preparar-emissao-manual, rascunho-manual}.
6. RH (Setor 07) — basico
   - /api/erp/genesis/rh/{resumo, colaboradores (CRUD), referencias}.
7. Desktop WPF (PDV offline-first)
   - MainWindow + 7 janelas (PDV, ManualNfe, FiscalRouting, Procurement,
     Logistics, CorporateMasterData, FinancialLedger) + DesktopApiClient +
     LocalOutboxService (contingencia offline) + ModuleWorkspaceWindow.
8. Frontend React 19
   - 14 paginas + 5 componentes de negocio + 46 componentes UI (Radix/shadcn)
     + contextos Auth/Cart + Tailwind + Recharts + react-router 7 + zod.
9. Infra/DevOps
   - 4 workflows (ci-cd, Pages, dotnet-desktop, npm-publish-github-packages),
     Docker (api/frontend/nginx/watchtower), scripts .cmd de operacao servidor
     (instalacao, aplicacao 5012, guardian, tunnel cloudflare), backup MySQL.
10. Schema de dados
   - 125 tabelas no schema GenesisGest original (prefixo adm_*) + schema
     e-commerce nexum_altivon; scripts de normalizacao e seed das 6 lojas.

-------------------------------------
B) PENDENCIAS / GAPS (ate completar infra e endpoints por setor)
-------------------------------------
1. ARQUITETURA — criticos
   B1. Solution quebrada: NexumAltivon.ERP.sln so inclui NexumAltivon.ERP.csproj
       (raiz) + Desktop. Os projetos NexumAltivon_Back-End/API (a API real) e
       NexumAltivon_ERP (camada ERP com services) NAO estao na solution e nao
       sao referenciados -> build da solution nao compila a API de producao.
       [inferred: a API e buildada diretamente via csproj no CI]
   B2. Controllers MVC mortos: existem ~14 [ApiController] em
       NexumAltivon_Back-End/API/Controllers/* mas o Program.cs NAO chama
       AddControllers() nem MapControllers() -> todos esses endpoints estao
       duplicados/inativos. Risco de divergencia de comportamento.
   B3. Duplicacao massiva: Controllers/, Services/, Models/, DTOs/, Data/,
       Configurations/ na raiz do repo sao copias legados excluidas do build
       (via <Compile Remove>) mas ainda versionadas -> confusao e manutencao
       dupla. Recomendado arquivar (ja existe pasta Arquivos_Mortos/).
   B4. Regra de ouro do dominio (Sys_AuditableEntity) NAO implementada:
       modelos usam int Id, sem TenantId, sem RowVersion, sem soft-delete
       padronizado (IsDeleted/DeletedAt). Multitenancy via TenantId ausente.
   B5. MFA, Workflow engine e SSO corporativo (Setor 00) ausentes.

2. POR SETOR — endpoints/servicos faltantes
   Setor 00 (Plataforma/SSO):
     - Falta: MFA (TOTP), refresh-token rotation, SSO (OIDC/SAML),
       workflow de aprovacao, multitenancy real (TenantId por requisicao),
       gerenciamento de permissoes granular (RBAC fino por modulo).
   Setor 01 (Cockpit):
     - Falta: relatorios dinamicos (-builder), exportacao PDF/Excel no cockpit
       (ClosedXML/DinkToPdf estao no csproj mas sem endpoints expostos),
       agendamento de relatorios.
   Setor 02 (GRC/IAM):
     - Falta: CRUD de perfis e permissoes via API, matriz perfilxpermissao,
       trilha de auditoria consultavel (endpoint /api/auditoria*),
       segregacao de funcoes (SoD).
   Setor 03 (Master Data):
     - Falta: cadastro unico de Pessoas (PF/PJ unificadas), centros de custo
       via API (so existem no ERP isolado), itens/servicos corporativos,
       vinculo multiloja de produto com preco por loja.
   Setor 04 (FICO):
     - Falta: integrar os services do NexumAltivon_ERP na API ativa
       (hoje duplicados em ERP/SharedData), contabilidade (lancamentos
       contabeis/razao), conciliacao bancaria via API, DRE via API,
       fechamento contabil, SPED/ECF.
   Setor 05 (SCM/WMS):
     - Falta: endpoints de WMS (inventario, kardex, transferencia, local de
       estoque) — existem como model no NexumAltivon_ERP mas sem controller/
       endpoint na API ativa; portaria (entrada/saida de veiculos/carga);
       cotacao automatica multi-fornecedor.
   Setor 06 (Comercial/CRM/Fiscal):
     - Falta: emissao real de NF-e/NFC-e (integracao com SEFAZ — hoje so ha
       rascunho manual e simulacao), CT-e, MDF-e, CRM avancado (pipeline,
       oportunidade, ticket — models existem no ERP isolado sem endpoint),
       faturamento em lote, integracao de marketplace bidirecional completa.
   Setor 07 (HCM/RH):
     - Falta: ponto eletronico (REP), folha de pagamento,-de desempenho,
       admissao/demissao, eSocial, beneficios. So ha colaborador+referencias.
   Setor 08 (MES/OPS):
     - Falta: 100% do modulo. Nao ha models, services, endpoints nem telas
       de Ordens de Servico, Producao, Manutencao, AP/PP.

3. QUALIDADE / ENTREGA
   B6. Sem testes automatizados (nenhum projeto *.Tests, nenhum arquivo
       .spec/.test no repo).
   B7. Sem observabilidade estruturada (Serilog configurado no csproj mas
       sem sinks/JSON de log definidos; sem OpenTelemetry; sem APM).
   B8. Backup MySQL existe (docker/scripts) mas sem validacao automatizada
       nem restore testado.
   B9. Sem migrate framework (EF Migrations) — schema gerenciado por scripts
       SQL manuais soltos -> risco de drift.
   B10. Secret management: JwtSettings:SecretKey em appsettings + env;
        nao ha cofre (Key Vault/Secrets Manager). appsettings.PrivateProduction
        .template.json indica fluxo manual.
   B11. Documentacao tecnica: so ha README/AGENTS legados (briefing, nao guia
        tecnico). Sem OpenTag exportado, sem diagramas de sequencia.

4. INFRA
   B12. docker-compose.prod.yml nao sobe MySQL nem Redis/cache -> depende de
        banco externo no servidor 192.168.1.72 (single point of failure).
   B13. Sem staging visivel no compose (so production); deploy staging via
        SSH opcional no CI.
   B14. Desktop WPF so roda em Windows (.NET 8-windows); sem canal de
        auto-update formal (watchtower e so para containers Linux).


================================================================================
DIAGRAMA 3 — PROJETO COMPLETO / FUNCIONAL / OPERACIONAL (ecossistema alvo)
================================================================================
Modelo-alvo multiempresarial consolidando o que existe + o que falta. Os blocos
[EXISTE] ja estao implementados; [FALTA] indica o que precisa ser adicionado.

                         +---------------------------------+
                         |     GRUPO NEXUM ALTIVON         |
                         |  (Holding multiempresa / multi |
                         |   loja: Chronos, EstruturaLine, |
                         |   Geracao Top, Gran Festas,     |
                         |   Gran Tur, Moda Mim)           |
                         +---------------------------------+
                                           |
            +------------------------------+------------------------------+
            |                             |                              |
            v                             v                              v
  +---------------------+     +------------------------+     +-----------------------+
  | CANAIS [EXISTE]     |     | APLICACAO DESKTOP [E]  |     | MARKETPLACES [E parc] |
  | - Site .com.br      |     | WPF PDV/Fiscal/Compras |     | ML, MP, Melhor Envio  |
  |   (GitHub Pages)    |     | Logistica/MasterData/  |     | [FALTA: bidirecional  |
  | - Area Cliente [E]  |     | Financeiro (offline)   |     |  completo, B2W, Via]  |
  | - Admin Painel [E]  |     +-----------+------------+     +-----------+-----------+
  +----------+----------+                 |                              |
             |                            |   HTTPS/JWT                  |
             +-------------+--------------+------------------------------+
                           |
                           v
         +-----------------------------------------------+
         |   API GATEWAY / BFF  (.NET 8 Minimal API)    |  [EXISTE — 90 endpoints]
         |   NexumAltivon_Back-End/API/Program.cs       |
         |   JWT Bearer + RBAC + Swagger + HealthChecks |
         +------+-----------------+---------------------+
                |                 |
                |                 |  [FALTA: MapControllers OU remover
                |                 |   14 controllers MVC mortos; expor
                |                 |   services do NexumAltivon_ERP]
                |                 |
   +------------+------------+    |    +---------------------------------+
   |  CAMADA APLICACAO       |    |    |  CAMADA DOMINIO/ERP [E+P]       |
   |  (Services API) [EXISTE]|<---+--->|  NexumAltivon_ERP               |
   |  Auth, Carrinho,        |         |  Financeiro/CRM/Estoque/Fiscal  |
   |  Checkout, Pedido,      |         |  Services + Models + DTOs       |
   |  Produto, Cliente,      |         |  [FALTA: referenciar na solution|
   |  Compras, Financeiro,   |         |   e expor via API; so assim      |
   |  Fiscal, CRM, RH,       |         |   controllers dedicados]         |
   |  Integracao, Dashboard  |         +---------------+------------------+
   +------------+------------+                         |
                |                                       |  EF Core / Pomelo
                |                                       |  [FALTA: EF Migrations]
                v                                       v
         +-----------------------------------------------+
         |  BANCO MySQL 8  (192.168.1.72:3309)  [EXISTE] |
         |  schema nexum_altivon  +  genesis_bd (125 tbl)|
         |  [FALTA: Sys_AuditableEntity (TenantId,       |
         |   RowVersion, IsDeleted, DeletedAt) em toda   |
         |   tabela transacional; soft-delete global;    |
         |   indices multitenant]                        |
         +---+---------------------------------------+---+
             |                                       |
             |  [FALTA: Redis cache / Hangfire store] |  [FALTA: S3/MinIO p/ anexos]
             |                                       |
         +---+-------------+                     +---+---------------+
         |  JOBS [E base]  |                     |  ARQUIVOS [FALTA] |
         |  Hangfire/Quartz|                     |  S3 p/ NF-e XML,  |
         |  [FALTA: jobs de |                    |  anexos, SPED]    |
         |  integracao,     |                     +-------------------+
         |  conciliacao,    |
         |  fechamento]     |
         +------------------+

  === MAPA DE ENDPOINTS POR SETOR (alvo) ===
  (E)=existente  (F)=faltante

  Setor 00 Plataforma/SSO:
   [E]  POST /api/auth/login, GET /api/auth/me, POST /api/sistema/validar-token
   [E]  GET/POST/PUT /api/admin/usuarios*, GET /api/admin/usuarios/perfis
   [F]  POST /api/auth/mfa/enable, POST /api/auth/mfa/verify
   [F]  POST /api/auth/refresh (rotacao), GET /api/tenants, middleware TenantResolver
   [F]  GET/POST/PUT /api/workflows/* (aprovacoes)

  Setor 01 Cockpit:
   [E]  GET /api/admin/dashboard/{completo,kpis}, GET /api/dashboard/resumo
   [E]  GET /api/gestao-corporativa/{painel,dicionario-dados,ciclo-operacional}
   [F]  POST /api/relatorios/dinamico (builder), GET /api/relatorios/{id}/export (pdf/xlsx)
   [F]  POST /api/relatorios/agendar

  Setor 02 GRC/IAM:
   [E]  (policies no JWT, sem endpoints de gestao)
   [F]  GET/POST/PUT/DELETE /api/perfis, /api/permissoes, /api/perfis/{id}/permissoes
   [F]  GET /api/auditoria (com filtros), GET /api/auditoria/{id}

  Setor 03 Master Data:
   [E]  /api/erp/empresas (GET/POST), /api/clientes* , /api/fornecedores*,
        /api/produtos* , /api/categorias*, /api/lojas
   [F]  /api/pessoas (PF/PJ unificadas), /api/centros-custo, /api/itens-servico
   [F]  /api/produtos/{id}/precos-por-loja, /api/fornecedores/{id}/contatos

  Setor 04 FICO:
   [E]  /api/financeiro/lancamentos*, /api/erp/genesis/financeiro/* (contas,
        boletos, referencias, baixas)
   [F]  /api/financeiro/contabil/lanamentos, /api/financeiro/razao,
        /api/financeiro/conciliacao, /api/financeiro/dre, /api/financeiro/fechamento
   [F]  /api/fiscal/sped, /api/fiscal/ecf

  Setor 05 SCM/WMS:
   [E]  /api/compras/{painel,solicitacoes,cotacoes,pedidos,entradas}*
   [F]  /api/estoque/movimentacoes, /api/estoque/inventario, /api/estoque/kardex
   [F]  /api/estoque/locais, /api/estoque/transferencias, /api/estoque/alertas
   [F]  /api/portaria/{entradas,saidas}

  Setor 06 Comercial/CRM/Fiscal:
   [E]  /api/pedidos* , /api/pdv/cockpit, /api/erp/genesis/pdv/vendas*,
        /api/desktop/genesis/pdv/vendas*, /api/fiscal/{pdv/configuracoes,
        pedidos, simular-roteamento, preparar-emissao-manual, rascunho-manual}
   [E]  /api/crm/leads* , /api/integracoes/* , /api/webhooks/mercadopago
   [E]  /api/frete/cotar, /api/logistica/roteamento
   [F]  /api/fiscal/nfe/emitir (SEFAZ real), /api/fiscal/nfce/emitir,
        /api/fiscal/cte, /api/fiscal/mdfe, /api/fiscal/inutilizar,
        /api/fiscal/cancelar, /api/fiscal/cartacorrecao
   [F]  /api/crm/pipelines, /api/crm/oportunidades, /api/crm/tickets,
        /api/crm/atividades, /api/crm/campanhas, /api/crm/segmentos
   [F]  /api/marketplaces/{ml,b2w,via}/sync, /api/faturamento/lote

  Setor 07 HCM/RH:
   [E]  /api/erp/genesis/rh/{resumo,colaboradores,referencias}*
   [F]  /api/rh/ponto, /api/rh/folha, /api/rh/avaliacoes, /api/rh/admissoes,
        /api/rh/demissoes, /api/rh/esocial, /api/rh/beneficios

  Setor 08 MES/OPS:
   [F]  /api/ops/ordens-servico, /api/ops/producao/apontamentos,
        /api/ops/manutencao (preventiva/corretiva), /api/ops/ativos

  === FLUXO OPERACIONAL ALVO (multiempresarial) ===
  Venda(PDV/Ecom) -> Pedido -> Fiscal(NF-e/NFC-e real) -> Estoque(saida/Kardex)
     -> Financeiro(conta a receber) -> Conciliacao -> Contabilidade -> DRE/SPED
  Compra -> Solicitacao -> Cotacao -> Pedido -> Recebimento(Portaria) ->
     Estoque(entrada/Kardex) -> Financeiro(conta a pagar) -> Conciliacao
  RH admissao -> Ponto -> Folha -> Contabilidade -> eSocial
  Tudo auditado (Sys_AuditableEntity) e isolado por TenantId por empresa/loja.


================================================================================
SCRIPTS SQL DO PROJETO (referencia)
================================================================================
- NexumAltivon_Back-End/API/Database/2026-06-29-genesisgest-original-schema.sql
    Schema GenesisGest original: 125 CREATE TABLE (adm_auditoria, adm_empresas,
    adm_perfis, adm_pessoas, ...). Base segura (IF NOT EXISTS, sem drop).
- NexumAltivon_Back-End/Database/nexum_altivon_schema.sql
    Schema do e-commerce Nexum (lojas, produtos, clientes, pedidos, pagamentos).
- Database/2026-06-19-corrigir-clientes-confirmacao-email.sql
    Corrige fluxo de confirmacao de e-mail de clientes.
- Database/2026-06-19-normalizar-enums-operacionais.sql
    Normaliza enums operacionais (status de pedido, pagamento, papel de usuario).
- Database/2026-06-19-sincronizar-6-lojas-operacao.sql
    Seed/sincronizacao das 6 lojas operacionais (Chronos, EstruturaLine,
    Geracao Top, Gran Festas, Gran Tur, Moda Mim).
- Database/erp_schema_update.sql / erp_update_schema.sql
    DDL incremental dos modulos ERP (financeiro/fiscal/estoque).

================================================================================
FIM DO RELATORIO
================================================================================
