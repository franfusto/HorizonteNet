### Arquitectura del Sistema de Gestión Modular HorizonteNet

El entorno modular **HorizonteNet** proporciona un marco extensible sobre .NET 10 para la carga dinámica, aislamiento, resolución de dependencias y ejecución de módulos, comandos y servicios en segundo plano.

Este documento define la estructura arquitectónica del sistema de gestión modular, redistribuyendo las responsabilidades del núcleo para lograr alta cohesión, desacoplamiento y claridad en las interfaces del sistema.

---

### Visión General de la Arquitectura

El núcleo del sistema modular evoluciona desde un modelo monolítico hacia una arquitectura orquestada por un coordinador central (`IEnvCoordinator`, evolución de `IhAssemblyManager`) que gobierna cuatro áreas clave del entorno, delegando en subsistemas especializados y servicios auxiliares:

1. **Ensamblados y Recursos (`IHAssemblySolver`)**: Resolución de dependencias, descarga de paquetes NuGet, gestión de caché de ensamblados (`IHAssemblyCache`), selección de versiones (`IPackageSolver`), andamiaje de enlaces simbólicos desde `.targets` (`ISymLinkScafolder`) y registro en memoria de activos estáticos web por dominio (`StaticFileRegistry`).
2. **Dominios y Aislamiento (`IHDomainManager`)**: Gestión del ciclo de vida de los contextos de carga (`AssemblyLoadContext`), dominios lógicos coleccionables, aislamiento de memoria y recarga en caliente (*Hot Reload*).
3. **Módulos y Comandos (`IHGesCom`)**: Descubrimiento dinámico de módulos (`[HorizonteModule]`), registro e invocación síncrona/asíncrona de comandos por roles, tipados o mediante JSON.
4. **Servicios en Segundo Plano (`IHWorkersManager`)**: Control del ciclo de vida, supervisión, arranque y detención de tareas y trabajadores (*Workers*) en segundo plano.

#### Esquema General de Integración y Flujo del Sistema

```
+----------------------------------------------------------------------------------------------------+
|                         CAPA DE CONSUMIDORES Y SERVICIOS DE APLICACIÓN                             |
|                                                                                                    |
|  +--------------------+  +----------------------+  +---------------------+  +-------------------+  |
|  | Interfaz Blazor/UI |  | Middleware ASP.NET   |  | Servicios / Workers |  | Scripts / Editor  |  |
|  | (AdminUi, Grids,   |  | (HorizonteStaticFile |  | (BackgroundServices,|  | (Monaco,          |  |
|  |  AssembliesViewer) |  |  Middelware)         |  |  WebWorker)         |  |  Completions)     |  |
|  +---------+----------+  +----------+-----------+  +----------+----------+  +---------+---------+  |
+------------|------------------------|-------------------------|-----------------------|------------+
             |                        |                         |                       |
             | (Load/Unload/Reload,   | (StaticFileRegistry     | (Start/StopWorker,    | (Assemblies,
             |  DomainChanged)        |  GetFile por Dominio)   |  BackgroundRunning)   |  CreateInstance)
             v                        v                         v                       v
+----------------------------------------------------------------------------------------------------+
|                                         IEnvCoordinator                                            |
|                                      (Coordinador Central)                                         |
|  - Fachada unificada y de compatibilidad para minimizar cambios en el código existente             |
|  - Orquestación de ciclo de vida: LoadDomain, UnloadDomain, ReloadDomain, DomainChanged            |
|  - Exposición de estado consolidado: Assemblies, AssembliesByDomain, StaticFileRegistry             |
|  - Delegación de invocaciones a los 4 subsistemas especializados                                   |
+----------------------------------------------------------------------------------------------------+
       |                                |                             |                        |
       v                                v                             v                        v
+---------------+               +---------------+             +---------------+        +---------------+
|IHAssemblySolve|               |IHDomainManager|             |    IHGesCom   |        |IHWorkersManage|
| (Ensamblados  |               |  (Dominios &  |             |   (Módulos &  |        |  (Servicios & |
|   & Recursos) |               |     ALC)      |             |   Comandos)   |        |   Workers)    |
+-------+-------+               +---------------+             +---------------+        +---------------+
        |                                                                                      |
        +-------------------+--------------------+--------------------+                        | (Gestiona)
        |                   |                    |                    |                        v
        v                   v                    v                    v                +---------------+
+---------------+   +---------------+    +---------------+    +---------------+        | Background    |
|IHAssemblyCache|   | IPackageSolver|    |ISymLinkScafold|    |StaticFileRegis|        | Services      |
| (Caché Resol.)|   | (Resol. NuGet)|    |(Links .targets|    | (Mapeo Web    |        +---------------+
+---------------+   +---------------+    | a dir proceso)|    |  por Dominio) |
                                         +---------------+    +-------+-------+
                                                                      |
                                                                      +---> [Consultado por el
                                                                             Middleware Web]
```

---

### Componentes Principales y Responsabilidades

