-- SSOT central do Nexum Altivon
-- Banco autoridade: 192.168.1.72:3309 / nexum_altivon
-- Script idempotente para saneamento de lojas, catalogo e estoque

USE nexum_altivon;

SET @script_nome = '2026-06-16-ssot-unificar-lojas-estoque';

CREATE TABLE IF NOT EXISTS deploy_auditoria_inconsistencias (
    id BIGINT NOT NULL AUTO_INCREMENT,
    script_nome VARCHAR(120) NOT NULL,
    tipo VARCHAR(60) NOT NULL,
    referencia VARCHAR(120) NULL,
    descricao TEXT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    KEY idx_deploy_auditoria_script (script_nome),
    KEY idx_deploy_auditoria_tipo (tipo)
);

CREATE TABLE IF NOT EXISTS ssot_lojas_config (
    id INT NOT NULL AUTO_INCREMENT,
    codigo_loja VARCHAR(20) NOT NULL,
    slug VARCHAR(50) NOT NULL,
    nome VARCHAR(120) NOT NULL,
    segmento VARCHAR(100) NOT NULL DEFAULT 'ecommerce',
    dominio VARCHAR(150) NULL,
    ordem_exibicao INT NOT NULL DEFAULT 0,
    ativa TINYINT(1) NOT NULL DEFAULT 1,
    updated_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    PRIMARY KEY (id),
    UNIQUE KEY ux_ssot_lojas_config_codigo (codigo_loja),
    UNIQUE KEY ux_ssot_lojas_config_slug (slug)
);

INSERT INTO ssot_lojas_config (codigo_loja, slug, nome, segmento, dominio, ordem_exibicao, ativa)
SELECT
    CONCAT('Lj-', LPAD(l.id, 3, '0')) AS codigo_loja,
    l.slug,
    l.nome,
    COALESCE(NULLIF(TRIM(l.segmento), ''), 'ecommerce') AS segmento,
    l.dominio,
    COALESCE(l.ordem_exibicao, 0) AS ordem_exibicao,
    IFNULL(l.ativa, 1) AS ativa
FROM lojas l
ON DUPLICATE KEY UPDATE
    nome = VALUES(nome),
    segmento = VALUES(segmento),
    dominio = VALUES(dominio),
    ordem_exibicao = VALUES(ordem_exibicao),
    ativa = VALUES(ativa),
    updated_at = CURRENT_TIMESTAMP;

CREATE TABLE IF NOT EXISTS ssot_origem_prefixos (
    tipo_origem VARCHAR(30) NOT NULL,
    prefixo VARCHAR(3) NOT NULL,
    descricao VARCHAR(120) NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    PRIMARY KEY (tipo_origem),
    UNIQUE KEY ux_ssot_origem_prefixos_prefixo (prefixo)
);

INSERT IGNORE INTO ssot_origem_prefixos (tipo_origem, prefixo, descricao) VALUES
    ('FILIAL', 'Lj', 'Filiais oficiais do grupo'),
    ('PARCEIRO', 'Pr', 'Parceiros comerciais'),
    ('DROPSHIPPING', 'Ds', 'Operadores de dropshipping'),
    ('FORNECEDOR', 'Fo', 'Fornecedores homologados'),
    ('CLIENTE', 'Cl', 'Clientes cadastrados'),
    ('FUNCIONARIO', 'Fu', 'Funcionarios do grupo'),
    ('ECOMMERCE', 'Ec', 'Canais de ecommerce');

INSERT INTO deploy_auditoria_inconsistencias (script_nome, tipo, referencia, descricao)
SELECT
    @script_nome,
    'PRODUTO_INCOMPLETO',
    CONCAT('produto:', p.id),
    CONCAT(
        'Produto desativado do catalogo por cadastro incompleto. slug=', COALESCE(p.slug, 'NULL'),
        ', sku=', COALESCE(p.sku, 'NULL')
    )
FROM produtos p
LEFT JOIN lojas l
    ON l.id = p.loja_id
LEFT JOIN categorias c
    ON c.id = p.categoria_id
WHERE p.ativo = 1
  AND (
      l.id IS NULL
      OR IFNULL(l.ativa, 0) = 0
      OR p.categoria_id IS NULL
      OR c.id IS NULL
      OR TRIM(COALESCE(p.nome, '')) = ''
      OR TRIM(COALESCE(p.slug, '')) = ''
      OR TRIM(COALESCE(p.sku, '')) = ''
      OR TRIM(COALESCE(p.imagem_principal, '')) = ''
      OR (
          TRIM(COALESCE(p.descricao_curta, '')) = ''
          AND TRIM(COALESCE(p.descricao_longa, '')) = ''
      )
      OR IFNULL(p.preco, 0) <= 0
      OR IFNULL(p.peso, 0) <= 0
      OR IFNULL(p.altura, 0) <= 0
      OR IFNULL(p.largura, 0) <= 0
      OR IFNULL(p.comprimento, 0) <= 0
  );

UPDATE produtos p
LEFT JOIN lojas l
    ON l.id = p.loja_id
LEFT JOIN categorias c
    ON c.id = p.categoria_id
SET
    p.ativo = 0,
    p.destaque = 0,
    p.updated_at = CURRENT_TIMESTAMP
WHERE p.ativo = 1
  AND (
      l.id IS NULL
      OR IFNULL(l.ativa, 0) = 0
      OR p.categoria_id IS NULL
      OR c.id IS NULL
      OR TRIM(COALESCE(p.nome, '')) = ''
      OR TRIM(COALESCE(p.slug, '')) = ''
      OR TRIM(COALESCE(p.sku, '')) = ''
      OR TRIM(COALESCE(p.imagem_principal, '')) = ''
      OR (
          TRIM(COALESCE(p.descricao_curta, '')) = ''
          AND TRIM(COALESCE(p.descricao_longa, '')) = ''
      )
      OR IFNULL(p.preco, 0) <= 0
      OR IFNULL(p.peso, 0) <= 0
      OR IFNULL(p.altura, 0) <= 0
      OR IFNULL(p.largura, 0) <= 0
      OR IFNULL(p.comprimento, 0) <= 0
  );

