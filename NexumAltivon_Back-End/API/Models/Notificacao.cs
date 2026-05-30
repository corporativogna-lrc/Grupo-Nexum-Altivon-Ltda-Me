using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NexumAltivon.API.Models;

public enum TipoNotificacao
{
    Sistema,
    Pedido,
    Pagamento,
    Envio,
    CRM,
    Marketing,
    Seguranca
}

public enum DestinatarioTipo
{
    Cliente,
    Usuario,
    Todos
}

[Table("notificacoes")]
public class Notificacao
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("tipo")]
    public TipoNotificacao Tipo { get; set; } = TipoNotificacao.Sistema;

    [Required]
    [Column("titulo")]
    [MaxLength(200)]
    public string Titulo { get; set; } = string.Empty;

    [Column("mensagem")]
    public string? Mensagem { get; set; }

    [Column("destinatario_tipo")]
    public DestinatarioTipo DestinatarioTipo { get; set; } = DestinatarioTipo.Todos;

    [Column("destinatario_id")]
    public int? DestinatarioId { get; set; }

    [Column("lida")]
    public bool Lida { get; set; } = false;

    [Column("data_leitura")]
    public DateTime? DataLeitura { get; set; }

    [Column("link_acao")]
    [MaxLength(255)]
    public string? LinkAcao { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