#### 1. `IEnvCoordinator` (Coordinador del Entorno)
Es el punto de entrada y orquestador central de la lógica del sistema modular. Diseñado como evolución directa de `IhAssemblyManager`, asume el rol de fachada y orquestador principal para que el resto de componentes (UI, Middleware Web, Workers, Editores de Scripts) interactúen de forma transparente y con el menor impacto en cambios de código.

- **Estrategia de Compatibilidad y Fachada**:
  - Para evitar refactorizaciones masivas en el resto de la solución, `IEnvCoordinator` expone propiedades y métodos de conveniencia que redirigen internamente a los subsistemas correspondientes:
    - **Gestión de Dominios**: `LoadDomainAsync` / `LoadDomain`, `UnloadDomainAsync` / `UnloadDomain`, `ReloadDomainAsync` / `ReloadDomain`, `DomainChanged` -> Orquestados coordinando `IHDomainManager`, `IHGesCom`, `IHWorkersManager` e `IHAssemblySolver`.
    - **Ensamblados y Estado**: `Assemblies`, `AssembliesByDomain` -> Consultados agregando `IHDomainManager` y el contexto por defecto.
    - **Activos Web**: `StaticFileRegistry` -> Expuesto a través de `IEnvCoordinator` delegando en `IHAssemblySolver.StaticFileRegistry` (consumido directamente por `HorizonteStaticFileMiddelware` y visores de archivos).
    - **Servicios en Segundo Plano**: `StartBackgroundService`, `StopBackgroundService`, `BackgroundServiceRunning` -> Delegados a `IHWorkersManager.StartWorker`, `StopWorker`, `WorkerIsRunning`.
    - **Gestión de Comandos y Módulos**: `LoadModule`, `UnloadModule`, `LoadCommandsByDomain`, `UnloadCommandsByDomain` -> Delegados a `IHGesCom`.
    - **Instanciación**: `CreateInstance(Type type)` -> Resolución automática de tipos mediante el contenedor de dependencias (`IServiceProvider`).

- **Definición de Interfaz Propuesta**:
```csharp
namespace Horizonte.Interfaces;

public interface IEnvCoordinator
{
    // Acceso a los subsistemas delegados principales
    IHAssemblySolver AssemblySolver { get; }
    IHDomainManager DomainManager { get; }
    IHGesCom GesCom { get; }
    IhWorkersManager WorkersManager { get; }

    // Estado global consolidado (consumido por UI, Scripts, Middleware)
    IReadOnlyList<Assembly> Assemblies { get; }
    IReadOnlyDictionary<string, List<Assembly>> AssembliesByDomain { get; }
    StaticFileRegistry StaticFileRegistry { get; }

    // Eventos de ciclo de vida
    event Action? DomainChanged;

    // Coordinación de ciclo de vida de dominios (Hot Reload y Gestión)
    Task LoadDomainAsync(string domainName);
    Task LoadDomainAsync(string domainName, IEnumerable<byte[]> assemblies);
    Task UnloadDomainAsync(string domainName);
    Task ReloadDomainAsync(string domainName);

    // Métodos de compatibilidad de servicios en segundo plano (Workers)
    bool StartBackgroundService(string serviceType);
    bool StopBackgroundService(string serviceType);
    bool BackgroundServiceRunning(string serviceType);

    // Métodos de compatibilidad para gestión de módulos y comandos por dominio
    void LoadModule(string domainName);
    void UnloadModule(string domainName);
    void LoadCommandsByDomain(string domainName);
    void UnloadCommandsByDomain(string domainName);

    // Instanciación y resolución mediante DI
    object? CreateInstance(Type type);
}
```

---

#### 2. `IHAssemblySolver` (Gestor de Ensamblados y Recursos)
Especializado en la resolución de dependencias, descarga de paquetes NuGet, gestión de enlaces simbólicos mediante archivos `.targets`, registro de activos web por dominio y la **caché de ensamblados**.

- **Responsabilidades**:
  - Localizar y resolver rutas físicas de DLLs y dependencias transitivas.
  - Integrar la búsqueda y descarga de paquetes desde feeds NuGet remotos/locales.
  - Administrar la **Caché de Ensamblados** (`IHAssemblyCache`) para optimizar tiempos de carga e inicios subsecuentes.
  - Gestionar `ISymLinkScafolder`: procesa los archivos MSBuild `.targets` y `.props` de los paquetes NuGet (`build/` y `buildTransitive/`) para recrear el comportamiento de compilación `CopyToOutputDirectory` mediante enlaces simbólicos en el directorio de trabajo de la aplicación (nivel de proceso).
  - Mantener el registro de activos estáticos web (`StaticFileRegistry`) en memoria mapeado por dominio, permitiendo al middleware web (`HorizonteStaticFileMiddelware`) despachar recursos web específicos por dominio sin colisiones en disco.

- **Puntos de Entrada para la Resolución de Ensamblados**:
  Las funciones principales de resolución provienen de la interfaz de ensamblados y actúan como punto de consulta hacia el solver, proporcionando como parámetros esenciales el **nombre del ensamblado**, la **versión** y opcionalmente el **dominio / ALC de destino**:
  - `ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName)`: Invocado por los eventos de resolución del ALC correspondiente cuando se requiere un tipo o ensamblado dependiente en dicho contexto.
  - `ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args)`: Punto de resolución global frente a eventos `AppDomain.CurrentDomain.AssemblyResolve` o resoluciones dinámicas delegadas.
  - `ResolveAssemblyDllPath(string packageName, string version)`: Resuelve y localiza la ruta física absoluta a la librería (.dll) de un paquete NuGet específico.

