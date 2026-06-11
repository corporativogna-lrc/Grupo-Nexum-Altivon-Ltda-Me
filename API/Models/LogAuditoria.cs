using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum AcaoAuditoria
{
    INSERT,
    UPDATE,
    DELETE,
    LOGIN,
    LOGOUT,
    ERRO,
    API
}

public enum TipoUsuarioAuditoria
{
    Cliente,
    Usuario,
    Sistema,
    API
}

[Table("logs_auditoria")]
public class LogAuditoria
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Required]
    [Column("tabela")]
    [MaxLength(50)]
    public string Tabela { get; set; } = string.Empty;

    [Required]
    [Column("registro_id")]
    public int RegistroId { get; set; }

    [Required]
    [Column("acao")]
    public AcaoAuditoria Acao { get; set; } = AcaoAuditoria.INSERT;

    [Column("usuario_id")]
    public int? UsuarioId { get; set; }

    [Column("usuario_tipo")]
    public TipoUsuarioAuditoria UsuarioTipo { get; set; } = TipoUsuarioAuditoria.Sistema;

    [Column("ip_address")]
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    [Column("user_agent")]
    [MaxLength(255)]
    public string? UserAgent { get; set; }

    [Column("dados_anteriores")]
    public string? DadosAnteriores { get; set; } // JSON

    [Column("dados_novos")]
    public string? DadosNovos { get; set; } // JSON

    [Column("endpoint")]
    [MaxLength(255)]
    public string? Endpoint { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
