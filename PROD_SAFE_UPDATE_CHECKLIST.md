# Checklist de atualizacao segura em producao

Objetivo: validar cada alteracao antes e depois da publicacao, sem derrubar o site, a API ou o painel operacional.

## Antes de publicar

- Confirmar o escopo exato e preservar alteracoes locais preexistentes.
- Gerar backup atualizado antes de alterar deploy, API, banco, DNS ou painel.
- Executar o build do frontend no diretorio `NexumAltivon_Front-End`.
- Executar `dotnet publish` da API em Release quando houver mudanca no backend.
- Conferir que nenhum segredo real foi versionado.
- Confirmar a URL canonica do site: `https://nexumaltivon.com.br`.
- Confirmar a API: `https://api.nexumaltivon.com.br` e o endpoint de login `/api/auth/login`.
- Usar a publicacao oficial existente e registrar o commit publicado.

## Depois de publicar

Validar fora do ambiente local:

- `https://nexumaltivon.com.br/` retorna HTTP 200.
- `https://nexumaltivon.com.br/login` e validado no navegador; o HTTP direto pode depender do roteamento SPA de GitHub Pages.
- `https://api.nexumaltivon.com.br/health` retorna HTTP 200.
- `https://api.nexumaltivon.com.br/health/db` e `/health/db/genesis` retornam sucesso.
- Preflight CORS do site para a API permite o fluxo de login.
- Login e sessao sao verificados com credencial autorizada, sem registrar senha em log.
- Conferir console/rede do navegador e o carregamento de assets.
- No Desktop, compilar a solucao completa, gerar o pacote oficial, instalar e abrir o executavel; validar login/API e modulos preservados.

Nao inserir cadastros artificiais em producao. Para testar escrita, usar somente dado controlado explicitamente autorizado, registrar identificador e remover de forma verificavel ao final. Os registros de login/usuario e cadastro de empresas existentes sao dados uteis e devem ser preservados.

## Se algo falhar

- Interromper novas publicacoes e localizar a falha em DNS, frontend, API, banco, CORS, credenciais ou empacotamento Desktop.
- Nao efetuar rollback destrutivo sem identificar o artefato anterior e preservar o estado atual.
- Registrar evidencia, causa, correcao e resultado no checklist do projeto.

## Configuracao de producao

- Usar as variaveis efetivamente exigidas pela configuracao da API publicada.
- Nao documentar valores de segredo nem presumir nomes de configuracao sem conferir o codigo e o arquivo privado de runtime.
- Configuracoes locais privadas permanecem fora do Git.