- **Servicios Especializados de Resolución y Caché**:
  Para separar de forma limpia la estrategia de almacenamiento/recuperación rápida frente a la lógica de búsqueda, descarga e inspección de paquetes, `IHAssemblySolver` delega en dos componentes especializados:

  1. **`IHAssemblyCache` (Caché de Ensamblados)**:
     - **Consulta (`Get` / `TryGet`)**: Busca un elemento en caché a partir del nombre del ensamblado, versión y opcionalmente el nombre del dominio de destino.
     - **Añadir Elemento (`Add` / `Set`)**: Registra el resultado de una resolución en la caché (incluyendo resoluciones fallidas o nulas).
     - **Limpiar Caché (`Clear`)**: Invalida, purga y reinicializa el almacén de caché en memoria o disco.
     - **Soporte de Caché Negativa**: Si una búsqueda devuelve un registro cuyo ensamblado o ruta es `null`, indica que la resolución ya fue intentada previamente sin éxito, evitando reintentos costosos e innecesarios contra el solver o la red.

  2. **`IPackageSolver` (Solver y Selector de Paquetes)**:
     - Responsable de determinar la librería DLL concreta a utilizar.
     - Inspecciona los repositorios locales (`NugetFolders`, caché de usuario) y los servidores remotos (`NugetServers`) para evaluar las versiones disponibles de un paquete.
     - Aplica las reglas y parámetros de configuración (`FrameworkPriorities`, versiones forzadas o mínimas) para decidir si reutilizar un paquete local o descargar e instalar una nueva versión en la carpeta de paquetes.

- **Estructura del Elemento de Caché (`AssemblyCacheItem`)**:
  Cada entrada almacenada en `IHAssemblyCache` contiene la información relevante del resultado de resolución:

```csharp
namespace Horizonte.Entities;

public class AssemblyCacheItem
{
    public string AssemblyName { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string DomainName { get; set; } = "Default";
    public string? PackageId { get; set; }
    public string? PackageVersion { get; set; }
    public string? DllPath { get; set; }
    public Assembly? ResolvedAssembly { get; set; }
    public bool IsResolved { get; set; }
    public bool Found { get; set; }
    public DateTime ResolvedAtUtc { get; set; } = DateTime.UtcNow;
}
```

- **Interfaces Propuestas para `IHAssemblyCache` e `IPackageSolver`**:

```csharp
namespace Horizonte.Interfaces;

public interface IHAssemblyCache
{
    AssemblyCacheItem? Get(string assemblyName, string? version = null, string? domainName = null);
    void Add(AssemblyCacheItem item);
    void Clear();
    bool Contains(string assemblyName, string? version = null, string? domainName = null);
}

public interface IPackageSolver
{
    Task<PackageResolutionResult> ResolvePackageAsync(string packageOrAssemblyName, string? requestedVersion = null, string? domainName = null);
    string? ResolveLocalDllPath(string packageName, string version);
    Task<string?> DownloadAndInstallPackageAsync(string packageName, string version);
}
```

- **Esquema de Flujo de Resolución e Interacción**:
  A continuación se detalla el esquema conceptual del proceso de resolución paso a paso:

```
[ Solicitud de Entrada ] (ResolveAssemblyFromALC / ResolveAssemblyFromNuGetPackages / ResolveAssemblyDllPath)
                         [Parámetros: AssemblyName, Versión, Dominio (Opcional)]
                                                    |
                                                    v
                                  +------------------------------------+
                                  | 1. Normalización de Nombre/Versión |
                                  |  - AssemblyToPackageMap            |
                                  +-----------------+------------------+
                                                    |
                                                    v
                                  +------------------------------------+
                                  | 2. Consulta a IHAssemblyCache      |
                                  |  - (AssemblyName, Version, Domain) |
                                  +-----------------+------------------+
                                                    |
                               +--------------------+--------------------+
                               |                                         |
                       [¿Existe en Caché?]                               |
                               | SÍ                                      | NO
                               v                                         v
               +-------------------------------+         +----------------------------------+
               | 3. Registro en Caché Hallado  |         | 4. Invocación a IPackageSolver   |
               |  - Si Dll/Assembly != null    |         |  - Validación BlockedPackages    |
               |    -> Retorna Ensamblado      |         |    (Política: null/error/mayor)  |
               |  - Si Dll/Assembly == null    |         |  - Inspección NugetFolders       |
               |    -> Ya falló previamente    |         |  - Consulta NugetServers         |
               |       (Retorna null inmediato)|         |  - Descarga si es requerida      |
               +-------------------------------+         |  - Selecciona DLL adecuada       |
                                                         +-----------------+----------------+
                                                                           |
                                                                           v
                                                         +----------------------------------+
                                                         | 5. Almacenar en IHAssemblyCache  |
                                                         |  - Añadir AssemblyCacheItem      |
                                                         |    (Éxito o Fallo/Null)          |
                                                         +-----------------+----------------+
                                                                           |
                                                                           v
                                                         +----------------------------------+
                                                         | 6. Procesamiento de Recursos     |
                                                         |  - ISymLinkScafolder (.targets)  |
                                                         |  - StaticFileRegistry (por dom.) |
                                                         |  - Carga en ALC y Retorno Final  |
                                                         +----------------------------------+
```

