using Horizonte.Extension.AiWorkFlows;

namespace Horizonte.WorkFlows;

/// <summary>
/// Gestiona la configuración de los flujos de trabajo en el sistema Horizonte.
/// </summary>
/// <remarks>
/// Proporciona una colección de definiciones de flujos de trabajo (<see cref="WorkFlowDef"/>),
/// permitiendo agregar, modificar y persistir múltiples flujos en la configuración del contexto de la aplicación.
/// </remarks>
public class WorkFlowConfig
{
    /// <summary>
    /// Obtiene o establece la lista de definiciones de flujos de trabajo (<see cref="WorkFlowDef"/>).
    /// </summary>
    public List<WorkFlowDef> WorkFlows { get; set; } = new List<WorkFlowDef>();
}