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
