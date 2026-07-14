/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;

namespace NexumAltivon.API.ERP.SharedData;

public static class GenesisDesktopOperationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static async Task<GenesisDesktopOperationDto> RegistrarOperacaoAsync(
        GenesisDbContext db,
        string module,
        GenesisDesktopOperationRequest request,
        string origemTerminal,
        CancellationToken ct)
    {
        var modulo = NormalizeRequired(module, "Módulo não informado.");
        var codigo = NormalizeRequired(request.Codigo, "Código da operação não informado.");
        var status = Normalize(request.Status) ?? "Recebido";
        var payloadJson = request.Payload.ValueKind == JsonValueKind.Undefined || request.Payload.ValueKind == JsonValueKind.Null
            ? "{}"
            : request.Payload.GetRawText();
        var hash = ComputeHash($"{modulo}|{codigo}|{payloadJson}");
        var now = DateTime.UtcNow;

        await EnsureSchemaAsync(db, ct);

        await db.Database.ExecuteSqlInterpolatedAsync($"""
            INSERT IGNORE INTO sys_operacoes_desktop
                (opd_modulo, opd_codigo, opd_terminal, opd_loja, opd_origem, opd_status, opd_payload_json, opd_hash_sha256, opd_data_cadastro, opd_observacoes)
            VALUES
                ({modulo}, {codigo}, {Normalize(request.Terminal)}, {Normalize(request.Loja)}, {Normalize(request.Origem) ?? origemTerminal}, {status}, {payloadJson}, {hash}, {now}, {Normalize(request.Observacoes)});
            """, ct);

        var operacao = await db.Database.SqlQueryRaw<GenesisDesktopOperationRow>(
                """
                SELECT
                    opd_id AS Id,
                    opd_modulo AS Modulo,
                    opd_codigo AS Codigo,
                    opd_status AS Status,
                    opd_origem AS Origem,
                    opd_data_cadastro AS CriadoEm,
                    opd_hash_sha256 AS HashSha256
                FROM sys_operacoes_desktop
                WHERE opd_modulo = {0}
                  AND opd_codigo = {1}
                  AND opd_hash_sha256 = {2}
                ORDER BY opd_id DESC
                LIMIT 1
                """,
                modulo,
                codigo,
                hash)
            .AsNoTracking()
            .FirstAsync(ct);

        return new GenesisDesktopOperationDto(
            operacao.Id,
            operacao.Modulo,
            operacao.Codigo,
            operacao.Status,
            operacao.Origem,
            operacao.CriadoEm,
            operacao.HashSha256);
    }

    private static async Task EnsureSchemaAsync(GenesisDbContext db, CancellationToken ct)
    {
        await db.Database.ExecuteSqlRawAsync(
            """
            CREATE TABLE IF NOT EXISTS sys_operacoes_desktop (
                opd_id BIGINT AUTO_INCREMENT PRIMARY KEY,
                opd_modulo VARCHAR(80) NOT NULL,
                opd_codigo VARCHAR(120) NOT NULL,
                opd_terminal VARCHAR(120) NULL,
                opd_loja VARCHAR(120) NULL,
                opd_origem VARCHAR(120) NULL,
                opd_status VARCHAR(60) NOT NULL,
                opd_payload_json LONGTEXT NOT NULL,
                opd_hash_sha256 CHAR(64) NOT NULL,
                opd_data_cadastro DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
                opd_data_processamento DATETIME NULL,
                opd_observacoes VARCHAR(500) NULL,
                INDEX ix_sys_operacoes_desktop_modulo_status (opd_modulo, opd_status),
                INDEX ix_sys_operacoes_desktop_codigo (opd_codigo),
                UNIQUE KEY uk_sys_operacoes_desktop_modulo_codigo_hash (opd_modulo, opd_codigo, opd_hash_sha256)
            );
            """,
            ct);
    }

    private static string ComputeHash(string value)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string NormalizeRequired(string? value, string message)
    {
        var normalized = Normalize(value);
        if (normalized is null)
        {
            throw new ArgumentException(message);
        }

        return normalized;
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private sealed class GenesisDesktopOperationRow
    {
        public long Id { get; set; }
        public string Modulo { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Origem { get; set; } = string.Empty;
        public DateTime CriadoEm { get; set; }
        public string HashSha256 { get; set; } = string.Empty;
    }
}

public sealed record GenesisDesktopOperationRequest(
    string Codigo,
    string? Terminal,
    string? Loja,
    string? Origem,
    string? Status,
    JsonElement Payload,
    string? Observacoes);

public sealed record GenesisDesktopOperationDto(
    long Id,
    string Modulo,
    string Codigo,
    string Status,
    string Origem,
    DateTime CriadoEm,
    string HashSha256);
