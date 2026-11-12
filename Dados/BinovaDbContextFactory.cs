
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Oracle.EntityFrameworkCore;

namespace Binova.Api.Dados;

public class BinovaDbContextFactory : IDesignTimeDbContextFactory<BinovaDbContext>
{
    // Configura o acesso ao Oracle para os comandos do Entity Framework Core
    public BinovaDbContext CreateDbContext(string[] args)
    {
        string ambiente =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT")
            ?? "Development";

        var configuracao = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{ambiente}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        string? conexao = configuracao.GetConnectionString("Oracle");

        if (string.IsNullOrWhiteSpace(conexao))
        {
            throw new InvalidOperationException(
                "A conexão com o Oracle não foi configurada. " +
                "Verifique o arquivo local de configuração ou a variável de ambiente."
            );
        }

        var opcoes = new DbContextOptionsBuilder<BinovaDbContext>()
            .UseOracle(conexao, oracle =>
            {
                oracle.MigrationsHistoryTable("BINOVA_EF_MIGRATIONS");
                oracle.UseOracleSQLCompatibility(
                    OracleSQLCompatibility.DatabaseVersion19
                );
            })
            .Options;

        return new BinovaDbContext(opcoes);
    }
}
