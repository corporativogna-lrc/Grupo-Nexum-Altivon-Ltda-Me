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
using System.Security.Cryptography;
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

    public static async Task<DesktopUpdateResult> CheckAndDownloadLatestAsync(CancellationToken cancellationToken = default)
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
            return new DesktopUpdateResult(true, currentVersion.ToString(), latestVersion.ToString(), packagePath, $"Pacote oficial baixado. A instalação requer confirmação do operador: {Path.GetFileName(packagePath)}.");
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
            .Where(asset => InstallerExtensions.Contains(Path.GetExtension(asset.Name), StringComparer.OrdinalIgnoreCase))
            .Where(asset => asset.Name.Contains("desktop", StringComparison.OrdinalIgnoreCase)
                || asset.Name.Contains("genesis", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(asset => asset.Name.Contains("desktop", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(asset => asset.Name.Contains("genesis", StringComparison.OrdinalIgnoreCase))
            .ThenByDescending(asset => InstallerExtensions.Contains(Path.GetExtension(asset.Name), StringComparer.OrdinalIgnoreCase))
            .ThenBy(asset => asset.Name)
            .FirstOrDefault();
    }

    private static async Task<string> DownloadAssetAsync(HttpClient httpClient, string releaseTag, GitHubAsset asset, CancellationToken cancellationToken)
    {
        if (asset.Size <= 0 || string.IsNullOrWhiteSpace(asset.Name) || !string.Equals(Path.GetFileName(asset.Name), asset.Name, StringComparison.Ordinal))
        {
            throw new InvalidDataException("A release retornou um pacote com nome ou tamanho inválido.");
        }

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
            if (string.IsNullOrWhiteSpace(asset.Digest)
                || await HasPublishedDigestAsync(packagePath, asset.Digest, cancellationToken))
            {
                return packagePath;
            }

            File.Delete(packagePath);
        }

        var temporaryPath = packagePath + ".download";
        await using var source = await httpClient.GetStreamAsync(asset.BrowserDownloadUrl, cancellationToken);
        await using (var target = File.Create(temporaryPath))
        {
            await source.CopyToAsync(target, cancellationToken);
        }

        if (!File.Exists(temporaryPath) || new FileInfo(temporaryPath).Length != asset.Size)
        {
            File.Delete(temporaryPath);
            throw new InvalidDataException("O tamanho do pacote baixado não confere com a release do GitHub.");
        }

        if (!string.IsNullOrWhiteSpace(asset.Digest)
            && !await HasPublishedDigestAsync(temporaryPath, asset.Digest, cancellationToken))
        {
            File.Delete(temporaryPath);
            throw new InvalidDataException("O digest SHA-256 do pacote diverge do digest publicado pelo GitHub.");
        }

        File.Move(temporaryPath, packagePath, true);

        return packagePath;
    }

    private static async Task<bool> HasPublishedDigestAsync(string packagePath, string digest, CancellationToken cancellationToken)
    {
        if (!digest.StartsWith("sha256:", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException("A release publicou um algoritmo de digest não suportado.");
        }

        await using var packageStream = File.OpenRead(packagePath);
        var actualDigest = Convert.ToHexString(await SHA256.HashDataAsync(packageStream, cancellationToken));
        return actualDigest.Equals(digest[7..], StringComparison.OrdinalIgnoreCase);
    }

    public static void InstallDownloadedPackage(string packagePath)
    {
        if (string.IsNullOrWhiteSpace(packagePath) || !File.Exists(packagePath))
        {
            throw new FileNotFoundException("O pacote baixado não está disponível no caminho informado.", packagePath);
        }

        var extension = Path.GetExtension(packagePath);
        if (!InstallerExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException($"Extensão de instalador não compatível: {extension}");
        }

        var startInfo = extension.Equals(".msi", StringComparison.OrdinalIgnoreCase)
            ? new ProcessStartInfo { FileName = "msiexec.exe", Arguments = $"/i \"{packagePath}\"", UseShellExecute = true }
            : new ProcessStartInfo { FileName = packagePath, UseShellExecute = true };
        if (Process.Start(startInfo) is null)
        {
            throw new InvalidOperationException("O Windows não iniciou o instalador.");
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

        [JsonPropertyName("digest")]
        public string? Digest { get; init; }
    }
}
