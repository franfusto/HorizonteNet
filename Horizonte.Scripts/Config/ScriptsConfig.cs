using Horizonte.Extensions.Interactive;

namespace Horizonte.Scripts.Config;

/// <summary>
/// Gestiona la configuración relacionada con los scripts del sistema.
/// </summary>
/// <remarks>
/// Esta clase proporciona una estructura para definir y almacenar fuentes y definiciones de scripts en el sistema.
/// Al inicializar una instancia de <see cref="ScriptsConfig"/>, se agregan automáticamente dos fuentes de script predeterminadas:
/// "App" con la ruta "./scripts" y "Privado" con la ruta "~/horizonte/scripts".
/// </remarks>
public class ScriptsConfig
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="ScriptsConfig"/> con las fuentes de scripts predeterminadas.
    /// </summary>
    public ScriptsConfig()
    {
        Sources.Add(new ScriptsSourceItem { Name = "App", Path = "./scripts" });
        Sources.Add(new ScriptsSourceItem { Name = "Privado", Path = "~/horizonte/scripts" });
    }

    /// <summary>
    /// Obtiene o establece la lista de ubicaciones o fuentes (<see cref="ScriptsSourceItem"/>) desde las cuales se pueden cargar scripts.
    /// </summary>
    public List<ScriptsSourceItem> Sources { get; set; } = new List<ScriptsSourceItem>();

    /// <summary>
    /// Obtiene o establece la lista de definiciones de scripts (<see cref="ScriptDef"/>) configurados en el sistema.
    /// </summary>
    /// <remarks>
    /// Esta colección es utilizada para cargar, compilar y ejecutar los módulos de script dentro del entorno de Horizonte.
    /// </remarks>
    public List<ScriptDef> Scripts { get; set; } = new List<ScriptDef>();
}

