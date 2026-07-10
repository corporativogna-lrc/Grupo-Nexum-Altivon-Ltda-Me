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

namespace NexumAltivon.API.Infrastructure.Storage;

public interface IAnexoStorageService
{
    AnexoStorageStatusDto ObterStatus();
    AnexoSignedUrlDto CriarUrlAssinada(AnexoSignedUrlRequest request, HttpContext httpContext, string method);
    bool ValidarUrlLocal(string method, string storageKey, IQueryCollection query, out string? erro);
    Task<AnexoLocalUploadDto> SalvarUploadLocalAsync(string storageKey, HttpRequest request, CancellationToken ct);
    Task<AnexoLocalDownloadDto?> AbrirDownloadLocalAsync(string storageKey, CancellationToken ct);
}

public sealed class AnexoStorageService : IAnexoStorageService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AnexoStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    public AnexoStorageStatusDto ObterStatus()
    {
        var provider = ObterProvider();
        var configured = provider == "local"
            ? !string.IsNullOrWhiteSpace(ObterChaveAssinatura(false))
            : ObterS3Options(false).Configurado;

        return new AnexoStorageStatusDto(
            provider,
            configured,
            provider == "local" ? ObterDiretorioLocal() : null,
            provider == "s3" ? ObterS3Options(false).Bucket : null,
            provider == "s3" ? ObterS3Options(false).Endpoint : null,
            configured
                ? "Storage operacional para anexos fiscais, ordens de servico, contratos e SPED."
                : "Storage pendente de configuracao de chaves e destino.");
    }

    public AnexoSignedUrlDto CriarUrlAssinada(AnexoSignedUrlRequest request, HttpContext httpContext, string method)
    {
        var normalizedMethod = NormalizeMethod(method);
        var storageKey = CriarStorageKey(request);
        var ttl = ObterTtl(request.ExpiraEmMinutos);
        var expiresAt = DateTimeOffset.UtcNow.Add(ttl);
        var provider = ObterProvider();

        if (provider == "s3")
        {
            return CriarUrlS3Assinada(storageKey, normalizedMethod, expiresAt);
        }

        return CriarUrlLocalAssinada(storageKey, normalizedMethod, expiresAt, httpContext);
    }

    public bool ValidarUrlLocal(string method, string storageKey, IQueryCollection query, out string? erro)
    {
        erro = null;
        var normalizedKey = NormalizarStorageKey(storageKey);
        if (normalizedKey is null)
        {
            erro = "Chave de anexo invalida.";
            return false;
        }

        if (!long.TryParse(query["expires"], out var expiresUnix))
        {
            erro = "Expiracao ausente ou invalida.";
            return false;
        }

        if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiresUnix)
        {
            erro = "Url assinada expirada.";
            return false;
        }

        var signature = query["signature"].ToString();
        if (string.IsNullOrWhiteSpace(signature))
        {
            erro = "Assinatura ausente.";
            return false;
        }

        var expected = CriarAssinaturaLocal(NormalizeMethod(method), normalizedKey, expiresUnix);
        var signatureBytes = Encoding.UTF8.GetBytes(signature);
        var expectedBytes = Encoding.UTF8.GetBytes(expected);

        if (signatureBytes.Length != expectedBytes.Length ||
            !CryptographicOperations.FixedTimeEquals(signatureBytes, expectedBytes))
        {
            erro = "Assinatura invalida.";
            return false;
        }

        return true;
    }

    public async Task<AnexoLocalUploadDto> SalvarUploadLocalAsync(string storageKey, HttpRequest request, CancellationToken ct)
    {
        var normalizedKey = NormalizarStorageKey(storageKey)
            ?? throw new InvalidOperationException("Chave de anexo invalida.");
        var maxBytes = _configuration.GetValue<long?>("Storage:MaxUploadBytes") ?? 52_428_800L;

        if (request.ContentLength is > 0 && request.ContentLength > maxBytes)
        {
            throw new InvalidOperationException($"Arquivo excede o limite de {maxBytes} bytes.");
        }

        var destino = ResolverCaminhoLocal(normalizedKey);
        Directory.CreateDirectory(Path.GetDirectoryName(destino)!);

        var temporario = destino + ".tmp";
        await using (var stream = File.Create(temporario))
        {
            await request.Body.CopyToAsync(stream, ct);
            if (stream.Length > maxBytes)
            {
                throw new InvalidOperationException($"Arquivo excede o limite de {maxBytes} bytes.");
            }
        }

        if (File.Exists(destino))
        {
            File.Delete(destino);
        }

        File.Move(temporario, destino);
        var info = new FileInfo(destino);
        var metadata = new AnexoLocalMetadata(
            normalizedKey,
            ExtrairNomeArquivo(normalizedKey),
            request.ContentType ?? "application/octet-stream",
            info.Length,
            DateTimeOffset.UtcNow);

        await File.WriteAllTextAsync(destino + ".metadata.json", JsonSerializer.Serialize(metadata, JsonOptions), ct);

        return new AnexoLocalUploadDto(normalizedKey, info.Length, metadata.ContentType, metadata.GravadoEm);
    }

    public async Task<AnexoLocalDownloadDto?> AbrirDownloadLocalAsync(string storageKey, CancellationToken ct)
    {
        var normalizedKey = NormalizarStorageKey(storageKey);
        if (normalizedKey is null)
        {
            return null;
        }

        var caminho = ResolverCaminhoLocal(normalizedKey);
        if (!File.Exists(caminho))
        {
            return null;
        }

        var metadata = await LerMetadataAsync(caminho, normalizedKey, ct);
        var stream = File.OpenRead(caminho);
        return new AnexoLocalDownloadDto(stream, metadata.ContentType, metadata.FileName, stream.Length);
    }

    private AnexoSignedUrlDto CriarUrlLocalAssinada(
        string storageKey,
        string method,
        DateTimeOffset expiresAt,
        HttpContext httpContext)
    {
        var expiresUnix = expiresAt.ToUnixTimeSeconds();
        var signature = CriarAssinaturaLocal(method, storageKey, expiresUnix);
        var baseUrl = TrimOrNull(_configuration["Storage:PublicBaseUrl"])
            ?? $"{httpContext.Request.Scheme}://{httpContext.Request.Host}";
        var route = method == "PUT" ? "upload" : "download";
        var url = $"{baseUrl.TrimEnd('/')}/api/anexos/{route}/{EscapeStorageKey(storageKey)}?expires={expiresUnix}&signature={signature}";

        return new AnexoSignedUrlDto(
            "local",
            storageKey,
            url,
            method,
            expiresAt,
            new Dictionary<string, string>
            {
                ["Content-Type"] = method == "PUT" ? "application/octet-stream" : "application/json"
            },
            null,
            "Url assinada pelo backend para storage local. Configure Storage:Provider=s3 para MinIO/S3.");
    }

    private AnexoSignedUrlDto CriarUrlS3Assinada(string storageKey, string method, DateTimeOffset expiresAt)
    {
        var options = ObterS3Options(true);
        var now = DateTimeOffset.UtcNow;
        var amzDate = now.ToString("yyyyMMdd'T'HHmmss'Z'");
        var dateStamp = now.ToString("yyyyMMdd");
        var expiresSeconds = Math.Max(1, (long)(expiresAt - now).TotalSeconds);
        var scope = $"{dateStamp}/{options.Region}/s3/aws4_request";
        var endpoint = new Uri(options.Endpoint.TrimEnd('/') + "/");
        var canonicalKey = EscapeStorageKey(storageKey);
        var canonicalUri = options.ForcePathStyle
            ? $"/{Encode(options.Bucket)}/{canonicalKey}"
            : $"/{canonicalKey}";
        var hostHeader = CriarHostHeader(endpoint, options);

        var query = new SortedDictionary<string, string>(StringComparer.Ordinal)
        {
            ["X-Amz-Algorithm"] = "AWS4-HMAC-SHA256",
            ["X-Amz-Credential"] = $"{options.AccessKey}/{scope}",
            ["X-Amz-Date"] = amzDate,
            ["X-Amz-Expires"] = expiresSeconds.ToString(),
            ["X-Amz-SignedHeaders"] = "host"
        };

        var canonicalQuery = CriarQueryString(query);
        var canonicalRequest = string.Join('\n', new[]
        {
            method,
            canonicalUri,
            canonicalQuery,
            $"host:{hostHeader}",
            string.Empty,
            "host",
            "UNSIGNED-PAYLOAD"
        });
        var stringToSign = string.Join('\n', new[]
        {
            "AWS4-HMAC-SHA256",
            amzDate,
            scope,
            Hex(SHA256.HashData(Encoding.UTF8.GetBytes(canonicalRequest)))
        });

        var signingKey = CriarS3SigningKey(options.SecretKey, dateStamp, options.Region);
        var signature = Hex(Hmac(signingKey, stringToSign));
        query["X-Amz-Signature"] = signature;

        var finalQuery = CriarQueryString(query);
        var url = $"{endpoint.Scheme}://{hostHeader}{canonicalUri}?{finalQuery}";

        return new AnexoSignedUrlDto(
            "s3",
            storageKey,
            url,
            method,
            expiresAt,
            new Dictionary<string, string>(),
            options.Bucket,
            "Url pre-assinada AWS Signature V4 compativel com S3 e MinIO.");
    }

    private string CriarAssinaturaLocal(string method, string storageKey, long expiresUnix)
    {
        var key = ObterChaveAssinatura(true)!;
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        return Hex(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{method}\n{storageKey}\n{expiresUnix}")));
    }

    private string CriarStorageKey(AnexoSignedUrlRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.StorageKey))
        {
            return NormalizarStorageKey(request.StorageKey)
                ?? throw new InvalidOperationException("Chave de anexo invalida.");
        }

        var modulo = SanitizarParte(request.Modulo, "geral").ToLowerInvariant();
        var origem = SanitizarParte(request.Origem, "avulso").ToLowerInvariant();
        var fileName = SanitizarArquivo(request.FileName);
        var now = DateTimeOffset.UtcNow;
        return $"{modulo}/{origem}/{now:yyyy}/{now:MM}/{Guid.NewGuid():N}-{fileName}";
    }

    private string ResolverCaminhoLocal(string storageKey)
    {
        var root = Path.GetFullPath(ObterDiretorioLocal());
        var fullPath = Path.GetFullPath(Path.Combine(root, storageKey.Replace('/', Path.DirectorySeparatorChar)));
        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Chave de anexo fora da raiz permitida.");
        }

        return fullPath;
    }

    private string ObterDiretorioLocal()
    {
        return TrimOrNull(_configuration["Storage:LocalRoot"])
            ?? Path.Combine(_environment.ContentRootPath, "storage", "anexos");
    }

    private string ObterProvider()
    {
        return (TrimOrNull(_configuration["Storage:Provider"]) ?? "local").ToLowerInvariant();
    }

    private TimeSpan ObterTtl(int? minutos)
    {
        var ttl = minutos ?? _configuration.GetValue<int?>("Storage:DefaultTtlMinutes") ?? 15;
        return TimeSpan.FromMinutes(Math.Clamp(ttl, 1, 120));
    }

    private string? ObterChaveAssinatura(bool obrigatoria)
    {
        var key = TrimOrNull(_configuration["Storage:SigningKey"])
            ?? TrimOrNull(_configuration["JwtSettings:SecretKey"])
            ?? TrimOrNull(Environment.GetEnvironmentVariable("STORAGE_SIGNING_KEY"));

        if (obrigatoria && key is null)
        {
            throw new InvalidOperationException("Storage:SigningKey ou JwtSettings:SecretKey deve ser configurado para assinar anexos.");
        }

        return key;
    }

    private S3Options ObterS3Options(bool obrigatorio)
    {
        var options = new S3Options(
            TrimOrNull(_configuration["Storage:S3:Endpoint"]) ?? TrimOrNull(Environment.GetEnvironmentVariable("S3_ENDPOINT")) ?? string.Empty,
            TrimOrNull(_configuration["Storage:S3:Bucket"]) ?? TrimOrNull(Environment.GetEnvironmentVariable("S3_BUCKET")) ?? string.Empty,
            TrimOrNull(_configuration["Storage:S3:Region"]) ?? TrimOrNull(Environment.GetEnvironmentVariable("S3_REGION")) ?? "us-east-1",
            TrimOrNull(_configuration["Storage:S3:AccessKey"]) ?? TrimOrNull(Environment.GetEnvironmentVariable("S3_ACCESS_KEY")) ?? string.Empty,
            TrimOrNull(_configuration["Storage:S3:SecretKey"]) ?? TrimOrNull(Environment.GetEnvironmentVariable("S3_SECRET_KEY")) ?? string.Empty,
            _configuration.GetValue<bool?>("Storage:S3:ForcePathStyle") ?? true);

        if (obrigatorio && !options.Configurado)
        {
            throw new InvalidOperationException("Storage S3/MinIO exige Endpoint, Bucket, AccessKey e SecretKey.");
        }

        return options;
    }

    private async Task<AnexoLocalMetadata> LerMetadataAsync(string caminho, string storageKey, CancellationToken ct)
    {
        var metadataPath = caminho + ".metadata.json";
        if (!File.Exists(metadataPath))
        {
            return new AnexoLocalMetadata(storageKey, ExtrairNomeArquivo(storageKey), "application/octet-stream", new FileInfo(caminho).Length, DateTimeOffset.UtcNow);
        }

        var json = await File.ReadAllTextAsync(metadataPath, ct);
        return JsonSerializer.Deserialize<AnexoLocalMetadata>(json, JsonOptions)
            ?? new AnexoLocalMetadata(storageKey, ExtrairNomeArquivo(storageKey), "application/octet-stream", new FileInfo(caminho).Length, DateTimeOffset.UtcNow);
    }

    private static string? NormalizarStorageKey(string storageKey)
    {
        var normalized = storageKey.Replace('\\', '/').Trim('/');
        if (string.IsNullOrWhiteSpace(normalized) ||
            normalized.Contains("..", StringComparison.Ordinal) ||
            normalized.StartsWith("/", StringComparison.Ordinal))
        {
            return null;
        }

        return normalized;
    }

    private static string NormalizeMethod(string method)
    {
        var normalized = method.ToUpperInvariant();
        return normalized is "GET" or "PUT"
            ? normalized
            : throw new InvalidOperationException("Metodo de anexo nao suportado.");
    }

    private static string SanitizarArquivo(string? fileName)
    {
        var safe = Path.GetFileName(TrimOrNull(fileName) ?? "anexo.bin");
        foreach (var invalid in Path.GetInvalidFileNameChars())
        {
            safe = safe.Replace(invalid, '-');
        }

        return safe.Trim().Length == 0 ? "anexo.bin" : safe;
    }

    private static string SanitizarParte(string? value, string fallback)
    {
        var raw = TrimOrNull(value) ?? fallback;
        var chars = raw
            .Select(ch => char.IsLetterOrDigit(ch) || ch is '-' or '_' ? ch : '-')
            .ToArray();
        var sanitized = new string(chars).Trim('-');
        return string.IsNullOrWhiteSpace(sanitized) ? fallback : sanitized;
    }

    private static string ExtrairNomeArquivo(string storageKey)
    {
        var name = storageKey.Split('/').LastOrDefault() ?? "anexo.bin";
        var separator = name.IndexOf('-', StringComparison.Ordinal);
        return separator >= 0 && separator < name.Length - 1 ? name[(separator + 1)..] : name;
    }

    private static string CriarHostHeader(Uri endpoint, S3Options options)
    {
        var host = options.ForcePathStyle ? endpoint.Host : $"{options.Bucket}.{endpoint.Host}";
        var defaultPort = endpoint.Scheme == "https" ? 443 : 80;
        return endpoint.Port > 0 && endpoint.Port != defaultPort ? $"{host}:{endpoint.Port}" : host;
    }

    private static byte[] CriarS3SigningKey(string secretKey, string dateStamp, string region)
    {
        var kDate = Hmac(Encoding.UTF8.GetBytes("AWS4" + secretKey), dateStamp);
        var kRegion = Hmac(kDate, region);
        var kService = Hmac(kRegion, "s3");
        return Hmac(kService, "aws4_request");
    }

    private static byte[] Hmac(byte[] key, string value)
    {
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(value));
    }

    private static string CriarQueryString(SortedDictionary<string, string> query)
    {
        return string.Join("&", query.Select(item => $"{Encode(item.Key)}={Encode(item.Value)}"));
    }

    private static string EscapeStorageKey(string key)
    {
        return string.Join("/", key.Split('/').Select(Encode));
    }

    private static string Encode(string value)
    {
        return Uri.EscapeDataString(value).Replace("%7E", "~", StringComparison.Ordinal);
    }

    private static string Hex(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string? TrimOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private sealed record S3Options(
        string Endpoint,
        string Bucket,
        string Region,
        string AccessKey,
        string SecretKey,
        bool ForcePathStyle)
    {
        public bool Configurado =>
            !string.IsNullOrWhiteSpace(Endpoint) &&
            !string.IsNullOrWhiteSpace(Bucket) &&
            !string.IsNullOrWhiteSpace(AccessKey) &&
            !string.IsNullOrWhiteSpace(SecretKey);
    }

    private sealed record AnexoLocalMetadata(
        string StorageKey,
        string FileName,
        string ContentType,
        long Bytes,
        DateTimeOffset GravadoEm);
}

public sealed record AnexoSignedUrlRequest(
    string Modulo,
    string Origem,
    string FileName,
    string? ContentType,
    long? TamanhoBytes,
    int? ExpiraEmMinutos,
    string? StorageKey);

public sealed record AnexoSignedUrlDto(
    string Provider,
    string StorageKey,
    string Url,
    string Method,
    DateTimeOffset ExpiresAt,
    IReadOnlyDictionary<string, string> Headers,
    string? Bucket,
    string Observacao);

public sealed record AnexoStorageStatusDto(
    string Provider,
    bool Configurado,
    string? LocalRoot,
    string? Bucket,
    string? Endpoint,
    string Mensagem);

public sealed record AnexoLocalUploadDto(
    string StorageKey,
    long Bytes,
    string ContentType,
    DateTimeOffset GravadoEm);

public sealed record AnexoLocalDownloadDto(
    Stream Stream,
    string ContentType,
    string FileName,
    long Bytes);
