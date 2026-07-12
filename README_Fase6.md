<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Infraestrutura e Deploy

Este arquivo registra somente o fluxo de infraestrutura aceito para a fase atual do GenesisGest.Net v1.1.5.

## Ambientes

- Desenvolvimento operacional: `D:\Nexum Altivon\NexumAltivon.com`
- API local: `http://127.0.0.1:5010`
- Banco local: `127.0.0.1:3309`
- API publica: `https://api.nexumaltivon.com.br`
- Frontend publico: `https://www.nexumaltivon.com.br`

## Docker

Arquivos oficiais:

- `docker\docker-compose.yml`
- `docker\docker-compose.prod.yml`
- `docker\docker-compose.staging.yml`
- `docker\Dockerfile.api`
- `docker\Dockerfile.frontend`

O compose de desenvolvimento deve subir API, frontend, MySQL, Redis, MinIO e Nginx quando Docker estiver instalado no host.

## Secrets

Nao versionar:

- `.env` real.
- `appsettings.Production.json` real.
- Senhas do MySQL.
- Chaves JWT.
- Tokens de Mercado Pago, SendGrid, OpenAI, Cloudflare ou GitHub.

Usar variaveis de ambiente, User Secrets em desenvolvimento ou cofre do provedor em producao.

## Validacao Antes de Deploy

```powershell
dotnet build "D:\Nexum Altivon\NexumAltivon.com\NexumAltivon.ERP.sln" -c Release --nologo
```

```powershell
cd "D:\Nexum Altivon\NexumAltivon.com\NexumAltivon_Front-End"
npm run build
```

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\VALIDAR-PUBLICACAO-BACKEND.ps1" -TimeoutSec 45
```

## Backup e Restore

Scripts oficiais ficam dentro do projeto oficial ou em `docker\scripts`.

O backup deve ser validado por restore de teste antes de migration destrutiva ou deploy com alteracao de schema.

## GitHub

Repositorio oficial:

```text
https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me
```

Fluxo correto:

1. Auditar worktree local.
2. Validar build/testes/smoke.
3. Criar commit atomico.
4. Enviar a branch oficial.
5. Fazer merge para `main` somente com diff revisado.
