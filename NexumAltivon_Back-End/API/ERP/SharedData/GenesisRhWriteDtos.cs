namespace NexumAltivon.API.ERP.SharedData;

public sealed record GenesisRhColaboradorUpsertRequest(
    string Nome,
    string? Cargo,
    string? Departamento,
    string? Status,
    DateTime? DataAdmissao
);

public sealed record GenesisRhStatusUpdateRequest(string Status);

public sealed record GenesisRhReferenciaCreateRequest(
    string Tipo,
    string Codigo,
    string Descricao,
    int Ordem
);
