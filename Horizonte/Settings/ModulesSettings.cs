namespace Horizonte;

/// La clase ModulesSettings se utiliza para gestionar la configuración de módulos, servidores NuGet, carpetas NuGet, prioridades de frameworks y dominios.
/// En el constructor de la clase, se inicializa una lista de configuraciones predeterminadas, incluida la definición de un módulo activo, la especificación de un servidor NuGet, y una lista de prioridades de framework.
/// Proporciona varias colecciones para mantener configuraciones específicas:
/// - List: Almacena configuraciones individuales de módulos.
/// - NugetServers: Contiene configuraciones de servidores NuGet.
/// - NugetFolders: Lista las carpetas de instalación de NuGet.
/// - FrameworkPriorities: Define una lista de prioridades para diferentes versiones de frameworks.
/// - ForcedPackages: Mantiene una lista de paquetes que están forzados dentro del entorno.
/// - Domains: Lista de dominios utilizados en la aplicación.
/// /
public class ModulesSettings
{

    /// <summary>
    /// Propiedad que representa una lista de objetos de tipo <c>ModulesSettingsItem</c>.
    /// Esta lista se utiliza para almacenar configuraciones de módulos relacionadas
    /// con la aplicación. Los elementos de esta lista determinan las características
    /// y el comportamiento de los módulos, como su activación, nombre y versión.
    /// </summary>
    public List<ModulesSettingsItem> List { get; set; } = [];

    /// <summary>
    /// Propiedad que representa una lista de servidores NuGet configurados.
    /// Cada elemento de la lista es una instancia de <see cref="NugetServerItem"/>,
    /// que contiene la información necesaria para interactuar con un servidor de paquetes NuGet específico,
    /// como la URL del servidor, el nombre, la versión del API soportada,
    /// el orden de preferencia en que se debe acceder a los servidores,
    /// y un indicador de si el servidor está activo.
    /// Esta configuración permite gestionar y priorizar múltiples fuentes de paquetes NuGet
    /// en la aplicación al resolver dependencias de paquetes.
    /// </summary>
    public List<NugetServerItem> NugetServers { get; set; } = new List<NugetServerItem>();

    /// <summary>
    /// Propiedad que representa una lista de directorios de paquetes NuGet configurados.
    /// </summary>
    /// <remarks>
    /// Cada elemento de la lista es una instancia de <c>NugetFolderItem</c>, que define las propiedades
    /// específicas de un directorio de paquetes, como el nombre de la carpeta, el nombre identificador,
    /// el orden dentro de la lista y si se debe instalar.
    /// </remarks>
    public List<NugetFolderItem> NugetFolders { get; set; } = new List<NugetFolderItem>();

    /// La propiedad `FrameworkPriorities` es una lista de cadenas que especifica las prioridades de los frameworks .NET
    /// que se utilizarán en el contexto de configuración de módulos. Esta propiedad determina el orden de preferencia
    /// para seleccionar el framework adecuado durante la gestión de módulos y puede incluir versiones específicas de .NET,
    /// como "net10.0", "net9.0", "net8.0", entre otras. Facilita la identificación de qué versiones de framework deben recibir
    /// prioridad al implementar o ejecutar aplicaciones y bibliotecas dentro del sistema.
    /// /
    public List<string> FrameworkPriorities { get; set; } = new List<string>();

    /// La propiedad ForcedPackages es una lista que contiene elementos de tipo ForcedPackageItem.
    /// Esta lista representa un conjunto de paquetes que se deben incluir de manera obligatoria en el contexto del sistema de ensamblado del proyecto.
    /// Cada entrada en esta lista incluye detalles específicos sobre el paquete, como su identificador y versión.
    /// Uso típico de ForcedPackages incluye la gestión de dependencias que no deben ser omitidas durante el cargado de dominios o módulos.
    /// Los paquetes forzados garantizan que ciertas bibliotecas necesarias estén presentes independientemente de otras configuraciones del entorno.
    /// /
    public List<ForcedPackageItem> ForcedPackages { get; set; } = new List<ForcedPackageItem>();

