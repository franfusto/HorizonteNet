using System.Collections;
using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Horizonte;

/// <summary>
/// Define el tipo de almacenamiento de una credencial.
/// </summary>
public enum CredentialType
{
    /// <summary>
    /// Almacenada en el archivo JSON local de secretos.
    /// </summary>
    Json,
    /// <summary>
    /// Almacenada como variable de entorno del sistema.
    /// </summary>
    Environment
}

/// <summary>
/// Representa un elemento de credencial con su clave, valor y tipo de origen.
/// </summary>
public class CredentialItem
{
    /// <summary>
    /// Clave identificadora de la credencial.
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Valor asociado a la credencial.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de origen (JSON o Variable de Entorno) de la credencial.
    /// </summary>
    public CredentialType Type { get; set; }
}

/// <summary>
/// Gestor de credenciales que permite leer y escribir secretos desde variables de entorno
/// y un archivo JSON local. Prioriza las variables de entorno sobre el archivo JSON.
/// </summary>
public class HCredManager : IHCredManager
{
    private readonly ILogger<HCredManager> _logger;

    /// <summary>
    /// Prefijo utilizado para identificar variables de entorno pertenecientes a Horizonte.
    /// </summary>
    private const string EnvPrefix = "horizonte_";

    /// <summary>
    /// Ruta completa al archivo JSON donde se guardan los secretos.
    /// </summary>
    private readonly string _secretsFilePath;

    /// <summary>
    /// Objeto de bloqueo para sincronizar el acceso al archivo de secretos.
    /// </summary>
    private static readonly object _fileLock = new();

    /// <summary>
    /// Diccionario concurrente que mantiene las credenciales en memoria para acceso rápido.
    /// </summary>
    private ConcurrentDictionary<string, CredentialItem> _credentials = new();

    /// <summary>
    /// Opciones de serialización JSON para el archivo de secretos.
    /// </summary>
    private readonly JsonSerializerOptions _jsonOptions = new() { WriteIndented = true };

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="HCredManager"/>.
    /// </summary>
    /// <param name="logger">Servicio de logging para registrar errores.</param>
    public HCredManager(ILogger<HCredManager> logger)
    {
        _logger = logger;
        _secretsFilePath = EnsureSecretsFileExists();
        GenerateCollection();
    }

    /// <summary>
    /// Asegura que el directorio .horizonte y el archivo secrets.json existan en el perfil del usuario.
    /// </summary>
    /// <returns>La ruta completa al archivo de secretos.</returns>
    private string EnsureSecretsFileExists()
    {
        string userHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(userHome))
        {
            userHome = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }

        string horizonDir = Path.Combine(userHome, ".horizonte");
        if (!Directory.Exists(horizonDir))
        {
            Directory.CreateDirectory(horizonDir);
        }

        string filePath = Path.Combine(horizonDir, "secrets.json");
        if (!File.Exists(filePath))
        {
            lock (_fileLock)
            {
                if (!File.Exists(filePath))
                {
                    // Crea un JSON vacío inicial
                    File.WriteAllText(filePath, "{}");
                }
            }
        }

