using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Horizonte.Services;
using Horizonte.Settings;

namespace Horizonte.TestRunner;

public class Program
{
    public static async Task Main(string[] args)
    {
        Console.WriteLine("================================================================================");
        Console.WriteLine("=== REPRODUCTOR Y DIAGNOSTICO DE RESOLUCION (Microsoft.CodeAnalysis.CSharp.Scripting) ===");
        Console.WriteLine("================================================================================");

        var targetPackage = "Microsoft.CodeAnalysis.CSharp.Scripting";
        var targetVersion = "5.0.0.0";

        var testSources = new (string Name, string Url, int Version)[]
        {
            ("Configurado nuget.org v2 (HTTP)", "http://www.nuget.org/api/v2", 2),
            ("Oficial nuget.org v3 (HTTPS)", "https://api.nuget.org/v3/index.json", 3),
            ("Local BaGet (HTTP)", "http://localhost:5555/v3", 3)
        };

        foreach (var (name, url, version) in testSources)
        {
            Console.WriteLine($"\n>>> [TEST FUENTE] {name} -> '{url}'");
            var sw = Stopwatch.StartNew();
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            try
            {
                await TestRemoteSourceDirectAsync(url, targetPackage, cts.Token);
                Console.WriteLine($"[TIEMPO] {name}: {sw.ElapsedMilliseconds} ms");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"[TIMEOUT / PARADA] {name} excedió el tiempo límite (15s). Tiempo transcurrido: {sw.ElapsedMilliseconds} ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR / EXCEPCION] {name}: {ex.GetType().Name} - {ex.Message} ({sw.ElapsedMilliseconds} ms)");
            }
        }

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("=== TEST END-TO-END CON PackageSolver y Configuración de horizonte.default.json ===");
        Console.WriteLine("================================================================================");

        await RunPackageSolverDiagnosticAsync(targetPackage, targetVersion);

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("=== TEST CONCURRENTE CON SERVIDOR INACCESIBLE Y PackageSolver.FindVersionResourcesAsync ===");
        Console.WriteLine("================================================================================");

        await RunConcurrentTimeoutTestAsync("Newtonsoft.Json");
    }

    private static async Task RunPackageSolverDiagnosticAsync(string packageId, string versionStr)
    {
        var settings = new ModulesSettings
        {
            NugetServers = new List<NugetServerItem>
            {
                new NugetServerItem
                {
                    Name = "Nuget.org",
                    Server = "http://www.nuget.org/api/v2",
                    Order = 2,
                    Version = 2,
                    Active = true
                },
                new NugetServerItem
                {
                    Name = "Local BaGet",
                    Server = "http://localhost:5555/v3",
                    Order = 1,
                    Version = 3,
                    Active = false
                }
            },
            NugetFolders = new List<NugetFolderItem>
            {
                new NugetFolderItem
                {
                    Folder = "packages",
                    Name = "Horizonte",
                    InstallFolder = true,
                    Active = true,
                    Order = 0
                }
            },
            FrameworkPriorities = new List<string> { "net10.0", "netstandard2.1", "netstandard2.0" }
        };

        var cache = new HAssemblyCache();
        var logger = NullLogger<PackageSolver>.Instance;
        var packageSolver = new PackageSolver(cache, settings, logger);

        Console.WriteLine($"\nEjecutando PackageSolver.ResolvePackageAsync('{packageId}', '{versionStr}')...");
        var sw = Stopwatch.StartNew();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

        try
        {
            var task = packageSolver.ResolvePackageAsync(packageId, versionStr);
            var completedTask = await Task.WhenAny(task, Task.Delay(Timeout.Infinite, cts.Token));
            if (completedTask == task)
            {
                var result = await task;
                Console.WriteLine($"[FIN PackageSolver] Tiempo: {sw.ElapsedMilliseconds} ms");
                Console.WriteLine($"Resultado -> Success: {result.Success}, PackageId: {result.PackageId}, Version: {result.Version}, DllPath: {result.DllPath}, Error: {result.ErrorMessage}");
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"[BLOQUEO DETECTADO] PackageSolver.ResolvePackageAsync quedó retenido por más de 20s ({sw.ElapsedMilliseconds} ms) esperando respuestas de servidores remotos.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EXCEPCION PackageSolver] {ex.GetType().Name} - {ex.Message} tras {sw.ElapsedMilliseconds} ms");
        }
    }

    private static async Task RunConcurrentTimeoutTestAsync(string packageId)
    {
        var settings = new ModulesSettings
        {
            NugetServers = new List<NugetServerItem>
            {
                new NugetServerItem
                {
                    Name = "Nuget.org Oficial",
                    Server = "https://api.nuget.org/v3/index.json",
                    Order = 1,
                    Version = 3,
                    Active = true
                },
                new NugetServerItem
                {
                    Name = "Servidor Caido / Lento",
                    Server = "http://10.255.255.1:5555/v3/index.json",
                    Order = 2,
                    Version = 3,
                    Active = true
                }
            },
            NugetFolders = new List<NugetFolderItem>()
        };

        var cache = new HAssemblyCache();
        var logger = NullLogger<PackageSolver>.Instance;
        var packageSolver = new PackageSolver(cache, settings, logger);

        Console.WriteLine($"Consultando en paralelo '{packageId}' con un servidor activo oficial y un servidor no alcanzable...");
        var sw = Stopwatch.StartNew();

        var resources = await packageSolver.FindVersionResourcesAsync(packageId);
        sw.Stop();

        Console.WriteLine($"[FIN CONCURRENTE] Tiempo total: {sw.ElapsedMilliseconds} ms");
        Console.WriteLine($"Orígenes obtenidos: {resources.Sources.Count}");
        foreach (var src in resources.Sources)
        {
            Console.WriteLine($"  - Origen: {src.SourceName}, Versiones: {src.Versions.Count}");
        }
    }

    private static async Task TestRemoteSourceDirectAsync(string sourceUrl, string packageId, CancellationToken cancellationToken)
    {
        var repository = Repository.Factory.GetCoreV3(sourceUrl);
        var resource = await repository.GetResourceAsync<FindPackageByIdResource>(cancellationToken);

        if (resource == null)
        {
            Console.WriteLine($"[ERROR] GetResourceAsync<FindPackageByIdResource>() retornó null para '{sourceUrl}'.");
            return;
        }

        using var cacheContext = new SourceCacheContext();
        var versions = await resource.GetAllVersionsAsync(
            packageId,
            cacheContext,
            NuGet.Common.NullLogger.Instance,
            cancellationToken);

        var versionList = versions?.ToList() ?? new();
        Console.WriteLine($"[EXITO] Versiones encontradas ({versionList.Count}): {string.Join(", ", versionList.Take(5).Select(v => v.ToNormalizedString()))} ...");
    }
}
