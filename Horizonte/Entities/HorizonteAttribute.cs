
namespace Horizonte
{
    /// <summary>
    /// Atributo utilizado para marcar una clase como un módulo dentro del marco Horizonte.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class HorizonteModule : Attribute
    {
        /// <summary>
        /// Nombre del módulo.
        /// </summary>
        public string Modulename;

        /// <summary>
        /// Constructor que inicializa el atributo con el nombre del módulo.
        /// </summary>
        /// <param name="name">Nombre del módulo.</param>
        public HorizonteModule(string name)
        {
            this.Modulename = name;
        }
    }



    /// <summary>
    /// Atributo para definir un comando en Horizonte. Este comando puede ser ejecutado y contiene metadata como una clave y descripción.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HorizonteCommand : Attribute
    {
        /// <summary>
        /// Clave única que identifica el comando.
        /// </summary>
        public string Key;

        /// <summary>
        /// Descripción del comando.
        /// </summary>
        public string Description;

        /// <summary>
        /// Constructor que inicializa el atributo con la clave del comando y, opcionalmente, una descripción.
        /// </summary>
        /// <param name="key">Clave del comando.</param>
        /// <param name="description">Descripción del comando (opcional).</param>
        public HorizonteCommand(string key, string description = "")
        {
            this.Key = key;
            this.Description = description;
        }
    }

    /// <summary>
    /// Atributo para especificar el rol necesario para la ejecución de un comando en Horizonte.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class HorizonteRole : Attribute
    {
        /// <summary>
        /// Nombre del rol requerido.
        /// </summary>
        public string Role;

        /// <summary>
        /// Constructor que inicializa el atributo con el nombre del rol.
        /// </summary>
        /// <param name="role">Nombre del rol.</param>
        public HorizonteRole(string role)
        {
            this.Role = role;
        }
    }
    /// <summary>
    /// Atributo utilizado para marcar un campo estático que define mapeos entre nombres de ensamblado
    /// y nombres de paquete NuGet.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class AssemblyMappingDefinition : Attribute
    {
    }
    
}