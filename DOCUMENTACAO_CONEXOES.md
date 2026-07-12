<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Documentacao de Conexoes Oficiais

Este documento fixa as conexoes operacionais aceitas para desenvolvimento e publicacao do GenesisGest.Net / Nexum Altivon.

## Diretorios Oficiais

- Projeto: `D:\Nexum Altivon\NexumAltivon.com`
- Banco e-commerce: `D:\xampp\mysql\data\nexum_altivon`
- Banco GenesisGest: `D:\xampp\mysql\data\genesis_bd`
- Repositorio GitHub: `https://github.com/corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me`

Diretorios de backup, recuperacao ou consulta nao sao fonte de desenvolvimento. Material externo so pode ser usado apos validacao tecnica e incorporacao no projeto oficial.

## Portas e URLs

- API local oficial: `http://127.0.0.1:5010`
- API publica: `https://api.nexumaltivon.com.br`
- Site publico principal: `https://www.nexumaltivon.com.br`
- Dominio secundario liberado: `nexumaltivon.com`
- MySQL local XAMPP: `127.0.0.1:3309`

## Fluxo Operacional

1. O navegador acessa `www.nexumaltivon.com.br`.
2. O frontend carrega `api-runtime.json` ou configuracao equivalente.
3. O frontend chama `https://api.nexumaltivon.com.br`.
4. O tunel ou proxy encaminha para a API local em `127.0.0.1:5010`.
5. A API usa connection strings privadas para `nexum_altivon` e `genesis_bd`.
6. O banco permanece privado e nao deve ser exposto diretamente na internet.

## Validacoes Obrigatorias

Executar antes de publicar alteracao operacional:

```powershell
dotnet build "D:\Nexum Altivon\NexumAltivon.com\NexumAltivon.ERP.sln" -c Release --nologo
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\VALIDAR-PUBLICACAO-BACKEND.ps1" -TimeoutSec 45
```

Validar banco:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\verificar-banco-xampp.ps1"
```

## Regras

- Nenhuma senha, token ou connection string real deve ser versionada.
- A API deve falhar de forma clara quando segredo obrigatorio nao existir.
- Endpoints consumidos pelo frontend devem responder `200`, `201`, `204`, `400`, `401` ou `403`; nunca `404` por rota ausente.
- Alteracoes locais so devem ir ao GitHub depois de build, varredura e diff revisado.
