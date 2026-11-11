using Binova.Api.Dados;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

var conexao = builder.Configuration.GetConnectionString("Oracle");
if (string.IsNullOrWhiteSpace(conexao))
{
    throw new InvalidOperationException(
        "A conexao Oracle nao foi configurada. Crie appsettings.Development.json " +
        "a partir do arquivo de exemplo e consulte o README.");
}

builder.Services.AddDbContext<BinovaDbContext>(opcoes =>
    opcoes.UseOracle(conexao, oracle => oracle.MigrationsHistoryTable("BINOVA_EF_MIGRATIONS")));

builder.Services.AddControllers();
builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(opcoes =>
{
    opcoes.InvalidModelStateResponseFactory = contexto =>
        new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new
        {
            erro = "Dados invalidos.",
            detalhes = contexto.ModelState.Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(x => x.Key, x => x.Value!.Errors.Select(e => e.ErrorMessage).ToArray())
        });
});

var app = builder.Build();
app.MapControllers();
app.Run();