- **Definición de Interfaz Propuesta**:
```csharp
namespace Horizonte.Interfaces;

public interface IHAssemblySolver
{
    // Registro de archivos estáticos y activos web
    StaticFileRegistry StaticFileRegistry { get; }

    // Resolución de ensamblados
    Assembly? ResolveAssemblyFromALC(AssemblyLoadContext context, AssemblyName assemblyName);
    Assembly? ResolveAssemblyFromNuGetPackages(object? sender, ResolveEventArgs args);
    string? ResolveAssemblyDllPath(string packageName, string version);

    // Gestión de paquetes y NuGet
    Task<string?> DownloadNuGetPackageAsync(string packageName, string version);
    string GetPackageName(string assemblyName);

    // Caché de ensamblados
    void CacheAssembly(string key, byte[] assemblyBytes);
    byte[]? GetCachedAssembly(string key);
    bool IsAssemblyCached(string key);
}
```

---

#### 3. `IHDomainManager` (Gestor de Dominios y ALC)
Especializado en el manejo directo de los contextos de carga de .NET (`AssemblyLoadContext`), el aislamiento de memoria y el ciclo de vida de cada dominio lógico.

- **Responsabilidades**:
  - Crear, configurar y destruir instancias de `AssemblyLoadContext` coleccionables (*Collectible ALC*).
  - Cargar ensamblados en dominios específicos desde rutas de archivos o desde secuencias de bytes (`byte[]`).
  - Administrar el ciclo de descarga de memoria y forzar la recolección de basura (`GC.Collect`) asociada a los dominios descargados.
  - Notificar cambios en la estructura de dominios activos.

- **Definición de Interfaz Propuesta**:
```csharp
namespace Horizonte.Interfaces;

public interface IHDomainManager
{
    IReadOnlyDictionary<string, AssemblyLoadContext> Domains { get; }
    
    AssemblyLoadContext? GetDomain(string domainName);
    AssemblyLoadContext CreateDomain(string domainName, bool isCollectible = true);
    
    Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName);
    Task<AssemblyLoadContext> LoadDomainContextAsync(string domainName, IEnumerable<byte[]> assemblyBytes);
    Task UnloadDomainContextAsync(string domainName);

    IEnumerable<Assembly> GetDomainAssemblies(string domainName);
}
```

---

#### 4. `IHGesCom` (Gestor de Comandos y Módulos)
Mantiene el registro dinámico de comandos y gestiona la ejecución modular basada en roles. Permite la invocación síncrona, asíncrona y mediante JSON.

- **Responsabilidades**:
  - Inspeccionar ensamblados en busca de clases marcadas con `[HorizonteModule]` y atributos de comando.
  - Registrar y desregistrar comandos dinámicamente según el dominio al que pertenezcan.
  - Ejecutar la inicialización de módulos mediante el rol `"init"`.
  - Servir como despachador de invocaciones tipadas o desacopladas mediante cadenas JSON.

- **Definición de Interfaz**:
```csharp
namespace Horizonte.Interfaces;

public interface IHGesCom
{
    void LoadModules();
    void InitzializeModules();
    List<HGesCom.RoleListItem> GetRoleCommands(string role);

    object? RunCommand(string commandKey, object[]? args = null);
    T? RunCommand<T>(string commandKey, object[]? args = null);
    Task<object?> RunCommandAsync(string commandKey, object[]? args = null);
    Task<T> RunCommandAsync<T>(string commandKey, object[]? args = null);

    string? RunCommandJson(string commandKey, string[]? jsonArgList);
    Task<string?> RunCommandJsonAsync(string commandKey, string[]? jsonArgList = null);

    bool IsAsyncCommand(string commandKey);
    bool ExistCommand(string commandKey);
    IEnumerable<HCommand> GetCommandList();
    HCommand? GetHCommand(string commandKey);

    void RegisterCommand(HCommand command);
    void LoadCommandsByDomain(string domainName);
    void UnloadCommandsByDomain(string domainName);
}
```

---

#### 5. `IHWorkersManager` (Gestor de Servicios en Segundo Plano)
Encargado de la supervisión y control del ciclo de vida de los servicios de fondo (*Background Services* / *Workers*).

- **Responsabilidades**:
  - Registrar y arrancar automáticamente los servicios configurados en el arranque.
  - Consultar definiciones de *workers* disponibles (`WorkerDefItem`).
  - Controlar de forma unitaria o agrupada el arranque (`StartWorker`) y parada (`StopWorker`) de los servicios.
  - Monitorizar el estado de ejecución de cada servicio (`WorkerIsRunning`).

