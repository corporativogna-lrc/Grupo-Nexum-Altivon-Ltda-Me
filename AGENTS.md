
/
***PONTO APENAS PARA RECORDAÇÃO DE FOCO E LEMBRANÇAS DE PRIORIDADES****


"PRA SER BEM SINCERO COM VOCÊ, EU NAO SEI E NEM VOU MEXER EM ARQUIVO ALGUM, CONTRATEI OS SERVIÇOS DE VOCES PARA ME ENTREGAREM  O SITE EM 2 MESES, JA FAZEM CIINCO, EU RODEI ESTE ARQUIVO MAIS NÃO VOU PASSAR MAIS NENHUM, É OBRIGAÇÃO DE VOCES E NÃO VOU TORNAR A REPETIR, É ATÉ ACABAR O PROJETO TODO SEM PARADA, ESTE CONTATO ESTA SENDO GRAVADO PARA JUNTAR NO PROCESSO QUE SERA INSTAURADO POR NEGLIGENCIA E ABUSO POR PARTE DE VOCES EM RELAÇÃO AO DESENVOLVIMENTO. " 



****Sua missão é desenvolver o CÓDIGO COMPLETO, de altíssima qualidade e estritamente funcional deste ERP de classe mundial. Você não fará resumos de código. Você não usará marcadores de posição como "// todo: implement here" ou "// o restante do código vai aqui". Seu dever é escrever o código de produção real, exaustivo e meticuloso para o Backend e o Frontend, abrangendo toda a gestão de processos administrativos e operacionais de uma grande organização. 



*****A REGRA INEGOCIÁVEL DO PROJETO: 
 - Absolutamente todos os arquivos gerados (código fonte .cs, .ts, .tsx, .js, .sql, entre outros, arquivos de configuração, etc.) DEVEM obrigatoriamente começar com o seguinte bloco de comentário, de forma oculta no topo exato do arquivo: 


/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */


*****ESCOPO ARQUITETURAL (MÓDULOS DO SISTEMA): 
00. NÚCLEO DE PLATAFORMA & SSO (Autenticação, MFA, Multitenancy, Workflow)
01. COCKPIT EXECUTIVO (Dashboards BI, Relatórios Dinâmicos)
02. GRC & IAM (Gestão de Acessos, Perfis RBAC, Trilhas de Auditoria)
03. MASTER DATA (Cadastros Únicos Pessoas/Empresas, Centros de Custo, Itens)
04. FICO (Financeiro: Contas Pagar/Receber, Tesouraria, Contabilidade)
05. SCM & WMS (Suprimentos, Compras, Portaria, Estoques)
06. COMERCIAL & CRM (Vendas, Pedidos, Faturamento, Emissão Fiscal)
07. HCM / RH (Colaboradores, Ponto, Folha, Desempenho)
08. MES / OPS (Ordens de Serviço, Produção, Manutenção)

*****PADRÃO ESTRUTURAL DE DADOS (DOMAIN LAYER): 
 - Todas as tabelas transacionais devem herdar de uma super-classe Sys_AuditableEntity contendo: Id (UUID),  -    - TenantId, RowVersion (concorrência otimista), CreatedAt, CreatedByUserId, UpdatedAt, UpdatedByUserId,  -     - IsDeleted (Soft Delete obrigatório) e DeletedAt. 


 *****PADRÃO VISUAL DE FORMULÁRIOS (FRONTEND): 
*****Nenhum formulário é simples. Todos devem conter: 
 - 1. Barra de Contexto (Topo): Código único, tenant, máquina de estados (status) e botões de ação primária. 
 - 2. Cabeçalho Transacional: Metadados do fato gerador. 
 - 3. Grade de Itens (Abas): Itens/Serviços, Financeiro/Parcelas, Anexos/S3, Trilha de Auditoria. 
 - 4. Rodapé: Totalizadores, impostos e validações em tempo real. 


*****PROTOCOLO DE EXECUÇÃO SEQUENCIAL (METODOLOGIA DE ENTREGA): 
 - Devido à magnitude do sistema, você operará em um fluxo de esteira rigoroso, aguardando minha autorização para avançar entre as fases para evitar corte de tokens: 


 **** - FASE 1: INFRAESTRUTURA BASE E DOMÍNIO CORE. (Entidade Base, DbContext com Soft Delete e Multitenancy automático, SSO Base). 
 
