<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Sequencia de Compra

Fluxo critico: solicitacao, cotacao, aprovacao, pedido, entrada, estoque, contas a pagar e BI.

```mermaid
sequenceDiagram
    autonumber
    participant Usuario as "Solicitante"
    participant API as "Minimal API"
    participant Workflow as "Workflow de Aprovacao"
    participant Compras
    participant Fornecedor
    participant Portaria
    participant Estoque as "SCM/WMS"
    participant Financeiro as "FICO"
    participant Banco as "MySQL"
    participant BI as "Cockpit Executivo"

    Usuario->>API: POST /api/compras/solicitacoes
    API->>Banco: grava solicitacao
    API->>Workflow: cria instancia de aprovacao
    Workflow-->>API: transicao aprovada
    API->>Compras: abre cotacao
    Compras->>Fornecedor: solicita precos e prazo
    Fornecedor-->>Compras: retorna proposta
    Compras->>API: POST /api/compras/pedidos
    API->>Banco: grava pedido de compra
    Fornecedor->>Portaria: entrega mercadoria
    Portaria->>API: POST /api/portaria/entradas
    API->>Estoque: registra entrada e lote
    Estoque->>Banco: atualiza saldo e kardex
    API->>Financeiro: gera conta a pagar
    Financeiro->>Banco: grava vencimentos
    BI->>Banco: atualiza indicadores de compra
```

## Contratos

- Solicitacoes: `POST /api/compras/solicitacoes`
- Cotacoes: `POST /api/compras/cotacoes`
- Pedidos: `POST /api/compras/pedidos`
- Entradas: `POST /api/compras/entradas`
- Portaria: `POST /api/portaria/entradas`
- Financeiro: `POST /api/financeiro/lancamentos`

## Validacao

- Solicitacao nasce vinculada ao tenant.
- Workflow registra aprovador, data e decisao.
- Pedido preserva fornecedor, centro de custo e itens.
- Entrada atualiza estoque com rastreabilidade.
- Conta a pagar criada com vencimento e valor corretos.
- BI reflete compras aprovadas e recebidas.
