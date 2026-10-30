-- Arvore inicial de categorias das seis lojas (3 grupos e 3 subcategorias por grupo).
-- Requer a carga previa das lojas. Categorias ficam prontas para receber os produtos.
CREATE TEMPORARY TABLE IF NOT EXISTS _nexum_seed_categorias_20261030 (
    loja_slug VARCHAR(50) COLLATE utf8mb4_unicode_ci NOT NULL,
    nome VARCHAR(100) NOT NULL,
    slug VARCHAR(100) COLLATE utf8mb4_general_ci NOT NULL,
    descricao VARCHAR(500) NOT NULL,
    parent_slug VARCHAR(100) COLLATE utf8mb4_general_ci NULL,
    ordem INT NOT NULL,
    PRIMARY KEY (loja_slug, slug)
) ENGINE=MEMORY DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;
TRUNCATE TABLE _nexum_seed_categorias_20261030;
START TRANSACTION;

INSERT INTO _nexum_seed_categorias_20261030 VALUES
('grann-tur','Bagagem e mochilas','bagagem-mochilas','Malas e mochilas para deslocamentos curtos, longos e diferentes modalidades de viagem.',NULL,1),
('grann-tur','Malas de viagem','malas-viagem','Malas para organizar roupas e pertences em viagens nacionais e internacionais.','bagagem-mochilas',1),
('grann-tur','Malas de bordo','malas-bordo','Bagagens compactas para transporte em cabine, conforme limites da companhia escolhida.','bagagem-mochilas',2),
('grann-tur','Mochilas de viagem','mochilas-viagem','Mochilas para transporte organizado de itens pessoais durante deslocamentos.','bagagem-mochilas',3),
('grann-tur','Organizacao de viagem','organizacao-viagem','Acessorios compactos que ajudam a separar, proteger e acondicionar itens pessoais.',NULL,2),
('grann-tur','Necessaires','necessaires','Estojos para higiene pessoal, cuidados e pequenos objetos de uso diario.','organizacao-viagem',1),
('grann-tur','Organizadores de mala','organizadores-mala','Organizadores para separar roupas, calcados e acessorios na bagagem.','organizacao-viagem',2),
('grann-tur','Frascos para viagem','frascos-viagem','Recipientes compactos para transportar produtos de higiene em deslocamentos.','organizacao-viagem',3),
('grann-tur','Acessorios de viagem','acessorios-viagem','Itens de apoio, identificacao e conforto para o viajante.',NULL,3),
('grann-tur','Travesseiros de viagem','travesseiros-viagem','Apoios de pescoco e descanso para trajetos terrestres e aereos.','acessorios-viagem',1),
('grann-tur','Cadeados para bagagem','cadeados-bagagem','Dispositivos de fechamento para auxiliar na protecao da bagagem.','acessorios-viagem',2),
('grann-tur','Etiquetas de identificacao','etiquetas-identificacao','Etiquetas para identificar malas e facilitar sua localizacao.','acessorios-viagem',3),

('chronos','Relogios analogicos','relogios-analogicos','Relogios de ponteiro com diferentes mecanismos, formatos e estilos.',NULL,1),
('chronos','Relogios classicos','relogios-classicos','Modelos analogicos de leitura tradicional para uso cotidiano e formal.','relogios-analogicos',1),
('chronos','Relogios esportivos','relogios-esportivos','Relogios analogicos com recursos e materiais orientados a atividades e uso casual.','relogios-analogicos',2),
('chronos','Relogios minimalistas','relogios-minimalistas','Modelos analogicos de mostrador limpo e leitura direta.','relogios-analogicos',3),
('chronos','Smartwatches','smartwatches','Relogios conectados com recursos digitais sujeitos a compatibilidade do fabricante.',NULL,2),
('chronos','Smartwatches Android','smartwatches-android','Relogios inteligentes com compatibilidade Android declarada pelo fabricante.','smartwatches',1),
('chronos','Relogios fitness','relogios-fitness','Dispositivos de pulso com funcoes de acompanhamento de atividade e bem-estar.','smartwatches',2),
('chronos','Pulseiras inteligentes','pulseiras-inteligentes','Dispositivos compactos de pulso para notificacoes e acompanhamento de atividade.','smartwatches',3),
('chronos','Acessorios de relojoaria','acessorios-relojoaria','Acessorios de reposicao, acondicionamento e cuidado de relogios.',NULL,3),
('chronos','Pulseiras para relogio','pulseiras-relogio','Pulseiras de reposicao em materiais e larguras compativeis com cada modelo.','acessorios-relojoaria',1),
('chronos','Estojos para relogio','estojos-relogio','Estojos para guardar ou transportar relogios e acessorios.','acessorios-relojoaria',2),
('chronos','Cuidados e limpeza','cuidados-limpeza','Itens para limpeza e conservacao conforme os materiais do relogio.','acessorios-relojoaria',3),

