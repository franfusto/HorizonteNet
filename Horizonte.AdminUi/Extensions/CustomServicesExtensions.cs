using System.Reflection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Horizonte.AdminUi.Extensions;

public static class CustomServicesExtensions
{
    public static bool IsCompletionActive { get; private set; } = true;

    public static void StartCompletion()
    {
        IsCompletionActive = true;
    }

    public static void StopCompletion()
    {
        IsCompletionActive = false;
    }

    public static void ConfigureCustomService(this IServiceCollection services, CustomServicesConfig? config, ILogger? logger = null)
    {
        if(config == null) config = new CustomServicesConfig();
        
        Helpers.SetUpHarcodedServices(config);
        if (config.Services.Count == 0)
        {
            return;
        }

        foreach (var serviceConfig in config.Services)
        {
            if (string.IsNullOrWhiteSpace(serviceConfig.ServiceType) ||
                string.IsNullOrWhiteSpace(serviceConfig.ImplementationType))
            {
                continue;
            }

            logger?.LogInformation("Solicitando cargar servicio desde configuración: ServiceType='{ServiceType}', ImplementationType='{ImplementationType}'",
                serviceConfig.ServiceType, serviceConfig.ImplementationType);

            try
            {
                var serviceType = ResolveType(serviceConfig.ServiceType);
                var implementationType = ResolveType(serviceConfig.ImplementationType);

                if (serviceType == null)
                {
                    var msg = $"No se pudo resolver el tipo de servicio '{serviceConfig.ServiceType}'.";
                    logger?.LogError("Error al cargar servicio desde configuración: {Message}", msg);
                    throw new InvalidOperationException(msg);
                }

                if (implementationType == null)
                {
                    var msg = $"No se pudo resolver el tipo de implementación '{serviceConfig.ImplementationType}'.";
                    logger?.LogError("Error al cargar servicio desde configuración: {Message}", msg);
                    throw new InvalidOperationException(msg);
                }

                if (!serviceType.IsAssignableFrom(implementationType))
                {
                    var msg = $"El tipo '{implementationType.FullName}' no implementa o hereda de '{serviceType.FullName}'.";
                    logger?.LogError("Error al cargar servicio desde configuración: {Message}", msg);
                    throw new InvalidOperationException(msg);
                }

                var lifetime = serviceConfig.Lifetime switch
                {
                    CustomServiceLifetime.Singleton => ServiceLifetime.Singleton,
                    CustomServiceLifetime.Scoped => ServiceLifetime.Scoped,
                    CustomServiceLifetime.Transient => ServiceLifetime.Transient,
                    _ => ServiceLifetime.Scoped
                };

                services.TryAdd(new ServiceDescriptor(serviceType, implementationType, lifetime));
                logger?.LogInformation("Servicio '{ServiceType}' cargado correctamente con implementación '{ImplementationType}' ({Lifetime}).",
                    serviceConfig.ServiceType, serviceConfig.ImplementationType, lifetime);
            }
            catch (Exception ex) //when (ex is not InvalidOperationException)
            {
                logger?.LogError(ex, "Error al cargar servicio '{ServiceType}' desde configuración.", serviceConfig.ServiceType);
            }
        }
    }

    
    
    private static Type? ResolveType(string typeName)
    {
        var type = Type.GetType(typeName, throwOnError: false, ignoreCase: false);

        if (type != null)
        {
            return type;
        }

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            type = assembly.GetType(typeName, throwOnError: false, ignoreCase: false);

            if (type != null)
            {
                return type;
            }
        }

        var assemblyName = GetAssemblyName(typeName);

        if (string.IsNullOrWhiteSpace(assemblyName))
        {
            return null;
        }

        try
        {
            var assembly = Assembly.Load(assemblyName);
            return assembly.GetType(GetTypeFullName(typeName), throwOnError: false, ignoreCase: false);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetAssemblyName(string typeName)
    {
        var parts = typeName.Split(',', 2, StringSplitOptions.TrimEntries);
        return parts.Length == 2 ? parts[1] : null;
    }

    private static string GetTypeFullName(string typeName)
    {
        var parts = typeName.Split(',', 2, StringSplitOptions.TrimEntries);
        return parts[0];
    }
}