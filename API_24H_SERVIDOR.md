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
scripts\server\INSTALAR-API-24H-SERVIDOR-COMO-ADMIN.cmd
```

O instalador:

- publica a API em `Y:\NexumAltivon_API_24H\api`;
- cria a configuração privada em `Y:\NexumAltivon_API_24H\config\api.env.ps1`;
- cria a tarefa automática `NexumAltivonApi24h`;
- inicia um guardião que testa `/health` e reinicia a API se ela cair.

## Instalação por pacote pronto

Quando o pacote já estiver publicado no compartilhamento do servidor, execute no próprio servidor como Administrador:

```cmd
INSTALAR-API-24H-PACOTE-COMO-ADMIN.cmd
```

Este caminho é o mais rápido quando o desenvolvimento foi feito em outra máquina: o pacote já contém a API compilada e só instala a operação 24h no servidor.

Pacote corrigido para a unidade certa do servidor:

```cmd
Y:\NexumAltivon_API_24H_Y_FIX\INSTALAR-API-24H-PACOTE-COMO-ADMIN.cmd
```

Para gerar esse pacote no compartilhamento do servidor:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\server\criar-pacote-api-24h-servidor.ps1
```

## Configuração privada

O arquivo real `Y:\NexumAltivon_API_24H\config\api.env.ps1` nunca deve ir para o Git.

Ele precisa conter:

- conexão com MariaDB/MySQL em `192.168.1.72:3309`;
- senha real do usuário `nexum_app`;
- chave JWT forte;
- senha real do usuário administrador.

Use `scripts\server\api.env.example.ps1` apenas como modelo.

## Publicação externa

Com a Wix ainda mantendo os nameservers, o DNS público deve continuar sendo ajustado na Wix até a transferência total do domínio.

Para `api.nexumaltivon.com.br`, o caminho operacional oficial é:

- API rodando em `http://127.0.0.1:5012` no servidor;
- Cloudflared rodando no mesmo servidor;
- rota pública do túnel apontando `api.nexumaltivon.com.br` para `http://127.0.0.1:5012`;
- CNAME/DNS do Cloudflare para o destino `*.cfargotunnel.com` correto do túnel nomeado.

## Verificação

No servidor, rode:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts\server\verificar-api-24h.ps1
```

Critérios mínimos de aceite:

- tarefa `NexumAltivonApi24h` existe;
- porta local `5012` responde;
- `http://127.0.0.1:5012/health` retorna saudável;
- `https://api.nexumaltivon.com.br/health` responde publicamente;
- login do painel funciona em `https://nexumaltivon.com.br/login`.

## Plano definitivo

Depois do dia 17/06/2026, quando a transferência completa do domínio for liberada, o caminho recomendado é mover a zona DNS inteira para Cloudflare ou publicar a API em VPS com IP público fixo.
