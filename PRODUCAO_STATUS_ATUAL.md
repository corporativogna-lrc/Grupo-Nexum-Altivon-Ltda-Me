<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Produção - status atual

Última atualização manual: 2026-06-02 02:45 BRT.

## Estado verificado

- `https://www.nexumaltivon.com/`: respondeu `200 OK`.
- `www.nexumaltivon.com`: aponta para `corporativogna-lrc.github.io` e IPs do GitHub Pages.
- `https://www.nexumaltivon.com/login`: respondeu `404 Not Found` em checagem HTTP direta.
- `api.nexumaltivon.com`: DNS público retornou `Non-existent domain`.
- `https://api.nexumaltivon.com/`: não pôde ser acessado porque o DNS da API não resolve.

## Regra operacional

Antes e depois de qualquer alteração em produção, executar:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\VALIDAR-PUBLICACAO-BACKEND.ps1" -TimeoutSec 45
```

## Critério para publicar

- Site principal precisa responder `200 OK`.
- Login precisa abrir sem quebrar rota.
- API precisa resolver DNS e responder HTTP.
- Se qualquer item acima falhar, pausar deploy e corrigir a causa antes de continuar.
