using System.ComponentModel;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.AddressValidation.Request;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using Horizonte;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Samples.Gmaps;

[HorizonteModule("Horizonte.Samples.Gmaps")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;
    private readonly IHCredManager _credManager;
    private GmapsConfig _config;

    public PanelModulo(ILogger<PanelModulo> logger, IhContext context, IHCredManager credManager)
    {
        _logger = logger;
        _context = context;
        _credManager = credManager;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("Gmaps_Init")]
    public bool Init()
    {
        _config = _context.Get<GmapsConfig>() ?? new GmapsConfig();
        _logger.LogInformation("Módulo Gmaps Inciciado");
        
        return true;
    }

    [HorizonteCommand("Gmaps_GetRoute"),Description("Obtiene la ruta entre dos direcciones, devuelve nulo sí no se encuentra")]
    public async Task<RouteResult?> GetRoute(string startAddress, string endAddress)
    {
        try
        {
            var directionservice = new GoogleApi.GoogleMaps.DirectionsApi();
            var req = new DirectionsRequest
            {
                Key = _credManager.GetCredential(_config.ApiKey) ?? string.Empty ,                                             
                Origin = new LocationEx(new Address(startAddress)),
                Destination = new LocationEx(new Address(endAddress))
            };

            var directionsresponse = await  directionservice.QueryAsync(req);
            
            RouteResult result = new RouteResult();
            result.RouteOrigin = startAddress;
            result.RouteDestination = endAddress;
            result.Time = directionsresponse.Routes.First().Legs.First().Duration.Text;
            result.Distance = directionsresponse.Routes.First().Legs.First().Distance.Text;
            return result;

        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error al obtener la ruta");
            return null;
        }
        
    }
    [HorizonteRole("configpage")]
    [HorizonteCommand("Gmaps_ConfigPage", "Widget de configuración del módulo Gmaps, Google Maps")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(GmapsConfigWidget), Parameters = null };
}