    /// <summary>
    /// Representa una lista de dominios asociados a la configuración de módulos dentro de la aplicación.
    /// </summary>
    /// <remarks>
    /// Los dominios pueden ser utilizados para categorizar o segmentar diferentes espacios de trabajo
    /// o entornos dentro de la aplicación. Inicialmente, la lista contiene un dominio por defecto y
    /// puede ser expandida según las necesidades del proyecto.
    /// </remarks>
    public List<string> Domains { get; set; } = new List<string>();
}

/// <summary>
/// Representa un ítem de paquete que se obliga a incluir en un pedido o proceso logístico.
/// </summary>
/// <remarks>
/// La clase ForcedPackageItem se utiliza en situaciones donde ciertos artículos deben ser incluidos por defecto
/// en un paquete o envío, independientemente de las preferencias del usuario o del contexto.
/// </remarks>
public class ForcedPackageItem
{
    /// La propiedad `Active` determina si un paquete forzado está activo o inactivo.
    /// Cuando está configurada como `true`, el paquete será procesado y cargado por el sistema.
    /// Si está configurada como `false`, el paquete será ignorado y no se llevará a cabo ninguna acción sobre él.
    /// Esta propiedad es útil para controlar dinámicamente qué paquetes deben estar activos, permitiendo
    /// a los administradores del sistema gestionar los paquetes que se requieren en diferentes contextos o entorno
    /// sin necesidad de eliminar su configuración del sistema.
    /// /
    public bool Active { get; set; } = true;

    /// <summary>
    /// Propiedad que representa el identificador único de un paquete forzado.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para gestionar paquetes en el sistema, permitiendo
    /// identificar específicamente qué paquete debe ser cargado o manipulado.
    /// Se espera que cada paquete tenga un identificador único que distinga
    /// un paquete de otro dentro de las aplicaciones que utilizan esta funcionalidad.
    /// </remarks>
    public string PackageId { get; set; } = string.Empty;

    /// Propiedad `Version` en la clase `ForcedPackageItem`, utilizada para definir la versión específica de un paquete forzado que se desea cargar o gestionar dentro del sistema.
    /// Es una cadena de texto que contiene el número de versión del paquete, y se utiliza fundamentalmente para identificar de manera precisa qué versión del paquete debe manejarse en las operaciones de carga.
    /// Esta propiedad es particularmente útil cuando se implementan métodos que requieren la carga de paquetes con versiones específicas, como en la clase `HAssemblyManager`, permitiendo el manejo correcto de diferentes versiones de paquetes.
    /// /
    public string Version { get; set; } = string.Empty;

    /// Representa el marco de trabajo (framework) específico que debe utilizarse para resolver y cargar un paquete forzado.
    /// Esta propiedad permite determinar el entorno de ejecución o conjunto de bibliotecas de clases bajo el cual el paquete está diseñado para funcionar. Algunos ejemplos de frameworks pueden incluir .NET Framework, .NET Core, y otros.
    /// Es fundamental que esta propiedad se configure apropiadamente para garantizar que el paquete se cargue y ejecute en un entorno compatible, evitando así posibles conflictos o errores de incompatibilidad.
    /// /
    public string Framework { get; set; } = string.Empty;

    /// /
    public string Domain { get; set; } = "Default";
}

/// La clase ModulesSettingsItem representa la configuración de un módulo dentro de un sistema.
/// Esta clase se utiliza para definir propiedades específicas de cada módulo, tales como su estado activo, nombre, versión, y demás configuraciones adicionales que puedan ser necesarias.
/// Propiedades:
/// - Active: Indica si el módulo está activo o no.
/// - ModuleName: Especifica el nombre del módulo.
/// - ModuleVersion: Define la versión actual del módulo.
/// - Path: Puede contener la ruta en la que se encuentra el módulo, si es aplicable.
/// - LoadAdditionalDlls: Indica si se deben cargar DLLs adicionales para el módulo.
/// - Domain: Permite especificar el dominio al que pertenece el módulo.
/// Esta clase es esencial para manejar dinámicamente la configuración de módulos en aplicaciones que requieren modularidad y extensibilidad.
/// /
public class ModulesSettingsItem
{
    /// <summary>
    /// Propiedad que indica si un módulo o servidor NuGet está activo.
    /// </summary>
    /// <remarks>
    /// Esta propiedad es de tipo booleano y se utiliza para determinar si un elemento debe estar habilitado o funcionando dentro del contexto de configuración de módulos o servidores NuGet.
    /// En el contexto de configuración de módulos, si esta propiedad se establece en <c>true</c>, el módulo correspondiente estará activo y disponible para su carga.
    /// En el contexto de servidores NuGet, si esta propiedad se establece en <c>true</c>, el servidor estará activo y podrá ser utilizado para la gestión de paquetes.
    /// </remarks>
    public bool Active { get; set; }

