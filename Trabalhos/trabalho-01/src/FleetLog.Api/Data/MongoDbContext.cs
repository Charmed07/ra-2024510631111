using FleetLog.Api.Models;
using FleetLog.Api.Configurations;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace FleetLog.Api.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;

        var connectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = _settings.ConnectionString;
        }

        var client = new MongoClient (connectionString);
        _database = client.GetDatabase(_settings.DatabaseName);
    }

    public IMongoCollection<Veiculo> Veiculos => _database.GetCollection<Veiculo>(_settings.VeiculosCollectionName);

    public IMongoCollection<OrdemDeCarga> OrdensCarga => _database.GetCollection<OrdemDeCarga>(_settings.OrdensCargaCollectionName);

}