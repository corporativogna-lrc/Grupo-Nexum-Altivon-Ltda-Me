using Microsoft.EntityFrameworkCore;

namespace NexumAltivon.API.ERP.SharedData;

public static class GenesisRhService
{
    private static readonly HashSet<string> StatusPermitidos = new(StringComparer.OrdinalIgnoreCase)
    {
        "ATIVO",
        "AFASTADO",
        "FERIAS_PROGRAMADAS",
        "FERIAS",
        "DESLIGADO",
        "INATIVO"
    };

    public static string NormalizarStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
        {
            return "ATIVO";
        }

        var normalized = status.Trim().ToUpperInvariant();
        return StatusPermitidos.Contains(normalized) ? normalized : "ATIVO";
    }

    private static string NormalizarTipoReferencia(string? tipo) =>
        string.IsNullOrWhiteSpace(tipo) ? string.Empty : tipo.Trim().ToUpperInvariant();

    public static async Task<GenesisRhSummaryDto> GetResumoAsync(GenesisDbContext genesisDb, CancellationToken ct)
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var fimMes = inicioMes.AddMonths(1);

        var total = await genesisDb.RhColaboradores.CountAsync(ct);
        var ativos = await genesisDb.RhColaboradores.CountAsync(item => item.Status == "ATIVO", ct);
        var feriasProgramadas = await genesisDb.RhColaboradores.CountAsync(item => item.Status == "FERIAS_PROGRAMADAS", ct);
        var admissoesNoMes = await genesisDb.RhColaboradores.CountAsync(item => item.DataAdmissao >= inicioMes && item.DataAdmissao < fimMes, ct);
        var desligamentosNoMes = await genesisDb.RhColaboradores.CountAsync(item => item.Status == "DESLIGADO", ct);

        return new GenesisRhSummaryDto(
            total,
            ativos,
            feriasProgramadas,
            admissoesNoMes,
            desligamentosNoMes,
            DateTime.UtcNow);
    }

    public static async Task<List<GenesisRhColaboradorDto>> GetColaboradoresAsync(GenesisDbContext genesisDb, CancellationToken ct)
    {
        return await genesisDb.RhColaboradores
            .AsNoTracking()
            .OrderBy(item => item.Nome)
            .Select(item => new GenesisRhColaboradorDto(
                item.Id,
                item.Nome,
                item.Cargo,
                item.Departamento,
                item.Status,
                item.DataAdmissao))
            .ToListAsync(ct);
    }

    public static async Task<GenesisRhColaboradorDto> CriarColaboradorAsync(GenesisDbContext genesisDb, GenesisRhColaboradorUpsertRequest request, CancellationToken ct)
    {
        var nome = (request.Nome ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do colaborador é obrigatório.");
        }

        var entity = new GenesisRhColaborador
        {
            Nome = nome,
            Cargo = request.Cargo?.Trim(),
            Departamento = request.Departamento?.Trim(),
            Status = NormalizarStatus(request.Status),
            DataAdmissao = request.DataAdmissao
        };

        genesisDb.RhColaboradores.Add(entity);
        await genesisDb.SaveChangesAsync(ct);

        return new GenesisRhColaboradorDto(entity.Id, entity.Nome, entity.Cargo, entity.Departamento, entity.Status, entity.DataAdmissao);
    }

    public static async Task<GenesisRhColaboradorDto?> AtualizarColaboradorAsync(GenesisDbContext genesisDb, int id, GenesisRhColaboradorUpsertRequest request, CancellationToken ct)
    {
        var entity = await genesisDb.RhColaboradores.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (entity is null)
        {
            return null;
        }

        var nome = (request.Nome ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(nome))
        {
            throw new InvalidOperationException("Nome do colaborador é obrigatório.");
        }

        entity.Nome = nome;
        entity.Cargo = request.Cargo?.Trim();
        entity.Departamento = request.Departamento?.Trim();
        entity.Status = NormalizarStatus(request.Status);
        entity.DataAdmissao = request.DataAdmissao;

        await genesisDb.SaveChangesAsync(ct);
        return new GenesisRhColaboradorDto(entity.Id, entity.Nome, entity.Cargo, entity.Departamento, entity.Status, entity.DataAdmissao);
    }

    public static async Task<GenesisRhColaboradorDto?> AtualizarStatusAsync(GenesisDbContext genesisDb, int id, string? status, CancellationToken ct)
    {
        var entity = await genesisDb.RhColaboradores.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (entity is null)
        {
            return null;
        }

        entity.Status = NormalizarStatus(status);
        await genesisDb.SaveChangesAsync(ct);

        return new GenesisRhColaboradorDto(entity.Id, entity.Nome, entity.Cargo, entity.Departamento, entity.Status, entity.DataAdmissao);
    }

    public static async Task<List<GenesisRhReferenciaDto>> ListarReferenciasAsync(GenesisDbContext genesisDb, string? tipo, CancellationToken ct)
    {
        var tipoNormalizado = NormalizarTipoReferencia(tipo);

        var query = genesisDb.RhReferencias
            .AsNoTracking()
            .Where(item => item.Ativo);

        if (!string.IsNullOrWhiteSpace(tipoNormalizado))
        {
            query = query.Where(item => item.Tipo == tipoNormalizado);
        }

        return await query
            .OrderBy(item => item.Tipo)
            .ThenBy(item => item.Ordem)
            .ThenBy(item => item.Descricao)
            .Select(item => new GenesisRhReferenciaDto(
                item.Id,
                item.Tipo,
                item.Codigo,
                item.Descricao,
                item.Ordem))
            .ToListAsync(ct);
    }

    public static async Task<GenesisRhReferenciaDto> CriarReferenciaAsync(GenesisDbContext genesisDb, GenesisRhReferenciaCreateRequest request, CancellationToken ct)
    {
        var tipo = NormalizarTipoReferencia(request.Tipo);
        var codigo = request.Codigo?.Trim().ToUpperInvariant() ?? string.Empty;
        var descricao = request.Descricao?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tipo))
        {
            throw new ArgumentException("Tipo de referencia RH obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("Codigo de referencia RH obrigatorio.");
        }

        if (string.IsNullOrWhiteSpace(descricao))
        {
            throw new ArgumentException("Descricao de referencia RH obrigatoria.");
        }

        var entity = await genesisDb.RhReferencias
            .FirstOrDefaultAsync(item => item.Tipo == tipo && item.Codigo == codigo, ct);

        if (entity is null)
        {
            entity = new GenesisRhReferencia
            {
                Tipo = tipo,
                Codigo = codigo,
                Descricao = descricao,
                Ordem = request.Ordem,
                Ativo = true
            };
            genesisDb.RhReferencias.Add(entity);
        }
        else
        {
            entity.Descricao = descricao;
            entity.Ordem = request.Ordem;
            entity.Ativo = true;
        }

        await genesisDb.SaveChangesAsync(ct);

        return new GenesisRhReferenciaDto(entity.Id, entity.Tipo, entity.Codigo, entity.Descricao, entity.Ordem);
    }
}