    /// Representa el nombre de un módulo en el sistema de configuraciones de módulos.
    /// Esta propiedad es utilizada para identificar de manera única a un módulo dentro de la aplicación.
    /// Es fundamental para la carga de módulos, ya que se utiliza en múltiples procesos, como el registro
    /// de directorios de módulos y la resolución de ensamblados. La correcta asignación de este nombre
    /// asegura la correcta operación del sistema de gestión de módulos.
    /// /
    public string ModuleName { get; set; } = string.Empty;

    /// Propiedad que representa la versión del módulo en el sistema.
    /// Esta propiedad es utilizada para especificar la versión actual de un módulo dentro del sistema.
    /// Es un valor clave para asegurar la correcta gestión y carga de los distintos módulos disponibles,
    /// permitiendo mantener la integridad y compatibilidad entre ellos. Su valor típicamente sigue el
    /// formato de versión semántica (por ejemplo, "1.0.0").
    /// /
    public string ModuleVersion { get; set; }= string.Empty;

    /// <summary>
    /// Propiedad que especifica la ruta de acceso a un archivo o directorio en el sistema de archivos.
    /// </summary>
    /// <remarks>
    /// Representa una cadena que contiene el camino absoluto o relativo al recurso en cuestión,
    /// permitiendo realizar operaciones como lectura, escritura o verificación de existencia.
    /// </remarks>
    public string? Path { get; set; }

    /// <summary>
    /// Propiedad que indica si se deben cargar ensamblados adicionales para un módulo específico.
    /// Cuando se establece en <c>true</c>, y la ruta del módulo no es <c>null</c>, permite la carga de ensamblados adicionales desde el directorio especificado.
    /// </summary>
    public bool? LoadAdditionalDlls { get; set; } = false;

    /// <summary>
    /// Propiedad que detalla el dominio para una aplicación o configuración específica.
    /// </summary>
    /// <remarks>
    /// Esta propiedad es utilizada para identificar de manera única el ámbito o área de operación dentro de un sistema,
    /// lo que facilita la segmentación y organización lógica de procesos o datos.
    /// </remarks>
    public string? Domain { get; set; } = "Default";
}

