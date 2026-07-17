<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# API oficial 24h no servidor Windows

## Arquitetura operacional

A API de produção não depende de login, área de trabalho aberta, Codex, navegador ou sessão de usuário.

Fluxo de inicialização:

1. O Windows inicia os serviços automáticos `NexumAltivonMySQL`, `Cloudflared` e `Schedule`.
2. A tarefa `NexumAltivonApi24h` é disparada pelo boot com atraso de 15 segundos.
3. A tarefa executa como `SYSTEM`, `ServiceAccount`, nível `Highest` e janela oculta.
4. `scripts\server\iniciar-api-oficial-24h.ps1` mantém um único supervisor residente.
5. O supervisor inicia `runtime\api-24h\api\NexumAltivon.API.dll` em `http://127.0.0.1:5010`.
6. Se o processo da API encerrar, o supervisor registra o código de saída e o reinicia automaticamente.
7. O serviço `Cloudflared` publica a origem local em `https://api.nexumaltivon.com.br`.

O mutex global `GenesisGest_NexumAltivonApi24h` e a política `IgnoreNew` impedem supervisores e APIs duplicados. A tarefa não possui gatilho de logon e não possui limite de duração.

## Estrutura oficial

- Projeto: `D:\Nexum Altivon\NexumAltivon.com`.
- Publicação: `D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api`.
- Configuração privada: `D:\Nexum Altivon\NexumAltivon.com\runtime\api-24h\api.env.ps1`.
- Logs da API e supervisor: `D:\Nexum Altivon\NexumAltivon.com\runtime-logs\api-24h`.
- Log de atualização: `D:\Nexum Altivon\NexumAltivon.com\runtime-logs\api-oficial-5010-update.log`.
- Dados e-commerce: `D:\xampp\mysql\data\nexum_altivon`.
- Dados GenesisGest: `D:\xampp\mysql\data\genesis_bd`.

Nenhuma pasta externa ao projeto é aceita pelo instalador.

## Instalação e atualização

Executar em PowerShell como Administrador:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\atualizar-api-oficial-5010.ps1"
```

O comando publica a API, para somente a tarefa oficial, registra novamente a tarefa como `SYSTEM`, inicia o runtime e valida banco, API local e API pública. Ele recusa outra porta e não encerra processo desconhecido que ocupe a `5010`.

O comando de compatibilidade abaixo encaminha para o mesmo atualizador oficial:

```cmd
"D:\Nexum Altivon\NexumAltivon.com\scripts\server\INSTALAR-API-24H-SERVIDOR-COMO-ADMIN.cmd"
```

## Configuração privada obrigatória

`runtime\api-24h\api.env.ps1` não pode ser versionado. O supervisor falha de forma explícita se não encontrar:

- `ConnectionStrings__DefaultConnection`;
- `ConnectionStrings__GenesisConnection`;
- `JwtSettings__SecretKey` ou `JWT_SECRET_KEY` com pelo menos 32 bytes.

As conexões locais usam o serviço `NexumAltivonMySQL` na porta `3309`. Senhas, tokens e certificados permanecem somente na configuração privada do servidor.

## Validação

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\validar-api-oficial-24h-task.ps1"
```

O retorno só é positivo quando todos estes critérios são verdadeiros:

- projeto em disco local fixo;
- tarefa em execução como `SYSTEM`/`ServiceAccount`/`Highest`;
- um gatilho de boot e nenhum gatilho de logon;
- `StartWhenAvailable`, `RestartCount=999`, `RestartInterval=PT1M`, `ExecutionTimeLimit=PT0S` e `IgnoreNew`;
- ação oculta apontando para o supervisor oficial e porta `5010`;
- `Schedule`, `NexumAltivonMySQL` e `Cloudflared` em `Running` e `Automatic`;
- processo `NexumAltivon.API.dll` escutando a porta `5010`;
- healthchecks locais de API, `nexum_altivon` e `genesis_bd` com HTTP 2xx;
- healthcheck e configuração pública acessíveis por HTTPS no domínio oficial.

## Diagnóstico sem dependência pessoal

Outro desenvolvedor pode diagnosticar a operação somente com os arquivos versionados e estes comandos:

```powershell
Get-ScheduledTask -TaskName NexumAltivonApi24h | Format-List *
Get-ScheduledTaskInfo -TaskName NexumAltivonApi24h | Format-List *
Get-Service Schedule,NexumAltivonMySQL,Cloudflared | Format-Table Name,Status,StartType
Get-Content "D:\Nexum Altivon\NexumAltivon.com\runtime-logs\api-24h\supervisor.log" -Tail 100
Get-ChildItem "D:\Nexum Altivon\NexumAltivon.com\runtime-logs\api-24h" -Filter "api-*.stderr.log" | Sort-Object LastWriteTime -Descending | Select-Object -First 1 | Get-Content -Tail 100
```

O supervisor mantém os 30 arquivos mais recentes de saída e erro. Falhas repetidas usam espera progressiva de até 300 segundos para evitar ciclo agressivo de CPU e disco.

## Comprovação após manutenção programada

O teste definitivo de boot físico deve ser executado em uma janela autorizada de reinicialização do servidor. Após o Windows chegar à tela de senha, sem efetuar login, um equipamento externo deve consultar `https://api.nexumaltivon.com.br/health`. A resposta HTTP 200 comprova a cadeia completa de boot, banco, tarefa, API e túnel.
