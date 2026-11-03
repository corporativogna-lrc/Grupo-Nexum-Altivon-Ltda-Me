/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Sistema de gestão: GenesisGest.Net (Grupo Nexum Altivon)
 * Módulo: Contratos de Comunicação e Modelos de Dados da API Desktop
 * Versão: 1.1.5.7259 | Data: 30/10/2026
 */

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NexumAltivon.Desktop.Models;

// --- Perfis e Autenticação ---
// TerminalProfile vive em Models/TerminalProfile.cs (classe de runtime com 12 campos).
// O stub antigo de 5 campos foi removido para evitar CS0101.
public sealed record DesktopLoginRequest(string Email, string Senha, string? MfaCode);
public sealed record DesktopLoginResponse(string Token, DateTime ExpiraEm, DesktopUsuario Usuario);
public sealed record DesktopUsuario(int Id, string Nome, string Email, string Perfil);
public sealed record DesktopLoginResult(bool Sucesso, string? Token, DesktopUsuario? Usuario, string Mensagem)
{
    public bool Success => Sucesso;
    public DesktopUsuario? User => Usuario;
    public string Detail => Mensagem;
}

// --- Infraestrutura e Envelopes ---
public sealed record DesktopApiEnvelope<T>(
    bool Sucesso,
    string Mensagem,
    T? Dados,
    int? TotalRegistros = null,
    int? PaginaAtual = null,
    int? TotalPaginas = null);
public sealed record DesktopApiDataResult<T>(
    bool Sucesso,
    T? Dados,
    string Mensagem,
    int? TotalRegistros = null,
    int? PaginaAtual = null,
    int? TotalPaginas = null)
{
    public bool Success => Sucesso;
    public T? Data => Dados;
    public string Detail => Mensagem;
}

public sealed record DesktopPdvCatalogProduct(
    string Id,
    string Nome,
    decimal Preco,
    decimal? PrecoPromocional,
    int Estoque,
    string Sku,
    decimal Custo,
    string? CodigoBarras = null,
    string? QrCode = null)
{
    public string Rotulo => $"{Sku} | {Nome} | R$ {PrecoAtual:N2} | estoque {Estoque}";
    public decimal PrecoAtual => PrecoPromocional ?? Preco;
}

public sealed record DesktopHealthResult(string Status, string Detalhe, DateTime VerificadoEm, bool LocalHealthy, bool PublicHealthy)
{
    public string Detail => Detalhe;
    public DateTime CheckedAt => VerificadoEm;
}

public sealed record DesktopGenesisSaldoEstoque(
    string Sku,
    string Produto,
    decimal QuantidadeFisica,
    decimal QuantidadeReservada,
    decimal QuantidadeDisponivel);

public sealed record DesktopGenesisMovimentacaoEstoque(
    int Id,
    string Sku,
    string Produto,
    string Tipo,
    DateTime DataMovimentacao,
    decimal Quantidade,
    decimal? CustoUnitario,
    decimal? CustoTotal,
    int? DepositoOrigemId,
    int? DepositoDestinoId,
    string? Lote,
    string? Serie,
    string? DocumentoTipo,
    int? DocumentoId,
    string? Observacoes);

public sealed record DesktopProdutoIdentificador(
    int Id,
    int LojaId,
    string Nome,
    string Sku,
    string? CodigoBarras,
    string? QrCode,
    bool Ativo,
    string RowVersion);

public sealed record DesktopProdutoIdentificadorUpdateRequest(
    string? CodigoBarras,
    string? QrCode,
    string RowVersion);

// --- Gestão de Tenants e Fluxos ---
public sealed record DesktopTenant(
    Guid Id,
    string Codigo,
    string Nome,
    string? Documento,
    bool Ativo,
    DateTime CriadoEm,
    DateTime AtualizadoEm);
public sealed record DesktopTenantRequest(string Codigo, string Nome, string? Documento, bool Ativo);

