<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Fase 2 - Painel Administrativo

Data de revisao: 2026-07-12.

## Estado oficial

- O painel administrativo operacional fica no React oficial: `NexumAltivon_Front-End/src/pages/Login.js`, `NexumAltivon_Front-End/src/pages/Dashboard.js`, `NexumAltivon_Front-End/src/context/AuthContext.js` e `NexumAltivon_Front-End/src/services/api.js`.
- A API oficial e a Minimal API em `NexumAltivon_Back-End/API/Program.cs`.
- O HTML paralelo `NexumAltivon_Front-End/admin/index.html` nao e painel produtivo e foi desativado para redirecionar ao login oficial `/?/login`.
- `admin-painel.html` na raiz e em `NexumAltivon_Front-End/public/admin-painel.html` tambem redirecionam ao login oficial.
- Nenhum fluxo administrativo deve usar painel estatico separado do React oficial.

## Endpoints administrativos confirmados no `Program.cs`

| Endpoint | Metodo | Politica | Uso atual |
|---|---|---|---|
| `/api/admin/dashboard/completo` | GET | `Gerente` | Dashboard completo do painel React |
| `/api/admin/dashboard/kpis` | GET | `Gerente` | KPIs administrativos do painel React |
| `/api/admin/usuarios` | GET/POST | `Admin` | Gestao real de usuarios administrativos |
| `/api/admin/usuarios/{id:int}` | PUT | `Admin` | Atualizacao real de usuario administrativo |
| `/api/admin/usuarios/perfis` | GET | `Admin` | Lista real de perfis administrativos |

## Validacao operacional

Comandos executados nesta revisao:

```powershell
npm.cmd run build
```

Resultado: build React concluido com sucesso em `NexumAltivon_Front-End`.

Validacoes publicas executadas para a API oficial:

```powershell
Invoke-WebRequest http://127.0.0.1:5010/health
Invoke-WebRequest http://127.0.0.1:5010/health/db
Invoke-WebRequest http://127.0.0.1:5010/health/db/genesis
Invoke-WebRequest https://api.nexumaltivon.com.br/health
Invoke-WebRequest https://api.nexumaltivon.com.br/health/db
Invoke-WebRequest https://api.nexumaltivon.com.br/health/db/genesis
```

Resultado: HTTP 200 nos endpoints acima.

## Regra de manutencao

Componentes visuais uteis do HTML desativado so podem voltar ao produto se forem migrados para o React oficial e ligados a endpoints reais da API, com persistencia validada no banco.
