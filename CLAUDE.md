<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7226 Data: 16/08/2026
-->

# GenesisGest.Net - Guia de Execucao Tecnica

## Escopo Oficial

O repositorio operacional oficial desta execucao e `D:\Nexum Altivon\NexumAltivon.com`.
API, backend, frontend, ERP e desktop devem permanecer alocados nesta arvore. Nao devem ser criadas copias paralelas soltas nem dependencias operacionais em pastas acessorias para contornar build, testes ou validacao.

Rotas travadas de desenvolvimento e validacao local:

- Projeto oficial: `D:\Nexum Altivon\NexumAltivon.com`.
- Dados MySQL do e-commerce: `D:\xampp\mysql\data\nexum_altivon`.
- Dados MySQL do ERP GenesisGest: `D:\xampp\mysql\data\genesis_bd`.
- Repositorio online: `https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me`.
- Materiais em `D:\` podem ser consultados como insumo tecnico, incluindo pacotes de atualizacao e patches existentes, mas nao podem virar dependencia operacional fora da arvore oficial do projeto.

## Decisoes Travadas

- A API oficial e a Minimal API em `NexumAltivon_Back-End/API/Program.cs`.
- Controllers MVC legados nao devem voltar para o pipeline ativo.
- A solution oficial e `NexumAltivon.ERP.sln`, compilando API, ERP raiz, biblioteca ERP e Desktop.
- Arquivos novos ou editados devem preservar o header de propriedade intelectual no topo, usando a sintaxe valida de cada formato.
- Segredos, banco de dados, repositorios e configuracoes operacionais permanecem privados.

## Inicializacao do servidor

- A unica tarefa oficial da API e `NexumAltivonApi24h`.
- A tarefa deve executar como `SYSTEM`, `ServiceAccount`, nivel `Highest`, com exatamente um gatilho de boot e nenhum gatilho de logon.
- A tarefa usa exclusivamente `http://127.0.0.1:5010`, `StartWhenAvailable`, `RestartCount=999`, `RestartInterval=PT1M`, `ExecutionTimeLimit=PT0S` e `IgnoreNew`.
- O supervisor oficial e `scripts/server/iniciar-api-oficial-24h.ps1`; ele reinicia o processo filho e impede instancias duplicadas por mutex global.
- `NexumAltivonMySQL`, `Cloudflared` e `Schedule` devem permanecer como servicos Windows automaticos.
- Instalacao, publicacao e reparo devem passar por `scripts/server/atualizar-api-oficial-5010.ps1`. Nao criar outro runtime, tarefa ou porta para contornar falha.
- A verificacao obrigatoria e `scripts/server/validar-api-oficial-24h-task.ps1`; falha local ou publica impede declarar o deploy concluido.

## Configuracoes Sophia SOL - Veracidade e Eficiencia Operacional

- Configuracoes Sophia SOL consolida integralmente o bloqueio Sophia 5.5 e todos os seus adendos; nenhuma exigencia anterior e removida.
- Priorizar recursos locais existentes, ferramentas gratuitas, cache seguro, validacao incremental e automacao auditavel para obter o maior resultado com o menor custo operacional.
- Economia de tempo ou custo nunca autoriza reduzir cobertura, suprimir validacoes, ocultar falhas, fabricar sucesso ou substituir codigo e operacao reais.
- Em duvida tecnica, consultar este guia, o codigo oficial e fontes tecnicas primarias; consultar o responsavel somente quando faltar decisao de negocio essencial que nao possa ser inferida com seguranca.
- Toda entrega deve apresentar resultado concreto, codigo real, endpoint real, persistencia real e validacao executada. Relatos sem evidencia tecnica nao encerram requisito.
- Ferramentas, funcoes e telas prometidas por paineis legados ou documentos anteriores devem ser mapeadas, comparadas com o painel React oficial e consolidadas somente como implementacao real no projeto oficial.
- Nenhuma ferramenta do painel pode ser tratada como finalizada quando existir apenas interface figurativa, botao sem acao, redirecionamento decorativo, retorno local fabricado, mensagem de sucesso sem gravacao no banco ou integracao sem chamada externa valida.
- Quando uma ferramenta depender de marketplace, gateway, SEFAZ, logistica, banco, storage ou outro provedor externo, a validacao deve usar credencial real de homologacao ou teste oficial disponivel no ambiente. Se a credencial nao existir, a API deve bloquear a operacao com erro rastreavel, sem sucesso falso.
- A arquitetura aceitavel para cada ferramenta do painel deve conter, no minimo: model ou contrato de dados, service real, endpoint Minimal API, chamada frontend pelo `src/services/api.js`, tela React oficial, persistencia/consulta no banco quando aplicavel e smoke test HTTP.
- O painel legado desativado deve ser usado apenas como mapa de intencao funcional. Ele nao deve voltar como HTML operacional nem substituir o painel React oficial.
- Publicacao e validacao devem confirmar o mesmo comportamento no runtime local `127.0.0.1:5010`, na API publica `https://api.nexumaltivon.com.br` e no portal publicado `https://nexumaltivon.com.br`, quando a funcao for exposta publicamente.
- O repositorio GitHub deve receber apenas codigo auditado a partir do projeto oficial vigente em `D:\Nexum Altivon\NexumAltivon.com`, nunca o contrario sem validacao previa.

