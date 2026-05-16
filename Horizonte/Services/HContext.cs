using System.Text.Json;
using Horizonte;
using System;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using log4net;

namespace Horizonte;

public class HContext : IHContext
{
    private static readonly ILog Log = LogManager.GetLogger(typeof(HContext));
    private readonly string _contextName = "horizonte";
    private JsonSerializerOptions _serializerOptions = new();
    private static readonly object _fileLock = new();
    private string? _userDirectory = null;

    public HContext(string contextName , string rootPath )
    {
        _contextName = contextName ?? _contextName;
        _serializerOptions =  new JsonSerializerOptions();
        _userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(_userDirectory))
        {
            _userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }

    private string GetJsonFilePath<T>(string? contextName)
    {
        // Definir el nombre de la carpeta que empieza con un punto
        string contextFolder = Path.Combine(_userDirectory!, "." + (contextName ?? _contextName));

        if (Directory.Exists(contextFolder))
        {
            // Obtener el nombre del archivo tipo `.json` basado en `typeof(T).Name`
            string jsonFileName = typeof(T).Name + ".json";

            // Combinar para obtener la ruta completa del archivo esperado dentro de la carpeta personal
            string jsonFilePath = Path.Combine(contextFolder, jsonFileName);

            // Verificar si el archivo existe
            if (File.Exists(jsonFilePath))
            {
                return jsonFilePath;
            }
        }

        // Retornar la ruta actual si no se encuentra el archivo en el directorio personalizado
        return (contextName ?? _contextName) + ".json";
    }

    private string GetLocalOverridesDir(string? contextName = null)
    {
        string localOverridesDir = Path.Combine(_userDirectory!, "." + (contextName ?? _contextName), "localoverrides");
        if (!Directory.Exists(localOverridesDir))
        {
            Directory.CreateDirectory(localOverridesDir);
        }

        return localOverridesDir;
    }

    private string GetLocalOverrideFilePath<T>(string? contextName = null)
    {
        return Path.Combine(GetLocalOverridesDir(contextName), typeof(T).Name + ".json");
    }

    public SectionSource Source<T>(string? contextname = null)
    {
        lock (_fileLock)
        {
            var localOverridePath = GetLocalOverrideFilePath<T>(contextname);
            return File.Exists(localOverridePath) ? SectionSource.LocalOverride : SectionSource.Context;
        }
    }


    public T? Get<T>(string? contextname = null)
    {
        try
        {
            lock (_fileLock)
            {
                var filePath = Source<T>(contextname) == SectionSource.LocalOverride
                    ? GetLocalOverrideFilePath<T>(contextname)
                    : GetJsonFilePath<T>(contextname ?? _contextName);

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

    public void Update<T>(Action<T> update, string? contextname = null)
    {
        try
        {
            lock (_fileLock)
            {
                var filePath = Source<T>(contextname) == SectionSource.LocalOverride
                    ? GetLocalOverrideFilePath<T>(contextname)
                    : GetJsonFilePath<T>(contextname ?? _contextName);

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


    public void Update<T>(T newvalue, string? contextname = null)
    {
        try
        {
            lock (_fileLock)
            {
                var filePath = Source<T>(contextname) == SectionSource.LocalOverride
                    ? GetLocalOverrideFilePath<T>(contextname)
                    : GetJsonFilePath<T>(contextname ?? _contextName);

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