using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using TOBem.TelemetriaApi.Data;
using TOBem.TelemetriaApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<Contexto>(opt => opt.UseOracle(OracleConnection.Montar(builder.Configuration)));

builder.Services
    .AddControllers()
    .ConfigureApiBehaviorOptions(o =>
    {
        // Dado inválido devolve 400 com mensagem em JSON: { "mensagem": "...", "erros": { campo: [..] } }
        o.InvalidModelStateResponseFactory = ctx =>
        {
            var erros = ctx.ModelState
                .Where(kv => kv.Value is { Errors.Count: > 0 })
                .ToDictionary(
                    kv => kv.Key.TrimStart('$', '.'),
                    kv => kv.Value!.Errors.Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Valor inválido." : e.ErrorMessage).ToArray());
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new { mensagem = "Dados inválidos.", erros });
        };
    });

var app = builder.Build();

// Erros inesperados viram 500 em JSON. Em Development (Visual Studio) a resposta inclui o motivo real
// (ex.: ORA-xxxxx) para facilitar o teste; fora de Development nada interno é exposto.
app.UseExceptionHandler(handler => handler.Run(async ctx =>
{
    ctx.Response.StatusCode = StatusCodes.Status500InternalServerError;
    ctx.Response.ContentType = "application/json";
    var erro = ctx.Features.Get<IExceptionHandlerFeature>()?.Error;
    app.Logger.LogError(erro, "Erro não tratado");

    var detalhe = app.Environment.IsDevelopment() && erro is not null
        ? (erro.InnerException?.Message ?? erro.Message)
        : null;
    await ctx.Response.WriteAsJsonAsync(new { mensagem = "Erro interno ao processar a requisição.", detalhe });
}));

app.MapControllers();
app.MapGet("/", () => Results.Ok(new { api = "Tô Bem - API de Telemetria", status = "no ar" }));

app.Run();
