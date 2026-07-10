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

public enum TipoCliente
{
    PF,
    PJ
}

public enum StatusCliente
{
    Ativo,
    Inativo,
    Bloqueado,
    Pendente
}

[Table("clientes")]
public class Cliente
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("tipo")]
    public TipoCliente Tipo { get; set; } = TipoCliente.PF;

    [Required]
    [Column("nome")]
    [MaxLength(150)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [Column("email")]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Column("senha_hash")]
    [MaxLength(255)]
    public string? SenhaHash { get; set; }

    [Column("cpf_cnpj")]
    [MaxLength(18)]
    public string? CpfCnpj { get; set; }

    [Column("rg_ie")]
    [MaxLength(20)]
    public string? RgIe { get; set; }

    [Column("data_nascimento")]
    public DateTime? DataNascimento { get; set; }

    [Column("telefone")]
    [MaxLength(20)]
    public string? Telefone { get; set; }

    [Column("whatsapp")]
    [MaxLength(20)]
    public string? Whatsapp { get; set; }

    [Column("avatar")]
    [MaxLength(255)]
    public string? Avatar { get; set; }

    [Column("newsletter")]
    public bool Newsletter { get; set; } = true;

    [Column("vip")]
    public bool Vip { get; set; } = false;

    [Column("pontos_fidelidade")]
    public int PontosFidelidade { get; set; } = 0;

    [Column("status")]
    public StatusCliente Status { get; set; } = StatusCliente.Pendente;

    [Column("ultimo_acesso")]
    public DateTime? UltimoAcesso { get; set; }

    [Column("token_reset_senha")]
    [MaxLength(255)]
    public string? TokenResetSenha { get; set; }

    [Column("token_confirmacao_email")]
    [MaxLength(255)]
    public string? TokenConfirmacaoEmail { get; set; }

    [Column("confirmado_em")]
    public DateTime? ConfirmadoEm { get; set; }

    [Column("token_expira_em")]
    public DateTime? TokenExpiraEm { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties
    public ICollection<Endereco>? Enderecos { get; set; }
    public ICollection<Pedido>? Pedidos { get; set; }
    public ICollection<Carrinho>? Carrinhos { get; set; }
}
