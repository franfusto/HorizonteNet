using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Horizonte;
using System.Threading;
using Horizonte.Ai.MCPServer.McpResources;
using Horizonte.Ai.MCPServer.Middleware;
using Horizonte.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;

namespace Horizonte.Ai.MCPServer;

public class MCPWorker : BackgroundService
{
    public bool IsRunning { get; set; } = false;

    private readonly ILogger<MCPWorker> _log;
    private readonly IHGesCom _gesCom;
    private readonly IhContext _context;
    private WebApplication? _app;

    public MCPWorker(ILogger<MCPWorker> logger, IHGesCom gesCom, IhContext context)
    {
        _log = logger;
        _gesCom = gesCom;   
        _context = context;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log.LogInformation("Iniciando MCPWorker");
        
        var cfg = _context.Get<MCPServerConfig>() ?? new();
        var builder = WebApplication.CreateBuilder();

        ConfigureLogging(builder);
        ConfigureCustomServices(builder.Services, cfg);
        ConfigureMcpServer(builder.Services, cfg);
        
        _app = builder.Build();
        
        ConfigureMiddleware(_app, cfg);
        
        _app.MapMcp("/" + cfg.Path);

        await _app.RunAsync(cfg.Url);
    }

    private void ConfigureLogging(WebApplicationBuilder builder)
    {
        if (_log != null)
        {
            builder.Logging.ClearProviders();
            builder.Logging.AddProvider(new ExistingLoggerProvider(_log));
            // builder.Logging.AddFilter("Microsoft", LogLevel.Error); 
            // builder.Logging.AddFilter("Horizonte", LogLevel.Trace); 
            builder.Logging.SetMinimumLevel(LogLevel.Information);
            builder.Services.AddSingleton(_log);
        }
    }

    private void ConfigureCustomServices(IServiceCollection services, MCPServerConfig cfg)
    {
        services.AddSingleton<IHGesCom>(_gesCom);
        services.AddSingleton<IhContext>(_context);
        
        foreach (var svc in cfg.Services)
        {
            if (string.IsNullOrWhiteSpace(svc.ImplementationType))
            {
                _log.LogWarning("Se encontró una definición de servicio sin ImplementationType especificado.");
                continue;
            }

            var implType = Type.GetType(svc.ImplementationType.Trim());
            if (implType == null)
            {
                _log.LogWarning("No se pudo resolver el tipo de implementación del servicio: {ImplementationType}", svc.ImplementationType);
                continue;
            }

            Type? ifaceType = null;
            if (!string.IsNullOrWhiteSpace(svc.InterfaceType))
            {
                ifaceType = Type.GetType(svc.InterfaceType.Trim());
                if (ifaceType == null)
                {
                    _log.LogWarning("No se pudo resolver el tipo de interfaz del servicio: {InterfaceType}", svc.InterfaceType);
                    continue;
                }
            }

            var descriptor = ifaceType != null
                ? new ServiceDescriptor(ifaceType, implType, svc.Scope)
                : new ServiceDescriptor(implType, implType, svc.Scope);

            services.Add(descriptor);
            _log.LogInformation("Registrado servicio {ServiceType} -> {ImplementationType} con ciclo de vida {Scope}",
                ifaceType?.FullName ?? implType.FullName, implType.FullName, svc.Scope);
        }
    }

    private void ConfigureMcpServer(IServiceCollection services, MCPServerConfig cfg)
    {
        var mcpBuilder = services
            .AddMcpServer()
            .WithHttpTransport()
            .WithCommands(cfg, _gesCom, _log);

        ConfigureResources(mcpBuilder, cfg);
        ConfigurePrompts(mcpBuilder, cfg);
    }

    private void ConfigureResources(IMcpServerBuilder mcpBuilder, MCPServerConfig cfg)
    {
        foreach (var resourceTypeName in cfg.ResourcesTypes)
        {
            if (string.IsNullOrWhiteSpace(resourceTypeName))
            {
                continue;
            }

            var resourceType = Type.GetType(resourceTypeName.Trim());
            if (resourceType == null)
            {
                _log.LogWarning("No se pudo resolver el tipo de recurso MCP: {ResourceType}", resourceTypeName);
                continue;
            }

            mcpBuilder.WithResources(resourceType);
            _log.LogInformation("Registrado recurso MCP: {ResourceType}", resourceType.FullName);
        }
    }

    private void ConfigurePrompts(IMcpServerBuilder mcpBuilder, MCPServerConfig cfg)
    {
        foreach (var promptTypeName in cfg.PromptsTypes)
        {
            if (string.IsNullOrWhiteSpace(promptTypeName))
            {
                continue;
            }

            var promptType = Type.GetType(promptTypeName.Trim());
            if (promptType == null)
            {
                _log.LogWarning("No se pudo resolver el tipo de prompt MCP: {PromptType}", promptTypeName);
                continue;
            }

            mcpBuilder.WithPrompts(promptType);
            _log.LogInformation("Registrado prompt MCP: {PromptType}", promptType.FullName);
        }
    }

    private void ConfigureMiddleware(WebApplication app, MCPServerConfig cfg)
    {
        if (!string.IsNullOrWhiteSpace(cfg.ValidatorMiddlewareType))
        {
            var middlewareType = Type.GetType(cfg.ValidatorMiddlewareType.Trim());
            if (middlewareType != null)
            {
                _log.LogInformation("Registrando middleware de validación: {MiddlewareType}", middlewareType.FullName);
                app.UseMiddleware(middlewareType);
            }
            else
            {
                _log.LogWarning("No se pudo resolver el tipo de middleware de validación: {ValidatorMiddlewareType}", cfg.ValidatorMiddlewareType);
            }
        }
        else
        {
            _log.LogInformation("No se configuró middleware de validación (ValidatorMiddlewareType vacío).");
        }
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Starting MCPWorker");
        return base.StartAsync(cancellationToken);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _log.LogInformation("Finalizando MCPWorker");
        if (_app != null)
        {
            await _app.StopAsync(cancellationToken);
        }
        await base.StopAsync(cancellationToken);
    }
}