namespace Horizonte.Extension.Ai.Factories;

/// <summary>
/// Excepción que se lanza cuando se produce un error durante la resolución, validación o instanciación de un agente en <see cref="HagentFactory"/>.
/// </summary>
public sealed class HagentFactoryException : InvalidOperationException
{
    /// <summary>
    /// Inicializa una nueva instancia de <see cref="HagentFactoryException"/> con un mensaje de error especificado.
    /// </summary>
    /// <param name="message">El mensaje descriptivo del error.</param>
    public HagentFactoryException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Inicializa una nueva instancia de <see cref="HagentFactoryException"/> con un mensaje de error y una excepción interna.
    /// </summary>
    /// <param name="message">El mensaje descriptivo del error.</param>
    /// <param name="innerException">La excepción subyacente que causó este error.</param>
    public HagentFactoryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}