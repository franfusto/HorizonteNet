using System.Text;
using Horizonte;
using Horizonte.WorkFlows.Widgets;
using Microsoft.Agents.AI.Workflows;

namespace Horizonte.WorkFlows;

/// <summary>
/// 
/// </summary>
[HorizonteModule("Horizonte.WorkFlows")]
public class PanelModulo
{
    private readonly IHorizonteEnv _env;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="env"></param>
    public PanelModulo(IHorizonteEnv env)
    {
        _env = env;
    }

    [HorizonteRole("init")]
    [HorizonteCommand("WorkFlows_Init")]
    public bool Init()
    {
        return true;
    }
    
    [HorizonteRole("widget")]
    [HorizonteCommand("WorkFlows_Dashboard", "")]
    public WidgetDef ScriptsViewerWidget() => new WidgetDef() { Type = typeof(Dashboard), Parameters = null };

    [HorizonteCommand("TestNode")]
    public async Task<string> TestNode(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(message + " TestNode ");
    }
    [HorizonteCommand("TestNode1")]
    public async Task<string> TestNode1(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        for (var i = 0; i < 5; i++)
        {
            if(cancellationToken.IsCancellationRequested) break;

            Console.WriteLine($"Iteration {i}");
            await Task.Delay(2000);
        }
        return message + " TestNode1 ";
    }
    [HorizonteCommand("TestNode2")]
    public async Task<string> TestNode2(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(message +" TestNode2 ");
    }
    [HorizonteCommand("TestNode3")]
    public async Task<string> TestNode3(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(message + " TestNode3 ");
    }
    [HorizonteCommand("TestNode4")]
    public async Task<string> TestNode4(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(message +" TestNode4 ");
    }
    [HorizonteCommand("TestNode5")]
    public async Task<string> TestNode5(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(message +" TestNode5 ");
    }
    internal sealed class CustomEvent(string message) : WorkflowEvent(message) { }
    [HorizonteCommand("TestNode6")]
    public async Task<string> TestNode6(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        
        await context.AddEventAsync(new CustomEvent($"Processing message: {message}"));
        return await Task.FromResult(message +" TestNode6 ");
    }
    [HorizonteCommand("TestNode7")]
    public async Task<string> TestNode7(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(message +" TestNode7 ");
    }
    [HorizonteCommand("TestNode8")]
    public async Task<string> TestNode8(string message, IWorkflowContext context, CancellationToken cancellationToken)
    {
        return await Task.FromResult(message +" TestNode8 ");
    }

    [HorizonteCommand("Condition1")]
    public bool Condition1(string message)
    {
        return true;
    }
    [HorizonteCommand("Condition2")]
    public bool Condition2(string message)
    {
        return false;
    }

}
