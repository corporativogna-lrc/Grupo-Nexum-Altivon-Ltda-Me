/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.Desktop.Models;

public sealed class FinancialLedgerDraft
{
    public string Codigo { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string Empresa { get; init; } = string.Empty;
    public string Pessoa { get; init; } = string.Empty;
    public string Documento { get; init; } = string.Empty;
    public string CentroCusto { get; init; } = string.Empty;
    public string ContaFinanceira { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public decimal Desconto { get; init; }
    public decimal JurosMulta { get; init; }
    public DateTime Vencimento { get; init; }
    public string Status { get; init; } = "Registrado localmente";
    public string Aprovacao { get; init; } = string.Empty;
    public string NivelAlcada { get; init; } = string.Empty;
    public string AprovadorResponsavel { get; init; } = string.Empty;
    public bool BloquearPagamentoSemAprovacao { get; init; }
    public string Observacoes { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; } = DateTime.Now;
}

public sealed class ProcurementDraft
{
    public string Codigo { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string StatusAprovacao { get; init; } = string.Empty;
    public string EmpresaDestino { get; init; } = string.Empty;
    public string OrigemAquisicao { get; init; } = string.Empty;
    public string FornecedorParceiro { get; init; } = string.Empty;
    public string ItemDescricao { get; init; } = string.Empty;
    public string Categoria { get; init; } = string.Empty;
    public string UnidadeComercial { get; init; } = string.Empty;
    public string Ncm { get; init; } = string.Empty;
    public string Cest { get; init; } = string.Empty;
    public string CfopSugerido { get; init; } = string.Empty;
    public string OrigemFiscalItem { get; init; } = string.Empty;
    public string GtinCodigoBarras { get; init; } = string.Empty;
    public string PesoDimensoes { get; init; } = string.Empty;
    public bool ProdutoFiscalmenteCompleto { get; init; }
    public string NumeroDocumentoFornecedor { get; init; } = string.Empty;
    public string SerieDocumentoFornecedor { get; init; } = string.Empty;
    public DateTime? DataEmissaoDocumento { get; init; }
    public DateTime? DataEntradaMercadoria { get; init; }
    public DateTime? PrevisaoEntrega { get; init; }
    public string CondicaoPagamento { get; init; } = string.Empty;
    public decimal Quantidade { get; init; }
    public decimal CustoUnitario { get; init; }
    public decimal FreteEstimado { get; init; }
    public decimal ImpostosEstimados { get; init; }
    public decimal TotalEstimado { get; init; }
    public string CentroCusto { get; init; } = string.Empty;
    public string NivelAprovacao { get; init; } = string.Empty;
    public string AprovadorResponsavel { get; init; } = string.Empty;
    public bool PodeGerarPedido { get; init; }
    public string XmlImportadoPath { get; init; } = string.Empty;
    public string XmlChaveAcesso { get; init; } = string.Empty;
    public string XmlFornecedor { get; init; } = string.Empty;
    public decimal XmlValorTotal { get; init; }
    public string Status { get; init; } = "Registrado localmente";
    public string Observacoes { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; } = DateTime.Now;
}

public sealed class FiscalRoutingDraft
{
    public string Codigo { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string EmpresaCandidata { get; init; } = string.Empty;
    public string RegimeFiscal { get; init; } = string.Empty;
    public string UfDestino { get; init; } = string.Empty;
    public string Cfop { get; init; } = string.Empty;
    public string NaturezaOperacao { get; init; } = string.Empty;
    public decimal ValorOperacao { get; init; }
    public decimal CustoFiscalEstimado { get; init; }
    public decimal CustoLogisticoEstimado { get; init; }
    public decimal MargemBrutaEstimada { get; init; }
    public string Decisao { get; init; } = string.Empty;
    public string Status { get; init; } = "Registrado localmente";
    public string Observacoes { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; } = DateTime.Now;
}

public sealed class LogisticsOperationDraft
{
    public string Codigo { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string PedidoReferencia { get; init; } = string.Empty;
    public string ClienteDestino { get; init; } = string.Empty;
    public string Transportadora { get; init; } = string.Empty;
    public string Origem { get; init; } = string.Empty;
    public string Destino { get; init; } = string.Empty;
    public string StatusEntrega { get; init; } = string.Empty;
    public decimal CustoFrete { get; init; }
    public DateTime PrevisaoColeta { get; init; }
    public DateTime PrevisaoEntrega { get; init; }
    public bool NotificarCliente { get; init; }
    public string CanalNotificacao { get; init; } = string.Empty;
    public string Observacoes { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; } = DateTime.Now;
}

public sealed class CorporateMasterDataDraft
{
    public string Codigo { get; init; } = string.Empty;
    public string TipoCadastro { get; init; } = string.Empty;
    public string NomeRazaoSocial { get; init; } = string.Empty;
    public string DocumentoFiscal { get; init; } = string.Empty;
    public string InscricaoEstadualMunicipal { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Telefone { get; init; } = string.Empty;
    public string Cep { get; init; } = string.Empty;
    public string EnderecoCompleto { get; init; } = string.Empty;
    public string RegimeFiscal { get; init; } = string.Empty;
    public string ContaContabil { get; init; } = string.Empty;
    public string CentroCusto { get; init; } = string.Empty;
    public string Departamento { get; init; } = string.Empty;
    public string CargoFuncao { get; init; } = string.Empty;
    public string TipoContrato { get; init; } = string.Empty;
    public decimal SalarioBase { get; init; }
    public DateTime? DataAdmissao { get; init; }
    public DateTime? DataDesligamento { get; init; }
    public string NivelAcesso { get; init; } = string.Empty;
    public string RegrasFiscaisComerciais { get; init; } = string.Empty;
    public bool CadastroCompleto { get; init; }
    public string Status { get; init; } = "Registrado localmente";
    public string Observacoes { get; init; } = string.Empty;
    public DateTime CriadoEm { get; init; } = DateTime.Now;
}
