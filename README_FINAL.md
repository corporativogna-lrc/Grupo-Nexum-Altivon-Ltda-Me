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
1. Copiar admin/index.html para NexumAltivon_Front-End/admin/
2. O painel consome a API via AJAX nos endpoints /api/admin/dashboard/*
3. Acesso restrito a perfis: Gerente, Admin, SuperAdmin

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
**E-mail**: contato@nexumaltivon.com
**Site**: www.nexumaltivon.com

---

(c) 2026 Grupo Nexum Altivon ME. Todos os direitos reservados.
