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
|   |-- public/index.html         → Template oficial React/GitHub Pages
|   |-- public/api-runtime.json   → API oficial publicada
|   |-- src/                      → Aplicacao operacional React
|
|-- Arquivos_Mortos/
|   |-- frontend-legado-20260629/ → HTMLs antigos retirados da operacao
|
|-- NexumAltivon_ERP/
    |-- Controllers/
    |-- Models/
    |-- Services/
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


 -->
# NEXUM ALTIVON COMMERCE PLATFORM
## FASE 2 — Painel Administrativo, Dashboard, Gestao Completa

### Grupo Nexum Altivon ME — www.nexumaltivon.com

---

## Entregaveis da FASE 2

### 1. Back-End — API Admin Dashboard

| Arquivo | Descricao |
|---|---|
| `Services/Admin/AdminDashboardService.cs` | Servico completo de KPIs, graficos e metricas |
| `Controllers/Admin/DashboardController.cs` | 8 endpoints REST para dashboard |
| `DTOs/Admin/DashboardDtos.cs` | 8 DTOs de KPIs, faturamento, vendas, produtos, clientes, leads |

#### Endpoints Admin (requer Gerente+)

| Endpoint | Metodo | Descricao |
|---|---|---|
| `/api/admin/dashboard/completo` | GET | Dashboard completo com tudo |
| `/api/admin/dashboard/kpis` | GET | Apenas KPIs principais |
| `/api/admin/dashboard/faturamento/semanal` | GET | Grafico de faturamento 7 dias |
| `/api/admin/dashboard/faturamento/mensal` | GET | Grafico de faturamento 12 meses |
| `/api/admin/dashboard/vendas/lojas` | GET | Vendas por loja (pie chart) |
| `/api/admin/dashboard/produtos/mais-vendidos` | GET | Top produtos (bar chart) |
| `/api/admin/dashboard/clientes/recentes` | GET | Clientes recentes |
| `/api/admin/dashboard/pedidos/recentes` | GET | Pedidos recentes |
| `/api/admin/dashboard/leads/recentes` | GET | Leads recentes |

### 2. Front-End — Painel Administrativo

| Arquivo | Descricao |
|---|---|
| `NexumAltivon_Front-End/src/pages/Dashboard.js` | Painel administrativo operacional integrado a API oficial |
| `Arquivos_Mortos/frontend-legado-20260629/admin-index-mock-legado.html` | Painel HTML antigo arquivado, fora da publicacao |

#### Funcionalidades do Painel

- **KPI Cards**: 6 cards com faturamento, pedidos, clientes, ticket medio, estoque baixo, leads
- **Grafico Faturamento Semanal**: Line chart com 7 dias
- **Grafico Vendas por Loja**: Doughnut chart com as 6 lojas
- **Grafico Faturamento Mensal**: Bar chart com 12 meses
- **Grafico Produtos Mais Vendidos**: Horizontal bar chart
- **Tabela Pedidos Recentes**: Ultimos 5 pedidos com status
- **Tabela Leads Recentes**: Ultimos 5 leads com prioridade
- **Tabela Clientes Recentes**: Ultimos 5 clientes com gasto total
- **Menu Lateral**: 15 secoes navegaveis (Dashboard, Pedidos, Produtos, Clientes, Lojas, Financeiro, Fiscal, Logistica, CRM, Cupons, Marketing, Marketplaces, Dropshipping, Usuarios, Configuracoes, Auditoria)
- **Design**: Escuro premium, cores gold #C9A227, identico ao site principal

#### Secoes do Menu (todas estruturadas)

1. **Dashboard** — Completo com KPIs e graficos
2. **Pedidos** — Tabela com filtros e exportacao
3. **Produtos** — Catalogo com estoque e acoes
4. **Clientes** — Base completa com perfis
5. **Lojas** — Cards das 6 lojas com metricas
6. **Financeiro** — KPIs financeiros
7. **Fiscal** — pendente de credenciais fiscais/certificado digital para emissao real
8. **Logistica** — em integracao real por transportadoras e regras de envio
9. **CRM** — Tabela de leads completa
10. **Cupons** — fluxo operacional vinculado a regras comerciais
11. **Marketing** — fluxo operacional vinculado a leads e notificacoes
12. **Marketplaces** — aguardando tokens reais para ativacao plena
13. **Dropshipping** — aguardando credenciais de fornecedores para ativacao plena
14. **Usuarios** — fluxo operacional por perfis administrativos
15. **Configuracoes** — Painel de configuracoes editaveis
16. **Auditoria** — trilha operacional para eventos e alteracoes criticas

---

## KPIs Implementados

| KPI | Fonte |
|---|---|
| Faturamento Hoje/Mes/Ano | Soma de pedidos aprovados |
| Pedidos Hoje/Mes/Pendentes/Enviados/Entregues | Count por status |
| Clientes Novos/Ativos/Total | Count de clientes |
| Ticket Medio | AVG(total) dos pedidos |
| Taxa de Conversao | Pedidos / Visitas (placeholder) |
| Produtos Ativos/Estoque Baixo/Sem Estoque | Count por condicao |
| Leads Novos/Convertidos/Em Atendimento | Count por status |

---

## Tecnologias Front-End do Painel

- Chart.js 4.4.1 (graficos)
- Font Awesome 6.5.0 (icones)
- Google Fonts Montserrat (tipografia)
- CSS Grid/Flexbox (layout responsivo)
- JavaScript Vanilla (sem frameworks)

---

## Proxima Fase

| Fase | Entregaveis |
|---|---|
| **FASE 3** | Carrinho Funcional, Checkout, Gateway Pagamento |
| **FASE 4** | Logistica, Marketplaces, Dropshipping |
| **FASE 5** | CRM Avancado, Automacoes, IA, Analytics |

---

(c) 2026 Grupo Nexum Altivon ME — Todos os direitos reservados



## Grupo Nexum Altivon — www.nexumaltivon.com

### Arquivos Entregues

| # | Arquivo | Descrição |
|---|---------|-----------|
| 1 | `DTOs/CarrinhoDtos.cs` | DTOs do carrinho e itens |
| 2 | `DTOs/CheckoutDtos.cs` | DTOs de checkout, endereço, frete e finalização |
| 3 | `DTOs/PagamentoDtos.cs` | DTOs de pagamento, webhook e reembolso |
| 4 | `Services/CarrinhoService.cs` | Carrinho com sessão + cliente, cupom, migração |
| 5 | `Services/CheckoutService.cs` | Fluxo completo de checkout |
| 6 | `Services/MercadoPagoService.cs` | Gateway PIX, Cartão e Boleto + webhooks |
| 7 | `Services/FreteService.cs` | Cálculo de frete (Melhor Envio + tabela própria) |
| 8 | `Services/NotificacaoService.cs` | E-mail (SendGrid) + WhatsApp |
| 9 | `Services/PedidoService.cs` | Geração de pedido e número NXYYMMDDXNNN |
| 10 | `Controllers/CarrinhoController.cs` | API pública de carrinho (anônima) |
| 11 | `Controllers/CheckoutController.cs` | API protegida de checkout |
| 12 | `Controllers/PagamentoController.cs` | Gestão de pagamentos e reembolsos |
| 13 | `Controllers/WebhookController.cs` | Recepção de webhooks Mercado Pago |
| 14 | `Models/CarrinhoCheckoutPagamento.cs` | Entidades EF Core complementares |
| 15 | `Configurations/ServiceExtensions.cs` | Registro de DI dos novos serviços |

### Configurações necessárias em `appsettings.json`

```json
{
  "Integracoes": {
    "MercadoPago": {
      "AccessToken": "TEST-xxxxxxxxxxxxxxxx",
      "WebhookSecret": "seu_secret",
      "Sandbox": true
    },
    "MelhorEnvio": {
      "Ativo": false,
      "Token": "seu_token",
      "CepOrigem": "01001000"
    },
    "SendGrid": {
      "ApiKey": "SG.xxxxx",
      "FromEmail": "naoresponder@nexumaltivon.com",
      "FromName": "Grupo Nexum Altivon"
    },
    "WhatsApp": {
      "Ativo": false,
      "ApiUrl": "http://sua-api-whatsapp:8080/message/sendText",
      "ApiKey": "sua_chave"
    }
  },
  "Alertas": {
    "EstoqueEmailAdmin": "vinicius@nexumaltivon.com"
  }
}
```

### Fluxo de Uso

1. **Cliente anônimo** adiciona itens ao carrinho (cookie `nx_session_id`)
2. Ao **logar**, chama `POST /api/carrinho/migrar` para unir carrinhos
3. Inicia checkout: `POST /api/checkout/iniciar` (endereço + cupom)
4. Seleciona frete: `POST /api/checkout/{id}/frete`
5. Finaliza: `POST /api/checkout/finalizar` (PIX, Cartão ou Boleto)
6. Recebe QR Code / link / boleto na resposta
7. Webhook MP atualiza status automaticamente para "PAGO"

### Próxima Fase
- **FASE 4**: Integrações com Marketplaces (Mercado Livre, Shopee), Dropshipping e Logística completa.


## Grupo Nexum Altivon — www.nexumaltivon.com

### Arquivos Entregues

| # | Arquivo | Descrição |
|---|---------|-----------|
| 1 | `DTOs/MarketplaceDtos.cs` | DTOs Mercado Livre, Shopee, Amazon, Hub unificado |
| 2 | `DTOs/DropshippingDtos.cs` | DTOs de roteamento, comissão, fornecedores |
| 3 | `DTOs/LogisticaDtos.cs` | DTOs de etiquetas, rastreamento, dashboard |
| 4 | `DTOs/ErpSyncDtos.cs` | DTOs de sincronização GenesisGest.Net |
| 5 | `Services/MercadoLivreService.cs` | Publicar, atualizar, importar pedidos ML |
| 6 | `Services/MarketplaceHubService.cs` | Hub multi-canal: Shopee, Amazon, sync automático |
| 7 | `Services/DropshippingService.cs` | Roteamento inteligente, comissões, notificações |
| 8 | `Services/LogisticaService.cs` | Etiquetas, rastreamento, status de entrega |
| 9 | `Services/ErpSyncService.cs` | Bridge GenesisGest.Net (produtos, clientes, pedidos, estoque) |
| 10 | `Services/MarketplaceSyncService.cs` | Orquestrador de sync automático e logs |
| 11 | `Controllers/IntegracoesController.cs` | Hub unificado REST (marketplaces, dropshipping, logística, ERP) |
| 12 | `Models/IntegracoesModels.cs` | Entidades: MarketplaceProduto, DropshippingPedido, Fornecedor, Transportadora, Etiqueta, SyncLog |
| 13 | `Configurations/IntegrationExtensions.cs` | Registro de DI das integrações |
| 14 | `README_Fase4.md` | Este documento |

### Endpoints da API de Integrações

#### Marketplaces
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/marketplaces/sync` | POST | Admin/Gerente | Sincroniza produto em canais |
| `/api/integracoes/marketplaces/sync-lote` | POST | Admin/Gerente | Sincroniza lote de produtos |
| `/api/integracoes/marketplaces/relatorio` | GET | Admin/Gerente | Relatório de sync por período |
| `/api/integracoes/marketplaces/status/{id}` | GET | Admin/Gerente | Status de sync de produto |
| `/api/integracoes/mercadolivre/publicar/{id}` | POST | Admin/Gerente | Publica produto no ML |
| `/api/integracoes/mercadolivre/importar-pedidos` | POST | Admin/Gerente | Importa pedidos pendentes do ML |
| `/api/integracoes/mercadolivre/marcar-enviado/{id}` | POST | Admin/Gerente | Marca pedido ML como enviado |

#### Dropshipping
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/dropshipping/roteiar` | POST | Admin/Gerente | Roteia pedido para fornecedor |
| `/api/integracoes/dropshipping/pendentes` | GET | Admin/Gerente | Lista pedidos pendentes |
| `/api/integracoes/dropshipping/{id}/status` | PUT | Admin/Gerente | Atualiza status/envio |
| `/api/integracoes/dropshipping/fornecedores` | GET | Admin/Gerente | Lista fornecedores |
| `/api/integracoes/dropshipping/comissao/{id}` | GET | Admin/Gerente | Relatório de comissão |

#### Logística
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/logistica/etiqueta` | POST | Admin/Gerente | Gera etiqueta de envio |
| `/api/integracoes/logistica/rastrear/{codigo}` | GET | Público | Rastreia envio |
| `/api/integracoes/logistica/status-envio` | PUT | Admin/Gerente | Atualiza status de entrega |
| `/api/integracoes/logistica/dashboard` | GET | Admin/Gerente | Dashboard operacional |
| `/api/integracoes/logistica/transportadoras` | GET | Admin/Gerente | Lista transportadoras |

#### ERP GenesisGest.Net
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/erp/sync` | POST | Admin | Sincroniza produtos/clientes/pedidos/estoque |
| `/api/integracoes/erp/status` | GET | Admin | Testa conexão com ERP |
| `/api/integracoes/erp/configuracao` | GET/PUT | Admin | Gerencia configuração |

