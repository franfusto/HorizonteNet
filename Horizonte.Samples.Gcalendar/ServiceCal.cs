using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Microsoft.Extensions.Logging;

namespace Horizonte.Samples.Gcalendar;

public class ServiceCal
{
    private GCalConfig _config;
    private ILogger<PanelModulo>? _logger;
    private CalendarService? _service;
    private UserCredential? _credential;
    private IHCredManager? _credManager;
    private string calendarid = string.Empty;
    public ServiceCal(GCalConfig gCalConfig, ILogger<PanelModulo>? logger, IHCredManager credManager)
    {
        _config = gCalConfig;
        _logger = logger;
        _credManager = credManager;
        GetService().Wait();
    }

    public async Task GetService()
    {
        try
        {
            _logger?.LogInformation("Google Calendar getService");
            calendarid =  _credManager?.GetCredential(_config.CalendarId) ?? string.Empty;
            _credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets
                {
                    ClientId =  _credManager?.GetCredential(_config.ClientId) ?? string.Empty ,
                    ClientSecret =  _credManager?.GetCredential(_config.ClientSecret) ?? string.Empty,
                },
                new[] { "https://www.googleapis.com/auth/calendar" },
                _credManager?.GetCredential(_config.UserName) ?? string.Empty,
                CancellationToken.None);
            _service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = _credential,
                ApplicationName = "Horizonte",
            });
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
        }
    }


    public async Task<List<CalendarEvent>?> GetEvents(DateTime start, DateTime end)
    {
        try
        {
            _logger?.LogInformation("Google Calendar getEvents");
            var query = _service?.Events.List(calendarid);
            if (query != null)
            {
                query.TimeMin = start;
                query.TimeMax = end;

                var events = (await query.ExecuteAsync()).Items;

                if (events == null) return null;
                var result = new List<CalendarEvent>();
                foreach (var item in events)
                {
                    var calendarEvent = new CalendarEvent
                    {
                        EventId = item.Id,
                        Summary = item.Summary,
                        StartDateTime = item.Start.DateTime ?? DateTime.Parse(item.Start.Date),
                        EndDateTime = item.End.DateTime ?? DateTime.Parse(item.End.Date),
                        Description = item.Description
                    };
                    result.Add(calendarEvent);
                }

                return result;
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
        }

        return new();
    }

    public async Task<bool> CreateEvent(CalendarEvent calendarEvent)
    {
        try
        {
            _logger?.LogInformation("Google Calendar CreateEvent");
            var newEvent = new Event()
            {
                Summary = calendarEvent.Summary,
                Description = calendarEvent.Description,
                Start = new EventDateTime()
                {
                    DateTime = calendarEvent.StartDateTime
                },
                End = new EventDateTime()
                {
                    DateTime = calendarEvent.EndDateTime
                },
            };

            await _service?.Events.Insert(newEvent, calendarid).ExecuteAsync()!;
            return true;
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
            return false;
        }
    }

    public async Task<bool> DeleteEvent(string eventId)
    {
        try
        {
            _logger?.LogInformation("Google Calendar DeleteEvent");
            if (_service?.Events != null)
            {
                await _service.Events.Delete(calendarid, eventId).ExecuteAsync();
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
            return false;
        }

        return true;
    }

    public async Task<bool> UpdateEvent(CalendarEvent calendarEvent, string eventId)
    {
        try
        {
            _logger?.LogInformation("Google Calendar UpdateEvent");
            await DeleteEvent(eventId);
            return await CreateEvent(calendarEvent);
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
            return false;
        }
    }
}