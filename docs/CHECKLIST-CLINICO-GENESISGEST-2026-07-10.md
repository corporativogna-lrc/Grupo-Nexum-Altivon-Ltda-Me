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
Apuracao da persistencia das configuracoes publicas: 2026-07-17.
Apuracao da maquina de estados de workflow: 2026-07-17.
Apuracao integrada de MFA, refresh-token e logout: 2026-07-17.
Apuracao integrada de Services, testes e cobertura: 2026-07-17.
Planejamento clinico de unificacao dos bancos: 2026-07-17.
Apuracao integrada de usuarios, perfis, permissoes e RBAC no Desktop: 2026-07-17.
Apuracao transacional do checkout e incidentes pos-commit: 2026-07-18.

## Medicao Estrita de Conclusao

- Definition of Done expandida: `7/31` itens concluidos, equivalente a `22,6%`.
- Bloqueadores B1 a B14: `4/14` integralmente concluidos, equivalente a `28,6%`.
- Itens parciais nao entram no numerador. Portanto, o projeto permanece objetivamente abaixo de 30% pelo criterio de aceite integral, embora exista implementacao parcial relevante em varios modulos.

## Rotas Travadas

| Item | Rota oficial | Status |
|---|---|---|
| Projeto oficial | `D:\Nexum Altivon\NexumAltivon.com` | Travado |
| Banco e-commerce | `D:\xampp\mysql\data\nexum_altivon` | Existe localmente |
| Banco GenesisGest | `D:\xampp\mysql\data\genesis_bd` | Existe localmente |
| GitHub | `https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me` | Remoto configurado |
| Materiais auxiliares | `D:\` | Consulta tecnica, sem dependencia runtime |

## Programa Clinico de Unificacao dos Bancos

Objetivo aprovado: substituir o uso permanente de `nexum_altivon` e `genesis_bd` por um unico schema canonico, somente depois de comprovar que API, frontend, Desktop, jobs, relatorios, migrations e integracoes usam o mesmo contrato de dados. Esta inclusao nao autoriza alteracao fisica imediata dos bancos oficiais.

| Etapa | Criterio obrigatorio | Status real |
|---|---|---|
| UBD-01 Inventario | Catalogar tabelas, views, colunas, tipos, chaves, indices, FKs, triggers, procedures, events, volume, datas extremas e tamanho nos dois schemas | Pendente programado |
| UBD-02 Linhagem | Mapear cada tabela para entidades, SQL, endpoints, services, telas React/WPF, jobs, relatorios, scripts e integracoes que a leem ou escrevem | Pendente programado |
| UBD-03 Equivalencias | Classificar estruturas exclusivas, equivalentes, duplicadas e conflitantes; maior volume prevalece apenas quando semantica, integridade e atualidade tambem forem comprovadas | Pendente programado |
| UBD-04 Modelo canonico | Definir nomenclatura padronizada, tipos, PK/FK, tenant, auditoria, soft-delete, concorrencia e compatibilidade temporaria para cada dominio | Pendente programado |
| UBD-05 Migracao homologada | Gerar scripts idempotentes, backup verificavel e ensaio em copia controlada sem editar arquivos fisicos `.frm`, `.ibd` ou diretorios internos do MySQL | Pendente programado |
| UBD-06 Reconciliacao | Comparar contagens, somatorios financeiros/fiscais, saldos, documentos, duplicidades, orfaos e hashes de conjuntos antes e depois | Pendente programado |
| UBD-07 Corte controlado | Suspender escrita pelo menor intervalo possivel, aplicar delta final, trocar uma unica connection string, executar smoke ponta a ponta e reverter automaticamente se qualquer gate falhar | Pendente programado |
| UBD-08 Desativacao | Remover o schema antigo somente depois de todos os consumidores, backups, observabilidade e periodo de estabilidade apontarem exclusivamente para o schema canonico | Pendente programado |

Regra de preservacao: nenhuma tabela sera escolhida apenas pelo nome ou pela quantidade de linhas. A decisao por entidade deve considerar completude, chaves, relacoes, atualidade, consistencia, valor fiscal/financeiro e consumidor operacional. Nao sera usado CRUD MySQL direto no Desktop para executar esta fusao.

## Bloqueadores B1 a B14

| ID | Requisito | Status real | Evidencia objetiva |
|---|---|---|---|
| B1 | Solution unica compila API, ERP e Desktop | Concluido local | `NexumAltivon.ERP.sln` lista API, ERP, Desktop e projeto raiz; build Release com 0 erros e 0 avisos |
| B2 | Controllers MVC fora do ativo e endpoints criticos sem 404 | Ajustado e publicado na branch de entrega | API oficial e Minimal API; controllers MVC removidos do projeto da API ativa no commit `4bacfe5`; smoke publico passou nos pontos obrigatorios |
| B3 | Duplicacao massiva da raiz removida do build ativo | Concluido para a raiz legacy | Diretorios legados da raiz foram removidos do build/versionamento no commit `7ccc668`; solution Release seguiu compilando com 0 erros e 0 avisos |
| B4 | `Sys_AuditableEntity` em 100% das entidades transacionais | Parcial avancado | Commit `07a465e` aplicou tenant, soft-delete, auditoria central e `row_version` BLOB no `NexumDbContext`; heranca direta ainda nao cobre todas as classes |
| B5 | MFA, refresh-token, tenants e workflows | Parcial avancado | Workflow, MFA TOTP, refresh-token rotativo e logout foram publicados e homologados no runtime oficial. MFA usa segredo protegido por Data Protection/DPAPI persistente, bloqueia reuso TOTP e sobrevive ao reinicio; refresh de usuario e cliente usa apenas hash SHA-256, expira, rotaciona atomicamente e e revogado no logout. Falta homologar integralmente o CRUD e o isolamento de tenants para concluir o bloqueador inteiro |
| B6 | Testes com cobertura minima de 70% em Services | Parcial avancado | Projeto xUnit isolado do binario de producao foi incluido na solution. Em 2026-07-17, `29/29` casos passaram e os Services ativos atingiram `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos; o CI bloqueia a imagem da API abaixo de 70%. Faltam os testes de integracao por modulo com `WebApplicationFactory` para Auth, Pedidos, Compras, Financeiro, Fiscal, Estoque, CRM e RH antes de concluir B6 integralmente |
| B7 | Observabilidade completa | Parcial | Health e Redis existem; Serilog/OpenTelemetry completos ainda nao foram homologados ponta a ponta |
| B8 | Backup diario e restore-test semanal | Parcial | Backup local 2h corrigido para `D:\Nexum Altivon\NexumAltivon.com` e executado com resultado 0; restore-test em CI ainda nao comprovado |
| B9 | EF Migrations | Parcial avancado | `dotnet-ef` 8.0.5 esta instalado e a migration `HardenPlatformSso` foi gerada pelo modelo real com somente cinco alteracoes de autenticacao. O banco oficial possui `__EFMigrationsHistory` vazio apesar do schema existente, portanto `database update` nessa base foi corretamente bloqueado para nao reaplicar a migration inicial; falta validar a cadeia completa em banco vazio controlado |
| B10 | Secrets fora dos arquivos versionados | Parcial avancado | Runtime local usa `runtime/api-24h/api.env.ps1`, ignorado pelo Git. Em 2026-07-17, a credencial MySQL operacional foi rotacionada com geracao criptografica, atualizacao das contas e do arquivo privado, validacao dos dois schemas e republicacao da API; ainda ha configuracoes de desenvolvimento e templates a revisar |
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
| Plataforma SSO publicada | Em 2026-07-17, a API oficial foi republicada na unica porta 5010. A tarefa `NexumAltivonApi24h` permaneceu como `SYSTEM`, `ServiceAccount`, gatilho exclusivo de boot, reinicio automatico e processo invisivel. Health local e publico, bancos Nexum/Genesis e configuracao publica responderam HTTP 200 |
| Persistencia segura de autenticacao | `usuarios` e `clientes` passaram a armazenar somente SHA-256 do refresh token e data de expiracao; `mfa_secret` foi ampliado e recebe apenas valor protegido por Data Protection com chave persistente cifrada por DPAPI da maquina; refresh legado sem hash/expiracao foi revogado na atualizacao de schema |
| Fluxo administrativo MFA | Ensaio controlado confirmou login local 200, hash/expiracao no MySQL, MFA enable 200, segredo cifrado no banco, codigo invalido 400, verify 200, reuso do mesmo passo TOTP 400, reinicio da API, login MFA pelo dominio publico 200, rotacao 200, replay do refresh antigo 401, logout 200 e refresh depois do logout 401 |
| Fluxo de cliente | Ensaio controlado pelo dominio publico confirmou login 200 com par de tokens, hash/expiracao no MySQL, rotacao 200, replay do token anterior 401, logout 200 e refresh revogado 401. Um cliente autenticado recebeu 401 ao tentar iniciar MFA administrativo, eliminando colisao entre IDs inteiros de clientes e usuarios |
| Gate dos Services ativos | `NexumAltivon.API.Tests.csproj` permanece separado do projeto Web e usa `obj/bin` exclusivos. `dotnet test` executou `29/29` casos com `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos no namespace ativo `NexumAltivon.API.Services`; o relatorio local foi emitido somente em `%TEMP%` |
| Integracoes de notificacao | `NotificacaoService` deixou de engolir recusas do WhatsApp, passou a validar URL, telefone, mensagem e configuracoes obrigatorias, propaga recusas de SendGrid/WhatsApp e recebe `CancellationToken` em toda E/S. `AssistenteIaService` fixa a persona no servidor, valida chave/modelo externamente e nao aceita seletor enviado pelo navegador |
| Publicacao do gate e Services | Em 2026-07-17T05:30, a API foi republicada somente na porta 5010. Tarefa `NexumAltivonApi24h` validada como `SYSTEM`, boot exclusivo, PID `2516`; MySQL e Cloudflared `Running/Auto`; health local/public, dois bancos, configuracao publica e Swagger responderam 200. O hash SHA-256 da DLL compilada coincide com a DLL publicada e o diretorio publicado contem zero assemblies de teste |
| Separacao Yara/Sophia publicada | Yara usa exclusivamente `/api/assistentes/yara/mensagem` e recebe contexto somente leitura de lojas/produtos publicaveis do MySQL. Sophia usa `/api/assistentes/sophia/mensagem` com policy `Admin`. Configuracao de chaves/modelos usa `/api/admin/integracoes/openai` com policy `SuperAdmin`. A rota ambigua anterior retorna 410, sem seletor de persona |
| Bloqueio explicito da IA | Como nenhuma chave OpenAI foi fornecida, Yara local e publica retornaram HTTP 503 com causa rastreavel; Sophia local e publica retornaram 401 sem JWT; nenhuma resposta de IA foi fabricada |
| Limpeza das validacoes SSO | As contas controladas de usuario e cliente foram removidas fisicamente; consultas finais retornaram zero registros residuais |
| Painel administrativo React | `Dashboard.js` ajustado no commit `0b8212e` para nao exibir sucesso visual com estado local fabricado em `Site & Banners` e rascunho fiscal manual; apos salvar, a tela usa retorno/releitura real da API antes de confirmar persistencia |
| Salvar configuracao publica | Em 2026-07-17, `Dashboard.js` passou a exibir sucesso ou erro na propria tela, bloquear clique duplo durante a gravacao e comparar cada valor retornado pela API. `PUT /api/site/configuracoes` deixou de responder `ok` generico: valida escopo, duplicidade, JSON, tamanho e permissao de edicao, executa `SaveChangesAsync`, rele o MySQL e devolve a colecao persistida. Ensaio autenticado na API oficial confirmou HTTP 400 para chave fora do escopo, JSON invalido e duplicidade sem alterar o banco; lote valido foi confirmado na resposta administrativa, configuracao publica e MySQL antes e depois do reinicio PID `7492` para `10980`; restauracao passou e a limpeza terminou com zero usuarios tecnicos. |
| Central de IA do site | `GlobalActions.js` nao inventa resposta local quando a API falha, removeu o seletor Yara/Sophia e envia o portal apenas para Yara. No dashboard, Sophia usa rota distinta e protegida; a mensagem de erro exibe a causa retornada pela API em vez de afirmar oscilacao inexistente |
| Frete/logistica sem sucesso falso | `Program.cs` ajustado no commit `4918d30`: `/api/frete/cotar` e `/api/logistica/roteamento` retornam 502 se Melhor Envio estiver configurado e falhar, recusar ou nao devolver cotacao utilizavel; tabela interna oficial so e usada quando nao ha credencial externa configurada |
| HTML estatico legado do frontend | `NexumAltivon_Front-End/index.html` ajustado no commit `937512d`: removido HTML legado com Formspree, links sem destino, termos de lancamento futuro e CNPJ inexistente; arquivo agora redireciona para o portal oficial publicado |
| Script legado de CRM | `nexum-integration.js` e `public/nexum-integration.js` ajustados no commit `6dbf1f0`: formulario legado so confirma cadastro quando `/api/crm/leads` retorna `Dados.Id`; sem confirmacao de persistencia, exibe erro operacional |
| Smoke publico painel/portal | Endpoints principais consumidos pelo painel e portal foram verificados em `https://api.nexumaltivon.com.br`: rotas publicas retornaram 200, rotas protegidas retornaram 401, nenhuma retornou 404 |
| Smoke publico 2026-07-14 | Local e publico responderam sem 404 para `/api/lojas/1`, `/api/pedidos/1`, `/api/relatorios/vendas`, `/api/financeiro/faturamento`, `/api/dashboard/resumo`, `/api/clientes`, `/api/fornecedores`, `/api/fiscal/pedidos` e `/swagger/v1/swagger.json`; protegidas retornaram 401 e publicas 200 |
| Frete em runtime 5010 | `POST /api/frete/cotar` local e publico retornou 200 com mensagem `Cotacao operacional calculada pela tabela interna oficial.`, fonte `Tabela interna oficial` e sem texto antigo de fallback silencioso |
| IA em runtime 5010 | Em 2026-07-17T06:14, Yara local/publica retornou 503 sem chave, Sophia local/publica retornou 401 sem JWT, configuracao retornou 401 sem `SuperAdmin` e a rota ambigua anterior retornou 410. Swagger confirmou as tres rotas oficiais novas |
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
| Chaves OpenAI no Desktop WPF | `NetworkSettingsWindow` possui login administrativo, campos e modelos separados para Yara/Sophia e salva pela API somente apos validacao real na OpenAI. A API cifra cada credencial com AES-256-GCM, persiste em `configuracoes_sistema`, rele e compara dentro da transacao e registra auditoria sem segredo. JWT e chaves permanecem apenas em memoria no Desktop; sem chave, Yara continua desativada |
| Workflow operacional | A API Minimal oficial passou a validar estados alcancaveis, transicoes configuradas, entidade da definicao, perfil autorizado e concorrencia. Ensaio autenticado em 2026-07-17 executou 27 verificacoes: criacao e reativacao 201, consultas/transicoes 200, exclusoes 204, entradas invalidas 400, RBAC 403, conflitos 409, rota Cloudflare 200, duas transicoes confirmadas diretamente no MySQL e limpeza final com zero registros residuais |
| Gestao administrativa IAM no Desktop | `AccessManagementWindow` foi integrada ao menu WPF e opera usuarios, perfis corporativos, permissoes e matriz RBAC exclusivamente pela API Minimal oficial, com JWT em memoria, restricao `Admin`/`SuperAdmin`, confirmacao de releitura e erro HTTP sem reenvio de escrita. O ensaio autenticado executou login 200, usuario 201/200, perfil 201, permissao 201, vinculo/matriz 200, desvinculo 200 e desativacoes 200; o MySQL confirmou as alteracoes e `logs_auditoria` registrou 9 eventos. A limpeza final confirmou zero usuarios, perfis, permissoes e vinculos controlados |
| Integridade estrutural IAM | As tabelas legadas `adm_perfis`, `adm_permissoes` e `adm_perfil_permissoes` existiam sem PK, `AUTO_INCREMENT`, unicidade ou FKs. A inicializacao oficial agora interrompe claramente se encontrar tabela sem PK contendo dados desconhecidos; como as tabelas estavam vazias apos a limpeza controlada, foram reparadas com 3 PKs, 3 `AUTO_INCREMENT`, indices unicos e 2 FKs, e os seeds canonicos ficaram estabilizados em 6 perfis e 9 permissoes sem duplicacao em reinicios |
| Rotacao da credencial MySQL operacional | `scripts/server/rotacionar-credencial-mysql-runtime.ps1` gera 256 bits por CSPRNG, altera todas as contas encontradas do usuario de runtime, atualiza somente o arquivo privado ignorado pelo Git, valida acesso aos dois schemas e reverte banco/configuracao em falha. A execucao real atualizou 2 contas, validou 2 schemas e foi seguida de republicacao na unica porta 5010; health local/publico e os dois bancos responderam 200 sob a nova credencial |
| Referencias societarias encerradas | Removidas das fontes, seeds, contato, rodape, documentacao e bundle estatico; varredura integral no projeto oficial retornou zero ocorrencias do nome solicitado |
| Checkout transacional e numeracao fiscal | O checkout passou a executar a escrita principal pela estrategia de execucao do EF Core, com transacao unica para cliente, pedido, itens, estoque, financeiro e pagamento. A reserva de numero fiscal usa atualizacao atomica condicional em `empresas_grupo`, incrementa `ProximaNfeNumero` e renova `RowVersion`; conflito encerra a preparacao sem duplicar numero |
| Incidentes pos-commit sem sucesso falso | Falhas de preparacao fiscal, integracao Genesis e notificacao sao persistidas em `notificacoes` e `logs_auditoria` com referencia e codigo seguro. Quando a venda ja foi confirmada, a API retorna HTTP 202 com `StatusFiscal`/`AlertaOperacional`; se nem o incidente puder ser persistido, retorna erro critico com o numero do pedido e proibe reenvio automatico |
| Ensaio controlado do checkout | Cenario fiscal recusado retornou HTTP 202, gravou estado `Preparacao fiscal falhou`, codigo MySQL e auditoria, sem consumir a numeracao. Cenario normal reservou o numero 7, avancou a empresa para 8, gravou pre-emissao, financeiro, pagamento e integracao Genesis; a indisponibilidade real do SendGrid foi registrada e exposta como pendencia. Dados controlados foram removidos e numeracao/estoque foram restaurados aos valores anteriores |
| Validacao integral da entrega de checkout | Em 2026-07-18, a solution Release compilou os cinco projetos com 0 avisos e 0 erros; o frontend compilou o bundle `main.301e9934.js`; `29/29` testes passaram com `70,84%` de cobertura de linhas dos Services. A API foi republicada na porta unica 5010 sob PID `12508`; tarefa `SYSTEM`, boot exclusivo, MySQL/Cloudflared automaticos e health local/publico retornaram HTTP 200 |
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
| Banco local oficial | MySQL XAMPP validado em `127.0.0.1:3309`; em 2026-07-18, `nexum_altivon` possui 203 tabelas base e 12 views, e `genesis_bd` possui 48 tabelas base |
| Catalogo publico | Consulta direta no banco confirmou 91 produtos ativos e 91 produtos publicaveis pelo filtro atual da API |
| API oficial 5010 | Task `NexumAltivonApi24h` validada em 2026-07-13T00:53:39 como `Running`, usuario `SISTEMA`, `RunLevel Highest`, processo `dotnet.exe NexumAltivon.API.dll`, PID `10928`, `/health`, `/health/db`, `/health/db/genesis` e `/api/site/configuracoes/publico` com HTTP 200 |
| GitHub oficial atualizado | `origin/main` e `origin/work/delivery-2026-06-13` alinhados no commit `8d58edb fix: api - limpar base de teste e corrigir seeds auditaveis` |

