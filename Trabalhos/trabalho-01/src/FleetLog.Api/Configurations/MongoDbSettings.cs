namespace FleetLog.Api.Configurations;

public class MongoDbSettings
{
    public const string SectionName = "MongoDbSettings";

    public string ConnectionString {get ; set ;} = string.Empty;
    public string DatabaseName {get; set;} = string.Empty;
    public string VeiculosCollectionName {get; set;}="veiculos";
    public string OrdensCargaCollectionName{get; set;} = "ordens_carga";
}