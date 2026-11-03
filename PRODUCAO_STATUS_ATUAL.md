<!--
 * Propriedade intelectual: Luis Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestao: GenesisGest.Net
 * Ano Inicio: 04/2024 Publicado e operacional: 05/2026
 * Versao do produto: 1.1.5; verificar Directory.Build.props para a revisao vigente
-->

# Producao - status verificado

Ultima verificacao: 2026-10-30 (BRT).

## Site e API

- `https://nexumaltivon.com.br/`: HTTP 200.
- Nova verificacao HTTP em 2026-10-30: `/health` e `/api/site/configuracoes/publico` responderam HTTP 200; ambas as respostas incluem `Access-Control-Allow-Origin: https://nexumaltivon.com.br`. A mensagem da captura de indisponibilidade de API nao foi reproduzida por requisicao HTTP fora do navegador.
- A raiz publica da API ainda identifica a versao `1.1.5.7226`, enquanto o fonte local que recebeu estas correcoes esta em `1.1.5.7243`; as correcoes locais nao estao no servico publico.
- Na mesma verificacao, `/api/lojas`, `/api/categorias` e `/api/produtos?pagina=1&itensPorPagina=1` responderam HTTP 200, porem com colecoes vazias. O site nao pode ser aceito como vitrine/pedidos operacionais enquanto as seeds nao forem importadas e os fluxos autenticados nao forem homologados.
- O host canonico e o apex `nexumaltivon.com.br`. A correcao de CNAME do GitHub Pages foi publicada pelo commit `aeb9fd2ffe4dd295edbf2f86e9b4579ca18d7e82`; workflow 41092984319 terminou com sucesso.
- A API `https://api.nexumaltivon.com.br` e a API local respondem em `/health`, `/health/db`, `/health/db/genesis` e `/api/site/configuracoes/publico`.
- CORS para `https://nexumaltivon.com.br` e preflight de `/api/auth/login` responderam corretamente.
- HTTP direto em `/login` retorna 404 do GitHub Pages; `/?/login` entrega o shell SPA. A rota ainda exige validacao no navegador apos carregar JavaScript.
- Login real ainda nao foi confirmado com credencial autorizada. Nao declarar autenticacao restaurada somente por health/preflight.
- `www.nexumaltivon.com.br` nao deve ser usado como URL canonica; DNS nao responde.

## API, banco e recuperacao

- MySQL oficial: `D:\xampp\mysql\data`, servico `NexumAltivonMySQL`, porta 3309.
- API oficial: `http://127.0.0.1:5010`, exposta pelo Cloudflare Tunnel.
- Tarefa `NexumAltivonApi24h` foi validada elevada em 2026-10-30: SYSTEM, gatilho de boot, reinicio configurado, supervisor oficial e processos ativos; endpoints locais e publicos passaram.
- Dados de login/usuarios e cadastro de empresas sao considerados uteis. Demais registros de negocio sao dados de teste; nao reutilizar como dados de producao.

## Lojas e catalogo

