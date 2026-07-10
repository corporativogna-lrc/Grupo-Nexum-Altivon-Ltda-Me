<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Checklist de Prontidao de Deploy

Data de referencia: 2026-06-16

## Codigo

- [ ] `dotnet build .\NexumAltivon.ERP.sln -c Release --no-restore -v minimal` finaliza sem erro.
- [ ] `dotnet test .\NexumAltivon_Back-End\NexumAltivon.API.Tests\NexumAltivon.API.Tests.csproj -c Release --no-build -v minimal` finaliza sem falha.
- [ ] `npm run build` em `NexumAltivon_Front-End` finaliza sem erro.
- [ ] `git diff --check` nao aponta whitespace quebrado nos arquivos alterados.
- [ ] Nenhum artefato `bin/`, `obj/`, `TestResults/`, `node_modules/` ou `.env` real entra em commit.

## API

- [ ] `/health` responde `200`.
- [ ] `/health/db` responde `200` no ambiente com banco.
- [ ] `/swagger/v1/swagger.json` expõe as rotas criticas.
- [ ] `/api/auth/refresh` responde `400` para payload invalido, nao `404` nem `500`.
- [ ] Endpoints protegidos consumidos pelo frontend respondem `401` sem JWT, nao `404`.
- [ ] `/api/assistentes/mensagem` responde para Yara e Sophia sem expor chave no cliente.

## Frontend

- [ ] `api-runtime.json` aponta para a API publica correta.
- [ ] Chat no corpo do site abre Yara e Sophia.
- [ ] Falha de IA externa cai em resposta local controlada.
- [ ] Login/refresh token usa `token`, `access_token`, `refreshToken` e `refresh_token`.
- [ ] Area do cliente mantem fluxo de confirmacao de email.

## Infra

- [ ] `docker/.env` privado contem valores reais rotacionados.
- [ ] `docker/docker-compose.yml` sobe MySQL, Redis, API, frontend e Nginx no ambiente dev.
- [ ] `docker/docker-compose.prod.yml` usa banco externo privado e Redis sidecar.
- [ ] TLS fica no Cloudflare Tunnel ou Nginx, sem HTTP publico direto para API.
- [ ] Logs de API ficam disponiveis em console e arquivo.

## Integracoes

- [ ] Mercado Pago validado com credencial do cofre.
- [ ] SendGrid validado com credencial do cofre.
- [ ] OpenAI validado via `OpenAI__ApiKey` no servidor.
- [ ] GitHub Actions executa build/teste antes de deploy.
- [ ] GitHub Pages publica sem depender de API instavel para concluir o job.

## Banco

- [ ] Backup recente existe antes de migration.
- [ ] Restore de teste foi executado em ambiente isolado.
- [ ] Migrations aplicam em banco vazio.
- [ ] TenantId, soft delete e RowVersion estao cobertos nas entidades transacionais novas.

## Liberacao

- [ ] Validar fluxo cadastro, venda, pagamento, estoque, fiscal, logistica, financeiro e BI.
- [ ] Confirmar isolamento por empresa/loja.
- [ ] Registrar evidencias no handoff de producao privada.
- [ ] Publicar somente depois da revisao final do diff.
