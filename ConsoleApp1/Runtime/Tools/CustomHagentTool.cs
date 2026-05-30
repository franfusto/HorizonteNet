using ConsoleApp1.Contracts;
using ConsoleApp1.Definitions;
using ConsoleApp1.Interfaces;

namespace ConsoleApp1.Runtime.Tools;

public sealed class CustomHagentTool : IHagentTool
{
    private readonly AgentToolDef _toolDef;

    public CustomHagentTool(AgentToolDef toolDef)
    {
        ArgumentNullException.ThrowIfNull(toolDef);

        _toolDef = toolDef;

        Id = string.IsNullOrWhiteSpace(toolDef.Id)
            ? "custom-tool"
            : toolDef.Id;

        Name = string.IsNullOrWhiteSpace(toolDef.Name)
            ? "Custom Tool"
            : toolDef.Name;

        ToolType = toolDef.ToolType;
        Enabled = toolDef.Enabled;
    }

    public string Id { get; }

    public string Name { get; }

    public AgentToolType ToolType { get; }

    public bool Enabled { get; }

    public bool CanHandle(HagentToolCall call)
    {
        ArgumentNullException.ThrowIfNull(call);

        return Enabled &&
               call.ToolType == AgentToolType.Custom &&
               (string.Equals(call.ToolId, Id, StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrWhiteSpace(call.ToolId));
    }

    public ValueTask<bool> ShouldAutoInvokeAsync(
        HagentToolContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        if (!Enabled)
        {
            return ValueTask.FromResult(false);
        }

        return ValueTask.FromResult(IsAutoUseEnabled());
    }

    public Task<HagentToolResult> ExecuteAsync(
        HagentToolCall call,
        HagentToolContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(call);
        ArgumentNullException.ThrowIfNull(context);

        cancellationToken.ThrowIfCancellationRequested();

        var action = ResolveAction(call);

        return action.ToLowerInvariant() switch
        {
            "echo" => Task.FromResult(ExecuteEcho(call, context)),
            "template" => Task.FromResult(ExecuteTemplate(call, context)),
            "contextitem" => Task.FromResult(ExecuteContextItem(call, context)),
            "setcontextitem" => Task.FromResult(ExecuteSetContextItem(call, context)),

            _ => Task.FromResult(new HagentToolResult
            {
                Success = false,
                ErrorMessage = $"CustomHagentTool no reconoce la acción '{action}'."
            })
        };
    }

    private bool IsAutoUseEnabled()
    {
        if (!_toolDef.Settings.TryGetValue("autoUse", out var value))
        {
            return false;
        }

        return value.Equals("true", StringComparison.OrdinalIgnoreCase) ||
               value.Equals("always", StringComparison.OrdinalIgnoreCase);
    }

    private string ResolveAction(HagentToolCall call)
    {
        if (call.Arguments.TryGetValue("action", out var argumentAction) &&
            argumentAction is not null &&
            !string.IsNullOrWhiteSpace(argumentAction.ToString()))
        {
            return argumentAction.ToString()!;
        }

        if (_toolDef.Settings.TryGetValue("action", out var settingAction) &&
            !string.IsNullOrWhiteSpace(settingAction))
        {
            return settingAction;
        }

        return "echo";
    }

    private HagentToolResult ExecuteEcho(
        HagentToolCall call,
        HagentToolContext context)
    {
        var text = ResolveText(
            call,
            context,
            fallback: context.UserQuery);

        StoreOptionalTargetItem(
            context,
            text);

        return CreateSuccessResult(
            text,
            "echo");
    }

    private HagentToolResult ExecuteTemplate(
        HagentToolCall call,
        HagentToolContext context)
    {
        var template = ResolveTemplate(call);

        var text = ApplyTemplate(
            template,
            context);

        StoreOptionalTargetItem(
            context,
            text);

        return CreateSuccessResult(
            text,
            "template");
    }

    private HagentToolResult ExecuteContextItem(
        HagentToolCall call,
        HagentToolContext context)
    {
        var sourceItemKey = ResolveSourceItemKey(call);

        if (string.IsNullOrWhiteSpace(sourceItemKey))
        {
            return new HagentToolResult
            {
                Success = false,
                ErrorMessage = "La acción 'contextItem' requiere 'sourceItemKey'."
            };
        }

        if (!context.Items.TryGetValue(sourceItemKey, out var value) || value is null)
        {
            return new HagentToolResult
            {
                Success = true,
                Text = $"No existe ningún item en contexto con la clave '{sourceItemKey}'.",
                Metadata =
                {
                    ["action"] = "contextItem",
                    ["sourceItemKey"] = sourceItemKey,
                    ["found"] = false
                }
            };
        }

        var text = value.ToString() ?? string.Empty;

        StoreOptionalTargetItem(
            context,
            text);

        var result = CreateSuccessResult(
            text,
            "contextItem");

        result.Metadata["sourceItemKey"] = sourceItemKey;
        result.Metadata["found"] = true;

        return result;
    }

    private HagentToolResult ExecuteSetContextItem(
        HagentToolCall call,
        HagentToolContext context)
    {
        var targetItemKey = ResolveTargetItemKey(call);

        if (string.IsNullOrWhiteSpace(targetItemKey))
        {
            return new HagentToolResult
            {
                Success = false,
                ErrorMessage = "La acción 'setContextItem' requiere 'targetItemKey'."
            };
        }

        var value = ResolveText(
            call,
            context,
            fallback: context.UserQuery);

        context.Items[targetItemKey] = value;

        var result = CreateSuccessResult(
            $"CustomHagentTool guardó un valor en context.Items[\"{targetItemKey}\"].",
            "setContextItem");

        result.Metadata["targetItemKey"] = targetItemKey;

        return result;
    }

    private string ResolveText(
        HagentToolCall call,
        HagentToolContext context,
        string fallback)
    {
        if (call.Arguments.TryGetValue("text", out var argumentText) &&
            argumentText is not null)
        {
            return argumentText.ToString() ?? string.Empty;
        }

        if (call.Arguments.TryGetValue("content", out var argumentContent) &&
            argumentContent is not null)
        {
            return argumentContent.ToString() ?? string.Empty;
        }

        if (_toolDef.Settings.TryGetValue("text", out var settingText))
        {
            return settingText;
        }

        if (_toolDef.Settings.TryGetValue("content", out var settingContent))
        {
            return settingContent;
        }

        if (_toolDef.Settings.TryGetValue("sourceItemKey", out var sourceItemKey) &&
            context.Items.TryGetValue(sourceItemKey, out var sourceValue) &&
            sourceValue is not null)
        {
            return sourceValue.ToString() ?? string.Empty;
        }

        return fallback;
    }

    private string ResolveTemplate(HagentToolCall call)
    {
        if (call.Arguments.TryGetValue("template", out var argumentTemplate) &&
            argumentTemplate is not null &&
            !string.IsNullOrWhiteSpace(argumentTemplate.ToString()))
        {
            return argumentTemplate.ToString()!;
        }

        if (_toolDef.Settings.TryGetValue("template", out var settingTemplate) &&
            !string.IsNullOrWhiteSpace(settingTemplate))
        {
            return settingTemplate;
        }

        return "{{query}}";
    }

    private string ResolveSourceItemKey(HagentToolCall call)
    {
        if (call.Arguments.TryGetValue("sourceItemKey", out var argumentKey) &&
            argumentKey is not null &&
            !string.IsNullOrWhiteSpace(argumentKey.ToString()))
        {
            return argumentKey.ToString()!;
        }

        if (_toolDef.Settings.TryGetValue("sourceItemKey", out var settingKey))
        {
            return settingKey;
        }

        return string.Empty;
    }

    private string ResolveTargetItemKey(HagentToolCall call)
    {
        if (call.Arguments.TryGetValue("targetItemKey", out var argumentKey) &&
            argumentKey is not null &&
            !string.IsNullOrWhiteSpace(argumentKey.ToString()))
        {
            return argumentKey.ToString()!;
        }

        if (_toolDef.Settings.TryGetValue("targetItemKey", out var settingKey))
        {
            return settingKey;
        }

        return $"custom.{Id}.text";
    }

    private void StoreOptionalTargetItem(
        HagentToolContext context,
        string text)
    {
        if (!_toolDef.Settings.TryGetValue("targetItemKey", out var targetItemKey) ||
            string.IsNullOrWhiteSpace(targetItemKey))
        {
            return;
        }

        context.Items[targetItemKey] = text;
    }

    private string ApplyTemplate(
        string template,
        HagentToolContext context)
    {
        var result = template
            .Replace("{{agentId}}", context.AgentId, StringComparison.OrdinalIgnoreCase)
            .Replace("{{query}}", context.UserQuery, StringComparison.OrdinalIgnoreCase)
            .Replace("{{toolId}}", Id, StringComparison.OrdinalIgnoreCase)
            .Replace("{{toolName}}", Name, StringComparison.OrdinalIgnoreCase)
            .Replace("{{nowUtc}}", DateTimeOffset.UtcNow.ToString("O"), StringComparison.OrdinalIgnoreCase);

        foreach (var item in context.Items)
        {
            var token = "{{item." + item.Key + "}}";
            result = result.Replace(
                token,
                item.Value?.ToString() ?? string.Empty,
                StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }

    private HagentToolResult CreateSuccessResult(
        string text,
        string action)
    {
        return new HagentToolResult
        {
            Success = true,
            Text = text,
            ResponseItems =
            {
                new ChatResponseItem
                {
                    Type = ChatResponseItemType.Tool,
                    Content = $"CustomHagentTool ejecutó la acción '{action}'."
                }
            },
            Metadata =
            {
                ["toolId"] = Id,
                ["toolType"] = ToolType.ToString(),
                ["action"] = action,
                ["provider"] = "settings"
            }
        };
    }
}