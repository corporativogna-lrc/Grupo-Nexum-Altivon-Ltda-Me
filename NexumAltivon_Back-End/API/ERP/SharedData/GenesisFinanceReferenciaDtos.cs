namespace NexumAltivon.API.ERP.SharedData;

public sealed record GenesisFinanceReferenciaCreateRequest(
    string Tipo,
    string Codigo,
    string Descricao,
    int Ordem
);
