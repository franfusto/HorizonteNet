using System.Text.Json;
using Horizonte;

namespace Horizonte;

public class HContext : IHContext
{
    private string _defaultcontextname = string.Empty;
    // /////////////// HAY QUE PASAR A JSONFILEHELPER LAS OPCIONES DE SERIALIZACIÓN
    
    public HContext(string? contextName = null, Func<JsonSerializerOptions>? defaultSerializerOptions = null)
    {
        _defaultcontextname = contextName ?? "horizonte";
    }
    
    public T? Get<T>()
    {
        return Get<T>(_defaultcontextname);
    }
    public T? Get<T>(string contextname)
    {
        contextname = contextname ?? _defaultcontextname;
        var jsonFilePath = contextname + ".json";
        return JsonFileHelper.TryGet(jsonFilePath, typeof(T).Name, out T? value ) ? value : default(T);
    }

    public void Update<T>(Action<T> update)
    {
        Update(update,_defaultcontextname);
    }
    public void Update<T>(Action<T> update, string contexname)
    {
        JsonFileHelper.AddOrUpdateSection(
            jsonFilePath: contexname  +".json", 
            sectionName: typeof(T).Name, 
            updateAction: update);
    }
    public void Update<T>(T newvalue)
    {
         Update(newvalue,_defaultcontextname);
    }
    public void Update<T>(T newvalue, string contexname) 
    {
        JsonFileHelper.AddOrUpdateSection(
            jsonFilePath: contexname  +".json", 
            sectionName: typeof(T).Name, 
            value: newvalue);
        
    }
}