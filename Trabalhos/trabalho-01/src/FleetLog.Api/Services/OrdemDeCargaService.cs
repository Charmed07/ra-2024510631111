using FleetLog.Api.Data;
using FleetLog.Api.Models;
using MongoDB.Driver;

namespace FleetLog.Api.Services;

public class OrdemDeCargaService : IOrdemDeCargaService
{
 private readonly MongoDbContext _context;

 public OrdemDeCargaService(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrdemDeCarga>> ObterTodasAsync(string? veiculoId = null)
    {
        if (string.IsNullOrWhiteSpace(veiculoId))
        {
            return await _context.OrdensCarga.Find(_=>true).ToListAsync();
        } 

        return await _context.OrdensCarga.Find(o =>o.VeiculoId==veiculoId).ToListAsync();
    }

    public async Task<OrdemDeCarga?> ObterPorIdAsync (string id)
    {
        return await _context.OrdensCarga.Find( o => o.Id == id).FirstOrDefaultAsync();
    }

    public async Task<decimal> ObterPesoTotalAlocadoAsync(string veiculoId)
    {
        var ordensAtivas = await _context.OrdensCarga.Find(o => o.VeiculoId==veiculoId &&(o.StatusEntrega== StatusEntrega.Pendente || o.StatusEntrega == StatusEntrega.EmTransito)).ToListAsync();

        return ordensAtivas.Sum(o => o.PesoCargaKg);
    }

    public async Task<OrdemDeCarga> CriarAsync(OrdemDeCarga ordem)
    {
        var veiculo = await _context.Veiculos.Find(v => v.Id == ordem.VeiculoId).FirstOrDefaultAsync();
        
        if(veiculo == null)
        {
            throw new KeyNotFoundException($"Veiculo com ID {ordem.VeiculoId} não foi enconttrado");
        }

        if(veiculo.StatusOperacional != StatusOperacional.Disponivel)
        {
            throw new InvalidOperationException($"O Veiculo {veiculo.Placa} está com status '{veiculo.StatusOperacional}'e não pode receber novas cargas.");
        }

        var pesoAtualAlocado = await ObterPesoTotalAlocadoAsync(ordem.VeiculoId);
        var pesoFinal = pesoAtualAlocado + ordem.PesoCargaKg;

        if( pesoFinal > veiculo.CapacidadeCargaKg)
        {
            var pesoDisponivel = veiculo.CapacidadeCargaKg - pesoAtualAlocado;
            throw new InvalidOperationException(
                $"Capacidade excedida. O veiculo suporta {veiculo.CapacidadeCargaKg}.Kg"+
                $"Já alocado:{pesoAtualAlocado}kg.Disponivel:{pesoDisponivel}kg. Tentativa de carga:{ordem.PesoCargaKg}kg."
            );
        }

        ordem.DataCriacao = DateTime.UtcNow;
        ordem.StatusEntrega = StatusEntrega.Pendente;

        await _context.OrdensCarga.InsertOneAsync(ordem);
        return ordem;
    
    }

    public async Task<bool> AtualizarStatusAsync(string id , StatusEntrega novoStatus)
    {
        var update = Builders<OrdemDeCarga>.Update.Set(o => o.StatusEntrega,novoStatus);
        var resultado = await _context.OrdensCarga.UpdateOneAsync(o => o.Id ==id, update);
        return resultado.ModifiedCount> 0;
    }

    public async Task<bool> RemoverAsync (string id)
    {
      var resultado = await _context.OrdensCarga.DeleteOneAsync(o=> o.Id == id);
      return resultado.DeletedCount>0;  
    }

}
