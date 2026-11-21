using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Horizonte.Entities;
using Horizonte.Helpers;
using Horizonte.Interfaces;
using Horizonte.Services;
using Horizonte.Settings;

namespace Horizonte.TestRunner;

/// <summary>
/// Arnés de pruebas y suite de validación exhaustiva de la matriz de resolución de paquetes,
/// contextos ALC, activos web estáticos, recursos nativos y scaffolding de targets.
/// </summary>
public static class PackageResolutionMatrixTests
{
    public class TestHarnessContext : IDisposable
    {
        public string TempDir { get; }
        public string PackagesDir { get; }
        public string WorkingDir { get; }
        public string PreviousCurrentDir { get; }
        public ServiceProvider ServiceProvider { get; }
        public HDomainManager DomainManager { get; }
        public HAssemblySolver AssemblySolver { get; }
        public HAssemblyCache AssemblyCache { get; }
        public PackageSolver PackageSolver { get; }
        public SymLinkScafolder Scafolder { get; }
        public StaticFileRegistry StaticFileRegistry => AssemblySolver.StaticFileRegistry;

        public TestHarnessContext(string tempDir, string packagesDir, string workingDir, string previousCurrentDir,
            ServiceProvider serviceProvider, HDomainManager domainManager, HAssemblySolver assemblySolver,
            HAssemblyCache assemblyCache, PackageSolver packageSolver, SymLinkScafolder scafolder)
        {
            TempDir = tempDir;
            PackagesDir = packagesDir;
            WorkingDir = workingDir;
            PreviousCurrentDir = previousCurrentDir;
            ServiceProvider = serviceProvider;
            DomainManager = domainManager;
            AssemblySolver = assemblySolver;
            AssemblyCache = assemblyCache;
            PackageSolver = packageSolver;
            Scafolder = scafolder;
        }

        public void Dispose()
        {
            try
            {
                Scafolder.CleanScafolder();
            }
            catch { }

            Directory.SetCurrentDirectory(PreviousCurrentDir);

            try
            {
                if (Directory.Exists(TempDir))
                {
                    Directory.Delete(TempDir, recursive: true);
                }
            }
            catch { }

            ServiceProvider.Dispose();
        }
    }

    /// <summary>
    /// Genera la estructura de carpetas y archivos para los paquetes de referencia:
    /// - foo (1.0.0 y 2.0.0): Solo administrado
    /// - bar (1.0.0 y 2.0.0): Administrado + runtimes/{rid}/native + staticwebassets
    /// - zap (1.0.0 y 2.0.0): Administrado + buildTransitive/.targets + bitcode + staticwebassets
    /// </summary>
    public static async Task GenerateReferencePackagesAsync(string packagesRoot)
    {
        Directory.CreateDirectory(packagesRoot);
        var effectiveRid = NugetHelpers.GetEffectiveRuntimeIdentifier();

        // Paquetes base
        await CreateManagedPackageAsync(packagesRoot, "foo", "1.0.0", "1.0.0.0");
        await CreateManagedPackageAsync(packagesRoot, "foo", "2.0.0", "2.0.0.0");
        await CreateHybridNativePackageAsync(packagesRoot, "bar", "1.0.0", "1.0.0.0", effectiveRid);
        await CreateHybridNativePackageAsync(packagesRoot, "bar", "2.0.0", "2.0.0.0", effectiveRid);
        await CreateBitcodeTargetsPackageAsync(packagesRoot, "zap", "1.0.0", "1.0.0.0");
        await CreateBitcodeTargetsPackageAsync(packagesRoot, "zap", "2.0.0", "2.0.0.0");

        // Paquetes dedicados por caso de prueba para aislamiento en Default ALC
        for (int i = 1; i <= 5; i++)
        {
            await CreateManagedPackageAsync(packagesRoot, $"foo{i}", "1.0.0", "1.0.0.0");
            await CreateManagedPackageAsync(packagesRoot, $"foo{i}", "2.0.0", "2.0.0.0");
        }

        for (int i = 6; i <= 10; i++)
        {
            await CreateHybridNativePackageAsync(packagesRoot, $"bar{i}", "1.0.0", "1.0.0.0", effectiveRid);
            await CreateHybridNativePackageAsync(packagesRoot, $"bar{i}", "2.0.0", "2.0.0.0", effectiveRid);
        }

        for (int i = 11; i <= 15; i++)
        {
            await CreateBitcodeTargetsPackageAsync(packagesRoot, $"zap{i}", "1.0.0", "1.0.0.0");
            await CreateBitcodeTargetsPackageAsync(packagesRoot, $"zap{i}", "2.0.0", "2.0.0.0");
        }
    }

