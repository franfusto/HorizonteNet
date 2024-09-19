using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace Horizonte;

public class HAssemblyManager : IHAssemblyManager
{
    private ModulesSettings? modset;
    private HorizonteEnv _env;
    public Assembly[]? Assemblies;


    public HAssemblyManager(ModulesSettings modulesSettings, HorizonteEnv env)
    {
        modset = modulesSettings;
        _env = env;
        Assemblies = LoadAssembliesFromEnviromente();
    }

    private void CheckAssetsFolder(ModulesSettingsItem item)
    {
        string assetsdir = string.Empty;
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

    public Assembly[] LoadAssembliesFromEnviromente()
    {
        //comprobar directorio
        if (!Directory.Exists(modset.ModulesPath))
            Directory.CreateDirectory(modset.ModulesPath);

        //cargamos ensamblados
        AppDomain currentDomain = AppDomain.CurrentDomain;
        foreach (var moditem in modset.List)
        {
            if (!moditem.Active) continue;
            try
            {
                string modulePath;
                if (moditem.Path != null)
                {
                    modulePath = moditem.Path;
                }
                else
                {
                    modulePath = Path.Combine(modset.ModulesPath, moditem.ModuleName, moditem.ModuleVersion,
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

        //

        Console.WriteLine("List of assemblies loaded in current appdomain:");
        foreach (Assembly assem in assemblies)
            Console.WriteLine(assem.ToString());


        return assemblies;
    }
}