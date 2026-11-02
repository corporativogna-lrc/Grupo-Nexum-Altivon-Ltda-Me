namespace NexumAltivon.API.DTOs;

public sealed record DesktopLogisticaExpedicaoDto(
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

public sealed record DesktopLogisticaAtualizacaoRequest(
    [property: System.Text.Json.Serialization.JsonPropertyName("frete_metodo")] string FreteMetodo,
    [property: System.Text.Json.Serialization.JsonPropertyName("frete_transportadora")] string FreteTransportadora,
    [property: System.Text.Json.Serialization.JsonPropertyName("frete_prazo_dias")] int FretePrazoDias,
    [property: System.Text.Json.Serialization.JsonPropertyName("frete_codigo_rastreio")] string FreteCodigoRastreio,
    [property: System.Text.Json.Serialization.JsonPropertyName("row_version")] string RowVersion);

public sealed record DesktopPedidoFluxoRequest(
    [property: System.Text.Json.Serialization.JsonPropertyName("row_version")] string RowVersion);

public sealed record DesktopLogisticaPedidoAtualizadoDto(
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
    string? FreteCodigoRastreio);

public sealed record DesktopLogisticaRastreamentoDto(
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
    IReadOnlyList<DesktopLogisticaRastreamentoEventoDto> Eventos,
    IReadOnlyList<string> Pendencias,
    string? RespostaSha256,
    DateTime ConsultadoEm);

public sealed record DesktopLogisticaRastreamentoEventoDto(
    DateTime? DataHora,
    string Status,
    string? Local,
    string? Descricao);
