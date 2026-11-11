
using Binova.Api.Dados;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Obtém a conexão Oracle das configurações da aplicação
string? conexao = builder.Configuration.GetConnectionString("Oracle");

if (string.IsNullOrWhiteSpace(conexao))
{
    throw new InvalidOperationException(
        "A conexão com o Oracle não foi configurada. " +
        "Crie o arquivo appsettings.Development.json " +
        "a partir do exemplo e consulte o README."
    );
}

// Registra o Entity Framework Core com o banco Oracle
builder.Services.AddDbContext<BinovaDbContext>(opcoes =>
{
    opcoes.UseOracle(conexao, oracle =>
    {
        oracle.MigrationsHistoryTable("BINOVA_EF_MIGRATIONS");
    });
});

// Habilita os controllers da API
builder.Services.AddControllers();

// Padroniza as respostas para dados inválidos
builder.Services.Configure<ApiBehaviorOptions>(opcoes =>
{
    opcoes.InvalidModelStateResponseFactory = contexto =>
    {
        var detalhes = contexto.ModelState
            .Where(campo => campo.Value?.Errors.Count > 0)
            .ToDictionary(
                campo => campo.Key,
                campo => campo.Value!.Errors
                    .Select(erro => erro.ErrorMessage)
                    .ToArray()
            );

        return new BadRequestObjectResult(new
        {
            erro = "Dados inválidos.",
            detalhes
        });
    };
});

// Inicializa a aplicação e disponibiliza as rotas
var app = builder.Build();

app.MapControllers();

app.Run();
