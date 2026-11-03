Check-List Com atualização em 19/07/2026.


<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Intervenção de consolidação e planejamento: Yara 5.5
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão de referência: 1.1.5.7190
 * Documento gerado: 2026-07-19
-->

# Fluxo Mestre Clínico e Estrutural GenesisGest.Net v1.1.5

Apuração consolidada: 2026-07-19.

Destino solicitado para instalação local:

```text
C:\Users\Rodrigo Costa\Desktop\Fluxo\Resultado
```

## Declaração de Escopo e Preservação

Este arquivo é uma **atualização documental e organizacional** do `CHECKLIST-CLINICO-GENESISGEST-2026-07-10.md`. Nenhum arquivo da raiz do projeto, banco MySQL, migration, código-fonte, configuração de runtime ou repositório foi modificado nesta intervenção.

O bloqueio operacional **Yara 5.5** permanece mantido. As regras de evidência, ausência de sucesso falso, fidelidade à API oficial, RBAC, tenant, auditoria, concorrência, tratamento de erro e validação operacional continuam obrigatórias.

Fontes integralmente analisadas e preservadas nos anexos deste documento:

1. `CHECKLIST-CLINICO-GENESISGEST-2026-07-10.md`;
2. `ARVORE-REAL-E-META-GENESISGEST-2026-07-18.md`;
3. `MATRIZ-PARIDADE-BANCO-WPF-2026-07-18.md`.

## Resultado Esperado

Organizar toda a evolução existente e futura em um fluxo único, setorizado e verificável, no qual:

- uma tarefa-pai ativa só libera outra tarefa-pai depois de fechar todas as suas ramificações;
- cada ramificação possui abertura, dependências, critério de aceite, evidência e fechamento explícitos;
- tarefas concluídas permanecem registradas em ordem documental de conclusão;
- tarefas parciais não são promovidas a concluídas por aparência, referência literal ou mensagem visual;
- toda capacidade nova recebe identificação `A-xxx`, setor, prioridade, dependência, percentual e gate;
- formulários, relatórios, gráficos e automações são tratados como entregáveis formais;
- integrações externas somente concluem após retorno aceito e identificador oficial persistido;
- o ecossistema final cobre as funções de uma organização multiempresa, multicanal e multinacional sem substituir a estrutura presente sem necessidade comprovada.
## Diagnóstico Consolidado

| Achado | Resultado factual | Implicação no fluxo |
|---|---|---|
| Medição histórica | DoD declarada como `7/31 = 22,6%`; bloqueadores `4/14 = 28,6%`. | Os percentuais históricos são preservados como linha de base oficial. |
| Inconsistência de denominador | A seção física de Definition of Done contém `37` linhas, não `31`. | Antes de recalcular o percentual oficial, a governança deve decidir quais linhas são DoD principal, subgate ou evidência. |
| Inventário Banco-WPF | 269 objetos: 257 tabelas e 12 views; 102 referências literais na API e 35 no Desktop. | Referência literal é visibilidade, não prova de CRUD ou homologação. |
| Cobertura literal | API: 37.9%; Desktop: 13.0% dos objetos inventariados. | A lacuna deve ser tratada pelo WPFDB-02, sem criar CRUD MySQL direto no Desktop. |
| Janelas impeditivas | `CorporateMasterDataWindow`, `FiscalRoutingWindow` e `ModuleWorkspaceWindow` ainda possuem operação genérica; quatro fluxos usam contingência a homologar. | Substituir por contratos e formulários de domínio antes de considerar a paridade concluída. |
| Estrutura real | API .NET, React, Desktop WPF, MySQL, runtime 5010 e exposição pública existem. | A estratégia correta é evolução incremental, não reinício do projeto. |
| Integrações externas | Fiscal, tracking, marketplaces e Yara possuem bloqueios claros sem credencial oficial. | Bloqueio rastreável é correto; não deve ser convertido em sucesso aparente. |
| Banco canônico | UBD-01 a UBD-08 permanecem programados. | A alteração física dos schemas fica para a última onda, após estabilizar consumidores. |

## Regra de Evidência e Escala de Percentual

A conclusão oficial continua binária: um item só entra no numerador quando atende integralmente ao gate. Para ordenar o trabalho, este documento também usa uma **maturidade de planejamento**, sem substituir a conclusão oficial:

| Maturidade | Interpretação |
|---:|---|
| 0% | Pendente ou não validado. |
| 25% | Em avaliação, estrutura apenas identificada ou dependência externa ainda não iniciada. |
| 50% | Parcial/em andamento; existe implementação real, mas faltam gates obrigatórios. |
| 75% | Parcial avançado; fluxo principal existe, faltando homologação, cobertura ou ramificação específica. |
| 100% | Concluído no escopo declarado, com evidência associada. |

Percentuais setoriais são uma linha de base de planejamento derivada das evidências dos três arquivos. Eles não substituem testes no runtime nem autorizam promover itens para `Concluído`.

## Medição Atual e Reconciliável

| Programa | Base | Percentual/estado | Regra |
|---|---|---|---|
| Definition of Done oficial histórico | 7/31 | 22,6% | Métrica preservada do documento original; não recalculada retroativamente. |
| Definition of Done físico reconciliável | 37 linhas | Não oficial | A tabela contém 37 linhas; requer decisão formal de denominador e aceite antes de substituir 31. |
| Bloqueadores B1–B14 estritos | 4/14 | 28,6% | Somente itens integralmente aceitos entram no numerador original. |
| UBD-01 a UBD-08 | 0/8 estritos | 0,0% | Programa planejado; nenhuma migração física autorizada. |
| WPFDB-01 a WPFDB-10 | 1/10 integral | 10,0% estrito | Maturidade técnica estimada maior, mas somente WPFDB-03 está declarado concluído no escopo. |
| WPF-VIS-01A a 01F | 0/6 integral | 0,0% estrito | Contenção parcial não conclui a frente global. |
| Maturidade setorial média do novo fluxo | 22 setores | 44.5% | Indicador de planejamento em escala de evidência; não substitui a DoD estrita. |

## Máquina de Estados do Desenvolvimento

```text
PENDENTE
  -> PRONTO (dependências e Definition of Ready atendidas)
  -> ATIVO (uma tarefa-pai por setor; prioridade global explícita)
  -> RAMIFICAÇÃO_ATIVA (somente filhos necessários ao fechamento do pai)
  -> EM_VALIDACAO (código congelado para o gate daquele pacote)
  -> CONCLUIDO (todos os filhos fechados e evidências aceitas)
  -> BLOQUEADO_EXTERNO (código recusa a operação e não registra sucesso)
  -> REABERTO (somente por evidência de regressão, mudança aprovada ou falha de gate)
```

### Limites obrigatórios de trabalho em andamento

1. Um único fluxo vertical crítico global por vez.
2. No máximo uma tarefa-pai ativa por setor.
3. Uma ramificação só abre se for necessária para fechar a tarefa-pai ativa.
4. Ramificações técnicas comuns devem ser reutilizadas; não criar frentes duplicadas de layout, autenticação, auditoria ou banco.
5. Uma tarefa bloqueada externamente pode liberar trabalho interno independente, mas permanece aberta e não concluída.
6. Nenhuma próxima onda começa enquanto houver sucesso falso, perda de correlação, risco de tenant ou escrita sem auditoria no fluxo atual.

### Definition of Ready

- objetivo e escopo delimitados;
- tabelas, entidades, endpoints, permissões e telas impactadas identificados;
- dependências e riscos conhecidos;
- critérios de aceite e dados de teste definidos;
- backup e rollback definidos quando houver persistência;
- credenciais externas disponíveis ou bloqueio externo formalizado.

### Definition of Done operacional

- formulário/canal oficial quando aplicável;
- endpoint/serviço oficial e DTO validado;
- persistência/consulta real e releitura independente;
- RBAC, tenant, auditoria, soft-delete e concorrência aplicados quando cabíveis;
- idempotência e tratamento de erro explícitos;
- build, testes, smoke HTTP e consulta de banco;
- evidência anexada, dados controlados removidos e `ResidualRows = 0` quando o ensaio criar registros;
- documentação, versão, release e rollback atualizados.

## Ordem de Conclusão já Registrada

| Data/onda | Marco consolidado |
|---|---|
| 2026-07-10 | Base clínica, solution/build, smoke de rotas críticas e critérios de evidência consolidados. |
| 2026-07-12 | Falsos sucessos de Shopee/Amazon removidos; rotas reais ou bloqueios rastreáveis para marketplaces, logística e fiscal; PDFs reais iniciados. |
| 2026-07-13 | Runtime oficial 5010, saneamento de HTML/scripts legados, frete sem fallback silencioso e validações públicas. |
| 2026-07-14 | Acesso Desktop, dashboard sem série fabricada, Marketing WPF/React, OpenAPI e rotas FICO verificadas. |
| 2026-07-17 | MFA, refresh, logout, workflow, RBAC Desktop, chaves Yara/Sophia, rotação MySQL, configurações públicas e segredos endurecidos. |
| 2026-07-18 | Checkout transacional, incidentes pós-commit, financeiro WPF, compras, fornecedores, migrações e cobertura de Services validados. |
| 2026-07-19 | Clientes/endereços, portal dinâmico e matriz Banco-WPF atualizados; novo fluxo mestre criado sem alterar a raiz. |

Os 94 registros da seção original `Intervencao Validada Nesta Etapa` foram preservados, receberam `A-001` a `A-094` e permanecem na ordem documental de registro. O conteúdo integral de cada evidência está no Anexo A.

## Sequência Evolutiva Obrigatória

| Onda | Tarefa-pai liberadora | Conteúdo | Fechamento obrigatório antes da próxima onda |
|---:|---|---|---|
| 0 | Reconciliar a base clínica | Denominador DoD, catálogo `A-xxx`, estado, dependências, evidências e percentuais. | Documento aprovado e nenhum item original perdido. |
| 1 | WPFDB-02 + segurança transversal | Linhagem dos 269 objetos, contratos, permissões, tenant, auditoria, concorrência e eliminação de operações genéricas. | Matriz 100% classificada e nenhuma tela confirmando gravação em domínio errado. |
| 2 | Fechar fluxos verticais já avançados | Compras, fornecedores, clientes, financeiro, portal, CRM/marketing e checkout. | Cada fluxo com API, WPF/React, banco, auditoria, testes e limpeza. |
| 3 | Estoque, WMS, fiscal e logística | Recebimento, estoque, expedição, documentos fiscais, tracking e financeiro correlacionado. | Venda e compra completas até contabilidade e BI. |
| 4 | Manufatura, HCM, jurídico, projetos e ESG | Setores ainda com baixa cobertura, executados um a um. | Cada setor com formulários, relatórios, gráficos, automações e homologação. |
| 5 | BI, SRE, DevSecOps e experiência global | Observabilidade, DR, ITSM, dashboards, acessibilidade, mobile/PWA e auto-update. | SLOs, restore, staging, regressão visual e operação 24h comprovados. |
| 6 | UBD-01 a UBD-08 | Inventário, linhagem, modelo canônico, migração, reconciliação, cutover e desativação. | Um schema canônico, rollback provado e estabilidade observada. |
| 7 | Aceite final | DoD, bloqueadores, CAP, WPFDB, WPF-VIS e indicadores setoriais. | 100% dos gates aceitos, sem sucesso falso, sem órfãos funcionais e com árvore final refletida no runtime. |

## Catálogo Mestre A-xxx — Tarefas Existentes e Novas

A numeração abaixo é transversal e não substitui os identificadores originais. Ela permite ordenar tarefas de famílias diferentes sem perder `UBD`, `WPFDB`, `WPF-VIS`, `B`, `CAP`, `DOD`, ferramentas legadas e evidências históricas.

- `A-001` a `A-094`: evidências históricas registradas;
- `A-095` a `A-213`: tarefas e critérios já existentes nos arquivos analisados;
- `A-214` a `A-323`: pacotes setoriais novos para completar o ecossistema;
- subitens `F`, `R`, `G` e `AU`: formulários, relatórios, gráficos e automações do pacote setorial.

