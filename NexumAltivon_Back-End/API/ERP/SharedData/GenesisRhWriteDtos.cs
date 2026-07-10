/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

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
