using Microsoft.Extensions.Hosting;

public interface IHorizonteEnv
{
    IHost HHost { get; }
    string RootPath { get; }
    void Quit();
    void Reboot();
    List<string> AssetsFolders { get; set; }
    public void AddOnStartWorker(BackgroundService obj);
    public  string Contextname { get;  }
    public T? GetService<T>();
    public T? GetRequiredService<T>(T value) where T : Type;
}

