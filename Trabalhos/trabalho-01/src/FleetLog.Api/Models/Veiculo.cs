using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FleetLog.Api.Models;

public class Veiculo
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id{get; set;}

    [BsonElement("placa")]
    public string Placa{ get; set;} = string.Empty;

    [BsonElement("modelo")]
    public string Modelo{ get ; set;} = string.Empty;

    [BsonElement("capacidadeCargaKg")]
    public decimal CapacidadeCargaKg{get;set;}

    [BsonElement("anoFabricacao")]
    public int AnoFabricacao{get;set;}

    [BsonElement("statusOperacional")]
    [BsonRepresentation(BsonType.String)]
    public StatusOperacional StatusOperacional{get;set;} = StatusOperacional.Disponivel;

}