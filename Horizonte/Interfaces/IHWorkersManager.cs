using Microsoft.Extensions.Hosting;

namespace Horizonte;

public interface IhWorkersManager
{
    void ConfigureWorkers();
    void StartWorkers();
    void StopWorkers();
    List<BackgroundService> GetRunningWorkers();
}
