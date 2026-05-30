namespace NexumAltivon.API.Services;

public class MelhorEnvioService : IMelhorEnvioService
{
    private readonly IConfiguration _configuration;

    public MelhorEnvioService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<string?> CalcularFreteAsync(string cepOrigem, string cepDestino, decimal peso, decimal altura, decimal largura, decimal comprimento)
    {
        // TODO: Implementar integração real com API Melhor Envio
        // Referência: https://melhorenvio.com.br/
        await Task.CompletedTask;
        return "15.90";
    }

    public async Task<string?> GerarEtiquetaAsync(int pedidoId)
    {
        // TODO: Implementar geração de etiqueta via Melhor Envio
        await Task.CompletedTask;
        return $"https://melhorenvio.com.br/etiqueta/STUB_{pedidoId}";
    }
}
