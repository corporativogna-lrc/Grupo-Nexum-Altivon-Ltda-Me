<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Checklist Operacional Por Tabelas - 29/06/2026

Este checklist transforma a base atual dos bancos `nexum_altivon` e `genesis_bd` em frentes operacionais reais. O criterio de conclusao de cada frente e simples: tabela disponivel so conta como funcional quando tiver API, tela, validacao, permissao, auditoria minima e teste real.

## Estado Consolidado Dos Bancos

| Banco | Estado | Realidade atual |
| --- | --- | --- |
| `nexum_altivon` | Base principal operacional | Vendas, clientes, produtos, compras, estoque, financeiro, fiscal inicial, CRM inicial e integracoes em andamento |
| `genesis_bd` | Base GenesisGest.Net compartilhada | Financeiro Genesis ja recebe contas a receber e contas a pagar; demais modulos precisam ganhar telas e rotas |

## Ferramentas Ja Operacionais

| Area | Tabelas base | Ferramenta atual | Status |
| --- | --- | --- | --- |
| Catalogo e vitrine | `produtos`, `categorias`, `lojas`, `configuracoes_sistema` | Home, catalogo, detalhe de produto e painel de produto | Validado |
| Cliente e endereco | `clientes`, `enderecos` | Cadastro, login, area do cliente e enderecos auxiliares | Validado |
| Venda online | `pedidos`, `pedido_itens`, `pagamentos`, `financeiro` | Checkout, pedido, acompanhamento e conta a receber | Validado |
| Compras operacionais | `compras_solicitacoes`, `compras_cotacoes`, `compras_pedidos`, `compras_pedido_itens`, `compras_entradas`, `compras_entrada_itens` | Cotacao, pedido de compra e entrada de mercadoria | Validado pela API e painel |
| Fornecedores | `fornecedores` | Cadastro e selecao em compras | Validado |
| Estoque por compra | `estoque_movimentos`, `produtos` | Entrada atualiza saldo, custo, codigo de barras, QR code e identificacao | Validado |
| Financeiro Genesis | `erp_contas_receber`, `erp_contas_pagar`, `erp_fluxo_caixa` | Contas a receber e a pagar sincronizadas com Nexum | Validado |
| Fiscal inicial | `fiscal`, `erp_impostos_config`, `erp_notas_fiscais` | Preparacao manual, roteamento e contingencia inicial | Parcial |
| CRM inicial | `crm_leads`, `crm_atendimentos` | Lead e status basico | Parcial |
| Integracoes | `dropshipping_config`, `marketplaces`, `transportadoras` | Diagnostico e cadastro de credenciais-modelo | Parcial |

## Tabelas Que Devem Virar Ferramentas

## Regra Geral De Campos Em Formularios

Todos os campos operacionais existentes nas tabelas atuais e todos os novos campos criados a partir deste levantamento devem ser tratados como parte da ferramenta, nao apenas como coluna de banco. A regra de aceite passa a ser:

- Campos de negocio devem aparecer nos formularios existentes ou nos novos formularios do modulo correspondente.
- Campos obrigatorios no banco devem ter validacao clara antes de gravar.
- Campos opcionais relevantes devem estar disponiveis em abas, secoes avancadas ou complementos do formulario.
- Campos de relacionamento devem usar seletores reais, busca ou autocomplete, nunca texto solto quando houver tabela relacionada.
- Campos calculados devem aparecer como leitura ou totalizador, com origem clara para conferencia.
- Campos de status devem alimentar botoes de acao, filtros e fluxo de aprovacao quando aplicavel.
- Campos internos de auditoria, controle tecnico, `Id`, `TenantId`, `RowVersion`, datas automaticas e soft delete nao precisam aparecer como campos editaveis, mas devem ser exibidos em trilha de auditoria quando fizer sentido.
- Nenhum campo novo pode ficar sem destino: formulario, listagem, detalhe, auditoria, relatorio ou integracao.

### 00. Nucleo, acesso e auditoria

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Usuarios e perfis | `usuarios`, `adm_usuarios`, `adm_perfis`, `adm_permissoes`, `adm_perfil_permissoes` | Gestao de usuarios, niveis, aprovacao de acesso e bloqueio | A fazer |
| Auditoria | `logs_auditoria`, `adm_auditoria`, `erp_logs` | Tela de trilha de auditoria por usuario, tabela, acao e periodo | A fazer |
| Parametros | `cfg_parametros`, `configuracoes_sistema`, `cfg_sequenciais` | Central de parametros, sequenciais, numeracao e chaves operacionais | A fazer |
| Workflows | `cfg_workflow_definicoes`, `cfg_workflow_etapas`, `cfg_workflow_instancias`, `cfg_workflow_aprovacoes` | Aprovacoes por etapa para compras, financeiro, fiscal e cadastros | A fazer |
| Anexos e comentarios | `cfg_anexos`, `cfg_comentarios`, `cfg_notificacoes`, `cfg_tarefas` | Anexos, comentarios internos, tarefas e notificacoes por processo | A fazer |

