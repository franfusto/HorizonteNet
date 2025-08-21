using System.Reflection;

namespace Horizonte;

/// <summary>
/// Clase abstracta que define el modelo para gestionar ensamblados en la aplicación.
/// </summary>
public abstract class IhAssemblyManager
{
    /// <summary>
    /// 
    /// </summary>
    public List<Assembly> Assemblies { get;  set; } 
}