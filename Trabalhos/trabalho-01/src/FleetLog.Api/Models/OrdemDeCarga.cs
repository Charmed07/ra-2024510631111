using FleetLog.Api.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FleetLog.Api.Models;

public class OrdemDeCarga
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id {get;set;}

    [BsonElement("codigoRastreio")]
    public string CodigoRastreio {get; set ;} = string.Empty;

    [BsonElement("destinoCidade")]
    public string DestinoCidade{ get; set;} = string.Empty;

    [BsonElement("pesoCargaKg")]
    public decimal PesoCargaKg {get ; set;}

    [BsonElement("valorFrete")]
    public decimal ValorFrete {get; set;}

    [BsonElement("statusEntrega")]
    [BsonRepresentation(BsonType.String)]
    public StatusEntrega StatusEntrega {get; set;} = StatusEntrega.Pendente;

    [BsonElement("dataCriacao")]
    public DateTime DataCriacao {get; set;} = DateTime.UtcNow;

    [BsonElement("veiculoId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string VeiculoId {get;set;} = string.Empty;
}