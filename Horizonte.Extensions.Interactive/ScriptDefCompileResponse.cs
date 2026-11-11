using System.Reflection;

namespace Horizonte.Extensions.Interactive;

/// <summary>
/// Representa la respuesta de la compilación de un <see cref="ScriptDef"/>.
/// Esta clase almacena los resultados de la operación de compilación, incluyendo
/// el ensamblado generado y cualquier error que se haya producido durante el proceso.
/// </summary>
public class ScriptDefCompileResponse
{
    /// <summary>
    /// Obtiene o establece el ensamblado compilado en forma de arreglo de bytes.
    /// </summary>
    public byte[] Assembly { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Obtiene o establece la colección de mensajes de error generados durante el proceso de compilación del script.
    /// </summary>
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
