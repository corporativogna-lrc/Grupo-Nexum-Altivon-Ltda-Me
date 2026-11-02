<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7184
-->

# NEXUM ALTIVON COMMERCE PLATFORM
## FASE 1 — Estrutura Base, Banco, API e Autenticação

### Grupo Nexum Altivon ME — www.nexumaltivon.com

---

## Estrutura do Projeto

```
NexumAltivon.com
|
|-- NexumAltivon_Back-End/
|   |-- API/
|   |   |-- Controllers/          → 8 Controllers RESTful
|   |   |-- Services/             → 15 Services (business logic)
|   |   |-- DTOs/                 → 8 arquivos de DTOs
|   |   |-- Models/               → 23 Entidades EF Core
|   |   |--44 Data/                 → DbContext + Migrations
|   |   |-- Middleware/           → Exception, Rate Limit, Auditoria
|   |   |-- Helpers/              → Health Checks
|   |   |-- Validators/           → FluentValidation
|   |   |-- Mappings/             → AutoMapper Profile
|   |   |-- Program.cs            → Entry point configurado
|   |   |-- appsettings.json      → ConnectionString + JWT + Integracoes
|   |-- Database/
|   |   |-- nexum_altivon_schema.sql  → Script SQL completo
|   |-- NexumAltivon.API.csproj → Dependencias NuGet
|
|-- NexumAltivon_Front-End/
|   |-- index.html                → Home Page original (preservada)
|
|-- NexumAltivon_ERP/
    |-- Controllers/
    |-- Models/
    |-- Services/

GenesisGest.Net - Central Desktop ERP/PDV
|
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
| `/api/auth/login` | `POST` | Publico | Login com JWT |
| `/api/auth/refresh` | `POST` | Publico | Renova token |
| `/api/auth/registrar` | `POST` | SuperAdmin | Novo usuario |
| `/api/lojas` | `GET` | Publico | Lista 6 lojas |
| `/api/lojas/{id}` | `GET` | Publico | Detalhes da loja |
| `/api/produtos` | `GET` | Publico | Lista produtos |
| `/api/produtos/destaques` | `GET` | Publico | Produtos em destaque |
| `/api/clientes` | `GET` | Vendedor | Lista clientes |
| `/api/clientes` | `POST` | Publico | Cadastro cliente |
| `/api/carrinho` | `GET` | Publico | Ver carrinho |
| `/api/carrinho/adicionar` | `POST` | Publico | Adicionar item |
| `/api/pedidos` | `GET` | Vendedor | Lista pedidos |
| `/api/pedidos` | `POST` | Publico | Criar pedido |
| `/api/crm/leads` | `GET` | Vendedor | Lista leads |
| `/api/crm/leads` | `POST` | Publico | Novo lead |
| `/api/financeiro/faturamento` | `GET` | Financeiro | Faturamento por periodo |
| `/health` | `GET` | Publico | Health check |
| `/swagger` | `GET` | Dev/Staging | Documentacao API |

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
Abrir NexumAltivon_Front-End/index.html em qualquer navegador.

---

## Proximas Fases

| Fase | Entregaveis |
|---|---|
| FASE 2 | Painel Admin, Dashboard, Gestao Completa |
| FASE 3 | Carrinho Funcional, Checkout, Gateway Pagamento |
| FASE 4 | Logistica, Marketplaces, Dropshipping |
| FASE 5 | CRM Avancado, Automacoes, IA, Analytics |

---

<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7226 Data: 16/08/2026
-->

# Arvore Real e Arvore Meta do GenesisGest.Net

Apuracao tecnica atualizada: 2026-08-16.

O mapa versionado de menu, formularios e estados do Desktop/PDV esta em docs/ARVORE-MENU-WPF-2026-08-16.md.

## Regra de Evidencia

O arquivo `ANALISE_ESTRUTURAL_GRUPO_NEXUM_ALTIVON2.txt` foi usado como referencia historica de amplitude, nao como prova de implementacao. Uma capacidade somente integra a arvore real quando existe no projeto oficial e possui evidencia verificavel. A arvore meta preserva as capacidades uteis do documento, mas exige API oficial, autorizacao, persistencia quando aplicavel, tratamento de erro, interface oficial e validacao operacional antes da conclusao.

Classificacoes usadas:

- `Comprovado`: contrato e comportamento foram validados no runtime oficial.
- `Parcial`: existe implementacao real, mas falta pelo menos um gate obrigatorio.
- `Bloqueado externamente`: o codigo recusa operacao sem credencial, certificado ou aceite do provedor e nao registra sucesso.
- `Nao comprovado`: nao existe evidencia suficiente para uso produtivo.

## Inventario Estrutural Atual

| Camada | Evidencia em 2026-07-18 |
|---|---|
| Solution .NET | 5 projetos: API, API.Tests, ERP, Desktop e projeto raiz |
| API oficial | 236 rotas Minimal API em `NexumAltivon_Back-End/API/Program.cs` |
| Frontend | 16 paginas React em `NexumAltivon_Front-End/src/pages` |
| Desktop | 14 arquivos XAML funcionais fora de `bin/obj` |
| Banco Nexum | 203 tabelas base e 12 views no schema `nexum_altivon` |
| Banco Genesis | 48 tabelas base no schema `genesis_bd` |
| Runtime | API unica em `127.0.0.1:5010`, tarefa `NexumAltivonApi24h` como `SYSTEM`, gatilho de boot e processo oculto |
| Exposicao | Cloudflared automatico; health local e publico comprovado |

## Arvore Real

```text
GenesisGest.Net v1.1.5
|-- Plataforma oficial
|   |-- API Minimal .NET 8 [comprovada em runtime]
|   |-- React 19 [build e publicacao existentes]
|   |-- Desktop WPF .NET 8 [solution e acesso local existentes]
|   `-- MySQL 8: nexum_altivon + genesis_bd [health comprovado]
|-- 00 Nucleo e SSO [parcial avancado]
|   |-- Login, JWT, refresh rotativo e logout
|   |-- MFA TOTP
|   |-- Usuarios, perfis, permissoes e RBAC
|   |-- Tenants [isolamento integral ainda nao comprovado]
|   `-- Workflow [backend comprovado; telas por dominio pendentes]
|-- 01 Cockpit executivo [parcial]
|   |-- Resumo e KPIs por API
|   `-- Grafico semanal estatico ainda presente no Dashboard React
|-- 02 GRC e IAM [parcial avancado]
|   |-- Gestao administrativa React/WPF
|   `-- Auditoria consultavel [cobertura de toda escrita pendente]
|-- 03 Master Data [parcial]
|   |-- Empresas, clientes, fornecedores, produtos e categorias
|   |-- Pessoas, centros de custo e itens/servicos
|   `-- Gestao dedicada das seis lojas pendente
|-- 04 FICO [parcial]
|   |-- Lancamentos, razao, conciliacao, DRE e fechamento
|   |-- PDFs de contas a pagar/receber integrados no WPF e homologados na API local/publica com dados MySQL
|   `-- PDF de DRE/razao/fechamento ainda pendente de fatos contabeis completos
|-- 05 SCM e WMS [parcial]
|   |-- Compras, solicitacoes, cotacoes, pedidos e entradas
|   `-- WMS completo e portaria ainda sem homologacao ponta a ponta
|-- 06 Comercial, CRM e Fiscal [parcial]
|   |-- Pedidos, PDV, checkout e catalogo
|   |-- Cupons [CRUD e uso publico comprovados]
|   |-- Campanhas e segmentos [CRUD comprovado]
|   |-- Checkout com incidente fiscal/pos-commit duravel
|   |-- Pre-emissao fiscal e reserva atomica de numero
|   `-- SEFAZ e marketplaces [bloqueados externamente sem credenciais]
|-- 07 HCM e RH [parcial]
|   `-- Resumo e colaboradores; folha, ponto e eSocial sem aceite integral
|-- 08 MES e OPS [parcial]
|   |-- Rotas de OS, producao, manutencao e ativos
|   `-- Telas React/WPF especificas e fluxo integral pendentes
|-- Integracoes [parcial]
|   |-- Frete interno oficial identificado
|   |-- Tracking externo bloqueado sem provedor
|   |-- Yara no portal e Sophia no administrativo, sem resposta fabricada
|   `-- Anexos/MinIO, Redis e jobs ainda exigem homologacao produtiva
`-- Operacao 24h [comprovada no servidor atual]
    |-- MySQL, Cloudflared e Agendador automaticos
    `-- API sobe no boot sem login interativo
```

