/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

namespace NexumAltivon.Desktop.Models;

public sealed class TerminalProfile
{
    public string StoreCode { get; set; } = GetEnv("NEXUM_DESKTOP_STORE_CODE", "NEXUM-MATRIZ");
    public string StoreName { get; set; } = GetEnv("NEXUM_DESKTOP_STORE_NAME", "Grupo Nexum Altivon");
    public string TerminalCode { get; set; } = GetEnv("NEXUM_DESKTOP_TERMINAL_CODE", Environment.MachineName);
    public string OperatorName { get; set; } = GetEnv("NEXUM_DESKTOP_OPERATOR", Environment.UserName);
    public string WorkMode { get; set; } = GetEnv("NEXUM_DESKTOP_WORK_MODE", "ERP + PDV");
    public string ApiBaseUrl { get; set; } = GetEnv("NEXUM_DESKTOP_API_BASE", "http://127.0.0.1:5010");
    public string PublicApiUrl { get; set; } = GetEnv("NEXUM_DESKTOP_PUBLIC_API", "https://api.nexumaltivon.com.br");
    public string DesktopAccessToken { get; set; } = GetEnv("NEXUM_DESKTOP_TOKEN", string.Empty);
    public string ServerAddress { get; set; } = GetEnv("NEXUM_DESKTOP_SERVER_ADDRESS", "127.0.0.1");
    public string DatabasePort { get; set; } = GetEnv("NEXUM_DESKTOP_DATABASE_PORT", "3309");
    public string LocalPrinter { get; set; } = GetEnv("NEXUM_DESKTOP_LOCAL_PRINTER", "Fiscal/Comprovante pendente");
    public bool OfflineContingencyEnabled { get; set; } = GetEnv("NEXUM_DESKTOP_CONTINGENCY", "true").Equals("true", StringComparison.OrdinalIgnoreCase);

    private static string GetEnv(string name, string fallback)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