- Estado anterior a importacao: a consulta de 2026-10-30 retornava zero lojas, categorias e produtos. Depois da autorizacao expressa do usuario, a carga controlada foi aplicada no banco oficial XAMPP `nexum_altivon` (MariaDB 10.4.32, porta 3309).
- Estado atual conferido no banco e pela API local/publica: 6 lojas, 72 categorias e 100 produtos. As seis lojas tem perfil publicado em `site_perfis_publicos`; `/api/site/lojas` retorna 6 e `/api/site/lojas/grann-tur` retorna HTTP 200. `/api/produtos` informa total 100, em duas paginas de 60 e 40.
- Backup anterior a carga: `D:\xampp\mysql\backup\nexum_catalogo_pre_import_20261030.sql` (tabelas lojas/categorias/produtos) e `D:\xampp\mysql\backup\nexum_site_perfis_publicos_pre_import_20261030.sql`. Logins e cadastro legal de empresas nao foram alterados.
- As 72 categorias tem 3 grupos principais e 9 subcategorias por loja. Seeds idempotentes ficam em `NexumAltivon_Back-End/Database/seeds/20261030_*.sql`; a seed de categorias foi corrigida para as collations e timestamps reais do banco.
- Os 100 SKUs tem estoque de homologacao de 5 unidades, imagem principal SVG ilustrativa 1000x1000 e opcoes por familia. Os precos sao referencias para homologacao, nao precos comerciais aprovados. Cada SKU tem asset correspondente em `NexumAltivon_Front-End/public/imagens/catalogo/`.
- Perfis das lojas usam nomes, segmentos, descricoes e cores existentes; CNPJ, dominio, logo, banner e contatos permanecem vazios ate serem fornecidos/validados.
- O frontend do catalogo foi ajustado para preservar metadados de paginacao e carregar as 40 unidades restantes sob demanda. Build isolado baseado na `main` oficial compilou com sucesso. A imagem SVG publica ainda responde 404 ate publicar o bundle/asset no GitHub Pages.
- O painel verifica no navegador que imagens enviadas tenham pelo menos 1 megapixel e 720 px no lado menor. A API repete a validacao de dimensoes para PNG/JPEG/WebP/GIF antes de gravar o arquivo, limita cada arquivo a 2 MB e recusa mais de quatro imagens opcionais. Cadastro/edicao exige imagem principal. Os filtros de vitrine existentes continuam excluindo produtos sem imagem, preco ou dimensoes comerciais validas.
- Em 2026-10-30, o frontend local passou a ler opcoes de variacao do JSON de tags do produto, exigir selecao, distinguir variantes no carrinho e enviar a escolha no checkout. A API local valida as opcoes contra as permitidas e persiste a selecao no nome do item do pedido (`pedido_itens.nome_produto`); ainda nao existe coluna normalizada de variante. Essa implementacao ainda nao foi publicada na API publica.
- Validacao apos essas alteracoes: build Release da API passou com 0 avisos/0 erros; `npm run build` compilou e executou `postbuild-pages.js` com sucesso. O build WPF foi repetido e segue falhando com 1.020 erros e 2 avisos, majoritariamente handlers XAML ausentes; isso bloqueia pacote Desktop.
- A tabela ERP `erp_empresas_grupo` e o endpoint atual exigem razao social e CNPJ; sem os dados legais individuais, nao criar empresa fiscal falsa. A preparacao das unidades de loja pode usar nomes fantasia, mantendo regularizacao fiscal pendente mediante suporte explicito a rascunhos no modelo/API.
- Catalogo e lojas foram importados, mas nao houve ainda pedido autenticado nem publicacao visual do bundle/assets. O commit local de publicacao Pages `27ffbd2` esta pronto; o push a `main` nao ocorreu porque o credential helper GitHub configurado aponta para `C:\Program Files\GitHub CLI\gh.exe`, que nao existe no ambiente. O asset ainda retorna HTTP 404.
- O PDV foi conectado a `POST /api/erp/genesis/pdv/vendas` no cliente Desktop; o backend agora exige empresa e operador identificáveis, cadastro ativo de cliente, SKU ativo e preço correspondente, valida estoque, impede repeticao de numero da venda e nao usa resolvers silenciosos para empresa/operador/cliente. O espelhamento Genesis→Nexum usa transacao serializavel e decremento condicional do estoque disponivel, sem consumir reservas de pedidos do site. Se o Genesis gravar e o espelhamento Nexum falhar, o estado fica explicitamente `ERRO_NEXUM` para reconciliacao; nao ha transacao distribuida entre os bancos.
- Esses fluxos foram alterados localmente e a API compilou; permanecem sem homologacao autenticada com cadastros reais. A importacao do catalogo nao declara login, pedido de site ou PDV restaurados.
- Revalidacao operacional desta continuacao (2026-10-30): `/health`, `/health/db`, `/health/db/genesis` e configuracao publica responderam HTTP 200 localmente; os dois health/configuracao publica tambem responderam 200 pela API publica. TCP 3309 e HTTPS 443 responderam; `NexumAltivonMySQL`, `Cloudflared` e `Schedule` estao `Running/Auto`. A verificacao estrita falhou porque a tarefa `NexumAltivonApi24h` nao foi encontrada e o processo da porta 5010 nao pode ser identificado pela sessao sem privilegio administrativo. O listener atual e `dotnet.exe` PID 5356, filho de PowerShell PID 3592; a linha de comando e o proprietario nao estao acessiveis a esta sessao. Isso comprova disponibilidade atual dos endpoints, nao inicializacao apos boot.
- Nesta leitura, as acoes de falha atuais estao aquem da politica desejada: MySQL tenta reiniciar duas vezes e Cloudflared uma vez. Os scripts oficiais de instalacao/reparo foram ajustados para configurar tres reinicios e sinalizar falhas de servico, mas essa politica e a tarefa SYSTEM ainda precisam ser aplicadas por sessao administrativa. Nenhum restart de servico ou reboot foi executado nesta validacao.

