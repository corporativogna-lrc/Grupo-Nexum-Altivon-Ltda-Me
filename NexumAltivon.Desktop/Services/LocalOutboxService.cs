/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Text.Json;
using System.IO;
using NexumAltivon.Desktop.Models;

namespace NexumAltivon.Desktop.Services;

public sealed class LocalOutboxService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public string BaseDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "GenesisGest.Net",
        "NexumAltivon",
        "pdv-outbox");

    public string OperationalDirectory { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "GenesisGest.Net",
        "NexumAltivon",
        "operational-outbox");

    public async Task<string> SaveSaleAsync(PdvSaleDraft sale, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(BaseDirectory);

        var safeCode = string.Join("_", sale.CodigoVenda.Split(Path.GetInvalidFileNameChars()));
        var filePath = Path.Combine(BaseDirectory, $"{safeCode}.json");
        var json = JsonSerializer.Serialize(sale, JsonOptions);

        await File.WriteAllTextAsync(filePath, json, cancellationToken);
        return filePath;
    }

    public async Task<string> SaveOperationAsync<TPayload>(
        string module,
        string operationCode,
        TPayload payload,
        CancellationToken cancellationToken = default)
    {
        var safeModule = SanitizePathPart(module);
        var safeCode = SanitizePathPart(operationCode);
        var directory = Path.Combine(OperationalDirectory, safeModule);
        Directory.CreateDirectory(directory);

        var filePath = Path.Combine(directory, $"{safeCode}.json");
        var json = JsonSerializer.Serialize(payload, JsonOptions);

        await File.WriteAllTextAsync(filePath, json, cancellationToken);
        return filePath;
    }

    private static string SanitizePathPart(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var clean = string.Join("_", value.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
        return string.IsNullOrWhiteSpace(clean) ? "registro" : clean;
    }
}
