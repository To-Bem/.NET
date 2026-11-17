using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TOBem.TelemetriaApi.Models;
using TOBem.TelemetriaApi.Dtos;

namespace TOBem.TelemetriaApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EventosController : ControllerBase
{
    private readonly Contexto _db;
    public EventosController(Contexto db) => _db = db;

    /// <summary>Lista eventos (mais recentes primeiro). Filtro opcional: ?dispositivoId=1</summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EventoResponse>>> Listar([FromQuery] int? dispositivoId)
    {
        var consulta = _db.Eventos.AsNoTracking().AsQueryable();
        if (dispositivoId is not null) consulta = consulta.Where(e => e.DispositivoId == dispositivoId);
        var lista = await consulta.OrderByDescending(e => e.DataHora).ToListAsync();
        return Ok(lista.Select(EventoResponse.De));
    }

    /// <summary>Busca um evento pelo id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<EventoResponse>> Buscar(int id)
    {
        var e = await _db.Eventos.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NaoEncontrado(id);
        return Ok(EventoResponse.De(e));
    }

    /// <summary>Endpoint chamado pelo ESP32. Um evento HEARTBEAT também atualiza o dispositivo.</summary>
    [HttpPost]
    public async Task<ActionResult<EventoResponse>> Criar([FromBody] EventoRequest req)
    {
        var erro = await ValidarAsync(req);
        if (erro is not null) return erro;

        var tipo = req.Tipo.ToUpperInvariant();
        var agora = (req.DataHora ?? DateTime.UtcNow).ToUniversalTime();
        var evento = new Evento { DispositivoId = req.DispositivoId, Tipo = tipo, Valor = req.Valor, DataHora = agora };
        _db.Eventos.Add(evento);

        if (tipo == Evento.Heartbeat)
        {
            var disp = await _db.Dispositivos.FirstAsync(d => d.Id == req.DispositivoId);
            disp.UltimoHeartbeat = agora;
            disp.Status = Dispositivo.StatusOnline;
        }

        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Buscar), new { id = evento.Id }, EventoResponse.De(evento));
    }

    /// <summary>Atualiza um evento.</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Atualizar(int id, [FromBody] EventoRequest req)
    {
        var erro = await ValidarAsync(req);
        if (erro is not null) return erro;

        var e = await _db.Eventos.FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NaoEncontrado(id);

        e.DispositivoId = req.DispositivoId;
        e.Tipo = req.Tipo.ToUpperInvariant();
        e.Valor = req.Valor;
        if (req.DataHora is not null) e.DataHora = req.DataHora.Value.ToUniversalTime();
        await _db.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>Remove um evento.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Remover(int id)
    {
        var e = await _db.Eventos.FirstOrDefaultAsync(x => x.Id == id);
        if (e is null) return NaoEncontrado(id);
        _db.Eventos.Remove(e);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private async Task<ActionResult?> ValidarAsync(EventoRequest req)
    {
        if (!Evento.TiposValidos.Contains(req.Tipo?.ToUpperInvariant()))
            return BadRequest(new { mensagem = $"tipo de evento inválido. Use: {string.Join(", ", Evento.TiposValidos)}." });
        // CountAsync (COUNT(*)) em vez de AnyAsync: o AnyAsync gera TRUE/FALSE, que o Oracle < 23 rejeita (ORA-00904).
        if (await _db.Dispositivos.CountAsync(d => d.Id == req.DispositivoId) == 0)
            return BadRequest(new { mensagem = $"dispositivoId {req.DispositivoId} não existe." });
        return null;
    }

    private NotFoundObjectResult NaoEncontrado(int id) =>
        NotFound(new { mensagem = $"Evento {id} não encontrado." });
}
