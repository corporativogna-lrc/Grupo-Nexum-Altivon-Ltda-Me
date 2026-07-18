/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5.7181
 */

using System.Globalization;
using System.Reflection;
using NexumAltivon.API.ERP.SharedData;
using PdfSharp;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace NexumAltivon.API.Infrastructure.Reports;

public sealed class FinancePdfReportService
{
    private const string FontFamily = "GenesisSans";
    private static readonly string ReleaseVersion = typeof(FinancePdfReportService).Assembly.GetName().Version?.ToString(4)
        ?? throw new InvalidOperationException("A versao do assembly da API nao foi definida.");
    private static readonly Lazy<bool> FontRegistration = new(RegisterEmbeddedFont, LazyThreadSafetyMode.ExecutionAndPublication);
    private static readonly CultureInfo ReportCulture = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly XColor Ink = XColor.FromArgb(30, 35, 42);
    private static readonly XColor Muted = XColor.FromArgb(91, 99, 111);
    private static readonly XColor Gold = XColor.FromArgb(201, 162, 39);
    private static readonly XColor Panel = XColor.FromArgb(245, 247, 250);
    private static readonly XColor Border = XColor.FromArgb(216, 221, 229);
    private static readonly XColor White = XColors.White;

    public byte[] CreatePayablesReport(
        IReadOnlyCollection<GenesisContaPagarDto> items,
        Guid tenantId,
        DateTime? start,
        DateTime? end,
        string? status,
        DateTime generatedAtUtc)
    {
        var rows = items.Select(item => new FinanceReportRow(
            item.NumeroDocumento,
            item.Descricao,
            item.DataEmissao,
            item.DataVencimento,
            item.Status,
            item.ValorOriginal,
            item.ValorPago,
            item.ValorAberto)).ToList();
        return CreateReport(
            "Contas a pagar",
            "Posição de obrigações financeiras",
            "Pago",
            rows,
            tenantId,
            start,
            end,
            status,
            generatedAtUtc);
    }

    public byte[] CreateReceivablesReport(
        IReadOnlyCollection<GenesisContaReceberDto> items,
        Guid tenantId,
        DateTime? start,
        DateTime? end,
        string? status,
        DateTime generatedAtUtc)
    {
        var rows = items.Select(item => new FinanceReportRow(
            item.NumeroDocumento,
            item.Descricao,
            item.DataEmissao,
            item.DataVencimento,
            item.Status,
            item.ValorOriginal,
            item.ValorRecebido,
            item.ValorAberto)).ToList();
        return CreateReport(
            "Contas a receber",
            "Posição de direitos financeiros",
            "Recebido",
            rows,
            tenantId,
            start,
            end,
            status,
            generatedAtUtc);
    }

    private static byte[] CreateReport(
        string title,
        string subtitle,
        string settledColumn,
        IReadOnlyList<FinanceReportRow> rows,
        Guid tenantId,
        DateTime? start,
        DateTime? end,
        string? status,
        DateTime generatedAtUtc)
    {
        if (rows.Count == 0)
        {
            throw new ArgumentException("O relatorio financeiro exige ao menos um titulo persistido.", nameof(rows));
        }

        _ = FontRegistration.Value;
        using var document = new PdfDocument();
        document.Info.Title = $"GenesisGest.Net - {title}";
        document.Info.Author = "Grupo Nexum Altivon";
        document.Info.Subject = subtitle;
        document.Info.Creator = $"GenesisGest.Net v{ReleaseVersion}";

        var fonts = new ReportFonts(
            new XFont(FontFamily, 18, XFontStyleEx.Bold),
            new XFont(FontFamily, 10, XFontStyleEx.Regular),
            new XFont(FontFamily, 9, XFontStyleEx.Bold),
            new XFont(FontFamily, 8, XFontStyleEx.Regular),
            new XFont(FontFamily, 8, XFontStyleEx.Bold),
            new XFont(FontFamily, 7, XFontStyleEx.Regular));
        var generatedLocal = TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(generatedAtUtc, DateTimeKind.Utc),
            ResolveSaoPauloTimeZone());
        var filters = BuildFilters(start, end, status);
        var totalOriginal = rows.Sum(row => row.Original);
        var totalSettled = rows.Sum(row => row.Settled);
        var totalOpen = rows.Sum(row => row.Open);

