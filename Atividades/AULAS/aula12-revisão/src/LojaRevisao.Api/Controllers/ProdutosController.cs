using LojaRevisao.Api.Data;
using LojaRevisao.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LojaRevisao.Api.Controllers;

[ApiController]
[Route("api/v1/produtos")]
public class ProdutosController : ControllerBase
{
    private readonly LojaDbContext _db;

    public ProdutosController(LojaDbContext db) => _db = db;

    // FALHA 04 — AULA 04 — Revisão de REST: paginação
    [HttpGet]
    public IActionResult Listar([FromQuery] int page = 1, [FromQuery] int size = 20)
    {
        page = Math.Max(1, page);
        size = Math.Clamp(size, 1, 100);

        var total = _db.Produtos.Count();
        var itens = _db.Produtos
            .OrderBy(p => p.Id)
            .Skip((page - 1) * size)
            .Take(size)
            .ToList();

        return Ok(new Pagina<Produto>(page, size, total, itens));
    }

    // FALHA 07 — AULA 07 — Segurança, OWASP Injection
    [HttpGet("busca")]
    public IActionResult Buscar([FromQuery] string nome)
    {
        return Ok(_db.Produtos
            .Where(p => EF.Functions.Like(p.Nome, $"%{nome}%"))
            .Take(50)
            .ToList());
    }

    // FALHA 02 — AULA 02 — Fundamentos de REST: status code (Parte A)
    [HttpGet("{id:int}")]
    public IActionResult Obter(int id)
    {
        var produto = _db.Produtos.Find(id);
        if (produto is null) return NotFound();
        return Ok(produto);
    }

    // FALHA 02 — AULA 02 — Parte B
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public IActionResult Criar(NovoProduto novo)
    {
        var produto = new Produto { Nome = novo.Nome, Preco = novo.Preco, Estoque = novo.Estoque };
        _db.Produtos.Add(produto);
        _db.SaveChanges();
        return CreatedAtAction(nameof(Obter), new { id = produto.Id }, produto);
    }

    // FALHA 09 — AULA 09 — RBAC (Parte A)
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public IActionResult Remover(int id)
    {
        var produto = _db.Produtos.Find(id);
        if (produto is null) return NotFound();

        _db.Produtos.Remove(produto);
        _db.SaveChanges();
        return NoContent();
    }
}