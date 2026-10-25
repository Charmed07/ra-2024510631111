namespace BibliotecaApi.Models;

/// <summary>
/// Envelope de resposta com metadados para consultas paginadas.
/// </summary>
/// <typeparam name="T">Tipo do item contido na página.</typeparam>
public class RespostaPaginada<T>
{
    /// <summary>
    /// Coleção dos itens da página corrente.
    /// </summary>
    public List<T> Itens { get; set; } = new();

    /// <summary>
    /// Número da página retornada.
    /// </summary>
    /// <example>1</example>
    public int Pagina { get; set; }

    /// <summary>
    /// Quantidade máxima de itens por página.
    /// </summary>
    /// <example>10</example>
    public int TamanhoPagina { get; set; }

    /// <summary>
    /// Total geral de itens existentes em todas as páginas.
    /// </summary>
    /// <example>25</example>
    public int TotalItens { get; set; }

    /// <summary>
    /// Total de páginas calculadas.
    /// </summary>
    /// <example>3</example>
    public int TotalPaginas => (int)Math.Ceiling((double)TotalItens / TamanhoPagina);
}