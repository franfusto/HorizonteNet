using Horizonte;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;


namespace Horizonte.Samples.Gcalendar;

[HorizonteModule("Hmod.Calendar")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private readonly IHCredManager _credManager;
    private ServiceCal? _calendarService;
    private GCalConfig _config;
    
    public PanelModulo(ILogger<PanelModulo> logger, IhContext context, IHCredManager credManager)
    {
        _logger = logger;
        _context = context;
        _credManager = credManager;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("Calendar_Init")]
    public bool Init()
    {
        _config = _context.Get<GCalConfig>() ?? new GCalConfig();
        _calendarService = new ServiceCal(_config, _logger, _credManager);
        _logger.LogInformation("Módulo Google Calendar Inciciado");
        return true;
    }

    [HorizonteRole("configpage")]
    [HorizonteCommand("Calendar_ConfigPage", "Widget de configuración del módulo Calendar")]
    public WidgetDef ConfigPage()
    {
        return new WidgetDef
        {
            Type = typeof(CalendarConfigWidget),
            Parameters = null,
        };
    }

   

    [HorizonteCommand("Calendar_GetEvents", "Obtener eventos del calendario dada una fecha de inicio y de fin")]
    public Task<List<CalendarEvent>>
        GetEvents( DateTime startDateTime, DateTime endDateTime) =>
        _calendarService?.GetEvents(startDateTime, endDateTime) ?? Task.FromResult(new List<CalendarEvent>());

    [HorizonteCommand("Calendar_CreateEvent", "Crear evento en el calendario")]
    public Task<bool> CreateEvent(CalendarEvent calendarEvent) =>
        _calendarService?.CreateEvent( calendarEvent) ?? Task.FromResult(false);

    [HorizonteCommand("Calendar_DeleteEvent", "Eliminar evento del calendario")]
    public Task<bool> DeleteEvent(string calendarname, string eventId) =>
        _calendarService?.DeleteEvent(eventId) ?? Task.FromResult(false);
    
    [HorizonteCommand("Calendar_UpdateEvent", "Actualizar evento del calendario")]
    public async Task<bool> UpdateEvent(CalendarEvent NewCalendarEvent, string CurrentEventId) =>  _calendarService?.UpdateEvent(NewCalendarEvent, CurrentEventId).Result ?? false;

    [HorizonteCommand("Calendar_FechaHoraActual", "Obtiene la fecha y hora actual en formato ISO 8601")]
    public DateTime FechaHoraActual() => DateTime.Now;


    [HorizonteCommand("Calendar_Test")]
    public async Task<string> Test()
    {
        try
        {
            var events = await _calendarService!.GetEvents(DateTime.Now.AddMonths(-3), DateTime.Now);
            var result =System.Text.Json.JsonSerializer.Serialize(events);
            return result;
        }
        catch (Exception e)
        {
            return e.Message;
        }
    }
    
    
}

