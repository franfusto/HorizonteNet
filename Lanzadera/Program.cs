using System.IO.Compression;
using System.Reflection;
using Microsoft.Extensions.Logging;

namespace Lanzadera;

using Horizonte;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

class Program
{
    static void Main(string[] args)
    {
        string contextname = "horizonte";
        var assembly = Assembly.GetExecutingAssembly();

        var rootPath = System.IO.Path.GetDirectoryName(Environment.GetCommandLineArgs()[0])!;
        Directory.SetCurrentDirectory(rootPath);

        if (!File.Exists(contextname + ".json"))
        {
            var textStreamReader = new StreamReader(assembly.GetManifestResourceStream("Lanzadera.horizonte.json")!);
            var text = textStreamReader.ReadToEnd();
            File.WriteAllText(contextname + ".json", text);
        }

        if (!Directory.Exists("mod"))
        {
            var textStreamReader = new StreamReader(assembly.GetManifestResourceStream("Lanzadera.mod.zip")!);
            var text = textStreamReader.ReadToEnd().ToArray();
            
            {                
                //write the resource zip file to the temp directory
                using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Lanzadera.mod.zip")!)
                {
                    using (ZipArchive archive = new ZipArchive(stream,ZipArchiveMode.Read) )
                    {
                        archive.ExtractToDirectory("./");
                    }
                }

       

            }
            
            
        }

        _ = new HorizonteEnv(contextname, args);
    }
}