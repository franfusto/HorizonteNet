using ConsoleApp1.Runtime.Files;

namespace ConsoleApp1.Interfaces;

public interface IHagentImageToTextClient
{
    Task<HagentImageToTextResult> DescribeAsync(
        HagentImageToTextRequest request,
        CancellationToken cancellationToken);
}

public sealed class HagentImageToTextRequest
{
    public string Prompt { get; set; } = string.Empty;

    public List<HagentFile> Images { get; } = [];

    public Dictionary<string, object?> Options { get; } = [];
}

public sealed class HagentImageToTextResult
{
    public string Text { get; set; } = string.Empty;

    public Dictionary<string, object?> Metadata { get; } = [];
}