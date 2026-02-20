using System.Text;
using System.Text.Json.Serialization;

namespace Horizonte.Extensions.Interactive;

/// <summary>
/// 
/// </summary>
public class ScriptDef
{
    /// <summary>
    /// 
    /// </summary>
    public string Id { get; set; } = string.Empty;
    private string _name = string.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string Name 
    { 
        get => _name; 
        set => _name = SanitizeName(value); 
    }

    /// <summary>
    /// 
    /// </summary>
    public string Description { get; set; } = string.Empty;

    private string SanitizeName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;
        var sb = new StringBuilder();
        foreach (char c in name)
        {
            if (char.IsLetterOrDigit(c))
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
    /// <summary>
    /// Código guardado en formato Base64
    /// </summary>
    public string Code {get; set;} = string.Empty;
    
    /// <summary>
    /// Propiedad para acceder al código en texto plano (no se serializa)
    /// </summary>
    [JsonIgnore]
    public string CodeText
    {
        get
        {
            if (string.IsNullOrEmpty(Code)) return string.Empty;
            try
            {
                return Encoding.UTF8.GetString(Convert.FromBase64String(Code));
            }
            catch
            {
                return Code; // Si no es base64 válido, devolvemos el valor original para evitar pérdida de datos
            }
        }
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Code = string.Empty;
            }
            else
            {
                Code = Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public bool Active { get; set; } = false;
}