<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 -->

# Portas Oficiais do Ambiente

## Servidor oficial

- Banco MySQL/MariaDB: `192.168.1.72:3309`
- API local/tunel: `5010`
- Frontend/site local: `3000`
- Admin/ERP local: `3002`
- Rotas auxiliares frontend/admin: `3010`
- Proxy/container interno quando aplicavel: `8080`

## Regra sobre 3306

`3306` nao e porta oficial do servidor. Ela e considerada porta de teste/dev e nao deve
ser usada para a conexao operacional do GenesisGest.Net.

Quando aparecer `3306` em Docker Compose, deve ser entendido apenas como porta interna
do container MySQL ou ambiente isolado de desenvolvimento. A exposicao/autoridade do
banco do ambiente oficial continua sendo `3309`.

## Regra sobre portas antigas da API

Referencias antigas de API local devem ser corrigidas para `5010`. Nao usar porta
alternativa como rota operacional para mascarar conflito.
