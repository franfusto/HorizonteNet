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
    /// <summary>
    /// Obtiene la definición del flujo de trabajo asociada al control.
    /// </summary>
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

    /// <summary>
    /// Obtiene el diagrama al cual pertenece y en el cual opera el control de configuración del enlace.
    /// </summary>
    public Diagram Diagram { get; private set; }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="LinkSettingsControl"/> asociada a un enlace de flujo de trabajo.
    /// </summary>
    /// <param name="LinkDef">Instancia de <see cref="WorkFlowLink"/> que define el enlace del flujo.</param>
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

    /// <summary>
    /// Obtiene o establece el delegado que se ejecuta cuando se modifica o elimina la condición del enlace.
    /// </summary>
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