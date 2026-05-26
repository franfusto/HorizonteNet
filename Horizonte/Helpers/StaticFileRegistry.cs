using System.Collections.Concurrent;
using System.Net.NetworkInformation;
using Horizonte.Settings;
using log4net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Horizonte;

/// <summary>
/// Clase encargada del registro y manejo de archivos estáticos dentro de un entorno de aplicación.
/// Permite registrar directorios específicos de paquetes y módulos para la gestión de archivos estáticos,
/// ofreciendo métodos de búsqueda y localización de archivos.
/// </summary>
public class StaticFileRegistry
{
    
    private static ConcurrentDictionary<string, string> _filesByDomain = new ConcurrentDictionary<string, string>();

    private static readonly ILog Log = LogManager.GetLogger(typeof(StaticFileRegistry));
    /// <summary>
    /// Almacena un diccionario concurrente que asocia rutas relativas en minúsculas
    /// con las rutas absolutas correspondientes de archivos registrados.
    /// Este diccionario funciona como una caché para la búsqueda rápida de archivos estáticos.
    /// </summary>
    private static ConcurrentDictionary<string, string> _files = new ConcurrentDictionary<string, string>();


    /// <summary>
    /// Ruta base utilizada para el registro de activos web estáticos dentro de la aplicación.
    /// </summary>
    /// <remarks>
    /// Esta constante define la ubicación raíz "/_staticwebassets/" empleada para organizar y estructurar
    /// los recursos estáticos en diferentes módulos o directorios. Sirve como prefijo estándar durante el
    /// registro y acceso a directorios específicos, tales como "Content" o "staticwebassets", dentro del sistema.
    /// </remarks>
    
    private const string StaticWebAssetsPath = "/_content/";

    /// <summary>
    /// Clase que gestiona el registro y recuperación de archivos estáticos dentro del entorno definido.
    /// Proporciona funciones para vincular directorios específicos de paquetes y módulos,
    /// así como para recuperar archivos estáticos basados en una ruta dada.
    /// </summary>
    public StaticFileRegistry()
    {
    }

    /// <summary>
    /// Proporciona acceso a un diccionario concurrente que mapea dominios con sus respectivas rutas
    /// de archivo estático almacenadas. Este diccionario permite una recuperación eficiente de rutas
    /// de archivos asociados a dominios específicos, facilitando la gestión y localización de archivos
    /// estáticos dentro de un entorno de aplicación.
    /// </summary>
    public ConcurrentDictionary<string, string> FilesByDomain
    {
        get => _filesByDomain;
    }

    /// <summary>
    /// Contiene un diccionario concurrente que mapea las rutas relativas de los archivos en minúsculas
    /// a sus rutas absolutas correspondientes dentro del registro.
    /// Sirve como un mecanismo de caché para agilizar la búsqueda de archivos estáticos.
    /// </summary>
    public ConcurrentDictionary<string, string> Files
    {
        get => _files;
    }

    /// <param name="packagepath">Ruta del paquete desde donde se intentarán identificar y registrar
        /// los directorios. Debe ser una ruta válida que apunte a un archivo o directorio existente.
        /// Si la ruta es inválida, nula o vacía, no se realizará ninguna acción.</param>
        /// <param name="domainName">Dominio lógico/ALC donde se ha cargado el paquete.</param>
        public void RegisterPackageDirectory(string packagepath, string? domainName = null)
        {
            // Validar que el path no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(packagepath) || (!File.Exists(packagepath) && !Directory.Exists(packagepath)))
                return;

            // Subir dos directorios desde el archivo packagepath
            var currentDirectory = GetParentDirectory(Path.GetDirectoryName(packagepath) ?? string.Empty, 2);

            if (currentDirectory == null) return;

            //obtener nombre del paquete
            string prefix = StaticWebAssetsPath + Path.GetFileName(GetParentDirectory(currentDirectory, 1));

            // Listar todos los subdirectorios en el directorio actual
            var directories = Directory.GetDirectories(currentDirectory);

            // Buscar directorio Content (insensible a mayúsculas/minúsculas)
            var contentDir = directories.FirstOrDefault(d =>
                string.Equals(Path.GetFileName(d), "Content", StringComparison.OrdinalIgnoreCase));

            if (contentDir != null && Directory.Exists(contentDir))
            {
                Log.Info($"Directory Content found: {contentDir}");
                RegisterDirectory(contentDir, prefix, domainName);
            }

            // Buscar directorio staticwebassets (insensible a mayúsculas/minúsculas)
            var staticWebAssetsDir = directories.FirstOrDefault(d =>
                string.Equals(Path.GetFileName(d), "staticwebassets", StringComparison.OrdinalIgnoreCase));

            if (staticWebAssetsDir != null && Directory.Exists(staticWebAssetsDir))
            {
                Log.Info ($"Found staticwebassets folder: {staticWebAssetsDir}");
                RegisterDirectory(staticWebAssetsDir, prefix, domainName);
            }
        }

