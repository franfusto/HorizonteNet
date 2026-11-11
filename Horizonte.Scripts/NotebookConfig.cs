using System.Text.Json.Serialization;
using Microsoft.DotNet.Interactive.Documents.Jupyter;
using Microsoft.DotNet.Interactive.Documents;

namespace Horizonte.Scripts;

/// <summary>
/// Gestiona la configuración global de fuentes y cuadernos interactivos (notebooks).
/// </summary>
/// <remarks>
/// Almacena una lista de fuentes de cuadernos (<see cref="NotebookSource"/>) que determinan de dónde se obtienen
/// los notebooks para su visualización y edición en la interfaz de usuario.
/// </remarks>
public class NotebookConfig
{
    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="NotebookConfig"/> con fuentes y cuadernos de ejemplo predeterminados.
    /// </summary>
    public NotebookConfig()
    {
        AddContentSample();
    }

    /// <summary>
    /// Obtiene o establece la lista de fuentes de cuadernos (<see cref="NotebookSource"/>) configuradas.
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
            ItemPath = "https://raw.githubusercontent.com/franfusto/HorizonteNet/refs/heads/master/githubfiles/HorizonteNet.ipynb"
        });
       
    }
}

/// <summary>
/// Representa una fuente de cuadernos (notebooks) utilizada para organizar y gestionar colecciones de cuadernos.
/// </summary>
/// <remarks>
/// Una fuente de cuadernos puede provenir de directorios locales o recursos remotos vía web, e incluye la lista de sus elementos (<see cref="NotebookItem"/>).
/// </remarks>
public class NotebookSource
{
    /// <summary>
    /// Obtiene o establece el nombre descriptivo de la fuente de cuadernos.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Obtiene o establece la ruta del directorio local donde se encuentran almacenados los cuadernos.
    /// </summary>
    public string? SourcePath { get; set; }

    /// <summary>
    /// Obtiene o establece el tipo de origen de los cuadernos (<see cref="NotebookSourceType"/>).
    /// </summary>
    public NotebookSourceType Type { get; set; }

    /// <summary>
    /// Obtiene o establece la lista de elementos o cuadernos individuales (<see cref="NotebookItem"/>) pertenecientes a esta fuente.
    /// </summary>
    public List<NotebookItem> Items { get; set; } = new List<NotebookItem>();
}

/// <summary>
/// Representa un elemento o cuaderno individual dentro de una colección de notebooks.
/// </summary>
public class NotebookItem
{
    /// <summary>
    /// Obtiene o establece el identificador único del elemento de cuaderno.
    /// </summary>
    public required string ItemId { get; set; }

    /// <summary>
    /// Obtiene o establece la ruta de archivo local o URL remota de donde se descarga o carga el cuaderno.
    /// </summary>
    public string? ItemPath { get; set; }

    /// <summary>
    /// Obtiene o establece el documento interactivo (<see cref="InteractiveDocument"/>) cargado en memoria asociado al cuaderno.
    /// </summary>
    [JsonIgnore]
    public InteractiveDocument? Document { get; set; }
}

/// <summary>
/// Define los tipos de fuentes desde las cuales se pueden obtener cuadernos de notas.
/// </summary>
public enum NotebookSourceType
{
    /// <summary>
    /// Indica que los cuadernos se obtienen desde un directorio en el sistema de archivos local.
    /// </summary>
    LocalDirectory,

    /// <summary>
    /// Indica que los cuadernos se descargan u obtienen desde una ubicación remota en la web mediante HTTP/HTTPS.
    /// </summary>
    Webitems
}