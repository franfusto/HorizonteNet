
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace YourNamespace.Middleware
{
    public class UnmanagedDllRedirectMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Dictionary<string, string> _dllPathMappings;
        private readonly ILogger<UnmanagedDllRedirectMiddleware> _logger;
        private readonly string _basePath;

        public UnmanagedDllRedirectMiddleware(
            RequestDelegate next, 
            Dictionary<string, string> dllPathMappings,
            ILogger<UnmanagedDllRedirectMiddleware> logger,
            IWebHostEnvironment environment)
        {
            _next = next;
            _dllPathMappings = dllPathMappings;
            _logger = logger;
            _basePath = environment.ContentRootPath;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var requestPath = context.Request.Path.Value?.TrimStart('/');
            
            if (!string.IsNullOrEmpty(requestPath))
            {
                var normalizedRequestPath = requestPath.Replace('/', '\\');
                string realPath = null;
                
                // Primero intentar buscar en el diccionario
                if (_dllPathMappings.TryGetValue(normalizedRequestPath, out var mappedPath))
                {
                    // Resolver ruta relativa a absoluta desde el mapeo
                    realPath = Path.IsPathRooted(mappedPath) 
                        ? mappedPath 
                        : Path.Combine(_basePath, mappedPath);
                    
                    _logger.LogInformation($"Ruta encontrada en diccionario: {normalizedRequestPath} -> {realPath}");
                }
                else
                {
                    // Si no está en el diccionario, intentar con la ruta original solicitada
                    realPath = Path.IsPathRooted(normalizedRequestPath) 
                        ? normalizedRequestPath 
                        : Path.Combine(_basePath, normalizedRequestPath);
                    
                    _logger.LogDebug($"Ruta no encontrada en diccionario, intentando ruta original: {realPath}");
                }
                
                // Verificar si el archivo existe y servirlo
                if (!string.IsNullOrEmpty(realPath) && File.Exists(realPath))
                {
                    _logger.LogInformation($"Sirviendo DLL: {requestPath} desde {realPath}");
                    
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.ContentLength = new FileInfo(realPath).Length;
                    
                    await context.Response.SendFileAsync(realPath);
                    return;
                }
            }
            
            // Si no se encontró el archivo o no es una solicitud de DLL, continuar con el pipeline
            await _next(context);
        }
    }
}