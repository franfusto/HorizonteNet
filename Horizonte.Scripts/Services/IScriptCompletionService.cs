using BlazorMonaco;
using BlazorMonaco.Languages;
using Microsoft.JSInterop;

namespace Horizonte.Scripts.Services;

/// <summary>
/// Interfaz para el servicio de autocompletado e indexación de sugerencias para scripts en Monaco Editor.
/// </summary>
public interface IScriptCompletionService : IDisposable
{
    /// <summary>
    /// Indica si el autocompletado está habilitado.
    /// </summary>
    bool IsEnabled { get; set; }

    /// <summary>
    /// Inicia/habilita la provisión de elementos de autocompletado.
    /// </summary>
    void StartCompletion();

    /// <summary>
    /// Detiene/deshabilita la provisión de elementos de autocompletado.
    /// </summary>
    void StopCompletion();

    /// <summary>
    /// Provee la lista de elementos de autocompletado en la posición actual del modelo de texto.
    /// </summary>
    /// <param name="modelUri">El identificador de URI del modelo de texto.</param>
    /// <param name="position">La posición actual del cursor.</param>
    /// <param name="context">El contexto de invocación de autocompletado.</param>
    /// <returns>Objeto <see cref="CompletionList"/> con las sugerencias disponibles.</returns>
    Task<CompletionList> ProvideCompletionItemsAsync(string modelUri, Position position, CompletionContext context);

    /// <summary>
    /// Invalida la caché de elementos de sugerencias para forzar su reindexación.
    /// </summary>
    void InvalidateCache();

    /// <summary>
    /// Registra el proveedor de autocompletado globalmente en Monaco Editor si aún no ha sido registrado.
    /// </summary>
    /// <param name="jsRuntime">Instancia de <see cref="IJSRuntime"/> para registrar el proveedor vía JS interop.</param>
    /// <param name="language">El lenguaje al cual asociar las sugerencias (por defecto "csharp").</param>
    Task RegisterProviderAsync(IJSRuntime jsRuntime, string language = "csharp");
}
