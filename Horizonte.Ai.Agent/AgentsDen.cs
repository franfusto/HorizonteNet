using System.ComponentModel;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Horizonte;
using Microsoft.Extensions.Logging;

namespace Horizonte.Ai.Agent;

public class AgentsDen
{
    private List<Agent> _agents = new(); // Lista que contiene todos los agentes gestionados.
    private readonly IHGesCom? _gesCom; // Componente externo para gestionar comandos.
    private readonly ILogger<AgentsDen>? _logger; // Logger para registrar eventos y errores.

    public AgentsDen(IHorizonteEnv env)
    {
        _gesCom = env.GetService<IHGesCom>(); // Obtiene instancia del servicio IHGesCom.
        _logger = env.GetService<ILogger<AgentsDen>>(); // Obtiene logger para la clase.
    }

    /// <summary>
    /// Retorna una lista con los nombres de todos los agentes registrados.
    /// </summary>
    /// <returns>Lista de nombres de agentes.</returns>
    public List<string> GetAgentsNames()
    {
        return _agents.Select(a => a.AgentName).ToList(); // Extrae y retorna los nombres de la lista de agentes.
    }

    /// <summary>
    /// Agrega un nuevo agente al sistema.
    /// </summary>
    /// <param name="agentname">Nombre del agente a agregar.</param>
    /// <param name="agentconfig">Configuración del agente.</param>
    /// <returns>True si se agregó correctamente, False si ya existía.</returns>
    public bool AddAgent(string agentname, AgentConfig agentconfig)
    {
        if (_agents.Any(a => a.AgentName == agentname)) // Verifica si ya existe un agente con el mismo nombre.
        {
            _logger?.LogWarning($"El agente '{agentname}' ya existe.");
            return false;
        }

        var agent = new Agent(agentname, agentconfig, _gesCom, _logger); // Crea una nueva instancia de Agent.
        _agents.Add(agent); // Lo agrega a la lista.
        _logger?.LogInformation($"Agente '{agentname}' agregado exitosamente.");
        return true;
    }

    /// <summary>
    /// Elimina un agente del sistema.
    /// </summary>
    /// <param name="agentname">Nombre del agente a eliminar.</param>
    public void RemoveAgent(string agentname)
    {
        var agent = _agents.FirstOrDefault(a => a.AgentName == agentname);
        if (agent == null)
        {
            _logger?.LogWarning($"El agente '{agentname}' no se encontró.");
            return;
        }

        _agents.Remove(agent); // Remueve el agente de la lista.
        _logger?.LogInformation($"Agente '{agentname}' eliminado exitosamente.");
    }

    /// <summary>
    /// Obtiene un agente específico por su nombre.
    /// </summary>
    /// <param name="agentname">Nombre del agente a buscar.</param>
    /// <returns>El agente, o null si no existe.</returns>
    public Agent? GetAgent(string agentname)
    {
        var agent = _agents.FirstOrDefault(a => a.AgentName == agentname);
        if (agent == null)
        {
            _logger?.LogWarning($"El agente '{agentname}' no se encontró.");
        }

        return agent;
    }

    /// <summary>
    /// Realiza una consulta específica a un agente.
    /// </summary>
    /// <param name="agentname">Nombre del agente.</param>
    /// <param name="query">Consulta a ejecutar.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>Resultado de la consulta.</returns>
    public object? Query(string agentname, string query, CancellationToken cancellationToken = default)
    {
        var agent = GetAgent(agentname);
        if (agent == null) return $"El agente '{agentname}' no existe.";
        return agent.Query(query, cancellationToken).Result; // Ejecución simulada de una consulta.
    }

    /// <summary>
    /// Obtiene el historial de acciones de un agente.
    /// </summary>
    /// <param name="agentname">Nombre del agente.</param>
    /// <returns>Historial del agente.</returns>
    public object GetHistory(string agentname)
    {
        var agent = GetAgent(agentname);
        if (agent == null) return $"El agente '{agentname}' no existe.";
        return agent.GetHistory(); // Recupera el historial del agente.
    }

    /// <summary>
    /// Limpia el historial de un agente.
    /// </summary>
    /// <param name="agentname">Nombre del agente.</param>
    public void ClearHistory(string agentname)
    {
        var agent = GetAgent(agentname);
        agent?.InitializeAgent(); // Inicializa el agente, esto Limpia el historial del agente.
    }

