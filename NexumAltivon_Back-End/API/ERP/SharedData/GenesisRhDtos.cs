namespace NexumAltivon.API.ERP.SharedData;

public sealed record GenesisRhSummaryDto(
    int TotalColaboradores,
    int Ativos,
    int FeriasProgramadas,
    int AdmissoesNoMes,
    int DesligamentosNoMes,
    DateTime AtualizadoEm
);

public sealed record GenesisRhColaboradorDto(
    int Id,
    string Nome,
    string? Cargo,
    string? Departamento,
    string? Status,
    DateTime? DataAdmissao
);

public sealed record GenesisRhReferenciaDto(
    int Id,
    string Tipo,
    string Codigo,
    string Descricao,
    int Ordem
);
