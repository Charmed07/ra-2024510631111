using FleetLog.Api.Data;
using FleetLog.Api.Models;
using MongoDB.Driver;

namespace FleetLog.Api.Services;

public class VeiculoService : IVeiculoService
{
    private readonly MongoDbContext _context;
    public VeiculoService(MongoDbContext context)
    {
        _context = context;
    }
    public async Task<List<Veiculo>> ObterTodosAsync()
    {
        return await _context.Veiculos.Find(_ => true).ToListAsync();
    }
    public async Task<Veiculo?> ObterPorIdAsync(string id)
    {
        return await _context.Veiculos.Find(v => v.Id == id).FirstOrDefaultAsync();
    }
    public async Task<Veiculo> CriarAsync(Veiculo veiculo)
    {
        await _context.Veiculos.InsertOneAsync(veiculo);
        return veiculo;
    }
    public async Task<bool> AtualizarAsync(string id, Veiculo veiculo)
    {
        veiculo.Id = id;
        var resultado = await _context.Veiculos.ReplaceOneAsync(v => v.Id == id, veiculo);
        return resultado.ModifiedCount > 0;
    }
    public async Task<bool> RemoverAsync(string id)
    {
        var resultado = await _context.Veiculos.DeleteOneAsync(v => v.Id == id);
        return resultado.DeletedCount > 0;
    }

}