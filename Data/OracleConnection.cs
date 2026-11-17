using Microsoft.Extensions.Configuration;

namespace TOBem.TelemetriaApi.Data;

/// <summary>
/// Descobre a connection string do Oracle SEM guardar credenciais no repositório.
/// Ordem de busca:
///   1) variável de ambiente ORACLE_CONNECTION_STRING (string completa);
///   2) configuração "ConnectionStrings:DefaultConnection" (user-secrets, variável
///      ConnectionStrings__DefaultConnection ou appsettings.Development.json ignorado pelo Git);
///   3) variáveis ORACLE_USER e ORACLE_PASSWORD (host/porta/serviço têm padrão da FIAP).
/// </summary>
public static class OracleConnection
{
    public static string Montar(IConfiguration? configuracao = null)
    {
        var completa = Environment.GetEnvironmentVariable("ORACLE_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(completa)) return completa;

        var daConfig = configuracao?.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(daConfig) && !daConfig.Contains('<')) return daConfig;

        var host = Environment.GetEnvironmentVariable("ORACLE_HOST") ?? "oracle.fiap.com.br";
        var porta = Environment.GetEnvironmentVariable("ORACLE_PORT") ?? "1521";
        var servico = Environment.GetEnvironmentVariable("ORACLE_SERVICE") ?? "orcl";
        var usuario = Environment.GetEnvironmentVariable("ORACLE_USER");
        var senha = Environment.GetEnvironmentVariable("ORACLE_PASSWORD");

        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrWhiteSpace(senha))
            throw new InvalidOperationException(
                "Credenciais do Oracle não encontradas. Defina ORACLE_USER e ORACLE_PASSWORD " +
                "(ou ORACLE_CONNECTION_STRING / user-secrets). Veja o README.");

        // Mesmo formato do exemplo do professor (descritor completo).
        return $"User Id={usuario};Password={senha};Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST={host})(PORT={porta})))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME={servico})))";
    }
}
