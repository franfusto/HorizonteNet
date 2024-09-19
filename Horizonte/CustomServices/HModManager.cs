using System.IO.Compression;
using System.Reflection.Metadata.Ecma335;
using System.Text.Json;
using Horizonte;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Horizonte;

public class HModManager : IHModManager
{
    private ILogger<HModManager>? _log;
    private IHContext? _context;
    private ModulesSettings _modulesSettings;

    public HModManager(IHorizonteEnv env)
    {
        _log = env.HHost.Services.GetService<ILogger<HModManager>>();
        _context = env.HHost.Services.GetService<IHContext>();
        _modulesSettings = _context?.Get<ModulesSettings>() ?? new ModulesSettings();
    }

    public void RemoveFiles(ModulesSettingsItem item)
    {
        var dir = Path.Combine(_modulesSettings.ModulesPath, item.ModuleName, item.ModuleVersion);
        if(Directory.Exists(dir)) Directory.Delete(dir,true);
    }
    
    public bool InstallFromFile(string filename)
    {
        try
        {
            _log?.LogInformation($"Instalando módulo {filename}");
            if (!Directory.Exists(_modulesSettings.ModulesPath))
                Directory.CreateDirectory(_modulesSettings.ModulesPath);
            using var archive = ZipFile.OpenRead(filename);
            var infofile = archive.Entries.FirstOrDefault(x => x.Name == "module.json");
            if (infofile == null) return false;
            using var stream = infofile.Open();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var info = JsonSerializer.Deserialize<ModuleConfig>(text);
            var moduledir = Path.Combine(_modulesSettings.ModulesPath, info.ModuleName);
            if (!Directory.Exists(moduledir)) Directory.CreateDirectory(moduledir);
            var versiondir = Path.Combine(moduledir, info.ModuleVersion);
            if (!Directory.Exists(versiondir)) Directory.CreateDirectory(versiondir);
            var modfiles = archive.Entries.Where(x => x.FullName.StartsWith("_module"));
            foreach (var file in modfiles)
            {
                string destinationPath = Path.GetFullPath(Path.Combine(versiondir, file.FullName.Replace("_module/", "")));
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                file.ExtractToFile(destinationPath, true);
            }
            _modulesSettings.List.Add(new ModulesSettingsItem()
            {
                Active = false, ModuleName = info.ModuleName, ModuleVersion = info.ModuleVersion
            });
            _context?.Update(_modulesSettings);

            return true;
        }
        catch (Exception e)
        {
            _log?.LogError(e.ToString());
            return false;
        }
    }

    public bool InstallFromRepo(string filename)
    {
        _log?.LogInformation($"Instalando módulo {filename}");
        return InstallFromFile(filename);
    }

    private ModuleConfig? GetInfo(string path)
    {
        try
        {
            using var archive = ZipFile.OpenRead(path);
            var zipfile = archive.Entries.FirstOrDefault(x => x.Name == "module.json");
            if (zipfile == null) return null;
            using var stream = zipfile.Open();
            using var reader = new StreamReader(stream);
            var text = reader.ReadToEnd();
            var info = JsonSerializer.Deserialize<ModuleConfig>(text);
            return info;
        }
        catch (Exception e)
        {
            _log?.LogError(e.ToString());
            return null;
        }
    }

    public List<ModulesSettingsItem> GetRepoFiles()
    {
        _log?.LogInformation($"Obtener lista de módulos del repositorio");
        var list = new List<ModulesSettingsItem>();

        if (!Directory.Exists(_modulesSettings.RepoPath)) return list;
        var direpo = new DirectoryInfo(_modulesSettings.RepoPath);
        foreach (var item in direpo.GetFiles("*.hmod"))
        {
            var moduleConfig = GetInfo(item.FullName);
            if (moduleConfig != null)
                list.Add(new ModulesSettingsItem()
                {
                    ModuleName = moduleConfig.ModuleName,
                    ModuleVersion = moduleConfig.ModuleVersion,
                    Path = item.FullName
                });
        }

        return list;
    }

    private static void AddFilesFromDirectoryToZip(ZipArchive archive, string sourcePath, string entryPrefix = "")
    {
        var directoryInfo = new DirectoryInfo(sourcePath);
        foreach (var file in directoryInfo.GetFiles())
        {
            var entryName = Path.Combine(entryPrefix, file.Name);
            archive.CreateEntryFromFile(file.FullName, entryName);
        }
        foreach (var subDirectory in directoryInfo.GetDirectories())
        {
            var newEntryPrefix = Path.Combine(entryPrefix, subDirectory.Name);
            AddFilesFromDirectoryToZip(archive, subDirectory.FullName, newEntryPrefix);
        }
    }
    public byte[] BuildModule(NewModuleConfig newModuleConfig)
    {
        try
        {
            var moduleConfig = new ModuleConfig()
            {
                ModuleName = newModuleConfig.ModuleName,
                ModuleVersion = newModuleConfig.ModuleVersion
            };
            _log?.LogInformation($"Construir módulo {newModuleConfig.ModuleName} {newModuleConfig.ModuleVersion}");
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var demoFile = archive.CreateEntry("module.json");

                using (var entryStream = demoFile.Open())
                using (var streamWriter = new StreamWriter(entryStream))
                {
                    string jsonString = JsonSerializer.Serialize(moduleConfig,
                        new JsonSerializerOptions() { WriteIndented = true,IncludeFields = true});
                    streamWriter.Write(jsonString);
                }
                AddFilesFromDirectoryToZip(archive, newModuleConfig.BinPath, "_module");
            }
            return memoryStream.ToArray();
        }
        catch (Exception e)
        {
            _log?.LogError(e.ToString());
        }
        return Array.Empty<byte>();
    }
}