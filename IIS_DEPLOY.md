# Publicacao em servidor local (Windows + IIS) - Nexum Altivon

Este guia serve para colocar `www.nexumaltivon.com` e `api.nexumaltivon.com` (e os demais subdominios) online usando um servidor Windows local com IIS.

> Importante: o IP `192.168.x.x` e outros IPs privados nao funcionam na Internet. DNS publico precisa apontar para um IP publico OU voce precisa usar um tunel (Cloudflare Tunnel).

## 1) Escolha como expor o servidor local

### Opcao A (direto na internet): IP publico + Port Forwarding

Use esta opcao somente se o seu link tiver **IP publico** e voce conseguir abrir as portas no roteador.

Checklist:
- IP do servidor LAN fixo (ex.: `192.168.1.72`) reservado no roteador (DHCP reservation).
- Port forwarding no roteador:
  - WAN `80`  -> `192.168.1.72:80`
  - WAN `443` -> `192.168.1.72:443`
- Firewall do Windows liberando `80` e `443` (somente para IIS).
- DNS no Wix apontando `@` e subdominios para o seu **IP publico**.

### Opcao B (recomendado p/ CGNAT e IP dinamico): Cloudflare Tunnel

Use esta opcao se:
- seu provedor usa CGNAT (port forwarding nao funciona), OU
- seu IP publico muda com frequencia, OU
- voce quer evitar abrir portas no roteador.

Resumo:
- Voce instala o `cloudflared` no servidor Windows
- Cria um Tunnel no Cloudflare
- Aponta cada hostname (www/api/back/erp/crm/pdv) para `<UUID>.cfargotunnel.com` via CNAME
- O Tunnel encaminha para `http://localhost:80` (IIS) e `http://localhost:5000` (API) ou o que voce definir.

> Atenção (importante): para usar hostnames personalizados com Cloudflare Tunnel, o DNS/rota desses hostnames precisa estar na **mesma conta Cloudflare**. Em geral isso exige o dominio no Cloudflare DNS (trocar nameservers) ou um setup partial (CNAME setup), que costuma ser Business/Enterprise.

## 2) Preparar o IIS (uma vez)

1. Instale o IIS (Windows Features).
2. Instale o **ASP.NET Core Hosting Bundle** compativel com o runtime do projeto.
3. Reinicie o IIS (ou o servidor) apos instalar o bundle.

## 3) Publicar o Front-End (www + erp/crm/pdv/admin provisoriamente)

O front-end ja possui pasta `build/` dentro de `NexumAltivon_Front-End`.

No IIS:
- Crie um Site: `Nexum-Frontend`
- Physical Path: `...\NexumAltivon_Front-End\build`
- Binding HTTP: `*:80` Hostname `www.nexumaltivon.com`
- (Opcional) Adicione bindings adicionais no mesmo Site:
  - `erp.nexumaltivon.com`
  - `crm.nexumaltivon.com`
  - `pdv.nexumaltivon.com`
  - `admin.nexumaltivon.com`

> Isso permite deixar os modulos "no ar" imediatamente, mesmo que eles ainda redirecionem/compartilhem o mesmo front-end por enquanto.

## 4) Publicar a API (api + back)

No servidor Windows (PowerShell/cmd):
1. Publique a API:
   - Projeto: `NexumAltivon_Back-End\NexumAltivon.API.csproj`
   - Saida sugerida: `C:\inetpub\nexum-api\`

2. No IIS:
   - Crie um Site: `Nexum-Api`
   - Physical Path: `C:\inetpub\nexum-api\`
   - App Pool: `No Managed Code`
   - Binding HTTP: `*:80` Hostname `api.nexumaltivon.com`
   - Adicione tambem Hostname `back.nexumaltivon.com` (mesma API)

3. Configure as variaveis de ambiente/secrets no servidor (nao versionar senhas):
   - Connection string MySQL
   - JWT secret
   - Admin user (email/senha)
   - Mercado Pago / SendGrid, etc.

## 5) DNS no Wix (apontamentos)

### Se estiver usando Opcao A (IP publico + port forwarding)

Crie registros `A` apontando para o seu IP publico:
- `@` -> `IP_PUBLICO`
- `www` -> `IP_PUBLICO`
- `api` -> `IP_PUBLICO`
- `back` -> `IP_PUBLICO`
- `erp` -> `IP_PUBLICO`
- `crm` -> `IP_PUBLICO`
- `pdv` -> `IP_PUBLICO`
- `admin` -> `IP_PUBLICO`

### Se estiver usando Opcao B (Cloudflare Tunnel)

Em geral voce criara `CNAME` apontando para `<UUID>.cfargotunnel.com` para cada hostname que vai passar pelo Tunnel (ex.: `www`, `api`, `back`, etc.).

## 6) Validacao rapida

Valide de fora da rede (ex.: 4G do celular):
- `https://www.nexumaltivon.com`
- `https://api.nexumaltivon.com/health`

## 7) Regras de seguranca (nao negociar)

- Nao exponha o MySQL na internet. O MySQL deve ficar apenas na rede local/VPN.
- Use HTTPS antes de trafegar login/checkout/pagamentos.
- Nao coloque tokens/senhas em repositorio.
