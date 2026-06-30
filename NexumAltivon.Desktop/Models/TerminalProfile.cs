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
    public string StoreCode { get; set; } = "NEXUM-MATRIZ";
    public string StoreName { get; set; } = "Grupo Nexum Altivon";
    public string TerminalCode { get; set; } = Environment.MachineName;
    public string OperatorName { get; set; } = "Operador local";
    public string WorkMode { get; set; } = "ERP + PDV";
    public string ApiBaseUrl { get; set; } = "http://192.168.1.72:5012";
    public string PublicApiUrl { get; set; } = "https://api.nexumaltivon.com.br";
    public string ServerAddress { get; set; } = "192.168.1.72";
    public string DatabasePort { get; set; } = "3309";
    public string LocalPrinter { get; set; } = "Fiscal/Comprovante pendente";
    public bool OfflineContingencyEnabled { get; set; } = true;
}
