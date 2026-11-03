<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Sequencia de Venda

Fluxo critico: cadastro, carrinho, pedido, pagamento, estoque, fiscal, logistica, financeiro e BI.

```mermaid
sequenceDiagram
    autonumber
    participant Cliente
    participant Site as "Frontend Nexum"
    participant API as "Minimal API"
    participant Banco as "MySQL nexum_altivon"
    participant Pagamento as "Mercado Pago"
    participant Estoque as "Estoque/WMS"
    participant Fiscal as "Fiscal NF-e/NFC-e"
    participant Logistica
    participant Financeiro
    participant BI as "Cockpit Executivo"

    Cliente->>Site: informa dados e itens
    Site->>API: POST /api/clientes ou login
    API->>Banco: valida cliente e tenant
    Site->>API: POST /api/pedidos
    API->>Banco: grava pedido e itens
    API->>Pagamento: cria preferencia ou cobranca
    Pagamento-->>API: webhook /api/webhooks/mercadopago
    API->>Banco: atualiza status de pagamento
    API->>Estoque: baixa reservas e movimentos
    Estoque->>Banco: persiste movimentacao e kardex
    API->>Fiscal: prepara emissao fiscal
    Fiscal->>Banco: grava XML, chave e status
    API->>Logistica: roteiriza entrega
    Logistica->>Banco: atualiza rastreio e status operacional
    API->>Financeiro: registra receita e parcelas
    Financeiro->>Banco: confirma lancamento
    BI->>Banco: consolida indicadores
    API-->>Site: retorna acompanhamento do pedido
```

## Contratos

- Pedido: `POST /api/pedidos`
- Acompanhamento: `GET /api/pedidos/acompanhar`
- Webhook: `POST /api/webhooks/mercadopago`
- Fiscal: `POST /api/fiscal/nfe/emitir` e `POST /api/fiscal/nfce/emitir`
- Logistica: `POST /api/logistica/roteamento`
- BI: `GET /api/dashboard/resumo`

## Validacao

- Pedido criado com tenant correto.
- Pagamento recebido por webhook e conciliado.
- Estoque movimentado sem quantidade negativa.
- Documento fiscal emitido em homologacao.
- Receita registrada no financeiro.
- Indicadores atualizados no cockpit.
