### Arquitectura de Horizonte.Extensions.Interactive

La biblioteca **Horizonte.Extensions.Interactive** amplía las capacidades del marco de trabajo HorizonteNet proporcionando un entorno de ejecución interactiva en C#, compilación en memoria de scripts dinámicos y su integración en el ciclo de vida modular del sistema.

Este documento detalla la arquitectura técnica de la extensión, sus modelos de datos, la gestión de dependencias en tiempo de ejecución, la integración con .NET Interactive y Roslyn, y los mecanismos de carga aislada en dominios modulares.

---

### Visión General de la Arquitectura

`Horizonte.Extensions.Interactive` actúa como un puente entre el motor interactivo (.NET Interactive), el compilador en memoria de Roslyn (`Microsoft.CodeAnalysis.CSharp.Scripting`) y el coordinador de entorno de HorizonteNet (`IEnvCoordinator`).

El sistema proporciona dos vías operativas principales:

1. **Núcleo Interactivo (Interactive Kernel)**: Construcción y orquestación de instancias `CompositeKernel` / `CSharpKernel` configurables en modo aislado o integrado, incorporando middleware personalizado para interceptar y resolver directivas de referencia (`#r "nuget:..."` y `#r "ruta.dll"`) contra los servicios de ensamblados de HorizonteNet.
2. **Compilación y Carga Dinámica de Scripts**: Transformación de definiciones declarativas de scripts (`ScriptDef`) en ensamblados binarios ejecutables en memoria mediante Roslyn, publicándolos dinámicamente en un dominio de aplicación coleccionable (`Script`) y ejecutando automáticamente sus comandos de inicialización (`role: "init"`).

#### Esquema de Integración y Componentes

```
+----------------------------------------------------------------------------------------------------+
|                                    CAPA DE CONSUMIDORES                                            |
|                                                                                                    |
|    +-----------------------------+                  +----------------------------------------+     |
|    |      Paneles y Widgets      |                  |         Servicios y Editores           |     |
|    |   (Horizonte.Scripts, UI)   |                  |          (Jupyter, Monaco)             |     |
|    +--------------+--------------+                  +-------------------+--------------------+     |
+-------------------|-----------------------------------------------------|--------------------------+
                    |                                                     |
                    | (LoadScriptModules, Compile)                        | (BuildKernelAsync)
                    v                                                     v
+----------------------------------------------------------------------------------------------------+
|                                HORZONTE.EXTENSIONS.INTERACTIVE                                     |
|                                                                                                    |
|  +---------------------------+  +-------------------------------+  +----------------------------+  |
|  |       Modelos Base        |  |     Compilador de Scripts     |  |     Kernel Interactivo     |  |
|  |   - ScriptDef             |  |   - Compile()                 |  |   - BuildKernelAsync()     |  |
|  |   - ScriptDefCompileResp. |  |   - Roslyn CSharpCompilation  |  |   - AddHorizonteMiddleware |  |
|  |   - KernelOptions         |  |   - Mapeo de Diagnósticos     |  |   - CSharpKernel / Composite   |  |
|  +---------------------------+  +---------------+---------------+  +--------------+-------------+  |
+-------------------------------------------------|---------------------------------|----------------+
                                                  |                                 |
                                                  | (Resolución de #r               | (Resolución de #r
                                                  |  y Carga de Dominio)            |  en tiempo real)
                                                  v                                 v
+----------------------------------------------------------------------------------------------------+
|                                   NÚCLEO DE HORIZONTENET                                           |
|                                                                                                    |
|  +----------------------------------------------------------------------------------------------+  |
|  |                                       IEnvCoordinator                                        |  |
|  +---------------------+-------------------------------+--------------------------------+-------+  |
|                        |                               |                                |          |
|                        v                               v                                v          |
|              +-------------------+           +-------------------+            +-----------------+  |
|              |  IHAssemblySolver |           |  IHDomainManager  |            |     IHGesCom    |  |
|              |  - NuGet Resolver |           |  - ALC "Script"   |            |  - Init Commands|  |
|              |  - Cache & DllPath|           |  - Hot Unload/Load|            |  - Roles & Exec |  |
|              +-------------------+           +-------------------+            +-----------------+  |
+----------------------------------------------------------------------------------------------------+
```

