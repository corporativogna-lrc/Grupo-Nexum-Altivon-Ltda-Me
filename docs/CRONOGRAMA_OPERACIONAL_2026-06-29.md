<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Cronograma Operacional Validado - 29/06/2026

Este documento confronta o panorama anterior de 26/06/2026 com o estado validado em 29/06/2026. Ele substitui o bloqueio antigo da ponte publica por um checklist real baseado nos testes executados no ambiente online.

## Estado Validado Hoje

| Frente | Estado | Validacao |
| --- | --- | --- |
| Site publico | Operante | `https://nexumaltivon.com.br` retornou HTTP 200 |
| API publica | Operante | `https://api.nexumaltivon.com.br/health` retornou `Healthy` |
| Banco via API | Operante | `https://api.nexumaltivon.com.br/health/db` retornou `Healthy` |
| Produtos na API | Operante | `/api/produtos?limite=5` retornou produtos reais |
| Lojas na API | Operante | `/api/lojas` retornou lojas reais |
| Login administrativo | Operante | Login admin autenticou com sucesso |
| GenesisGest original | Sincronizado | 129 estruturas reconhecidas e 3 pontes Nexum/Genesis ativas |
| Modulo de compras | Operante na API | `/api/compras/painel` respondeu autenticado |
| Checkout controlado | Operante | Pedido real de teste criado em producao controlada |
| Contas a receber | Operante | Pedido criou lancamento financeiro local automaticamente |
| Genesis contas a receber | Operante | Pedido novo foi sincronizado no Genesis com valor correto |
| Backend .NET | Compilado | `dotnet build` em Release com 0 erros e 0 avisos |
| Versionamento recente | Atualizado | `main` contem schema Genesis, compras e estoque versionados |

## Cronograma Confrontado

### Panorama de 26/06/2026

O cronograma anterior indicava o sistema com boa condicao para MVP, mas com bloqueio critico na rota publica da API. Naquele momento o percentual estimado era:

- MVP de vendas online controladas: 82%.
- Operacao completa com integracoes, logistica, faturamento fiscal real, gateways e PDV: 60% a 65%.
- Site + API + checkout + area do cliente + base local: 75%.

### Atualizacao de 29/06/2026

O bloqueio principal de publicacao foi superado para a API principal. A operacao publica ja responde, o banco esta acessivel pela API, produtos reais retornam, login administrativo autentica e o modulo de compras responde.

Percentual atualizado:

- MVP de vendas online controladas: 91%.
- Operacao completa com integracoes externas, fiscal real, logistica automatizada e PDV: 70%.
- Base site + API + banco + checkout + area cliente + admin + compras + Genesis: 85%.

## Checklist Validado

- [x] Dominio `nexumaltivon.com.br` online.
- [x] API publica `api.nexumaltivon.com.br` online.
- [x] Banco respondendo por `/health/db`.
- [x] Produtos reais retornando pela API.
- [x] Lojas reais retornando pela API.
- [x] Admin autenticando.
- [x] Painel de compras acessivel por API autenticada.
- [x] Schema original GenesisGest integrado sem quebra de build.
- [x] Pontes Genesis/Nexum criadas para produtos, compras e financeiro.
- [x] Build Release do backend sem erros.
- [x] Compras, entradas e movimentos de estoque ja versionados em commits anteriores.
- [x] Pedido de venda criado em producao controlada.
- [x] Conta a receber local criada automaticamente no checkout.
- [x] Conta a receber Genesis criada automaticamente no checkout.
- [x] Dashboard administrativo refletindo pedidos e faturamento do dia.

## Checklist A Realizar Para Venda Controlada

Prioridade imediata:

- [x] Validar via API publica o fluxo completo: cliente -> checkout -> pedido criado -> financeiro -> Genesis.
- [ ] Validar no navegador o fluxo completo: Home -> produto -> carrinho -> checkout -> pedido criado.
- [ ] Confirmar que a Home publica esta consumindo `https://api.nexumaltivon.com.br` e nao cache antigo.
- [x] Validar criacao de contas a receber no pedido real de teste.
- [x] Validar painel administrativo refletindo pedido/faturamento criado.
- [ ] Validar area do cliente exibindo o pedido criado.
- [ ] Ajustar texto de pagamento inicial para modo controlado: PIX/manual/deposito enquanto gateway real nao estiver liberado.
- [ ] Executar backup do estado publicado apos o teste de venda ponta a ponta.

## Checklist A Realizar Para Operacao Full

Integracoes e canais:

- [ ] Ativar credenciais reais de gateway de pagamento.
- [ ] Ativar credenciais reais de logistica.
- [ ] Ativar marketplaces conforme tokens oficiais chegarem.
- [ ] Ativar dropshipping por parceiro com credencial propria.
- [ ] Registrar logs de sincronizacao por integracao.
- [ ] Criar tela administrativa de status por integracao.

Fiscal e financeiro:

- [ ] Instalar certificados digitais oficiais.
- [ ] Configurar emitentes reais por CNPJ.
- [ ] Homologar emissao fiscal real.
- [ ] Fechar contingencia fiscal manual.
- [ ] Validar contas a pagar, contas a receber, baixa e rastreio financeiro.

Operacao interna:

- [ ] Finalizar formulários completos de compras, cotacoes, pedidos de compra e entrada fiscal/fisica.
- [ ] Expandir auditoria corporativa `sys_*`.
- [ ] Consolidar multitenancy e trilha de alteracoes por usuario.
- [ ] Validar reinicio do servidor com API, guardian e Cloudflare retornando sem intervencao.
- [ ] Revisar imagens dos produtos para eliminar divergencias visuais.

## Proxima Frente Recomendada

Executar o teste real de venda controlada antes de acrescentar novas integracoes. Se o pedido real fechar ponta a ponta, a proxima frente deve ser integracoes de aquisicao e entrada de mercadorias, porque ela alimenta estoque, financeiro, fiscal, fornecedores e compras.

## Ordem De Execucao Atual

1. Validacao visual no navegador do checkout ja validado pela API publica.
2. Area do cliente exibindo pedido criado.
3. Backup do estado publicado apos validacao visual.
4. Modulo completo de compras e aquisicoes na interface.
5. Integracoes reais conforme credenciais oficiais.
6. Fiscal, PDV e operacao full.
