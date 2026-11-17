using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOBem.TelemetriaApi.Models;
using TOBem.TelemetriaApi.Dtos;

namespace TOBem.TelemetriaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DispositivosController : ControllerBase
{
    private readonly Contexto _db;
    public DispositivosController(Contexto db) => _db = db;

    /// <summary>Lista todos os dispositivos.</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DispositivoResponse>>> Listar()
    {
        var lista = await _db.Dispositivos.AsNoTracking().OrderBy(d => d.Id).ToListAsync();
        return Ok(lista.Select(DispositivoResponse.De));
    }

    /// <summary>Busca um dispositivo pelo id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DispositivoResponse>> Buscar(int id)
    {
        var d = await _db.Dispositivos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (d is null) return NaoEncontrado(id);
        return Ok(DispositivoResponse.De(d));
    }

    /// <summary>Lista os eventos de um dispositivo (mais recentes primeiro).</summary>
    [HttpGet("{id:int}/eventos")]
    public async Task<ActionResult<IEnumerable<EventoResponse>>> ListarEventos(int id)
    {
        // CountAsync (COUNT(*)) em vez de AnyAsync: o AnyAsync gera TRUE/FALSE, que o Oracle < 23 rejeita (ORA-00904).
        if (await _db.Dispositivos.CountAsync(x => x.Id == id) == 0) return NaoEncontrado(id);
        var eventos = await _db.Eventos.AsNoTracking()
            .Where(e => e.DispositivoId == id).OrderByDescending(e => e.DataHora).ToListAsync();
        return Ok(eventos.Select(EventoResponse.De));
    }

    /// <summary>Cadastra um dispositivo.</summary>
    [HttpPost]
    public async Task<ActionResult<DispositivoResponse>> Criar([FromBody] DispositivoRequest req)
    {
        var erro = Validar(req);
        if (erro is not null) return erro;

        var d = new Dispositivo
        {
            IdosoId = req.IdosoId,
            Tipo = req.Tipo.ToUpperInvariant(),
            Comodo = req.Comodo.Trim(),
            Status = (req.Status ?? Dispositivo.StatusOffline).ToUpperInvariant()
        };
        _db.Dispositivos.Add(d);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Buscar), new { id = d.Id }, DispositivoResponse.De(d));
    }

    /// <summary>Atualiza um dispositivo.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] DispositivoRequest req)
    {
        var erro = Validar(req);
        if (erro is not null) return erro;

        var d = await _db.Dispositivos.FirstOrDefaultAsync(x => x.Id == id);
        if (d is null) return NaoEncontrado(id);

        d.IdosoId = req.IdosoId;
        d.Tipo = req.Tipo.ToUpperInvariant();
        d.Comodo = req.Comodo.Trim();
        if (req.Status is not null) d.Status = req.Status.ToUpperInvariant();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Remove um dispositivo e, em cascata, seus eventos.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var d = await _db.Dispositivos.FirstOrDefaultAsync(x => x.Id == id);
        if (d is null) return NaoEncontrado(id);
        _db.Dispositivos.Remove(d);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private ActionResult? Validar(DispositivoRequest req)
    {
        if (!Dispositivo.TiposValidos.Contains(req.Tipo?.ToUpperInvariant()))
            return BadRequest(new { mensagem = $"tipo inválido. Use: {string.Join(", ", Dispositivo.TiposValidos)}." });
        if (req.Status is not null && !Dispositivo.StatusValidos.Contains(req.Status.ToUpperInvariant()))
            return BadRequest(new { mensagem = $"status inválido. Use: {string.Join(", ", Dispositivo.StatusValidos)}." });
        return null;
    }

    private NotFoundObjectResult NaoEncontrado(int id) =>
        NotFound(new { mensagem = $"Dispositivo {id} não encontrado." });
}
