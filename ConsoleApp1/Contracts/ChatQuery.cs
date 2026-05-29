namespace ConsoleApp1.Contracts;

public class ChatQuery
{
    public string Query { get; set; } = string.Empty;
    public string[]? Files { get; set; } = null;
}