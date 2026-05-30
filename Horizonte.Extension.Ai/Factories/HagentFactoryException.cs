namespace Horizonte.Extension.Ai.Factories;

public sealed class HagentFactoryException : InvalidOperationException
{
    public HagentFactoryException(string message)
        : base(message)
    {
    }

    public HagentFactoryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}