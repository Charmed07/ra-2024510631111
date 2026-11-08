using System.Security.Claims;
using LojaRevisao.Api.Models;
using LojaRevisao.Api.Modulos;
using LojaRevisao.Api.Modulos.Pedidos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LojaRevisao.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/pedidos")]
public class PedidosController : ControllerBase
{
    private readonly PedidoService _service;

    public PedidosController(PedidoService service) => _service = service;

    private int UsuarioLogadoId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool PodeAcessar(Pedido pedido) =>
        User.IsInRole("Admin") || pedido.UsuarioId == UsuarioLogadoId;

    /// <summary>Lista os pedidos do usuário logado.</summary>
    [HttpGet]
    public IActionResult Listar() => Ok(_service.ListarDoUsuario(UsuarioLogadoId));

    // FALHA 09 — AULA 09 — ABAC (Parte B)
    [HttpGet("{id:int}")]
    public IActionResult Obter(int id)
    {
        var pedido = _service.Obter(id);
        if (pedido is null) return NotFound();
        if (!PodeAcessar(pedido)) return Forbid();
        return Ok(pedido);
    }

    // FALHA 10 — AULA 10 — Contrato OpenAPI (Parte A)
    [HttpPost]
    [ProducesResponseType(typeof(Pedido), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Criar(NovoPedido novo)
    {
        try
        {
            var pedido = await _service.CriarAsync(UsuarioLogadoId, novo);
            return CreatedAtAction(nameof(Obter), new { id = pedido.Id }, pedido);
        }
        catch (RegraDeNegocioException ex)
        {
            return Problem(detail: ex.Message, statusCode: ex.Status, title: ex.Titulo);
        }
    }

    // FALHA 03 — AULA 03 — Design de APIs (HttpDelete e sem corpo)
    [HttpDelete("{id:int}")]
    public IActionResult Cancelar(int id)
    {
        var pedido = _service.Obter(id);
        if (pedido is null) return NotFound();
        if (!PodeAcessar(pedido)) return Forbid();

        _service.Cancelar(pedido);
        return NoContent();
    }
}