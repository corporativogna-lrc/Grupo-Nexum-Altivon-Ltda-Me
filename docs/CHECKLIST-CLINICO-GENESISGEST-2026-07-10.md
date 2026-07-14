<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Checklist Clinico GenesisGest.Net v1.1.5

Data da apuracao: 2026-07-10.
Apuracao complementar de integridade funcional: 2026-07-12.
Apuracao de paridade do painel legado: 2026-07-14.

## Rotas Travadas

| Item | Rota oficial | Status |
|---|---|---|
| Projeto oficial | `D:\Nexum Altivon\NexumAltivon.com` | Travado |
| Banco e-commerce | `D:\xampp\mysql\data\nexum_altivon` | Existe localmente |
| Banco GenesisGest | `D:\xampp\mysql\data\genesis_bd` | Existe localmente |
| GitHub | `https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me` | Remoto configurado |
| Materiais auxiliares | `D:\` | Consulta tecnica, sem dependencia runtime |

## Bloqueadores B1 a B14

| ID | Requisito | Status real | Evidencia objetiva |
|---|---|---|---|
| B1 | Solution unica compila API, ERP e Desktop | Concluido local | `NexumAltivon.ERP.sln` lista API, ERP, Desktop e projeto raiz; build Release com 0 erros e 0 avisos |
| B2 | Controllers MVC fora do ativo e endpoints criticos sem 404 | Ajustado e publicado na branch de entrega | API oficial e Minimal API; controllers MVC removidos do projeto da API ativa no commit `4bacfe5`; smoke publico passou nos pontos obrigatorios |
| B3 | Duplicacao massiva da raiz removida do build ativo | Concluido para a raiz legacy | Diretorios legados da raiz foram removidos do build/versionamento no commit `7ccc668`; solution Release seguiu compilando com 0 erros e 0 avisos |
| B4 | `Sys_AuditableEntity` em 100% das entidades transacionais | Parcial avancado | Commit `07a465e` aplicou tenant, soft-delete, auditoria central e `row_version` BLOB no `NexumDbContext`; heranca direta ainda nao cobre todas as classes |
| B5 | MFA, refresh-token, tenants e workflows | Parcial | Rotas existem e compilam; tenant smoke validado; fluxo completo ainda exige teste integrado |
| B6 | Testes com cobertura minima de 70% em Services | Pendente | Por orientacao operacional, a base de testes criada para validacao foi removida da solution e do projeto oficial; cobertura >=70% ainda precisa ser implementada sem confundir codigo operacional com artefato de validacao |
| B7 | Observabilidade completa | Parcial | Health e Redis existem; Serilog/OpenTelemetry completos ainda nao foram homologados ponta a ponta |
| B8 | Backup diario e restore-test semanal | Parcial | Backup local 2h corrigido para `D:\Nexum Altivon\NexumAltivon.com` e executado com resultado 0; restore-test em CI ainda nao comprovado |
| B9 | EF Migrations | Parcial avancado | Commit `07a465e` versionou migrations do Nexum com `row_version` BLOB alinhado ao banco real; `dotnet ef` nao esta instalado no PATH desta maquina |
| B10 | Secrets fora dos arquivos versionados | Parcial | Runtime local usa `runtime/api-24h/api.env.ps1`, ignorado pelo Git; ainda ha configuracoes de desenvolvimento e templates a revisar |
| B11 | Documentacao tecnica e OpenAPI | Parcial | Docs principais existem; `/swagger/v1/swagger.json` foi ajustado para ser registrado fora de Development/Staging |
| B12 | Compose dev com MySQL e Redis | Concluido em arquivo | `docker/docker-compose.yml` contem MySQL 8, Redis 7 e MinIO |
| B13 | Staging | Parcial | `docker-compose.staging.yml` e job de staging existem; health-check pos-deploy ainda precisa ajuste |
| B14 | Desktop auto-update | Parcial | Servico consulta GitHub Releases, baixa pacote e pode iniciar instalador por variavel de ambiente; delta nao comprovado |

## Intervencao Validada Nesta Etapa

| Item | Resultado |
|---|---|
| Seeds auditaveis do `NexumDbContext` | `Loja`, `Transportadora`, `Marketplace` e `DropshippingConfig` passaram a declarar valores obrigatorios de auditoria e flags operacionais em `HasData` |
| Validacao operacional da etapa | `dotnet build NexumAltivon.ERP.sln -c Release --no-restore -m:1 /nr:false /p:UseSharedCompilation=false` passou com 0 avisos e 0 erros apos remover a base de validacao da solution |
| Republicacao oficial 5010 | Validacao elevada em 2026-07-13T00:53:39 confirmou task `NexumAltivonApi24h` `Running`, usuario `SISTEMA`, `RunLevel Highest`, PID `10928`, `dotnet.exe NexumAltivon.API.dll`, `/health`, `/health/db`, `/health/db/genesis` e `/api/site/configuracoes/publico` com HTTP 200 |
| Republicacao oficial 5010 complementar | Validacao elevada em 2026-07-13T23:45:31 confirmou task `NexumAltivonApi24h` `Running`, usuario `SISTEMA`, `RunLevel Highest`, PID `12876`, `dotnet.exe NexumAltivon.API.dll`, `/health`, `/health/db`, `/health/db/genesis` e `/api/site/configuracoes/publico` com HTTP 200 |
| Painel administrativo React | `Dashboard.js` ajustado no commit `0b8212e` para nao exibir sucesso visual com estado local fabricado em `Site & Banners` e rascunho fiscal manual; apos salvar, a tela usa retorno/releitura real da API antes de confirmar persistencia |
| Central de IA do site | `GlobalActions.js` ajustado no commit `5848622` para nao inventar resposta local quando a API retorna payload sem mensagem; sem resposta textual real, o chat exibe falha operacional |
| Frete/logistica sem sucesso falso | `Program.cs` ajustado no commit `4918d30`: `/api/frete/cotar` e `/api/logistica/roteamento` retornam 502 se Melhor Envio estiver configurado e falhar, recusar ou nao devolver cotacao utilizavel; tabela interna oficial so e usada quando nao ha credencial externa configurada |
| HTML estatico legado do frontend | `NexumAltivon_Front-End/index.html` ajustado no commit `937512d`: removido HTML legado com Formspree, links sem destino, termos de lancamento futuro e CNPJ inexistente; arquivo agora redireciona para o portal oficial publicado |
| Script legado de CRM | `nexum-integration.js` e `public/nexum-integration.js` ajustados no commit `6dbf1f0`: formulario legado so confirma cadastro quando `/api/crm/leads` retorna `Dados.Id`; sem confirmacao de persistencia, exibe erro operacional |
| Smoke publico painel/portal | Endpoints principais consumidos pelo painel e portal foram verificados em `https://api.nexumaltivon.com.br`: rotas publicas retornaram 200, rotas protegidas retornaram 401, nenhuma retornou 404 |
| Smoke publico 2026-07-14 | Local e publico responderam sem 404 para `/api/lojas/1`, `/api/pedidos/1`, `/api/relatorios/vendas`, `/api/financeiro/faturamento`, `/api/dashboard/resumo`, `/api/clientes`, `/api/fornecedores`, `/api/fiscal/pedidos` e `/swagger/v1/swagger.json`; protegidas retornaram 401 e publicas 200 |
| Frete em runtime 5010 | `POST /api/frete/cotar` local e publico retornou 200 com mensagem `Cotacao operacional calculada pela tabela interna oficial.`, fonte `Tabela interna oficial` e sem texto antigo de fallback silencioso |
| IA em runtime 5010 | `POST /api/assistentes/mensagem` com mensagem vazia retornou 400 local e publico com `Mensagem obrigatoria para acionar a central de IA`, sem resposta local fabricada |
| Portal publicado | Bundle publico `/static/js/main.5963bdf6.js` verificado em 2026-07-14 sem `Em Breve`, `formspree`, `Entraremos em contato` e `Recebi sua mensagem`; `/nexum-integration.js` publicado contem validacao de ID real do CRM |
| OpenAPI em runtime local | `/swagger/v1/swagger.json` respondeu 200 em API local com o build atual |
| Alias FICO razao | `/api/financeiro/contabil/razao` respondeu 401 sem token, sem 404 |
| Alias FICO conciliacao | `/api/financeiro/contabil/conciliacao` respondeu 401 sem token, sem 404 |
| Alias FICO DRE | `/api/financeiro/contabil/dre` respondeu 401 sem token, sem 404 |
| Alias FICO fechamento | `/api/financeiro/contabil/fechamento` respondeu 401 sem token, sem 404 |
| Publicacao atual | `https://api.nexumaltivon.com.br` respondeu 200 para Swagger JSON e 401 sem token para os aliases FICO, sem 404 |
| Runtime oficial | API publicada em `D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api` |
| Configuracao privada oficial | `D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api.env.ps1` criado fora do Git, sem dependencia de pasta recuperada externa |
| Usuario MySQL de runtime | `nexum_api_24h` validado nos schemas `nexum_altivon` e `genesis_bd` com privilegios operacionais minimos, incluindo `CREATE VIEW` para schema incremental |
| CRM Marketing operacional | Segmentos e campanhas foram implementados com tela React, endpoints Minimal API, tenant, auditoria, soft-delete, maquina de estados e concorrencia por `RowVersion`; ensaio no runtime oficial retornou POST 201/201, PUT 200, conflito 409, GET 200 e DELETE 204/204; MySQL confirmou `is_deleted=1` e limpeza controlada `0,0` |
| Desktop WPF sem sucesso fabricado | `ModuleWorkspaceWindow` deixou de confirmar operacao apenas alterando texto local: agora exige resposta persistida da API oficial e referencia do servidor; falha usa outbox real somente quando a contingencia esta ativa. A rota desktop foi validada com HTTP 201 e linha confirmada no MySQL, seguida de limpeza controlada |
| Marketing no Desktop WPF | `MarketingWindow` adicionada ao menu `Comercial e CRM`; autentica em `/api/auth/login`, mantem JWT somente em memoria e consome os mesmos CRUDs de campanhas/segmentos. Credencial controlada invalida recebeu HTTP 401 e a solution completa compilou com 0 erros e 0 avisos |
| Referencias societarias encerradas | Removidas das fontes, seeds, contato, rodape, documentacao e bundle estatico; varredura integral no projeto oficial retornou zero ocorrencias do nome solicitado |
| Publicacao Marketing | Commit `6933f98` enviado para `main`; CI/CD `29306776375` e Pages `29306776372` concluiram com sucesso. Portal respondeu 200 com bundle `main.260894af.js`, contendo `Marketing operacional` e sem a referencia societaria removida |
| Rotas de banco ativas | Runtime oficial usa `127.0.0.1:3309/nexum_altivon` e `127.0.0.1:3309/genesis_bd`; `health/db` e `health/db/genesis` retornaram 200 local e publico |
| Configuracoes versionadas da API | `API/appsettings.json` e template privado nao carregam mais `192.168.1.72`, porta `3306` ou segredos aparentes; sem env privado, a API falha de forma clara |
| EF Core design-time | Factory de migrations usa XAMPP local `127.0.0.1:3309` quando nao houver connection string real por ambiente |
| Inicializacao fixa | Task `NexumAltivonApi24h` instalada e em execucao como `SISTEMA`, `RunLevel Highest`, conforme `runtime-logs/api-24h-task-query.log` gerado pela execucao elevada |
| Porta publica interna | API escutando `127.0.0.1:5010`; Cloudflared permanece apontado para esta porta |
| Causa real do 502 | API nao subia porque a configuracao privada apontava para caminho externo ausente e, depois, faltava `CREATE VIEW` ao usuario de aplicacao |
| Processo atual | `dotnet.exe NexumAltivon.API.dll` escutando `127.0.0.1:5010`, iniciado por `scripts/server/iniciar-api-oficial-24h.ps1` via task `NexumAltivonApi24h` |
| Validacao da tarefa 24h | Concluida via log elevado: `TaskState=Running`, `TaskUser=SISTEMA`, `RunLevel=Highest`, `HealthStatus=200`; consultas nao elevadas podem retornar acesso negado/nao encontrado |
| Revisao local de legados | Nesta apuracao a pasta `Revisao_Exclusao_2026-07-10` nao existe no diretorio oficial; nao foi usada como evidencia |
| GitHub publicado | Branch `origin/work/delivery-2026-06-13` atualizada ate `07a465e feat: api - alinhar auditoria tenant e fluxos reais` |
| Backup local 2h | Task `NexumAltivon Backup Local 2h` corrigida de `Y:\...` para `D:\Nexum Altivon\NexumAltivon.com\scripts\backup-nexum-local-2h.ps1`; execucao manual retornou `Último resultado: 0` |
| ERP isolado | Commit `5611329` ajustou navegacoes nullable e headers no `NexumAltivon_ERP`; `dotnet build NexumAltivon.ERP.sln -c Release` passou com 0 erros e 0 avisos |
| API ativa | Commit `07a465e` alinhou auditoria tenant, migrations, confirmacao de cadastro, Melhor Envio real e filtros de produto publicavel; `dotnet build NexumAltivon_Back-End\NexumAltivon.API.csproj -c Release` passou com 0 erros e 0 avisos |
| Banco local oficial | MySQL XAMPP validado em `127.0.0.1:3309`; schemas `nexum_altivon` e `genesis_bd` existem, com 212 e 47 tabelas respectivamente |
| Catalogo publico | Consulta direta no banco confirmou 91 produtos ativos e 91 produtos publicaveis pelo filtro atual da API |
| API oficial 5010 | Task `NexumAltivonApi24h` validada em 2026-07-13T00:53:39 como `Running`, usuario `SISTEMA`, `RunLevel Highest`, processo `dotnet.exe NexumAltivon.API.dll`, PID `10928`, `/health`, `/health/db`, `/health/db/genesis` e `/api/site/configuracoes/publico` com HTTP 200 |
| GitHub oficial atualizado | `origin/main` e `origin/work/delivery-2026-06-13` alinhados no commit `8d58edb fix: api - limpar base de teste e corrigir seeds auditaveis` |

