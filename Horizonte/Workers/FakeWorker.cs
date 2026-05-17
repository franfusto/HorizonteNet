
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Horizonte;

/// <summary>
/// FakeWorker es un servicio en segundo planode ejemplo, que implementa la interfaz IHservice.
/// Está diseñado para ejecutar lógica de trabajo personalizada como un servicio de larga duración dentro de un entorno de hospedaje.
/// </summary>
public class FakeWorker : BackgroundService
{

    
    private ILogger? _log;

    public FakeWorker(ILogger<FakeWorker> logger)
    {
        _log = logger;

    }

    private void DoWork(CancellationToken stoppingToken)
    {
        var i = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            _log?.LogInformation("FakeWorker contador: " + i.ToString());
            Thread.Sleep(5000);
            i++;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _log?.LogInformation("Ejecutando FakeWorker");
        return Task.Run(() => DoWork(stoppingToken), stoppingToken);
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
       
        _log?.LogInformation("Iniciando FakeWorker");
        return base.StartAsync(cancellationToken);
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Finalizando FakeWorker");
        return base.StopAsync(cancellationToken);
    }


}