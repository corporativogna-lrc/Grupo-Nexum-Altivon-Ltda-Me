namespace NexumAltivon_ERP.DTOs.Financeiro
{
    public class PlanoContasDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Natureza { get; set; } = string.Empty;
        public int? PaiId { get; set; }
        public string PaiNome { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }

    public class CentroCustoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public bool Ativo { get; set; }
    }
}