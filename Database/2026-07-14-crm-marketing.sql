/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

CREATE TABLE IF NOT EXISTS crm_segmentos (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    row_version BLOB NOT NULL,
    nome VARCHAR(100) NOT NULL,
    descricao VARCHAR(500) NULL,
    cor VARCHAR(20) NOT NULL DEFAULT '#C9A227',
    prioridade INT NOT NULL DEFAULT 1,
    ticket_medio_minimo DECIMAL(15,2) NULL,
    ticket_medio_maximo DECIMAL(15,2) NULL,
    frequencia_minima_dias INT NULL,
    frequencia_maxima_dias INT NULL,
    ativo TINYINT(1) NOT NULL DEFAULT 1,
    created_at DATETIME NOT NULL,
    created_by_user_id CHAR(36) NULL,
    updated_at DATETIME NULL,
    updated_by_user_id CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME NULL,
    PRIMARY KEY (id),
    UNIQUE KEY ux_crm_segmentos_tenant_nome (tenant_id, nome),
    KEY ix_crm_segmentos_tenant_deleted (tenant_id, is_deleted)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

CREATE TABLE IF NOT EXISTS crm_campanhas (
    id CHAR(36) NOT NULL,
    tenant_id CHAR(36) NOT NULL,
    row_version BLOB NOT NULL,
    nome VARCHAR(200) NOT NULL,
    descricao VARCHAR(1000) NULL,
    tipo VARCHAR(40) NOT NULL,
    status VARCHAR(40) NOT NULL,
    segmento_id CHAR(36) NULL,
    data_inicio DATETIME NOT NULL,
    data_fim DATETIME NULL,
    orcamento DECIMAL(15,2) NOT NULL DEFAULT 0.00,
    custo_atual DECIMAL(15,2) NOT NULL DEFAULT 0.00,
    alcance INT NOT NULL DEFAULT 0,
    cliques INT NOT NULL DEFAULT 0,
    leads_gerados INT NOT NULL DEFAULT 0,
    oportunidades_geradas INT NOT NULL DEFAULT 0,
    vendas_geradas INT NOT NULL DEFAULT 0,
    receita_gerada DECIMAL(15,2) NOT NULL DEFAULT 0.00,
    publico_alvo VARCHAR(1000) NULL,
    conteudo LONGTEXT NULL,
    created_at DATETIME NOT NULL,
    created_by_user_id CHAR(36) NULL,
    updated_at DATETIME NULL,
    updated_by_user_id CHAR(36) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME NULL,
    PRIMARY KEY (id),
    UNIQUE KEY ux_crm_campanhas_tenant_nome (tenant_id, nome),
    KEY ix_crm_campanhas_tenant_deleted (tenant_id, is_deleted),
    KEY ix_crm_campanhas_segmento (segmento_id),
    CONSTRAINT fk_crm_campanhas_segmento FOREIGN KEY (segmento_id) REFERENCES crm_segmentos (id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
