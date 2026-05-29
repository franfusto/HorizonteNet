using System.Reflection;
using System.Runtime.Loader;
using Horizonte.Entities;
namespace Horizonte;

/// <summary>
/// Proporciona métodos y propiedades de ayuda para la gestión de ensamblados en el contexto de la plataforma Horizonte.
/// </summary>
public static class AssemblyHelpers
{
    /// <summary>
    /// <c>RuntimeAssemblyNames</c> es un conjunto de cadenas de solo lectura que representa
    /// los nombres de los ensamblados que son reconocidos como parte del entorno de ejecución.
    /// Este conjunto incluye nombres como "Horizonte", "System.Runtime", "System.Private.CoreLib",
    /// "mscorlib", "netstandard" y "Microsoft.CSharp".
    /// Se utiliza principalmente para determinar si un ensamblado forma parte del núcleo de
    /// la plataforma de ejecución o es un ensamblado de confianza, lo que afecta la manera
    /// en que se resuelven y cargan los ensamblados.
    /// </summary>
    public static readonly HashSet<string> RuntimeAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Horizonte",
        "System.Runtime",
        "System.Private.CoreLib",
        "mscorlib",
        "netstandard",
        "Microsoft.CSharp"
    };

    /// <summary>
    /// Determina si un ensamblado específico está presente en la lista de ensamblados de plataforma de confianza.
    /// </summary>
    /// <param name="assemblyName">El nombre del ensamblado que se desea verificar.</param>
    /// <returns>Devuelve <c>true</c> si el ensamblado está en la lista de ensamblados de plataforma de confianza; de lo contrario, devuelve <c>false</c>.</returns>
    public static bool IsTrustedPlatformAssembly(string assemblyName)
    {
        var tpa = AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") as string;
        if (string.IsNullOrWhiteSpace(tpa))
            return false;

        var paths = tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries);
        return paths.Any(path =>
            string.Equals(
                Path.GetFileNameWithoutExtension(path),
                assemblyName,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determina si se debe omitir la resolución de un paquete NuGet para un ensamblado específico.
    /// </summary>
    /// <param name="assemblyName">El nombre del ensamblado que se está evaluando para la resolución de NuGet.</param>
    /// <returns>Devuelve <c>true</c> si se debe omitir la resolución del ensamblado a través de NuGet; de lo contrario, devuelve <c>false</c>.</returns>
    public static bool ShouldSkipNuGetResolution(string assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            return true;

        if (RuntimeAssemblyNames.Contains(assemblyName))
            return true;

        if (IsTrustedPlatformAssembly(assemblyName))
            return true;

        return false;
    }

    /// <summary>
    /// Determina si un nombre de dominio específico corresponde a un dominio de scripts.
    /// </summary>
    /// <param name="domainName">El nombre del dominio que se desea verificar.</param>
    /// <returns>Devuelve <c>true</c> si el nombre del dominio coincide con el nombre del dominio de scripts predeterminado; de lo contrario, devuelve <c>false</c>.</returns>
    public static bool IsScriptDomain(string domainName)
    {
        return string.Equals(domainName, Const.ScriptDomainName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtiene el marco de trabajo solicitado por la aplicación en ejecución.
    /// </summary>
    /// <returns>Devuelve una cadena que representa el marco de trabajo en el formato abreviado, como "net5.0" o "netstandard2.1". Si no se puede determinar el marco de trabajo solicitado, devuelve el marco de trabajo predeterminado.</returns>
    public static string GetRequestedFramework()
    {
        var targetFramework = AppContext.TargetFrameworkName;

        if (string.IsNullOrEmpty(targetFramework))
        {
            return Const.DefaultFramework;
        }

        var parts = targetFramework.Split(',');
        if (parts.Length > 1 && parts[1].Trim().StartsWith("Version=v"))
        {
            var version = parts[1].Trim().Substring("Version=v".Length);

            if (parts[0].Contains(".NETCoreApp"))
            {
                return $"net{version}";
            }

            if (parts[0].Contains(".NETStandard"))
            {
                return $"netstandard{version}";
            }
        }

        return Const.DefaultFramework;
    }

    /// <summary>
    /// Analiza el nombre completo de un ensamblado y extrae su nombre y versión.
    /// </summary>
    /// <param name="assemblyFullName">El nombre completo del ensamblado que se desea analizar.</param>
    /// <returns>Una tupla que contiene el nombre y la versión del ensamblado. Si la versión no está especificada, el campo de la versión estará vacío.</returns>
    public static (string Name, string Version) ParseAssemblyName(string assemblyFullName)
    {
        string name = assemblyFullName.Split(',')[0].Trim();
        string version = string.Empty;
        var parts = assemblyFullName.Split(',');

        foreach (var part in parts)
        {
            if (part.Trim().StartsWith("Version="))
            {
                version = part.Trim().Substring("Version=".Length);
                break;
            }
        }

        return (name, version);
    }

    /// <summary>
    /// Obtiene el contexto de carga de ensamblados basado en el nombre del dominio especificado.
    /// Si el nombre del dominio es nulo, vacío o coincide con el nombre del dominio predeterminado,
    /// devuelve el contexto de carga de ensamblados predeterminado. Si el dominio existe en el
    /// diccionario, devuelve el contexto de carga asociado; de lo contrario, devuelve el contexto
    /// de carga de ensamblados predeterminado.
    /// </summary>
    /// <param name="domains">Un diccionario que mapea nombres de dominio a sus respectivos contextos de carga de ensamblados.</param>
    /// <param name="domainName">El nombre del dominio del cual obtener el contexto de carga de ensamblados.</param>
    /// <returns>Devuelve el contexto de carga de ensamblados asociado con el nombre del dominio especificado,
    /// o el contexto de carga de ensamblados predeterminado si el nombre del dominio no es válido o no se encuentra.</returns>
    public static AssemblyLoadContext GetAssemblyLoadContextByDomain(
        Dictionary<string, AssemblyLoadContext> domains, string? domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, Const.DefaultDomainName, StringComparison.OrdinalIgnoreCase))
        {
            return AssemblyLoadContext.Default;
        }

        if (domains.TryGetValue(domainName, out var alc))
        {
            return alc;
        }

        return AssemblyLoadContext.Default;
    }

    /// <summary>
    /// Obtiene el nombre del dominio asociado a un contexto de carga de ensamblados específico.
    /// </summary>
    /// <param name="domains">Un diccionario que mapea nombres de dominio a sus respectivos contextos de carga de ensamblados.</param>
    /// <param name="alc">El contexto de carga de ensamblados para el cual se quiere determinar el nombre del dominio.</param>
    /// <returns>El nombre del dominio si se encuentra en el diccionario; de lo contrario, el nombre del contexto proporcionado o "Default" si no está disponible.</returns>
    public static string GetDomainNameForAssemblyLoadContext(
        Dictionary<string, AssemblyLoadContext> domains, AssemblyLoadContext alc)
    {
        if (ReferenceEquals(alc, AssemblyLoadContext.Default))
            return Const.DefaultDomainName;

        foreach (var item in domains)
        {
            if (ReferenceEquals(item.Value, alc))
                return item.Key;
        }

        return alc.Name ?? Const.DefaultDomainName;
    }

    /// <summary>
    /// Determina si un ensamblado debe compartirse desde el contexto de carga de ensamblados predeterminado.
    /// </summary>
    /// <param name="assemblyName">El nombre del ensamblado que se va a evaluar.</param>
    /// <returns>Devuelve <c>true</c> si el ensamblado debe compartirse desde el contexto predeterminado; de lo contrario, devuelve <c>false</c>.</returns>
    public static bool ShouldShareAssemblyFromDefault(string? assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            return false;

        var coreAssemblyName = typeof(IhAssemblyManager).Assembly.GetName().Name;

        return string.Equals(assemblyName, coreAssemblyName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Intenta obtener un ensamblado compartido desde el contexto de carga predeterminado.
    /// </summary>
    /// <param name="assemblyName">El nombre del ensamblado que se desea cargar.</param>
    /// <returns>Devuelve una instancia de <see cref="Assembly"/> si el ensamblado compartido es encontrado en el contexto de carga predeterminado; de lo contrario, devuelve <c>null</c>.</returns>
    public static Assembly? TryGetSharedAssemblyFromDefault(AssemblyName assemblyName)
    {
        if (!AssemblyHelpers.ShouldShareAssemblyFromDefault(assemblyName.Name))
            return null;

        var sharedAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a =>
            string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));

        return sharedAssembly;
    }

    /// <summary>
    /// Intenta obtener el equivalente del tipo proporcionado en el dominio de ensamblaje predeterminado.
    /// </summary>
    /// <param name="type">El tipo cuyo equivalente en el dominio predeterminado se desea obtener.</param>
    /// <returns>Devuelve una instancia de <c>Type</c> que representa el tipo equivalente en el dominio de ensamblaje predeterminado, o <c>null</c> si no se encuentra un equivalente.</returns>
    public static Type? TryGetDefaultDomainTypeEquivalent(Type type)
    {
        var assemblyName = type.Assembly.GetName().Name;
        if (!ShouldShareAssemblyFromDefault(assemblyName))
            return null;

        return AssemblyLoadContext.Default.Assemblies
            .Where(a => string.Equals(a.GetName().Name, assemblyName, StringComparison.OrdinalIgnoreCase))
            .Select(a => a.GetType(type.FullName ?? string.Empty, throwOnError: false, ignoreCase: false))
            .FirstOrDefault(t => t != null);
    }

    /// <summary>
    /// Intenta crear un objeto <see cref="AssemblyName"/> para la resolución en tiempo de ejecución de un ensamblado a partir de su nombre completo.
    /// </summary>
    /// <param name="assemblyFullName">El nombre completo del ensamblado que se va a resolver.</param>
    /// <param name="assemblyName">Cuando este método regresa, contiene un objeto <see cref="AssemblyName"/> que representa el ensamblado, si la creación fue exitosa; de lo contrario, es <c>null</c>.</param>
    /// <returns>Devuelve <c>true</c> si se pudo crear el objeto <see cref="AssemblyName"/> correctamente; de lo contrario, devuelve <c>false</c>.</returns>
    public static bool TryCreateAssemblyNameForRuntimeResolution(
        string? assemblyFullName,
        out AssemblyName assemblyName)
    {
        assemblyName = null!;

        if (string.IsNullOrWhiteSpace(assemblyFullName))
            return false;

        var versionPart = assemblyFullName
            .Split(',', StringSplitOptions.TrimEntries)
            .FirstOrDefault(part => part.StartsWith("Version=", StringComparison.OrdinalIgnoreCase));

        if (versionPart != null)
        {
            var version = versionPart["Version=".Length..];

            if (version.Contains('-', StringComparison.Ordinal))
                return false;
        }

        try
        {
            assemblyName = new AssemblyName(assemblyFullName);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
}
 