using FleetLog.Api.Models;
using FleetLog.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace FleetLog.Api.Cotrollers;

[ApiController]
[Route("api/ordens-carga")]
public class OrdensCargaController : ControllerBase
{
    private readonly IOrdemDeCargaService _ordemService;

    public OrdensCargaController(IOrdemDeCargaService ordemService)
    {
        _ordemService = ordemService;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrdemDeCarga>>> ObterTodas([FromQuery] string? veiculoId = null)
    {
       var ordens = await _ordemService.ObterTodasAsync(veiculoId);
       return Ok(ordens); 
    } 

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<ActionResult<OrdemDeCarga>>ObterPorId(string id)
    {
        var ordem = await _ordemService.ObterPorIdAsync(id);
        if (ordem == null)
        {
            return NotFound (new {mensagem=$"Ordem de carga com ID {id} não encontrada."});
        }
        return Ok (ordem);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
     public async Task<ActionResult<OrdemDeCarga>> Criar([FromBody] OrdemDeCarga ordem)
    {
        if (ordem.PesoCargaKg <= 0)
        {
            return BadRequest(new { mensagem = "O peso da carga deve ser maior que zero." });
        }
        try
        {
            var novaOrdem = await _ordemService.CriarAsync(ordem);
            return CreatedAtAction(nameof(ObterPorId), new { id = novaOrdem.Id }, novaOrdem);
        }
        catch (KeyNotFoundException ex)
        {
            
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
          
            return BadRequest(new { mensagem = ex.Message });
        }

    }

    [HttpPut("{id}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarStatus(string id, [FromBody] StatusEntrega novoStatus)
    {
        var atualizado = await _ordemService.AtualizarStatusAsync(id, novoStatus);
        if (!atualizado)
        {
            return NotFound(new { mensagem = $"Ordem de carga com ID {id} não encontrada para atualização." });
        }
        return NoContent();
    }
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(string id)
    {
        var removido = await _ordemService.RemoverAsync(id);
        if (!removido)
        {
            return NotFound(new { mensagem = $"Ordem de carga com ID {id} não encontrada para exclusão." });
        }
        return NoContent();
    }

}