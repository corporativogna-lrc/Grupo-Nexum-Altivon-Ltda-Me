# Classificacao das paginas WPF sem code-behind

Data do inventario: 2026-10-30. Fontes: XAMLs de `NexumAltivon.Desktop`, arvore `docs/ARVORE-MENU-WPF-2026-08-16.md` e referencias C# do projeto.

Encontradas 294 janelas XAML: 254 sem code-behind e 40 com code-behind. A contagem exclui `App.xaml` e as views de controles. A reconciliação considera `SaldosProdutoWindow` e `CodigoBarrasQrWindow` como janelas integradas.

O agrupamento associa nome e titulo XAML aos ramos da arvore; nao presume que a tela esteja publicada no menu. `Referencias C#` conta arquivos C# que citam a janela fora do proprio code-behind. Zero referencias exige verificacao adicional da navegacao. Foram encontrados cabeçalhos de telas descritas como copias em triagem e sem integracao real; isso nao e contado como funcionalidade.

## Financeiro / Contabilidade (35)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `CentrosContabilWindow.xaml` | GenesisGest.Net - Centros de custo e lucro | 0 |
| `CobrancaReguaWindow.xaml` | GenesisGest.Net - Cobrança, régua e acordos | 0 |
| `ComissoesLiquidacaoWindow.xaml` | GenesisGest.Net - Comissões e liquidação do fornecedor | 0 |
| `ComissoesRegrasWindow.xaml` | GenesisGest.Net - Comissões e regras | 0 |
| `ConciliacaoBancariaWindow.xaml` | GenesisGest.Net - Conciliação bancária | 0 |
| `ConsolidacaoGrupoWindow.xaml` | GenesisGest.Net - Consolidação e grupo econômico | 0 |
| `ConsultaDistribuicaoWindow.xaml` | GenesisGest.Net - Consulta e distribuição de XML | 0 |
| `ContasBancariasCorporativasWindow.xaml` | GenesisGest.Net - Contas bancárias corporativas | 0 |
| `ContasBancariasSaldosWindow.xaml` | GenesisGest.Net - Contas bancárias e saldos | 0 |
| `CustosProjetoWindow.xaml` | GenesisGest.Net - Custos, despesas e orçamento do projeto | 0 |
| `DescontoAprovacaoWindow.xaml` | GenesisGest.Net - Política de desconto e aprovação | 0 |
| `EmprestimosGarantiasWindow.xaml` | GenesisGest.Net - Empréstimos, financiamentos e garantias | 0 |
| `EncargosProvisoesWindow.xaml` | GenesisGest.Net - Encargos, provisões e pagamentos | 0 |
| `FidelidadeCashbackWindow.xaml` | GenesisGest.Net - Fidelidade, cashback e cupons | 0 |
| `FluxoCaixaWindow.xaml` | GenesisGest.Net - Fluxo de caixa realizado e projetado | 0 |
| `ForecastFinanceiroWindow.xaml` | GenesisGest.Net - Forecast financeiro | 0 |
| `ImportacaoExtratoWindow.xaml` | GenesisGest.Net - Importação e classificação de extrato | 0 |
| `IntercompanyWindow.xaml` | GenesisGest.Net - Operações intercompany | 0 |
| `LimiteCreditoWindow.xaml` | GenesisGest.Net - Limite e análise de crédito | 0 |
| `MoedasCalendariosWindow.xaml` | GenesisGest.Net - Moedas, calendários e localidades | 0 |
| `MoedasCambioWindow.xaml` | GenesisGest.Net - Moedas e taxas de câmbio | 0 |
| `OrcamentoVersoesWindow.xaml` | GenesisGest.Net - Orçamento e versões | 0 |
| `PartidasHistoricosWindow.xaml` | GenesisGest.Net - Partidas, históricos e documentos | 0 |
| `PatrimonioWindow.xaml` | GenesisGest.Net - Patrimônio e movimentações | 0 |
| `PlanoContasWindow.xaml` | GenesisGest.Net - Plano de contas | 0 |
| `ProvisionamentoTenantWindow.xaml` | GenesisGest.Net - Provisionamento e desprovisionamento de tenant | 0 |
| `ProvisoesRecorrenciasWindow.xaml` | GenesisGest.Net - Provisões e recorrências | 0 |
| `RateiosAlocacaoWindow.xaml` | GenesisGest.Net - Rateios e critérios de alocação | 0 |
| `ReaberturaPeriodoWindow.xaml` | GenesisGest.Net - Reabertura controlada de período | 0 |
| `ReconciliacaoWindow.xaml` | GenesisGest.Net - Reconciliação de contagens/valores/hashes | 0 |
| `RepassesTaxasWindow.xaml` | GenesisGest.Net - Repasses, taxas e conciliação | 0 |
| `SegurosApolicesWindow.xaml` | GenesisGest.Net - Seguros e apólices | 0 |
| `TabelaPrecosWindow.xaml` | GenesisGest.Net - Tabela de preços e vigência | 0 |
| `TesourariaWindow.xaml` | GenesisGest.Net - Tesouraria e posição diária | 0 |
| `TresViasWindow.xaml` | GenesisGest.Net - Três vias: pedido-recebimento-fatura | 0 |