## Mapa Clinico das Ferramentas Prometidas pelo Painel Legado

O painel HTML removido foi recuperado somente para leitura pelo historico Git (`5d05ab0^:NexumAltivon_Front-End/admin/index.html`). Ele anunciava 16 ferramentas, mas continha secoes HTML somente para Dashboard, Pedidos, Produtos, Clientes, CRM, Lojas, Financeiro e Configuracoes. Fiscal, Logistica, Cupons, Marketing, Marketplaces, Dropshipping, Usuarios e Auditoria apareciam no menu sem secao funcional correspondente.

| Ferramenta prometida | Tela React oficial | API/persistencia | Status clinico em 2026-07-14 | Evidencia ou bloqueio atual |
|---|---|---|---|---|
| Dashboard | Existe em `Dashboard.js` | `/api/dashboard/resumo` e `/api/admin/dashboard/completo` | Parcial validado | KPIs reais existem; a serie semanal de linhas 453-459 ainda e estatica e deve ser substituida por consulta real por tenant e periodo antes de aceite |
| Pedidos | Existe | `/api/pedidos*`, MySQL `pedidos` | Parcial validado | Lista e alteracao de status existem; fluxo venda-pagamento-fiscal-logistica-financeiro ainda nao foi homologado integralmente |
| Produtos | Existe | `/api/produtos*`, MySQL `produtos` | Parcial validado | CRUD e catalogo existem; falta homologar todas as restricoes de estoque/tenant e marketplace |
| Clientes | Existe | `/api/clientes*`, MySQL `clientes` | Parcial validado | CRUD, portal e confirmacao existem; falta teste completo de isolamento e historico operacional |
| Lojas | Sem tela dedicada equivalente | `/api/lojas*`, MySQL `lojas` | Parcial | Dados aparecem em dashboard/empresas; falta gestao dedicada das seis lojas com gravacao e permissoes |
| Financeiro | Existe | `/api/financeiro*` e `/api/erp/genesis/financeiro*` | Parcial avancado | Lancamentos, razao, conciliacao, DRE e fechamento existem; PDFs foram corrigidos, mas fluxo e tela completa ainda requerem homologacao |
| Fiscal | Existe | `/api/fiscal*` | Parcial real bloqueado externamente | Endpoints bloqueiam sem certificado/provedor; falta certificado e homologacao SEFAZ real |
| Logistica | Existe | `/api/frete*`, `/api/logistica*`, estoque/transportes | Parcial real | Cotacao interna identificada funciona; tracking externo exige endpoint/token real e retorna bloqueio rastreavel sem eles |
| CRM | Existe | `/api/crm/leads*`, campanhas e segmentos | Parcial avancado | Leads, campanhas e segmentos possuem persistencia comprovada; pipelines, oportunidades, tickets e atividades ainda nao estao completos na API/tela |
| Cupons | Implementada nesta rodada em `CupomAdminPanel.js` | `/api/admin/cupons*` e `/api/cupons/{codigo}`, MySQL `cupons` | Concluido e validado no escopo CRUD/uso publico | Em 2026-07-14, no runtime oficial `127.0.0.1:5010`: POST retornou 201 e ID persistido, subtotal abaixo do minimo retornou 400, consulta publica com vigencia `DATE` retornou 200, PUT retornou 200, DELETE desativou com 200 e nova consulta publica retornou 404. A limpeza controlada confirmou zero registros residuais no MySQL |
| Marketing | Implementada em `MarketingAdminPanel.js` | `/api/crm/campanhas*`, `/api/crm/segmentos*`, MySQL `crm_campanhas` e `crm_segmentos` | Concluido e validado no escopo CRUD/segmentacao e metricas | Tela oficial possui campanhas, segmentos, investimento, resultados e transicoes de estado. Ensaio autenticado em 2026-07-14 confirmou criacao 201, atualizacao 200, concorrencia 409, consulta 200, exclusao 204, soft-delete no MySQL e limpeza final sem linhas controladas |
| Marketplaces | Diagnostico e formulario de sincronizacao em Integracoes | `/api/marketplaces/{canal}/sync` e services | Parcial bloqueado por credenciais | Tela React envia canal, direcao, entidade e limite para a API real. Mercado Livre/B2W/Via responderam 424 local e publicamente sem tokens, sem registrar sucesso; Shopee/Amazon nao gravam mais sucesso fabricado, mas conectores oficiais continuam pendentes |
| Dropshipping | CRUD operacional incorporado ao React e Desktop WPF | `/api/dropshipping/canais*`, MySQL `dropshipping_config`, `DropshippingAdminPanel.js`, `DropshippingWindow.xaml` e roteamento existente | Parcial avancado | Em 2026-07-14, o schema enum e os tipos Shopify/CJ foram corrigidos; CRUD autenticado confirmou bloqueio 424 sem conector, criacao 201, consulta persistida, atualizacao 200, concorrencia 409, exclusao 204, soft-delete no MySQL e limpeza sem residuos. React e WPF listam e editam canais sem expor segredos; o WPF compilou sem erros ou avisos e os dois menus de dropshipping abrem a janela especifica. Falta implementar e homologar o ciclo externo de catalogo, cotacao, pedido, retorno e rastreamento para cada provedor contratado |
| Usuarios | Implementada no React e no Desktop WPF por `AccessManagementWindow` | `/api/admin/usuarios*`, `/api/perfis*`, `/api/permissoes*`, `/api/perfis/{id}/permissoes*`, MySQL `usuarios`, `adm_perfis`, `adm_permissoes`, `adm_perfil_permissoes` e `logs_auditoria` | Concluido no escopo de gestao administrativa | Ensaio autenticado confirmou criacao, edicao, ativacao/desativacao, perfis, permissoes, vinculo e remocao da matriz RBAC com HTTP 200/201; persistencia e 9 auditorias foram comprovadas diretamente no MySQL. `SuperAdmin` so pode ser atribuido por outro `SuperAdmin`; a limpeza terminou sem registros controlados |
| Configuracoes | Existe | `/api/site/configuracoes*`, MySQL | Concluido no escopo de salvamento; parcial no conjunto do portal | Em 2026-07-17, o salvamento foi validado de ponta a ponta com autenticacao, confirmacao chave a chave, leitura administrativa, payload publico, consulta MySQL e reinicio da tarefa oficial. Entradas invalidas retornaram 400 sem alterar a base, o valor original foi restaurado e nao restaram usuarios tecnicos. Ainda falta homologar individualmente todos os campos editaveis e concluir a evolucao visual ampla do portal |
| Auditoria | Implementada nesta rodada em `AccessAuditPanel.js` | `/api/auditoria*`, MySQL `logs_auditoria` | Parcial validado | API local e publica retornaram HTTP 200 e 42 eventos; tela de filtros/detalhe compilou. Cobertura de toda escrita ainda nao foi comprovada |