---

### Componentes Principales y Modelos de Datos

#### 1. `KernelOptions` (Configuración de Núcleo Interactivo)

Representa las opciones de configuración necesarias para instanciar y personalizar un kernel interactivo dentro de la aplicación.

- **Responsabilidades**:
  - Definir la identidad lógica del kernel para propósitos de registro y trazabilidad.
  - Determinar el nivel de aislamiento operativo del kernel respecto al contenedor de dependencias del sistema.

- **Estructura y Parámetros**:
  - `Name`: Cadena de texto que identifica el nombre del kernel. Por defecto es una cadena vacía.
  - `Isolated`: Booleano que indica si el kernel opera en modo aislado (`true`) o integrado (`false`). En modo integrado, se inyecta automáticamente el `IServiceProvider` en las variables globales del kernel y se precargan los paquetes NuGet base de HorizonteNet y Dependency Injection.

---

#### 2. `ScriptDef` (Definición Declarativa de Script)

Representa la entidad de datos que modela un script C# ejecutable, sus metadatos y su código fuente.

- **Responsabilidades**:
  - Almacenar la identidad y descripción del script.
  - Sanitizar el nombre del script para asegurar que sea apto para nombres de ensamblado y referencias internas.
  - Gestionar el almacenamiento transparente del código fuente en texto plano o codificado en Base64 para persistencia segura en configuraciones serializables.
  - Controlar el estado de activación del script dentro del catálogo de ejecución.

- **Estructura y Propiedades**:
  - `Id`: Identificador único del script en el sistema.
  - `Name`: Nombre descriptivo del script. Al asignarse, se procesa mediante una rutina de sanitización que conserva exclusivamente caracteres alfanuméricos (`char.IsLetterOrDigit`).
  - `Description`: Descripción detallada del propósito y funcionamiento del script.
  - `Code`: Cadena que almacena el contenido del script codificado en Base64.
  - `CodeText`: Propiedad no serializada (`[JsonIgnore]`) que proporciona acceso bidireccional al código en texto legible. En lectura decodifica el valor Base64 a UTF-8 (con respaldo al texto original ante fallos), y en escritura convierte el texto plano a Base64.
  - `Active`: Indicador booleano que determina si el script debe ser compilado y cargado en el dominio de ejecución activo.

---

#### 3. `ScriptDefCompileResponse` (Respuesta de Compilación)

Estructura de transporte que encapsula el resultado del proceso de compilación emitido por Roslyn para un `ScriptDef`.

- **Responsabilidades**:
  - Transportar los bytes binarios del ensamblado generado cuando la compilación es exitosa.
  - Recopilar la lista de mensajes de error y advertencias críticas formateadas con información de línea y columna en caso de fallo sintáctico o semántico.

- **Estructura y Propiedades**:
  - `Assembly`: Arreglo de bytes (`byte[]`) que contiene la imagen DLL del ensamblado compilado en memoria.
  - `Errors`: Colección enumerable de cadenas (`IEnumerable<string>`) con el detalle de errores de diagnóstico producidos durante la compilación.

---

#### 4. `Mapping` (Mapeo de Ensamblados y Paquetes de Análisis)

Clase estática decorada con el atributo de descubrimiento de HorizonteNet `[AssemblyMappingDefinition]`, encargada de establecer la correspondencia entre nombres de ensamblados de análisis de código y sus identificadores de paquete NuGet.

- **Responsabilidades**:
  - Proveer un diccionario insensible a mayúsculas y minúsculas (`AssemblyToPackageMap`) que mapea ensamblados como `Microsoft.CodeAnalysis`, `Microsoft.CodeAnalysis.Scripting` y `Microsoft.CodeAnalysis.Workspaces` a sus respectivos identificadores de paquete NuGet.
  - Permitir que el subsistema `IHAssemblySolver` resuelva automáticamente dependencias transitivas de Roslyn solicitadas por scripts o kernels interactivos.

