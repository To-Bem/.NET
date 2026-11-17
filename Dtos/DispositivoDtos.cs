using System.ComponentModel.DataAnnotations;
using TOBem.TelemetriaApi.Models;

namespace TOBem.TelemetriaApi.Dtos;

public class DispositivoRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "idosoId deve ser um número maior que zero.")]
    public int IdosoId { get; set; }

    [Required(ErrorMessage = "tipo é obrigatório (LAMPADA ou BASE).")]
    [StringLength(10)]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "comodo é obrigatório."), StringLength(50, MinimumLength = 2, ErrorMessage = "comodo deve ter entre 2 e 50 caracteres.")]
    public string Comodo { get; set; } = string.Empty;

    [StringLength(10)]
    public string? Status { get; set; }
}

public record DispositivoResponse(int Id, int IdosoId, string Tipo, string Comodo, string Status, DateTime? UltimoHeartbeat)
{
    public static DispositivoResponse De(Dispositivo d) =>
        new(d.Id, d.IdosoId, d.Tipo, d.Comodo, d.Status, d.UltimoHeartbeat);
}
