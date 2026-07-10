/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.Desktop.Models;

public sealed class PdvSaleDraft
{
    public string CodigoVenda { get; init; } = string.Empty;
    public string Loja { get; init; } = string.Empty;
    public string Terminal { get; init; } = string.Empty;
    public string Operador { get; init; } = string.Empty;
    public string ClienteNome { get; init; } = string.Empty;
    public string ClienteDocumento { get; init; } = string.Empty;
    public bool ClienteDocumentoValido { get; init; }
    public string Canal { get; init; } = "PDV";
    public string EmpresaEmissora { get; init; } = "Seleção automática";
    public string Status { get; init; } = "Registrada no PDV";
    public string TipoEntrega { get; init; } = "Retirada na loja";
    public string StatusPedido { get; init; } = "Inicial";
    public string StatusFiscal { get; init; } = "Aguardando emissão";
    public string StatusFinanceiro { get; init; } = "Aguardando baixa";
    public string StatusLogistico { get; init; } = "Aguardando separação";
    public string DecisaoEmpresaEmissora { get; init; } = string.Empty;
    public decimal Subtotal { get; init; }
    public decimal Desconto { get; init; }
    public decimal Total { get; init; }
    public decimal Pago { get; init; }
    public decimal Troco { get; init; }
    public decimal MargemEstimada { get; init; }
    public DateTime CriadaEm { get; init; } = DateTime.Now;
    public List<PdvCartItem> Itens { get; init; } = new();
    public List<PdvPaymentLine> Pagamentos { get; init; } = new();
}
