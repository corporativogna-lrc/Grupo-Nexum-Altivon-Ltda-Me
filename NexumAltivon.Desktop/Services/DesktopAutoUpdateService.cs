/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NexumAltivon.Desktop.Services;

public sealed record DesktopUpdateResult(
    bool UpdateAvailable,
    string CurrentVersion,
    string? LatestVersion,
    string? DownloadedFile,
    string Message);

public static class DesktopAutoUpdateService
{
    private const string DefaultRepository = "corporativogna-lrc/Grupo-Nexum-Altivon-Ltda-Me";
    private static readonly string[] InstallerExtensions = [".msixbundle", ".msix", ".msi", ".exe"];
    private static readonly string[] PackageExtensions = [".msixbundle", ".msix", ".msi", ".exe", ".zip"];

    public static async Task<DesktopUpdateResult> CheckDownloadAndApplyAsync(CancellationToken cancellationToken = default)
    {
        var currentVersion = GetCurrentVersion();
        var repository = Environment.GetEnvironmentVariable("GENESIS_UPDATE_GITHUB_REPOSITORY");
        if (string.IsNullOrWhiteSpace(repository))
        {
            repository = DefaultRepository;
        }

        try
        {
            using var httpClient = CreateHttpClient();
            var release = await GetLatestReleaseAsync(httpClient, repository, cancellationToken);
            if (release is null)
            {
                return new DesktopUpdateResult(false, currentVersion.ToString(), null, null, "Nenhuma release publicada encontrada para o canal desktop.");
            }

            var latestVersion = ParseVersion(release.TagName);
            if (latestVersion <= currentVersion)
            {
                return new DesktopUpdateResult(false, currentVersion.ToString(), latestVersion.ToString(), null, "Desktop já está na versão mais recente publicada.");
            }

            var asset = SelectDesktopAsset(release.Assets);
            if (asset is null)
            {
                return new DesktopUpdateResult(true, currentVersion.ToString(), latestVersion.ToString(), null, "Release nova encontrada, mas sem pacote desktop compatível.");
            }

            var packagePath = await DownloadAssetAsync(httpClient, release.TagName, asset, cancellationToken);
            TryStartInstaller(packagePath);

            return new DesktopUpdateResult(true, currentVersion.ToString(), latestVersion.ToString(), packagePath, $"Pacote de atualização baixado: {Path.GetFileName(packagePath)}.");
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new DesktopUpdateResult(false, currentVersion.ToString(), null, null, $"Falha ao verificar atualização desktop: {ex.Message}");
        }
    }

    private static HttpClient CreateHttpClient()
    {
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("GenesisGest.Net-Desktop-Updater/1.1.5");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));

        var token = Environment.GetEnvironmentVariable("GENESIS_UPDATE_GITHUB_TOKEN");
        if (!string.IsNullOrWhiteSpace(token))
        {
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return httpClient;
    }

    private static async Task<GitHubRelease?> GetLatestReleaseAsync(HttpClient httpClient, string repository, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"https://api.github.com/repos/{repository}/releases/latest", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        return await JsonSerializer.DeserializeAsync<GitHubRelease>(stream, JsonOptions, cancellationToken);
    }

    private static GitHubAsset? SelectDesktopAsset(IReadOnlyList<GitHubAsset> assets)
    {
        return assets
            .Where(asset => !string.IsNullOrWhiteSpace(asset.BrowserDownloadUrl))
            .Where(asset => PackageExtensions.Contains(Path.GetExtension(asset.Name), StringComparer.OrdinalIgnoreCase))
            .OrderByDescending(asset => asset.Name.Contains("desktop", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(asset => asset.Name.Contains("genesis", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(asset => asset.Name.Contains("delta", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(asset => InstallerExtensions.Contains(Path.GetExtension(asset.Name), StringComparer.OrdinalIgnoreCase))
            .ThenBy(asset => asset.Name)
            .FirstOrDefault();
    }

    private static async Task<string> DownloadAssetAsync(HttpClient httpClient, string releaseTag, GitHubAsset asset, CancellationToken cancellationToken)
    {
        var safeTag = string.Join("_", releaseTag.Split(Path.GetInvalidFileNameChars(), StringSplitOptions.RemoveEmptyEntries));
        var updateDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GenesisGest.Net",
            "Updates",
            safeTag);
        Directory.CreateDirectory(updateDirectory);

        var packagePath = Path.Combine(updateDirectory, asset.Name);
        if (File.Exists(packagePath) && new FileInfo(packagePath).Length == asset.Size)
        {
            return packagePath;
        }

        await using var source = await httpClient.GetStreamAsync(asset.BrowserDownloadUrl, cancellationToken);
        await using var target = File.Create(packagePath);
        await source.CopyToAsync(target, cancellationToken);

        return packagePath;
    }

    private static void TryStartInstaller(string packagePath)
    {
        var autoInstall = string.Equals(
            Environment.GetEnvironmentVariable("GENESIS_UPDATE_AUTO_INSTALL"),
            "true",
            StringComparison.OrdinalIgnoreCase);
        if (!autoInstall)
        {
            return;
        }

        var extension = Path.GetExtension(packagePath);
        if (extension.Equals(".msi", StringComparison.OrdinalIgnoreCase))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "msiexec.exe",
                Arguments = $"/i \"{packagePath}\"",
                UseShellExecute = true
            });
            return;
        }

        if (InstallerExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = packagePath,
                UseShellExecute = true
            });
        }
    }

    private static Version GetCurrentVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        return assembly.GetName().Version ?? new Version(1, 1, 5, 0);
    }

    private static Version ParseVersion(string tag)
    {
        var normalized = tag.Trim().TrimStart('v', 'V');
        return Version.TryParse(normalized, out var version) ? version : new Version(0, 0, 0, 0);
    }

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private sealed class GitHubRelease
    {
        [JsonPropertyName("tag_name")]
        public string TagName { get; init; } = "0.0.0";

        [JsonPropertyName("assets")]
        public List<GitHubAsset> Assets { get; init; } = [];
    }

    private sealed class GitHubAsset
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = string.Empty;

        [JsonPropertyName("size")]
        public long Size { get; init; }

        [JsonPropertyName("browser_download_url")]
        public string BrowserDownloadUrl { get; init; } = string.Empty;
    }
}
