<!--
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
-->

# Arvore Real e Arvore Meta do GenesisGest.Net

Apuracao tecnica: 2026-07-18.

## Regra de Evidencia

O arquivo `ANALISE_ESTRUTURAL_GRUPO_NEXUM_ALTIVON2.txt` foi usado como referencia historica de amplitude, nao como prova de implementacao. Uma capacidade somente integra a arvore real quando existe no projeto oficial e possui evidencia verificavel. A arvore meta preserva as capacidades uteis do documento, mas exige API oficial, autorizacao, persistencia quando aplicavel, tratamento de erro, interface oficial e validacao operacional antes da conclusao.

Classificacoes usadas:

- `Comprovado`: contrato e comportamento foram validados no runtime oficial.
- `Parcial`: existe implementacao real, mas falta pelo menos um gate obrigatorio.
- `Bloqueado externamente`: o codigo recusa operacao sem credencial, certificado ou aceite do provedor e nao registra sucesso.
- `Nao comprovado`: nao existe evidencia suficiente para uso produtivo.

## Inventario Estrutural Atual

| Camada | Evidencia em 2026-07-18 |
|---|---|
| Solution .NET | 5 projetos: API, API.Tests, ERP, Desktop e projeto raiz |
| API oficial | 236 rotas Minimal API em `NexumAltivon_Back-End/API/Program.cs` |
| Frontend | 16 paginas React em `NexumAltivon_Front-End/src/pages` |
| Desktop | 14 arquivos XAML funcionais fora de `bin/obj` |
| Banco Nexum | 203 tabelas base e 12 views no schema `nexum_altivon` |
| Banco Genesis | 48 tabelas base no schema `genesis_bd` |
| Runtime | API unica em `127.0.0.1:5010`, tarefa `NexumAltivonApi24h` como `SYSTEM`, gatilho de boot e processo oculto |
| Exposicao | Cloudflared automatico; health local e publico comprovado |

## Arvore Real

```text
GenesisGest.Net v1.1.5
|-- Plataforma oficial
|   |-- API Minimal .NET 8 [comprovada em runtime]
|   |-- React 19 [build e publicacao existentes]
|   |-- Desktop WPF .NET 8 [solution e acesso local existentes]
|   `-- MySQL 8: nexum_altivon + genesis_bd [health comprovado]
|-- 00 Nucleo e SSO [parcial avancado]
|   |-- Login, JWT, refresh rotativo e logout
|   |-- MFA TOTP
|   |-- Usuarios, perfis, permissoes e RBAC
|   |-- Tenants [isolamento integral ainda nao comprovado]
|   `-- Workflow [backend comprovado; telas por dominio pendentes]
|-- 01 Cockpit executivo [parcial]
|   |-- Resumo e KPIs por API
|   `-- Grafico semanal estatico ainda presente no Dashboard React
|-- 02 GRC e IAM [parcial avancado]
|   |-- Gestao administrativa React/WPF
|   `-- Auditoria consultavel [cobertura de toda escrita pendente]
|-- 03 Master Data [parcial]
|   |-- Empresas, clientes, fornecedores, produtos e categorias
|   |-- Pessoas, centros de custo e itens/servicos
|   `-- Gestao dedicada das seis lojas pendente
|-- 04 FICO [parcial]
|   |-- Lancamentos, razao, conciliacao, DRE e fechamento
|   `-- PDFs financeiros gerados; consumo completo por tela/API pendente
|-- 05 SCM e WMS [parcial]
|   |-- Compras, solicitacoes, cotacoes, pedidos e entradas
|   `-- WMS completo e portaria ainda sem homologacao ponta a ponta
|-- 06 Comercial, CRM e Fiscal [parcial]
|   |-- Pedidos, PDV, checkout e catalogo
|   |-- Cupons [CRUD e uso publico comprovados]
|   |-- Campanhas e segmentos [CRUD comprovado]
|   |-- Checkout com incidente fiscal/pos-commit duravel
|   |-- Pre-emissao fiscal e reserva atomica de numero
|   `-- SEFAZ e marketplaces [bloqueados externamente sem credenciais]
|-- 07 HCM e RH [parcial]
|   `-- Resumo e colaboradores; folha, ponto e eSocial sem aceite integral
|-- 08 MES e OPS [parcial]
|   |-- Rotas de OS, producao, manutencao e ativos
|   `-- Telas React/WPF especificas e fluxo integral pendentes
|-- Integracoes [parcial]
|   |-- Frete interno oficial identificado
|   |-- Tracking externo bloqueado sem provedor
|   |-- Yara no portal e Sophia no administrativo, sem resposta fabricada
|   `-- Anexos/MinIO, Redis e jobs ainda exigem homologacao produtiva
`-- Operacao 24h [comprovada no servidor atual]
    |-- MySQL, Cloudflared e Agendador automaticos
    `-- API sobe no boot sem login interativo
