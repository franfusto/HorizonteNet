using System.Reflection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

namespace Horizonte.Extension.AspNetCore;

/// <summary>
/// Clase <c>HorizonteStaticFileMiddelware</c> que representa un middleware para la gestión de archivos estáticos en una aplicación ASP.NET Core.
/// </summary>
/// <remarks>
/// Este middleware se integra con el pipeline del servidor para proporcionar archivos estáticos desde el sistema de archivos configurado.
/// </remarks>

public class HorizonteStaticFileMiddelware
{
    /// <summary>
    /// Variable privada que almacena las opciones de configuración para el manejo de archivos estáticos.
    /// </summary>
    /// <remarks>
    /// Esta variable se utiliza para mantener las configuraciones específicas, como el proveedor de archivos,
    /// el proveedor de tipos de contenido y la ruta de solicitud para el manejo de archivos estáticos en la aplicación.
    /// </remarks>
    private readonly StaticFileOptions _options;

    /// <summary>
    /// Variable privada que representa la URL de coincidencia para el middleware de archivos estáticos en la aplicación.
    /// </summary>
    /// <remarks>
    /// Esta variable se utiliza para comparar y validar las rutas de las solicitudes entrantes.
    /// Se inicializa con la ruta de solicitud especificada en las opciones de archivos estáticos.
    /// </remarks>
    private readonly PathString _matchUrl;

    /// <summary>
    /// Delegado de la siguiente función en la cadena del middleware de ASP.NET Core.
    /// </summary>
    private readonly RequestDelegate _next;

    /// <summary>
    /// Representa el logger utilizado en el middleware de HorizonteStaticFile para registrar información,
    /// advertencias y mensajes de error durante el procesamiento de solicitudes HTTP relacionadas
    /// con archivos estáticos en la aplicación.
    /// </summary>
    private readonly ILogger<HorizonteStaticFileMiddelware> _logger;

    /// Proveedor de archivos utilizado para acceder a los recursos estáticos en el middleware de Horizonte.
    /// Es una instancia de `IFileProvider` que proporciona una abstracción para acceder al sistema de archivos.
    /// Si no se especifica un proveedor de archivos en las opciones, se utiliza un proveedor determinado por el entorno de hospedaje, típicamente el proveedor de archivos raíz web.
    /// En casos donde el proveedor de archivos raíz web no está disponible, se emite una advertencia en los registros y se recurre al proveedor de archivos raíz de contenido.
    /// /
    private readonly IFileProvider _fileProvider;

    /// <summary>
    /// Variable privada que representa una instancia de <see cref="IContentTypeProvider"/>.
    /// Este proveedor es utilizado para determinar el tipo de contenido (MIME type)
    /// en función de la extensión del archivo solicitado en un entorno de servidor ASP.NET Core.
    /// </summary>
    private readonly IContentTypeProvider _contentTypeProvider;

    /// <summary>
    /// Variable privada que representa una instancia de <see cref="IhAssemblyManager"/>.
    /// Se utiliza para gestionar y manipular ensamblados en el contexto del middleware de archivos estáticos de Horizonte.
    /// </summary>
    private readonly IhAssemblyManager _assemblyManager;

    /// <summary>
    /// Representa el nombre de dominio registrado para el middleware de archivos estáticos en el contexto de la aplicación.
    /// Utilizado para la resolución de archivos estáticos asociados a un dominio específico.
    /// Este valor es obtenido a partir del ensamblado que está registrando el middleware.
    /// Si no se proporciona un nombre de dominio, se utiliza un valor predeterminado.
    /// </summary>
    private readonly string? _domainName;


