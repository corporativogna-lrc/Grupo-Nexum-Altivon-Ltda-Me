<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Deploy de produção GenesisGest.Net

## Origem oficial

- Projeto operacional: `D:\Nexum Altivon\NexumAltivon.com`.
- Repositório privado: `https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me`.
- Fluxo permitido: projeto oficial auditado para GitHub. Conteúdo remoto não substitui o projeto local sem revisão.
- Branch de produção: `main`.

## Componentes do servidor

- API ASP.NET Core: tarefa Windows `NexumAltivonApi24h`, origem `http://127.0.0.1:5010`.
- Banco: serviço `NexumAltivonMySQL`, porta `3309`, datadirs oficiais em `D:\xampp\mysql\data`.
- Publicação externa: serviço `Cloudflared`, HTTPS em `https://api.nexumaltivon.com.br`.
- Frontend: artefato React publicado pelo fluxo GitHub Pages configurado no repositório.

## Segredos

Segredos não podem existir em arquivos versionados. A API do servidor carrega `runtime\api-24h\api.env.ps1`, ignorado pelo Git, contendo as conexões, segredo JWT e credenciais das integrações realmente habilitadas.

Depois de exposição acidental, o valor deve ser rotacionado no provedor e no arquivo privado. Remover apenas o texto do Git não revoga uma credencial já divulgada.

## Atualização da API

Em PowerShell elevado:

```powershell
Set-Location "D:\Nexum Altivon\NexumAltivon.com"
git status --short --branch
dotnet build .\NexumAltivon.ERP.sln -c Release
powershell.exe -NoProfile -ExecutionPolicy Bypass -File ".\scripts\server\atualizar-api-oficial-5010.ps1"
```

O atualizador executa `dotnet publish`, instala a tarefa exclusiva de boot como `SYSTEM` e encerra com erro se banco, túnel, API local ou API pública não estiverem saudáveis.

## Atualização do frontend

```powershell
Set-Location "D:\Nexum Altivon\NexumAltivon.com\NexumAltivon_Front-End"
npm ci
npm run build
```

O artefato publicado deve ser produzido pelo workflow oficial após commit auditado. Não alterar manualmente o conteúdo remoto para divergir do código-fonte.

## Verificação pós-deploy

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\validar-api-oficial-24h-task.ps1"
Invoke-WebRequest -UseBasicParsing "https://api.nexumaltivon.com.br/health"
Invoke-WebRequest -UseBasicParsing "https://api.nexumaltivon.com.br/health/db"
```

Os detalhes de boot, recuperação e diagnóstico estão em `API_24H_SERVIDOR.md`.