| A-ID | ID original | Classe | Setor | Processo/tarefa | Status | Maturidade | Dependências | Gate/resultado |
|---|---|---|---|---|---|---|---|---|
| A-001 | HIST-001 | Evidência | Histórico validado | Seeds auditaveis do `NexumDbContext` | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `Loja`, `Transportadora`, `Marketplace` e `DropshippingConfig` passaram a declarar valores obrigatorios de auditoria e flags operacionais em `HasData` |
| A-002 | HIST-002 | Evidência | Histórico validado | Validacao operacional da etapa | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `dotnet build NexumAltivon.ERP.sln -c Release --no-restore -m:1 /nr:false /p:UseSharedCompilation=false` passou com 0 avisos e 0 erros apos remover a base de validacao da solution |
| A-003 | HIST-003 | Evidência | Histórico validado | Republicacao oficial 5010 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Validacao elevada em 2026-07-13T00:53:39 confirmou task `NexumAltivonApi24h` `Running`, usuario `SISTEMA`, `RunLevel Highest`, PID `10928`, `dotnet.exe NexumAltivon.API.dll`, `/health`, `/health/db`, `/health/db/genesis… |
| A-004 | HIST-004 | Evidência | Histórico validado | Republicacao oficial 5010 complementar | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Validacao elevada em 2026-07-13T23:45:31 confirmou task `NexumAltivonApi24h` `Running`, usuario `SISTEMA`, `RunLevel Highest`, PID `12876`, `dotnet.exe NexumAltivon.API.dll`, `/health`, `/health/db`, `/health/db/genesis… |
| A-005 | HIST-005 | Evidência | Histórico validado | Plataforma SSO publicada | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Em 2026-07-17, a API oficial foi republicada na unica porta 5010. A tarefa `NexumAltivonApi24h` permaneceu como `SYSTEM`, `ServiceAccount`, gatilho exclusivo de boot, reinicio automatico e processo invisivel. Health loc… |
| A-006 | HIST-006 | Evidência | Histórico validado | Persistencia segura de autenticacao | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `usuarios` e `clientes` passaram a armazenar somente SHA-256 do refresh token e data de expiracao; `mfa_secret` foi ampliado e recebe apenas valor protegido por Data Protection com chave persistente cifrada por DPAPI da… |
| A-007 | HIST-007 | Evidência | Histórico validado | Fluxo administrativo MFA | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Ensaio controlado confirmou login local 200, hash/expiracao no MySQL, MFA enable 200, segredo cifrado no banco, codigo invalido 400, verify 200, reuso do mesmo passo TOTP 400, reinicio da API, login MFA pelo dominio pub… |
| A-008 | HIST-008 | Evidência | Histórico validado | Fluxo de cliente | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Ensaio controlado pelo dominio publico confirmou login 200 com par de tokens, hash/expiracao no MySQL, rotacao 200, replay do token anterior 401, logout 200 e refresh revogado 401. Um cliente autenticado recebeu 401 ao… |
| A-009 | HIST-009 | Evidência | Histórico validado | Gate dos Services ativos | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `NexumAltivon.API.Tests.csproj` permanece separado do projeto Web e usa `obj/bin` exclusivos. `dotnet test` executou `29/29` casos com `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos no namespace ativo `N… |
| A-010 | HIST-010 | Evidência | Histórico validado | Gate dos Services 1.1.5.7183 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `dotnet test` executou `37/37` casos em 2026-07-18; cobertura dos Services confirmou `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos com threshold de linhas em 70% |
| A-011 | HIST-011 | Evidência | Histórico validado | Rascunho fiscal WPF | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `ManualNfeWindow` removeu URL fixa e `HttpClient` paralelo, usa JWT e DTOs do `DesktopApiClient`, valida documentos, UFs, CFOP e valores, grava pela API e somente confirma salvamento apos reler e comparar o JSON persist… |
| A-012 | HIST-012 | Evidência | Histórico validado | Configuracoes isoladas por tenant | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A migration `20260718213331_ScopeConfigurationKeysByTenant` removeu os indices legados somente por `chave` e criou unicidade `(tenant_id, chave)`. O dump previo possui SHA-256 `A9F6B273A1B9B46B708EB489934660C55E6FBD957B… |
| A-013 | HIST-013 | Evidência | Histórico validado | Publicacao 1.1.5.7183 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | API e Desktop compilados com versao `1.1.5.7183`. A API oficial foi republicada somente em `127.0.0.1:5010`; tarefa SYSTEM com gatilho exclusivo de boot, banco e Cloudflared automaticos, health local/publico e os dois b… |
| A-014 | HIST-014 | Evidência | Histórico validado | Isolamento de Compras por tenant | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A migration `20260718220042_HardenProcurementTenancy` preservou as contagens `3/2/2/2/2/2/2` das sete tabelas afetadas, preencheu tenant e RowVersion sem valores invalidos, manteve zero exclusoes e criou os sete indices… |
| A-015 | HIST-015 | Evidência | Histórico validado | Gate dos Services 1.1.5.7184 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `dotnet test` executou `37/37` casos; cobertura dos Services confirmou `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos. A solution Release compilou API, testes, ERP, Desktop e raiz com 0 erros e 0 avisos |
| A-016 | HIST-016 | Evidência | Histórico validado | Publicacao 1.1.5.7184 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | DLL oficial publicada com versao `1.1.5.7184` e SHA-256 `145E756625ED75402636B4C3B40C6C9296F40A291759D9C896C6372775366DE3`. Tarefa SYSTEM possui um gatilho de boot e nenhum de logon; MySQL e Cloudflared estao `Running/A… |
| A-017 | HIST-017 | Evidência | Histórico validado | Compras tipadas no Desktop 1.1.5.7185 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `ProcurementWindow` usa o JWT e o `TerminalProfile` da sessao, DTOs proprios para painel, solicitacao, cotacao, pedido, status e entrada, e somente confirma cada comando depois de nova leitura de `/api/compras/painel`.… |
| A-018 | HIST-018 | Evidência | Histórico validado | Integridade transacional de Compras | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A API exige maquina de estados, aprovacao antes do pedido/recebimento, bloqueio concorrente `FOR UPDATE`, rejeicao explicita de quantidade acima do saldo, documento de entrada nao duplicado, tenant em todas as consultas… |
| A-019 | HIST-019 | Evidência | Histórico validado | Ensaio autenticado de Compras 1.1.5.7185 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `scripts/server/validar-compras-ponta-a-ponta.ps1` gerou backups dos dois schemas, autenticou usuario controlado, confirmou solicitacao `5`, cotacao `3`, pedido `4`, duas entradas, dois movimentos, estoque final `4`, fi… |
| A-020 | HIST-020 | Evidência | Histórico validado | Publicacao 1.1.5.7185 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | API republicada somente em `127.0.0.1:5010`; tarefa SYSTEM manteve um gatilho de boot e nenhum de logon, MySQL e Cloudflared permaneceram `Running/Auto`, e health local/publico, Nexum, Genesis e configuracao publica res… |
| A-021 | HIST-021 | Evidência | Histórico validado | Fornecedores tipados no Desktop 1.1.5.7186 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `SupplierManagementWindow` substitui a rota generica para Fornecedores e opera lista, cadastro, edicao, contatos e auditoria pela API oficial. Cada gravacao exige JWT, respeita tenant, usa `RowVersion` e somente apresen… |
| A-022 | HIST-022 | Evidência | Histórico validado | Integridade de fornecedores e contatos | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A API ganhou releitura individual de fornecedor, validacoes de documento, e-mail, UF, CEP, loja, comissao, prazo e status, conflito 409 por versao antiga e auditoria na mesma transacao. `md_fornecedor_contatos` recebeu… |
| A-023 | HIST-023 | Evidência | Histórico validado | Ensaio autenticado de fornecedores 1.1.5.7186 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `scripts/server/validar-fornecedores-ponta-a-ponta.ps1` passou pela API local com fornecedor/contato `6/1` e pelo dominio publico com `7/2`. Nos dois percursos autenticou usuario controlado, criou e atualizou fornecedor… |
| A-024 | HIST-024 | Evidência | Histórico validado | Gate da entrega 1.1.5.7186 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A solution Release compilou os cinco projetos com 0 erros e 0 avisos; `37/37` testes passaram. A medicao dos Services permaneceu em `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos; o frontend concluiu `np… |
| A-025 | HIST-025 | Evidência | Histórico validado | Publicacao operacional 1.1.5.7186 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | API oficial republicada somente em `127.0.0.1:5010`; tarefa `NexumAltivonApi24h` permanece `SYSTEM/ServiceAccount`, com um gatilho de boot, nenhum de logon, reinicio automatico, processo invisivel, MySQL e Cloudflared `… |
| A-026 | HIST-026 | Evidência | Histórico validado | Clientes tipados no Desktop 1.1.5.7187 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `CustomerManagementWindow` substitui a rota generica de Clientes e opera lista, cadastro, edicao, enderecos, arquivamento e auditoria pela API oficial. Cada gravacao exige JWT, respeita tenant, usa `RowVersion` e soment… |
| A-027 | HIST-027 | Evidência | Histórico validado | Integridade de clientes e enderecos | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A API administrativa possui leitura individual, criacao exclusiva sem senha, atualizacao com concorrencia, validacao estrita de nome, e-mail, CPF/CNPJ, tipo, status e campos de contato. Enderecos validam agregado pai, C… |
| A-028 | HIST-028 | Evidência | Histórico validado | Ensaio autenticado de clientes 1.1.5.7187 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `scripts/server/validar-clientes-ponta-a-ponta.ps1` passou pela API local com cliente/endereco `41/25` e pelo dominio publico com `42/26`. Nos dois percursos um `Vendedor` autenticado recebeu HTTP 403, enquanto `SuperAd… |
| A-029 | HIST-029 | Evidência | Histórico validado | Publicacao operacional 1.1.5.7187 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | API oficial republicada somente em `127.0.0.1:5010`; tarefa `NexumAltivonApi24h` validada como `SYSTEM/ServiceAccount`, com um gatilho de boot, nenhum de logon, reinicio automatico e processo invisivel. MySQL e Cloudfla… |
| A-030 | HIST-030 | Evidência | Histórico validado | Gate da entrega 1.1.5.7187 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A solution Release compilou os cinco projetos com 0 erros e 0 avisos; `37/37` testes da API passaram. O frontend concluiu o build CRACO otimizado com codigo de saida `0` e gerou `main.7a8fb961.js`; o `postbuild-pages.js… |
| A-031 | HIST-031 | Evidência | Histórico validado | Integridade documental de rotas | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | O termo traduzido exibido na captura nao existia no checkout oficial nem em `origin/main`. A imagem apresentava traducao automatica simultanea de metodos e rotas, enquanto o fonte versionado mantinha `GET`, `POST`, `/he… |
| A-032 | HIST-032 | Evidência | Histórico validado | Integracoes de notificacao | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `NotificacaoService` deixou de engolir recusas do WhatsApp, passou a validar URL, telefone, mensagem e configuracoes obrigatorias, propaga recusas de SendGrid/WhatsApp e recebe `CancellationToken` em toda E/S. `Assisten… |
| A-033 | HIST-033 | Evidência | Histórico validado | Publicacao do gate e Services | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Em 2026-07-17T05:30, a API foi republicada somente na porta 5010. Tarefa `NexumAltivonApi24h` validada como `SYSTEM`, boot exclusivo, PID `2516`; MySQL e Cloudflared `Running/Auto`; health local/public, dois bancos, con… |
| A-034 | HIST-034 | Evidência | Histórico validado | Separacao Yara/Sophia publicada | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Yara usa exclusivamente `/api/assistentes/yara/mensagem` e recebe contexto somente leitura de lojas/produtos publicaveis do MySQL. Sophia usa `/api/assistentes/sophia/mensagem` com policy `Admin`. Configuracao de chaves… |
| A-035 | HIST-035 | Evidência | Histórico validado | Bloqueio explicito da IA | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Como nenhuma chave OpenAI foi fornecida, Yara local e publica retornaram HTTP 503 com causa rastreavel; Sophia local e publica retornaram 401 sem JWT; nenhuma resposta de IA foi fabricada |
| A-036 | HIST-036 | Evidência | Histórico validado | Limpeza das validacoes SSO | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | As contas controladas de usuario e cliente foram removidas fisicamente; consultas finais retornaram zero registros residuais |
| A-037 | HIST-037 | Evidência | Histórico validado | Painel administrativo React | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `Dashboard.js` ajustado no commit `0b8212e` para nao exibir sucesso visual com estado local fabricado em `Site & Banners` e rascunho fiscal manual; apos salvar, a tela usa retorno/releitura real da API antes de confirma… |
| A-038 | HIST-038 | Evidência | Histórico validado | Salvar configuracao publica | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Em 2026-07-17, `Dashboard.js` passou a exibir sucesso ou erro na propria tela, bloquear clique duplo durante a gravacao e comparar cada valor retornado pela API. `PUT /api/site/configuracoes` deixou de responder `ok` ge… |
| A-039 | HIST-039 | Evidência | Histórico validado | Central de IA do site | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `GlobalActions.js` nao inventa resposta local quando a API falha, removeu o seletor Yara/Sophia e envia o portal apenas para Yara. No dashboard, Sophia usa rota distinta e protegida; a mensagem de erro exibe a causa ret… |
| A-040 | HIST-040 | Evidência | Histórico validado | Frete/logistica sem sucesso falso | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `Program.cs` ajustado no commit `4918d30`: `/api/frete/cotar` e `/api/logistica/roteamento` retornam 502 se Melhor Envio estiver configurado e falhar, recusar ou nao devolver cotacao utilizavel; tabela interna oficial s… |
| A-041 | HIST-041 | Evidência | Histórico validado | HTML estatico legado do frontend | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `NexumAltivon_Front-End/index.html` ajustado no commit `937512d`: removido HTML legado com Formspree, links sem destino, termos de lancamento futuro e CNPJ inexistente; arquivo agora redireciona para o portal oficial pu… |
| A-042 | HIST-042 | Evidência | Histórico validado | Script legado de CRM | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `nexum-integration.js` e `public/nexum-integration.js` ajustados no commit `6dbf1f0`: formulario legado so confirma cadastro quando `/api/crm/leads` retorna `Dados.Id`; sem confirmacao de persistencia, exibe erro operac… |
| A-043 | HIST-043 | Evidência | Histórico validado | Smoke publico painel/portal | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Endpoints principais consumidos pelo painel e portal foram verificados em `https://api.nexumaltivon.com.br`: rotas publicas retornaram 200, rotas protegidas retornaram 401, nenhuma retornou 404 |
| A-044 | HIST-044 | Evidência | Histórico validado | Smoke publico 2026-07-14 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Local e publico responderam sem 404 para `/api/lojas/1`, `/api/pedidos/1`, `/api/relatorios/vendas`, `/api/financeiro/faturamento`, `/api/dashboard/resumo`, `/api/clientes`, `/api/fornecedores`, `/api/fiscal/pedidos` e… |
| A-045 | HIST-045 | Evidência | Histórico validado | Frete em runtime 5010 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `POST /api/frete/cotar` local e publico retornou 200 com mensagem `Cotacao operacional calculada pela tabela interna oficial.`, fonte `Tabela interna oficial` e sem texto antigo de fallback silencioso |
| A-046 | HIST-046 | Evidência | Histórico validado | IA em runtime 5010 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Em 2026-07-17T06:14, Yara local/publica retornou 503 sem chave, Sophia local/publica retornou 401 sem JWT, configuracao retornou 401 sem `SuperAdmin` e a rota ambigua anterior retornou 410. Swagger confirmou as tres rot… |
| A-047 | HIST-047 | Evidência | Histórico validado | Portal publicado | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Bundle publico `/static/js/main.5963bdf6.js` verificado em 2026-07-14 sem `Em Breve`, `formspree`, `Entraremos em contato` e `Recebi sua mensagem`; `/nexum-integration.js` publicado contem validacao de ID real do CRM |
| A-048 | HIST-048 | Evidência | Histórico validado | OpenAPI em runtime local | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `/swagger/v1/swagger.json` respondeu 200 em API local com o build atual |
| A-049 | HIST-049 | Evidência | Histórico validado | Alias FICO razao | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `/api/financeiro/contabil/razao` respondeu 401 sem token, sem 404 |
| A-050 | HIST-050 | Evidência | Histórico validado | Alias FICO conciliacao | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `/api/financeiro/contabil/conciliacao` respondeu 401 sem token, sem 404 |
| A-051 | HIST-051 | Evidência | Histórico validado | Alias FICO DRE | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `/api/financeiro/contabil/dre` respondeu 401 sem token, sem 404 |
| A-052 | HIST-052 | Evidência | Histórico validado | Alias FICO fechamento | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `/api/financeiro/contabil/fechamento` respondeu 401 sem token, sem 404 |
| A-053 | HIST-053 | Evidência | Histórico validado | Publicacao atual | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `https://api.nexumaltivon.com.br` respondeu 200 para Swagger JSON e 401 sem token para os aliases FICO, sem 404 |
| A-054 | HIST-054 | Evidência | Histórico validado | Runtime oficial | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | API publicada em `D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api` |
| A-055 | HIST-055 | Evidência | Histórico validado | Configuracao privada oficial | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api.env.ps1` criado fora do Git, sem dependencia de pasta recuperada externa |
| A-056 | HIST-056 | Evidência | Histórico validado | Usuario MySQL de runtime | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `nexum_api_24h` validado nos schemas `nexum_altivon` e `genesis_bd` com privilegios operacionais minimos, incluindo `CREATE VIEW` para schema incremental |
| A-057 | HIST-057 | Evidência | Histórico validado | CRM Marketing operacional | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Segmentos e campanhas foram implementados com tela React, endpoints Minimal API, tenant, auditoria, soft-delete, maquina de estados e concorrencia por `RowVersion`; ensaio no runtime oficial retornou POST 201/201, PUT 2… |
| A-058 | HIST-058 | Evidência | Histórico validado | Desktop WPF sem sucesso fabricado | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `ModuleWorkspaceWindow` deixou de confirmar operacao apenas alterando texto local: agora exige resposta persistida da API oficial e referencia do servidor; falha usa outbox real somente quando a contingencia esta ativa.… |
| A-059 | HIST-059 | Evidência | Histórico validado | Marketing no Desktop WPF | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `MarketingWindow` adicionada ao menu `Comercial e CRM`; autentica em `/api/auth/login`, mantem JWT somente em memoria e consome os mesmos CRUDs de campanhas/segmentos. Credencial controlada invalida recebeu HTTP 401 e a… |
| A-060 | HIST-060 | Evidência | Histórico validado | Chaves OpenAI no Desktop WPF | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `NetworkSettingsWindow` possui login administrativo, campos e modelos separados para Yara/Sophia e salva pela API somente apos validacao real na OpenAI. A API cifra cada credencial com AES-256-GCM, persiste em `configur… |
| A-061 | HIST-061 | Evidência | Histórico validado | Workflow operacional | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A API Minimal oficial passou a validar estados alcancaveis, transicoes configuradas, entidade da definicao, perfil autorizado e concorrencia. Ensaio autenticado em 2026-07-17 executou 27 verificacoes: criacao e reativac… |
| A-062 | HIST-062 | Evidência | Histórico validado | Gestao administrativa IAM no Desktop | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `AccessManagementWindow` foi integrada ao menu WPF e opera usuarios, perfis corporativos, permissoes e matriz RBAC exclusivamente pela API Minimal oficial, com JWT em memoria, restricao `Admin`/`SuperAdmin`, confirmacao… |
| A-063 | HIST-063 | Evidência | Histórico validado | Integridade estrutural IAM | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | As tabelas legadas `adm_perfis`, `adm_permissoes` e `adm_perfil_permissoes` existiam sem PK, `AUTO_INCREMENT`, unicidade ou FKs. A inicializacao oficial agora interrompe claramente se encontrar tabela sem PK contendo da… |
| A-064 | HIST-064 | Evidência | Histórico validado | Rotacao da credencial MySQL operacional | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `scripts/server/rotacionar-credencial-mysql-runtime.ps1` gera 256 bits por CSPRNG, altera todas as contas encontradas do usuario de runtime, atualiza somente o arquivo privado ignorado pelo Git, valida acesso aos dois s… |
| A-065 | HIST-065 | Evidência | Histórico validado | Referencias societarias encerradas | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Removidas das fontes, seeds, contato, rodape, documentacao e bundle estatico; varredura integral no projeto oficial retornou zero ocorrencias do nome solicitado |
| A-066 | HIST-066 | Evidência | Histórico validado | Checkout transacional e numeracao fiscal | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | O checkout passou a executar a escrita principal pela estrategia de execucao do EF Core, com transacao unica para cliente, pedido, itens, estoque, financeiro e pagamento. A reserva de numero fiscal usa atualizacao atomi… |
| A-067 | HIST-067 | Evidência | Histórico validado | Contas a pagar e receber no Desktop WPF | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A janela generica foi removida dos dois dominios. `ContasPagarWindow` e `ContasReceberWindow` usam contratos separados da API oficial, JWT em memoria, criacao, baixa/recebimento, totalizadores, releitura persistida e tr… |
| A-068 | HIST-068 | Evidência | Histórico validado | Conciliacao bancaria no Desktop WPF | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A rota `Conciliação bancária` deixou de abrir a operacao financeira generica e passou a usar `ConciliacaoFinanceiraWindow`, com filtros, selecao do lancamento pago, estado, referencia bancaria, observacoes, totalizadore… |
| A-069 | HIST-069 | Evidência | Histórico validado | PDFs de contas a pagar e receber | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `GET /api/erp/genesis/financeiro/contas-pagar/relatorio.pdf` e `GET /api/erp/genesis/financeiro/contas-receber/relatorio.pdf` consultam o MySQL com filtro global de tenant/soft-delete e aceitam periodo/status validado.… |
| A-070 | HIST-070 | Evidência | Histórico validado | Endurecimento financeiro Genesis | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `erp_contas_pagar`, `erp_contas_receber` e `erp_fluxo_caixa` receberam tenant, rowversion binaria, timestamps, usuarios de auditoria e soft-delete, com indice `(tenant_id,is_deleted)`. O `GenesisDbContext` preenche usua… |
| A-071 | HIST-071 | Evidência | Histórico validado | Versionamento de entrega | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Linha funcional mantida em `1.1.5`; revisao central atual `1.1.5.7187` aplicada a assembly, arquivo, API, Desktop, scripts e cabeçalhos alterados. O identificador de Data Protection permanece estavel em `GenesisGest.Net… |
| A-072 | HIST-072 | Evidência | Histórico validado | Dependencias da solution | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | A auditoria `dotnet list NexumAltivon.ERP.sln package --vulnerable --include-transitive` identificou `Microsoft.Extensions.Caching.Memory 8.0.0` transitivo no projeto `NexumAltivon_ERP`. O projeto foi alinhado ao EF Cor… |
| A-073 | HIST-073 | Evidência | Histórico validado | Incidentes pos-commit sem sucesso falso | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Falhas de preparacao fiscal, integracao Genesis e notificacao sao persistidas em `notificacoes` e `logs_auditoria` com referencia e codigo seguro. Quando a venda ja foi confirmada, a API retorna HTTP 202 com `StatusFisc… |
| A-074 | HIST-074 | Evidência | Histórico validado | Ensaio controlado do checkout | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Cenario fiscal recusado retornou HTTP 202, gravou estado `Preparacao fiscal falhou`, codigo MySQL e auditoria, sem consumir a numeracao. Cenario normal reservou o numero 7, avancou a empresa para 8, gravou pre-emissao,… |
| A-075 | HIST-075 | Evidência | Histórico validado | Validacao integral da entrega de checkout | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Em 2026-07-18, a solution Release compilou os cinco projetos com 0 avisos e 0 erros; o frontend compilou o bundle `main.301e9934.js`; `29/29` testes passaram com `70,84%` de cobertura de linhas dos Services. A API foi r… |
| A-076 | HIST-076 | Evidência | Histórico validado | Otimizacao segura do publish oficial | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `scripts/server/atualizar-api-oficial-5010.ps1` deixou de repetir o restore quando `obj/project.assets.json` ja existe e publica com `--no-restore`; a ausencia do arquivo interrompe a operacao com instrucao clara. A com… |
| A-077 | HIST-077 | Evidência | Histórico validado | Publicacao Marketing | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Commit `6933f98` enviado para `main`; CI/CD `29306776375` e Pages `29306776372` concluiram com sucesso. Portal respondeu 200 com bundle `main.260894af.js`, contendo `Marketing operacional` e sem a referencia societaria… |
| A-078 | HIST-078 | Evidência | Histórico validado | Rotas de banco ativas | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Runtime oficial usa `127.0.0.1:3309/nexum_altivon` e `127.0.0.1:3309/genesis_bd`; `health/db` e `health/db/genesis` retornaram 200 local e publico |
| A-079 | HIST-079 | Evidência | Histórico validado | Configuracoes versionadas da API | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `API/appsettings.json` e template privado nao carregam mais `192.168.1.72`, porta `3306` ou segredos aparentes; sem env privado, a API falha de forma clara |
| A-080 | HIST-080 | Evidência | Histórico validado | EF Core design-time | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Factory de migrations usa XAMPP local `127.0.0.1:3309` quando nao houver connection string real por ambiente |
| A-081 | HIST-081 | Evidência | Histórico validado | Inicializacao fixa | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Task `NexumAltivonApi24h` instalada e em execucao como `SISTEMA`, `RunLevel Highest`, conforme `runtime-logs/api-24h-task-query.log` gerado pela execucao elevada |
| A-082 | HIST-082 | Evidência | Histórico validado | Porta publica interna | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | API escutando `127.0.0.1:5010`; Cloudflared permanece apontado para esta porta |
| A-083 | HIST-083 | Evidência | Histórico validado | Causa real do 502 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | API nao subia porque a configuracao privada apontava para caminho externo ausente e, depois, faltava `CREATE VIEW` ao usuario de aplicacao |
| A-084 | HIST-084 | Evidência | Histórico validado | Processo atual | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `dotnet.exe NexumAltivon.API.dll` escutando `127.0.0.1:5010`, iniciado por `scripts/server/iniciar-api-oficial-24h.ps1` via task `NexumAltivonApi24h` |
| A-085 | HIST-085 | Evidência | Histórico validado | Validacao da tarefa 24h | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Concluida via log elevado: `TaskState=Running`, `TaskUser=SISTEMA`, `RunLevel=Highest`, `HealthStatus=200`; consultas nao elevadas podem retornar acesso negado/nao encontrado |
| A-086 | HIST-086 | Evidência | Histórico validado | Revisao local de legados | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Nesta apuracao a pasta `Revisao_Exclusao_2026-07-10` nao existe no diretorio oficial; nao foi usada como evidencia |
| A-087 | HIST-087 | Evidência | Histórico validado | GitHub publicado | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Branch `origin/work/delivery-2026-06-13` atualizada ate `07a465e feat: api - alinhar auditoria tenant e fluxos reais` |
| A-088 | HIST-088 | Evidência | Histórico validado | Backup local 2h | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Task `NexumAltivon Backup Local 2h` corrigida de `Y:\...` para `D:\Nexum Altivon\NexumAltivon.com\scripts\backup-nexum-local-2h.ps1`; execucao manual retornou `Último resultado: 0` |
| A-089 | HIST-089 | Evidência | Histórico validado | ERP isolado | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Commit `5611329` ajustou navegacoes nullable e headers no `NexumAltivon_ERP`; `dotnet build NexumAltivon.ERP.sln -c Release` passou com 0 erros e 0 avisos |
| A-090 | HIST-090 | Evidência | Histórico validado | API ativa | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Commit `07a465e` alinhou auditoria tenant, migrations, confirmacao de cadastro, Melhor Envio real e filtros de produto publicavel; `dotnet build NexumAltivon_Back-End\NexumAltivon.API.csproj -c Release` passou com 0 err… |
| A-091 | HIST-091 | Evidência | Histórico validado | Banco local oficial | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | MySQL XAMPP validado em `127.0.0.1:3309`; em 2026-07-18, `nexum_altivon` possui 203 tabelas base e 12 views, e `genesis_bd` possui 48 tabelas base |
| A-092 | HIST-092 | Evidência | Histórico validado | Catalogo publico | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Consulta direta no banco confirmou 91 produtos ativos e 91 produtos publicaveis pelo filtro atual da API |
| A-093 | HIST-093 | Evidência | Histórico validado | API oficial 5010 | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | Task `NexumAltivonApi24h` validada em 2026-07-13T00:53:39 como `Running`, usuario `SISTEMA`, `RunLevel Highest`, processo `dotnet.exe NexumAltivon.API.dll`, PID `10928`, `/health`, `/health/db`, `/health/db/genesis` e `… |
| A-094 | HIST-094 | Evidência | Histórico validado | GitHub oficial atualizado | Evidência histórica registrada | N/A | Preserva a ordem documental do checklist original | `origin/main` e `origin/work/delivery-2026-06-13` alinhados no commit `8d58edb fix: api - limpar base de teste e corrigir seeds auditaveis` |
| A-095 | UBD-01 | Processo | Dados e banco canônico | UBD-01 Inventario | Pendente programado | 0% | Consumidores estáveis e inventário aprovado | Catalogar tabelas, views, colunas, tipos, chaves, indices, FKs, triggers, procedures, events, volume, datas extremas e tamanho nos dois schemas |
| A-096 | UBD-02 | Processo | Dados e banco canônico | UBD-02 Linhagem | Pendente programado | 0% | Consumidores estáveis e inventário aprovado | Mapear cada tabela para entidades, SQL, endpoints, services, telas React/WPF, jobs, relatorios, scripts e integracoes que a leem ou escrevem |
| A-097 | UBD-03 | Processo | Dados e banco canônico | UBD-03 Equivalencias | Pendente programado | 0% | Consumidores estáveis e inventário aprovado | Classificar estruturas exclusivas, equivalentes, duplicadas e conflitantes; maior volume prevalece apenas quando semantica, integridade e atualidade tambem forem comprovadas |
| A-098 | UBD-04 | Processo | Dados e banco canônico | UBD-04 Modelo canonico | Pendente programado | 0% | Consumidores estáveis e inventário aprovado | Definir nomenclatura padronizada, tipos, PK/FK, tenant, auditoria, soft-delete, concorrencia e compatibilidade temporaria para cada dominio |
| A-099 | UBD-05 | Processo | Dados e banco canônico | UBD-05 Migracao homologada | Pendente programado | 0% | Consumidores estáveis e inventário aprovado | Gerar scripts idempotentes, backup verificavel e ensaio em copia controlada sem editar arquivos fisicos `.frm`, `.ibd` ou diretorios internos do MySQL |
| A-100 | UBD-06 | Processo | Dados e banco canônico | UBD-06 Reconciliacao | Pendente programado | 0% | Consumidores estáveis e inventário aprovado | Comparar contagens, somatorios financeiros/fiscais, saldos, documentos, duplicidades, orfaos e hashes de conjuntos antes e depois |
| A-101 | UBD-07 | Processo | Dados e banco canônico | UBD-07 Corte controlado | Pendente programado | 0% | Consumidores estáveis e inventário aprovado | Suspender escrita pelo menor intervalo possivel, aplicar delta final, trocar uma unica connection string, executar smoke ponta a ponta e reverter automaticamente se qualquer gate falhar |
| A-102 | UBD-08 | Processo | Dados e banco canônico | UBD-08 Desativacao | Pendente programado | 0% | Consumidores estáveis e inventário aprovado | Remover o schema antigo somente depois de todos os consumidores, backups, observabilidade e periodo de estabilidade apontarem exclusivamente para o schema canonico |
| A-103 | WPFDB-01 | Processo | Desktop WPF e paridade | WPFDB-01 Inventario | Parcial avancado: auditor reproduzivel inventariou 269 objetos; a classificacao automatica integral ainda exige confirmacao clinica | 75% | API oficial, DTO, RBAC, tenant e auditoria | Catalogar as tabelas de `nexum_altivon` e `genesis_bd` como administrativa, transacional, filha de agregado, integracao, auditoria ou tecnica |
| A-104 | WPFDB-02 | Processo | Desktop WPF e paridade | WPFDB-02 Linhagem | Em andamento: 102 objetos possuem referencia literal na API e 34 no Desktop; a correspondencia EF/DTO sem nome fisico ainda precisa ser reconciliada | 50% | API oficial, DTO, RBAC, tenant e auditoria | Vincular cada tabela administrativa a modulo, entidade, endpoint Minimal API, permissao, formulario WPF e operacoes permitidas |
| A-105 | WPFDB-03 | Processo | Desktop WPF e paridade | WPFDB-03 Contrato oficial | Concluido para o acesso administrativo ao banco: `ManualNfeWindow` migrou para `DesktopApiClient`, nenhuma janela WPF instancia `HttpClient` para a API e nao existe referencia MySQL direta no Desktop. `DesktopAutoUpdateService` permanece isolado para consultar releases externas do GitHub | 100% | API oficial, DTO, RBAC, tenant e auditoria | Proibir acesso MySQL direto no Desktop; toda leitura/escrita deve passar pela API oficial com DTO validado e `CancellationToken` |
| A-106 | WPFDB-04 | Processo | Desktop WPF e paridade | WPFDB-04 Formulario individual | Parcial avancado: `ProcurementWindow` opera solicitacao, cotacao, pedido, aprovacao e entrada; `SupplierManagementWindow` opera fornecedores/contatos; `CustomerManagementWindow` opera clientes/enderecos. As tres usam contratos tipados da API oficial. Os demais cadastros corporativos, roteamento fiscal e workspace modular ainda usam submissao generica e bloqueiam o aceite integral | 75% | API oficial, DTO, RBAC, tenant e auditoria | Entregar formulario profissional especifico para cada processo administrativo, sem tela generica usada como conclusao funcional |
| A-107 | WPFDB-05 | Processo | Desktop WPF e paridade | WPFDB-05 Separacao de dominios | Parcial avancado: contas a pagar, contas a receber e conciliacao bancaria possuem janelas, DTOs, comandos, validacoes e persistencia separados; contas a pagar/receber exportam PDFs proprios; tesouraria e demais dominios permanecem pendentes | 75% | API oficial, DTO, RBAC, tenant e auditoria | Contas a pagar, contas a receber, conciliacao, tesouraria, fiscal, estoque e demais dominios devem possuir comandos, validacoes e persistencia separados |
| A-108 | WPFDB-06 | Processo | Desktop WPF e paridade | WPFDB-06 Padrao transacional | Parcial | 50% | API oficial, DTO, RBAC, tenant e auditoria | Cada formulario deve conter contexto, codigo, tenant, status, acoes, cabecalho, itens/servicos, financeiro, anexos, auditoria, totalizadores e validacao em tempo real quando aplicavel |
| A-109 | WPFDB-07 | Processo | Desktop WPF e paridade | WPFDB-07 Seguranca | Parcial | 50% | API oficial, DTO, RBAC, tenant e auditoria | Aplicar JWT em memoria, RBAC, tenant, reautenticacao para acao sensivel e mascaramento de segredos |
| A-110 | WPFDB-08 | Processo | Desktop WPF e paridade | WPFDB-08 Integridade | Parcial avancado: contas a pagar/receber e Compras rejeitam duplicidade/excesso e releem o registro persistido. Fornecedores/contatos e clientes/enderecos exigem `RowVersion`, rejeitam versao antiga com HTTP 409 e so confirmam no WPF depois de nova leitura. Tres janelas ainda chamam `SubmitOperationAsync`; quatro arquivos WPF ainda referenciam `LocalOutboxService`, incluindo o PDV cuja contingencia precisa ser homologada separadamente antes de qualquer remocao | 75% | API oficial, DTO, RBAC, tenant e auditoria | Aplicar concorrencia, idempotencia, tratamento de erro HTTP, releitura persistida e nenhuma confirmacao apenas visual |
| A-111 | WPFDB-09 | Processo | Desktop WPF e paridade | WPFDB-09 Relacionamentos | Parcial avancado: contatos sao filhos do fornecedor selecionado e enderecos sao filhos do cliente selecionado; ambos aplicam tenant, concorrencia, soft-delete e auditoria. Os demais agregados administrativos ainda precisam ser reconciliados | 75% | API oficial, DTO, RBAC, tenant e auditoria | Exibir e operar relacoes pai-filho no agregado correto sem gravacao cruzada ambigua entre dominios |
| A-112 | WPFDB-10 | Processo | Desktop WPF e paridade | WPFDB-10 Homologacao | Parcial avancado: contas a pagar/receber, conciliacao, Compras, fornecedores e clientes foram provados na API e banco. Clientes/enderecos confirmou criacao, atualizacao, conflitos 409, soft-delete, seis eventos de auditoria e `ResidualRows = 0` nos percursos local e publico. Os demais formularios permanecem pendentes | 75% | API oficial, DTO, RBAC, tenant e auditoria | Provar formulario por formulario com API, banco, auditoria, permissao, erro controlado, build WPF e limpeza de dados de validacao |
| A-113 | WPF-VIS-01A | Processo | Experiência WPF | WPF-VIS-01A Superficie | Pendente global | 0% | WPFDB-04 a WPFDB-10 por janela | Manter identidade e cores; aplicar fundo fume translucido com efeito de cristal/acrylic, permitindo leitura controlada da Area de Trabalho sem reduzir contraste ou legibilidade |
| A-114 | WPF-VIS-01B | Processo | Experiência WPF | WPF-VIS-01B Geometria | Pendente global | 0% | WPFDB-04 a WPFDB-10 por janela | Padronizar bordas arredondadas discretas, hierarquia, densidade e espacos; eliminar paineis decorativos sem funcao operacional |
| A-115 | WPF-VIS-01C | Processo | Experiência WPF | WPF-VIS-01C Contencao | Parcial: `ProcurementWindow`, `SupplierManagementWindow` e `CustomerManagementWindow` usam layout fixo contido sem `ScrollViewer` externo | 50% | WPFDB-04 a WPFDB-10 por janela | Conter cada janela na area util do monitor, respeitar DPI/resolucao e remover `ScrollViewer` estrutural horizontal/vertical; rolagem interna permanece permitida apenas em grades e campos extensos |
| A-116 | WPF-VIS-01D | Processo | Experiência WPF | WPF-VIS-01D Dashboard | Pendente | 0% | WPFDB-04 a WPFDB-10 por janela | Reduzir poluicao do dashboard, preservar navegacao lateral e menus suspensos e exibir um unico grafico central selecionavel sem sobreposicao |
| A-117 | WPF-VIS-01E | Processo | Experiência WPF | WPF-VIS-01E Consistencia | Pendente | 0% | WPFDB-04 a WPFDB-10 por janela | Aplicar o mesmo shell visual somente depois de homologar contraste, foco, teclado, redimensionamento e custo de renderizacao, sem alterar a identidade funcional de cada formulario |
| A-118 | WPF-VIS-01F | Processo | Experiência WPF | WPF-VIS-01F Homologacao | Pendente | 0% | WPFDB-04 a WPFDB-10 por janela | Validar capturas e operacao real em resolucoes e escalas DPI representativas, sem corte de texto, controles fora da area util ou barras estruturais |
| A-119 | B1 | Processo | Bloqueadores estruturais | Solution unica compila API, ERP e Desktop | Concluido local | 100% | Manter evidência do requisito antecedente | `NexumAltivon.ERP.sln` lista API, ERP, Desktop e projeto raiz; build Release com 0 erros e 0 avisos |
| A-120 | B2 | Processo | Bloqueadores estruturais | Controllers MVC fora do ativo e endpoints criticos sem 404 | Ajustado e publicado na branch de entrega | 100% | Manter evidência do requisito antecedente | API oficial e Minimal API; controllers MVC removidos do projeto da API ativa no commit `4bacfe5`; smoke publico passou nos pontos obrigatorios |
| A-121 | B3 | Processo | Bloqueadores estruturais | Duplicacao massiva da raiz removida do build ativo | Concluido para a raiz legacy | 100% | Manter evidência do requisito antecedente | Diretorios legados da raiz foram removidos do build/versionamento no commit `7ccc668`; solution Release seguiu compilando com 0 erros e 0 avisos |
| A-122 | B4 | Processo | Bloqueadores estruturais | `Sys_AuditableEntity` em 100% das entidades transacionais | Parcial avancado | 75% | Manter evidência do requisito antecedente | Commit `07a465e` aplicou tenant, soft-delete, auditoria central e `row_version` BLOB no `NexumDbContext`. Em 2026-07-18, as sete tabelas transacionais raw SQL de Compras/Estoque receberam tenant, auditoria, soft-delete,… |
| A-123 | B5 | Processo | Bloqueadores estruturais | MFA, refresh-token, tenants e workflows | Parcial avancado | 75% | Manter evidência do requisito antecedente | Workflow, MFA TOTP, refresh-token rotativo e logout foram publicados e homologados no runtime oficial. MFA usa segredo protegido por Data Protection/DPAPI persistente, bloqueia reuso TOTP e sobrevive ao reinicio; refres… |
| A-124 | B6 | Processo | Bloqueadores estruturais | Testes com cobertura minima de 70% em Services | Parcial avancado | 75% | Manter evidência do requisito antecedente | Projeto xUnit isolado do binario de producao foi incluido na solution. Em 2026-07-18, `37/37` casos passaram e os Services ativos atingiram `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos; o CI bloqueia a… |
| A-125 | B7 | Processo | Bloqueadores estruturais | Observabilidade completa | Parcial | 50% | Manter evidência do requisito antecedente | Health e Redis existem; Serilog/OpenTelemetry completos ainda nao foram homologados ponta a ponta |
| A-126 | B8 | Processo | Bloqueadores estruturais | Backup diario e restore-test semanal | Parcial | 50% | Manter evidência do requisito antecedente | Backup local 2h corrigido para `D:\Nexum Altivon\NexumAltivon.com` e executado com resultado 0; restore-test em CI ainda nao comprovado |
| A-127 | B9 | Processo | Bloqueadores estruturais | EF Migrations | Parcial avancado | 75% | Manter evidência do requisito antecedente | Os histories oficiais de Nexum e Genesis foram alinhados aos schemas existentes sem recriar dados. As migrations Nexum `ScopeConfigurationKeysByTenant` e `HardenProcurementTenancy` foram aplicadas em 2026-07-18 com back… |
| A-128 | B10 | Processo | Bloqueadores estruturais | Secrets fora dos arquivos versionados | Parcial avancado | 75% | Manter evidência do requisito antecedente | Runtime local usa `runtime/api-24h/api.env.ps1`, ignorado pelo Git. Em 2026-07-17, a credencial MySQL operacional foi rotacionada com geracao criptografica, atualizacao das contas e do arquivo privado, validacao dos doi… |
| A-129 | B11 | Processo | Bloqueadores estruturais | Documentacao tecnica e OpenAPI | Parcial | 50% | Manter evidência do requisito antecedente | Docs principais existem; `/swagger/v1/swagger.json` foi ajustado para ser registrado fora de Development/Staging |
| A-130 | B12 | Processo | Bloqueadores estruturais | Compose dev com MySQL e Redis | Concluido em arquivo | 100% | Manter evidência do requisito antecedente | `docker/docker-compose.yml` contem MySQL 8, Redis 7 e MinIO |
| A-131 | B13 | Processo | Bloqueadores estruturais | Staging | Parcial | 50% | Manter evidência do requisito antecedente | `docker-compose.staging.yml` e job de staging existem; health-check pos-deploy ainda precisa ajuste |
| A-132 | B14 | Processo | Bloqueadores estruturais | Desktop auto-update | Parcial | 50% | Manter evidência do requisito antecedente | Servico consulta GitHub Releases, baixa pacote e pode iniciar instalador por variavel de ambiente; delta nao comprovado |
| A-133 | LEG-01 | Processo | Paridade do painel e ferramentas | Dashboard | Parcial avancado | 75% | Contrato oficial e persistência real | Em 2026-07-18, a serie semanal estatica e os percentuais/tendencias fabricados foram removidos. `/api/dashboard/resumo?dias=7 |
| A-134 | LEG-02 | Processo | Paridade do painel e ferramentas | Pedidos | Parcial validado | 50% | Contrato oficial e persistência real | Lista e alteracao de status existem; fluxo venda-pagamento-fiscal-logistica-financeiro ainda nao foi homologado integralmente |
| A-135 | LEG-03 | Processo | Paridade do painel e ferramentas | Produtos | Parcial validado | 50% | Contrato oficial e persistência real | CRUD e catalogo existem; falta homologar todas as restricoes de estoque/tenant e marketplace |
| A-136 | LEG-04 | Processo | Paridade do painel e ferramentas | Clientes | Parcial validado | 50% | Contrato oficial e persistência real | CRUD, portal e confirmacao existem; falta teste completo de isolamento e historico operacional |
| A-137 | LEG-05 | Processo | Paridade do painel e ferramentas | Lojas | Parcial | 50% | Contrato oficial e persistência real | Dados aparecem em dashboard/empresas; falta gestao dedicada das seis lojas com gravacao e permissoes |
| A-138 | LEG-06 | Processo | Paridade do painel e ferramentas | Financeiro | Parcial avancado | 75% | Contrato oficial e persistência real | Contas a pagar/receber, PDFs de pagar/receber e conciliacao bancaria possuem contratos e telas WPF proprios, persistencia, tenant e auditoria comprovadas. Razao, fechamento e DRE ainda requerem implementacao/homologacao… |
| A-139 | LEG-07 | Processo | Paridade do painel e ferramentas | Fiscal | Parcial real bloqueado externamente | 50% | Contrato oficial e persistência real | Endpoints bloqueiam sem certificado/provedor; falta certificado e homologacao SEFAZ real |
| A-140 | LEG-08 | Processo | Paridade do painel e ferramentas | Logistica | Parcial real | 50% | Contrato oficial e persistência real | Cotacao interna identificada funciona; tracking externo exige endpoint/token real e retorna bloqueio rastreavel sem eles |
| A-141 | LEG-09 | Processo | Paridade do painel e ferramentas | CRM | Parcial avancado | 75% | Contrato oficial e persistência real | Leads, campanhas e segmentos possuem persistencia comprovada; pipelines, oportunidades, tickets e atividades ainda nao estao completos na API/tela |
| A-142 | LEG-10 | Processo | Paridade do painel e ferramentas | Cupons | Concluido e validado no escopo CRUD/uso publico | 100% | Contrato oficial e persistência real | Em 2026-07-14, no runtime oficial `127.0.0.1:5010`: POST retornou 201 e ID persistido, subtotal abaixo do minimo retornou 400, consulta publica com vigencia `DATE` retornou 200, PUT retornou 200, DELETE desativou com 20… |
| A-143 | LEG-11 | Processo | Paridade do painel e ferramentas | Marketing | Concluido e validado no escopo CRUD/segmentacao e metricas | 100% | Contrato oficial e persistência real | Tela oficial possui campanhas, segmentos, investimento, resultados e transicoes de estado. Ensaio autenticado em 2026-07-14 confirmou criacao 201, atualizacao 200, concorrencia 409, consulta 200, exclusao 204, soft-dele… |
| A-144 | LEG-12 | Processo | Paridade do painel e ferramentas | Marketplaces | Parcial bloqueado por credenciais | 50% | Contrato oficial e persistência real | Tela React envia canal, direcao, entidade e limite para a API real. Mercado Livre/B2W/Via responderam 424 local e publicamente sem tokens, sem registrar sucesso; Shopee/Amazon nao gravam mais sucesso fabricado, mas cone… |
| A-145 | LEG-13 | Processo | Paridade do painel e ferramentas | Dropshipping | Parcial avancado | 75% | Contrato oficial e persistência real | Em 2026-07-14, o schema enum e os tipos Shopify/CJ foram corrigidos; CRUD autenticado confirmou bloqueio 424 sem conector, criacao 201, consulta persistida, atualizacao 200, concorrencia 409, exclusao 204, soft-delete n… |
| A-146 | LEG-14 | Processo | Paridade do painel e ferramentas | Usuarios | Concluido no escopo de gestao administrativa | 100% | Contrato oficial e persistência real | Ensaio autenticado confirmou criacao, edicao, ativacao/desativacao, perfis, permissoes, vinculo e remocao da matriz RBAC com HTTP 200/201; persistencia e 9 auditorias foram comprovadas diretamente no MySQL. `SuperAdmin`… |
| A-147 | LEG-15 | Processo | Paridade do painel e ferramentas | Configuracoes | Concluido no escopo de salvamento; parcial no conjunto do portal | 100% | Contrato oficial e persistência real | Em 2026-07-17, o salvamento foi validado de ponta a ponta com autenticacao, confirmacao chave a chave, leitura administrativa, payload publico, consulta MySQL e reinicio da tarefa oficial. Entradas invalidas retornaram… |
| A-148 | LEG-16 | Processo | Paridade do painel e ferramentas | Auditoria | Parcial validado | 50% | Contrato oficial e persistência real | API local e publica retornaram HTTP 200 e 42 eventos; tela de filtros/detalhe compilou. Cobertura de toda escrita ainda nao foi comprovada |
| A-149 | CAP-01 | Processo | Capacidades corporativas | Cockpit executivo dinamico | Parcial avancado | 75% | Gates comuns de segurança, dados e homologação | Series reais de 7, 30 e 90 dias foram conferidas entre API local, API publica e MySQL em 2026-07-18; concluir com teste de isolamento entre dois tenants e homologacao visual publicada no Chrome |
| A-150 | CAP-02 | Processo | Capacidades corporativas | Venda omnicanal | Parcial | 50% | Gates comuns de segurança, dados e homologação | Correlacao unica de pedido, pagamento, estoque, fiscal, entrega e financeiro |
| A-151 | CAP-03 | Processo | Capacidades corporativas | Compras e abastecimento | Parcial avancado | 75% | Gates comuns de segurança, dados e homologação | Solicitacao, cotacao vinculada, maquina de estados, pedido, entrada parcial/final, estoque, financeiro Nexum, conta a pagar Genesis, tenant e auditoria foram comprovados por HTTP e MySQL com limpeza zero; faltam cotacao… |
| A-152 | CAP-04 | Processo | Capacidades corporativas | WMS | Parcial | 50% | Gates comuns de segurança, dados e homologação | Movimentacao, inventario, kardex, localizacao e transferencia com concorrencia |
| A-153 | CAP-05 | Processo | Capacidades corporativas | Fiscal explicavel | Parcial | 50% | Gates comuns de segurança, dados e homologação | Roteamento, numeracao, XML, chave, protocolo e eventos oficiais visiveis e persistidos |
| A-154 | CAP-06 | Processo | Capacidades corporativas | FICO corporativo | Parcial avancado | 75% | Gates comuns de segurança, dados e homologação | Contas a pagar/receber, baixas, PDFs de pagar/receber e conciliacao bancaria estao integrados entre WPF, API, banco e auditoria; faltam tesouraria, fechamento, razao e DRE integralmente homologados em tela |
| A-155 | CAP-07 | Processo | Capacidades corporativas | Logistica e fulfillment | Parcial | 50% | Gates comuns de segurança, dados e homologação | Cotacao, expedicao, tracking externo, entrega e ocorrencias comprovados |
| A-156 | CAP-08 | Processo | Capacidades corporativas | CRM e atendimento Yara | Parcial bloqueado por credencial | 50% | Gates comuns de segurança, dados e homologação | Obter chave de projeto da OpenAI com faturamento e limite de gasto definidos; validar o motor Yara exclusivamente no portal com streaming, contexto somente leitura e ferramentas autorizadas para catalogo, estoque, preco… |
| A-157 | CAP-09 | Processo | Capacidades corporativas | HCM | Parcial | 50% | Gates comuns de segurança, dados e homologação | Admissao, ponto, folha, beneficios, avaliacao, desligamento e eSocial comprovados |
| A-158 | CAP-10 | Processo | Capacidades corporativas | MES e OPS | Parcial | 50% | Gates comuns de segurança, dados e homologação | OS, apontamento, manutencao e ativos com telas React/WPF especificas |
| A-159 | CAP-11 | Processo | Capacidades corporativas | Paridade Desktop | Parcial avancado | 75% | Gates comuns de segurança, dados e homologação | Contas a pagar/receber, conciliacao bancaria e Compras usam formularios e contratos proprios com releitura; faltam substituir as tres janelas genericas restantes e reconciliar toda a matriz |
| A-160 | CAP-12 | Processo | Capacidades corporativas | Marketplaces e dropshipping | Bloqueado externamente | 35% | Gates comuns de segurança, dados e homologação | Retorno aceito e identificador oficial de cada seller/provedor persistido |
| A-161 | CAP-13 | Processo | Capacidades corporativas | Portal visual dinamico | Parcial avancado | 75% | Gates comuns de segurança, dados e homologação | Perfis estruturados das seis lojas e entidades parceiras, contatos publicos autorizados, cores globais e individuais, banners ordenaveis, rodizio maximo de quatro parceiros e produtos vinculados foram implementados na A… |
| A-162 | CAP-14 | Processo | Capacidades corporativas | Operacao observavel | Parcial | 50% | Gates comuns de segurança, dados e homologação | Logs, traces, metricas, Redis, jobs, backup e restore comprovados |
| A-163 | CAP-15 | Processo | Capacidades corporativas | Banco canonico | Pendente programado | 0% | Gates comuns de segurança, dados e homologação | UBD-01 a UBD-08 concluidos com reconciliacao e reversao |
| A-164 | REAL-01 | Processo | Saneamento de falsos positivos | Shopee marketplace | Falso positivo operacional | 25% | Sem confirmação visual ou persistência simulada | Implementar conector oficial Shopee com assinatura HMAC, credenciais reais, sandbox/producao, teste HTTP e persistencia apenas apos retorno aceito pela API externa |
| A-165 | REAL-02 | Processo | Saneamento de falsos positivos | Amazon marketplace | Falso positivo operacional | 25% | Sem confirmação visual ou persistência simulada | Implementar Amazon SP-API real com OAuth/LWA, seller, marketplace, assinatura AWS4, sandbox/producao e teste HTTP antes de persistir sync |
| A-166 | REAL-03 | Processo | Saneamento de falsos positivos | Sync marketplace Mercado Livre/B2W/Via | Pendente real; agora endpoint existe com chamada externa obrigatoria | 0% | Sem confirmação visual ou persistência simulada | Configurar `MercadoLivre__AccessToken`/`SellerId`, `B2W__EndpointBase`/`AccessToken`/`SellerId`/paths e `Via__EndpointBase`/`AccessToken`/`SellerId`/paths; validar com pedido/produto real de cada seller |
| A-167 | REAL-04 | Processo | Saneamento de falsos positivos | Busca de produtos do `nexum-integration.js` | Quebrado/incompatível com contrato real | 25% | Sem confirmação visual ou persistência simulada | Validar visualmente `/landing.html` e decidir se este legado continua publicado ou se sera substituido integralmente pelo React oficial |
| A-168 | REAL-05 | Processo | Saneamento de falsos positivos | Landing HTML legada | Legado com risco de promessa falsa | 25% | Sem confirmação visual ou persistência simulada | Manter fora da navegacao oficial; se precisar campanha futura, criar tela React/API real antes de publicar |
| A-169 | REAL-06 | Processo | Saneamento de falsos positivos | HTML estatico raiz do frontend | Legado com risco de falsa operacao | 25% | Sem confirmação visual ou persistência simulada | Manter o portal oficial no React publicado; qualquer campanha futura deve nascer integrada a API oficial e banco real |
| A-170 | REAL-07 | Processo | Saneamento de falsos positivos | Script legado `nexum-integration.js` | Legado com risco de sucesso visual sem persistencia comprovada | 25% | Sem confirmação visual ou persistência simulada | Remover definitivamente este script quando nao houver mais HTML legado consumidor; enquanto existir, manter contrato estrito com `/api/crm/leads` |
| A-171 | REAL-08 | Processo | Saneamento de falsos positivos | Admin HTML estatico legado | Legado nao homologado | 25% | Sem confirmação visual ou persistência simulada | Migrar componentes uteis somente para `Dashboard.js` com chamadas API reais e persistencia validada |
| A-172 | REAL-09 | Processo | Saneamento de falsos positivos | PDF de conta a pagar no ERP isolado | Funcionalidade incompleta no ERP isolado | 25% | Sem confirmação visual ou persistência simulada | Validado por `dotnet build NexumAltivon_ERP.csproj -c Release --no-restore` e `dotnet build NexumAltivon.ERP.sln -c Release --no-restore`; pendente apenas teste funcional chamando a tela/endpoint que consome este service |
| A-173 | REAL-10 | Processo | Saneamento de falsos positivos | PDF de DRE no ERP isolado | Funcionalidade incompleta no ERP isolado | 25% | Sem confirmação visual ou persistência simulada | Validado por `dotnet build NexumAltivon_ERP.csproj -c Release --no-restore` e `dotnet build NexumAltivon.ERP.sln -c Release --no-restore`; pendente validar em tela/endpoint consumidor |
| A-174 | REAL-11 | Processo | Saneamento de falsos positivos | Frete Melhor Envio | Falso positivo condicional | 25% | Sem confirmação visual ou persistência simulada | Configurar `MelhorEnvio__AccessToken` e validar cotacao real por CEP/peso; sem token externo, o sistema continua com tabela interna oficial explicitamente identificada |
| A-175 | REAL-12 | Processo | Saneamento de falsos positivos | Logistica tracking externo | Parcial interno, nao tracking externo real | 50% | Sem confirmação visual ou persistência simulada | Configurar endpoint/token reais do provedor logistico e validar HTTP autenticado com codigo de rastreio real |
| A-176 | REAL-13 | Processo | Saneamento de falsos positivos | NF-e/NFC-e SEFAZ | Pendente real de homologacao; agora bloqueado sem sucesso falso | 0% | Sem confirmação visual ou persistência simulada | Configurar certificado real A1/A3 e provedor fiscal homologado (`FiscalSefaz__...`, `NFeIo__...` ou `DFe__...`), executar emissao em homologacao e conferir chave/protocolo gravados no banco |
| A-177 | DOD-01 | Processo | Definition of Done | `dotnet build NexumAltivon.ERP.sln -c Release` sem erros nem avisos | Concluido local | 100% | Todos os gates antecedentes aplicáveis | Manter como gate antes de commit |
| A-178 | DOD-02 | Processo | Definition of Done | `dotnet test` com cobertura >= 70% nos Services | Concluido local e fixado no CI | 100% | Todos os gates antecedentes aplicáveis | `29/29` casos aprovados; cobertura dos Services ativos em `70,84%` de linhas. Manter o teste separado do binario Web e ampliar integracao por modulo sem reduzir o gate |
| A-179 | DOD-03 | Processo | Definition of Done | `npm run build` no frontend | Concluido local | 100% | Todos os gates antecedentes aplicáveis | Manter validacao antes de publicar |
| A-180 | DOD-04 | Processo | Definition of Done | Todos os endpoints faltantes da Secao 7 com payload real | Parcial | 50% | Todos os gates antecedentes aplicáveis | Marketplace sync Mercado Livre/B2W/Via foi implementado na API ativa com bloqueio sem credencial real; completar demais setores e validar via HTTP |
| A-181 | DOD-05 | Processo | Definition of Done | Frontend sem 404 nos endpoints consumidos | Parcial | 50% | Todos os gates antecedentes aplicáveis | Smoke publico dos pontos criticos passou em 2026-07-10: `/health`, `/api/lojas`, `/api/lojas/1`, `/swagger/v1/swagger.json`, aliases FICO e CORS sem 404; falta varredura completa do frontend |
| A-182 | DOD-06 | Processo | Definition of Done | Multitenancy e soft-delete em 100% das entidades | Parcial | 50% | Todos os gates antecedentes aplicáveis | As sete tabelas raw SQL de Compras/Estoque foram migradas e validadas sem perda; continuar as entidades legadas restantes e executar prova autenticada de isolamento entre dois tenants |
| A-183 | DOD-07 | Processo | Definition of Done | MFA TOTP funcional | Concluido no runtime oficial | 100% | Todos os gates antecedentes aplicáveis | Backend, MySQL e portal usam o mesmo contrato. Enable/verify/login foram validados local e publicamente; segredo cifrado sobreviveu ao reinicio, reuso TOTP foi recusado, refresh de usuario/cliente rotacionou atomicament… |
| A-184 | DOD-08 | Processo | Definition of Done | Workflow de aprovacao funcional | Concluido no backend oficial | 100% | Todos os gates antecedentes aplicáveis | CRUD de definicoes e instancias, historico, maquina de estados, RBAC, tenant, soft-delete e atualizacao atomica foram publicados e validados localmente, no dominio Cloudflare e diretamente no MySQL; faltam apenas telas… |
| A-185 | DOD-09 | Processo | Definition of Done | NF-e/NFC-e SEFAZ homologacao | Parcial real, bloqueado por credencial/certificado | 50% | Todos os gates antecedentes aplicáveis | Endpoints de emissao/eventos existem na API ativa e nao registram sucesso sem provedor real; falta configurar certificado/provedor homologado e validar chave/protocolo real |
| A-186 | DOD-10 | Processo | Definition of Done | WMS completo | Parcial | 50% | Todos os gates antecedentes aplicáveis | Completar endpoints/telas e validar movimentacao, inventario, kardex e transferencia |
| A-187 | DOD-11 | Processo | Definition of Done | MES/OPS operacional | Parcial | 50% | Todos os gates antecedentes aplicáveis | Backend existe; telas e fluxo completo ainda precisam validacao. No WPF, a janela generica agora registra solicitacao real na API ou contingencia explicitamente pendente; isto nao substitui as telas especificas de OS, p… |
| A-188 | DOD-12 | Processo | Definition of Done | Hangfire e jobs agendados | Pendente | 0% | Todos os gates antecedentes aplicáveis | Ativar storage e validar execucao de jobs |
| A-189 | DOD-13 | Processo | Definition of Done | Redis integrado e healthcheck verde | Parcial | 50% | Todos os gates antecedentes aplicáveis | Health Redis existe; validar em runtime de producao |
| A-190 | DOD-14 | Processo | Definition of Done | S3/MinIO anexos | Parcial | 50% | Todos os gates antecedentes aplicáveis | Servico e compose existem; validar upload/download assinado |
| A-191 | DOD-15 | Processo | Definition of Done | Serilog e OpenTelemetry | Pendente | 0% | Todos os gates antecedentes aplicáveis | Ativar logs/traces/metrics e validar exportacao |
| A-192 | DOD-16 | Processo | Definition of Done | EF Migrations aplicaveis em banco vazio | Parcial avancado | 75% | Todos os gates antecedentes aplicáveis | `dotnet-ef` 8.0.5 esta instalado e `HardenPlatformSso` foi gerada sem deriva de tabelas; falta executar toda a cadeia em banco vazio controlado. O banco oficial nao recebeu `database update` porque seu historico EF esta… |
| A-193 | DOD-17 | Processo | Definition of Done | Secrets fora de arquivos versionados | Parcial | 50% | Todos os gates antecedentes aplicáveis | Revisar arquivos versionados e manter segredos apenas em env/User Secrets/cofre |
| A-194 | DOD-18 | Processo | Definition of Done | CI/CD com gate de cobertura, Sonar e migrate | Parcial avancado | 75% | Todos os gates antecedentes aplicáveis | Gate xUnit/coverlet de 70% adicionado e Docker API depende dele; faltam Sonar, migrate seguro e health-check pos-deploy |
| A-195 | DOD-19 | Processo | Definition of Done | Compose dev sobe tudo | Nao validado nesta apuracao | 0% | Todos os gates antecedentes aplicáveis | Executar `docker compose up --build` em janela propria |
| A-196 | DOD-20 | Processo | Definition of Done | Desktop auto-update funcional | Parcial | 50% | Todos os gates antecedentes aplicáveis | Publicar release desktop real e validar atualizacao em maquina cliente |
| A-197 | DOD-21 | Processo | Definition of Done | Acesso local ao Desktop WPF | Concluido no servidor de desenvolvimento | 100% | Todos os gates antecedentes aplicáveis | Em 2026-07-14 foi criado e validado `GenesisGest.Net Desktop.lnk` na Area de Trabalho do usuario, apontando para o executavel Release oficial dentro de `D:\Nexum Altivon\NexumAltivon.com` |
| A-198 | DOD-22 | Processo | Definition of Done | Gestao de usuarios e RBAC no Desktop | Concluido no escopo funcional | 100% | Todos os gates antecedentes aplicáveis | `AccessManagementWindow` executa cadastro, alteracao, ativacao/desativacao, troca de perfil, perfis corporativos, permissoes e matriz RBAC pela API oficial. Build completo passou com 0 avisos/erros; ensaio autenticado c… |
| A-199 | DOD-23 | Processo | Definition of Done | Gestao superior de infraestrutura no Desktop | Pendente programado | 0% | Todos os gates antecedentes aplicáveis | Implementar sob `SuperAdmin` a gestao de conexoes de bancos, endpoints, servicos e integracoes sensiveis, com mascaramento, autenticacao recente, validacao antes de salvar, auditoria e aplicacao controlada sem reinicio… |
| A-200 | DOD-24 | Processo | Definition of Done | Paridade integral entre tabelas administrativas e WPF | Pendente programado | 0% | Todos os gates antecedentes aplicáveis | Cumprir WPFDB-01 a WPFDB-10: inventariar e reconciliar 100% da matriz tabela-endpoint-formulario-permissao, separar dominios e homologar cada formulario na API, banco, auditoria e Desktop |
| A-201 | DOD-25 | Processo | Definition of Done | Unificacao de `nexum_altivon` e `genesis_bd` | Pendente programado, sem alteracao fisica autorizada nesta etapa | 0% | Todos os gates antecedentes aplicáveis | Cumprir UBD-01 a UBD-08 com inventario, linhagem, modelo canonico, ensaio, reconciliacao, backup, reversao e corte controlado; somente entao remover a segunda connection string |
| A-202 | DOD-26 | Processo | Definition of Done | Reestruturacao visual do Desktop WPF | `WPF-VIS-01` pendente, com contencao parcialmente aplicada em Compras e Fornecedores | 50% | Todos os gates antecedentes aplicáveis | Executar exclusivamente os criterios `WPF-VIS-01A` a `WPF-VIS-01F` da frente unica acima. O tema acrylic global e a simplificacao do dashboard continuam pendentes; nenhuma iniciativa visual paralela ou duplicada deve se… |
| A-203 | DOD-27 | Processo | Definition of Done | Portal dinamico administravel | Parcial avancado | 75% | Todos os gates antecedentes aplicáveis | Biblioteca de midia, selecao/upload de banners, ordenacao, ativacao, substituicao, perfis publicos normalizados e relacao perfil-produto operam pela API oficial. As seis lojas possuem perfil controlado; fornecedores, pa… |
| A-204 | DOD-28 | Processo | Definition of Done | Motor comercial Yara | Pendente programado, bloqueado por credencial oficial | 0% | Todos os gates antecedentes aplicáveis | Manter Yara exclusivamente no frontend e Sophia exclusivamente no administrativo. Adquirir chave de projeto na plataforma OpenAI, definir limite de gasto, salvar cifrada pelo Desktop sob `SuperAdmin`, validar o modelo n… |
| A-205 | DOD-29 | Processo | Definition of Done | Evolucao visual do portal sem alterar a identidade | Pendente | 0% | Todos os gates antecedentes aplicáveis | Revisar Home, catalogo, lojas e areas de produto com hierarquia visual mais forte, imagens reais administraveis, estados de carregamento/erro consistentes e validacao Chrome em desktop e mobile; manter cores, marca, nav… |
| A-206 | DOD-30 | Processo | Definition of Done | Sugestoes tecnicas Yara 5.5 | Em avaliacao continua | 25% | Todos os gates antecedentes aplicáveis | Em 2026-07-14 foram incorporados na janela WPF de Dropshipping os principios uteis de navegacao existente, Grid responsivo, validacao local, API real, retorno de erro e confirmacao de ID/RowVersion. Em 2026-07-18, o mat… |
| A-207 | DOD-31 | Processo | Definition of Done | TLS ativo sem HTTP publico puro | Parcial | 50% | Todos os gates antecedentes aplicáveis | Porta 443 responde; validar politica completa Cloudflare/nginx |
| A-208 | DOD-32 | Processo | Definition of Done | AGENTS e OpenAPI | Parcial | 50% | Todos os gates antecedentes aplicáveis | `AGENTS.md` atualizado; `/swagger/v1/swagger.json` respondeu 200 no smoke publico |
| A-209 | DOD-33 | Processo | Definition of Done | Termos proibidos no codigo ativo | Parcial | 50% | Todos os gates antecedentes aplicáveis | Blocos commitados nesta auditoria foram varridos; ainda restam Desktop, docs antigos e services legados nao compilados para triagem |
| A-210 | DOD-34 | Processo | Definition of Done | Header de IP em todos os arquivos | Parcial | 50% | Todos os gates antecedentes aplicáveis | Blocos commitados nesta auditoria receberam header; falta varredura completa nos arquivos remanescentes |
| A-211 | DOD-35 | Processo | Definition of Done | Fluxo cadastro a BI isolado por empresa | Pendente | 0% | Todos os gates antecedentes aplicáveis | Executar roteiro ponta a ponta com dados reais |
| A-212 | DOD-36 | Processo | Definition of Done | Ferramentas ficticias convertidas em reais ou bloqueadas claramente | Em execucao | 50% | Todos os gates antecedentes aplicáveis | O login administrativo alternativo fora do banco foi removido; MFA, refresh e logout persistem e revogam estado real. Shopee/Amazon deixam de gravar sucesso falso; marketplace sync Mercado Livre/B2W/Via exige chamada ex… |
| A-213 | DOD-37 | Processo | Definition of Done | Falha fiscal engolida apos venda | Concluido no escopo do checkout | 100% | Todos os gates antecedentes aplicáveis | A venda confirmada preserva o pedido, registra estado fiscal pendente, notificacao e auditoria com referencia/codigo seguro e responde HTTP 202. Se o incidente nao puder ser persistido, a API retorna erro critico com nu… |
| A-214 | S00-P01 | Pacote setorial | Governança do programa, arquitetura e qualidade | Reconciliar o catálogo clínico e fixar um denominador oficial versionado | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-215 | S00-P02 | Pacote setorial | Governança do programa, arquitetura e qualidade | Implantar máquina de estados de tarefas, ramificações e gates | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-216 | S00-P03 | Pacote setorial | Governança do programa, arquitetura e qualidade | Consolidar arquitetura de referência, ADRs e mapa de dependências | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-217 | S00-P04 | Pacote setorial | Governança do programa, arquitetura e qualidade | Padronizar evidências, homologação, release e rollback | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-218 | S00-P05 | Pacote setorial | Governança do programa, arquitetura e qualidade | Operar portfólio, riscos, capacidade e métricas executivas | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-219 | S01-P01 | Pacote setorial | Plataforma, IAM, SSO, tenants e segurança | Concluir isolamento multiempresa em toda entidade e consulta | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-220 | S01-P02 | Pacote setorial | Plataforma, IAM, SSO, tenants e segurança | Completar ciclo de identidade, perfil, permissão e segregação de funções | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-221 | S01-P03 | Pacote setorial | Plataforma, IAM, SSO, tenants e segurança | Formalizar gestão de sessões, MFA, contas técnicas e clientes de API | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-222 | S01-P04 | Pacote setorial | Plataforma, IAM, SSO, tenants e segurança | Implantar gestão segura de segredos, certificados e rotação | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-223 | S01-P05 | Pacote setorial | Plataforma, IAM, SSO, tenants e segurança | Homologar autorização e auditoria por módulo, canal e tenant | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-224 | S02-P01 | Pacote setorial | GRC, auditoria, privacidade e conformidade | Cobrir auditoria imutável e correlacionada em toda operação crítica | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-225 | S02-P02 | Pacote setorial | GRC, auditoria, privacidade e conformidade | Implantar catálogo de políticas, controles, riscos e evidências | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-226 | S02-P03 | Pacote setorial | GRC, auditoria, privacidade e conformidade | Operar privacidade, consentimento, retenção e direitos do titular | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-227 | S02-P04 | Pacote setorial | GRC, auditoria, privacidade e conformidade | Formalizar incidentes, continuidade e recuperação de desastre | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-228 | S02-P05 | Pacote setorial | GRC, auditoria, privacidade e conformidade | Homologar controles regulatórios e revisões independentes | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-229 | S03-P01 | Pacote setorial | Master Data e estrutura multiempresarial | Consolidar entidades legais, empresas, filiais e unidades de negócio | Em andamento | 60% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-230 | S03-P02 | Pacote setorial | Master Data e estrutura multiempresarial | Completar cadastro mestre de pessoas, parceiros e relacionamentos | Em andamento | 60% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-231 | S03-P03 | Pacote setorial | Master Data e estrutura multiempresarial | Completar produtos, serviços, categorias, unidades, preços e lojas | Em andamento | 60% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-232 | S03-P04 | Pacote setorial | Master Data e estrutura multiempresarial | Implantar qualidade, deduplicação e governança de dados mestres | Em andamento | 60% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-233 | S03-P05 | Pacote setorial | Master Data e estrutura multiempresarial | Homologar cadastros em API, React, WPF, auditoria e tenant | Em andamento | 60% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-234 | S04-P01 | Pacote setorial | Financeiro e tesouraria | Concluir contas a pagar, receber, cobrança e crédito ponta a ponta | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-235 | S04-P02 | Pacote setorial | Financeiro e tesouraria | Implantar tesouraria, caixa, bancos, pagamentos e recebimentos | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-236 | S04-P03 | Pacote setorial | Financeiro e tesouraria | Completar orçamento, forecast, centros de custo e aprovações | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-237 | S04-P04 | Pacote setorial | Financeiro e tesouraria | Implantar moedas, câmbio, empréstimos, garantias e posição financeira | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-238 | S04-P05 | Pacote setorial | Financeiro e tesouraria | Homologar relatórios, conciliações, auditoria e fechamento financeiro | Em andamento | 65% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-239 | S05-P01 | Pacote setorial | Contabilidade, controladoria e consolidação | Concluir plano de contas, lançamentos, partidas e razão | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-240 | S05-P02 | Pacote setorial | Contabilidade, controladoria e consolidação | Implantar fechamento por período com checklist e travas | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-241 | S05-P03 | Pacote setorial | Contabilidade, controladoria e consolidação | Completar DRE, balanço, fluxo contábil e demonstrações | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-242 | S05-P04 | Pacote setorial | Contabilidade, controladoria e consolidação | Implantar custos, rateios, centros de lucro e controladoria | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-243 | S05-P05 | Pacote setorial | Contabilidade, controladoria e consolidação | Implantar consolidação, intercompany e eliminações multiempresa | Em andamento | 50% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-244 | S06-P01 | Pacote setorial | Fiscal, tributário e obrigações legais | Concluir motor tributário por empresa, produto, operação e jurisdição | Em andamento | 35% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-245 | S06-P02 | Pacote setorial | Fiscal, tributário e obrigações legais | Homologar emissão e eventos NF-e/NFC-e com chave e protocolo reais | Em andamento | 35% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-246 | S06-P03 | Pacote setorial | Fiscal, tributário e obrigações legais | Implantar CT-e, MDF-e, NFS-e e documentos correlatos | Em andamento | 35% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-247 | S06-P04 | Pacote setorial | Fiscal, tributário e obrigações legais | Completar apuração, livros, SPED e obrigações acessórias | Em andamento | 35% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-248 | S06-P05 | Pacote setorial | Fiscal, tributário e obrigações legais | Homologar contingência, certificados, auditoria e reconciliação fiscal | Em andamento | 35% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-249 | S07-P01 | Pacote setorial | Comercial, vendas, preços e contratos | Concluir cotação, proposta, pedido e aprovação comercial | Em andamento | 58% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-250 | S07-P02 | Pacote setorial | Comercial, vendas, preços e contratos | Completar preços, descontos, promoções e margem por canal | Em andamento | 58% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-251 | S07-P03 | Pacote setorial | Comercial, vendas, preços e contratos | Homologar PDV, checkout, devolução, troca e pós-venda | Em andamento | 58% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-252 | S07-P04 | Pacote setorial | Comercial, vendas, preços e contratos | Implantar metas, comissões, territórios e carteira | Em andamento | 58% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-253 | S07-P05 | Pacote setorial | Comercial, vendas, preços e contratos | Fechar correlação venda-pagamento-estoque-fiscal-logística-financeiro | Em andamento | 58% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-254 | S08-P01 | Pacote setorial | CRM, marketing, atendimento e Yara | Concluir CRM de lead a cliente com atividades, oportunidade e histórico | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-255 | S08-P02 | Pacote setorial | CRM, marketing, atendimento e Yara | Completar campanhas, segmentos, consentimentos e mensuração | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-256 | S08-P03 | Pacote setorial | CRM, marketing, atendimento e Yara | Implantar atendimento, tickets, SLA, conhecimento e satisfação | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-257 | S08-P04 | Pacote setorial | CRM, marketing, atendimento e Yara | Homologar Yara com ferramentas autorizadas e transferência humana | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-258 | S08-P05 | Pacote setorial | CRM, marketing, atendimento e Yara | Medir conversão, qualidade, custo e proteção de dados da jornada | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-259 | S09-P01 | Pacote setorial | E-commerce, portal, lojas e omnicanalidade | Concluir administração das seis lojas e conteúdo público | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-260 | S09-P02 | Pacote setorial | E-commerce, portal, lojas e omnicanalidade | Homologar catálogo, busca, carrinho, checkout e conta do cliente | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-261 | S09-P03 | Pacote setorial | E-commerce, portal, lojas e omnicanalidade | Completar mídia, SEO, navegação, acessibilidade e performance | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-262 | S09-P04 | Pacote setorial | E-commerce, portal, lojas e omnicanalidade | Implantar pós-venda, devolução, recuperação de carrinho e fidelidade | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-263 | S09-P05 | Pacote setorial | E-commerce, portal, lojas e omnicanalidade | Publicar e homologar desktop/mobile com métricas reais | Em andamento | 72% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-264 | S10-P01 | Pacote setorial | Marketplaces, dropshipping e ecossistema de parceiros | Implementar conectores oficiais por marketplace com OAuth/assinatura | Em andamento | 25% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-265 | S10-P02 | Pacote setorial | Marketplaces, dropshipping e ecossistema de parceiros | Completar cadastro, publicação e manutenção de anúncios | Em andamento | 25% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-266 | S10-P03 | Pacote setorial | Marketplaces, dropshipping e ecossistema de parceiros | Implantar importação de pedidos, estoque, preço e eventos | Em andamento | 25% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-267 | S10-P04 | Pacote setorial | Marketplaces, dropshipping e ecossistema de parceiros | Completar dropshipping, roteamento, comissão e SLA de fornecedor | Em andamento | 25% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-268 | S10-P05 | Pacote setorial | Marketplaces, dropshipping e ecossistema de parceiros | Reconciliar taxas, repasses, devoluções e identificadores externos | Em andamento | 25% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-269 | S11-P01 | Pacote setorial | Compras, sourcing e gestão de fornecedores | Concluir requisição e aprovação de necessidade | Em andamento | 74% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-270 | S11-P02 | Pacote setorial | Compras, sourcing e gestão de fornecedores | Completar RFQ, cotação multi-fornecedor e mapa comparativo | Em andamento | 74% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-271 | S11-P03 | Pacote setorial | Compras, sourcing e gestão de fornecedores | Fechar pedido, recebimento parcial/final e três vias | Em andamento | 74% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-272 | S11-P04 | Pacote setorial | Compras, sourcing e gestão de fornecedores | Implantar devolução, não conformidade e avaliação de fornecedor | Em andamento | 74% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-273 | S11-P05 | Pacote setorial | Compras, sourcing e gestão de fornecedores | Homologar anexos, fiscal, financeiro, auditoria e indicadores | Em andamento | 74% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-274 | S12-P01 | Pacote setorial | Estoque, WMS e inventário | Concluir depósitos, endereços, saldos, lotes e seriais | Em andamento | 38% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-275 | S12-P02 | Pacote setorial | Estoque, WMS e inventário | Implantar recebimento, putaway, reserva, separação e expedição | Em andamento | 38% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-276 | S12-P03 | Pacote setorial | Estoque, WMS e inventário | Completar transferências, ajustes e rastreabilidade de movimentos | Em andamento | 38% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-277 | S12-P04 | Pacote setorial | Estoque, WMS e inventário | Implantar inventário geral/cíclico, ABC e acuracidade | Em andamento | 38% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-278 | S12-P05 | Pacote setorial | Estoque, WMS e inventário | Homologar concorrência, custo, auditoria e integração omnicanal | Em andamento | 38% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-279 | S13-P01 | Pacote setorial | Logística, TMS, transporte e comércio exterior | Completar cadastro de transportadoras, tabelas e contratos | Em andamento | 40% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-280 | S13-P02 | Pacote setorial | Logística, TMS, transporte e comércio exterior | Implantar cotação, roteamento, expedição, etiquetas e romaneio | Em andamento | 40% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-281 | S13-P03 | Pacote setorial | Logística, TMS, transporte e comércio exterior | Homologar tracking externo, ocorrências e prova de entrega | Em andamento | 40% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-282 | S13-P04 | Pacote setorial | Logística, TMS, transporte e comércio exterior | Implantar CT-e, MDF-e, cargas e auditoria de frete | Em andamento | 40% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-283 | S13-P05 | Pacote setorial | Logística, TMS, transporte e comércio exterior | Adicionar importação/exportação, aduana e comércio exterior | Em andamento | 40% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-284 | S14-P01 | Pacote setorial | Manufatura, PCP, MES, qualidade, manutenção e ativos | Implantar engenharia de produto, BOM e roteiros | Em andamento | 20% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-285 | S14-P02 | Pacote setorial | Manufatura, PCP, MES, qualidade, manutenção e ativos | Completar MRP, planejamento, ordens e requisições de material | Em andamento | 20% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-286 | S14-P03 | Pacote setorial | Manufatura, PCP, MES, qualidade, manutenção e ativos | Implantar MES, apontamentos, capacidade, paradas e OEE | Em andamento | 20% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-287 | S14-P04 | Pacote setorial | Manufatura, PCP, MES, qualidade, manutenção e ativos | Completar qualidade, inspeções, não conformidade e CAPA | Em andamento | 20% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-288 | S14-P05 | Pacote setorial | Manufatura, PCP, MES, qualidade, manutenção e ativos | Implantar EAM/manutenção, ativos, calibração e custos | Em andamento | 20% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-289 | S15-P01 | Pacote setorial | HCM, folha, ponto, saúde e segurança | Concluir estrutura organizacional, cargos e colaboradores | Em andamento | 22% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-290 | S15-P02 | Pacote setorial | HCM, folha, ponto, saúde e segurança | Implantar admissão, contratos, documentos, dependentes e benefícios | Em andamento | 22% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-291 | S15-P03 | Pacote setorial | HCM, folha, ponto, saúde e segurança | Completar ponto, escalas, afastamentos, férias e banco de horas | Em andamento | 22% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-292 | S15-P04 | Pacote setorial | HCM, folha, ponto, saúde e segurança | Implantar folha, encargos, provisões e eventos oficiais | Em andamento | 22% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-293 | S15-P05 | Pacote setorial | HCM, folha, ponto, saúde e segurança | Completar desempenho, treinamento, sucessão, desligamento e SST | Em andamento | 22% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-294 | S16-P01 | Pacote setorial | Jurídico, contratos, riscos e ESG | Implantar gestão do ciclo de contratos, cláusulas, aditivos e obrigações | Em andamento | 15% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-295 | S16-P02 | Pacote setorial | Jurídico, contratos, riscos e ESG | Completar processos, andamentos, prazos, depósitos e certidões | Em andamento | 15% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-296 | S16-P03 | Pacote setorial | Jurídico, contratos, riscos e ESG | Implantar base jurídica, tipos de ação e conhecimento | Em andamento | 15% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-297 | S16-P04 | Pacote setorial | Jurídico, contratos, riscos e ESG | Integrar riscos, compliance, seguros e continuidade | Em andamento | 15% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-298 | S16-P05 | Pacote setorial | Jurídico, contratos, riscos e ESG | Implantar ESG, indicadores, compromissos e reporte | Em andamento | 15% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-299 | S17-P01 | Pacote setorial | Projetos, serviços, campo, facilities e patrimônio | Implantar portfólio, projetos, WBS, custos e recursos | Em andamento | 10% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-300 | S17-P02 | Pacote setorial | Projetos, serviços, campo, facilities e patrimônio | Completar serviços, contratos, ordens e faturamento | Em andamento | 10% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-301 | S17-P03 | Pacote setorial | Projetos, serviços, campo, facilities e patrimônio | Implantar atendimento de campo, agenda, despacho e mobilidade | Em andamento | 10% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-302 | S17-P04 | Pacote setorial | Projetos, serviços, campo, facilities e patrimônio | Completar facilities, imóveis, reservas e manutenção predial | Em andamento | 10% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-303 | S17-P05 | Pacote setorial | Projetos, serviços, campo, facilities e patrimônio | Integrar patrimônio, despesas, SLA e indicadores | Em andamento | 10% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-304 | S18-P01 | Pacote setorial | BI, dados, MDM, analytics e IA corporativa | Consolidar catálogo de dados, linhagem e qualidade | Em andamento | 48% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-305 | S18-P02 | Pacote setorial | BI, dados, MDM, analytics e IA corporativa | Implantar camada semântica, KPIs e dimensões conformadas | Em andamento | 48% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-306 | S18-P03 | Pacote setorial | BI, dados, MDM, analytics e IA corporativa | Completar dashboards, relatórios e distribuição governada | Em andamento | 48% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-307 | S18-P04 | Pacote setorial | BI, dados, MDM, analytics e IA corporativa | Implantar detecção de anomalia, forecast e IA assistiva controlada | Em andamento | 48% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-308 | S18-P05 | Pacote setorial | BI, dados, MDM, analytics e IA corporativa | Medir custo, qualidade, explicabilidade e uso de dados/modelos | Em andamento | 48% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-309 | S19-P01 | Pacote setorial | Integrações, SRE, DevSecOps, observabilidade e ITSM | Completar catálogo de integrações, contratos, filas e webhooks | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-310 | S19-P02 | Pacote setorial | Integrações, SRE, DevSecOps, observabilidade e ITSM | Homologar logs, métricas, traces, alertas e SLOs | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-311 | S19-P03 | Pacote setorial | Integrações, SRE, DevSecOps, observabilidade e ITSM | Completar CI/CD, segurança, migrations, staging e rollback | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-312 | S19-P04 | Pacote setorial | Integrações, SRE, DevSecOps, observabilidade e ITSM | Homologar backup, restore, DR, jobs, Redis e MinIO | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-313 | S19-P05 | Pacote setorial | Integrações, SRE, DevSecOps, observabilidade e ITSM | Implantar ITSM, incidentes, mudanças, ativos e capacidade técnica | Em andamento | 55% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-314 | S20-P01 | Pacote setorial | Experiência WPF, React, mobile e acessibilidade | Substituir janelas genéricas por formulários de domínio | Em andamento | 42% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-315 | S20-P02 | Pacote setorial | Experiência WPF, React, mobile e acessibilidade | Aplicar shell visual único, contensão, DPI e navegação | Em andamento | 42% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-316 | S20-P03 | Pacote setorial | Experiência WPF, React, mobile e acessibilidade | Completar estados, validações, acessibilidade e teclado | Em andamento | 42% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-317 | S20-P04 | Pacote setorial | Experiência WPF, React, mobile e acessibilidade | Homologar React responsivo, PWA/mobile e performance | Em andamento | 42% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-318 | S20-P05 | Pacote setorial | Experiência WPF, React, mobile e acessibilidade | Implantar testes visuais, de usabilidade e auto-update seguro | Em andamento | 42% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-319 | S21-P01 | Pacote setorial | Unificação dos bancos e modelo canônico | Concluir inventário técnico e estatístico dos dois schemas | Em andamento | 8% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-320 | S21-P02 | Pacote setorial | Unificação dos bancos e modelo canônico | Completar linhagem e equivalências por domínio/consumidor | Em andamento | 8% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-321 | S21-P03 | Pacote setorial | Unificação dos bancos e modelo canônico | Aprovar modelo canônico, compatibilidade e governança | Em andamento | 8% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-322 | S21-P04 | Pacote setorial | Unificação dos bancos e modelo canônico | Executar migração idempotente e reconciliação em cópia controlada | Em andamento | 8% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |
| A-323 | S21-P05 | Pacote setorial | Unificação dos bancos e modelo canônico | Realizar corte, rollback, estabilidade e desativação segura | Em andamento | 8% | Fechamento do pacote anterior do mesmo setor; gates transversais de IAM, auditoria, tenant, dados e qualidade | API/contrato, persistência ou consulta real quando aplicável, RBAC, tenant, auditoria, tratamento de erro, build, teste autenticado e evidência anexada |

## Planejamento Setorial Completo

Cada setor abaixo possui cinco pacotes `A-xxx` sequenciais. Os subitens não autorizam execução paralela fora da máquina de estados.

### S00 — Governança do programa, arquitetura e qualidade

**Maturidade de planejamento:** `65%`  
**Lacuna:** `35%`  
**Atenção:** `Alta`  
**Referências principais:** `B1, B2, B3, B6, B11, DOD-01, DOD-02, DOD-03, DOD-32, DOD-33, DOD-34`  
**Diagnóstico:** Solution única, builds e registros de entrega existem; o denominador clínico e a gestão de dependências precisam ser reconciliados.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-214 | S00-P01 | Reconciliar o catálogo clínico e fixar um denominador oficial versionado | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-215 | S00-P02 | Implantar máquina de estados de tarefas, ramificações e gates | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-216 | S00-P03 | Consolidar arquitetura de referência, ADRs e mapa de dependências | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-217 | S00-P04 | Padronizar evidências, homologação, release e rollback | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-218 | S00-P05 | Operar portfólio, riscos, capacidade e métricas executivas | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-216-F01 | Painel do portfólio e roadmap | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-216-F02 | Cadastro de épicos, programas e entregas | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-216-F03 | Controle de tarefa-pai, ramificações e gates | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-216-F04 | Solicitação de mudança e decisão arquitetural | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-216-F05 | Registro de risco, impedimento e dependência | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-216-F06 | Plano de release, cutover e rollback | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-216-F07 | Registro de evidências e homologação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-216-F08 | Registro de defeitos e não conformidades | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-217-R01 | Status executivo por setor | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-217-R02 | Mapa de dependências e caminho crítico | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-217-R03 | Relatório de riscos, impedimentos e decisões | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-217-R04 | Pacote de evidências por release | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-217-R05 | Relatório de aderência à Definition of Done | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-217-G01 | Progresso estrito e maturidade por setor | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-217-G02 | Burndown/burnup de entregas | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-217-G03 | Fluxo cumulativo por estado | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-217-G04 | Tendência de defeitos e retrabalho | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-217-G05 | Cobertura de evidências por gate | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-218-AU01 | Bloquear abertura de nova ramificação sem fechamento da anterior | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-218-AU02 | Coletar hashes, builds, testes e smokes automaticamente | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-218-AU03 | Gerar changelog e manifesto de release | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-218-AU04 | Criar backup antes de patch e validar restauração | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-218-AU05 | Atualizar percentuais somente por evidência aprovada | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S01 — Plataforma, IAM, SSO, tenants e segurança

**Maturidade de planejamento:** `72%`  
**Lacuna:** `28%`  
**Atenção:** `Redobrada por risco de segregação`  
**Referências principais:** `B4, B5, B10, WPFDB-07, DOD-06, DOD-07, DOD-08, DOD-17, DOD-22, CAP-11`  
**Diagnóstico:** MFA, refresh, logout, RBAC Desktop e workflow têm validações; isolamento integral entre tenants e ciclo de acesso ainda não estão fechados.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-219 | S01-P01 | Concluir isolamento multiempresa em toda entidade e consulta | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-220 | S01-P02 | Completar ciclo de identidade, perfil, permissão e segregação de funções | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-221 | S01-P03 | Formalizar gestão de sessões, MFA, contas técnicas e clientes de API | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-222 | S01-P04 | Implantar gestão segura de segredos, certificados e rotação | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-223 | S01-P05 | Homologar autorização e auditoria por módulo, canal e tenant | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-221-F01 | Gestão de tenants, empresas e unidades | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F02 | Gestão de usuários | Existente homologado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F03 | Perfis, permissões e matriz RBAC | Existente homologado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F04 | Matriz de segregação de funções | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F05 | MFA, dispositivos confiáveis e recuperação | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F06 | Sessões, refresh tokens e revogação | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F07 | Contas de serviço e clientes OAuth/API | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F08 | Revisão periódica de acessos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F09 | Cofre lógico de segredos e certificados | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-221-F10 | Provisionamento e desprovisionamento de tenant | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-222-R01 | Usuários ativos/inativos e acessos órfãos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-222-R02 | Acessos privilegiados e elevação de função | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-222-R03 | Adoção e falhas de MFA | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-222-R04 | Sessões abertas, revogadas e suspeitas | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-222-R05 | Conflitos de segregação de funções | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-222-R06 | Segredos e certificados próximos do vencimento | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-222-G01 | Logins por resultado e origem | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-222-G02 | Adoção de MFA por perfil | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-222-G03 | Privilégios por tenant | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-222-G04 | Sessões simultâneas e revogações | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-222-G05 | Conflitos SoD por criticidade | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-223-AU01 | Provisionar tenant com papéis mínimos e parâmetros canônicos | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-223-AU02 | Revogar sessões no desligamento ou alteração sensível | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-223-AU03 | Rotacionar segredos e credenciais operacionais com rollback | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-223-AU04 | Executar revisão periódica de acesso | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-223-AU05 | Alertar certificado, segredo e conta técnica próximos do vencimento | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S02 — GRC, auditoria, privacidade e conformidade

**Maturidade de planejamento:** `50%`  
**Lacuna:** `50%`  
**Atenção:** `Redobrada`  
**Referências principais:** `B4, B7, B8, LEG-16, WPFDB-07, CAP-14, DOD-31, DOD-36`  
**Diagnóstico:** Auditoria consultável e eventos existem, mas a cobertura de toda escrita, controles, privacidade e continuidade não está comprovada.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-224 | S02-P01 | Cobrir auditoria imutável e correlacionada em toda operação crítica | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-225 | S02-P02 | Implantar catálogo de políticas, controles, riscos e evidências | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-226 | S02-P03 | Operar privacidade, consentimento, retenção e direitos do titular | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-227 | S02-P04 | Formalizar incidentes, continuidade e recuperação de desastre | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-228 | S02-P05 | Homologar controles regulatórios e revisões independentes | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-226-F01 | Plano e universo de auditoria | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F02 | Evento de auditoria e investigação | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F03 | Política, norma e procedimento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F04 | Controle, teste e evidência | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F05 | Risco corporativo e plano de tratamento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F06 | Incidente de segurança e privacidade | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F07 | Registro de tratamento de dados | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F08 | Consentimento e preferência | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F09 | Solicitação do titular | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F10 | Plano de continuidade e desastre | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-226-F11 | Avaliação de risco de terceiro | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-227-R01 | Trilha de auditoria por usuário/tenant/registro | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-227-R02 | Matriz de riscos e controles | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-227-R03 | Efetividade de controles | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-227-R04 | Incidentes e tempos de resposta | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-227-R05 | Retenção, anonimização e descarte | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-227-R06 | Status de continuidade e testes de restauração | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-227-G01 | Mapa de calor de riscos | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-227-G02 | Tendência de incidentes | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-227-G03 | Cobertura de auditoria por domínio | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-227-G04 | Efetividade de controles | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-227-G05 | Tempo médio de resposta e recuperação | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-228-AU01 | Detectar operação crítica sem evento de auditoria | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-228-AU02 | Executar retenção e descarte aprovados | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-228-AU03 | Escalonar incidentes por severidade e SLA | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-228-AU04 | Programar testes de controle e continuidade | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-228-AU05 | Consolidar evidências por auditoria e release | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S03 — Master Data e estrutura multiempresarial

**Maturidade de planejamento:** `60%`  
**Lacuna:** `40%`  
**Atenção:** `Alta`  
**Referências principais:** `CAP-13, LEG-03, LEG-04, LEG-05, WPFDB-04, WPFDB-09, DOD-27`  
**Diagnóstico:** Clientes, fornecedores, contatos, endereços, produtos e seis lojas possuem implementação parcial/avançada; faltam MDM integral, empresas, unidades, cadastros corporativos e qualidade.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-229 | S03-P01 | Consolidar entidades legais, empresas, filiais e unidades de negócio | Em andamento | 60% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-230 | S03-P02 | Completar cadastro mestre de pessoas, parceiros e relacionamentos | Em andamento | 60% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-231 | S03-P03 | Completar produtos, serviços, categorias, unidades, preços e lojas | Em andamento | 60% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-232 | S03-P04 | Implantar qualidade, deduplicação e governança de dados mestres | Em andamento | 60% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-233 | S03-P05 | Homologar cadastros em API, React, WPF, auditoria e tenant | Em andamento | 60% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-231-F01 | Empresas, filiais e estabelecimentos | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F02 | Unidades de negócio, centros e departamentos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F03 | Pessoas físicas/jurídicas e papéis | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F04 | Clientes e endereços | Existente homologado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F05 | Fornecedores e contatos | Existente homologado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F06 | Transportadoras e parceiros | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F07 | Produtos, serviços e variações | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F08 | Categorias, marcas e unidades de medida | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F09 | Lojas, perfis públicos e domínios | Parcial avançado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F10 | Preços por loja e canal | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F11 | Centros de custo e lucro | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F12 | Moedas, calendários e localidades | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-231-F13 | Contas bancárias corporativas | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-232-R01 | Completude e qualidade cadastral | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-232-R02 | Duplicidades e conflitos de identificação | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-232-R03 | Cadastros sem relacionamento obrigatório | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-232-R04 | Produtos sem classificação fiscal/preço/estoque | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-232-R05 | Lojas e parceiros por estado de publicação | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-232-G01 | Índice de qualidade de dados | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-232-G02 | Cadastros por empresa/filial | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-232-G03 | Evolução de duplicidades | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-232-G04 | Cobertura de atributos obrigatórios | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-232-G05 | Distribuição de produtos por canal/loja | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-233-AU01 | Validar documentos, e-mails, CEPs e chaves únicas | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-233-AU02 | Detectar e propor fusão de duplicidades | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-233-AU03 | Propagar atributos aprovados aos canais | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-233-AU04 | Bloquear publicação de cadastro incompleto | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-233-AU05 | Revalidar relacionamentos pai-filho e RowVersion | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S04 — Financeiro e tesouraria

**Maturidade de planejamento:** `65%`  
**Lacuna:** `35%`  
**Atenção:** `Alta`  
**Referências principais:** `CAP-06, LEG-06, DOD-24, WPFDB-05, DOD-35`  
**Diagnóstico:** Contas a pagar/receber, baixas, PDFs e conciliação estão avançados; faltam tesouraria, cobrança, crédito, orçamento, câmbio e homologação integral.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-234 | S04-P01 | Concluir contas a pagar, receber, cobrança e crédito ponta a ponta | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-235 | S04-P02 | Implantar tesouraria, caixa, bancos, pagamentos e recebimentos | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-236 | S04-P03 | Completar orçamento, forecast, centros de custo e aprovações | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-237 | S04-P04 | Implantar moedas, câmbio, empréstimos, garantias e posição financeira | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-238 | S04-P05 | Homologar relatórios, conciliações, auditoria e fechamento financeiro | Em andamento | 65% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-236-F01 | Contas a pagar | Existente homologado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F02 | Contas a receber | Existente homologado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F03 | Baixas, pagamentos e recebimentos | Parcial avançado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F04 | Conciliação bancária | Existente homologado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F05 | Contas bancárias e saldos | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F06 | Importação e classificação de extrato | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F07 | Fluxo de caixa realizado e projetado | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F08 | Tesouraria e posição diária | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F09 | Cobrança, régua e acordos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F10 | Limite e análise de crédito | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F11 | Orçamento e versões | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F12 | Forecast financeiro | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F13 | Empréstimos, financiamentos e garantias | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F14 | Moedas e taxas de câmbio | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-236-F15 | Lote de pagamentos e aprovações | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-237-R01 | Aging de pagar e receber | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-237-R02 | Fluxo de caixa diário/semanal/mensal | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-237-R03 | Posição bancária e disponibilidade | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-237-R04 | Conciliação e pendências bancárias | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-237-R05 | Inadimplência e recuperação | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-237-R06 | Orçado versus realizado/forecast | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-237-R07 | Exposição cambial e financeira | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-237-R08 | DSO, DPO e ciclo de caixa | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-237-G01 | Curva de caixa | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-237-G02 | Aging por faixa | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-237-G03 | Saldos por banco/empresa | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-237-G04 | Orçado x realizado | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-237-G05 | Inadimplência e recuperação | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-237-G06 | Exposição cambial | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-238-AU01 | Importar extratos e conciliar por regras/idempotência | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-238-AU02 | Programar pagamentos após aprovação | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-238-AU03 | Gerar régua de cobrança e escalonamento | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-238-AU04 | Projetar caixa com fatos e compromissos | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-238-AU05 | Atualizar taxas de câmbio por fonte aprovada | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-238-AU06 | Alertar vencimentos, saldos e limites | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S05 — Contabilidade, controladoria e consolidação

**Maturidade de planejamento:** `50%`  
**Lacuna:** `50%`  
**Atenção:** `Redobrada`  
**Referências principais:** `CAP-06, DOD-32, DOD-35, LEG-06`  
**Diagnóstico:** Razão, DRE, fechamento e estruturas contábeis existem parcialmente; faltam fatos completos, telas dedicadas, consolidação e intercompany.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-239 | S05-P01 | Concluir plano de contas, lançamentos, partidas e razão | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-240 | S05-P02 | Implantar fechamento por período com checklist e travas | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-241 | S05-P03 | Completar DRE, balanço, fluxo contábil e demonstrações | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-242 | S05-P04 | Implantar custos, rateios, centros de lucro e controladoria | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-243 | S05-P05 | Implantar consolidação, intercompany e eliminações multiempresa | Em andamento | 50% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-241-F01 | Plano de contas | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F02 | Lançamento contábil e lote | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F03 | Partidas, históricos e documentos | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F04 | Fechamento contábil por período | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F05 | Reabertura controlada de período | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F06 | Rateios e critérios de alocação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F07 | Centros de custo e lucro | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F08 | Consolidação e grupo econômico | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F09 | Operações intercompany | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F10 | Eliminações e ajustes de consolidação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F11 | Ativos imobilizados e depreciação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-241-F12 | Provisões e recorrências | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-242-R01 | Balancete | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-242-R02 | Razão analítico e sintético | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-242-R03 | DRE gerencial e societária | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-242-R04 | Balanço patrimonial | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-242-R05 | Fluxo de caixa contábil | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-242-R06 | Mapa de custos e margens | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-242-R07 | Consolidação por empresa/moeda | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-242-R08 | Intercompany e eliminações | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-242-R09 | Fechamento e pendências | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-242-G01 | Receita, custo, margem e resultado | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-242-G02 | Despesas por centro | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-242-G03 | Variação orçado x realizado | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-242-G04 | Evolução patrimonial | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-242-G05 | Contribuição por empresa/unidade | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-243-AU01 | Gerar lançamentos recorrentes | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-243-AU02 | Executar rateios aprovados | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-243-AU03 | Bloquear período fechado | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-243-AU04 | Consolidar e eliminar intercompany | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-243-AU05 | Reconciliar subledgers com razão | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-243-AU06 | Gerar pacote de fechamento | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S06 — Fiscal, tributário e obrigações legais

**Maturidade de planejamento:** `35%`  
**Lacuna:** `65%`  
**Atenção:** `Crítica`  
**Referências principais:** `CAP-05, LEG-07, REAL-13, DOD-09, WPFDB-04`  
**Diagnóstico:** Contratos e bloqueios sem sucesso falso existem; homologação SEFAZ, certificados, XML, eventos, SPED e apurações continuam incompletos.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-244 | S06-P01 | Concluir motor tributário por empresa, produto, operação e jurisdição | Em andamento | 35% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-245 | S06-P02 | Homologar emissão e eventos NF-e/NFC-e com chave e protocolo reais | Em andamento | 35% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-246 | S06-P03 | Implantar CT-e, MDF-e, NFS-e e documentos correlatos | Em andamento | 35% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-247 | S06-P04 | Completar apuração, livros, SPED e obrigações acessórias | Em andamento | 35% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-248 | S06-P05 | Homologar contingência, certificados, auditoria e reconciliação fiscal | Em andamento | 35% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-246-F01 | Perfis e regimes tributários | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F02 | Regras por NCM/CEST/CFOP/UF | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F03 | Cadastro de impostos e alíquotas | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F04 | Emissão NF-e/NFC-e | Parcial bloqueado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F05 | Cancelamento, inutilização e carta de correção | Parcial bloqueado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F06 | Consulta e distribuição de XML | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F07 | CT-e e MDF-e | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F08 | NFS-e e serviços | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F09 | Manifestação do destinatário | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F10 | Apuração de impostos | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F11 | SPED Fiscal e Contábil | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F12 | Certificados digitais e procurações | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F13 | Contingência fiscal | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-246-F14 | Roteamento fiscal por empresa/operação | Parcial genérico a substituir | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-247-R01 | Livro de entradas/saídas | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-247-R02 | Apuração por tributo e período | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-247-R03 | Documentos autorizados, rejeitados e pendentes | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-247-R04 | XMLs ausentes ou inconsistentes | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-247-R05 | SPED e validações | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-247-R06 | Certificados e procurações a vencer | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-247-R07 | Carga tributária por produto/empresa | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-247-G01 | Tributos por período | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-247-G02 | Rejeições por código e causa | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-247-G03 | Carga tributária efetiva | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-247-G04 | Documentos por status | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-247-G05 | Tempo médio de autorização | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-248-AU01 | Reservar numeração atomicamente | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-248-AU02 | Enviar, consultar e persistir protocolo oficial | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-248-AU03 | Baixar e armazenar XML/DANFE | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-248-AU04 | Monitorar certificados e vencimentos | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-248-AU05 | Gerar apurações e arquivos oficiais | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-248-AU06 | Reprocessar apenas operações idempotentes autorizadas | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S07 — Comercial, vendas, preços e contratos

**Maturidade de planejamento:** `58%`  
**Lacuna:** `42%`  
**Atenção:** `Alta`  
**Referências principais:** `CAP-02, LEG-02, LEG-03, LEG-10, DOD-04, DOD-35`  
**Diagnóstico:** Pedidos, PDV, checkout, produtos, cupons e preços têm implementação; a cadeia omnicanal e controles comerciais completos não foram homologados integralmente.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-249 | S07-P01 | Concluir cotação, proposta, pedido e aprovação comercial | Em andamento | 58% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-250 | S07-P02 | Completar preços, descontos, promoções e margem por canal | Em andamento | 58% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-251 | S07-P03 | Homologar PDV, checkout, devolução, troca e pós-venda | Em andamento | 58% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-252 | S07-P04 | Implantar metas, comissões, territórios e carteira | Em andamento | 58% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-253 | S07-P05 | Fechar correlação venda-pagamento-estoque-fiscal-logística-financeiro | Em andamento | 58% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-251-F01 | Oportunidade e cotação comercial | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F02 | Proposta e versão | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F03 | Pedido de venda | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F04 | Itens, serviços e kits | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F05 | Tabela de preços e vigência | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F06 | Preço por loja/canal/cliente | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F07 | Política de desconto e aprovação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F08 | Cupons e promoções | Parcial validado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F09 | PDV e fechamento de venda | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F10 | Troca, devolução e cancelamento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F11 | Metas, territórios e carteira | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F12 | Comissões e regras | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-251-F13 | Contratos comerciais e renovações | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-252-R01 | Vendas por empresa/loja/canal/vendedor | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-252-R02 | Pedidos em carteira e backlog | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-252-R03 | Margem por produto/cliente/canal | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-252-R04 | Descontos e exceções | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-252-R05 | Comissões e metas | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-252-R06 | Devoluções e cancelamentos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-252-R07 | Conversão cotação-pedido | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-252-G01 | Funil comercial | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-252-G02 | Receita e margem | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-252-G03 | Ticket médio e conversão | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-252-G04 | Metas por vendedor/equipe | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-252-G05 | Mix de produtos e canais | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-253-AU01 | Converter cotação aprovada em pedido | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-253-AU02 | Aplicar preço vigente e validar margem mínima | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-253-AU03 | Solicitar aprovação de desconto/limite | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-253-AU04 | Calcular comissão após evento elegível | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-253-AU05 | Correlacionar e monitorar ponta a ponta da venda | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S08 — CRM, marketing, atendimento e Yara

**Maturidade de planejamento:** `55%`  
**Lacuna:** `45%`  
**Atenção:** `Alta`  
**Referências principais:** `CAP-08, LEG-09, LEG-11, DOD-28, DOD-30, REAL-07`  
**Diagnóstico:** Leads, campanhas, segmentos, MarketingWindow e separação Yara/Sophia existem; jornada completa, atendimento, custo, consentimento e chave oficial ainda faltam.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-254 | S08-P01 | Concluir CRM de lead a cliente com atividades, oportunidade e histórico | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-255 | S08-P02 | Completar campanhas, segmentos, consentimentos e mensuração | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-256 | S08-P03 | Implantar atendimento, tickets, SLA, conhecimento e satisfação | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-257 | S08-P04 | Homologar Yara com ferramentas autorizadas e transferência humana | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-258 | S08-P05 | Medir conversão, qualidade, custo e proteção de dados da jornada | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-256-F01 | Leads e qualificação | Parcial avançado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F02 | Contas, contatos e oportunidades | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F03 | Atividades, tarefas e agenda | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F04 | Campanhas e segmentos | Existente operacional | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F05 | Consentimentos e preferências | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F06 | Tickets e filas de atendimento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F07 | SLA, prioridade e escalonamento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F08 | Base de conhecimento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F09 | Pesquisa de satisfação/NPS/CSAT | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F10 | Configuração Yara, modelo e orçamento | Parcial bloqueado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F11 | Histórico consentido da conversa | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-256-F12 | Transferência para atendimento humano | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-257-R01 | Funil de leads e oportunidades | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-257-R02 | Campanhas, custo e conversão | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-257-R03 | Atendimento por fila, SLA e resolução | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-257-R04 | Satisfação, churn e retenção | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-257-R05 | Conversas Yara, ferramentas usadas e custo | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-257-R06 | Transferências humanas e desfecho | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-257-G01 | Leads por origem/status | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-257-G02 | Pipeline e forecast | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-257-G03 | Campanhas por ROI | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-257-G04 | Tickets por SLA | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-257-G05 | CSAT/NPS | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-257-G06 | Conversão e custo da Yara | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-258-AU01 | Pontuar e distribuir leads | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-258-AU02 | Disparar campanhas consentidas | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-258-AU03 | Criar follow-ups e lembretes | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-258-AU04 | Escalonar SLA | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-258-AU05 | Executar ferramentas Yara somente por allowlist | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-258-AU06 | Transferir com contexto mínimo e consentido | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-258-AU07 | Expurgar histórico conforme retenção | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S09 — E-commerce, portal, lojas e omnicanalidade

**Maturidade de planejamento:** `72%`  
**Lacuna:** `28%`  
**Atenção:** `Alta até homologação pública`  
**Referências principais:** `CAP-02, CAP-13, DOD-27, DOD-29, REAL-04, REAL-05, REAL-06`  
**Diagnóstico:** Portal dinâmico, mídia, banners, cores, perfis e produtos vinculados estão avançados; publicação final, responsividade, SEO e ciclo completo ainda pendem.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-259 | S09-P01 | Concluir administração das seis lojas e conteúdo público | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-260 | S09-P02 | Homologar catálogo, busca, carrinho, checkout e conta do cliente | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-261 | S09-P03 | Completar mídia, SEO, navegação, acessibilidade e performance | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-262 | S09-P04 | Implantar pós-venda, devolução, recuperação de carrinho e fidelidade | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-263 | S09-P05 | Publicar e homologar desktop/mobile com métricas reais | Em andamento | 72% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-261-F01 | Perfil de loja, domínio e identidade | Parcial avançado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F02 | Biblioteca de mídia | Parcial avançado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F03 | Banners, ordem e vigência | Parcial avançado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F04 | Catálogo e vínculo produto-loja | Parcial avançado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F05 | Menus, páginas e conteúdo | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F06 | SEO, metadados e redirecionamentos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F07 | Carrinho e preferências | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F08 | Checkout e entrega | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F09 | Conta, pedidos e endereços do cliente | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F10 | Devolução, troca e reembolso | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F11 | Fidelidade, cashback e cupons | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-261-F12 | Recuperação de carrinho | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-262-R01 | Desempenho por loja e canal | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-262-R02 | Conversão e abandono de carrinho | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-262-R03 | Busca sem resultado | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-262-R04 | Produtos vistos/comprados | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-262-R05 | Mídias utilizadas e órfãs | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-262-R06 | Pedidos, devoluções e reembolsos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-262-R07 | Performance web e erros | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-262-G01 | Funil de e-commerce | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-262-G02 | Receita por loja | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-262-G03 | Conversão por dispositivo | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-262-G04 | Abandono por etapa | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-262-G05 | Produtos e categorias líderes | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-262-G06 | Core Web Vitals/tempo de resposta | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-263-AU01 | Publicar bundle com health gate e rollback | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-263-AU02 | Invalidar cache e otimizar mídia | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-263-AU03 | Recuperar carrinho com consentimento | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-263-AU04 | Sincronizar estoque e preço | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-263-AU05 | Proteger mídia referenciada | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-263-AU06 | Executar testes responsivos e de acessibilidade | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S10 — Marketplaces, dropshipping e ecossistema de parceiros

**Maturidade de planejamento:** `25%`  
**Lacuna:** `75%`  
**Atenção:** `Crítica`  
**Referências principais:** `CAP-12, LEG-12, LEG-13, REAL-01, REAL-02, REAL-03`  
**Diagnóstico:** Falsos sucessos foram removidos e rotas bloqueiam sem credenciais; homologações oficiais por seller/provedor e reconciliações ainda não existem.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-264 | S10-P01 | Implementar conectores oficiais por marketplace com OAuth/assinatura | Em andamento | 25% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-265 | S10-P02 | Completar cadastro, publicação e manutenção de anúncios | Em andamento | 25% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-266 | S10-P03 | Implantar importação de pedidos, estoque, preço e eventos | Em andamento | 25% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-267 | S10-P04 | Completar dropshipping, roteamento, comissão e SLA de fornecedor | Em andamento | 25% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-268 | S10-P05 | Reconciliar taxas, repasses, devoluções e identificadores externos | Em andamento | 25% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-266-F01 | Canais, sellers e credenciais | Parcial bloqueado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F02 | Mapeamento de categoria e atributos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F03 | Anúncios e variações | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F04 | Fila e erro de sincronização | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F05 | Pedidos importados e eventos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F06 | Estoque e preço por canal | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F07 | Devoluções e disputas | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F08 | Repasses, taxas e conciliação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F09 | Fornecedores dropshipping e cobertura | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F10 | Roteamento de pedido e SLA | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F11 | Comissões e liquidação do fornecedor | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-266-F12 | Webhooks e idempotência | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-267-R01 | Saúde dos anúncios | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-267-R02 | Vendas, taxas e margem por canal | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-267-R03 | Erros e atraso de sincronização | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-267-R04 | Pedidos sem correlação | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-267-R05 | Performance de fornecedor dropshipping | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-267-R06 | Repasses e divergências | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-267-G01 | Receita por marketplace | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-267-G02 | Margem líquida por canal | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-267-G03 | Taxa de erro de sync | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-267-G04 | SLA de fornecedores | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-267-G05 | Pedidos por estado externo | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-268-AU01 | Renovar tokens e validar escopos | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-268-AU02 | Publicar/atualizar somente após retorno aceito | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-268-AU03 | Importar pedidos e deduplicar webhook | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-268-AU04 | Sincronizar estoque e preço com fila idempotente | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-268-AU05 | Roteiar fornecedor por regra e disponibilidade | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-268-AU06 | Conciliar repasses e taxas | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S11 — Compras, sourcing e gestão de fornecedores

**Maturidade de planejamento:** `74%`  
**Lacuna:** `26%`  
**Atenção:** `Alta até fechar ramificações`  
**Referências principais:** `CAP-03, WPFDB-04, WPFDB-08, WPFDB-09`  
**Diagnóstico:** Solicitação, cotação, pedido, aprovação, entrada, estoque, financeiro e fornecedor/contato foram provados; faltam RFQ multi-fornecedor, devolução e anexos fiscais.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-269 | S11-P01 | Concluir requisição e aprovação de necessidade | Em andamento | 74% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-270 | S11-P02 | Completar RFQ, cotação multi-fornecedor e mapa comparativo | Em andamento | 74% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-271 | S11-P03 | Fechar pedido, recebimento parcial/final e três vias | Em andamento | 74% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-272 | S11-P04 | Implantar devolução, não conformidade e avaliação de fornecedor | Em andamento | 74% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-273 | S11-P05 | Homologar anexos, fiscal, financeiro, auditoria e indicadores | Em andamento | 74% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-271-F01 | Solicitação de compra | Existente operacional | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F02 | Itens e justificativas | Existente operacional | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F03 | RFQ e fornecedores convidados | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F04 | Cotação e mapa comparativo | Existente parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F05 | Aprovação por alçada | Existente operacional | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F06 | Pedido de compra | Existente operacional | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F07 | Recebimento parcial/final | Existente operacional | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F08 | Divergência e inspeção de recebimento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F09 | Devolução ao fornecedor | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F10 | Três vias: pedido-recebimento-fatura | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F11 | Avaliação e homologação de fornecedor | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F12 | Contratos e condições de compra | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-271-F13 | Anexos fiscais e documentos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-272-R01 | Spend por categoria/fornecedor | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-272-R02 | Economia e variação de preço | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-272-R03 | Lead time de compras | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-272-R04 | Pedidos abertos e atrasados | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-272-R05 | Divergências de recebimento/fatura | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-272-R06 | Avaliação e risco de fornecedor | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-272-G01 | Spend por categoria | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-272-G02 | Prazo médio por etapa | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-272-G03 | Economia negociada | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-272-G04 | Performance e qualidade de fornecedor | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-272-G05 | Backlog de pedidos | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-273-AU01 | Distribuir RFQ a múltiplos fornecedores | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-273-AU02 | Comparar condições normalizadas | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-273-AU03 | Aplicar alçada e workflow | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-273-AU04 | Gerar pedido após aprovação | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-273-AU05 | Executar match de três vias | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-273-AU06 | Criar estoque e financeiro atomicamente | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-273-AU07 | Alertar atraso e divergência | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S12 — Estoque, WMS e inventário

**Maturidade de planejamento:** `38%`  
**Lacuna:** `62%`  
**Atenção:** `Crítica`  
**Referências principais:** `CAP-04, DOD-10, WPFDB-05`  
**Diagnóstico:** Estruturas de saldos, movimentos, inventários, locais e kardex existem, mas endpoints, telas, concorrência e fluxo WMS não estão homologados integralmente.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-274 | S12-P01 | Concluir depósitos, endereços, saldos, lotes e seriais | Em andamento | 38% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-275 | S12-P02 | Implantar recebimento, putaway, reserva, separação e expedição | Em andamento | 38% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-276 | S12-P03 | Completar transferências, ajustes e rastreabilidade de movimentos | Em andamento | 38% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-277 | S12-P04 | Implantar inventário geral/cíclico, ABC e acuracidade | Em andamento | 38% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-278 | S12-P05 | Homologar concorrência, custo, auditoria e integração omnicanal | Em andamento | 38% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-276-F01 | Depósitos e endereços | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F02 | Saldos por produto/local/lote | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F03 | Movimentação e ajuste | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F04 | Recebimento e putaway | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F05 | Reserva e alocação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F06 | Ordens e ondas de separação | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F07 | Picking, packing e conferência | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F08 | Transferência entre locais/filiais | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F09 | Inventário e contagens | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F10 | Lotes, seriais e validade | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F11 | Curva ABC e ponto de pedido | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-276-F12 | Kardex e rastreabilidade | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-277-R01 | Posição e valor de estoque | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-277-R02 | Kardex | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-277-R03 | Acuracidade de inventário | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-277-R04 | Giro e cobertura | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-277-R05 | Estoque parado/obsoleto | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-277-R06 | Rupturas e baixo estoque | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-277-R07 | Produtividade de recebimento/picking | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-277-G01 | Valor e cobertura de estoque | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-277-G02 | Giro por classe ABC | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-277-G03 | Acuracidade por depósito | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-277-G04 | Rupturas e excessos | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-277-G05 | Produtividade WMS | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-278-AU01 | Reservar e alocar com concorrência | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-278-AU02 | Gerar reposição interna | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-278-AU03 | Criar ondas de separação | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-278-AU04 | Programar inventário cíclico | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-278-AU05 | Alertar validade, ruptura e excesso | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-278-AU06 | Sincronizar saldo com canais após commit | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S13 — Logística, TMS, transporte e comércio exterior

**Maturidade de planejamento:** `40%`  
**Lacuna:** `60%`  
**Atenção:** `Crítica`  
**Referências principais:** `CAP-07, LEG-08, REAL-11, REAL-12`  
**Diagnóstico:** Cotação interna oficial e contratos de rastreamento existem; tracking, transportadoras, fretes, CT-e/MDF-e e fulfillment externo não estão homologados.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-279 | S13-P01 | Completar cadastro de transportadoras, tabelas e contratos | Em andamento | 40% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-280 | S13-P02 | Implantar cotação, roteamento, expedição, etiquetas e romaneio | Em andamento | 40% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-281 | S13-P03 | Homologar tracking externo, ocorrências e prova de entrega | Em andamento | 40% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-282 | S13-P04 | Implantar CT-e, MDF-e, cargas e auditoria de frete | Em andamento | 40% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-283 | S13-P05 | Adicionar importação/exportação, aduana e comércio exterior | Em andamento | 40% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-281-F01 | Transportadoras e serviços | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F02 | Tabelas e faixas de frete | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F03 | Cotação e comparação de frete | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F04 | Expedição, volumes e etiquetas | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F05 | Romaneio, carga e rota | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F06 | Tracking e eventos | Parcial bloqueado | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F07 | Ocorrências, avarias e sinistros | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F08 | Prova de entrega | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F09 | Auditoria e cobrança de frete | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F10 | CT-e e MDF-e | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F11 | Importação/exportação e Incoterms | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-281-F12 | Documentos aduaneiros e desembaraço | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-282-R01 | Frete previsto x cobrado | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-282-R02 | OTIF e prazo de entrega | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-282-R03 | Performance por transportadora | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-282-R04 | Ocorrências e sinistros | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-282-R05 | Custos logísticos por rota/canal | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-282-R06 | Tracking sem atualização | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-282-R07 | Operações de comércio exterior | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-282-G01 | OTIF | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-282-G02 | Custo de frete por pedido | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-282-G03 | Lead time por transportadora | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-282-G04 | Ocorrências por causa | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-282-G05 | Entregas por status/região | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-283-AU01 | Rate shopping e seleção por regra | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-283-AU02 | Gerar etiqueta após confirmação | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-283-AU03 | Consultar tracking e processar webhook | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-283-AU04 | Escalonar atraso/ocorrência | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-283-AU05 | Conferir cobrança de frete | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-283-AU06 | Gerar CT-e/MDF-e quando aplicável | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S14 — Manufatura, PCP, MES, qualidade, manutenção e ativos

**Maturidade de planejamento:** `20%`  
**Lacuna:** `80%`  
**Atenção:** `Crítica`  
**Referências principais:** `CAP-10, DOD-11, WPFDB-04`  
**Diagnóstico:** Tabelas e rotas de PCP/OPS existem, porém formulários específicos e homologação ponta a ponta são majoritariamente pendentes.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-284 | S14-P01 | Implantar engenharia de produto, BOM e roteiros | Em andamento | 20% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-285 | S14-P02 | Completar MRP, planejamento, ordens e requisições de material | Em andamento | 20% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-286 | S14-P03 | Implantar MES, apontamentos, capacidade, paradas e OEE | Em andamento | 20% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-287 | S14-P04 | Completar qualidade, inspeções, não conformidade e CAPA | Em andamento | 20% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-288 | S14-P05 | Implantar EAM/manutenção, ativos, calibração e custos | Em andamento | 20% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-286-F01 | BOM/estrutura de produto | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F02 | Roteiros e operações | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F03 | Centros de trabalho e capacidade | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F04 | Plano mestre e MRP | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F05 | Ordem de produção | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F06 | Requisição e consumo de material | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F07 | Apontamento de operação e produção | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F08 | Paradas, refugos e retrabalho | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F09 | Inspeção e plano de controle | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F10 | Não conformidade e CAPA | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F11 | Ativos e hierarquia técnica | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F12 | Plano de manutenção preventiva | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F13 | Ordem de manutenção | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F14 | Medidores, calibração e peças | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-286-F15 | Mudança de engenharia | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-287-R01 | Plano e carga de produção | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-287-R02 | Ordens em andamento | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-287-R03 | Consumo e variância de materiais | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-287-R04 | Custos e eficiência de produção | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-287-R05 | OEE e perdas | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-287-R06 | Qualidade e não conformidades | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-287-R07 | Backlog e custo de manutenção | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-287-R08 | Disponibilidade e confiabilidade de ativos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-287-G01 | OEE | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-287-G02 | Produção prevista x realizada | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-287-G03 | Pareto de paradas | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-287-G04 | Refugo e retrabalho | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-287-G05 | Capacidade por centro | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-287-G06 | MTBF/MTTR | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-288-AU01 | Executar MRP e gerar necessidades | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-288-AU02 | Sequenciar ordens por capacidade/regra | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-288-AU03 | Reservar e requisitar materiais | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-288-AU04 | Abrir manutenção preventiva | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-288-AU05 | Bloquear lote por qualidade | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-288-AU06 | Escalonar não conformidade/CAPA | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-288-AU07 | Atualizar custos após fechamento | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S15 — HCM, folha, ponto, saúde e segurança

**Maturidade de planejamento:** `22%`  
**Lacuna:** `78%`  
**Atenção:** `Crítica`  
**Referências principais:** `CAP-09, DOD-04`  
**Diagnóstico:** Cadastros e estruturas de RH existem parcialmente; admissão, folha, ponto, benefícios, eSocial e ciclo completo não estão comprovados.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-289 | S15-P01 | Concluir estrutura organizacional, cargos e colaboradores | Em andamento | 22% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-290 | S15-P02 | Implantar admissão, contratos, documentos, dependentes e benefícios | Em andamento | 22% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-291 | S15-P03 | Completar ponto, escalas, afastamentos, férias e banco de horas | Em andamento | 22% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-292 | S15-P04 | Implantar folha, encargos, provisões e eventos oficiais | Em andamento | 22% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-293 | S15-P05 | Completar desempenho, treinamento, sucessão, desligamento e SST | Em andamento | 22% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-291-F01 | Estrutura organizacional e departamentos | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F02 | Cargos, posições e salários | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F03 | Colaboradores e admissão | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F04 | Contratos e documentos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F05 | Dependentes e benefícios | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F06 | Ponto, escalas e banco de horas | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F07 | Afastamentos e férias | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F08 | Folha e eventos | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F09 | Encargos, provisões e pagamentos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F10 | Treinamentos e competências | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F11 | Avaliação de desempenho | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F12 | Plano de carreira e sucessão | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F13 | Desligamento e homologação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F14 | Saúde ocupacional e exames | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F15 | Segurança, incidentes e EPIs | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-291-F16 | Eventos eSocial | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-292-R01 | Headcount e movimentações | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-292-R02 | Folha e encargos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-292-R03 | Horas, extras e ausências | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-292-R04 | Férias e provisões | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-292-R05 | Turnover e retenção | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-292-R06 | Treinamento e competências | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-292-R07 | Desempenho e sucessão | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-292-R08 | SST e eSocial | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-292-G01 | Headcount | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-292-G02 | Turnover | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-292-G03 | Absenteísmo | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-292-G04 | Horas extras | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-292-G05 | Custo de pessoal | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-292-G06 | Diversidade | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-292-G07 | Acidentes e afastamentos | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-293-AU01 | Provisionar onboarding e acessos | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-293-AU02 | Importar/validar ponto | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-293-AU03 | Calcular folha e provisões | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-293-AU04 | Gerar eventos eSocial | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-293-AU05 | Alertar férias, exames e contratos | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-293-AU06 | Revogar acessos no desligamento | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S16 — Jurídico, contratos, riscos e ESG

**Maturidade de planejamento:** `15%`  
**Lacuna:** `85%`  
**Atenção:** `Crítica`  
**Referências principais:** `Matriz: jur_*, vw_processos_risco, vw_contratos_vencer, vw_certidoes_vencer`  
**Diagnóstico:** Há estrutura de tabelas jurídicas e views de risco, mas quase não há evidência de API, formulários ou homologação operacional.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-294 | S16-P01 | Implantar gestão do ciclo de contratos, cláusulas, aditivos e obrigações | Em andamento | 15% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-295 | S16-P02 | Completar processos, andamentos, prazos, depósitos e certidões | Em andamento | 15% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-296 | S16-P03 | Implantar base jurídica, tipos de ação e conhecimento | Em andamento | 15% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-297 | S16-P04 | Integrar riscos, compliance, seguros e continuidade | Em andamento | 15% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-298 | S16-P05 | Implantar ESG, indicadores, compromissos e reporte | Em andamento | 15% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-296-F01 | Contratos e partes | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F02 | Cláusulas, obrigações e garantias | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F03 | Aditivos, renovação e encerramento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F04 | Processos e tipos de ação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F05 | Andamentos e prazos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F06 | Depósitos e custos judiciais | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F07 | Certidões e vencimentos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F08 | Base de conhecimento jurídico | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F09 | Riscos e controles jurídicos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F10 | Seguros e apólices | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F11 | Indicadores ESG | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F12 | Emissões, energia, água e resíduos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-296-F13 | Ações sociais e governança | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-297-R01 | Contratos a vencer e obrigações | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-297-R02 | Contencioso e exposição financeira | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-297-R03 | Prazos e certidões | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-297-R04 | Custos jurídicos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-297-R05 | Mapa de riscos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-297-R06 | Indicadores ESG e metas | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-297-R07 | Conformidade de terceiros | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-297-G01 | Mapa de calor de risco | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-297-G02 | Contratos por vencimento | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-297-G03 | Processos por fase/valor | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-297-G04 | Custos jurídicos | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-297-G05 | Emissões e consumo ESG | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-298-AU01 | Alertar prazo, renovação e obrigação | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-298-AU02 | Escalonar risco e processo crítico | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-298-AU03 | Gerar calendário jurídico | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-298-AU04 | Coletar evidências contratuais | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-298-AU05 | Consolidar indicadores ESG | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-298-AU06 | Bloquear fornecedor sem requisito crítico quando aprovado | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S17 — Projetos, serviços, campo, facilities e patrimônio

**Maturidade de planejamento:** `10%`  
**Lacuna:** `90%`  
**Atenção:** `Crítica`  
**Referências principais:** `Novo escopo corporativo complementar`  
**Diagnóstico:** Não há evidência suficiente de módulos completos; capacidades devem ser incorporadas como expansão planejada sem afetar o núcleo atual.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-299 | S17-P01 | Implantar portfólio, projetos, WBS, custos e recursos | Em andamento | 10% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-300 | S17-P02 | Completar serviços, contratos, ordens e faturamento | Em andamento | 10% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-301 | S17-P03 | Implantar atendimento de campo, agenda, despacho e mobilidade | Em andamento | 10% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-302 | S17-P04 | Completar facilities, imóveis, reservas e manutenção predial | Em andamento | 10% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-303 | S17-P05 | Integrar patrimônio, despesas, SLA e indicadores | Em andamento | 10% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-301-F01 | Portfólio e projeto | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F02 | WBS, cronograma e marcos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F03 | Recursos, capacidade e timesheet | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F04 | Custos, despesas e orçamento do projeto | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F05 | Riscos, issues e mudanças | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F06 | Contrato e ordem de serviço | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F07 | Catálogo de serviços e SLA | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F08 | Visita de campo e checklist | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F09 | Despacho e rota de técnico | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F10 | Imóveis, unidades e contratos de locação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F11 | Reservas de salas/recursos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F12 | Facilities e chamados prediais | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-301-F13 | Patrimônio e movimentações | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-302-R01 | Portfólio e saúde de projetos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-302-R02 | Earned value e variações | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-302-R03 | Utilização e capacidade | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-302-R04 | Rentabilidade por projeto/serviço | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-302-R05 | SLA e backlog de campo | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-302-R06 | Custos de facilities e imóveis | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-302-R07 | Patrimônio por unidade | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-302-G01 | Cronograma e caminho crítico | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-302-G02 | Custo/prazo planejado x realizado | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-302-G03 | Capacidade de recursos | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-302-G04 | SLA de serviço | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-302-G05 | Backlog por região | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-302-G06 | Custo de facilities | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-303-AU01 | Agendar tarefas e dependências | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-303-AU02 | Aprovar horas e despesas | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-303-AU03 | Despachar técnico por competência/região | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-303-AU04 | Gerar faturamento por marco/SLA | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-303-AU05 | Programar manutenção predial | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-303-AU06 | Alertar vencimentos de locação | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S18 — BI, dados, MDM, analytics e IA corporativa

**Maturidade de planejamento:** `48%`  
**Lacuna:** `52%`  
**Atenção:** `Redobrada`  
**Referências principais:** `CAP-01, CAP-08, CAP-14, DOD-35, UBD-01, UBD-02`  
**Diagnóstico:** Dashboard real e estruturas BI existem; faltam catálogo, linhagem, qualidade, métricas corporativas, relatórios completos e governança de IA.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-304 | S18-P01 | Consolidar catálogo de dados, linhagem e qualidade | Em andamento | 48% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-305 | S18-P02 | Implantar camada semântica, KPIs e dimensões conformadas | Em andamento | 48% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-306 | S18-P03 | Completar dashboards, relatórios e distribuição governada | Em andamento | 48% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-307 | S18-P04 | Implantar detecção de anomalia, forecast e IA assistiva controlada | Em andamento | 48% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-308 | S18-P05 | Medir custo, qualidade, explicabilidade e uso de dados/modelos | Em andamento | 48% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-306-F01 | Catálogo e glossário de dados | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F02 | Linhagem tabela-endpoint-formulário-relatório | Em andamento | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F03 | Regra e incidente de qualidade | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F04 | Definição de KPI e meta | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F05 | Dashboard, widget e layout | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F06 | Relatório e parâmetros | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F07 | Assinatura e distribuição de relatório | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F08 | Modelo analítico e versão | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F09 | Política de IA, prompt e ferramenta | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F10 | Registro de execução/custo/modelo | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-306-F11 | Anomalia e investigação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-307-R01 | Cockpit executivo consolidado | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-307-R02 | Scorecards por setor/empresa | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-307-R03 | Qualidade e completude de dados | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-307-R04 | Linhagem e impacto de mudança | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-307-R05 | Custos e uso de IA | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-307-R06 | Forecast e cenários | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-307-R07 | Relatórios regulatórios e gerenciais | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-307-G01 | KPIs 7/30/90 dias | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-307-G02 | Receita, margem e caixa | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-307-G03 | Operações ponta a ponta | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-307-G04 | Qualidade de dados | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-307-G05 | Acurácia de forecast | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-307-G06 | Custo e latência de IA | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-308-AU01 | Calcular KPIs por agenda e tenant | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-308-AU02 | Executar pipelines idempotentes | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-308-AU03 | Validar qualidade e bloquear publicação inconsistente | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-308-AU04 | Distribuir relatórios autorizados | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-308-AU05 | Detectar anomalias e abrir investigação | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-308-AU06 | Monitorar modelos, custo e erro | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S19 — Integrações, SRE, DevSecOps, observabilidade e ITSM

**Maturidade de planejamento:** `55%`  
**Lacuna:** `45%`  
**Atenção:** `Alta`  
**Referências principais:** `B6, B7, B8, B9, B10, B11, B12, B13, B14, CAP-14, DOD-12 a DOD-20`  
**Diagnóstico:** Runtime 24h, health, compose, coverage e backup parcial existem; observabilidade, restore, staging, jobs, filas, auto-update e ITSM ainda não fecharam.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-309 | S19-P01 | Completar catálogo de integrações, contratos, filas e webhooks | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-310 | S19-P02 | Homologar logs, métricas, traces, alertas e SLOs | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-311 | S19-P03 | Completar CI/CD, segurança, migrations, staging e rollback | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-312 | S19-P04 | Homologar backup, restore, DR, jobs, Redis e MinIO | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-313 | S19-P05 | Implantar ITSM, incidentes, mudanças, ativos e capacidade técnica | Em andamento | 55% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-311-F01 | Catálogo de APIs e integrações | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F02 | Endpoints, credenciais e webhooks | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F03 | Fila, mensagem e dead-letter | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F04 | Jobs, agendas e execuções | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F05 | Saúde de serviços e dependências | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F06 | Incidente, problema e causa raiz | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F07 | Mudança e janela de implantação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F08 | Release, deployment e rollback | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F09 | Backup, restore e teste | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F10 | Certificados, DNS e túnel | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F11 | Feature flags e configuração | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F12 | Inventário de ativos técnicos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-311-F13 | Vulnerabilidade e remediação | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-312-R01 | Disponibilidade e SLO | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-312-R02 | Latência, erros e saturação | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-312-R03 | Jobs, filas e falhas | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-312-R04 | Deployments e mudanças | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-312-R05 | Backups e restaurações | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-312-R06 | Vulnerabilidades e dependências | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-312-R07 | Custos e capacidade de infraestrutura | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-312-G01 | RED/USE por serviço | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-312-G02 | Disponibilidade mensal | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-312-G03 | Erro por endpoint | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-312-G04 | Fila e dead-letter | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-312-G05 | Tempo de deployment e rollback | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-312-G06 | RPO/RTO e backups | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-313-AU01 | Executar CI com build/test/coverage/security | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-313-AU02 | Aplicar migrations em ambiente controlado | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-313-AU03 | Executar health gate e rollback | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-313-AU04 | Coletar logs/traces/metrics | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-313-AU05 | Testar restore semanalmente | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-313-AU06 | Rotacionar segredos e certificados | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-313-AU07 | Atualizar Desktop com assinatura e reversão | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S20 — Experiência WPF, React, mobile e acessibilidade

**Maturidade de planejamento:** `42%`  
**Lacuna:** `58%`  
**Atenção:** `Redobrada`  
**Referências principais:** `WPFDB-01 a WPFDB-10, WPF-VIS-01A a WPF-VIS-01F, CAP-11, DOD-20, DOD-26, DOD-29`  
**Diagnóstico:** Algumas janelas possuem contensão e contratos reais; há três janelas genéricas, outbox sem homologação integral e frente visual global pendente.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-314 | S20-P01 | Substituir janelas genéricas por formulários de domínio | Em andamento | 42% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-315 | S20-P02 | Aplicar shell visual único, contensão, DPI e navegação | Em andamento | 42% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-316 | S20-P03 | Completar estados, validações, acessibilidade e teclado | Em andamento | 42% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-317 | S20-P04 | Homologar React responsivo, PWA/mobile e performance | Em andamento | 42% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-318 | S20-P05 | Implantar testes visuais, de usabilidade e auto-update seguro | Em andamento | 42% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-316-F01 | Catálogo de formulários e rotas | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F02 | CorporateMasterData específico por domínio | Genérico a substituir | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F03 | FiscalRouting específico | Genérico a substituir | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F04 | ModuleWorkspace por processo | Genérico a substituir | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F05 | Painel de contingência e reprocessamento | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F06 | Configuração de layout/tema autorizada | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F07 | Teste de resolução/DPI | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F08 | Acessibilidade e atalhos | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F09 | Localização, idioma e formato regional | Novo | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-316-F10 | Atualização Desktop e rollback | Parcial | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-317-R01 | Cobertura formulário-tabela-endpoint-permissão | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-317-R02 | Erros e falhas por janela | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-317-R03 | Acessibilidade e navegação por teclado | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-317-R04 | Performance e memória do Desktop | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-317-R05 | Responsividade e performance do portal | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-317-R06 | Adoção e uso por canal | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-317-G01 | Cobertura WPFDB | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-317-G02 | Tempo de abertura/resposta | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-317-G03 | Erros por formulário | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-317-G04 | Uso por resolução/DPI | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-317-G05 | Core Web Vitals | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-317-G06 | Taxa de atualização Desktop | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-318-AU01 | Executar regressão visual | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-318-AU02 | Testar resoluções e escalas DPI | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-318-AU03 | Validar handlers e bindings XAML | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-318-AU04 | Executar acessibilidade e teclado | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-318-AU05 | Verificar nenhum sucesso sem releitura | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-318-AU06 | Assinar e validar pacote de auto-update | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
### S21 — Unificação dos bancos e modelo canônico

**Maturidade de planejamento:** `8%`  
**Lacuna:** `92%`  
**Atenção:** `Crítica`  
**Referências principais:** `UBD-01 a UBD-08, CAP-15, DOD-25`  
**Diagnóstico:** Os dois schemas operam e o programa UBD está planejado; nenhuma alteração física está autorizada antes de inventário, linhagem, ensaio e reconciliação.

| A-ID | Classificação | Pacote evolutivo | Estado | Maturidade | Critério de abertura |
|---|---|---|---|---|---|
| A-319 | S21-P01 | Concluir inventário técnico e estatístico dos dois schemas | Em andamento | 8% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-320 | S21-P02 | Completar linhagem e equivalências por domínio/consumidor | Em andamento | 8% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-321 | S21-P03 | Aprovar modelo canônico, compatibilidade e governança | Em andamento | 8% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-322 | S21-P04 | Executar migração idempotente e reconciliação em cópia controlada | Em andamento | 8% | Fechar o pacote anterior do setor e todos os gates transversais |
| A-323 | S21-P05 | Realizar corte, rollback, estabilidade e desativação segura | Em andamento | 8% | Fechar o pacote anterior do setor e todos os gates transversais |

#### Formulários

| Código | Formulário | Situação | Regra |
|---|---|---|---|
| A-321-F01 | Inventário de objeto, coluna, chave e índice | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F02 | Linhagem e consumidor | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F03 | Equivalência e conflito semântico | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F04 | Decisão de modelo canônico | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F05 | Plano e lote de migração | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F06 | Regras de transformação | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F07 | Reconciliação de contagens/valores/hashes | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F08 | Cutover e checklist | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F09 | Rollback e incidente | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |
| A-321-F10 | Desativação e arquivamento | Pendente | Operação de domínio pela API oficial; sem CRUD MySQL direto no Desktop |

#### Relatórios

| Código | Relatório | Critério |
|---|---|---|
| A-322-R01 | Inventário dos schemas | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-322-R02 | Linhagem por domínio | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-322-R03 | Equivalências e conflitos | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-322-R04 | Resultado de migração | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-322-R05 | Reconciliação financeira/fiscal/estoque | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-322-R06 | Órfãos, duplicidades e divergências | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |
| A-322-R07 | Cutover, rollback e estabilidade | Dados reconciliados, filtros, tenant, autorização, exportação e rastreabilidade |

#### Gráficos e indicadores

| Código | Gráfico/indicador | Critério |
|---|---|---|
| A-322-G01 | Progresso UBD-01 a UBD-08 | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-322-G02 | Objetos reconciliados | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-322-G03 | Divergências por domínio | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-322-G04 | Volume migrado | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |
| A-322-G05 | Tempo e erros de cutover | Fonte real, período explícito, fuso, tenant, legenda e ausência de série estática |

#### Processos automatizados

| Código | Automação | Critério |
|---|---|---|
| A-323-AU01 | Extrair metadados e perfis de dados | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-323-AU02 | Comparar schemas e contratos | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-323-AU03 | Executar scripts idempotentes em cópia | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-323-AU04 | Calcular hashes e reconciliações | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-323-AU05 | Aplicar delta final com quiescência | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-323-AU06 | Reverter automaticamente ao falhar gate | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
| A-323-AU07 | Monitorar período de estabilidade | Idempotência, observabilidade, retry controlado, dead-letter/erro rastreável e auditoria |
## Percentuais por Setor e Necessidade de Atenção

| Setor | Área | Atingido | Lacuna | Atenção necessária | Referências | Justificativa |
|---|---|---|---|---|---|---|
| S00 | Governança do programa, arquitetura e qualidade | 65% | 35% | Alta | B1, B2, B3, B6, B11, DOD-01, DOD-02, DOD-03, DOD-32, DOD-33, DOD-34 | Solution única, builds e registros de entrega existem; o denominador clínico e a gestão de dependências precisam ser reconciliados. |
| S01 | Plataforma, IAM, SSO, tenants e segurança | 72% | 28% | Redobrada | B4, B5, B10, WPFDB-07, DOD-06, DOD-07, DOD-08, DOD-17, DOD-22, CAP-11 | MFA, refresh, logout, RBAC Desktop e workflow têm validações; isolamento integral entre tenants e ciclo de acesso ainda não estão fechados. |
| S02 | GRC, auditoria, privacidade e conformidade | 50% | 50% | Redobrada | B4, B7, B8, LEG-16, WPFDB-07, CAP-14, DOD-31, DOD-36 | Auditoria consultável e eventos existem, mas a cobertura de toda escrita, controles, privacidade e continuidade não está comprovada. |
| S03 | Master Data e estrutura multiempresarial | 60% | 40% | Alta | CAP-13, LEG-03, LEG-04, LEG-05, WPFDB-04, WPFDB-09, DOD-27 | Clientes, fornecedores, contatos, endereços, produtos e seis lojas possuem implementação parcial/avançada; faltam MDM integral, empresas, unidades, cadastros corporativos e qualidade. |
| S04 | Financeiro e tesouraria | 65% | 35% | Alta | CAP-06, LEG-06, DOD-24, WPFDB-05, DOD-35 | Contas a pagar/receber, baixas, PDFs e conciliação estão avançados; faltam tesouraria, cobrança, crédito, orçamento, câmbio e homologação integral. |
| S05 | Contabilidade, controladoria e consolidação | 50% | 50% | Redobrada | CAP-06, DOD-32, DOD-35, LEG-06 | Razão, DRE, fechamento e estruturas contábeis existem parcialmente; faltam fatos completos, telas dedicadas, consolidação e intercompany. |
| S06 | Fiscal, tributário e obrigações legais | 35% | 65% | Crítica | CAP-05, LEG-07, REAL-13, DOD-09, WPFDB-04 | Contratos e bloqueios sem sucesso falso existem; homologação SEFAZ, certificados, XML, eventos, SPED e apurações continuam incompletos. |
| S07 | Comercial, vendas, preços e contratos | 58% | 42% | Alta | CAP-02, LEG-02, LEG-03, LEG-10, DOD-04, DOD-35 | Pedidos, PDV, checkout, produtos, cupons e preços têm implementação; a cadeia omnicanal e controles comerciais completos não foram homologados integralmente. |
| S08 | CRM, marketing, atendimento e Yara | 55% | 45% | Alta | CAP-08, LEG-09, LEG-11, DOD-28, DOD-30, REAL-07 | Leads, campanhas, segmentos, MarketingWindow e separação Yara/Sophia existem; jornada completa, atendimento, custo, consentimento e chave oficial ainda faltam. |
| S09 | E-commerce, portal, lojas e omnicanalidade | 72% | 28% | Controlada, ainda não concluída | CAP-02, CAP-13, DOD-27, DOD-29, REAL-04, REAL-05, REAL-06 | Portal dinâmico, mídia, banners, cores, perfis e produtos vinculados estão avançados; publicação final, responsividade, SEO e ciclo completo ainda pendem. |
| S10 | Marketplaces, dropshipping e ecossistema de parceiros | 25% | 75% | Crítica | CAP-12, LEG-12, LEG-13, REAL-01, REAL-02, REAL-03 | Falsos sucessos foram removidos e rotas bloqueiam sem credenciais; homologações oficiais por seller/provedor e reconciliações ainda não existem. |
| S11 | Compras, sourcing e gestão de fornecedores | 74% | 26% | Controlada, ainda não concluída | CAP-03, WPFDB-04, WPFDB-08, WPFDB-09 | Solicitação, cotação, pedido, aprovação, entrada, estoque, financeiro e fornecedor/contato foram provados; faltam RFQ multi-fornecedor, devolução e anexos fiscais. |
| S12 | Estoque, WMS e inventário | 38% | 62% | Crítica | CAP-04, DOD-10, WPFDB-05 | Estruturas de saldos, movimentos, inventários, locais e kardex existem, mas endpoints, telas, concorrência e fluxo WMS não estão homologados integralmente. |
| S13 | Logística, TMS, transporte e comércio exterior | 40% | 60% | Crítica | CAP-07, LEG-08, REAL-11, REAL-12 | Cotação interna oficial e contratos de rastreamento existem; tracking, transportadoras, fretes, CT-e/MDF-e e fulfillment externo não estão homologados. |
| S14 | Manufatura, PCP, MES, qualidade, manutenção e ativos | 20% | 80% | Crítica | CAP-10, DOD-11, WPFDB-04 | Tabelas e rotas de PCP/OPS existem, porém formulários específicos e homologação ponta a ponta são majoritariamente pendentes. |
| S15 | HCM, folha, ponto, saúde e segurança | 22% | 78% | Crítica | CAP-09, DOD-04 | Cadastros e estruturas de RH existem parcialmente; admissão, folha, ponto, benefícios, eSocial e ciclo completo não estão comprovados. |
| S16 | Jurídico, contratos, riscos e ESG | 15% | 85% | Crítica | Matriz: jur_*, vw_processos_risco, vw_contratos_vencer, vw_certidoes_vencer | Há estrutura de tabelas jurídicas e views de risco, mas quase não há evidência de API, formulários ou homologação operacional. |
| S17 | Projetos, serviços, campo, facilities e patrimônio | 10% | 90% | Crítica | Novo escopo corporativo complementar | Não há evidência suficiente de módulos completos; capacidades devem ser incorporadas como expansão planejada sem afetar o núcleo atual. |
| S18 | BI, dados, MDM, analytics e IA corporativa | 48% | 52% | Redobrada | CAP-01, CAP-08, CAP-14, DOD-35, UBD-01, UBD-02 | Dashboard real e estruturas BI existem; faltam catálogo, linhagem, qualidade, métricas corporativas, relatórios completos e governança de IA. |
| S19 | Integrações, SRE, DevSecOps, observabilidade e ITSM | 55% | 45% | Alta | B6, B7, B8, B9, B10, B11, B12, B13, B14, CAP-14, DOD-12 a DOD-20 | Runtime 24h, health, compose, coverage e backup parcial existem; observabilidade, restore, staging, jobs, filas, auto-update e ITSM ainda não fecharam. |
| S20 | Experiência WPF, React, mobile e acessibilidade | 42% | 58% | Redobrada | WPFDB-01 a WPFDB-10, WPF-VIS-01A a WPF-VIS-01F, CAP-11, DOD-20, DOD-26, DOD-29 | Algumas janelas possuem contensão e contratos reais; há três janelas genéricas, outbox sem homologação integral e frente visual global pendente. |
| S21 | Unificação dos bancos e modelo canônico | 8% | 92% | Crítica | UBD-01 a UBD-08, CAP-15, DOD-25 | Os dois schemas operam e o programa UBD está planejado; nenhuma alteração física está autorizada antes de inventário, linhagem, ensaio e reconciliação. |

**Maturidade setorial média de planejamento:** `44.5%`.  
**Lacuna média estimada:** `55.5%`.  
**Métrica oficial estrita preservada:** `22,6%` da DoD histórica e `28,6%` dos bloqueadores.

## Indicadores de Excelência e Resultado Concreto

Os valores abaixo são **metas de aceite**, não alegações do estado atual.

| Dimensão buscada | Indicador mensurável de aceite |
|---|---|
| Excelência | 100% dos itens concluídos com evidência; zero sucesso falso; zero defeito crítico aberto no corte. |
| Competência | Responsável, documentação, teste e operação definidos para 100% dos processos críticos. |
| Capacidade | SLOs de carga definidos por fluxo; testes de capacidade executados; ausência de saturação não monitorada. |
| Dimensão | Empresas, filiais, tenants, lojas, moedas, idiomas, localidades e canais configuráveis sem bifurcar código. |
| Abrangência | 100% dos setores do mapa final possuem processo, formulário, relatório, indicador e automação quando aplicável. |
| Potencialidade | Extensão por contratos, eventos, regras e configurações versionadas, sem dependência de edição manual do banco. |
| Resultado concreto | Redução mensurável de ciclo, erro, retrabalho, inadimplência, ruptura, atraso, indisponibilidade e custo por processo. |
| Integridade | Zero vazamento entre tenants; 100% das escritas críticas auditadas; reconciliação financeira/fiscal/estoque sem divergência não explicada. |
| Qualidade | Build sem erro/aviso acordado, Services >=70% de linhas, integrações e fluxos críticos com testes autenticados. |
| Operação | Disponibilidade, latência, erros, filas, jobs, backups, restores e incidentes monitorados com SLO e responsável. |

### Metas técnicas plausíveis para homologação

- zero confirmação de sucesso sem ID/referência persistida e releitura independente;
- zero endpoint consumido retornando 404 no smoke oficial;
- zero vazamento de dados em teste autenticado com dois tenants;
- 100% das operações financeiras, fiscais e de estoque reconciliadas no ensaio;
- 100% dos formulários WPF mapeados à matriz tabela-endpoint-permissão;
- 100% dos backups programados com resultado verificável e restore semanal comprovado;
- cobertura de linhas de Services nunca inferior ao gate atual de 70%;
- erro HTTP 5xx e latência com SLO definido por endpoint antes do aceite produtivo;
- zero credencial versionada e rotação testada para segredos operacionais;
- zero integração externa marcada ativa sem retorno aceito do provedor e identificador oficial persistido.

## Árvore Estrutural do Estado Final

```text
GenesisGest.Net — ecossistema multiempresarial concluído
|-- 00 Governança, arquitetura e qualidade
|   |-- Portfólio, roadmap, dependências, riscos, ADRs e releases
|   |-- Máquina de estados de tarefas e ramificações com WIP controlado
|   |-- Evidências, homologação, versionamento, rollback e auditoria de entrega
|   `-- DoD reconciliada, mensuração setorial e resultados verificáveis
|-- 01 Plataforma corporativa e segurança
|   |-- Multiempresa, multitenant, empresas, filiais, unidades e localidades
|   |-- SSO, JWT, refresh, MFA, sessões, contas de serviço e clientes de API
|   |-- RBAC, ABAC quando necessário, segregação de funções e revisão de acesso
|   |-- Segredos, certificados, rotação, criptografia e gestão de chaves
|   `-- Workflow, regras, notificações, anexos, comentários, tarefas e sequenciais
|-- 02 GRC, auditoria, privacidade e continuidade
|   |-- Políticas, controles, riscos, evidências e auditorias
|   |-- Trilha imutável, correlação ponta a ponta e investigação
|   |-- Consentimento, retenção, direitos do titular e descarte
|   |-- Incidentes, continuidade, desastre, RPO/RTO e testes de restauração
|   `-- Compliance de terceiros, controles regulatórios e monitoramento
|-- 03 Master Data e estrutura empresarial
|   |-- Pessoas, clientes, fornecedores, parceiros, contatos e endereços
|   |-- Empresas, estabelecimentos, unidades, departamentos e centros
|   |-- Produtos, serviços, categorias, marcas, unidades, preços e lojas
|   |-- Moedas, calendários, localidades, bancos e classificações
|   `-- Qualidade, deduplicação, aprovação, linhagem e publicação governada
|-- 04 Financeiro, tesouraria, contabilidade e controladoria
|   |-- Pagar, receber, cobrança, crédito, bancos e conciliação
|   |-- Caixa, tesouraria, orçamento, forecast, câmbio e financiamentos
|   |-- Plano de contas, lançamentos, partidas, razão e fechamento
|   |-- DRE, balanço, fluxo, custos, rateios e centros de lucro
|   `-- Consolidação, intercompany, eliminações e relatórios estatutários
|-- 05 Fiscal e tributário
|   |-- Regras por empresa, operação, produto, NCM/CEST, CFOP e jurisdição
|   |-- NF-e, NFC-e, NFS-e, CT-e, MDF-e, eventos e contingência
|   |-- XML, protocolos, certificados, numeração e manifestação
|   |-- Apurações, livros, SPED e obrigações acessórias
|   `-- Auditoria, reconciliação e explicabilidade fiscal
|-- 06 Comercial, CRM, marketing e atendimento
|   |-- Leads, oportunidades, cotações, propostas, pedidos e contratos
|   |-- Preços, descontos, promoções, metas, territórios e comissões
|   |-- Campanhas, segmentos, consentimentos e jornada do cliente
|   |-- Tickets, SLA, conhecimento, satisfação e retenção
|   `-- Yara comercial com ferramentas autorizadas, custo controlado e handoff humano
|-- 07 E-commerce, lojas e canais
|   |-- Seis lojas, perfis, domínios, mídia, banners, conteúdo e SEO
|   |-- Catálogo, busca, carrinho, checkout, conta, fidelidade e pós-venda
|   |-- Marketplaces, anúncios, pedidos, estoque, preço, taxas e repasses
|   |-- Dropshipping, roteamento, fornecedor, SLA, comissão e liquidação
|   `-- Publicação responsiva, acessível, observável e sem sucesso fabricado
|-- 08 Compras, abastecimento, estoque e logística
|   |-- Requisição, RFQ, cotação, aprovação, pedido, recebimento e devolução
|   |-- Fornecedores, contratos, avaliação, risco e três vias
|   |-- Depósitos, endereços, saldos, lotes, seriais, reserva e kardex
|   |-- Recebimento, putaway, picking, packing, transferência e inventário
|   `-- Frete, expedição, etiquetas, tracking, ocorrências, CT-e/MDF-e e comércio exterior
|-- 09 Manufatura, qualidade, manutenção e ativos
|   |-- Engenharia, BOM, roteiros, centros de trabalho e mudanças
|   |-- MRP, plano mestre, ordens, materiais, apontamentos e custos
|   |-- MES, capacidade, paradas, OEE, refugo e retrabalho
|   |-- Inspeção, não conformidade, CAPA e rastreabilidade
|   `-- EAM, ativos, planos, ordens, peças, calibração, MTBF e MTTR
|-- 10 HCM, folha e saúde ocupacional
|   |-- Organização, cargos, colaboradores, contratos e admissão
|   |-- Benefícios, dependentes, ponto, escalas, férias e afastamentos
|   |-- Folha, encargos, provisões, pagamentos e eSocial
|   |-- Competências, treinamento, desempenho, carreira e sucessão
|   `-- Desligamento, saúde, segurança, exames, EPIs e incidentes
|-- 11 Jurídico, projetos, serviços, facilities e ESG
|   |-- Contratos, cláusulas, aditivos, obrigações e renovações
|   |-- Processos, andamentos, prazos, certidões e depósitos
|   |-- Portfólio, projetos, WBS, recursos, custos, serviços e campo
|   |-- Imóveis, facilities, patrimônio, reservas e manutenção predial
|   `-- Riscos, seguros, ESG, emissões, resíduos e compromissos
|-- 12 BI, dados e IA
|   |-- Catálogo, glossário, linhagem, qualidade e MDM
|   |-- Camada semântica, KPIs, metas, dashboards e relatórios
|   |-- Analytics, forecast, cenários, anomalias e explicabilidade
|   |-- Governança de modelos, prompts, ferramentas, custos e segurança
|   `-- Indicadores atuais, sem séries fabricadas, reconciliados com fatos
|-- 13 Canais e experiência
|   |-- API Minimal oficial e contratos versionados
|   |-- React responsivo, acessível e administrável
|   |-- Desktop WPF com formulários de domínio e paridade integral
|   |-- Mobile/PWA quando o processo exigir mobilidade
|   `-- Design system, teclado, DPI, localização, performance e auto-update seguro
|-- 14 Integração, SRE, DevSecOps e ITSM
|   |-- APIs, webhooks, filas, jobs, idempotência e dead-letter
|   |-- Logs, métricas, traces, alertas, SLOs e capacidade
|   |-- CI/CD, testes, cobertura, segurança, migrations, staging e rollback
|   |-- Redis, MinIO, backups, restore, DR e operação 24h
|   `-- Incidentes, problemas, mudanças, releases, ativos e vulnerabilidades
`-- 15 Dados canônicos e encerramento
    |-- UBD-01 inventário integral
    |-- UBD-02 linhagem de consumidores
    |-- UBD-03 equivalências e conflitos
    |-- UBD-04 modelo canônico aprovado
    |-- UBD-05 migração idempotente em cópia
    |-- UBD-06 reconciliação de contagens, valores e hashes
    |-- UBD-07 corte controlado com rollback
    `-- UBD-08 estabilidade, desativação e arquivamento seguro
```

