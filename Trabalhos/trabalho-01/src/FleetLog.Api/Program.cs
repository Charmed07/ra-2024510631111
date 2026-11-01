using FleetLog.Api.Configurations;
using FleetLog.Api.Data;
using FleetLog.Api.Services;
var builder = WebApplication.CreateBuilder(args);
// Configuração do MongoDbSettings via Options Pattern
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection(MongoDbSettings.SectionName));
// Injeção de Dependências (Singleton/Scoped)
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddScoped<IVeiculoService, VeiculoService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();
// Configuração do Swagger no pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
