using System.Security.Claims;
using ApiVazada.Data;
using ApiVazada.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiVazada.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly AppDbContext _db;

    public UsuariosController(AppDbContext db) => _db = db;

    // Helper para extrair o ID do token (bônus de segurança)
    private int UsuarioLogadoId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [Authorize]
    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        var usuario = _db.Usuarios.Find(id);
        if (usuario is null) return NotFound();
        return Ok(new { usuario.Id, usuario.Nome, usuario.Email, usuario.Role });
    }

    // CORREÇÃO: DTO de entrada com apenas o que o cliente pode mudar
    public record AtualizarUsuarioRequest(string Nome, string Email);

    [Authorize]
    [HttpPut("{id}")]
    public IActionResult Atualizar(int id, [FromBody] AtualizarUsuarioRequest dados)
    {
        var usuario = _db.Usuarios.Find(id);
        if (usuario is null) return NotFound();

        // Bônus: checagem de dono (proteção contra IDOR)
        if (usuario.Id != UsuarioLogadoId) return Forbid();

        // Mapeamento manual e seguro
        usuario.Nome = dados.Nome;
        usuario.Email = dados.Email;
        // Role NÃO é tocada: mudar papel é operação administrativa.

        _db.SaveChanges();
        return Ok(new { usuario.Id, usuario.Nome, usuario.Email, usuario.Role });
    }
}
