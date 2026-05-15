using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Text.Json;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte;

/// <summary>
/// Representa un panel de módulo que proporciona una interfaz entre un módulo y el entorno modular.
/// Esta clase expone funcionalidades de los módulos mediante comandos, conocidos como HorizonteCommand.
/// Cada comando tiene una clave única, utilizada para exponerlo al entorno modular, y una descripción opcional.
/// </summary>
[HorizonteModule("Horizonte")]
public class PanelModulo
{
    /// <summary>
    /// Representa la interfaz del entorno modular utilizada por la clase PanelModulo en el framework Horizonte.
    /// Proporciona funcionalidad central y acceso a servicios modulares, recursos y operaciones del ciclo de vida.
    /// </summary>
    private readonly IHorizonteEnv _env;
    private readonly IHGesCom _gesCom;
    private readonly ILogger<PanelModulo> _logger;


    /// <summary>
    /// Represent un panel de módulo en el framework Horizonte que funciona como una interfaz 
    /// entre un módulo y el entorno modular. Expone las funcionalidades del módulo 
    /// como comandos (HorizonteCommand), cada uno de los cuales tiene una clave única y, opcionalmente, 
    /// una descripción.
    /// </summary>
    public PanelModulo(IHorizonteEnv enviorment, IHGesCom gesCom, ILogger<PanelModulo> logger)
    {
        _env = enviorment;
        _gesCom = gesCom;
        _logger = logger;
    }

    /// <summary>
    /// Inicializa la interfaz del módulo con el entorno modular en Horizonte.
    /// Este método establece el estado inicial o la configuración necesaria para el módulo.
    /// </summary>
    /// <returns>
    /// Devuelve <c>true</c> indicando que la inicialización fue exitosa.
    /// </returns>
    [HorizonteRole("init")]
    [HorizonteCommand("Horizonte_Init")]
    public bool Init()
    {
        _logger.LogInformation("Módulo Horizonte Iniciado");
        return true;
    }

    //App

    /// <summary>
    /// Reinicia el sistema mediante la interacción con el entorno Horizonte.
    /// Este comando está asociado a la clave "Horizonte_Reboot".
    /// </summary>
    [HorizonteCommand("Horizonte_Reboot", Description = "Reinicia el sistema")]
    public void Reboot()
    {
        _env.Reboot();
    }

    /// <summary>
    /// Cierra el sistema invocando la función Quit en el entorno Horizonte asociado.
    /// Este método se expone como un comando Horizonte con la clave única "Horizonte_Quit" y una descripción "Cierra el sistema".
    /// </summary>
    [HorizonteCommand("Horizonte_Quit", description: "Cierra el sistema")]
    public void Quit()
    {
        _env.Quit();
    }

    //Memory Log

    /// <summary>
    /// Verifica si la funcionalidad de registro de memoria está habilitada en el contexto actual del sistema.
    /// </summary>
    /// <returns>True si el registro de memoria está habilitado; de lo contrario, false.</returns>
    [HorizonteCommand("MemoryLog_IsEnabled", description: "Habilita el registro en memoria")]
    public bool MemoryLog_IsEnabled()
    {
        var memap = GetMemoryAppender();
        return memap != null;
    }

