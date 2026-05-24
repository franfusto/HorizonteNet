namespace Horizonte;

/// La clase SymLinkDef representa una definición de enlace simbólico en un sistema de archivos.
/// Un enlace simbólico es una referencia que apunta a otro archivo o directorio en el sistema.
/// Esta clase encapsula la información necesaria para definir un enlace simbólico incluyendo:
/// - La ruta fuente (`Source`), que indica el archivo o directorio original al que se dirige el enlace.
/// - La ruta de destino (`Destination`), que especifica la ubicación del nuevo enlace simbólico.
/// - Un indicador (`IsDirecory`) para determinar si el enlace apunta a un directorio.
/// - Un indicador (`HardCoded`) que determina si la definición del enlace es fija o puede ser modificada.
/// /
public class SymLinkDef
{
    /// La propiedad `Source` representa la ubicación de origen en el sistema de archivos para una definición de enlace simbólico.
    /// En el contexto de enlaces simbólicos, esta propiedad almacena el camino del directorio o archivo desde donde el enlace simbólico
    /// apunta. Es una cadena de texto que generalmente contiene una ruta relativa o absoluta. Se utiliza para especificar de dónde debe
    /// comenzar el enlace simbólico al crear o verificar enlaces en el sistema.
    /// /
    public string  Source { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que define el destino de un enlace simbólico.
    /// Indica la ruta relativa a la que el enlace simbólico apuntará en el sistema de archivos.
    /// Esta propiedad debe contener una ruta que sea relativa al directorio de trabajo actual.
    /// Si se especifica una ruta absoluta, se generará un error.
    /// </summary>
    public string Destination { get; set; } = string.Empty;

    /// Propiedad que indica si el enlace simbólico está destinado a un directorio.
    /// Esta propiedad es de tipo booleano y se utiliza para determinar si el destino del enlace simbólico tratado debe ser interpretado como un directorio.
    /// Valor por defecto: `false`.
    /// /
    public bool IsDirecory { get; set; } = false;

    /// Propiedad que indica si el enlace simbólico es predefinido o fijo en el sistema.
    /// En el contexto de la clase `SymLinkDef`, esta propiedad se utiliza para señalar
    /// si el enlace simbólico fue determinado de forma explícita, es decir,
    /// configurado manualmente o a través de un fichero de configuración estática,
    /// en contraposición a aquellos enlaces que son generados o resueltos
    /// dinámicamente durante la ejecución del programa.
    /// La propiedad se establece como `true` por defecto, lo que implica que, a menos
    /// que se especifique lo contrario, los enlaces simbólicos se consideran codificados
    /// de forma estricta.
    /// /
    public bool HardCoded { get; set; } = true;
}