## Mapa Clinico das Ferramentas Prometidas pelo Painel Legado

O painel HTML removido foi recuperado somente para leitura pelo historico Git (`5d05ab0^:NexumAltivon_Front-End/admin/index.html`). Ele anunciava 16 ferramentas, mas continha secoes HTML somente para Dashboard, Pedidos, Produtos, Clientes, CRM, Lojas, Financeiro e Configuracoes. Fiscal, Logistica, Cupons, Marketing, Marketplaces, Dropshipping, Usuarios e Auditoria apareciam no menu sem secao funcional correspondente.

| Ferramenta prometida | Tela React oficial | API/persistencia | Status clinico em 2026-07-14 | Evidencia ou bloqueio atual |
|---|---|---|---|---|
| Dashboard | Existe em `Dashboard.js` | `/api/dashboard/resumo` e `/api/admin/dashboard/completo` | Parcial validado | KPIs reais existem; ainda ha indicadores textuais de arquitetura a revisar para nao aparentarem integracao homologada |
| Pedidos | Existe | `/api/pedidos*`, MySQL `pedidos` | Parcial validado | Lista e alteracao de status existem; fluxo venda-pagamento-fiscal-logistica-financeiro ainda nao foi homologado integralmente |
| Produtos | Existe | `/api/produtos*`, MySQL `produtos` | Parcial validado | CRUD e catalogo existem; falta homologar todas as restricoes de estoque/tenant e marketplace |
| Clientes | Existe | `/api/clientes*`, MySQL `clientes` | Parcial validado | CRUD, portal e confirmacao existem; falta teste completo de isolamento e historico operacional |
| Lojas | Sem tela dedicada equivalente | `/api/lojas*`, MySQL `lojas` | Parcial | Dados aparecem em dashboard/empresas; falta gestao dedicada das seis lojas com gravacao e permissoes |
| Financeiro | Existe | `/api/financeiro*` e `/api/erp/genesis/financeiro*` | Parcial avancado | Lancamentos, razao, conciliacao, DRE e fechamento existem; PDFs foram corrigidos, mas fluxo e tela completa ainda requerem homologacao |
| Fiscal | Existe | `/api/fiscal*` | Parcial real bloqueado externamente | Endpoints bloqueiam sem certificado/provedor; falta certificado e homologacao SEFAZ real |
| Logistica | Existe | `/api/frete*`, `/api/logistica*`, estoque/transportes | Parcial real | Cotacao interna identificada funciona; tracking externo exige endpoint/token real e retorna bloqueio rastreavel sem eles |
| CRM | Existe | `/api/crm/leads*`, MySQL `crm_leads` | Parcial | Leads possuem persistencia confirmada; pipelines, oportunidades, tickets, atividades, campanhas e segmentos ainda nao estao completos na API/tela |
| Cupons | Implementada nesta rodada em `CupomAdminPanel.js` | `/api/admin/cupons*` e `/api/cupons/{codigo}`, MySQL `cupons` | Concluido e validado no escopo CRUD/uso publico | Em 2026-07-14, no runtime oficial `127.0.0.1:5010`: POST retornou 201 e ID persistido, subtotal abaixo do minimo retornou 400, consulta publica com vigencia `DATE` retornou 200, PUT retornou 200, DELETE desativou com 200 e nova consulta publica retornou 404. A limpeza controlada confirmou zero registros residuais no MySQL |
| Marketing | Implementada em `MarketingAdminPanel.js` | `/api/crm/campanhas*`, `/api/crm/segmentos*`, MySQL `crm_campanhas` e `crm_segmentos` | Concluido e validado no escopo CRUD/segmentacao e metricas | Tela oficial possui campanhas, segmentos, investimento, resultados e transicoes de estado. Ensaio autenticado em 2026-07-14 confirmou criacao 201, atualizacao 200, concorrencia 409, consulta 200, exclusao 204, soft-delete no MySQL e limpeza final sem linhas controladas |
| Marketplaces | Diagnostico e formulario de sincronizacao em Integracoes | `/api/marketplaces/{canal}/sync` e services | Parcial bloqueado por credenciais | Tela React envia canal, direcao, entidade e limite para a API real. Mercado Livre/B2W/Via responderam 424 local e publicamente sem tokens, sem registrar sucesso; Shopee/Amazon nao gravam mais sucesso fabricado, mas conectores oficiais continuam pendentes |
| Dropshipping | CRUD operacional incorporado ao React e Desktop WPF | `/api/dropshipping/canais*`, MySQL `dropshipping_config`, `DropshippingAdminPanel.js`, `DropshippingWindow.xaml` e roteamento existente | Parcial avancado | Em 2026-07-14, o schema enum e os tipos Shopify/CJ foram corrigidos; CRUD autenticado confirmou bloqueio 424 sem conector, criacao 201, consulta persistida, atualizacao 200, concorrencia 409, exclusao 204, soft-delete no MySQL e limpeza sem residuos. React e WPF listam e editam canais sem expor segredos; o WPF compilou sem erros ou avisos e os dois menus de dropshipping abrem a janela especifica. Falta implementar e homologar o ciclo externo de catalogo, cotacao, pedido, retorno e rastreamento para cada provedor contratado |
| Usuarios | Implementada nesta rodada em `AccessAuditPanel.js` | `/api/admin/usuarios*`, MySQL `usuarios` | Parcial validado | API local e publica retornaram HTTP 200, 1 usuario e 7 perfis; tela compilou. Criacao/edicao/desativacao ainda precisa prova controlada com limpeza |
| Configuracoes | Existe | `/api/site/configuracoes*`, MySQL | Parcial avancado | Em 2026-07-14, a inicializacao deixou de sobrescrever valores administrativos; alteracao controlada persistiu no MySQL apos reinicio, o valor original foi restaurado, novo reinicio manteve a restauracao e `/health` retornou 200. O payload publico foi validado sem referencias societarias encerradas; falta homologar individualmente todos os campos editaveis do portal |
| Auditoria | Implementada nesta rodada em `AccessAuditPanel.js` | `/api/auditoria*`, MySQL `logs_auditoria` | Parcial validado | API local e publica retornaram HTTP 200 e 42 eventos; tela de filtros/detalhe compilou. Cobertura de toda escrita ainda nao foi comprovada |

