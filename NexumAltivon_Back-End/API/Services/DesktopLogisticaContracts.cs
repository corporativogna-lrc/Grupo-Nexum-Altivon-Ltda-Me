namespace NexumAltivon.API.Services;

public interface ILogAuditoriaService
{
    Task RegistrarAsync(string tabela, int registroId, string acao, int? usuarioId, string usuarioTipo,
        string? ip, string? userAgent, string? dadosAnteriores, string? dadosNovos, string? endpoint);
    Task RegistrarAsync(string tabela, string descricao, int? registroId);
}
