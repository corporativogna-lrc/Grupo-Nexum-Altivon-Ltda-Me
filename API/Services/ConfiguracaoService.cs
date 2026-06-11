using Microsoft.EntityFrameworkCore;
using NexumAltivon.API.Data;
using NexumAltivon.API.Models;

namespace NexumAltivon.API.Services;

public class ConfiguracaoService : IConfiguracaoService
{
    private readonly NexumDbContext _context;

    public ConfiguracaoService(NexumDbContext context)
    {
        _context = context;
    }

    public async Task<string?> ObterValorAsync(string chave)
    {
        var config = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave == chave);
        return config?.Valor;
    }

    public async Task<T?> ObterValorAsync<T>(string chave)
    {
        var valor = await ObterValorAsync(chave);
        if (valor == null) return default;

        try
        {
            return (T?)Convert.ChangeType(valor, typeof(T));
        }
        catch
        {
            return default;
        }
    }

    public async Task AtualizarValorAsync(string chave, string valor)
    {
        var config = await _context.ConfiguracoesSistema
            .FirstOrDefaultAsync(c => c.Chave == chave);

        if (config != null)
        {
            config.Valor = valor;
            config.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
