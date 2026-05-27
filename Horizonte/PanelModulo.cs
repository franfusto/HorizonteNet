using System.Diagnostics.Eventing.Reader;
using System.Reflection;
using System.Text.Json;
using Horizonte.Entities;
using Horizonte.Interfaces;
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
    private readonly IHGesCom _gesCom;
    private readonly ILogger<PanelModulo> _logger;
    private IServiceProvider _serviceProvider;
    private IAppManager _appManager;
    private IhWorkersManager _workersManager;
    private IhAssemblyManager _assemblyManager;

    /// <summary>
    /// Represent un panel de módulo en el framework Horizonte que funciona como una interfaz 
    /// entre un módulo y el entorno modular. Expone las funcionalidades del módulo 
    /// como comandos (HorizonteCommand), cada uno de los cuales tiene una clave única y, opcionalmente, 
    /// una descripción.
    /// </summary>
    public PanelModulo(IServiceProvider serviceProvider,
        IhAssemblyManager assemblyManager,
        IAppManager appManager,
        IhWorkersManager workersManager,
        IHGesCom gesCom,
        ILogger<PanelModulo> logger)
    {
        _serviceProvider = serviceProvider;
        _gesCom = gesCom;
        _logger = logger;
        _appManager =  appManager;
        _workersManager = workersManager;
        _assemblyManager = assemblyManager;
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


/*
    [HorizonteCommand("Horizonte_AssetsMap")]
    public List<string> AssetsMap()
    {
        List<string> list = _assemblyManager.StaticFileRegistry.Files
            .OrderBy(x => x.Key)
            .Select(x => $"{x.Key} >> {x.Value}")
            .ToList();
        return list;
    }
    */
    
    //App

    /// <summary>
    /// Reinicia el sistema mediante la interacción con el entorno Horizonte.
    /// Este comando está asociado a la clave "Horizonte_Reboot".
    /// </summary>
    [HorizonteCommand("Horizonte_Reboot", Description = "Reinicia el sistema")]
    public void Reboot()
    {
        _appManager.Reboot();
    }

    /// <summary>
    /// Cierra el sistema invocando la función Quit en el entorno Horizonte asociado.
    /// Este método se expone como un comando Horizonte con la clave única "Horizonte_Quit" y una descripción "Cierra el sistema".
    /// </summary>
    [HorizonteCommand("Horizonte_Quit", description: "Cierra el sistema")]
    public void Quit()
    {
        _appManager.Quit();
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
    /// Obtiene la lista de Workers disponibles en el sistema.
    /// Este método interactúa con el gestor de Workers para recuperar todos los Workers que están
    /// actualmente disponibles para su uso.
    /// </summary>
    /// <returns>Una lista de objetos de tipo WorkerDefItem, cada uno representando un Worker disponible.</returns>
    [HorizonteCommand("Workers_GetAvailableWorkers", "Obtiene la lista de Workers disponibles")]
    public List<WorkerDefItem> Workers_GetAvailableWorkers()
    {
        return _workersManager.GetAvailableWorkers();
    }

    /// <summary>
    /// Obtiene los tipos de los BackgroundServices registrados en el sistema, explorando
    /// todos los ensamblados disponibles y filtrando aquellos que son instancias de
    /// BackgroundService y no son interfaces o clases abstractas.
    /// </summary>
    /// <returns>
    /// Una lista de nombres completos de los tipos que implementan BackgroundService.
    /// </returns>
    [HorizonteCommand("Workers_GetAvailablesBackServices", "Obtiene los Tipos de los BackgroundService registrados en el sistema")]
    public List<string> Workers_GetAvailablesServices()
    {
        var assemblymanager = _serviceProvider.GetService<IhAssemblyManager>();
        if (assemblymanager == null) return new List<string>();

        var types = assemblymanager.Assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => typeof(BackgroundService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
            .Select(t => t.FullName)
            .OfType<string>()
            .ToList();

        return types;
    }

    /// <summary>
    /// Determina si un servicio específico está actualmente en ejecución dentro del sistema.
    /// </summary>
    /// <param name="servicename">El nombre del servicio para el cual se desea verificar el estado de ejecución.</param>
    /// <returns>Devuelve <c>true</c> si el servicio está en ejecución; de lo contrario, devuelve <c>false</c>.</returns>
    [HorizonteCommand("Workers_IsRunning", "Obtiene el estado de ejecución de un servico")]
    public bool IsRunning(string servicename)
    {
        return _workersManager.WorkerIsRunning(servicename);
    }


    /// <summary>
    /// Inicia un servicio especificado utilizando el nombre del servicio.
    /// Se invoca al administrador de trabajadores para realizar la acción de inicio.
    /// </summary>
    /// <param name="servicename">El nombre del servicio que se desea iniciar.</param>
    /// <returns>Retorna un valor booleano indicando si el servicio se inició correctamente o no.</returns>
    [HorizonteCommand("Workers_StartService", "Inicia un servicio")]
    public bool Workers_StartService(string servicename)
    {
        return _workersManager.StartWorker(servicename);
    }


    /// <summary>
    /// Detiene un servicio en ejecución identificado por su nombre.
    /// </summary>
    /// <param name="servicename">Nombre del servicio que se desea detener.</param>
    /// <return>Devuelve un valor booleano que indica si el servicio fue detenido con éxito.</return>
    [HorizonteCommand("Workers_StopService", "Detiene un servicio")]
    public bool Workers_StopService(string servicename)
    {
       
        return _workersManager.StopWorker(servicename);
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
        //string fileContent = File.ReadAllText(_env!.Contextname + ".json"); // TODO: 
        string fileContent = File.ReadAllText("horizonte.json"); // TODO: 
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
        File.WriteAllText("horizonte.json", content); //todo
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
                string line = assembly.Location + "=>  " + assembly.FullName;
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