---

### Subsistemas Funcionales y Operaciones

```
+----------------------------------------------------------------------------------------------------+
|                               SUBSISTEMAS EN EXTENSIONS.CS                                         |
|                                                                                                    |
|  1. Motor Interactivo           2. Compilación Roslyn             3. Ciclo de Vida de Scripts      |
|  +---------------------------+  +------------------------------+  +-----------------------------+  |
|  | - BuildKernelAsync        |  | - Compile                    |  | - LoadScriptModules         |  |
|  | - AddHorizonteMiddleware  |  | - ParseText (Script)         |  | - Compilación por lotes     |  |
|  | - Intercepción #r nuget   |  | - Intercepción #r nuget/path |  | - UnloadDomain("Script")    |  |
|  | - Inyección ServiceProv.  |  | - Inyección MetadataRef      |  | - LoadDomain("Script", asms)|  |
|  | - Helper display()        |  | - Emit a MemoryStream        |  | - Disparo de Init Commands  |  |
|  +---------------------------+  +------------------------------+  +-----------------------------+  |
+----------------------------------------------------------------------------------------------------+
```

#### 1. Construcción y Middleware del Kernel Interactivo

La construcción del entorno interactivo se efectúa mediante el método de extensión `BuildKernelAsync`, el cual crea un `CompositeKernel` compuesto por un `CSharpKernel` configurado.

##### Flujo de Inicialización del Kernel
1. **Instanciación**: Se crea una instancia de `Microsoft.DotNet.Interactive.CSharp.CSharpKernel`.
2. **Configuración de Integración**:
   - Si `KernelOptions.Isolated` es `false`:
     - Se vincula el middleware personalizado de Horizonte (`AddHorizonteMiddleware`).
     - Se registra la instancia de `IServiceProvider` en el espacio de variables del kernel mediante `SetValueAsync("ServiceProvider", serviceProvider, typeof(IServiceProvider))`.
     - Se envían comandos de inicialización `SubmitCode` para referenciar paquetes base de HorizonteNet (`Horizonte` y `Microsoft.Extensions.DependencyInjection.Abstractions`).
3. **Inyección de Funciones Auxiliares**: Se define la función global `display(object x)` apuntando a la salida de consola (`Console.WriteLine(x.ToString())`).
4. **Composición**: Se ensambla el `CompositeKernel`, estableciendo el kernel C# como el kernel predeterminado.

##### Middleware de Intercepción de Ensamblados (`AddHorizonteMiddleware`)
El middleware intercepta los comandos de tipo `SubmitCode` antes de su procesamiento por el motor interactivo:
- Analiza línea por línea el código fuente en busca de directivas de referencia:
  - **Patrón NuGet**: `#r "nuget:PackageName, Version"` — Extrae el nombre del paquete y su versión opcional. Consulta a `IEnvCoordinator.AssemblySolver` (`ResolveAssemblyFromNuGetPackages` / `ResolveAssemblyDllPath`). Si el ensamblado está disponible o ubicado en disco, se normaliza su ruta absoluta y se agrega al kernel mediante `csharpKernel.AddAssemblyReferences(...)`.
  - **Patrón Ruta DLL**: `#r "ruta/al/ensamblado.dll"` — Extrae la ruta del archivo, resuelve rutas relativas respecto al directorio de trabajo actual y, si el archivo existe en disco, lo registra en las referencias del kernel.
- **Saneamiento del Código**: Si se interceptaron directivas `#r`, se eliminan del cuerpo del comando `SubmitCode` actualizando el campo interno del comando para evitar incompatibilidades en la compilación interactiva subsiguiente.

---

#### 2. Compilación de Scripts en Memoria con Roslyn

El método de extensión `Compile(this ScriptDef, IEnvCoordinator)` gestiona la compilación dinámica del código fuente de un script a binario DLL en memoria.