## Arvore Meta de Aceite Integral

```text
GenesisGest.Net concluido
|-- Plataforma e seguranca
|   |-- Tenant, auditoria, concorrencia e soft-delete em toda entidade transacional
|   |-- Segregacao de funcoes e autorizacao comprovadas por modulo
|   |-- Segredos somente em armazenamento privado e rotacionavel
|   `-- Banco canonico unico apos UBD-01 a UBD-08
|-- Experiencia multicanal
|   |-- React dinamico, responsivo e administravel por midia real
|   |-- WPF com paridade funcional, navegacao limpa e visual fume translucido
|   `-- Nenhum botao confirma operacao sem resposta persistida
|-- Operacao corporativa 00 a 08
|   |-- Cada formulario com contexto, cabecalho, itens, financeiro, anexos e auditoria
|   |-- Cada escrita com transacao, tenant, RBAC, concorrencia e trilha
|   `-- Cada leitura derivada de dados atuais, sem indicadores estaticos
|-- Cadeia ponta a ponta
|   |-- Cadastro -> venda -> pagamento -> estoque
|   |-- Estoque -> fiscal homologado -> logistica
|   `-- Logistica -> financeiro -> contabil -> BI
|-- Integracoes externas
|   |-- SEFAZ com chave e protocolo de homologacao persistidos
|   |-- Marketplace com identificador aceito pelo seller persistido
|   |-- Tracking com evento retornado pelo transportador
|   `-- IA com chave administrada, custo controlado e erro rastreavel
|-- Resiliencia e operacao
|   |-- Redis, jobs, anexos, observabilidade e backups homologados
|   |-- Restore semanal comprovado
|   |-- Staging e producao com migrate e health gates
|   `-- Auto-update Desktop validado em maquina cliente
`-- Qualidade
    |-- Solution e frontend compilam sem erro
    |-- Services mantem cobertura minima de 70%
    |-- Contratos consumidos nao retornam 404
    `-- Definition of Done em 31/31 com evidencia anexada ao checklist
```

