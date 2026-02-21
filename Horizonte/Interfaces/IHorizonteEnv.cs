using System.Reflection;
using Horizonte;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Define la interfaz central para gestionar el entorno de una aplicación modular.
/// Esta interfaz proporciona métodos y propiedades para administrar el ciclo de vida de la aplicación, 
/// acceder a servicios y manejar componentes modulares.
/// </summary>
public interface IHorizonteEnv
{
    IHost HHost { get; }
    string RootPath { get; }
    Task Quit();
    Task Reboot();
    public StaticFileRegistry StaticFileRegistry { get; }
    public void AddOnStartWorker(BackgroundService obj);
    public  string Contextname { get;  }
    public T? GetService<T>();
    public T? GetRequiredService<T>(T value) where T : Type;
    public IhAssemblyManager? AssemblyManager { get; set; }
}

