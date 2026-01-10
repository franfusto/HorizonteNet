using Horizonte;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;

namespace Horizonte.Extensions.Interactive;

public static class Extensions
{
    //public static void ConfigureLog4Net(this HostApplicationBuilder builder, HContext context)
    public static CompositeKernel BuildKernel(this KernelOptions kernelOptions, IHorizonteEnv env)
    {
        
        var csharpKernel = new CSharpKernel();

        var kernel = new CompositeKernel
        {
            csharpKernel
        };
        return kernel;

// Opcional pero recomendado
        kernel.DefaultKernelName = csharpKernel.Name;
    }
}