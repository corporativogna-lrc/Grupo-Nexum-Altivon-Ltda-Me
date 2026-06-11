using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

[Table("erp_empresas_grupo")]
public class EmpresaGrupo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("tipo_cadastro")]
    [MaxLength(40)]
    public string TipoCadastro { get; set; } = "GrupoSocietario";

    [Required]
    [Column("razao_social")]
    [MaxLength(200)]
    public string RazaoSocial { get; set; } = string.Empty;

    [Column("nome_fantasia")]
    [MaxLength(200)]
    public string? NomeFantasia { get; set; }

    [Required]
    [Column("cnpj")]
    [MaxLength(18)]
    public string Cnpj { get; set; } = string.Empty;

    [Column("inscricao_estadual")]
    [MaxLength(30)]
    public string? InscricaoEstadual { get; set; }

    [Column("inscricao_municipal")]
    [MaxLength(30)]
    public string? InscricaoMunicipal { get; set; }

    [Column("matriz_filial")]
    [MaxLength(20)]
    public string? MatrizFilial { get; set; }

    [Column("codigo_empresa")]
    [MaxLength(50)]
    public string? CodigoEmpresa { get; set; }

    [Column("regime_tributario")]
    [MaxLength(60)]
    public string? RegimeTributario { get; set; }

    [Column("crt")]
    [MaxLength(10)]
    public string? Crt { get; set; }

    [Column("cnae_principal")]
    [MaxLength(20)]
    public string? CnaePrincipal { get; set; }

    [Column("cnaes_secundarios")]
    public string? CnaesSecundarios { get; set; }

    [Column("categoria_fiscal")]
    [MaxLength(100)]
    public string? CategoriaFiscal { get; set; }

    [Column("subcategoria_fiscal")]
    [MaxLength(100)]
    public string? SubcategoriaFiscal { get; set; }

    [Column("ncm_padrao")]
    [MaxLength(20)]
    public string? NcmPadrao { get; set; }

    [Column("natureza_operacao_padrao")]
    [MaxLength(120)]
    public string? NaturezaOperacaoPadrao { get; set; }

    [Column("responsavel_legal")]
    [MaxLength(150)]
    public string? ResponsavelLegal { get; set; }

    [Column("responsavel_fiscal")]
    [MaxLength(150)]
    public string? ResponsavelFiscal { get; set; }

    [Column("email_fiscal")]
    [MaxLength(150)]
    public string? EmailFiscal { get; set; }

    [Column("email_comercial")]
    [MaxLength(150)]
    public string? EmailComercial { get; set; }

    [Column("telefone")]
    [MaxLength(25)]
    public string? Telefone { get; set; }

    [Column("whatsapp")]
    [MaxLength(25)]
    public string? Whatsapp { get; set; }

    [Column("cep")]
    [MaxLength(12)]
    public string? Cep { get; set; }

    [Column("logradouro")]
    [MaxLength(200)]
    public string? Logradouro { get; set; }

    [Column("numero")]
    [MaxLength(20)]
    public string? Numero { get; set; }

    [Column("complemento")]
    [MaxLength(120)]
    public string? Complemento { get; set; }

    [Column("bairro")]
    [MaxLength(120)]
    public string? Bairro { get; set; }

    [Column("cidade")]
    [MaxLength(120)]
    public string? Cidade { get; set; }

    [Column("estado")]
    [MaxLength(2)]
    public string? Estado { get; set; }

    [Column("pais")]
    [MaxLength(60)]
    public string? Pais { get; set; }

    [Column("ambiente_nfe")]
    [MaxLength(30)]
    public string? AmbienteNfe { get; set; }

    [Column("serie_nfe")]
    [MaxLength(10)]
    public string? SerieNfe { get; set; }

    [Column("serie_nfce")]
    [MaxLength(10)]
    public string? SerieNfce { get; set; }

    [Column("modelo_documento_pdv")]
    [MaxLength(20)]
    public string? ModeloDocumentoPdv { get; set; }

    [Column("ambiente_nfce")]
    [MaxLength(30)]
    public string? AmbienteNfce { get; set; }

    [Column("proxima_nfce_numero")]
    public int? ProximaNfceNumero { get; set; }

    [Column("nfce_csc")]
    [MaxLength(120)]
    public string? NfceCsc { get; set; }

    [Column("nfce_csc_id_token")]
    [MaxLength(20)]
    public string? NfceCscIdToken { get; set; }

    [Column("pdv_serie_sat")]
    [MaxLength(20)]
    public string? PdvSerieSat { get; set; }

    [Column("pdv_impressora_fiscal")]
    [MaxLength(120)]
    public string? PdvImpressoraFiscal { get; set; }

    [Column("pdv_nome_caixa_padrao")]
    [MaxLength(80)]
    public string? PdvNomeCaixaPadrao { get; set; }

    [Column("pdv_contingencia_offline")]
    public bool PdvContingenciaOffline { get; set; }

    [Column("proxima_nfe_numero")]
    public int? ProximaNfeNumero { get; set; }

    [Column("cfop_padrao_interno")]
    [MaxLength(10)]
    public string? CfopPadraoInterno { get; set; }

    [Column("cfop_padrao_interestadual")]
    [MaxLength(10)]
    public string? CfopPadraoInterestadual { get; set; }

    [Column("aliquota_icms_interna")]
    public decimal? AliquotaIcmsInterna { get; set; }

    [Column("aliquota_icms_interestadual")]
    public decimal? AliquotaIcmsInterestadual { get; set; }

    [Column("aliquota_pis")]
    public decimal? AliquotaPis { get; set; }

    [Column("aliquota_cofins")]
    public decimal? AliquotaCofins { get; set; }

    [Column("aliquota_iss")]
    public decimal? AliquotaIss { get; set; }

    [Column("aliquota_ipi")]
    public decimal? AliquotaIpi { get; set; }

    [Column("carga_tributaria_percentual")]
    public decimal? CargaTributariaPercentual { get; set; }

    [Column("perfil_tributacao")]
    [MaxLength(40)]
    public string? PerfilTributacao { get; set; }

    [Column("usa_st_legado")]
    public bool UsaStLegado { get; set; }

    [Column("destaca_icms_st_separado")]
    public bool DestacaIcmsStSeparado { get; set; }

    [Column("custo_operacional_percentual")]
    public decimal? CustoOperacionalPercentual { get; set; }

    [Column("margem_minima_percentual")]
    public decimal? MargemMinimaPercentual { get; set; }

    [Column("prioridade_fiscal")]
    public int PrioridadeFiscal { get; set; } = 100;

    [Column("permite_nfe_entrada")]
    public bool PermiteNfeEntrada { get; set; } = true;

    [Column("permite_nfe_saida")]
    public bool PermiteNfeSaida { get; set; } = true;

    [Column("permite_dropshipping")]
    public bool PermiteDropshipping { get; set; }

    [Column("permite_marketplace")]
    public bool PermiteMarketplace { get; set; }

    [Column("emitente_preferencial")]
    public bool EmitentePreferencial { get; set; }

    [Column("ativa")]
    public bool Ativa { get; set; } = true;

    [Column("beneficios_estrategicos")]
    public string? BeneficiosEstrategicos { get; set; }

    [Column("contrato_resumo")]
    public string? ContratoResumo { get; set; }

    [Column("observacoes")]
    public string? Observacoes { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
