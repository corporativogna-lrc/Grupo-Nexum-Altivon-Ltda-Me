namespace NexumAltivon_ERP.DTOs.Financeiro
{
    public class BancoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string ISPB { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    public class ContaBancariaCreateDto
    {
        public int BancoId { get; set; }
        public string Agencia { get; set; } = string.Empty;
        public string Conta { get; set; } = string.Empty;
        public string Digito { get; set; } = string.Empty;
        public string Titular { get; set; } = string.Empty;
        public string Tipo { get; set; } = "Corrente";
        public decimal SaldoInicial { get; set; }
        public string Observacoes { get; set; } = string.Empty;
    }

    public class ContaBancariaResponseDto
    {
        public int Id { get; set; }
        public string BancoNome { get; set; } = string.Empty;
        public string BancoCodigo { get; set; } = string.Empty;
        public string Agencia { get; set; } = string.Empty;
        public string Conta { get; set; } = string.Empty;
        public string Titular { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public decimal SaldoAtual { get; set; }
        public bool Ativo { get; set; }
    }

    public class MovimentacaoBancariaCreateDto
    {
        public int ContaBancariaId { get; set; }
        public string Tipo { get; set; } = "Credito";
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public int? ContaPagarId { get; set; }
        public int? ContaReceberId { get; set; }
    }

    public class MovimentacaoBancariaResponseDto
    {
        public int Id { get; set; }
        public DateTime Data { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string NumeroDocumento { get; set; } = string.Empty;
        public string StatusConciliacao { get; set; } = string.Empty;
    }
}