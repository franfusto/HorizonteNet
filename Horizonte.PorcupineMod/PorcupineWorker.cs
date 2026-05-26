using System.Reflection;
using Horizonte.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Pv;

namespace Horizonte.PorcupineMod;

public class PorcupineWorker : BackgroundService
{

    private ILogger? _log;
    private IhContext? _context;
    private PorcupineConfig _config = new();
    private CancellationTokenSource? _cancellationTokenSource;
    private Porcupine _porcupine;
    private PvRecorder _recorder;
    private IHCredManager? _credManager;
    

    public PorcupineWorker(ILogger<PorcupineWorker> logger,IhContext context,IHCredManager credManager)
    {
        _log = logger;
        _context = context;
        _config = _context?.Get<PorcupineConfig>() ?? new PorcupineConfig();
        _credManager = credManager;

    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Task.Run(() =>
        {
            _log?.LogInformation($"Using device: {_recorder.SelectedDevice}");
            Console.WriteLine("Listening...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    short[] frame = _recorder.Read();
                    int keywordIndex = _porcupine.Process(frame);
                    if (keywordIndex >= 0)
                    {
                        switch (keywordIndex)
                        {
                            case 0:
                                Console.WriteLine("ALEXA");
                                break;
                            case 1:
                                Console.WriteLine("JARVIS");
                                break;
                            case 2:
                                Console.WriteLine("HEY_GOOGLE");
                                break;
                            case 3:
                                Console.WriteLine("HEY_SIRI");
                                break;
                            default:
                                Console.WriteLine("Unknown:" + keywordIndex);
                                break;
                        }
                    }
                }
                catch (Exception e)
                {
                    _log?.LogError(e.Message);
                    break;
                }
            }

            Console.WriteLine("End Listening...");
        });
        return Task.CompletedTask;
    }

    public override Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {

            _log?.LogInformation("Starting Service Porcupine");

            List<BuiltInKeyword> commands = new List<BuiltInKeyword>
            {
                BuiltInKeyword.ALEXA,
                BuiltInKeyword.JARVIS,
                BuiltInKeyword.HEY_GOOGLE,
                BuiltInKeyword.HEY_SIRI
                
            };
            var _key = _credManager?.GetCredential(_config.AccessKey) ?? string.Empty;
            _porcupine = Porcupine.FromBuiltInKeywords(_key, commands);
            //_porcupine = Porcupine.FromKeywordPaths(ACCESS_KEY, GetKeywordPaths(),null, null);

            _recorder = PvRecorder.Create(_porcupine.FrameLength);
            _recorder.Start();
            Thread.Sleep(1000); // esperar _recorder
        }
        catch (Exception e)
        {
            _log?.LogError(e.ToString());
        }

        _cancellationTokenSource = new CancellationTokenSource();
        return base.StartAsync(_cancellationTokenSource.Token);
    }


    private List<string> GetKeywordPaths()
    {
        List<string> paths = new();
        paths.Add(
            "/home/f.fuster/.nuget/packages/porcupine/3.0.7/buildTransitive/net8.0/resources/keyword_files/linux/alexa_linux.ppn");
        paths.Add(
            "/home/f.fuster/.nuget/packages/porcupine/3.0.7/buildTransitive/net8.0/resources/keyword_files/linux/hey google_linux_linux.ppn");
        return paths;
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _log?.LogInformation("Ending Service Porcupine");
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        Thread.Sleep(500); // esperar recorder
        _porcupine.Dispose();
        _recorder.Dispose();
        return Task.CompletedTask;
    }
}