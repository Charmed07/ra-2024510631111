using System.ComponentModel.DataAnnotations;

namespace BibliotecaApi.Models;

/// <summary>
/// Representa o empréstimo de um livro para um usuário da biblioteca.
/// </summary>
public class Emprestimo
{
    /// <summary>
    /// Identificador único do registro de empréstimo.
    /// </summary>
    /// <example>1</example>
    public int Id { get; set; }

    /// <summary>
    /// Identificador do livro emprestado.
    /// </summary>
    /// <example>1</example>
    [Required]
    public int LivroId { get; set; }

    /// <summary>
    /// Nome do usuário que realizou o empréstimo.
    /// </summary>
    /// <example>João Antonio</example>
    [Required(ErrorMessage = "O nome do usuário é obrigatório.")]
    [StringLength(200)]
    public string NomeUsuario { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora em que o empréstimo foi iniciado (UTC).
    /// </summary>
    /// <example>2026-10-24T20:00:00Z</example>
    public DateTime DataEmprestimo { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Data limite para devolução do exemplar (padrão de 14 dias).
    /// </summary>
    /// <example>2026-11-07T20:00:00Z</example>
    public DateTime DataDevolucaoPrevista { get; set; } = DateTime.UtcNow.AddDays(14);

    /// <summary>
    /// Data e hora em que a devolução foi confirmada (nula enquanto ativo).
    /// </summary>
    public DateTime? DataDevolucaoEfetiva { get; set; }

    /// <summary>
    /// Situação atual do empréstimo (Ativo, Devolvido ou Atrasado).
    /// </summary>
    /// <example>0</example>
    public StatusEmprestimo Status { get; set; } = StatusEmprestimo.Ativo;
}

/// <summary>
/// Status do empréstimo do livro.
/// </summary>
public enum StatusEmprestimo
{
    /// <summary>Empréstimo em andamento dentro do prazo.</summary>
    Ativo,
    /// <summary>Livro devolvido com sucesso.</summary>
    Devolvido,
    /// <summary>Prazo de devolução expirado.</summary>
    Atrasado
}