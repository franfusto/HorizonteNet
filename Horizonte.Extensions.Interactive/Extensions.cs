using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.RegularExpressions;
using Horizonte;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

namespace Horizonte.Extensions.Interactive;

public static class Extensions
{
    /// <summary>
    /// Construye un objeto de tipo <c>CompositeKernel</c> de manera asincrónica, utilizando las opciones del núcleo y el entorno proporcionados.
    /// </summary>
    /// <param name="kernelOptions">
    /// Objeto de configuración que contiene opciones específicas para la construcción del núcleo.
    /// </param>
    /// <param name="env">
    /// Instancia que implementa la interfaz <c>IHorizonteEnv</c>, utilizada para definir información o funcionalidades del entorno necesarias para la construcción del núcleo.
    /// </param>
    /// <returns>
    /// Una tarea asincrónica que, al completarse, devuelve una instancia de <c>CompositeKernel</c>.
    /// </returns>
    public static async Task<CompositeKernel> BuildKernelAsync(this KernelOptions kernelOptions, IHorizonteEnv env)
    {
        try
        {
            var csharpKernel = new Microsoft.DotNet.Interactive.CSharp.CSharpKernel();

            // Interceptar #r "nuget:..." y otros comandos específicos de Horizonte
            csharpKernel.AddHorizonteMiddleware(env);

            // Obtener la versión del ensamblado Horizonte actualmente cargado
            var horizonteAssembly = typeof(IHorizonteEnv).Assembly;
            var horizonteVersion = horizonteAssembly.GetName().Version?.ToString() ?? "10.0.0";
            AssemblyNameInfo currenthorzonte = new AssemblyNameInfo("Horizonte", new Version(horizonteVersion));
            await csharpKernel.SendAsync(new SubmitCode($"#r \"nuget:{currenthorzonte.Name},{currenthorzonte.Version}\""));
            
            // Inyectar el entorno modular
            await csharpKernel.SetValueAsync("henv", env, typeof(IHorizonteEnv));

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

    private static void AddHorizonteMiddleware(this Microsoft.DotNet.Interactive.CSharp.CSharpKernel csharpKernel, IHorizonteEnv env)
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
                        var assemblyManager = env.GetService<IhAssemblyManager>();
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


    public static async Task<ScriptDefCompileResponse> Compile(this ScriptDef scriptDef, IHorizonteEnv env)
    {
        var response = new ScriptDefCompileResponse();
        try
        {
            var code = scriptDef.CodeText;
            var lines = code.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            var cleanCodeLines = new List<string>();
            var additionalReferences = new List<MetadataReference>();
            var assemblyManager = env.GetService<IhAssemblyManager>();

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
                        if (assembly != null && !string.IsNullOrEmpty(assembly.Location))
                        {
                            additionalReferences.Add(MetadataReference.CreateFromFile(assembly.Location));
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
            var assemblyName = Path.GetRandomFileName();

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
            var horizonteLoc = typeof(IHorizonteEnv).Assembly.Location;
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
            response.Assembly = Assembly.Load(ms.ToArray());
            return response;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            response.Errors = new List<string> { e.Message };
        }
        return response;
    }
    
}