INSERT INTO deploy_auditoria_inconsistencias (script_nome, tipo, referencia, descricao)
SELECT
    @script_nome,
    'ESTOQUE_INVALIDO',
    CONCAT('produto:', p.id),
    CONCAT(
        'Estoque normalizado. estoque_atual=', IFNULL(p.estoque_atual, 0),
        ', estoque_reservado=', IFNULL(p.estoque_reservado, 0)
    )
FROM produtos p
WHERE IFNULL(p.estoque_atual, 0) < 0
   OR IFNULL(p.estoque_reservado, 0) < 0
   OR IFNULL(p.estoque_reservado, 0) > IFNULL(p.estoque_atual, 0);

UPDATE produtos
SET
    estoque_atual = GREATEST(IFNULL(estoque_atual, 0), 0),
    estoque_reservado = GREATEST(IFNULL(estoque_reservado, 0), 0),
    updated_at = CURRENT_TIMESTAMP
WHERE IFNULL(estoque_atual, 0) < 0
   OR IFNULL(estoque_reservado, 0) < 0;

UPDATE produtos
SET
    estoque_reservado = LEAST(IFNULL(estoque_reservado, 0), IFNULL(estoque_atual, 0)),
    updated_at = CURRENT_TIMESTAMP
WHERE IFNULL(estoque_reservado, 0) > IFNULL(estoque_atual, 0);

CREATE OR REPLACE VIEW vw_catalogo_publicavel_ssot AS
SELECT
    p.id,
    p.loja_id,
    l.nome AS loja_nome,
    l.slug AS loja_slug,
    p.categoria_id,
    c.nome AS categoria_nome,
    c.slug AS categoria_slug,
    p.sku,
    p.slug,
    p.nome,
    p.preco,
    p.preco_promocional,
    p.custo,
    p.peso,
    p.altura,
    p.largura,
    p.comprimento,
    p.imagem_principal,
    p.estoque_atual,
    p.estoque_reservado,
    GREATEST(IFNULL(p.estoque_atual, 0) - IFNULL(p.estoque_reservado, 0), 0) AS estoque_disponivel,
    p.tipo_produto,
    p.fornecedor_id,
    p.marca,
    p.ativo,
    p.updated_at
FROM produtos p
INNER JOIN lojas l
    ON l.id = p.loja_id
   AND l.ativa = 1
INNER JOIN categorias c
    ON c.id = p.categoria_id
   AND c.ativa = 1
WHERE p.ativo = 1
  AND TRIM(COALESCE(p.nome, '')) <> ''
  AND TRIM(COALESCE(p.slug, '')) <> ''
  AND TRIM(COALESCE(p.sku, '')) <> ''
  AND TRIM(COALESCE(p.imagem_principal, '')) <> ''
  AND (
      TRIM(COALESCE(p.descricao_curta, '')) <> ''
      OR TRIM(COALESCE(p.descricao_longa, '')) <> ''
  )
  AND IFNULL(p.preco, 0) > 0
  AND IFNULL(p.peso, 0) > 0
  AND IFNULL(p.altura, 0) > 0
  AND IFNULL(p.largura, 0) > 0
  AND IFNULL(p.comprimento, 0) > 0;

CREATE OR REPLACE VIEW vw_estoque_consolidado_ssot AS
SELECT
    l.id AS loja_id,
    CONCAT('Lj-', LPAD(l.id, 3, '0')) AS codigo_loja,
    l.nome AS loja_nome,
    l.slug AS loja_slug,
    COUNT(p.id) AS total_produtos,
    SUM(CASE WHEN p.ativo = 1 THEN 1 ELSE 0 END) AS produtos_ativos,
    SUM(IFNULL(p.estoque_atual, 0)) AS estoque_atual_total,
    SUM(IFNULL(p.estoque_reservado, 0)) AS estoque_reservado_total,
    SUM(GREATEST(IFNULL(p.estoque_atual, 0) - IFNULL(p.estoque_reservado, 0), 0)) AS estoque_disponivel_total
FROM lojas l
LEFT JOIN produtos p
    ON p.loja_id = l.id
GROUP BY l.id, l.nome, l.slug;

CREATE OR REPLACE VIEW vw_emitentes_fiscais_ativos AS
SELECT
    eg.id,
    eg.codigo_empresa,
    eg.razao_social,
    eg.cnpj,
    eg.estado,
    eg.regime_tributario,
    eg.categoria_fiscal,
    eg.subcategoria_fiscal,
    eg.permite_nfe_saida,
    eg.permite_dropshipping,
    eg.permite_marketplace,
    eg.emitente_preferencial,
    eg.prioridade_fiscal,
    IFNULL(eg.carga_tributaria_percentual, 0) AS carga_tributaria_percentual,
    IFNULL(eg.custo_operacional_percentual, 0) AS custo_operacional_percentual,
    IFNULL(eg.margem_minima_percentual, 0) AS margem_minima_percentual,
    (IFNULL(eg.carga_tributaria_percentual, 0) + IFNULL(eg.custo_operacional_percentual, 0)) AS custo_total_percentual
FROM erp_empresas_grupo eg
WHERE eg.ativa = 1
  AND eg.permite_nfe_saida = 1
ORDER BY custo_total_percentual ASC, eg.prioridade_fiscal ASC, eg.emitente_preferencial DESC;
