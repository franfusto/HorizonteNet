using Horizonte;
using Horizonte.AdminUi.HorizonteSettings.Widgets;
using Microsoft.Extensions.Logging;
using Blazor.Diagrams;
namespace Horizonte.AdminUi.HorizonteSettings;

/// <summary>
/// La clase <c>PanelModulo</c> es responsable de inicializar y gestionar diferentes widgets y configuraciones en la aplicación.
/// </summary>
/// <remarks>
/// La clase se encuentra dentro del namespace <c>Horizonte.AdminUi.HorizonteSettings</c> y está decorada con el atributo <c>HorizonteModule</c>
/// para indicar que pertenece al módulo correspondiente de la aplicación.
/// </remarks>
[HorizonteModule("Horizonte.AdminUi.HorizonteSettings")]
public class PanelModulo
{
    private readonly ILogger<PanelModulo> _logger;

    /// <summary>
    /// La clase <c>PanelModulo</c> forma parte del namespace <c>Horizonte.AdminUi.HorizonteSettings</c> y está marcado con el atributo <c>[HorizonteModule("Horizonte.AdminUi.HorizonteSettings")]</c>.
    /// Esta clase se encarga de implementar varios métodos que devuelven objetos <c>WidgetDef</c>, los cuales representan diferentes widgets para administrar y visualizar configuraciones en la aplicación.
    /// También incluye un método para inicializar el módulo utilizando el atributo <c>[HorizonteRole("init")]</c>.
    /// </summary>
    public PanelModulo(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// El método <c>Init</c> forma parte de la clase <c>PanelModulo</c> en el namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Está marcado con los atributos <c>[HorizonteRole("init")]</c> y <c>[HorizonteCommand("HorizonteConfig_Init")]</c>.
    /// Este método se encarga de inicializar el módulo correspondiente a Horizonte.AdminUi.HorizonteSettings,
    /// lo que incluye la carga forzada del ensamblado <c>Blazor.Diagrams</c>.
    /// Además, genera un registro de información indicando que el módulo ha sido iniciado correctamente.
    /// <return>Devuelve un <c>bool</c> indicando el éxito de la operación, siempre retorna <c>true</c>.</return>
    /// </summary>
    [HorizonteRole("init")]
    [HorizonteCommand("HorizonteConfig_Init")]
    public bool Init()
    {
        _ = new BlazorDiagram();// forzamos la carga del ensamblado
        _logger.LogInformation("Módulo Horizonte.AdminUi.HorizonteSettings Iniciado");
        return true;
    }

    //widgets

    /// <summary>
    /// El método <c>Workers</c> está definido dentro de la clase <c>PanelModulo</c> en el namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Este método está marcado con los atributos <c>[HorizonteRole("widget")]</c> y <c>[HorizonteCommand("HorizonteConfig_Workers", "Este widget permite ver y descargar el contexto o archivo de configuración de la aplicación")]</c>.
    /// Su propósito es devolver un objeto <c>WidgetDef</c> que representa un widget utilizado para visualizar y manejar los servicios configurables dentro de la aplicación.
    /// El objeto <c>WidgetDef</c> devuelto tiene su propiedad <c>Type</c> asignada al tipo <c>Workers</c>, y los parámetros están inicialmente establecidos a nulo.
    /// </summary>
    /// <returns>Retorna un objeto <c>WidgetDef</c> configurado para el widget Representación de Trabajadores, que permite ver y descargar configuraciones relacionadas con los servicios gestionados por la aplicación.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Workers",
        "Este widget permite ver y descargar el contexto o archivo de configuración de la aplicación")]
    public WidgetDef Workers() => new WidgetDef() { Type = typeof(Workers), Parameters = null };

    /// <summary>
    /// El método <c>ViewContext</c> es parte de la clase <c>PanelModulo</c> dentro del namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Este método está decorado con los atributos <c>[HorizonteRole("widget")]</c> y <c>[HorizonteCommand("HorizonteConfig_ViewContext")]</c>,
    /// lo que sugiere que representa un widget en la aplicación.
    /// Su función principal es proporcionar un objeto <c>WidgetDef</c> que describe el contexto o archivo de configuración de la aplicación,
    /// facilitando así su visualización y descarga.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> que especifica el tipo <c>Context</c> y no define parámetros adicionales.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_ViewContext",
        "Este widget permite ver y descargar el contexto o archivo de configuración de la aplicación")]
    public WidgetDef ViewContext() => new WidgetDef() { Type = typeof(Context), Parameters = null };

    /// <summary>
    /// El método <c>Modules</c> está marcado con los atributos <c>[HorizonteRole("widget")]</c> y <c>[HorizonteCommand("HorizonteConfig_Modules", "")]</c>.
    /// Este método es parte del namespace <c>Horizonte.AdminUi.HorizonteSettings</c> dentro de la clase <c>PanelModulo</c>.
    /// Devuelve un objeto <c>WidgetDef</c> que representa un widget de tipo <c>Modules</c> sin parámetros adicionales.
    /// Es utilizado para configurar o visualizar los módulos de la aplicación.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> que define un widget del tipo <c>Modules</c>.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Modules", "")]
    public WidgetDef Modules() => new WidgetDef() { Type = typeof(Modules), Parameters = null };


    /// <summary>
    /// El método <c>Commands</c> es parte de la clase <c>PanelModulo</c>,
    /// ubicada en el namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Este método está anotado con el atributo <c>[HorizonteRole("widget")]</c> y el atributo <c>[HorizonteCommand("HorizonteConfig_Commands", "")]</c>.
    /// Su propósito es devolver una instancia de <c>WidgetDef</c> inicializada con el tipo <c>Commands</c> del widget,
    /// representando una definición de widget configurable en la aplicación.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> que representa un widget de comandos en la configuración de Horizonte.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Commands", "")]
    public WidgetDef Commands() => new WidgetDef() { Type = typeof(Commands), Parameters = null };

    /// <summary>
    /// El método <c>Log</c> forma parte de la clase <c>PanelModulo</c> dentro del namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Está marcado con los atributos <c>[HorizonteRole("widget")]</c> y <c>[HorizonteCommand("HorizonteConfig_Log", "este widget permite ver el registro de la aplicación")]</c>.
    /// Este método se encarga de crear un objeto <c>WidgetDef</c> que representa un widget especializado en mostrar el registro de eventos de la aplicación.
    /// Al ser ejecutado, inicializa un nuevo <c>WidgetDef</c> cuyo tipo se asigna al componente <c>Log</c>, permitiendo la visualización y administración de los registros.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> configurado para mostrar el registro de la aplicación.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Log", "este widget permite ver el registro de la aplicación")]
    public WidgetDef Log() => new WidgetDef() { Type = typeof(Log), Parameters = null };

    /// <summary>
    /// El método <c>AssembliesMap</c> es parte de la clase <c>PanelModulo</c> dentro del namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Este método está decorado con los atributos <c>[HorizonteRole("widget")]</c> y <c>[HorizonteCommand("HorizonteConfig_AssembliesMap", "")]</c>.
    /// Su principal responsabilidad es devolver un objeto <c>WidgetDef</c> que representa un widget asociado al tipo <c>AssembliesMap</c>.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> que contiene la información del tipo asociado al widget <c>AssembliesMap</c>.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_AssembliesMap", "")]
    public WidgetDef AssembliesMap() => new WidgetDef() { Type = typeof(AssembliesMap), Parameters = null };    
    
    //config


    /// <summary>
    /// El método <c>WorkersConfig</c> está definido en la clase <c>PanelModulo</c> dentro del namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Este método está anotado con el atributo <c>[HorizonteRole("configpage")]</c> y <c>[HorizonteCommand("HorizonteConfig_WorkersConfig", "")]</c>.
    /// El método se encarga de devolver un objeto de tipo <c>WidgetDef</c> cuya propiedad <c>Type</c> está establecida en <c>typeof(WorkersConfig)</c>.
    /// Es parte del sistema de configuración y administración de la aplicación.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> configurado para representar el widget de configuración de trabajadores.</returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteConfig_WorkersConfig", "")]
    public WidgetDef WorkersConfig() => new WidgetDef() { Type = typeof(WorkersConfig), Parameters = null };

    /// <summary>
    /// El método <c>CredentialsManager</c> forma parte de la clase <c>PanelModulo</c> en el namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Está decorado con los atributos <c>[HorizonteRole("configpage")]</c> y <c>[HorizonteCommand("HorizonteConfig_CredentialsManager", "")]</c>.
    /// Este método devuelve un objeto <c>WidgetDef</c>, el cual está configurado para utilizar el tipo <c>CredentialsManager</c>.
    /// Se utiliza principalmente para gestionar y visualizar configuraciones relacionadas con las credenciales en la aplicación.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> que representa el widget del administrador de credenciales.</returns>
    [HorizonteRole("configpage")]
    [HorizonteCommand("HorizonteConfig_CredentialsManager", "")]
    public WidgetDef CredentialsManager() => new WidgetDef() { Type = typeof(CredentialsManager), Parameters = null };

    /// <summary>
    /// El método <c>AssembliesViewer</c> forma parte de la clase <c>PanelModulo</c> en el namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Está decorado con el atributo <c>[HorizonteCommand("HorizonteConfig_AssembliesViewer")]</c>, lo que sugiere que este método es responsable de proporcionar un widget relacionado con la visualización de ensamblados en la aplicación.
    /// Devuelve un objeto <c>WidgetDef</c> con el tipo asociado <c>AssembliesWidget</c>, lo que facilita la generación y manejo de la interfaz de usuario para esta funcionalidad específica.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> que representa la configuración necesaria para el widget de visualización de ensamblados.</returns>
    [HorizonteCommand("HorizonteConfig_AssembliesViewer", "")]
    public WidgetDef AssembliesViewer() => new WidgetDef() { Type = typeof(AssembliesWidget), Parameters = null };


    /// <summary>
    /// El método <c>AppFilesViewer</c> forma parte de la clase <c>PanelModulo</c> en el namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Está anotado con el atributo <c>[HorizonteCommand("HorizonteConfig_AppFilesViewer", "")]</c>, lo que sugiere que se utiliza para manejar comandos o interacciones configuradas.
    /// Este método devuelve un objeto <c>WidgetDef</c>, que representa un widget específico de tipo <c>AppFilesViewer</c>.
    /// No define parámetros, lo que indica que es un método sencillo para obtener y posiblemente visualizar la información relacionada con los archivos de la aplicación.
    /// <return>Un objeto <c>WidgetDef</c> configurado para la visualización de archivos de la aplicación.</return>
    /// </summary>
    [HorizonteCommand("HorizonteConfig_AppFilesViewer", "")]
    public WidgetDef AppFilesViewer() => new WidgetDef() { Type = typeof(AppFilesViewer), Parameters = null };

    /// <summary>
    /// El método <c>Assemblies</c> está diseñado para devolver un objeto <c>WidgetDef</c> que representa un widget específico dentro del módulo de configuración de <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Está anotado con el atributo <c>[HorizonteRole("widget")]</c>, indicando que es un widget, y el atributo <c>[HorizonteCommand("HorizonteConfig_Assemblies", "este widget permite ver los ensamblados cargados y su mapa de dependencias")]</c>,
    /// que proporciona una descripción clara de su funcionalidad. Este widget es particularmente útil para visualizar los ensamblados cargados en la aplicación y su mapa de dependencias, lo que puede ser esencial para el seguimiento de cómo los diferentes componentes de la aplicación están interrelacionados.
    /// </summary>
    /// <returns>Un objeto <c>WidgetDef</c> que representa el widget para ver ensamblados y su mapa de dependencias.</returns>
    [HorizonteRole("widget")]
    [HorizonteCommand("HorizonteConfig_Assemblies", "este widget permite ver los ensamblados cargados y su mapa de dependencias")]
    public WidgetDef Assemblies() => new WidgetDef() { Type = typeof(AssembliesWidget), Parameters = null };

    /// <summary>
    /// El método <c>HostInfo</c> forma parte de la clase <c>PanelModulo</c> dentro del namespace <c>Horizonte.AdminUi.HorizonteSettings</c>.
    /// Está decorado con el atributo <c>[HorizonteCommand("HorizonteConfig_HostInfo")]</c>.
    /// Este método devuelve un objeto <c>WidgetDef</c> que representa información relacionada con el host en forma de widget.
    /// El objeto <c>WidgetDef</c> generado tiene como tipo al tipo <c>HostInfo</c>.
    /// </summary>
    /// <returns>Un objeto de tipo <c>WidgetDef</c> configurado para representar información del host.</returns>
    [HorizonteCommand("HorizonteConfig_HostInfo", "")]
    public WidgetDef HostInfo() => new WidgetDef() { Type = typeof(HostInfo), Parameters = null };
           
    
    
}