## Fiscal (20)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `AnexosFiscaisWindow.xaml` | GenesisGest.Net - Anexos fiscais e documentos | 0 |
| `ApuracaoImpostosWindow.xaml` | GenesisGest.Net - Apuração de impostos | 0 |
| `CancelamentoInutilizacaoWindow.xaml` | GenesisGest.Net - Cancelamento, inutilização e carta de correção | 0 |
| `CertidoesVencimentosWindow.xaml` | GenesisGest.Net - Certidões e vencimentos | 0 |
| `CertificadosDigitaisWindow.xaml` | GenesisGest.Net - Certificados digitais e procurações | 0 |
| `CertificadosDnsWindow.xaml` | GenesisGest.Net - Certificados, DNS e túnel | 0 |
| `CofreSegredosWindow.xaml` | GenesisGest.Net - Cofre lógico de segredos e certificados | 0 |
| `ContingenciaFiscalWindow.xaml` | GenesisGest.Net - Contingência fiscal | 0 |
| `CteMdfeLogisticaWindow.xaml` | GenesisGest.Net - CT-e e MDF-e | 0 |
| `CteMdfeWindow.xaml` | GenesisGest.Net - CT-e e MDF-e | 0 |
| `DocumentosAduaneirosWindow.xaml` | GenesisGest.Net - Documentos aduaneiros e desembaraço | 0 |
| `EmissaoNfeWindow.xaml` | GenesisGest.Net - Emissão NF-e/NFC-e | 0 |
| `EventosEsocialWindow.xaml` | GenesisGest.Net - Eventos eSocial | 0 |
| `ImpostosAliquotasWindow.xaml` | GenesisGest.Net - Cadastro de impostos e alíquotas | 0 |
| `ManifestacaoDestinatarioWindow.xaml` | GenesisGest.Net - Manifestação do destinatário | 0 |
| `MapeamentoCategoriaWindow.xaml` | GenesisGest.Net - Mapeamento de categoria e atributos | 0 |
| `PerfisRegimesWindow.xaml` | GenesisGest.Net - Perfis e regimes tributários | 0 |
| `PickingPackingWindow.xaml` | GenesisGest.Net - Picking, packing e conferência | 0 |
| `RegrasNcmCfopWindow.xaml` | GenesisGest.Net - Regras por NCM/CEST/CFOP/UF | 0 |
| `RoteamentoFiscalWindow.xaml` | GenesisGest.Net - Roteamento fiscal por empresa/operação | 0 |