### 01. BI e cockpit executivo

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Indicadores | `bi_kpis`, `bi_kpi_valores`, `bi_widgets`, `bi_dashboards` | Cockpit de indicadores por venda, compra, estoque, financeiro e fiscal | A fazer |
| Relatorios | `bi_relatorios`, `bi_relatorio_historico` | Relatorios salvos, exportacao e historico de execucao | A fazer |
| Visoes gerenciais | `vw_dre_gerencial`, `vw_fluxo_caixa_projetado`, `vw_ranking_produtos`, `vw_saldo_estoque`, `vw_inadimplencia` | Painel executivo com leitura direta das visoes | A fazer |

### 02. Master data corporativo

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Pessoas e empresas | `adm_pessoas_empresas`, `adm_empresas`, `adm_contatos`, `erp_empresas_grupo` | Cadastro unico de pessoas, empresas, contatos e empresas do grupo | Parcial |
| Itens e categorias | `produtos`, `categorias`, `vnd_itens`, `vnd_itens_precos` | Unificacao de item comercial, item fiscal, preco e estoque | Parcial |
| Fornecedores | `fornecedores`, `erp_fornecedores`, `erp_avaliacoes_fornecedor` | Cadastro completo, avaliacao, prazo, risco, origem e performance | Parcial |

### 03. Compras, suprimentos e aquisicoes

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Fluxo atual validado | `compras_*` | Cotacao, pedido, entrada, estoque, financeiro e Genesis | Validado |
| Fluxo Genesis completo | `cmp_requisicoes`, `cmp_requisicao_itens`, `cmp_cotacoes`, `cmp_cotacao_itens`, `cmp_cotacao_fornecedores`, `cmp_pedidos`, `cmp_pedido_itens`, `cmp_notas_fiscais`, `cmp_aprovacoes` | Requisicao interna, cotacao multi-fornecedor, aprovacao, pedido e nota de entrada | A fazer |
| Dropshipping e parcerias | `dropshipping_config`, `marketplaces`, `fornecedores` | Regras de parceiro, custo, prazo, margem, status e origem por item | Parcial |

### 04. Estoque, WMS e movimentacoes

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Movimentos reais | `estoque_movimentos`, `produtos` | Historico de entrada por compra e saldo do produto | Validado |
| Depositos e enderecos | `est_depositos`, `est_enderecos`, `erp_locais_estoque` | Depositos, ruas, prateleiras, bins e saldo por local | A fazer |
| Inventario | `est_inventarios`, `est_inventario_contagens`, `erp_inventarios`, `erp_itens_inventario` | Contagem, divergencia, ajuste e auditoria de estoque | A fazer |
| Kardex e saldos | `est_movimentacoes`, `est_saldos`, `erp_kardex`, `erp_movimentacoes_estoque` | Kardex por produto, custo medio e saldo historico | A fazer |
| Separacao | `est_ordens_separacao`, `est_ordem_separacao_itens`, `est_ponto_pedido`, `est_curva_abc` | Picking, ponto de pedido, curva ABC e reposicao automatica | A fazer |

### 05. Financeiro, tesouraria e contabilidade

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Financeiro operacional | `financeiro`, `erp_contas_receber`, `erp_contas_pagar`, `erp_fluxo_caixa` | Receber, pagar e fluxo basico | Validado |
| Financeiro completo | `fin_titulos_pagar`, `fin_titulos_receber`, `fin_pagamentos`, `fin_recebimentos`, `fin_contas_bancarias`, `erp_contas_bancarias` | Titulos, baixas, bancos, pagamentos e recebimentos | A fazer |
| Conciliacao | `fin_extratos_bancarios`, `fin_conciliacoes`, `fin_aprovacoes_pagar` | Importacao de extrato, conciliacao e aprovacao de pagamento | A fazer |
| Orcamento e moedas | `fin_orcamentos`, `fin_orcamento_itens`, `fin_moedas`, `fin_taxas_cambio` | Orcamento empresarial, moedas e cambio | A fazer |
| Contabilidade | `cnt_lancamentos`, `cnt_partidas`, `cnt_fechamentos`, `fin_plano_contas`, `fin_centros_custo`, `erp_centros_custo` | Plano de contas, partidas e fechamentos | A fazer |

### 06. Fiscal e faturamento

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Fiscal inicial | `fiscal`, `erp_notas_fiscais`, `erp_itens_nota_fiscal` | Rascunho fiscal, contingencia manual e status do pedido | Parcial |
| Tributacao | `fis_tributacao_ncm`, `erp_impostos_config`, `fis_apuracao_impostos` | NCM, impostos, CFOP, apuracao e regras por loja/emitente | A fazer |
| SPED | `fis_sped_fiscal`, `fis_sped_contabil` | Exportacao fiscal e contabil | A fazer |
| Vendas fiscais | `vnd_notas_fiscais`, `vnd_pedidos`, `vnd_pedido_itens` | Pedido fiscal/comercial e nota vinculada | A fazer |