- **Definición de Interfaz**:
```csharp
namespace Horizonte.Interfaces;

public interface IhWorkersManager
{
    void StartWorkers();
    List<WorkerDefItem> GetAvailableWorkers();
    bool StartWorker(string serviceName);
    bool StopWorker(string serviceName);
    bool WorkerIsRunning(string serviceName);
}
```

---

### Flujos de Interacción y Coordinación

#### Flujo de Recarga en Caliente de un Dominio (*Hot Reload*)

Cuando se solicita la recarga de un dominio a través de `IEnvCoordinator.ReloadDomainAsync("MiDominio")`, se ejecuta la siguiente secuencia coordinada:

```
[Cliente / AdminUI]
        |
        v
[IEnvCoordinator.ReloadDomainAsync]
        |
        +---> 1. Detener servicios del dominio (IHWorkersManager)
        |
        +---> 2. Desregistrar comandos del dominio (IHGesCom)
        |
        +---> 3. Descargar el ALC y liberar memoria (IHDomainManager)
        |
        +---> 4. Limpiar/Actualizar resolución y enlaces (IHAssemblySolver)
        |
        +---> 5. Crear nuevo ALC y cargar ensamblados (IHDomainManager + IHAssemblySolver)
        |
        +---> 6. Escanear y registrar nuevos comandos (IHGesCom)
        |
        +---> 7. Reiniciar servicios asociados (IHWorkersManager)
        |
        +---> 8. Disparar evento DomainChanged
```

---

### Matriz de Distribución de Responsabilidades

| Funcionalidad / Área | IEnvCoordinator | IHAssemblySolver | IHDomainManager | IHGesCom | IHWorkersManager |
| :--- | :---: | :---: | :---: | :---: | :---: |
| **Orquestación Global** | **Principal** | - | - | - | - |
| **Caché y Descarga NuGet** | Delegado | **Principal** | - | - | - |
| **Archivos Estáticos Web** | Delegado | **Principal** | - | - | - |
| **Ciclo de Vida ALC** | Delegado | - | **Principal** | - | - |
| **Aislamiento de Ensamblados** | Delegado | Consulta | **Principal** | - | - |
| **Registro de Comandos** | Delegado | - | - | **Principal** | - |
| **Invocación por Roles/JSON** | Delegado | - | - | **Principal** | - |
| **Ciclo de Vida de Workers** | Delegado | - | - | - | **Principal** |
| **Instanciación con DI** | **Principal** | - | - | - | - |

---

### Definición del Contexto y Elementos de Configuración

El entorno modular de **HorizonteNet** se fundamenta en un sistema de contexto (`IhContext`), materializado a través de archivos de configuración JSON (por ejemplo, `horizonte.json` / `horizonte.default.json`) que articulan y definen la estructura operativa de la aplicación.

#### Sección Central: `ModulesSettings`

`ModulesSettings` es el componente de configuración primordial dentro del contexto. Centraliza toda la información requerida para el descubrimiento y carga de módulos, la resolución e instalación de paquetes NuGet, las rutas locales, el mapeo de ensamblados y la configuración de dominios de ejecución.

Con el objetivo de unificar y simplificar el contexto de configuración, la sección de paquetes forzados (`ForcedPackageItem`), que anteriormente se encontraba desacoplada o separada, se integra directamente como parte de `ModulesSettings` mediante la propiedad `ForcedPackages`.

#### Estructura y Propiedades de `ModulesSettings`

1. **`List` (`List<ModulesSettingsItem>`)**:
   - Define el catálogo de módulos a inicializar en la aplicación.
   - Cada entrada especifica el nombre del módulo (`ModuleName`), versión (`ModuleVersion`), ruta local opcional (`Path`), si requiere carga de librerías adicionales (`LoadAdditionalDlls`), el dominio asignado (`Domain`) y su estado de activación (`Active`).

2. **`NugetServers` (`List<NugetServerItem>`)**:
   - Lista priorizada de feeds y repositorios de paquetes NuGet (públicos como NuGet.org o privados como BaGet/Nexus).
   - Incluye URL del servidor, nombre descriptivo, versión del protocolo API (v2 / v3), orden de prioridad y flag de activación.

3. **`NugetFolders` (`List<NugetFolderItem>`)**:
   - Directorios locales de paquetes utilizados para la resolución o almacenamiento de paquetes instalados.
   - Contiene la ruta (`Folder`), identificador, orden de búsqueda, indicador de carpeta de instalación (`InstallFolder`) y estado activo.

4. **`UseUserNugetFolder` (`bool`)**:
   - Bandera booleana que habilita o deshabilita la utilización de la caché global de paquetes NuGet del usuario del sistema operativo (`~/.nuget/packages`).

5. **`FrameworkPriorities` (`List<string>`)**:
   - Lista ordenada de Framework Monikers (TFMs) por orden de preferencia para la resolución de dependencias (ej. `net10.0`, `net9.0`, `netstandard2.1`, etc.).

