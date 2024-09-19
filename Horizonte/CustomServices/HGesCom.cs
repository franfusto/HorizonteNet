using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;

namespace Horizonte;

public class HGesCom : IHGesCom
{
    private Dictionary<string, HCommand> CommandList = new();
    private Lazy<IHorizonteEnv> _env = new();
    public record RoleListItem(string CommandName, string Description);
    private ILogger? _log;

    public HGesCom(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
        LoadModules();
    }

    public void InitzializeModules()
    {
        _log = _env.Value.HHost.Services.GetService<ILogger<HGesCom>>();
        foreach (var item in GetRoleCommands("init"))
        {
            RunCommand(item.CommandName);
        }
        _log?.LogInformation("Módulos iniciados");
    }

    public List<RoleListItem> GetRoleCommands(string role)
    {
        return CommandList
            .Where(x => x.Value.Roles.Contains(role))
            .Select(item => new RoleListItem(item.Value.CommandKey, item.Value.Description))
            .ToList();
    }

    private void LoadModules()
    {
        try
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            IHorizonteEnv env = _env.Value;

            //llenamos diccionario de commandos
            foreach (var assembly in assemblies)
            {
                var modulostypes = from type in assembly.GetTypes()
                    where Attribute.IsDefined(type, typeof(HorizonteModule))
                    select type;

                foreach (var modtype in modulostypes)
                {
                    try
                    {
                        if (modtype == null) continue;
                        object? modInstance;
                        try
                        {
                            modInstance = Activator.CreateInstance(modtype, env);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("Error al crear instancia: " + modtype.FullName);
                            continue;
                        }

                        var metodos = modtype.GetMethods().Where(t => t.IsDefined(typeof(HorizonteCommand)));
                        foreach (var method in metodos)
                        {
                            if (modInstance != null)
                            {
                                AddCommandToList(method, modInstance);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _log?.LogError(e.ToString());
                    }
                }
            }

            _log?.LogInformation($"Loaded {CommandList.Count} HCommands");
            
        }
        catch (Exception e)
        {
            _log?.LogError(e.ToString());
        }
    }


    private void AddCommandToList(MethodInfo method, object instance)
    {
        var hAttrib = method.CustomAttributes.First(t =>
            t.AttributeType == typeof(HorizonteCommand));
        var roleAttrib = (method.GetCustomAttributes(typeof(HorizonteRole), false)
            as HorizonteRole[] ?? Array.Empty<HorizonteRole>()).ToList().Select(x => x.Role).ToList();

        HCommand miCmd = new HCommand();
        miCmd.CommandKey = hAttrib.ConstructorArguments[0].Value!.ToString()!;
        miCmd.Description = hAttrib.ConstructorArguments[1].Value?.ToString() ?? string.Empty;
        ;
        miCmd.CommandAction = method;
        miCmd.Instance = instance;
        miCmd.InTypes = method.GetParameters().Select(p => p.ParameterType).ToList();
        miCmd.InNames = GetParameterNames(method);
        miCmd.OutType = method.ReturnType;
        miCmd.Roles = roleAttrib;
        CommandList.Add(miCmd.CommandKey, miCmd);
    }


    private List<string>? GetParameterNames(MethodInfo methodInfo)
    {
        return methodInfo.GetParameters()
            .Where(x => x.Name != null)
            .Select(parameter => parameter.Name!)
            .ToList();
    }

    public string? RunCommandJson(string commandKey, string[] jsonarglist)
    {
        try
        {
            object?[]? inobjparams = null;

            HCommand? method = GetHCommand(commandKey);
            if (method == null) return null;

            var intypes = method.InTypes?.ToArray();
            if (intypes != null && intypes.Length != 0)
            {
                List<object?> tmpobjlst = new List<object?>();
                for (int i = 0; i < intypes.Count(); i++)
                {
                    Type? T = intypes![i];
                    object? serob = JsonConvert.DeserializeObject(jsonarglist[i], T);
                    tmpobjlst.Add(serob);
                }

                inobjparams = tmpobjlst.ToArray();
            }

            object? resobj = RunCommand(method.CommandKey, inobjparams!);
            return JsonConvert.SerializeObject(resobj);
        }
        catch (Exception e)
        {
            _log?.LogError(e.ToString());
            return null;
        }
    }

    public object? RunCommand(string commandKeyor, object[]? arg = null)
    {
        return RunCommand<object>(commandKeyor, arg);
    }

    public T? RunCommand<T>(string commandKeyor, object[]? arg = null)
    {
        try
        {
            commandKeyor = commandKeyor.Trim();

            // recuperamos del diccionario el comando correspondiente al CommandKey
            HCommand rCommand = (HCommand)CommandList[commandKeyor];


            // invocamos el método de la clase Panel con los argumentos
            if (rCommand.CommandAction != null)
            {
                var resObject = rCommand.CommandAction.Invoke(rCommand.Instance, arg);
                if (typeof(T) != typeof(object))
                    resObject = Convert.ChangeType(resObject, typeof(T));
                //devolvemos el objeto
                return (T)resObject!;
            }

            return default(T);
        }
        catch (Exception ex)
        {
            _log?.LogError($"**** {commandKeyor} ****" + ex.ToString());
            return default(T);
        }
    }

    public HCommand? GetHCommand(string commandKey)
    {
        CommandList.TryGetValue(commandKey, out HCommand? rCommand);
        return rCommand;
    }

    public IEnumerable<HCommand> GetCommandList()
    {
        return CommandList.Values.ToList();
    }
}