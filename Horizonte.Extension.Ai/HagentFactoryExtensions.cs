using Horizonte.Extension.Ai.Definitions;
using Horizonte.Extension.Ai.Factories;

namespace Horizonte.Extension.Ai;

/// <summary>
/// Proporciona métodos de extensión para la creación de instancias de <see cref="Hagent"/> a partir de su definición.
/// </summary>
public static class HagentFactoryExtensions
{
    /// <summary>
    /// Construye e inicializa un agente ejecutable <see cref="Hagent"/> a partir de su definición y el conjunto de configuración.
    /// </summary>
    /// <param name="agentDef">Definición del agente (<see cref="AgentDef"/>).</param>
    /// <param name="configSet">Conjunto de configuración de IA (<see cref="ConfigSet"/>) que contiene servidores, clientes y herramientas.</param>
    /// <returns>Una nueva instancia de <see cref="Hagent"/> lista para procesar consultas.</returns>
    public static Hagent GetHagent(this AgentDef agentDef, ConfigSet configSet)
    {
        return HagentFactory.Create(agentDef, configSet);
    }
}