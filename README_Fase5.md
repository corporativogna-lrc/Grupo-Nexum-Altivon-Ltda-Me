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
**E-mail**: corporativo.gna@gmail.com  
**Site**: www.nexumaltivon.com

---

© 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.
