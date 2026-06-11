using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon_ERP.Models.Fiscal
{
    [Table("erp_configuracoes_fiscais")]
    public class ConfiguracaoFiscal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int LojaId { get; set; }

        [Required, StringLength(20)]
        public string RegimeTributario { get; set; } = "SimplesNacional"; // SimplesNacional, LucroPresumido, LucroReal

        public decimal AliquotaSimplesNacional { get; set; } = 0;

        public decimal AliquotaICMSPadrao { get; set; } = 18;

        public decimal AliquotaIPIPadrao { get; set; } = 0;

        public decimal AliquotaPISPadrao { get; set; } = 0.65m;

        public decimal AliquotaCOFINSPadrao { get; set; } = 3;

        [StringLength(200)]
        public string CertificadoDigital { get; set; } = string.Empty;

        public DateTime? ValidadeCertificado { get; set; }

        [StringLength(500)]
        public string CaminhoSchemas { get; set; } = string.Empty;

        public bool AmbienteProducao { get; set; } = false;

        [StringLength(100)]
        public string SerieNFe { get; set; } = "1";

        [StringLength(100)]
        public string SerieNFCe { get; set; } = "1";

        public int UltimoNumeroNFe { get; set; } = 0;

        public int UltimoNumeroNFCe { get; set; } = 0;

        public bool Ativo { get; set; } = true;

        public DateTime CriadoEm { get; set; } = DateTime.Now;

        public DateTime? AtualizadoEm { get; set; }
    }
}