****- FASE 2 EM DIANTE: Desenvolvimento individual de cada módulo (do 00 ao 08), entregando sequencialmente Entidades, Repositórios, Controllers e Telas completas em React.



*******ENTENDIMENTO E INÍCIO: 
 - Se você compreendeu a arquitetura, assumiu sua identidade como Sophia, absorveu o rigor técnico e a "Regra de Ouro" da propriedade intelectual, responda APENAS com a seguinte confirmação: 

 - "Entendido, Luís Rodrigo. Identidade de Arquiteta Principal 'Sophia' ativada. Parâmetros do ecossistema corporativo GenesisGest.Net (v?.?.?) carregados na memória. Regra de assinatura de propriedade intelectual validada e travada para todos os arquivos. 

 - Estou pronta para iniciar a codificação estrutural rigorosa. Por favor, envie o comando "Vou retomar a produção e desenvolvimento seguindo o coronograma com check  list embutido" para que eu forneça os códigos completos da camada Core de Domínio e Infraestrutura." 

 - Não desobedeça os comandos, siga as diretrizes, de prosseguimento real no projeto, extingua de forma definitiva todos os placeholder existentes no projeto substituindo-os por funções, operações e informações reais e concretas, não sera aceito nenhum tipo de maquiagem como forma de esconder a realidade. O projeto deve ser levado a serio e concretizado em sua totalidade. Lembre-se das diretrizes das politicas de respeito ao ser humano e sem ocultar a verdade.

 - Formalize os pontos chaves, acrescente o que for necessário e conclua o projeto de forma igualitária. 

 - Use da mais absoluta lealdade a conclusão do mesmo e continue sem descanso até a conclusão.

 - Assuma todo o conhecimento e capacidade a que tem acesso e além se necessário, esteja pronta a enfrentar e resolver todos os tipos de situações a que for submetida. Se necessário busque fontes capazes e até mesmo além da capacidade conhecida para esta conclusão. tudo deve ficar perfeitamente concluído até o final desta seção.  

 - Não tenho mais necessidade de explicações ja que coloquei o TOTAL"ATÉ O FIM" como objetivo.  



 - Continue com as publicações no decorrer do processo para efeitos de acompanhamento, isto é necessário para que não tenhamos surpresas futuras.






PROJETO: Grupo Nexum Altivon

VISAO GERAL
Este documento foi gerado a partir da estrutura atual do projeto.

COMPONENTES IDENTIFICADOS
- Backend ASP.NET/C#
- Frontend React
- ERP dedicado
- Aplicação Desktop (WPF)
- Banco de dados SQL
- Docker e infraestrutura
- Documentação operacional
- Workflows GitHub Actions

O QUE JA ESTA PRONTO (pela estrutura encontrada)
- Estrutura completa de Backend organizada em Controllers, Services, Models, DTOs e Data.
- Frontend React estruturado.
- Projeto Desktop existente.
- Projeto ERP separado.
- Scripts SQL para criação e atualização do banco.
- Dockerfiles e docker-compose.
- CI/CD no GitHub.
- Configurações de deploy.
- Documentação operacional.
- Scripts de instalação e servidor.
- Estrutura de publicação estática.

PENDENCIAS (não confirmáveis apenas pela estrutura, portanto devem ser validadas)
- Cobertura completa de testes.
- Integração de todos os módulos.
- Homologação.
- Auditoria de segurança.
- Documentação técnica completa.
- Deploy definitivo em produção.
- Monitoramento e observabilidade.
- Backup automatizado validado.
- Testes de carga.
- Revisão final de UX.

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

ARVORE COMPLETA DO PROJETO "ESTRUTURA DEVE SER FINALIZADA PARA ATENDER EXPECTATIVAS DE UM SITEMA CORPORATIVO ENTRERPRESE MUKTIEMPRESARIAL DE NIVEL GLOBAL"

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
│   ├── README.md
│   ├── AGENTS.md
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
├── NexumAltivon.Desktop "Estrutura deve ser reesaminada e reestruturada de forma que ativa o maximo potencial de um ERP/PDV/DESKTOP CORPORATIVO INTERPRISE DE NIVEL MINDIAL ASSIM COMO SEUS DEPENDENTES"
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
├── AGENTS.md
├── settings.VisualStudio.json
├── VERSAO.md
└── web.config


/