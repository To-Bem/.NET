using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using TOBem.TelemetriaApi.Models;

namespace TOBem.TelemetriaApi.Data;

/// <summary>
/// Usada só pelas ferramentas do EF (dotnet ef / Add-Migration / Update-Database).
/// Para criar uma Migration não é preciso conectar ao banco; sem credenciais usa uma string de mentira.
/// Para "Update-Database" as credenciais precisam estar definidas (veja o README).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<Contexto>
{
    public Contexto CreateDbContext(string[] args)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets<DesignTimeDbContextFactory>(optional: true)
            .AddEnvironmentVariables()
            .Build();

        string conexao;
        try { conexao = OracleConnection.Montar(config); }
        catch (InvalidOperationException) { conexao = "User Id=design;Password=design;Data Source=localhost:1521/orcl;"; }

        var options = new DbContextOptionsBuilder<Contexto>().UseOracle(conexao).Options;
        return new Contexto(options);
    }
}
