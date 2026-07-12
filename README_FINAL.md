<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# GenesisGest.Net / Nexum Altivon

Repositorio oficial do ecossistema GenesisGest.Net v1.1.5 para o Grupo Nexum Altivon.

## Diretorio Oficial

Todo desenvolvimento operacional deve ocorrer em:

```text
D:\Nexum Altivon\NexumAltivon.com
```

Diretorios de backup, recuperacao ou consulta nao sao origem de desenvolvimento.

## Stack Ativa

- Backend: .NET 8 Minimal API.
- Banco: MySQL/MariaDB XAMPP em `127.0.0.1:3309`.
- Frontend: React.
- Desktop: WPF .NET 8 Windows.
- Publicacao: GitHub oficial e Cloudflare.

## Projetos Principais

- API: `NexumAltivon_Back-End\NexumAltivon.API.csproj`
- Frontend: `NexumAltivon_Front-End`
- ERP isolado: `NexumAltivon_ERP\NexumAltivon_ERP.csproj`
- Desktop: `NexumAltivon.Desktop\NexumAltivon.Desktop.csproj`
- Solution: `NexumAltivon.ERP.sln`

## Banco Oficial

Diretorios fisicos:

```text
D:\xampp\mysql\data\nexum_altivon
D:\xampp\mysql\data\genesis_bd
```

Validacao:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\verificar-banco-xampp.ps1"
```

## Build

```powershell
dotnet build "D:\Nexum Altivon\NexumAltivon.com\NexumAltivon.ERP.sln" -c Release --nologo
```

Frontend:

```powershell
cd "D:\Nexum Altivon\NexumAltivon.com\NexumAltivon_Front-End"
npm run build
```

## Runtime da API

- Porta local: `5010`
- URL local: `http://127.0.0.1:5010`
- URL publica: `https://api.nexumaltivon.com.br`

Validacao:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\validar-api-oficial-24h-task.ps1"
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\VALIDAR-PUBLICACAO-BACKEND.ps1" -TimeoutSec 45
```

## Regras de Entrega

- Commits devem sair do projeto oficial para o GitHub, nunca do backup para o projeto.
- Codigo nao compilado, tela decorativa ou fluxo sem persistencia nao pode ser marcado como pronto.
- Falta de segredo, banco, API ou certificado deve gerar erro claro e rastreavel.
- Toda alteracao deve ser validada por build, teste, smoke HTTP, consulta de banco ou verificacao equivalente.

## Documentos Operacionais

- `docs/CHECKLIST-CLINICO-GENESISGEST-2026-07-10.md`
- `docs/2026-06-16-checklist-prontidao-deploy.md`
- `docs/PORTAS_OFICIAIS.md`
- `docs/HANDOFF-PRODUCAO-PRIVADA-INTEGRACOES.md`
