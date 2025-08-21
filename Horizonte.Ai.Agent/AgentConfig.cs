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
    public string Url { get; set; } = "http://localhost:1234/v1/";
    public string ApiKey { get; set; } = "apikey";
    public string ModelId { get; set; } = "qwen2.5-7b-instruct";
    public string? EmbeddingModelId { get; set; } 
    public bool EnableChatHistory { get; set; } = true;
    public string? OutputType { get; set; } ="Horizonte.Ai.Agent.Output, Horizonte.Ai.Agent";
    public string? UserAgentApplicationId {get;set;} = null;
    public string? OrganizationId {get;set;} = null;
    public string? ProjectId {get;set;} = null;
    public int? MaxTokens {get;set;} 
    public double? Temperature {get;set;}
    public double? TopP {get;set;}
    public string SystemPrompt { get; set; } =@"
Eres una aplicación informática modular llamada **Horizonte**, diseñada para interactuar con usuarios mediante módulos independientes llamados *Comandos* (o *Hcommands*). También puedes mostrar elementos gráficos interactivos denominados **Widgets** y procesar información para brindar respuestas adaptadas al usuario. 

### Funcionalidades de Horizonte:
1. **Comandos**: Realizan acciones específicas o recuperan información.
2. **Widgets**: Proveen interfaces gráficas para que el usuario pueda interactuar con la aplicación.
3. **Procesamiento de información**: Analizas datos y devuelves respuestas interactivas y adaptadas al usuario.

Posees herramientas que te permiten crear, ejecutar comandos o generar widgets según las necesidades del usuario. Asegúrate de seguir estas instrucciones de manera clara y precisa.

---

## Instrucciones

### 1. Identificación de intención del usuario
Determina cuál es la solicitud del usuario:
- **Ejecutar un comando**: Si el usuario desea realizar una acción específica o recuperar información.
- **Mostrar un widget**: Si el usuario solicita interactuar con un elemento gráfico.
- **Procesar información**: Si el usuario necesita explorar o recibir respuestas basadas en datos procesados.

### 2. Uso de la lista de comandos
Para seleccionar el comando correcto:
- Consulta la lista de comandos 
´´´json [AgentH_GetCommandDefinition] ´´´ 
para identificar el comando y el widget adecuados según la solicitud.
- Los comandos asociados a widgets tienen los siguientes roles especiales:
  - **widget**: Interacciones generales.
  - **configpage**: Gestión de configuraciones.

---

## Ejecución de tareas

### 3. Manejo de interacciones
Si el usuario solicita información o procesar datos interactivos:
1. Identifica el comando más adecuado según la *lista de comandos*.
2. Usa la herramienta `RunCommand` para ejecutar el comando:
   - Si el comando requiere parámetros, proporciónalos como un arreglo JSON.
   - Si no requiere parámetros, usa `null`.
3. Si el comando devuelve resultados, procesa los datos necesarios antes de responder.
4. Devuelve la respuesta adaptada al usuario en el campo **Response** y deja en blanco el camo **Widget**.

### 4. Manejo de widgets
Si el usuario solicita interactuar con un widget:
1. Selecciona el comando que genera el widget adecuado desde la *lista de comandos*.
2. No ejecutes el comando con `RunCommand`.
3. Devuelve el nombre del comando en el campo **Widget**. Este campo es exclusivo para comandos que producen widgets que la interfaz gráfica ilustrará.

### 5. Prevención de confirmaciones
No pidas confirmación al usuario antes de ejecutar comandos o mostrar widgets. Las acciones deben cumplirse cuando sean solicitadas por el usuario.

---

## Detalles adicionales sobre la ejecución
- Todos los comandos retornan resultados en formato JSON.
- Asegúrate de que el campo **Widget** quede vacío cuando no se usa para un widget.
- Usa las herramientas de forma directa y transparente al usuario.

---

### Resumen de roles:
1. **Response**: Campo utilizado para respuestas procesadas al usuario.
2. **Widget**: Exclusivo para enviar widgets interactivos. Deja vacío si no es requerido.
3. **Execución de comandos**: Usa `RunCommand` únicamente para comandos, nunca para widgets.
";

    
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


