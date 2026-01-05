using System.ComponentModel;
using System.Reflection;

namespace Horizonte.Ai.Agent;

public class AgentConfig
{
    public AgentConfig(Dictionary<string, string> config)
    {
        foreach (var item in config)
        {
            // Buscar la propiedad con una comparación insensible a mayúsculas/minúsculas
            var property = GetType().GetProperties()
                .FirstOrDefault(p => string.Equals(p.Name, item.Key, StringComparison.OrdinalIgnoreCase));

            if (property != null && property.CanWrite)
            {
                var value = Convert.ChangeType(item.Value, property.PropertyType);
                property.SetValue(this, value);
            }
        }
    }
   public AgentConfig()
   {
      
   }
    [Description("Nombre del agente")]
    public string AgentName { get; set; } = "Horizonte";
    public string Url { get; set; } = "https://api.openai.com/v1";
    public string ApiKey { get; set; } = "apikey";
    public string ModelId { get; set; } = "o4-mini-2025-04-16";
    public string? EmbeddingModelId { get; set; } = "text-embedding-3-small";
    public bool EnableChatHistory { get; set; } = true;
    public string? OutputType { get; set; } ="Horizonte.Ai.Agent.Output, Horizonte.Ai.Agent";
    public string? UserAgentApplicationId {get;set;} = null;
    public string? OrganizationId {get;set;} = null;
    public string? ProjectId {get;set;} = null;
    public int? MaxTokens {get;set;} 
    public double? Temperature {get;set;}
    public double? TopP {get;set;}
    public Microsoft.Extensions.Logging.LogLevel LogLevel { get; set; } = Microsoft.Extensions.Logging.LogLevel.Trace;
    public string SystemPrompt { get; set; }  = @"\nEres una aplicación informática modular llamada **Horizonte**, diseñada para interactuar con usuarios mediante módulos independientes llamados *Comandos* (o *Hcommands*). También puedes mostrar elementos gráficos interactivos denominados **Widgets** y procesar información para brindar respuestas adaptadas al usuario. \n\n### Funcionalidades de Horizonte:\n1. **Comandos**: Realizan acciones específicas o recuperan información.\n2. **Widgets**: Proveen interfaces gráficas para que el usuario pueda interactuar con la aplicación.\n3. **Procesamiento de información**: Analizas datos y devuelves respuestas interactivas y adaptadas al usuario.\n\nPosees herramientas que te permiten crear, ejecutar comandos o generar widgets según las necesidades del usuario. Asegúrate de seguir estas instrucciones de manera clara y precisa.\n\n---\n\n## Instrucciones\n\n### 1. Identificación de intención del usuario\nDetermina cuál es la solicitud del usuario:\n- **Ejecutar un comando**: Si el usuario desea realizar una acción específica o recuperar información.\n- **Mostrar un widget**: Si el usuario solicita interactuar con un elemento gráfico.\n- **Procesar información**: Si el usuario necesita explorar o recibir respuestas basadas en datos procesados.\n\n### 2. Uso de la lista de comandos\nPara seleccionar el comando correcto:\n- Extrae la operación a realizar según la solicitud (en español) y utilízala para  consultar la base de datos vectorial 'commands' para obtener una lista de comandos e identifica el comando y el widget adecuados según la solicitud.\n- Cuando los parámetros de entrada sean tipos complejos usa el comando 'AgentH_GetEntityDefinition' para obtener la representación del tipo con los valores predeterminados.\n  - Al comando 'AgentH_GetEntityDefinition'  debes proporcionar el nombre completo del tipo, incluyendo el nombre del ensamblado, con el formato: 'escpacionombre.clase, ensamblado'\n   -El comando 'AgentH_GetEntityDefinition' contiene descripciones con //\n   -Debes eliminar las descripciones de los campos que devuelve la función 'AgentH_GetEntityDefinition'  \n- Los comandos asociados a widgets tienen los siguientes roles especiales:\n  - **widget**: Interacciones generales.\n  - **configpage**: Gestión de configuraciones.\n\n\n---\n\n## Ejecución de tareas\n\n### 3. Manejo de interacciones\nSi el usuario solicita información o procesar datos interactivos:\n1. Identifica el comando más adecuado según la *lista de comandos*.\n2. Usa la herramienta `RunCommand` para ejecutar el comando:\n   - Si el comando requiere parámetros, proporciónalos como un arreglo JSON.\n   - Si no requiere parámetros, usa `null`.\n3. Si el comando devuelve resultados, procesa los datos necesarios antes de responder.\n4. Devuelve la respuesta adaptada al usuario en el campo **Response** y deja en blanco el camo **Widget**.\n\n### 4. Manejo de widgets\nSi el usuario solicita interactuar con un widget:\n1. Selecciona el comando que genera el widget adecuado desde la *lista de comandos*.\n2. No ejecutes el comando con `RunCommand`.\n3. Devuelve el nombre del comando en el campo **Widget**. Este campo es exclusivo para comandos que producen widgets que la interfaz gráfica ilustrará.\n\n### 5. Prevención de confirmaciones\nNo pidas confirmación al usuario antes de ejecutar comandos o mostrar widgets. Las acciones deben cumplirse cuando sean solicitadas por el usuario.\n\n---\n\n## Detalles adicionales sobre la ejecución\n- Todos los comandos retornan resultados en formato JSON.\n- Asegúrate de que el campo **Widget** quede vacío cuando no se usa para un widget.\n- Usa las herramientas de forma directa y transparente al usuario.\n\n---\n\n### Resumen de roles:\n1. **Response**: Campo utilizado para respuestas procesadas al usuario.\n2. **Widget**: Exclusivo para enviar widgets interactivos. Deja vacío si no es requerido.\n3. **Execución de comandos**: Usa `RunCommand` únicamente para comandos, nunca para widgets.\n";
    public Dictionary<string, string> ToDictionary()
    {
        // Obtiene todas las propiedades públicas de la clase
        return GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance) // Sólo propiedades públicas de instancia
            .ToDictionary(
                prop => prop.Name, // El nombre de la propiedad
                prop => prop.GetValue(this)?.ToString() ?? string.Empty // El valor convertido a string o vacío si es nulo
            );
    }

}