## Parametros adicionais consolidados

- O Desktop WPF deve manter paridade administrativa verificavel por tabela: cada tabela que participa de um processo administrativo deve ter formulario proprio, endpoint correspondente, permissao RBAC especifica, tenant, auditoria, validacao, concorrencia quando aplicavel, persistencia e releitura. Um formulario nao pode misturar lancamentos de contas a pagar com contas a receber ou substituir individualidades de modulos diferentes.
- A triagem dos materiais LURCO432 deve seguir a ordem operacional definida: arquivos soltos da raiz de `D:\Atualizacoes e Desenvolvimentos Uteis\Projeto Nexum Acompanhamento LURCO432`, depois `Patch_GenesisGest.Net_Parte_01`, depois `Patch_GenesisGest.Net_Parte_02`. Cada item deve ser classificado como integrado ao projeto oficial, reservado para avaliacao posterior ou descartado por incompatibilidade, sem copia operacional paralela.
- Sugestoes Yara e demais patches somente podem entrar no codigo depois de conferencia de contrato, dependencias, seguranca, persistencia e compatibilidade com a API Minimal oficial, o banco oficial, o frontend ou o Desktop que receber a alteracao. Material de consulta nunca vira dependencia de runtime.
- Testes automatizados, dados de validacao e credenciais de homologacao devem permanecer isolados do fluxo produtivo. Eles servem para comprovar comportamento e nao podem substituir os dados reais, alterar silenciosamente o banco operacional ou ser apresentados como entrega funcional.
- Midias, banners, logos, cores e perfis publicos expostos no portal devem usar configuracao persistida no banco e leitura pela API oficial. Arquivos em `wwwroot` podem ser assets reais do projeto, mas a escolha, ordem, visibilidade, link e identidade comercial devem ser administraveis sem alterar codigo quando o requisito for dinamico.
- Cada arquivo novo ou editado deve atualizar o identificador de versao do cabecalho de forma monotonicamente verificavel. O valor deve refletir a entrega real e nunca usar data ou marcador indefinido.
- A persona Sophia Sol define criterio de trabalho e resposta; nao autoriza alegacoes de capacidades, validacoes, credenciais, consenso ou resultados que nao possam ser demonstrados por codigo, comando, resposta HTTP, persistencia e releitura.

## Estado da Fase A

- `NexumAltivon.ERP.sln` inclui `NexumAltivon.API`, `NexumAltivon_ERP`, `NexumAltivon.Desktop` e o projeto raiz `NexumAltivon.ERP`.
- Os 14 controllers MVC foram removidos do pipeline ativo e preservados dentro do proprio backend em `NexumAltivon_Back-End/API/Legacy/Controllers`, excluidos do build por `NexumAltivon.API.csproj`.
- A pasta ativa `NexumAltivon_Back-End/API/Controllers` foi removida do fluxo operacional.
- As pastas legadas de raiz `Controllers`, `Services`, `Models`, `DTOs`, `Data` e `Configurations` nao existem mais neste checkout.
- A Minimal API possui rotas diretas para:
  - `GET /api/lojas/{id}`
  - `GET /api/pedidos/{id}`
  - `GET /api/relatorios/vendas`
  - `GET /api/financeiro/faturamento`
  - `POST /api/auth/refresh`
  - `POST /api/auth/logout`

