using System.Reflection;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
namespace Horizonte.Extensions.Interactive;

/// <summary>
/// La clase estática <c>Extensions</c> proporciona métodos de extensión para facilitar la interacción con componentes de núcleo y scripts en el contexto de uso de .NET Interactive.
/// </summary>
/// <remarks>
/// Esta clase incluye métodos para construir un objeto de tipo <c>CompositeKernel</c>, compilar definiciones de script y cargar módulos de script de manera asincrónica.
/// Facilita la integración y personalización de funciones nucleares y de scripts dentro de entornos proporcionados.
/// </remarks>
public static class Extensions
{
    /// <summary>
    /// Construye un <see cref="CompositeKernel"/> de forma asincrónica utilizando las opciones de kernel especificadas y un proveedor de servicios.
    /// </summary>
    /// <param name="kernelOptions">
    /// Configuraciones y opciones que se utilizarán para construir el kernel compuesto.
    /// </param>
    /// <param name="serviceProvider">
    /// Proveedor de servicios que proporciona las dependencias necesarias al construir el kernel.
    /// </param>
    /// <returns>
    /// Una tarea que representa la operación asincrónica. La tarea contiene el <see cref="CompositeKernel"/> construido.
    /// </returns>
    /// <exception cref="Exception">
    /// Se produce si ocurre un error durante la construcción del kernel compuesto.
    /// </exception>
    public static async Task<CompositeKernel> BuildKernelAsync(this KernelOptions kernelOptions,IServiceProvider serviceProvider,bool aislado = false)
    {
        try
        {
            var csharpKernel = new Microsoft.DotNet.Interactive.CSharp.CSharpKernel();

            if (!aislado)
            {
                // Inyectar el proveedor de servicios
                csharpKernel.AddHorizonteMiddleware(serviceProvider);
                await csharpKernel.SetValueAsync("ServiceProvider", serviceProvider, typeof(IServiceProvider));

                // Incluimos soporte mínimo a referencias de ensamblados
                await csharpKernel.SendAsync(new SubmitCode($"#r \"nuget:Horizonte,10.0.0\""));
                await csharpKernel.SendAsync(new SubmitCode("#r \"nuget: Microsoft.Extensions.DependencyInjection.Abstractions, 10.0.1\""));
            }
            //aqui hay que implementar la función display para integrarla con el Widget...
            //await csharpKernel.SendAsync(new SubmitCode("static void display(object x) => Microsoft.DotNet.Interactive.KernelInvocationContext.Current?.Display(x, \"text/plain\");"));
            await csharpKernel.SendAsync(new SubmitCode("static void display(object x) =>Console.WriteLine(x.ToString());"));

            var kernel = new CompositeKernel
            {
                csharpKernel
            };
            kernel.DefaultKernelName = csharpKernel.Name;


            return kernel;

        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
    }

    private static void AddHorizonteMiddleware(this Microsoft.DotNet.Interactive.CSharp.CSharpKernel csharpKernel, IServiceProvider serviceProvider)
    {
        csharpKernel.AddMiddleware(async (command, context, next) =>
        {
            if (command is SubmitCode submitCode)
            {
                var code = submitCode.Code;
                var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
                var newLines = new List<string>();
                var intercepted = false;

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();

                    // Caso 1: #r "nuget:..."
                    var nugetMatch = Regex.Match(trimmedLine, @"^#r\s+""nuget:\s*([^,/""]+)(?:,\s*([^""]+))?""\s*$", RegexOptions.IgnoreCase);
                    if (nugetMatch.Success)
                    {
                        var packageName = nugetMatch.Groups[1].Value.Trim();
                        var version = nugetMatch.Groups[2].Success ? nugetMatch.Groups[2].Value.Trim() : "1.0.0"; // Versión por defecto si no se especifica
                        var assemblyManager = serviceProvider.GetService<IhAssemblyManager>() ;
                        if (assemblyManager != null)
                        {
                            var args = new ResolveEventArgs($"{packageName}, Version={version}");
                            var assembly = assemblyManager.ResolveAssemblyFromNuGetPackages(null, args);
                            if (assembly != null)
                            {
                                //si el ensamblado esta embedido en el ejecutable...no tiene localización
                                if (string.IsNullOrEmpty(assembly.Location))
                                {
                                    var location = assemblyManager.ResolveAssemblyDllPath(packageName, version);
                                    if (!string.IsNullOrEmpty(location))
                                    {
                                        // Asegurar que la ruta sea absoluta y normalizada
                                        var absolutePath = Path.IsPathRooted(location)
                                            ? Path.GetFullPath(location)
                                            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), location));
                                        csharpKernel.AddAssemblyReferences(new[] { absolutePath });
                                    }
                                }
                                else
                                {
                                    csharpKernel.AddAssemblyReferences(new[] { assembly.Location });
                                }
                                intercepted = true;
                                continue; // No añadir esta línea al código final si se manejó aquí
                            }
                        }

                    }

                    // Caso 2: #r "ruta/al/dll" (directo)
                    var pathMatch = Regex.Match(trimmedLine, @"^#r\s+""([^""]+\.dll)""\s*$", RegexOptions.IgnoreCase);
                    if (pathMatch.Success)
                    {
                        var dllPath = pathMatch.Groups[1].Value.Trim();
                        // Si la ruta no es absoluta, resolverla relativa al directorio actual
                        var absoluteDllPath = Path.IsPathRooted(dllPath)
                            ? Path.GetFullPath(dllPath)
                            : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), dllPath));

