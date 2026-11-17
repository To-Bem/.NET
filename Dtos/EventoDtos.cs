using System.ComponentModel.DataAnnotations;
using TOBem.TelemetriaApi.Models;

namespace TOBem.TelemetriaApi.Dtos;

public class EventoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "dispositivoId deve ser um número maior que zero.")]
    public int DispositivoId { get; set; }

    [Required(ErrorMessage = "tipo é obrigatório."), StringLength(30)]
    public string Tipo { get; set; } = string.Empty;

    public int Valor { get; set; }

    /// <summary>Opcional. Se omitido, usa a hora atual (UTC).</summary>
    public DateTime? DataHora { get; set; }
}

public record EventoResponse(int Id, int DispositivoId, string Tipo, int Valor, DateTime DataHora)
{
    public static EventoResponse De(Evento e) => new(e.Id, e.DispositivoId, e.Tipo, e.Valor, e.DataHora);
}
