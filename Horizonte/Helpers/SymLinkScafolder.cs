
using Horizonte.Interfaces;
using Microsoft.Extensions.Logging;

namespace Horizonte.Helpers;

/// <summary>
/// Implementa la creación, consulta y limpieza de un andamio de enlaces simbólicos
/// dentro del directorio de trabajo actual.
/// Horizonte soporta aislamiento de ensamblados administrados por dominio ALC y aislamiento de assets web mediante StaticFileRegistry.
/// Los recursos nativos, bitcode, archivos resueltos por rutas relativas, librerías no administradas y artefactos procesados desde .targets se materializan a nivel de proceso/directorio de aplicación. Por tanto, no se garantiza la coexistencia de dos versiones de un paquete que requieran archivos nativos o recursos con el mismo nombre/ruta efectiva.
/// Si varios dominios requieren el mismo recurso nativo, deben usar versiones compatibles o rutas/nombres diferenciados.
/// </summary>
/// <remarks>
/// Esta clase mantiene una colección en memoria con las definiciones de enlaces creados
/// durante la ejecución y utiliza un archivo de bloqueo para evitar que varias instancias
/// modifiquen simultáneamente el mismo andamio.
/// </remarks>
public class SymLinkScafolder :ISymLinkScafolder
{
    private readonly ILogger<PanelModulo> _logger;
    private const string LockFileName = ".symlink.lock";
    private readonly int _currentPid = Environment.ProcessId;
    private string? _ownedLockFilePath;
    private List<SymLinkDef> _symLinkDefs = new List<SymLinkDef>();

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="SymLinkScafolder"/>.
    /// </summary>
    /// <param name="logger">
    /// Registrador utilizado para informar de operaciones, incidencias y errores
    /// relacionados con la gestión de enlaces simbólicos.
    /// </param>
    public SymLinkScafolder(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }
    
    /// <summary>
    /// Devuelve la colección de definiciones de enlaces simbólicos registradas
    /// por esta instancia del andamio.
    /// </summary>
    /// <returns>
    /// Secuencia con las definiciones de enlaces simbólicos creadas o cargadas en memoria.
    /// </returns>
    public IEnumerable<SymLinkDef> GetScafolder() => _symLinkDefs;
    
    /// <summary>
    /// Limpia el andamio actual eliminando los enlaces simbólicos encontrados de forma recursiva
    /// desde el directorio de trabajo.
    /// </summary>
    /// <remarks>
    /// Antes de realizar la limpieza se intenta adquirir un archivo de bloqueo exclusivo.
    /// Si otra instancia posee el bloqueo, la operación se omite.
    /// </remarks>
    public void CleanScafolder()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        try
        {
            if (!TryAcquireScafolderLock(currentDirectory))
            {
                _logger.LogInformation("Otra instancia activa es propietaria del andamio de enlaces simbólicos de este directorio. Se omite la limpieza.");
                return;
            }

            _symLinkDefs.Clear();
            DeleteSymLinksRecursively(currentDirectory);
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
        finally
        {
            ReleaseScafolderLock();
        }
    }

    private static string GetLockFilePath(string currentDirectory)
    {
        return Path.Combine(currentDirectory, LockFileName);
    }

