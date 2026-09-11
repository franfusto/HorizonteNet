// #sdk Cake.Sdk@6.0.0

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");
var version = Argument("packageVersion", "10.0.0-beta");
var promptPackageVersion = Argument("promptPackageVersion", true);
var useProjectReferences = Argument("useProjectReferences", false);
var promptUseProjectReferences = Argument("promptUseProjectReferences", true);
var nugetApiKey = Argument("nugetApiKey", EnvironmentVariable("NUGET_API_KEY") ?? "");
var promptNugetApiKey = Argument("promptNugetApiKey", true);
var nugetSource = Argument("nugetSource", "local"); // "local" o "nuget.org"
var promptNugetSource = Argument("promptNugetSource", true);

if (promptPackageVersion && !HasArgument("packageVersion") && ShouldPromptForPackageVersion(target))
{
    version = PromptForPackageVersion(version);
}
if (promptUseProjectReferences && !HasArgument("useProjectReferences") && ShouldPromptForUseProjectReferences(target))
{
    useProjectReferences = PromptForUseProjectReferences(useProjectReferences);
}

if (promptNugetSource && !HasArgument("nugetSource") && ShouldPromptForNugetSource(target))
{
    nugetSource = PromptForNugetSource(nugetSource);
}

if (promptNugetApiKey && !HasArgument("nugetApiKey") && ShouldPromptForNugetApiKey(target, nugetSource))
{
    nugetApiKey = PromptForNugetApiKey(nugetApiKey);
}

var solution = "./HorizonteNet.sln";
var artifactsDir = Directory("./artifacts");

bool ShouldPromptForPackageVersion(string targetName)
{
    return targetName.Equals("Build", StringComparison.OrdinalIgnoreCase)
        || targetName.Equals("Test", StringComparison.OrdinalIgnoreCase)
        || targetName.Equals("Pack", StringComparison.OrdinalIgnoreCase)
        || targetName.Equals("Push", StringComparison.OrdinalIgnoreCase);
}

bool ShouldPromptForNugetSource(string targetName)
{
    return targetName.Equals("Push", StringComparison.OrdinalIgnoreCase);
}
bool ShouldPromptForNugetApiKey(string targetName, string currentNugetSource)
{
    return targetName.Equals("Push", StringComparison.OrdinalIgnoreCase)
           && currentNugetSource.Equals("nuget.org", StringComparison.OrdinalIgnoreCase);
}
bool ShouldPromptForUseProjectReferences(string targetName)
{
    return targetName.Equals("Build", StringComparison.OrdinalIgnoreCase)
           || targetName.Equals("Test", StringComparison.OrdinalIgnoreCase)
           || targetName.Equals("Pack", StringComparison.OrdinalIgnoreCase)
           || targetName.Equals("Push", StringComparison.OrdinalIgnoreCase);
}
string PromptForPackageVersion(string currentVersion)
{
    Console.WriteLine($"PackageVersion actual: {currentVersion}");
    Console.Write("Nueva PackageVersion, Enter para mantener la actual: ");

    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine($"Se mantiene PackageVersion: {currentVersion}");
        return currentVersion;
    }

    Console.WriteLine($"Nueva PackageVersion: {input}");
    return input;
}
bool PromptForUseProjectReferences(bool currentValue)
{
    Console.WriteLine();
    Console.WriteLine($"Usar referencias de proyecto actual: {currentValue}");
    Console.Write("¿Usar referencias de proyecto? [s/N], Enter para mantener el valor actual: ");

    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input))
    {
        Console.WriteLine($"Usar referencias de proyecto: {currentValue}");
        return currentValue;
    }

    if (input.Equals("s", StringComparison.OrdinalIgnoreCase)
        || input.Equals("si", StringComparison.OrdinalIgnoreCase)
        || input.Equals("sí", StringComparison.OrdinalIgnoreCase)
        || input.Equals("y", StringComparison.OrdinalIgnoreCase)
        || input.Equals("yes", StringComparison.OrdinalIgnoreCase)
        || input.Equals("true", StringComparison.OrdinalIgnoreCase)
        || input.Equals("1", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Usar referencias de proyecto: true");
        return true;
    }

    if (input.Equals("n", StringComparison.OrdinalIgnoreCase)
        || input.Equals("no", StringComparison.OrdinalIgnoreCase)
        || input.Equals("false", StringComparison.OrdinalIgnoreCase)
        || input.Equals("0", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Usar referencias de proyecto: false");
        return false;
    }

    Console.WriteLine($"Valor no válido. Se usará el valor actual: {currentValue}.");
    return currentValue;
}

string PromptForNugetSource(string currentSource)
{
    var options = new[]
    {
        "local",
        "nuget.org"
    };

    var defaultIndex = Array.FindIndex(
        options,
        x => x.Equals(currentSource, StringComparison.OrdinalIgnoreCase));

    if (defaultIndex < 0)
    {
        defaultIndex = 0;
    }

    Console.WriteLine();
    Console.WriteLine($"NuGet source actual: {options[defaultIndex]}");
    Console.WriteLine("Selecciona NuGet source:");

    for (var i = 0; i < options.Length; i++)
    {
        var defaultMarker = i == defaultIndex ? " [predeterminado]" : "";
        Console.WriteLine($"{i + 1}) {options[i]}{defaultMarker}");
    }

    while (true)
    {
        Console.Write($"Opción, Enter para usar '{options[defaultIndex]}': ");
        var input = Console.ReadLine()?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine($"NuGet source seleccionado: {options[defaultIndex]}");
            return options[defaultIndex];
        }

        if (int.TryParse(input, out var selectedOption)
            && selectedOption >= 1
            && selectedOption <= options.Length)
        {
            var selectedSource = options[selectedOption - 1];
            Console.WriteLine($"NuGet source seleccionado: {selectedSource}");
            return selectedSource;
        }

        Console.WriteLine("Opción no válida. Introduce 1, 2 o pulsa Enter.");
    }
}