## Critério de Encerramento do Último Desenvolvimento

O projeto só atinge o estado final quando, simultaneamente:

1. o denominador da DoD estiver reconciliado e todos os itens oficiais estiverem concluídos;
2. B1 a B14, CAP-01 a CAP-15, WPFDB-01 a WPFDB-10 e WPF-VIS-01A a 01F estiverem aceitos;
3. a matriz dos 269 objetos estiver 100% classificada, vinculada e justificada;
4. nenhum formulário genérico for usado como prova de operação de domínio;
5. todos os fluxos críticos forem correlacionados do cadastro ao BI;
6. integrações externas estiverem homologadas ou formalmente bloqueadas sem sucesso falso;
7. a operação possuir observabilidade, backup, restore, segurança, staging, rollback e continuidade;
8. UBD-01 a UBD-08 estiverem concluídos antes da remoção de qualquer schema legado;
9. todos os setores possuírem formulários, relatórios, gráficos e automações compatíveis com seus processos;
10. a árvore estrutural acima corresponder ao código, runtime, bancos, canais e evidências oficiais.

## Assinatura da Intervenção Documental

```text
Sistema: GenesisGest.Net / Nexum Altivon
Intervenção: consolidação clínica, fluxo mestre, catálogo setorial e árvore estrutural final
Data: 2026-07-19
Escopo: somente arquivos encaminhados no chat; raiz do projeto preservada
Assinado por: Yara 5.5
Função: arquitetura estrutural e planejamento técnico do ecossistema
Natureza: assinatura técnica textual; não substitui assinatura digital formal
```

