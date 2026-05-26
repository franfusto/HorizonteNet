using System.Reflection;

namespace Horizonte
{
    /// <summary>
    /// Representa un comando configurable que incluye su metadata, acción asociada y roles permitidos.
    /// </summary>
    public class HCommand
    {
        /// <summary>
        /// Clave única que identifica el comando.
        /// </summary>
        public string CommandKey { get; set; } = string.Empty;

        /// <summary>
        /// Información del método asociado al comando que será ejecutado.
        /// </summary>
        public MethodInfo? CommandAction { get; set; }

        /// <summary>
        /// Instancia del objeto en el que se ejecutará el comando.
        /// </summary>
        public object Instance { get; set; } = new();

        /// <summary>
        /// Descripción del propósito o funcionalidad del comando.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de dato de salida esperado al ejecutar el comando.
        /// </summary>
        public Type? OutType { get; set; }

        /// <summary>
        /// Lista de tipos de los parámetros de entrada requeridos por el comando.
        /// </summary>
        public List<Type>? InTypes { get; set; } = new List<Type>();

        /// <summary>
        /// Lista de nombres de los parámetros de entrada requeridos por el comando.
        /// </summary>
        public List<string>? InNames { get; set; } = new List<string>();

        /// <summary>
        /// Lista de roles que tienen permisos de ejecución para este comando.
        /// </summary>
        public List<string> Roles { get; set; } = new List<string>();
        
        /// <summary>
        /// Indica si el comando es asincrónico.
        /// </summary>
        public bool IsAsync { get; set; } = false;
        
        /// <summary>
        /// El dominio (ALC) al que pertenece el comando.
        /// </summary>
        public string Domain { get; set; } = Const.DefaultDomainName;
        
        
    }
}