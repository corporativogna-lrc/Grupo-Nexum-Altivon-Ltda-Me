# Deploy privado - Grupo Nexum Altivon

Este projeto deve ser publicado apenas em repositorios privados e com banco de dados privado da empresa.

## Repositorio GitHub

Repositorio alvo:

```text
https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me.git
```

Antes do primeiro push, confirme no GitHub que o repositorio esta como `Private`.

## Segredos obrigatorios

Configure estes valores como GitHub Actions Secrets e tambem no arquivo `.env` do servidor de producao:

```text
API_DEFAULT_CONNECTION
JWT_SECRET_KEY
ADMIN_EMAIL
ADMIN_PASSWORD
ADMIN_NAME
ADMIN_ROLE
MP_ACCESS_TOKEN
SENDGRID_API_KEY
PROD_HOST
PROD_USER
PROD_SSH_KEY
```

`API_DEFAULT_CONNECTION` deve apontar para o MySQL privado da empresa. Use `docker/.env.example` como modelo e preencha a senha apenas no servidor ou nos secrets do GitHub.

## Deploy no servidor

No host de producao:

```bash
mkdir -p /opt/nexumaltivon/production
cd /opt/nexumaltivon/production
git clone https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-M.git .
cp docker/.env.example .env
```

Edite `.env` no proprio servidor com os valores reais.

Suba a aplicacao completa:

```bash
docker compose -f docker/docker-compose.prod.yml --env-file .env pull
docker compose -f docker/docker-compose.prod.yml --env-file .env up -d
docker compose -f docker/docker-compose.prod.yml --env-file .env ps
```

O compose de producao sobe:

- `frontend`: site React em Nginx.
- `api`: API ASP.NET em `8080` interno.
- `nginx`: proxy publico na porta `80`, roteando `www.nexumaltivon.com` para o front e `api.nexumaltivon.com` para a API.
- `watchtower`: atualizacao automatica das imagens publicadas no GHCR.

DNS esperado:

```text
nexumaltivon.com        A/AAAA -> IP do servidor
www.nexumaltivon.com    A/AAAA -> IP do servidor
api.nexumaltivon.com    A/AAAA -> IP do servidor
back.nexumaltivon.com   A/AAAA -> IP do servidor
admin.nexumaltivon.com  A/AAAA -> IP do servidor
erp.nexumaltivon.com    A/AAAA -> IP do servidor
crm.nexumaltivon.com    A/AAAA -> IP do servidor
pdv.nexumaltivon.com    A/AAAA -> IP do servidor
```

Para HTTPS, use um proxy/terminador TLS no servidor ou Cloudflare apontando para a porta `80` do `nexum-nginx`.

## Observacoes de seguranca

- Nao versionar `.env`, senhas, tokens, chaves SSH ou certificados.
- O banco de dados deve aceitar conexoes somente da rede/hosts autorizados.
- O usuario de banco usado pela API deve ter permissoes minimas para a aplicacao.
- Ative HTTPS antes de expor checkout, login ou pagamentos em producao.