## Validacao Executada

- `dotnet build NexumAltivon_Back-End/NexumAltivon.API.csproj -c Release`: sucesso, 0 erros, 0 avisos.
- `dotnet build NexumAltivon.ERP.sln -c Release`: sucesso, 0 erros, 0 avisos.
- `dotnet vstest NexumAltivon_Back-End/bin/Tests/Release/net8.0/NexumAltivon.API.Tests.dll --TestAdapterPath:NexumAltivon_Back-End/bin/Tests/Release/net8.0`: sucesso, 37 aprovados, 0 falhos, 0 ignorados, 13,0622 segundos. O comando `dotnet test` com o SDK 10.0.302 travou na descoberta do runner; o runner direto e validado deve ser usado até a instalação de um SDK 8 compatível.
- A fonte atualizada compila com 0 erros/0 avisos e os testes permanecem em 37 aprovados. A publicação elevada ainda esta pendente: o servico oficial em `http://127.0.0.1:5010` continua no binario anterior e retornou 404 para `GET/PUT/DELETE /api/erp/empresas/1`. O atualizador nao foi executado porque a sessao de desenvolvimento nao obteve permissao administrativa; nenhuma rota auxiliar foi criada.
- O instalador oficial foi reforcado na versao `1.1.5.7220`: durante a troca em 5010, se restar um `dotnet.exe` sem linha de comando identificavel mas o `/health` oficial responder HTTP 200, o processo remanescente e encerrado antes do registro da tarefa `NexumAltivonApi24h`. Qualquer outro processo ou health diferente de 200 interrompe a instalacao com erro rastreavel.
- API oficial local: `/health` retornou 200; `/api/tenants` retornou 401 sem JWT; `/api/governanca/tenants` retornou 404 por ser rota legada inexistente.
- `dotnet build NexumAltivon.Desktop/NexumAltivon.Desktop.csproj -c Release --no-restore`: sucesso, 0 erros, 0 avisos.
- API oficial local: `GET /api/categorias` retornou 200 com payload real; `POST /api/categorias` sem JWT retornou 401 e nao gravou dados.

## Estado da Fase B

- Foi criada a base `Sys_AuditableEntity` para novas entidades transacionais com `Id` UUID, `TenantId`, `RowVersion`, auditoria de criacao/alteracao e soft-delete.
- `NexumDbContext` agora aplica filtro global `IsDeleted == false`, indice `tenant_id/is_deleted`, row version como token de concorrencia e preenchimento automatico de auditoria para entidades que herdam de `Sys_AuditableEntity`.
- Foi criado `TenantContext`/`TenantResolver`, resolvendo tenant por claim `tenant_id`, header `X-Tenant-Id`, query `tenant_id` ou `TenantSettings:DefaultTenantId`.
- Foi criada a entidade `Tenant` em `sys_tenants`.
- Foram adicionados endpoints Minimal API:
  - `GET /api/tenants`
  - `GET /api/tenants/{id}`
  - `POST /api/tenants`
  - `PUT /api/tenants/{id}`
  - `DELETE /api/tenants/{id}`
- Foi implementado MFA TOTP para usuarios internos:
  - `POST /api/auth/mfa/enable`
  - `POST /api/auth/mfa/verify`
  - Login passa a exigir `mfaCode` quando `mfa_enabled` estiver ativo para o usuario.
- `EnsureOperationalSchemaAsync` cria/ajusta `sys_tenants`, `token_refresh` e colunas MFA de `usuarios` quando ha conexao com o banco.

### Limite tecnico honesto da Fase B

A infraestrutura de auditoria e multitenancy esta implementada e compilando, mas as entidades legadas ainda usam `int Id`.
A migracao completa de todas as tabelas transacionais legadas para herdar `Sys_AuditableEntity` deve ser feita em fase propria, com script de backfill `LegacyId`, validacao de dados e janela de migracao.

## Auditoria LURCO432 - etapa 1

