<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Checklist de prontidão do servidor

## API, banco e túnel

- [Concluído em 2026-07-17] Projeto operacional em disco local fixo: `D:\Nexum Altivon\NexumAltivon.com`.
- [Concluído em 2026-07-17] Banco e-commerce em `D:\xampp\mysql\data\nexum_altivon`.
- [Concluído em 2026-07-17] Banco GenesisGest em `D:\xampp\mysql\data\genesis_bd`.
- [Concluído em 2026-07-17] Serviço `NexumAltivonMySQL`: `Running`, `Automatic`, conta `LocalSystem`.
- [Concluído em 2026-07-17] Serviço `Cloudflared`: `Running`, `Automatic`, conta `LocalSystem`.
- [Concluído em 2026-07-17] Serviço `Schedule`: `Running`, `Automatic`, conta `LocalSystem`.
- [Concluído em 2026-07-17] API exclusiva em `http://127.0.0.1:5010`.
- [Concluído em 2026-07-17] API pública em `https://api.nexumaltivon.com.br`.

## Inicialização sem login

- [Concluído em 2026-07-17] Tarefa única `NexumAltivonApi24h` executada como `SYSTEM` com `LogonType=ServiceAccount` e `RunLevel=Highest`.
- [Concluído em 2026-07-17] Exatamente um gatilho de boot e nenhum gatilho de logon.
- [Concluído em 2026-07-17] `StartWhenAvailable=True`, `RestartCount=999`, `RestartInterval=PT1M`, `ExecutionTimeLimit=PT0S` e `MultipleInstances=IgnoreNew`.
- [Concluído em 2026-07-17] Ação invisível aponta para `scripts\server\iniciar-api-oficial-24h.ps1` e usa somente a porta `5010`.
- [Concluído em 2026-07-17] Teste controlado `Stop-ScheduledTask`/`Start-ScheduledTask`: API iniciada pela conta `SYSTEM` em 22 segundos, HTTP 200, sem execução interativa.
- [Pendente de janela de manutenção] Reinicialização física do Windows e consulta externa enquanto o servidor permanece na tela de senha. O reboot não foi executado durante desenvolvimento para não interromper operações sem janela autorizada.

## Recuperação de falha

- [Concluído em 2026-07-17] Supervisor persistente com mutex global, retenção de logs e espera progressiva.
- [Concluído em 2026-07-17] Processo filho PID `7432` encerrado de forma controlada; nova API PID `12184` iniciou automaticamente em 35 segundos sem reiniciar manualmente a tarefa.
- [Concluído em 2026-07-17] Após a recuperação havia uma API e um supervisor, ambos vinculados à tarefa oficial.
- [Concluído em 2026-07-17] Após a recuperação, `/health`, `/health/db`, `/health/db/genesis` e `https://api.nexumaltivon.com.br/health` retornaram HTTP 200.

## Estrutura e manutenção

- [Concluído em 2026-07-17] Publicação e configuração permanecem em `runtime\api-24h` dentro do projeto oficial.
- [Concluído em 2026-07-17] O instalador legado foi consolidado como entrada compatível para o atualizador oficial e recusa pasta externa ou porta divergente.
- [Concluído em 2026-07-17] O atualizador para somente a tarefa e processos identificados como oficiais; processo desconhecido na `5010` gera erro e não é encerrado.
- [Concluído em 2026-07-17] Documentação de instalação, deploy, validação e diagnóstico alinhada ao servidor Windows real.

## Comandos oficiais

Atualizar e reinstalar:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\atualizar-api-oficial-5010.ps1"
```

Validar:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\validar-api-oficial-24h-task.ps1"
```

Consultar falha:

```powershell
Get-Content "D:\Nexum Altivon\NexumAltivon.com\runtime-logs\api-24h\supervisor.log" -Tail 100
Get-ScheduledTaskInfo -TaskName NexumAltivonApi24h | Format-List *
```
