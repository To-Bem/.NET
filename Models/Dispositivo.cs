using System.ComponentModel.DataAnnotations;

namespace TOBem.TelemetriaApi.Models;

/// <summary>Lâmpada ou base instalada na casa de um idoso. Lado "1" da relação 1:N com Evento.</summary>
public class Dispositivo
{
    public const string TipoLampada = "LAMPADA";
    public const string TipoBase = "BASE";
    public static readonly string[] TiposValidos = { TipoLampada, TipoBase };

    public const string StatusOnline = "ONLINE";
    public const string StatusOffline = "OFFLINE";
    public static readonly string[] StatusValidos = { StatusOnline, StatusOffline };

    public int Id { get; set; }

    /// <summary>Referência ao idoso (tabela IDOSO, de propriedade da API Java). Só guarda o id.</summary>
    public int IdosoId { get; set; }

    [Required, StringLength(10)]
    public string Tipo { get; set; } = TipoLampada;

    [Required, StringLength(50)]
    public string Comodo { get; set; } = string.Empty;

    [Required, StringLength(10)]
    public string Status { get; set; } = StatusOffline;

    public DateTime? UltimoHeartbeat { get; set; }

    public List<Evento> Eventos { get; set; } = new();
}
