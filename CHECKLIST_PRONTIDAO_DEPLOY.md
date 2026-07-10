<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Checklist de Prontidao de Deploy - Nexum Altivon

## Estado operacional validado

- Banco local validado em 2026-07-09: `localhost:3309`, `127.0.0.1:3309` e `192.168.1.66:3309`.
- O IP historico `192.168.1.72` nao esta atribuido nesta sessao; nao usar esse endpoint ate reconfigurar IP fixo ou atualizar as connection strings privadas para `127.0.0.1`.
- Datadir fisico do banco: `D:\xampp\mysql\data\nexum_altivon`.
- Datadir fisico Genesis: `D:\xampp\mysql\data\genesis_bd`.
- Rotas compartilhadas oficiais informadas: `Y:\xampp\mysql\data\nexum_altivon` e `Y:\xampp\mysql\data\genesis_bd`.
- Estrutura XAMPP/MySQL validada em 2026-07-09: arquivos `mysqld.exe`, `mysqladmin.exe`, `my.ini` e os dois datadirs existem.
- Serviço real do banco: `NexumAltivonMySQL`, `Running`, `Automatic`, porta `3309` em listener.
- Serviço legado `mysql`: permanece `StartPending` e `Disabled`; nao deve ser usado pela API.
- API local do servidor: `http://127.0.0.1:5010/health` e `http://127.0.0.1:5010/health/db`.
- API publica validada: `https://api.nexumaltivon.com.br/health`.
- DNS/rota ainda pendente de validacao: `https://api.nexumaltivon.com/health`.
- Banco restaurado em 2026-07-09 a partir do datadir completo recuperado com `ibdata1`; datadir quebrado preservado em `D:\NexumAltivon_DB_RECOVERY\data-broken-20260709-210126`.

## Riscos operacionais atuais

1. `api.nexumaltivon.com` ainda nao foi validado com DNS/rota Cloudflare nesta operacao.
   Correcao: criar/validar hostname publico no Cloudflare apontando para a mesma origem `http://127.0.0.1:5010` ja usada por `api.nexumaltivon.com.br`.

2. Serviço local da API pode estar ausente apos reinicialização ou queda de energia.
   Correcao: executar `scripts\server\VERIFICAR-BANCO-XAMPP-COMO-ADMIN.cmd -StartIfStopped` e depois `scripts\server\INSTALAR-API-24H-SERVIDOR-COMO-ADMIN.cmd` no servidor, apos criar `D:\NexumAltivon_API_24H\config\api.env.ps1` com valores reais.

3. Connection strings privadas ainda podem apontar para `192.168.1.72`, que nao esta atribuido nesta sessao.
   Correcao: para execucao no mesmo servidor do banco, usar `127.0.0.1:3309` nas variaveis privadas `ConnectionStrings__DefaultConnection`, `ConnectionStrings__NexumDb` e `ConnectionStrings__GenesisConnection`, ou restabelecer IP fixo `192.168.1.72` no adaptador de rede antes de instalar a API 24h.

4. Estoque duplicado entre lojas pode permitir venda de produto divergente.
   Correcao: executar `Database\2026-06-18-unificar-lojas-estoque.sql` contra `127.0.0.1:3309/nexum_altivon` no servidor do banco.

## Script de correcao de estoque

Arquivo: `Database\2026-06-18-unificar-lojas-estoque.sql`

Executar:

```bat
D:\xampp\mysql\bin\mysql.exe -h 127.0.0.1 -P 3309 -u nexum_app -p nexum_altivon < "D:\Nexum Altivon\NexumAltivon.com\Database\2026-06-18-unificar-lojas-estoque.sql"
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
- `scripts\server\INSTALAR-API-24H-SERVIDOR-COMO-ADMIN.cmd`
- `scripts\server\instalar-api-24h-servidor.ps1`
- `scripts\server\VERIFICAR-BANCO-XAMPP-COMO-ADMIN.cmd`
- `scripts\server\verificar-banco-xampp.ps1`
- `scripts\server\CONFIGURAR-USUARIO-BANCO-XAMPP-COMO-ADMIN.cmd`
- `scripts\server\configurar-usuario-banco-xampp.ps1`
- `scripts\server\REPARAR-BANCO-XAMPP-SERVICO-COMO-ADMIN.cmd`
- `scripts\server\reparar-banco-xampp-servico.ps1`
- `scripts\server\ATIVAR-CLOUDFLARE-TUNNEL-COMO-ADMIN.cmd`
- `scripts\server\ativar-cloudflare-tunnel.ps1`
- `scripts\server\verificar-api-24h.ps1`

## Verificacao final

```bat
curl -i http://127.0.0.1:5010/health/db
curl -i https://api.nexumaltivon.com.br/health/db
```

Validacao operacional no servidor:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\server\verificar-api-24h.ps1
```

O retorno esperado nos testes validados e `{"status":"Healthy"}` para `/health/db` e HTTP `200` para `/health`. O domínio `.com` deve ser acrescentado ao teste após configuração real no Cloudflare.