#### Sync Automático
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/sync/executar-agendado` | POST | Admin | Executa sync manual agendado |
| `/api/integracoes/sync/logs` | GET | Admin | Logs de sincronização |

### Configurações `appsettings.json`

```json
{
  "Integracoes": {
    "MercadoLivre": {
      "AccessToken": "APP_USR-...",
      "SellerId": "123456789"
    },
    "Shopee": {
      "BaseUrl": "https://partner.shopeemobile.com",
      "PartnerId": "123456",
      "ShopId": "789012"
    },
    "MelhorEnvio": {
      "Ativo": true,
      "Token": "...",
      "CepOrigem": "01001000"
    },
    "GenesisGest": {
      "UrlBase": "http://192.168.1.72:8080",
      "TokenApi": "...",
      "AutoSync": true,
      "IntervaloMinutos": 60,
      "Entidades": ["PRODUTOS", "CLIENTES", "PEDIDOS", "ESTOQUE"]
    }
  }
}
```

### Próximos Passos
- Configurar tokens reais de cada marketplace
- Implementar tokenização PCI-compliant para cartões (MercadoPago.js)
- Ativar webhook de confirmação de envio do Melhor Envio
- Configurar job recorrente (Hangfire/Quartz) para sync automático ERP


## Grupo Nexum Altivon ME | www.nexumaltivon.com

---

## 📋 Sumário

Esta fase entrega o **ERP/CRM completo em C#** (GenesisGest.Net), integrado ao banco MySQL existente da Fase 1. O sistema gerencia financeiro, fiscal, estoque avançado, CRM e fornecedores, com bridge de sincronização bidirecional com o e-commerce.

---

## 🗂️ Estrutura de Arquivos

```
NexumAltivon_ERP/
├── Models/
│   ├── FinanceiroModels.cs          → Contas Pagar/Receber, Fluxo Caixa, Centros Custo, Contas Bancárias
│   ├── FiscalEstoqueModels.cs       → NFe, Itens NF, Movimentações, Inventário, Kardex, Locais
│   └── CrmFornecedorModels.cs       → Leads, Interações, Tarefas, Fornecedores, Avaliações
├── DTOs/
│   ├── FinanceiroDtos.cs            → DRE, Resumo, Fluxo, Baixa de títulos
│   ├── CrmDtos.cs                   → Leads, Pipeline, Tarefas, Interações
│   └── ErpDtos.cs                   → DTOs consolidados de todas as entidades
├── Services/
│   ├── FinanceiroService.cs         → Contas Pagar/Receber, DRE, Fluxo de Caixa
│   ├── CrmService.cs                → Leads, Pipeline, Conversão, Tarefas
│   ├── EstoqueService.cs            → Movimentações, Inventário, Kardex, Transferências
│   ├── FiscalService.cs             → Emissão NFe, Cancelamento, Impostos
│   ├── RelatorioService.cs          → DRE, Fluxo Caixa, Posição Estoque, Ranking
│   ├── SyncErpService.cs            → Bridge ERP ↔ E-Commerce
│   └── FornecedorService.cs         → Gestão de fornecedores e avaliações
├── Controllers/
│   ├── FinanceiroController.cs      → /api/erp/financeiro/*
│   ├── CrmController.cs             → /api/erp/crm/*
│   ├── EstoqueController.cs         → /api/erp/estoque/*
│   ├── FiscalController.cs          → /api/erp/fiscal/*
│   ├── RelatoriosController.cs      → /api/erp/relatorios/*
│   ├── ErpDashboardController.cs    → /api/erp/dashboard/*
│   ├── FornecedoresController.cs    → /api/erp/fornecedores/*
│   └── SyncController.cs            → /api/erp/sync/*
├── Data/
│   ├── NexumDbContext_ERP.cs        → Extensão do DbContext com índices otimizados
│   ├── ErpDbContext.cs              → DbContext isolado do ERP
│   └── GenesisDbContext.cs          → DbContext legado GenesisGest.Net
├── Database/
│   └── erp_schema_update.sql        → Script SQL de atualização (15 tabelas + views + procedures)
├── Configurations/
│   ├── ServiceExtensions.cs         → Registro de DI unificado
│   └── ErpMappingProfile.cs         → AutoMapper profiles
└── README_Fase5.md                  → Este arquivo
```

---

## 🗄️ Banco de Dados — Novas Tabelas (15)

| Tabela | Domínio | Descrição |
|---|---|---|
| `erp_centros_custo` | Financeiro | Estrutura analítica de custos |
| `erp_contas_bancarias` | Financeiro | Carteiras e saldos bancários |
| `erp_contas_pagar` | Financeiro | Títulos de obrigações |
| `erp_contas_receber` | Financeiro | Títulos de receitas |
| `erp_fluxo_caixa` | Financeiro | Movimentação diária de caixa |
| `erp_notas_fiscais` | Fiscal | Cabeçalho NFe |
| `erp_itens_nota_fiscal` | Fiscal | Itens com impostos |
| `erp_impostos_config` | Fiscal | Alíquotas por NCM/CFOP |
| `erp_movimentacoes_estoque` | Estoque | Entradas, saídas, transferências |
| `erp_inventarios` | Estoque | Contagem física |
| `erp_itens_inventario` | Estoque | Produtos contados |
| `erp_kardex` | Estoque | Rastreamento contábil |
| `erp_locais_estoque` | Estoque | Endereçamento físico |
| `erp_leads_crm` | CRM | Captação de oportunidades |
| `erp_interacoes_crm` | CRM | Histórico de contatos |
| `erp_tarefas_crm` | CRM | Follow-up e compromissos |
| `erp_fornecedores` | Fornecedores | Cadastro completo |
| `erp_avaliacoes_fornecedor` | Fornecedores | Notas e comentários |

---

## ⚙️ Como Aplicar

### Passo 1: Executar Script SQL

```bash
mysql -h 192.168.1.72 -P 3309 -u root -p nexum_altivon < Database/erp_schema_update.sql
```

### Passo 2: Integrar ao Projeto Visual Studio

1. Copie todos os arquivos `.cs` para o projeto `NexumAltivon.API` (Fase 1)
2. Adicione as `DbSet` do `NexumDbContext_ERP.cs` ao seu `NexumDbContext` principal
3. Chame `OnModelCreatingERP(modelBuilder)` no `OnModelCreating` do DbContext
4. Execute `dotnet ef migrations add Fase5_ERP`
5. Execute `dotnet ef database update`

### Passo 3: Registrar Serviços (Program.cs)

```csharp
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<ICrmService, CrmService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IFiscalService, FiscalService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<ISyncErpService, SyncErpService>();
```

---

## 🔌 Principais Endpoints da API ERP

### Financeiro
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/financeiro/contas-pagar` | POST | Criar conta a pagar |
| `/api/erp/financeiro/contas-pagar/{id}/baixar` | POST | Baixar título |
| `/api/erp/financeiro/contas-receber` | POST | Criar conta a receber |
| `/api/erp/financeiro/contas-receber/{id}/baixar` | POST | Receber título |
| `/api/erp/financeiro/resumo` | GET | Posição financeira em tempo real |
| `/api/erp/financeiro/dre` | GET | DRE por período |
| `/api/erp/financeiro/fluxo-caixa` | GET | Movimentação detalhada |

### CRM
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/crm/leads` | POST | Criar lead |
| `/api/erp/crm/leads/{id}/status` | PUT | Atualizar status |
| `/api/erp/crm/leads/{id}/converter` | POST | Converter em cliente |
| `/api/erp/crm/pipeline` | GET | Pipeline visual |
| `/api/erp/crm/interacoes` | POST | Registrar interação |
| `/api/erp/crm/tarefas` | POST | Criar tarefa |

### Estoque
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/estoque/entrada` | POST | Registrar entrada |
| `/api/erp/estoque/saida` | POST | Registrar saída |
| `/api/erp/estoque/transferencia` | POST | Transferir entre lojas |
| `/api/erp/estoque/inventario` | POST | Criar inventário |
| `/api/erp/estoque/inventario/{id}/finalizar` | POST | Finalizar e ajustar |
| `/api/erp/estoque/kardex/{produtoId}` | GET | Histórico do produto |

### Fiscal
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/fiscal/nfe/emitir` | POST | Emitir NFe |
| `/api/erp/fiscal/nfe/{id}/cancelar` | POST | Cancelar NFe |
| `/api/erp/fiscal/impostos` | GET | Configurações de impostos |

### Relatórios
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/relatorios/dre/excel` | GET | Exportar DRE em Excel |
| `/api/erp/relatorios/fluxo-caixa/excel` | GET | Exportar Fluxo de Caixa |
| `/api/erp/relatorios/posicao-estoque` | GET | Posição de estoque |
| `/api/erp/relatorios/pipeline-crm` | GET | Pipeline CRM |

### Sincronização
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/sync/completo` | POST | Executar sync manual |
| `/api/erp/sync/agendado` | POST | Executar sync agendado |
| `/api/erp/sync/produtos` | POST | Sync apenas produtos |
| `/api/erp/sync/pedidos` | POST | Sync apenas pedidos |

---

## 🔄 Bridge ERP ↔ E-Commerce

O `SyncErpService` mantém sincronização automática:

| Direção | Entidade | Frequência |
|---|---|---|
| ERP → E-Commerce | Produtos (preço, estoque) | A cada 15 min |
| E-Commerce → ERP | Clientes (novos cadastros) | A cada 15 min |
| E-Commerce → ERP | Pedidos pagos | A cada 5 min |
| ERP → E-Commerce | Estoque (recálculo) | A cada 30 min |

**Para agendamento automático**, configure Hangfire ou Quartz:

```csharp
// Program.cs
builder.Services.AddHangfire(config => config.UseSqlServerStorage(...));
builder.Services.AddHangfireServer();

// Agendamento
RecurringJob.AddOrUpdate<ISyncErpService>(
    "sync-completo",
    service => service.ExecutarSyncAgendadoAsync(),
    Cron.Minutely(15));
```

---

## 📊 Dashboard ERP

O `ErpDashboardController` consolida KPIs em tempo real:

- **Financeiro**: Saldo, contas atrasadas, projeção 7 dias
- **CRM**: Leads novos, pipeline, tarefas atrasadas
- **Estoque**: Itens críticos, valor em estoque, inventários pendentes
- **Fiscal**: NFe emitidas, pendentes, canceladas

---

## 🚀 Próximo Passo: FASE 6 — Estrutura GitHub + CI/CD

Quando você estiver pronto, a Fase 6 entregará:

| Entregável | Descrição |
|---|---|
| Organização de Repositórios | NexumAltivon.API, .ERP, .CRM, .Database, .Front |
| CI/CD GitHub Actions | Build, testes e deploy automático |
| Docker Compose | Orquestração de containers |
| Documentação de Deploy | Guia passo a passo de produção |
| Scripts de Backup | MySQL + arquivos automáticos |

---

## 📞 Suporte

**Rodrigo Costa** — (14) 99673-1879  
**Vinicius** — (14) 99634-8409  
**E-mail**: corporativo.gna@gmail.com  
**Site**: www.nexumaltivon.com

---

© 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.


# PROJETO GRUPO NEXUM ALTIVON — DOCUMENTACAO COMPLETA
## Grupo Nexum Altivon ME | www.nexumaltivon.com
### Versao: 1.0.00.2600 | Stack: ASP.NET Core 8 + MySQL + Docker + CI/CD

---

## RESUMO EXECUTIVO

Este documento consolida todas as 6 fases do projeto de e-commerce e ERP/CRM do Grupo Nexum Altivon ME, unificando as 6 lojas societarias (Grann-Tur, Chronos, Moda Mim, Geracao Top+, Estruturaline, Gran-fest-festas) em uma plataforma moderna, escalavel e profissional.

---

## FASE 1 — FUNDACAO (Banco + API + Autenticacao)

### O que foi entregue
- Script SQL completo do banco nexum_altivon com 22 tabelas, views, procedures, triggers e seed das 6 lojas
- API ASP.NET Core 8 com estrutura profissional: Program.cs, appsettings.json, DbContext
- JWT completo com refresh token (24h expiracao, 7 dias refresh), BCrypt para senhas
- 8 Controllers RESTful: Auth, Lojas, Clientes, Produtos, Carrinho, Pedidos, CRM, Financeiro
- 15 Services de logica de negocio com tratamento global de erros, rate limiting (100 req/min) e auditoria automatica
- Swagger/OpenAPI documentado
- AutoMapper para mapeamento DTO <-> Entidade

### Como aplicar
1. Executar o script SQL no MySQL (servidor 192.168.1.72:3309)
2. Configurar ConnectionStrings:NexumDb no appsettings.json
3. Rodar dotnet ef database update (ou deixar o EnsureCreated() na primeira execucao)
4. Testar via Swagger: https://localhost:5001/swagger

---

## FASE 2 — PAINEL ADMINISTRATIVO

### O que foi entregue
- DashboardController com 8 endpoints de KPIs e graficos
- AdminDashboardService com metricas de faturamento, pedidos, clientes, estoque baixo, leads
- Painel Admin HTML completo (57 KB) com design identico ao site principal (preto #0A0A0A + dourado #C9A227)
- 6 KPI Cards, graficos Chart.js (semanal, por loja, mensal, top produtos)
- Menu lateral com 16 secoes: Dashboard, Pedidos, Produtos, Clientes, Lojas, Financeiro, Fiscal, Logistica, CRM, Cupons, Marketing, Marketplaces, Dropshipping, Usuarios, Configuracoes, Auditoria

### Como aplicar
1. Usar o painel oficial em `NexumAltivon_Front-End/src/pages/Dashboard.js`
2. Publicar pelo workflow `Nexum 2026-06-28 - Deploy Operacional Oficial .com.br`
3. O painel consome a API oficial `https://api.nexumaltivon.com.br`
4. Acesso restrito a perfis: Gerente, Admin, SuperAdmin

---

## FASE 3 — CARRINHO, CHECKOUT E GATEWAY DE PAGAMENTO

### O que foi entregue
- Carrinho anonimo via cookie nx_session_id (HttpOnly, Secure, 30 dias)
- Migracao automatica do carrinho de sessao para cliente logado
- Cupons de desconto (percentual ou valor fixo) com validade e limite de uso
- Checkout completo: endereco -> calculo de frete (Melhor Envio + tabela propria) -> pagamento
- Gateway Mercado Pago: PIX (QR Code base64 + texto), Cartao de Credito (parcelado), Boleto
- Webhooks para atualizacao automatica de status de pagamento (/api/webhooks/mercadopago)
- Notificacoes: E-mail transacional (SendGrid) + WhatsApp + alerta de estoque baixo
- Reembolso total ou parcial via API

### Como aplicar
1. Configurar tokens do Mercado Pago em appsettings.json
2. Registrar URL de webhook no dashboard do Mercado Pago: https://api.nexumaltivon.com/api/webhooks/mercadopago
3. Configurar SendGrid para e-mails transacionais
4. Fluxo: Adicionar ao Carrinho -> Checkout (JWT) -> Selecionar Frete -> Finalizar (PIX/Cartao/Boleto) -> Webhook confirma -> Pedido PAGO

---

## FASE 4 — INTEGRACOES COMPLETAS

### O que foi entregue
- Mercado Livre: Publicar produtos, atualizar preco/estoque, importar pedidos, marcar como enviado
- Hub Multi-Canal: Shopee e Amazon (estrutura pronta, stubs documentados)
- Dropshipping: Roteamento inteligente de pedidos para fornecedores, calculo automatico de comissao, notificacao ao fornecedor
- Logistica: Geracao de etiquetas, rastreamento com eventos simulados, dashboard operacional, transportadoras
- ERP GenesisGest.Net: Bridge completa para sincronizacao de produtos, clientes, pedidos e estoque via API REST
- Sync Automatico: Orquestrador que mantem estoque e preco sincronizados entre todos os canais automaticamente

### Como aplicar
1. Configurar tokens de cada marketplace em appsettings.json
2. Para ML: registrar app em developers.mercadolivre.com.br, obter APP_ID e SECRET
3. ERP: configurar URL base do GenesisGest.Net (192.168.1.72:8080) e token
4. Agendar job recorrente (Hangfire/Quartz) para ExecutarSyncAgendadoAsync()

---

## FASE 5 — ERP/CRM GENESISGEST.NET

### O que foi entregue
- Financeiro: Contas Pagar/Receber, Fluxo de Caixa, DRE automatica, centros de custo, contas bancarias
- CRM: Leads, pipeline visual, conversao, interacoes, tarefas com prioridade
- Estoque Avancado: Movimentacoes (entrada/saida/transferencia/ajuste), Kardex automatico, inventario fisico, locais de estoque
- Fiscal: NFe com calculo automatico de ICMS/IPI/PIS/COFINS, cancelamento, download XML/DANFE
- Relatorios: DRE, Fluxo de Caixa, Posicao de Estoque, Ranking — exportacao PDF/Excel
- Fornecedores: Cadastro completo, avaliacoes, comissoes
- Bridge ERP <-> E-Commerce: Sincronizacao bidirecional automatica
- Dashboard ERP: KPIs consolidados em tempo real (financeiro, CRM, estoque, fiscal, alertas)
- Hangfire: Jobs automaticos de sync, alertas de contas vencidas

### Novas tabelas (18+)
- erp_centros_custo, erp_contas_bancarias, erp_contas_pagar, erp_contas_receber
- erp_fluxo_caixa, erp_notas_fiscais, erp_itens_nota_fiscal, erp_impostos_config
- erp_movimentacoes_estoque, erp_inventarios, erp_itens_inventario, erp_kardex
- erp_locais_estoque, erp_leads_crm, erp_interacoes_crm, erp_tarefas_crm
- erp_fornecedores, erp_avaliacoes_fornecedor

### Como aplicar
1. Executar Database/erp_schema_update.sql no MySQL
2. Integrar arquivos .cs ao projeto NexumAltivon.API
3. Adicionar DbSet do NexumDbContext_ERP.cs ao DbContext principal
4. Executar dotnet ef migrations add Fase5_ERP e dotnet ef database update
5. Registrar servicos em Program.cs (ja incluido no Program_ERP.cs entregue)

---

## FASE 6 — GITHUB + CI/CD + DOCKER + DEPLOY

### O que foi entregue
- Repositorios GitHub organizados: .API, .Front, .Database, .Docs, .Infra
- GitHub Actions: Build, testes xUnit, Docker build/push, deploy automatico staging/producao via SSH
- Docker: Multi-stage build, usuario nao-root, health checks
- Docker Compose: Desenvolvimento (MySQL + API + Redis + Nginx) e Producao (MySQL + API + Watchtower)
- Nginx: Reverse proxy com headers de seguranca, pronto para SSL
- Backup/Restore: Scripts automaticos com retencao de 30 dias
- Documentacao de Deploy: Guia passo a passo completo

### Como aplicar
1. Criar organizacao nexumaltivon no GitHub
2. Criar repositorios e configurar secrets (SSH keys, tokens)
3. Configurar servidor com Docker e Docker Compose
4. Executar docker-compose -f docker-compose.prod.yml up -d
5. Configurar SSL com Let's Encrypt
6. Agendar backups no crontab

---

## ESTRUTURA FINAL DOS CAMINHOS

```
D:\Users\Rodrigo Costa\source\repos\GenesisGest.Net_E-Commerce\NexumAltivon.com
|
├── NexumAltivon_Back-End/
│   ├── NexumAltivon.API.csproj
│   ├── Program.cs (ERP unificado)
│   ├── appsettings.json / appsettings.ERP.json
│   ├── API/
│   │   ├── Controllers/          -> 13+ controllers
│   │   ├── Services/             -> 25+ services
│   │   ├── Models/               -> Entidades EF Core completas
│   │   ├── DTOs/                 -> DTOs organizados por dominio
│   │   ├── Data/
│   │   │   └── NexumDbContext.cs -> DbContext unificado com seed
│   │   ├── Middleware/
│   │   ├── Mappings/
│   │   └── Configurations/
│   └── Database/
│       ├── nexum_altivon_schema.sql
│       └── erp_schema_update.sql
|
├── NexumAltivon_Front-End/
│   ├── index.html                -> Home Page institucional (preservada)
│   └── admin/
│       └── index.html            -> Painel administrativo completo
|
├── NexumAltivon_ERP/
│   └── (estrutura preparada para expansao desktop/WinForms futura)
|
├── docker/
│   ├── Dockerfile.api
│   ├── docker-compose.yml
│   ├── docker-compose.prod.yml
│   ├── nginx/
│   │   └── nginx.conf
│   └── scripts/
│       ├── backup-mysql.sh
│       └── restore-mysql.sh
|
├── .github/
│   └── workflows/
│       └── ci-cd.yml
|
└── docs/
    ├── README_Fase1.md
    ├── README_Fase2.md
    ├── README_Fase3.md
    ├── README_Fase4.md
    ├── README_Fase5.md
    ├── README_Fase6.md
    └── README_FINAL.md (este arquivo)
```

---

## PROXIMOS PASSOS SUGERIDOS

### Imediatos (Validacao)
1. Testar Fase 1: Subir banco MySQL e API, validar JWT e Swagger
2. Testar Fase 2: Acessar painel admin, verificar KPIs e graficos
3. Testar Fase 3: Simular carrinho -> checkout -> pagamento PIX (sandbox MP)
4. Testar Fase 4: Configurar token ML, publicar 1 produto de teste
5. Testar Fase 5: Criar conta a pagar, lead, movimentacao de estoque
6. Testar Fase 6: Subir ambiente Docker, validar CI/CD

### Futuros (Melhorias)
- [ ] Front-end Next.js consumindo a API
- [ ] Aplicativo mobile (React Native/Flutter)
- [ ] Testes unitarios xUnit (cobertura > 80%)
- [ ] Testes de integracao com TestContainers
- [ ] Monitoramento com Prometheus + Grafana
- [ ] Cache distribuido com Redis
- [ ] Mensageria com RabbitMQ/Apache Kafka
- [ ] Inteligencia artificial para recomendacoes de produtos
- [ ] Chatbot com IA para atendimento
- [ ] BI avancado com Power BI / Metabase

---

## SUPORTE

**Rodrigo Costa** — (14) 99673-1879
**Vinicius** — (14) 99634-8409
**E-mail**: corporativo.gna@gmail.com
**Site**: www.nexumaltivon.com

---

(c) 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.


# Checklist de Prontidao de Deploy

Data de corte: 16/06/2026
Versao alvo: 1.1.5
Deadline operacional informado pelo projeto: 18/06/2026

## 1. Diagnostico de riscos

### Risco 1: runtime fragmentado entre 5010, 5011 e 5012

Impacto comercial:
- frontend e automacoes podem falar com uma API diferente da API realmente ativa
- checkout, area do cliente e ERP passam a ler estados diferentes
- qualquer teste de venda deixa de ser confiavel

Evidencias confirmadas no codigo:
- [VERSAO.md](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\VERSAO.md) ainda aponta para `localhost:5010`
- [scripts/01-instalar-api-local-permanente.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\01-instalar-api-local-permanente.ps1) usa `5011`
- [scripts/start-nexum-auto.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\start-nexum-auto.ps1) usa `192.168.1.72:5012`
- [NexumAltivon_Front-End/src/services/api.js](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Front-End\src\services\api.js) usa `192.168.1.72:5012`

Correcao imediata:
- congelar o padrao definitivo em `http://192.168.1.72:5012`
- desativar inicializadores antigos de `5010` e `5011`
- republicar backend e frontend apontando para a mesma base
- validar `/health`, login admin, cadastro cliente e checkout na mesma porta

### Risco 2: checkout parcialmente funcional, mas ainda nao homologado ponta a ponta

Impacto comercial:
- o pedido pode nascer, mas falhar em pagamento, fiscal, estoque ou sincronizacao ERP
- qualquer venda real fica sem garantias de baixa financeira e acompanhamento logistico

Evidencias confirmadas no codigo:
- [NexumAltivon_Back-End/API/Program.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Program.cs) possui rota de checkout em `/api/pedidos`
- [NexumAltivon_Back-End/API/Data/NexumDbContext.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Data\NexumDbContext.cs) recebeu conversoes de enums para `Cliente`, `Produto`, `Pedido`, `PedidoItem` e `Pagamento`
- [NexumAltivon_Back-End/API/Program.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Program.cs) atualiza status do pedido e movimenta estoque em `/api/pedidos/{id}/status`

Correcao imediata:
- subir uma unica API em `5012`
- revalidar oficialmente:
  - criacao de pedido
  - confirmacao de pagamento
  - troca de status para `Pago`, `EmSeparacao`, `Enviado`, `Entregue`
  - baixa de estoque
  - emissao fiscal
  - sincronizacao ERP

### Risco 3: fiscal e notificacoes ainda dependem de configuracao real de producao

Impacto comercial:
- o motor de roteamento fiscal pode rodar sem emitente valido
- os e-mails de confirmacao podem apenas simular envio
- sem credenciais reais, pagamento e logistica nao concluem a venda

Evidencias confirmadas no codigo:
- [NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\ERP\FiscalRouting\FiscalRoutingEngine.cs) calcula ranking por custo tributario, custo operacional e margem
- [NexumAltivon_Back-End/API/Program.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Program.cs) cria e amplia `erp_empresas_grupo` e `fiscal`
- [NexumAltivon_Back-End/API/Services/NotificacaoService.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Services\NotificacaoService.cs) envia copia para `corporativo.gna@gmail.com`, mas sem `Integracoes:SendGrid:ApiKey` cai em e-mail simulado
- [NexumAltivon_Back-End/API/appsettings.json](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\appsettings.json) ainda tem chaves vazias para gateway, logistica, dropshipping, certificado e fiscal

Correcao imediata:
- preencher `erp_empresas_grupo` com emitentes reais e marcacao de `ativa=1`
- configurar `Integracoes:SendGrid:ApiKey`
- configurar gateway principal e secundario
- configurar Melhor Envio e demais operadores logisticos
- configurar certificado NF-e/NFC-e no servidor principal

## 2. Script de correcao SSOT

Executar:
- [scripts/sql/2026-06-16-ssot-unificar-lojas-estoque.sql](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\sql\2026-06-16-ssot-unificar-lojas-estoque.sql)

Esse script faz:
- cria tabela de auditoria de inconsistencias
- normaliza tabela de configuracao SSOT das lojas
- registra prefixos oficiais de origem
- desativa produtos fantasma ou incompletos
- corrige estoque negativo e reserva acima do estoque
- cria views de catalogo publicavel, estoque consolidado e emitentes fiscais ativos

## 3. Protocolo de lancamento

Atualizar ou sobrepor obrigatoriamente no servidor principal:
- [NexumAltivon_Back-End/API/Program.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Program.cs)
- [NexumAltivon_Back-End/API/Data/NexumDbContext.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Data\NexumDbContext.cs)
- [NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\ERP\FiscalRouting\FiscalRoutingEngine.cs)
- [NexumAltivon_Back-End/API/Services/NotificacaoService.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Services\NotificacaoService.cs)
- [NexumAltivon_Back-End/API/appsettings.PrivateProduction.template.json](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\appsettings.PrivateProduction.template.json)
- [NexumAltivon_Front-End/src/services/api.js](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Front-End\src\services\api.js)
- [NexumAltivon_Front-End/public/api-runtime.json](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Front-End\public\api-runtime.json)
- [scripts/start-nexum-auto.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\start-nexum-auto.ps1)
- [scripts/nexum-api-guardian.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\nexum-api-guardian.ps1)
- [scripts/sql/2026-06-16-ssot-unificar-lojas-estoque.sql](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\sql\2026-06-16-ssot-unificar-lojas-estoque.sql)

Arquivos que nao devem continuar ativos sem padronizacao:
- [VERSAO.md](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\VERSAO.md)
- [scripts/01-instalar-api-local-permanente.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\01-instalar-api-local-permanente.ps1)
- [scripts/02-instalar-api-definitiva-tarefa.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\02-instalar-api-definitiva-tarefa.ps1)
- [scripts/80-instalar-api-autostart-usuario.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\80-instalar-api-autostart-usuario.ps1)
- [publish-iis.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\publish-iis.ps1)

## 4. Ordem executiva ate o deploy

1. Padronizar a porta unica em `5012`.
2. Rodar o SQL SSOT no banco central `192.168.1.72:3309`.
3. Publicar backend com as conversoes de enum e o motor fiscal atual.
4. Publicar frontend consumindo a mesma API `5012`.
5. Injetar as credenciais reais de pagamento, logistica, fiscal e e-mail.
6. Rodar teste oficial do fluxo cliente:
   - cadastro
   - confirmacao por link
   - login
   - carrinho
   - checkout
   - pagamento
   - pedido no painel
   - faturamento
   - notificacoes
   - status logistico final

## 5. Code freeze

Nao abrir nova frente antes de estabilizar:
- Checkout
- Fiscal routing
- Emissao NF-e/NFC-e
- Notificacoes
- ERP sync
- Estoque SSOT


# Nexum Altivon — API 24h no servidor

Este documento fixa o caminho operacional para a API não depender do Codex, do navegador ou da máquina de desenvolvimento.

## Prioridade

Até sábado às 08:00, a prioridade é manter a API online para sustentar:

- painel administrativo;
- checkout e pedidos;
- cadastros reais;
- imagens de produtos;
- integrações de dropshipping, logística, gateways, e-commerce/marketplaces e financeiro.

## Decisão técnica

A API ASP.NET Core deve rodar no servidor como tarefa automática do Windows.

O Cloudflare Tunnel pode publicar a API para a internet, mas ele não substitui o servidor da API. O Cloudflare transporta/protege o tráfego; quem executa a API continua sendo o servidor local ou uma VPS.

## Instalação no servidor

No servidor, execute como Administrador:

```cmd
scripts\server\INSTALAR-API-24H-SERVIDOR-COMO-ADMIN.cmd
```

O instalador:

- publica a API em `Y:\NexumAltivon_API_24H\api`;
- cria a configuração privada em `Y:\NexumAltivon_API_24H\config\api.env.ps1`;
- cria a tarefa automática `NexumAltivonApi24h`;
- inicia um guardião que testa `/health` e reinicia a API se ela cair.

## Instalação por pacote pronto

Quando o pacote já estiver publicado no compartilhamento do servidor, execute no próprio servidor como Administrador:

```cmd
INSTALAR-API-24H-PACOTE-COMO-ADMIN.cmd
```

Este caminho é o mais rápido quando o desenvolvimento foi feito em outra máquina: o pacote já contém a API compilada e só instala a operação 24h no servidor.

Pacote corrigido para a unidade certa do servidor:

```cmd
Y:\NexumAltivon_API_24H_Y_FIX\INSTALAR-API-24H-PACOTE-COMO-ADMIN.cmd
```

Para gerar esse pacote no compartilhamento do servidor:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\server\criar-pacote-api-24h-servidor.ps1
```

## Configuração privada

O arquivo real `Y:\NexumAltivon_API_24H\config\api.env.ps1` nunca deve ir para o Git.

Ele precisa conter:

- conexão com MariaDB/MySQL em `192.168.1.72:3309`;
- senha real do usuário `nexum_app`;
- chave JWT forte;
- senha real do usuário administrador.

Use `scripts\server\api.env.example.ps1` apenas como modelo.

## Publicação externa

Com a Wix ainda mantendo os nameservers, o DNS público deve continuar sendo ajustado na Wix até a transferência total do domínio.

Para `api.nexumaltivon.com.br`, o caminho operacional oficial é:

- API rodando em `http://127.0.0.1:5012` no servidor;
- Cloudflared rodando no mesmo servidor;
- rota pública do túnel apontando `api.nexumaltivon.com.br` para `http://127.0.0.1:5012`;
- CNAME/DNS do Cloudflare para o destino `*.cfargotunnel.com` correto do túnel nomeado.

## Verificação

No servidor, rode:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\server\verificar-api-24h.ps1
```

Critérios mínimos de aceite:

- tarefa `NexumAltivonApi24h` existe;
- porta local `5012` responde;
- `http://127.0.0.1:5012/health` retorna saudável;
- `https://api.nexumaltivon.com.br/health` responde publicamente;
- login do painel funciona em `https://nexumaltivon.com.br/login`.

## Plano definitivo

Depois do dia 17/06/2026, quando a transferência completa do domínio for liberada, o caminho recomendado é mover a zona DNS inteira para Cloudflare ou publicar a API em VPS com IP público fixo.


<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Checklist Operacional Por Tabelas - 29/06/2026

Este checklist transforma a base atual dos bancos `nexum_altivon` e `genesis_bd` em frentes operacionais reais. O criterio de conclusao de cada frente e simples: tabela disponivel so conta como funcional quando tiver API, tela, validacao, permissao, auditoria minima e teste real.

## Estado Consolidado Dos Bancos

| Banco | Estado | Realidade atual |
| --- | --- | --- |
| `nexum_altivon` | Base principal operacional | Vendas, clientes, produtos, compras, estoque, financeiro, fiscal inicial, CRM inicial e integracoes em andamento |
| `genesis_bd` | Base GenesisGest.Net compartilhada | Financeiro Genesis ja recebe contas a receber e contas a pagar; demais modulos precisam ganhar telas e rotas |

## Ferramentas Ja Operacionais

| Area | Tabelas base | Ferramenta atual | Status |
| --- | --- | --- | --- |
| Catalogo e vitrine | `produtos`, `categorias`, `lojas`, `configuracoes_sistema` | Home, catalogo, detalhe de produto e painel de produto | Validado |
| Cliente e endereco | `clientes`, `enderecos` | Cadastro, login, area do cliente e enderecos auxiliares | Validado |
| Venda online | `pedidos`, `pedido_itens`, `pagamentos`, `financeiro` | Checkout, pedido, acompanhamento e conta a receber | Validado |
| Compras operacionais | `compras_solicitacoes`, `compras_cotacoes`, `compras_pedidos`, `compras_pedido_itens`, `compras_entradas`, `compras_entrada_itens` | Cotacao, pedido de compra e entrada de mercadoria | Validado pela API e painel |
| Fornecedores | `fornecedores` | Cadastro e selecao em compras | Validado |
| Estoque por compra | `estoque_movimentos`, `produtos` | Entrada atualiza saldo, custo, codigo de barras, QR code e identificacao | Validado |
| Financeiro Genesis | `erp_contas_receber`, `erp_contas_pagar`, `erp_fluxo_caixa` | Contas a receber e a pagar sincronizadas com Nexum | Validado |
| Fiscal inicial | `fiscal`, `erp_impostos_config`, `erp_notas_fiscais` | Preparacao manual, roteamento e contingencia inicial | Parcial |
| CRM inicial | `crm_leads`, `crm_atendimentos` | Lead e status basico | Parcial |
| Integracoes | `dropshipping_config`, `marketplaces`, `transportadoras` | Diagnostico e cadastro de credenciais-modelo | Parcial |

## Tabelas Que Devem Virar Ferramentas

## Regra Geral De Campos Em Formularios

Todos os campos operacionais existentes nas tabelas atuais e todos os novos campos criados a partir deste levantamento devem ser tratados como parte da ferramenta, nao apenas como coluna de banco. A regra de aceite passa a ser:

- Campos de negocio devem aparecer nos formularios existentes ou nos novos formularios do modulo correspondente.
- Campos obrigatorios no banco devem ter validacao clara antes de gravar.
- Campos opcionais relevantes devem estar disponiveis em abas, secoes avancadas ou complementos do formulario.
- Campos de relacionamento devem usar seletores reais, busca ou autocomplete, nunca texto solto quando houver tabela relacionada.
- Campos calculados devem aparecer como leitura ou totalizador, com origem clara para conferencia.
- Campos de status devem alimentar botoes de acao, filtros e fluxo de aprovacao quando aplicavel.
- Campos internos de auditoria, controle tecnico, `Id`, `TenantId`, `RowVersion`, datas automaticas e soft delete nao precisam aparecer como campos editaveis, mas devem ser exibidos em trilha de auditoria quando fizer sentido.
- Nenhum campo novo pode ficar sem destino: formulario, listagem, detalhe, auditoria, relatorio ou integracao.

### 00. Nucleo, acesso e auditoria

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Usuarios e perfis | `usuarios`, `adm_usuarios`, `adm_perfis`, `adm_permissoes`, `adm_perfil_permissoes` | Gestao de usuarios, niveis, aprovacao de acesso e bloqueio | A fazer |
| Auditoria | `logs_auditoria`, `adm_auditoria`, `erp_logs` | Tela de trilha de auditoria por usuario, tabela, acao e periodo | A fazer |
| Parametros | `cfg_parametros`, `configuracoes_sistema`, `cfg_sequenciais` | Central de parametros, sequenciais, numeracao e chaves operacionais | A fazer |
| Workflows | `cfg_workflow_definicoes`, `cfg_workflow_etapas`, `cfg_workflow_instancias`, `cfg_workflow_aprovacoes` | Aprovacoes por etapa para compras, financeiro, fiscal e cadastros | A fazer |
| Anexos e comentarios | `cfg_anexos`, `cfg_comentarios`, `cfg_notificacoes`, `cfg_tarefas` | Anexos, comentarios internos, tarefas e notificacoes por processo | A fazer |

### 01. BI e cockpit executivo

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Indicadores | `bi_kpis`, `bi_kpi_valores`, `bi_widgets`, `bi_dashboards` | Cockpit de indicadores por venda, compra, estoque, financeiro e fiscal | A fazer |
| Relatorios | `bi_relatorios`, `bi_relatorio_historico` | Relatorios salvos, exportacao e historico de execucao | A fazer |
| Visoes gerenciais | `vw_dre_gerencial`, `vw_fluxo_caixa_projetado`, `vw_ranking_produtos`, `vw_saldo_estoque`, `vw_inadimplencia` | Painel executivo com leitura direta das visoes | A fazer |

### 02. Master data corporativo

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Pessoas e empresas | `adm_pessoas_empresas`, `adm_empresas`, `adm_contatos`, `erp_empresas_grupo` | Cadastro unico de pessoas, empresas, contatos e empresas do grupo | Parcial |
| Itens e categorias | `produtos`, `categorias`, `vnd_itens`, `vnd_itens_precos` | Unificacao de item comercial, item fiscal, preco e estoque | Parcial |
| Fornecedores | `fornecedores`, `erp_fornecedores`, `erp_avaliacoes_fornecedor` | Cadastro completo, avaliacao, prazo, risco, origem e performance | Parcial |

### 03. Compras, suprimentos e aquisicoes

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Fluxo atual validado | `compras_*` | Cotacao, pedido, entrada, estoque, financeiro e Genesis | Validado |
| Fluxo Genesis completo | `cmp_requisicoes`, `cmp_requisicao_itens`, `cmp_cotacoes`, `cmp_cotacao_itens`, `cmp_cotacao_fornecedores`, `cmp_pedidos`, `cmp_pedido_itens`, `cmp_notas_fiscais`, `cmp_aprovacoes` | Requisicao interna, cotacao multi-fornecedor, aprovacao, pedido e nota de entrada | A fazer |
| Dropshipping e parcerias | `dropshipping_config`, `marketplaces`, `fornecedores` | Regras de parceiro, custo, prazo, margem, status e origem por item | Parcial |

### 04. Estoque, WMS e movimentacoes

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Movimentos reais | `estoque_movimentos`, `produtos` | Historico de entrada por compra e saldo do produto | Validado |
| Depositos e enderecos | `est_depositos`, `est_enderecos`, `erp_locais_estoque` | Depositos, ruas, prateleiras, bins e saldo por local | A fazer |
| Inventario | `est_inventarios`, `est_inventario_contagens`, `erp_inventarios`, `erp_itens_inventario` | Contagem, divergencia, ajuste e auditoria de estoque | A fazer |
| Kardex e saldos | `est_movimentacoes`, `est_saldos`, `erp_kardex`, `erp_movimentacoes_estoque` | Kardex por produto, custo medio e saldo historico | A fazer |
| Separacao | `est_ordens_separacao`, `est_ordem_separacao_itens`, `est_ponto_pedido`, `est_curva_abc` | Picking, ponto de pedido, curva ABC e reposicao automatica | A fazer |

### 05. Financeiro, tesouraria e contabilidade

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Financeiro operacional | `financeiro`, `erp_contas_receber`, `erp_contas_pagar`, `erp_fluxo_caixa` | Receber, pagar e fluxo basico | Validado |
| Financeiro completo | `fin_titulos_pagar`, `fin_titulos_receber`, `fin_pagamentos`, `fin_recebimentos`, `fin_contas_bancarias`, `erp_contas_bancarias` | Titulos, baixas, bancos, pagamentos e recebimentos | A fazer |
| Conciliacao | `fin_extratos_bancarios`, `fin_conciliacoes`, `fin_aprovacoes_pagar` | Importacao de extrato, conciliacao e aprovacao de pagamento | A fazer |
| Orcamento e moedas | `fin_orcamentos`, `fin_orcamento_itens`, `fin_moedas`, `fin_taxas_cambio` | Orcamento empresarial, moedas e cambio | A fazer |
| Contabilidade | `cnt_lancamentos`, `cnt_partidas`, `cnt_fechamentos`, `fin_plano_contas`, `fin_centros_custo`, `erp_centros_custo` | Plano de contas, partidas e fechamentos | A fazer |

### 06. Fiscal e faturamento

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Fiscal inicial | `fiscal`, `erp_notas_fiscais`, `erp_itens_nota_fiscal` | Rascunho fiscal, contingencia manual e status do pedido | Parcial |
| Tributacao | `fis_tributacao_ncm`, `erp_impostos_config`, `fis_apuracao_impostos` | NCM, impostos, CFOP, apuracao e regras por loja/emitente | A fazer |
| SPED | `fis_sped_fiscal`, `fis_sped_contabil` | Exportacao fiscal e contabil | A fazer |
| Vendas fiscais | `vnd_notas_fiscais`, `vnd_pedidos`, `vnd_pedido_itens` | Pedido fiscal/comercial e nota vinculada | A fazer |

### 07. Comercial, CRM e atendimento

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Leads | `crm_leads`, `erp_leads_crm` | Captura, status e origem do lead | Parcial |
| Atendimento | `crm_atendimentos`, `erp_interacoes_crm`, `erp_tarefas_crm` | Linha do tempo, tarefas, retorno e responsavel | A fazer |
| Precos e vendas | `vnd_tabelas_preco`, `vnd_itens_precos`, `vnd_pedidos`, `vnd_pedido_itens` | Tabelas de preco, pedido comercial e condicoes | A fazer |
| Performance | `vw_performance_vendedores` | Ranking e acompanhamento comercial | A fazer |

### 08. Logistica

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Entrega atual | `envios`, `pedidos`, `transportadoras` | Status logistico basico no pedido | Parcial |
| Frete | `log_transportadoras`, `log_tabelas_frete`, `log_frete_faixas`, `log_auditoria_fretes` | Transportadoras, tabelas, cotacao e auditoria de frete | A fazer |
| Tracking | `log_tracking` | Linha do tempo do envio e notificacao cliente | A fazer |
| Documentos transporte | `log_cte`, `log_mdfe`, `log_mdfe_documentos` | CTe, MDFe e documentos vinculados | A fazer |

### 09. RH e HCM

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Estrutura RH | `rh_departamentos`, `rh_cargos`, `rh_colaboradores`, `rh_dependentes` | Colaboradores, departamentos, cargos e dependentes | A fazer |
| Jornada | `rh_ponto`, `rh_afastamentos`, `rh_ferias` | Ponto, ferias e afastamentos | A fazer |
| Folha | `rh_folhas_pagamento`, `rh_folha_itens`, `rh_eventos_folha`, `rh_historico_salarial`, `rh_esocial_eventos` | Folha, eventos, historico salarial e eSocial | A fazer |

### 10. Producao, manutencao e operacoes

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Engenharia de produto | `pcp_bom`, `pcp_bom_itens`, `pcp_roteiros`, `pcp_roteiro_operacoes` | Ficha tecnica, roteiro e operacoes | A fazer |
| Producao | `pcp_ordens_producao`, `pcp_apontamentos`, `pcp_requisicoes_material`, `pcp_centros_trabalho` | Ordem, apontamento, material e centro de trabalho | A fazer |
| Qualidade | `pcp_inspecoes`, `pcp_nao_conformidades` | Inspecao e nao conformidade | A fazer |

### 11. Juridico

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Contratos | `jur_contratos`, `jur_contrato_clausulas`, `jur_aditivos`, `vw_contratos_vencer` | Contratos, clausulas, aditivos e alertas | A fazer |
| Processos | `jur_processos`, `jur_andamentos`, `jur_prazos`, `jur_tipos_acao`, `vw_processos_risco` | Processos, prazos, andamentos e risco | A fazer |
| Certidoes e apoio | `jur_certidoes`, `jur_depositos_judiciais`, `jur_base_conhecimento`, `vw_certidoes_vencer` | Certidoes, depositos e base juridica | A fazer |

## Ordem De Transformacao Em Ferramentas

1. `cfg_*`, `adm_*` e auditoria: base de permissoes, parametros, workflows e trilha de auditoria.
2. `fin_*`, `cnt_*` e centros de custo: completar financeiro, tesouraria e contabilidade.
3. `est_*` e `erp_*` de estoque: WMS, depositos, inventario, kardex e saldos.
4. `cmp_*`: evoluir compras validado para requisicoes, cotacoes multi-fornecedor e aprovacao.
5. `log_*`: frete, tracking, transportadoras, CTe e MDFe.
6. `fis_*` e `vnd_*`: fiscal real, SPED, pedidos comerciais e faturamento.
7. `crm_*` e `vnd_*`: CRM completo, tarefas, funil e tabelas de preco.
8. `bi_*` e `vw_*`: cockpit executivo e relatorios gerenciais.
9. `rh_*`, `pcp_*` e `jur_*`: RH, producao e juridico.

## Regra De Conclusao Por Grupo

Cada grupo so pode ser marcado como pronto quando cumprir todos os pontos:

- API com leitura, criacao e atualizacao quando aplicavel.
- Tela administrativa com formulario funcional e listagem real.
- Todos os campos operacionais da tabela mapeados para formulario, detalhe, listagem, auditoria, relatorio ou integracao.
- Novos campos criados no banco refletidos imediatamente nos formularios existentes ou nos novos formularios planejados.
- Validacao de obrigatoriedade e consistencia.
- Registro em auditoria ou log operacional.
- Integracao com financeiro, estoque, fiscal ou CRM quando a tabela exigir.
- Teste real pela API publica.
- Teste visual no painel publicado quando houver tela.
- Atualizacao deste checklist com evidencia do teste.


<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Cronograma Operacional Validado - 29/06/2026

Este documento confronta o panorama anterior de 26/06/2026 com o estado validado em 29/06/2026. Ele substitui o bloqueio antigo da ponte publica por um checklist real baseado nos testes executados no ambiente online.

## Estado Validado Hoje

| Frente | Estado | Validacao |
| --- | --- | --- |
| Site publico | Operante | `https://nexumaltivon.com.br` retornou HTTP 200 |
| API publica | Operante | `https://api.nexumaltivon.com.br/health` retornou `Healthy` |
| Banco via API | Operante | `https://api.nexumaltivon.com.br/health/db` retornou `Healthy` |
| Produtos na API | Operante | `/api/produtos?limite=5` retornou produtos reais |
| Lojas na API | Operante | `/api/lojas` retornou lojas reais |
| Login administrativo | Operante | Login admin autenticou com sucesso |
| GenesisGest original | Sincronizado | 129 estruturas reconhecidas e 3 pontes Nexum/Genesis ativas |
| Modulo de compras | Operante na API | `/api/compras/painel` respondeu autenticado |
| Checkout controlado | Operante | Pedido real de teste criado em producao controlada |
| Contas a receber | Operante | Pedido criou lancamento financeiro local automaticamente |
| Genesis contas a receber | Operante | Pedido novo foi sincronizado no Genesis com valor correto |
| Fluxo visual de compra | Operante ate checkout | Home, produto, carrinho e tela de checkout validados no site publicado |
| Area do cliente | Operante | Cliente de teste logou e visualizou pedido, total e endereco |
| Compras e aquisicoes | Operante pela API publica | Cotacao, pedido de compra, entrada, estoque, financeiro local e Genesis validados |
| Compras no painel admin | Operante visualmente | Formularios de cotacao, pedido de compra e entrada executaram sem erro no navegador |
| Estoque por entrada | Operante | Entrada de compra atualizou saldo, codigo de barras, QR code e identificacao de estoque |
| Contas a pagar | Operante | Pedido de compra criou despesa local e conta a pagar no Genesis |
| Backend .NET | Compilado | `dotnet build` em Release com 0 erros e 0 avisos |
| Versionamento recente | Atualizado | `main` contem schema Genesis, compras e estoque versionados |

## Cronograma Confrontado

### Panorama de 26/06/2026

O cronograma anterior indicava o sistema com boa condicao para MVP, mas com bloqueio critico na rota publica da API. Naquele momento o percentual estimado era:

- MVP de vendas online controladas: 82%.
- Operacao completa com integracoes, logistica, faturamento fiscal real, gateways e PDV: 60% a 65%.
- Site + API + checkout + area do cliente + base local: 75%.

### Atualizacao de 29/06/2026

O bloqueio principal de publicacao foi superado para a API principal. A operacao publica ja responde, o banco esta acessivel pela API, produtos reais retornam, login administrativo autentica e o modulo de compras responde.

Percentual atualizado:

- MVP de vendas online controladas: 92%.
- Operacao completa com integracoes externas, fiscal real, logistica automatizada e PDV: 73%.
- Base site + API + banco + checkout + area cliente + admin + compras + Genesis: 89%.

## Checklist Validado

- [x] Dominio `nexumaltivon.com.br` online.
- [x] API publica `api.nexumaltivon.com.br` online.
- [x] Banco respondendo por `/health/db`.
- [x] Produtos reais retornando pela API.
- [x] Lojas reais retornando pela API.
- [x] Admin autenticando.
- [x] Painel de compras acessivel por API autenticada.
- [x] Schema original GenesisGest integrado sem quebra de build.
- [x] Pontes Genesis/Nexum criadas para produtos, compras e financeiro.
- [x] Build Release do backend sem erros.
- [x] Compras, entradas e movimentos de estoque ja versionados em commits anteriores.
- [x] Pedido de venda criado em producao controlada.
- [x] Conta a receber local criada automaticamente no checkout.
- [x] Conta a receber Genesis criada automaticamente no checkout.
- [x] Dashboard administrativo refletindo pedidos e faturamento do dia.
- [x] Home publicada exibindo produtos reais da API.
- [x] Carrinho visual adicionando item e recalculando total.
- [x] Checkout visual abrindo com dados pessoais e resumo do pedido.
- [x] Area do cliente exibindo pedido criado, total comprado e endereco.
- [x] Cotacao de compra criada pela API publica.
- [x] Pedido de compra criado pela API publica.
- [x] Entrada de mercadoria registrada pela API publica.
- [x] Estoque atualizado pela entrada de mercadoria.
- [x] Codigo de barras, QR code e identificacao de estoque gerados no item recebido.
- [x] Conta a pagar local criada para pedido de compra.
- [x] Conta a pagar Genesis criada para pedido de compra.
- [x] Formulario visual de cotacao executado no painel admin.
- [x] Formulario visual de pedido de compra executado no painel admin.
- [x] Formulario visual de entrada de mercadoria executado no painel admin.
- [x] Pedido visual `COMP-20260629144221` criou financeiro local, Genesis e atualizou estoque.

## Checklist A Realizar Para Venda Controlada

Prioridade imediata:

- [x] Validar via API publica o fluxo completo: cliente -> checkout -> pedido criado -> financeiro -> Genesis.
- [x] Validar no navegador o fluxo: Home -> produto -> carrinho -> checkout.
- [ ] Validar no navegador o envio final do pedido criado.
- [ ] Confirmar que a Home publica esta consumindo `https://api.nexumaltivon.com.br` e nao cache antigo.
- [x] Validar criacao de contas a receber no pedido real de teste.
- [x] Validar painel administrativo refletindo pedido/faturamento criado.
- [x] Validar area do cliente exibindo o pedido criado.
- [ ] Ajustar texto de pagamento inicial para modo controlado: PIX/manual/deposito enquanto gateway real nao estiver liberado.
- [ ] Executar backup do estado publicado apos o teste de venda ponta a ponta.
- [x] Validar visualmente no painel admin os formularios de cotacao, pedido de compra e entrada apos correcao da API.

## Checklist A Realizar Para Operacao Full

Integracoes e canais:

- [ ] Ativar credenciais reais de gateway de pagamento.
- [ ] Ativar credenciais reais de logistica.
- [ ] Ativar marketplaces conforme tokens oficiais chegarem.
- [ ] Ativar dropshipping por parceiro com credencial propria.
- [ ] Registrar logs de sincronizacao por integracao.
- [ ] Criar tela administrativa de status por integracao.

Fiscal e financeiro:

- [ ] Instalar certificados digitais oficiais.
- [ ] Configurar emitentes reais por CNPJ.
- [ ] Homologar emissao fiscal real.
- [ ] Fechar contingencia fiscal manual.
- [ ] Validar contas a pagar, contas a receber, baixa e rastreio financeiro.

Operacao interna:

- [x] Finalizar backend operacional de compras, cotacoes, pedidos de compra e entrada fiscal/fisica.
- [x] Finalizar validacao visual dos formularios administrativos de compras no navegador.
- [x] Aplicar no painel de compras a regra de campos operacionais: solicitacao vinculada no pedido e listagens completas de solicitacoes, pedidos, produtos e entradas.
- [ ] Mapear todos os campos operacionais das tabelas atuais para formularios, detalhes, listagens, auditoria, relatorios ou integracoes.
- [ ] Incluir nos formularios existentes os novos campos criados nas tabelas, mantendo campos internos apenas em auditoria ou leitura tecnica.
- [ ] Expandir auditoria corporativa `sys_*`.
- [ ] Consolidar multitenancy e trilha de alteracoes por usuario.
- [ ] Validar reinicio do servidor com API, guardian e Cloudflare retornando sem intervencao.
- [ ] Revisar imagens dos produtos para eliminar divergencias visuais.

## Proxima Frente Recomendada

Atacar a transformacao das tabelas disponiveis em ferramentas operacionais completas, incluindo todos os campos de negocio nos formularios existentes e nos novos formularios. O modulo de aquisicao ja alimenta estoque, financeiro local e Genesis por API e pelo painel administrativo.

## Ordem De Execucao Atual

1. Envio final do checkout pelo navegador em pedido controlado.
2. Mapeamento campo a campo dos formularios por grupo de tabelas.
3. Backup do estado publicado apos validacao visual.
4. Integracoes reais conforme credenciais oficiais.
5. Fiscal, PDV e operacao full.
6. Backup operacional do estado publicado validado.


# Deploy privado - Grupo Nexum Altivon

Este projeto deve ser publicado apenas em repositorios privados e com banco de dados privado da empresa.

## Repositorio GitHub

Repositorio alvo:

```text
https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me.git
```

Antes do primeiro push, confirme no GitHub que o repositorio esta como `Private`.

## Segredos obrigatorios

Configure estes valores como GitHub Actions Secrets e tambem no arquivo `.env` do servidor de producao:

```text
API_DEFAULT_CONNECTION
JWT_SECRET_KEY
ADMIN_EMAIL
ADMIN_PASSWORD
ADMIN_NAME
ADMIN_ROLE
MP_ACCESS_TOKEN
SENDGRID_API_KEY
PROD_HOST
PROD_USER
PROD_SSH_KEY
```

`API_DEFAULT_CONNECTION` deve apontar para o MySQL privado da empresa. Use `docker/.env.example` como modelo e preencha a senha apenas no servidor ou nos secrets do GitHub.

## Deploy no servidor

No host de producao:

```bash
mkdir -p /opt/nexumaltivon/production
cd /opt/nexumaltivon/production
git clone https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-M.git .
cp docker/.env.example .env
```

Edite `.env` no proprio servidor com os valores reais.

Suba a aplicacao completa:

```bash
docker compose -f docker/docker-compose.prod.yml --env-file .env pull
docker compose -f docker/docker-compose.prod.yml --env-file .env up -d
docker compose -f docker/docker-compose.prod.yml --env-file .env ps
```

O compose de producao sobe:

- `frontend`: site React em Nginx.
- `api`: API ASP.NET em `8080` interno.
- `nginx`: proxy publico na porta `80`, roteando `www.nexumaltivon.com` para o front e `api.nexumaltivon.com` para a API.
- `watchtower`: atualizacao automatica das imagens publicadas no GHCR.

DNS esperado:

```text
nexumaltivon.com        A/AAAA -> IP do servidor
www.nexumaltivon.com    A/AAAA -> IP do servidor
api.nexumaltivon.com    A/AAAA -> IP do servidor
back.nexumaltivon.com   A/AAAA -> IP do servidor
admin.nexumaltivon.com  A/AAAA -> IP do servidor
erp.nexumaltivon.com    A/AAAA -> IP do servidor
crm.nexumaltivon.com    A/AAAA -> IP do servidor
pdv.nexumaltivon.com    A/AAAA -> IP do servidor
```

Para HTTPS, use um proxy/terminador TLS no servidor ou Cloudflare apontando para a porta `80` do `nexum-nginx`.

## Observacoes de seguranca

- Nao versionar `.env`, senhas, tokens, chaves SSH ou certificados.
- O banco de dados deve aceitar conexoes somente da rede/hosts autorizados.
- O usuario de banco usado pela API deve ter permissoes minimas para a aplicacao.
- Ative HTTPS antes de expor checkout, login ou pagamentos em producao.


# Documentação — Conexões e Publicação (Nexum Altivon)

Este documento é um “mapa” do que conecta com o quê (DNS → Front-End → API → Banco), e quais são as opções práticas de publicação em produção mantendo o banco privado.

## 1) Visão geral (arquitetura atual)

Fluxo principal:

1. Usuário acessa um hostname (ex.: `www.nexumaltivon.com`).
2. O DNS aponta o hostname para um **servidor público** (VPS) ou para o **seu servidor local exposto**.
3. O servidor entrega o **Front-End** (React build).
4. O Front-End chama a **API** (ASP.NET Core) via HTTP(S).
5. A API acessa o **MySQL** (idealmente privado).

## 2) Hostnames (módulos) e destinos

Requisito do projeto: manter os hostnames abaixo online e operantes.

Hostname | Função | Destino recomendado (fase 1)
---|---|---
`nexumaltivon.com` | raiz do domínio | mesmo do `www` (redirect para `www` no servidor)
`www.nexumaltivon.com` | Front-End (site + dashboard) | Front-End (React em IIS ou Nginx)
`api.nexumaltivon.com` | API pública | API (ASP.NET Core)
`back.nexumaltivon.com` | “Back-End” (alias) | mesma API de `api`
`admin.nexumaltivon.com` | backoffice / painel | mesmo Front-End (fase 1)
`erp.nexumaltivon.com` | ERP | mesmo Front-End (fase 1) + APIs ERP/CRM via API
`crm.nexumaltivon.com` | CRM | mesmo Front-End (fase 1) + endpoints CRM via API
`pdv.nexumaltivon.com` | PDV | mesmo Front-End (fase 1) + integrações via API

Observação importante:
- Na fase 1, **ERP/CRM/PDV/Admin** podem compartilhar o mesmo Front-End publicado e diferenciar por rotas (`/dashboard`, abas, etc.). Isso garante “no ar” sem travar a operação por falta de UI dedicada.

## 3) DNS (o que precisa existir)

Para cada hostname acima, você precisa de um DNS válido.

### 3.1 Quando você tem IP público (VPS ou link com port-forwarding)

Crie registros `A` apontando para o **IP público**:
- `@` → `IP_PUBLICO`
- `www` → `IP_PUBLICO`
- `api` → `IP_PUBLICO`
- `back` → `IP_PUBLICO`
- `admin` → `IP_PUBLICO`
- `erp` → `IP_PUBLICO`
- `crm` → `IP_PUBLICO`
- `pdv` → `IP_PUBLICO`

### 3.2 Onde editar (Wix x Cloudflare)

Você só deve editar no provedor que realmente está “servindo” o DNS da zona.

No Windows:

```bat
nslookup -type=ns nexumaltivon.com
```

- Se os NS forem do **Wix** → edite no Wix.
- Se os NS forem do **Cloudflare** → edite no Cloudflare.

## 4) Publicação (opções práticas)

### Opção A — VPS + Docker (produção recomendada)

Se você tiver uma VPS Linux com Docker:
- Use `docker/docker-compose.prod.yml` como base de produção.
- O `nginx` expõe `80` e roteia por hostname:
  - `www.* / erp.* / crm.* / pdv.* / admin.*` → Front-End
  - `api.* / back.*` → API

Arquivo chave:
- `C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\DEPLOY.md`

### Opção B — Servidor local (Windows + IIS)

Use quando você conseguir:
- IP público real (não CGNAT), e
- abrir portas `80`/`443` no roteador para o servidor (port forwarding).

Guia:
- `C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\IIS_DEPLOY.md`
- Runbook direto ao ponto:
  - `C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\PROD_DNS_IIS_RUNBOOK.md`

## 5) Banco de dados (privacidade e segurança)

Recomendação:
- **Não expor o MySQL na Internet**.

Melhores opções:
- Manter MySQL na LAN e a API no mesmo local (IIS/servidor local) **com DNS apontando para IP público** (se possível).
- Se usar VPS: colocar o MySQL na VPS, mas com firewall/segurança:
  - bind local (`127.0.0.1`) quando possível, ou
  - liberar só para IPs permitidos (allowlist), VPN, rede privada.

Ponto do projeto (já referenciado no template):
- MySQL em `Servidor_NexumAltivon` na rede local (ex.: `192.168.1.72`) usando `3309` (mapeamento comum do Docker dev).
- Template de connection string (produção) está em:
  - `C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\docker\.env.example`

## 6) API (ASP.NET Core) — portas e healthcheck

Dev local (sem IIS):
- API costuma rodar em `http://localhost:5000` (script supervisor).

Produção (Docker):
- API roda interna em `:8080` e o Nginx publica em `:80` por hostname.

Healthcheck:
- `http(s)://api.nexumaltivon.com/health`
- `http(s)://back.nexumaltivon.com/health`

## 7) Front-End (React) — publicação e rotas

Front-End publicado como estático (build).

Pontos práticos:
- Em IIS, é essencial ter rewrite para SPA (refresh não pode virar 404).
- Em produção, o Front-End chama a API via `REACT_APP_BACKEND_URL` (em build/dev) ou URL configurada no ambiente.

## 8) Segredos e variáveis (obrigatórios)

Os segredos descritos em `DEPLOY.md` devem existir no servidor e/ou GitHub secrets:
- `API_DEFAULT_CONNECTION`
- `JWT_SECRET_KEY`
- `ADMIN_EMAIL`, `ADMIN_PASSWORD`, `ADMIN_NAME`, `ADMIN_ROLE`
- `MP_ACCESS_TOKEN`
- `SENDGRID_API_KEY`
- `PROD_HOST`, `PROD_USER`, `PROD_SSH_KEY` (se houver pipeline de deploy)

Regra: **não versionar** senhas/tokens/chaves.

## 9) Evidência rápida de “está no ar”

Teste sempre fora da sua rede (4G):
- `http://www.nexumaltivon.com`
- `http://api.nexumaltivon.com/health`

Se falhar:
- primeiro confirme o NS ativo (Wix/Cloudflare),
- depois confirme o IP público,
- depois confira portas `80/443`, firewall, e bindings (host header) no IIS.



# Handoff privado de produção

Este repositório **não deve receber segredos reais**.  
Preencha credenciais reais **somente no servidor**.

## Arquivo-base

Use como modelo:

- `NexumAltivon_Back-End/API/appsettings.PrivateProduction.template.json`

Crie no servidor um arquivo privado fora do versionamento, por exemplo:

- `appsettings.Production.json`

## Campos mínimos antes de liberar a equipe

### Base operacional

- `ConnectionStrings:DefaultConnection`
- `JwtSettings:SecretKey`
- `AdminUser:Password`
- `EmailSettings:Password`

### Shopify

- `Shopify:StoreDomain`
- `Shopify:ApiVersion`
- `Shopify:AdminApiAccessToken`
- `Shopify:WebhookSecret`

### CJ Dropshipping

- `CJDropshipping:ApiEndpoint`
- `CJDropshipping:AccessToken`
- `CJDropshipping:WebhookSecret`

## Validação segura

Depois de preencher no servidor:

1. subir a API;
2. entrar no painel administrativo;
3. abrir `Integrações`;
4. testar `Shopify`;
5. testar `CJ Dropshipping`;
6. só então vincular produtos reais aos canais.

## Regras de segurança

- não commitar `appsettings.Production.json`;
- não enviar tokens por chat;
- não salvar senha em arquivo público do front;
- usar somente backend/servidor para tokens e segredos;
- revisar logs antes de liberar a operação.

# Publicacao em servidor local (Windows + IIS) - Nexum Altivon

Este guia serve para colocar `www.nexumaltivon.com` e `api.nexumaltivon.com` (e os demais subdominios) online usando um servidor Windows local com IIS.

> Importante: o IP `192.168.x.x` e outros IPs privados nao funcionam na Internet. DNS publico precisa apontar para um IP publico OU voce precisa usar um tunel (Cloudflare Tunnel).

## 1) Escolha como expor o servidor local

### Opcao A (direto na internet): IP publico + Port Forwarding

Use esta opcao somente se o seu link tiver **IP publico** e voce conseguir abrir as portas no roteador.

Checklist:
- IP do servidor LAN fixo (ex.: `192.168.1.72`) reservado no roteador (DHCP reservation).
- Port forwarding no roteador:
  - WAN `80`  -> `192.168.1.72:80`
  - WAN `443` -> `192.168.1.72:443`
- Firewall do Windows liberando `80` e `443` (somente para IIS).
- DNS no Wix apontando `@` e subdominios para o seu **IP publico**.

### Opcao B (recomendado p/ CGNAT e IP dinamico): Cloudflare Tunnel

Use esta opcao se:
- seu provedor usa CGNAT (port forwarding nao funciona), OU
- seu IP publico muda com frequencia, OU
- voce quer evitar abrir portas no roteador.

Resumo:
- Voce instala o `cloudflared` no servidor Windows
- Cria um Tunnel no Cloudflare
- Aponta cada hostname (www/api/back/erp/crm/pdv) para `<UUID>.cfargotunnel.com` via CNAME
- O Tunnel encaminha para `http://localhost:80` (IIS) e `http://localhost:5000` (API) ou o que voce definir.

> Atenção (importante): para usar hostnames personalizados com Cloudflare Tunnel, o DNS/rota desses hostnames precisa estar na **mesma conta Cloudflare**. Em geral isso exige o dominio no Cloudflare DNS (trocar nameservers) ou um setup partial (CNAME setup), que costuma ser Business/Enterprise.

## 2) Preparar o IIS (uma vez)

1. Instale o IIS (Windows Features).
2. Instale o **ASP.NET Core Hosting Bundle** compativel com o runtime do projeto.
3. Reinicie o IIS (ou o servidor) apos instalar o bundle.

## 3) Publicar o Front-End (www + erp/crm/pdv/admin provisoriamente)

O front-end ja possui pasta `build/` dentro de `NexumAltivon_Front-End`.

No IIS:
- Crie um Site: `Nexum-Frontend`
- Physical Path: `...\NexumAltivon_Front-End\build`
- Binding HTTP: `*:80` Hostname `www.nexumaltivon.com`
- (Opcional) Adicione bindings adicionais no mesmo Site:
  - `erp.nexumaltivon.com`
  - `crm.nexumaltivon.com`
  - `pdv.nexumaltivon.com`
  - `admin.nexumaltivon.com`

> Isso permite deixar os modulos "no ar" imediatamente, mesmo que eles ainda redirecionem/compartilhem o mesmo front-end por enquanto.

## 4) Publicar a API (api + back)

No servidor Windows (PowerShell/cmd):
1. Publique a API:
   - Projeto: `NexumAltivon_Back-End\NexumAltivon.API.csproj`
   - Saida sugerida: `C:\inetpub\nexum-api\`

2. No IIS:
   - Crie um Site: `Nexum-Api`
   - Physical Path: `C:\inetpub\nexum-api\`
   - App Pool: `No Managed Code`
   - Binding HTTP: `*:80` Hostname `api.nexumaltivon.com`
   - Adicione tambem Hostname `back.nexumaltivon.com` (mesma API)

3. Configure as variaveis de ambiente/secrets no servidor (nao versionar senhas):
   - Connection string MySQL
   - JWT secret
   - Admin user (email/senha)
   - Mercado Pago / SendGrid, etc.

## 5) DNS no Wix (apontamentos)

### Se estiver usando Opcao A (IP publico + port forwarding)

Crie registros `A` apontando para o seu IP publico:
- `@` -> `IP_PUBLICO`
- `www` -> `IP_PUBLICO`
- `api` -> `IP_PUBLICO`
- `back` -> `IP_PUBLICO`
- `erp` -> `IP_PUBLICO`
- `crm` -> `IP_PUBLICO`
- `pdv` -> `IP_PUBLICO`
- `admin` -> `IP_PUBLICO`

### Se estiver usando Opcao B (Cloudflare Tunnel)

Em geral voce criara `CNAME` apontando para `<UUID>.cfargotunnel.com` para cada hostname que vai passar pelo Tunnel (ex.: `www`, `api`, `back`, etc.).

## 6) Validacao rapida

Valide de fora da rede (ex.: 4G do celular):
- `https://www.nexumaltivon.com`
- `https://api.nexumaltivon.com/health`

## 7) Regras de seguranca (nao negociar)

- Nao exponha o MySQL na internet. O MySQL deve ficar apenas na rede local/VPN.
- Use HTTPS antes de trafegar login/checkout/pagamentos.
- Nao coloque tokens/senhas em repositorio.


# Runbook (Produção) — DNS + IIS — Nexum Altivon

Objetivo: deixar **todos os subdomínios respondendo** (mesmo que alguns módulos compartilhem a mesma aplicação no início) e com **API + Front-End operantes** para a equipe iniciar importações/cadastros.

## 0) Ponto crítico (não pular)

- **DNS público não aceita IP privado** (ex.: `192.168.x.x`). Para a Internet enxergar seus hostnames, você precisa de:
  - **IP público** + portas abertas para o IIS (ou reverse proxy), **ou**
  - **VPS** com IP público, **ou**
  - **Cloudflare (DNS/Proxy/Tunnel)** — mas isso depende do tipo de setup do seu domínio (ver seção 6).

## 1) Escolha o caminho (recomendado: VPS)

### Caminho A — VPS (mais rápido e estável p/ produção)

1. Contrate uma VPS com **IP público** (preferencialmente IP fixo).
2. Publique nela (Linux com Docker **ou** Windows com IIS).
3. No Wix, aponte os DNS `A` para o IP da VPS (seção 2).

Vantagem: não depende de CGNAT, não depende de abrir portas no seu roteador, e dá uptime melhor.

### Caminho B — Servidor local + IIS (somente se der IP público e portas)

Pré-requisitos:
- Link com **IP público** (se for CGNAT, port-forwarding não funciona).
- Port forwarding no roteador: `80` e `443` indo para o servidor (ex.: `192.168.1.72`).
- Firewall do Windows liberando `80/443`.
- Energia: no Windows, deixe **Suspensão = Nunca** (pode apagar a tela, mas não pode “dormir”).

Depois siga seções 2, 3 e 4.

## 2) DNS no Wix (passo a passo)

Antes de mexer em registros: confirme **onde o DNS está ativo** (Wix ou Cloudflare).

No Windows (no seu PC), rode:

```bat
nslookup -type=ns nexumaltivon.com
```

- Se os nameservers apontarem para **Wix**, ajuste os registros no Wix.
- Se apontarem para **Cloudflare**, ajuste os registros no Cloudflare (não no Wix), senão nada muda.

No Wix: **Domínios → Gerenciar DNS → Registros DNS**.

Crie/ajuste registros `A` apontando para o **IP público** do seu servidor (VPS ou link com port-forwarding):

Host / Nome | Tipo | Valor / Aponta para
---|---|---
`@` | `A` | `IP_PUBLICO`
`www` | `A` | `IP_PUBLICO`
`api` | `A` | `IP_PUBLICO`
`back` | `A` | `IP_PUBLICO`
`admin` | `A` | `IP_PUBLICO`
`erp` | `A` | `IP_PUBLICO`
`crm` | `A` | `IP_PUBLICO`
`pdv` | `A` | `IP_PUBLICO`

Notas:
- Se o Wix não permitir `A` duplicado para `www` (ou já existir um `CNAME`), remova o conflito e deixe **apenas 1 tipo por hostname** (regra geral de DNS).
- Se você quiser forçar `nexumaltivon.com` → `www.nexumaltivon.com`, faça isso no **servidor** (IIS/redirect) ou via regra do provedor, sem depender de “CNAME na raiz”.

## 3) IIS — publicar o Front-End (www + módulos “placeholder”)

O **Front-End atual** atende site + dashboard (admin/ERP/CRM/PDV) no mesmo build.

1. Gere o build do front-end:
   - Pasta: `NexumAltivon_Front-End`
   - Comando: `npm ci` e `npm run build`
2. No IIS, crie um site: `Nexum-Frontend`
   - Physical Path: `...\NexumAltivon_Front-End\build`
   - Binding HTTP: `*:80` Hostname `www.nexumaltivon.com`
3. Adicione bindings adicionais no **mesmo site**:
   - `admin.nexumaltivon.com`
   - `erp.nexumaltivon.com`
   - `crm.nexumaltivon.com`
   - `pdv.nexumaltivon.com`

Importante (React/SPA):
- Garanta regra de rewrite para que refresh em rotas (ex.: `/dashboard`) não dê 404.
- Se necessário, crie um `web.config` no `build/` com rewrite para `index.html`.

## 4) IIS — publicar a API (api + back)

1. Publique a API:
   - Projeto: `NexumAltivon_Back-End\NexumAltivon.API.csproj`
   - Exemplo: `dotnet publish -c Release -o C:\inetpub\nexum-api\`
2. No IIS:
   - Crie um Site: `Nexum-Api`
   - Physical Path: `C:\inetpub\nexum-api\`
   - App Pool: `No Managed Code`
   - Binding HTTP: `*:80` Hostname `api.nexumaltivon.com`
   - Adicione Hostname `back.nexumaltivon.com` (mesma API)
3. Configure os secrets/variáveis no servidor (não commitar):
   - Connection string MySQL (ideal: MySQL privado, sem exposição pública)
   - JWT secret
   - Admin user (email/senha)
   - Tokens (Mercado Pago / SendGrid etc.)

## 5) Validação externa (prova de “está online”)

Testar de fora da rede (4G do celular):
- `http://www.nexumaltivon.com`
- `http://api.nexumaltivon.com/health`
- `http://back.nexumaltivon.com/health`
- `http://erp.nexumaltivon.com`
- `http://crm.nexumaltivon.com`
- `http://pdv.nexumaltivon.com`
- `http://admin.nexumaltivon.com` (deve redirecionar/abrir o dashboard)

Se ainda estiver “OFF”:
- Confirme que o DNS que você editou é o **ativo** (seção 2).
- Confirme que `IP_PUBLICO` é realmente o IP que a Internet enxerga (não `192.168.x.x`).
- Se for servidor local: confirme se o seu link não é **CGNAT** e se o roteador está com port-forwarding de `80/443`.
- Confirme firewall do Windows + bindings no IIS (Hostname correto).

## 6) Cloudflare (para quando “sem abrir portas” for obrigatório)

Dois pontos importantes:
- **CNAME setup (partial)** “sem trocar nameservers” é **Business/Enterprise** no Cloudflare.
- O hostname `<UUID>.cfargotunnel.com` do Tunnel só “funciona” quando o DNS/rota do hostname é gerenciado dentro da **mesma conta Cloudflare** (na prática, isso tende a exigir o domínio no Cloudflare DNS, ou setup partial elegível).

Se você estiver em **plano Free** e não puder trocar nameservers no Wix, o caminho mais simples geralmente vira:
- **VPS** + DNS `A` no Wix (seção 2).

## 7) Segurança (não negociar)

- **Não exponha MySQL na Internet**. Se precisar banco fora, use VPN/allowlist/privatenet.
- Habilite **HTTPS** antes de login/checkout/pagamentos reais.
- Não coloque senhas/tokens em GitHub nem em arquivos versionados.


# Checklist de atualizacao segura em producao

Objetivo: toda alteracao publicada em producao deve ser validada antes e depois da subida, sem derrubar o site, a API ou o painel operacional.

## 1) Antes de publicar

- Confirmar escopo exato da alteracao.
- Gerar backup local atualizado quando a alteracao mexer em deploy, API, banco, DNS ou painel.
- Rodar build local do front-end:
  - `npm run build` em `NexumAltivon_Front-End`.
- Rodar publish local da API quando houver alteracao no backend:
  - `dotnet publish NexumAltivon_Back-End\NexumAltivon.API.csproj -c Release`.
- Conferir que nenhum segredo real foi versionado.
- Conferir se URLs publicas estao corretas:
  - Front-end deve chamar `https://api.nexumaltivon.com`.
  - Login deve usar `/api/auth/login`.

## 2) Publicacao

- Publicar primeiro o front-end quando a alteracao for visual ou de apontamento da API.
- Publicar API separadamente quando houver alteracao de backend.
- Evitar trocar DNS junto com codigo, salvo quando isso for o objetivo da etapa.
- Manter uma versao anterior pronta para rollback.

## 3) Depois de publicar

Testar fora do ambiente local:

- `https://www.nexumaltivon.com`
- `https://www.nexumaltivon.com/login`
- `https://api.nexumaltivon.com/`
- `https://api.nexumaltivon.com/health`

Validar no painel:

- Login administrativo.
- Abrir dashboard.
- Listar produtos.
- Cadastrar um produto de teste controlado.
- Cadastrar cliente de teste controlado.
- Cadastrar fornecedor de teste controlado.
- Cadastrar lead de teste controlado.

## 4) Se algo falhar

- Nao continuar novas alteracoes.
- Identificar se a falha esta em DNS, front-end, API, banco ou senha/variavel de ambiente.
- Fazer rollback para o ultimo pacote funcional.
- Registrar o erro e a acao tomada antes de tentar nova publicacao.

## 5) Variaveis obrigatorias da API em producao

- `AdminUser__Email`
- `AdminUser__Password`
- `AdminUser__Name`
- `AdminUser__Role`
- `JwtSettings__SecretKey`
- `ConnectionStrings__DefaultConnection`



# Produção - status atual

Última atualização manual: 2026-06-02 02:45 BRT.

## Estado verificado

- `https://www.nexumaltivon.com/`: respondeu `200 OK`.
- `www.nexumaltivon.com`: aponta para `corporativogna-lrc.github.io` e IPs do GitHub Pages.
- `https://www.nexumaltivon.com/login`: respondeu `404 Not Found` em checagem HTTP direta.
- `api.nexumaltivon.com`: DNS público retornou `Non-existent domain`.
- `https://api.nexumaltivon.com/`: não pôde ser acessado porque o DNS da API não resolve.

## Regra operacional

Antes e depois de qualquer alteração em produção, executar:

```cmd
scripts\check-production.cmd
```

## Critério para publicar

- Site principal precisa responder `200 OK`.
- Login precisa abrir sem quebrar rota.
- API precisa resolver DNS e responder HTTP.
- Se qualquer item acima falhar, pausar deploy e corrigir a causa antes de continuar.


<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 -->
# NEXUM ALTIVON COMMERCE PLATFORM
## FASE 2 — Painel Administrativo, Dashboard, Gestao Completa

### Grupo Nexum Altivon ME — www.nexumaltivon.com

---

## Entregaveis da FASE 2

### 1. Back-End — API Admin Dashboard

| Arquivo | Descricao |
|---|---|
| `Services/Admin/AdminDashboardService.cs` | Servico completo de KPIs, graficos e metricas |
| `Controllers/Admin/DashboardController.cs` | 8 endpoints REST para dashboard |
| `DTOs/Admin/DashboardDtos.cs` | 8 DTOs de KPIs, faturamento, vendas, produtos, clientes, leads |

#### Endpoints Admin (requer Gerente+)

| Endpoint | Metodo | Descricao |
|---|---|---|
| `/api/admin/dashboard/completo` | GET | Dashboard completo com tudo |
| `/api/admin/dashboard/kpis` | GET | Apenas KPIs principais |
| `/api/admin/dashboard/faturamento/semanal` | GET | Grafico de faturamento 7 dias |
| `/api/admin/dashboard/faturamento/mensal` | GET | Grafico de faturamento 12 meses |
| `/api/admin/dashboard/vendas/lojas` | GET | Vendas por loja (pie chart) |
| `/api/admin/dashboard/produtos/mais-vendidos` | GET | Top produtos (bar chart) |
| `/api/admin/dashboard/clientes/recentes` | GET | Clientes recentes |
| `/api/admin/dashboard/pedidos/recentes` | GET | Pedidos recentes |
| `/api/admin/dashboard/leads/recentes` | GET | Leads recentes |

### 2. Front-End — Painel Administrativo

| Arquivo | Descricao |
|---|---|
| `NexumAltivon_Front-End/src/pages/Dashboard.js` | Painel administrativo operacional integrado a API oficial |
| `Arquivos_Mortos/frontend-legado-20260629/admin-index-mock-legado.html` | Painel HTML antigo arquivado, fora da publicacao |

#### Funcionalidades do Painel

- **KPI Cards**: 6 cards com faturamento, pedidos, clientes, ticket medio, estoque baixo, leads
- **Grafico Faturamento Semanal**: Line chart com 7 dias
- **Grafico Vendas por Loja**: Doughnut chart com as 6 lojas
- **Grafico Faturamento Mensal**: Bar chart com 12 meses
- **Grafico Produtos Mais Vendidos**: Horizontal bar chart
- **Tabela Pedidos Recentes**: Ultimos 5 pedidos com status
- **Tabela Leads Recentes**: Ultimos 5 leads com prioridade
- **Tabela Clientes Recentes**: Ultimos 5 clientes com gasto total
- **Menu Lateral**: 15 secoes navegaveis (Dashboard, Pedidos, Produtos, Clientes, Lojas, Financeiro, Fiscal, Logistica, CRM, Cupons, Marketing, Marketplaces, Dropshipping, Usuarios, Configuracoes, Auditoria)
- **Design**: Escuro premium, cores gold #C9A227, identico ao site principal

#### Secoes do Menu (todas estruturadas)

1. **Dashboard** — Completo com KPIs e graficos
2. **Pedidos** — Tabela com filtros e exportacao
3. **Produtos** — Catalogo com estoque e acoes
4. **Clientes** — Base completa com perfis
5. **Lojas** — Cards das 6 lojas com metricas
6. **Financeiro** — KPIs financeiros
7. **Fiscal** — pendente de credenciais fiscais/certificado digital para emissao real
8. **Logistica** — em integracao real por transportadoras e regras de envio
9. **CRM** — Tabela de leads completa
10. **Cupons** — fluxo operacional vinculado a regras comerciais
11. **Marketing** — fluxo operacional vinculado a leads e notificacoes
12. **Marketplaces** — aguardando tokens reais para ativacao plena
13. **Dropshipping** — aguardando credenciais de fornecedores para ativacao plena
14. **Usuarios** — fluxo operacional por perfis administrativos
15. **Configuracoes** — Painel de configuracoes editaveis
16. **Auditoria** — trilha operacional para eventos e alteracoes criticas

---

## KPIs Implementados

| KPI | Fonte |
|---|---|
| Faturamento Hoje/Mes/Ano | Soma de pedidos aprovados |
| Pedidos Hoje/Mes/Pendentes/Enviados/Entregues | Count por status |
| Clientes Novos/Ativos/Total | Count de clientes |
| Ticket Medio | AVG(total) dos pedidos |
| Taxa de Conversao | Pedidos / Visitas (placeholder) |
| Produtos Ativos/Estoque Baixo/Sem Estoque | Count por condicao |
| Leads Novos/Convertidos/Em Atendimento | Count por status |

---

## Tecnologias Front-End do Painel

- Chart.js 4.4.1 (graficos)
- Font Awesome 6.5.0 (icones)
- Google Fonts Montserrat (tipografia)
- CSS Grid/Flexbox (layout responsivo)
- JavaScript Vanilla (sem frameworks)

---

## Proxima Fase

| Fase | Entregaveis |
|---|---|
| **FASE 3** | Carrinho Funcional, Checkout, Gateway Pagamento |
| **FASE 4** | Logistica, Marketplaces, Dropshipping |
| **FASE 5** | CRM Avancado, Automacoes, IA, Analytics |

---

(c) 2026 Grupo Nexum Altivon ME — Todos os direitos reservados


# FASE 3 — Carrinho, Checkout e Gateway de Pagamento

## Grupo Nexum Altivon — www.nexumaltivon.com

### Arquivos Entregues

| # | Arquivo | Descrição |
|---|---------|-----------|
| 1 | `DTOs/CarrinhoDtos.cs` | DTOs do carrinho e itens |
| 2 | `DTOs/CheckoutDtos.cs` | DTOs de checkout, endereço, frete e finalização |
| 3 | `DTOs/PagamentoDtos.cs` | DTOs de pagamento, webhook e reembolso |
| 4 | `Services/CarrinhoService.cs` | Carrinho com sessão + cliente, cupom, migração |
| 5 | `Services/CheckoutService.cs` | Fluxo completo de checkout |
| 6 | `Services/MercadoPagoService.cs` | Gateway PIX, Cartão e Boleto + webhooks |
| 7 | `Services/FreteService.cs` | Cálculo de frete (Melhor Envio + tabela própria) |
| 8 | `Services/NotificacaoService.cs` | E-mail (SendGrid) + WhatsApp |
| 9 | `Services/PedidoService.cs` | Geração de pedido e número NXYYMMDDXNNN |
| 10 | `Controllers/CarrinhoController.cs` | API pública de carrinho (anônima) |
| 11 | `Controllers/CheckoutController.cs` | API protegida de checkout |
| 12 | `Controllers/PagamentoController.cs` | Gestão de pagamentos e reembolsos |
| 13 | `Controllers/WebhookController.cs` | Recepção de webhooks Mercado Pago |
| 14 | `Models/CarrinhoCheckoutPagamento.cs` | Entidades EF Core complementares |
| 15 | `Configurations/ServiceExtensions.cs` | Registro de DI dos novos serviços |

### Configurações necessárias em `appsettings.json`

```json
{
  "Integracoes": {
    "MercadoPago": {
      "AccessToken": "TEST-xxxxxxxxxxxxxxxx",
      "WebhookSecret": "seu_secret",
      "Sandbox": true
    },
    "MelhorEnvio": {
      "Ativo": false,
      "Token": "seu_token",
      "CepOrigem": "01001000"
    },
    "SendGrid": {
      "ApiKey": "SG.xxxxx",
      "FromEmail": "naoresponder@nexumaltivon.com",
      "FromName": "Grupo Nexum Altivon"
    },
    "WhatsApp": {
      "Ativo": false,
      "ApiUrl": "http://sua-api-whatsapp:8080/message/sendText",
      "ApiKey": "sua_chave"
    }
  },
  "Alertas": {
    "EstoqueEmailAdmin": "vinicius@nexumaltivon.com"
  }
}
```

### Fluxo de Uso

1. **Cliente anônimo** adiciona itens ao carrinho (cookie `nx_session_id`)
2. Ao **logar**, chama `POST /api/carrinho/migrar` para unir carrinhos
3. Inicia checkout: `POST /api/checkout/iniciar` (endereço + cupom)
4. Seleciona frete: `POST /api/checkout/{id}/frete`
5. Finaliza: `POST /api/checkout/finalizar` (PIX, Cartão ou Boleto)
6. Recebe QR Code / link / boleto na resposta
7. Webhook MP atualiza status automaticamente para "PAGO"

### Próxima Fase
- **FASE 4**: Integrações com Marketplaces (Mercado Livre, Shopee), Dropshipping e Logística completa.


# FASE 4 — Integrações Completas

## Grupo Nexum Altivon — www.nexumaltivon.com

### Arquivos Entregues

| # | Arquivo | Descrição |
|---|---------|-----------|
| 1 | `DTOs/MarketplaceDtos.cs` | DTOs Mercado Livre, Shopee, Amazon, Hub unificado |
| 2 | `DTOs/DropshippingDtos.cs` | DTOs de roteamento, comissão, fornecedores |
| 3 | `DTOs/LogisticaDtos.cs` | DTOs de etiquetas, rastreamento, dashboard |
| 4 | `DTOs/ErpSyncDtos.cs` | DTOs de sincronização GenesisGest.Net |
| 5 | `Services/MercadoLivreService.cs` | Publicar, atualizar, importar pedidos ML |
| 6 | `Services/MarketplaceHubService.cs` | Hub multi-canal: Shopee, Amazon, sync automático |
| 7 | `Services/DropshippingService.cs` | Roteamento inteligente, comissões, notificações |
| 8 | `Services/LogisticaService.cs` | Etiquetas, rastreamento, status de entrega |
| 9 | `Services/ErpSyncService.cs` | Bridge GenesisGest.Net (produtos, clientes, pedidos, estoque) |
| 10 | `Services/MarketplaceSyncService.cs` | Orquestrador de sync automático e logs |
| 11 | `Controllers/IntegracoesController.cs` | Hub unificado REST (marketplaces, dropshipping, logística, ERP) |
| 12 | `Models/IntegracoesModels.cs` | Entidades: MarketplaceProduto, DropshippingPedido, Fornecedor, Transportadora, Etiqueta, SyncLog |
| 13 | `Configurations/IntegrationExtensions.cs` | Registro de DI das integrações |
| 14 | `README_Fase4.md` | Este documento |

### Endpoints da API de Integrações

#### Marketplaces
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/marketplaces/sync` | POST | Admin/Gerente | Sincroniza produto em canais |
| `/api/integracoes/marketplaces/sync-lote` | POST | Admin/Gerente | Sincroniza lote de produtos |
| `/api/integracoes/marketplaces/relatorio` | GET | Admin/Gerente | Relatório de sync por período |
| `/api/integracoes/marketplaces/status/{id}` | GET | Admin/Gerente | Status de sync de produto |
| `/api/integracoes/mercadolivre/publicar/{id}` | POST | Admin/Gerente | Publica produto no ML |
| `/api/integracoes/mercadolivre/importar-pedidos` | POST | Admin/Gerente | Importa pedidos pendentes do ML |
| `/api/integracoes/mercadolivre/marcar-enviado/{id}` | POST | Admin/Gerente | Marca pedido ML como enviado |

#### Dropshipping
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/dropshipping/roteiar` | POST | Admin/Gerente | Roteia pedido para fornecedor |
| `/api/integracoes/dropshipping/pendentes` | GET | Admin/Gerente | Lista pedidos pendentes |
| `/api/integracoes/dropshipping/{id}/status` | PUT | Admin/Gerente | Atualiza status/envio |
| `/api/integracoes/dropshipping/fornecedores` | GET | Admin/Gerente | Lista fornecedores |
| `/api/integracoes/dropshipping/comissao/{id}` | GET | Admin/Gerente | Relatório de comissão |

#### Logística
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/logistica/etiqueta` | POST | Admin/Gerente | Gera etiqueta de envio |
| `/api/integracoes/logistica/rastrear/{codigo}` | GET | Público | Rastreia envio |
| `/api/integracoes/logistica/status-envio` | PUT | Admin/Gerente | Atualiza status de entrega |
| `/api/integracoes/logistica/dashboard` | GET | Admin/Gerente | Dashboard operacional |
| `/api/integracoes/logistica/transportadoras` | GET | Admin/Gerente | Lista transportadoras |

#### ERP GenesisGest.Net
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/erp/sync` | POST | Admin | Sincroniza produtos/clientes/pedidos/estoque |
| `/api/integracoes/erp/status` | GET | Admin | Testa conexão com ERP |
| `/api/integracoes/erp/configuracao` | GET/PUT | Admin | Gerencia configuração |

#### Sync Automático
| Endpoint | Método | Acesso | Descrição |
|----------|--------|--------|-----------|
| `/api/integracoes/sync/executar-agendado` | POST | Admin | Executa sync manual agendado |
| `/api/integracoes/sync/logs` | GET | Admin | Logs de sincronização |

### Configurações `appsettings.json`

```json
{
  "Integracoes": {
    "MercadoLivre": {
      "AccessToken": "APP_USR-...",
      "SellerId": "123456789"
    },
    "Shopee": {
      "BaseUrl": "https://partner.shopeemobile.com",
      "PartnerId": "123456",
      "ShopId": "789012"
    },
    "MelhorEnvio": {
      "Ativo": true,
      "Token": "...",
      "CepOrigem": "01001000"
    },
    "GenesisGest": {
      "UrlBase": "http://192.168.1.72:8080",
      "TokenApi": "...",
      "AutoSync": true,
      "IntervaloMinutos": 60,
      "Entidades": ["PRODUTOS", "CLIENTES", "PEDIDOS", "ESTOQUE"]
    }
  }
}
```

### Próximos Passos
- Configurar tokens reais de cada marketplace
- Implementar tokenização PCI-compliant para cartões (MercadoPago.js)
- Ativar webhook de confirmação de envio do Melhor Envio
- Configurar job recorrente (Hangfire/Quartz) para sync automático ERP


# FASE 5 — ERP/CRM GenesisGest.Net
## Grupo Nexum Altivon ME | www.nexumaltivon.com

---

## 📋 Sumário

Esta fase entrega o **ERP/CRM completo em C#** (GenesisGest.Net), integrado ao banco MySQL existente da Fase 1. O sistema gerencia financeiro, fiscal, estoque avançado, CRM e fornecedores, com bridge de sincronização bidirecional com o e-commerce.

---

## 🗂️ Estrutura de Arquivos

```
NexumAltivon_ERP/
├── Models/
│   ├── FinanceiroModels.cs          → Contas Pagar/Receber, Fluxo Caixa, Centros Custo, Contas Bancárias
│   ├── FiscalEstoqueModels.cs       → NFe, Itens NF, Movimentações, Inventário, Kardex, Locais
│   └── CrmFornecedorModels.cs       → Leads, Interações, Tarefas, Fornecedores, Avaliações
├── DTOs/
│   ├── FinanceiroDtos.cs            → DRE, Resumo, Fluxo, Baixa de títulos
│   ├── CrmDtos.cs                   → Leads, Pipeline, Tarefas, Interações
│   └── ErpDtos.cs                   → DTOs consolidados de todas as entidades
├── Services/
│   ├── FinanceiroService.cs         → Contas Pagar/Receber, DRE, Fluxo de Caixa
│   ├── CrmService.cs                → Leads, Pipeline, Conversão, Tarefas
│   ├── EstoqueService.cs            → Movimentações, Inventário, Kardex, Transferências
│   ├── FiscalService.cs             → Emissão NFe, Cancelamento, Impostos
│   ├── RelatorioService.cs          → DRE, Fluxo Caixa, Posição Estoque, Ranking
│   ├── SyncErpService.cs            → Bridge ERP ↔ E-Commerce
│   └── FornecedorService.cs         → Gestão de fornecedores e avaliações
├── Controllers/
│   ├── FinanceiroController.cs      → /api/erp/financeiro/*
│   ├── CrmController.cs             → /api/erp/crm/*
│   ├── EstoqueController.cs         → /api/erp/estoque/*
│   ├── FiscalController.cs          → /api/erp/fiscal/*
│   ├── RelatoriosController.cs      → /api/erp/relatorios/*
│   ├── ErpDashboardController.cs    → /api/erp/dashboard/*
│   ├── FornecedoresController.cs    → /api/erp/fornecedores/*
│   └── SyncController.cs            → /api/erp/sync/*
├── Data/
│   ├── NexumDbContext_ERP.cs        → Extensão do DbContext com índices otimizados
│   ├── ErpDbContext.cs              → DbContext isolado do ERP
│   └── GenesisDbContext.cs          → DbContext legado GenesisGest.Net
├── Database/
│   └── erp_schema_update.sql        → Script SQL de atualização (15 tabelas + views + procedures)
├── Configurations/
│   ├── ServiceExtensions.cs         → Registro de DI unificado
│   └── ErpMappingProfile.cs         → AutoMapper profiles
└── README_Fase5.md                  → Este arquivo
```

---

## 🗄️ Banco de Dados — Novas Tabelas (15)

| Tabela | Domínio | Descrição |
|---|---|---|
| `erp_centros_custo` | Financeiro | Estrutura analítica de custos |
| `erp_contas_bancarias` | Financeiro | Carteiras e saldos bancários |
| `erp_contas_pagar` | Financeiro | Títulos de obrigações |
| `erp_contas_receber` | Financeiro | Títulos de receitas |
| `erp_fluxo_caixa` | Financeiro | Movimentação diária de caixa |
| `erp_notas_fiscais` | Fiscal | Cabeçalho NFe |
| `erp_itens_nota_fiscal` | Fiscal | Itens com impostos |
| `erp_impostos_config` | Fiscal | Alíquotas por NCM/CFOP |
| `erp_movimentacoes_estoque` | Estoque | Entradas, saídas, transferências |
| `erp_inventarios` | Estoque | Contagem física |
| `erp_itens_inventario` | Estoque | Produtos contados |
| `erp_kardex` | Estoque | Rastreamento contábil |
| `erp_locais_estoque` | Estoque | Endereçamento físico |
| `erp_leads_crm` | CRM | Captação de oportunidades |
| `erp_interacoes_crm` | CRM | Histórico de contatos |
| `erp_tarefas_crm` | CRM | Follow-up e compromissos |
| `erp_fornecedores` | Fornecedores | Cadastro completo |
| `erp_avaliacoes_fornecedor` | Fornecedores | Notas e comentários |

---

## ⚙️ Como Aplicar

### Passo 1: Executar Script SQL

```bash
mysql -h 192.168.1.72 -P 3309 -u root -p nexum_altivon < Database/erp_schema_update.sql
```

### Passo 2: Integrar ao Projeto Visual Studio

1. Copie todos os arquivos `.cs` para o projeto `NexumAltivon.API` (Fase 1)
2. Adicione as `DbSet` do `NexumDbContext_ERP.cs` ao seu `NexumDbContext` principal
3. Chame `OnModelCreatingERP(modelBuilder)` no `OnModelCreating` do DbContext
4. Execute `dotnet ef migrations add Fase5_ERP`
5. Execute `dotnet ef database update`

### Passo 3: Registrar Serviços (Program.cs)

```csharp
builder.Services.AddScoped<IFinanceiroService, FinanceiroService>();
builder.Services.AddScoped<ICrmService, CrmService>();
builder.Services.AddScoped<IEstoqueService, EstoqueService>();
builder.Services.AddScoped<IFiscalService, FiscalService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<ISyncErpService, SyncErpService>();
```

---

## 🔌 Principais Endpoints da API ERP

### Financeiro
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/financeiro/contas-pagar` | POST | Criar conta a pagar |
| `/api/erp/financeiro/contas-pagar/{id}/baixar` | POST | Baixar título |
| `/api/erp/financeiro/contas-receber` | POST | Criar conta a receber |
| `/api/erp/financeiro/contas-receber/{id}/baixar` | POST | Receber título |
| `/api/erp/financeiro/resumo` | GET | Posição financeira em tempo real |
| `/api/erp/financeiro/dre` | GET | DRE por período |
| `/api/erp/financeiro/fluxo-caixa` | GET | Movimentação detalhada |

### CRM
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/crm/leads` | POST | Criar lead |
| `/api/erp/crm/leads/{id}/status` | PUT | Atualizar status |
| `/api/erp/crm/leads/{id}/converter` | POST | Converter em cliente |
| `/api/erp/crm/pipeline` | GET | Pipeline visual |
| `/api/erp/crm/interacoes` | POST | Registrar interação |
| `/api/erp/crm/tarefas` | POST | Criar tarefa |

### Estoque
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/estoque/entrada` | POST | Registrar entrada |
| `/api/erp/estoque/saida` | POST | Registrar saída |
| `/api/erp/estoque/transferencia` | POST | Transferir entre lojas |
| `/api/erp/estoque/inventario` | POST | Criar inventário |
| `/api/erp/estoque/inventario/{id}/finalizar` | POST | Finalizar e ajustar |
| `/api/erp/estoque/kardex/{produtoId}` | GET | Histórico do produto |

### Fiscal
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/fiscal/nfe/emitir` | POST | Emitir NFe |
| `/api/erp/fiscal/nfe/{id}/cancelar` | POST | Cancelar NFe |
| `/api/erp/fiscal/impostos` | GET | Configurações de impostos |

### Relatórios
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/relatorios/dre/excel` | GET | Exportar DRE em Excel |
| `/api/erp/relatorios/fluxo-caixa/excel` | GET | Exportar Fluxo de Caixa |
| `/api/erp/relatorios/posicao-estoque` | GET | Posição de estoque |
| `/api/erp/relatorios/pipeline-crm` | GET | Pipeline CRM |

### Sincronização
| Endpoint | Método | Descrição |
|---|---|---|
| `/api/erp/sync/completo` | POST | Executar sync manual |
| `/api/erp/sync/agendado` | POST | Executar sync agendado |
| `/api/erp/sync/produtos` | POST | Sync apenas produtos |
| `/api/erp/sync/pedidos` | POST | Sync apenas pedidos |

---

## 🔄 Bridge ERP ↔ E-Commerce

O `SyncErpService` mantém sincronização automática:

| Direção | Entidade | Frequência |
|---|---|---|
| ERP → E-Commerce | Produtos (preço, estoque) | A cada 15 min |
| E-Commerce → ERP | Clientes (novos cadastros) | A cada 15 min |
| E-Commerce → ERP | Pedidos pagos | A cada 5 min |
| ERP → E-Commerce | Estoque (recálculo) | A cada 30 min |

**Para agendamento automático**, configure Hangfire ou Quartz:

```csharp
// Program.cs
builder.Services.AddHangfire(config => config.UseSqlServerStorage(...));
builder.Services.AddHangfireServer();

// Agendamento
RecurringJob.AddOrUpdate<ISyncErpService>(
    "sync-completo",
    service => service.ExecutarSyncAgendadoAsync(),
    Cron.Minutely(15));
```

---

## 📊 Dashboard ERP

O `ErpDashboardController` consolida KPIs em tempo real:

- **Financeiro**: Saldo, contas atrasadas, projeção 7 dias
- **CRM**: Leads novos, pipeline, tarefas atrasadas
- **Estoque**: Itens críticos, valor em estoque, inventários pendentes
- **Fiscal**: NFe emitidas, pendentes, canceladas

---

## 🚀 Próximo Passo: FASE 6 — Estrutura GitHub + CI/CD

Quando você estiver pronto, a Fase 6 entregará:

| Entregável | Descrição |
|---|---|
| Organização de Repositórios | NexumAltivon.API, .ERP, .CRM, .Database, .Front |
| CI/CD GitHub Actions | Build, testes e deploy automático |
| Docker Compose | Orquestração de containers |
| Documentação de Deploy | Guia passo a passo de produção |
| Scripts de Backup | MySQL + arquivos automáticos |

---

## 📞 Suporte

**Rodrigo Costa** — (14) 99673-1879  
**Vinicius** — (14) 99634-8409  
**E-mail**: corporativo.gna@gmail.com  
**Site**: www.nexumaltivon.com

---

© 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.


# FASE 6 — Estrutura GitHub + CI/CD + Docker + Documentação de Deploy
## Grupo Nexum Altivon ME | www.nexumaltivon.com

---

## 📋 Sumário

Esta fase final entrega a infraestrutura completa de DevOps: repositórios GitHub organizados, pipelines CI/CD automatizadas, containers Docker, orquestração com Docker Compose, scripts de backup/restore e documentação de deploy em produção.

---

## 🗂️ Estrutura de Repositórios GitHub

```
Organização: github.com/nexumaltivon
│
├── NexumAltivon.API          → Back-end ASP.NET Core 8 (Fases 1-5)
├── NexumAltivon.Front        → Front-end Next.js (futuro)
├── NexumAltivon.Database     → Scripts SQL e migrations
├── NexumAltivon.Docs         → Documentação técnica e de negócio
└── NexumAltivon.Infra        → Docker, CI/CD, scripts de deploy
```

---

## 🔄 Pipeline CI/CD (GitHub Actions)

### Arquivo: `.github/workflows/ci-cd.yml`

| Stage | Descrição | Gatilho |
|-------|-----------|---------|
| **Build & Test** | Restore, build e testes xUnit com MySQL em container | Push em `main` ou `develop` |
| **Docker Build** | Build de imagem e push para GitHub Container Registry | Após build/test OK |
| **Deploy Staging** | SSH no servidor staging + `docker-compose up` | Branch `develop` |
| **Deploy Production** | SSH no servidor produção + `docker-compose up` | Branch `main` |

### Secrets Necessários (GitHub)

| Secret | Descrição |
|--------|-----------|
| `STAGING_HOST` | IP do servidor staging |
| `STAGING_USER` | Usuário SSH staging |
| `STAGING_SSH_KEY` | Chave privada SSH staging |
| `PROD_HOST` | IP do servidor produção |
| `PROD_USER` | Usuário SSH produção |
| `PROD_SSH_KEY` | Chave privada SSH produção |

---

## 🐳 Docker

### Dockerfile.api
- Base: `mcr.microsoft.com/dotnet/aspnet:8.0`
- Multi-stage build (build → publish → runtime)
- Usuário não-root (`nexum`) para segurança
- Health check em `/health`
- Exposição nas portas 8080/8081

### Docker Compose — Desenvolvimento
```bash
cd docker
docker-compose up -d
```
Serviços:
- **mysql**: Banco MySQL 8.0 com seed automático
- **api**: API ASP.NET Core 8
- **redis**: Cache distribuído (opcional)
- **nginx**: Reverse proxy (opcional)

### Docker Compose — Produção
```bash
cd docker
docker-compose -f docker-compose.prod.yml up -d
```
Serviços:
- **mysql**: MySQL 8.0 com volumes persistentes
- **api**: Imagem do GitHub Container Registry
- **watchtower**: Atualização automática de imagens

---

## 🔐 Nginx Reverse Proxy

Arquivo: `docker/nginx/nginx.conf`

- Proxy reverso para API
- Suporte a WebSocket (Upgrade/Connection)
- Headers de segurança (X-Real-IP, X-Forwarded-For)
- Health check otimizado
- Pronto para SSL/TLS (configurar certificados)

---

## 💾 Backup e Restore

### Backup Automático
```bash
# Agendar no crontab (todo dia às 2h)
0 2 * * * /opt/nexumaltivon/scripts/backup-mysql.sh
```

- Backup completo com `mysqldump`
- Compactação com `gzip`
- Retenção de 30 dias
- Log de execução

### Restore
```bash
./restore-mysql.sh /backups/mysql/nexum_altivon_20260527_020000.sql.gz
```

- Confirmação interativa
- Descompactação automática
- Restore direto no MySQL

---

## 🚀 Guia de Deploy Passo a Passo

### 1. Preparação do Servidor

```bash
# Instalar Docker e Docker Compose
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# Criar estrutura de diretórios
sudo mkdir -p /opt/nexumaltivon/{production,staging,backups,logs}
sudo chown -R $USER:$USER /opt/nexumaltivon
```

### 2. Configurar Variáveis de Ambiente

Criar `/opt/nexumaltivon/production/.env`:
```env
MYSQL_ROOT_PASSWORD=sua_senha_forte
MYSQL_USER=nexum_prod
MYSQL_PASSWORD=sua_senha_forte
CONNECTION_STRING=Server=mysql;Port=3306;Database=nexum_altivon;Uid=nexum_prod;Pwd=sua_senha_forte;
JWT_SECRET=sua_chave_jwt_256bits_producao
MP_ACCESS_TOKEN=APP_USR-...
SENDGRID_API_KEY=SG.xxxxx
```

### 3. Primeiro Deploy

```bash
cd /opt/nexumaltivon/production

# Clonar repositório
git clone https://github.com/nexumaltivon/NexumAltivon.API.git .

# Copiar docker-compose.prod.yml
cp docker/docker-compose.prod.yml .

# Iniciar serviços
docker-compose -f docker-compose.prod.yml up -d

# Verificar status
docker-compose -f docker-compose.prod.yml ps
docker logs -f nexum-api-prod
```

### 4. Configurar SSL (Let's Encrypt)

```bash
# Instalar certbot
docker run -it --rm   -v /opt/nexumaltivon/ssl:/etc/letsencrypt   -v /opt/nexumaltivon/ssl-data:/data/letsencrypt   certbot/certbot certonly   --standalone -d api.nexumaltivon.com

# Atualizar nginx.conf com caminhos dos certificados
# Reiniciar nginx
```

### 5. Agendar Backups

```bash
# Adicionar ao crontab
crontab -e
# Adicionar linha:
0 2 * * * /opt/nexumaltivon/scripts/backup-mysql.sh >> /var/log/nexum-backup.log 2>&1
```

---

## 📊 Monitoramento

### Health Checks
- `/health` — Status geral da API + banco
- `/hangfire` — Dashboard de jobs (protegido)
- Nginx status page (opcional)

### Logs
```bash
# API
docker logs -f nexum-api-prod

# MySQL
docker logs -f nexum-mysql-prod

# Nginx
docker logs -f nexum-nginx
```

### Métricas Recomendadas (futuro: Prometheus + Grafana)
- Taxa de requests/segundo
- Tempo de resposta médio
- Taxa de erro 5xx
- Uso de CPU/Memória
- Conexões ativas MySQL

---

## 🔧 Comandos Úteis

```bash
# Ver logs em tempo real
docker-compose -f docker-compose.prod.yml logs -f api

# Reiniciar serviço específico
docker-compose -f docker-compose.prod.yml restart api

# Escalar API (se usar Docker Swarm/K8s futuramente)
docker-compose -f docker-compose.prod.yml up -d --scale api=3

# Acessar container MySQL
docker exec -it nexum-mysql-prod mysql -u root -p

# Acessar container API
docker exec -it nexum-api-prod /bin/sh

# Atualizar imagem manualmente
docker-compose -f docker-compose.prod.yml pull api
docker-compose -f docker-compose.prod.yml up -d api
```

---

## ✅ Checklist de Produção

- [ ] Variáveis de ambiente configuradas
- [ ] SSL/TLS ativo
- [ ] Firewall configurado (apenas 80/443/3309 expostos)
- [ ] Backups automáticos agendados
- [ ] Monitoramento ativo
- [ ] Documentação de rollback preparada
- [ ] Testes de carga realizados
- [ ] Política de senhas forte no banco
- [ ] JWT secret diferente do ambiente de dev
- [ ] Tokens de integração (MP, SendGrid) são de produção

---

## 📞 Suporte

**Rodrigo Costa** — (14) 99673-1879  
**Vinicius** — (14) 99634-8409  
**E-mail**: corporativo.gna@gmail.com  
**Site**: www.nexumaltivon.com

---

© 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.


<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 -->
# PROJETO GRUPO NEXUM ALTIVON — DOCUMENTACAO COMPLETA
## Grupo Nexum Altivon ME | www.nexumaltivon.com
### Versao: 1.0.00.2600 | Stack: ASP.NET Core 8 + MySQL + Docker + CI/CD

---

## RESUMO EXECUTIVO

Este documento consolida todas as 6 fases do projeto de e-commerce e ERP/CRM do Grupo Nexum Altivon ME, unificando as 6 lojas societarias (Grann-Tur, Chronos, Moda Mim, Geracao Top+, Estruturaline, Gran-fest-festas) em uma plataforma moderna, escalavel e profissional.

---

## FASE 1 — FUNDACAO (Banco + API + Autenticacao)

### O que foi entregue
- Script SQL completo do banco nexum_altivon com 22 tabelas, views, procedures, triggers e seed das 6 lojas
- API ASP.NET Core 8 com estrutura profissional: Program.cs, appsettings.json, DbContext
- JWT completo com refresh token (24h expiracao, 7 dias refresh), BCrypt para senhas
- 8 Controllers RESTful: Auth, Lojas, Clientes, Produtos, Carrinho, Pedidos, CRM, Financeiro
- 15 Services de logica de negocio com tratamento global de erros, rate limiting (100 req/min) e auditoria automatica
- Swagger/OpenAPI documentado
- AutoMapper para mapeamento DTO <-> Entidade

### Como aplicar
1. Executar o script SQL no MySQL (servidor 192.168.1.72:3309)
2. Configurar ConnectionStrings:NexumDb no appsettings.json
3. Rodar dotnet ef database update (ou deixar o EnsureCreated() na primeira execucao)
4. Testar via Swagger: https://localhost:5001/swagger

---

## FASE 2 — PAINEL ADMINISTRATIVO

### O que foi entregue
- DashboardController com 8 endpoints de KPIs e graficos
- AdminDashboardService com metricas de faturamento, pedidos, clientes, estoque baixo, leads
- Painel Admin HTML completo (57 KB) com design identico ao site principal (preto #0A0A0A + dourado #C9A227)
- 6 KPI Cards, graficos Chart.js (semanal, por loja, mensal, top produtos)
- Menu lateral com 16 secoes: Dashboard, Pedidos, Produtos, Clientes, Lojas, Financeiro, Fiscal, Logistica, CRM, Cupons, Marketing, Marketplaces, Dropshipping, Usuarios, Configuracoes, Auditoria

### Como aplicar
1. Usar o painel oficial em `NexumAltivon_Front-End/src/pages/Dashboard.js`
2. Publicar pelo workflow `Nexum 2026-06-28 - Deploy Operacional Oficial .com.br`
3. O painel consome a API oficial `https://api.nexumaltivon.com.br`
4. Acesso restrito a perfis: Gerente, Admin, SuperAdmin

---

## FASE 3 — CARRINHO, CHECKOUT E GATEWAY DE PAGAMENTO

### O que foi entregue
- Carrinho anonimo via cookie nx_session_id (HttpOnly, Secure, 30 dias)
- Migracao automatica do carrinho de sessao para cliente logado
- Cupons de desconto (percentual ou valor fixo) com validade e limite de uso
- Checkout completo: endereco -> calculo de frete (Melhor Envio + tabela propria) -> pagamento
- Gateway Mercado Pago: PIX (QR Code base64 + texto), Cartao de Credito (parcelado), Boleto
- Webhooks para atualizacao automatica de status de pagamento (/api/webhooks/mercadopago)
- Notificacoes: E-mail transacional (SendGrid) + WhatsApp + alerta de estoque baixo
- Reembolso total ou parcial via API

### Como aplicar
1. Configurar tokens do Mercado Pago em appsettings.json
2. Registrar URL de webhook no dashboard do Mercado Pago: https://api.nexumaltivon.com/api/webhooks/mercadopago
3. Configurar SendGrid para e-mails transacionais
4. Fluxo: Adicionar ao Carrinho -> Checkout (JWT) -> Selecionar Frete -> Finalizar (PIX/Cartao/Boleto) -> Webhook confirma -> Pedido PAGO

---

## FASE 4 — INTEGRACOES COMPLETAS

### O que foi entregue
- Mercado Livre: Publicar produtos, atualizar preco/estoque, importar pedidos, marcar como enviado
- Hub Multi-Canal: Shopee e Amazon (estrutura pronta, stubs documentados)
- Dropshipping: Roteamento inteligente de pedidos para fornecedores, calculo automatico de comissao, notificacao ao fornecedor
- Logistica: Geracao de etiquetas, rastreamento com eventos simulados, dashboard operacional, transportadoras
- ERP GenesisGest.Net: Bridge completa para sincronizacao de produtos, clientes, pedidos e estoque via API REST
- Sync Automatico: Orquestrador que mantem estoque e preco sincronizados entre todos os canais automaticamente

### Como aplicar
1. Configurar tokens de cada marketplace em appsettings.json
2. Para ML: registrar app em developers.mercadolivre.com.br, obter APP_ID e SECRET
3. ERP: configurar URL base do GenesisGest.Net (192.168.1.72:8080) e token
4. Agendar job recorrente (Hangfire/Quartz) para ExecutarSyncAgendadoAsync()

---

## FASE 5 — ERP/CRM GENESISGEST.NET

### O que foi entregue
- Financeiro: Contas Pagar/Receber, Fluxo de Caixa, DRE automatica, centros de custo, contas bancarias
- CRM: Leads, pipeline visual, conversao, interacoes, tarefas com prioridade
- Estoque Avancado: Movimentacoes (entrada/saida/transferencia/ajuste), Kardex automatico, inventario fisico, locais de estoque
- Fiscal: NFe com calculo automatico de ICMS/IPI/PIS/COFINS, cancelamento, download XML/DANFE
- Relatorios: DRE, Fluxo de Caixa, Posicao de Estoque, Ranking — exportacao PDF/Excel
- Fornecedores: Cadastro completo, avaliacoes, comissoes
- Bridge ERP <-> E-Commerce: Sincronizacao bidirecional automatica
- Dashboard ERP: KPIs consolidados em tempo real (financeiro, CRM, estoque, fiscal, alertas)
- Hangfire: Jobs automaticos de sync, alertas de contas vencidas

### Novas tabelas (18+)
- erp_centros_custo, erp_contas_bancarias, erp_contas_pagar, erp_contas_receber
- erp_fluxo_caixa, erp_notas_fiscais, erp_itens_nota_fiscal, erp_impostos_config
- erp_movimentacoes_estoque, erp_inventarios, erp_itens_inventario, erp_kardex
- erp_locais_estoque, erp_leads_crm, erp_interacoes_crm, erp_tarefas_crm
- erp_fornecedores, erp_avaliacoes_fornecedor

### Como aplicar
1. Executar Database/erp_schema_update.sql no MySQL
2. Integrar arquivos .cs ao projeto NexumAltivon.API
3. Adicionar DbSet do NexumDbContext_ERP.cs ao DbContext principal
4. Executar dotnet ef migrations add Fase5_ERP e dotnet ef database update
5. Registrar servicos em Program.cs (ja incluido no Program_ERP.cs entregue)

---

## FASE 6 — GITHUB + CI/CD + DOCKER + DEPLOY

### O que foi entregue
- Repositorios GitHub organizados: .API, .Front, .Database, .Docs, .Infra
- GitHub Actions: Build, testes xUnit, Docker build/push, deploy automatico staging/producao via SSH
- Docker: Multi-stage build, usuario nao-root, health checks
- Docker Compose: Desenvolvimento (MySQL + API + Redis + Nginx) e Producao (MySQL + API + Watchtower)
- Nginx: Reverse proxy com headers de seguranca, pronto para SSL
- Backup/Restore: Scripts automaticos com retencao de 30 dias
- Documentacao de Deploy: Guia passo a passo completo

### Como aplicar
1. Criar organizacao nexumaltivon no GitHub
2. Criar repositorios e configurar secrets (SSH keys, tokens)
3. Configurar servidor com Docker e Docker Compose
4. Executar docker-compose -f docker-compose.prod.yml up -d
5. Configurar SSL com Let's Encrypt
6. Agendar backups no crontab

---

## ESTRUTURA FINAL DOS CAMINHOS

```
D:\Users\Rodrigo Costa\source\repos\GenesisGest.Net_E-Commerce\NexumAltivon.com
|
├── NexumAltivon_Back-End/
│   ├── NexumAltivon.API.csproj
│   ├── Program.cs (ERP unificado)
│   ├── appsettings.json / appsettings.ERP.json
│   ├── API/
│   │   ├── Controllers/          -> 13+ controllers
│   │   ├── Services/             -> 25+ services
│   │   ├── Models/               -> Entidades EF Core completas
│   │   ├── DTOs/                 -> DTOs organizados por dominio
│   │   ├── Data/
│   │   │   └── NexumDbContext.cs -> DbContext unificado com seed
│   │   ├── Middleware/
│   │   ├── Mappings/
│   │   └── Configurations/
│   └── Database/
│       ├── nexum_altivon_schema.sql
│       └── erp_schema_update.sql
|
├── NexumAltivon_Front-End/
│   ├── index.html                -> Home Page institucional (preservada)
│   └── admin/
│       └── index.html            -> Painel administrativo completo
|
├── NexumAltivon_ERP/
│   └── (estrutura preparada para expansao desktop/WinForms futura)
|
├── docker/
│   ├── Dockerfile.api
│   ├── docker-compose.yml
│   ├── docker-compose.prod.yml
│   ├── nginx/
│   │   └── nginx.conf
│   └── scripts/
│       ├── backup-mysql.sh
│       └── restore-mysql.sh
|
├── .github/
│   └── workflows/
│       └── ci-cd.yml
|
└── docs/
    ├── README_Fase1.md
    ├── README_Fase2.md
    ├── README_Fase3.md
    ├── README_Fase4.md
    ├── README_Fase5.md
    ├── README_Fase6.md
    └── README_FINAL.md (este arquivo)
```

---

## PROXIMOS PASSOS SUGERIDOS

### Imediatos (Validacao)
1. Testar Fase 1: Subir banco MySQL e API, validar JWT e Swagger
2. Testar Fase 2: Acessar painel admin, verificar KPIs e graficos
3. Testar Fase 3: Simular carrinho -> checkout -> pagamento PIX (sandbox MP)
4. Testar Fase 4: Configurar token ML, publicar 1 produto de teste
5. Testar Fase 5: Criar conta a pagar, lead, movimentacao de estoque
6. Testar Fase 6: Subir ambiente Docker, validar CI/CD

### Futuros (Melhorias)
- [ ] Front-end Next.js consumindo a API
- [ ] Aplicativo mobile (React Native/Flutter)
- [ ] Testes unitarios xUnit (cobertura > 80%)
- [ ] Testes de integracao com TestContainers
- [ ] Monitoramento com Prometheus + Grafana
- [ ] Cache distribuido com Redis
- [ ] Mensageria com RabbitMQ/Apache Kafka
- [ ] Inteligencia artificial para recomendacoes de produtos
- [ ] Chatbot com IA para atendimento
- [ ] BI avancado com Power BI / Metabase

---

## SUPORTE

**Rodrigo Costa** — (14) 99673-1879
**Vinicius** — (14) 99634-8409
**E-mail**: corporativo.gna@gmail.com
**Site**: www.nexumaltivon.com

---

(c) 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.