    /// <summary>
    /// Obtiene estadísticas de un agente.
    /// </summary>
    /// <param name="agentname">Nombre del agente.</param>
    /// <returns>Diccionario con estadísticas clave-valor.</returns>
    public Dictionary<string, string> GetStatics(string agentname)
    {
        var agent = GetAgent(agentname);
        if (agent == null) return new Dictionary<string, string> { { "Error", "Agente no encontrado" } };
        return agent.GetStatics(); // Recupera estadísticas del agente.
    }


    public string? GetEntityDefinition(string entityname)
    {
        try
        {
            // Intentar cargar el tipo a partir del nombre completo del tipo proporcionado
            var entityType = Type.GetType(entityname);

            if (entityType == null)
            {
                throw new ArgumentException(
                    $"No se pudo encontrar el tipo '{entityname}'. Asegúrate de proporcionar el nombre completo del tipo, incluyendo el ensamblado.");
            }

            // Crear una instancia por defecto del tipo
            var instance = Activator.CreateInstance(entityType);

            if (instance == null)
            {
                throw new InvalidOperationException(
                    $"No se pudo crear una instancia del tipo '{entityname}'. Asegúrate de que tenga un constructor sin parámetros.");
            }

            // Obtener las propiedades públicas de la clase
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Usar un StringBuilder para construir el JSON-like con comentarios
            var jsonBuilder = new StringBuilder();
            jsonBuilder.AppendLine("{");

            foreach (var property in properties)
            {
                // Obtener el valor por defecto de la propiedad
                var defaultValue = property.GetValue(instance);

                // Leer la descripción, si existe
                var descriptionAttr = property.GetCustomAttribute<DescriptionAttribute>();
                var description = descriptionAttr?.Description ?? "Sin descripción";

                // Agregar la propiedad en formato "nombre": valor, seguido del comentario
                var value = defaultValue == null
                    ? "null"
                    : FormatValue(defaultValue); // Manejar valores predeterminados
                jsonBuilder.AppendLine($"  \"{property.Name}\": {value}, // {description}");
            }

            // Eliminar la última coma para un formato JSON válido
            if (properties.Any())
            {
                jsonBuilder.Remove(jsonBuilder.Length - 3, 1); // Remover la última coma ','
            }

            jsonBuilder.AppendLine("}");
            var result = jsonBuilder.ToString();
            return result;
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
            return null;
        }
    }

    private string FormatValue(object value)
    {
        // Si el valor es un string, envolverlo en comillas
        if (value is string stringValue)
        {
            return $"\"{stringValue}\"";
        }

        // Si es un booleano, convertirlo a texto JSON (true/false)
        if (value is bool boolValue)
        {
            return boolValue.ToString().ToLower();
        }

        // Cualquier otro valor devolverlo como está (números, etc.)
        return value.ToString();
    }


    /// <summary>
    /// Genera una definición en formato JSON con la lista de comandos disponibles.
    /// </summary>
    /// <returns>Cadena JSON con la definición de comandos.</returns>
    public string GetCommandDefinition()
    {
        List<CommandDefinition> agentComList = new();
        try
        {
            var list = _gesCom?.GetCommandList() as List<HCommand>;
            if (list == null)
            {
                _logger?.LogWarning("GetCommandList no devolvió una lista válida.");
                return "[]";
            }

            foreach (var item in list)
            {
                var listitem = new CommandDefinition
                {
                    CommandName = item.CommandKey,
                    CommandDescription = item.Description,
                    CommandRoles = item.Roles.ToArray(),
                };

                if (item.InNames != null && item.InTypes != null && item.InNames.Count == item.InTypes.Count)
                {
                    for (int i = 0; i < item.InNames.Count; i++)
                    {
                        listitem.Parameters.Add(new CommandParameterDefinition
                        {
                            Type = item.InTypes[i]?.AssemblyQualifiedName ?? "Tipo desconocido",
                            Name = item.InNames[i]
                        });
                    }
                }
                else
                {
                    _logger?.LogWarning(
                        $"Error en los parámetros de {item.CommandKey}: listas inválidas o de distinta longitud.");
                }

                agentComList.Add(listitem);
            }

            return JsonSerializer.Serialize(agentComList, new JsonSerializerOptions
            {
                WriteIndented = false,
                IncludeFields = true,
            });
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error al recuperar los comandos disponibles");
        }

        return "[]";
    }
}