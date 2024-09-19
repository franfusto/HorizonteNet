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
    public T Get<T>(string contextname)
    {
        contextname = contextname ?? _defaultcontextname;
        string jsonFilePath = contextname + ".json";
        T value;
        if(JsonFileHelper.TryGet(jsonFilePath, typeof(T).Name, out value ))
        {
            return (T)value;
        }
        return default(T);
    }

    public void Update<T>(Action<T> update)
    {
        Update<T>(update,_defaultcontextname);
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
         Update<T>(newvalue,_defaultcontextname);
    }
    public void Update<T>(T newvalue, string contexname) 
    {
        JsonFileHelper.AddOrUpdateSection(
            jsonFilePath: contexname  +".json", 
            sectionName: typeof(T).Name, 
            value: newvalue);
        
    }
}