```

## Arvore Meta de Aceite Integral

```text
GenesisGest.Net concluido
|-- Plataforma e seguranca
|   |-- Tenant, auditoria, concorrencia e soft-delete em toda entidade transacional
|   |-- Segregacao de funcoes e autorizacao comprovadas por modulo
|   |-- Segredos somente em armazenamento privado e rotacionavel
|   `-- Banco canonico unico apos UBD-01 a UBD-08
|-- Experiencia multicanal
|   |-- React dinamico, responsivo e administravel por midia real
|   |-- WPF com paridade funcional, navegacao limpa e visual fume translucido
|   `-- Nenhum botao confirma operacao sem resposta persistida
|-- Operacao corporativa 00 a 08
|   |-- Cada formulario com contexto, cabecalho, itens, financeiro, anexos e auditoria
|   |-- Cada escrita com transacao, tenant, RBAC, concorrencia e trilha
|   `-- Cada leitura derivada de dados atuais, sem indicadores estaticos
|-- Cadeia ponta a ponta
|   |-- Cadastro -> venda -> pagamento -> estoque
|   |-- Estoque -> fiscal homologado -> logistica
|   `-- Logistica -> financeiro -> contabil -> BI
|-- Integracoes externas
|   |-- SEFAZ com chave e protocolo de homologacao persistidos
|   |-- Marketplace com identificador aceito pelo seller persistido
|   |-- Tracking com evento retornado pelo transportador
|   `-- IA com chave administrada, custo controlado e erro rastreavel
|-- Resiliencia e operacao
|   |-- Redis, jobs, anexos, observabilidade e backups homologados
|   |-- Restore semanal comprovado
|   |-- Staging e producao com migrate e health gates
|   `-- Auto-update Desktop validado em maquina cliente
`-- Qualidade
    |-- Solution e frontend compilam sem erro
    |-- Services mantem cobertura minima de 70%
    |-- Contratos consumidos nao retornam 404
    `-- Definition of Done em 31/31 com evidencia anexada ao checklist
```

## Capacidades Recuperadas e Aprofundadas

| ID | Capacidade | Estado atual | Gate para conclusao |
|---|---|---|---|
| CAP-01 | Cockpit executivo dinamico | Parcial avancado | Series estaticas removidas e periodos 7/30/90 reconciliados entre API local, API publica e MySQL; falta provar isolamento entre dois tenants e homologar a tela publicada no Chrome |
| CAP-02 | Venda omnicanal | Parcial | Provar pedido, pagamento, reserva/baixa, fiscal, entrega e financeiro na mesma correlacao |
| CAP-03 | Compras e abastecimento | Parcial | Provar solicitacao, cotacao, aprovacao, pedido, entrada, estoque e financeiro |
| CAP-04 | WMS | Parcial | Provar movimentacao, inventario, kardex, localizacao e transferencia com concorrencia |
| CAP-05 | Fiscal explicavel | Parcial | Exibir roteamento, motivo, numeracao, XML, chave, protocolo e eventos oficiais |
| CAP-06 | FICO corporativo | Parcial | Integrar lancamentos, conciliacao, fechamento, DRE e PDFs em tela/API oficial |
| CAP-07 | Logistica e fulfillment | Parcial | Provar cotacao, expedicao, rastreamento externo, entrega e ocorrencias |
| CAP-08 | CRM e atendimento Yara | Parcial | Provar pipeline, oportunidade, atividade, ticket e atendimento com chave real, sem seletor de persona |
| CAP-09 | HCM | Parcial | Provar admissao, ponto, folha, beneficios, avaliacao, desligamento e eSocial |
| CAP-10 | MES e OPS | Parcial | Entregar telas React/WPF e provar OS, apontamento, manutencao e ativos |
| CAP-11 | Paridade Desktop | Parcial | Toda acao critica deve usar a API oficial e confirmar releitura persistida |
| CAP-12 | Marketplaces e dropshipping | Bloqueado externamente | Homologar cada seller e provedor com credencial e identificador oficial |
| CAP-13 | Portal visual dinamico | Parcial | Administrar imagens de lojas/produtos, validar responsividade e remover conteudo estatico operacional |
| CAP-14 | Operacao observavel | Parcial | Comprovar logs, traces, metricas, jobs, Redis, backup e restore |
| CAP-15 | Banco canonico | Pendente programado | Cumprir inventario, linhagem, equivalencias, migracao, reconciliacao e reversao |

## Prioridade de Execucao

1. Encerrar erros que possam confirmar venda ou escrita sem registrar incidentes.
2. Substituir dados estaticos e acoes apenas visuais por contratos persistidos.
3. Completar um fluxo vertical por vez, preservando os gates comuns de seguranca.
4. Homologar integracoes externas somente com credenciais oficiais e retorno do provedor.
5. Consolidar React e WPF sobre a mesma API antes da futura unificacao fisica dos schemas.

Este mapa nao altera a medicao do checklist: capacidades parciais permanecem fora do numerador ate o atendimento integral do respectivo gate.
