<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Checklist Clinico GenesisGest.Net v1.1.5

Data da apuracao: 2026-07-10.

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
| B2 | Controllers MVC fora do ativo e endpoints criticos sem 404 | Ajustado | API oficial e Minimal API; controllers preservados em `NexumAltivon_Back-End/API/Legacy`; smoke publico passou nos pontos obrigatorios |
| B3 | Duplicacao massiva da raiz removida do build ativo | Ajustado local | Pastas legadas aparecem como removidas no Git e preservadas fora do pipeline ativo |
| B4 | `Sys_AuditableEntity` em 100% das entidades transacionais | Parcial | Infraestrutura existe; auditoria central no DbContext existe; heranca direta ainda nao cobre todas as classes |
| B5 | MFA, refresh-token, tenants e workflows | Parcial | Rotas existem e compilam; tenant smoke validado; fluxo completo ainda exige teste integrado |
| B6 | Testes com cobertura minima de 70% em Services | Pendente | Nao ha projeto de teste ativo na arvore oficial |
| B7 | Observabilidade completa | Parcial | Health e Redis existem; Serilog/OpenTelemetry completos ainda nao foram homologados ponta a ponta |
| B8 | Backup diario e restore-test semanal | Parcial | Scripts e containers existem; execucao agendada e restore-test em CI ainda nao comprovados |
| B9 | EF Migrations | Parcial | Migrations do Nexum existem; `dotnet ef database update` em banco vazio ainda nao foi validado nesta apuracao |
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
| Inicializacao fixa | Tarefa `NexumAltivonApi24h` registrada como `SISTEMA`, `RunLevel Highest`, chamando o script oficial dentro do projeto |
| Porta publica interna | API escutando `127.0.0.1:5010`; Cloudflared permanece apontado para esta porta |
| Causa real do 502 | API nao subia porque a configuracao privada apontava para caminho externo ausente e, depois, faltava `CREATE VIEW` ao usuario de aplicacao |
| Processo atual | `dotnet.exe NexumAltivon.API.dll` executando em PowerShell oculto pelo script oficial iniciado pela tarefa `NexumAltivonApi24h` |
| Validacao elevada da tarefa | `runtime-logs/api-24h-task-query.log`: `TaskState=Running`, `HealthStatus=200`, `ListenerCommand=dotnet.exe NexumAltivon.API.dll` |

## Definition of Done

| Item | Status real | Proxima acao tecnica |
|---|---|---|
| `dotnet build NexumAltivon.ERP.sln -c Release` sem erros nem avisos | Concluido local | Manter como gate antes de commit |
| `dotnet test` com cobertura >= 70% nos Services | Pendente | `dotnet test` retornou 0, mas nao ha projeto de teste ativo; criar projeto real e gate de cobertura |
| `npm run build` no frontend | Concluido local | Manter validacao antes de publicar |
| Todos os endpoints faltantes da Secao 7 com payload real | Parcial | Completar por setor e validar via HTTP |
| Frontend sem 404 nos endpoints consumidos | Parcial | Smoke publico dos pontos criticos passou; CORS de login retornou 204; falta varredura completa do frontend |
| Multitenancy e soft-delete em 100% das entidades | Parcial | Migrar entidades legadas e validar isolamento por tenant |
| MFA TOTP funcional | Parcial | Executar teste integrado enable, verify e login com MFA |
| NF-e/NFC-e SEFAZ homologacao | Pendente | Integrar certificado real e validar emissao em homologacao |
| WMS completo | Parcial | Completar endpoints/telas e validar movimentacao, inventario, kardex e transferencia |
| MES/OPS operacional | Parcial | Backend existe; telas e fluxo completo ainda precisam validacao |
| Hangfire e jobs agendados | Pendente | Ativar storage e validar execucao de jobs |
| Redis integrado e healthcheck verde | Parcial | Health Redis existe; validar em runtime de producao |
| S3/MinIO anexos | Parcial | Servico e compose existem; validar upload/download assinado |
| Serilog e OpenTelemetry | Pendente | Ativar logs/traces/metrics e validar exportacao |
| EF Migrations aplicaveis em banco vazio | Parcial | Executar `dotnet ef database update` em ambiente controlado |
| Secrets fora de arquivos versionados | Parcial | Revisar arquivos versionados e manter segredos apenas em env/User Secrets/cofre |
| CI/CD com gate de cobertura, Sonar e migrate | Parcial | Adicionar teste/cobertura/migrate e health-check pos-deploy |
| Compose dev sobe tudo | Nao validado nesta apuracao | Executar `docker compose up --build` em janela propria |
| Desktop auto-update funcional | Parcial | Publicar release desktop real e validar atualizacao em maquina cliente |
| TLS ativo sem HTTP publico puro | Parcial | Porta 443 responde; validar politica completa Cloudflare/nginx |
| AGENTS e OpenAPI | Parcial | `AGENTS.md` atualizado; OpenAPI ajustado no codigo e precisa publicar nova API |
| Termos proibidos no codigo ativo | Pendente | Triar ocorrencias em frontend, docs e arquivos antigos |
| Header de IP em todos os arquivos | Pendente | Varredura achou 226 arquivos ativos sem header |
| Fluxo cadastro a BI isolado por empresa | Pendente | Executar roteiro ponta a ponta com dados reais |

## GitHub e Commit

Estado apurado:

- Branch local: `work/delivery-2026-06-13`.
- Commit local base: `b7e138b8a7eb34a74ddeac5a2d96d77dcb756c70`.
- `origin/work/delivery-2026-06-13`: mesmo commit base.
- `origin/main`: `5cb041d46d30af0ab7da4a7f9eeda1b2a4ea983f`.
- O estado atual da arvore ainda possui muitas alteracoes nao commitadas e nao deve ser tratado como sincronizado com `main`.
- Total de alteracoes locais apuradas em 2026-07-10: 281 entradas no `git status --short`.
- Publicacao GitHub deve ser seletiva e auditada; publicar todo o worktree atual sem triagem pode enviar arquivos legados/falsos que o prompt bloqueia.

## Regra de continuidade

Nao declarar item como concluido sem uma evidencia tecnica associada: build, teste, rota HTTP, consulta de banco, migracao aplicada, arquivo publicado, commit remoto ou validacao visual quando o item for de interface.
