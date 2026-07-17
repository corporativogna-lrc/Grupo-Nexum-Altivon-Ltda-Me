<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
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

## Bloqueio Sophia 5.5 - Adendo de Veracidade Operacional

- O bloqueio Sophia 5.5 permanece integralmente ativo; este adendo apenas acrescenta criterios de aceite.
- Toda entrega deve apresentar resultado concreto, codigo real, endpoint real, persistencia real e validacao executada. Relatos sem evidencia tecnica nao encerram requisito.
- Ferramentas, funcoes e telas prometidas por paineis legados ou documentos anteriores devem ser mapeadas, comparadas com o painel React oficial e consolidadas somente como implementacao real no projeto oficial.
- Nenhuma ferramenta do painel pode ser tratada como finalizada quando existir apenas interface figurativa, botao sem acao, redirecionamento decorativo, retorno local fabricado, mensagem de sucesso sem gravacao no banco ou integracao sem chamada externa valida.
- Quando uma ferramenta depender de marketplace, gateway, SEFAZ, logistica, banco, storage ou outro provedor externo, a validacao deve usar credencial real de homologacao ou teste oficial disponivel no ambiente. Se a credencial nao existir, a API deve bloquear a operacao com erro rastreavel, sem sucesso falso.
- A arquitetura aceitavel para cada ferramenta do painel deve conter, no minimo: model ou contrato de dados, service real, endpoint Minimal API, chamada frontend pelo `src/services/api.js`, tela React oficial, persistencia/consulta no banco quando aplicavel e smoke test HTTP.
- O painel legado desativado deve ser usado apenas como mapa de intencao funcional. Ele nao deve voltar como HTML operacional nem substituir o painel React oficial.
- Publicacao e validacao devem confirmar o mesmo comportamento no runtime local `127.0.0.1:5010`, na API publica `https://api.nexumaltivon.com.br` e no portal publicado `https://nexumaltivon.com.br`, quando a funcao for exposta publicamente.
- O repositorio GitHub deve receber apenas codigo auditado a partir do projeto oficial vigente em `D:\Nexum Altivon\NexumAltivon.com`, nunca o contrario sem validacao previa.

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

## Proximas Fases

1. Migrar entidades transacionais legadas para `Sys_AuditableEntity` com backfill seguro.
2. Completar workflow e SSO sem placeholders.
3. Criar testes xUnit e cobertura real dos services.
4. Ativar observabilidade, Redis, Hangfire, backup/restore e staging.
5. Avancar modulo por modulo dos setores 02 a 08 com Model, Service, Endpoint e Tela.