## Desktop

- Corrigido `DesktopApiClient.CheckHealthAsync`: consultava `/api/health`, rota que não existe na API ativa; agora consulta `/health` tanto local quanto publicamente. Ambos os endereços responderam HTTP 200 após a correção.
- Build oficial WPF falha e o produto Desktop nao esta pronto para publicacao.
- Diagnostico recompilado em 2026-10-30: build Release WPF falhou com 1.020 erros e 2 avisos no log `D:\xampp\mysql\backup\desktop-build-followup34-20261030.log`; nao repetir essa contagem como atual sem novo build. Foram inventariadas 294 paginas XAML; 255 nao possuem code-behind e estao agrupadas em `docs/CLASSIFICACAO-XAML-WPF-2026-10-30.md`. O fluxo PDV deixou de ser apenas rascunho e recebeu integracao real com API, mas a compilacao e homologacao ainda estao pendentes. O Desktop continua sem build publicavel.
- Existem 255 XAML sem code-behind entre 294 paginas; a classificacao tematica esta registrada, mas o mapeamento de cada pagina para navegacao real e contratos ainda esta pendente. Nenhuma pagina deve ser tratada como funcional apenas por existir.
- Nao foi encontrado pacote/instalador Desktop validado. Nao publicar artefato ate compilar, empacotar, instalar e validar o fluxo real.

## Publicacao e criterio de aceite

- O workflow do site esta verde, mas isso nao valida o login nem o Desktop.
- As implantacoes vermelhas do ambiente `production` na captura sao historicas. Nos workflows CI/CD consultados de 2026-10-09, API Build e API Tests passaram, mas Frontend Build falhou: `API_BASE_URL` foi importado de `../services/api` sem estar exportado no commit executado; Docker Frontend e Deploy Production foram ignorados por dependencia do build. O código atual em `main` exporta `API_BASE_URL` e `API_URL`.
- Corrigido no commit `a5cbd33b2d9e587b9d0b8d904a57e5afefc23585`: os filtros push/PR do CI/CD agora incluem `NexumAltivon_Front-End/**`. A execução 41097234355 concluiu com sucesso: API Build, gate de testes, Frontend Build, Docker API e Docker Frontend passaram. Deploy Production e Deploy Staging foram ignorados pelas condições de acionamento manual. A publicação GitHub Pages usa workflow separado e passou em 2026-10-30.
- Seguir `PROD_SAFE_UPDATE_CHECKLIST.md` e o checklist clinico; evidencias e pendencias devem ser atualizadas sem alterar denominadores historicos sem auditoria.
- Revalidar o status apos qualquer alteracao de DNS, frontend, API, MySQL, tunnel ou instalador.
