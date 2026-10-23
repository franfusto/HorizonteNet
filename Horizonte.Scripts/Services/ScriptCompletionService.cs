using BlazorMonaco;
using BlazorMonaco.Languages;
using Horizonte;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Horizonte.Scripts.Services;

/// <summary>
/// Servicio que extrae tipos, métodos, propiedades y miembros de los ensamblados cargados en <see cref="IhAssemblyManager"/>
/// y los convierte en elementos de autocompletado (<see cref="CompletionItem"/>) para Monaco Editor.
/// </summary>
public class ScriptCompletionService : IScriptCompletionService
{
    
    private readonly IhAssemblyManager? _assemblyManager;
    private readonly ILogger<ScriptCompletionService>? _logger;
    private List<CompletionItem>? _cachedItems;
    private readonly object _lock = new();
    private readonly HashSet<string> _registeredLanguages = new(StringComparer.OrdinalIgnoreCase);
    private bool _disposed;

    /// <summary>
    /// Indica si el autocompletado está habilitado. Por defecto es true.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Habilita la provisión de sugerencias de autocompletado en tiempo de ejecución.
    /// </summary>
    public void StartCompletion()
    {
        IsEnabled = true;
        _logger?.LogInformation("ScriptCompletionService: Autocompletado iniciado/activado.");
    }

    /// <summary>
    /// Deshabilita la provisión de sugerencias de autocompletado en tiempo de ejecución.
    /// </summary>
    public void StopCompletion()
    {
        IsEnabled = false;
        _logger?.LogInformation("ScriptCompletionService: Autocompletado detenido/desactivado.");
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="ScriptCompletionService"/>.
    /// </summary>
    /// <param name="assemblyManager">El gestor de ensamblados de Horizonte.</param>
    /// <param name="logger">El servicio de logging opcional.</param>
    public ScriptCompletionService(IhAssemblyManager? assemblyManager, ILogger<ScriptCompletionService>? logger = null)
    {
        _assemblyManager = assemblyManager;
        _logger = logger;

        if (_assemblyManager != null)
        {
            _assemblyManager.DomainChanged += OnDomainChanged;
        }
    }

    private void OnDomainChanged()
    {
        InvalidateCache();
    }

    /// <summary>
    /// Invalida la caché de elementos de sugerencias para forzar su reindexación.
    /// </summary>
    public void InvalidateCache()
    {
        lock (_lock)
        {
            _cachedItems = null;
        }
        _logger?.LogInformation("ScriptCompletionService: Cache invalidada debido a cambios en los dominios de ensamblados.");
    }

    /// <summary>
    /// Registra el proveedor de autocompletado globalmente en Monaco Editor si aún no ha sido registrado para ese lenguaje.
    /// </summary>
    public async Task RegisterProviderAsync(IJSRuntime jsRuntime, string language = "csharp")
    {
        if (jsRuntime == null) return;

        lock (_lock)
        {
            if (_registeredLanguages.Contains(language))
            {
                return;
            }
            _registeredLanguages.Add(language);
        }

        try
        {
            var provider = new CompletionItemProvider(
                new List<string> { ".", " " },
                ProvideCompletionItemsAsync,
                null
            );

            await BlazorMonaco.Languages.Global.RegisterCompletionItemProvider(jsRuntime, language, provider);
            _logger?.LogInformation("ScriptCompletionService: CompletionItemProvider registrado exitosamente para el lenguaje '{Language}'.", language);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "ScriptCompletionService: Error al registrar CompletionItemProvider para el lenguaje '{Language}'.", language);
            lock (_lock)
            {
                _registeredLanguages.Remove(language);
            }
        }
    }

    /// <summary>
    /// Provee la lista de elementos de autocompletado en la posición actual del modelo de texto.
    /// </summary>
    public Task<CompletionList> ProvideCompletionItemsAsync(string modelUri, Position position, CompletionContext context)
    {
        if (!IsEnabled)
        {
            return Task.FromResult(new CompletionList
            {
                Suggestions = new List<CompletionItem>()
            });
        }

        var items = GetOrBuildCompletionItems();
        return Task.FromResult(new CompletionList
        {
            Suggestions = items
        });
    }