6. **`ForcedPackages` (`List<ForcedPackageItem>`)** *(Consolidado en ModulesSettings)*:
   - Colección de paquetes NuGet cuya presencia y carga es crítica y obligatoria para el funcionamiento del sistema o módulos dependientes.
   - Cada elemento especifica:
     - `PackageId`: Identificador del paquete NuGet.
     - `Version`: Versión requerida.
     - `Framework`: Moniker de framework destino compatible.
     - `Domain`: Dominio / ALC en el que debe cargarse (por defecto `"Default"`).
     - `Active`: Indicador de procesamiento activo.

7. **`AssemblyToPackageMap` (`Dictionary<string, string>`)**:
   - Diccionario de mapeo explícito entre nombres de ensamblados (.dll) y el identificador del paquete NuGet que los contiene, resolviendo discrepancias donde el nombre del ensamblado difiere del identificador de paquete.

8. **`BlockedPackages` (`List<BlockedPackageItem>`)**:
   - Lista de paquetes o versiones bloqueadas/excluidas. Permite prevenir que el solver de paquetes (`IHAssemblySolver` / `IPackageSolver`) seleccione, descargue o cargue versiones de paquetes marcadas como no seguras (por ejemplo, con vulnerabilidades de seguridad o incompatibilidades críticas).
   - Cada elemento especifica:
     - `PackageId`: Identificador del paquete NuGet bloqueado.
     - `Version`: Versión específica o patrón de versión bloqueada.
     - `Reason`: Motivo o descripción del bloqueo (ej. alerta de seguridad / CVE, incompatibilidad).
     - `Active`: Indicador de regla de bloqueo activa.

9. **`BlockedPackagesPolicy` (`BlockedPackagesPolicy` / `string`)**:
   - Política o estrategia de actuación que debe aplicar el solver de paquetes cuando se detecta una coincidencia con un paquete o versión bloqueada en `BlockedPackages`.
   - Valores admitidos (enum / string):
     - `"null"` (`BlockedPackagesPolicy.Null`): Devuelve `null` (cancela la resolución del paquete sin lanzar excepción).
     - `"error"` (`BlockedPackagesPolicy.Error`): Produce un error o excepción de resolución notificando el bloqueo de seguridad/incompatibilidad.
     - `"mayor"` (`BlockedPackagesPolicy.Mayor`): Busca y selecciona la versión superior/más reciente disponible y no bloqueada para dicho paquete NuGet.

10. **`Domains` (`List<string>`)**:
   - Lista declarativa de los dominios lógicos (ALCs) a inicializar y gestionar dentro del ciclo de vida de la aplicación.

#### Modelo de Datos C# de Configuración

```csharp
namespace Horizonte.Settings;

public enum BlockedPackagesPolicy
{
    Null,
    Error,
    Mayor
}

public class ModulesSettings
{
    public List<ModulesSettingsItem> List { get; set; } = [];
    public List<NugetServerItem> NugetServers { get; set; } = new();
    public List<NugetFolderItem> NugetFolders { get; set; } = new();
    public bool UseUserNugetFolder { get; set; } = false;
    public List<string> FrameworkPriorities { get; set; } = new();
    public List<ForcedPackageItem> ForcedPackages { get; set; } = new();
    public List<BlockedPackageItem> BlockedPackages { get; set; } = new();
    public BlockedPackagesPolicy BlockedPackagesPolicy { get; set; } = BlockedPackagesPolicy.Null;
    public Dictionary<string, string> AssemblyToPackageMap { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Domains { get; set; } = new();
}

public class ForcedPackageItem
{
    public bool Active { get; set; } = true;
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Framework { get; set; } = string.Empty;
    public string Domain { get; set; } = "Default";
}

public class BlockedPackageItem
{
    public bool Active { get; set; } = true;
    public string PackageId { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
```

#### Ejemplo de Representación en JSON del Contexto

```json
{
  "ModulesSettings": {
    "List": [
      {
        "Active": true,
        "ModuleName": "Horizonte",
        "ModuleVersion": "10.0.6-beta",
        "Path": "",
        "LoadAdditionalDlls": false,
        "Domain": "Default"
      }
    ],
    "NugetServers": [
      {
        "Server": "https://api.nuget.org/v3/index.json",
        "Name": "Nuget.org",
        "Order": 1,
        "Version": 3,
        "Active": true
      }
    ],
    "NugetFolders": [
      {
        "Folder": "packages",
        "Name": "Horizonte",
        "Order": 0,
        "InstallFolder": true,
        "Active": true
      }
    ],
    "UseUserNugetFolder": false,
    "FrameworkPriorities": [
      "net10.0",
      "netstandard2.1",
      "net8.0"
    ],
    "ForcedPackages": [
      {
        "Active": true,
        "PackageId": "Newtonsoft.Json",
        "Version": "13.0.3",
        "Framework": "net10.0",
        "Domain": "Default"
      }
    ],
    "BlockedPackages": [
      {
        "Active": true,
        "PackageId": "Vulnerable.Package.Sample",
        "Version": "1.0.0",
        "Reason": "Vulnerabilidad de seguridad crítica (CVE-XXXX)"
      }
    ],
    "BlockedPackagesPolicy": "mayor",
    "AssemblyToPackageMap": {
      "Microsoft.CodeAnalysis": "Microsoft.CodeAnalysis.Common"
    },
    "Domains": [
      "Default"
    ]
  }
}
```