### 07. Comercial, CRM e atendimento

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Leads | `crm_leads`, `erp_leads_crm` | Captura, status e origem do lead | Parcial |
| Atendimento | `crm_atendimentos`, `erp_interacoes_crm`, `erp_tarefas_crm` | Linha do tempo, tarefas, retorno e responsavel | A fazer |
| Precos e vendas | `vnd_tabelas_preco`, `vnd_itens_precos`, `vnd_pedidos`, `vnd_pedido_itens` | Tabelas de preco, pedido comercial e condicoes | A fazer |
| Performance | `vw_performance_vendedores` | Ranking e acompanhamento comercial | A fazer |

### 08. Logistica

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Entrega atual | `envios`, `pedidos`, `transportadoras` | Status logistico basico no pedido | Parcial |
| Frete | `log_transportadoras`, `log_tabelas_frete`, `log_frete_faixas`, `log_auditoria_fretes` | Transportadoras, tabelas, cotacao e auditoria de frete | A fazer |
| Tracking | `log_tracking` | Linha do tempo do envio e notificacao cliente | A fazer |
| Documentos transporte | `log_cte`, `log_mdfe`, `log_mdfe_documentos` | CTe, MDFe e documentos vinculados | A fazer |

### 09. RH e HCM

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Estrutura RH | `rh_departamentos`, `rh_cargos`, `rh_colaboradores`, `rh_dependentes` | Colaboradores, departamentos, cargos e dependentes | A fazer |
| Jornada | `rh_ponto`, `rh_afastamentos`, `rh_ferias` | Ponto, ferias e afastamentos | A fazer |
| Folha | `rh_folhas_pagamento`, `rh_folha_itens`, `rh_eventos_folha`, `rh_historico_salarial`, `rh_esocial_eventos` | Folha, eventos, historico salarial e eSocial | A fazer |

### 10. Producao, manutencao e operacoes

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Engenharia de produto | `pcp_bom`, `pcp_bom_itens`, `pcp_roteiros`, `pcp_roteiro_operacoes` | Ficha tecnica, roteiro e operacoes | A fazer |
| Producao | `pcp_ordens_producao`, `pcp_apontamentos`, `pcp_requisicoes_material`, `pcp_centros_trabalho` | Ordem, apontamento, material e centro de trabalho | A fazer |
| Qualidade | `pcp_inspecoes`, `pcp_nao_conformidades` | Inspecao e nao conformidade | A fazer |

### 11. Juridico

| Grupo | Tabelas | Ferramentas a entregar | Status |
| --- | --- | --- | --- |
| Contratos | `jur_contratos`, `jur_contrato_clausulas`, `jur_aditivos`, `vw_contratos_vencer` | Contratos, clausulas, aditivos e alertas | A fazer |
| Processos | `jur_processos`, `jur_andamentos`, `jur_prazos`, `jur_tipos_acao`, `vw_processos_risco` | Processos, prazos, andamentos e risco | A fazer |
| Certidoes e apoio | `jur_certidoes`, `jur_depositos_judiciais`, `jur_base_conhecimento`, `vw_certidoes_vencer` | Certidoes, depositos e base juridica | A fazer |

## Ordem De Transformacao Em Ferramentas

1. `cfg_*`, `adm_*` e auditoria: base de permissoes, parametros, workflows e trilha de auditoria.
2. `fin_*`, `cnt_*` e centros de custo: completar financeiro, tesouraria e contabilidade.
3. `est_*` e `erp_*` de estoque: WMS, depositos, inventario, kardex e saldos.
4. `cmp_*`: evoluir compras validado para requisicoes, cotacoes multi-fornecedor e aprovacao.
5. `log_*`: frete, tracking, transportadoras, CTe e MDFe.
6. `fis_*` e `vnd_*`: fiscal real, SPED, pedidos comerciais e faturamento.
7. `crm_*` e `vnd_*`: CRM completo, tarefas, funil e tabelas de preco.
8. `bi_*` e `vw_*`: cockpit executivo e relatorios gerenciais.
9. `rh_*`, `pcp_*` e `jur_*`: RH, producao e juridico.

## Regra De Conclusao Por Grupo

Cada grupo so pode ser marcado como pronto quando cumprir todos os pontos:

- API com leitura, criacao e atualizacao quando aplicavel.
- Tela administrativa com formulario funcional e listagem real.
- Todos os campos operacionais da tabela mapeados para formulario, detalhe, listagem, auditoria, relatorio ou integracao.
- Novos campos criados no banco refletidos imediatamente nos formularios existentes ou nos novos formularios planejados.
- Validacao de obrigatoriedade e consistencia.
- Registro em auditoria ou log operacional.
- Integracao com financeiro, estoque, fiscal ou CRM quando a tabela exigir.
- Teste real pela API publica.
- Teste visual no painel publicado quando houver tela.
- Atualizacao deste checklist com evidencia do teste.