    /// <param name="moduleSettingsItem">Una instancia de `ModulesSettingsItem` que contiene información sobre el módulo,
    /// incluyendo su nombre, versión, estado de actividad y la ruta al archivo DLL correspondiente.</param>
    /// <param name="domainName">Dominio lógico/ALC donde se ha cargado el módulo.</param>
    public void RegisterModuleDirectory(ModulesSettingsItem moduleSettingsItem, string? domainName = null)
    {
        string? dllPath = moduleSettingsItem.Path;

        if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
        {
            Log.Error($"The provided DLL file path is not valid: {dllPath}");
            return;
        }

        string prefix = StaticWebAssetsPath + moduleSettingsItem.ModuleName;

        // Obtener el directorio donde se encuentra la DLL
        var currentDirectory = Path.GetDirectoryName(dllPath)!;
        string? wwwRootPath = null;
        for (int i = 0; i <= 3; i++)
        {
            var potentialPath = Path.Combine(currentDirectory, "wwwroot");
            if (Directory.Exists(potentialPath))
            {
                wwwRootPath = potentialPath;
                Log.Info
                    ($"Found wwwroot folder: {wwwRootPath}");
                RegisterDirectory(wwwRootPath, prefix, domainName);
                break;
            }

            // Subir un nivel
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            if (currentDirectory == null) break;
        }
    }

    /// <summary>
    /// Busca y retorna un archivo estático registrado en el sistema, basado en la ruta proporcionada.
    /// Permite especificar un nombre de dominio para realizar la búsqueda en el contexto de un dominio específico.
    /// </summary>
    /// <param name="path">La ruta del archivo a buscar dentro del registro de archivos estáticos.</param>
    /// <param name="domainName">Opcional. El nombre del dominio en el cual buscar el archivo especificado.</param>
    /// <returns>
    /// Un objeto <c>FileInfo</c> que representa el archivo encontrado,
    /// o <c>null</c> si no se encuentra un archivo que coincida con la ruta y el dominio especificados.
    /// </returns>
    public FileInfo? GetFile(string path, string? domainName)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var normalizedPath = NormalizeRegistryPath(path);

        if (!string.IsNullOrWhiteSpace(domainName))
        {
            var domainKey = BuildDomainKey(domainName, normalizedPath);
            if (_filesByDomain.TryGetValue(domainKey, out var domainFile))
            {
                return new FileInfo(domainFile);
            }
        }

        if (_files.TryGetValue(normalizedPath.ToLowerInvariant(), out var file))
        {
            return new FileInfo(file);
        }

        return null;
    }

    
    private string? GetParentDirectory(string path, int levelsUp)
    {
        var currentDirectory = path;
        for (int i = 0; i < levelsUp; i++)
        {
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            if (currentDirectory == null) return null;
        }

        return currentDirectory;
    }



    private void RegisterDirectory(string path, string prefix = "", string? domainName = null)
    {
        if (!Directory.Exists(path))
        {
            Log.Error($"El directorio especificado no existe: {path}");
            return;
        }

        // Obtener todos los archivos en el directorio y sus subdirectorios
        string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            string relativePath =
                prefix + "/" + Path.GetRelativePath(path, file).Replace(Path.DirectorySeparatorChar, '/');

            var normalizedPath = NormalizeRegistryPath(relativePath);

            // Cache legacy/global. Se mantiene por compatibilidad.
            _files[normalizedPath.ToLowerInvariant()] = file;

            if (!string.IsNullOrWhiteSpace(domainName))
            {
                _filesByDomain[BuildDomainKey(domainName, normalizedPath)] = file;
            }
        }
    }

    private static string BuildDomainKey(string domainName, string path)
    {
        return $"{domainName.Trim().ToLowerInvariant()}::{NormalizeRegistryPath(path).ToLowerInvariant()}";
    }

    private static string NormalizeRegistryPath(string path)
    {
        var normalized = path.Replace('\\', '/');

        if (!normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = "/" + normalized;
        }

        return normalized;
    }
}
