using NexumAltivon.API.Data;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services;

public class LogAuditoriaService : ILogAuditoriaService
{
    private readonly NexumDbContext _context;

    public LogAuditoriaService(NexumDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarAsync(string tabela, int registroId, string acao, int? usuarioId, string usuarioTipo,
        string? ip, string? userAgent, string? dadosAnteriores, string? dadosNovos, string? endpoint)
    {
        try
        {
            if (!Enum.TryParse<AcaoAuditoria>(acao, out var acaoEnum))
                acaoEnum = AcaoAuditoria.API;

            if (!Enum.TryParse<TipoUsuarioAuditoria>(usuarioTipo, out var tipoUsuarioEnum))
                tipoUsuarioEnum = TipoUsuarioAuditoria.Sistema;

            var log = new LogAuditoria
            {
                Tabela = tabela,
                RegistroId = registroId,
                Acao = acaoEnum,
                UsuarioId = usuarioId,
                UsuarioTipo = tipoUsuarioEnum,
                IpAddress = ip,
                UserAgent = userAgent,
                DadosAnteriores = dadosAnteriores,
                DadosNovos = dadosNovos,
                Endpoint = endpoint,
                CreatedAt = DateTime.UtcNow
            };

            _context.LogsAuditoria.Add(log);
            await _context.SaveChangesAsync();
        }
        catch
        {
            // Silenciar falhas de auditoria para não quebrar o fluxo principal
        }
    }
}