    private static async Task CreateManagedPackageAsync(string packagesRoot, string pkgName, string pkgVer, string asmVer)
    {
        var pkgDir = Path.Combine(packagesRoot, pkgName, pkgVer);
        var libDir = Path.Combine(pkgDir, "lib", "net10.0");
        Directory.CreateDirectory(libDir);

        var dllPath = Path.Combine(libDir, $"{pkgName}.dll");
        byte[] asmBytes = CompileDynamicAssembly(pkgName, asmVer, $"public class {char.ToUpper(pkgName[0]) + pkgName.Substring(1)}Service {{ public string Version => \"{pkgVer}\"; }}");
        await File.WriteAllBytesAsync(dllPath, asmBytes);
    }

    private static async Task CreateHybridNativePackageAsync(string packagesRoot, string pkgName, string pkgVer, string asmVer, string rid)
    {
        var pkgDir = Path.Combine(packagesRoot, pkgName, pkgVer);
        var libDir = Path.Combine(pkgDir, "lib", "net10.0");
        var nativeDir = Path.Combine(pkgDir, "runtimes", rid, "native");
        var staticAssetsDir = Path.Combine(pkgDir, "staticwebassets");

        Directory.CreateDirectory(libDir);
        Directory.CreateDirectory(nativeDir);
        Directory.CreateDirectory(staticAssetsDir);

        // Assembly
        var dllPath = Path.Combine(libDir, $"{pkgName}.dll");
        byte[] asmBytes = CompileDynamicAssembly(pkgName, asmVer, $"public class {char.ToUpper(pkgName[0]) + pkgName.Substring(1)}Service {{ public string Version => \"{pkgVer}\"; }}");
        await File.WriteAllBytesAsync(dllPath, asmBytes);

        // Native binary
        string nativeFileName = OperatingSystem.IsWindows()
            ? $"{pkgName}.dll"
            : (OperatingSystem.IsMacOS() ? $"lib{pkgName}.dylib" : $"lib{pkgName}.so");
        var nativeFilePath = Path.Combine(nativeDir, nativeFileName);

        // Buscar un binario nativo existente en dotnetDir para simular una librería nativa cargable
        var dotnetDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        var validNativeSource = Directory.EnumerateFiles(dotnetDir)
            .FirstOrDefault(f => f.EndsWith(".so") || f.EndsWith(".dylib") || (OperatingSystem.IsWindows() && f.EndsWith(".dll") && !f.Contains("System.Private.CoreLib")));

        if (validNativeSource != null && File.Exists(validNativeSource))
        {
            File.Copy(validNativeSource, nativeFilePath, overwrite: true);
        }
        else
        {
            await File.WriteAllTextAsync(nativeFilePath, $"/* native binary content for {pkgName} v{pkgVer} */");
        }

        // Static Web Asset
        var cssPath = Path.Combine(staticAssetsDir, $"{pkgName}.css");
        await File.WriteAllTextAsync(cssPath, $"/* styles for {pkgName} v{pkgVer} */\nbody {{ background-color: #{pkgVer.Replace(".", "")}; }}");
    }

    private static async Task CreateBitcodeTargetsPackageAsync(string packagesRoot, string pkgName, string pkgVer, string asmVer)
    {
        var pkgDir = Path.Combine(packagesRoot, pkgName, pkgVer);
        var libDir = Path.Combine(pkgDir, "lib", "net10.0");
        var buildTransitiveDir = Path.Combine(pkgDir, "buildTransitive");
        var staticAssetsDir = Path.Combine(pkgDir, "staticwebassets");

        Directory.CreateDirectory(libDir);
        Directory.CreateDirectory(buildTransitiveDir);
        Directory.CreateDirectory(staticAssetsDir);

        // Assembly
        var dllPath = Path.Combine(libDir, $"{pkgName}.dll");
        byte[] asmBytes = CompileDynamicAssembly(pkgName, asmVer, $"public class {char.ToUpper(pkgName[0]) + pkgName.Substring(1)}Service {{ public string Version => \"{pkgVer}\"; }}");
        await File.WriteAllBytesAsync(dllPath, asmBytes);

        // Bitcode file
        var bitcodePath = Path.Combine(buildTransitiveDir, $"{pkgName}.bc");
        await File.WriteAllTextAsync(bitcodePath, $"BITCODE_PAYLOAD_V_{pkgVer}");

        // Targets file que referencia al bitcode usando <Content> con <CopyToOutputDirectory>
        var targetsContent = $@"<Project xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <ItemGroup>
    <Content Include=""$(MSBuildThisFileDirectory){pkgName}.bc"">
      <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
      <Link>{pkgName}.bc</Link>
    </Content>
  </ItemGroup>
</Project>";
        var targetsPath = Path.Combine(buildTransitiveDir, $"{pkgName}.targets");
        await File.WriteAllTextAsync(targetsPath, targetsContent);

        // Static Web Asset
        var jsPath = Path.Combine(staticAssetsDir, $"{pkgName}.js");
        await File.WriteAllTextAsync(jsPath, $"// script for {pkgName} v{pkgVer}\nconsole.log('{pkgName} v{pkgVer}');");
    }

