using System.Reflection;

namespace Horizonte.Extensions.Interactive;

/// <summary>
/// Representa la respuesta de la compilación de un <see cref="ScriptDef"/>.
/// Esta clase almacena los resultados de la operación de compilación, incluyendo
/// el ensamblado generado y cualquier error que se haya producido durante el proceso.
/// </summary>
public class ScriptDefCompileResponse
{
    /// La propiedad `Assembly` representa un conjunto compilado de código en forma de un arreglo de bytes.
    /// Esta propiedad es utilizada para almacenar el resultado de una compilación de un script en tiempo de ejecución.
    /// En el contexto de la clase `ScriptDefCompileResponse`, `Assembly` se inicializa como un arreglo vacío de bytes,
    /// y posteriormente, durante el proceso de compilación de código mediante el método `Compile` en la clase `Extensions`,
    /// se asigna el resultado del ensamblado generado al flujo de memoria (`MemoryStream`).
    /// Esto permite que el código de script dinámico sea compilado y ejecutado en tiempo de ejecución.
    /// /
    public byte[] Assembly { get; set; } = Array.Empty<byte>();

    /// Propiedad que proporciona una colección enumerable de errores generados durante un proceso de compilación de scripts.
    /// Es utilizada para recuperar la lista de mensajes de error que se producen al compilar un script.
    /// Cada error es representado como una cadena que describe el problema específico encontrado durante la compilación.
    /// Esta propiedad es particularmente útil para la depuración y para informar al usuario sobre los problemas que deben corregirse.
    /// Por defecto, se inicializa como una colección vacía, lo que indica que no hubo errores en el proceso de compilación.
    /// /
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
