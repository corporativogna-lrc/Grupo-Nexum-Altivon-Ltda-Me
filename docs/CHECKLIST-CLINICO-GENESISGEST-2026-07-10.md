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
| Painel administrativo React | `Dashboard.js` ajustado no commit `0b8212e` para nao exibir sucesso visual com estado local fabricado em `Site & Banners` e rascunho fiscal manual; apos salvar, a tela usa retorno/releitura real da API antes de confirmar persistencia |
| Central de IA do site | `GlobalActions.js` ajustado no commit `5848622` para nao inventar resposta local quando a API retorna payload sem mensagem; sem resposta textual real, o chat exibe falha operacional |
| Frete/logistica sem sucesso falso | `Program.cs` ajustado no commit `4918d30`: `/api/frete/cotar` e `/api/logistica/roteamento` retornam 502 se Melhor Envio estiver configurado e falhar, recusar ou nao devolver cotacao utilizavel; tabela interna oficial so e usada quando nao ha credencial externa configurada |
| HTML estatico legado do frontend | `NexumAltivon_Front-End/index.html` ajustado no commit `937512d`: removido HTML legado com Formspree, links sem destino, termos de lancamento futuro e CNPJ inexistente; arquivo agora redireciona para o portal oficial publicado |
| Script legado de CRM | `nexum-integration.js` e `public/nexum-integration.js` ajustados no commit `6dbf1f0`: formulario legado so confirma cadastro quando `/api/crm/leads` retorna `Dados.Id`; sem confirmacao de persistencia, exibe erro operacional |
| Smoke publico painel/portal | Endpoints principais consumidos pelo painel e portal foram verificados em `https://api.nexumaltivon.com.br`: rotas publicas retornaram 200, rotas protegidas retornaram 401, nenhuma retornou 404 |
| OpenAPI em runtime local | `/swagger/v1/swagger.json` respondeu 200 em API local com o build atual |
| Alias FICO razao | `/api/financeiro/contabil/razao` respondeu 401 sem token, sem 404 |
| Alias FICO conciliacao | `/api/financeiro/contabil/conciliacao` respondeu 401 sem token, sem 404 |
| Alias FICO DRE | `/api/financeiro/contabil/dre` respondeu 401 sem token, sem 404 |
| Alias FICO fechamento | `/api/financeiro/contabil/fechamento` respondeu 401 sem token, sem 404 |
| Publicacao atual | `https://api.nexumaltivon.com.br` respondeu 200 para Swagger JSON e 401 sem token para os aliases FICO, sem 404 |
| Runtime oficial | API publicada em `D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api` |
| Configuracao privada oficial | `D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api.env.ps1` criado fora do Git, sem dependencia de pasta recuperada externa |
| Usuario MySQL de runtime | `nexum_api_24h` validado nos schemas `nexum_altivon` e `genesis_bd` com privilegios operacionais minimos, incluindo `CREATE VIEW` para schema incremental |
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
| MES/OPS operacional | Parcial | Backend existe; telas e fluxo completo ainda precisam validacao |
| Hangfire e jobs agendados | Pendente | Ativar storage e validar execucao de jobs |
| Redis integrado e healthcheck verde | Parcial | Health Redis existe; validar em runtime de producao |
| S3/MinIO anexos | Parcial | Servico e compose existem; validar upload/download assinado |
| Serilog e OpenTelemetry | Pendente | Ativar logs/traces/metrics e validar exportacao |
| EF Migrations aplicaveis em banco vazio | Parcial avancado | Migrations foram versionadas; instalar `dotnet-ef` e executar `dotnet ef database update` em banco vazio controlado |
| Secrets fora de arquivos versionados | Parcial | Revisar arquivos versionados e manter segredos apenas em env/User Secrets/cofre |
| CI/CD com gate de cobertura, Sonar e migrate | Parcial | Adicionar teste/cobertura/migrate e health-check pos-deploy |
| Compose dev sobe tudo | Nao validado nesta apuracao | Executar `docker compose up --build` em janela propria |
| Desktop auto-update funcional | Parcial | Publicar release desktop real e validar atualizacao em maquina cliente |
| TLS ativo sem HTTP publico puro | Parcial | Porta 443 responde; validar politica completa Cloudflare/nginx |
| AGENTS e OpenAPI | Parcial | `AGENTS.md` atualizado; `/swagger/v1/swagger.json` respondeu 200 no smoke publico |
| Termos proibidos no codigo ativo | Parcial | Blocos commitados nesta auditoria foram varridos; ainda restam Desktop, docs antigos e services legados nao compilados para triagem |
| Header de IP em todos os arquivos | Parcial | Blocos commitados nesta auditoria receberam header; falta varredura completa nos arquivos remanescentes |
| Fluxo cadastro a BI isolado por empresa | Pendente | Executar roteiro ponta a ponta com dados reais |
| Ferramentas ficticias convertidas em reais ou bloqueadas claramente | Em execucao | Shopee/Amazon deixam de gravar sucesso falso; marketplace sync Mercado Livre/B2W/Via foi implementado com HTTP externo e bloqueio por credencial; landing legada e admin estatico legado foram removidos da exposicao operacional; PDF financeiro agora gera arquivo real; tracking logistico externo e emissao/eventos NF-e/NFC-e foram implementados com dependencia explicita e sem sucesso fabricado | Concluir demais achados da secao acima e validar com credenciais reais de cada integracao externa |

## GitHub e Commit

Estado apurado:

- Branch local: `work/delivery-2026-06-13`.
- Commit local e remoto atual: `6dbf1f0 fix: frontend - exigir lead persistido no script legado`.
- `origin/work/delivery-2026-06-13`: alinhado com `6dbf1f0`.
- `origin/main`: alinhado com `6dbf1f0`.
- Estado do worktree antes desta atualizacao documental: limpo.
- Commits atomicos enviados nesta rodada de saneamento: `8d58edb`, `0b8212e`, `a7c23d9`, `5848622`, `fd988d0`, `d766314`, `4918d30`, `3879914`, `937512d`, `404468b`, `6dbf1f0`.
- Publicacao GitHub permanece seletiva e auditada; nao publicar arquivos legados/falsos sem validacao direta no projeto oficial.

## Regra de continuidade

Nao declarar item como concluido sem uma evidencia tecnica associada: build, teste, rota HTTP, consulta de banco, migracao aplicada, arquivo publicado, commit remoto ou validacao visual quando o item for de interface.
