using System;

namespace NexumAltivon_ERP.DTOs.Fiscal
{
    public class ConfiguracaoFiscalCreateDto
    {
        public int LojaId { get; set; }
        public string RegimeTributario { get; set; } = "SimplesNacional";
        public decimal AliquotaSimplesNacional { get; set; }
        public decimal AliquotaICMSPadrao { get; set; } = 18;
        public decimal AliquotaIPIPadrao { get; set; }
        public decimal AliquotaPISPadrao { get; set; } = 0.65m;
        public decimal AliquotaCOFINSPadrao { get; set; } = 3;
        public string CertificadoDigital { get; set; } = string.Empty;
        public DateTime? ValidadeCertificado { get; set; }
        public string CaminhoSchemas { get; set; } = string.Empty;
        public bool AmbienteProducao { get; set; } = false;
        public string SerieNFe { get; set; } = "1";
        public string SerieNFCe { get; set; } = "1";
    }

    public class ConfiguracaoFiscalResponseDto
    {
        public int Id { get; set; }
        public int LojaId { get; set; }
        public string RegimeTributario { get; set; } = string.Empty;
        public decimal AliquotaSimplesNacional { get; set; }
        public decimal AliquotaICMSPadrao { get; set; }
        public bool AmbienteProducao { get; set; }
        public string SerieNFe { get; set; } = string.Empty;
        public string SerieNFCe { get; set; } = string.Empty;
        public int UltimoNumeroNFe { get; set; }
        public int UltimoNumeroNFCe { get; set; }
        public bool Ativo { get; set; }
    }

    public class SPEDCreateDto
    {
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string Tipo { get; set; } = "Fiscal";
        public int? LojaId { get; set; }
    }

    public class SPEDResponseDto
    {
        public int Id { get; set; }
        public int Ano { get; set; }
        public int Mes { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CaminhoArquivo { get; set; } = string.Empty;
        public int TotalRegistros { get; set; }
        public DateTime CriadoEm { get; set; }
    }

    public class CFOPDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public string Aplicacao { get; set; } = string.Empty;
    }
}