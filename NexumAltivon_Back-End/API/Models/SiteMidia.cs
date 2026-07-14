/*
 * Propriedade intelectual: Luís Rodrigo da Costa
 * Com apoio: IA Chatgpt/Codex que atende por nome: Sophia
 * Sistema de gestão: GenesisGest.Net
 * Ano Início: 04/2024 Publicado e operacional: 05/2026
 * Versão: 1.1.5
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoMidiaSite
{
    Logo,
    Banner,
    Loja,
    Institucional
}

[Table("site_midias")]
public sealed class SiteMidia : Sys_AuditableEntity
{
    [Required]
    [MaxLength(150)]
    [Column("nome")]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("tipo")]
    public TipoMidiaSite Tipo { get; set; }

    [Required]
    [MaxLength(240)]
    [Column("texto_alternativo")]
    public string TextoAlternativo { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("nome_arquivo_original")]
    public string NomeArquivoOriginal { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("caminho_relativo")]
    public string CaminhoRelativo { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    [Column("content_type")]
    public string ContentType { get; set; } = string.Empty;

    [Column("tamanho_bytes")]
    public long TamanhoBytes { get; set; }

    [Column("largura")]
    public int Largura { get; set; }

    [Column("altura")]
    public int Altura { get; set; }
}
