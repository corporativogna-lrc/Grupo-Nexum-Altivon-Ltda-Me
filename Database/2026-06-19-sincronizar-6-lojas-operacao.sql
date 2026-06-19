-- Nexum Altivon - sincronizacao das 6 lojas oficiais
-- Autoridade unica: MySQL/MariaDB em 192.168.1.72:3309 / database nexum_altivon
-- Regra de seguranca: nao altera a loja id=1 porque o banco de producao ja possui
-- produto vinculado a Chronos neste id.

START TRANSACTION;

INSERT INTO lojas (
    id, nome, slug, segmento, descricao, cor_primaria, cor_secundaria, dominio, ativa, ordem_exibicao
) VALUES
    (1, 'Chronos', 'chronos', 'Relogios & Acessorios', 'Relogios que marcam estilo', '#C9A227', '#2E5A8F', 'chronos.nexumaltivon.com', 1, 1),
    (2, 'Grann-Tur', 'grann-tur', 'Viagens & Turismo', 'Mochilas, malas, acessorios de viagem', '#C9A227', '#1E3A5F', 'grann-tur.nexumaltivon.com', 1, 2),
    (3, 'Moda Mim', 'moda-mim', 'Moda & Vestuario', 'Tendencias que vestem a sua personalidade', '#C9A227', '#8B1E3F', 'moda-mim.nexumaltivon.com', 1, 3),
    (4, 'Geracao Top+', 'geracao-top', 'Tecnologia & Gadgets', 'Tecnologia de ponta ao alcance de todos', '#C9A227', '#0F4C3A', 'geracao-top.nexumaltivon.com', 1, 4),
    (5, 'Estruturaline', 'estruturaline', 'Construcao & Estruturas', 'Ferramentas e materiais de construcao', '#C9A227', '#4A3728', 'estruturaline.nexumaltivon.com', 1, 5),
    (6, 'Gran-fest-festas', 'gran-fest', 'Festas & Eventos', 'Decoracoes e utensilios para festas', '#C9A227', '#6B2D5C', 'gran-fest.nexumaltivon.com', 1, 6)
ON DUPLICATE KEY UPDATE
    nome = VALUES(nome),
    segmento = VALUES(segmento),
    descricao = VALUES(descricao),
    cor_primaria = VALUES(cor_primaria),
    cor_secundaria = VALUES(cor_secundaria),
    dominio = VALUES(dominio),
    ativa = VALUES(ativa),
    ordem_exibicao = VALUES(ordem_exibicao),
    updated_at = CURRENT_TIMESTAMP;

CREATE OR REPLACE VIEW vw_lojas_operacionais AS
SELECT
    l.id,
    l.nome,
    l.slug,
    l.segmento,
    l.dominio,
    l.ativa,
    COUNT(p.id) AS produtos_total,
    COALESCE(SUM(GREATEST(p.estoque_atual - p.estoque_reservado, 0)), 0) AS estoque_disponivel
FROM lojas l
LEFT JOIN produtos p ON p.loja_id = l.id AND p.ativo = 1
GROUP BY l.id, l.nome, l.slug, l.segmento, l.dominio, l.ativa;

COMMIT;