## Logistica / TMS (21)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `AuditoriaFreteWindow.xaml` | GenesisGest.Net - Auditoria e cobrança de frete | 0 |
| `CadastroEpicoWindow.xaml` | GenesisGest.Net - Cadastro de épicos, programas e entregas | 0 |
| `CatalogoFormulariosWindow.xaml` | GenesisGest.Net - Catálogo de formulários e rotas | 0 |
| `CatalogoServicosWindow.xaml` | GenesisGest.Net - Catálogo de serviços e SLA | 0 |
| `CheckoutEntregaWindow.xaml` | GenesisGest.Net - Checkout e entrega | 0 |
| `CotacaoFreteWindow.xaml` | GenesisGest.Net - Cotação e comparação de frete | 0 |
| `DespachoRotaWindow.xaml` | GenesisGest.Net - Despacho e rota de técnico | 0 |
| `DivergenciaInspecaoWindow.xaml` | GenesisGest.Net - Divergência e inspeção de recebimento | 0 |
| `ExpedicaoVolumesWindow.xaml` | GenesisGest.Net - Expedição, volumes e etiquetas | 0 |
| `OcorrenciasAvariasWindow.xaml` | GenesisGest.Net - Ocorrências, avarias e sinistros | 0 |
| `OndasSeparacaoWindow.xaml` | GenesisGest.Net - Ordens e ondas de separação | 0 |
| `ProvaEntregaWindow.xaml` | GenesisGest.Net - Prova de entrega | 0 |
| `RecebimentoPutawayWindow.xaml` | GenesisGest.Net - Recebimento e putaway | 0 |
| `RecebimentoWindow.xaml` | GenesisGest.Net - Recebimento parcial/final | 0 |
| `RomaneioRotaWindow.xaml` | GenesisGest.Net - Romaneio, carga e rota | 0 |
| `RoteamentoPedidoWindow.xaml` | GenesisGest.Net - Roteamento de pedido e SLA | 0 |
| `SlaEscalonamentoWindow.xaml` | GenesisGest.Net - SLA, prioridade e escalonamento | 0 |
| `TabelasFreteWindow.xaml` | GenesisGest.Net - Tabelas e faixas de frete | 0 |
| `TrackingEventosWindow.xaml` | GenesisGest.Net - Tracking e eventos | 0 |
| `TransportadorasParceirosWindow.xaml` | GenesisGest.Net - Transportadoras e parceiros | 0 |
| `TransportadorasServicosWindow.xaml` | GenesisGest.Net - Transportadoras e serviços | 0 |

## HCM / RH (19)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `AditivosRenovacaoWindow.xaml` | GenesisGest.Net - Aditivos, renovação e encerramento | 0 |
| `AfastamentosFeriasWindow.xaml` | GenesisGest.Net - Afastamentos e férias | 0 |
| `AvaliacaoDesempenhoWindow.xaml` | GenesisGest.Net - Avaliação de desempenho | 0 |
| `CargosSalariosWindow.xaml` | GenesisGest.Net - Cargos, posições e salários | 0 |
| `ColaboradoresAdmissaoWindow.xaml` | GenesisGest.Net - Colaboradores e admissão | 0 |
| `ContratosComerciaisWindow.xaml` | GenesisGest.Net - Contratos comerciais e renovações | 0 |
| `ContratosCompraWindow.xaml` | GenesisGest.Net - Contratos e condições de compra | 0 |
| `ContratosDocumentosWindow.xaml` | GenesisGest.Net - Contratos e documentos | 0 |
| `ContratoServicoWindow.xaml` | GenesisGest.Net - Contrato e ordem de serviço | 0 |
| `ContratosPartesWindow.xaml` | GenesisGest.Net - Contratos e partes | 0 |
| `CurvaAbcWindow.xaml` | GenesisGest.Net - Curva ABC e ponto de pedido | 0 |
| `DependentesBeneficiosWindow.xaml` | GenesisGest.Net - Dependentes e benefícios | 0 |
| `DesligamentoWindow.xaml` | GenesisGest.Net - Desligamento e homologação | 0 |
| `FolhaEventosWindow.xaml` | GenesisGest.Net - Folha e eventos | 0 |
| `ImoveisLocacaoWindow.xaml` | GenesisGest.Net - Imóveis, unidades e contratos de locação | 0 |
| `PlanoCarreiraWindow.xaml` | GenesisGest.Net - Plano de carreira e sucessão | 0 |
| `PontoEscalasWindow.xaml` | GenesisGest.Net - Ponto, escalas e banco de horas | 0 |
| `SaudeOcupacionalWindow.xaml` | GenesisGest.Net - Saúde ocupacional e exames | 0 |
| `TreinamentosWindow.xaml` | GenesisGest.Net - Treinamentos e competências | 0 |