---

### Apéndice: Procesos Detallados y Lógica de Negocio

#### A.1. Lógica de Resolución, Carga de Ensamblados y Gestión de Dominios

El sistema de gestión modular de **HorizonteNet** implementa un modelo de aislamiento basado en múltiples contextos de carga (`AssemblyLoadContext` - ALC). Cada solicitud de ensamblado se realiza siempre en el contexto de un dominio específico, garantizando que siempre exista al menos el dominio principal `"Default"`.

##### 1. Premisas y Reglas Fundamentales del Runtime

1. **Dominio `"Default"` (`AssemblyLoadContext.Default`)**:
   - Es el contexto raíz del proceso .NET donde residen el host de la aplicación, el framework base y los ensamblados centrales.
   - Cualquier ensamblado cargado en `"Default"` es visible para los ALCs secundarios si estos delegan la resolución hacia el contexto por defecto.
   - No es coleccionable (*non-collectible*): los ensamblados cargados en `"Default"` no pueden descargarse de memoria sin reiniciar el proceso.

2. **Dominios Secundarios Aislados (ALCs Coleccionables)**:
   - Creados dinámicamente (`DominioA`, `DominioB`, etc.) con aislamiento de tipos y memoria.
   - Permiten la convivencia simultánea de múltiples versiones de un mismo paquete/librería en memoria (aislamiento multi-versión).
   - Son coleccionables (*collectible*): pueden descargarse individualmente mediante el ciclo de vida orquestado por `IEnvCoordinator` (*Unload / Hot Reload*).

3. **Inmutabilidad por ALC**:
   - Dentro de un mismo dominio (sea `"Default"` o un ALC aislado), una vez que un ensamblado con un `AssemblyName` concreto ha sido cargado, el runtime no permite cargar una versión diferente de ese mismo ensamblado en ese mismo contexto.

---

#### A.2. Esquema de Flujo: Resolución, Symlinks y StaticFileRegistry

A continuación se ilustra la interacción entre la solicitud de carga por dominio, el solver/caché, la carga en memoria ALC, la creación de enlaces simbólicos (`ISymLinkScafolder`) y el registro de activos web (`StaticFileRegistry`):

```
[ Petición de Carga de Paquete/Ensamblado ]
  (PackageId / AssemblyName, Versión, DominioDestino)
                     |
                     v
+-------------------------------------------------------------+
| 1. Identificación y Validación del Dominio Destino           |
|    - Si Dominio == "Default" -> ALC Default                 |
|    - Si Dominio == "DominioX" -> Obtener/Crear ALC Aislado  |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
| 2. Resolución de Librería DLL (IHAssemblySolver)            |
|    - Consulta en IHAssemblyCache (AssemblyName, Ver, Domain)|
|    - Si no existe -> IPackageSolver busca en NugetFolders/  |
|      NugetServers y descarga/selecciona DLL correspondiente |
|    - Actualización de IHAssemblyCache                       |
+------------------------------+------------------------------+
                               |
                               v
+-------------------------------------------------------------+
| 3. Carga en Contexto de Memoria (IHDomainManager)           |
|    - ALC.LoadFromAssemblyPath(dllPath) o LoadFromStream     |
|    - Aislamiento de tipos en el dominio solicitado          |
+------------------------------+------------------------------+
                               |
            +------------------+------------------+
            |                                     |
            v                                     v
+-----------------------------------+ +-----------------------------------+
| 4. Andamiaje de Symlinks (.targets)| | 5. Registro Web por Dominio       |
|    (ISymLinkScafolder)            | |    (StaticFileRegistry)           |
| - Procesa .targets y .props de    | | - Inspecciona Content/ o          |
|   build/ y buildTransitive/       | |   staticwebassets/ del paquete    |
| - Recrea CopyToOutputDirectory con| | - Mapea /_content/{paquete}/...   |
|   enlaces simbólicos en el dir    | |   en memoria indexado por DOMINIO |
|   de trabajo (nivel de proceso)   | |   (_filesByDomain)                |
| - No sobrescribe destinos si ya   | | - Consumido por el middleware web |
|   existen enlaces previos         | |   HorizonteStaticFileMiddelware   |
+-----------------------------------+ +-----------------------------------+
```

---

#### A.3. Análisis de Casos y Escenarios de Carga Multi-Versión

El comportamiento del sistema ante la solicitud de múltiples versiones de un mismo paquete depende directamente del tipo de recursos que dicho paquete contenga:

##### Escenario 1: Paquete con Activos Web (`StaticFileRegistry`)
`StaticFileRegistry` gestiona el catálogo de activos web (`/_content/{paquete}/...`) manteniendo un diccionario en memoria indexado por dominio (`_filesByDomain` con clave compuesta `dominio#ruta`).

