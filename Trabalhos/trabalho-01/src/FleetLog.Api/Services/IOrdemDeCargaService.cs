using FleetLog.Api.Models;

namespace FleetLog.Api.Services;

public interface IOrdemDeCargaService
{
    Task<List<OrdemDeCarga>> ObterTodasAsync(string? veiculoId = null);
    Task<OrdemDeCarga?> ObterPorIdAsync (string id);
    Task<OrdemDeCarga> CriarAsync (OrdemDeCarga ordem);
    Task<bool> AtualizarStatusAsync(string id, StatusEntrega novoStatus);
    Task<bool> RemoverAsync(string id);
    Task<decimal> ObterPesoTotalAlocadoAsync(string veiculoId);

}