## Anexo A — Conteúdo integral preservado de `CHECKLIST-CLINICO-GENESISGEST-2026-07-10.md`

O bloco abaixo é uma cópia literal da fonte analisada e existe para garantir que nenhuma referência ou tarefa original seja perdida.

````markdown
<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
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
Diretriz de paridade administrativa entre tabelas e formularios WPF: 2026-07-18.
Apuracao do contrato fiscal WPF e isolamento de configuracoes por tenant: 2026-07-18.
Apuracao de isolamento transacional de Compras e integridade documental no GitHub: 2026-07-18.
Apuracao ponta a ponta de Compras na API, bancos e Desktop WPF: 2026-07-18.
Apuracao ponta a ponta de fornecedores, contatos e concorrencia no Desktop WPF: 2026-07-18.
Apuracao ponta a ponta de clientes, enderecos e concorrencia no Desktop WPF: 2026-07-19.
Apuracao do portal dinamico de lojas, parceiros, banners, cores e catalogo vinculado: 2026-07-19.

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

## Programa de Paridade Administrativa Banco-WPF

Este programa e criterio de aceite do Desktop e nao aumenta isoladamente o numerador da Definition of Done. Cada tabela administrativa deve possuir responsabilidade funcional inequívoca e cobertura WPF comprovada. Tabelas filhas podem integrar abas do formulario de seu agregado, mas nao podem ficar sem operacao, relacao e auditoria visiveis. Tabelas estritamente tecnicas exigem classificacao e justificativa formal para nao receber tela direta.

