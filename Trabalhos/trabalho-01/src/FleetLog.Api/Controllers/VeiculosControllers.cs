using FleetLog.Api.Models;
using FleetLog.Api.Services;
using Microsoft.AspNetCore.Mvc;
namespace FleetLog.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class VeiculosController : ControllerBase
{
    private readonly IVeiculoService _veiculoService;
    public VeiculosController(IVeiculoService veiculoService)
    {
        _veiculoService = veiculoService;
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<Veiculo>>> ObterTodos()
    {
        var veiculos = await _veiculoService.ObterTodosAsync();
        return Ok(veiculos);
    }
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Veiculo>> ObterPorId(string id)
    {
        var veiculo = await _veiculoService.ObterPorIdAsync(id);
        if (veiculo == null)
        {
            return NotFound(new { mensagem = $"Veículo com ID {id} não encontrado." });
        }
        return Ok(veiculo);
    }
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Veiculo>> Criar([FromBody] Veiculo veiculo)
    {
        if (string.IsNullOrWhiteSpace(veiculo.Placa))
        {
            return BadRequest(new { mensagem = "A placa do veículo é obrigatória." });
        }
        var novoVeiculo = await _veiculoService.CriarAsync(veiculo);
        return CreatedAtAction(nameof(ObterPorId), new { id = novoVeiculo.Id }, novoVeiculo);
    }
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Atualizar(string id, [FromBody] Veiculo veiculo)
    {
        var atualizado = await _veiculoService.AtualizarAsync(id, veiculo);
        if (!atualizado)
        {
            return NotFound(new { mensagem = $"Veículo com ID {id} não encontrado para atualização." });
        }
        return NoContent();
    }
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(string id)
    {
        var removido = await _veiculoService.RemoverAsync(id);
        if (!removido)
        {
            return NotFound(new { mensagem = $"Veículo com ID {id} não encontrado para exclusão." });
        }
        return NoContent();
    }
}