## Capacidades Recuperadas e Aprofundadas

| ID | Capacidade | Estado atual | Gate para conclusao |
|---|---|---|---|
| CAP-01 | Cockpit executivo dinamico | Parcial avancado | Series estaticas removidas e periodos 7/30/90 reconciliados entre API local, API publica e MySQL; falta provar isolamento entre dois tenants e homologar a tela publicada no Chrome |
| CAP-02 | Venda omnicanal | Parcial | Provar pedido, pagamento, reserva/baixa, fiscal, entrega e financeiro na mesma correlacao |
| CAP-03 | Compras e abastecimento | Parcial | Provar solicitacao, cotacao, aprovacao, pedido, entrada, estoque e financeiro |
| CAP-04 | WMS | Parcial | Provar movimentacao, inventario, kardex, localizacao e transferencia com concorrencia |
| CAP-05 | Fiscal explicavel | Parcial | Exibir roteamento, motivo, numeracao, XML, chave, protocolo e eventos oficiais |
| CAP-06 | FICO corporativo | Parcial avancado | Contas a pagar/receber, baixas, PDFs de pagar/receber e conciliacao bancaria usam janelas, contratos, persistencia, tenant, concorrencia e auditoria proprios; PDFs foram homologados na API local/publica e o acionamento WPF compilado; concluir homologacao interativa WPF, tesouraria, fechamento, razao e DRE |
| CAP-07 | Logistica e fulfillment | Parcial | Provar cotacao, expedicao, rastreamento externo, entrega e ocorrencias |
| CAP-08 | CRM e atendimento Yara | Parcial bloqueado por credencial | Provar jornada comercial com chave de projeto OpenAI real, streaming, ferramentas autorizadas de catalogo/estoque/preco/carrinho/checkout, transferencia humana, limite de custo e protecao de dados sigilosos; sem seletor de persona e sem resposta fabricada |
| CAP-09 | HCM | Parcial | Provar admissao, ponto, folha, beneficios, avaliacao, desligamento e eSocial |
| CAP-10 | MES e OPS | Parcial | Entregar telas React/WPF e provar OS, apontamento, manutencao e ativos |
| CAP-11 | Paridade Desktop | Parcial avancado | Contas a pagar/receber e conciliacao bancaria confirmam releitura; substituir as cinco janelas genericas e reconciliar 100% da matriz tabela-endpoint-formulario-permissao |
| CAP-12 | Marketplaces e dropshipping | Bloqueado externamente | Homologar cada seller e provedor com credencial e identificador oficial |
| CAP-13 | Portal visual dinamico | Parcial avancado | API/MySQL, React e WPF ja administram banners, cores, seis lojas, parceiros, contatos autorizados e produtos vinculados; concluir publicacao do bundle e homologacao Chrome desktop/mobile |
| CAP-14 | Operacao observavel | Parcial | Comprovar logs, traces, metricas, jobs, Redis, backup e restore |
| CAP-15 | Banco canonico | Pendente programado | Cumprir inventario, linhagem, equivalencias, migracao, reconciliacao e reversao |

## Prioridade de Execucao

1. Encerrar erros que possam confirmar venda ou escrita sem registrar incidentes.
2. Substituir dados estaticos e acoes apenas visuais por contratos persistidos.
3. Completar um fluxo vertical por vez, preservando os gates comuns de seguranca.
4. Homologar integracoes externas somente com credenciais oficiais e retorno do provedor.
5. Consolidar React e WPF sobre a mesma API antes da futura unificacao fisica dos schemas.

Este mapa nao altera a medicao do checklist: capacidades parciais permanecem fora do numerador ate o atendimento integral do respectivo gate.


(c) 2026 Grupo Nexum Altivon ME — Todos os direitos reservados
