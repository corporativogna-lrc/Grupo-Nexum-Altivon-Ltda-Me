<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Nexum Altivon — API 24h no servidor

Este documento fixa o caminho operacional para a API não depender do Codex, do navegador ou da máquina de desenvolvimento.

## Prioridade

Até sábado às 08:00, a prioridade é manter a API online para sustentar:

- painel administrativo;
- checkout e pedidos;
- cadastros reais;
- imagens de produtos;
- integrações de dropshipping, logística, gateways, e-commerce/marketplaces e financeiro.

## Decisão técnica

A API ASP.NET Core deve rodar no servidor como tarefa automática do Windows.

O Cloudflare Tunnel pode publicar a API para a internet, mas ele não substitui o servidor da API. O Cloudflare transporta/protege o tráfego; quem executa a API continua sendo o servidor local ou uma VPS.

## Instalação no servidor

No servidor, execute como Administrador:

```cmd
scripts\server\VERIFICAR-BANCO-XAMPP-COMO-ADMIN.cmd -StartIfStopped
scripts\server\INSTALAR-API-24H-SERVIDOR-COMO-ADMIN.cmd
```

O instalador:

- publica a API em `D:\NexumAltivon_API_24H\api`;
- exige a configuração privada real em `D:\NexumAltivon_API_24H\config\api.env.ps1`;
- cria a tarefa automática `NexumAltivonApi24h`;
- testa `/health` na porta `5010` antes de concluir.

## Validação operacional oficial

Após instalar ou reparar a API 24h, validar a tarefa oficial no próprio servidor:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\validar-api-oficial-24h-task.ps1"
```

Esse validador confere a tarefa `NexumAltivonApi24h`, o processo real do `dotnet`, a porta `5010` e o `/health` local antes de considerar a API operacional.

Para reinstalar a tarefa oficial quando necessário, executar como Administrador:

```powershell
powershell -ExecutionPolicy Bypass -File "D:\Nexum Altivon\NexumAltivon.com\scripts\server\instalar-api-oficial-24h-task.ps1"
```

## Configuração privada

O arquivo real `D:\NexumAltivon_API_24H\config\api.env.ps1` nunca deve ir para o Git.

Ele precisa definir estes nomes com valores reais:

- `ConnectionStrings__DefaultConnection`
- `ConnectionStrings__NexumDb`
- `ConnectionStrings__GenesisConnection`
- `JwtSettings__SecretKey` ou `JWT_SECRET_KEY`
- `AdminUser__Email`
- `AdminUser__Password`
- `AdminUser__Name`
- `AdminUser__Role`

Rotas fisicas de dados MySQL/MariaDB que o instalador valida no servidor apos a reinstalacao do XAMPP:

- `D:\xampp\mysql\data\nexum_altivon`
- `D:\xampp\mysql\data\genesis_bd`

Rotas de unidade mapeada ou compartilhamento de rede nao sao dependencia operacional da API 24h. O serviço Windows deve usar o caminho fisico local `D:\xampp` para evitar falha de permissao em conta `LocalSystem`.

A porta oficial local do MySQL/MariaDB e `3309`.

Verificação/partida do banco no servidor:

```cmd
scripts\server\VERIFICAR-BANCO-XAMPP-COMO-ADMIN.cmd -StartIfStopped
```

Depois de reinstalar o XAMPP, recrie e valide o usuario real da API no MariaDB com a senha definida na `DefaultConnection` ativa:

```cmd
scripts\server\CONFIGURAR-USUARIO-BANCO-XAMPP-COMO-ADMIN.cmd
```

O serviço Windows funcional do banco, apos a reinstalacao do XAMPP em 09/07/2026, e `NexumAltivonMySQL`. Se o serviço legado `mysql` ficar preso em `StartPending`, ele deve permanecer desabilitado e o reparador deve manter `NexumAltivonMySQL` como serviço real:

```cmd
scripts\server\REPARAR-BANCO-XAMPP-SERVICO-COMO-ADMIN.cmd -ForceRecreateService
```

## Publicação externa

Para `api.nexumaltivon.com.br` e `api.nexumaltivon.com`, o caminho operacional é:

- API rodando em `http://127.0.0.1:5010` no servidor;
- Cloudflared rodando no mesmo servidor;
- rota pública verificada em 2026-07-09: `api.nexumaltivon.com.br` para `http://127.0.0.1:5010`;
- DNS/rota pendente de validação real: `api.nexumaltivon.com`.

Ativação do serviço Windows do túnel:

```cmd
scripts\server\ATIVAR-CLOUDFLARE-TUNNEL-COMO-ADMIN.cmd
```

## Verificação

No servidor, rode:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\server\verificar-api-24h.ps1
```

Critérios mínimos de aceite:

- diretórios `D:\xampp\mysql\data\nexum_altivon` e `D:\xampp\mysql\data\genesis_bd` existem;
- serviço `NexumAltivonMySQL` esta `Running` e `Automatic`;
- porta local `3309` está escutando;
- tarefa `NexumAltivonApi24h` existe;
- porta local `5010` responde;
- `http://127.0.0.1:5010/health` retorna saudável;
- `https://api.nexumaltivon.com.br/health` responde publicamente;
- `https://api.nexumaltivon.com/health` deve ser validado somente depois de configurar DNS/rota Cloudflare para o domínio `.com`;
- login do painel funciona em `https://www.nexumaltivon.com/login`.

## Recuperação de banco após reinstalação do XAMPP

Em 2026-07-09, a reinstalação do XAMPP deixou `D:\xampp\mysql\data\nexum_altivon` e `D:\xampp\mysql\data\genesis_bd` com tabelas em estado `ERROR`, porque as pastas de schema antigas estavam desacopladas do `ibdata1` ativo.

Correção executada:

- datadir quebrado preservado em `D:\NexumAltivon_DB_RECOVERY\data-broken-20260709-210126`;
- datadir íntegro restaurado de `D:\Arquivo Recuperado 03.08.2026\Pacote de Recuperação Completo Segunda Execução\xampp\mysql\data`;
- serviço `NexumAltivonMySQL` reiniciado em `3309`;
- usuário `nexum_app` recriado/validado via `scripts\server\CONFIGURAR-USUARIO-BANCO-XAMPP-COMO-ADMIN.cmd`;
- API republicada em `D:\NexumAltivon_API_24H\api` e validada em `https://api.nexumaltivon.com.br`.

## Plano definitivo

Depois do dia 17/06/2026, quando a transferência completa do domínio for liberada, o caminho recomendado é mover a zona DNS inteira para Cloudflare ou publicar a API em VPS com IP público fixo.