    private bool TryAcquireScafolderLock(string currentDirectory)
    {
        string lockFilePath = GetLockFilePath(currentDirectory);

        while (true)
        {
            try
            {
                using FileStream stream = new FileStream(lockFilePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
                using StreamWriter writer = new StreamWriter(stream);
                writer.Write(_currentPid);
                writer.Flush();

                _ownedLockFilePath = lockFilePath;
                return true;
            }
            catch (IOException)
            {
                if (!File.Exists(lockFilePath))
                {
                    continue;
                }

                if (!TryReadLockPid(lockFilePath, out int lockPid))
                {
                    _logger.LogWarning($"El archivo lock {lockFilePath} es inválido. Se intentará recuperar.");
                    TryDeleteStaleLock(lockFilePath);
                    continue;
                }

                if (lockPid == _currentPid)
                {
                    _ownedLockFilePath = lockFilePath;
                    return true;
                }

                if (!ProcessExists(lockPid))
                {
                    _logger.LogWarning($"Se encontró un lock huérfano en {lockFilePath} con PID {lockPid}. Se intentará recuperar.");
                    TryDeleteStaleLock(lockFilePath);
                    continue;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error adquiriendo el lock del andamio en {lockFilePath}: {ex.Message}");
                return false;
            }
        }
    }

    private bool TryReadLockPid(string lockFilePath, out int pid)
    {
        pid = 0;

        try
        {
            string content = File.ReadAllText(lockFilePath).Trim();
            return int.TryParse(content, out pid);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error leyendo el archivo lock {lockFilePath}: {ex.Message}");
            return false;
        }
    }

    private static bool ProcessExists(int pid)
    {
        try
        {
            var process = System.Diagnostics.Process.GetProcessById(pid);
            return !process.HasExited;
        }
        catch
        {
            return false;
        }
    }

    private void TryDeleteStaleLock(string lockFilePath)
    {
        try
        {
            File.Delete(lockFilePath);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error eliminando lock huérfano {lockFilePath}: {ex.Message}");
        }
    }

    private void ReleaseScafolderLock()
    {
        if (string.IsNullOrEmpty(_ownedLockFilePath))
        {
            return;
        }

        try
        {
            if (!File.Exists(_ownedLockFilePath))
            {
                _ownedLockFilePath = null;
                return;
            }

            if (!TryReadLockPid(_ownedLockFilePath, out int lockPid))
            {
                _logger.LogWarning($"No se pudo validar el PID del lock {_ownedLockFilePath}. No se liberará automáticamente.");
                return;
            }

            if (lockPid != _currentPid)
            {
                _logger.LogWarning($"El lock {_ownedLockFilePath} pertenece al PID {lockPid} y no al actual {_currentPid}. No se liberará.");
                return;
            }

            File.Delete(_ownedLockFilePath);
            _ownedLockFilePath = null;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error liberando el lock del andamio {_ownedLockFilePath}: {ex.Message}");
        }
    }

    private void DeleteSymLinksRecursively(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        foreach (FileInfo file in directoryInfo.GetFiles())
        {
            if (file.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                try
                {
                    file.Delete();
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error eliminando enlace simbólico (archivo) {file.FullName}: {e.Message}");
                }
            }
        }

        foreach (DirectoryInfo subDir in directoryInfo.GetDirectories())
        {
            if (subDir.Attributes.HasFlag(FileAttributes.ReparsePoint))
            {
                try
                {
                    subDir.Delete();
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error eliminando enlace simbólico (directorio) {subDir.FullName}: {e.Message}");
                }
            }
            else
            {
                DeleteSymLinksRecursively(subDir.FullName);

                try
                {
                    subDir.Refresh();
                    if (subDir.Exists && !subDir.EnumerateFileSystemInfos().Any())
                    {
                        subDir.Delete();
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError($"Error eliminando directorio vacío {subDir.FullName}: {e.Message}");
                }
            }
        }
    }
    /// <summary>
    /// Construye el andamio de enlaces simbólicos a partir de una colección de definiciones.
    /// </summary>
    /// <param name="symlinklist">
    /// Colección de definiciones que describen el origen, el destino y el tipo
    /// de cada enlace simbólico a crear.
    /// </param>
    /// <remarks>
    /// Los destinos deben ser rutas relativas al directorio de trabajo actual.
    /// Si una definición apunta fuera de dicho directorio o produce un error durante
    /// la creación, se registra la incidencia y se continúa con el resto.
    /// </remarks>
    public void BuildScafolder(IEnumerable<SymLinkDef> symlinklist)
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        try
        {
            if (!TryAcquireScafolderLock(currentDirectory))
            {
                _logger.LogInformation("Otra instancia activa es propietaria del andamio de enlaces simbólicos de este directorio. Se omite la construcción.");
                return;
            }

            foreach (var symLinkDef in symlinklist)
            {
                try
                {
                    if (Path.IsPathRooted(symLinkDef.Destination))
                    {
                        _logger.LogError($"Error: El destino debe ser una ruta relativa: {symLinkDef.Destination}");
                        continue;
                    }

                    string fullDestinationPath = Path.GetFullPath(Path.Combine(currentDirectory, symLinkDef.Destination));

                    if (!fullDestinationPath.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        _logger.LogError($"Error: El destino está fuera del directorio actual: {symLinkDef.Destination}");
                        continue;
                    }

                    string? parentDir = Path.GetDirectoryName(fullDestinationPath);
                    if (parentDir != null && !Directory.Exists(parentDir))
                    {
                        Directory.CreateDirectory(parentDir);
                    }

                    if (symLinkDef.IsDirecory)
                    {
                        if (Directory.Exists(fullDestinationPath))
                        {
                            if (IsSameLinkTarget(fullDestinationPath, symLinkDef.Source))
                            {
                                _logger.LogInformation($"El enlace simbólico ya existe: {symLinkDef.Destination} -> {symLinkDef.Source}");
                                continue;
                            }

                            _logger.LogWarning(
                                $"Conflicto de enlace simbólico para {symLinkDef.Destination}. " +
                                $"Ya existe un destino en {fullDestinationPath} y no se sobrescribirá con {symLinkDef.Source}.");
                            continue;
                        }

                        Directory.CreateSymbolicLink(fullDestinationPath, symLinkDef.Source);
                    }
                    else
                    {
                        if (File.Exists(fullDestinationPath))
                        {
                            if (IsSameLinkTarget(fullDestinationPath, symLinkDef.Source))
                            {
                                _logger.LogInformation($"El enlace simbólico ya existe: {symLinkDef.Destination} -> {symLinkDef.Source}");
                                continue;
                            }

                            _logger.LogWarning(
                                $"Conflicto de enlace simbólico para {symLinkDef.Destination}. " +
                                $"Ya existe un destino en {fullDestinationPath} y no se sobrescribirá con {symLinkDef.Source}.");
                            continue;
                        }

                        File.CreateSymbolicLink(fullDestinationPath, symLinkDef.Source);
                    }

                    _symLinkDefs.Add(symLinkDef);
                    _logger.LogInformation($"Creado enlace simbólico: {symLinkDef.Destination} -> {symLinkDef.Source}");
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error creando enlace simbólico {symLinkDef.Destination}: {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e.ToString());
        }
    }
    private static bool IsSameLinkTarget(string linkPath, string expectedTarget)
    {
        try
        {
            var fileInfo = new FileInfo(linkPath);
            if (fileInfo.Exists && fileInfo.LinkTarget != null)
            {
                return PathsEqual(fileInfo.LinkTarget, expectedTarget);
            }

            var directoryInfo = new DirectoryInfo(linkPath);
            if (directoryInfo.Exists && directoryInfo.LinkTarget != null)
            {
                return PathsEqual(directoryInfo.LinkTarget, expectedTarget);
            }
        }
        catch
        {
            return false;
        }

        return false;
    }

    private static bool PathsEqual(string left, string right)
    {
        try
        {
            return string.Equals(
                Path.GetFullPath(left),
                Path.GetFullPath(right),
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal);
        }
        catch
        {
            return string.Equals(
                left,
                right,
                OperatingSystem.IsWindows()
                    ? StringComparison.OrdinalIgnoreCase
                    : StringComparison.Ordinal);
        }
    }
    
}