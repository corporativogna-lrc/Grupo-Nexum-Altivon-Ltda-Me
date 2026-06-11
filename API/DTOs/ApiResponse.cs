namespace NexumAltivon.API.DTOs;

public class ApiResponse<T>
{
    public bool Sucesso { get; set; } = true;
    public string? Mensagem { get; set; }
    public T? Dados { get; set; }
    public List<string>? Erros { get; set; }
    public int? TotalRegistros { get; set; }
    public int? PaginaAtual { get; set; }
    public int? TotalPaginas { get; set; }

    public static ApiResponse<T> Ok(T dados, string? mensagem = null, int? total = null, int? pagina = null, int? totalPaginas = null)
    {
        return new ApiResponse<T>
        {
            Sucesso = true,
            Mensagem = mensagem,
            Dados = dados,
            TotalRegistros = total,
            PaginaAtual = pagina,
            TotalPaginas = totalPaginas
        };
    }

    public static ApiResponse<T> Erro(string mensagem, List<string>? erros = null)
    {
        return new ApiResponse<T>
        {
            Sucesso = false,
            Mensagem = mensagem,
            Erros = erros
        };
    }
}

public class PaginacaoDto
{
    public int Pagina { get; set; } = 1;
    public int ItensPorPagina { get; set; } = 20;
    public string? OrdenarPor { get; set; }
    public bool OrdemDescendente { get; set; } = true;
    public string? Busca { get; set; }
}
