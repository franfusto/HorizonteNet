namespace LinuxApp;

using System.IO.Compression;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Horizonte;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static async Task Main(string[] args)
    {
        string contextname = "horizonte";

        var rootPath = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;
        Directory.SetCurrentDirectory(rootPath);

        if (!File.Exists(contextname + ".json"))
        {
            var textStreamReader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("LinuxApp.horizonte.default.json")!);
            var text = textStreamReader.ReadToEnd();
            File.WriteAllText(contextname + ".json", text);
        }

        var builder = Host.CreateEmptyApplicationBuilder(new HostApplicationBuilderSettings { Args = args });
        builder.ConfigureContext(contextname);

        var host = builder.Build();
        host.LoadEnviroment();
        await host.RunAsync();
    }
}