                        if (File.Exists(absoluteDllPath))
                        {
                            csharpKernel.AddAssemblyReferences(new[] { absoluteDllPath });
                            intercepted = true;
                            continue;
                        }
                    }

                    newLines.Add(line);
                }

                if (intercepted)
                {
                    var newCode = string.Join(Environment.NewLine, newLines);
                    if (string.IsNullOrWhiteSpace(newCode))
                    {
                        // Si no queda código, completamos el comando exitosamente
                        context.Publish(new CommandSucceeded(command));
                        return;
                    }

                    // Actualizar el código del comando (si es posible, aunque SubmitCode.Code es readonly en algunas versiones, 
                    // pero aquí solemos poder recrear el comando o simplemente dejar que siga si modificamos el objeto si es mutable)
                    // En .NET Interactive SubmitCode.Code suele ser mutable o podemos enviar uno nuevo.
                    // Sin embargo, lo más limpio es dejar que siga con las líneas restantes.

                    // Reflejamos el cambio en el comando actual si es posible
                    var codeField = typeof(SubmitCode).GetField("<Code>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (codeField != null)
                    {
                        codeField.SetValue(submitCode, newCode);
                    }
                }
            }
            await next(command, context);
        });
    }


    public static async Task<ScriptDefCompileResponse> Compile(this ScriptDef scriptDef,IhAssemblyManager assemblyManager)
    {
        var response = new ScriptDefCompileResponse();
        try
        {
            var code = scriptDef.CodeText;
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var cleanCodeLines = new List<string>();
            var additionalReferences = new List<MetadataReference>();

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Caso 1: #r "nuget:..." con Regex más flexible
                var nugetMatch = Regex.Match(trimmedLine, @"^#r\s+""nuget:\s*([^,/""]+)(?:,\s*([^""]+))?""\s*", RegexOptions.IgnoreCase);
                if (nugetMatch.Success)
                {
                    if (assemblyManager != null)
                    {
                        var packageName = nugetMatch.Groups[1].Value.Trim();
                        var version = nugetMatch.Groups[2].Success ? nugetMatch.Groups[2].Value.Trim() : "1.0.0";
                        var args = new ResolveEventArgs($"{packageName}, Version={version}");
                        var assembly = assemblyManager.ResolveAssemblyFromNuGetPackages(null, args);
                        if (assembly != null)
                        {
                            var location = assembly.Location;
                            if (string.IsNullOrEmpty(location))
                            {
                                location = assemblyManager.ResolveAssemblyDllPath(packageName, version);
                            }

                            if (!string.IsNullOrEmpty(location))
                            {
                                additionalReferences.Add(MetadataReference.CreateFromFile(location));
                            }
                        }
                    }
                    cleanCodeLines.Add(""); // Reemplazar con línea vacía para mantener números de línea
                    continue;
                }

                // Caso 2: #r "ruta/al/dll" (directo)
                var pathMatch = Regex.Match(trimmedLine, @"^#r\s+""([^""]+\.dll)""\s*", RegexOptions.IgnoreCase);
                if (pathMatch.Success)
                {
                    var dllPath = pathMatch.Groups[1].Value.Trim();
                    var absoluteDllPath = Path.IsPathRooted(dllPath)
                        ? Path.GetFullPath(dllPath)
                        : Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), dllPath));

                    if (File.Exists(absoluteDllPath))
                    {
                        additionalReferences.Add(MetadataReference.CreateFromFile(absoluteDllPath));
                    }
                    cleanCodeLines.Add(""); // Reemplazar con línea vacía
                    continue;
                }
                
                // Si la línea empieza por #r pero no coincidió con los anteriores, 
                // mejor eliminarla para evitar el error de Roslyn "Metadata references are not supported"
                if (trimmedLine.StartsWith("#r ", StringComparison.OrdinalIgnoreCase) || trimmedLine.StartsWith("#r\"", StringComparison.OrdinalIgnoreCase))
                {
                    cleanCodeLines.Add("");
                    continue;
                }

                cleanCodeLines.Add(line);
            }

            var cleanCode = string.Join(Environment.NewLine, cleanCodeLines);
            var syntaxTree = CSharpSyntaxTree.ParseText(cleanCode, new CSharpParseOptions(kind: SourceCodeKind.Script));
            var assemblyName = scriptDef.Name+".script";

            // Referencias básicas necesarias (mínimo absoluto)
            var references = new List<MetadataReference>();
            var addedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Intentar cargar lo básico del AppDomain
            var coreDlls = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "System.Runtime.dll",
                "mscorlib.dll",
                "System.Private.CoreLib.dll",
                "System.Console.dll",
                "System.Collections.dll",
                "System.Linq.dll",
                "System.IO.dll",
                "System.Threading.dll",
                "Microsoft.CSharp.dll"
            };

            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (!asm.IsDynamic && !string.IsNullOrEmpty(asm.Location))
                {
                    var fileName = Path.GetFileName(asm.Location);
                    if (coreDlls.Contains(fileName) || addedFiles.Count < 50) // Limitar para evitar saturar si no es core
                    {
                        if (addedFiles.Add(fileName))
                        {
                            references.Add(MetadataReference.CreateFromFile(asm.Location));
                        }
                    }
                }
            }
            
            // Asegurar System.Runtime específicamente si no se cargó
            if (!addedFiles.Contains("System.Runtime.dll"))
            {
                try {
                    var rt = Assembly.Load("System.Runtime");
                    if (!string.IsNullOrEmpty(rt.Location) && addedFiles.Add(Path.GetFileName(rt.Location)))
                        references.Add(MetadataReference.CreateFromFile(rt.Location));
                } catch {}
            }

            // Añadir Horizonte
            var horizonteAssembly = typeof(IhAssemblyManager).Assembly; //revisar***********************************
            var horizonteLoc = horizonteAssembly.Location;
            
            if (string.IsNullOrEmpty(horizonteLoc) && assemblyManager != null)
            {
                var version = horizonteAssembly.GetName().Version?.ToString() ?? "10.0.0";
                horizonteLoc = assemblyManager.ResolveAssemblyDllPath("Horizonte", version);
            }
            
            if (!string.IsNullOrEmpty(horizonteLoc) && addedFiles.Add(Path.GetFileName(horizonteLoc)))
            {
                references.Add(MetadataReference.CreateFromFile(horizonteLoc));
            }

            // Añadir referencias extraídas de #r
            foreach (var r in additionalReferences)
            {
                if (!string.IsNullOrEmpty(r.Display))
                {
                    var fileName = Path.GetFileName(r.Display);
                    if (addedFiles.Add(fileName))
                    {
                        references.Add(r);
                    }
                }
            }

            var compilation = CSharpCompilation.Create(
                assemblyName,
                syntaxTrees: new[] { syntaxTree },
                references: references,
                options: new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    scriptClassName: "Script",
                    //scriptClassName: scriptDef.Name + "Class",
                    metadataReferenceResolver: ScriptMetadataResolver.Default,
                    usings: new[] { "System", "System.Collections.Generic", "System.Linq", "System.Text", "System.Threading.Tasks" }));

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (!result.Success)
            {
                var failures = result.Diagnostics.Where(diagnostic => 
                    diagnostic.IsWarningAsError || 
                    diagnostic.Severity == DiagnosticSeverity.Error);

                var errorList = new List<string>();
                foreach (var diagnostic in failures)
                {
                    var lineSpan = diagnostic.Location.GetLineSpan();
                    errorList.Add($"Line {lineSpan.StartLinePosition.Line + 1},{lineSpan.StartLinePosition.Character + 1}\t: {diagnostic.GetMessage()}");
                }
                response.Errors = errorList;
                return response;
            }

            ms.Seek(0, SeekOrigin.Begin);
            
            response.Assembly = ms.ToArray();
            
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            response.Errors = new List<string> { e.Message };
        }
        return response;
    }

 public static async Task LoadScriptModules(this IEnumerable<ScriptDef> scriptDefs, IServiceProvider serviceProvider)
        {
            var  logger = serviceProvider.GetService<ILogger<PanelModulo>>();
            try
            {
                var asmmanager = serviceProvider.GetService<IhAssemblyManager>();
                if (asmmanager == null)
                {
                    logger?.LogWarning("No se encontró IhAssemblyManager. No se pueden cargar scripts.");
                    return;
                }

                var scriptasmlist = new List<byte[]>();

                foreach (var script in scriptDefs.Where(x => x.Active))
                {
                    logger?.LogInformation($"Loading script {script.Name}...");

                    var compileresponse = await script.Compile(asmmanager);

                    if (compileresponse.Errors.Any() )
                    {
                        logger?.LogError(
                            "Error compilando script {ScriptName}: {Errors}",
                            script.Name,
                            string.Join(Environment.NewLine, compileresponse.Errors));

                        continue;
                    }

                    if (compileresponse.Assembly is { Length: > 0 } asm)
                    {
                        scriptasmlist.Add(asm);
                    }
                }

                await asmmanager.UnloadDomain("Script");
                await asmmanager.LoadDomain("Script", scriptasmlist);
                
                // Forzar la inicialización de los nuevos comandos si tienen el rol "init"
                var gesCom = serviceProvider.GetService<IHGesCom>();
                if (gesCom != null)
                {
                    foreach (var item in gesCom.GetRoleCommands("init"))
                    {
                        var hCmd = gesCom.GetHCommand(item.CommandName);
                        if (hCmd != null &&
                            string.Equals(hCmd.Domain, "Script", StringComparison.OrdinalIgnoreCase))
                        {
                            logger?.LogInformation($"Ejecutando Init de script: {item.CommandName}");
                            gesCom.RunCommand(item.CommandName);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger?.LogError(e.Message);
            }
        }

    }
       

