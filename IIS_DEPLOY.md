<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Publicacao Windows Local

Este guia registra o fluxo operacional aceito para publicar o frontend e a API em servidor Windows local, mantendo banco e segredos privados.

## Premissas

- Projeto oficial em `D:\Nexum Altivon\NexumAltivon.com`.
- API oficial em `127.0.0.1:5010`.
- Banco MySQL/MariaDB XAMPP em `127.0.0.1:3309`.
- Dominios liberados na Cloudflare: `nexumaltivon.com.br` e `nexumaltivon.com`.
- Processo de exposicao publica preferencial: Cloudflare Tunnel ou proxy local com TLS.

## Frontend

Publicar o build do React em site estatico do IIS ou hospedagem equivalente.

Validacao:

```powershell
cd "D:\Nexum Altivon\NexumAltivon.com\NexumAltivon_Front-End"
npm run build
```

O frontend deve apontar para `https://api.nexumaltivon.com.br`.

## API

A API oficial deve rodar como processo local invisivel ao usuario, preferencialmente por tarefa agendada ou servico Windows.

Validacao da tarefa oficial:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\validar-api-oficial-24h-task.ps1"
```

Validacao publica:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\VALIDAR-PUBLICACAO-BACKEND.ps1" -TimeoutSec 45
```

## Variaveis Privadas

Configurar fora do Git:

- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__NexumDb`
- `ConnectionStrings__GenesisConnection`
- `JwtSettings__SecretKey`
- `Integracoes__MercadoPago__AccessToken`
- `Integracoes__SendGrid__ApiKey`
- `OpenAI__ApiKey`
- `Redis__ConnectionString`

## Banco

O banco oficial local deve permanecer em:

- `D:\xampp\mysql\data\nexum_altivon`
- `D:\xampp\mysql\data\genesis_bd`

Validar:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\verificar-banco-xampp.ps1"
```

## Criterio de Liberacao

- Solution Release compila sem avisos e sem erros.
- Frontend build compila sem erro.
- API responde `/health`.
- Banco responde via healthcheck.
- Login administrativo e cliente funcionam pelo portal publico.
- Nenhum segredo real entra no repositorio.
