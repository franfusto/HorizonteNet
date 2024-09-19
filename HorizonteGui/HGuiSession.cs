using Horizonte;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace HorizonteGui;

public class HGuiSession
{
    public event EventHandler StateChanged = new EventHandler(delegate(object sender, EventArgs e) { });
    private ILogger<HGuiSession>? _logger;
    private HGuiSettings _hGuiSettings;
    public bool UserRegistered { get; private set; } = false;
    public bool ProtectApp = false;

    public HGuiSession(IHorizonteEnv env, IHGesCom gesCom, IHContext context, ILogger<HGuiSession> logger)
    {
        _hGuiSettings = context.Get<HGuiSettings>() ?? new HGuiSettings();
        _logger = logger;
        ProtectApp = _hGuiSettings.ProtectApp;
        if (!_hGuiSettings.ProtectApp) UserRegistered = true;
    }

    public bool DoLogin(string password)
    {
        bool res = _hGuiSettings.ProtectApp &&
                   password == _hGuiSettings!.AppPassword;
        UserRegistered = res;
        StateChanged?.Invoke(this, EventArgs.Empty);
        if (UserRegistered) _logger?.LogInformation("Inicio sesión correcto");
        else _logger?.LogInformation("Inicio sesión fallido");
        return res;
    }

    public void LogOut()
    {
        if ((!_hGuiSettings.ProtectApp)) return;
        UserRegistered = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
        _logger?.LogInformation("Fin sesión");
    }
}