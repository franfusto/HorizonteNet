using System.ComponentModel;
using GoogleApi.Entities.Common;
using GoogleApi.Entities.Maps.AddressValidation.Request;
using GoogleApi.Entities.Maps.Common;
using GoogleApi.Entities.Maps.Directions.Request;
using GoogleApi.Entities.Maps.Directions.Response;
using Horizonte;
using Microsoft.Extensions.Logging;

namespace Horizonte.Samples.Gmaps;

[HorizonteModule("Horizonte.Samples.Gmaps")]
public class PanelModulo
{
    private ILogger<PanelModulo>? _logger;
    private Lazy<IHorizonteEnv> _env;
    private GmapsConfig _config;
    private IHCredManager? _credManager;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }

    [HorizonteRole("init")]
    [HorizonteCommand("Gmaps_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
        var context = _env.Value.GetService<IHContext>();
        _config = context?.Get<GmapsConfig>() ?? new GmapsConfig();
        _credManager = _env.Value.GetService<IHCredManager>();
        _logger?.LogInformation("Módulo Gmaps Inciciado");
        
        
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
                Key = _credManager?.GetCredential(_config.ApiKey) ?? string.Empty ,                                             
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
            _logger?.LogError(e, "Error al obtener la ruta");
            return null;
        }
        
    }
    [HorizonteRole("configpage")]
    [HorizonteCommand("Gmaps_ConfigPage", "Widget de configuración del módulo Gmaps, Google Maps")]
    public WidgetDef ConfigPage() => new WidgetDef() { Type = typeof(GmapsConfigWidget), Parameters = null };
}