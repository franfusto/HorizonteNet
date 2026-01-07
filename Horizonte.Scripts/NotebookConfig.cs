using Microsoft.DotNet.Interactive.Documents.Jupyter;
using Microsoft.DotNet.Interactive.Documents;

namespace Horizonte.Scripts;

public class NotebookConfig
{
    public NotebookConfig()
    {
        AddContentSample();
    }

    public List<NotebookSource> Sources { get; set; } = new List<NotebookSource>();

    private void AddContentSample()
    {
        Sources.Add(new NotebookSource()
        {
            Type = NotebookSourceType.localDirectory,
            Name =  "LocalDirectory",
            Items = new List<NotebookItem>()
        });

        Sources[0].Items.Add(new NotebookItem()
        {
            Document = null,
            ItemId = "SampleNotebook01",
            ItemPath = "https://raw.githubusercontent.com/dotnet/csharp-notebooks/refs/heads/main/notebook-getting-started/01-What%20are%20Notebooks.ipynb"
        });
        Sources[0].Items.Add(new NotebookItem()
        {
            Document = null,
            ItemId = "SampleNotebook02",
            ItemPath = "https://raw.githubusercontent.com/dotnet/csharp-notebooks/refs/heads/main/notebook-getting-started/02-Code%20Cells.ipynb"
        });
        Sources[0].Items.Add(new NotebookItem()
        {
            Document = null,
            ItemId = "SampleNotebook03",
            ItemPath = "https://raw.githubusercontent.com/dotnet/csharp-notebooks/refs/heads/main/notebook-getting-started/03-Markdown%20Cells.ipynb"
        });
        Sources[0].Items.Add(new NotebookItem()
        {
            Document = null,
            ItemId = "SampleNotebook04",
        });
    }
}

public class NotebookSource
{
    public string Name { get; set; }
    public string SourcePath { get; set; }
    public NotebookSourceType Type { get; set; }
    public List<NotebookItem> Items { get; set; } = new List<NotebookItem>();
}

public class NotebookItem
{
    public string ItemId { get; set; }
    public string ItemPath { get; set; }
    public InteractiveDocument? Document { get; set; }
}

public enum NotebookSourceType
{
    localDirectory,
    web
}