using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Horizonte.Extension.AspNetCore;

/// <summary>
/// Enables serving static files for a given request path
/// </summary>
public class HorizonteStaticFileMiddelware
{
    private readonly StaticFileOptions _options;
    private readonly PathString _matchUrl;
    private readonly RequestDelegate _next;
    private readonly ILogger<HorizonteStaticFileMiddelware> _logger;
    private readonly IFileProvider _fileProvider;
    private readonly IContentTypeProvider _contentTypeProvider;
    private readonly IhAssemblyManager _assemblyManager;


    /// <summary>
    /// Creates a new instance of the StaticFileMiddleware.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="hostingEnv">The <see cref="IWebHostEnvironment"/> used by this middleware.</param>
    /// <param name="options">The configuration options.</param>
    /// <param name="loggerFactory">An <see cref="ILoggerFactory"/> instance used to create loggers.</param>
    public HorizonteStaticFileMiddelware(RequestDelegate next, IWebHostEnvironment hostingEnv,
        IOptions<StaticFileOptions> options, ILogger<HorizonteStaticFileMiddelware> logger,IhAssemblyManager assemblyManager)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(hostingEnv);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(assemblyManager);
        _assemblyManager = assemblyManager;
        _next = next;
        _options = options.Value;
        _contentTypeProvider = _options.ContentTypeProvider ?? new FileExtensionContentTypeProvider();
        _matchUrl = _options.RequestPath;
        _logger = logger;
        _fileProvider = _options.FileProvider ?? ResolveFileProvider(hostingEnv);

        // See HostingEnvironmentExtensions.Initialize
        if (_fileProvider is NullFileProvider && _fileProvider == hostingEnv.WebRootFileProvider)
        {
            _logger.LogWarning(
                "The web root file provider is not available, falling back to the content root file provider.");
        }
    }


    internal static IFileProvider ResolveFileProvider(IWebHostEnvironment hostingEnv)
    {
        if (hostingEnv.WebRootFileProvider == null)
        {
            throw new InvalidOperationException("Missing FileProvider.");
        }

        return hostingEnv.WebRootFileProvider;
    }


    /// <summary>
    /// Processes a request to determine if it matches a known file, and if so, serves it.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task Invoke(HttpContext context)
    {
        if (!ValidateNoEndpointDelegate(context))
        {
            _logger.LogWarning("Endpoint matched");
        }
        else if (!ValidateMethod(context))
        {
            _logger.LogWarning($"Method not supported {context.Request.Method}");
        }
        else if (!ValidatePath(context, _matchUrl, out var subPath))
        {
            _logger.LogWarning($"Path not supported {context.Request.Path}");
        }
        else if (!LookupContentType(_contentTypeProvider, _options, subPath, out var contentType))
        {
            _logger.LogWarning($"Content type not supported {subPath}");
        }
        else
        {
            // If we get here, we can try to serve the file
            return TryServeStaticFile(context, contentType, subPath);
        }

        return _next(context);
    }

    // Return true because we only want to run if there is no endpoint delegate.
    private static bool ValidateNoEndpointDelegate(HttpContext context) =>
        context.GetEndpoint()?.RequestDelegate is null;

    private static bool ValidateMethod(HttpContext context)
    {
        return HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
    }

    internal static bool ValidatePath(HttpContext context, PathString matchUrl, out PathString subPath)
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments(matchUrl, out subPath))
        {
            return true;
        }

        return false;
    }

    internal static bool LookupContentType(IContentTypeProvider contentTypeProvider, StaticFileOptions options,
        PathString subPath, out string? contentType)
    {
        if (contentTypeProvider.TryGetContentType(subPath.Value!, out contentType))
        {
            return true;
        }

        if (options.ServeUnknownFileTypes)
        {
            contentType = options.DefaultContentType;
            return true;
        }

        return false;
    }

    private async Task TryServeStaticFile(HttpContext context, string? contentType, PathString subPath)
    {
        if (subPath.Value == null) return;
        FileInfo? fileInfo = _assemblyManager.StaticFileRegistry.GetFile(subPath.Value);
        if (fileInfo != null)
        {
            context.Response.ContentType = contentType;
            context.Response.ContentLength = fileInfo.Length;
            var sendFile = context.Response.HttpContext.Features.GetRequiredFeature<IHttpResponseBodyFeature>();
            await sendFile.SendFileAsync(fileInfo.FullName, 0, fileInfo.Length, default);
        }
        else
        {
            _logger.LogWarning($"File not found {subPath}");
        }

        await _next(context);
    }
}