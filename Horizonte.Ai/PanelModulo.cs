using Microsoft.Agents.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;
//using OpenAI;
using OpenAI.Chat;

namespace Horizonte.Ai;

[HorizonteModule("Horizonte.Ai")]
public class PanelModulo
{
    private Lazy<IHorizonteEnv> _env;
    private ILogger<PanelModulo>? _logger;
    private IHCredManager? _credManager;
    private AIAgent _agent;
    public PanelModulo(IHorizonteEnv env)
    {
        _env = new Lazy<IHorizonteEnv>(() => env);
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("Horizonte.Ai_Init")]
    public bool Init()
    {
        _logger = _env.Value.GetService<ILogger<PanelModulo>>();
        _credManager = _env.Value.GetService<IHCredManager>();
        var context = _env.Value.GetService<IHContext>();
        
        var apiKey = _credManager?.GetCredential("openai.key") ?? throw new InvalidOperationException("OPENAI_API_KEY is not set.");
        var model = "gpt-4o-mini";
         _agent = new OpenAIClient(
                apiKey)
            .GetChatClient(model)
            .CreateAIAgent(instructions: "Eres bueno contando chistes", name: "Joker");
        
        Test();
        return true;
    }

    [HorizonteCommand("Horizonte.Ai.Test")]
    public async Task<string> Test()
    {
        _logger?.LogInformation("Test");
        try
        {


            var result = await _agent.RunAsync("Cuentame un chiste de piratas.");
            Console.WriteLine(result);
            return result.ToString();
        }
        catch (Exception e)
        {
            _logger?.LogError(e.ToString());
        }
        return "fail";
    }

}