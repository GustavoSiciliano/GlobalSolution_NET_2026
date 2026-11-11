using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Binova.Api.Dados;

public class BinovaDbContextFactory : IDesignTimeDbContextFactory<BinovaDbContext>
{
    public BinovaDbContext CreateDbContext(string[] args)
    {
        var ambiente = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{ambiente}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var conexao = config.GetConnectionString("Oracle");
        if (string.IsNullOrWhiteSpace(conexao))
            throw new InvalidOperationException("Configure a conexao Oracle local antes da Migration.");

        var options = new DbContextOptionsBuilder<BinovaDbContext>()
            .UseOracle(conexao, oracle => oracle.MigrationsHistoryTable("BINOVA_EF_MIGRATIONS"))
            .Options;
        return new BinovaDbContext(options);
    }
}
