<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Handoff Producao Privada e Integracoes

Este handoff fixa o estado operacional necessario para publicar GenesisGest.Net / Grupo Nexum Altivon sem expor segredos e sem quebrar o site de vendas ja em uso.

## Ambientes

- Frontend publico: `https://www.nexumaltivon.com.br`
- API publica: `https://api.nexumaltivon.com.br`
- API local de producao privada: porta `5010`
- MySQL privado atual: `192.168.1.72:3309`
- Docker dev: API `5000`, frontend `3000`, MySQL `3309`, Redis `6379`, Nginx `80/443`

## Integracoes Ativas

- Mercado Pago: token via `Integracoes__MercadoPago__AccessToken`
- SendGrid: chave via `Integracoes__SendGrid__ApiKey`
- OpenAI: chave via `OpenAI__ApiKey`, usada por Yara e Sophia no endpoint `/api/assistentes/mensagem`
- Redis: `Redis__ConnectionString` e `Hangfire__Storage__Redis`
- MySQL: `ConnectionStrings__DefaultConnection` e `ConnectionStrings__NexumDb`
- GitHub Actions: workflows em `.github/workflows/`
- GitHub Pages: frontend publicado com `api-runtime.json`
- Cloudflare Tunnel ou Nginx TLS: ponto unico de exposicao publica da API

## Regras de Segredo

Valores reais ficam somente no cofre do ambiente ou em `.env` local nao versionado.

Variaveis que exigem rotacao antes de nova publicacao:

- `JWT_SECRET_KEY`
- `ADMIN_PASSWORD`
- `MYSQL_ROOT_PASSWORD`
- `MYSQL_PASSWORD`
- `API_DEFAULT_CONNECTION`
- `API_NEXUM_CONNECTION`
- `MP_ACCESS_TOKEN`
- `SENDGRID_API_KEY`
- `OPENAI_API_KEY`

## Sequencia de Handoff

1. Sincronizar branch `main` local com o remoto somente apos conferir diff e artefatos gerados.
2. Executar build da solution e testes da API.
3. Executar build do frontend.
4. Confirmar `.env` privado no host de deploy com as variaveis de `DEPLOY.md`.
5. Subir compose do ambiente alvo.
6. Validar `/health`, `/health/db`, `/swagger/v1/swagger.json` e `/api/assistentes/mensagem`.
7. Validar `api-runtime.json` publicado no site.
8. Verificar GitHub Actions e Pages depois do deploy.

## Rollback

- API: voltar imagem anterior no compose ou restaurar executavel anterior da porta `5010`.
- Frontend: voltar versao anterior do Pages ou imagem anterior do Nginx/frontend.
- Banco: restaurar backup validado antes de aplicar migration destrutiva.
- Túnel/TLS: manter configuracao anterior ate a nova rota responder `200` em `/health`.

## Evidencia Minima

Cada entrega precisa registrar:

- commit ou hash da imagem publicada;
- resultado de `dotnet test` da API;
- resultado de `npm run build`;
- resposta de `/health` e `/health/db`;
- rota publica consumida pelo frontend;
- status do workflow GitHub Actions ligado ao deploy.
