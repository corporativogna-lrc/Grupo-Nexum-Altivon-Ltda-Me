namespace NexumAltivon.API.DTOs;

public class ClienteDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "PF";
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public bool Vip { get; set; }
    public int PontosFidelidade { get; set; }
    public string Status { get; set; } = "Ativo";
    public DateTime? ConfirmadoEm { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<EnderecoDto>? Enderecos { get; set; }
}

public class CriarClienteDto
{
    public string Tipo { get; set; } = "PF";
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Senha { get; set; }
    public string? CpfCnpj { get; set; }
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public bool Newsletter { get; set; } = true;
    public bool Vip { get; set; } = false;
}

public class AtualizarClienteDto
{
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Whatsapp { get; set; }
    public bool Newsletter { get; set; }
    public bool Vip { get; set; }
    public string Status { get; set; } = "Ativo";
}

public class EnderecoDto
{
    public int Id { get; set; }
    public string Tipo { get; set; } = "Entrega";
    public string Apelido { get; set; } = "Principal";
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public bool Padrao { get; set; }
}

public class CriarEnderecoDto
{
    public string Tipo { get; set; } = "Entrega";
    public string Apelido { get; set; } = "Principal";
    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    public string? Cidade { get; set; }
    public string? Estado { get; set; }
    public bool Padrao { get; set; } = false;
}