('moda-mim','Vestuário','vestuario','Pecas de vestuario com tamanhos, materiais e instrucoes especificas por produto.',NULL,1),
('moda-mim','Blusas e camisetas','blusas-camisetas','Pecas superiores casuais e sociais com composicao e medidas informadas por item.','vestuario',1),
('moda-mim','Calcas e saias','calcas-saias','Pecas inferiores com modelagens e tabelas de medida por produto.','vestuario',2),
('moda-mim','Vestidos','vestidos','Pecas inteiricas para diferentes ocasioes, com tamanhos e materiais especificados.','vestuario',3),
('moda-mim','Calcados','calcados','Calcados com numeracao, materiais e orientacoes de ajuste por modelo.',NULL,2),
('moda-mim','Tenis casuais','tenis-casuais','Calcados de uso diario com grade de numeracao especificada por produto.','calcados',1),
('moda-mim','Sandalias','sandalias','Calcados abertos com materiais, numeracao e detalhes de uso informados por modelo.','calcados',2),
('moda-mim','Calcados sociais','calcados-sociais','Calcados para composicoes sociais, com materiais e numeracao por modelo.','calcados',3),
('moda-mim','Bolsas e acessorios','bolsas-acessorios','Acessorios de moda com dimensoes e materiais informados em cada produto.',NULL,3),
('moda-mim','Bolsas','bolsas','Bolsas de diferentes formatos e capacidades descritas por modelo.','bolsas-acessorios',1),
('moda-mim','Cintos','cintos','Cintos com materiais, comprimentos e orientacoes de ajuste por produto.','bolsas-acessorios',2),
('moda-mim','Bijuterias e adornos','bijuterias-adornos','Acessorios pessoais com materiais e cuidados descritos por item.','bolsas-acessorios',3),

('geracao-top','Informatica e perifericos','informatica-perifericos','Equipamentos e perifericos para computadores, estudo e trabalho.',NULL,1),
('geracao-top','Teclados','teclados','Teclados com layout, conexao e compatibilidade informados por modelo.','informatica-perifericos',1),
('geracao-top','Mouses','mouses','Mouses com tipo de sensor, conexao e compatibilidade informados por modelo.','informatica-perifericos',2),
('geracao-top','Armazenamento digital','armazenamento-digital','Dispositivos de armazenamento com capacidade e interface especificadas.','informatica-perifericos',3),
('geracao-top','Audio e comunicacao','audio-comunicacao','Acessorios de audio e conectividade com interfaces e compatibilidade por modelo.',NULL,2),
('geracao-top','Fones de ouvido','fones-ouvido','Fones com tipo de conexao, microfone e compatibilidade descritos por modelo.','audio-comunicacao',1),
('geracao-top','Caixas de som','caixas-som','Caixas de som com potencia, autonomia e conectividade informadas pelo fabricante.','audio-comunicacao',2),
('geracao-top','Cabos e adaptadores','cabos-adaptadores','Cabos e adaptadores com padrao de conexao e compatibilidade identificados.','audio-comunicacao',3),
('geracao-top','Energia e casa conectada','energia-casa-conectada','Acessorios de alimentacao e automacao com tensao e compatibilidade especificadas.',NULL,3),
('geracao-top','Carregadores','carregadores','Carregadores com potencia, padrao de conexao e tensao especificados.','energia-casa-conectada',1),
('geracao-top','Baterias portateis','baterias-portateis','Baterias externas com capacidade, portas e potencia informadas pelo fabricante.','energia-casa-conectada',2),
('geracao-top','Automacao residencial','automacao-residencial','Dispositivos conectados com requisitos de rede, tensao e compatibilidade descritos.','energia-casa-conectada',3),

('estruturaline','Ferramentas eletricas','ferramentas-eletricas','Ferramentas motorizadas com tensao, potencia e aplicacao especificadas.',NULL,1),
('estruturaline','Furadeiras e parafusadeiras','furadeiras-parafusadeiras','Equipamentos de perfuracao e fixacao com tensao e capacidade declaradas.','ferramentas-eletricas',1),
('estruturaline','Serras','serras','Serras para materiais e aplicacoes indicadas pelo fabricante.','ferramentas-eletricas',2),
('estruturaline','Lixadeiras','lixadeiras','Equipamentos de acabamento com potencia e consumiveis compativeis descritos.','ferramentas-eletricas',3),
('estruturaline','Ferramentas manuais','ferramentas-manuais','Ferramentas de uso manual com material, dimensoes e aplicacao indicados.',NULL,2),
('estruturaline','Chaves e bits','chaves-bits','Chaves e ponteiras identificadas por tipo, medida e aplicacao.','ferramentas-manuais',1),
('estruturaline','Alicates','alicates','Alicates para corte, aperto ou instalacao conforme modelo.','ferramentas-manuais',2),
('estruturaline','Medicao e nivelamento','medicao-nivelamento','Instrumentos de medicao e nivelamento com precisao e escala especificadas.','ferramentas-manuais',3),
('estruturaline','Seguranca e materiais','seguranca-materiais','Equipamentos de protecao e itens de instalacao com especificacao tecnica.',NULL,3),
('estruturaline','Equipamentos de protecao','equipamentos-protecao','Itens de protecao com classe, tamanho e certificacao informados por produto.','seguranca-materiais',1),
('estruturaline','Fixadores e ancoragens','fixadores-ancoragens','Fixadores com material, medida e substrato de aplicacao indicados.','seguranca-materiais',2),
('estruturaline','Iluminacao de obra','iluminacao-obra','Iluminacao para manutencao e obra com potencia e tensao declaradas.','seguranca-materiais',3),

