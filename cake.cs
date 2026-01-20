// #sdk Cake.Sdk@6.0.0

var target = Argument("target", "Test");
var configuration = Argument("configuration", "Release");
var version = Argument("packageVersion", "10.0.0-beta");
var nugetApiKey = Argument("nugetApiKey", "");
var nugetSource = Argument("nugetSource", "local"); // "local" o "nuget.org"

var solution = "./HorizonteNet.sln";
var artifactsDir = Directory("./artifacts");

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
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
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
        ArgumentCustomization = args => args.Append($"/p:Version={version}")
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

    foreach(var package in packages)
    {
        Information($"Pushing {package.GetFilename()} to {url}...");
        
        var settings = new DotNetNuGetPushSettings
        {
            Source = url,
            ApiKey = nugetApiKey,
            SkipDuplicate = true
        };

        DotNetNuGetPush(package, settings);
    }
});

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target); 