## MES / OPS (26)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `ApontamentoProducaoWindow.xaml` | GenesisGest.Net - Apontamento de operação e produção | 0 |
| `AtivosHierarquiaWindow.xaml` | GenesisGest.Net - Ativos e hierarquia técnica | 0 |
| `BannersVigenciaWindow.xaml` | GenesisGest.Net - Banners, ordem e vigência | 0 |
| `BomEstruturaWindow.xaml` | GenesisGest.Net - BOM/estrutura de produto | 0 |
| `CentrosTrabalhoWindow.xaml` | GenesisGest.Net - Centros de trabalho e capacidade | 0 |
| `CotacaoMapaWindow.xaml` | GenesisGest.Net - Cotação e mapa comparativo | 0 |
| `EstruturaOrganizacionalWindow.xaml` | GenesisGest.Net - Estrutura organizacional e departamentos | 0 |
| `FacilitiesChamadosWindow.xaml` | GenesisGest.Net - Facilities e chamados prediais | 0 |
| `ImobilizadoDepreciacaoWindow.xaml` | GenesisGest.Net - Ativos imobilizados e depreciação | 0 |
| `InspecaoControleWindow.xaml` | GenesisGest.Net - Inspeção e plano de controle | 0 |
| `InventarioAtivosWindow.xaml` | GenesisGest.Net - Inventário de ativos técnicos | 0 |
| `ManutencaoPreventivaWindow.xaml` | GenesisGest.Net - Plano de manutenção preventiva | 0 |
| `MedidoresCalibracaoWindow.xaml` | GenesisGest.Net - Medidores, calibração e peças | 0 |
| `MudancaEngenhariaWindow.xaml` | GenesisGest.Net - Mudança de engenharia | 0 |
| `NaoConformidadeCapaWindow.xaml` | GenesisGest.Net - Não conformidade e CAPA | 0 |
| `OrdemManutencaoWindow.xaml` | GenesisGest.Net - Ordem de manutenção | 0 |
| `OrdemProducaoWindow.xaml` | GenesisGest.Net - Ordem de produção | 0 |
| `ParadasRefugosWindow.xaml` | GenesisGest.Net - Paradas, refugos e retrabalho | 0 |
| `PlanoMestreMrpWindow.xaml` | GenesisGest.Net - Plano mestre e MRP | 0 |
| `ProcessosAcaoWindow.xaml` | GenesisGest.Net - Processos e tipos de ação | 0 |
| `QualidadeDadosWindow.xaml` | GenesisGest.Net - Regra e incidente de qualidade | 0 |
| `RecursosTimesheetWindow.xaml` | GenesisGest.Net - Recursos, capacidade e timesheet | 0 |
| `RegistroDefeitoWindow.xaml` | GenesisGest.Net - Registro de defeitos e não conformidades | 0 |
| `RequisicaoMaterialWindow.xaml` | GenesisGest.Net - Requisição e consumo de material | 0 |
| `RiscoCorporativoWindow.xaml` | GenesisGest.Net - Risco corporativo e plano de tratamento | 0 |
| `RoteirosOperacoesWindow.xaml` | GenesisGest.Net - Roteiros e operações | 0 |

