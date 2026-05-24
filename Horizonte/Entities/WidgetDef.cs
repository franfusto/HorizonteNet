namespace Horizonte;

/// La clase `WidgetDef` se utiliza para definir un widget en el marco Horizonte.
/// Ofrece propiedades para especificar el tipo de widget y un diccionario opcional
/// para los parámetros que configuran el comportamiento o la apariencia del widget.
/// - `Type`: Indica el tipo del widget a través de una referencia al tipo de clase.
/// - `Parameters`: Un diccionario opcional que puede contener pares clave-valor para
/// personalizar el widget. Si no se especifican parámetros, su valor es nulo.
/// Esta clase se utiliza principalmente en el contexto de creación o configuración
/// de widgets dentro de módulos definidos en Horizonte, facilitando la extensión
/// y personalización de la interfaz de usuario de la aplicación.
/// /
public class WidgetDef
{
    /// La propiedad `Type` en la clase `WidgetDef` representa un identificador de tipo CLR (Common Language Runtime) utilizado para definir el tipo del widget correspondiente en las aplicaciones de la plataforma Horizonte.
    /// Esta propiedad permite almacenar y acceder dinámicamente al tipo específico de widget, lo cual es crucial para gestionar la configuración y el comportamiento de los widgets en tiempo de ejecución. Se utiliza en conjunto con otros componentes para personalizar las interacciones y funcionalidades del usuario con el sistema.
    /// /
    public Type Type { get; set; }

    /// <summary>
    /// Propiedad que permite establecer o recuperar un diccionario de parámetros personalizados para el widget.
    /// Este diccionario contiene pares clave-valor donde la clave es un string que representa el nombre del parámetro
    /// y el valor es un objeto que representa el valor asociado a dicho parámetro.
    /// </summary>
    public Dictionary<string, object>? Parameters { get; set; }
}