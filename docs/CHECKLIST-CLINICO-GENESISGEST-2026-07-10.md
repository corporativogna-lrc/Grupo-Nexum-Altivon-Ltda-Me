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
| B6 | Testes com cobertura minima de 70% em Services | Pendente | Nao ha projeto de teste ativo na arvore oficial |
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
| API oficial 5010 | Task `NexumAltivonApi24h` validada em 2026-07-12 como `Running`, usuario `SISTEMA`, `RunLevel Highest`, processo `dotnet.exe NexumAltivon.API.dll`, `/health`, `/health/db`, `/health/db/genesis` e `/api/site/configuracoes/publico` com HTTP 200 |
| GitHub oficial atualizado | `origin/main` e `origin/work/delivery-2026-06-13` alinhados no commit `1e458b1 fix: api - aguardar readiness da tarefa oficial 5010` antes da apuracao complementar de ferramentas ficticias |

## Ferramentas que nao podem mais ser tratadas como prontas sem evidencia

| Ferramenta/modulo | Evidencia encontrada | Classificacao real | Acao aplicada em 2026-07-12 | Proxima acao obrigatoria |
|---|---|---|---|---|
| Shopee marketplace | `NexumAltivon_Back-End/API/Services/MarketplaceHubService.cs` gravava `IdExterno = SHP{produtoId}`, `Status = active` e log `[STUB]` sem chamada externa real | Falso positivo operacional | Removida gravacao de sucesso fabricado; publicacao/atualizacao Shopee agora falha de forma rastreavel e nao registra sync como ativo | Implementar conector oficial Shopee com assinatura HMAC, credenciais reais, sandbox/producao, teste HTTP e persistencia apenas apos retorno aceito pela API externa |
| Amazon marketplace | `MarketplaceHubService.cs` gravava `IdExterno = AMZ{produtoId}`, `Status = active` e log `[STUB]` sem Amazon SP-API | Falso positivo operacional | Removida gravacao de sucesso fabricado; publicacao/atualizacao Amazon agora falha de forma rastreavel e nao registra sync como ativo | Implementar Amazon SP-API real com OAuth/LWA, seller, marketplace, assinatura AWS4, sandbox/producao e teste HTTP antes de persistir sync |
| Busca de produtos do `nexum-integration.js` | Script buscava `/api/produtos?limit=100` e exigia array direto, mas a API real usa `itensPorPagina` e envelope `ApiResponse.dados` | Quebrado/incompatível com contrato real | `nexum-integration.js` raiz e `NexumAltivon_Front-End/public/nexum-integration.js` ajustados para `itensPorPagina=60` e leitura de `dados/data` | Validar visualmente `/landing.html` e decidir se este legado continua publicado ou se sera substituido integralmente pelo React oficial |
| Landing HTML legada | `landing.html` e `public/landing.html` possuiam links `#`, textos `Em Breve`, `CNPJ em breve` e script legado | Legado com risco de promessa falsa | Ajustado em 2026-07-12: landing raiz e `public/landing.html` foram substituidas por redirecionamento real para `/`, sem links `#`, sem mensagens `Em Breve`, sem `CNPJ em breve` e sem `nexum-integration.js` | Manter fora da navegacao oficial; se precisar campanha futura, criar tela React/API real antes de publicar |
| Admin HTML estatico legado | `NexumAltivon_Front-End/admin/index.html` continha painel administrativo estatico separado do React oficial | Legado nao homologado | Ajustado em 2026-07-12: `admin/index.html`, `admin-painel.html` raiz e `public/admin-painel.html` redirecionam para `/?/login` e possuem header de propriedade intelectual | Migrar componentes uteis somente para `Dashboard.js` com chamadas API reais e persistencia validada |
| PDF de conta a pagar no ERP isolado | `NexumAltivon_ERP/Services/Financeiro/ContaPagarService.cs` possui comentario de stub para geracao de PDF | Funcionalidade incompleta no ERP isolado | Registrado como pendencia; nao tratado como pronto | Implementar PDF real com biblioteca ja disponivel ou remover botao/fluxo ate existir geracao validada |
| Logistica tracking externo | `LogisticaService.cs` declara simulacao de eventos de transportadora | Parcial interno, nao tracking externo real | Registrado como pendencia; roteamento/frete nao deve ser anunciado como rastreio externo completo | Integrar transportadora/hub real ou expor apenas status interno, sem promessa de rastreamento externo |

## Definition of Done

| Item | Status real | Proxima acao tecnica |
|---|---|---|
| `dotnet build NexumAltivon.ERP.sln -c Release` sem erros nem avisos | Concluido local | Manter como gate antes de commit |
| `dotnet test` com cobertura >= 70% nos Services | Pendente | `dotnet test` retornou 0, mas nao ha projeto de teste ativo; criar projeto real e gate de cobertura |
| `npm run build` no frontend | Concluido local | Manter validacao antes de publicar |
| Todos os endpoints faltantes da Secao 7 com payload real | Parcial | Completar por setor e validar via HTTP |
| Frontend sem 404 nos endpoints consumidos | Parcial | Smoke publico dos pontos criticos passou em 2026-07-10: `/health`, `/api/lojas`, `/api/lojas/1`, `/swagger/v1/swagger.json`, aliases FICO e CORS sem 404; falta varredura completa do frontend |
| Multitenancy e soft-delete em 100% das entidades | Parcial | Migrar entidades legadas e validar isolamento por tenant |
| MFA TOTP funcional | Parcial | Executar teste integrado enable, verify e login com MFA |
| NF-e/NFC-e SEFAZ homologacao | Pendente | Integrar certificado real e validar emissao em homologacao |
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
| Ferramentas ficticias convertidas em reais ou bloqueadas claramente | Em execucao | Shopee/Amazon deixam de gravar sucesso falso; landing legada e admin estatico legado foram removidos da exposicao operacional e redirecionam para o portal React oficial; concluir PDF financeiro, tracking externo e demais achados da secao acima |

## GitHub e Commit

Estado apurado:

- Branch local: `work/delivery-2026-06-13`.
- Commit local e remoto atual: `07a465e feat: api - alinhar auditoria tenant e fluxos reais`.
- `origin/work/delivery-2026-06-13`: alinhado com `07a465e`.
- `origin/main`: `5cb041d46d30af0ab7da4a7f9eeda1b2a4ea983f`.
- O estado atual da arvore ainda possui muitas alteracoes nao commitadas e nao deve ser tratado como sincronizado com `main`.
- Total de alteracoes locais remanescentes apos o commit `07a465e`: 51 entradas no `git status --short --untracked-files=normal`.
- Commits atomicos enviados nesta rodada: `c7076d5`, `7ccc668`, `028ee9f`, `1e0a4ec`, `5611329`, `07a465e`.
- Publicacao GitHub deve ser seletiva e auditada; publicar todo o worktree atual sem triagem pode enviar arquivos legados/falsos que o prompt bloqueia.

## Regra de continuidade

Nao declarar item como concluido sem uma evidencia tecnica associada: build, teste, rota HTTP, consulta de banco, migracao aplicada, arquivo publicado, commit remoto ou validacao visual quando o item for de interface.
