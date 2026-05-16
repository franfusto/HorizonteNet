namespace Horizonte;

public static class AssemblyHelpers
{
    public static readonly HashSet<string> RuntimeAssemblyNames = new(StringComparer.OrdinalIgnoreCase)
    {
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
}