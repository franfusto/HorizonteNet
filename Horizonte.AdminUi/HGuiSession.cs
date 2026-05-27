using Horizonte;
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Horizonte.AdminUi;

/// <summary>
/// Representa una sesión gráfica de usuario dentro del sistema de administración Horizonte.
/// </summary>
/// <remarks>
/// La clase <c>HGuiSession</c> maneja la lógica de estado de la sesión de usuario, incluyendo la autenticación,
/// y mantiene la configuración de autenticación de la aplicación.
/// </remarks>
public class HGuiSession
{
    /// <summary>
    /// Evento que se desencadena cuando ocurre un cambio en el estado de la sesión de usuario.
    /// </summary>
    /// <remarks>
    /// Este evento es utilizado para notificar a los componentes o servicios interesados cuando hay
    /// cambios importantes en el estado de la sesión, como cuando un usuario inicia sesión
    /// o cierra sesión. Los gestores de eventos pueden utilizar esta notificación para actualizar
    /// la interfaz de usuario o realizar otras tareas relacionadas con la sesión.
    /// </remarks>
    public event EventHandler StateChanged = new EventHandler(delegate { });
    private ILogger<HGuiSession>? _logger;
    private HGuiSettings _hGuiSettings;

    /// <summary>
    /// Indica si el usuario ha iniciado sesión y está registrado en la aplicación.
    /// </summary>
    /// <remarks>
    /// Esta propiedad determina si el usuario tiene acceso a las funcionalidades protegidas de la aplicación.
    /// Se inicializa en <see langword="false"/> de manera predeterminada. Si el valor de la propiedad
    /// <c>ProtectApp</c> en <c>HGuiSettings</c> es <see langword="false"/>,
    /// <c>UserRegistered</c> se establece automáticamente en <see langword="true"/> durante la construcción de la instancia.
    /// </remarks>
    public bool UserRegistered { get; private set; } = false;

    /// <summary>
    /// Indica si la aplicación está protegida mediante autenticación.
    /// </summary>
    /// <remarks>
    /// La propiedad se inicializa utilizando el valor correspondiente de la configuración de <c>HGuiSettings</c>.
    /// Si <c>ProtectApp</c> es <see langword="true"/>, el usuario necesitará autenticarse para acceder a ciertas funcionalidades.
    /// En caso contrario, el acceso se otorga sin necesidad de autenticación.
    /// </remarks>
    public readonly bool ProtectApp = false;


    /// <summary>
    /// Clase HGuiSession se encarga de gestionar la sesión GUI dentro del sistema Horizonte.
    /// Esta clase gestiona el estado de la sesión, incluyendo la verificación del estado de registro de usuario
    /// y las operaciones de inicio y cierre de sesión.
    /// </summary>
    public HGuiSession(IHGesCom gesCom, IhContext context, ILogger<HGuiSession> logger)
    {
        _hGuiSettings = context.Get<HGuiSettings>() ?? new HGuiSettings();
        _logger = logger;
        ProtectApp = _hGuiSettings.ProtectApp;
        if (!_hGuiSettings.ProtectApp) UserRegistered = true;
    }

    /// <summary>
    /// Realiza el proceso de inicio de sesión para el sistema Horizonte, verificando la contraseña proporcionada.
    /// </summary>
    /// <param name="password">La contraseña que se utilizará para validar el intento de inicio de sesión.</param>
    /// <returns>
    /// Devuelve <see langword="true"/> si el inicio de sesión es exitoso y el usuario es registrado como autenticado;
    /// de lo contrario, devuelve <see langword="false"/> indicando que la autenticación falló.
    /// </returns>
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

    /// <summary>
    /// El método LogOut finaliza la sesión actual del usuario en la aplicación Horizonte.
    /// Este método verifica si la aplicación requiere protección mediante autenticación antes de proceder.
    /// En caso afirmativo, cambia el estado de registro del usuario a no registrado,
    /// desencadena el evento StateChanged para notificar a los suscriptores sobre el cambio de estado,
    /// y registra en el sistema de logs la finalización de la sesión.
    /// </summary>
    public void LogOut()
    {
        if (!_hGuiSettings.ProtectApp) return;
        UserRegistered = false;
        StateChanged?.Invoke(this, EventArgs.Empty);
        _logger?.LogInformation("Fin sesión");
    }
}