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

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("=== TEST DE SELECCION SEMANTICA Y COMPATIBILIDAD DE VERSIONES (PackageSolver) ===");
        Console.WriteLine("================================================================================");

        await RunSemanticVersionSelectionTestsAsync();
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

    private static async Task RunSemanticVersionSelectionTestsAsync()
    {
        var packageId = "Horizonte.Extension.AspNetCore";
        var cache = new HAssemblyCache();

        // Sembrar en cache VersionResources simulados como en BaGet / Nuget remoto
        var versionResources = new Horizonte.Entities.VersionResources
        {
            PackageId = packageId,
            RetrievedAtUtc = DateTime.UtcNow
        };

        versionResources.Sources.Add(new Horizonte.Entities.PackageSourceResource(
            SourceName: "Local BaGet",
            IsLocal: false,
            Versions: new List<Horizonte.Entities.PackageVersionInfo>
            {
                new Horizonte.Entities.PackageVersionInfo(NuGetVersion.Parse("10.0.7-beta"), false, null),
                new Horizonte.Entities.PackageVersionInfo(NuGetVersion.Parse("10.0.6"), false, null),
                new Horizonte.Entities.PackageVersionInfo(NuGetVersion.Parse("10.0.5"), false, null)
            }));

        cache.SetVersionResources(packageId, versionResources);

        var settings = new ModulesSettings
        {
            NugetServers = new List<NugetServerItem>(),
            NugetFolders = new List<NugetFolderItem>()
        };

        var packageSolver = new PackageSolver(cache, settings, NullLogger<PackageSolver>.Instance);

        // Test 1: Solicitud con versión de ensamblado de 4 componentes "10.0.7.0"
        Console.WriteLine("\n[TEST 1] Solicitando '10.0.7.0' cuando existe '10.0.7-beta'...");
        var res1 = await packageSolver.ResolvePackageAsync(packageId, "10.0.7.0");
        Console.WriteLine($"  -> Target version resuelta: '{res1.Version}' (Esperada: '10.0.7-beta')");
        if (res1.Version == "10.0.7-beta")
        {
            Console.WriteLine("  ✓ [OK] Versión de ensamblado 10.0.7.0 seleccionó correctamente 10.0.7-beta.");
        }
        else
        {
            Console.WriteLine($"  ✗ [FAIL] Versión incorrecta: '{res1.Version}'");
        }

        // Test 2: Solicitud sin versión especificada (null o "any")
        Console.WriteLine("\n[TEST 2] Solicitando versión null/any...");
        var res2 = await packageSolver.ResolvePackageAsync(packageId, null);
        Console.WriteLine($"  -> Target version resuelta: '{res2.Version}' (Esperada: '10.0.7-beta')");
        if (res2.Version == "10.0.7-beta")
        {
            Console.WriteLine("  ✓ [OK] Versión vacía seleccionó correctamente la versión más reciente.");
        }
        else
        {
            Console.WriteLine($"  ✗ [FAIL] Versión incorrecta: '{res2.Version}'");
        }

        // Test 3: Preferencia de versión estable sobre prerelease
        var packageId2 = "Horizonte.Samples.WebApp";
        var versionResources2 = new Horizonte.Entities.VersionResources
        {
            PackageId = packageId2,
            RetrievedAtUtc = DateTime.UtcNow
        };

        versionResources2.Sources.Add(new Horizonte.Entities.PackageSourceResource(
            SourceName: "Nuget.org",
            IsLocal: false,
            Versions: new List<Horizonte.Entities.PackageVersionInfo>
            {
                new Horizonte.Entities.PackageVersionInfo(NuGetVersion.Parse("10.0.7-beta"), false, null),
                new Horizonte.Entities.PackageVersionInfo(NuGetVersion.Parse("10.0.7"), false, null),
                new Horizonte.Entities.PackageVersionInfo(NuGetVersion.Parse("10.0.6"), false, null)
            }));

        cache.SetVersionResources(packageId2, versionResources2);

        Console.WriteLine("\n[TEST 3] Solicitando '10.0.7.0' cuando existen '10.0.7' (estable) y '10.0.7-beta'...");
        var res3 = await packageSolver.ResolvePackageAsync(packageId2, "10.0.7.0");
        Console.WriteLine($"  -> Target version resuelta: '{res3.Version}' (Esperada: '10.0.7')");
        if (res3.Version == "10.0.7")
        {
            Console.WriteLine("  ✓ [OK] Versión de ensamblado 10.0.7.0 priorizó correctamente la versión estable 10.0.7.");
        }
        else
        {
            Console.WriteLine($"  ✗ [FAIL] Versión incorrecta: '{res3.Version}'");
        }

        // Test 4: Solicitud de versión inexistente garantiza resolución a una versión descubierta real
        var packageId3 = "Horizonte.PorcupineMod";
        var versionResources3 = new Horizonte.Entities.VersionResources
        {
            PackageId = packageId3,
            RetrievedAtUtc = DateTime.UtcNow
        };

        versionResources3.Sources.Add(new Horizonte.Entities.PackageSourceResource(
            SourceName: "Nuget.org",
            IsLocal: false,
            Versions: new List<Horizonte.Entities.PackageVersionInfo>
            {
                new Horizonte.Entities.PackageVersionInfo(NuGetVersion.Parse("2.1.0-preview"), false, null),
                new Horizonte.Entities.PackageVersionInfo(NuGetVersion.Parse("2.0.0"), false, null)
            }));

        cache.SetVersionResources(packageId3, versionResources3);

        Console.WriteLine("\n[TEST 4] Solicitando '99.0.0' inexistente con versiones disponibles '2.1.0-preview', '2.0.0'...");
        var res4 = await packageSolver.ResolvePackageAsync(packageId3, "99.0.0");
        Console.WriteLine($"  -> Target version resuelta: '{res4.Version}' (Esperada: '2.1.0-preview')");
        if (res4.Version == "2.1.0-preview")
        {
            Console.WriteLine("  ✓ [OK] Solicitud de versión inexistente seleccionó la versión real más reciente descubierta.");
        }
        else
        {
            Console.WriteLine($"  ✗ [FAIL] Versión incorrecta: '{res4.Version}'");
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
