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

    /// <summary>
    /// Proporciona un marco contextual para gestionar y recuperar archivos de datos
    /// con capacidades de serialización y deserialización. Permite realizar actualizaciones 
    /// utilizando datos o acciones específicas, garantizando la seguridad en el acceso concurrente
    /// a través de mecanismos de bloqueo.
    /// </summary>
    /// <param name="contextName">
    /// Un valor opcional que define el nombre del contexto. Si se omite, se utiliza un nombre predeterminado.
    /// </param>
    /// <param name="defaultSerializerOptions">
    /// Opciones de serialización predeterminadas. Si se omiten, se usan las opciones por defecto.
    /// </param>
    public HContext(string? contextName = null, JsonSerializerOptions? defaultSerializerOptions = null)
    {
        _contextName = contextName ?? _contextName;
        _serializerOptions = defaultSerializerOptions ?? new JsonSerializerOptions();
        _userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        if (string.IsNullOrEmpty(_userDirectory))
        {
            _userDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        }
    }

    /// <summary>
    /// Recupera la ruta de archivo para un archivo JSON correspondiente a un tipo específico.
    /// </summary>
    /// <typeparam name="T">
    /// El tipo asociado con el archivo JSON.
    /// </typeparam>
    /// <param name="contextName">
    /// El nombre del contexto que determina la estructura del directorio y el nombre del archivo.
    /// Si es nulo, se utiliza un nombre de contexto predeterminado.
    /// </param>
    /// <returns>
    /// La ruta de archivo del archivo JSON para el tipo especificado. Retorna la ruta de archivo
    /// predeterminada si no existe el archivo en el directorio del usuario.
    /// </returns>
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

    /// <summary>
    /// Obtiene el directorio de sobrescrituras locales para el contexto especificado.
    /// Crea el directorio si no existe.
    /// </summary>
    /// <param name="contextName">Nombre del contexto. Si es nulo, utiliza el predeterminado.</param>
    /// <returns>La ruta completa al directorio de sobrescrituras locales.</returns>
    private string GetLocalOverridesDir(string? contextName = null)
    {
        string localOverridesDir = Path.Combine(_userDirectory!, "." + (contextName ?? _contextName), "localoverrides");
        if (!Directory.Exists(localOverridesDir))
        {
            Directory.CreateDirectory(localOverridesDir);
        }

        return localOverridesDir;
    }

    /// <summary>
    /// Obtiene la ruta del archivo de sobrescritura local para un tipo específico y contexto.
    /// </summary>
    /// <typeparam name="T">El tipo asociado con el archivo de sobrescritura.</typeparam>
    /// <param name="contextName">Nombre del contexto. Si es nulo, utiliza el predeterminado.</param>
    /// <returns>La ruta completa al archivo JSON de sobrescritura local.</returns>
    private string GetLocalOverrideFilePath<T>(string? contextName = null)
    {
        return Path.Combine(GetLocalOverridesDir(contextName), typeof(T).Name + ".json");
    }

    /// <summary>
    /// Determina el origen de los datos para la sección del tipo especificado.
    /// Comprueba si existe un archivo de sobrescritura local antes de recurrir al contexto general.
    /// </summary>
    /// <typeparam name="T">El tipo de la sección a consultar.</typeparam>
    /// <param name="contextname">Nombre del contexto opcional.</param>
    /// <returns>El origen de la sección: <see cref="SectionSource.LocalOverride"/> o <see cref="SectionSource.Context"/>.</returns>
    public SectionSource Source<T>(string? contextname = null)
    {
        lock (_fileLock)
        {
            var localOverridePath = GetLocalOverrideFilePath<T>(contextname);
            return File.Exists(localOverridePath) ? SectionSource.LocalOverride : SectionSource.Context;
        }
    }


    /// <summary>
    /// Recupera un objeto del tipo especificado desde el archivo de contexto, utilizando el
    /// nombre del contexto proporcionado o el nombre de contexto predeterminado si no se especifica.
    /// La deserialización se realiza con base en las opciones de serialización definidas en el contexto.
    /// </summary>
    /// <typeparam name="T">
    /// El tipo de objeto a recuperar del archivo de contexto.
    /// </typeparam>
    /// <param name="contextname">
    /// Opcional. El nombre específico del contexto con el cual sobrescribir el predeterminado.
    /// </param>
    /// <returns>
    /// Devuelve el objeto del tipo <typeparamref name="T"/> si la obtención y deserialización
    /// son exitosas; de lo contrario, devuelve el valor predeterminado para el tipo <typeparamref name="T"/>.
    /// </returns>
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

    /// <summary>
    /// Actualiza una sección específica del archivo de contexto con la acción proporcionada.
    /// Garantiza la seguridad durante el proceso de actualización y permite modificaciones
    /// del contenido de la sección dentro del archivo de contexto.
    /// </summary>
    /// <typeparam name="T">
    /// El tipo de la sección que se está actualizando dentro del archivo de contexto.
    /// </typeparam>
    /// <param name="update">
    /// La acción que define las modificaciones que se deben realizar en la sección.
    /// </param>
    /// <param name="contextname">
    /// Parámetro opcional que especifica el nombre del archivo de contexto a actualizar.
    /// Si es nulo, se utiliza el nombre de contexto predeterminado.
    /// </param>
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


    /// <summary>
    /// Actualiza el archivo de contexto con un nuevo valor para el tipo especificado.
    /// La actualización se aplica serializando el nuevo valor y escribiéndolo
    /// al archivo JSON designado que corresponde al nombre del contexto y tipo especificado.
    /// Garantiza la seguridad durante la operación mediante mecanismos de bloqueo.
    /// </summary>
    /// <typeparam name="T">
    /// El tipo de los datos a actualizar en el contexto.
    /// </typeparam>
    /// <param name="newvalue">
    /// El nuevo valor a actualizar en el archivo de contexto.
    /// </param>
    /// <param name="contextname">
    /// El nombre específico del contexto para usar en la actualización. 
    /// Por defecto, se utiliza el nombre de contexto global si no se especifica.
    /// </param>
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


    /// <summary>
    /// Proporciona métodos auxiliares para manipular archivos JSON, incluyendo funciones para leer,
    /// actualizar y escribir secciones dentro de archivos JSON.
    /// </summary>
    public static class JsonFileHelper
    {
        /// <summary>
        /// Representa una función delegada que proporciona opciones de serialización predeterminadas para la serialización JSON.
        /// </summary>
        /// <remarks>
        /// Las opciones predeterminadas incluyen las siguientes configuraciones:
        /// - Formateo JSON con indentación.
        /// - Inclusión de campos en la serialización.
        /// - Uso de un escape relajado y no seguro para el codificador (encoder).
        /// - Un <see cref="JsonStringEnumConverter"/> predefinido para manejar la serialización de enums.
        /// </remarks>
        public static Func<JsonSerializerOptions> DefaultSerializerOptions = new Func<JsonSerializerOptions>(() =>
            new JsonSerializerOptions
            {
                WriteIndented = true,
                IncludeFields = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                Converters = { new JsonStringEnumConverter() }
            });

        /// <summary>
        /// Intenta actualizar una sección existente en el archivo JSON utilizando una acción.
        /// Si la sección existe, se recupera, se aplica la acción y se vuelve a guardar.
        /// </summary>
        /// <typeparam name="T">El tipo de la sección a actualizar.</typeparam>
        /// <param name="jsonFilePath">Ruta del archivo JSON.</param>
        /// <param name="sectionName">Nombre de la sección (propiedad) en el JSON.</param>
        /// <param name="updateAction">Acción para modificar el valor existente.</param>
        /// <param name="serializerOptions">Opciones de serialización.</param>
        public static void AddOrUpdateSection<T>(string jsonFilePath, string sectionName,
            Action<T>? updateAction = null, JsonSerializerOptions? serializerOptions = null)
        {
            var updatedValue = TryGet<T>(jsonFilePath, sectionName, out var value, serializerOptions)
                ? value
                : default(T);
            if (updatedValue == null) return;
            updateAction?.Invoke(updatedValue);
            AddOrUpdateSection(jsonFilePath, sectionName, updatedValue, serializerOptions);
        }

        /// <summary>
        /// Agrega o reemplaza una sección específica en el archivo JSON con el valor proporcionado.
        /// Si la sección ya existe, se sobrescribe completamente con el nuevo valor.
        /// Si no existe, se añade una nueva propiedad al objeto raíz del JSON.
        /// </summary>
        /// <typeparam name="T">El tipo de la sección a guardar.</typeparam>
        /// <param name="jsonFilePath">Ruta del archivo JSON.</param>
        /// <param name="sectionName">Nombre de la sección (propiedad) en el JSON.</param>
        /// <param name="value">El valor a establecer para la sección.</param>
        /// <param name="serializerOptions">Opciones de serialización.</param>
        public static void AddOrUpdateSection<T>(string jsonFilePath, string sectionName, T value,
            JsonSerializerOptions? serializerOptions = null)
        {
            var jsonContent = ReadOrCreateJsonFile(jsonFilePath);
            using var jsonDocument = JsonDocument.Parse(jsonContent);
            using var stream = File.Create(jsonFilePath);
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions()
            {
                Indented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            });
            writer.WriteStartObject();
            var isWritten = false;
            var optionsElement = JsonDocument.Parse(JsonSerializer.SerializeToUtf8Bytes(value, serializerOptions));
            foreach (var element in jsonDocument.RootElement.EnumerateObject())
            {
                if (element.Name != sectionName)
                {
                    element.WriteTo(writer);
                    continue;
                }

                writer.WritePropertyName(element.Name);
                optionsElement.WriteTo(writer);
                isWritten = true;
            }

            if (!isWritten)
            {
                writer.WritePropertyName(sectionName);
                optionsElement.WriteTo(writer);
            }

            writer.WriteEndObject();
            writer.Flush();
            stream.SetLength(stream.Position);
        }

        /// <summary>
        /// Intenta obtener un objeto del tipo <typeparamref name="T"/> desde un archivo JSON y sección especificados.
        /// Devuelve un valor booleano indicando si la operación fue exitosa.
        /// </summary>
        /// <param name="jsonFilePath">
        /// La ruta del archivo JSON que será leído.
        /// </param>
        /// <param name="sectionName">
        /// El nombre de la sección dentro del archivo JSON desde donde se recuperarán los datos.
        /// </param>
        /// <param name="value">
        /// Cuando este método regrese, contendrá el objeto deserializado del tipo <typeparamref name="T"/> si fue exitoso,
        /// o el valor predeterminado de <typeparamref name="T"/> si la operación falló.
        /// </param>
        /// <param name="serializerOptions">
        /// Opciones de serialización opcionales que influyen en cómo se deserializan los contenidos JSON. Si se omiten, se usarán opciones predeterminadas.
        /// </param>
        /// <returns>
        /// Un valor booleano que indica si el objeto fue recuperado y deserializado con éxito.
        /// Devuelve true si la operación tuvo éxito, de lo contrario false.
        /// </returns>
        public static bool TryGet<T>(string jsonFilePath, string sectionName, out T? value,
            JsonSerializerOptions? serializerOptions = null)
        {
            if (File.Exists(jsonFilePath))
            {
                var jsonContent = File.ReadAllBytes(jsonFilePath);
                using var jsonDocument = JsonDocument.Parse(jsonContent);
                if (jsonDocument.RootElement.TryGetProperty(sectionName, out var sectionValue))
                {
                    value = JsonSerializer.Deserialize<T>(sectionValue.ToString(), serializerOptions);
                    return true;
                }
            }

            value = default(T);
            return false;
        }

        /// <summary>
        /// Lee el contenido de un archivo JSON si existe. Si el archivo no existe,
        /// crea un nuevo archivo con un objeto JSON vacío, asegurándose de que la estructura
        /// del directorio también sea creada si es necesario. Devuelve el contenido del archivo como un array de bytes.
        /// </summary>
        /// <param name="jsonFilePath">
        /// La ruta completa del archivo JSON. Este parámetro determina la ubicación
        /// en la que se leerá o creará el archivo.
        /// </param>
        /// <returns>
        /// Un array de bytes que contiene el contenido del archivo JSON especificado. Si el archivo
        /// es recién creado, devuelve los bytes que representan un objeto JSON vacío.
        /// </returns>
        private static byte[] ReadOrCreateJsonFile(string jsonFilePath)
        {
            if (File.Exists(jsonFilePath)) return File.ReadAllBytes(jsonFilePath);
            var fileDirectoryPath = Path.GetDirectoryName(jsonFilePath);
            if (!string.IsNullOrEmpty(fileDirectoryPath))
                Directory.CreateDirectory(fileDirectoryPath);
            File.WriteAllText(jsonFilePath, "{}");
            return File.ReadAllBytes(jsonFilePath);
        }
    }
}