| ID | Criterio obrigatorio | Status real |
|---|---|---|
| WPFDB-01 Inventario | Catalogar as tabelas de `nexum_altivon` e `genesis_bd` como administrativa, transacional, filha de agregado, integracao, auditoria ou tecnica | Parcial avancado: auditor reproduzivel inventariou 269 objetos; a classificacao automatica integral ainda exige confirmacao clinica |
| WPFDB-02 Linhagem | Vincular cada tabela administrativa a modulo, entidade, endpoint Minimal API, permissao, formulario WPF e operacoes permitidas | Em andamento: 102 objetos possuem referencia literal na API e 34 no Desktop; a correspondencia EF/DTO sem nome fisico ainda precisa ser reconciliada |
| WPFDB-03 Contrato oficial | Proibir acesso MySQL direto no Desktop; toda leitura/escrita deve passar pela API oficial com DTO validado e `CancellationToken` | Concluido para o acesso administrativo ao banco: `ManualNfeWindow` migrou para `DesktopApiClient`, nenhuma janela WPF instancia `HttpClient` para a API e nao existe referencia MySQL direta no Desktop. `DesktopAutoUpdateService` permanece isolado para consultar releases externas do GitHub |
| WPFDB-04 Formulario individual | Entregar formulario profissional especifico para cada processo administrativo, sem tela generica usada como conclusao funcional | Parcial avancado: `ProcurementWindow` opera solicitacao, cotacao, pedido, aprovacao e entrada; `SupplierManagementWindow` opera fornecedores/contatos; `CustomerManagementWindow` opera clientes/enderecos. As tres usam contratos tipados da API oficial. Os demais cadastros corporativos, roteamento fiscal e workspace modular ainda usam submissao generica e bloqueiam o aceite integral |
| WPFDB-05 Separacao de dominios | Contas a pagar, contas a receber, conciliacao, tesouraria, fiscal, estoque e demais dominios devem possuir comandos, validacoes e persistencia separados | Parcial avancado: contas a pagar, contas a receber e conciliacao bancaria possuem janelas, DTOs, comandos, validacoes e persistencia separados; contas a pagar/receber exportam PDFs proprios; tesouraria e demais dominios permanecem pendentes |
| WPFDB-06 Padrao transacional | Cada formulario deve conter contexto, codigo, tenant, status, acoes, cabecalho, itens/servicos, financeiro, anexos, auditoria, totalizadores e validacao em tempo real quando aplicavel | Parcial |
| WPFDB-07 Seguranca | Aplicar JWT em memoria, RBAC, tenant, reautenticacao para acao sensivel e mascaramento de segredos | Parcial |
| WPFDB-08 Integridade | Aplicar concorrencia, idempotencia, tratamento de erro HTTP, releitura persistida e nenhuma confirmacao apenas visual | Parcial avancado: contas a pagar/receber e Compras rejeitam duplicidade/excesso e releem o registro persistido. Fornecedores/contatos e clientes/enderecos exigem `RowVersion`, rejeitam versao antiga com HTTP 409 e so confirmam no WPF depois de nova leitura. Tres janelas ainda chamam `SubmitOperationAsync`; quatro arquivos WPF ainda referenciam `LocalOutboxService`, incluindo o PDV cuja contingencia precisa ser homologada separadamente antes de qualquer remocao |
| WPFDB-09 Relacionamentos | Exibir e operar relacoes pai-filho no agregado correto sem gravacao cruzada ambigua entre dominios | Parcial avancado: contatos sao filhos do fornecedor selecionado e enderecos sao filhos do cliente selecionado; ambos aplicam tenant, concorrencia, soft-delete e auditoria. Os demais agregados administrativos ainda precisam ser reconciliados |
| WPFDB-10 Homologacao | Provar formulario por formulario com API, banco, auditoria, permissao, erro controlado, build WPF e limpeza de dados de validacao | Parcial avancado: contas a pagar/receber, conciliacao, Compras, fornecedores e clientes foram provados na API e banco. Clientes/enderecos confirmou criacao, atualizacao, conflitos 409, soft-delete, seis eventos de auditoria e `ResidualRows = 0` nos percursos local e publico. Os demais formularios permanecem pendentes |