- A raiz de `D:\Atualizações e Desenvolvimentos Uteis\Projeto Nexum Acompanhamento LURCO432` possui 29 arquivos de relatorio e acompanhamento, sem codigo C# ou XAML operacional para copiar para o projeto oficial.
- `Patch_GenesisGest.Net_Parte_01` possui 68 arquivos fisicos: 33 C#, 33 XAML, um relatorio Markdown e um ZIP. O relatorio declara 33 contratos de rota, mas a comparacao com `Program.cs` encontrou zero correspondencias exatas.
- O formulario de multiempresa do patch usava `/api/governanca/tenants`; essa rota retornou `404` no servidor oficial 5010. O contrato real existente e `/api/tenants`, protegido por `Admin`, com GUID, persistencia em `sys_tenants` e exclusao logica.
- O contrato de tenants foi adaptado no Desktop oficial em `TenantManagementWindow.xaml/.cs` e `DesktopApiClient.cs`, usando somente `/api/tenants`. A validacao HTTP no servidor 5010 retornou `/health=200`, `/api/tenants=401` sem JWT e `/api/governanca/tenants=404`, confirmando protecao e ausencia da rota legada.
- `Patch_GenesisGest.Net_Parte_02` possui 538 arquivos fisicos: 269 C#, 267 XAML, um relatorio Markdown e um ZIP. O material permanece em triagem por dominio, tabela, campos e regras; a ausencia de correspondencia literal de rota nao e motivo para descarte.
- A divergencia entre contagem declarada no relatorio e contagem fisica deve permanecer registrada: o relatorio informa 539 itens e 270 formularios, enquanto a varredura fisica encontrou 538 arquivos e 269 arquivos C#.
- Na Parte 02, a varredura dos 267 code-behinds encontrou 267 endpoints declarados nos cabecalhos e 270 chamadas genericas sem implementacao HTTP verificavel. Esse resultado determina a necessidade de reimplementacao por contrato, nao descarte automatico. A equivalencia semantica `ConciliacaoBancariaWindow` ja possui contrato oficial em `/api/financeiro/conciliacao` e esta implementada no Desktop em `ConciliacaoFinanceiraWindow.xaml/.cs`, com DTOs, GET/POST, validacao financeira, concorrencia, persistencia e releitura.
- A equivalencia semantica `CategoriasMarcasWindow` foi viabilizada no Desktop oficial como `CategoriasWindow.xaml/.cs`: o menu agora usa formulario individual, `DesktopApiClient` desserializa o `CategoriaDto` real retornado pelo POST, e a tela confirma a criacao somente apos GET de releitura. Como a API oficial possui apenas GET/POST para categorias, nao foram criados botoes de edicao ou exclusao sem contrato correspondente.
- A equivalencia semantica `CentrosCustoLucroWindow` foi viabilizada no Desktop oficial como `CentrosCustoWindow.xaml/.cs`, conectada ao menu `Centros de custo`. O formulario usa GET/POST/PUT/DELETE de `/api/centros-custo`, os tipos aceitos pela API e a exclusao logica de `fin_centros_custo`; a operacao so e informada como concluida apos releitura.
- A equivalencia semantica de catalogo/servicos foi viabilizada no Desktop oficial como `ItensServicoWindow.xaml/.cs`, conectada ao menu `Produtos e servicos`. O formulario usa os 16 campos do contrato `vnd_itens`, consulta filtrada e GET/POST/PUT/DELETE de `/api/itens-servico`, com desativacao por `itm_ativo = 0` e releitura posterior.
- O contrato de empresas do grupo foi ampliado na API oficial: `GET/POST /api/erp/empresas` continuam ativos e foram adicionados `GET/PUT/DELETE /api/erp/empresas/{id:int}`. O PUT valida CNPJ e codigo duplicados, atualiza os campos fiscais e registra `UpdatedAt`; o DELETE preserva o registro e marca `Ativa=false`. O Desktop usa esses contratos em `EmpresasGrupoWindow.xaml/.cs` com botoes Salvar e Desativar e releitura de confirmacao.
- O item de menu `Workflow e aprovações` foi conectado a `WorkflowAprovacoesWindow.xaml/.cs`. A tela opera definições, instâncias, histórico e transições com os contratos oficiais `/api/workflows/definicoes`, `/api/workflows/instancias` e `/api/workflows/instancias/{id}/transicoes`; toda escrita exige resposta da API e releitura antes da confirmação visual.
- A regra de integracao passa a ser: para cada formulario LURCO, localizar a tabela e os campos reais no schema oficial, identificar o endpoint Minimal API equivalente ou implementa-lo no projeto, criar a tela WPF individual, aplicar permissao, validacao, persistencia e releitura, e somente depois marcar o formulario como integrado. Arquivos de patch continuam como fonte de consulta e nao entram como dependencia de runtime.