    /// <summary>
    /// Obtiene o construye la lista de elementos de autocompletado a partir de los ensamblados cargados.
    /// </summary>
    public List<CompletionItem> GetOrBuildCompletionItems()
    {
        lock (_lock)
        {
            if (_cachedItems != null)
            {
                return _cachedItems;
            }

            _cachedItems = BuildCompletionItems();
            return _cachedItems;
        }
    }

    private static JsonElement ToJsonElement(object value)
    {
        return JsonSerializer.SerializeToElement(value);
    }

    private List<CompletionItem> BuildCompletionItems()
    {
        var items = new List<CompletionItem>();
        var seenSignatures = new HashSet<string>(StringComparer.Ordinal);
        var seenNamespaces = new HashSet<string>(StringComparer.Ordinal);

        if (_assemblyManager == null)
        {
            return items;
        }

        // Obtener todos los ensamblados gestionados y cargados
        var assemblies = new HashSet<Assembly>();
        try
        {
            if (_assemblyManager.Assemblies != null)
            {
                foreach (var asm in _assemblyManager.Assemblies)
                {
                    if (asm != null) assemblies.Add(asm);
                }
            }

            if (_assemblyManager.AssembliesByDomain != null)
            {
                foreach (var domainList in _assemblyManager.AssembliesByDomain.Values)
                {
                    if (domainList != null)
                    {
                        foreach (var asm in domainList)
                        {
                            if (asm != null) assemblies.Add(asm);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger?.LogWarning(ex, "Error al recopilar ensamblados desde IhAssemblyManager");
        }

        foreach (var asm in assemblies)
        {
            Type[] types;
            try
            {
                types = asm.GetExportedTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
            }
            catch
            {
                try
                {
                    types = asm.GetTypes().Where(t => t != null && t.IsPublic).ToArray();
                }
                catch
                {
                    continue;
                }
            }

            foreach (var type in types)
            {
                if (type == null || !type.IsPublic) continue;

                // Registrar Namespace si no se ha agregado
                if (!string.IsNullOrEmpty(type.Namespace) && seenNamespaces.Add(type.Namespace))
                {
                    items.Add(new CompletionItem
                    {
                        Label = ToJsonElement(type.Namespace),
                        Kind = CompletionItemKind.Module,
                        InsertText = type.Namespace,
                        Detail = $"namespace {type.Namespace}",
                        Documentation = ToJsonElement(new MarkdownString { Value = $"Espacio de nombres `{type.Namespace}`" })
                    });
                }

                // Determinar Kind de tipo
                var kind = CompletionItemKind.Class;
                var typeDesc = "class";
                if (type.IsInterface)
                {
                    kind = CompletionItemKind.Interface;
                    typeDesc = "interface";
                }
                else if (type.IsEnum)
                {
                    kind = CompletionItemKind.Enum;
                    typeDesc = "enum";
                }
                else if (type.IsValueType)
                {
                    kind = CompletionItemKind.Struct;
                    typeDesc = "struct";
                }

                var typeDisplayName = GetCleanTypeName(type);
                var typeKey = $"T:{type.FullName ?? type.Name}";

                if (seenSignatures.Add(typeKey))
                {
                    var doc = $"**{typeDesc}** `{type.FullName ?? type.Name}`\n\n*Ensamblado:* `{asm.GetName().Name}`";
                    items.Add(new CompletionItem
                    {
                        Label = ToJsonElement(typeDisplayName),
                        Kind = kind,
                        InsertText = typeDisplayName,
                        Detail = $"({typeDesc}) {type.FullName ?? type.Name}",
                        Documentation = ToJsonElement(new MarkdownString { Value = doc })
                    });
                }

                // Si es un Enum, exportar sus miembros
                if (type.IsEnum)
                {
                    try
                    {
                        var names = Enum.GetNames(type);
                        foreach (var enumName in names)
                        {
                            var enumKey = $"E:{type.FullName}.{enumName}";
                            if (seenSignatures.Add(enumKey))
                            {
                                items.Add(new CompletionItem
                                {
                                    Label = ToJsonElement($"{typeDisplayName}.{enumName}"),
                                    Kind = CompletionItemKind.EnumMember,
                                    InsertText = $"{typeDisplayName}.{enumName}",
                                    Detail = $"{type.FullName}.{enumName}",
                                    Documentation = ToJsonElement(new MarkdownString { Value = $"Valor enum `{enumName}` del tipo `{type.FullName}`" })
                                });
                            }
                        }
                    }
                    catch { }
                    continue;
                }

                // Extraer métodos públicos
                try
                {
                    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var method in methods)
                    {
                        if (method.IsSpecialName) continue; // get_, set_, etc.

                        var paramList = method.GetParameters();
                        var paramStr = string.Join(", ", paramList.Select(p => $"{GetCleanTypeName(p.ParameterType)} {p.Name}"));
                        var returnTypeName = GetCleanTypeName(method.ReturnType);
                        var methodSig = $"{method.Name}({paramStr})";
                        var methodKey = $"M:{type.FullName}.{method.Name}({string.Join(",", paramList.Select(p => p.ParameterType.Name))})";

                        if (seenSignatures.Add(methodKey))
                        {
                            var isStatic = method.IsStatic ? "static " : "";
                            items.Add(new CompletionItem
                            {
                                Label = ToJsonElement(method.Name),
                                Kind = CompletionItemKind.Method,
                                InsertText = method.Name,
                                Detail = $"{isStatic}{returnTypeName} {typeDisplayName}.{methodSig}",
                                Documentation = ToJsonElement(new MarkdownString { Value = $"Método `{isStatic}{returnTypeName} {methodSig}`\n\nPerteneciente a `{type.FullName}`" })
                            });
                        }
                    }
                }
                catch { }

                // Extraer propiedades públicas
                try
                {
                    var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                    foreach (var prop in properties)
                    {
                        var propTypeName = GetCleanTypeName(prop.PropertyType);
                        var propKey = $"P:{type.FullName}.{prop.Name}";

                        if (seenSignatures.Add(propKey))
                        {
                            items.Add(new CompletionItem
                            {
                                Label = ToJsonElement(prop.Name),
                                Kind = CompletionItemKind.Property,
                                InsertText = prop.Name,
                                Detail = $"{propTypeName} {typeDisplayName}.{prop.Name}",
                                Documentation = ToJsonElement(new MarkdownString { Value = $"Propiedad `{propTypeName} {prop.Name}`\n\nPerteneciente a `{type.FullName}`" })
                            });
                        }
                    }
                }
                catch { }
            }
        }

        return items;
    }

    private static string GetCleanTypeName(Type type)
    {
        if (type == typeof(void)) return "void";
        if (type == typeof(string)) return "string";
        if (type == typeof(int)) return "int";
        if (type == typeof(long)) return "long";
        if (type == typeof(bool)) return "bool";
        if (type == typeof(double)) return "double";
        if (type == typeof(float)) return "float";
        if (type == typeof(decimal)) return "decimal";
        if (type == typeof(object)) return "object";
        if (type == typeof(byte)) return "byte";
        if (type == typeof(char)) return "char";

        if (type.IsGenericType)
        {
            var genericName = type.Name;
            var backtickIdx = genericName.IndexOf('`');
            if (backtickIdx > 0)
            {
                genericName = genericName.Substring(0, backtickIdx);
            }
            var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetCleanTypeName));
            return $"{genericName}<{genericArgs}>";
        }

        return type.Name;
    }

    /// <summary>
    /// Libera los recursos y desuscribe los eventos de cambio de dominio.
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            if (_assemblyManager != null)
            {
                _assemblyManager.DomainChanged -= OnDomainChanged;
            }
        }
    }
}