## Governanca / Seguranca (37)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `AcoesSociaisWindow.xaml` | GenesisGest.Net - Ações sociais e governança | 0 |
| `AnomaliaInvestigacaoWindow.xaml` | GenesisGest.Net - Anomalia e investigação | 0 |
| `AprovacaoAlcadaWindow.xaml` | GenesisGest.Net - Aprovação por alçada | 0 |
| `BaseJuridicaWindow.xaml` | GenesisGest.Net - Base de conhecimento jurídico | 0 |
| `ClausulasObrigacoesWindow.xaml` | GenesisGest.Net - Cláusulas, obrigações e garantias | 0 |
| `ConsentimentosCrmWindow.xaml` | GenesisGest.Net - Consentimentos e preferências | 0 |
| `ConsentimentoWindow.xaml` | GenesisGest.Net - Consentimento e preferência | 0 |
| `ContinuidadeDesastreWindow.xaml` | GenesisGest.Net - Plano de continuidade e desastre | 0 |
| `DesativacaoWindow.xaml` | GenesisGest.Net - Desativação e arquivamento | 0 |
| `EliminacoesWindow.xaml` | GenesisGest.Net - Eliminações e ajustes de consolidação | 0 |
| `EndpointsCredenciaisWindow.xaml` | GenesisGest.Net - Endpoints, credenciais e webhooks | 0 |
| `EventoAuditoriaWindow.xaml` | GenesisGest.Net - Evento de auditoria e investigação | 0 |
| `GestaoTenantsWindow.xaml` | GenesisGest.Net - Gestão de tenants, empresas e unidades | 0 |
| `GestaoUsuariosWindow.xaml` | GenesisGest.Net - Gestão de usuários | 0 |
| `IncidenteCausaWindow.xaml` | GenesisGest.Net - Incidente, problema e causa raiz | 0 |
| `IncidenteSegurancaWindow.xaml` | GenesisGest.Net - Incidente de segurança e privacidade | 0 |
| `ItensJustificativasWindow.xaml` | GenesisGest.Net - Itens e justificativas | 0 |
| `MatrizSegregacaoWindow.xaml` | GenesisGest.Net - Matriz de segregação de funções | 0 |
| `MfaDispositivosWindow.xaml` | GenesisGest.Net - MFA, dispositivos confiáveis e recuperação | 0 |
| `PerfilLojaIdentidadeWindow.xaml` | GenesisGest.Net - Perfil de loja, domínio e identidade | 0 |
| `PerfisPermissoesWindow.xaml` | GenesisGest.Net - Perfis, permissões e matriz RBAC | 0 |
| `PlanoAuditoriaWindow.xaml` | GenesisGest.Net - Plano e universo de auditoria | 0 |
| `PoliticaIaWindow.xaml` | GenesisGest.Net - Política de IA, prompt e ferramenta | 0 |
| `PoliticaNormaWindow.xaml` | GenesisGest.Net - Política, norma e procedimento | 0 |
| `RegistroEvidenciaWindow.xaml` | GenesisGest.Net - Registro de evidências e homologação | 0 |
| `RegistroRiscoWindow.xaml` | GenesisGest.Net - Registro de risco, impedimento e dependência | 0 |
| `RevisaoAcessosWindow.xaml` | GenesisGest.Net - Revisão periódica de acessos | 0 |
| `RiscosIssuesWindow.xaml` | GenesisGest.Net - Riscos, issues e mudanças | 0 |
| `RiscosJuridicosWindow.xaml` | GenesisGest.Net - Riscos e controles jurídicos | 0 |
| `RiscoTerceiroWindow.xaml` | GenesisGest.Net - Avaliação de risco de terceiro | 0 |
| `RollbackIncidenteWindow.xaml` | GenesisGest.Net - Rollback e incidente | 0 |
| `SegurancaEpiWindow.xaml` | GenesisGest.Net - Segurança, incidentes e EPIs | 0 |
| `SessoesTokensWindow.xaml` | GenesisGest.Net - Sessões, refresh tokens e revogação | 0 |
| `SolicitacaoTitularWindow.xaml` | GenesisGest.Net - Solicitação do titular | 0 |
| `TratamentoDadosWindow.xaml` | GenesisGest.Net - Registro de tratamento de dados | 0 |
| `VulnerabilidadeWindow.xaml` | GenesisGest.Net - Vulnerabilidade e remediação | 0 |
| `WebhooksIdempotenciaWindow.xaml` | GenesisGest.Net - Webhooks e idempotência | 0 |

## Integracoes / Infraestrutura (17)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `BackupRestoreWindow.xaml` | GenesisGest.Net - Backup, restore e teste | 0 |
| `CanaisCredenciaisWindow.xaml` | GenesisGest.Net - Canais, sellers e credenciais | 0 |
| `CatalogoApisWindow.xaml` | GenesisGest.Net - Catálogo de APIs e integrações | 0 |
| `ContasServicoWindow.xaml` | GenesisGest.Net - Contas de serviço e clientes OAuth/API | 0 |
| `EquivalenciaSemanticaWindow.xaml` | GenesisGest.Net - Equivalência e conflito semântico | 0 |
| `EstoquePrecoCanalWindow.xaml` | GenesisGest.Net - Estoque e preço por canal | 0 |
| `ExecucaoIaWindow.xaml` | GenesisGest.Net - Registro de execução/custo/modelo | 0 |
| `FilaDeadletterWindow.xaml` | GenesisGest.Net - Fila, mensagem e dead-letter | 0 |
| `FilaSincronizacaoWindow.xaml` | GenesisGest.Net - Fila e erro de sincronização | 0 |
| `ImportExportWindow.xaml` | GenesisGest.Net - Importação/exportação e Incoterms | 0 |
| `LinhagemConsumidorWindow.xaml` | GenesisGest.Net - Linhagem e consumidor | 0 |
| `LinhagemDadosWindow.xaml` | GenesisGest.Net - Linhagem tabela-endpoint-formulário-relatório | 0 |
| `ModeloAnaliticoWindow.xaml` | GenesisGest.Net - Modelo analítico e versão | 0 |
| `ModeloCanonicoWindow.xaml` | GenesisGest.Net - Decisão de modelo canônico | 0 |
| `PrecoLojaClienteWindow.xaml` | GenesisGest.Net - Preço por loja/canal/cliente | 0 |
| `PrecosLojaCanalWindow.xaml` | GenesisGest.Net - Preços por loja e canal | 0 |
| `RegrasTransformacaoWindow.xaml` | GenesisGest.Net - Regras de transformação | 0 |

