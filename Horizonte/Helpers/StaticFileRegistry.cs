using System.Collections.Concurrent;
using System.Net.NetworkInformation;
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
    public StaticFileRegistry(IHorizonteEnv env)
    {
    }

    /// Registra los directorios de contenido estático y de recursos web estáticos dentro
    /// del directorio que corresponde al paquete especificado.
    /// Este método busca subdirectorios específicos en el directorio base del paquete, tales como
    /// "Content" y "staticwebassets", los cuales son sensibles para entornos de aplicaciones.
    /// Si estos subdirectorios existen, se registran para su utilización como recursos accesibles a través
    /// de prefijos específicos, permitiendo una gestión adecuada de archivos estáticos asociados al paquete.
    /// <param name="packagepath">Ruta del paquete desde donde se intentarán identificar y registrar
    /// los directorios. Debe ser una ruta válida que apunte a un archivo o directorio existente.
    /// Si la ruta es inválida, nula o vacía, no se realizará ninguna acción.</param>
    public void RegisterPackageDirectory(string packagepath)
    {
        // Validar que el path no sea nulo o vacío
        if (string.IsNullOrWhiteSpace(packagepath) || (!File.Exists(packagepath) && !Directory.Exists(packagepath)))
            return;

        // Subir dos directorios desde el archivo packagepath
        var currentDirectory = GetParentDirectory(Path.GetDirectoryName(packagepath), 2);

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
            Console.WriteLine($"Directory Content not found: {contentDir}");
            RegisterDirectory(contentDir, prefix);
        }

        // Buscar directorio staticwebassets (insensible a mayúsculas/minúsculas)
        var staticWebAssetsDir = directories.FirstOrDefault(d =>
            string.Equals(Path.GetFileName(d), "staticwebassets", StringComparison.OrdinalIgnoreCase));

        if (staticWebAssetsDir != null && Directory.Exists(staticWebAssetsDir))
        {
            Console.WriteLine ($"Found staticwebassets folder: {staticWebAssetsDir}");
            RegisterDirectory(staticWebAssetsDir, prefix);
        }
    }

    /// Registra el directorio de un módulo en el sistema para que se puedan servir archivos estáticos desde este.
    /// Este método localiza de manera recursiva la carpeta "wwwroot" asociada al módulo cuyo archivo DLL se encuentra
    /// en la ruta especificada en `moduleSettingsItem.Path`. Si encuentra dicha carpeta, registra su contenido
    /// en el sistema con un prefijo que incluye el nombre del módulo.
    /// Si la ruta del DLL no es válida o no existe, se registra un mensaje de error en el sistema de log.
    /// <param name="moduleSettingsItem">Una instancia de `ModulesSettingsItem` que contiene información sobre el módulo,
    /// incluyendo su nombre, versión, estado de actividad y la ruta al archivo DLL correspondiente.</param>
    public void RegisterModuleDirectory(ModulesSettingsItem moduleSettingsItem)
    {
        string? dllPath = moduleSettingsItem.Path;

        if (string.IsNullOrWhiteSpace(dllPath) || !File.Exists(dllPath))
        {
            Console.WriteLine($"The provided DLL file path is not valid: {dllPath}");
            return;
        }

        string prefix = StaticWebAssetsPath + moduleSettingsItem.ModuleName;

        // Obtener el directorio donde se encuentra la DLL
        var currentDirectory = Path.GetDirectoryName(dllPath);
        string? wwwRootPath = null;
        for (int i = 0; i <= 3; i++)
        {
            var potentialPath = Path.Combine(currentDirectory, "wwwroot");
            if (Directory.Exists(potentialPath))
            {
                wwwRootPath = potentialPath;
                Console.WriteLine
                    ($"Found wwwroot folder: {wwwRootPath}");
                RegisterDirectory(wwwRootPath, prefix);
                break;
            }

            // Subir un nivel
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            if (currentDirectory == null) break;
        }
    }

    /// Obtiene un archivo basado en la ruta proporcionada.
    /// Este método intenta recuperar la información del archivo desde un registro interno
    /// utilizando la ruta especificada. Si el archivo es encontrado, devuelve un objeto FileInfo
    /// que contiene los detalles del archivo, de lo contrario retorna null.
    /// <param name="path">
    /// Ruta del archivo que se desea obtener. La ruta es comparada de manera insensible a mayúsculas/minúsculas.
    /// </param>
    /// <returns>
    /// Un objeto FileInfo que representa el archivo solicitado si es encontrado;
    /// de lo contrario, null.
    /// </returns>
    public FileInfo? GetFile(string path)
    {
        var file = string.Empty;
        _files.TryGetValue(path.ToLowerInvariant(), out file);
        if (file != null) return new FileInfo(file);
        return null;
    }

    /// Obtiene el directorio ascendente de un directorio dado, subiendo un número especificado de niveles.
    /// <param name="path">Ruta del directorio base desde el cual se buscará el directorio padre.</param>
    /// <param name="levelsUp">Cantidad de niveles que se subirán en el árbol de directorios desde el directorio base.</param>
    /// <returns>La ruta del directorio resultante después de subir los niveles especificados, o <c>null</c> si no se puede subir más niveles.</returns>
    private string? GetParentDirectory(string? path, int levelsUp)
    {
        var currentDirectory = path;
        for (int i = 0; i < levelsUp; i++)
        {
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            if (currentDirectory == null) return null;
        }

        return currentDirectory;
    }


    /// Registra todos los archivos de un directorio y sus subdirectorios en un diccionario interno,
    /// almacenando las rutas relativas con un prefijo especificado.
    /// Este método permite recorrer recursivamente todos los archivos en un directorio dado y
    /// asociarlos a una clave generada mediante una ruta relativa con un prefijo personalizado.
    /// Si el directorio especificado no existe, se registrará un error en los registros.
    /// <param name="path">
    /// Ruta al directorio que se desea registrar. Si el directorio no existe, no se realizará ninguna operación.
    /// </param>
    /// <param name="prefix">
    /// Prefijo que se añadirá al comienzo de las rutas relativas de los archivos encontrados.
    /// El valor predeterminado es una cadena vacía.
    /// </param>
    private void RegisterDirectory(string path, string prefix = "")
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"El directorio especificado no existe: {path}");
            return;
        }

        // Obtener todos los archivos en el directorio y sus subdirectorios
        string[] files = Directory.GetFiles(path, "*", SearchOption.AllDirectories);
        foreach (var file in files)
        {
            string relativePath =
                prefix + "/" + Path.GetRelativePath(path, file).Replace(Path.DirectorySeparatorChar, '/');

            //añadir a la cache
            _files[relativePath.ToLowerInvariant()] = file;
        }
    }
}