Regra de aceite para cada ferramenta: somente marcar `Concluido` quando houver tela oficial, endpoint Minimal API, persistencia/consulta real quando aplicavel, autorizacao, tratamento de erro, build e teste HTTP autenticado. Integracao externa tambem exige chamada aceita pelo provedor oficial e persistencia do identificador retornado.

## Ferramentas que nao podem mais ser tratadas como prontas sem evidencia

| Ferramenta/modulo | Evidencia encontrada | Classificacao real | Acao aplicada em 2026-07-12 | Proxima acao obrigatoria |
|---|---|---|---|---|
| Shopee marketplace | `NexumAltivon_Back-End/API/Services/MarketplaceHubService.cs` gravava `IdExterno = SHP{produtoId}`, `Status = active` e log `[STUB]` sem chamada externa real | Falso positivo operacional | Removida gravacao de sucesso fabricado; publicacao/atualizacao Shopee agora falha de forma rastreavel e nao registra sync como ativo | Implementar conector oficial Shopee com assinatura HMAC, credenciais reais, sandbox/producao, teste HTTP e persistencia apenas apos retorno aceito pela API externa |
| Amazon marketplace | `MarketplaceHubService.cs` gravava `IdExterno = AMZ{produtoId}`, `Status = active` e log `[STUB]` sem Amazon SP-API | Falso positivo operacional | Removida gravacao de sucesso fabricado; publicacao/atualizacao Amazon agora falha de forma rastreavel e nao registra sync como ativo | Implementar Amazon SP-API real com OAuth/LWA, seller, marketplace, assinatura AWS4, sandbox/producao e teste HTTP antes de persistir sync |
| Sync marketplace Mercado Livre/B2W/Via | Checklist exigia `/api/marketplaces/{mercadolivre,b2w,via}/sync`, mas a API ativa nao tinha rota Minimal API oficial para esse contrato | Pendente real; agora endpoint existe com chamada externa obrigatoria | Ajustado em 2026-07-12: `POST /api/marketplaces/{canal}/sync` suporta `mercadolivre`, `b2w` e `via`; usa HTTP externo real, exige token/seller/endpoint/path conforme canal, bloqueia combinacoes que nao executam operacao real, retorna 424 sem credencial, 502 em recusa externa e so atualiza `UltimaSincronizacao` depois de sucesso do provedor | Configurar `MercadoLivre__AccessToken`/`SellerId`, `B2W__EndpointBase`/`AccessToken`/`SellerId`/paths e `Via__EndpointBase`/`AccessToken`/`SellerId`/paths; validar com pedido/produto real de cada seller |
| Busca de produtos do `nexum-integration.js` | Script buscava `/api/produtos?limit=100` e exigia array direto, mas a API real usa `itensPorPagina` e envelope `ApiResponse.dados` | Quebrado/incompatível com contrato real | `nexum-integration.js` raiz e `NexumAltivon_Front-End/public/nexum-integration.js` ajustados para `itensPorPagina=60` e leitura de `dados/data` | Validar visualmente `/landing.html` e decidir se este legado continua publicado ou se sera substituido integralmente pelo React oficial |
| Landing HTML legada | `landing.html` e `public/landing.html` possuiam links `#`, textos `Em Breve`, `CNPJ em breve` e script legado | Legado com risco de promessa falsa | Ajustado em 2026-07-12: landing raiz e `public/landing.html` foram substituidas por redirecionamento real para `/`, sem links `#`, sem mensagens `Em Breve`, sem `CNPJ em breve` e sem `nexum-integration.js` | Manter fora da navegacao oficial; se precisar campanha futura, criar tela React/API real antes de publicar |
| HTML estatico raiz do frontend | `NexumAltivon_Front-End/index.html` continha landing completa fora do React oficial, formulários Formspree sem credencial real, links `#`, lojas em estado futuro e CNPJ inexistente | Legado com risco de falsa operacao | Ajustado em 2026-07-13: conteudo legado removido e substituido por redirecionamento para `https://nexumaltivon.com.br/`; `npm run build` retornou codigo 0 | Manter o portal oficial no React publicado; qualquer campanha futura deve nascer integrada a API oficial e banco real |
| Script legado `nexum-integration.js` | Formularios legados exibiam sucesso com texto de contato futuro apos qualquer 2xx do CRM, sem exigir confirmacao do ID gravado | Legado com risco de sucesso visual sem persistencia comprovada | Ajustado em 2026-07-13: script raiz e `public/nexum-integration.js` exigem `Dados.Id`/`dados.id` da API real; `build/nexum-integration.js` foi regenerado e validado apos `npm.cmd run build` | Remover definitivamente este script quando nao houver mais HTML legado consumidor; enquanto existir, manter contrato estrito com `/api/crm/leads` |
| Admin HTML estatico legado | `NexumAltivon_Front-End/admin/index.html` continha painel administrativo estatico separado do React oficial | Legado nao homologado | Ajustado em 2026-07-12: `admin/index.html`, `admin-painel.html` raiz e `public/admin-painel.html` redirecionam para `/?/login` e possuem header de propriedade intelectual | Migrar componentes uteis somente para `Dashboard.js` com chamadas API reais e persistencia validada |
| PDF de conta a pagar no ERP isolado | `NexumAltivon_ERP/Services/Financeiro/ContaPagarService.cs` retornava `Array.Empty<byte>()` | Funcionalidade incompleta no ERP isolado | Ajustado em 2026-07-12: `GerarRelatorioPDFAsync` consulta `GenesisDbContext`, aplica filtros reais, calcula totais e gera PDF `%PDF-1.4` em memoria sem dependencia paga | Validado por `dotnet build NexumAltivon_ERP.csproj -c Release --no-restore` e `dotnet build NexumAltivon.ERP.sln -c Release --no-restore`; pendente apenas teste funcional chamando a tela/endpoint que consome este service |
| PDF de DRE no ERP isolado | `NexumAltivon_ERP/Services/Financeiro/DREService.cs` retornava `Array.Empty<byte>()` e o DRE automatico usava percentual fixo de imposto | Funcionalidade incompleta no ERP isolado | Ajustado em 2026-07-12: `GerarRelatorioPDFAsync` gera PDF real com valores do DRE gravado; `GerarAutomaticoAsync` calcula impostos/despesas por classificacao de contas pagas e fluxo financeiro real | Validado por `dotnet build NexumAltivon_ERP.csproj -c Release --no-restore` e `dotnet build NexumAltivon.ERP.sln -c Release --no-restore`; pendente validar em tela/endpoint consumidor |
| Frete Melhor Envio | `CotarFreteAsync` capturava qualquer excecao do provedor e seguia com cotacao local, mascarando queda, erro HTTP ou retorno vazio do Melhor Envio configurado | Falso positivo condicional | Ajustado em 2026-07-13: `POST /api/frete/cotar` e `POST /api/logistica/roteamento` agora retornam 502 quando o provedor configurado falha, recusa ou nao retorna cotacao utilizavel; a tabela interna foi renomeada para `Tabela interna oficial` e nao simula resposta externa | Configurar `MelhorEnvio__AccessToken` e validar cotacao real por CEP/peso; sem token externo, o sistema continua com tabela interna oficial explicitamente identificada |
| Logistica tracking externo | `LogisticaService.cs` declarava eventos locais sem chamada de transportadora externa | Parcial interno, nao tracking externo real | Ajustado em 2026-07-12: API ativa ganhou `GET /api/logistica/rastreamento/{codigo}`; consulta pedido real no banco, chama provedor externo quando `Logistica__RastreamentoEndpointTemplate`/`MelhorEnvio__RastreamentoEndpointTemplate` e token real existem, e retorna 424/502 sem sucesso falso quando faltar configuracao ou houver recusa externa | Configurar endpoint/token reais do provedor logistico e validar HTTP autenticado com codigo de rastreio real |
| NF-e/NFC-e SEFAZ | API ativa nao tinha `POST /api/fiscal/nfe/emitir` nem `POST /api/fiscal/nfce/emitir`; status manual podia marcar emissao sem chave/protocolo | Pendente real de homologacao; agora bloqueado sem sucesso falso | Ajustado em 2026-07-12: API ativa ganhou emissao NF-e/NFC-e, cancelamento, inutilizacao e carta de correcao via provedor externo configurado; valida PFX A1, emitente, destinatario, endereco, itens, CFOP, serie e numeracao; sem `FiscalSefaz__EndpointBase`, rota, token e certificado real retorna 424 e nao registra autorizacao | Configurar certificado real A1/A3 e provedor fiscal homologado (`FiscalSefaz__...`, `NFeIo__...` ou `DFe__...`), executar emissao em homologacao e conferir chave/protocolo gravados no banco |