string PromptForNugetApiKey(string currentApiKey)
{
    Console.WriteLine();
    if (!string.IsNullOrWhiteSpace(currentApiKey))
    {
        Console.WriteLine("NuGet API Key actual: [configurada]");
        Console.Write("Nueva NuGet API Key, Enter para mantener la actual: ");
    }
    else
    {
        Console.Write("Introduce la NuGet API Key: ");
    }

    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrWhiteSpace(input))
    {
        return currentApiKey;
    }

    return input;
}

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
    .WithCriteria(c => HasArgument("rebuild"))
    .Does(() =>
{
    CleanDirectory(artifactsDir);
    DotNetClean(solution, new DotNetCleanSettings
    {
        Configuration = configuration,
    });
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        DotNetBuild(solution, new DotNetBuildSettings
        {
            Configuration = configuration,
            ArgumentCustomization = args => args
                .Append($"/p:Version={version}")
                .Append($"/p:InternalPackageVersion={version}")
                .Append($"/p:UseProjectReferences={useProjectReferences.ToString().ToLowerInvariant()}")
        });
    });
Task("Test")
    .IsDependentOn("Build")
    .Does(() =>
    {
        DotNetTest(solution, new DotNetTestSettings
        {
            Configuration = configuration,
            NoBuild = true,
            ArgumentCustomization = args => args
                .Append($"/p:InternalPackageVersion={version}")
                .Append($"/p:UseProjectReferences={useProjectReferences.ToString().ToLowerInvariant()}")
        });
    });

Task("Pack")
    .IsDependentOn("Test")
    .Does(() =>
    {
        var settings = new DotNetPackSettings
        {
            Configuration = configuration,
            OutputDirectory = artifactsDir,
            NoBuild = true,
            ArgumentCustomization = args => args
                .Append($"/p:Version={version}")
                .Append($"/p:InternalPackageVersion={version}")
                .Append($"/p:UseProjectReferences={useProjectReferences.ToString().ToLowerInvariant()}")
        };

        DotNetPack(solution, settings);
    });
Task("Push")
    .IsDependentOn("Pack")
    .Does(() =>
    {
        var url = nugetSource == "nuget.org"
            ? "https://api.nuget.org/v3/index.json"
            : "http://localhost:5555/v3/index.json";

        var packages = GetFiles($"{artifactsDir}/*.nupkg");

        foreach (var package in packages)
        {
            Information($"Pushing {package.GetFilename()} to {url}...");

            var settings = new DotNetNuGetPushSettings
            {
                Source = url,
                ApiKey = string.IsNullOrWhiteSpace(nugetApiKey) ? null : nugetApiKey,
                SkipDuplicate = true
            };

            DotNetNuGetPush(package, settings);
        }
    });

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);