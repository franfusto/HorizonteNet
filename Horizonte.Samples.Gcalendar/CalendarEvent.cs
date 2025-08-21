using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Horizonte.Samples.Gcalendar;

public class CalendarEvent
{
    [JsonPropertyName("EventId")]
    [Description("El identificador del evento en el calendario.")]
    public string? EventId { get; set; }

    [JsonPropertyName("Summary")]
    [Description("Resumen del evento.")]
    public string? Summary { get; set; }

    [JsonPropertyName("Description")]
    [Description("Descripción del evento del calendario.")]
    public string? Description { get; set; }

    [JsonPropertyName("Start")]
    [Description("Fecha y hora de inicio del evento.")]
    public DateTime StartDateTime { get; set; }
    
    [JsonPropertyName("End")]
    [Description("Fecha y hora de fin del evento.")]
    public DateTime EndDateTime { get; set; }
    
}