/// <summary>
/// Representa un servidor de NuGet utilizado dentro de la configuración de módulos.
/// </summary>
/// <remarks>
/// La clase NugetServerItem permite la definición de un servidor NuGet,
/// especificando detalles como la URL del servidor, el nombre, el orden y la versión del API de NuGet que utiliza.
/// </remarks>
public class NugetServerItem
{
    /// <summary>
    /// Representa la URL del servidor NuGet utilizado para obtener y gestionar paquetes.
    /// </summary>
    /// <remarks>
    /// La propiedad <c>Server</c> especifica la dirección base del servidor NuGet que
    /// se utiliza para conectarse y realizar operaciones relacionadas con los paquetes
    /// NuGet, como la descarga, la consulta de versiones disponibles y la indexación
    /// de paquetes.
    /// </remarks>
    public string Server { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que representa el nombre de un objeto o entidad.
    /// </summary>
    /// <remarks>
    /// Esta propiedad se utiliza para identificar de manera única a un objeto dentro de su contexto
    /// y puede estar sujeta a restricciones de formato o longitud dependiendo de su implementación.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// La propiedad <c>Order</c> representa el orden en el que se procesan los elementos de tipo <c>NugetServerItem</c>
    /// y <c>NugetFolderItem</c>. Este valor entero determina la secuencia de prioridad para operaciones como la descarga
    /// de paquetes en servidores NuGet o la instalación de carpetas NuGet, donde los elementos se procesan de menor a
    /// mayor orden. Un valor por defecto de 0 suele indicarse para representar el primer lugar en el orden de ejecución.
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Propiedad que indica la versión actual de la aplicación o componente.
    /// </summary>
    /// <remarks>
    /// La versión sigue el formato común de versionado, generalmente compuesto por
    /// números mayor, menor, de revisión y compilación.
    /// </remarks>
    public int Version { get; set; } = 2;

    /// <summary>
    /// Propiedad que indica si el objeto actual está activo o no.
    /// </summary>
    /// <remarks>
    /// Un valor de <c>true</c> significa que el objeto está activo, mientras que <c>false</c> indica que está inactivo.
    /// Esta propiedad es útil para determinar el estado actual del objeto en escenarios donde se requiere saber si debe procesarse o no.
    /// </remarks>
    public bool Active { get; set; } = true;
}

/// <summary>
/// Representa un elemento de configuración para una carpeta de paquetes NuGet dentro del sistema.
/// </summary>
/// <remarks>
/// La clase NugetFolderItem se utiliza para definir las propiedades de una carpeta donde se
/// alojan los paquetes NuGet. Esto es parte de la configuración de un módulo que maneja
/// diferentes carteras de NuGet.
/// </remarks>
public class NugetFolderItem
{
    /// La propiedad `Folder` representa la ubicación del directorio asociada con un elemento de Nuget dentro de la configuración de módulos.
    /// Esta propiedad es utilizada principalmente para especificar el directorio donde se almacenan los paquetes Nuget, y puede ser clave
    /// para la instalación y gestión de dichos paquetes en la aplicación.
    /// En el contexto de gestión de ensamblados, como se usa en `HAssemblyManager`, `Folder` es parte del proceso para determinar rutas de búsqueda
    /// y la carpeta de instalación para los paquetes Nuget. Asegura que el directorio existe y organiza las rutas de búsqueda en base a
    /// la configuración especificada en `ModulesSettings`.
    /// El uso correcto de `Folder` es importante para mantener la estructura de directorios adecuada y para que el sistema pueda acceder
    /// a los recursos Nuget necesarios sin inconvenientes.
    /// /
    public string Folder { get; set; } = string.Empty;

    /// <summary>
    /// Propiedad que representa el nombre asociado a un objeto o entidad.
    /// </summary>
    /// <remarks>
    /// El valor de esta propiedad suele utilizarse para identificar de forma única o describir al objeto o entidad en distintas operaciones o contextos.
    /// </remarks>
    public string Name { get; set; } = string.Empty;

    /// La propiedad "Order" indica el orden de prioridad que se debe asignar a un elemento dentro de una colección, como puede ser una lista de servidores NuGet o carpetas NuGet.
    /// Esta propiedad es utilizada para determinar el orden en el que se procesan o se muestran los elementos.
    /// Un valor numérico más bajo generalmente indicará una mayor prioridad en el orden de manejo.
    /// /
    public int Order { get; set; } = 0;

    /// <summary>
    /// Propiedad que indica si una carpeta específica debe ser utilizada como la carpeta de instalación.
    /// </summary>
    /// <remarks>
    /// Esta propiedad determina si la carpeta asociada será utilizada para la instalación de paquetes NuGet.
    /// Si se establece en <c>true</c>, el directorio correspondiente será designado como la carpeta de
    /// instalación donde se gestionarán y almacenarán los paquetes descargados.
    /// </remarks>
    public bool InstallFolder { get; set; } = false;

    /// <summary>
    /// Indica si el estado actual es activo o no.
    /// </summary>
    /// <remarks>
    /// La propiedad se puede utilizar para verificar y establecer si un objeto o proceso está
    /// en funcionamiento o habilitado dentro del contexto de la aplicación.
    /// </remarks>
    public bool Active { get; set; } = true;
}