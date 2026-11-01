using FleetLog.Api.Models;

namespace FleetLog.Api.Services;

public interface IVeiculoService
{
    Task<List<Veiculo>>ObterTodosAsync();
    Task<Veiculo?> ObterPorIdAsync(String id);
    Task<Veiculo> CriarAsync(Veiculo veiculo);
    Task<bool> AtualizarAsync(string id, Veiculo veiculo);
    Task<bool> RemoverAsync (string id);
}