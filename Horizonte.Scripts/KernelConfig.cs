namespace Horizonte.Scripts;

public class KernelConfig
{
    public KernelConfig()
    {
        KernelItems.Add(new KernelItem()
        {
            Name = "Default",
            Value = string.Empty,
        });
        KernelItems.Add(new KernelItem()
        {
            Name = "Aislado",
            Value = string.Empty,
        });
    }

    public   List<KernelItem> KernelItems {get; set;} = new List<KernelItem>();
}

public class KernelItem
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}