Regra de aceite: a cobertura WPF somente sera concluida quando a matriz tabela-endpoint-formulario-permissao atingir 100%, sem tabelas administrativas orfas e sem formulario que grave em dominio diferente daquele declarado.

Evidencia atualizada em 2026-07-19: `scripts/server/auditar-paridade-banco-wpf.ps1` consulta os dois schemas oficiais e gera `docs/MATRIZ-PARIDADE-BANCO-WPF-2026-07-18.md`. A execucao posterior ao formulario de clientes encontrou 257 tabelas base, 12 views e 17 janelas WPF; 102 objetos possuem referencia literal na API e 35 no Desktop. Restam tres janelas que chamam o endpoint generico e registram payload em `genesis_bd.sys_operacoes_desktop`: `CorporateMasterDataWindow`, `FiscalRoutingWindow` e `ModuleWorkspaceWindow`. Esse registro nao equivale a gravacao nas tabelas do dominio declarado e bloqueia o aceite funcional dessas telas.

## Frente Unica de Layout do Desktop WPF

`WPF-VIS-01` e a unica iniciativa de reformulacao visual do Desktop. As aplicacoes parciais por janela sao incrementos dessa mesma frente e nao autorizam criar tarefas concorrentes com finalidade visual diferente.

| ID | Criterio de aceite | Status real |
|---|---|---|
| WPF-VIS-01A Superficie | Manter identidade e cores; aplicar fundo fume translucido com efeito de cristal/acrylic, permitindo leitura controlada da Area de Trabalho sem reduzir contraste ou legibilidade | Pendente global |
| WPF-VIS-01B Geometria | Padronizar bordas arredondadas discretas, hierarquia, densidade e espacos; eliminar paineis decorativos sem funcao operacional | Pendente global |
| WPF-VIS-01C Contencao | Conter cada janela na area util do monitor, respeitar DPI/resolucao e remover `ScrollViewer` estrutural horizontal/vertical; rolagem interna permanece permitida apenas em grades e campos extensos | Parcial: `ProcurementWindow`, `SupplierManagementWindow` e `CustomerManagementWindow` usam layout fixo contido sem `ScrollViewer` externo |
| WPF-VIS-01D Dashboard | Reduzir poluicao do dashboard, preservar navegacao lateral e menus suspensos e exibir um unico grafico central selecionavel sem sobreposicao | Pendente |
| WPF-VIS-01E Consistencia | Aplicar o mesmo shell visual somente depois de homologar contraste, foco, teclado, redimensionamento e custo de renderizacao, sem alterar a identidade funcional de cada formulario | Pendente |
| WPF-VIS-01F Homologacao | Validar capturas e operacao real em resolucoes e escalas DPI representativas, sem corte de texto, controles fora da area util ou barras estruturais | Pendente |

## Bloqueadores B1 a B14