##### Fases del Proceso de Compilación
1. **Preprocesamiento y Extracción de Directivas**:
   - Se procesa el texto del script (`ScriptDef.CodeText`).
   - Se interceptan las directivas `#r "nuget:..."` y `#r "ruta.dll"`.
   - Se resuelven las dependencias contra `IEnvCoordinator.AssemblySolver` generando objetos `MetadataReference` basados en archivos físicos.
   - Las líneas con directivas `#r` se reemplazan por líneas vacías en el código limpio, preservando la numeración exacta de líneas para los diagnósticos de error posteriores.
2. **Generación del Árbol de Sintaxis**:
   - Se invoca `CSharpSyntaxTree.ParseText` configurado con `SourceCodeKind.Script`.
   - Se define el nombre del ensamblado de salida como `{ScriptDef.Name}.script`.
3. **Resolución de Referencias de Ensamblado**:
   - Se agregan las librerías fundamentales del entorno en ejecución (`System.Runtime.dll`, `mscorlib.dll`, `System.Private.CoreLib.dll`, `System.Console.dll`, `System.Collections.dll`, `System.Linq.dll`, `System.IO.dll`, `System.Threading.dll`, `Microsoft.CSharp.dll`).
   - Se asegura la inclusión explícita de `System.Runtime` mediante carga por reflexión.
   - Se incluye el ensamblado principal de HorizonteNet (`typeof(IEnvCoordinator).Assembly`). En caso de ejecuciones en modo archivo único (*Single File*) donde la ubicación física no está presente en disco, se resuelve dinámicamente mediante `AssemblySolver.ResolveAssemblyDllPath`.
   - Se añaden todas las referencias adicionales resueltas en el paso de preprocesamiento.
4. **Configuración de Compilación (`CSharpCompilation`)**:
   - Salida configurada como biblioteca de enlace dinámico (`OutputKind.DynamicallyLinkedLibrary`).
   - Clase de script configurada como `"Script"`.
   - Resolutor de metadatos `ScriptMetadataResolver.Default`.
   - Importaciones globales (*usings*) por defecto: `System`, `System.Collections.Generic`, `System.Linq`, `System.Text`, `System.Threading.Tasks`.
5. **Emisión y Tratamiento de Errores**:
   - Se emiten los bytes binarios a un flujo en memoria (`MemoryStream`).
   - Si la emisión no es exitosa, se filtran los diagnósticos con severidad de error o advertencia tratada como error, formateando cada mensaje con el esquema `Line {Línea},{Columna}\t: {Mensaje}` en `ScriptDefCompileResponse.Errors`.
   - Si la emisión es exitosa, el arreglo de bytes resultante se asigna a `ScriptDefCompileResponse.Assembly`.

---

#### 3. Carga y Ciclo de Vida de Módulos de Script

El método de extensión `LoadScriptModules(this IEnumerable<ScriptDef>, IServiceProvider)` automatiza el ciclo completo de compilación, descarga de dominios obsoletos, carga aislada e invocación de rutinas de inicialización.

##### Flujo de Carga de Módulos
1. **Filtrado y Compilación por Lotes**:
   - Se seleccionan todos los scripts configurados cuyo estado sea activo (`script.Active == true`).
   - Para cada script, se invoca `script.Compile(envCoordinator)`.
   - Si un script presenta errores, se registran en el log del sistema y se omite.
   - Los binarios generados se acumulan en una colección en memoria (`List<byte[]>`).
2. **Descarga y Recarga de Dominio Aislado**:
   - Se solicita a `IEnvCoordinator.UnloadDomainAsync("Script")` la descarga y liberación del contexto ALC previo asociado a los scripts.
   - Se invoca `IEnvCoordinator.LoadDomainAsync("Script", scriptasmlist)` para cargar atómicamente la lista de ensamblados en el dominio coleccionable dedicado.
