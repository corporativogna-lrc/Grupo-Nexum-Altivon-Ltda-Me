-- Cadastros-base das seis lojas conforme os segmentos do projeto.
-- Execucao idempotente: preserva qualquer loja ja cadastrada e deixa dominios,
-- marcas, imagens e informacoes fiscais sem preencher ate haver dados validados.
START TRANSACTION;

INSERT INTO lojas
    (nome, slug, segmento, descricao, logo, banner, cor_primaria, cor_secundaria,
     dominio, ativa, ordem_exibicao)
SELECT 'Grann-Tur', 'grann-tur', 'Viagens & Turismo',
       'Mochilas, malas e acessorios de viagem para explorar o mundo com praticidade e estilo.',
       NULL, NULL, '#C9A227', '#1E3A5F', NULL, 1, 1
WHERE NOT EXISTS (SELECT 1 FROM lojas WHERE slug = 'grann-tur');

INSERT INTO lojas
    (nome, slug, segmento, descricao, logo, banner, cor_primaria, cor_secundaria,
     dominio, ativa, ordem_exibicao)
SELECT 'Chronos', 'chronos', 'Relogios & Acessorios',
       'Relogios e acessorios para diferentes estilos, ocasioes e rotinas.',
       NULL, NULL, '#C9A227', '#2E5A8F', NULL, 1, 2
WHERE NOT EXISTS (SELECT 1 FROM lojas WHERE slug = 'chronos');

INSERT INTO lojas
    (nome, slug, segmento, descricao, logo, banner, cor_primaria, cor_secundaria,
     dominio, ativa, ordem_exibicao)
SELECT 'Moda Mim', 'moda-mim', 'Moda & Vestuario',
       'Roupas, calcados e acessorios para compor estilos pessoais em diferentes ocasioes.',
       NULL, NULL, '#C9A227', '#8B1E3F', NULL, 1, 3
WHERE NOT EXISTS (SELECT 1 FROM lojas WHERE slug = 'moda-mim');

INSERT INTO lojas
    (nome, slug, segmento, descricao, logo, banner, cor_primaria, cor_secundaria,
     dominio, ativa, ordem_exibicao)
SELECT 'Geração Top+', 'geracao-top', 'Tecnologia & Gadgets',
       'Tecnologia, eletronicos e acessorios para trabalho, estudo e entretenimento.',
       NULL, NULL, '#C9A227', '#0F4C3A', NULL, 1, 4
WHERE NOT EXISTS (SELECT 1 FROM lojas WHERE slug = 'geracao-top');

INSERT INTO lojas
    (nome, slug, segmento, descricao, logo, banner, cor_primaria, cor_secundaria,
     dominio, ativa, ordem_exibicao)
SELECT 'Estruturaline', 'estruturaline', 'Construcao & Estruturas',
       'Ferramentas, materiais e equipamentos para construcao e manutencao.',
       NULL, NULL, '#C9A227', '#4A3728', NULL, 1, 5
WHERE NOT EXISTS (SELECT 1 FROM lojas WHERE slug = 'estruturaline');

INSERT INTO lojas
    (nome, slug, segmento, descricao, logo, banner, cor_primaria, cor_secundaria,
     dominio, ativa, ordem_exibicao)
SELECT 'Gran-Fest Festas', 'gran-fest', 'Festas & Eventos',
       'Decoracoes e utensilios para organizacao de festas e eventos.',
       NULL, NULL, '#C9A227', '#6B2D5C', NULL, 1, 6
WHERE NOT EXISTS (SELECT 1 FROM lojas WHERE slug = 'gran-fest');

COMMIT;

SELECT id, nome, slug, segmento, ativa, ordem_exibicao
FROM lojas
WHERE slug IN ('grann-tur', 'chronos', 'moda-mim', 'geracao-top', 'estruturaline', 'gran-fest')
ORDER BY ordem_exibicao;