- **Paso 1: Solicitud de `PaqueteWeb 1.0.0` en `DominioA`**
  - **Memoria (ALC)**: `PaqueteWeb 1.0.0.dll` se carga en el ALC de `DominioA`.
  - **StaticFileRegistry**: Se registran los archivos de `1.0.0` asociados a la clave de `DominioA`.
  - **Resultado**: Las peticiones web en el contexto de `DominioA` resuelven los activos de la versión 1.0.0.

- **Paso 2: Solicitud de `PaqueteWeb 2.0.0` en `DominioB`**
  - **Memoria (ALC)**: `PaqueteWeb 2.0.0.dll` se carga en el ALC de `DominioB`.
  - **StaticFileRegistry**: Se registran los archivos de `2.0.0` asociados a la clave de `DominioB`.
  - **Resultado**: Coexistencia completa y aislada. `DominioA` sirve v1.0.0 y `DominioB` sirve v2.0.0 a través de `HorizonteStaticFileMiddelware` mediante `StaticFileRegistry.GetFile(path, domainName)`.

---

##### Escenario 2: Paquete con Enlaces Simbólicos / `.targets` (`ISymLinkScafolder`)
`ISymLinkScafolder` procesa los archivos `.targets` y `.props` de MSBuild del paquete NuGet para recrear la copia de archivos al hacer build (`CopyToOutputDirectory`) mediante enlaces simbólicos directos en el directorio de trabajo de la aplicación.

- **Paso 1: Solicitud de `PaqueteNativo 1.0.0` en `DominioA`**
  - **Memoria (ALC)**: Se carga `PaqueteNativo 1.0.0.dll` en `ALC_A`.
  - **Symlinks (Disco)**: `ISymLinkScafolder` crea los enlaces simbólicos en el directorio de trabajo apuntando a los archivos de `packages/.../PaqueteNativo.1.0.0/`.
  - **Resultado**: `DominioA` funciona correctamente con los binarios/recursos enlazados de v1.0.0.

- **Paso 2: Solicitud de `PaqueteNativo 2.0.0` en `DominioB`**
  - **Memoria (ALC)**: Se carga `PaqueteNativo 2.0.0.dll` en `ALC_B` (aislado en memoria).
  - **Symlinks (Disco)**: `ISymLinkScafolder` detecta que los archivos de destino en el directorio de trabajo ya existen (creados previamente por v1.0.0). Para prevenir corrupción o inconsistencias en los módulos activos, **no sobrescribe los enlaces existentes** y emite una advertencia.
  - **Resultado**: `DominioB` ejecuta su código C# en v2.0.0, pero cuando intente leer los archivos físicos del directorio de trabajo, accederá a los recursos físicos de **v1.0.0**.

---

#### A.4. Limitación del Sistema: Colisión en Enlaces Simbólicos y Recursos Físicos

Existe una diferencia fundamental entre el aislamiento administrado y los recursos físicos del sistema de archivos:

##### 1. Regla de Arquitectura
> **No se deben cargar dos versiones diferentes del mismo paquete si este utiliza enlaces simbólicos (`ISymLinkScafolder` / `.targets` / binarios nativos).**

##### 2. Diagnóstico: ¿Qué Ensamblados se Entregan y qué Enlaces se Utilizan?
Si a pesar de la restricción se solicita la carga de dos versiones de un paquete con `.targets` / enlaces simbólicos en dominios distintos:

| Componente | DominioA (Paquete 1.0.0 - Primero en cargar) | DominioB (Paquete 2.0.0 - Carga posterior) |
| :--- | :--- | :--- |
| **Ensamblado Entregado en Memoria** | `v1.0.0` (Aislado en `ALC_A`) | `v2.0.0` (Aislado en `ALC_B`) |
| **Ejecución de Lógica C#** | Ejecuta código de v1.0.0 | Ejecuta código de v2.0.0 |
| **Enlaces Simbólicos Activos en Disco** | **Activos** (Creados por v1.0.0) | **Omitidos** (No se sobrescriben en disco) |
| **Acceso a Archivos Físicos / Nativos en Runtime** | Accede a archivos de **v1.0.0** | **Accede a archivos de v1.0.0** *(Incompatibilidad potencial)* |

##### 3. Resumen de Diferenciación entre Subsistemas de Recursos
- **`StaticFileRegistry`**: Opera **en memoria** y soporta aislamiento multi-versión por dominio (`_filesByDomain`). Cada dominio puede tener su propia versión de assets web para el middleware.
- **`ISymLinkScafolder`**: Opera **en el sistema de archivos** (directorio de trabajo de la aplicación) recreando `<CopyToOutputDirectory>` de `.targets`. Es compartido a nivel de proceso y **no soporta multi-versión**.

##### 4. Recomendaciones Arquitectónicas
1. **Unificación en Dominio `"Default"`**: Los paquetes con dependencias nativas o archivos `.targets` deben configurarse en `ForcedPackages` o cargarse exclusivamente en el dominio `"Default"` para asegurar una versión única y consistente.
2. **Ciclo de Descarga (*Hot Reload*)**: Al descargar un dominio (`UnloadDomainAsync`), `ISymLinkScafolder` debe limpiar los enlaces y `StaticFileRegistry` debe desregistrar los mapeos del dominio correspondiente.
