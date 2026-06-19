# Checklist de Prontidao de Deploy - Nexum Altivon

## Estado operacional validado

- Banco unico: `192.168.1.72:3309`, database `nexum_altivon`.
- Datadir do banco: `Y:\xampp\mysql\data\nexum_altivon`.
- API local do servidor: `http://192.168.1.72:5010/health/db`.
- DNS preparado no Cloudflare: `api.nexumaltivon.com` e raiz apontados para `201.216.81.88`.

## 3 riscos que impedem operar

1. `api.nexumaltivon.com` ainda depende da ativacao definitiva dos nameservers no registrador.
   Correcao: no Wix/registrador, trocar para `destiny.ns.cloudflare.com` e `jonah.ns.cloudflare.com`.

2. Porta 80 publica ainda pode estar presa no Apache/XAMPP errado.
   Correcao: executar `scripts\server\ATIVAR-API-PORTA-80-COMO-ADMIN.cmd` no servidor, ou manter Apache com vhost `api.nexumaltivon.com -> http://127.0.0.1:5010`.

3. Estoque duplicado entre lojas pode permitir venda de produto divergente.
   Correcao: executar `Database\2026-06-18-unificar-lojas-estoque.sql` contra `192.168.1.72:3309/nexum_altivon`.

## Script de correcao de estoque

Arquivo: `Database\2026-06-18-unificar-lojas-estoque.sql`

Executar:

```bat
Y:\xampp\mysql\bin\mysql.exe -h 192.168.1.72 -P 3309 -u nexum_app -p nexum_altivon < "Y:\Nexum Altivon\NexumAltivon.com\Database\2026-06-18-unificar-lojas-estoque.sql"
```

## Protocolo de lancamento

Atualizar/sobrepor estes arquivos no servidor principal:

- `NexumAltivon_Back-End\API\Program.cs`
- `NexumAltivon_Back-End\API\Services\NotificacaoService.cs`
- `NexumAltivon_Back-End\API\appsettings.json`
- `NexumAltivon_Front-End\src\services\api.js`
- `NexumAltivon_Front-End\admin\index.html`
- `NexumAltivon.Desktop\MainWindow.xaml.cs`
- `NexumAltivon.Desktop\ManualNfeWindow.xaml.cs`
- `Database\2026-06-18-unificar-lojas-estoque.sql`
- `scripts\server\REPARAR-CONEXOES-SERVIDOR-COMO-ADMIN.cmd`
- `scripts\server\15-reparar-conexoes-servidor.ps1`
- `scripts\server\ATIVAR-API-PORTA-80-COMO-ADMIN.cmd`
- `scripts\server\ativar-api-porta-80.ps1`

## Verificacao final

```bat
curl -i http://192.168.1.72:5010/health/db
curl -i http://192.168.1.72/health/db
curl -i https://api.nexumaltivon.com/health/db
```

O retorno esperado nos tres testes e `{"status":"Healthy"}`.
