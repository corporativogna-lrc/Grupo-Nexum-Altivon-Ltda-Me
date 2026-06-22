# Checklist de Prontidao de Deploy

Data de corte: 16/06/2026
Versao alvo: 1.1.5
Deadline operacional informado pelo projeto: 18/06/2026

## 1. Diagnostico de riscos

### Risco 1: runtime fragmentado entre 5010, 5011 e 5012

Impacto comercial:
- frontend e automacoes podem falar com uma API diferente da API realmente ativa
- checkout, area do cliente e ERP passam a ler estados diferentes
- qualquer teste de venda deixa de ser confiavel

Evidencias confirmadas no codigo:
- [VERSAO.md](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\VERSAO.md) ainda aponta para `localhost:5010`
- [scripts/01-instalar-api-local-permanente.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\01-instalar-api-local-permanente.ps1) usa `5011`
- [scripts/start-nexum-auto.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\start-nexum-auto.ps1) usa `192.168.1.72:5012`
- [NexumAltivon_Front-End/src/services/api.js](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Front-End\src\services\api.js) usa `192.168.1.72:5012`

Correcao imediata:
- congelar o padrao definitivo em `http://192.168.1.72:5012`
- desativar inicializadores antigos de `5010` e `5011`
- republicar backend e frontend apontando para a mesma base
- validar `/health`, login admin, cadastro cliente e checkout na mesma porta

### Risco 2: checkout parcialmente funcional, mas ainda nao homologado ponta a ponta

Impacto comercial:
- o pedido pode nascer, mas falhar em pagamento, fiscal, estoque ou sincronizacao ERP
- qualquer venda real fica sem garantias de baixa financeira e acompanhamento logistico

Evidencias confirmadas no codigo:
- [NexumAltivon_Back-End/API/Program.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Program.cs) possui rota de checkout em `/api/pedidos`
- [NexumAltivon_Back-End/API/Data/NexumDbContext.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Data\NexumDbContext.cs) recebeu conversoes de enums para `Cliente`, `Produto`, `Pedido`, `PedidoItem` e `Pagamento`
- [NexumAltivon_Back-End/API/Program.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Program.cs) atualiza status do pedido e movimenta estoque em `/api/pedidos/{id}/status`

Correcao imediata:
- subir uma unica API em `5012`
- revalidar oficialmente:
  - criacao de pedido
  - confirmacao de pagamento
  - troca de status para `Pago`, `EmSeparacao`, `Enviado`, `Entregue`
  - baixa de estoque
  - emissao fiscal
  - sincronizacao ERP

### Risco 3: fiscal e notificacoes ainda dependem de configuracao real de producao

Impacto comercial:
- o motor de roteamento fiscal pode rodar sem emitente valido
- os e-mails de confirmacao podem apenas simular envio
- sem credenciais reais, pagamento e logistica nao concluem a venda

Evidencias confirmadas no codigo:
- [NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\ERP\FiscalRouting\FiscalRoutingEngine.cs) calcula ranking por custo tributario, custo operacional e margem
- [NexumAltivon_Back-End/API/Program.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Program.cs) cria e amplia `erp_empresas_grupo` e `fiscal`
- [NexumAltivon_Back-End/API/Services/NotificacaoService.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Services\NotificacaoService.cs) envia copia para `corporativo.gna@gmail.com`, mas sem `Integracoes:SendGrid:ApiKey` cai em e-mail simulado
- [NexumAltivon_Back-End/API/appsettings.json](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\appsettings.json) ainda tem chaves vazias para gateway, logistica, dropshipping, certificado e fiscal

Correcao imediata:
- preencher `erp_empresas_grupo` com emitentes reais e marcacao de `ativa=1`
- configurar `Integracoes:SendGrid:ApiKey`
- configurar gateway principal e secundario
- configurar Melhor Envio e demais operadores logisticos
- configurar certificado NF-e/NFC-e no servidor principal

## 2. Script de correcao SSOT

Executar:
- [scripts/sql/2026-06-16-ssot-unificar-lojas-estoque.sql](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\sql\2026-06-16-ssot-unificar-lojas-estoque.sql)

Esse script faz:
- cria tabela de auditoria de inconsistencias
- normaliza tabela de configuracao SSOT das lojas
- registra prefixos oficiais de origem
- desativa produtos fantasma ou incompletos
- corrige estoque negativo e reserva acima do estoque
- cria views de catalogo publicavel, estoque consolidado e emitentes fiscais ativos

## 3. Protocolo de lancamento

Atualizar ou sobrepor obrigatoriamente no servidor principal:
- [NexumAltivon_Back-End/API/Program.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Program.cs)
- [NexumAltivon_Back-End/API/Data/NexumDbContext.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Data\NexumDbContext.cs)
- [NexumAltivon_Back-End/API/ERP/FiscalRouting/FiscalRoutingEngine.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\ERP\FiscalRouting\FiscalRoutingEngine.cs)
- [NexumAltivon_Back-End/API/Services/NotificacaoService.cs](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\Services\NotificacaoService.cs)
- [NexumAltivon_Back-End/API/appsettings.PrivateProduction.template.json](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Back-End\API\appsettings.PrivateProduction.template.json)
- [NexumAltivon_Front-End/src/services/api.js](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Front-End\src\services\api.js)
- [NexumAltivon_Front-End/public/api-runtime.json](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\NexumAltivon_Front-End\public\api-runtime.json)
- [scripts/start-nexum-auto.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\start-nexum-auto.ps1)
- [scripts/nexum-api-guardian.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\nexum-api-guardian.ps1)
- [scripts/sql/2026-06-16-ssot-unificar-lojas-estoque.sql](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\sql\2026-06-16-ssot-unificar-lojas-estoque.sql)

Arquivos que nao devem continuar ativos sem padronizacao:
- [VERSAO.md](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\VERSAO.md)
- [scripts/01-instalar-api-local-permanente.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\01-instalar-api-local-permanente.ps1)
- [scripts/02-instalar-api-definitiva-tarefa.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\02-instalar-api-definitiva-tarefa.ps1)
- [scripts/80-instalar-api-autostart-usuario.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\scripts\80-instalar-api-autostart-usuario.ps1)
- [publish-iis.ps1](C:\Users\Rodrigo Costa\Documents\Codex\2026-05-28\files-mentioned-by-the-user-nexumaltivon\NexumAltivon.com\publish-iis.ps1)

## 4. Ordem executiva ate o deploy

1. Padronizar a porta unica em `5012`.
2. Rodar o SQL SSOT no banco central `192.168.1.72:3309`.
3. Publicar backend com as conversoes de enum e o motor fiscal atual.
4. Publicar frontend consumindo a mesma API `5012`.
5. Injetar as credenciais reais de pagamento, logistica, fiscal e e-mail.
6. Rodar teste oficial do fluxo cliente:
   - cadastro
   - confirmacao por link
   - login
   - carrinho
   - checkout
   - pagamento
   - pedido no painel
   - faturamento
   - notificacoes
   - status logistico final

## 5. Code freeze

Nao abrir nova frente antes de estabilizar:
- Checkout
- Fiscal routing
- Emissao NF-e/NFC-e
- Notificacoes
- ERP sync
- Estoque SSOT