        return filePath;
    }

    /// <summary>
    /// Genera la colección interna de credenciales cargando primero del entorno y luego del JSON.
    /// </summary>
    private void GenerateCollection()
    {
        var newCollection = new ConcurrentDictionary<string, CredentialItem>();

        // 1. Cargar desde variables de entorno (tienen mayor prioridad)
        LoadFromEnvironment(newCollection);

        // 2. Cargar desde JSON (solo añade si no existen ya por entorno)
        LoadFromJson(newCollection);

        _credentials = newCollection;
    }

    /// <summary>
    /// Busca variables de entorno con el prefijo definido y las añade a la colección.
    /// </summary>
    /// <param name="collection">Colección donde se cargarán las variables.</param>
    private void LoadFromEnvironment(ConcurrentDictionary<string, CredentialItem> collection)
    {
        try
        {
            var envVars = Environment.GetEnvironmentVariables();
            foreach (DictionaryEntry de in envVars)
            {
                string key = de.Key.ToString()!;
                if (key.StartsWith(EnvPrefix))
                {
                    string cleanKey = key.Substring(EnvPrefix.Length);
                    collection[cleanKey] = new CredentialItem
                    {
                        Key = cleanKey,
                        Value = de.Value?.ToString() ?? string.Empty,
                        Type = CredentialType.Environment
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError( "Error al cargar variables de entorno",ex);
        }
    }

    /// <summary>
    /// Carga las credenciales guardadas en el archivo JSON de secretos.
    /// </summary>
    /// <param name="collection">Colección donde se cargarán las credenciales.</param>
    private void LoadFromJson(ConcurrentDictionary<string, CredentialItem> collection)
    {
        try
        {
            var jsonCredentials = ReadSecretsDict();
            if (jsonCredentials == null) return;

            foreach (var (key, value) in jsonCredentials)
            {
                // Solo añadimos si no existe ya (prioridad de entorno)
                collection.TryAdd(key, new CredentialItem
                {
                    Key = key,
                    Value = value,
                    Type = CredentialType.Json
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError( $"Error al cargar el archivo de secretos en {_secretsFilePath}", ex);
        }
    }

    /// <summary>
    /// Lee y deserializa el contenido del archivo de secretos.
    /// </summary>
    /// <returns>Diccionario con las claves y valores, o null si el archivo no existe.</returns>
    private Dictionary<string, string>? ReadSecretsDict()
    {
        lock (_fileLock)
        {
            if (!File.Exists(_secretsFilePath)) return null;

            // Uso de FileShare.ReadWrite para evitar bloqueos si otros procesos están leyendo/escribiendo
            using var stream = new FileStream(_secretsFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var reader = new StreamReader(stream);
            string json = reader.ReadToEnd();
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
    }

    /// <summary>
    /// Serializa y escribe el diccionario de secretos en el archivo JSON local.
    /// </summary>
    /// <param name="dict">Diccionario a guardar.</param>
    private void WriteSecretsDict(Dictionary<string, string> dict)
    {
        lock (_fileLock)
        {
            string updatedJson = JsonSerializer.Serialize(dict, _jsonOptions);
            using var stream = new FileStream(_secretsFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = new StreamWriter(stream);
            writer.Write(updatedJson);
        }
    }

    /// <summary>
    /// Persiste el valor de una credencial en su destino correspondiente (Entorno o JSON).
    /// </summary>
    /// <param name="item">La credencial a persistir.</param>
    private void SaveCollectionValue(CredentialItem item)
    {
        if (item.Type == CredentialType.Environment)
        {
            try
            {
                Environment.SetEnvironmentVariable(EnvPrefix + item.Key, item.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError( $"Error al establecer variable de entorno {item.Key}", ex);
            }
        }
        else
        {
            try
            {
                var dict = ReadSecretsDict() ?? new Dictionary<string, string>();
                dict[item.Key] = item.Value;
                WriteSecretsDict(dict);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error al guardar el secreto {item.Key} en {_secretsFilePath}",ex);
            }
        }
    }

    /// <summary>
    /// Obtiene la lista de todas las claves de credenciales disponibles.
    /// </summary>
    public IEnumerable<string> KeyList => _credentials.Keys;

    /// <summary>
    /// Obtiene el valor de una credencial. Si no existe, crea una nueva entrada vacía en el archivo JSON.
    /// </summary>
    /// <param name="key">La clave de la credencial.</param>
    /// <returns>El valor de la credencial o string.Empty si no existía.</returns>
    public string GetCredential(string key)
    {
        if (_credentials.TryGetValue(key, out var item))
        {
            return item.Value;
        }

        // Si no existe, lo añadimos como Json por defecto con valor vacío
        var newItem = new CredentialItem
        {
            Key = key,
            Value = string.Empty,
            Type = CredentialType.Json
        };

        if (_credentials.TryAdd(key, newItem))
        {
            SaveCollectionValue(newItem);
        }

        return string.Empty;
    }

    /// <summary>
    /// Establece o actualiza el valor de una credencial.
    /// </summary>
    /// <param name="key">Clave de la credencial.</param>
    /// <param name="value">Nuevo valor.</param>
    public void SetCredential(string key, string value)
    {
        if (_credentials.TryGetValue(key, out var item))
        {
            item.Value = value;
        }
        else
        {
            item = new CredentialItem
            {
                Key = key,
                Value = value,
                Type = CredentialType.Json
            };
            _credentials[key] = item;
        }

        SaveCollectionValue(item);
    }

    /// <summary>
    /// Elimina una credencial de la memoria y del almacenamiento persistente.
    /// </summary>
    /// <param name="key">Clave de la credencial a eliminar.</param>
    public void DeleteCredential(string key)
    {
        if (_credentials.TryRemove(key, out var item))
        {
            if (item.Type == CredentialType.Environment)
            {
                try
                {
                    Environment.SetEnvironmentVariable(EnvPrefix + item.Key, null);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al eliminar variable de entorno {item.Key}", ex);
                }
            }
            else
            {
                try
                {
                    var dict = ReadSecretsDict();
                    if (dict != null && dict.Remove(item.Key))
                    {
                        WriteSecretsDict(dict);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error al eliminar el secreto {item.Key} en {_secretsFilePath}", ex);
                }
            }
        }
    }
}