        var pageNumber = 0;
        var page = AddLandscapePage(document);
        pageNumber++;
        var graphics = XGraphics.FromPdfPage(page);
        var y = DrawFirstPageHeader(
            graphics,
            page,
            fonts,
            title,
            subtitle,
            tenantId,
            filters,
            generatedLocal,
            rows.Count,
            totalOriginal,
            totalSettled,
            totalOpen);
        y = DrawTableHeader(graphics, fonts, y, settledColumn);

        for (var index = 0; index < rows.Count; index++)
        {
            if (y + 25 > page.Height.Point - 42)
            {
                graphics.Dispose();
                page = AddLandscapePage(document);
                pageNumber++;
                graphics = XGraphics.FromPdfPage(page);
                y = DrawContinuationHeader(graphics, page, fonts, title, tenantId, pageNumber);
                y = DrawTableHeader(graphics, fonts, y, settledColumn);
            }

            y = DrawTableRow(graphics, fonts, y, rows[index], index % 2 == 1);
        }

        graphics.Dispose();
        DrawFooters(document, fonts, generatedLocal);
        using var stream = new MemoryStream();
        document.Save(stream, false);
        var bytes = stream.ToArray();
        if (bytes.Length < 5 || bytes[0] != (byte)'%' || bytes[1] != (byte)'P' || bytes[2] != (byte)'D' || bytes[3] != (byte)'F')
        {
            throw new InvalidOperationException("O gerador financeiro nao produziu um documento PDF valido.");
        }

