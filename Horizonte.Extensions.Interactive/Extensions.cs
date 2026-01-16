using System.Reflection;
using System.Text.RegularExpressions;
using Horizonte;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;

namespace Horizonte.Extensions.Interactive;

public static class Extensions
{
    public static async Task<CompositeKernel> BuildKernelAsync(this KernelOptions kernelOptions, IHorizonteEnv env)
    {
        try
        {
            var csharpKernel = new CSharpKernel();

            // Interceptar #r "nuget:..."
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
                                    csharpKernel.AddAssemblyReferences(new[] { assembly.Location });
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
                            // Si la ruta no es absoluta, podríamos intentar resolverla relativa al RootPath, 
                            // pero el ejemplo del usuario es una ruta absoluta.
                            
                            if (File.Exists(dllPath))
                            {
                                csharpKernel.AddAssemblyReferences(new[] { dllPath });
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

            // Añadir referencia al ensamblado que contiene IHorizonteEnv
            csharpKernel.AddAssemblyReferences(new[] { typeof(IHorizonteEnv).Assembly.Location });

            // Inyectar el entorno modular
            await csharpKernel.SetValueAsync("env", env, typeof(IHorizonteEnv));

            // Asegurar que el namespace esté disponible
            await csharpKernel.SendAsync(new SubmitCode("using Horizonte;"));

            //aqui hay que implementar la función display para integrarla con el Widget...
            await csharpKernel.SendAsync(new SubmitCode("void display(object x) => Microsoft.DotNet.Interactive.KernelInvocationContext.Current?.Display(x);"));

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
}