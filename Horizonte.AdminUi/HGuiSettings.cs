namespace Horizonte.AdminUi;

/// <summary>
/// La clase <c>HGuiSettings</c> se encarga de gestionar las configuraciones y preferencias
/// de la interfaz gráfica de usuario (GUI) en una aplicación.
/// </summary>
/// <remarks>
/// Esta clase permite la configuración de diversos aspectos visuales y de comportamiento
/// de la GUI, tales como temas, dimensiones de ventana, idiomas, entre otros.
/// Es crucial para asegurar que la aplicación se alinee con las preferencias del usuario
/// y para proporcionar una experiencia de usuario personalizada y consistente.
/// </remarks>
public class HGuiSettings 
{
    /// <summary>
    /// Obtiene o establece la lista de elementos de menú personalizados que se configuran en la interfaz de usuario de Horizonte AdminUI.
    /// </summary>
    public List<HGuiMenuItem> CustomMenuItems { get; set; } = new();

    /// <summary>
    /// Obtiene o establece un valor que indica si la aplicación requiere autenticación por contraseña para su acceso.
    /// </summary>
    public bool ProtectApp { get; set; } = false;

    /// <summary>
    /// Obtiene o establece la contraseña utilizada para proteger la aplicación cuando <see cref="ProtectApp"/> está habilitado.
    /// </summary>
    public string AppPassword { get; set; } = "admin";

    /// <summary>
    /// Obtiene o establece el título descriptivo de la aplicación mostrado en la interfaz gráfica.
    /// </summary>
    public string AppTitle { get; set; } = "Horizonte Net";

    /// <summary>
    /// Propiedad que representa la URL de la aplicación.
    /// </summary>
    /// <value>
    /// Cadena de texto que contiene la dirección URL donde se aloja la aplicación.
    /// Esta dirección puede ser utilizada para acceder a la aplicación desde un navegador web u otras herramientas que requieran conectividad a través de una URL.
    /// </value>
    public string AppUrl { get; set; } = "http://0.0.0.0:5000";

    /// <summary>
    /// Propiedad que define el tema por defecto de la aplicación en la interfaz de usuario.
    /// </summary>
    /// <remarks>
    /// Almacena el valor del tema que se aplicará inicialmente al cargar la aplicación.
    /// El valor predeterminado es "material-dark", y puede ser modificado para aplicar
    /// diferentes estilos visuales según las necesidades del usuario.
    /// </remarks>
    public string DefaultTheme { get; set; } = "material-dark";

    /// <summary>
    /// Obtiene o establece la configuración de un servicio personalizado registrado en la aplicación.
    /// </summary>
    public CustomServiceConfig CustomServices { get; set; } = new CustomServiceConfig();
}


