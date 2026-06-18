-- Nexum Altivon - unificacao operacional das 6 lojas e estoque
-- Autoridade unica: MySQL/MariaDB em 192.168.1.72:3309 / database nexum_altivon
-- Execucao sugerida:
--   mysql -h 192.168.1.72 -P 3309 -u nexum_app -p nexum_altivon < Database/2026-06-18-unificar-lojas-estoque.sql

START TRANSACTION;

CREATE TABLE IF NOT EXISTS auditoria_estoque_unificado (
    id BIGINT NOT NULL AUTO_INCREMENT,
    executado_em DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    sku VARCHAR(50) NOT NULL,
    produto_ids TEXT NOT NULL,
    lojas_ids TEXT NOT NULL,
    estoque_total INT NOT NULL,
    estoque_reservado_total INT NOT NULL,
    produto_canonico_id INT NOT NULL,
    observacao VARCHAR(255) NOT NULL,
    PRIMARY KEY (id),
    KEY ix_auditoria_estoque_unificado_sku (sku)
);

CREATE TEMPORARY TABLE tmp_produto_estoque_unificado AS
SELECT
    sku,
    MIN(id) AS produto_canonico_id,
    GROUP_CONCAT(id ORDER BY id) AS produto_ids,
    GROUP_CONCAT(DISTINCT loja_id ORDER BY loja_id) AS lojas_ids,
    SUM(GREATEST(estoque_atual, 0)) AS estoque_total,
    SUM(GREATEST(estoque_reservado, 0)) AS estoque_reservado_total,
    MIN(estoque_minimo) AS estoque_minimo_grupo,
    MAX(updated_at) AS ultima_atualizacao
FROM produtos
WHERE ativo = 1
  AND sku IS NOT NULL
  AND TRIM(sku) <> ''
GROUP BY sku
HAVING COUNT(*) > 1;

INSERT INTO auditoria_estoque_unificado (
    sku,
    produto_ids,
    lojas_ids,
    estoque_total,
    estoque_reservado_total,
    produto_canonico_id,
    observacao
)
SELECT
    sku,
    produto_ids,
    lojas_ids,
    estoque_total,
    estoque_reservado_total,
    produto_canonico_id,
    'Unificacao de estoque por SKU entre lojas ativas'
FROM tmp_produto_estoque_unificado;

UPDATE produtos p
JOIN tmp_produto_estoque_unificado u ON u.produto_canonico_id = p.id
SET
    p.estoque_atual = u.estoque_total,
    p.estoque_reservado = LEAST(u.estoque_reservado_total, u.estoque_total),
    p.estoque_minimo = u.estoque_minimo_grupo,
    p.updated_at = CURRENT_TIMESTAMP;

UPDATE produtos p
JOIN tmp_produto_estoque_unificado u ON FIND_IN_SET(p.id, u.produto_ids) > 0
SET
    p.estoque_atual = 0,
    p.estoque_reservado = 0,
    p.ativo = 0,
    p.tags = CONCAT_WS(',', NULLIF(p.tags, ''), 'duplicado_estoque_unificado'),
    p.updated_at = CURRENT_TIMESTAMP
WHERE p.id <> u.produto_canonico_id;

UPDATE pedido_itens pi
JOIN tmp_produto_estoque_unificado u ON FIND_IN_SET(pi.produto_id, u.produto_ids) > 0
SET pi.produto_id = u.produto_canonico_id
WHERE pi.produto_id <> u.produto_canonico_id;

CREATE OR REPLACE VIEW vw_estoque_autoridade AS
SELECT
    p.id AS produto_id,
    p.loja_id,
    l.nome AS loja_nome,
    p.sku,
    p.nome,
    p.estoque_atual,
    p.estoque_reservado,
    GREATEST(p.estoque_atual - p.estoque_reservado, 0) AS estoque_disponivel,
    p.estoque_minimo,
    p.ativo,
    p.updated_at
FROM produtos p
JOIN lojas l ON l.id = p.loja_id
WHERE p.ativo = 1;

COMMIT;