public sealed record DesktopWorkflowDefinition(
    string Id,
    string Entidade,
    string Codigo,
    string Nome,
    string EstadosJson,
    string TransicoesJson,
    bool Ativo,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record DesktopWorkflowDefinitionRequest(
    string Entidade,
    string Codigo,
    string Nome,
    List<string> Estados,
    List<DesktopWorkflowTransitionRule> Transicoes,
    bool Ativo);

public sealed record DesktopWorkflowTransitionRule(
    string Origem,
    string Destino,
    string Acao,
    List<string>? PerfisAutorizados);

public sealed record DesktopWorkflowInstance(
    string Id,
    string DefinicaoId,
    string Entidade,
    string RegistroChave,
    string EstadoAtual,
    int? SolicitanteUserId,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record DesktopWorkflowInstanceRequest(
    Guid DefinicaoId,
    string Entidade,
    string RegistroChave,
    string? EstadoInicial,
    string? Observacao);

public sealed record DesktopWorkflowTransition(
    string Id,
    string InstanciaId,
    string EstadoOrigem,
    string EstadoDestino,
    string Acao,
    int? UsuarioId,
    DateTime CriadoEm);

public sealed record DesktopWorkflowTransitionRequest(
    string EstadoDestino,
    string? Acao,
    string? Observacao);

public sealed record DesktopPdvSaleRequest(
    string? Numero,
    string? EmpresaCodigo,
    int? EmpresaNexumId,
    string ClienteNome,
    string? ClienteDocumento,
    string? ClienteEmail,
    int? ClienteNexumId,
    string? Terminal,
    string? CaixaCodigo,
    string? Operador,
    decimal Desconto,
    decimal Frete,
    string? Observacoes,
    List<DesktopPdvSaleItemRequest> Itens,
    List<DesktopPdvPaymentRequest> Pagamentos);

public sealed record DesktopPdvSaleItemRequest(
    int? ProdutoNexumId,
    string? ProdutoCodigo,
    string? Sku,
    string Descricao,
    decimal Quantidade,
    decimal PrecoUnitario,
    decimal CustoUnitario,
    decimal Desconto,
    string? OrigemAquisicao);

public sealed record DesktopPdvPaymentRequest(
    string Forma,
    decimal Valor,
    int Parcelas,
    string? Autorizacao,
    string? Nsu,
    string? Bandeira);

public sealed record DesktopPdvSaleResult(
    int Id,
    string Numero,
    string? Status,
    string StatusSincronizacao,
    decimal Total,
    decimal ValorPago);

// --- Módulo de Compras ---
public sealed record DesktopComprasPainel(
    DesktopComprasKpi Kpis,
    List<string> Alertas,
    List<DesktopCompraSolicitacao> Solicitacoes,
    List<DesktopCompraCotacao> Cotacoes,
    List<DesktopCompraPedido> Pedidos,
    List<DesktopCompraEntrada> Entradas,
    List<DesktopCompraProdutoReposicao> ProdutosReposicao,
    List<DesktopCompraFornecedor> Fornecedores);

public sealed record DesktopCompraSolicitacaoRequest(
    int? ProdutoId,
    string? ProdutoNome,
    int Quantidade,
    string? Origem,
    string? Finalidade,
    string? Prioridade,
    string? Observacoes);

public sealed record DesktopCompraCotacaoRequest(
    int FornecedorId,
    int? SolicitacaoId,
    int? ProdutoId,
    string? ProdutoNome,
    int Quantidade,
    decimal CustoUnitario,
    string? Origem,
    string? Finalidade,
    string? Prioridade,
    int? PrazoEntregaDias,
    string? Observacoes);

public sealed record DesktopCompraPedidoRequest(
    int FornecedorId,
    int? SolicitacaoId,
    string? Origem,
    string? Finalidade,
    DateTime? DataPrevistaEntrega,
    DateTime? DataVencimento,
    string? MeioPagamento,
    string? Observacoes,
    List<DesktopCompraPedidoItemRequest> Itens);

public sealed record DesktopCompraEntradaRequest(
    string? NumeroDocumento,
    string? ChaveNfeEntrada,
    string? TipoEntrada,
    string? RecebidoPor,
    string? Observacoes,
    List<DesktopCompraEntradaItemRequest>? Itens);

public sealed record DesktopCompraStatusRequest(
    [property: JsonPropertyName("novo_status")] string NovoStatus,
    [property: JsonPropertyName("observacoes")] string? Observacoes);

public sealed record DesktopItemCompra(int ItemId, decimal Quantidade, decimal PrecoEstimado);
public sealed record DesktopCotacaoItem(int ItemId, decimal PrecoUnitario, int FornecedorId);
public sealed record DesktopEntradaItem(int ItemId, decimal QuantidadeRecebida, decimal CustoUnitario);

// --- Cadastro de Fornecedores ---
// DesktopFornecedor/Request ricos vivem em Models/SupplierModels.cs.
// Os stubs antigos de 6 campos foram removidos para evitar CS0101.

// --- Categorias e Centros de Custo ---
public sealed record DesktopCategoria(
    string Id,
    string Nome,
    string Descricao,
    string? CategoriaPaiId,
    int Nivel,
    string? Caminho,
    int? Ordem,
    bool Ativa);
public sealed record DesktopCategoriaRequest(
    string Id,
    string Nome,
    string Descricao,
    string? CategoriaPaiId = null,
    int Nivel = 1,
    string? Caminho = null,
    int? Ordem = null,
    bool Ativa = true);

public sealed record DesktopCentroCusto(
    int Id,
    string Codigo,
    string Nome,
    string? Descricao,
    string Tipo,
    int? PaiId,
    int? ResponsavelUsuarioId,
    string Status,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);
public sealed record DesktopCentroCustoRequest(
    string Codigo,
    string Nome,
    string? Descricao,
    string? Observacoes,
    string? Tipo,
    int? PaiId,
    int? ResponsavelUsuarioId,
    string? Status);

// --- Itens e Serviços ---
public sealed record DesktopItemServico(
    int Id,
    int EmpresaId,
    string Codigo,
    string Tipo,
    string Descricao,
    string? DescricaoDetalhada,
    string Unidade,
    string? Ncm,
    string? Cest,
    bool ControlaEstoque,
    bool ControlaLote,
    bool ControlaSerie,
    bool Ativo,
    DateTime CriadoEm);
public sealed record DesktopItemServicoRequest(
    int EmpresaId,
    string Codigo,
    string Tipo,
    string Descricao,
    string? DescricaoDetalhada,
    string? Unidade,
    string? Ncm,
    string? Cest,
    decimal? PesoBruto,
    decimal? PesoLiquido,
    decimal? Altura,
    decimal? Largura,
    decimal? Profundidade,
    bool ControlaEstoque,
    bool ControlaLote,
    bool ControlaSerie,
    bool Ativo);

public sealed record DesktopFiscalRoutingSimulationRequest(
    string TipoOperacao,
    decimal ValorProdutos,
    decimal ValorFrete,
    string EstadoOrigem,
    string EstadoDestino,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    string? NaturezaOperacao,
    bool ExigeMarketplace,
    bool ExigeDropshipping,
    bool RequerSaidaNfe,
    bool RequerEntradaNfe);

public sealed record DesktopFiscalRoutingSimulation(
    bool Sucesso,
    string Resumo,
    string? CodigoEmpresaSelecionada,
    string? RazaoSocialSelecionada,
    string? CnpjSelecionado,
    string? EstadoSelecionado,
    List<DesktopFiscalRoutingRanking> Ranking);

public sealed record DesktopFiscalRoutingRanking(
    string CodigoEmpresa,
    string RazaoSocial,
    string Cnpj,
    string? RegimeTributario,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    decimal CustoTributarioEstimado,
    decimal CustoOperacionalEstimado,
    decimal CustoTotalEstimado,
    decimal LucroEstimado,
    decimal MargemEstimadaPercentual,
    decimal Score,
    List<string> Justificativas);

// --- Gestão de Acesso (IAM) ---
// Contratos espelhados do backend (Program.cs linhas 22961+):
//   UsuarioAcessoDto / UsuarioAcessoUpsertRequest
//   PerfilAcessoDto  / PerfilAcessoUpsertRequest
//   PermissaoAcessoDto / PermissaoAcessoUpsertRequest
//   PerfilPermissaoDto / PerfilPermissaoUpsertRequest
public sealed record DesktopUsuarioAcesso(
    int Id,
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    string? Telefone,
    DateTime? UltimoLogin,
    DateTime UpdatedAt);

public sealed record DesktopUsuarioAcessoRequest(
    string Nome,
    string Email,
    string Perfil,
    bool Ativo,
    string? Telefone,
    string? Senha);

public sealed record DesktopPerfilAcesso(
    int Id,
    string Nome,
    string? Descricao,
    decimal AlcadaMaxima,
    int NivelHierarquico,
    bool Ativo,
    DateTime CriadoEm);

public sealed record DesktopPerfilAcessoRequest(
    string Nome,
    string? Descricao,
    decimal AlcadaMaxima,
    int NivelHierarquico,
    bool Ativo);

public sealed record DesktopPermissaoAcesso(
    int Id,
    string Modulo,
    string Funcionalidade,
    string Chave,
    string? Descricao,
    bool Ativo);

public sealed record DesktopPermissaoAcessoRequest(
    string Modulo,
    string Funcionalidade,
    string Chave,
    string? Descricao,
    bool Ativo);

public sealed record DesktopPerfilPermissao(
    int Id,
    int PerfilId,
    int PermissaoId,
    string Modulo,
    string Funcionalidade,
    string Chave,
    bool Leitura,
    bool Escrita,
    bool Exclusao,
    bool Impressao);

public sealed record DesktopPerfilPermissaoRequest(
    int PermissaoId,
    bool Leitura,
    bool Escrita,
    bool Exclusao,
    bool Impressao);

// --- Módulo de Compras (Procurement) ---
// Contratos espelhados do backend (Program.cs ~linha 23858+):
//   CompraSolicitacaoDto/Request, CompraCotacaoDto/Request,
//   CompraPedidoResumoDto/Request, CompraEntradaResumoDto/Request,
//   CompraProdutoReposicaoDto, CompraFornecedorResumoDto, ComprasKpiDto
public sealed record DesktopCompraFornecedor(
    int Id,
    string Nome,
    string? Documento,
    string? Segmento,
    int PrazoEntregaDias,
    int PrazoPagamentoDias);

public sealed record DesktopCompraProdutoReposicao(
    int ProdutoId,
    string ProdutoNome,
    string Sku,
    int EstoqueAtual,
    int EstoqueMinimo,
    string TipoProduto,
    int? FornecedorId,
    decimal CustoAtual);

public sealed record DesktopCompraSolicitacao(
    int Id,
    int? ProdutoId,
    string ProdutoNome,
    int Quantidade,
    string Finalidade,
    string Origem,
    string Status,
    string Prioridade,
    DateTime CreatedAt);

public sealed record DesktopCompraSolicitacaoRequestV2(
    int? ProdutoId,
    string? ProdutoNome,
    int Quantidade,
    string? Origem,
    string? Finalidade,
    string? Prioridade,
    string? Observacoes);

public sealed record DesktopCompraCotacao(
    int Id,
    int SolicitacaoId,
    int FornecedorId,
    string FornecedorNome,
    int? ProdutoId,
    string ProdutoNome,
    int Quantidade,
    decimal CustoUnitario,
    decimal ValorTotal,
    int PrazoEntregaDias,
    string Origem,
    string Status,
    DateTime CreatedAt);

public sealed record DesktopCompraCotacaoRequestV2(
    int FornecedorId,
    int? SolicitacaoId,
    int? ProdutoId,
    string? ProdutoNome,
    int Quantidade,
    decimal CustoUnitario,
    string? Origem,
    string? Finalidade,
    string? Prioridade,
    int? PrazoEntregaDias,
    string? Observacoes);

public sealed record DesktopCompraPedidoItem(
    int Id,
    int? ProdutoId,
    string ProdutoNome,
    string? Sku,
    int Quantidade,
    int QuantidadeRecebida,
    int QuantidadePendente,
    decimal CustoUnitario,
    decimal ValorTotal,
    string Origem,
    string Finalidade);

public sealed record DesktopCompraPedido(
    int Id,
    string Numero,
    int FornecedorId,
    string FornecedorNome,
    string Origem,
    string Finalidade,
    string Status,
    string StatusFiscal,
    decimal ValorTotal,
    DateTime? DataPrevistaEntrega,
    DateTime CreatedAt,
    List<DesktopCompraPedidoItem> Itens);

public sealed record DesktopCompraPedidoItemRequest(
    int? ProdutoId,
    string? ProdutoNome,
    string? Sku,
    int Quantidade,
    decimal CustoUnitario);

public sealed record DesktopCompraPedidoRequestV2(
    int FornecedorId,
    int? SolicitacaoId,
    string? Origem,
    string? Finalidade,
    DateTime? DataPrevistaEntrega,
    DateTime? DataVencimento,
    string? MeioPagamento,
    string? Observacoes,
    List<DesktopCompraPedidoItemRequest> Itens);

public sealed record DesktopCompraEntrada(
    int Id,
    int CompraPedidoId,
    string PedidoNumero,
    string FornecedorNome,
    string? NumeroDocumento,
    string? ChaveNfeEntrada,
    string TipoEntrada,
    string StatusFiscal,
    decimal ValorTotal,
    DateTime CreatedAt);

public sealed record DesktopCompraEntradaRequestV2(
    string? NumeroDocumento,
    string? ChaveNfeEntrada,
    string? TipoEntrada,
    string? RecebidoPor,
    string? Observacoes,
    List<DesktopCompraEntradaItemRequest>? Itens);

public sealed record DesktopCompraEntradaItemRequest(int ItemId, int QuantidadeRecebida);

public sealed record DesktopComprasKpi(
    int SolicitacoesAbertas,
    int PedidosAbertos,
    int EntradasMes,
    decimal ValorComprasAbertas,
    int ContasAPagarCompras,
    int FornecedoresAtivos,
    int ProdutosReposicao);

// --- CRM / Marketing ---
// Contratos espelhados do backend (Program.cs ~linha 23363+):
//   CrmSegmentoDto/Request, CrmCampanhaDto/Request, LeadRequest, LeadLojaDto
public sealed record DesktopCrmSegmento(
    Guid Id,
    string Nome,
    string? Descricao,
    string Cor,
    int Prioridade,
    decimal? TicketMedioMinimo,
    decimal? TicketMedioMaximo,
    int? FrequenciaMinimaDias,
    int? FrequenciaMaximaDias,
    bool Ativo,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DesktopCrmSegmentoRequest(
    string Nome,
    string? Descricao,
    string Cor,
    int Prioridade,
    decimal? TicketMedioMinimo,
    decimal? TicketMedioMaximo,
    int? FrequenciaMinimaDias,
    int? FrequenciaMaximaDias,
    bool Ativo,
    string? RowVersion);

public sealed record DesktopCrmCampanha(
    Guid Id,
    string Nome,
    string? Descricao,
    string Tipo,
    string Status,
    Guid? SegmentoId,
    string? SegmentoNome,
    DateTime DataInicio,
    DateTime? DataFim,
    decimal Orcamento,
    decimal CustoAtual,
    int Alcance,
    int Cliques,
    int LeadsGerados,
    int OportunidadesGeradas,
    int VendasGeradas,
    decimal ReceitaGerada,
    decimal Roas,
    decimal Cpc,
    decimal Cpl,
    decimal Cpa,
    string? PublicoAlvo,
    string? Conteudo,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DesktopCrmCampanhaRequest(
    string Nome,
    string? Descricao,
    string Tipo,
    string Status,
    Guid? SegmentoId,
    DateTime DataInicio,
    DateTime? DataFim,
    decimal Orcamento,
    decimal CustoAtual,
    int Alcance,
    int Cliques,
    int LeadsGerados,
    int OportunidadesGeradas,
    int VendasGeradas,
    decimal ReceitaGerada,
    string? PublicoAlvo,
    string? Conteudo,
    string? RowVersion);

public sealed record DesktopLead(
    int Id,
    string Nome,
    string Email,
    string Telefone,
    string Status,
    DateTime CreatedAt,
    string? Empresa,
    string? Origem,
    string? Mensagem);

public sealed record DesktopLeadRequest(
    string Nome,
    string Email,
    string? Telefone,
    string? Status,
    string? Origem,
    string? Observacao,
    string? Empresa,
    string? Cnpj,
    string? Segmento,
    string? Mensagem,
    string? Whatsapp,
    string? Tipo);
public sealed record DesktopLeadStatusUpdateRequest(
    [property: JsonPropertyName("novo_status")] string NovoStatus,
    [property: JsonPropertyName("responsavel_id")] int? ResponsavelId);

// --- Cupons e Promoções ---
// Contratos espelhados do backend (Program.cs ~linha 23665+):
//   CupomDto, CupomAdminDto, CupomAdminRequest
public sealed record DesktopCupom(
    string Codigo,
    decimal? DescontoPercentual,
    decimal? DescontoValor,
    decimal? ValorMinimo,
    decimal? ValorMaximoDesconto,
    bool FreteGratis);

public sealed record DesktopCupomAdmin(
    int Id,
    string Codigo,
    string Tipo,
    decimal Valor,
    decimal ValorMinimoPedido,
    decimal? ValorMaximoDesconto,
    int? QuantidadeUsos,
    int UsosAtuais,
    int QuantidadePorCliente,
    DateTime? ValidoDe,
    DateTime? ValidoAte,
    bool PrimeiroCompraOnly,
    bool Ativo,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record DesktopCupomRequest(
    string Codigo,
    string Tipo,
    decimal Valor,
    decimal ValorMinimoPedido,
    decimal? ValorMaximoDesconto,
    int? QuantidadeUsos,
    int QuantidadePorCliente,
    DateTime? ValidoDe,
    DateTime? ValidoAte,
    bool PrimeiroCompraOnly,
    bool Ativo);

// --- Dropshipping ---
// Contratos espelhados do backend (Program.cs ~linha 24214+):
//   DropshippingCanalDto, DropshippingCanalRequest
public sealed record DesktopDropshippingCanal(
    int Id,
    string Nome,
    string Slug,
    string Tipo,
    string? ApiEndpoint,
    bool Ativo,
    bool CredenciaisConfiguradas,
    string StatusCredenciais,
    string DetalheCredenciais,
    string RowVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record DesktopDropshippingCanalRequest(
    string Nome,
    string Slug,
    string Tipo,
    string? ApiEndpoint,
    bool Ativo,
    string? RowVersion);

// --- Empresas do Grupo ---
// Contratos espelhados do backend (Program.cs ~linha 24876+):
//   EmpresaGrupoDto, EmpresaGrupoRequest
public sealed record DesktopEmpresaGrupo(
    int Id,
    string TipoCadastro,
    string RazaoSocial,
    string? NomeFantasia,
    string Cnpj,
    string? InscricaoEstadual,
    string? InscricaoMunicipal,
    string? MatrizFilial,
    string? CodigoEmpresa,
    string? RegimeTributario,
    string? Crt,
    string? CnaePrincipal,
    string? CnaesSecundarios,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    string? NcmPadrao,
    string? NaturezaOperacaoPadrao,
    string? ResponsavelLegal,
    string? ResponsavelFiscal,
    string? EmailFiscal,
    string? EmailComercial,
    string? Telefone,
    string? Whatsapp,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    string? Pais,
    string? AmbienteNfe,
    string? SerieNfe,
    string? SerieNfce,
    string? ModeloDocumentoPdv,
    string? AmbienteNfce,
    int? ProximaNfceNumero,
    string? NfceCsc,
    string? NfceCscIdToken,
    string? PdvSerieSat,
    string? PdvImpressoraFiscal,
    string? PdvNomeCaixaPadrao,
    bool PdvContingenciaOffline,
    int? ProximaNfeNumero,
    string? CfopPadraoInterno,
    string? CfopPadraoInterestadual,
    decimal? AliquotaIcmsInterna,
    decimal? AliquotaIcmsInterestadual,
    decimal? AliquotaPis,
    decimal? AliquotaCofins,
    decimal? AliquotaIss,
    decimal? AliquotaIpi,
    decimal? CargaTributariaPercentual,
    string? PerfilTributacao,
    bool UsaStLegado,
    bool DestacaIcmsStSeparado,
    decimal? CustoOperacionalPercentual,
    decimal? MargemMinimaPercentual,
    int PrioridadeFiscal,
    bool PermiteNfeEntrada,
    bool PermiteNfeSaida,
    bool PermiteDropshipping,
    bool PermiteMarketplace,
    bool EmitentePreferencial,
    bool Ativa,
    string? BeneficiosEstrategicos,
    string? ContratoResumo,
    string? Observacoes,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record DesktopEmpresaGrupoRequest(
    string? TipoCadastro,
    string RazaoSocial,
    string? NomeFantasia,
    string Cnpj,
    string? InscricaoEstadual = null,
    string? InscricaoMunicipal = null,
    string? MatrizFilial = null,
    string? CodigoEmpresa = null,
    string? RegimeTributario = null,
    string? Crt = null,
    string? CnaePrincipal = null,
    string? CnaesSecundarios = null,
    string? CategoriaFiscal = null,
    string? SubcategoriaFiscal = null,
    string? NcmPadrao = null,
    string? NaturezaOperacaoPadrao = null,
    string? ResponsavelLegal = null,
    string? ResponsavelFiscal = null,
    string? EmailFiscal = null,
    string? EmailComercial = null,
    string? Telefone = null,
    string? Whatsapp = null,
    string? Cep = null,
    string? Logradouro = null,
    string? Numero = null,
    string? Complemento = null,
    string? Bairro = null,
    string? Cidade = null,
    string? Estado = null,
    string? Pais = null,
    string? AmbienteNfe = null,
    string? SerieNfe = null,
    string? SerieNfce = null,
    string? ModeloDocumentoPdv = null,
    string? AmbienteNfce = null,
    int? ProximaNfceNumero = null,
    string? NfceCsc = null,
    string? NfceCscIdToken = null,
    string? PdvSerieSat = null,
    string? PdvImpressoraFiscal = null,
    string? PdvNomeCaixaPadrao = null,
    bool? PdvContingenciaOffline = null,
    int? ProximaNfeNumero = null,
    string? CfopPadraoInterno = null,
    string? CfopPadraoInterestadual = null,
    decimal? AliquotaIcmsInterna = null,
    decimal? AliquotaIcmsInterestadual = null,
    decimal? AliquotaPis = null,
    decimal? AliquotaCofins = null,
    decimal? AliquotaIss = null,
    decimal? AliquotaIpi = null,
    decimal? CargaTributariaPercentual = null,
    string? PerfilTributacao = null,
    bool? UsaStLegado = null,
    bool? DestacaIcmsStSeparado = null,
    decimal? CustoOperacionalPercentual = null,
    decimal? MargemMinimaPercentual = null,
    int? PrioridadeFiscal = null,
    bool? PermiteNfeEntrada = null,
    bool? PermiteNfeSaida = null,
    bool? PermiteDropshipping = null,
    bool? PermiteMarketplace = null,
    bool? EmitentePreferencial = null,
    bool? Ativa = null,
    string? BeneficiosEstrategicos = null,
    string? ContratoResumo = null,
    string? Observacoes = null);

// --- Fiscal (NF-e manual) ---
// Contratos espelhados do backend (Program.cs ~linha 25113+):
//   FiscalManualEmissaoRequest
//   O tipo OperacaoFiscal eh enum no backend; tratado como string aqui
public sealed record DesktopFiscalManualEmissaoRequest(
    string EmpresaEmissora,
    string CnpjEmissor,
    string ClienteDestinatario,
    string DocumentoDestinatario,
    string NaturezaOperacao,
    string Cfop,
    decimal Subtotal,
    decimal Frete,
    decimal ImpostosEstimados,
    decimal MargemMinima,
    string? Observacoes,
    string TipoOperacao,
    string EstadoOrigem,
    string EstadoDestino,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    bool ExigeMarketplace,
    bool ExigeDropshipping,
    bool RequerSaidaNfe,
    bool RequerEntradaNfe);

public sealed record DesktopFiscalManualEmissaoResponse(
    bool CertificadoOperacional,
    string CertificadoStatus,
    string? CertificadoReferencia,
    string RoteamentoResumo,
    string? CodigoEmpresaSelecionada,
    string? RazaoSocialSelecionada,
    string? CnpjSelecionado,
    string? EstadoSelecionado,
    List<string> Pendencias,
    DateTime GeradoEm);

public sealed record DesktopFiscalDraftSaveResult(bool Salvo, string? Chave, DateTime SalvoEm);
public sealed record DesktopFiscalDraftResult(bool Existe, string? Valor, DateTime? UpdatedAt);

// --- Logística / TMS ---
// Contratos espelhados do backend (Program.cs ~linha 24640+):
//   LogisticaExpedicaoDto, LogisticaRastreamentoEventoDto
public sealed record DesktopLogisticaRastreamentoEvento(
    DateTime? DataHora,
    string Status,
    string? Local,
    string? Descricao);

public sealed record DesktopLogisticaExpedicao(
    int PedidoId,
    string NumeroPedido,
    string ClienteNome,
    string Status,
    string? FreteMetodo,
    string? FreteTransportadora,
    int FretePrazoDias,
    string? FreteCodigoRastreio,
    DateTime? DataEnvio,
    DateTime? DataEntrega,
    DateTime? PrevisaoEntrega,
    bool UltimoRastreamentoOperacional,
    string? UltimoStatusExterno,
    DateTime? UltimaConsultaEm,
    string RowVersion,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record DesktopPedidoLogisticaRequest(
    [property: JsonPropertyName("frete_metodo")] string? FreteMetodo,
    [property: JsonPropertyName("frete_transportadora")] string? FreteTransportadora,
    [property: JsonPropertyName("frete_prazo_dias")] int? FretePrazoDias,
    [property: JsonPropertyName("frete_codigo_rastreio")] string? FreteCodigoRastreio,
    [property: JsonPropertyName("row_version")] string? RowVersion);

public sealed record DesktopLogisticaRastreamento(
    Guid ConsultaId,
    string CodigoRastreio,
    int PedidoId,
    string NumeroPedido,
    string? Transportadora,
    string? MetodoFrete,
    string StatusInterno,
    DateTime? DataEnvio,
    DateTime? DataEntrega,
    DateTime? PrevisaoEntrega,
    bool RastreamentoExternoConfigurado,
    bool RastreamentoExternoOperacional,
    string FonteExterna,
    int? HttpStatusCode,
    string? StatusExterno,
    List<DesktopLogisticaRastreamentoEvento> Eventos,
    List<string> Pendencias,
    string? RespostaSha256,
    DateTime ConsultadoEm);

public sealed record DesktopPedidoLoja(
    int Id,
    string NumeroPedido,
    decimal Total,
    string Status,
    DateTime CreatedAt,
    string StatusPagamento,
    string? MeioPagamento,
    string? GatewayPagamento,
    string? GatewayTransacaoId,
    decimal FreteValor,
    string? FreteMetodo,
    string? FreteTransportadora,
    int FretePrazoDias,
    string? FreteCodigoRastreio,
    string InstrucaoPagamento,
    int Parcelas,
    string? PixQrcode,
    string? PaymentUrl,
    string? StatusFiscal,
    string? AlertaOperacional);

// --- OpenAI / Assistentes IA ---
// Contratos espelhados do backend (Services/AssistenteIaService.cs ~linha 386+):
//   OpenAiAssistentesStatus, OpenAiAssistenteStatus,
//   OpenAiAssistentesConfiguracaoRequest, AssistenteIaResposta, AssistenteIaMensagem
public sealed record DesktopOpenAiAssistenteStatus(
    string Assistente,
    bool Configurada,
    string Modelo,
    string Origem,
    DateTime? AtualizadoEm);

public sealed record DesktopOpenAiAssistentesStatus(
    DesktopOpenAiAssistenteStatus Yara,
    DesktopOpenAiAssistenteStatus Sophia);

public sealed record DesktopOpenAiAssistentesConfiguracaoRequest(
    string? YaraApiKey,
    string? YaraModel,
    string? SophiaApiKey,
    string? SophiaModel);

public sealed record DesktopAssistenteIaResposta(
    string Assistente,
    string Mensagem,
    string Modelo,
    DateTime RespondidoEm);

public sealed record DesktopAssistenteIaMensagem(string Autor, string Texto);

// --- Operações (Manufatura / PCP) ---
// Contratos espelhados do backend (Program.cs ~linha 22706+):
//   OpsAtivoDto/Request, OpsOrdemServicoDto/Request,
//   OpsOrdemServicoItemRequest/Dto, OpsOrdemServicoDetalheDto,
//   OpsProducaoApontamentoDto/Request, OpsProducaoInsumoRequest
public sealed record DesktopOpsAtivo(
    int Id,
    string Codigo,
    string Nome,
    string Tipo,
    string? Localizacao,
    string Status,
    string? Fabricante,
    string? Modelo,
    string? NumeroSerie,
    DateTime? ProximaManutencao,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record DesktopOpsAtivoRequest(
    string Codigo,
    string Nome,
    string? Tipo,
    string? Localizacao,
    string? Status,
    string? Fabricante,
    string? Modelo,
    string? NumeroSerie,
    DateTime? ProximaManutencao);

public sealed record DesktopOpsOrdemServicoItem(
    int Id,
    string Tipo,
    string? Codigo,
    string Descricao,
    decimal Quantidade,
    string Unidade,
    decimal? CustoUnitario,
    decimal? Total);

public sealed record DesktopOpsOrdemServicoItemRequest(
    string? Tipo,
    string? Codigo,
    string Descricao,
    decimal Quantidade,
    string? Unidade,
    decimal? CustoUnitario);

public sealed record DesktopOpsOrdemServico(
    int Id,
    string Numero,
    int? AtivoId,
    string Titulo,
    string? Descricao,
    string Status,
    string Prioridade,
    int? ResponsavelUserId,
    DateTime DataAbertura,
    DateTime? DataPrevista,
    DateTime? DataConclusao,
    int? TempoEstimadoMinutos,
    int? TempoRealMinutos,
    decimal? CustoPrevisto,
    decimal? CustoReal,
    string? Observacoes,
    DateTime CriadoEm,
    DateTime AtualizadoEm);

public sealed record DesktopOpsOrdemServicoRequest(
    string? Numero,
    int? AtivoId,
    string Titulo,
    string? Descricao,
    string? Status,
    string? Prioridade,
    int? ResponsavelUserId,
    DateTime? DataAbertura,
    DateTime? DataPrevista,
    DateTime? DataConclusao,
    int? TempoEstimadoMinutos,
    int? TempoRealMinutos,
    decimal? CustoPrevisto,
    decimal? CustoReal,
    string? Observacoes,
    List<DesktopOpsOrdemServicoItemRequest>? Itens);

public sealed record DesktopOpsOrdemServicoDetalhe(
    DesktopOpsOrdemServico Ordem,
    List<DesktopOpsOrdemServicoItem> Itens);

public sealed record DesktopOpsProducaoInsumoRequest(
    string? Codigo,
    string Descricao,
    decimal Quantidade,
    string? Unidade);

public sealed record DesktopOpsProducaoApontamento(
    int Id,
    int? OrdemServicoId,
    int? ProdutoId,
    string? ProdutoCodigo,
    string ProdutoNome,
    decimal QuantidadeProduzida,
    decimal QuantidadeRefugo,
    int? TempoMinutos,
    int? OperadorUserId,
    DateTime DataApontamento,
    string? InsumosJson,
    string? Observacoes);

public sealed record DesktopOpsProducaoApontamentoRequest(
    int? OrdemServicoId,
    int? ProdutoId,
    string? ProdutoCodigo,
    string ProdutoNome,
    decimal QuantidadeProduzida,
    decimal QuantidadeRefugo,
    int? TempoMinutos,
    int? OperadorUserId,
    DateTime? DataApontamento,
    List<DesktopOpsProducaoInsumoRequest>? Insumos,
    string? Observacoes);

// --- Site (Portal público) ---
// Contratos espelhados do backend (Program.cs ~linha 24305+ e 24414+):
//   SiteMidiaDto, SiteMidiaUpdateRequest, SiteMidiaUploadRequest,
//   SitePerfilPublicoRequest, SitePerfilPublicoAdminDto,
//   SitePerfilOrigemDto, SitePerfilProdutoDisponivelDto
public sealed record DesktopSiteMidia(
    Guid Id,
    Guid TenantId,
    string Nome,
    string Tipo,
    string TextoAlternativo,
    string NomeArquivoOriginal,
    string CaminhoRelativo,
    string Url,
    string ContentType,
    long TamanhoBytes,
    int Largura,
    int Altura,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DesktopSiteMidiaUpdateRequest(
    string? Nome,
    string? Tipo,
    string? TextoAlternativo,
    string? RowVersion);

public sealed record DesktopSiteMidiaUploadRequest(
    string? Nome,
    string? Tipo,
    string? TextoAlternativo,
    string? FileName,
    string? ContentType,
    string? DataUrl);

public sealed record DesktopSiteProfileOrigin(
    string Tipo,
    int Id,
    string Nome,
    string Slug,
    string Status);

public sealed record DesktopSiteProfileProduct(
    int Id,
    string Sku,
    string Nome,
    string Slug,
    decimal Preco,
    string ImagemUrl,
    int LojaId,
    int? FornecedorId,
    string TipoProduto);

public sealed record DesktopSitePublicProfile(
    Guid Id,
    string TipoPerfil,
    string OrigemTipo,
    int? OrigemId,
    string Nome,
    string Slug,
    string Segmento,
    string Atividade,
    string Descricao,
    string? LogoUrl,
    string? BannerUrl,
    string? Icone,
    string? CtaTexto,
    string? CtaUrl,
    string? SiteUrl,
    string? EmailPublico,
    string? TelefonePublico,
    string? EnderecoPublico,
    string? CorPrimaria,
    string? CorSecundaria,
    string? CorFundo,
    string? CorTexto,
    List<int> ProdutoIds,
    bool Publicado,
    int OrdemExibicao,
    string RowVersion,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public sealed record DesktopSitePublicProfileRequest(
    string TipoPerfil,
    string OrigemTipo,
    int? OrigemId,
    string Nome,
    string Slug,
    string Segmento,
    string Atividade,
    string Descricao,
    string? LogoUrl,
    string? BannerUrl,
    string? Icone,
    string? CtaTexto,
    string? CtaUrl,
    string? SiteUrl,
    string? EmailPublico,
    string? TelefonePublico,
    string? EnderecoPublico,
    string? CorPrimaria,
    string? CorSecundaria,
    string? CorFundo,
    string? CorTexto,
    List<int>? ProdutoIds,
    bool Publicado,
    int OrdemExibicao,
    string? RowVersion = null);
