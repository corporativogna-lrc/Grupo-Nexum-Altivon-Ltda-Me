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
|   |   |-- Data/                 → DbContext + Migrations
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
```

---

## Banco de Dados — MySQL

**Servidor:** 192.168.1.72:3309
**Banco:** nexum_altivon
**Usuario:** root

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

## Como Executar

### 1. Banco de Dados
```bash
mysql -h 192.168.1.72 -P 3309 -u root -p < Database/nexum_altivon_schema.sql
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

(c) 2026 Grupo Nexum Altivon ME — Todos os direitos reservados