## Compras (8)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `AvaliacaoFornecedorWindow.xaml` | GenesisGest.Net - Avaliação e homologação de fornecedor | 0 |
| `DevolucaoFornecedorWindow.xaml` | GenesisGest.Net - Devolução ao fornecedor | 0 |
| `FornecedoresContatosWindow.xaml` | GenesisGest.Net - Fornecedores e contatos | 0 |
| `FornecedoresDropWindow.xaml` | GenesisGest.Net - Fornecedores dropshipping e cobertura | 0 |
| `OportunidadeCotacaoWindow.xaml` | GenesisGest.Net - Oportunidade e cotação comercial | 0 |
| `PedidoCompraWindow.xaml` | GenesisGest.Net - Pedido de compra | 0 |
| `RfqFornecedoresWindow.xaml` | GenesisGest.Net - RFQ e fornecedores convidados | 0 |
| `SolicitacaoCompraWindow.xaml` | GenesisGest.Net - Solicitação de compra | 0 |

## Estoque / WMS (11 pendentes)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `DepositosCustosWindow.xaml` | GenesisGest.Net - Depósitos e custos judiciais | 0 |
| `DepositosEnderecosWindow.xaml` | GenesisGest.Net - Depósitos e endereços | 0 |
| `InventarioContagensWindow.xaml` | GenesisGest.Net - Inventário e contagens | 0 |
| `InventarioDbWindow.xaml` | GenesisGest.Net - Inventário de objeto, coluna, chave e índice | 0 |
| `KardexRastreabilidadeWindow.xaml` | GenesisGest.Net - Kardex e rastreabilidade | 0 |
| `LotePagamentosWindow.xaml` | GenesisGest.Net - Lote de pagamentos e aprovações | 0 |
| `LotesSeriaisWindow.xaml` | GenesisGest.Net - Lotes, seriais e validade | 0 |
| `MovimentacaoAjusteWindow.xaml` | GenesisGest.Net - Movimentação e ajuste | 0 |
| `PlanoMigracaoWindow.xaml` | GenesisGest.Net - Plano e lote de migração | 0 |
| `TransferenciaEstoqueWindow.xaml` | GenesisGest.Net - Transferência entre locais/filiais | 0 |
| `TransferenciaHumanoWindow.xaml` | GenesisGest.Net - Transferência para atendimento humano | 0 |

## Operacao / PDV (9)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `CarrinhoPreferenciasWindow.xaml` | GenesisGest.Net - Carrinho e preferências | 0 |
| `ContaPedidosWindow.xaml` | GenesisGest.Net - Conta, pedidos e endereços do cliente | 0 |
| `DevolucaoTrocaWindow.xaml` | GenesisGest.Net - Devolução, troca e reembolso | 0 |
| `DevolucoesDisputasWindow.xaml` | GenesisGest.Net - Devoluções e disputas | 0 |
| `PdvFechamentoWindow.xaml` | GenesisGest.Net - PDV e fechamento de venda | 0 |
| `PedidosImportadosWindow.xaml` | GenesisGest.Net - Pedidos importados e eventos | 0 |
| `PedidoVendaWindow.xaml` | GenesisGest.Net - Pedido de venda | 0 |
| `RecuperacaoCarrinhoWindow.xaml` | GenesisGest.Net - Recuperação de carrinho | 0 |
| `TrocaDevolucaoWindow.xaml` | GenesisGest.Net - Troca, devolução e cancelamento | 0 |

