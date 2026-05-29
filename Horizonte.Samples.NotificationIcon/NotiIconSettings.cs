namespace Horizonte.Samples.NotificationIcon;

/// <summary>
/// La clase <c>NotiIconSettings</c> representa la configuración del icono de notificación utilizado dentro de la aplicación "Horizonte".
/// Este icono permite a los usuarios interactuar con diferentes funcionalidades de la aplicación a través de un menú contextual.
/// </summary>
public class NotiIconSettings
{
    /// <summary>
    /// La clase <c>NotiIconSettings</c> se utiliza para configurar las propiedades y opciones de menús
    /// asociados con un icono de notificación en la aplicación Horizonte.
    /// </summary>
    public NotiIconSettings()
    {
        MenuItems.Add(new IconMenuItem { Label = "Quit", StockIcon = "gtk-quit", Command = "Horizonte_Quit" });
        MenuItems.Add(new IconMenuItem { Label = "Horizonte Net", StockIcon = "gtk-home", Command = "HorizonteGui_OpenGui" });
    }

    /// <summary>
    /// Representa el título del icono en la configuración del icono de notificación.
    /// </summary>
    /// <remarks>
    /// Este título se utiliza para identificar o describir el icono en la interfaz del usuario.
    /// </remarks>
    public string IconTitle { get; set; } = "Horizonte Net";

    /// <summary>
    /// Propiedad que representa el comando a ejecutar cuando se activa la notificación del icono.
    /// </summary>
    /// <remarks>
    /// Esta propiedad contiene una cadena que especifica el comando que debe ejecutarse cuando el usuario interactúa con la notificación asociada al icono.
    /// Si esta propiedad está configurada con un valor no nulo o vacío, el sistema ejecutará el comando correspondiente utilizando el mecanismo proporcionado por la interfaz <c>IHGesCom</c>.
    /// </remarks>
    public string? OnActivatedCommand { get; set; }

    /// La propiedad IconResource representa la ubicación del recurso de icono que se utilizará para la barra de notificaciones.
    /// Esta propiedad almacena una cadena que especifica la ruta del archivo de imagen del icono, que puede ser un recurso incrustado en el ensamblado de la aplicación.
    /// Es utilizada por IconManager para cargar y mostrar el icono en la interfaz del usuario.
    /// Valor predeterminado: "Horizonte.Samples.NotificationIcon.icons.home.png".
    /// /
    public string IconResource { get; set; } = "Horizonte.Samples.NotificationIcon.icons.home.png";

    /// <summary>
    /// Propiedad que representa una lista de elementos de menú configurables para un icono de notificación.
    /// </summary>
    /// <remarks>
    /// Cada elemento del menú está representado por una instancia de <see cref="IconMenuItem"/>, la cual contiene
    /// la etiqueta del menú, el icono estándar asociado y el comando que se ejecutará al activarse el elemento.
    /// Esta propiedad permite personalizar dinámicamente el menú de un icono de notificación, facilitando la
    /// inclusión de opciones de interacción personalizadas en la interfaz de usuario.
    /// </remarks>
    public List<IconMenuItem> MenuItems { get; set; } = new List<IconMenuItem>();
    
    
}

/// <summary>
/// Representa un elemento de menú que incluye un icono y un comando asociado en un entorno de interfaz gráfica de usuario.
/// </summary>
public class IconMenuItem()
{
    /// Propiedad que representa el texto visible en un elemento de menú de ícono.
    /// Este texto actúa como etiqueta descriptiva para el elemento de menú, ofreciendo al usuario información contextual sobre la acción que se ejecutará al interactuar con dicho elemento.
    /// Puede ser cualquier cadena que describa claramente la acción o el propósito del elemento en el menú.
    /// /
    public string Label { get; set; } = string.Empty;

    /// Representa la propiedad `StockIcon` de un elemento de menú icono.
    /// Esta propiedad se utiliza para definir el nombre del icono que se mostrará
    /// en el elemento de menú. El nombre debe corresponder a un icono reconocido
    /// por la biblioteca gráfica utilizada, en este caso, normalmente se refiere
    /// a iconos estándar disponibles en el sistema o la aplicación.
    /// `StockIcon` es una cadena de texto que almacena el identificador del icono.
    /// La correcta asignación de valores a esta propiedad asegura que el icono
    /// correspondiente se renderice adecuadamente en la interfaz de usuario,
    /// mejorando la experiencia visual y de interacción.
    /// /
    public string StockIcon { get; set; } = string.Empty;

    /// La propiedad `Command` representa una cadena de texto asociada a cada elemento del menú de iconos dentro de la aplicación.
    /// Esta propiedad contiene el comando que se ejecutará cuando el elemento del menú correspondiente sea activado por el usuario.
    /// Está diseñada para ser utilizada dentro de la clase `IconMenuItem`, permitiendo definir acciones específicas a ejecutar
    /// en respuesta a las interacciones del usuario con los elementos del menú.
    /// Forma parte del sistema de notificación de iconos en la aplicación y juega un papel crucial en la configuración del comportamiento
    /// de los ítems del menú vinculándolos con comandos específicos que pueden ser ejecutados.
    /// /
    public string Command { get; set; } = string.Empty;
}