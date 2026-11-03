-- Publica no site as seis lojas cadastradas, sem domínios ou imagens não fornecidos.
-- Idempotente: preserva perfis existentes por origem ou slug.
START TRANSACTION;

INSERT INTO site_perfis_publicos
    (id,tenant_id,row_version,created_at,updated_at,is_deleted,tipo_perfil,origem_tipo,origem_id,
     nome,slug,segmento,atividade,descricao,logo_url,banner_url,icone,cta_texto,cta_url,site_url,
     email_publico,telefone_publico,endereco_publico,cor_primaria,cor_secundaria,cor_fundo,cor_texto,
     publicado,ordem_exibicao)
SELECT UUID(),l.tenant_id,UNHEX(SHA2(CONCAT(UUID(),':',l.id),256)),UTC_TIMESTAMP(6),UTC_TIMESTAMP(6),0,
       'Loja','Loja',l.id,l.nome,l.slug,l.segmento,l.segmento,l.descricao,NULL,NULL,
       CASE l.slug
           WHEN 'grann-tur' THEN 'Plane'
           WHEN 'chronos' THEN 'Watch'
           WHEN 'moda-mim' THEN 'Shirt'
           WHEN 'geracao-top' THEN 'Smartphone'
           WHEN 'estruturaline' THEN 'Hammer'
           WHEN 'gran-fest' THEN 'Gift'
           ELSE 'Store'
       END,
       'Conhecer loja',CONCAT('/lojas/',l.slug),NULL,NULL,NULL,NULL,
       l.cor_primaria,l.cor_secundaria,NULL,NULL,1,l.ordem_exibicao
FROM lojas l
WHERE l.ativa=1
  AND l.slug IN ('grann-tur','chronos','moda-mim','geracao-top','estruturaline','gran-fest')
  AND NOT EXISTS (
      SELECT 1 FROM site_perfis_publicos p
      WHERE p.tenant_id=l.tenant_id
        AND ( (p.origem_tipo='Loja' AND p.origem_id=l.id) OR p.slug=l.slug )
  );

COMMIT;

SELECT origem_id AS loja_id,nome,slug,publicado,ordem_exibicao
FROM site_perfis_publicos
WHERE tipo_perfil='Loja'
  AND slug IN ('grann-tur','chronos','moda-mim','geracao-top','estruturaline','gran-fest')
ORDER BY ordem_exibicao;