        return bytes;
    }

    private static double DrawFirstPageHeader(
        XGraphics graphics,
        PdfPage page,
        ReportFonts fonts,
        string title,
        string subtitle,
        Guid tenantId,
        string filters,
        DateTime generatedLocal,
        int count,
        decimal totalOriginal,
        decimal totalSettled,
        decimal totalOpen)
    {
        graphics.DrawRectangle(new XSolidBrush(Ink), 0, 0, page.Width.Point, 86);
        graphics.DrawRectangle(new XSolidBrush(Gold), 0, 0, 10, 86);
        graphics.DrawString("GENESISGEST.NET", fonts.TableHeader, new XSolidBrush(Gold), new XRect(28, 18, 220, 15), XStringFormats.TopLeft);
        graphics.DrawString(title, fonts.Title, new XSolidBrush(White), new XRect(28, 34, 430, 26), XStringFormats.TopLeft);
        graphics.DrawString(subtitle, fonts.Body, new XSolidBrush(XColor.FromArgb(213, 218, 225)), new XRect(29, 62, 430, 15), XStringFormats.TopLeft);
        graphics.DrawString($"Tenant: {tenantId:D}", fonts.Small, new XSolidBrush(White), new XRect(475, 24, page.Width.Point - 503, 14), XStringFormats.TopRight);
        graphics.DrawString(filters, fonts.Small, new XSolidBrush(White), new XRect(475, 42, page.Width.Point - 503, 14), XStringFormats.TopRight);
        graphics.DrawString($"Emitido em {generatedLocal:dd/MM/yyyy HH:mm:ss}", fonts.Small, new XSolidBrush(White), new XRect(475, 60, page.Width.Point - 503, 14), XStringFormats.TopRight);

        const double cardY = 102;
        var available = page.Width.Point - 56;
        var cardWidth = (available - 36) / 4;
        DrawSummaryCard(graphics, fonts, 28, cardY, cardWidth, "Títulos", count.ToString(ReportCulture));
        DrawSummaryCard(graphics, fonts, 28 + cardWidth + 12, cardY, cardWidth, "Valor original", FormatMoney(totalOriginal));
        DrawSummaryCard(graphics, fonts, 28 + (cardWidth + 12) * 2, cardY, cardWidth, "Liquidado", FormatMoney(totalSettled));
        DrawSummaryCard(graphics, fonts, 28 + (cardWidth + 12) * 3, cardY, cardWidth, "Em aberto", FormatMoney(totalOpen), Gold);
        return 161;
    }

    private static double DrawContinuationHeader(XGraphics graphics, PdfPage page, ReportFonts fonts, string title, Guid tenantId, int pageNumber)
    {
        graphics.DrawRectangle(new XSolidBrush(Ink), 0, 0, page.Width.Point, 54);
        graphics.DrawRectangle(new XSolidBrush(Gold), 0, 0, 10, 54);
        graphics.DrawString($"GENESISGEST.NET | {title}", fonts.TableHeader, new XSolidBrush(White), new XRect(28, 18, 450, 18), XStringFormats.TopLeft);
        graphics.DrawString($"Tenant {tenantId:D} | continuação {pageNumber}", fonts.Small, new XSolidBrush(White), new XRect(475, 20, page.Width.Point - 503, 15), XStringFormats.TopRight);
        return 70;
    }

    private static void DrawSummaryCard(XGraphics graphics, ReportFonts fonts, double x, double y, double width, string label, string value, XColor? valueColor = null)
    {
        graphics.DrawRectangle(new XSolidBrush(Panel), x, y, width, 45);
        graphics.DrawString(label, fonts.Small, new XSolidBrush(Muted), new XRect(x + 12, y + 8, width - 24, 12), XStringFormats.TopLeft);
        graphics.DrawString(value, fonts.TableHeader, new XSolidBrush(valueColor ?? Ink), new XRect(x + 12, y + 23, width - 24, 16), XStringFormats.TopLeft);
    }

    private static double DrawTableHeader(XGraphics graphics, ReportFonts fonts, double y, string settledColumn)
    {
        var columns = GetColumns(settledColumn);
        var x = 28d;
        foreach (var column in columns)
        {
            graphics.DrawRectangle(new XSolidBrush(Ink), x, y, column.Width, 24);
            graphics.DrawString(column.Label, fonts.TableHeader, new XSolidBrush(White), new XRect(x + 6, y + 7, column.Width - 12, 12), column.RightAligned ? XStringFormats.TopRight : XStringFormats.TopLeft);
            x += column.Width;
        }

        return y + 24;
    }

    private static double DrawTableRow(XGraphics graphics, ReportFonts fonts, double y, FinanceReportRow row, bool alternate)
    {
        var values = new[]
        {
            row.Document,
            row.Description,
            row.IssuedAt.ToString("dd/MM/yyyy", ReportCulture),
            row.DueAt.ToString("dd/MM/yyyy", ReportCulture),
            row.Status,
            FormatMoney(row.Original),
            FormatMoney(row.Settled),
            FormatMoney(row.Open)
        };
        var columns = GetColumns(string.Empty);
        var x = 28d;
        var background = alternate ? XColor.FromArgb(249, 250, 252) : White;
        for (var index = 0; index < columns.Length; index++)
        {
            var column = columns[index];
            graphics.DrawRectangle(new XSolidBrush(background), x, y, column.Width, 25);
            graphics.DrawRectangle(new XPen(Border, 0.4), x, y, column.Width, 25);
            var fitted = FitText(graphics, fonts.Cell, values[index] ?? string.Empty, column.Width - 10);
            graphics.DrawString(fitted, fonts.Cell, new XSolidBrush(index == 7 && row.Open > 0 ? Ink : Muted), new XRect(x + 5, y + 8, column.Width - 10, 11), column.RightAligned ? XStringFormats.TopRight : XStringFormats.TopLeft);
            x += column.Width;
        }

        return y + 25;
    }

    private static void DrawFooters(PdfDocument document, ReportFonts fonts, DateTime generatedLocal)
    {
        for (var index = 0; index < document.PageCount; index++)
        {
            var page = document.Pages[index];
            using var graphics = XGraphics.FromPdfPage(page, XGraphicsPdfPageOptions.Append);
            graphics.DrawLine(new XPen(Border, 0.6), 28, page.Height.Point - 28, page.Width.Point - 28, page.Height.Point - 28);
            graphics.DrawString(
                $"GenesisGest.Net v{ReleaseVersion} | dados persistidos | {generatedLocal:dd/MM/yyyy HH:mm:ss}",
                fonts.Footer,
                new XSolidBrush(Muted),
                new XRect(28, page.Height.Point - 22, 500, 12),
                XStringFormats.TopLeft);
            graphics.DrawString(
                $"Página {index + 1} de {document.PageCount}",
                fonts.Footer,
                new XSolidBrush(Muted),
                new XRect(page.Width.Point - 180, page.Height.Point - 22, 152, 12),
                XStringFormats.TopRight);
        }
    }

    private static PdfPage AddLandscapePage(PdfDocument document)
    {
        var page = document.AddPage();
        page.Size = PageSize.A4;
        page.Orientation = PdfSharp.PageOrientation.Landscape;
        return page;
    }

    private static ReportColumn[] GetColumns(string settledColumn) =>
    [
        new("Documento", 86, false),
        new("Descrição", 229, false),
        new("Emissão", 70, false),
        new("Vencimento", 76, false),
        new("Status", 78, false),
        new("Original", 82, true),
        new(string.IsNullOrWhiteSpace(settledColumn) ? "Liquidado" : settledColumn, 82, true),
        new("Em aberto", 82, true)
    ];

    private static string FitText(XGraphics graphics, XFont font, string value, double availableWidth)
    {
        var normalized = string.Join(' ', value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
        if (graphics.MeasureString(normalized, font).Width <= availableWidth)
        {
            return normalized;
        }

        const string suffix = "...";
        var low = 0;
        var high = normalized.Length;
        while (low < high)
        {
            var middle = (low + high + 1) / 2;
            var candidate = normalized[..middle].TrimEnd() + suffix;
            if (graphics.MeasureString(candidate, font).Width <= availableWidth)
            {
                low = middle;
            }
            else
            {
                high = middle - 1;
            }
        }

        return normalized[..low].TrimEnd() + suffix;
    }

    private static string BuildFilters(DateTime? start, DateTime? end, string? status)
    {
        var period = start.HasValue || end.HasValue
            ? $"Vencimento: {(start.HasValue ? start.Value.ToString("dd/MM/yyyy", ReportCulture) : "início")} a {(end.HasValue ? end.Value.ToString("dd/MM/yyyy", ReportCulture) : "sem limite")}"
            : "Vencimento: todos os períodos";
        return string.IsNullOrWhiteSpace(status) ? period : $"{period} | Status: {status}";
    }

    private static string FormatMoney(decimal value) => value.ToString("C2", ReportCulture);

    private static bool RegisterEmbeddedFont()
    {
        if (GlobalFontSettings.FontResolver is null)
        {
            GlobalFontSettings.FontResolver = new EmbeddedGenesisFontResolver();
        }

        if (GlobalFontSettings.FontResolver is not EmbeddedGenesisFontResolver)
        {
            throw new InvalidOperationException("O resolvedor global de fontes PDF foi ocupado por outro componente antes do relatorio financeiro.");
        }

        return true;
    }

    private static TimeZoneInfo ResolveSaoPauloTimeZone()
    {
        foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
            }
        }

        throw new InvalidOperationException("Fuso America/Sao_Paulo nao esta instalado no servidor.");
    }

    private sealed class EmbeddedGenesisFontResolver : IFontResolver
    {
        private const string FaceName = "GenesisSans#Regular";
        private static readonly Lazy<byte[]> FontBytes = new(LoadFontBytes, LazyThreadSafetyMode.ExecutionAndPublication);

        public FontResolverInfo? ResolveTypeface(string familyName, bool bold, bool italic)
        {
            if (!string.Equals(familyName, FontFamily, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return new FontResolverInfo(FaceName, bold, italic);
        }

        public byte[]? GetFont(string faceName) =>
            string.Equals(faceName, FaceName, StringComparison.Ordinal) ? FontBytes.Value : null;

        private static byte[] LoadFontBytes()
        {
            var assembly = typeof(FinancePdfReportService).Assembly;
            var resourceName = assembly.GetManifestResourceNames()
                .SingleOrDefault(name => name.EndsWith(".DejaVuSans.ttf", StringComparison.Ordinal));
            if (resourceName is null)
            {
                throw new InvalidOperationException("Fonte DejaVu Sans incorporada nao foi encontrada no assembly da API.");
            }

            using var stream = assembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException("Fonte DejaVu Sans incorporada nao pode ser aberta.");
            using var memory = new MemoryStream();
            stream.CopyTo(memory);
            var bytes = memory.ToArray();
            if (bytes.Length < 100_000)
            {
                throw new InvalidOperationException("Fonte DejaVu Sans incorporada esta incompleta.");
            }

            return bytes;
        }
    }

    private sealed record FinanceReportRow(
        string Document,
        string Description,
        DateTime IssuedAt,
        DateTime DueAt,
        string Status,
        decimal Original,
        decimal Settled,
        decimal Open);

    private sealed record ReportColumn(string Label, double Width, bool RightAligned);

    private sealed record ReportFonts(
        XFont Title,
        XFont Body,
        XFont TableHeader,
        XFont Cell,
        XFont Small,
        XFont Footer);
}
