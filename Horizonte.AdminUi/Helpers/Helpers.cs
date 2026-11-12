using System.Reflection;
using Horizonte.AdminUi.Attributes;
using Horizonte.AdminUi.Extensions;

namespace Horizonte.AdminUi;

/// <summary>
/// Proporciona métodos auxiliares para el descubrimiento y configuración de servicios en AdminUI.
/// </summary>
public static class Helpers
{
    /// <summary>
    /// Examina los tipos cargados en los ensamblados del dominio actual en busca de campos estáticos
    /// decorados con <see cref="AdminUiCustomServiceDefinition"/> y los añade a la configuración de servicios.
    /// </summary>
    /// <param name="config">Instancia de <see cref="CustomServicesConfig"/> donde se añadirán los servicios encontrados.</param>
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