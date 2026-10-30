# Catalogo e lojas de homologacao

Em 2026-10-30, foram carregados dados de visualizacao e homologacao no MySQL local do projeto (`127.0.0.1:3309`, schema `nexum_altivon`). O banco efetivamente servido pela API local e pelo Cloudflare Tunnel foi confirmado por leitura posterior das rotas locais e publicas.

## Conteudo carregado

- 6 lojas ativas: Grann-Tur, Chronos, Moda Mim, Geração Top+, Estruturaline e Gran-Fest Festas.
- 6 perfis publicos associados aos registros das lojas, sem dados fiscais, dominios, logotipos, banners ou contatos que nao foram fornecidos.
- 72 categorias: 3 categorias principais e 9 subcategorias por loja.
- 100 produtos (SKUs `HOM-*`), distribuidos pelas seis lojas e categorias correspondentes.
- Estoque inicial de homologacao: 5 unidades por produto.
- Cada produto aponta para seu SVG ilustrativo 1000×1000 em `public/imagens/catalogo/`. As imagens sao simbolicas e nao fotografias comerciais.
- Opcoes de variacao estruturadas em `tags` para testar seletores no fluxo web.
- Os precos registrados sao referencias de homologacao e nao foram aprovados como valores comerciais.

## Evidencia de leitura

As rotas publicas `/api/site/lojas` e `/api/produtos` retornaram HTTP 200. A primeira lista 6 lojas. A segunda informa 100 produtos e 2 paginas no limite da API (60 na pagina 1, 40 na pagina 2). A rota `/api/site/lojas/grann-tur` tambem respondeu HTTP 200.

## Arquivos e seguranca da carga

- Seeds idempotentes em `NexumAltivon_Back-End/Database/seeds/20261030_*.sql`.
- Gerador dos 100 SVGs e da seed de produtos em `scripts/catalogo/gerar_catalogo_homologacao.py`.
- Backup das tabelas de lojas, categorias e produtos: `D:\xampp\mysql\backup\nexum_catalogo_pre_import_20261030.sql`.
- Backup dos perfis publicos: `D:\xampp\mysql\backup\nexum_site_perfis_publicos_pre_import_20261030.sql`.
- Logins e cadastros legais de empresas nao foram alterados.

## Publicacao e aceite pendentes

Os SVGs ainda devem ser publicados no GitHub Pages e conferidos por HTTP e no navegador. A importacao dos registros nao valida login, checkout, pagamento ou venda no PDV. Esses fluxos seguem pendentes de homologacao independente.
