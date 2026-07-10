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

[Table("sys_tenants")]
public class Tenant : Sys_AuditableEntity
{
    [Required]
    [Column("codigo")]
    [MaxLength(50)]
    public string Codigo { get; set; } = string.Empty;

    [Required]
    [Column("nome")]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Column("documento")]
    [MaxLength(18)]
    public string? Documento { get; set; }

    [Column("loja_id")]
    public int? LojaId { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;
}
