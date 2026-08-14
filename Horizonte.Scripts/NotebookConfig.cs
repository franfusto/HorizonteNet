using System.Text.Json.Serialization;
using Microsoft.DotNet.Interactive.Documents.Jupyter;
using Microsoft.DotNet.Interactive.Documents;

namespace Horizonte.Scripts;

/// <summary>
/// 
/// </summary>
public class NotebookConfig
{
    /// <summary>
    /// 
    /// </summary>
    public NotebookConfig()
    {
        AddContentSample();
    }

    /// <summary>
    /// 
    /// </summary>
    public List<NotebookSource> Sources { get; set; } = new List<NotebookSource>();

    private void AddContentSample()
    {
        Sources.Add(new NotebookSource()
        {
            Type = NotebookSourceType.Webitems,
            Name =  "Web Notebooks",
            Items = new List<NotebookItem>()
        });

        Sources[0].Items.Add(new NotebookItem()
        {
            Document = null,
            ItemId = "SampleNotebook01",
            ItemPath = "https://raw.githubusercontent.com/dotnet/csharp-notebooks/refs/heads/main/notebook-getting-started/01-What%20are%20Notebooks.ipynb"
        });
       
    }
}

/// <summary>
/// Representa una fuente de cuadernos (notebooks) que puede ser utilizada para organizar y gestionar colecciones de notebooks de distintos orígenes.
/// </summary>
/// <remarks>
/// Un objeto de tipo NotebookSource puede contener información sobre el nombre de la fuente, su tipo, la ruta de origen, así como los elementos que pertenecen a esa fuente.
/// </remarks>
public class NotebookSource
{
    /// <summary>
    /// Representa el nombre de la fuente del cuaderno. Este nombre es una cadena
    /// que identifica de manera única la colección de cuadernos o el origen
    /// de los mismos, como por ejemplo "Web Notebooks" o "Local Notebooks".
    /// </summary>
    public string? Name { get; set; }

    /// Propiedad que representa la ruta de acceso donde se localizan las fuentes de cuadernos electrónicos.
    /// Se utiliza para definir el camino de acceso a directorios locales que contienen cuadernos que pueden ser procesados
    /// o mostrados por la aplicación. Este camino puede ser una ruta relativa o absoluta del sistema de archivos.
    /// /
    public string? SourcePath { get; set; }

    /// La propiedad `Type` representa el tipo de fuente desde la cual se obtienen los contenidos de los cuadernos de notas (notebooks).
    /// Esta propiedad es de tipo `NotebookSourceType`, que es un enumerador que define los posibles tipos de fuentes que se pueden utilizar
    /// para obtener cuadernos de notas. Los posibles valores que puede tomar esta propiedad son:
    /// - `localDirectory`: Indica que la fuente de los cuadernos está en un directorio local.
    /// - `webitems`: Indica que la fuente de los cuadernos está en la web.
    /// Esta propiedad es utilizada para definir desde dónde se recupera la información de los cuadernos, permitiendo así gestionar
    /// y organizar eficientemente las diferentes fuentes de contenido en una aplicación o sistema que maneje notebooks.
    /// /
    public NotebookSourceType Type { get; set; }

    /// <summary>
    /// Propiedad que representa una lista de objetos NotebookItem asociados a una fuente de cuadernos.
    /// </summary>
    /// <remarks>
    /// Esta propiedad es utilizada para almacenar y gestionar los diferentes elementos (cuadernos) de un origen de cuadernos específico,
    /// permitiendo su acceso y manipulación. Cada NotebookItem puede contener información como el identificador del elemento, la ruta
    /// del cuaderno, y opcionalmente el documento interactivo asociado si ya ha sido cargado.
    /// </remarks>
    public List<NotebookItem> Items { get; set; } = new List<NotebookItem>();
}

/// <summary>
/// La clase NotebookItem representa un elemento individual dentro de una colección de notebooks.
/// Cada NotebookItem se identifica de manera única mediante una combinación de un identificador y una ruta de acceso.
/// </summary>
public class NotebookItem
{
    /// Propiedad que representa el identificador único de un elemento Notebook.
    /// Esta propiedad es utilizada para diferenciar y acceder a notebooks específicos dentro de una colección de notebooks.
    /// Puede ser asignada y leída según sea necesario para identificar de manera única cada notebook.
    /// /
    public required string ItemId { get; set; }

    /// Propiedad que representa la ruta de acceso a un elemento de cuaderno.
    /// Puede ser una URL que apunte a un recurso web que contiene un cuaderno,
    /// o una ruta de archivo local en el sistema de archivos del usuario.
    /// Se utiliza para localizar y acceder al contenido del cuaderno específico.
    /// /
    public string? ItemPath { get; set; }

    /// Representa un documento interactivo dentro de un elemento de cuaderno (notebook).
    /// Esta propiedad permite acceder o modificar el documento interactivo asociado a un objeto `NotebookItem`.
    /// El documento es utilizado para definir el contenido y funcionamiento del cuaderno,
    /// facilitando la edición y ejecución de código o el despliegue de contenido.
    /// Al ser un objeto de tipo `InteractiveDocument`, se espera que contenga la estructura
    /// y los elementos necesarios para representar la lógica y el contenido de un notebook.
    /// Es posible que esta propiedad no siempre contenga un valor, en cuyo caso devolverá `null`.
    /// Esto podría suceder mientras el documento aún no se ha definido o cargado en el `NotebookItem`.
    /// /
    [JsonIgnore]
    public InteractiveDocument? Document { get; set; }
}

/// `NotebookSourceType` es una enumeración que representa los distintos tipos de fuentes desde las cuales se pueden obtener cuadernos de notas (notebooks).
/// Esta enumeración define los siguientes tipos de fuentes:
/// 1. **localDirectory**: Indica que los cuadernos de notas se encuentran en un directorio local. Este tipo de fuente se utiliza cuando los archivos de cuadernos están almacenados en el sistema de archivos del usuario, accesibles a través de una ruta de directorio específica.
/// 2. **webitems**: Indica que los cuadernos de notas se obtienen desde una ubicación web. Este tipo se utiliza cuando los cuadernos están disponibles para ser descargados o accesados desde una URL, generalmente almacenados en un servidor remoto o un repositorio online.
/// /
public enum NotebookSourceType
{
    /// Representa una fuente de tipo Notebook que se encuentra en un directorio local.
    /// Este miembro de enumeración es utilizado para especificar que los notebooks se obtendrán
    /// de una ubicación en el sistema de archivos del usuario, lo que permite trabajar con notebooks
    /// almacenados localmente en lugar de descargarlos desde una ubicación remota.
    /// Es útil para acceder a notebooks que el usuario ha guardado o editado en su propio entorno.
    LocalDirectory,

    /// El miembro `webitems` de la enumeración `NotebookSourceType` indica que los cuadernos (notebooks) se obtienen desde fuentes web.
    /// Este tipo de fuente está diseñado para manejar listas de cuadernos que se almacenan en ubicaciones accesibles a través de URLs, permitiendo su uso y visualización
    /// directa desde plataformas web. Es ideal para integraciones donde los documentos de cuadernos se encuentran alojados en repositorios o servidores remotos
    /// accesibles vía HTTP o HTTPS.
    /// /
    Webitems
}