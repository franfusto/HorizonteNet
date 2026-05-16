using System.Text.Json;
using log4net;

namespace Horizonte.Services;

/// <summary>
/// Implementación de <see cref="IHContext"/> basada en archivos JSON.
/// </summary>
/// <remarks>
/// <para>
/// Esta clase resuelve secciones tipadas usando <c>typeof(T).Name</c> como nombre de sección y permite
/// leer datos tanto desde el contexto principal como desde sobrescrituras locales del usuario.
/// </para>
/// <para>
/// La prioridad de resolución es la siguiente:
/// </para>
/// <list type="number">
/// <item>
/// <description>Archivo de sobrescritura local: <c>~/.{contextName}/localoverrides/{Tipo}.json</c>.</description>
/// </item>
/// <item>
/// <description>Archivo de contexto del usuario: <c>~/.{contextName}/{Tipo}.json</c>.</description>
/// </item>
/// <item>
/// <description>Archivo de contexto base: <c>{contextName}.json</c>.</description>
/// </item>
/// </list>
/// <para>
/// Las operaciones de lectura y escritura están sincronizadas mediante un bloqueo estático para evitar
/// accesos concurrentes al sistema de archivos dentro del proceso.
/// </para>
/// </remarks>
public class HContext : IHContext
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(HContext));
    private JsonSerializerOptions _serializerOptions = new();
    private static readonly object _fileLock = new();
    private string? _userDirectory = null;
    
    /// <summary>
    /// Obtiene la ruta raíz asociada al contexto actual.
    /// </summary>
    /// <remarks>
    /// Esta propiedad identifica la ubicación base utilizada por la implementación para resolver
    /// archivos, recursos o rutas relacionadas con el contexto.
    /// </remarks>
    public string RootPath { get; }
    
    /// <summary>
    /// Obtiene el nombre del contexto actual.
    /// </summary>
    /// <remarks>
    /// Este valor representa el identificador lógico del contexto predeterminado que utilizará
    /// la implementación cuando no se especifique uno explícitamente en las operaciones.
    /// </remarks>
    public string ContextName { get; }

    /// <summary>
    /// Inicializa una nueva instancia de la clase <see cref="HContext"/>.
    /// </summary>
    /// <param name="contextName">Nombre lógico del contexto que se utilizará como valor predeterminado.</param>
    /// <param name="rootPath">
    /// Ruta raíz asociada al contexto.
    /// </param>
    /// <remarks>
    /// Actualmente, el parámetro <paramref name="rootPath"/> no se utiliza en la implementación.
    /// El directorio base efectivo se obtiene del perfil del usuario actual.
    /// </remarks>
    public HContext(string contextName , string rootPath )
    {
        ContextName = contextName;
        RootPath = rootPath;
        _serializerOptions =  new JsonSerializerOptions();
        _userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(_userDirectory))
        {
            _userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }

    /// <summary>
    /// Obtiene la ruta del archivo JSON principal asociado al tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo cuya sección se desea resolver.</typeparam>
    /// <param name="contextName">Nombre opcional del contexto.</param>
    /// <returns>
    /// La ruta del archivo específico del tipo dentro del directorio del usuario si existe;
    /// en caso contrario, el archivo base <c>{contextName}.json</c>.
    /// </returns>
    /// <remarks>
    /// Si existe el directorio <c>~/.{contextName}</c> y dentro de él existe el archivo
    /// <c>{typeof(T).Name}.json</c>, se devuelve esa ruta. Si no existe, se utiliza como
    /// fallback el archivo de contexto base.
    /// </remarks>
    private string GetJsonFilePath<T>(string? contextName)
    {
        string contextFolder = Path.Combine(_userDirectory!, "." +ContextName);

        if (Directory.Exists(contextFolder))
        {
            string jsonFileName = typeof(T).Name + ".json";
            string jsonFilePath = Path.Combine(contextFolder, jsonFileName);

            if (File.Exists(jsonFilePath))
            {
                return jsonFilePath;
            }
        }

        return ContextName + ".json";
    }

    /// <summary>
    /// Obtiene el directorio donde se almacenan las sobrescrituras locales del contexto.
    /// </summary>
    /// <param name="contextName">Nombre opcional del contexto.</param>
    /// <returns>Ruta del directorio de sobrescrituras locales.</returns>
    /// <remarks>
    /// Si el directorio no existe, se crea automáticamente.
    /// </remarks>
    private string GetLocalOverridesDir(string? contextName = null)
    {
        string localOverridesDir = Path.Combine(_userDirectory!, "." +ContextName, "localoverrides");
        if (!Directory.Exists(localOverridesDir))
        {
            Directory.CreateDirectory(localOverridesDir);
        }

        return localOverridesDir;
    }

    /// <summary>
    /// Obtiene la ruta del archivo de sobrescritura local para el tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo cuya sobrescritura local se desea resolver.</typeparam>
    /// <param name="contextName">Nombre opcional del contexto.</param>
    /// <returns>Ruta completa del archivo JSON de sobrescritura local.</returns>
    private string GetLocalOverrideFilePath<T>(string? contextName = null)
    {
        return Path.Combine(GetLocalOverridesDir(contextName), typeof(T).Name + ".json");
    }

    /// <summary>
    /// Determina el origen efectivo de la sección para el tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo cuya procedencia se desea consultar.</typeparam>
    /// <param name="contextname">Nombre opcional del contexto.</param>
    /// <returns>
    /// <see cref="SectionSource.LocalOverride"/> si existe una sobrescritura local para el tipo;
    /// en caso contrario, <see cref="SectionSource.Context"/>.
    /// </returns>
    public SectionSource Source<T>(string? contextname = null)
    {
        lock (_fileLock)
        {
            var localOverridePath = GetLocalOverrideFilePath<T>(contextname);
            return File.Exists(localOverridePath) ? SectionSource.LocalOverride : SectionSource.Context;
        }
    }

    /// <summary>
    /// Recupera y deserializa la sección asociada al tipo especificado.
    /// </summary>
    /// <typeparam name="T">Tipo de la sección a recuperar.</typeparam>
    /// <param name="contextname">Nombre opcional del contexto.</param>
    /// <returns>
    /// La instancia de <typeparamref name="T"/> obtenida desde el origen efectivo, o <see langword="default"/>
    /// si la sección no existe o si ocurre un error durante la operación.
    /// </returns>
    /// <remarks>
    /// Si existe una sobrescritura local, se utiliza en prioridad. En caso contrario, se consulta
    /// el contexto principal.
    /// </remarks>
    public T? Get<T>(string? contextname = null)
    {
        try
        {
            lock (_fileLock)
            {
                var filePath = Source<T>(contextname) == SectionSource.LocalOverride
                    ? GetLocalOverrideFilePath<T>(contextname)
                    : GetJsonFilePath<T>(ContextName);

                return JsonFileHelper.TryGet(filePath, typeof(T).Name, out T? value, _serializerOptions)
                    ? value
                    : default(T);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error al obtener datos del archivo de contexto: {e.Message}");
            return default(T);
        }
    }

    /// <summary>
    /// Actualiza la sección asociada al tipo especificado aplicando una acción de modificación.
    /// </summary>
    /// <typeparam name="T">Tipo de la sección a modificar.</typeparam>
    /// <param name="update">Acción que modifica la sección existente.</param>
    /// <param name="contextname">Nombre opcional del contexto.</param>
    /// <remarks>
    /// La escritura se realiza sobre el origen efectivo de la sección: si existe una sobrescritura local,
    /// se actualiza ese archivo; en caso contrario, se actualiza el archivo del contexto principal.
    /// </remarks>
    public void Update<T>(Action<T> update, string? contextname = null)
    {
        try
        {
            lock (_fileLock)
            {
                var filePath = Source<T>(contextname) == SectionSource.LocalOverride
                    ? GetLocalOverrideFilePath<T>(contextname)
                    : GetJsonFilePath<T>(ContextName);

                JsonFileHelper.AddOrUpdateSection(
                    jsonFilePath: filePath,
                    sectionName: typeof(T).Name,
                    updateAction: update,
                    serializerOptions: _serializerOptions);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error al actualizar los datos del archivo de contexto: {e.Message}");
        }
    }

    /// <summary>
    /// Reemplaza o crea la sección asociada al tipo especificado con el valor proporcionado.
    /// </summary>
    /// <typeparam name="T">Tipo de la sección a guardar.</typeparam>
    /// <param name="newvalue">Nuevo valor de la sección.</param>
    /// <param name="contextname">Nombre opcional del contexto.</param>
    /// <remarks>
    /// La escritura se realiza sobre el origen efectivo de la sección: si existe una sobrescritura local,
    /// se actualiza ese archivo; en caso contrario, se actualiza el archivo del contexto principal.
    /// </remarks>
    public void Update<T>(T newvalue, string? contextname = null)
    {
        try
        {
            lock (_fileLock)
            {
                var filePath = Source<T>(contextname) == SectionSource.LocalOverride
                    ? GetLocalOverrideFilePath<T>(contextname)
                    : GetJsonFilePath<T>(ContextName);

                JsonFileHelper.AddOrUpdateSection(
                    jsonFilePath: filePath,
                    sectionName: typeof(T).Name,
                    value: newvalue,
                    serializerOptions: _serializerOptions);
            }
        }
        catch (Exception e)
        {
            Log.Error($"Error al actualizar los datos del archivo de contexto: {e.Message}");
        }
    }
}