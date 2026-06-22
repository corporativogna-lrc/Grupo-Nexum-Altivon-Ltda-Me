# API - ordem de execução

## Ambiente local

1. `scripts\01-instalar-api-local-permanente.cmd`
2. `scripts\02-instalar-api-definitiva-tarefa.cmd`
3. `scripts\03-reparar-tunel-api.cmd`

## Ambiente servidor

1. `scripts\server\01-instalar-api-24h-servidor.cmd`
2. `scripts\server\02-criar-pacote-api-24h-servidor.ps1`
3. `scripts\server\03-instalar-api-24h-pacote.cmd`
4. `scripts\server\04-iniciar-api-24h.ps1`
5. `scripts\server\05-verificar-api-24h.ps1`

## Utilitários

- `scripts\80-instalar-api-autostart-usuario.ps1`
- `scripts\99-desinstalar-api-definitiva-tarefa.ps1`
- `scripts\server\99-api.env.example.ps1`
