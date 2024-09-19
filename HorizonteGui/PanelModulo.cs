using System.Diagnostics;
using Horizonte;
using HorizonteGui.Components.Elements;

namespace HorizonteGui;

[HorizonteModule("HorizonteGui")]
public class PanelModulo
{
    private IHorizonteEnv _env;
    private ILogger<PanelModulo>? _logger;
    private HGuiSettings? _settings;
    private IHContext? _context;

    public PanelModulo(IHorizonteEnv env)
    {
        _env = env;
    }

    [HorizonteCommand("HorizonteGui_Command1")]
    public DateTime Command1()
    {
        _logger?.LogInformation("desde el módulo HorizonteGui");
        return DateTime.Now;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("HorizonteGui_Init")]
    public bool Init()
    {
        _logger = _env.GetService<ILogger<PanelModulo>>();
        _context = _env.GetService<IHContext>();
        _logger?.LogInformation("HorizonteGui Inciciado");
        _settings = _context?.Get<HGuiSettings>();
        return true;
    }

    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteGui_ConfigPage", "Aplicación")]
    public string ConfigPage() => typeof(AppPanel).FullName!;

    [HorizonteCommand("HorizonteGui_OpenGui")]
    public void OpenGui()
    {
        try
        {
            if (!string.IsNullOrEmpty(_settings?.AppUrl)) 
                Process.Start(new ProcessStartInfo(_settings.AppUrl) { UseShellExecute = true });
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
        }
    }
}