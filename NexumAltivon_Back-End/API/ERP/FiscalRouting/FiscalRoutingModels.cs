namespace NexumAltivon.API.ERP.FiscalRouting;

public enum TipoOperacaoFiscal
{
    VendaInterna,
    VendaInterestadual,
    Marketplace,
    Dropshipping,
    EntradaCompra,
    Transferencia
}

public sealed record FiscalRoutingRequest(
    TipoOperacaoFiscal TipoOperacao,
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

public sealed record FiscalCompanySnapshot(
    int Id,
    string CodigoEmpresa,
    string RazaoSocial,
    string Cnpj,
    string Estado,
    string? RegimeTributario,
    string? CategoriaFiscal,
    string? SubcategoriaFiscal,
    decimal AliquotaIcmsInterna,
    decimal AliquotaIcmsInterestadual,
    decimal AliquotaPis,
    decimal AliquotaCofins,
    decimal AliquotaIss,
    decimal AliquotaIpi,
    decimal CargaTributariaPercentual,
    decimal CustoOperacionalPercentual,
    decimal MargemMinimaPercentual,
    int PrioridadeFiscal,
    bool PermiteNfeEntrada,
    bool PermiteNfeSaida,
    bool PermiteDropshipping,
    bool PermiteMarketplace,
    bool EmitentePreferencial,
    bool Ativa);

public sealed record FiscalRoutingCandidate(
    FiscalCompanySnapshot Empresa,
    decimal ReceitaBruta,
    decimal CustoTributarioEstimado,
    decimal CustoOperacionalEstimado,
    decimal LucroEstimado,
    decimal MargemEstimadaPercentual,
    decimal Score,
    IReadOnlyList<string> Justificativas);

public sealed record FiscalRoutingDecision(
    FiscalCompanySnapshot? EmpresaSelecionada,
    IReadOnlyList<FiscalRoutingCandidate> Ranking,
    bool Sucesso,
    string Resumo);
