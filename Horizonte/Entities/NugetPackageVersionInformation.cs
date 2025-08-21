namespace Horizonte;

public class NugetPackageVersionInformation()
{
    public Version Version { get; set; } = new Version();
    public string PackageId { get; set; } =string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string DllPath { get; set; }= string.Empty;
}