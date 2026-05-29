using Horizonte.Extension.AiWorkFlows;

namespace Horizonte.WorkFlows;

/// La clase WorkFlowConfig es parte del espacio de nombres Horizonte.WorkFlows y se utiliza para gestionar configuraciones de flujos de trabajo en el contexto de la aplicación.
/// La clase proporciona una colección de definiciones de flujos de trabajo (WorkFlowDef), lo que permite agregar, modificar y eliminar flujos de trabajo según sea necesario.
/// Esta clase desempeña un papel crucial en la configuración y gestión de los diferentes flujos de trabajo que una aplicación puede tener, facilitando la organización y manipulación de los mismos a través de su colección. Es utilizada en componentes como Dashboard para cargar, guardar y gestionar las configuraciones actuales del flujo de trabajo en el sistema.
/// /
public class WorkFlowConfig
{
    /// <summary>
    /// Propiedad que representa una lista de definiciones de flujos de trabajo (WorkFlows) dentro de la configuración del flujo de trabajo.
    /// Este conjunto de flujos de trabajo permite gestionar y preservar múltiples configuraciones de flujos, cada una con su propio conjunto de nodos y enlaces.
    /// </summary>
    public List<WorkFlowDef> WorkFlows { get; set; } = new List<WorkFlowDef>();
}