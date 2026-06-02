# Checklist de atualizacao segura em producao

Objetivo: toda alteracao publicada em producao deve ser validada antes e depois da subida, sem derrubar o site, a API ou o painel operacional.

## 1) Antes de publicar

- Confirmar escopo exato da alteracao.
- Gerar backup local atualizado quando a alteracao mexer em deploy, API, banco, DNS ou painel.
- Rodar build local do front-end:
  - `npm run build` em `NexumAltivon_Front-End`.
- Rodar publish local da API quando houver alteracao no backend:
  - `dotnet publish NexumAltivon_Back-End\NexumAltivon.API.csproj -c Release`.
- Conferir que nenhum segredo real foi versionado.
- Conferir se URLs publicas estao corretas:
  - Front-end deve chamar `https://api.nexumaltivon.com`.
  - Login deve usar `/api/auth/login`.

## 2) Publicacao

- Publicar primeiro o front-end quando a alteracao for visual ou de apontamento da API.
- Publicar API separadamente quando houver alteracao de backend.
- Evitar trocar DNS junto com codigo, salvo quando isso for o objetivo da etapa.
- Manter uma versao anterior pronta para rollback.

## 3) Depois de publicar

Testar fora do ambiente local:

- `https://www.nexumaltivon.com`
- `https://www.nexumaltivon.com/login`
- `https://api.nexumaltivon.com/`
- `https://api.nexumaltivon.com/health`

Validar no painel:

- Login administrativo.
- Abrir dashboard.
- Listar produtos.
- Cadastrar um produto de teste controlado.
- Cadastrar cliente de teste controlado.
- Cadastrar fornecedor de teste controlado.
- Cadastrar lead de teste controlado.

## 4) Se algo falhar

- Nao continuar novas alteracoes.
- Identificar se a falha esta em DNS, front-end, API, banco ou senha/variavel de ambiente.
- Fazer rollback para o ultimo pacote funcional.
- Registrar o erro e a acao tomada antes de tentar nova publicacao.

## 5) Variaveis obrigatorias da API em producao

- `AdminUser__Email`
- `AdminUser__Password`
- `AdminUser__Name`
- `AdminUser__Role`
- `JwtSettings__SecretKey`
- `ConnectionStrings__DefaultConnection`