## Retaguarda / CRM / Portal (24)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `AndamentosPrazosWindow.xaml` | GenesisGest.Net - Andamentos e prazos | 0 |
| `AnunciosVariacoesWindow.xaml` | GenesisGest.Net - Anúncios e variações | 0 |
| `AssinaturaRelatorioWindow.xaml` | GenesisGest.Net - Assinatura e distribuição de relatório | 0 |
| `AtividadesAgendaWindow.xaml` | GenesisGest.Net - Atividades, tarefas e agenda | 0 |
| `BaseConhecimentoWindow.xaml` | GenesisGest.Net - Base de conhecimento | 0 |
| `BibliotecaMidiaWindow.xaml` | GenesisGest.Net - Biblioteca de mídia | 0 |
| `CatalogoDadosWindow.xaml` | GenesisGest.Net - Catálogo e glossário de dados | 0 |
| `CatalogoVinculoWindow.xaml` | GenesisGest.Net - Catálogo e vínculo produto-loja | 0 |
| `DashboardWidgetWindow.xaml` | GenesisGest.Net - Dashboard, widget e layout | 0 |
| `HistoricoConversaWindow.xaml` | GenesisGest.Net - Histórico consentido da conversa | 0 |
| `IndicadoresEsgWindow.xaml` | GenesisGest.Net - Indicadores ESG | 0 |
| `JobsAgendasWindow.xaml` | GenesisGest.Net - Jobs, agendas e execuções | 0 |
| `KpiMetaWindow.xaml` | GenesisGest.Net - Definição de KPI e meta | 0 |
| `LojasDominiosWindow.xaml` | GenesisGest.Net - Lojas, perfis públicos e domínios | 0 |
| `MenusPaginasWindow.xaml` | GenesisGest.Net - Menus, páginas e conteúdo | 0 |
| `MetasTerritoriosWindow.xaml` | GenesisGest.Net - Metas, territórios e carteira | 0 |
| `PainelPortfolioWindow.xaml` | GenesisGest.Net - Painel do portfólio e roadmap | 0 |
| `PesquisaSatisfacaoWindow.xaml` | GenesisGest.Net - Pesquisa de satisfação/NPS/CSAT | 0 |
| `PortfolioProjetoWindow.xaml` | GenesisGest.Net - Portfólio e projeto | 0 |
| `RelatorioParametrosWindow.xaml` | GenesisGest.Net - Relatório e parâmetros | 0 |
| `SeoMetadadosWindow.xaml` | GenesisGest.Net - SEO, metadados e redirecionamentos | 0 |
| `TicketsFilasWindow.xaml` | GenesisGest.Net - Tickets e filas de atendimento | 0 |
| `VisitaCampoWindow.xaml` | GenesisGest.Net - Visita de campo e checklist | 0 |
| `WbsCronogramaWindow.xaml` | GenesisGest.Net - WBS, cronograma e marcos | 0 |

## Cadastros (10)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `CategoriasMarcasWindow.xaml` | GenesisGest.Net - Categorias, marcas e unidades de medida | 0 |
| `CentrosCustoLucroWindow.xaml` | GenesisGest.Net - Centros de custo e lucro | 0 |
| `ClientesEnderecosWindow.xaml` | GenesisGest.Net - Clientes e endereços | 0 |
| `ContasOportunidadesWindow.xaml` | GenesisGest.Net - Contas, contatos e oportunidades | 0 |
| `EmpresasFiliaisWindow.xaml` | GenesisGest.Net - Empresas, filiais e estabelecimentos | 0 |
| `ItensServicosKitsWindow.xaml` | GenesisGest.Net - Itens, serviços e kits | 0 |
| `NfseServicosWindow.xaml` | GenesisGest.Net - NFS-e e serviços | 0 |
| `ProdutosVariacoesWindow.xaml` | GenesisGest.Net - Produtos, serviços e variações | 0 |
| `SaudeServicosWindow.xaml` | GenesisGest.Net - Saúde de serviços e dependências | 0 |
| `UnidadesDepartamentosWindow.xaml` | GenesisGest.Net - Unidades de negócio, centros e departamentos | 0 |

## Sistema / Administracao (17)

