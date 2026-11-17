namespace TOBem.TelemetriaApi.Models;

/// <summary>Leitura enviada pelo ESP32 (movimento, botão, temperatura, heartbeat...). Lado "N" da relação 1:N.</summary>
public class Evento
{
    public const string Heartbeat = "HEARTBEAT";

    public static readonly string[] TiposValidos =
    {
        "MOVIMENTO", "PRESENCA_PARADA", "SEM_PRESENCA", "GELADEIRA_ABERTA",
        "BOTAO_OK", "BOTAO_AUSENTE", "BOTAO_VOLTEI", "TEMPERATURA", Heartbeat
    };

    public int Id { get; set; }
    public int DispositivoId { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public int Valor { get; set; }
    public DateTime DataHora { get; set; }

    public Dispositivo? Dispositivo { get; set; }
}
