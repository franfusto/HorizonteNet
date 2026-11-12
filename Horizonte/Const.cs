namespace Horizonte;

/// <summary>
/// La clase <c>Const</c> proporciona constantes utilizadas en el dominio de la
/// aplicación para configurar nombres de dominios y marcos de trabajo.
/// </summary>
public static class Const
{
    /// <summary>
    /// Define el nombre del dominio utilizado por defecto para identificar un contexto de ejecución de scripts en Horizonte ("Script").
    /// </summary>
    public const string ScriptDomainName = "Script";

    /// <summary>
    /// Constante que representa el nombre del dominio predeterminado utilizado en el contexto de la aplicación.
    /// </summary>
    /// <remarks>
    /// El dominio predeterminado, identificado por esta constante, es utilizado principalmente
    /// para cargar y gestionar ensamblados en el `AssemblyLoadContext` por defecto. Es un punto de referencia estándar
    /// cuando no se especifica un dominio personalizado.
    /// </remarks>
    public const string DefaultDomainName = "Default";

    /// <summary>
    /// Representa el marco de trabajo predeterminado utilizado por el sistema.
    /// </summary>
    /// <remarks>
    /// Esta constante se utiliza cuando no se puede determinar el marco de trabajo solicitado por la aplicación en ejecución.
    /// Es una cadena que indica la versión predeterminada del marco de trabajo utilizada en operaciones donde no se especifica ningún otro.
    /// </remarks>
    public const string DefaultFramework = "net10.0";
}