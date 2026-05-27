using Horizonte.Extensions.Interactive;

namespace Horizonte.Scripts;

/// <summary>
/// La clase ScriptsConfig gestiona la configuración relacionada con los scripts del sistema.
/// </summary>
/// <remarks>
/// Esta clase proporciona una estructura para definir y almacenar fuentes y definiciones de scripts en el sistema.
/// Al inicializar una instancia de ScriptsConfig, se agregan automáticamente dos fuentes de script predeterminadas:
/// "App" con la ruta "./scripts" y "Privado" con la ruta "~/horizonte/scripts".
/// </remarks>
/// <example>
/// ScriptsConfig se utiliza dentro del sistema para acceder y modificar las fuentes y scripts disponibles.
/// Cualquier cambio en la lista de scripts o fuentes debe realizarse a través de una instancia de esta clase.
/// </example>
public class ScriptsConfig
{
    /// <summary>
    /// La clase ScriptsConfig es responsable de la configuración y gestión de las fuentes de scripts en el proyecto.
    /// Se inicializa con dos fuentes predeterminadas:
    /// - Una fuente llamada "App" con la ruta "./scripts".
    /// - Una fuente llamada "Privado" con la ruta "~/horizonte/scripts".
    /// </summary>
    public ScriptsConfig()
    {
        Sources.Add(new ScriptsSourceItem { Name = "App", Path = "./scripts" });
        Sources.Add(new ScriptsSourceItem { Name = "Privado", Path = "~/horizonte/scripts" });
    }

    /// <summary>
    /// La propiedad Sources es una lista de objetos de tipo ScriptsSourceItem que representa distintas ubicaciones
    /// desde las cuales se pueden cargar scripts. Cada ScriptsSourceItem incluye un nombre y una ruta
    /// correspondiente al origen del script. Esta propiedad está inicializada por defecto como una lista vacía.
    /// </summary>
    public List<ScriptsSourceItem> Sources { get; set; } = new List<ScriptsSourceItem>();

    /// La propiedad `Scripts` representa una lista de definiciones de scripts (`ScriptDef`) utilizados dentro de la configuración de scripts de un sistema.
    /// Esta propiedad permite obtener o establecer una colección de objetos `ScriptDef`, que conforman los scripts actualmente configurados y disponibles.
    /// La lista inicializada al instanciar un objeto de `ScriptsConfig` es una lista vacía y se espera que se agreguen elementos conforme se vayan añadiendo nuevos scripts al sistema.
    /// Esta colección es utilizada en diferentes partes del sistema para cargar y manipular los módulos de script necesarios, así como para gestionar la visualización y la interactividad con scripts dentro de interfaces de usuario.
    /// /
    public List<ScriptDef> Scripts { get; set; } = new List<ScriptDef>();
}

/// <summary>
/// Representa un elemento fuente para los scripts en la configuración.
/// </summary>
/// <remarks>
/// Esta clase se utiliza para definir la información de origen de scripts,
/// incluyendo el nombre del origen y la ruta donde se encuentran los scripts.
/// </remarks>
public class ScriptsSourceItem
{
    /// <summary>
    /// La propiedad Name representa el nombre de la fuente de scripts dentro de un objeto ScriptsSourceItem.
    /// Este nombre identifica de manera única cada origen de scripts configurado.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Obtiene o establece la ruta del archivo o directorio relacionado con un elemento de origen de scripts.
    /// </summary>
    public string Path { get; set; } = string.Empty;
}
