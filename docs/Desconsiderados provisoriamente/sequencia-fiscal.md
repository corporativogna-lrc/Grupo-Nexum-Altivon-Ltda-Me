<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Sequencia Fiscal

Fluxo critico: preparacao, certificado, autorizacao SEFAZ, armazenamento XML, cancelamento e eventos.

```mermaid
sequenceDiagram
    autonumber
    participant Operador
    participant API as "Minimal API Fiscal"
    participant Banco as "MySQL"
    participant Certificado as "Certificado A1/A3"
    participant Sefaz as "SEFAZ Homologacao/Producao"
    participant Storage as "S3/MinIO"
    participant Auditoria
    participant Financeiro

    Operador->>API: POST /api/fiscal/preparar-emissao-manual
    API->>Banco: carrega pedido, itens, emitente e destinatario
    API->>Certificado: valida disponibilidade e senha via cofre
    API->>Sefaz: POST /api/fiscal/nfe/emitir ou /nfce/emitir
    Sefaz-->>API: autorizacao, rejeicao ou denegacao
    API->>Storage: grava XML autorizado e DANFE
    API->>Banco: persiste chave, protocolo, status e trilha
    API->>Auditoria: registra evento fiscal
    API->>Financeiro: vincula documento fiscal ao faturamento

    alt cancelamento
        Operador->>API: POST /api/fiscal/cancelar
        API->>Sefaz: envia evento de cancelamento
        Sefaz-->>API: protocolo de cancelamento
        API->>Banco: atualiza status fiscal
        API->>Storage: grava XML de evento
    else carta de correcao
        Operador->>API: POST /api/fiscal/cartacorrecao
        API->>Sefaz: envia evento CC-e
        Sefaz-->>API: protocolo CC-e
        API->>Storage: grava XML de evento
    end
```

## Contratos

- Preparacao: `POST /api/fiscal/preparar-emissao-manual`
- NF-e: `POST /api/fiscal/nfe/emitir`
- NFC-e: `POST /api/fiscal/nfce/emitir`
- Cancelamento: `POST /api/fiscal/cancelar`
- Inutilizacao: `POST /api/fiscal/inutilizar`
- Carta de correcao: `POST /api/fiscal/cartacorrecao`
- SPED: `POST /api/fiscal/sped`

## Validacao

- Certificado vem do cofre, sem segredo em arquivo.
- Ambiente de homologacao fica separado de producao.
- XML autorizado e eventos ficam armazenados.
- Rejeicao SEFAZ retorna codigo e mensagem tratavel.
- Auditoria registra usuario, tenant, entidade e acao.
- Financeiro recebe chave fiscal para conciliacao.
