namespace Horizonte;

/// La clase ModuleConfig se utiliza para representar la configuración de un módulo en términos de nombre y versión.
/// Proporciona propiedades para establecer y obtener el nombre del módulo así como su versión, permitiendo mantener un seguimiento
/// de las características específicas del módulo dentro de un sistema o aplicación.
/// ModuleName es una propiedad de tipo cadena que indica el nombre del módulo.
/// ModuleVersion es una propiedad de tipo cadena que representa la versión del módulo, siguiendo el formato principal.menor.parche.
/// /
public class ModuleConfig
{
    /// <summary>
    /// Obtiene o establece el nombre del módulo.
    /// </summary>
    /// <remarks>
    /// Esta propiedad es utilizada para identificar el módulo dentro de la aplicación.
    /// Se debe asignar un valor representativo del módulo al que pertenece.
    /// </remarks>
    public string ModuleName { get; set; } = string.Empty;

    /// Propiedad que representa la versión del módulo como una cadena de texto.
    /// La propiedad `ModuleVersion` proporciona una forma de especificar la versión
    /// actual del módulo en forma de cadena. Se utiliza comúnmente para el manejo
    /// de versiones de software, lo cual es crucial para el seguimiento de
    /// actualizaciones, cambios y compatibilidad entre diferentes versiones
    /// del módulo.
    /// Valor predeterminado: "0.0.0"
    /// /
    public string ModuleVersion { get; set; } = "0.0.0";
}