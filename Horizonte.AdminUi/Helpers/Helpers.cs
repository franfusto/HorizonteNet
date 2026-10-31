using System.Reflection;
using Horizonte.AdminUi.Attributes;
using Horizonte.AdminUi.Extensions;

namespace Horizonte.AdminUi;

public static class Helpers
{
    
    
    public static void SetUpHarcodedServices(CustomServicesConfig config)
    {
        var fields = AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(GetLoadableTypes)
            .SelectMany(type => type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
            .Where(field =>
                field.FieldType == typeof(CustomServiceConfig) &&
                field.GetCustomAttribute<AdminUiCustomServiceDefinition>() != null);

        foreach (var field in fields)
        {
            if (field.GetValue(null) is not CustomServiceConfig serviceConfig)
            {
                continue;
            }

            config.Services.Add(new CustomServiceConfig
            {
                ServiceType = serviceConfig.ServiceType,
                ImplementationType = serviceConfig.ImplementationType,
                Lifetime = serviceConfig.Lifetime
            });
        }
    }

    private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
    }
}