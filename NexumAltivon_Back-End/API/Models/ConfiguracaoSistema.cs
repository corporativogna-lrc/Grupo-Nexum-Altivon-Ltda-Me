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

public enum TipoConfiguracao
{
    Texto,
    Numero,
    Booleano,
    JSON,
    Imagem,
    Senha
}

[Table("configuracoes_sistema")]
public class ConfiguracaoSistema
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("chave")]
    [MaxLength(100)]
    public string Chave { get; set; } = string.Empty;

    [Column("valor")]
    public string? Valor { get; set; }

    [Column("tipo")]
    public TipoConfiguracao Tipo { get; set; } = TipoConfiguracao.Texto;

    [Column("descricao")]
    [MaxLength(255)]
    public string? Descricao { get; set; }

    [Column("grupo")]
    [MaxLength(50)]
    public string? Grupo { get; set; }

    [Column("editavel")]
    public bool Editavel { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
