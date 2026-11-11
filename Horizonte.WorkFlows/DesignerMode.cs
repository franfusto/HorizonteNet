namespace Horizonte.WorkFlows;

/// <summary>
/// Enumeración <c>DesignerMode</c> que define los diferentes modos de operación
/// para un diseñador de flujos de trabajo en la aplicación.
/// </summary>
/// <remarks>
/// Esta enumeración es utilizada para determinar el comportamiento visual y funcional
/// de los elementos de interfaz de usuario basados en el modo actual en el que se encuentra
/// el diseñador.
/// </remarks>
/// <enum value="Show">
/// Modo de visualización donde los elementos del nodo se muestran solo para lectura,
/// sin permitir ediciones.
/// </enum>
/// <enum value="Design">
/// Modo de diseño que permite a los usuarios editar los detalles del nodo, como su nombre
/// y las acciones de comando asociadas.
/// </enum>
/// <enum value="Run">
/// Modo de ejecución utilizado para mostrar el mensaje de ejecución de un nodo.
/// </enum>
public enum DesignerMode
{
    /// <summary>
    /// El miembro "Show" del enum "DesignerMode" indica un modo de visualización
    /// donde se presenta la información básica del nodo, como su nombre y la acción de comando asociada.
    /// En este modo, no se permite la edición, sirviendo únicamente para mostrar los datos existentes.
    /// Es útil cuando se requiere una visión general del flujo de trabajo sin modificar su configuración.
    /// </summary>
    Show,

    /// <summary>
    /// Modo de diseño que permite a los usuarios editar los detalles y configuración del nodo.
    /// </summary>
    Design,

    /// <summary>
    /// Modo de ejecución utilizado para visualizar el estado y los mensajes generados durante la ejecución del flujo.
    /// </summary>
    Run
}