    /// <summary>
    /// Activa el registro en memoria inicializando y registrando una instancia de MemoryAppender.
    /// Este método verifica la existencia de un MemoryAppender en la configuración de registro.
    /// Si no existe tal appender, crea y activa un nuevo MemoryAppender, 
    /// lo agrega al repositorio de registros y genera un evento de cambio de configuración para aplicar los cambios.
    /// </summary>
    [HorizonteCommand("MemoryLog_Enable", "Habilita el registro")]
    public void MemoryLog_Enable()
    {
        var appender = GetMemoryAppender();
        if (appender != null) return;

        MemoryAppender memory = new MemoryAppender();
        memory.ActivateOptions();
        ((Hierarchy)LogManager.GetRepository()).Root.AddAppender(memory);
        ((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
    }

    /// <summary>
    /// Desactiva el sistema de registro en memoria en el entorno actual.
    /// </summary>
    /// <remarks>
    /// Este método elimina el appender en memoria de la jerarquía de registro, desactivando efectivamente el registro en memoria.
    /// También cierra el appender en memoria para liberar cualquier recurso subyacente y genera un evento de cambio de configuración
    /// para señalar actualizaciones en la configuración del sistema de registro.
    /// </remarks>
    [HorizonteCommand("MemoryLog_Disable", "Deshabilita el registro")]
    public void MemoryLog_Disable()
    {
        var appender = GetMemoryAppender();
        if (appender != null)
        {
            appender.Close();
            ((Hierarchy)LogManager.GetRepository()).Root.RemoveAppender(appender);
            ((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
        }
    }

    /// <summary>
    /// Recupera el registro en memoria como un arreglo de eventos registrados.
    /// </summary>
    /// <returns>Un arreglo de <see cref="log4net.Core.LoggingEvent"/> que contiene los eventos registrados actualmente en memoria.
    /// Si no hay registros disponibles, devuelve un arreglo vacío.</returns>
    [HorizonteCommand("MemoryLog_GetLog", "Obtiene el log de la aplicación")]
    public LoggingEvent[] MemoryLog_GetLog()
    {
        var appender = GetMemoryAppender();
        if (appender != null) return appender.GetEvents();
        return Array.Empty<LoggingEvent>();
    }

    /// <summary>
    /// Limpia el registro en memoria, eliminando todos los eventos registrados almacenados en el appender de memoria.
    /// </summary>
    /// <remarks>
    /// Este método interactúa con el appender de memoria para limpiar los eventos registrados.
    /// Forma parte del sistema modular Horizonte y puede ser invocado como un comando.
    /// </remarks>
    [HorizonteCommand("MemoryLog_Clear", "Limpia el registro")]
    public void MemoryLog_Clear()
    {
        var appender = GetMemoryAppender();
        if (appender != null) appender.Clear();
    }

    /// <summary>
    /// Recupera la instancia actual de MemoryAppender en la configuración de registro.
    /// </summary>
    /// <remarks>
    /// Este método busca entre los appenders registrados dentro del repositorio de registro actual 
    /// y devuelve el primer appender del tipo MemoryAppender si existe. Si no se encuentra un MemoryAppender, 
    /// el método devuelve null.
    /// </remarks>
    /// <returns>La instancia de MemoryAppender si se encuentra; de lo contrario, null.</returns>
    private MemoryAppender? GetMemoryAppender()
    {
        return LogManager.GetRepository().GetAppenders().FirstOrDefault(x => x.GetType() == typeof(MemoryAppender)) as
            MemoryAppender;
    }


    //Workers

    /// <summary>
    /// Recupera una lista de servicios en segundo plano (Workers) actualmente en ejecución con su información.
    /// </summary>
    /// <remarks>
    /// Este método devuelve una lista de los servicios que se están ejecutando en el entorno de la aplicación, 
    /// incluyendo el nombre del tipo del servicio, su estado de ejecución y su nombre (si está disponible).
    /// </remarks>
    /// <returns>
    /// Una lista de objetos RunningServiceInfo. Cada objeto contiene:
    /// - Typename: El nombre del tipo del servicio.
    /// - Isrunning: Un booleano que indica si el servicio está ejecutándose.
    /// - Name: El nombre del servicio, o una cadena vacía si no está disponible.
    /// </returns>
    [HorizonteCommand("Workers_GetServicesRunning", "Obtiene la lista de Workers que se están ejecutando")]
    public List<RunningServiceInfo> Workers_GetServicesRunning()
    {
        return _env.HHost.Services.GetServices<BackgroundService>()
            .Select(service => new RunningServiceInfo(
                Typename: service.GetType().Name,
                Isrunning: IsRunning(service),
                Name: (service as IHorizonteBackgroundService)?.ServiceName ?? string.Empty))
            .ToList();
    }

    [HorizonteCommand("Workers_GetAvailablesServices", "Obtiene los Tipos de los Workers disponibles en el sistema")]
    public List<Type> Workers_GetAvailablesServices()
    {
        var assemblymanager = _env.HHost.Services.GetService<IhAssemblyManager>();
        if (assemblymanager == null) return new List<Type>();

        var types = assemblymanager.Assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(IHorizonteBackgroundService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .ToList();

        return types;
    }

    /// <summary>
    /// Determina si el servicio en segundo plano especificado está actualmente en ejecución.
    /// </summary>
    /// <param name="service">El servicio en segundo plano a verificar.</param>
    /// <returns>True si el servicio está en ejecución; de lo contrario, false.</returns>
    private bool IsRunning(BackgroundService service)
    {
        bool running;
        if (service is IHorizonteBackgroundService)
            running = (bool)(service as IHorizonteBackgroundService)?.IsRunning;
        else
            running = false;
        return running;
    }

    /// <summary>
    /// Inicia un servicio de trabajador específico por su nombre.
    /// </summary>
    /// <param name="servicename">El nombre del servicio a iniciar.</param>
    /// <returns>
    /// Devuelve un valor de tipo <see cref="bool" /> que indica si el servicio se ha iniciado correctamente o no:
    /// <list type="bullet">
    /// <item>
    /// <description><c>true</c> si el servicio fue iniciado correctamente o ya estaba en ejecución.</description>
    /// </item>
    /// <item>
    /// <description><c>false</c> si no se pudo encontrar el servicio, ocurrió un error durante el proceso, o ya estaba en ejecución.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Este método busca entre todos los servicios registrados en el host que implementan <see cref="BackgroundService"/>.
    /// Si el servicio especificado se encuentra y no está en ejecución, intentará iniciarlo llamando a su método <c>StartAsync</c>.
    /// Cualquier error durante el proceso se registra con el logger.
    /// </remarks>
    [HorizonteCommand("Workers_StartService", "Inicia un servicio")]
    public bool Workers_StartService(string servicename)
    {
        try
        {
            // Obtenemos todos los servicios que implementan BackgroundService
            var services = _env?.HHost.Services.GetServices<BackgroundService>() ??
                           Enumerable.Empty<BackgroundService>();

            // Buscamos el servicio que implementa IHservice y cuyo nombre coincide
            var service = services.OfType<IHorizonteBackgroundService>()
                .FirstOrDefault(x => x.ServiceName.Equals(servicename, StringComparison.OrdinalIgnoreCase));

            // Si no encontramos el servicio, salimos de la función
            if (service == null)
            {
                return false;
            }

            // Verificamos si el servicio ya está en ejecución
            if (!IsRunning((BackgroundService)service))
            {
                // Iniciamos el servicio
                (service as BackgroundService)?.StartAsync(new CancellationToken());
                _logger?.LogInformation($"Servicio '{servicename}' iniciado");
                return true;
            }

            return false;
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Error al iniciar el servicio '{servicename}'");
            return false;
        }
    }

    /// <summary>
    /// Detiene un servicio de trabajador específico por su nombre.
    /// </summary>
    /// <param name="servicename">El nombre del servicio a detener.</param>
    /// <returns>
    /// Devuelve un valor de tipo <see cref="bool" /> que indica si el servicio se ha detenido correctamente o no:
    /// <list type="bullet">
    /// <item>
    /// <description><c>true</c> si el servicio fue detenido correctamente o ya estaba detenido.</description>
    /// </item>
    /// <item>
    /// <description><c>false</c> si no se pudo encontrar el servicio o ocurrió un error durante el proceso.</description>
    /// </item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// Este método busca entre todos los servicios registrados en el host que implementan <see cref="BackgroundService"/>.
    /// Si el servicio especificado se encuentra y está en ejecución, intentará detenerlo llamando a su método <c>StopAsync</c>.
    /// Cualquier error durante el proceso se registra con el logger.
    /// </remarks>
    [HorizonteCommand("Workers_StopService", "Detiene un servicio")]
    public bool Workers_StopService(string servicename)
    {
        try
        {
            // Obtenemos todos los servicios que implementan BackgroundService
            var services = _env?.HHost.Services.GetServices<BackgroundService>() ??
                           Enumerable.Empty<BackgroundService>();

            // Buscamos el servicio que implementa IHservice y cuyo nombre coincide
            var service = services.OfType<IHorizonteBackgroundService>()
                .FirstOrDefault(x => x.ServiceName.Equals(servicename, StringComparison.OrdinalIgnoreCase));

            // Si no encontramos el servicio, salimos de la función
            if (service == null)
            {
                return false;
            }

            // Verificamos si el servicio está en ejecución
            if (IsRunning((BackgroundService)service))
            {
                // Detenemos el servicio
                (service as BackgroundService)?.StopAsync(new CancellationToken());
                _logger?.LogInformation($"Servicio '{servicename}' detenido");
                return true;
            }

            return false; // Ya estaba detenido
        }
        catch (Exception e)
        {
            _logger?.LogError(e, $"Error al detener el servicio '{servicename}'");
            return false;
        }
    }

    //contexto
    /// <summary>
    /// Recupera el contenido del contexto de la aplicación como una cadena JSON formateada.
    /// </summary>
    /// <returns>
    /// Una cadena JSON formateada que representa el contenido del contexto de la aplicación.
    /// </returns>
    [HorizonteCommand("Context_GetJsonContent", "obtiene el contenido el contexto de la aplicación")]
    public string Context_GetJsonContent()
    {
        string fileContent = File.ReadAllText(_env!.Contextname + ".json");
        object? jsonObject = JsonSerializer.Deserialize<object>(fileContent);
        string formattedJsonString = JsonSerializer.Serialize(jsonObject, new JsonSerializerOptions
        {
            WriteIndented = true
        });
        return formattedJsonString;
    }

    /// <summary>
    /// Establece el contenido del contexto de la aplicación con la cadena JSON especificada y lo guarda en un archivo.
    /// </summary>
    /// <param name="content">La cadena formateada en JSON que representa el contexto de la aplicación a guardar.</param>
    [HorizonteCommand("Context_SetJsonContent", "Establece el contexto de la aplicación")]
    public void Context_SetJsonContent(string content)
    {
        File.WriteAllText(_env!.Contextname + ".json", content);
    }

    /// <summary>
    /// Devuelve la fecha y hora actuales del sistema.
    /// </summary>
    /// <returns>
    /// La fecha y hora representada como un objeto DateTime correspondiente al momento de la ejecución.
    /// </returns>
    [HorizonteCommand("Horizonte_SytstemDateTime", "Obtiene la fecha y hora del sistema")]
    public DateTime Horizonte_SytstemDateTime()
    {
        return DateTime.Now;
    }


    /// <summary>
    /// Obtiene y devuelve una lista en formato de cadena que describe los ensamblados cargados actualmente en el dominio de la aplicación.
    /// Incluye la ubicación y el nombre completo de cada ensamblado cargado.
    /// También registra esta información en el sistema de logs.
    /// </summary>
    /// <returns>Una cadena que contiene la ubicación y el nombre completo de cada ensamblado cargado, ordenados por su directorio.</returns>
    [HorizonteCommand("Horizonte_ListLoadedAssemblies", "Obtiene una lista de ensamblados cargados en el sistema")]
    public string ListLoadedAssemblies()
    {
        _logger?.LogInformation("ListLoadedAssemblies()=>");
        string result = string.Empty;
        try
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()
                         .OrderBy(x => Path.GetDirectoryName(x.Location)))
            {
                string line = assembly.Location + "=>  " + assembly.FullName ;
                _logger?.LogInformation(line);
            }
        }
        catch (Exception e)
        {
            _logger?.LogError(e, "Error al listar los ensamblados cargados");
        }

        return result;
    }
}

/*
//Módulos
[HorizonteCommand("Modules_InstallFromFile")]
public bool Modules_InstallFromFile(string filename)
{
   // Thread.Sleep(3000);
    return false;
}
[HorizonteCommand("Modules_InstallFromRepo")]
public bool Modules_InstallFromRepo(string filename)
{
   // Thread.Sleep(3000);
    return true;
}
[HorizonteCommand("Modules_GetRepoFiles")]
public List<ModulesSettingsItem> Modules_GetRepoFiles()
{
    var list = new List<ModulesSettingsItem>();
    return list;
}
[HorizonteCommand("Modulo_NuevoModulo")]
public byte[] CrearModulo(NewModuleConfig newModuleConfig)
{

    return Array.Empty<byte>();
}
*/

/// <summary>
/// Representa información sobre un servicio en ejecución en el sistema.
/// </summary>
/// <remarks>
/// Este registro se utiliza para almacenar metadatos sobre un servicio en segundo plano específico, 
/// incluyendo el nombre de su tipo, el estado de ejecución y el nombre para mostrar.
/// </remarks>
/// <param name="Typename">El nombre del tipo del servicio.</param>
/// <param name="Isrunning">Indica si el servicio se encuentra actualmente en ejecución.</param>
/// <param name="Name">El nombre para mostrar o identificador del servicio.</param>
public record RunningServiceInfo(string Typename, bool Isrunning, string Name);