('gran-fest','Decoracao de eventos','decoracao-eventos','Itens decorativos para composicao visual de festas e eventos.',NULL,1),
('gran-fest','Baloes e arcos','baloes-arcos','Baloes e acessorios de montagem com dimensoes e materiais informados.','decoracao-eventos',1),
('gran-fest','Paineis e fundos','paineis-fundos','Paineis e fundos decorativos com dimensoes e metodo de fixacao especificados.','decoracao-eventos',2),
('gran-fest','Iluminacao decorativa','iluminacao-decorativa','Iluminacao decorativa com tensao, conexao e uso interno ou externo informados.','decoracao-eventos',3),
('gran-fest','Mesa e descartaveis','mesa-descartaveis','Itens de mesa e consumo com material, dimensoes e quantidades por embalagem.',NULL,2),
('gran-fest','Pratos e bandejas','pratos-bandejas','Pratos e bandejas com material, dimensoes e quantidade por pacote.','mesa-descartaveis',1),
('gran-fest','Copos e canudos','copos-canudos','Copos e canudos com capacidade, material e quantidade por embalagem.','mesa-descartaveis',2),
('gran-fest','Talheres e guardanapos','talheres-guardanapos','Itens de mesa com material e quantidade declarados por embalagem.','mesa-descartaveis',3),
('gran-fest','Kits e lembrancas','kits-lembrancas','Conjuntos e embalagens para lembrancas, identificados por conteudo e quantidade.',NULL,3),
('gran-fest','Kits tematicos','kits-tematicos','Conjuntos de decoracao com itens inclusos descritos individualmente.','kits-lembrancas',1),
('gran-fest','Lembrancinhas','lembrancinhas','Pequenos itens para convidados, com material e quantidade especificados.','kits-lembrancas',2),
('gran-fest','Embalagens para festa','embalagens-festa','Embalagens para doces e lembrancas com dimensoes e quantidades por pacote.','kits-lembrancas',3);

INSERT INTO categorias
    (loja_id,nome,slug,descricao,imagem,categoria_pai_id,ordem,ativa,created_at,updated_at)
SELECT l.id,s.nome,s.slug,s.descricao,NULL,NULL,s.ordem,1,UTC_TIMESTAMP(6),UTC_TIMESTAMP(6)
FROM _nexum_seed_categorias_20261030 s
JOIN lojas l ON l.slug=s.loja_slug
WHERE s.parent_slug IS NULL
  AND NOT EXISTS (SELECT 1 FROM categorias c WHERE c.loja_id=l.id AND c.slug=s.slug);

INSERT INTO categorias
    (loja_id,nome,slug,descricao,imagem,categoria_pai_id,ordem,ativa,created_at,updated_at)
SELECT l.id,s.nome,s.slug,s.descricao,NULL,p.id,s.ordem,1,UTC_TIMESTAMP(6),UTC_TIMESTAMP(6)
FROM _nexum_seed_categorias_20261030 s
JOIN lojas l ON l.slug=s.loja_slug
JOIN categorias p ON p.loja_id=l.id AND p.slug=s.parent_slug
WHERE s.parent_slug IS NOT NULL
  AND NOT EXISTS (SELECT 1 FROM categorias c WHERE c.loja_id=l.id AND c.slug=s.slug);

UPDATE categorias c
JOIN lojas l ON l.id=c.loja_id
JOIN _nexum_seed_categorias_20261030 s ON s.loja_slug=l.slug AND s.slug=c.slug
SET c.created_at=IF(c.created_at='0000-00-00 00:00:00',UTC_TIMESTAMP(6),c.created_at),
    c.updated_at=IF(c.updated_at='0000-00-00 00:00:00',UTC_TIMESTAMP(6),c.updated_at);

COMMIT;
DROP TEMPORARY TABLE _nexum_seed_categorias_20261030;

SELECT l.nome AS loja, COUNT(c.id) AS categorias
FROM lojas l
LEFT JOIN categorias c ON c.loja_id=l.id
WHERE l.slug IN ('grann-tur','chronos','moda-mim','geracao-top','estruturaline','gran-fest')
GROUP BY l.id,l.nome
ORDER BY l.ordem_exibicao;
