using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Controls;
using Blazor.Diagrams.Core.Behaviors;
using Blazor.Diagrams.Core.Events;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using Blazor.Diagrams.Core.Positions;
using System;
using System.Threading.Tasks;
using Horizonte.Extension.AiWorkFlows;

namespace Horizonte.WorkFlows.NodeModels;

/// <summary>
/// La clase <c>LinkSettingsControl</c> representa un control ejecutable utilizado para gestionar configuraciones de enlaces
/// dentro de un diagrama de flujos de trabajo.
/// </summary>
/// <remarks>
/// Esta clase permite editar y eliminar condiciones asociadas a enlaces de flujos de trabajo.
/// Implementa métodos para obtener la posición del control en función de los límites de un modelo
/// y para manejar eventos de puntero.
/// </remarks>
public class LinkSettingsControl : ExecutableControl
{
    /// `WorkFlowDef` es una clase que representa la definición de un flujo de trabajo en el contexto de una aplicación.
    /// Contiene las propiedades esenciales para definir las características y la estructura de un flujo de trabajo.
    /// Propiedades:
    /// - `Id`: Identificador único del flujo de trabajo. Se inicializa con un valor de GUID vacío.
    /// - `Name`: Nombre del flujo de trabajo. Se inicializa como una cadena vacía.
    /// - `Nodes`: Lista de nodos (`WorkFlowNode`) que conforman el flujo de trabajo. Se inicializa como una lista vacía.
    /// - `Links`: Lista de enlaces (`WorkFlowLink`) que conectan los nodos del flujo de trabajo. Se inicializa como una lista vacía.
    /// La clase `WorkFlowDef` permite la configuración y seguimiento de los nodos y enlaces que componen un flujo de trabajo automatizado,
    /// permitiendo así la creación de procesos dinámicos y personalizados.
    /// /
    public WorkFlowDef WorkFlowDef { get; private set; }

    /// <summary>
    /// La propiedad `WorkFlowLink` representa un enlace dentro de un flujo de trabajo en el contexto del control de configuración de enlaces.
    /// </summary>
    /// <remarks>
    /// Esta propiedad mantiene un objeto `WorkFlowLink`, que es esencial para gestionar la configuración, edición y eliminación de condiciones
    /// asociadas con el enlace en un flujo de trabajo. Es de solo lectura desde fuera de la clase, garantizando que solo se pueda modificar internamente,
    /// lo cual proporciona seguridad y control sobre las modificaciones que se realizan.
    /// </remarks>
    public WorkFlowLink WorkFlowLink { get; private set; }

    /// <summary>
    /// La propiedad <c>Bounds</c> representa el área rectangular correspondiente a las dimensiones del control, de tipo <c>Rectangle</c>.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para obtener o establecer los límites espaciales del control dentro del diagrama y es esencial para calcular posiciones relativas.
    /// Inicialmente, se establece en <c>Rectangle.Zero</c> y se actualiza al invocar el método <c>GetPosition</c> si el modelo implementa la interfaz <c>IHasBounds</c>.
    /// </remarks>
    public Rectangle Bounds { get; private set; } = Rectangle.Zero;

    /// La propiedad `Diagram` en la clase `LinkSettingsControl` representa el diagrama asociado al control de configuración del enlace.
    /// Esta propiedad es de solo lectura, ya que se define con un modificador de acceso privado para el set.
    /// El propósito de esta propiedad es proporcionar acceso al diagrama en el cual está operando el control de enlace,
    /// permitiendo así realizar operaciones relacionadas con la interacción y configuración de enlaces en el contexto del diagrama.
    /// /
    public Diagram Diagram { get; private set; }

    /// <summary>
    /// La clase <c>LinkSettingsControl</c> representa un control ejecutable dentro de un flujo de trabajo
    /// que actúa sobre un enlace de flujo de trabajo definido.
    /// </summary>
    /// <remarks>
    /// Esta clase se utiliza dentro del contexto de los flujos de trabajo en el espacio de nombres
    /// <c>Horizonte.WorkFlows.NodeModels</c>. Hereda de <c>ExecutableControl</c> y requiere una instancia
    /// de <c>WorkFlowLink</c> para su construcción.
    /// </remarks>
    public LinkSettingsControl(WorkFlowLink LinkDef)
    {
        WorkFlowLink = LinkDef;
    }

    /// <summary>
    /// Obtiene la posición de un modelo específico, basándose en los límites del mismo.
    /// </summary>
    /// <param name="model">El modelo para el cual se va a obtener la posición. Debe implementar la interfaz IHasBounds.</param>
    /// <returns>
    /// Un objeto <c>Point</c> que representa la coordenada oeste (West) del rectángulo de límites del modelo
    /// proporcionado. Retorna <c>null</c> si el modelo no implementa IHasBounds o si los límites son nulos.
    /// </returns>
    public override Point? GetPosition(Model model)
    {
        if (model is not IHasBounds hb)
            return null;

        var bounds = hb.GetBounds();
        if (bounds == null)
            return null;

        Bounds = bounds;
        return Bounds.West;
    }

    /// <summary>
    /// Método que se ejecuta cuando se detecta un evento de presión del puntero sobre el control.
    /// </summary>
    /// <param name="diagram">El diagrama en el que se realiza el evento.</param>
    /// <param name="model">El modelo asociado al control.</param>
    /// <param name="e">Los argumentos del evento que contienen información sobre el evento de presión del puntero.</param>
    /// <return>Una tarea de valor que representa la operación asincrónica.</return>
    public override ValueTask OnPointerDown(Diagram diagram, Model model, PointerEventArgs e)
    {
        return new ValueTask();
    }

    /// La propiedad OnConditionChanged es un delegado de tipo Action que se activa cuando ocurre un cambio en la condición de un enlace de flujo de trabajo.
    /// Esta propiedad es utilizada en el contexto de un control de interfaz de usuario llamado LinkSettingsControl que maneja la configuración de enlaces en un flujo de trabajo.
    /// Se invoca el delegado cada vez que se edita o se elimina una condición asociada a un enlace de flujo de trabajo, permitiendo así la ejecución de lógica personalizada en respuesta a estos cambios.
    /// /
    public required Action OnConditionChanged { get; set; }

    /// <summary>
    /// Modifica la condición de un enlace de flujo de trabajo existente.
    /// </summary>
    /// <param name="updLink">
    /// La instancia de <c>WorkFlowLink</c> que contiene los datos actualizados del enlace,
    /// incluida la nueva condición a establecer.
    /// </param>
    public void EditCondition(WorkFlowLink updLink)
    {
        WorkFlowLink = updLink;
        OnConditionChanged?.Invoke();
    }

    /// <summary>
    /// Elimina la condición asociada al enlace de flujo de trabajo existente en <see cref="WorkFlowLink"/>.
    /// Una vez eliminada la condición, se invoca el evento <see cref="OnConditionChanged"/> para notificar que
    /// ha ocurrido un cambio en la condición.
    /// </summary>
    public void DeleteCondition()
    {
        WorkFlowLink.Condition = null;
        OnConditionChanged?.Invoke();
    }
}