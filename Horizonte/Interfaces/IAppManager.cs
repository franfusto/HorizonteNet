namespace Horizonte.Interfaces;

/// <summary>
/// Interfaz que define las funcionalidades para gestionar el ciclo de vida de la aplicación.
/// </summary>
public interface IAppManager
{
    /// <summary>
    /// Reinicia la aplicación.
    /// </summary>
    Task Reboot();

    /// <summary>
    /// Cierra la aplicación de forma controlada.
    /// </summary>
    Task Quit();
}