| ID | Requisito | Status real | Evidencia objetiva |
|---|---|---|---|
| B1 | Solution unica compila API, ERP e Desktop | Concluido local | `NexumAltivon.ERP.sln` lista API, ERP, Desktop e projeto raiz; build Release com 0 erros e 0 avisos |
| B2 | Controllers MVC fora do ativo e endpoints criticos sem 404 | Ajustado e publicado na branch de entrega | API oficial e Minimal API; controllers MVC removidos do projeto da API ativa no commit `4bacfe5`; smoke publico passou nos pontos obrigatorios |
| B3 | Duplicacao massiva da raiz removida do build ativo | Concluido para a raiz legacy | Diretorios legados da raiz foram removidos do build/versionamento no commit `7ccc668`; solution Release seguiu compilando com 0 erros e 0 avisos |
| B4 | `Sys_AuditableEntity` em 100% das entidades transacionais | Parcial avancado | Commit `07a465e` aplicou tenant, soft-delete, auditoria central e `row_version` BLOB no `NexumDbContext`. Em 2026-07-18, as sete tabelas transacionais raw SQL de Compras/Estoque receberam tenant, auditoria, soft-delete, RowVersion e indice `(tenant_id,is_deleted)`; heranca direta ainda nao cobre todas as classes |
| B5 | MFA, refresh-token, tenants e workflows | Parcial avancado | Workflow, MFA TOTP, refresh-token rotativo e logout foram publicados e homologados no runtime oficial. MFA usa segredo protegido por Data Protection/DPAPI persistente, bloqueia reuso TOTP e sobrevive ao reinicio; refresh de usuario e cliente usa apenas hash SHA-256, expira, rotaciona atomicamente e e revogado no logout. Falta homologar integralmente o CRUD e o isolamento de tenants para concluir o bloqueador inteiro |
| B6 | Testes com cobertura minima de 70% em Services | Parcial avancado | Projeto xUnit isolado do binario de producao foi incluido na solution. Em 2026-07-18, `37/37` casos passaram e os Services ativos atingiram `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos; o CI bloqueia a imagem da API abaixo de 70%. Faltam os testes de integracao por modulo com `WebApplicationFactory` para Auth, Pedidos, Compras, Financeiro, Fiscal, Estoque, CRM e RH antes de concluir B6 integralmente |
| B7 | Observabilidade completa | Parcial | Health e Redis existem; Serilog/OpenTelemetry completos ainda nao foram homologados ponta a ponta |
| B8 | Backup diario e restore-test semanal | Parcial | Backup local 2h corrigido para `D:\Nexum Altivon\NexumAltivon.com` e executado com resultado 0; restore-test em CI ainda nao comprovado |
| B9 | EF Migrations | Parcial avancado | Os histories oficiais de Nexum e Genesis foram alinhados aos schemas existentes sem recriar dados. As migrations Nexum `ScopeConfigurationKeysByTenant` e `HardenProcurementTenancy` foram aplicadas em 2026-07-18 com backups previos e sem migrations pendentes; a cadeia completa ainda precisa ser aplicada e reconciliada em banco vazio controlado para concluir B9 |
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
| Gate dos Services 1.1.5.7183 | `dotnet test` executou `37/37` casos em 2026-07-18; cobertura dos Services confirmou `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos com threshold de linhas em 70% |
| Rascunho fiscal WPF | `ManualNfeWindow` removeu URL fixa e `HttpClient` paralelo, usa JWT e DTOs do `DesktopApiClient`, valida documentos, UFs, CFOP e valores, grava pela API e somente confirma salvamento apos reler e comparar o JSON persistido. A homologacao interativa autenticada da janela permanece pendente; os endpoints local e publico responderam 401 sem token e nunca 404 |
| Configuracoes isoladas por tenant | A migration `20260718213331_ScopeConfigurationKeysByTenant` removeu os indices legados somente por `chave` e criou unicidade `(tenant_id, chave)`. O dump previo possui SHA-256 `A9F6B273A1B9B46B708EB489934660C55E6FBD957B2A771852164046D071FC07`; o rascunho existente preservou quantidade, tenant e `updated_at` |
| Publicacao 1.1.5.7183 | API e Desktop compilados com versao `1.1.5.7183`. A API oficial foi republicada somente em `127.0.0.1:5010`; tarefa SYSTEM com gatilho exclusivo de boot, banco e Cloudflared automaticos, health local/publico e os dois bancos responderam 200 |
| Isolamento de Compras por tenant | A migration `20260718220042_HardenProcurementTenancy` preservou as contagens `3/2/2/2/2/2/2` das sete tabelas afetadas, preencheu tenant e RowVersion sem valores invalidos, manteve zero exclusoes e criou os sete indices `(tenant_id,is_deleted)`. A unicidade de numero de pedido passou de global para `(tenant_id,numero)`. O dump previo possui SHA-256 `BC86BACD7A576F80E74C44A7578753D6E75E0C91A6826D94ED3F6AC60BEFDABC` |
| Gate dos Services 1.1.5.7184 | `dotnet test` executou `37/37` casos; cobertura dos Services confirmou `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos. A solution Release compilou API, testes, ERP, Desktop e raiz com 0 erros e 0 avisos |
| Publicacao 1.1.5.7184 | DLL oficial publicada com versao `1.1.5.7184` e SHA-256 `145E756625ED75402636B4C3B40C6C9296F40A291759D9C896C6372775366DE3`. Tarefa SYSTEM possui um gatilho de boot e nenhum de logon; MySQL e Cloudflared estao `Running/Auto`; health local/publico e os dois bancos responderam 200. Oito verificacoes local/publica das rotas de Compras retornaram 401 sem token e nenhuma 404 |
| Compras tipadas no Desktop 1.1.5.7185 | `ProcurementWindow` usa o JWT e o `TerminalProfile` da sessao, DTOs proprios para painel, solicitacao, cotacao, pedido, status e entrada, e somente confirma cada comando depois de nova leitura de `/api/compras/painel`. A janela nao usa `SubmitOperationAsync`, `LocalOutboxService` nem gravacao em `sys_operacoes_desktop`; devolucao a fornecedor permanece explicitamente bloqueada ate existir contrato atomico de estoque, financeiro e fiscal |
| Integridade transacional de Compras | A API exige maquina de estados, aprovacao antes do pedido/recebimento, bloqueio concorrente `FOR UPDATE`, rejeicao explicita de quantidade acima do saldo, documento de entrada nao duplicado, tenant em todas as consultas e auditoria na mesma transacao. As transacoes executam dentro de `MySqlRetryingExecutionStrategy`, enquanto integracao Genesis e releitura ocorrem depois do commit sem repetir a escrita primaria |
| Ensaio autenticado de Compras 1.1.5.7185 | `scripts/server/validar-compras-ponta-a-ponta.ps1` gerou backups dos dois schemas, autenticou usuario controlado, confirmou solicitacao `5`, cotacao `3`, pedido `4`, duas entradas, dois movimentos, estoque final `4`, financeiro Nexum, conta a pagar Genesis e `13` eventos de auditoria. Documento duplicado retornou 409 e excesso retornou 400. A limpeza em `finally` confirmou `ResidualRows = 0` |
| Publicacao 1.1.5.7185 | API republicada somente em `127.0.0.1:5010`; tarefa SYSTEM manteve um gatilho de boot e nenhum de logon, MySQL e Cloudflared permaneceram `Running/Auto`, e health local/publico, Nexum, Genesis e configuracao publica responderam HTTP 200 |
| Fornecedores tipados no Desktop 1.1.5.7186 | `SupplierManagementWindow` substitui a rota generica para Fornecedores e opera lista, cadastro, edicao, contatos e auditoria pela API oficial. Cada gravacao exige JWT, respeita tenant, usa `RowVersion` e somente apresenta conclusao depois de releitura independente; nao usa `SubmitOperationAsync`, `LocalOutboxService` nem `sys_operacoes_desktop` |
| Integridade de fornecedores e contatos | A API ganhou releitura individual de fornecedor, validacoes de documento, e-mail, UF, CEP, loja, comissao, prazo e status, conflito 409 por versao antiga e auditoria na mesma transacao. `md_fornecedor_contatos` recebeu tenant, RowVersion, autoria, soft-delete e indice `(fco_tenant_id,fco_is_deleted)` de forma idempotente; contatos sao sempre validados contra um fornecedor visivel no tenant |
| Ensaio autenticado de fornecedores 1.1.5.7186 | `scripts/server/validar-fornecedores-ponta-a-ponta.ps1` passou pela API local com fornecedor/contato `6/1` e pelo dominio publico com `7/2`. Nos dois percursos autenticou usuario controlado, criou e atualizou fornecedor, rejeitou RowVersion antiga, criou e atualizou contato, rejeitou conflito, confirmou soft-delete, cinco eventos de auditoria, valores no MySQL e `ResidualRows = 0`. Backups SHA-256 `9D4BEA1A5E16903D4E8FBA9B10EE577993E2DC7EAF2C822AFEB29950C4EF7589` e `66AE96517D565D07C0E224E2421E73B74912D6F29A58CB61BFC2A8BB9990A9EF` |
| Gate da entrega 1.1.5.7186 | A solution Release compilou os cinco projetos com 0 erros e 0 avisos; `37/37` testes passaram. A medicao dos Services permaneceu em `70,84%` de linhas, `62,01%` de branches e `83,09%` de metodos; o frontend concluiu `npm run build` sem erro |
| Publicacao operacional 1.1.5.7186 | API oficial republicada somente em `127.0.0.1:5010`; tarefa `NexumAltivonApi24h` permanece `SYSTEM/ServiceAccount`, com um gatilho de boot, nenhum de logon, reinicio automatico, processo invisivel, MySQL e Cloudflared `Running/Auto`. Health local/publico, Nexum, Genesis e configuracao publica responderam HTTP 200 |
| Clientes tipados no Desktop 1.1.5.7187 | `CustomerManagementWindow` substitui a rota generica de Clientes e opera lista, cadastro, edicao, enderecos, arquivamento e auditoria pela API oficial. Cada gravacao exige JWT, respeita tenant, usa `RowVersion` e somente apresenta conclusao depois de releitura independente; nao usa `SubmitOperationAsync`, `LocalOutboxService` nem `sys_operacoes_desktop` |
| Integridade de clientes e enderecos | A API administrativa possui leitura individual, criacao exclusiva sem senha, atualizacao com concorrencia, validacao estrita de nome, e-mail, CPF/CNPJ, tipo, status e campos de contato. Enderecos validam agregado pai, CEP, UF e tipo; endereco principal e ajustado atomicamente; insert, update e soft-delete registram auditoria dentro da transacao e sao relidos de forma independente |
| Ensaio autenticado de clientes 1.1.5.7187 | `scripts/server/validar-clientes-ponta-a-ponta.ps1` passou pela API local com cliente/endereco `41/25` e pelo dominio publico com `42/26`. Nos dois percursos um `Vendedor` autenticado recebeu HTTP 403, enquanto `SuperAdmin` criou e atualizou cliente/endereco, rejeitou `RowVersion` antiga com HTTP 409, confirmou soft-delete, seis eventos de auditoria, valores no MySQL e `ResidualRows = 0`. Backups SHA-256 `24E9A5EE300156519990D3716CB377AB51BB5691219679278D3AB2ECE997413D` e `47EE148774083F7CD6CB1461DF1B4FFA228B48F5AD145BEE2F795D5FCE23E02C` |
| Publicacao operacional 1.1.5.7187 | API oficial republicada somente em `127.0.0.1:5010`; tarefa `NexumAltivonApi24h` validada como `SYSTEM/ServiceAccount`, com um gatilho de boot, nenhum de logon, reinicio automatico e processo invisivel. MySQL e Cloudflared estao `Running/Auto`; health local/publico, Nexum, Genesis e configuracao publica responderam HTTP 200. API compilada/publicada e Desktop carregam a revisao `98f6e8a` no `ProductVersion`; as DLLs compilada e publicada da API possuem o mesmo SHA-256 `D6012958C7E22E5791330F701058A27D74743BC47C3E3FB9090B4112338917FB` |
| Gate da entrega 1.1.5.7187 | A solution Release compilou os cinco projetos com 0 erros e 0 avisos; `37/37` testes da API passaram. O frontend concluiu o build CRACO otimizado com codigo de saida `0` e gerou `main.7a8fb961.js`; o `postbuild-pages.js` executou isoladamente com rastreamento de deprecacao e codigo de saida `0` |
| Integridade documental de rotas | O termo traduzido exibido na captura nao existia no checkout oficial nem em `origin/main`. A imagem apresentava traducao automatica simultanea de metodos e rotas, enquanto o fonte versionado mantinha `GET`, `POST`, `/health` e `/swagger`. As rotas e metodos do README foram protegidos como literais de codigo Markdown para impedir traducao do conteudo tecnico |
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
| Contas a pagar e receber no Desktop WPF | A janela generica foi removida dos dois dominios. `ContasPagarWindow` e `ContasReceberWindow` usam contratos separados da API oficial, JWT em memoria, criacao, baixa/recebimento, totalizadores, releitura persistida e trilha de auditoria. Ensaio autenticado confirmou criacoes 201, duplicidade 409, excesso de baixa 400, baixas parciais 200, dois eventos de auditoria por dominio, linhas `PARCIAL` e movimentos de caixa no MySQL; a limpeza confirmou zero registros controlados residuais |
| Conciliacao bancaria no Desktop WPF | A rota `Conciliação bancária` deixou de abrir a operacao financeira generica e passou a usar `ConciliacaoFinanceiraWindow`, com filtros, selecao do lancamento pago, estado, referencia bancaria, observacoes, totalizadores, releitura e trilha de auditoria. Os dois aliases Minimal API consultam a tabela real `financeiro`, aplicam tenant e soft-delete e persistem em `ctb_conciliacoes` com concorrencia por `row_version`. Ensaio autenticado confirmou consulta 200, lancamento inexistente 404, estado invalido 400, criacao 201, precondicao ausente 428, concorrencia obsoleta 409, atualizacao 200, filtro 200, duas auditorias e limpeza final com zero registro controlado residual |
| PDFs de contas a pagar e receber | `GET /api/erp/genesis/financeiro/contas-pagar/relatorio.pdf` e `GET /api/erp/genesis/financeiro/contas-receber/relatorio.pdf` consultam o MySQL com filtro global de tenant/soft-delete e aceitam periodo/status validado. Colecao vazia retorna 404 e filtro invalido retorna 400. `ContasPagarWindow` e `ContasReceberWindow` implementam download exclusivo de `application/pdf` autenticado, validacao `%PDF-` e gravacao no caminho escolhido; a solution comprovou a integracao WPF, mas o clique interativo no dialogo ainda deve ser homologado no Desktop. Ensaio autenticado local e publico confirmou quatro respostas 200, 2 titulos a pagar, 13 a receber, totais coerentes, PDF A4 paisagem renderizado e revisao `1.1.5.7181`; `33/33` testes passaram e os Services mantiveram `70,84%` de cobertura; arquivos e schema de validacao foram removidos |
| Endurecimento financeiro Genesis | `erp_contas_pagar`, `erp_contas_receber` e `erp_fluxo_caixa` receberam tenant, rowversion binaria, timestamps, usuarios de auditoria e soft-delete, com indice `(tenant_id,is_deleted)`. O `GenesisDbContext` preenche usuario de criacao/alteracao quando o JWT fornece identidade UUID; teste confirma tenant, usuario, timestamp e rowversion de 16 bytes. Migration foi aplicada em schema vazio e removida apos conferencia; no banco oficial o bootstrap preservou 2/13/2 registros e confirmou tenant e rowversion em 100% das linhas |
| Versionamento de entrega | Linha funcional mantida em `1.1.5`; revisao central atual `1.1.5.7187` aplicada a assembly, arquivo, API, Desktop, scripts e cabeçalhos alterados. O identificador de Data Protection permanece estavel em `GenesisGest.Net.v1.1.5` para nao invalidar material protegido entre revisoes |
| Dependencias da solution | A auditoria `dotnet list NexumAltivon.ERP.sln package --vulnerable --include-transitive` identificou `Microsoft.Extensions.Caching.Memory 8.0.0` transitivo no projeto `NexumAltivon_ERP`. O projeto foi alinhado ao EF Core `8.0.29` e fixado em `Microsoft.Extensions.Caching.Memory 8.0.1`; a segunda auditoria confirmou zero pacote vulneravel nos cinco projetos da solution |
| Incidentes pos-commit sem sucesso falso | Falhas de preparacao fiscal, integracao Genesis e notificacao sao persistidas em `notificacoes` e `logs_auditoria` com referencia e codigo seguro. Quando a venda ja foi confirmada, a API retorna HTTP 202 com `StatusFiscal`/`AlertaOperacional`; se nem o incidente puder ser persistido, retorna erro critico com o numero do pedido e proibe reenvio automatico |
| Ensaio controlado do checkout | Cenario fiscal recusado retornou HTTP 202, gravou estado `Preparacao fiscal falhou`, codigo MySQL e auditoria, sem consumir a numeracao. Cenario normal reservou o numero 7, avancou a empresa para 8, gravou pre-emissao, financeiro, pagamento e integracao Genesis; a indisponibilidade real do SendGrid foi registrada e exposta como pendencia. Dados controlados foram removidos e numeracao/estoque foram restaurados aos valores anteriores |
| Validacao integral da entrega de checkout | Em 2026-07-18, a solution Release compilou os cinco projetos com 0 avisos e 0 erros; o frontend compilou o bundle `main.301e9934.js`; `29/29` testes passaram com `70,84%` de cobertura de linhas dos Services. A API foi republicada na porta unica 5010 sob PID `12508`; tarefa `SYSTEM`, boot exclusivo, MySQL/Cloudflared automaticos e health local/publico retornaram HTTP 200 |
| Otimizacao segura do publish oficial | `scripts/server/atualizar-api-oficial-5010.ps1` deixou de repetir o restore quando `obj/project.assets.json` ja existe e publica com `--no-restore`; a ausencia do arquivo interrompe a operacao com instrucao clara. A compilacao Release, os testes, a geracao dos binarios, a troca da API oficial e os health checks permanecem obrigatorios; nao ha copia de DLL antiga nem supressao de build |
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
| Dashboard | Existe em `Dashboard.js` | `/api/dashboard/resumo` e `/api/admin/dashboard/completo` | Parcial avancado | Em 2026-07-18, a serie semanal estatica e os percentuais/tendencias fabricados foram removidos. `/api/dashboard/resumo?dias=7|30|90` passou a consolidar somente pagamentos aprovados, excluindo cancelados e reembolsados, no fuso de Sao Paulo e sob os filtros globais de tenant e soft-delete. A API local e a publica retornaram series identicas; 90 dias totalizaram 6 pedidos e R$ 5.289,73, exatamente iguais a consulta direta no MySQL. Ainda faltam prova com dois tenants distintos e homologacao visual publicada no Chrome. |
| Pedidos | Existe | `/api/pedidos*`, MySQL `pedidos` | Parcial validado | Lista e alteracao de status existem; fluxo venda-pagamento-fiscal-logistica-financeiro ainda nao foi homologado integralmente |
| Produtos | Existe | `/api/produtos*`, MySQL `produtos` | Parcial validado | CRUD e catalogo existem; falta homologar todas as restricoes de estoque/tenant e marketplace |
| Clientes | Existe | `/api/clientes*`, MySQL `clientes` | Parcial validado | CRUD, portal e confirmacao existem; falta teste completo de isolamento e historico operacional |
| Lojas | Sem tela dedicada equivalente | `/api/lojas*`, MySQL `lojas` | Parcial | Dados aparecem em dashboard/empresas; falta gestao dedicada das seis lojas com gravacao e permissoes |
| Financeiro | Existe | `/api/financeiro*` e `/api/erp/genesis/financeiro*` | Parcial avancado | Contas a pagar/receber, PDFs de pagar/receber e conciliacao bancaria possuem contratos e telas WPF proprios, persistencia, tenant e auditoria comprovadas. Razao, fechamento e DRE ainda requerem implementacao/homologacao integral em tela e API oficial |
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
| CAP-01 | Cockpit executivo dinamico | Parcial avancado | Series reais de 7, 30 e 90 dias foram conferidas entre API local, API publica e MySQL em 2026-07-18; concluir com teste de isolamento entre dois tenants e homologacao visual publicada no Chrome |
| CAP-02 | Venda omnicanal | Parcial | Correlacao unica de pedido, pagamento, estoque, fiscal, entrega e financeiro |
| CAP-03 | Compras e abastecimento | Parcial avancado | Solicitacao, cotacao vinculada, maquina de estados, pedido, entrada parcial/final, estoque, financeiro Nexum, conta a pagar Genesis, tenant e auditoria foram comprovados por HTTP e MySQL com limpeza zero; faltam cotacao automatica multi-fornecedor, devolucao atomica e anexos fiscais da compra |
| CAP-04 | WMS | Parcial | Movimentacao, inventario, kardex, localizacao e transferencia com concorrencia |
| CAP-05 | Fiscal explicavel | Parcial | Roteamento, numeracao, XML, chave, protocolo e eventos oficiais visiveis e persistidos |
| CAP-06 | FICO corporativo | Parcial avancado | Contas a pagar/receber, baixas, PDFs de pagar/receber e conciliacao bancaria estao integrados entre WPF, API, banco e auditoria; faltam tesouraria, fechamento, razao e DRE integralmente homologados em tela |
| CAP-07 | Logistica e fulfillment | Parcial | Cotacao, expedicao, tracking externo, entrega e ocorrencias comprovados |
| CAP-08 | CRM e atendimento Yara | Parcial bloqueado por credencial | Obter chave de projeto da OpenAI com faturamento e limite de gasto definidos; validar o motor Yara exclusivamente no portal com streaming, contexto somente leitura e ferramentas autorizadas para catalogo, estoque, precos, carrinho, checkout e transferencia humana. A Yara deve conduzir a jornada comercial do cumprimento ao fechamento sem receber, registrar ou repetir dados sigilosos de pagamento. A assinatura ChatGPT nao sera tratada como chave da API e, sem chamada oficial aceita, a rota permanece HTTP 503 sem resposta fabricada |
| CAP-09 | HCM | Parcial | Admissao, ponto, folha, beneficios, avaliacao, desligamento e eSocial comprovados |
| CAP-10 | MES e OPS | Parcial | OS, apontamento, manutencao e ativos com telas React/WPF especificas |
| CAP-11 | Paridade Desktop | Parcial avancado | Contas a pagar/receber, conciliacao bancaria e Compras usam formularios e contratos proprios com releitura; faltam substituir as tres janelas genericas restantes e reconciliar toda a matriz |
| CAP-12 | Marketplaces e dropshipping | Bloqueado externamente | Retorno aceito e identificador oficial de cada seller/provedor persistido |
| CAP-13 | Portal visual dinamico | Parcial avancado | Perfis estruturados das seis lojas e entidades parceiras, contatos publicos autorizados, cores globais e individuais, banners ordenaveis, rodizio maximo de quatro parceiros e produtos vinculados foram implementados na API/MySQL, React e configuracao WPF. O ensaio autenticado confirmou gravacao, releitura, bloqueios de integridade e limpeza sem residuos. Falta concluir publicacao do bundle e homologacao Chrome responsiva das paginas publicas para aceite integral |
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
| Multitenancy e soft-delete em 100% das entidades | Parcial | As sete tabelas raw SQL de Compras/Estoque foram migradas e validadas sem perda; continuar as entidades legadas restantes e executar prova autenticada de isolamento entre dois tenants |
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
| Paridade integral entre tabelas administrativas e WPF | Pendente programado | Cumprir WPFDB-01 a WPFDB-10: inventariar e reconciliar 100% da matriz tabela-endpoint-formulario-permissao, separar dominios e homologar cada formulario na API, banco, auditoria e Desktop |
| Unificacao de `nexum_altivon` e `genesis_bd` | Pendente programado, sem alteracao fisica autorizada nesta etapa | Cumprir UBD-01 a UBD-08 com inventario, linhagem, modelo canonico, ensaio, reconciliacao, backup, reversao e corte controlado; somente entao remover a segunda connection string |
| Reestruturacao visual do Desktop WPF | `WPF-VIS-01` pendente, com contencao parcialmente aplicada em Compras e Fornecedores | Executar exclusivamente os criterios `WPF-VIS-01A` a `WPF-VIS-01F` da frente unica acima. O tema acrylic global e a simplificacao do dashboard continuam pendentes; nenhuma iniciativa visual paralela ou duplicada deve ser aberta |
| Portal dinamico administravel | Parcial avancado | Biblioteca de midia, selecao/upload de banners, ordenacao, ativacao, substituicao, perfis publicos normalizados e relacao perfil-produto operam pela API oficial. As seis lojas possuem perfil controlado; fornecedores, parceiros de compra/venda e marketplaces podem publicar somente campos autorizados, cores e produtos reais vinculados. O rodizio React exibe no maximo quatro parceiros por vez. Configuracoes globais de seis cores e intervalos sao editaveis no React e Desktop WPF e o salvamento e auditado, relido e confirmado no MySQL. O ensaio `validar-portal-dinamico-ponta-a-ponta.ps1` confirmou CRUD, cores, dois banners, produto real, referencias externas rejeitadas, midia referenciada protegida e `ResidualRows=0`; oito URLs publicas oficiais de logo, banners e lojas responderam HTTP 200. Falta publicar o bundle e homologar visualmente desktop/mobile no Chrome para concluir |
| Motor comercial Yara | Pendente programado, bloqueado por credencial oficial | Manter Yara exclusivamente no frontend e Sophia exclusivamente no administrativo. Adquirir chave de projeto na plataforma OpenAI, definir limite de gasto, salvar cifrada pelo Desktop sob `SuperAdmin`, validar o modelo no provedor e somente entao homologar streaming, historico consentido, catalogo/estoque/preco em tempo real, carrinho, checkout, transferencia humana, observabilidade, limite de custo e protecao contra coleta de dados sigilosos. Sem chave valida, retornar erro rastreavel e nunca gerar resposta local aparente |

Referencia oficial para aquisicao da credencial Yara: o faturamento do ChatGPT e da API e separado. A chave deve ser criada em um projeto da plataforma OpenAI, com faturamento da API habilitado e orçamento/limites acompanhados no proprio projeto: `https://help.openai.com/en/articles/9039756-billing-settings-in-chatgpt-vs-platform`, `https://platform.openai.com/docs/quickstart/make-your-first-api-request` e `https://help.openai.com/en/articles/9186755-managing-projects-in-the-api-platform`.
| Evolucao visual do portal sem alterar a identidade | Pendente | Revisar Home, catalogo, lojas e areas de produto com hierarquia visual mais forte, imagens reais administraveis, estados de carregamento/erro consistentes e validacao Chrome em desktop e mobile; manter cores, marca, navegacao e linguagem atuais |
| Sugestoes tecnicas Yara 5.5 | Em avaliacao continua | Em 2026-07-14 foram incorporados na janela WPF de Dropshipping os principios uteis de navegacao existente, Grid responsivo, validacao local, API real, retorno de erro e confirmacao de ID/RowVersion. Em 2026-07-18, o material de workflow/tabelas foi reavaliado para Compras: foram aproveitados densidade, organizacao e formulario por processo; o CRUD MySQL direto e o armazenamento local de credenciais foram rejeitados porque contornariam API, JWT, RBAC, tenant, auditoria e regras de negocio. Configuracoes sensiveis tambem nao usam outbox offline por dependerem do estado atual do servidor |
| TLS ativo sem HTTP publico puro | Parcial | Porta 443 responde; validar politica completa Cloudflare/nginx |
| AGENTS e OpenAPI | Parcial | `AGENTS.md` atualizado; `/swagger/v1/swagger.json` respondeu 200 no smoke publico |
| Termos proibidos no codigo ativo | Parcial | Blocos commitados nesta auditoria foram varridos; ainda restam Desktop, docs antigos e services legados nao compilados para triagem |
| Header de IP em todos os arquivos | Parcial | Blocos commitados nesta auditoria receberam header; falta varredura completa nos arquivos remanescentes |
| Fluxo cadastro a BI isolado por empresa | Pendente | Executar roteiro ponta a ponta com dados reais |
| Ferramentas ficticias convertidas em reais ou bloqueadas claramente | Em execucao | O login administrativo alternativo fora do banco foi removido; MFA, refresh e logout persistem e revogam estado real. Shopee/Amazon deixam de gravar sucesso falso; marketplace sync Mercado Livre/B2W/Via exige chamada externa; PDFs de pagar/receber foram homologados local/publico e no WPF; tracking externo e emissao/eventos NF-e/NFC-e bloqueiam sem configuracao oficial; checkout registra falha fiscal, Genesis e notificacao depois da venda sem ocultar a pendencia | Concluir demais achados da secao acima e validar com credenciais reais de cada integracao externa |
| Falha fiscal engolida apos venda | Concluido no escopo do checkout | A venda confirmada preserva o pedido, registra estado fiscal pendente, notificacao e auditoria com referencia/codigo seguro e responde HTTP 202. Se o incidente nao puder ser persistido, a API retorna erro critico com numero do pedido e proibe reenvio automatico. Ensaios de falha e caminho normal foram conferidos no MySQL e integralmente limpos |

## GitHub e Commit

Estado apurado:

- Branch local: `work/delivery-2026-06-13`.
- Base local e remota antes da entrega `1.1.5.7186`: `e853ec2 feat: compras - operar fluxo real no desktop`.
- Antes do commit de fornecedores, `HEAD`, `origin/work/delivery-2026-06-13` e `origin/main` estavam alinhados sem divergencia no commit `e853ec2`.
- Endurecimento transacional do checkout: `4b4c3ba fix: checkout - registrar incidentes pos-commit reais`.
- Fornecedores tipados, persistencia, concorrencia e auditoria no Desktop/API: `597eeee feat: fornecedores - operar cadastro no desktop`.
- Clientes e enderecos tipados, persistencia, concorrencia, RBAC e auditoria no Desktop/API: `98f6e8a feat: clientes - operar cadastro no desktop`.
- A entrega documental consolida a arvore real/meta, as capacidades recuperadas e as evidencias tecnicas; cada hash remoto deve ser conferido depois do respectivo push.
- Commits atomicos enviados nesta rodada de saneamento: `8d58edb`, `0b8212e`, `a7c23d9`, `5848622`, `fd988d0`, `d766314`, `4918d30`, `3879914`, `937512d`, `404468b`, `6dbf1f0`.
- Publicacao GitHub permanece seletiva e auditada; nao publicar arquivos legados/falsos sem validacao direta no projeto oficial.

## Regra de continuidade

Nao declarar item como concluido sem uma evidencia tecnica associada: build, teste, rota HTTP, consulta de banco, migracao aplicada, arquivo publicado, commit remoto ou validacao visual quando o item for de interface.
````

## Anexo B — Conteúdo integral preservado de `ARVORE-REAL-E-META-GENESISGEST-2026-07-18.md`

O bloco abaixo é uma cópia literal da fonte analisada e existe para garantir que nenhuma referência ou tarefa original seja perdida.

````markdown
<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7190
-->

# Arvore Real e Arvore Meta do GenesisGest.Net

Apuracao tecnica: 2026-07-18.

## Regra de Evidencia

O arquivo `ANALISE_ESTRUTURAL_GRUPO_NEXUM_ALTIVON2.txt` foi usado como referencia historica de amplitude, nao como prova de implementacao. Uma capacidade somente integra a arvore real quando existe no projeto oficial e possui evidencia verificavel. A arvore meta preserva as capacidades uteis do documento, mas exige API oficial, autorizacao, persistencia quando aplicavel, tratamento de erro, interface oficial e validacao operacional antes da conclusao.

Classificacoes usadas:

- `Comprovado`: contrato e comportamento foram validados no runtime oficial.
- `Parcial`: existe implementacao real, mas falta pelo menos um gate obrigatorio.
- `Bloqueado externamente`: o codigo recusa operacao sem credencial, certificado ou aceite do provedor e nao registra sucesso.
- `Nao comprovado`: nao existe evidencia suficiente para uso produtivo.

## Inventario Estrutural Atual

| Camada | Evidencia em 2026-07-18 |
|---|---|
| Solution .NET | 5 projetos: API, API.Tests, ERP, Desktop e projeto raiz |
| API oficial | 236 rotas Minimal API em `NexumAltivon_Back-End/API/Program.cs` |
| Frontend | 16 paginas React em `NexumAltivon_Front-End/src/pages` |
| Desktop | 14 arquivos XAML funcionais fora de `bin/obj` |
| Banco Nexum | 203 tabelas base e 12 views no schema `nexum_altivon` |
| Banco Genesis | 48 tabelas base no schema `genesis_bd` |
| Runtime | API unica em `127.0.0.1:5010`, tarefa `NexumAltivonApi24h` como `SYSTEM`, gatilho de boot e processo oculto |
| Exposicao | Cloudflared automatico; health local e publico comprovado |

## Arvore Real

```text
GenesisGest.Net v1.1.5
|-- Plataforma oficial
|   |-- API Minimal .NET 8 [comprovada em runtime]
|   |-- React 19 [build e publicacao existentes]
|   |-- Desktop WPF .NET 8 [solution e acesso local existentes]
|   `-- MySQL 8: nexum_altivon + genesis_bd [health comprovado]
|-- 00 Nucleo e SSO [parcial avancado]
|   |-- Login, JWT, refresh rotativo e logout
|   |-- MFA TOTP
|   |-- Usuarios, perfis, permissoes e RBAC
|   |-- Tenants [isolamento integral ainda nao comprovado]
|   `-- Workflow [backend comprovado; telas por dominio pendentes]
|-- 01 Cockpit executivo [parcial]
|   |-- Resumo e KPIs por API
|   `-- Grafico semanal estatico ainda presente no Dashboard React
|-- 02 GRC e IAM [parcial avancado]
|   |-- Gestao administrativa React/WPF
|   `-- Auditoria consultavel [cobertura de toda escrita pendente]
|-- 03 Master Data [parcial]
|   |-- Empresas, clientes, fornecedores, produtos e categorias
|   |-- Pessoas, centros de custo e itens/servicos
|   `-- Gestao dedicada das seis lojas pendente
|-- 04 FICO [parcial]
|   |-- Lancamentos, razao, conciliacao, DRE e fechamento
|   |-- PDFs de contas a pagar/receber integrados no WPF e homologados na API local/publica com dados MySQL
|   `-- PDF de DRE/razao/fechamento ainda pendente de fatos contabeis completos
|-- 05 SCM e WMS [parcial]
|   |-- Compras, solicitacoes, cotacoes, pedidos e entradas
|   `-- WMS completo e portaria ainda sem homologacao ponta a ponta
|-- 06 Comercial, CRM e Fiscal [parcial]
|   |-- Pedidos, PDV, checkout e catalogo
|   |-- Cupons [CRUD e uso publico comprovados]
|   |-- Campanhas e segmentos [CRUD comprovado]
|   |-- Checkout com incidente fiscal/pos-commit duravel
|   |-- Pre-emissao fiscal e reserva atomica de numero
|   `-- SEFAZ e marketplaces [bloqueados externamente sem credenciais]
|-- 07 HCM e RH [parcial]
|   `-- Resumo e colaboradores; folha, ponto e eSocial sem aceite integral
|-- 08 MES e OPS [parcial]
|   |-- Rotas de OS, producao, manutencao e ativos
|   `-- Telas React/WPF especificas e fluxo integral pendentes
|-- Integracoes [parcial]
|   |-- Frete interno oficial identificado
|   |-- Tracking externo bloqueado sem provedor
|   |-- Yara no portal e Sophia no administrativo, sem resposta fabricada
|   `-- Anexos/MinIO, Redis e jobs ainda exigem homologacao produtiva
`-- Operacao 24h [comprovada no servidor atual]
    |-- MySQL, Cloudflared e Agendador automaticos
    `-- API sobe no boot sem login interativo
```

## Arvore Meta de Aceite Integral

```text
GenesisGest.Net concluido
|-- Plataforma e seguranca
|   |-- Tenant, auditoria, concorrencia e soft-delete em toda entidade transacional
|   |-- Segregacao de funcoes e autorizacao comprovadas por modulo
|   |-- Segredos somente em armazenamento privado e rotacionavel
|   `-- Banco canonico unico apos UBD-01 a UBD-08
|-- Experiencia multicanal
|   |-- React dinamico, responsivo e administravel por midia real
|   |-- WPF com paridade funcional, navegacao limpa e visual fume translucido
|   `-- Nenhum botao confirma operacao sem resposta persistida
|-- Operacao corporativa 00 a 08
|   |-- Cada formulario com contexto, cabecalho, itens, financeiro, anexos e auditoria
|   |-- Cada escrita com transacao, tenant, RBAC, concorrencia e trilha
|   `-- Cada leitura derivada de dados atuais, sem indicadores estaticos
|-- Cadeia ponta a ponta
|   |-- Cadastro -> venda -> pagamento -> estoque
|   |-- Estoque -> fiscal homologado -> logistica
|   `-- Logistica -> financeiro -> contabil -> BI
|-- Integracoes externas
|   |-- SEFAZ com chave e protocolo de homologacao persistidos
|   |-- Marketplace com identificador aceito pelo seller persistido
|   |-- Tracking com evento retornado pelo transportador
|   `-- IA com chave administrada, custo controlado e erro rastreavel
|-- Resiliencia e operacao
|   |-- Redis, jobs, anexos, observabilidade e backups homologados
|   |-- Restore semanal comprovado
|   |-- Staging e producao com migrate e health gates
|   `-- Auto-update Desktop validado em maquina cliente
`-- Qualidade
    |-- Solution e frontend compilam sem erro
    |-- Services mantem cobertura minima de 70%
    |-- Contratos consumidos nao retornam 404
    `-- Definition of Done em 31/31 com evidencia anexada ao checklist
