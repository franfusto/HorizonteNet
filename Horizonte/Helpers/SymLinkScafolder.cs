using System.IO;
using System.Text;

namespace Horizonte;

public class SymLinkScafolder :ISymLinkScafolder
{
    private List<SymLinkDef> _symLinkDefs = new List<SymLinkDef>();
    
    public void CleanScafolder()
    {
        try
        {
            if (IsAnotherInstanceRunning())
            {
                Console.WriteLine("Otra instancia de la aplicación está en ejecución. Se mantiene el andamio de enlaces simbólicos.");
                return;
            }

            _symLinkDefs.Clear();
            string currentDirectory = Directory.GetCurrentDirectory();
            DeleteSymLinksRecursively(currentDirectory);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    // esta función la podemos cambiar de sitio y hacerla publica para horizonte
    // y poner una variable en configuración para permitir o no multiples instancias de la aplicacin   
    private bool IsAnotherInstanceRunning() 
    {
        string currentProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
        var processes = System.Diagnostics.Process.GetProcessesByName(currentProcessName);
        return processes.Length > 1;
    }

    private void DeleteSymLinksRecursively(string path)
    {
        DirectoryInfo directoryInfo = new DirectoryInfo(path);

        // Procesar archivos en el directorio actual
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
                    Console.WriteLine($"Error eliminando enlace simbólico (archivo) {file.FullName}: {e.Message}");
                }
            }
        }

        // Procesar subdirectorios
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
                    Console.WriteLine($"Error eliminando enlace simbólico (directorio) {subDir.FullName}: {e.Message}");
                }
            }
            else
            {
                // Si no es un enlace simbólico, entrar recursivamente
                DeleteSymLinksRecursively(subDir.FullName);

                // Después de procesar, si la carpeta está vacía, la eliminamos
                try
                {
                    subDir.Refresh(); // Actualizar el estado del objeto DirectoryInfo
                    if (subDir.Exists && !subDir.EnumerateFileSystemInfos().Any())
                    {
                        subDir.Delete();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error eliminando directorio vacío {subDir.FullName}: {e.Message}");
                }
            }
        }
    }

    public void BuildScafolder(IEnumerable<SymLinkDef> symlinklist)
    {
        try
        {
            if (IsAnotherInstanceRunning())
            {
                Console.WriteLine("Otra instancia de la aplicación está en ejecución. Se asume que el andamio de enlaces simbólicos ya está construido.");
                return;
            }

            string currentDirectory = Directory.GetCurrentDirectory();

            foreach (var symLinkDef in symlinklist)
            {
                try
                {
                    // Validar que el destino sea relativo y no salga del directorio actual
                    if (Path.IsPathRooted(symLinkDef.Destination))
                    {
                        Console.WriteLine($"Error: El destino debe ser una ruta relativa: {symLinkDef.Destination}");
                        continue;
                    }

                    string fullDestinationPath = Path.GetFullPath(Path.Combine(currentDirectory, symLinkDef.Destination));

                    if (!fullDestinationPath.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine($"Error: El destino está fuera del directorio actual: {symLinkDef.Destination}");
                        continue;
                    }

                    // Asegurarse de que el directorio padre del destino existe
                    string? parentDir = Path.GetDirectoryName(fullDestinationPath);
                    if (parentDir != null && !Directory.Exists(parentDir))
                    {
                        Directory.CreateDirectory(parentDir);
                    }

                    if (symLinkDef.IsDirecory)
                    {
                        if (Directory.Exists(fullDestinationPath))
                        {
                            Directory.Delete(fullDestinationPath);
                        }
                        Directory.CreateSymbolicLink(fullDestinationPath, symLinkDef.Source);
                    }
                    else
                    {
                        if (File.Exists(fullDestinationPath))
                        {
                            File.Delete(fullDestinationPath);
                        }
                        File.CreateSymbolicLink(fullDestinationPath, symLinkDef.Source);
                    }

                    _symLinkDefs.Add(symLinkDef);
                    Console.WriteLine($"Creado enlace simbólico: {symLinkDef.Destination} -> {symLinkDef.Source}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error creando enlace simbólico {symLinkDef.Destination}: {ex.Message}");
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public IEnumerable<SymLinkDef> GetScafolder() => _symLinkDefs;



}