Regra de aceite para cada ferramenta: somente marcar `Concluido` quando houver tela oficial, endpoint Minimal API, persistencia/consulta real quando aplicavel, autorizacao, tratamento de erro, build e teste HTTP autenticado. Integracao externa tambem exige chamada aceita pelo provedor oficial e persistencia do identificador retornado.

## Capacidades Estruturais Incorporadas ao Checklist

A arvore comprovada e a arvore de aceite integral estao em `docs/ARVORE-REAL-E-META-GENESISGEST-2026-07-18.md`. O documento estrutural historico foi usado apenas para recuperar amplitude funcional; suas alegacoes nao foram aceitas como evidencia. Estes itens detalham requisitos ja contidos na Definition of Done e nao aumentam o numerador separadamente.

| ID | Capacidade recuperada | Status clinico | Evidencia obrigatoria para concluir |
|---|---|---|---|
| CAP-01 | Cockpit executivo dinamico | Parcial | KPIs e series por tenant/periodo provenientes da API e conferidos no banco |
| CAP-02 | Venda omnicanal | Parcial | Correlacao unica de pedido, pagamento, estoque, fiscal, entrega e financeiro |
| CAP-03 | Compras e abastecimento | Parcial | Solicitacao, cotacao, aprovacao, pedido, entrada, estoque e financeiro comprovados |
| CAP-04 | WMS | Parcial | Movimentacao, inventario, kardex, localizacao e transferencia com concorrencia |
| CAP-05 | Fiscal explicavel | Parcial | Roteamento, numeracao, XML, chave, protocolo e eventos oficiais visiveis e persistidos |
| CAP-06 | FICO corporativo | Parcial | Lancamentos, conciliacao, fechamento, DRE e PDFs integrados em API e tela |
| CAP-07 | Logistica e fulfillment | Parcial | Cotacao, expedicao, tracking externo, entrega e ocorrencias comprovados |
| CAP-08 | CRM e atendimento Yara | Parcial | Pipeline, oportunidade, atividade, ticket e atendimento com chave oficial |
| CAP-09 | HCM | Parcial | Admissao, ponto, folha, beneficios, avaliacao, desligamento e eSocial comprovados |
| CAP-10 | MES e OPS | Parcial | OS, apontamento, manutencao e ativos com telas React/WPF especificas |
| CAP-11 | Paridade Desktop | Parcial | Operacoes criticas usam a API oficial e confirmam releitura persistida |
| CAP-12 | Marketplaces e dropshipping | Bloqueado externamente | Retorno aceito e identificador oficial de cada seller/provedor persistido |
| CAP-13 | Portal visual dinamico | Parcial | Midias reais de lojas/produtos, conteudo administravel e validacao Chrome responsiva |
| CAP-14 | Operacao observavel | Parcial | Logs, traces, metricas, Redis, jobs, backup e restore comprovados |
| CAP-15 | Banco canonico | Pendente programado | UBD-01 a UBD-08 concluidos com reconciliacao e reversao |

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
| `dotnet test` com cobertura >= 70% nos Services | Concluido local e fixado no CI | `29/29` casos aprovados; cobertura dos Services ativos em `70,84%` de linhas. Manter o teste separado do binario Web e ampliar integracao por modulo sem reduzir o gate |
| `npm run build` no frontend | Concluido local | Manter validacao antes de publicar |
| Todos os endpoints faltantes da Secao 7 com payload real | Parcial | Marketplace sync Mercado Livre/B2W/Via foi implementado na API ativa com bloqueio sem credencial real; completar demais setores e validar via HTTP |
| Frontend sem 404 nos endpoints consumidos | Parcial | Smoke publico dos pontos criticos passou em 2026-07-10: `/health`, `/api/lojas`, `/api/lojas/1`, `/swagger/v1/swagger.json`, aliases FICO e CORS sem 404; falta varredura completa do frontend |
| Multitenancy e soft-delete em 100% das entidades | Parcial | Migrar entidades legadas e validar isolamento por tenant |
| MFA TOTP funcional | Concluido no runtime oficial | Backend, MySQL e portal usam o mesmo contrato. Enable/verify/login foram validados local e publicamente; segredo cifrado sobreviveu ao reinicio, reuso TOTP foi recusado, refresh de usuario/cliente rotacionou atomicamente e logout revogou a sessao sem residuos controlados |
| Workflow de aprovacao funcional | Concluido no backend oficial | CRUD de definicoes e instancias, historico, maquina de estados, RBAC, tenant, soft-delete e atualizacao atomica foram publicados e validados localmente, no dominio Cloudflare e diretamente no MySQL; faltam apenas telas especificas quando cada modulo consumir o workflow |
| NF-e/NFC-e SEFAZ homologacao | Parcial real, bloqueado por credencial/certificado | Endpoints de emissao/eventos existem na API ativa e nao registram sucesso sem provedor real; falta configurar certificado/provedor homologado e validar chave/protocolo real |
| WMS completo | Parcial | Completar endpoints/telas e validar movimentacao, inventario, kardex e transferencia |
| MES/OPS operacional | Parcial | Backend existe; telas e fluxo completo ainda precisam validacao. No WPF, a janela generica agora registra solicitacao real na API ou contingencia explicitamente pendente; isto nao substitui as telas especificas de OS, producao, manutencao e ativos |
| Hangfire e jobs agendados | Pendente | Ativar storage e validar execucao de jobs |
| Redis integrado e healthcheck verde | Parcial | Health Redis existe; validar em runtime de producao |
| S3/MinIO anexos | Parcial | Servico e compose existem; validar upload/download assinado |
| Serilog e OpenTelemetry | Pendente | Ativar logs/traces/metrics e validar exportacao |
| EF Migrations aplicaveis em banco vazio | Parcial avancado | `dotnet-ef` 8.0.5 esta instalado e `HardenPlatformSso` foi gerada sem deriva de tabelas; falta executar toda a cadeia em banco vazio controlado. O banco oficial nao recebeu `database update` porque seu historico EF esta vazio e reaplicaria a migration inicial sobre tabelas existentes |
| Secrets fora de arquivos versionados | Parcial | Revisar arquivos versionados e manter segredos apenas em env/User Secrets/cofre |
| CI/CD com gate de cobertura, Sonar e migrate | Parcial avancado | Gate xUnit/coverlet de 70% adicionado e Docker API depende dele; faltam Sonar, migrate seguro e health-check pos-deploy |
| Compose dev sobe tudo | Nao validado nesta apuracao | Executar `docker compose up --build` em janela propria |
| Desktop auto-update funcional | Parcial | Publicar release desktop real e validar atualizacao em maquina cliente |
| Acesso local ao Desktop WPF | Concluido no servidor de desenvolvimento | Em 2026-07-14 foi criado e validado `GenesisGest.Net Desktop.lnk` na Area de Trabalho do usuario, apontando para o executavel Release oficial dentro de `D:\Nexum Altivon\NexumAltivon.com` |
| Gestao de usuarios e RBAC no Desktop | Concluido no escopo funcional | `AccessManagementWindow` executa cadastro, alteracao, ativacao/desativacao, troca de perfil, perfis corporativos, permissoes e matriz RBAC pela API oficial. Build completo passou com 0 avisos/erros; ensaio autenticado confirmou HTTP 200/201, persistencia, 9 eventos de auditoria e limpeza final sem residuos controlados |
| Gestao superior de infraestrutura no Desktop | Pendente programado | Implementar sob `SuperAdmin` a gestao de conexoes de bancos, endpoints, servicos e integracoes sensiveis, com mascaramento, autenticacao recente, validacao antes de salvar, auditoria e aplicacao controlada sem reinicio silencioso |
| Unificacao de `nexum_altivon` e `genesis_bd` | Pendente programado, sem alteracao fisica autorizada nesta etapa | Cumprir UBD-01 a UBD-08 com inventario, linhagem, modelo canonico, ensaio, reconciliacao, backup, reversao e corte controlado; somente entao remover a segunda connection string |
| Reestruturacao visual do Desktop WPF | Pendente programado | Manter identidade e cores, aplicar fundo fume translucido com efeito de cristal/acrylic que preserve a visibilidade controlada da Area de Trabalho, bordas arredondadas e janela sempre contida na area util do monitor. Remover barras de rolagem estruturais horizontais/verticais e reduzir a poluicao do dashboard: manter navegacao lateral e menus suspensos, com um unico grafico central selecionavel e conteudo operacional organizado sem sobreposicao |
| Portal dinamico administravel | Parcial avancado | Em 2026-07-14 foi implementada a biblioteca de midia em `SiteMediaLibrary.js`, endpoints protegidos `/api/site/midias`, tabela MySQL `site_midias`, validacao binaria PNG/JPEG/WebP, dimensoes, limite de 8 MB, tenant, soft-delete e concorrencia por `RowVersion`. O `wwwroot` oficial passou a ser servido pelo processo da API na unica porta 5010. Ensaio autenticado real confirmou login 200, upload 201, listagem do ID, arquivo local/publico 200 com 11.080 bytes, atualizacao 200, concorrencia 409, exclusao 204, `is_deleted=1`, logout 200 e limpeza final sem linha ou arquivo controlado residual. Em 2026-07-17, `Salvar configuracao publica` foi corrigido e validado com confirmacao da API, payload publico, MySQL e persistencia apos reinicio. React compilou com codigo 0; ainda faltam evolucao visual ampla, administracao especifica de imagens de loja/produto e validacao Chrome dos fluxos responsivos publicados |
| Evolucao visual do portal sem alterar a identidade | Pendente | Revisar Home, catalogo, lojas e areas de produto com hierarquia visual mais forte, imagens reais administraveis, estados de carregamento/erro consistentes e validacao Chrome em desktop e mobile; manter cores, marca, navegacao e linguagem atuais |
| Sugestoes tecnicas Yara 5.5 | Em avaliacao continua | Em 2026-07-14 foram incorporados na janela WPF de Dropshipping os principios uteis de navegacao existente, Grid responsivo, validacao local, API real, retorno de erro e confirmacao de ID/RowVersion. Em 2026-07-17, o relatorio e o patch `Desktop_Workflow_Tabelas` foram reavaliados: o catalogo pode apoiar UBD-01, mas o CRUD MySQL direto e o armazenamento local de credenciais continuam rejeitados porque contornariam API, JWT, RBAC, tenant, auditoria e regras de negocio. Configuracoes sensiveis tambem nao usam outbox offline por dependerem do estado atual do servidor |
| TLS ativo sem HTTP publico puro | Parcial | Porta 443 responde; validar politica completa Cloudflare/nginx |
| AGENTS e OpenAPI | Parcial | `AGENTS.md` atualizado; `/swagger/v1/swagger.json` respondeu 200 no smoke publico |
| Termos proibidos no codigo ativo | Parcial | Blocos commitados nesta auditoria foram varridos; ainda restam Desktop, docs antigos e services legados nao compilados para triagem |
| Header de IP em todos os arquivos | Parcial | Blocos commitados nesta auditoria receberam header; falta varredura completa nos arquivos remanescentes |
| Fluxo cadastro a BI isolado por empresa | Pendente | Executar roteiro ponta a ponta com dados reais |
| Ferramentas ficticias convertidas em reais ou bloqueadas claramente | Em execucao | O login administrativo alternativo fora do banco foi removido; MFA, refresh e logout persistem e revogam estado real. Shopee/Amazon deixam de gravar sucesso falso; marketplace sync Mercado Livre/B2W/Via exige chamada externa; PDF financeiro gera arquivo real; tracking externo e emissao/eventos NF-e/NFC-e bloqueiam sem configuracao oficial; checkout registra falha fiscal, Genesis e notificacao depois da venda sem ocultar a pendencia | Concluir demais achados da secao acima e validar com credenciais reais de cada integracao externa |
| Falha fiscal engolida apos venda | Concluido no escopo do checkout | A venda confirmada preserva o pedido, registra estado fiscal pendente, notificacao e auditoria com referencia/codigo seguro e responde HTTP 202. Se o incidente nao puder ser persistido, a API retorna erro critico com numero do pedido e proibe reenvio automatico. Ensaios de falha e caminho normal foram conferidos no MySQL e integralmente limpos |

## GitHub e Commit

Estado apurado:

- Branch local: `work/delivery-2026-06-13`.
- Base local e remota antes da entrega atual: `fa58e2d feat: iam - operar usuarios e rbac no desktop`.
- Antes dos commits desta entrega, `HEAD`, `origin/work/delivery-2026-06-13` e `origin/main` estavam alinhados sem divergencia no commit `fa58e2d`.
- Endurecimento transacional do checkout: `4b4c3ba fix: checkout - registrar incidentes pos-commit reais`.
- A entrega documental atual consolida a arvore real/meta e as capacidades recuperadas; os hashes remotos devem ser conferidos depois do push.
- Commits atomicos enviados nesta rodada de saneamento: `8d58edb`, `0b8212e`, `a7c23d9`, `5848622`, `fd988d0`, `d766314`, `4918d30`, `3879914`, `937512d`, `404468b`, `6dbf1f0`.
- Publicacao GitHub permanece seletiva e auditada; nao publicar arquivos legados/falsos sem validacao direta no projeto oficial.

## Regra de continuidade

Nao declarar item como concluido sem uma evidencia tecnica associada: build, teste, rota HTTP, consulta de banco, migracao aplicada, arquivo publicado, commit remoto ou validacao visual quando o item for de interface.