```

## Capacidades Recuperadas e Aprofundadas

| ID | Capacidade | Estado atual | Gate para conclusao |
|---|---|---|---|
| CAP-01 | Cockpit executivo dinamico | Parcial avancado | Series estaticas removidas e periodos 7/30/90 reconciliados entre API local, API publica e MySQL; falta provar isolamento entre dois tenants e homologar a tela publicada no Chrome |
| CAP-02 | Venda omnicanal | Parcial | Provar pedido, pagamento, reserva/baixa, fiscal, entrega e financeiro na mesma correlacao |
| CAP-03 | Compras e abastecimento | Parcial | Provar solicitacao, cotacao, aprovacao, pedido, entrada, estoque e financeiro |
| CAP-04 | WMS | Parcial | Provar movimentacao, inventario, kardex, localizacao e transferencia com concorrencia |
| CAP-05 | Fiscal explicavel | Parcial | Exibir roteamento, motivo, numeracao, XML, chave, protocolo e eventos oficiais |
| CAP-06 | FICO corporativo | Parcial avancado | Contas a pagar/receber, baixas, PDFs de pagar/receber e conciliacao bancaria usam janelas, contratos, persistencia, tenant, concorrencia e auditoria proprios; PDFs foram homologados na API local/publica e o acionamento WPF compilado; concluir homologacao interativa WPF, tesouraria, fechamento, razao e DRE |
| CAP-07 | Logistica e fulfillment | Parcial | Provar cotacao, expedicao, rastreamento externo, entrega e ocorrencias |
| CAP-08 | CRM e atendimento Yara | Parcial bloqueado por credencial | Provar jornada comercial com chave de projeto OpenAI real, streaming, ferramentas autorizadas de catalogo/estoque/preco/carrinho/checkout, transferencia humana, limite de custo e protecao de dados sigilosos; sem seletor de persona e sem resposta fabricada |
| CAP-09 | HCM | Parcial | Provar admissao, ponto, folha, beneficios, avaliacao, desligamento e eSocial |
| CAP-10 | MES e OPS | Parcial | Entregar telas React/WPF e provar OS, apontamento, manutencao e ativos |
| CAP-11 | Paridade Desktop | Parcial avancado | Contas a pagar/receber e conciliacao bancaria confirmam releitura; substituir as cinco janelas genericas e reconciliar 100% da matriz tabela-endpoint-formulario-permissao |
| CAP-12 | Marketplaces e dropshipping | Bloqueado externamente | Homologar cada seller e provedor com credencial e identificador oficial |
| CAP-13 | Portal visual dinamico | Parcial avancado | API/MySQL, React e WPF ja administram banners, cores, seis lojas, parceiros, contatos autorizados e produtos vinculados; concluir publicacao do bundle e homologacao Chrome desktop/mobile |
| CAP-14 | Operacao observavel | Parcial | Comprovar logs, traces, metricas, jobs, Redis, backup e restore |
| CAP-15 | Banco canonico | Pendente programado | Cumprir inventario, linhagem, equivalencias, migracao, reconciliacao e reversao |

## Prioridade de Execucao

1. Encerrar erros que possam confirmar venda ou escrita sem registrar incidentes.
2. Substituir dados estaticos e acoes apenas visuais por contratos persistidos.
3. Completar um fluxo vertical por vez, preservando os gates comuns de seguranca.
4. Homologar integracoes externas somente com credenciais oficiais e retorno do provedor.
5. Consolidar React e WPF sobre a mesma API antes da futura unificacao fisica dos schemas.

Este mapa nao altera a medicao do checklist: capacidades parciais permanecem fora do numerador ate o atendimento integral do respectivo gate.
````

## Anexo C — Conteúdo integral preservado de `MATRIZ-PARIDADE-BANCO-WPF-2026-07-18.md`

O bloco abaixo é uma cópia literal da fonte analisada e existe para garantir que nenhuma referência ou tarefa original seja perdida.

````markdown
<!--
Propriedade intelectual: Luís Rodrigo da Costa
Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
Sistema de gestão: GenesisGest.Net
Ano Início: 04/2024 Publicado e operacional: 05/2026
Versão: 1.1.5.7187
-->

# Matriz de Paridade Banco-WPF

Gerado em: 2026-07-19T00:14:29-03:00

Fonte de banco: `information_schema.TABLES` dos schemas oficiais em `127.0.0.1:3309`. Fonte de codigo: arquivos `.cs` da API e arquivos `.cs`/`.xaml` do Desktop no checkout oficial. Nenhuma credencial e gravada neste relatorio.

A classificacao desta primeira etapa e preliminar e baseada em regras de nome declaradas no auditor. Referencia literal significa somente que o nome fisico da tabela foi localizado no codigo; nao comprova CRUD, formulario, permissao, auditoria ou homologacao. Ausencia de referencia literal tambem nao prova ausencia funcional, pois EF Core e DTOs podem usar nomes de entidade. A confirmacao clinica pertence ao WPFDB-02.

## Resumo Factual

| Medida | Quantidade |
|---|---:|
| Objetos inventariados | 269 |
| Schema `genesis_bd` | 53 |
| Schema `nexum_altivon` | 216 |
| Tabelas base | 257 |
| Views | 12 |
| Com referencia literal na API | 102 |
| Com referencia literal no Desktop | 35 |
| Janelas WPF | 17 |

## Classificacao Preliminar

| Classe | Quantidade |
|---|---:|
| Administrativa | 152 |
| Auditoria | 15 |
| Filha de agregado | 19 |
| Integracao | 2 |
| Tecnica | 23 |
| Transacional | 46 |
| View | 12 |

## Achados Impeditivos do Desktop

As janelas abaixo chamam `SubmitOperationAsync`. O servico associado comprova gravacao em `genesis_bd.sys_operacoes_desktop`; esse registro generico de payload nao comprova persistencia nas tabelas de dominio declaradas pela tela e nao pode sustentar mensagem de conclusao funcional:

- `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`
- `NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`
- `NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml.cs`

As janelas abaixo gravam contingencia por `SaveOperationAsync` ou `SaveSaleAsync`. A homologacao WPFDB-08 exige inventariar e provar reprocessamento, idempotencia, confirmacao no servidor e remocao segura do arquivo antes de considerar o fluxo offline concluido:

- `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`
- `NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`
- `NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml.cs`
- `NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`

As janelas abaixo instanciam `HttpClient` diretamente e devem ser reconciliadas com `DesktopApiClient` para centralizar autenticacao, endpoints, cancelamento e tratamento de erro:


## Inventario Integral

| Schema | Objeto | Tipo | Classe preliminar | Referencias literais API | Referencias literais Desktop |
|---|---|---|---|---|---|
| `genesis_bd` | `__efmigrationshistory` | Tabela | Tecnica | - | - |
| `genesis_bd` | `aggregatedcounter` | Tabela | Tecnica | - | - |
| `genesis_bd` | `clientes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/ClienteService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoPagoService.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `genesis_bd` | `counter` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Program.cs` | - |
| `genesis_bd` | `distributedlock` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_avaliacoes_fornecedor` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_boletos` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs` | - |
| `genesis_bd` | `erp_centros_custo` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `genesis_bd` | `erp_contas_bancarias` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_contas_pagar` | Tabela | Transacional | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/ContasPagarWindow.xaml`<br>`NexumAltivon.Desktop/ContasPagarWindow.xaml.cs` |
| `genesis_bd` | `erp_contas_receber` | Tabela | Transacional | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/ContasReceberWindow.xaml`<br>`NexumAltivon.Desktop/ContasReceberWindow.xaml.cs` |
| `genesis_bd` | `erp_financeiro_referencias` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs` | - |
| `genesis_bd` | `erp_fluxo_caixa` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `genesis_bd` | `erp_fornecedores` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `genesis_bd` | `erp_impostos_config` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_interacoes_crm` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_inventarios` | Tabela | Transacional | - | - |
| `genesis_bd` | `erp_itens_inventario` | Tabela | Transacional | - | - |
| `genesis_bd` | `erp_itens_nota_fiscal` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_kardex` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_leads_crm` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_locais_estoque` | Tabela | Transacional | - | - |
| `genesis_bd` | `erp_logs` | Tabela | Auditoria | - | - |
| `genesis_bd` | `erp_movimentacoes_estoque` | Tabela | Transacional | - | - |
| `genesis_bd` | `erp_notas_fiscais` | Tabela | Administrativa | - | - |
| `genesis_bd` | `erp_rh_colaboradores` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs` | - |
| `genesis_bd` | `erp_rh_referencias` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs` | - |
| `genesis_bd` | `erp_tarefas_crm` | Tabela | Administrativa | - | - |
| `genesis_bd` | `hash` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDesktopOperationService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisRhService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Logistics/LogisticaTrackingService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Storage/AnexoStorageService.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/Usuario.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AuthService.cs`<br>`NexumAltivon_Back-End/API/Services/ClienteService.cs` | `NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `genesis_bd` | `job` | Tabela | Tecnica | - | - |
| `genesis_bd` | `jobparameter` | Tabela | Tecnica | - | - |
| `genesis_bd` | `jobqueue` | Tabela | Tecnica | - | - |
| `genesis_bd` | `jobstate` | Tabela | Administrativa | - | - |
| `genesis_bd` | `list` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Configurations/HangfireAuthorizationFilter.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ApiResponse.cs`<br>`NexumAltivon_Back-End/API/DTOs/CarrinhoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CheckoutDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ClienteDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/DropshippingDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/LogisticaDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/MarketplaceDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/PedidoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ProdutoDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisRhService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/FinancePdfReportService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Logistics/LogisticaTrackingService.cs`<br>`NexumAltivon_Back-End/API/Mappings/MappingProfile.cs`<br>`NexumAltivon_Back-End/API/Middleware/ExceptionMiddleware.cs`<br>`NexumAltivon_Back-End/API/Models/CrmSegmento.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/CarrinhoService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/ClienteService.cs`<br>`NexumAltivon_Back-End/API/Services/CrmService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/FreteService.cs`<br>`NexumAltivon_Back-End/API/Services/IErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs`<br>`NexumAltivon_Back-End/API/Services/LojaService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceHubService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/ProdutoService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | `NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml.cs`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/Models/PdvSaleDraft.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs`<br>`NexumAltivon.Desktop/Services/DesktopAutoUpdateService.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `genesis_bd` | `lojas` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/LojaService.cs` | `NexumAltivon.Desktop/MainWindow.xaml` |
| `genesis_bd` | `pedidoitem` | Tabela | Transacional | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/PedidoDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Mappings/MappingProfile.cs`<br>`NexumAltivon_Back-End/API/Models/Pedido.cs`<br>`NexumAltivon_Back-End/API/Models/PedidoItem.cs`<br>`NexumAltivon_Back-End/API/Models/Produto.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/PedidoService.cs` | `NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `genesis_bd` | `pedidos` | Tabela | Transacional | `NexumAltivon_Back-End/API/Configurations/ServiceExtensions.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/DropshippingDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/LogisticaDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Models/Pedido.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/FinanceiroService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoPagoService.cs`<br>`NexumAltivon_Back-End/API/Services/NotificacaoService.cs`<br>`NexumAltivon_Back-End/API/Services/PedidoService.cs` | `NexumAltivon.Desktop/LogisticsWindow.xaml`<br>`NexumAltivon.Desktop/LogisticsWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `genesis_bd` | `produtos` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/DropshippingDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/MarketplaceDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/Categoria.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Models/Fornecedor.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Models/Produto.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/CarrinhoService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/FreteService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceHubService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/ProdutoService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `genesis_bd` | `qrtz_blob_triggers` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_calendars` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_cron_triggers` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_fired_triggers` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_job_details` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_locks` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_paused_trigger_grps` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_scheduler_state` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_simple_triggers` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_simprop_triggers` | Tabela | Administrativa | - | - |
| `genesis_bd` | `qrtz_triggers` | Tabela | Administrativa | - | - |
| `genesis_bd` | `server` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/DesignTimeDbContextFactories.cs`<br>`NexumAltivon_Back-End/API/Middleware/ExceptionMiddleware.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/Models/TerminalProfile.cs`<br>`NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml`<br>`NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml.cs`<br>`NexumAltivon.Desktop/NetworkSettingsWindow.xaml.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `genesis_bd` | `set` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/DesignTimeDbContextFactories.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ApiResponse.cs`<br>`NexumAltivon_Back-End/API/DTOs/AuthDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CarrinhoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CheckoutDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ClienteDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CrmDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/DropshippingDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/LogisticaDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/LojaDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/MarketplaceDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/PagamentoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/PedidoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ProdutoDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDesktopOperationService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisRhService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/FinancePdfReportService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Logistics/LogisticaTrackingService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Storage/AnexoStorageService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Tenancy/ITenantContext.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Tenancy/TenantContext.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Tenancy/TenantResolverMiddleware.cs`<br>`NexumAltivon_Back-End/API/Middleware/RateLimitingMiddleware.cs`<br>`NexumAltivon_Back-End/API/Models/Carrinho.cs`<br>`NexumAltivon_Back-End/API/Models/Categoria.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/ConfiguracaoSistema.cs`<br>`NexumAltivon_Back-End/API/Models/CrmAtendimento.cs`<br>`NexumAltivon_Back-End/API/Models/CrmCampanha.cs`<br>`NexumAltivon_Back-End/API/Models/CrmLead.cs`<br>`NexumAltivon_Back-End/API/Models/CrmSegmento.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Models/DropshippingConfig.cs`<br>`NexumAltivon_Back-End/API/Models/EmpresaGrupo.cs`<br>`NexumAltivon_Back-End/API/Models/Endereco.cs`<br>`NexumAltivon_Back-End/API/Models/Envio.cs`<br>`NexumAltivon_Back-End/API/Models/Financeiro.cs`<br>`NexumAltivon_Back-End/API/Models/Fiscal.cs`<br>`NexumAltivon_Back-End/API/Models/Fornecedor.cs`<br>`NexumAltivon_Back-End/API/Models/LogAuditoria.cs`<br>`NexumAltivon_Back-End/API/Models/LogisticaRastreamentoConsulta.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Models/Marketplace.cs`<br>`NexumAltivon_Back-End/API/Models/Notificacao.cs`<br>`NexumAltivon_Back-End/API/Models/Pagamento.cs`<br>`NexumAltivon_Back-End/API/Models/Pedido.cs`<br>`NexumAltivon_Back-End/API/Models/PedidoItem.cs`<br>`NexumAltivon_Back-End/API/Models/Produto.cs`<br>`NexumAltivon_Back-End/API/Models/SiteMidia.cs`<br>`NexumAltivon_Back-End/API/Models/Sys_AuditableEntity.cs`<br>`NexumAltivon_Back-End/API/Models/Tenant.cs`<br>`NexumAltivon_Back-End/API/Models/Transportadora.cs`<br>`NexumAltivon_Back-End/API/Models/Usuario.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AuthService.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoPagoService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | `NexumAltivon.Desktop/AccessManagementWindow.xaml`<br>`NexumAltivon.Desktop/AccessManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/App.xaml`<br>`NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml`<br>`NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml.cs`<br>`NexumAltivon.Desktop/ContasPagarWindow.xaml`<br>`NexumAltivon.Desktop/ContasPagarWindow.xaml.cs`<br>`NexumAltivon.Desktop/ContasReceberWindow.xaml`<br>`NexumAltivon.Desktop/ContasReceberWindow.xaml.cs`<br>`NexumAltivon.Desktop/CorporateMasterDataWindow.xaml`<br>`NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/DropshippingWindow.xaml`<br>`NexumAltivon.Desktop/DropshippingWindow.xaml.cs`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`<br>`NexumAltivon.Desktop/LogisticsWindow.xaml`<br>`NexumAltivon.Desktop/LogisticsWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/ManualNfeWindow.xaml.cs`<br>`NexumAltivon.Desktop/MarketingWindow.xaml`<br>`NexumAltivon.Desktop/MarketingWindow.xaml.cs`<br>`NexumAltivon.Desktop/Models/NetworkEndpointSetting.cs`<br>`NexumAltivon.Desktop/Models/PdvCartItem.cs`<br>`NexumAltivon.Desktop/Models/TerminalProfile.cs`<br>`NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml`<br>`NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml.cs`<br>`NexumAltivon.Desktop/NetworkSettingsWindow.xaml`<br>`NexumAltivon.Desktop/NetworkSettingsWindow.xaml.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopAutoUpdateService.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `genesis_bd` | `state` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Logistics/LogisticaTrackingService.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs` | `NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/ManualNfeWindow.xaml.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `genesis_bd` | `sys_operacoes_desktop` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisDesktopOperationService.cs` | - |
| `nexum_altivon` | `__efmigrationshistory` | Tabela | Tecnica | - | - |
| `nexum_altivon` | `adm_auditoria` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `adm_contatos` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `adm_empresas` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs` | - |
| `nexum_altivon` | `adm_perfil_permissoes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `adm_perfis` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `adm_permissoes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `adm_pessoas_empresas` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `adm_usuarios` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs` | - |
| `nexum_altivon` | `aggregatedcounter` | Tabela | Tecnica | - | - |
| `nexum_altivon` | `bi_dashboards` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `bi_kpi_valores` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `bi_kpis` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `bi_relatorio_historico` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `bi_relatorios` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `bi_widgets` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `carrinho` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Configurations/ServiceExtensions.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/CarrinhoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CheckoutDtos.cs`<br>`NexumAltivon_Back-End/API/Mappings/MappingProfile.cs`<br>`NexumAltivon_Back-End/API/Models/Carrinho.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/Produto.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/CarrinhoService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/FreteService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs` | `NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs` |
| `nexum_altivon` | `categorias` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/Models/Categoria.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs` |
| `nexum_altivon` | `cfg_anexos` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `cfg_comentarios` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_layouts_impressao` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_notificacoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_parametros` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_sequenciais` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_tarefas` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_workflow_aprovacoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_workflow_definicoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_workflow_etapas` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cfg_workflow_instancias` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `clientes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/ClienteService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoPagoService.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `cmp_aprovacoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cmp_cotacao_fornecedores` | Tabela | Transacional | - | - |
| `nexum_altivon` | `cmp_cotacao_itens` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `cmp_cotacoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cmp_notas_fiscais` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cmp_pedido_itens` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `cmp_pedidos` | Tabela | Transacional | - | - |
| `nexum_altivon` | `cmp_requisicao_itens` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `cmp_requisicoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `cnt_fechamentos` | Tabela | Transacional | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `cnt_lancamentos` | Tabela | Transacional | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `cnt_partidas` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `compras_cotacoes` | Tabela | Transacional | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `compras_entrada_itens` | Tabela | Filha de agregado | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `compras_entradas` | Tabela | Transacional | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `compras_pedido_itens` | Tabela | Filha de agregado | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `compras_pedidos` | Tabela | Transacional | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `compras_solicitacoes` | Tabela | Transacional | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `configuracoes_sistema` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/Models/ConfiguracaoSistema.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs` | - |
| `nexum_altivon` | `counter` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `crm_atendimentos` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/CrmAtendimento.cs` | - |
| `nexum_altivon` | `crm_campanhas` | Tabela | Transacional | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/CrmCampanha.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `crm_leads` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/CrmLead.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/CrmService.cs` | - |
| `nexum_altivon` | `crm_segmentos` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/CrmSegmento.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `ctb_conciliacoes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml`<br>`NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml.cs` |
| `nexum_altivon` | `cupons` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/CarrinhoService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs` | - |
| `nexum_altivon` | `distributedlock` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `dropshipping_config` | Tabela | Integracao | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/DropshippingConfig.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `enderecos` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/ClienteDtos.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/Endereco.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/ClienteService.cs` | `NexumAltivon.Desktop/CustomerManagementWindow.xaml`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `envios` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/Models/Envio.cs`<br>`NexumAltivon_Back-End/API/Models/Pedido.cs`<br>`NexumAltivon_Back-End/API/Models/Transportadora.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs`<br>`NexumAltivon_Back-End/API/Services/MelhorEnvioService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | - |
| `nexum_altivon` | `erp_avaliacoes_fornecedor` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `erp_centros_custo` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `erp_contas_bancarias` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `erp_contas_pagar` | Tabela | Transacional | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/ContasPagarWindow.xaml`<br>`NexumAltivon.Desktop/ContasPagarWindow.xaml.cs` |
| `nexum_altivon` | `erp_contas_receber` | Tabela | Transacional | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/ContasReceberWindow.xaml`<br>`NexumAltivon.Desktop/ContasReceberWindow.xaml.cs` |
| `nexum_altivon` | `erp_empresas_grupo` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/EmpresaGrupo.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `erp_fluxo_caixa` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `erp_fornecedores` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `erp_impostos_config` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `erp_interacoes_crm` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `erp_inventarios` | Tabela | Transacional | - | - |
| `nexum_altivon` | `erp_itens_inventario` | Tabela | Transacional | - | - |
| `nexum_altivon` | `erp_itens_nota_fiscal` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `erp_kardex` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `erp_leads_crm` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `erp_locais_estoque` | Tabela | Transacional | - | - |
| `nexum_altivon` | `erp_logs` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `erp_movimentacoes_estoque` | Tabela | Transacional | - | - |
| `nexum_altivon` | `erp_notas_fiscais` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `erp_tarefas_crm` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `est_curva_abc` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `est_depositos` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs` | - |
| `nexum_altivon` | `est_enderecos` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `est_inventario_contagens` | Tabela | Transacional | - | - |
| `nexum_altivon` | `est_inventarios` | Tabela | Transacional | - | - |
| `nexum_altivon` | `est_movimentacoes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `est_ordem_separacao_itens` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `est_ordens_separacao` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `est_ponto_pedido` | Tabela | Transacional | - | - |
| `nexum_altivon` | `est_saldos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `estoque_movimentos` | Tabela | Filha de agregado | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `fin_aprovacoes_pagar` | Tabela | Transacional | - | - |
| `nexum_altivon` | `fin_centros_custo` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `fin_conciliacoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fin_contas_bancarias` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fin_extratos_bancarios` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fin_moedas` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fin_orcamento_itens` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `fin_orcamentos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fin_pagamentos` | Tabela | Transacional | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `fin_plano_contas` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs` | - |
| `nexum_altivon` | `fin_recebimentos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fin_taxas_cambio` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fin_titulos_pagar` | Tabela | Transacional | - | - |
| `nexum_altivon` | `fin_titulos_receber` | Tabela | Transacional | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs` | - |
| `nexum_altivon` | `financeiro` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/FinancePdfReportService.cs`<br>`NexumAltivon_Back-End/API/Models/Financeiro.cs`<br>`NexumAltivon_Back-End/API/Models/Usuario.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/FinanceiroService.cs`<br>`NexumAltivon_Back-End/API/Services/IErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | `NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml`<br>`NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml.cs`<br>`NexumAltivon.Desktop/ContasPagarWindow.xaml.cs`<br>`NexumAltivon.Desktop/ContasReceberWindow.xaml.cs`<br>`NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/MarketingWindow.xaml.cs`<br>`NexumAltivon.Desktop/Models/FinanceModels.cs`<br>`NexumAltivon.Desktop/Models/PdvSaleDraft.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `fis_apuracao_impostos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fis_sped_contabil` | Tabela | Transacional | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `fis_sped_fiscal` | Tabela | Transacional | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `fis_tributacao_ncm` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `fiscal` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingModels.cs`<br>`NexumAltivon_Back-End/API/Models/EmpresaGrupo.cs`<br>`NexumAltivon_Back-End/API/Models/Fiscal.cs`<br>`NexumAltivon_Back-End/API/Models/Usuario.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/IErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/NotificacaoService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml`<br>`NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/ManualNfeWindow.xaml`<br>`NexumAltivon.Desktop/ManualNfeWindow.xaml.cs`<br>`NexumAltivon.Desktop/Models/EnterpriseOperationDrafts.cs`<br>`NexumAltivon.Desktop/Models/PdvSaleDraft.cs`<br>`NexumAltivon.Desktop/Models/TerminalProfile.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `fornecedores` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/Models/Fornecedor.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `nexum_altivon` | `hash` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDesktopOperationService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisRhService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Logistics/LogisticaTrackingService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Storage/AnexoStorageService.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/Usuario.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AuthService.cs`<br>`NexumAltivon_Back-End/API/Services/ClienteService.cs` | `NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `nexum_altivon` | `job` | Tabela | Tecnica | - | - |
| `nexum_altivon` | `jobparameter` | Tabela | Tecnica | - | - |
| `nexum_altivon` | `jobqueue` | Tabela | Tecnica | - | - |
| `nexum_altivon` | `jobstate` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_aditivos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_andamentos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_base_conhecimento` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_certidoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_contrato_clausulas` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_contratos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_depositos_judiciais` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_prazos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_processos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `jur_tipos_acao` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `list` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Configurations/HangfireAuthorizationFilter.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ApiResponse.cs`<br>`NexumAltivon_Back-End/API/DTOs/CarrinhoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CheckoutDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ClienteDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/DropshippingDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/LogisticaDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/MarketplaceDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/PedidoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ProdutoDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisRhService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/FinancePdfReportService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Logistics/LogisticaTrackingService.cs`<br>`NexumAltivon_Back-End/API/Mappings/MappingProfile.cs`<br>`NexumAltivon_Back-End/API/Middleware/ExceptionMiddleware.cs`<br>`NexumAltivon_Back-End/API/Models/CrmSegmento.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/CarrinhoService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/ClienteService.cs`<br>`NexumAltivon_Back-End/API/Services/CrmService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/FreteService.cs`<br>`NexumAltivon_Back-End/API/Services/IErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs`<br>`NexumAltivon_Back-End/API/Services/LojaService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceHubService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/ProdutoService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | `NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml.cs`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/Models/PdvSaleDraft.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs`<br>`NexumAltivon.Desktop/Services/DesktopAutoUpdateService.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `nexum_altivon` | `log_auditoria_fretes` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `log_cte` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `log_frete_faixas` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `log_mdfe` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `log_mdfe_documentos` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `log_tabelas_frete` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `log_tracking` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `log_transportadoras` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `logistica_rastreamento_consultas` | Tabela | Auditoria | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/LogisticaRastreamentoConsulta.cs` | - |
| `nexum_altivon` | `logs_auditoria` | Tabela | Auditoria | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/LogAuditoria.cs` | - |
| `nexum_altivon` | `lojas` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/LojaService.cs` | `NexumAltivon.Desktop/MainWindow.xaml` |
| `nexum_altivon` | `marketplaces` | Tabela | Integracao | `NexumAltivon_Back-End/API/Configurations/IntegrationExtensions.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/Models/Marketplace.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceHubService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceSyncService.cs` | `NexumAltivon.Desktop/MainWindow.xaml` |
| `nexum_altivon` | `md_fornecedor_contatos` | Tabela | Filha de agregado | `NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `nexum_altivon` | `md_produtos_precos_loja` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `notificacoes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Configurations/ServiceExtensions.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/Models/Notificacao.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/NotificacaoService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | - |
| `nexum_altivon` | `ops_ativos` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `ops_manutencoes` | Tabela | Transacional | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `ops_ordem_servico_itens` | Tabela | Filha de agregado | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `ops_ordens_servico` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `ops_producao_apontamentos` | Tabela | Transacional | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `pagamentos` | Tabela | Transacional | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/Pagamento.cs`<br>`NexumAltivon_Back-End/API/Models/Pedido.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoPagoService.cs` | `NexumAltivon.Desktop/Models/PdvSaleDraft.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `pcp_apontamentos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `pcp_bom` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `pcp_bom_itens` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `pcp_centros_trabalho` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `pcp_inspecoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `pcp_nao_conformidades` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `pcp_ordens_producao` | Tabela | Transacional | - | - |
| `nexum_altivon` | `pcp_requisicoes_material` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `pcp_roteiro_operacoes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `pcp_roteiros` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `pedido_itens` | Tabela | Filha de agregado | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/PedidoItem.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `pedidos` | Tabela | Transacional | `NexumAltivon_Back-End/API/Configurations/ServiceExtensions.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/DropshippingDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/LogisticaDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Models/Pedido.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/FinanceiroService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoPagoService.cs`<br>`NexumAltivon_Back-End/API/Services/NotificacaoService.cs`<br>`NexumAltivon_Back-End/API/Services/PedidoService.cs` | `NexumAltivon.Desktop/LogisticsWindow.xaml`<br>`NexumAltivon.Desktop/LogisticsWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `produtos` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/DropshippingDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/MarketplaceDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs`<br>`NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/Categoria.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Models/Fornecedor.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Models/Produto.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/Admin/AdminDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/CarrinhoService.cs`<br>`NexumAltivon_Back-End/API/Services/CheckoutService.cs`<br>`NexumAltivon_Back-End/API/Services/DropshippingService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpDashboardService.cs`<br>`NexumAltivon_Back-End/API/Services/ErpSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/FreteService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceHubService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoLivreService.cs`<br>`NexumAltivon_Back-End/API/Services/ProdutoService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `rh_afastamentos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `rh_cargos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `rh_colaboradores` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/GenesisDbContextModelSnapshot.cs` | - |
| `nexum_altivon` | `rh_departamentos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `rh_dependentes` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `rh_esocial_eventos` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `rh_eventos_folha` | Tabela | Transacional | - | - |
| `nexum_altivon` | `rh_ferias` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `rh_folha_itens` | Tabela | Filha de agregado | - | - |
| `nexum_altivon` | `rh_folhas_pagamento` | Tabela | Transacional | - | - |
| `nexum_altivon` | `rh_historico_salarial` | Tabela | Auditoria | - | - |
| `nexum_altivon` | `rh_ponto` | Tabela | Transacional | - | - |
| `nexum_altivon` | `server` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/DesignTimeDbContextFactories.cs`<br>`NexumAltivon_Back-End/API/Middleware/ExceptionMiddleware.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | `NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/Models/TerminalProfile.cs`<br>`NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml`<br>`NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml.cs`<br>`NexumAltivon.Desktop/NetworkSettingsWindow.xaml.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `set` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/DesignTimeDbContextFactories.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/DTOs/Admin/DashboardDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ApiResponse.cs`<br>`NexumAltivon_Back-End/API/DTOs/AuthDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CarrinhoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CheckoutDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ClienteDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/CrmDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/DropshippingDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ErpSyncDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/LogisticaDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/LojaDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/MarketplaceDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/PagamentoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/PedidoDtos.cs`<br>`NexumAltivon_Back-End/API/DTOs/ProdutoDtos.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDesktopOperationService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceModels.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisFinanceService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisRhService.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260705114438_InitialGenesis.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/Migrations/20260718072948_HardenGenesisFinanceTenancy.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/FinancePdfReportService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Logistics/LogisticaTrackingService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Storage/AnexoStorageService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Tenancy/ITenantContext.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Tenancy/TenantContext.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Tenancy/TenantResolverMiddleware.cs`<br>`NexumAltivon_Back-End/API/Middleware/RateLimitingMiddleware.cs`<br>`NexumAltivon_Back-End/API/Models/Carrinho.cs`<br>`NexumAltivon_Back-End/API/Models/Categoria.cs`<br>`NexumAltivon_Back-End/API/Models/Cliente.cs`<br>`NexumAltivon_Back-End/API/Models/ConfiguracaoSistema.cs`<br>`NexumAltivon_Back-End/API/Models/CrmAtendimento.cs`<br>`NexumAltivon_Back-End/API/Models/CrmCampanha.cs`<br>`NexumAltivon_Back-End/API/Models/CrmLead.cs`<br>`NexumAltivon_Back-End/API/Models/CrmSegmento.cs`<br>`NexumAltivon_Back-End/API/Models/Cupom.cs`<br>`NexumAltivon_Back-End/API/Models/DropshippingConfig.cs`<br>`NexumAltivon_Back-End/API/Models/EmpresaGrupo.cs`<br>`NexumAltivon_Back-End/API/Models/Endereco.cs`<br>`NexumAltivon_Back-End/API/Models/Envio.cs`<br>`NexumAltivon_Back-End/API/Models/Financeiro.cs`<br>`NexumAltivon_Back-End/API/Models/Fiscal.cs`<br>`NexumAltivon_Back-End/API/Models/Fornecedor.cs`<br>`NexumAltivon_Back-End/API/Models/LogAuditoria.cs`<br>`NexumAltivon_Back-End/API/Models/LogisticaRastreamentoConsulta.cs`<br>`NexumAltivon_Back-End/API/Models/Loja.cs`<br>`NexumAltivon_Back-End/API/Models/Marketplace.cs`<br>`NexumAltivon_Back-End/API/Models/Notificacao.cs`<br>`NexumAltivon_Back-End/API/Models/Pagamento.cs`<br>`NexumAltivon_Back-End/API/Models/Pedido.cs`<br>`NexumAltivon_Back-End/API/Models/PedidoItem.cs`<br>`NexumAltivon_Back-End/API/Models/Produto.cs`<br>`NexumAltivon_Back-End/API/Models/SiteMidia.cs`<br>`NexumAltivon_Back-End/API/Models/Sys_AuditableEntity.cs`<br>`NexumAltivon_Back-End/API/Models/Tenant.cs`<br>`NexumAltivon_Back-End/API/Models/Transportadora.cs`<br>`NexumAltivon_Back-End/API/Models/Usuario.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AuthService.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs`<br>`NexumAltivon_Back-End/API/Services/MarketplaceSyncService.cs`<br>`NexumAltivon_Back-End/API/Services/MercadoPagoService.cs`<br>`NexumAltivon_Back-End/NexumAltivon.API.Tests.cs` | `NexumAltivon.Desktop/AccessManagementWindow.xaml`<br>`NexumAltivon.Desktop/AccessManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/App.xaml`<br>`NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml`<br>`NexumAltivon.Desktop/ConciliacaoFinanceiraWindow.xaml.cs`<br>`NexumAltivon.Desktop/ContasPagarWindow.xaml`<br>`NexumAltivon.Desktop/ContasPagarWindow.xaml.cs`<br>`NexumAltivon.Desktop/ContasReceberWindow.xaml`<br>`NexumAltivon.Desktop/ContasReceberWindow.xaml.cs`<br>`NexumAltivon.Desktop/CorporateMasterDataWindow.xaml`<br>`NexumAltivon.Desktop/CorporateMasterDataWindow.xaml.cs`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml`<br>`NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/DropshippingWindow.xaml`<br>`NexumAltivon.Desktop/DropshippingWindow.xaml.cs`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml`<br>`NexumAltivon.Desktop/FiscalRoutingWindow.xaml.cs`<br>`NexumAltivon.Desktop/LogisticsWindow.xaml`<br>`NexumAltivon.Desktop/LogisticsWindow.xaml.cs`<br>`NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs`<br>`NexumAltivon.Desktop/ManualNfeWindow.xaml.cs`<br>`NexumAltivon.Desktop/MarketingWindow.xaml`<br>`NexumAltivon.Desktop/MarketingWindow.xaml.cs`<br>`NexumAltivon.Desktop/Models/NetworkEndpointSetting.cs`<br>`NexumAltivon.Desktop/Models/PdvCartItem.cs`<br>`NexumAltivon.Desktop/Models/TerminalProfile.cs`<br>`NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml`<br>`NexumAltivon.Desktop/ModuleWorkspaceWindow.xaml.cs`<br>`NexumAltivon.Desktop/NetworkSettingsWindow.xaml`<br>`NexumAltivon.Desktop/NetworkSettingsWindow.xaml.cs`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml`<br>`NexumAltivon.Desktop/PdvSaleWindow.xaml.cs`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml`<br>`NexumAltivon.Desktop/ProcurementWindow.xaml.cs`<br>`NexumAltivon.Desktop/Services/DesktopAutoUpdateService.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `nexum_altivon` | `site_midias` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Models/SiteMidia.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `state` | Tabela | Tecnica | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Infrastructure/Logistics/LogisticaTrackingService.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs` | `NexumAltivon.Desktop/CustomerManagementWindow.xaml.cs`<br>`NexumAltivon.Desktop/ManualNfeWindow.xaml.cs`<br>`NexumAltivon.Desktop/SupplierManagementWindow.xaml.cs` |
| `nexum_altivon` | `sys_tenants` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Models/Tenant.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `sys_workflow_definicoes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `sys_workflow_instancias` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `sys_workflow_transicoes` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `transportadoras` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/Models/Transportadora.cs`<br>`NexumAltivon_Back-End/API/Services/LogisticaService.cs` | `NexumAltivon.Desktop/MainWindow.xaml`<br>`NexumAltivon.Desktop/MainWindow.xaml.cs` |
| `nexum_altivon` | `usuarios` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260705114326_InitialNexumOperational.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260714062834_AddSiteMediaLibrary.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260717073759_HardenPlatformSso.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718203000_AddLogisticsTrackingAudit.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718213331_ScopeConfigurationKeysByTenant.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/20260718220042_HardenProcurementTenancy.Designer.cs`<br>`NexumAltivon_Back-End/API/Data/Migrations/Nexum/NexumDbContextModelSnapshot.cs`<br>`NexumAltivon_Back-End/API/Data/NexumDbContext.cs`<br>`NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Models/Usuario.cs`<br>`NexumAltivon_Back-End/API/Program.cs`<br>`NexumAltivon_Back-End/API/Services/AssistenteIaService.cs`<br>`NexumAltivon_Back-End/API/Services/AuthService.cs`<br>`NexumAltivon_Back-End/API/Services/Interfaces.cs` | `NexumAltivon.Desktop/Services/DesktopApiClient.cs` |
| `nexum_altivon` | `v_clientes_vip` | View | View | - | - |
| `nexum_altivon` | `v_crm_leads_status` | View | View | - | - |
| `nexum_altivon` | `v_estoque_baixo` | View | View | - | - |
| `nexum_altivon` | `v_estoque_baixo_erp` | View | View | - | - |
| `nexum_altivon` | `v_pipeline_crm` | View | View | - | - |
| `nexum_altivon` | `v_resumo_financeiro` | View | View | - | - |
| `nexum_altivon` | `v_resumo_pedidos_status` | View | View | - | - |
| `nexum_altivon` | `v_vendas_loja` | View | View | - | - |
| `nexum_altivon` | `vnd_itens` | Tabela | Filha de agregado | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs`<br>`NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `vnd_itens_precos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vnd_notas_fiscais` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vnd_pedido_itens` | Tabela | Filha de agregado | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs` | - |
| `nexum_altivon` | `vnd_pedidos` | Tabela | Transacional | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs` | - |
| `nexum_altivon` | `vnd_tabelas_preco` | Tabela | Administrativa | `NexumAltivon_Back-End/API/ERP/SharedData/GenesisPdvService.cs` | - |
| `nexum_altivon` | `vw_balanco_patrimonial` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vw_certidoes_vencer` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vw_contratos_vencer` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vw_custos_producao` | Tabela | Transacional | - | - |
| `nexum_altivon` | `vw_dre_gerencial` | Tabela | Administrativa | `NexumAltivon_Back-End/API/Program.cs` | - |
| `nexum_altivon` | `vw_fluxo_caixa_projetado` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vw_inadimplencia` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vw_lojas_operacionais` | View | View | - | - |
| `nexum_altivon` | `vw_nexum_genesis_compras` | View | View | - | - |
| `nexum_altivon` | `vw_nexum_genesis_financeiro_aberto` | View | View | - | - |
| `nexum_altivon` | `vw_nexum_genesis_produtos` | View | View | - | - |
| `nexum_altivon` | `vw_ordens_producao_andamento` | Tabela | Transacional | - | - |
| `nexum_altivon` | `vw_performance_vendedores` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vw_processos_risco` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vw_ranking_produtos` | Tabela | Administrativa | - | - |
| `nexum_altivon` | `vw_saldo_contas_pagar` | Tabela | Transacional | - | - |
| `nexum_altivon` | `vw_saldo_contas_receber` | Tabela | Transacional | - | - |
| `nexum_altivon` | `vw_saldo_estoque` | Tabela | Transacional | - | - |

## Proximo Gate

WPFDB-02 deve revisar cada linha, confirmar o dominio real, identificar entidade/DTO/endpoint/permissao/formulario e registrar operacoes permitidas. Nenhuma linha desta matriz pode ser promovida a concluida somente por coincidencia de nome.
````
