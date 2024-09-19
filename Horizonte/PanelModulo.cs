using System.Diagnostics.Eventing.Reader;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace Horizonte;

public record RunningServiceInfo(string Typename, bool Isrunning,string Name);

[HorizonteModule("Horizonte")]
public class PanelModulo
{
    private IHorizonteEnv? _env;

    public PanelModulo(IHorizonteEnv env)
    {
        _env = env;
    }

    [HorizonteRole("init")]
    public bool Init()
    {
        return true;
    }



    [HorizonteCommand("Horizonte_Reboot")]
    public void Reboot()
    {
        _env?.Reboot();
    }
    
    [HorizonteCommand("Horizonte_Quit")]
    public void Quit()
    {
        _env?.Quit();
    }

    //Memory Log

    [HorizonteCommand("MemoryLog_IsEnabled")]
    public bool MemoryLog_IsEnabled()
    {
        var memap = GetMemoryAppender();
        return memap != null;
    }

    [HorizonteCommand("MemoryLog_Enable")]
    public void MemoryLog_Enable()
    {
        var appender = GetMemoryAppender();
        if (appender != null) return;

        MemoryAppender memory = new MemoryAppender();
        memory.ActivateOptions();
        ((Hierarchy)LogManager.GetRepository()).Root.AddAppender(memory);
        ((Hierarchy)LogManager.GetRepository()).RaiseConfigurationChanged(EventArgs.Empty);
    }

    [HorizonteCommand("MemoryLog_Disable")]
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

    [HorizonteCommand("MemoryLog_GetLog")]
    public LoggingEvent[] MemoryLog_GetLog()
    {
        var appender = GetMemoryAppender();
        if (appender != null) return appender.GetEvents();
        return Array.Empty<LoggingEvent>();
    }

    [HorizonteCommand("MemoryLog_Clear")]
    public void MemoryLog_Clear()
    {
        var appender = GetMemoryAppender();
        if (appender != null) appender.Clear();
    }

    private MemoryAppender? GetMemoryAppender()
    {
        return LogManager.GetRepository().GetAppenders().FirstOrDefault(x => x.GetType() == typeof(MemoryAppender)) as
            MemoryAppender;
    }
    


    //Workers
    
    [HorizonteCommand("Workers_GetServicesRunning")]
    public List<RunningServiceInfo> Workers_GetServicesRunning()
    {
        return _env?.HHost.Services.GetServices<BackgroundService>()
            .Select(service => new RunningServiceInfo(
                Typename: service.GetType().Name,
                Isrunning: IsRunning(service),
                Name: (service as IHservice)?.ServiceName ?? string.Empty ))
            .ToList() ?? new();
    }


    private bool IsRunning(BackgroundService service)
    {
        bool running ;
        if (service is IHservice)
            running = (bool)(service as IHservice)?.IsRunning;
        else
            running = false;
        return running;
    }

    [HorizonteCommand("Workers_StartService")]
    public void Workers_StartService(string servicename)
    {
        var service = (_env?.HHost.Services.GetServices<BackgroundService>() ??
                       Array.Empty<BackgroundService>()).FirstOrDefault(x =>
            x.GetType().Name == servicename);
        if(service==null) return;
        if(!IsRunning(service)) service.StartAsync(new());
    }

    [HorizonteCommand("Workers_StopService")]
    public void Workers_StopService(string servicename)
    {
        var service = (_env?.HHost.Services.GetServices<BackgroundService>() ??
                       Array.Empty<BackgroundService>()).FirstOrDefault(x =>
            x.GetType().Name == servicename);
        if(service==null) return;
        if(IsRunning(service)) service.StopAsync(new());
    }
    
    //contexto
    [HorizonteCommand("Context_GetJsonContent")]
    public string Context_GetJsonContent()
    {
        
        string filecontent = File.ReadAllText(_env!.Contextname+".json");
        object? jsonObject = JsonConvert.DeserializeObject(filecontent);
        string formattedJsonString = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
        return formattedJsonString;
    }
    
    [HorizonteCommand("Context_SetJsonContent")]
    public void Context_SetJsonContent(string content)
    {
        File.WriteAllText(_env!.Contextname+ ".json",content); 
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

}