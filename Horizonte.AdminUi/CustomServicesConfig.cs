namespace Horizonte.AdminUi;

public class CustomServicesConfig
{
    public CustomServicesConfig()
    {
        Services.Add(new CustomServiceConfig
        {
            ServiceType = "Horizonte.Scripts.Services.IScriptCompletionService, Horizonte.Scripts",
            ImplementationType = "Horizonte.Scripts.Services.ScriptCompletionService, Horizonte.Scripts",
            Lifetime = CustomServiceLifetime.Scoped
        });
    }

    public List<CustomServiceConfig> Services { get; set; } = new();
}

public class CustomServiceConfig
{
    public string ServiceType { get; set; } = string.Empty;

    public string ImplementationType { get; set; } = string.Empty;

    public CustomServiceLifetime Lifetime { get; set; } = CustomServiceLifetime.Scoped;
}

public enum CustomServiceLifetime
{
    Singleton,
    Scoped,
    Transient
}