using System.Text;
using System.Text.Json.Serialization;

namespace Horizonte.Extensions.Interactive;

/// La clase ScriptDef representa la definición de un script dentro del contexto de la extensión interactiva de Horizonte.
/// Esta clase gestionará los detalles básicos y el código asociado al script, permitiendo su manipulación y almacenamiento.
/// Propiedades:
/// - Id: Cadena que identifica de manera única al script.
/// - Name: Nombre del script, el cual se procesa para asegurar que sea válido de acuerdo a restricciones específicas.
/// - Description: Breve texto que describe la funcionalidad o propósito del script.
/// - Code: Cadena en formato Base64 que contiene el código del script. Está diseñado para facilitar su almacenamiento y transmisión.
/// - CodeText: Propiedad que proporciona el acceso al código del script en texto plano. Gestiona la codificación y decodificación en Base64 del código del script.
/// - Active: Indicador booleano que señala si el script está activo y disponible para su uso.
public class ScriptDef
{
    /// <summary>
    /// Representa un identificador único para una instancia de la clase <c>ScriptDef</c>.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para asignar y obtener un valor que identifica de manera única
    /// a cada script dentro de la configuración de scripts. Es fundamental para operaciones que
    /// requieren la distinción entre diferentes scripts, como la carga, modificación o eliminación
    /// de un script específico. El valor por defecto es una cadena vacía.
    /// </remarks>
    public string Id { get; set; } = string.Empty;
    private string _name = string.Empty;

    /// <summary>
    /// Representa el nombre del script dentro de la clase <c>ScriptDef</c>.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para asignar y recuperar el nombre que identifica al script de manera legible por el usuario.
    /// El valor es procesado mediante un método de sanitización para asegurar que cumple con las normas de nomenclatura establecidas.
    /// Es importante para distinguir entre diferentes scripts en interfaces de usuario y gestión de configuraciones.
    /// </remarks>
    public string Name
    {
        get => _name;
        set => _name = SanitizeName(value);
    }


    /// <summary>
    /// Proporciona una descripción para una instancia de la clase <c>ScriptDef</c>.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite asignar y obtener una descripción detallada del script,
    /// que puede incluir información sobre su propósito, funcionalidad o cualquier otra
    /// observación relevante. Es útil para identificar rápidamente el objetivo o la especificidad
    /// del script cuando se presentan múltiples scripts en un entorno interactivo.
    /// El valor inicial es una cadena vacía.
    /// </remarks>
    public string Description { get; set; } = string.Empty;

    private string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        var sb = new StringBuilder();
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Proporciona el contenido del código fuente para una instancia de la clase <c>ScriptDef</c>.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se usa para almacenar y recuperar el código fuente del script asociado
    /// a una determinada instancia de <c>ScriptDef</c>. Es esencial para operaciones que
    /// implican la ejecución o manipulación del script, tales como compilación, edición
    /// y almacenamiento persistente. El valor por defecto es una cadena vacía, lo que asegura
    /// que el script tenga siempre un estado inicial válido.
    /// </remarks>
    public string Code { get; set; } = string.Empty;


    /// <summary>
    /// Proporciona el texto del código fuente para una instancia de la clase <c>ScriptDef</c>.
    /// </summary>
    /// <remarks>
    /// Esta propiedad permite asignar y obtener el texto del código fuente de un script asociado,
    /// manejando internamente su codificación en base64 para el almacenamiento seguro y eficiente
    /// de datos. Al establecerse un nuevo valor, el texto es convertido a base64 y almacenado en la
    /// propiedad subyacente, <c>Code</c>. Al obtenerse, se decodifica el texto desde base64. Si el
    /// texto almacenado no es un valor base64 válido, se devuelve como está para evitar pérdida de datos.
    /// </remarks>
    [JsonIgnore]
    public string CodeText
    {
        get
        {
            if (string.IsNullOrEmpty(Code)) return string.Empty;
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(Code));
            }
            catch
            {
                return Code; // Si no es base64 válido, devolvemos el valor original para evitar pérdida de datos
            }
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Code = string.Empty;
            }
            else
            {
                Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            }
        }
    }

    /// <summary>
    /// Indica si una instancia de la clase <c>ScriptDef</c> está actualmente activa.
    /// </summary>
    /// <remarks>
    /// La propiedad <c>Active</c> se utiliza para determinar si un script en particular
    /// está en uso o habilitado. Este estado puede afectar su ejecución y visibilidad
    /// dentro de una aplicación. Por defecto, su valor es <c>false</c>, indicando que
    /// el script no está activo. Al modificar esta propiedad, se puede activar o
    /// desactivar el script según los requerimientos del sistema.
    /// </remarks>
    public bool Active { get; set; } = false;
}