using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum PerfilUsuario
{
    SuperAdmin,
    Admin,
    Gerente,
    Vendedor,
    Suporte,
    Financeiro
}

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("nome")]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [Column("senha_hash")]
    [MaxLength(255)]
    public string SenhaHash { get; set; } = string.Empty;

    [Column("perfil")]
    public PerfilUsuario Perfil { get; set; } = PerfilUsuario.Vendedor;

    [Column("avatar")]
    [MaxLength(255)]
    public string? Avatar { get; set; }

    [Column("telefone")]
    [MaxLength(20)]
    public string? Telefone { get; set; }

    [Column("ativo")]
    public bool Ativo { get; set; } = true;

    [Column("ultimo_login")]
    public DateTime? UltimoLogin { get; set; }

    [Column("token_refresh")]
    [MaxLength(255)]
    public string? TokenRefresh { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<CrmAtendimento>? Atendimentos { get; set; }
}