    private static byte[] CompileDynamicAssembly(string assemblyName, string assemblyVersion, string sourceCode)
    {
        var code = $@"
using System.Reflection;
[assembly: AssemblyVersion(""{assemblyVersion}"")]
[assembly: AssemblyFileVersion(""{assemblyVersion}"")]

namespace SamplePackages
{{
    {sourceCode}
}}";
        var syntaxTree = CSharpSyntaxTree.ParseText(code);

        var systemObjLocation = typeof(object).Assembly.Location;
        var runtimeLocation = Path.Combine(Path.GetDirectoryName(systemObjLocation)!, "System.Runtime.dll");

        var references = new List<MetadataReference>
        {
            MetadataReference.CreateFromFile(systemObjLocation),
            MetadataReference.CreateFromFile(runtimeLocation)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName,
            new[] { syntaxTree },
            references,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);
        if (!result.Success)
        {
            var errors = string.Join(Environment.NewLine, result.Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).Select(d => d.GetMessage()));
            throw new InvalidOperationException($"Error compilando ensamblado dinámico '{assemblyName}' v{assemblyVersion}: {errors}");
        }

        return ms.ToArray();
    }

    /// <summary>
    /// Crea un contexto de prueba aislado con su propio directorio de trabajo y servicios.
    /// </summary>
    public static async Task<TestHarnessContext> CreateIsolatedContextAsync()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "horizonte_matrix_" + Guid.NewGuid().ToString("N"));
        var packagesDir = Path.Combine(tempDir, "packages");
        var workingDir = Path.Combine(tempDir, "work");

        Directory.CreateDirectory(packagesDir);
        Directory.CreateDirectory(workingDir);

        // Generar paquetes de prueba
        await GenerateReferencePackagesAsync(packagesDir);

        var prevDir = Directory.GetCurrentDirectory();
        Directory.SetCurrentDirectory(workingDir);

        var settings = new ModulesSettings
        {
            NugetFolders = new List<NugetFolderItem>
            {
                new NugetFolderItem
                {
                    Folder = packagesDir,
                    Name = "MatrixLocalPackages",
                    InstallFolder = true,
                    Active = true,
                    Order = 0
                }
            },
            NugetServers = new List<NugetServerItem>(),
            FrameworkPriorities = new List<string> { "net10.0" },
            Domains = new List<string> { Const.DefaultDomainName, "DominioX", "DominioY" }
        };

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton(settings);

        var cache = new HAssemblyCache();
        var packageSolver = new PackageSolver(cache, settings, NullLogger<PackageSolver>.Instance);
        var scafolder = new SymLinkScafolder(NullLogger<PanelModulo>.Instance);

        services.AddSingleton<IHAssemblyCache>(cache);
        services.AddSingleton<IPackageSolver>(packageSolver);
        services.AddSingleton<ISymLinkScafolder>(scafolder);

        var sp = services.BuildServiceProvider();

        var domainManager = new HDomainManager(sp, NullLogger<HDomainManager>.Instance);
        var assemblySolver = new HAssemblySolver(cache, packageSolver, sp, scafolder, NullLogger<HAssemblySolver>.Instance);

        // Registrar servicios en ServiceProvider simulado si es necesario
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddSingleton<IHAssemblyCache>(cache);
        serviceCollection.AddSingleton<IPackageSolver>(packageSolver);
        serviceCollection.AddSingleton<ISymLinkScafolder>(scafolder);
        serviceCollection.AddSingleton<IHDomainManager>(domainManager);
        serviceCollection.AddSingleton<IHAssemblySolver>(assemblySolver);
        serviceCollection.AddSingleton(settings);

        var finalSp = serviceCollection.BuildServiceProvider();

        return new TestHarnessContext(tempDir, packagesDir, workingDir, prevDir, finalSp, domainManager, assemblySolver, cache, packageSolver, scafolder);
    }

    /// <summary>
    /// Carga un paquete por nombre y versión en el ALC del dominio especificado,
    /// disparando la resolución por ALC o por PackageSolver y post-procesamiento.
    /// </summary>
    public static Assembly? LoadPackageInDomain(TestHarnessContext ctx, string packageName, string version, string domainName)
    {
        var alc = ctx.DomainManager.GetDomain(domainName);
        if (alc == null)
        {
            alc = ctx.DomainManager.CreateDomain(domainName, isCollectible: true);
        }

        // Si ya está cargado en el ALC de destino, .NET ALC y HAssemblySolver devuelven la instancia existente (inmutabilidad ALC)
        var existingAsm = alc.Assemblies.FirstOrDefault(a => string.Equals(a.GetName().Name, packageName, StringComparison.OrdinalIgnoreCase));
        if (existingAsm != null)
        {
            return existingAsm;
        }

        // Resolver la ruta física del ensamblado a través de HAssemblySolver
        var dllPath = ctx.AssemblySolver.ResolveAssemblyDllPath(packageName, version, null, domainName);
        if (!string.IsNullOrEmpty(dllPath) && File.Exists(dllPath))
        {
            // Cargar en ALC
            var asm = alc.LoadFromAssemblyPath(dllPath);

            // Registrar en caché de HAssemblyCache
            ctx.AssemblyCache.Add(new AssemblyCacheItem
            {
                AssemblyName = packageName,
                Version = version,
                DomainName = domainName,
                PackageId = packageName,
                PackageVersion = version,
                DllPath = dllPath,
                ResolvedAssembly = asm,
                IsResolved = true,
                Found = true
            });

            // Post-procesar activos web por dominio
            ctx.StaticFileRegistry.RegisterPackageDirectory(dllPath, domainName);

            return asm;
        }

        return null;
    }

    /// <summary>
    /// Ejecuta y valida de forma automatizada las 15 combinaciones de la matriz técnica.
    /// </summary>
    public static async Task<bool> RunAll15CasesMatrixAsync()
    {
        Console.WriteLine("\n================================================================================");
        Console.WriteLine("=== MATRIZ EXHAUSTIVA DE RESOLUCION DE PAQUETES Y RECURSOS (15 CASOS) ===");
        Console.WriteLine("================================================================================\n");

        int passed = 0;
        int total = 15;

        // ============================================================================================
        // CASOS 1 a 5: Paquete 'foo' (Solo Administrado)
        // ============================================================================================
        Console.WriteLine("--- GRUPO 1: Paquetes Solo Administrados (foo: 1.0.0 y 2.0.0) ---");

        // CASO 1: foo1 1.0.0 en Default -> foo1 2.0.0 en Default
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "foo1", "1.0.0", Const.DefaultDomainName);
            var ver1 = asm1?.GetName().Version?.ToString();

            // En el mismo ALC, la inmutabilidad de .NET no permite sobrescribir el ensamblado 'foo1' ya cargado
            var asm2 = LoadPackageInDomain(ctx, "foo1", "2.0.0", Const.DefaultDomainName);
            var ver2 = asm2?.GetName().Version?.ToString();

            bool ok = asm1 != null && ver1 == "1.0.0.0" && asm2 != null && ver2 == "1.0.0.0";
            Console.WriteLine($"[CASO 1] 1º foo1 1.0.0 (Default) -> 2º foo1 2.0.0 (Default): {(ok ? "✓ PASS" : "✗ FAIL")} (Versión en Default: {ver1}, tras 2ª solicitud: {ver2})");
            if (ok) passed++;
        }

        // CASO 2: foo2 1.0.0 en Default -> foo2 2.0.0 en DominioX
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "foo2", "1.0.0", Const.DefaultDomainName);
            var asm2 = LoadPackageInDomain(ctx, "foo2", "2.0.0", "DominioX");

            var ver1 = asm1?.GetName().Version?.ToString();
            var ver2 = asm2?.GetName().Version?.ToString();

            bool ok = asm1 != null && ver1 == "1.0.0.0" && asm2 != null && ver2 == "2.0.0.0";
            Console.WriteLine($"[CASO 2] 1º foo2 1.0.0 (Default) -> 2º foo2 2.0.0 (DominioX): {(ok ? "✓ PASS" : "✗ FAIL")} (Default: {ver1}, DominioX: {ver2})");
            if (ok) passed++;
        }

        // CASO 3: foo3 1.0.0 en DominioX -> foo3 2.0.0 en DominioX
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "foo3", "1.0.0", "DominioX");
            var ver1 = asm1?.GetName().Version?.ToString();

            var asm2 = LoadPackageInDomain(ctx, "foo3", "2.0.0", "DominioX");
            var ver2 = asm2?.GetName().Version?.ToString();

            bool ok = asm1 != null && ver1 == "1.0.0.0" && asm2 != null && ver2 == "1.0.0.0";
            Console.WriteLine($"[CASO 3] 1º foo3 1.0.0 (DominioX) -> 2º foo3 2.0.0 (DominioX): {(ok ? "✓ PASS" : "✗ FAIL")} (DominioX mantiene: {ver1}, tras 2ª solicitud: {ver2})");
            if (ok) passed++;
        }

        // CASO 4: foo4 1.0.0 en DominioX -> foo4 2.0.0 en DominioY
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "foo4", "1.0.0", "DominioX");
            var asm2 = LoadPackageInDomain(ctx, "foo4", "2.0.0", "DominioY");

            var ver1 = asm1?.GetName().Version?.ToString();
            var ver2 = asm2?.GetName().Version?.ToString();

            bool ok = asm1 != null && ver1 == "1.0.0.0" && asm2 != null && ver2 == "2.0.0.0";
            Console.WriteLine($"[CASO 4] 1º foo4 1.0.0 (DominioX) -> 2º foo4 2.0.0 (DominioY): {(ok ? "✓ PASS" : "✗ FAIL")} (DominioX: {ver1}, DominioY: {ver2})");
            if (ok) passed++;
        }

        // CASO 5: foo5 1.0.0 en DominioX -> foo5 2.0.0 en Default
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "foo5", "1.0.0", "DominioX");
            var asm2 = LoadPackageInDomain(ctx, "foo5", "2.0.0", Const.DefaultDomainName);

            var ver1 = asm1?.GetName().Version?.ToString();
            var ver2 = asm2?.GetName().Version?.ToString();

            bool ok = asm1 != null && ver1 == "1.0.0.0" && asm2 != null && ver2 == "2.0.0.0";
            Console.WriteLine($"[CASO 5] 1º foo5 1.0.0 (DominioX) -> 2º foo5 2.0.0 (Default): {(ok ? "✓ PASS" : "✗ FAIL")} (DominioX: {ver1}, Default: {ver2})");
            if (ok) passed++;
        }

        // ============================================================================================
        // CASOS 6 a 10: Paquete 'bar' (Administrado + Nativo + Web Assets)
        // ============================================================================================
        Console.WriteLine("\n--- GRUPO 2: Paquetes Administrados + Nativos + Web Assets (bar: 1.0.0 y 2.0.0) ---");

        // CASO 6: bar6 1.0.0 en Default -> bar6 2.0.0 en Default
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "bar6", "1.0.0", Const.DefaultDomainName);
            var ver1 = asm1?.GetName().Version?.ToString();

            var asm2 = LoadPackageInDomain(ctx, "bar6", "2.0.0", Const.DefaultDomainName);
            var ver2 = asm2?.GetName().Version?.ToString();

            var webAssetDef = ctx.StaticFileRegistry.GetFile("/_content/bar6/bar6.css", "Default");
            bool webAssetOk = webAssetDef != null && webAssetDef.Exists;

            var links = ctx.Scafolder.GetScafolder().ToList();
            bool linksDefaultOk = links.Any(l => l.Destination.Contains("bar6"));

            var handleDefault = ctx.AssemblySolver.ResolveUnmanagedDll(AssemblyLoadContext.Default, "bar6");
            bool nativeOk = handleDefault != IntPtr.Zero;

            bool ok = ver1 == "1.0.0.0" && ver2 == "1.0.0.0" && webAssetOk && linksDefaultOk && nativeOk;
            Console.WriteLine($"[CASO 6] 1º bar6 1.0.0 (Default) -> 2º bar6 2.0.0 (Default): {(ok ? "✓ PASS" : "✗ FAIL")} (Default: {ver1}, WebAssetOk: {webAssetOk}, NativeOk: {nativeOk})");
            if (ok) passed++;
        }

        // CASO 7: bar7 1.0.0 en Default -> bar7 2.0.0 en DominioX
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "bar7", "1.0.0", Const.DefaultDomainName);
            var asm2 = LoadPackageInDomain(ctx, "bar7", "2.0.0", "DominioX");

            var ver1 = asm1?.GetName().Version?.ToString();
            var ver2 = asm2?.GetName().Version?.ToString();

            // Web assets aislados por dominio en StaticFileRegistry
            var assetDefault = ctx.StaticFileRegistry.GetFile("/_content/bar7/bar7.css", "Default");
            var assetDomX = ctx.StaticFileRegistry.GetFile("/_content/bar7/bar7.css", "DominioX");

            bool assetsOk = assetDefault != null && assetDefault.Exists && assetDomX != null && assetDomX.Exists;
            bool assetsIsolated = assetDefault != null && assetDomX != null && assetDefault.FullName != assetDomX.FullName;

            // Scaffolding aislado por dominio: verificar enlaces nativos en disco
            var links = ctx.Scafolder.GetScafolder().ToList();
            bool linksDefaultOk = links.Any(l => l.Destination.Contains("bar7") && (l.Destination.Contains("Default") || !l.Destination.Contains("DominioX")));
            bool linksDomXOk = links.Any(l => l.Destination.Contains("bar7") && l.Destination.Contains("DominioX"));

            var domXAlc = ctx.DomainManager.GetDomain("DominioX");
            var handleDefault = ctx.AssemblySolver.ResolveUnmanagedDll(AssemblyLoadContext.Default, "bar7");
            var handleDomX = ctx.AssemblySolver.ResolveUnmanagedDll(domXAlc, "bar7");
            bool nativeOk = handleDefault != IntPtr.Zero && handleDomX != IntPtr.Zero;

            bool ok = ver1 == "1.0.0.0" && ver2 == "2.0.0.0" && assetsOk && assetsIsolated && linksDefaultOk && linksDomXOk && nativeOk;
            Console.WriteLine($"[CASO 7] 1º bar7 1.0.0 (Default) -> 2º bar7 2.0.0 (DominioX): {(ok ? "✓ PASS" : "✗ FAIL")} (Def: {ver1}, DomX: {ver2}, AssetsIsolated: {assetsIsolated}, ScaffoldingIsolated: {linksDomXOk}, NativeOk: {nativeOk})");
            if (ok) passed++;
        }

        // CASO 8: bar8 1.0.0 en DominioX -> bar8 2.0.0 en DominioX
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "bar8", "1.0.0", "DominioX");
            var ver1 = asm1?.GetName().Version?.ToString();

            var asm2 = LoadPackageInDomain(ctx, "bar8", "2.0.0", "DominioX");
            var ver2 = asm2?.GetName().Version?.ToString();

            var assetDomX = ctx.StaticFileRegistry.GetFile("/_content/bar8/bar8.css", "DominioX");
            bool assetOk = assetDomX != null && assetDomX.Exists;

            var links = ctx.Scafolder.GetScafolder().ToList();
            bool linksDomXOk = links.Any(l => l.Destination.Contains("bar8") && l.Destination.Contains("DominioX"));

            var domXAlc = ctx.DomainManager.GetDomain("DominioX");
            var handleDomX = ctx.AssemblySolver.ResolveUnmanagedDll(domXAlc, "bar8");
            bool nativeOk = handleDomX != IntPtr.Zero;

            bool ok = ver1 == "1.0.0.0" && ver2 == "1.0.0.0" && assetOk && linksDomXOk && nativeOk;
            Console.WriteLine($"[CASO 8] 1º bar8 1.0.0 (DominioX) -> 2º bar8 2.0.0 (DominioX): {(ok ? "✓ PASS" : "✗ FAIL")} (DominioX mantiene: {ver1}, AssetOk: {assetOk}, NativeOk: {nativeOk})");
            if (ok) passed++;
        }

        // CASO 9: bar9 1.0.0 en DominioX -> bar9 2.0.0 en DominioY
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "bar9", "1.0.0", "DominioX");
            var asm2 = LoadPackageInDomain(ctx, "bar9", "2.0.0", "DominioY");

            var ver1 = asm1?.GetName().Version?.ToString();
            var ver2 = asm2?.GetName().Version?.ToString();

            var assetDomX = ctx.StaticFileRegistry.GetFile("/_content/bar9/bar9.css", "DominioX");
            var assetDomY = ctx.StaticFileRegistry.GetFile("/_content/bar9/bar9.css", "DominioY");

            bool assetsOk = assetDomX != null && assetDomX.Exists && assetDomY != null && assetDomY.Exists;
            bool assetsIsolated = assetDomX != null && assetDomY != null && assetDomX.FullName != assetDomY.FullName;

            var links = ctx.Scafolder.GetScafolder().ToList();
            bool linksDomXOk = links.Any(l => l.Destination.Contains("bar9") && l.Destination.Contains("DominioX"));
            bool linksDomYOk = links.Any(l => l.Destination.Contains("bar9") && l.Destination.Contains("DominioY"));

            var domXAlc = ctx.DomainManager.GetDomain("DominioX");
            var domYAlc = ctx.DomainManager.GetDomain("DominioY");
            var handleDomX = ctx.AssemblySolver.ResolveUnmanagedDll(domXAlc, "bar9");
            var handleDomY = ctx.AssemblySolver.ResolveUnmanagedDll(domYAlc, "bar9");
            bool nativeOk = handleDomX != IntPtr.Zero && handleDomY != IntPtr.Zero;

            bool ok = ver1 == "1.0.0.0" && ver2 == "2.0.0.0" && assetsOk && assetsIsolated && linksDomXOk && linksDomYOk && nativeOk;
            Console.WriteLine($"[CASO 9] 1º bar9 1.0.0 (DominioX) -> 2º bar9 2.0.0 (DominioY): {(ok ? "✓ PASS" : "✗ FAIL")} (DomX: {ver1}, DomY: {ver2}, AssetsIsolated: {assetsIsolated}, LinksIsolated: {linksDomXOk && linksDomYOk}, NativeOk: {nativeOk})");
            if (ok) passed++;
        }

        // CASO 10: bar10 1.0.0 en DominioX -> bar10 2.0.0 en Default
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "bar10", "1.0.0", "DominioX");
            var asm2 = LoadPackageInDomain(ctx, "bar10", "2.0.0", Const.DefaultDomainName);

            var ver1 = asm1?.GetName().Version?.ToString();
            var ver2 = asm2?.GetName().Version?.ToString();

            var assetDomX = ctx.StaticFileRegistry.GetFile("/_content/bar10/bar10.css", "DominioX");
            var assetDef = ctx.StaticFileRegistry.GetFile("/_content/bar10/bar10.css", "Default");

            bool assetsOk = assetDomX != null && assetDomX.Exists && assetDef != null && assetDef.Exists;
            bool assetsIsolated = assetDomX != null && assetDef != null && assetDomX.FullName != assetDef.FullName;

            var links = ctx.Scafolder.GetScafolder().ToList();
            bool linksDomXOk = links.Any(l => l.Destination.Contains("bar10") && l.Destination.Contains("DominioX"));
            bool linksDefaultOk = links.Any(l => l.Destination.Contains("bar10") && (l.Destination.Contains("Default") || !l.Destination.Contains("DominioX")));

            var domXAlc = ctx.DomainManager.GetDomain("DominioX");
            var handleDomX = ctx.AssemblySolver.ResolveUnmanagedDll(domXAlc, "bar10");
            var handleDefault = ctx.AssemblySolver.ResolveUnmanagedDll(AssemblyLoadContext.Default, "bar10");
            bool nativeOk = handleDomX != IntPtr.Zero && handleDefault != IntPtr.Zero;

            bool ok = ver1 == "1.0.0.0" && ver2 == "2.0.0.0" && assetsOk && assetsIsolated && linksDomXOk && linksDefaultOk && nativeOk;
            Console.WriteLine($"[CASO 10] 1º bar10 1.0.0 (DominioX) -> 2º bar10 2.0.0 (Default): {(ok ? "✓ PASS" : "✗ FAIL")} (DomX: {ver1}, Default: {ver2}, AssetsIsolated: {assetsIsolated}, LinksIsolated: {linksDomXOk && linksDefaultOk}, NativeOk: {nativeOk})");
            if (ok) passed++;
        }

        // ============================================================================================
        // CASOS 11 a 15: Paquete 'zap' (Administrado + Bitcode/.targets + Web Assets)
        // ============================================================================================
        Console.WriteLine("\n--- GRUPO 3: Paquetes Administrados + Bitcode/.targets + Web Assets (zap: 1.0.0 y 2.0.0) ---");

        // CASO 11: zap11 1.0.0 en Default -> zap11 2.0.0 en Default
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "zap11", "1.0.0", Const.DefaultDomainName);
            var ver1 = asm1?.GetName().Version?.ToString();

            var bitcodeDest = Path.Combine(Directory.GetCurrentDirectory(), "zap11.bc");
            var bitcodeContent1 = File.Exists(bitcodeDest) ? File.ReadAllText(bitcodeDest) : "";

            var asm2 = LoadPackageInDomain(ctx, "zap11", "2.0.0", Const.DefaultDomainName);
            var ver2 = asm2?.GetName().Version?.ToString();
            var bitcodeContent2 = File.Exists(bitcodeDest) ? File.ReadAllText(bitcodeDest) : "";

            bool bitcodePreserved = bitcodeContent1.Contains("1.0.0") && bitcodeContent2.Contains("1.0.0");
            bool ok = ver1 == "1.0.0.0" && ver2 == "1.0.0.0" && bitcodePreserved;
            Console.WriteLine($"[CASO 11] 1º zap11 1.0.0 (Default) -> 2º zap11 2.0.0 (Default): {(ok ? "✓ PASS" : "✗ FAIL")} (Default: {ver1}, BitcodePreservado: {bitcodePreserved})");
            if (ok) passed++;
        }

        // CASO 12: zap12 1.0.0 en Default -> zap12 2.0.0 en DominioX
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "zap12", "1.0.0", Const.DefaultDomainName);
            var ver1 = asm1?.GetName().Version?.ToString();

            var bitcodeDest = Path.Combine(Directory.GetCurrentDirectory(), "zap12.bc");
            var bitcodeContent1 = File.Exists(bitcodeDest) ? File.ReadAllText(bitcodeDest) : "";

            var asm2 = LoadPackageInDomain(ctx, "zap12", "2.0.0", "DominioX");
            var ver2 = asm2?.GetName().Version?.ToString();

            var bitcodeContent2 = File.Exists(bitcodeDest) ? File.ReadAllText(bitcodeDest) : "";

            var assetDefault = ctx.StaticFileRegistry.GetFile("/_content/zap12/zap12.js", "Default");
            var assetDomX = ctx.StaticFileRegistry.GetFile("/_content/zap12/zap12.js", "DominioX");

            bool assetsOk = assetDefault != null && assetDefault.Exists && assetDomX != null && assetDomX.Exists;
            bool assetsIsolated = assetDefault != null && assetDomX != null && assetDefault.FullName != assetDomX.FullName;

            // Scaffolding para 2.0.0 detecta destino existente y NO sobrescribe (permanece 1.0.0)
            bool bitcodePreserved = bitcodeContent1.Contains("1.0.0") && bitcodeContent2.Contains("1.0.0");

            bool ok = ver1 == "1.0.0.0" && ver2 == "2.0.0.0" && assetsOk && assetsIsolated && bitcodePreserved;
            Console.WriteLine($"[CASO 12] 1º zap12 1.0.0 (Default) -> 2º zap12 2.0.0 (DominioX): {(ok ? "✓ PASS" : "✗ FAIL")} (Def: {ver1}, DomX: {ver2}, AssetsIsolated: {assetsIsolated}, BitcodePreserved: {bitcodePreserved})");
            if (ok) passed++;
        }

        // CASO 13: zap13 1.0.0 en DominioX -> zap13 2.0.0 en DominioX
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "zap13", "1.0.0", "DominioX");
            var ver1 = asm1?.GetName().Version?.ToString();

            var bitcodeDest = Path.Combine(Directory.GetCurrentDirectory(), "zap13.bc");
            var bitcodeContent1 = File.Exists(bitcodeDest) ? File.ReadAllText(bitcodeDest) : "";

            var asm2 = LoadPackageInDomain(ctx, "zap13", "2.0.0", "DominioX");
            var ver2 = asm2?.GetName().Version?.ToString();

            var bitcodeContent2 = File.Exists(bitcodeDest) ? File.ReadAllText(bitcodeDest) : "";

            bool bitcodePreserved = bitcodeContent1.Contains("1.0.0") && bitcodeContent2.Contains("1.0.0");
            bool ok = ver1 == "1.0.0.0" && ver2 == "1.0.0.0" && bitcodePreserved;
            Console.WriteLine($"[CASO 13] 1º zap13 1.0.0 (DominioX) -> 2º zap13 2.0.0 (DominioX): {(ok ? "✓ PASS" : "✗ FAIL")} (DominioX: {ver1}, BitcodePreservado: {bitcodePreserved})");
            if (ok) passed++;
        }

        // CASO 14: zap14 1.0.0 en DominioX -> zap14 2.0.0 en DominioY
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "zap14", "1.0.0", "DominioX");
            var asm2 = LoadPackageInDomain(ctx, "zap14", "2.0.0", "DominioY");

            var ver1 = asm1?.GetName().Version?.ToString();
            var ver2 = asm2?.GetName().Version?.ToString();

            var bitcodeDest = Path.Combine(Directory.GetCurrentDirectory(), "zap14.bc");
            var bitcodeContent = File.Exists(bitcodeDest) ? File.ReadAllText(bitcodeDest) : "";

            var assetDomX = ctx.StaticFileRegistry.GetFile("/_content/zap14/zap14.js", "DominioX");
            var assetDomY = ctx.StaticFileRegistry.GetFile("/_content/zap14/zap14.js", "DominioY");

            bool assetsOk = assetDomX != null && assetDomX.Exists && assetDomY != null && assetDomY.Exists;
            bool assetsIsolated = assetDomX != null && assetDomY != null && assetDomX.FullName != assetDomY.FullName;
            bool bitcodePreserved = bitcodeContent.Contains("1.0.0");

            bool ok = ver1 == "1.0.0.0" && ver2 == "2.0.0.0" && assetsOk && assetsIsolated && bitcodePreserved;
            Console.WriteLine($"[CASO 14] 1º zap14 1.0.0 (DominioX) -> 2º zap14 2.0.0 (DominioY): {(ok ? "✓ PASS" : "✗ FAIL")} (DomX: {ver1}, DomY: {ver2}, AssetsIsolated: {assetsIsolated}, BitcodePreserved: {bitcodePreserved})");
            if (ok) passed++;
        }

        // CASO 15: zap15 1.0.0 en DominioX -> zap15 2.0.0 en Default
        {
            using var ctx = await CreateIsolatedContextAsync();
            var asm1 = LoadPackageInDomain(ctx, "zap15", "1.0.0", "DominioX");
            var asm2 = LoadPackageInDomain(ctx, "zap15", "2.0.0", Const.DefaultDomainName);

            var ver1 = asm1?.GetName().Version?.ToString();
            var ver2 = asm2?.GetName().Version?.ToString();

            var bitcodeDest = Path.Combine(Directory.GetCurrentDirectory(), "zap15.bc");
            var bitcodeContent = File.Exists(bitcodeDest) ? File.ReadAllText(bitcodeDest) : "";

            var assetDomX = ctx.StaticFileRegistry.GetFile("/_content/zap15/zap15.js", "DominioX");
            var assetDef = ctx.StaticFileRegistry.GetFile("/_content/zap15/zap15.js", "Default");

            bool assetsOk = assetDomX != null && assetDomX.Exists && assetDef != null && assetDef.Exists;
            bool assetsIsolated = assetDomX != null && assetDef != null && assetDomX.FullName != assetDef.FullName;
            bool bitcodePreserved = bitcodeContent.Contains("1.0.0");

            bool ok = ver1 == "1.0.0.0" && ver2 == "2.0.0.0" && assetsOk && assetsIsolated && bitcodePreserved;
            Console.WriteLine($"[CASO 15] 1º zap15 1.0.0 (DominioX) -> 2º zap15 2.0.0 (Default): {(ok ? "✓ PASS" : "✗ FAIL")} (DomX: {ver1}, Default: {ver2}, AssetsIsolated: {assetsIsolated}, BitcodePreserved: {bitcodePreserved})");
            if (ok) passed++;
        }

        Console.WriteLine("\n================================================================================");
        Console.WriteLine($"=== RESUMEN DE MATRIZ DE RESOLUCION: {passed}/{total} CASOS EXITOSOS ===");
        Console.WriteLine("================================================================================\n");

        return passed == total;
    }
}