3. **Ejecución Automática de Comandos de Inicialización**:
   - Se obtiene el gestor de comandos `IHGesCom` desde el proveedor de servicios.
   - Se consultan todos los comandos registrados con el rol `"init"` (`gesCom.GetRoleCommands("init")`).
   - Para cada comando perteneciente al dominio `"Script"`, se invoca de manera inmediata su ejecución mediante `gesCom.RunCommand(...)`, permitiendo que los scripts inicialicen módulos, registren widgets o configuren servicios dinámicos.

---

### Diagramas de Flujo y Secuencia

#### 1. Flujo de Ejecución en Núcleo Interactivo

```
Usuario / Consumidor
        |
        | 1. BuildKernelAsync(options, serviceProvider)
        v
+------------------------------------+
|  Construcción de CompositeKernel   |
|  - CSharpKernel creado             |
|  - Inyección de ServiceProvider    |
|  - Middleware Horizonte vinculado  |
+-----------------+------------------+
                  |
                  | 2. SubmitCode: #r "nuget:Package, 1.0"
                  v
+------------------------------------+
|       AddHorizonteMiddleware       |
|  - Regex detecta directiva #r      |
+-----------------+------------------+
                  |
                  | 3. ResolveAssemblyFromNuGetPackages()
                  v
+------------------------------------+
|         IHAssemblySolver           |
|  - Localiza DLL en caché / disco   |
+-----------------+------------------+
                  |
                  | 4. Ruta absoluta de ensamblado
                  v
+------------------------------------+
|  csharpKernel.AddAssemblyReference |
|  - Código limpio enviado a kernel  |
+-----------------+------------------+
                  |
                  | 5. Ejecución interactiva del código C#
                  v
               Resultado
```

#### 2. Flujo de Compilación y Despliegue de Módulos de Script

```
Colección ScriptDef[] (Activos)
        |
        | 1. script.Compile(envCoordinator)
        v
+------------------------------------+
|         Compilador Roslyn          |
|  - Preproceso de directivas #r     |
|  - MetadataReferences inyectadas   |
|  - CSharpCompilation.Emit()        |
+-----------------+------------------+
                  |
                  +---------------------------+
                  | (Éxito)                   | (Fallo)
                  v                           v
+------------------------------------+   +------------------------------------+
|    Binario en memoria (byte[])     |   | Errores con línea y columna        |
+-----------------+------------------+   | (Registrados en Logger)            |
                  |                      +------------------------------------+
                  | 2. Recolectar lista de bytes
                  v
+------------------------------------+
|    IEnvCoordinator.UnloadDomain    |
|    - Dominio "Script" descargado   |
+-----------------+------------------+
                  |
                  | 3. IEnvCoordinator.LoadDomain("Script", asms)
                  v
+------------------------------------+
|    IHDomainManager / ALC           |
|    - Carga ensamblados en memoria  |
|    - Descubrimiento de [HModule]   |
+-----------------+------------------+
                  |
                  | 4. Buscar comandos con rol "init" en dominio "Script"
                  v
+------------------------------------+
|              IHGesCom              |
|  - gesCom.RunCommand(commandName)  |
|  - Inicialización de widgets/datos |
+------------------------------------+
```

---

### Integración con el Ecosistema HorizonteNet

| Componente HorizonteNet | Interacción desde `Horizonte.Extensions.Interactive` |
|---|---|
| `IEnvCoordinator` | Fachada principal utilizada para coordinar la descarga y carga en caliente del dominio de scripts (`LoadDomainAsync`, `UnloadDomainAsync`). |
| `IHAssemblySolver` | Resolución de nombres de paquetes NuGet, descarga y localización de archivos `.dll` físicos a partir de directivas `#r` en scripts y notebooks. |
| `IHDomainManager` | Gestión subyacente del contexto `AssemblyLoadContext` coleccionable con identificador `"Script"`. |
| `IHGesCom` | Descubrimiento e invocación de comandos decorados con `[HorizonteCommand]` y clasificados con `[HorizonteRole("init")]` tras la carga del dominio. |
| `IServiceProvider` | Acceso a servicios registrados e inyección transparente en kernels interactivos para acceso contextual a la infraestructura de la aplicación. |