| Pagina | Titulo XAML | Referencias C# |
|---|---|---:|
| `AcessibilidadeWindow.xaml` | GenesisGest.Net - Acessibilidade e atalhos | 0 |
| `ConfigLayoutWindow.xaml` | GenesisGest.Net - Configuração de layout/tema autorizada | 0 |
| `ConfigYaraWindow.xaml` | GenesisGest.Net - Configuração Yara, modelo e orçamento | 0 |
| `ControleTarefasWindow.xaml` | GenesisGest.Net - Controle de tarefa-pai, ramificações e gates | 0 |
| `ControleTesteWindow.xaml` | GenesisGest.Net - Controle, teste e evidência | 0 |
| `CutoverWindow.xaml` | GenesisGest.Net - Cutover e checklist | 0 |
| `FeatureFlagsWindow.xaml` | GenesisGest.Net - Feature flags e configuração | 0 |
| `LocalizacaoIdiomaWindow.xaml` | GenesisGest.Net - Localização, idioma e formato regional | 0 |
| `MudancaJanelaWindow.xaml` | GenesisGest.Net - Mudança e janela de implantação | 0 |
| `PainelContingenciaWindow.xaml` | GenesisGest.Net - Painel de contingência e reprocessamento | 0 |
| `PlanoReleaseWindow.xaml` | GenesisGest.Net - Plano de release, cutover e rollback | 0 |
| `PropostaVersaoWindow.xaml` | GenesisGest.Net - Proposta e versão | 0 |
| `ReleaseDeploymentWindow.xaml` | GenesisGest.Net - Release, deployment e rollback | 0 |
| `ReservaAlocacaoWindow.xaml` | GenesisGest.Net - Reserva e alocação | 0 |
| `ReservasRecursosWindow.xaml` | GenesisGest.Net - Reservas de salas/recursos | 0 |
| `SolicitacaoMudancaWindow.xaml` | GenesisGest.Net - Solicitação de mudança e decisão arquitetural | 0 |
| `TesteDpiWindow.xaml` | GenesisGest.Net - Teste de resolução/DPI | 0 |

## Sequencia de implementacao

1. Priorizar as janelas canonicas da arvore e do checklist WPFDB: operacao/PDV, cadastros, compras, financeiro/contabilidade, fiscal e logistica.
2. Confirmar a navegacao no menu e correspondencia com uma janela canonica antes de criar code-behind. Reaproveitar o modulo funcional quando for duplicata e registrar a equivalencia.
3. Implementar handlers somente com contrato confirmado: leitura, validacao, comando, erro HTTP e releitura persistida. Quando faltar endpoint, permissao ou regra, manter bloqueado e registrar a pendencia.
4. Recompilar depois de lotes pequenos. Desktop segue sem publicacao ate build, pacote, instalacao e login passarem.

## Atualizacao 2026-10-30 — 1.1.5.7255

- `SaldosProdutoWindow.xaml` foi reaproveitada como tela canônica do menu Estoque > Saldo físico. O formulário de gravação em memória foi substituído por uma consulta somente leitura ao contrato real `GET /api/erp/estoque-fisico`, com sessão JWT, pesquisa e colunas física, reservada e disponível.
- Contagem reconciliada naquele ponto: 293 janelas XAML, 39 com code-behind e 254 sem code-behind. A tela de saldo físico deixou a classificação de pendentes. O build WPF caiu de 1.020 para 1.016 erros (2 avisos permanecem); as 254 páginas restantes ainda têm eventos sem implementação.
- Próximo lote continua priorizado pelo menu e contratos comprovados. Saldo fiscal, inventário e transferências não recebem handlers até haver contratos transacionais e política multiempresa verificáveis.

## Atualizacao 2026-10-30 — 1.1.5.7258

- A janela `CodigoBarrasQrWindow` agora lê metadados de paginação da resposta autenticada e oferece navegação anterior/próxima, pesquisa reiniciada na página 1 e preservação da seleção ao recarregar a página atual.
- A listagem usa 100 itens por página; o total e a página vêm dos metadados da API, sem baixar o catálogo inteiro.
- A contagem de code-behinds aumentou para 40, mantendo 254 XAMLs sem code-behind. As 4 referências de evento de cada página continuam como bloqueio explícito; não foram trocadas por handlers sem persistência.
