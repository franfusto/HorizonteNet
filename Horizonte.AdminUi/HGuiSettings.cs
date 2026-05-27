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
    /// CustomMenuItems es una propiedad que representa una lista de elementos de menú personalizados que se pueden configurar
    /// en la interfaz de usuario de la aplicación Horizonte AdminUI. Cada elemento de menú está definido por la clase HGuiMenuItem,
    /// que incluye propiedades como Id, ParentId, Icon, Caption y Component.
    /// La lista se inicializa de forma predeterminada como una lista vacía y puede ser modificada para incluir diversos
    /// elementos de menú según las necesidades del usuario. Esta colección permite al usuario definir y gestionar
    /// dinámicamente el conjunto de opciones que aparecerán en el menú de la aplicación.
    /// CustomMenuItems es importante para proporcionar flexibilidad en la configuración de la interfaz, permitiendo una
    /// organización jerárquica de los menús y personalización en términos de iconografía y componentes asociados.
    /// /
    public List<HGuiMenuItem> CustomMenuItems { get; set; } = new();

    /// /
    public bool ProtectApp { get; set; } = false;

    /// La propiedad `AppPassword` representa la contraseña utilizada para proteger la aplicación en el sistema Horizonte.
    /// Esta propiedad es parte del objeto `HGuiSettings` y su propósito es almacenar la contraseña que se necesita para
    /// autenticar los intentos de inicio de sesión cuando la protección de la aplicación está habilitada (`ProtectApp` es verdadero).
    /// Valor por defecto: `"admin"`.
    /// /
    public string AppPassword { get; set; } = "admin";

    /// Propiedad que representa el título de la aplicación.
    /// El valor por defecto de esta propiedad es "Horizonte Net".
    /// Se utiliza para mostrar el nombre de la aplicación en distintas partes de la interfaz de usuario.
    /// /
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
    /// El valor predeterminado es "material-dar", y puede ser modificado para aplicar
    /// diferentes estilos visuales según las necesidades del usuario.
    /// </remarks>
    public string DefaultTheme { get; set; } = "material-dark";
}


