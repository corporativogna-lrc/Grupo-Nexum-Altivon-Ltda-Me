# Propriedade intelectual: Luís Rodrigo da Costa
# Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
# Sistema de gestão: GenesisGest.Net
# Ano Início: 04/2024 Publicado e operacional: 05/2026
# Versão: 1.1.5

# Bloqueios Operacionais

## 2026-07-02 - API 5012 conectada ao runtime antigo de banco

Status: bloqueio operacional externo ao repositorio, pendente de aplicacao no servidor principal.

Evidencia objetiva:
- GitHub Pages publicou com sucesso o commit `a84020db47288d4dc6cd47c03881df2488d8a658`.
- `https://nexumaltivon.com.br/api-runtime.json` aponta para `https://api.nexumaltivon.com.br`.
- `https://api.nexumaltivon.com.br/health` retorna `200 Healthy`.
- `https://api.nexumaltivon.com.br/health/db` retorna `500` com detalhe `Banco configurado, mas sem conexão`.
- `https://api.nexumaltivon.com.br/api/produtos/destaques` retorna `500`.

Causa localizada:
- O `appsettings.json` publicado no pacote oficial em `Y:\Nexum Altivon\_publish_runtime_main\NexumAltivon_Back-End\API` apontava as connection strings MySQL para `localhost:3309`.
- A sessao HeidiSQL operacional usa `192.168.1.72:3309`, usuario `nexum_app`, bancos `genesis_bd` e `nexum_altivon`.
- O pacote em `Y:` foi corrigido para `192.168.1.72:3309`, mas a API ativa ainda roda a partir do runtime do servidor `C:\NexumAltivon_API_24H\api` e nao recarregou a configuracao corrigida.

Tentativas de aplicacao remota:
- `SC` remoto para `192.168.1.72` retornou acesso negado.
- `WMI` remoto para `192.168.1.72` retornou acesso negado.
- `tasklist /S 192.168.1.72` retornou credenciais invalidas.
- Admin share `\\192.168.1.72\C$` nao ficou acessivel de forma confiavel nesta sessao.

Correcao preparada:
- `Y:\Nexum Altivon\_publish_runtime_main\NexumAltivon_Back-End\API\appsettings.json` esta corrigido para `192.168.1.72:3309`.
- `Y:\Nexum Altivon\NexumAltivon.com\scripts\server\APLICAR-API-5012-SERVIDOR.cmd` foi reforcado para bloquear aplicacao se ainda existir `localhost:3309` e validar `/health/db`, `/health/db/genesis` e `/api/produtos/destaques` antes de declarar sucesso.

Acao necessaria no servidor:
- Executar como administrador, no proprio servidor `192.168.1.72`, o script:
  `Y:\Nexum Altivon\NexumAltivon.com\scripts\server\APLICAR-API-5012-SERVIDOR.cmd`

Condicao de desbloqueio:
- `https://api.nexumaltivon.com.br/health/db` retorna `200`.
- `https://api.nexumaltivon.com.br/health/db/genesis` retorna `200`.
- `https://api.nexumaltivon.com.br/api/produtos/destaques` retorna `200`.
