/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

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