## Proximas Fases

## Compilacao economica oficial

- Para uma alteracao exclusiva do Desktop WPF, executar powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\compilar-validar-oficial.ps1 -Modo Wpf. A saida isolada evita sobrescrever o runtime ativo e o tempo observado para o bloco contabil foi 18,49 segundos, com 0 erros e 0 avisos.
- Para uma alteracao exclusiva da API, executar o mesmo script com -Modo Api. O comando usa --no-restore, compilacao compartilhada e caminho intermediario isolado.
- Para fechamento de setor ou release, executar o mesmo script com -Modo Solucao. A compilacao completa continua obrigatoria no fechamento e foi validada neste bloco em 1m06,52s, com API, ERP, Desktop e testes, 0 erros e 0 avisos.
- O runner direto dotnet vstest deve ser executado contra o DLL de testes produzido na mesma saida da compilacao integrada. Neste bloco foram aprovados 37 testes, com 0 falhas e 0 ignorados.
- Uma falha de compilacao deve interromper o comando com codigo diferente de zero. Nenhuma saida isolada e runtime de validacao pode ser publicada como runtime oficial sem a etapa de publicacao correspondente.

## FICO - fechamento contabil

- A API oficial usa GET e POST em /api/financeiro/contabil/fechamento, com autorizacao Financeiro, periodo normalizado para o primeiro dia do mes, upsert por empresa e competencia, transacao ReadCommitted, releitura antes do commit e confirmacao somente depois do commit.
- O Desktop possui a tela individual FechamentoPeriodoWindow, acessivel em Financeiro > Fechamento contabil, com empresa, competencia, bloqueio, observacoes, consulta, registro e trilha de auditoria de cnt_fechamentos.
- O material LURCO432 FechamentoPeriodoWindow.xaml/.xaml.cs foi classificado como utilizado somente depois da compilacao WPF, compilacao API e compilacao integrada. O contrato legado /api/contabil/fechamento e a exclusao sem endpoint nao foram incorporados.
- A compilacao integrada deste bloco aprovou 37 testes com 0 falhas e 0 ignorados. A publicacao no runtime oficial 5010 nao foi declarada neste registro.

## Governanca de progresso, versionamento e arvore WPF/PDV

- Cada bloco de trabalho deve ser reportado ao proprietario com setor, tabela, endpoint, formulario, permissao, persistencia, releitura, compilacao e teste. O relato frequente nao equivale a aceite comercial.
- O marco de promocao e aceite externo permanece bloqueado ate superar 260 tabelas operacionais, cada uma com contrato real, formulario ou consulta correspondente quando aplicavel, RBAC, auditoria, persistencia e evidencia de funcionamento. Tabelas sem todos esses gates nao entram no numerador.
- A versao central do produto e mantida em Directory.Build.props. Os assemblies, artefatos de release, cabecalhos dos arquivos alterados e documentos de entrega devem apontar para o identificador da mesma entrega.
- A arvore de menu e formularios do Desktop/PDV deve ser atualizada em docs/ARVORE-MENU-WPF-2026-08-16.md a cada inclusao de janela, contrato ou bloqueio. O mapa deve distinguir tela operacional, tela de consulta, pendencia e bloqueio externo.
- O mapa de desenvolvimento tambem fica acessivel pelo menu Sistema > Mapa de desenvolvimento WPF/PDV. Ele e somente leitura e exibe a mesma evidencia documental versionada no GitHub.
- Progresso, commit e publicacao devem partir do checkout oficial D:\Nexum Altivon\NexumAltivon.com. Materiais LURCO/Yara sao fontes auditaveis, nunca origem reversa nem dependencia de runtime.

1. Migrar entidades transacionais legadas para `Sys_AuditableEntity` com backfill seguro.
2. Completar workflow e SSO sem placeholders.
3. Criar testes xUnit e cobertura real dos services.
4. Ativar observabilidade, Redis, Hangfire, backup/restore e staging.
5. Avancar modulo por modulo dos setores 02 a 08 com Model, Service, Endpoint e Tela.