    /// <summary>
    /// Representa un middleware para manejar archivos estáticos en una aplicación ASP.NET Core,
    /// específicamente diseñado para ser utilizado con el componente "Horizonte".
    /// </summary>
    public HorizonteStaticFileMiddelware(RequestDelegate next, IWebHostEnvironment hostingEnv,
        IOptions<StaticFileOptions> options, ILogger<HorizonteStaticFileMiddelware> logger,IhAssemblyManager assemblyManager,
        Assembly? registeringAssembly = null)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(hostingEnv);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentNullException.ThrowIfNull(assemblyManager);
        _assemblyManager = assemblyManager;
        _domainName = ResolveRegisteringAssemblyDomain(registeringAssembly, assemblyManager);
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

        _logger.LogInformation(
            "Horizonte static file middleware registered for domain {DomainName}",
            _domainName ?? Const.DefaultDomainName);
    }


    /// <summary>
    /// Resuelve el proveedor de archivos a utilizar para el entorno de alojamiento web dado.
    /// </summary>
    /// <param name="hostingEnv">
    /// El entorno de alojamiento web que contiene la información sobre el proveedor de archivos de la raíz web.
    /// </param>
    /// <returns>Devuelve una implementación de <see cref="IFileProvider"/> que representa el proveedor de archivos de la raíz web.</returns>
    /// <exception cref="InvalidOperationException">
    /// Se lanza cuando no se encuentra disponible un proveedor de archivos para la raíz web en el entorno de alojamiento proporcionado.
    /// </exception>
    internal static IFileProvider ResolveFileProvider(IWebHostEnvironment hostingEnv)
    {
        if (hostingEnv.WebRootFileProvider == null)
        {
            throw new InvalidOperationException("Missing FileProvider.");
        }

        return hostingEnv.WebRootFileProvider;
    }


    /// <summary>
    /// Método encargado de procesar la solicitud HTTP entrante y, potencialmente, servir archivos estáticos
    /// si se cumplen ciertos criterios. Si no se puede servir un archivo, procede
    /// a la siguiente etapa del middleware en la cadena de procesamiento.
    /// </summary>
    /// <param name="context">
    /// El <see cref="HttpContext"/> que representa toda la información de la solicitud HTTP actual.
    /// </param>
    /// <returns>
    /// Un <see cref="Task"/> que representa la operación asincrónica en curso,
    /// y el direccionamiento hacia el siguiente middleware si no es posible servir un archivo estático.
    /// </returns>
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
    /// <summary>
    /// Valida si un contexto <see cref="HttpContext"/> no tiene un delegado de endpoint asociado.
    /// </summary>
    /// <param name="context">El contexto HTTP actual que se está procesando.</param>
    /// <returns>Devuelve <c>true</c> si el contexto no tiene un delegado de endpoint asociado; de lo contrario, <c>false</c>.</returns>
    private static bool ValidateNoEndpointDelegate(HttpContext context) =>
        context.GetEndpoint()?.RequestDelegate is null;

    /// <summary>
    /// Valida si el método HTTP de la solicitud es compatible con las operaciones de archivos estáticos específicas.
    /// </summary>
    /// <param name="context">El contexto HTTP actual, que contiene la solicitud que se está procesando.</param>
    /// <returns>
    /// Retorna <c>true</c> si el método de la solicitud es GET o HEAD, indicando que es adecuado para la entrega de archivos estáticos;
    /// de lo contrario, retorna <c>false</c>.
    /// </returns>
    private static bool ValidateMethod(HttpContext context)
    {
        return HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsHead(context.Request.Method);
    }

    /// <summary>
    /// Valida si la ruta de la solicitud HTTP comienza con un segmento específico.
    /// </summary>
    /// <param name="context">El contexto HTTP que contiene la información de la solicitud.</param>
    /// <param name="matchUrl">El segmento de ruta que se espera coincidir al inicio de la ruta de la solicitud.</param>
    /// <param name="subPath">Cuando este método devuelve, contiene el subcamino que sigue al segmento coincidente, si la coincidencia fue exitosa.</param>
    /// <returns>Devuelve <c>true</c> si la ruta de la solicitud comienza con el segmento especificado; de lo contrario, <c>false</c>.</returns>
    internal static bool ValidatePath(HttpContext context, PathString matchUrl, out PathString subPath)
    {
        var path = context.Request.Path;
        if (path.StartsWithSegments(matchUrl, out subPath))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determina el tipo de contenido para un archivo especificado en función de su extensión.
    /// </summary>
    /// <param name="contentTypeProvider">
    /// Proveedor de tipos de contenido que se utiliza para asociar extensiones de archivo con tipos MIME.
    /// </param>
    /// <param name="options">
    /// Opciones de archivo estático que pueden influir en el comportamiento del método, como el manejo de tipos de archivo desconocidos.
    /// </param>
    /// <param name="subPath">
    /// Ruta del archivo para el cual se quiere determinar el tipo de contenido.
    /// </param>
    /// <param name="contentType">
    /// Devuelve el tipo MIME asociado al archivo especificado, o el tipo por defecto si se permiten tipos de archivo desconocidos.
    /// </param>
    /// <returns>
    /// Verdadero si se determina correctamente un tipo de contenido; de lo contrario, falso si el tipo de archivo es desconocido y no se permite servir tales archivos.
    /// </returns>
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

    /// Intenta servir un archivo estático al cliente. Este método verifica si el archivo solicitado existe
    /// y, de ser así, lo envía como respuesta. Si no se encuentra el archivo, se registra una advertencia.
    /// <param name="context">El contexto HTTP que contiene toda la información de la solicitud HTTP actual.</param>
    /// <param name="contentType">El tipo de contenido del archivo que se desea servir, usado para establecer el encabezado Content-Type en la respuesta.</param>
    /// <param name="subPath">La ruta parcial al archivo solicitado, utilizada para localizar el archivo dentro del sistema de archivos.</param>
    /// <return>Una tarea que representa la operación asincrónica de servir el archivo HTTP.</return>
    private async Task TryServeStaticFile(HttpContext context, string? contentType, PathString subPath)
    {
        if (subPath.Value == null) return;
        FileInfo? fileInfo = _assemblyManager.StaticFileRegistry.GetFile(subPath.Value, _domainName);
        if (fileInfo != null)
        {
            context.Response.ContentType = contentType;
            context.Response.ContentLength = fileInfo.Length;
            var sendFile = context.Response.HttpContext.Features.GetRequiredFeature<IHttpResponseBodyFeature>();
            await sendFile.SendFileAsync(fileInfo.FullName, 0, fileInfo.Length, default);
        }
        else
        {
            _logger.LogWarning($"File not found {subPath} for domain {_domainName ?? Const.DefaultDomainName}");
        }

        await _next(context);
    }

    /// <summary>
    /// Resuelve el dominio de ensamblaje de registro proporcionado, utilizando un administrador de ensamblajes para determinar
    /// el nombre del dominio asociado al ensamblaje de registro.
    /// </summary>
    /// <param name="registeringAssembly">
    /// El ensamblaje que se está registrando. Si es nulo, el método devuelve nulo.
    /// </param>
    /// <param name="assemblyManager">
    /// El administrador de ensamblajes que contiene información sobre los dominios y sus ensamblajes asociados.
    /// </param>
    /// <returns>
    /// El nombre del dominio asociado al ensamblaje de registro si se encuentra uno coincidente; de lo contrario, nulo.
    /// </returns>
    private static string? ResolveRegisteringAssemblyDomain(
        Assembly? registeringAssembly,
        IhAssemblyManager assemblyManager)
    {
        if (registeringAssembly == null)
            return null;

        foreach (var domain in assemblyManager.AssembliesByDomain)
        {
            if (domain.Value.Any(assembly => ReferenceEquals(assembly, registeringAssembly)))
            {
                return domain.Key;
            }

            if (domain.Value.Any(assembly =>
                    string.Equals(assembly.FullName, registeringAssembly.FullName, StringComparison.Ordinal)))
            {
                return domain.Key;
            }
        }

        return null;
    }
}