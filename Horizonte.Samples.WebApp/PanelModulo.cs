using Horizonte.Interfaces;

namespace Horizonte.Samples.WebApp;

[HorizonteModule("Horizonte.Samples.WebApp")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;
    private readonly IhContext _context;

    public PanelModulo(ILogger<PanelModulo> logger, IhContext context)
    {
        _logger = logger;
        _context = context;
    }
    [HorizonteRole("init")]
    [HorizonteCommand("WebApp_Init")]
    public bool Init()
    {
        _logger.LogInformation("HWebApp Inciciado");
        return true;
    }

}