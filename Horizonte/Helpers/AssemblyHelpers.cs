using System.Reflection;
using System.Runtime.Loader;
using Horizonte.Entities;
namespace Horizonte;

public static class AssemblyHelpers
{
    public static readonly HashSet<string> RuntimeAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Horizonte",
        "System.Runtime",
        "System.Private.CoreLib",
        "mscorlib",
        "netstandard",
        "Microsoft.CSharp"
    };

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

    public static bool IsScriptDomain(string domainName)
    {
        return string.Equals(domainName, Const.ScriptDomainName, StringComparison.OrdinalIgnoreCase);
    }

    public static string GetRequestedFramework()
    {
        var targetFramework = AppContext.TargetFrameworkName;

        if (string.IsNullOrEmpty(targetFramework))
        {
            return "net10.0";
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

        return "net10.0";
    }

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

    public static AssemblyLoadContext GetAssemblyLoadContextByDomain(
        Dictionary<string, AssemblyLoadContext> domains, string? domainName)
    {
        if (string.IsNullOrWhiteSpace(domainName) ||
            string.Equals(domainName, "Default", StringComparison.OrdinalIgnoreCase))
        {
            return AssemblyLoadContext.Default;
        }

        if (domains.TryGetValue(domainName, out var alc))
        {
            return alc;
        }

        return AssemblyLoadContext.Default;
    }

    public static string GetDomainNameForAssemblyLoadContext(
        Dictionary<string, AssemblyLoadContext> domains, AssemblyLoadContext alc)
    {
        if (ReferenceEquals(alc, AssemblyLoadContext.Default))
            return "Default";

        foreach (var item in domains)
        {
            if (ReferenceEquals(item.Value, alc))
                return item.Key;
        }

        return alc.Name ?? "Default";
    }

    public static bool ShouldShareAssemblyFromDefault(string? assemblyName)
    {
        if (string.IsNullOrWhiteSpace(assemblyName))
            return false;

        var coreAssemblyName = typeof(IhAssemblyManager).Assembly.GetName().Name;

        return string.Equals(assemblyName, coreAssemblyName, StringComparison.OrdinalIgnoreCase);
    }

    public static Assembly? TryGetSharedAssemblyFromDefault(AssemblyName assemblyName)
    {
        if (!AssemblyHelpers.ShouldShareAssemblyFromDefault(assemblyName.Name))
            return null;

        var sharedAssembly = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a =>
            string.Equals(a.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase));

        return sharedAssembly;
    }
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
}
 