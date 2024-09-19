using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Horizonte;

public class HAssemblyManager : IhAssemblyManager
{
    public Assembly[]? Assemblies;
    private readonly ModulesSettings _modset;
    private readonly HorizonteEnv _env;


    public HAssemblyManager(ModulesSettings modulesSettings, HorizonteEnv env)
    {
        _modset = modulesSettings ?? new ModulesSettings();
        _env = env;
        Assemblies = LoadAssembliesFromEnviromente();
    }

    private void CheckAssetsFolder(ModulesSettingsItem item)
    {
        string assetsdir;
        if (item.Path == null)
        {
            assetsdir = Path.Combine(_env.RootPath,"mod", item.ModuleName, item.ModuleVersion, "wwwroot");
        }
        else
        {
            var dir = new FileInfo(item.Path).Directory!;
            assetsdir = Path.Combine(dir.FullName, "wwwroot");
        }

        if (Directory.Exists(assetsdir)) _env.AssetsFolders.Add(assetsdir);
    }

    private new Assembly[] LoadAssembliesFromEnviromente()
    {
        //comprobar directorio
        if (!Directory.Exists(_modset.ModulesPath))
            Directory.CreateDirectory(_modset.ModulesPath);

        //cargamos ensamblados
        var currentDomain = AppDomain.CurrentDomain;
        foreach (var moditem in _modset.List.Where(moditem => moditem.Active))
        {
            try
            {
                string modulePath;
                if (moditem.Path != null)
                {
                    modulePath = moditem.Path;
                }
                else
                {
                    modulePath = Path.Combine(_modset.ModulesPath, moditem.ModuleName, moditem.ModuleVersion,
                        moditem.ModuleName + ".dll");
                }

                if (File.Exists(modulePath))
                {
                    Assembly.LoadFrom(modulePath);
                    CheckAssetsFolder(moditem);
                    Console.WriteLine($"Cargado módulo desde : {modulePath}");
                }
                else
                {
                    Console.WriteLine($"No se encuentra el módulo {modulePath}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        var assemblies = currentDomain.GetAssemblies();
        Console.WriteLine("List of assemblies loaded in current appdomain:");
        foreach (var assem in assemblies)
            Console.WriteLine(assem.ToString());
        return assemblies;
    }
}