## Definition of Done

| Item | Status real | Proxima acao tecnica |
|---|---|---|
| `dotnet build NexumAltivon.ERP.sln -c Release` sem erros nem avisos | Concluido local | Manter como gate antes de commit |
| `dotnet test` com cobertura >= 70% nos Services | Pendente | Criar estrategia definitiva de testes sem artefatos que confundam operacao real; adicionar cobertura dos Services e gate no CI |
| `npm run build` no frontend | Concluido local | Manter validacao antes de publicar |
| Todos os endpoints faltantes da Secao 7 com payload real | Parcial | Marketplace sync Mercado Livre/B2W/Via foi implementado na API ativa com bloqueio sem credencial real; completar demais setores e validar via HTTP |
| Frontend sem 404 nos endpoints consumidos | Parcial | Smoke publico dos pontos criticos passou em 2026-07-10: `/health`, `/api/lojas`, `/api/lojas/1`, `/swagger/v1/swagger.json`, aliases FICO e CORS sem 404; falta varredura completa do frontend |
| Multitenancy e soft-delete em 100% das entidades | Parcial | Migrar entidades legadas e validar isolamento por tenant |
| MFA TOTP funcional | Parcial | Executar teste integrado enable, verify e login com MFA |
| NF-e/NFC-e SEFAZ homologacao | Parcial real, bloqueado por credencial/certificado | Endpoints de emissao/eventos existem na API ativa e nao registram sucesso sem provedor real; falta configurar certificado/provedor homologado e validar chave/protocolo real |
| WMS completo | Parcial | Completar endpoints/telas e validar movimentacao, inventario, kardex e transferencia |
| MES/OPS operacional | Parcial | Backend existe; telas e fluxo completo ainda precisam validacao. No WPF, a janela generica agora registra solicitacao real na API ou contingencia explicitamente pendente; isto nao substitui as telas especificas de OS, producao, manutencao e ativos |
| Hangfire e jobs agendados | Pendente | Ativar storage e validar execucao de jobs |
| Redis integrado e healthcheck verde | Parcial | Health Redis existe; validar em runtime de producao |
| S3/MinIO anexos | Parcial | Servico e compose existem; validar upload/download assinado |
| Serilog e OpenTelemetry | Pendente | Ativar logs/traces/metrics e validar exportacao |
| EF Migrations aplicaveis em banco vazio | Parcial avancado | Migrations foram versionadas; instalar `dotnet-ef` e executar `dotnet ef database update` em banco vazio controlado |
| Secrets fora de arquivos versionados | Parcial | Revisar arquivos versionados e manter segredos apenas em env/User Secrets/cofre |
| CI/CD com gate de cobertura, Sonar e migrate | Parcial | Adicionar teste/cobertura/migrate e health-check pos-deploy |
| Compose dev sobe tudo | Nao validado nesta apuracao | Executar `docker compose up --build` em janela propria |
| Desktop auto-update funcional | Parcial | Publicar release desktop real e validar atualizacao em maquina cliente |
| Acesso local ao Desktop WPF | Concluido no servidor de desenvolvimento | Em 2026-07-14 foi criado e validado `GenesisGest.Net Desktop.lnk` na Area de Trabalho do usuario, apontando para o executavel Release oficial dentro de `D:\Nexum Altivon\NexumAltivon.com` |
| Reestruturacao visual do Desktop WPF | Pendente programado | Manter identidade e cores, aplicar fundo fume translucido com efeito de cristal/acrylic que preserve a visibilidade controlada da Area de Trabalho, bordas arredondadas e janela sempre contida na area util do monitor. Remover barras de rolagem estruturais horizontais/verticais e reduzir a poluicao do dashboard: manter navegacao lateral e menus suspensos, com um unico grafico central selecionavel e conteudo operacional organizado sem sobreposicao |
| Portal dinamico administravel | Parcial avancado | Em 2026-07-14 foi implementada a biblioteca de midia em `SiteMediaLibrary.js`, endpoints protegidos `/api/site/midias`, tabela MySQL `site_midias`, validacao binaria PNG/JPEG/WebP, dimensoes, limite de 8 MB, tenant, soft-delete e concorrencia por `RowVersion`. O `wwwroot` oficial passou a ser servido pelo processo da API na unica porta 5010. Ensaio autenticado real confirmou login 200, upload 201, listagem do ID, arquivo local/publico 200 com 11.080 bytes, atualizacao 200, concorrencia 409, exclusao 204, `is_deleted=1`, logout 200 e limpeza final sem linha ou arquivo controlado residual. A exclusao remove tambem o arquivo fisico; a politica `no-store` recebeu `CF-Cache-Status: BYPASS` e a mesma URL retornou 404 local/publicamente apos o DELETE. React compilou com codigo 0; ainda faltam evolucao visual ampla, administracao especifica de imagens de loja/produto e validacao Chrome dos fluxos responsivos publicados |
| Evolucao visual do portal sem alterar a identidade | Pendente | Revisar Home, catalogo, lojas e areas de produto com hierarquia visual mais forte, imagens reais administraveis, estados de carregamento/erro consistentes e validacao Chrome em desktop e mobile; manter cores, marca, navegacao e linguagem atuais |
| Sugestoes tecnicas Yara 5.5 | Em avaliacao continua | Em 2026-07-14 foram incorporados na janela WPF de Dropshipping os principios uteis de navegacao existente, Grid responsivo, validacao local, API real, retorno de erro e confirmacao de ID/RowVersion. O hub de CRUD direto no MySQL nao foi adotado porque contornaria JWT, RBAC, tenant, auditoria e regras de negocio; configuracoes sensiveis tambem nao usam outbox offline por dependerem do estado atual do servidor |
| TLS ativo sem HTTP publico puro | Parcial | Porta 443 responde; validar politica completa Cloudflare/nginx |
| AGENTS e OpenAPI | Parcial | `AGENTS.md` atualizado; `/swagger/v1/swagger.json` respondeu 200 no smoke publico |
| Termos proibidos no codigo ativo | Parcial | Blocos commitados nesta auditoria foram varridos; ainda restam Desktop, docs antigos e services legados nao compilados para triagem |
| Header de IP em todos os arquivos | Parcial | Blocos commitados nesta auditoria receberam header; falta varredura completa nos arquivos remanescentes |
| Fluxo cadastro a BI isolado por empresa | Pendente | Executar roteiro ponta a ponta com dados reais |
| Ferramentas ficticias convertidas em reais ou bloqueadas claramente | Em execucao | Shopee/Amazon deixam de gravar sucesso falso; marketplace sync Mercado Livre/B2W/Via foi implementado com HTTP externo e bloqueio por credencial; landing legada e admin estatico legado foram removidos da exposicao operacional; PDF financeiro agora gera arquivo real; tracking logistico externo e emissao/eventos NF-e/NFC-e foram implementados com dependencia explicita e sem sucesso fabricado | Concluir demais achados da secao acima e validar com credenciais reais de cada integracao externa |

## GitHub e Commit

Estado apurado:

- Branch local: `work/delivery-2026-06-13`.
- Commit local e remoto atual antes deste registro: `ecec6ee docs: checklist - registrar crm legado persistido`.
- `origin/work/delivery-2026-06-13`: alinhado com `ecec6ee`.
- `origin/main`: alinhado com `ecec6ee`.
- Estado do worktree antes desta atualizacao documental: limpo.
- Commits atomicos enviados nesta rodada de saneamento: `8d58edb`, `0b8212e`, `a7c23d9`, `5848622`, `fd988d0`, `d766314`, `4918d30`, `3879914`, `937512d`, `404468b`, `6dbf1f0`.
- Publicacao GitHub permanece seletiva e auditada; nao publicar arquivos legados/falsos sem validacao direta no projeto oficial.

## Regra de continuidade

Nao declarar item como concluido sem uma evidencia tecnica associada: build, teste, rota HTTP, consulta de banco, migracao aplicada, arquivo publicado, commit remoto ou validacao visual quando o item for de interface.
