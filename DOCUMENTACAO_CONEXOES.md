# Documentação — Conexões e Publicação (Nexum Altivon)

Este documento é um “mapa” do que conecta com o quê (DNS → Front-End → API → Banco), e quais são as opções práticas de publicação em produção mantendo o banco privado.

## 1) Visão geral (arquitetura atual)

Fluxo principal:

1. Usuário acessa um hostname (ex.: `www.nexumaltivon.com`).
2. O DNS aponta o hostname para um **servidor público** (VPS) ou para o **seu servidor local exposto**.
3. O servidor entrega o **Front-End** (React build).
4. O Front-End chama a **API** (ASP.NET Core) via HTTP(S).
5. A API acessa o **MySQL** (idealmente privado).

## 2) Hostnames (módulos) e destinos

Requisito do projeto: manter os hostnames abaixo online e operantes.

Hostname | Função | Destino recomendado (fase 1)
---|---|---
`nexumaltivon.com` | raiz do domínio | mesmo do `www` (redirect para `www` no servidor)
`www.nexumaltivon.com` | Front-End (site + dashboard) | Front-End (React em IIS ou Nginx)
`api.nexumaltivon.com` | API pública | API (ASP.NET Core)
`back.nexumaltivon.com` | “Back-End” (alias) | mesma API de `api`
`admin.nexumaltivon.com` | backoffice / painel | mesmo Front-End (fase 1)
`erp.nexumaltivon.com` | ERP | mesmo Front-End (fase 1) + APIs ERP/CRM via API
`crm.nexumaltivon.com` | CRM | mesmo Front-End (fase 1) + endpoints CRM via API
`pdv.nexumaltivon.com` | PDV | mesmo Front-End (fase 1) + integrações via API

Observação importante:
- Na fase 1, **ERP/CRM/PDV/Admin** podem compartilhar o mesmo Front-End publicado e diferenciar por rotas (`/dashboard`, abas, etc.). Isso garante “no ar” sem travar a operação por falta de UI dedicada.

## 3) DNS (o que precisa existir)

Para cada hostname acima, você precisa de um DNS válido.

### 3.1 Quando você tem IP público (VPS ou link com port-forwarding)

Crie registros `A` apontando para o **IP público**:
- `@` → `IP_PUBLICO`
- `www` → `IP_PUBLICO`
- `api` → `IP_PUBLICO`
- `back` → `IP_PUBLICO`
- `admin` → `IP_PUBLICO`
- `erp` → `IP_PUBLICO`
- `crm` → `IP_PUBLICO`
- `pdv` → `IP_PUBLICO`

### 3.2 Onde editar (Wix x Cloudflare)

Você só deve editar no provedor que realmente está “servindo” o DNS da zona.

No Windows:

```bat
nslookup -type=ns nexumaltivon.com
```

- Se os NS forem do **Wix** → edite no Wix.
- Se os NS forem do **Cloudflare** → edite no Cloudflare.

## 4) Publicação (opções práticas)

### Opção A — VPS + Docker (produção recomendada)

Se você tiver uma VPS Linux com Docker:
- Use `docker/docker-compose.prod.yml` como base de produção.
- O `nginx` expõe `80` e roteia por hostname:
  - `www.* / erp.* / crm.* / pdv.* / admin.*` → Front-End
  - `api.* / back.*` → API

Arquivo chave:
- `C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\DEPLOY.md`

### Opção B — Servidor local (Windows + IIS)

Use quando você conseguir:
- IP público real (não CGNAT), e
- abrir portas `80`/`443` no roteador para o servidor (port forwarding).

Guia:
- `C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\IIS_DEPLOY.md`
- Runbook direto ao ponto:
  - `C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\PROD_DNS_IIS_RUNBOOK.md`

## 5) Banco de dados (privacidade e segurança)

Recomendação:
- **Não expor o MySQL na Internet**.

Melhores opções:
- Manter MySQL na LAN e a API no mesmo local (IIS/servidor local) **com DNS apontando para IP público** (se possível).
- Se usar VPS: colocar o MySQL na VPS, mas com firewall/segurança:
  - bind local (`127.0.0.1`) quando possível, ou
  - liberar só para IPs permitidos (allowlist), VPN, rede privada.

Ponto do projeto (já referenciado no template):
- MySQL em `Servidor_NexumAltivon` na rede local (ex.: `192.168.1.72`) usando `3309` (mapeamento comum do Docker dev).
- Template de connection string (produção) está em:
  - `C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\docker\.env.example`

## 6) API (ASP.NET Core) — portas e healthcheck

Dev local (sem IIS):
- API costuma rodar em `http://localhost:5000` (script supervisor).

Produção (Docker):
- API roda interna em `:8080` e o Nginx publica em `:80` por hostname.

Healthcheck:
- `http(s)://api.nexumaltivon.com/health`
- `http(s)://back.nexumaltivon.com/health`

## 7) Front-End (React) — publicação e rotas

Front-End publicado como estático (build).

Pontos práticos:
- Em IIS, é essencial ter rewrite para SPA (refresh não pode virar 404).
- Em produção, o Front-End chama a API via `REACT_APP_BACKEND_URL` (em build/dev) ou URL configurada no ambiente.

## 8) Segredos e variáveis (obrigatórios)

Os segredos descritos em `DEPLOY.md` devem existir no servidor e/ou GitHub secrets:
- `API_DEFAULT_CONNECTION`
- `JWT_SECRET_KEY`
- `ADMIN_EMAIL`, `ADMIN_PASSWORD`, `ADMIN_NAME`, `ADMIN_ROLE`
- `MP_ACCESS_TOKEN`
- `SENDGRID_API_KEY`
- `PROD_HOST`, `PROD_USER`, `PROD_SSH_KEY` (se houver pipeline de deploy)

Regra: **não versionar** senhas/tokens/chaves.

## 9) Evidência rápida de “está no ar”

Teste sempre fora da sua rede (4G):
- `http://www.nexumaltivon.com`
- `http://api.nexumaltivon.com/health`

Se falhar:
- primeiro confirme o NS ativo (Wix/Cloudflare),
- depois confirme o IP público,
- depois confira portas `80/443`, firewall, e bindings (host header) no IIS.

