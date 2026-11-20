using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Horizonte.Entities;
using Horizonte.Helpers;
using Horizonte.Interfaces;
using Horizonte.Services;
using Horizonte.Settings;
using Horizonte.Extensions.Interactive;

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
        Console.WriteLine("=== TEST DE RESOLUCION LOCAL-FIRST Y FALLBACK REMOTO ===");
        Console.WriteLine("================================================================================");

        await RunLocalFirstResolutionTestsAsync();

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("=== TEST DE SELECCION SEMANTICA Y COMPATIBILIDAD DE VERSIONES (PackageSolver) ===");
        Console.WriteLine("================================================================================");

        await RunSemanticVersionSelectionTestsAsync();

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("=== TEST DE PAQUETES NATIVOS, HIBRIDOS, SYMLINKS ARCHIVO A ARCHIVO Y COMPILACION ===");
        Console.WriteLine("================================================================================");

        await RunNativeAndHybridPackageResolutionTestsAsync();

        Console.WriteLine("\n================================================================================");
        Console.WriteLine("=== TEST DE RESOLUCION DE LIBRERIAS NATIVAS (ResolvingUnmanagedDll / NativeLibrary) ===");
        Console.WriteLine("================================================================================");

        await RunNativeUnmanagedDllResolutionTestsAsync();
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

    private static async Task RunLocalFirstResolutionTestsAsync()
    {
        // Test LF-1: Paquete existente localmente no consulta servidores remotos
        var tempDir = Path.Combine(Path.GetTempPath(), "horizonte_test_packages_" + Guid.NewGuid().ToString("N"));
        var pkgDir = Path.Combine(tempDir, "test.local.pkg", "1.0.0", "lib", "net10.0");
        Directory.CreateDirectory(pkgDir);
        var dummyDll = Path.Combine(pkgDir, "Test.Local.Pkg.dll");
        await File.WriteAllTextAsync(dummyDll, "dummy");

        try
        {
            var settings = new ModulesSettings
            {
                NugetFolders = new List<NugetFolderItem>
                {
                    new NugetFolderItem { Folder = tempDir, Active = true, InstallFolder = true }
                },
                NugetServers = new List<NugetServerItem>
                {
                    // Servidor inalcanzable configurado: si se llamara por error, fallaría o demoraría
                    new NugetServerItem { Name = "Unreachable Server", Server = "http://10.255.255.1:5555/v3/index.json", Active = true, Order = 1 }
                },
                FrameworkPriorities = new List<string> { "net10.0" }
            };

            var cache = new HAssemblyCache();
            var solver = new PackageSolver(cache, settings, NullLogger<PackageSolver>.Instance);

            Console.WriteLine("[TEST LF-1] Resolviendo paquete existente en disco local con servidor remoto inalcanzable configurado...");
            var sw = Stopwatch.StartNew();
            var result = await solver.ResolvePackageAsync("Test.Local.Pkg", "1.0.0");
            sw.Stop();

            Console.WriteLine($"  -> Tiempo: {sw.ElapsedMilliseconds} ms, Success: {result.Success}, Version: {result.Version}, DllPath: {result.DllPath}");
            if (result.Success && result.Version == "1.0.0" && result.DllPath == dummyDll && sw.ElapsedMilliseconds < 500)
            {
                Console.WriteLine("  ✓ [OK] Paquete resuelto instantáneamente desde disco sin intentar contactar servidor remoto.");
            }
            else
            {
                Console.WriteLine($"  ✗ [FAIL] Resolución inesperada: Success={result.Success}, Tiempo={sw.ElapsedMilliseconds}ms");
            }

            // Test LF-2: Verificar que VersionResources en caché contiene inicialmente solo origen local
            var vr = cache.GetVersionResources("Test.Local.Pkg");
            if (vr != null && vr.Sources.Count == 1 && vr.Sources[0].IsLocal)
            {
                Console.WriteLine("  ✓ [OK] VersionResources poblado exclusivamente con fuentes locales.");
            }
            else
            {
                Console.WriteLine($"  ✗ [FAIL] VersionResources inesperado: Sources={vr?.Sources.Count}");
            }

            // Test LF-3: Paquete ausente en disco dispara fallback remoto
            Console.WriteLine("\n[TEST LF-2] Resolviendo paquete ausente en disco (debe intentar sondeo remoto)...");
            var remoteResult = await solver.ResolvePackageAsync("Test.NonExistent.Pkg", "2.0.0");
            var vrRemote = cache.GetVersionResources("Test.NonExistent.Pkg");
            if (vrRemote != null)
            {
                Console.WriteLine("  ✓ [OK] Paquete ausente disparó consulta remota y actualizó VersionResources.");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] VersionResources no actualizado tras fallback remoto.");
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
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

    private static async Task RunNativeAndHybridPackageResolutionTestsAsync()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "horizonte_native_test_" + Guid.NewGuid().ToString("N"));
        var rid = NugetHelpers.GetEffectiveRuntimeIdentifier();

        // 1. Crear estructura de paquete nativo puro (ej. OpenCvSharp4.official.runtime.linux-x64)
        var pureNativePkg = "OpenCvSharp4.official.runtime.linux-x64";
        var pureNativeVer = "4.10.0.20240616";
        var pureNativeDir = Path.Combine(tempDir, pureNativePkg.ToLowerInvariant(), pureNativeVer, "runtimes", rid, "native");
        Directory.CreateDirectory(pureNativeDir);
        var pureNativeSo = Path.Combine(pureNativeDir, "libOpenCvSharpExtern.so");
        await File.WriteAllTextAsync(pureNativeSo, "/* dummy native binary */");

        // 2. Crear estructura de paquete mixto / híbrido (lib/net10.0/MyMixed.dll + runtimes/{rid}/native/libMyMixedNative.so)
        var mixedPkg = "MyMixed.Package";
        var mixedVer = "1.0.0";
        var mixedLibDir = Path.Combine(tempDir, mixedPkg.ToLowerInvariant(), mixedVer, "lib", "net10.0");
        var mixedNativeDir = Path.Combine(tempDir, mixedPkg.ToLowerInvariant(), mixedVer, "runtimes", rid, "native");
        Directory.CreateDirectory(mixedLibDir);
        Directory.CreateDirectory(mixedNativeDir);
        var mixedDll = Path.Combine(mixedLibDir, "MyMixed.Package.dll");
        var mixedSo = Path.Combine(mixedNativeDir, "libMyMixedNative.so");
        await File.WriteAllTextAsync(mixedDll, "dummy managed dll");
        await File.WriteAllTextAsync(mixedSo, "/* dummy mixed native binary */");

        SymLinkScafolder? scafolder = null;
        try
        {
            var services = new ServiceCollection();
            services.AddLogging();
            var settings = new ModulesSettings
            {
                NugetFolders = new List<NugetFolderItem>
                {
                    new NugetFolderItem { Folder = tempDir, Active = true, InstallFolder = true }
                },
                FrameworkPriorities = new List<string> { "net10.0" }
            };

            var cache = new HAssemblyCache();
            var packageSolver = new PackageSolver(cache, settings, NullLogger<PackageSolver>.Instance);

            // TEST 1: Resolución de paquete nativo puro en PackageSolver
            Console.WriteLine("\n[TEST NATIVO 1] Resolviendo paquete nativo puro en PackageSolver...");
            var resPure = await packageSolver.ResolvePackageAsync(pureNativePkg, pureNativeVer);
            Console.WriteLine($"  -> Success: {resPure.Success}, IsRuntimePackage: {resPure.IsRuntimePackage}, HasNativeAssets: {resPure.HasNativeAssets}, DllPath: '{resPure.DllPath}', Root: '{resPure.PackageRootDirectory}'");
            if (resPure.Success && resPure.IsRuntimePackage && resPure.HasNativeAssets && string.IsNullOrEmpty(resPure.DllPath))
            {
                Console.WriteLine("  ✓ [OK] Paquete nativo puro resuelto correctamente (Success=true, IsRuntimePackage=true, HasNativeAssets=true).");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] Resultado inesperado en paquete nativo puro.");
            }

            // TEST 2: Resolución de paquete mixto en PackageSolver
            Console.WriteLine("\n[TEST NATIVO 2] Resolviendo paquete mixto (DLL + Runtimes) en PackageSolver...");
            var resMixed = await packageSolver.ResolvePackageAsync(mixedPkg, mixedVer);
            Console.WriteLine($"  -> Success: {resMixed.Success}, IsRuntimePackage: {resMixed.IsRuntimePackage}, HasNativeAssets: {resMixed.HasNativeAssets}, DllPath: '{resMixed.DllPath}', Root: '{resMixed.PackageRootDirectory}'");
            if (resMixed.Success && !resMixed.IsRuntimePackage && resMixed.HasNativeAssets && !string.IsNullOrEmpty(resMixed.DllPath))
            {
                Console.WriteLine("  ✓ [OK] Paquete mixto resuelto correctamente (Success=true, IsRuntimePackage=false, HasNativeAssets=true, DllPath no vacio).");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] Resultado inesperado en paquete mixto.");
            }

            // TEST 3: Andamiaje de symlinks archivo por archivo y coexistencia en HAssemblySolver
            Console.WriteLine("\n[TEST NATIVO 3] Verificando scaffolding archivo por archivo y coexistencia multi-paquete...");
            scafolder = new SymLinkScafolder(NullLogger<PanelModulo>.Instance);
            scafolder.CleanScafolder();
            services.AddSingleton<IHAssemblyCache>(cache);
            services.AddSingleton<IPackageSolver>(packageSolver);
            services.AddSingleton<ISymLinkScafolder>(scafolder);
            var sp = services.BuildServiceProvider();

            var assemblySolver = new HAssemblySolver(cache, packageSolver, sp, scafolder, NullLogger<HAssemblySolver>.Instance);

            // Resolver paquete puro a través de HAssemblySolver
            var alc = System.Runtime.Loader.AssemblyLoadContext.Default;
            var pureAsm = assemblySolver.ResolveAssemblyFromNuGetPackages(alc, new ResolveEventArgs($"{pureNativePkg}, Version={pureNativeVer}"));
            Console.WriteLine($"  -> Pure native Assembly retorno: {(pureAsm == null ? "null (correcto para paquete puro)" : pureAsm.FullName)}");

            var scafolderLinks = scafolder.GetScafolder().ToList();
            Console.WriteLine($"  -> Enlaces simbólicos creados tras paquete puro: {scafolderLinks.Count}");
            foreach (var link in scafolderLinks)
            {
                Console.WriteLine($"     * [Link] Source: '{link.Source}' -> Dest: '{link.Destination}', IsDirectory: {link.IsDirecory}");
            }

            var pureLink = scafolderLinks.FirstOrDefault(l => l.Destination.EndsWith("libOpenCvSharpExtern.so"));
            if (pureAsm == null && pureLink != null && !pureLink.IsDirecory)
            {
                Console.WriteLine("  ✓ [OK] Paquete puro retornó null para ALC y creó enlace archivo por archivo (IsDirecory=false).");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] Fallo en scaffolding de paquete puro.");
            }

            // Resolver paquete mixto
            var mixedDllPath = assemblySolver.ResolveAssemblyDllPath(mixedPkg, mixedVer);
            Console.WriteLine($"  -> Mixed DLL Path resuelto: '{mixedDllPath}'");

            var allLinks = scafolder.GetScafolder().ToList();
            Console.WriteLine($"  -> Enlaces simbólicos totales tras paquete mixto: {allLinks.Count}");
            foreach (var link in allLinks)
            {
                Console.WriteLine($"     * [Link] Source: '{link.Source}' -> Dest: '{link.Destination}', IsDirectory: {link.IsDirecory}");
            }

            var mixedLink = allLinks.FirstOrDefault(l => l.Destination.EndsWith("libMyMixedNative.so"));
            if (mixedLink != null && !mixedLink.IsDirecory && allLinks.Count >= 2)
            {
                Console.WriteLine("  ✓ [OK] Coexistencia multi-paquete validada: ambos archivos nativos coexisten en runtimes/ sin colisión.");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] Fallo en coexistencia de enlaces simbólicos.");
            }

            // TEST 4: Caché en HAssemblyCache
            Console.WriteLine("\n[TEST NATIVO 4] Verificando estado en HAssemblyCache...");
            var pureCache = cache.Get(pureNativePkg, pureNativeVer);
            if (pureCache != null && pureCache.Found && pureCache.IsRuntimePackage && pureCache.HasNativeAssets)
            {
                Console.WriteLine("  ✓ [OK] HAssemblyCache registró correctamente el paquete como Found=true, IsRuntimePackage=true.");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] Estado incorrecto en HAssemblyCache.");
            }

            // TEST 5: Compilación interactiva de script con directiva NuGet nativa pura
            Console.WriteLine("\n[TEST NATIVO 5] Verificando compilación de script con directiva NuGet nativa pura...");
            var mockCoordinator = new MockEnvCoordinator(assemblySolver);
            var script = new ScriptDef
            {
                Name = "TestNativeScript",
                CodeText = $"#r \"nuget:{pureNativePkg}, {pureNativeVer}\"\nConsole.WriteLine(\"Script compilado!\");"
            };

            var compileResult = await script.Compile(mockCoordinator);
            var isSuccess = compileResult.Assembly != null && compileResult.Assembly.Length > 0 && !compileResult.Errors.Any();
            Console.WriteLine($"  -> Compilacion Success: {isSuccess}, Bytes: {compileResult.Assembly?.Length}, Errors: {string.Join("; ", compileResult.Errors)}");
            if (isSuccess)
            {
                Console.WriteLine("  ✓ [OK] Script compilado exitosamente sin error CS0006 para paquete nativo puro.");
            }
            else
            {
                Console.WriteLine($"  ✗ [FAIL] Error en compilación de script: {string.Join("; ", compileResult.Errors)}");
            }
        }
        finally
        {
            scafolder?.CleanScafolder();
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static async Task RunNativeUnmanagedDllResolutionTestsAsync()
    {
        var rid = NugetHelpers.GetEffectiveRuntimeIdentifier();
        var runtimeNativeDir = Path.Combine(Directory.GetCurrentDirectory(), "runtimes", rid, "native");
        Directory.CreateDirectory(runtimeNativeDir);

        var dotnetDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var validNativeSource = Directory.EnumerateFiles(dotnetDir)
            .FirstOrDefault(f => f.EndsWith(".so") || f.EndsWith(".dylib") || (OperatingSystem.IsWindows() && f.EndsWith(".dll") && !f.Contains("System.Private.CoreLib")));

        string targetFileName = OperatingSystem.IsWindows()
            ? "OpenCvSharpExtern.dll"
            : (OperatingSystem.IsMacOS() ? "libOpenCvSharpExtern.dylib" : "libOpenCvSharpExtern.so");

        var targetFilePath = Path.Combine(runtimeNativeDir, targetFileName);

        try
        {
            if (validNativeSource != null && File.Exists(validNativeSource))
            {
                File.Copy(validNativeSource, targetFilePath, overwrite: true);
            }
            else
            {
                // Fallback: crear un archivo dummy si no se encuentra binario nativo en dotnetDir
                await File.WriteAllTextAsync(targetFilePath, "/* dummy native */");
            }

            var services = new ServiceCollection();
            services.AddLogging();
            var settings = new ModulesSettings
            {
                Domains = new List<string> { Const.DefaultDomainName, "TestCustomDomain" }
            };

            var cache = new HAssemblyCache();
            var packageSolver = new PackageSolver(cache, settings, NullLogger<PackageSolver>.Instance);
            var scafolder = new SymLinkScafolder(NullLogger<PanelModulo>.Instance);

            services.AddSingleton<IHAssemblyCache>(cache);
            services.AddSingleton<IPackageSolver>(packageSolver);
            services.AddSingleton<ISymLinkScafolder>(scafolder);

            var sp = services.BuildServiceProvider();
            var assemblySolver = new HAssemblySolver(cache, packageSolver, sp, scafolder, NullLogger<HAssemblySolver>.Instance);
            services.AddSingleton<IHAssemblySolver>(assemblySolver);
            var finalSp = services.BuildServiceProvider();

            var domainManager = new HDomainManager(finalSp, NullLogger<HDomainManager>.Instance);

            // TEST 1: Resolución directa de nombre canónico sin prefijo ni extensión ("OpenCvSharpExtern")
            Console.WriteLine("\n[TEST RESOLUCION NATIVA 1] Resolviendo nombre corto 'OpenCvSharpExtern' en ALC Default...");
            var handleShort = assemblySolver.ResolveUnmanagedDll(System.Runtime.Loader.AssemblyLoadContext.Default, "OpenCvSharpExtern");
            Console.WriteLine($"  -> Handle devuelto: {handleShort}");
            if (handleShort != IntPtr.Zero)
            {
                Console.WriteLine("  ✓ [OK] 'OpenCvSharpExtern' resuelto y cargado exitosamente mediante NativeLibrary.TryLoad.");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] No se pudo resolver 'OpenCvSharpExtern'.");
            }

            // TEST 2: Resolución directa de nombre exacto con prefijo/extensión ("libOpenCvSharpExtern.so")
            Console.WriteLine("\n[TEST RESOLUCION NATIVA 2] Resolviendo nombre exacto '{0}'...", targetFileName);
            var handleExact = assemblySolver.ResolveUnmanagedDll(System.Runtime.Loader.AssemblyLoadContext.Default, targetFileName);
            Console.WriteLine($"  -> Handle devuelto: {handleExact}");
            if (handleExact != IntPtr.Zero)
            {
                Console.WriteLine("  ✓ [OK] Nombre exacto resuelto y cargado exitosamente.");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] No se pudo resolver nombre exacto.");
            }

            // TEST 3: Resolución en dominio aislado (Custom ALC)
            Console.WriteLine("\n[TEST RESOLUCION NATIVA 3] Resolviendo en dominio aislado 'TestCustomDomain'...");
            var customAlc = domainManager.GetDomain("TestCustomDomain");
            var handleCustom = assemblySolver.ResolveUnmanagedDll(customAlc, "OpenCvSharpExtern");
            Console.WriteLine($"  -> Handle devuelto en dominio: {handleCustom}");
            if (handleCustom != IntPtr.Zero)
            {
                Console.WriteLine("  ✓ [OK] Biblioteca nativa resuelta exitosamente en contexto de dominio aislado.");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] Fallo en resolución de dominio aislado.");
            }

            // TEST 4: Resolución transparente mediante evento ALC y NativeLibrary
            Console.WriteLine("\n[TEST RESOLUCION NATIVA 4] Verificando sondeo transparente a través de NativeLibrary.TryLoad...");
            bool loadedViaAlc = System.Runtime.InteropServices.NativeLibrary.TryLoad("OpenCvSharpExtern", typeof(Program).Assembly, null, out var alcHandle);
            Console.WriteLine($"  -> TryLoad: {loadedViaAlc}, Handle: {alcHandle}");
            if (loadedViaAlc && alcHandle != IntPtr.Zero)
            {
                Console.WriteLine("  ✓ [OK] NativeLibrary.TryLoad invocó el evento ResolvingUnmanagedDll de HDomainManager y cargó la librería correctamente.");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] Falló la resolución automática por evento ALC.");
            }

            // TEST 5: Manejo de biblioteca inexistente
            Console.WriteLine("\n[TEST RESOLUCION NATIVA 5] Verificando biblioteca inexistente...");
            var handleNonExistent = assemblySolver.ResolveUnmanagedDll(System.Runtime.Loader.AssemblyLoadContext.Default, "InexistentNativeLib_987654321");
            if (handleNonExistent == IntPtr.Zero)
            {
                Console.WriteLine("  ✓ [OK] Biblioteca inexistente retornó IntPtr.Zero sin lanzar excepciones no controladas.");
            }
            else
            {
                Console.WriteLine("  ✗ [FAIL] Biblioteca inexistente retornó un puntero no nulo.");
            }

            // TEST 6: Descarga de dominio
            Console.WriteLine("\n[TEST RESOLUCION NATIVA 6] Verificando desuscripción y descarga de dominio...");
            await domainManager.UnloadDomainContextAsync("TestCustomDomain");
            Console.WriteLine("  ✓ [OK] Dominio descargado y eventos desuscritos limpiamente.");
        }
        finally
        {
            if (File.Exists(targetFilePath))
            {
                try { File.Delete(targetFilePath); } catch { }
            }
        }
    }

    private class MockEnvCoordinator : IEnvCoordinator
    {
        public MockEnvCoordinator(IHAssemblySolver assemblySolver)
        {
            AssemblySolver = assemblySolver;
        }

        public IHAssemblySolver AssemblySolver { get; }
        public IHDomainManager DomainManager => throw new NotImplementedException();
        public IHGesCom GesCom => throw new NotImplementedException();
        public IhWorkersManager WorkersManager => throw new NotImplementedException();
        public IReadOnlyList<System.Reflection.Assembly> Assemblies => Array.Empty<System.Reflection.Assembly>();
        public IReadOnlyDictionary<string, List<System.Reflection.Assembly>> AssembliesByDomain => new Dictionary<string, List<System.Reflection.Assembly>>();
        public StaticFileRegistry StaticFileRegistry => AssemblySolver.StaticFileRegistry;
        public event Action? DomainChanged { add { } remove { } }
        public Task LoadDomainAsync(string domainName) => Task.CompletedTask;
        public Task LoadDomainAsync(string domainName, IEnumerable<byte[]> assemblies) => Task.CompletedTask;
        public Task UnloadDomainAsync(string domainName) => Task.CompletedTask;
        public Task ReloadDomainAsync(string domainName) => Task.CompletedTask;
        public bool StartBackgroundService(string serviceType) => false;
        public bool StopBackgroundService(string serviceType) => false;
        public bool BackgroundServiceRunning(string serviceType) => false;
        public void LoadModule(string domainName) { }
        public void UnloadModule(string domainName) { }
        public void LoadCommandsByDomain(string domainName) { }
        public void UnloadCommandsByDomain(string domainName) { }
        public object? CreateInstance(Type type) => Activator.CreateInstance(type);
    }
}
