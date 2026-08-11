using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Explore;

public class Program
{
    public static void Main()
    {
        try
        {
             // Try to find the AddMcpServer method and its containing assembly
             var method = typeof(ServiceCollectionServiceExtensions).Assembly.GetType("Microsoft.Extensions.DependencyInjection.McpServerServiceCollectionExtensions");
             if (method == null)
             {
                 Console.WriteLine("Could not find McpServerServiceCollectionExtensions in Microsoft.Extensions.DependencyInjection");
                 // Maybe it's in another assembly. Let's look for assemblies with "ModelContextProtocol" in name.
                 foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                 {
                     if (assembly.FullName.Contains("ModelContextProtocol"))
                     {
                         Console.WriteLine($"Found assembly: {assembly.FullName}");
                         foreach (var type in assembly.GetTypes())
                         {
                             if (type.IsPublic) Console.WriteLine($"Type: {type.FullName}");
                         }
                     }
                 }
             }
             else
             {
                 Console.WriteLine($"Found extension class: {method.FullName}");
                 var assembly = method.Assembly;
                 foreach (var type in assembly.GetTypes())
                 {
                     if (type.IsPublic) Console.WriteLine($"Type: {type.FullName}");
                 }
             }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
