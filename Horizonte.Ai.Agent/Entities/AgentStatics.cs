using System.Linq;
using System.Reflection;
using OpenAI.Chat;
using ChatMessageContent = Microsoft.SemanticKernel.ChatMessageContent;

namespace Horizonte.Ai.Agent;

/// <summary>
/// Clase utilizada para realizar el seguimiento y análisis de estadísticas relacionadas con los mensajes procesados por un agente.
/// Proporciona funcionalidades para calcular y recuperar estadísticas de tokens y funciones utilizadas.
/// </summary>
public class AgentStatics
{

    /// <summary>
    /// Procesa los mensajes y actualiza las estadísticas de tokens y funciones.
    /// </summary>
    /// <param name="chatMessageContents">Una lista de mensajes procesados.</param>
    public void ComputeResponse(IReadOnlyList<ChatMessageContent> chatMessageContents)
    {
        if (chatMessageContents == null! || !chatMessageContents.Any())
        {
            return;
        }

        foreach (var message in chatMessageContents)
        {
            var usage = (message.InnerContent as ChatCompletion)?.Usage ;
            var functions = (message.InnerContent as ChatCompletion)?.ToolCalls ;
            if (usage == null) continue;
            
            // Sumar la cantidad de tokens de entrada.
            InputTokenCount += usage.InputTokenCount;

            
            // Sumar la cantidad de tokens de salida.
            OutputTokenCount += usage.OutputTokenCount;

            // Procesar las funciones invocadas.
            FunctionCount += functions?.Count ?? 0;
        }
        TotalInputTokenCount += InputTokenCount;
        TotalOutputTokenCount += OutputTokenCount;
        TotalFunctionCount += FunctionCount;
    }

    /// <summary>
    /// Representa la cantidad total de tokens de entrada procesados hasta el momento.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se incrementa al sumar la cantidad de tokens de entrada utilizados en cada mensaje procesado.
    /// Su valor se utiliza para rastrear el uso de tokens de entrada a lo largo de múltiples operaciones.
    /// Es de solo lectura desde fuera de la clase y se inicializa en 0.
    /// </remarks>
    public int InputTokenCount { get; private set; } = 0;

    /// <summary>
    /// Representa la cantidad de tokens de salida generados durante el procesamiento de los mensajes.
    /// Este valor se actualiza al procesar los mensajes y refleja la suma de los tokens generados como respuesta.
    /// </summary>
    public int OutputTokenCount { get; private set; } = 0;

    /// <summary>
    /// Representa la cantidad total de funciones invocadas por los mensajes procesados.
    /// </summary>
    /// <remarks>
    /// El valor de esta propiedad se incrementa automáticamente al procesar los mensajes en el método
    /// <c>ComputeResponse</c>, sumando la cantidad de funciones invocadas asociadas a cada mensaje.
    /// Es de solo lectura para evitar modificaciones externas.
    /// </remarks>
    public int FunctionCount { get; private set; } = 0;

    /// <summary>
    /// Propiedad que representa el total acumulado de tokens de entrada procesados.
    /// </summary>
    /// <remarks>
    /// Este valor se incrementa al procesar mensajes de entrada mediante el método <c>ComputeResponse</c>.
    /// Refleja la suma total de todos los tokens de entrada manejados por la instancia.
    /// </remarks>
    public int TotalInputTokenCount { get; private set; } = 0;

    /// <summary>
    /// Representa el conteo total acumulado de tokens de salida procesados.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se actualiza al sumar el conteo de tokens de salida de cada mensaje procesado.
    /// Es útil para realizar un seguimiento del uso general de tokens de salida en las operaciones.
    /// </remarks>
    public int TotalOutputTokenCount { get; private set; } = 0;

    /// <summary>
    /// Propiedad que representa la cantidad total de funciones invocadas durante el procesamiento de mensajes.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se acumula al sumar las funciones invocadas en cada mensaje procesado.
    /// Es útil para rastrear el uso total de funciones en un flujo de trabajo.
    /// </remarks>
    public int TotalFunctionCount { get; private set; } = 0;

    /// <summary>
    /// Devuelve un diccionario con las propiedades de la clase y sus valores actuales.
    /// </summary>
    /// <returns>Diccionario con nombre de propiedad y valor como string.</returns>
    public Dictionary<string, string> GetStatics()
    {
        var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var result = new Dictionary<string, string>();

        foreach (var property in properties)
        {
            if (property.CanRead)
            {
                var value = property.GetValue(this)?.ToString() ?? "null";
                result.Add(property.Name, value);
            }
        }

        return result;
    }


}