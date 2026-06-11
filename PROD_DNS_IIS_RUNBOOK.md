# Runbook (Produção) — DNS + IIS — Nexum Altivon

Objetivo: deixar **todos os subdomínios respondendo** (mesmo que alguns módulos compartilhem a mesma aplicação no início) e com **API + Front-End operantes** para a equipe iniciar importações/cadastros.

## 0) Ponto crítico (não pular)

- **DNS público não aceita IP privado** (ex.: `192.168.x.x`). Para a Internet enxergar seus hostnames, você precisa de:
  - **IP público** + portas abertas para o IIS (ou reverse proxy), **ou**
  - **VPS** com IP público, **ou**
  - **Cloudflare (DNS/Proxy/Tunnel)** — mas isso depende do tipo de setup do seu domínio (ver seção 6).

## 1) Escolha o caminho (recomendado: VPS)

### Caminho A — VPS (mais rápido e estável p/ produção)

1. Contrate uma VPS com **IP público** (preferencialmente IP fixo).
2. Publique nela (Linux com Docker **ou** Windows com IIS).
3. No Wix, aponte os DNS `A` para o IP da VPS (seção 2).

Vantagem: não depende de CGNAT, não depende de abrir portas no seu roteador, e dá uptime melhor.

### Caminho B — Servidor local + IIS (somente se der IP público e portas)

Pré-requisitos:
- Link com **IP público** (se for CGNAT, port-forwarding não funciona).
- Port forwarding no roteador: `80` e `443` indo para o servidor (ex.: `192.168.1.72`).
- Firewall do Windows liberando `80/443`.
- Energia: no Windows, deixe **Suspensão = Nunca** (pode apagar a tela, mas não pode “dormir”).

Depois siga seções 2, 3 e 4.

## 2) DNS no Wix (passo a passo)

Antes de mexer em registros: confirme **onde o DNS está ativo** (Wix ou Cloudflare).

No Windows (no seu PC), rode:

```bat
nslookup -type=ns nexumaltivon.com
```

- Se os nameservers apontarem para **Wix**, ajuste os registros no Wix.
- Se apontarem para **Cloudflare**, ajuste os registros no Cloudflare (não no Wix), senão nada muda.

No Wix: **Domínios → Gerenciar DNS → Registros DNS**.

Crie/ajuste registros `A` apontando para o **IP público** do seu servidor (VPS ou link com port-forwarding):

Host / Nome | Tipo | Valor / Aponta para
---|---|---
`@` | `A` | `IP_PUBLICO`
`www` | `A` | `IP_PUBLICO`
`api` | `A` | `IP_PUBLICO`
`back` | `A` | `IP_PUBLICO`
`admin` | `A` | `IP_PUBLICO`
`erp` | `A` | `IP_PUBLICO`
`crm` | `A` | `IP_PUBLICO`
`pdv` | `A` | `IP_PUBLICO`

Notas:
- Se o Wix não permitir `A` duplicado para `www` (ou já existir um `CNAME`), remova o conflito e deixe **apenas 1 tipo por hostname** (regra geral de DNS).
- Se você quiser forçar `nexumaltivon.com` → `www.nexumaltivon.com`, faça isso no **servidor** (IIS/redirect) ou via regra do provedor, sem depender de “CNAME na raiz”.

## 3) IIS — publicar o Front-End (www + módulos “placeholder”)

O **Front-End atual** atende site + dashboard (admin/ERP/CRM/PDV) no mesmo build.

1. Gere o build do front-end:
   - Pasta: `NexumAltivon_Front-End`
   - Comando: `npm ci` e `npm run build`
2. No IIS, crie um site: `Nexum-Frontend`
   - Physical Path: `...\NexumAltivon_Front-End\build`
   - Binding HTTP: `*:80` Hostname `www.nexumaltivon.com`
3. Adicione bindings adicionais no **mesmo site**:
   - `admin.nexumaltivon.com`
   - `erp.nexumaltivon.com`
   - `crm.nexumaltivon.com`
   - `pdv.nexumaltivon.com`

Importante (React/SPA):
- Garanta regra de rewrite para que refresh em rotas (ex.: `/dashboard`) não dê 404.
- Se necessário, crie um `web.config` no `build/` com rewrite para `index.html`.

## 4) IIS — publicar a API (api + back)

1. Publique a API:
   - Projeto: `NexumAltivon_Back-End\NexumAltivon.API.csproj`
   - Exemplo: `dotnet publish -c Release -o C:\inetpub\nexum-api\`
2. No IIS:
   - Crie um Site: `Nexum-Api`
   - Physical Path: `C:\inetpub\nexum-api\`
   - App Pool: `No Managed Code`
   - Binding HTTP: `*:80` Hostname `api.nexumaltivon.com`
   - Adicione Hostname `back.nexumaltivon.com` (mesma API)
3. Configure os secrets/variáveis no servidor (não commitar):
   - Connection string MySQL (ideal: MySQL privado, sem exposição pública)
   - JWT secret
   - Admin user (email/senha)
   - Tokens (Mercado Pago / SendGrid etc.)

## 5) Validação externa (prova de “está online”)

Testar de fora da rede (4G do celular):
- `http://www.nexumaltivon.com`
- `http://api.nexumaltivon.com/health`
- `http://back.nexumaltivon.com/health`
- `http://erp.nexumaltivon.com`
- `http://crm.nexumaltivon.com`
- `http://pdv.nexumaltivon.com`
- `http://admin.nexumaltivon.com` (deve redirecionar/abrir o dashboard)

Se ainda estiver “OFF”:
- Confirme que o DNS que você editou é o **ativo** (seção 2).
- Confirme que `IP_PUBLICO` é realmente o IP que a Internet enxerga (não `192.168.x.x`).
- Se for servidor local: confirme se o seu link não é **CGNAT** e se o roteador está com port-forwarding de `80/443`.
- Confirme firewall do Windows + bindings no IIS (Hostname correto).

## 6) Cloudflare (para quando “sem abrir portas” for obrigatório)

Dois pontos importantes:
- **CNAME setup (partial)** “sem trocar nameservers” é **Business/Enterprise** no Cloudflare.
- O hostname `<UUID>.cfargotunnel.com` do Tunnel só “funciona” quando o DNS/rota do hostname é gerenciado dentro da **mesma conta Cloudflare** (na prática, isso tende a exigir o domínio no Cloudflare DNS, ou setup partial elegível).

Se você estiver em **plano Free** e não puder trocar nameservers no Wix, o caminho mais simples geralmente vira:
- **VPS** + DNS `A` no Wix (seção 2).

## 7) Segurança (não negociar)

- **Não exponha MySQL na Internet**. Se precisar banco fora, use VPN/allowlist/privatenet.
- Habilite **HTTPS** antes de login/checkout/pagamentos reais.
- Não coloque senhas/tokens em GitHub nem em arquivos versionados.
