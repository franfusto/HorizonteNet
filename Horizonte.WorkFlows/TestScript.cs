
using System;
using Horizonte;
using Microsoft.Extensions.Logging;
using Microsoft.Agents.AI.Workflows;
using System.Threading;

[HorizonteModule("WorkFlowScript")]
public class Sample
{
    private ILogger<PanelModulo>? _logger;

    public Sample(ILogger<PanelModulo> logger)
    {
        _logger = logger;
    }
    
    [HorizonteRole("init")]
    [HorizonteCommand("WorkFlowScript_Init")]
    public bool Init()
    {
        
        _logger?.LogInformation("Módulo WorkFlowScript Inciciado");
        return true;
    }

    [HorizonteCommand("WorkFlowScript_NodeInSample")]
    public async Task<Dictionary<string,string>> NodeInSample(Dictionary<string,string> message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        Console.WriteLine("Test_1: "+ message);
        return message;
    }
    [HorizonteCommand("WorkFlowScript_NodeOutSample")]
    public async Task<Dictionary<string,string>> NodeOutSample(Dictionary<string,string> message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        message["validado"] = "true";
        Console.WriteLine("Test_2: "+ message);
        return message;
    }

   [HorizonteCommand("WorkFlowScript_Node1")]
    public async Task<Dictionary<string,string>> Node1(Dictionary<string,string> message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        return message;
    }
    
    [HorizonteCommand("WorkFlowScript_Node2")]
    public async Task<Dictionary<string,string>> Node2(Dictionary<string,string> message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        int x = 0;
        while (x<=10)
        {
            x++;
            Console.WriteLine("tick.....");
            if (cancellationToken.IsCancellationRequested) break;
            message["tick"] = x.ToString();
            Thread.Sleep(1000);
        }

        if (x < 5)
        {
            throw new Exception("*****BOMBA*****x < 5");
        }

        return message;
    }
    

    [HorizonteCommand("WorkFlowScript_Condition1")]
    public async Task<bool> Condition1(Dictionary<string,string> message)
    {
        return message.Count == 0;
    }
    //procesadores
    [HorizonteCommand("WorkFlowScript_ErrorProcessor")]
    public async Task<Dictionary<string,string>> ErrorProcessor(Dictionary<string,string> message,Exception e)
    {
        message["error"] = e.Message;
        return message;
    }
    [HorizonteCommand("WorkFlowScript_CancelProcessor")]
    public async Task<Dictionary<string,string>> CancelProcessor(Dictionary<string,string> message)
    {
        message["cancel"] = "true";
        return message;
    }
}