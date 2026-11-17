### Architecture of Horizonte.Extensions.Interactive

The **Horizonte.Extensions.Interactive** library extends the capabilities of the HorizonteNet framework by providing an interactive C# execution environment, in-memory compilation of dynamic scripts, and their integration into the modular lifecycle of the system.

This document details the technical architecture of the extension, its data models, runtime dependency management, integration with .NET Interactive and Roslyn, and isolated loading mechanisms in modular domains.

---

### Architecture Overview

`Horizonte.Extensions.Interactive` serves as a bridge between the interactive engine (.NET Interactive), the Roslyn in-memory compiler (`Microsoft.CodeAnalysis.CSharp.Scripting`), and the HorizonteNet environment coordinator (`IEnvCoordinator`).

The system provides two main operational pathways:

1. **Interactive Kernel**: Construction and orchestration of configurable `CompositeKernel` / `CSharpKernel` instances in isolated or integrated mode, incorporating custom middleware to intercept and resolve reference directives (`#r "nuget:..."` and `#r "path.dll"`) against HorizonteNet assembly services.
2. **Dynamic Script Compilation and Loading**: Transformation of declarative script definitions (`ScriptDef`) into executable binary assemblies in memory using Roslyn, dynamically publishing them into a collectible application domain (`Script`), and automatically executing their initialization commands (`role: "init"`).

#### Integration Diagram and Components

```
+----------------------------------------------------------------------------------------------------+
|                                    CONSUMERS LAYER                                                 |
|                                                                                                    |
|    +-----------------------------+                  +----------------------------------------+     |
|    |      Panels & Widgets       |                  |         Services & Editors             |     |
|    |   (Horizonte.Scripts, UI)   |                  |          (Jupyter, Monaco)             |     |
|    +--------------+--------------+                  +-------------------+--------------------+     |
+-------------------|-----------------------------------------------------|--------------------------+
|                   |                                                     |
                    | (LoadScriptModules, Compile)                        | (BuildKernelAsync)
                    v                                                     v
+----------------------------------------------------------------------------------------------------+
|                                HORIZONTE.EXTENSIONS.INTERACTIVE                                    |
|                                                                                                    |
|  +---------------------------+  +-------------------------------+  +----------------------------+  |
|  |       Base Models         |  |       Script Compiler         |  |     Interactive Kernel     |  |
|  |   - ScriptDef             |  |   - Compile()                 |  |   - BuildKernelAsync()     |  |
|  |   - ScriptDefCompileResp. |  |   - Roslyn CSharpCompilation  |  |   - AddHorizonteMiddleware |  |
|  |   - KernelOptions         |  |   - Diagnostic Mapping        |  |   - CSharpKernel / Composite   |  |
|  +---------------------------+  +---------------+---------------+  +--------------+-------------+  |
+-------------------------------------------------|---------------------------------|----------------+
                                                  |                                 |
                                                  | (#r Resolution                  | (#r Real-time
                                                  |  & Domain Load)                 |  Resolution)
                                                  v                                 v
+----------------------------------------------------------------------------------------------------+
|                                   HORIZONTENET CORE                                                |
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

### Main Components and Data Models

#### 1. `KernelOptions` (Interactive Kernel Configuration)

Represents configuration options required to instantiate and customize an interactive kernel within the application.

- **Responsibilities**:
  - Define the logical identity of the kernel for logging and traceability purposes.
  - Determine the operational isolation level of the kernel relative to the system dependency injection container.

- **Structure and Parameters**:
  - `Name`: String identifying the kernel name. Defaults to an empty string.
  - `Isolated`: Boolean indicating whether the kernel operates in isolated (`true`) or integrated (`false`) mode. In integrated mode, `IServiceProvider` is automatically injected into kernel global variables, and base HorizonteNet and Dependency Injection NuGet packages are preloaded.

---

#### 2. `ScriptDef` (Declarative Script Definition)

Represents the data entity modeling an executable C# script, its metadata, and source code.

- **Responsibilities**:
  - Store script identity and description.
  - Sanitize the script name to ensure it is suitable for assembly names and internal references.
  - Manage transparent storage of source code in plain text or Base64-encoded format for safe persistence in serializable configurations.
  - Control the activation status of the script within the execution catalog.

- **Structure and Properties**:
  - `Id`: Unique script identifier in the system.
  - `Name`: Descriptive script name. When assigned, it is sanitized to preserve only alphanumeric characters (`char.IsLetterOrDigit`).
  - `Description`: Detailed description of the script's purpose and functionality.
  - `Code`: String storing Base64-encoded script content.
  - `CodeText`: Non-serialized property (`[JsonIgnore]`) providing bidirectional access to code in readable text. When read, decodes Base64 to UTF-8 (falling back to original text on error), and when written, encodes plain text to Base64.
  - `Active`: Boolean indicator determining whether the script should be compiled and loaded into the active execution domain.

---

#### 3. `ScriptDefCompileResponse` (Compilation Response)

Transport structure encapsulating the compilation result emitted by Roslyn for a `ScriptDef`.

- **Responsibilities**:
  - Transport binary assembly bytes when compilation succeeds.
  - Collect formatted error messages and critical warnings with line and column information on syntax or semantic failure.

- **Structure and Properties**:
  - `Assembly`: Byte array (`byte[]`) containing the in-memory compiled assembly DLL image.
  - `Errors`: Enumerable collection of strings (`IEnumerable<string>`) with diagnostic error details produced during compilation.

---

#### 4. `Mapping` (Assembly and Code Analysis Package Mapping)

Static class decorated with the HorizonteNet discovery attribute `[AssemblyMappingDefinition]`, responsible for mapping code analysis assembly names to their corresponding NuGet package IDs.

- **Responsibilities**:
  - Provide a case-insensitive dictionary (`AssemblyToPackageMap`) that maps assemblies such as `Microsoft.CodeAnalysis`, `Microsoft.CodeAnalysis.Scripting`, and `Microsoft.CodeAnalysis.Workspaces` to their respective NuGet package identifiers.
  - Enable the `IHAssemblySolver` subsystem to automatically resolve transitive Roslyn dependencies requested by scripts or interactive kernels.

---

### Functional Subsystems and Operations

```
+----------------------------------------------------------------------------------------------------+
|                               SUBSYSTEMS IN EXTENSIONS.CS                                          |
|                                                                                                    |
|  1. Interactive Engine          2. Roslyn Compilation             3. Script Lifecycle              |
|  +---------------------------+  +------------------------------+  +-----------------------------+  |
|  | - BuildKernelAsync        |  | - Compile                    |  | - LoadScriptModules         |  |
|  | - AddHorizonteMiddleware  |  | - ParseText (Script)         |  | - Batch compilation         |  |
|  | - Intercept #r nuget      |  | - Intercept #r nuget/path    |  | - UnloadDomain("Script")    |  |
|  | - Inject ServiceProvider  |  | - MetadataRef Injection      |  | - LoadDomain("Script", asms)|  |
|  | - Helper display()        |  | - Emit to MemoryStream       |  | - Trigger Init Commands     |  |
|  +---------------------------+  +------------------------------+  +-----------------------------+  |
+----------------------------------------------------------------------------------------------------+
```

#### 1. Interactive Kernel Construction and Middleware

Building the interactive environment is performed via the `BuildKernelAsync` extension method, which creates a `CompositeKernel` containing a configured `CSharpKernel`.

##### Kernel Initialization Flow
1. **Instantiation**: Creates an instance of `Microsoft.DotNet.Interactive.CSharp.CSharpKernel`.
2. **Integration Configuration**:
   - If `KernelOptions.Isolated` is `false`:
     - Binds custom Horizonte middleware (`AddHorizonteMiddleware`).
     - Registers the `IServiceProvider` instance in the kernel variable space via `SetValueAsync("ServiceProvider", serviceProvider, typeof(IServiceProvider))`.
     - Submits `SubmitCode` initialization commands to reference base HorizonteNet packages (`Horizonte` and `Microsoft.Extensions.DependencyInjection.Abstractions`).
3. **Helper Function Injection**: Defines the global `display(object x)` function routing to console output (`Console.WriteLine(x.ToString())`).
4. **Composition**: Assembles the `CompositeKernel`, setting the C# kernel as the default kernel.

##### Assembly Interception Middleware (`AddHorizonteMiddleware`)
The middleware intercepts `SubmitCode` commands before processing by the interactive engine:
- Analyzes source code line-by-line looking for reference directives:
  - **NuGet Pattern**: `#r "nuget:PackageName, Version"` — Extracts package name and optional version. Queries `IEnvCoordinator.AssemblySolver` (`ResolveAssemblyFromNuGetPackages` / `ResolveAssemblyDllPath`). If the assembly is available or located on disk, normalizes its absolute path and adds it to the kernel via `csharpKernel.AddAssemblyReferences(...)`.
  - **DLL Path Pattern**: `#r "path/to/assembly.dll"` — Extracts file path, resolves relative paths against the current working directory, and if the file exists on disk, registers it in kernel references.
- **Code Sanitization**: If `#r` directives were intercepted, they are removed from the `SubmitCode` command body to avoid incompatibilities during subsequent interactive compilation.

---

#### 2. In-Memory Script Compilation with Roslyn

The extension method `Compile(this ScriptDef, IEnvCoordinator)` manages dynamic compilation of script source code into an in-memory DLL binary.

##### Compilation Process Phases
1. **Preprocessing and Directive Extraction**:
   - Processes script text (`ScriptDef.CodeText`).
   - Intercepts `#r "nuget:..."` and `#r "path.dll"` directives.
   - Resolves dependencies against `IEnvCoordinator.AssemblySolver`, generating physical file-based `MetadataReference` objects.
   - Replaces lines containing `#r` directives with blank lines in clean code, preserving exact line numbering for subsequent error diagnostics.
2. **Syntax Tree Generation**:
   - Calls `CSharpSyntaxTree.ParseText` configured with `SourceCodeKind.Script`.
   - Defines output assembly name as `{ScriptDef.Name}.script`.
3. **Assembly Reference Resolution**:
   - Adds fundamental runtime libraries (`System.Runtime.dll`, `mscorlib.dll`, `System.Private.CoreLib.dll`, `System.Console.dll`, `System.Collections.dll`, `System.Linq.dll`, `System.IO.dll`, `System.Threading.dll`, `Microsoft.CSharp.dll`).
   - Ensures explicit inclusion of `System.Runtime` via reflection-based loading.
   - Includes the core HorizonteNet assembly (`typeof(IEnvCoordinator).Assembly`). In Single File execution modes where physical location is not on disk, resolves dynamically via `AssemblySolver.ResolveAssemblyDllPath`.
   - Appends all additional references resolved during preprocessing.
4. **Compilation Setup (`CSharpCompilation`)**:
   - Configured as dynamically linked library (`OutputKind.DynamicallyLinkedLibrary`).
   - Script host class configured as `"Script"`.
   - Metadata resolver: `ScriptMetadataResolver.Default`.
   - Default global imports (*usings*): `System`, `System.Collections.Generic`, `System.Linq`, `System.Text`, `System.Threading.Tasks`.
5. **Emission and Error Handling**:
   - Emits binary bytes into an in-memory stream (`MemoryStream`).
   - If emission fails, filters diagnostics with Error severity or warnings treated as errors, formatting each message as `Line {Line},{Column}\t: {Message}` in `ScriptDefCompileResponse.Errors`.
   - If emission succeeds, assigns the resulting byte array to `ScriptDefCompileResponse.Assembly`.

---

#### 3. Script Module Loading and Lifecycle

The extension method `LoadScriptModules(this IEnumerable<ScriptDef>, IServiceProvider)` automates the entire cycle of compilation, obsolete domain unloading, isolated loading, and initialization routine invocation.

##### Module Loading Flow
1. **Filtering and Batch Compilation**:
   - Selects all configured scripts where active state is true (`script.Active == true`).
   - For each script, invokes `script.Compile(envCoordinator)`.
   - If a script produces errors, logs them to the system log and skips the script.
   - Collects generated binaries into an in-memory collection (`List<byte[]>`).
2. **Isolated Domain Unload and Reload**:
   - Requests `IEnvCoordinator.UnloadDomainAsync("Script")` to unload and release the previous ALC context associated with scripts.
   - Invokes `IEnvCoordinator.LoadDomainAsync("Script", scriptasmlist)` to atomically load the assembly list into the dedicated collectible domain.
3. **Automatic Initialization Command Execution**:
   - Obtains the command manager `IHGesCom` from the service provider.
   - Queries all commands registered with the `"init"` role (`gesCom.GetRoleCommands("init")`).
   - For each command belonging to the `"Script"` domain, immediately executes it via `gesCom.RunCommand(...)`, allowing scripts to initialize modules, register widgets, or configure dynamic services.

---

### Sequence and Flow Diagrams

#### 1. Interactive Kernel Execution Flow

```
User / Consumer
        |
        | 1. BuildKernelAsync(options, serviceProvider)
        v
+------------------------------------+
|    CompositeKernel Construction    |
|  - CSharpKernel created            |
|  - ServiceProvider injected        |
|  - Horizonte Middleware bound      |
+-----------------+------------------+
                  |
                  | 2. SubmitCode: #r "nuget:Package, 1.0"
                  v
+------------------------------------+
|       AddHorizonteMiddleware       |
|  - Regex detects #r directive      |
+-----------------+------------------+
                  |
                  | 3. ResolveAssemblyFromNuGetPackages()
                  v
+------------------------------------+
|         IHAssemblySolver           |
|  - Locates DLL in cache / disk     |
+-----------------+------------------+
                  |
                  | 4. Absolute assembly path
                  v
+------------------------------------+
|  csharpKernel.AddAssemblyReference |
|  - Clean code forwarded to kernel  |
+-----------------+------------------+
                  |
                  | 5. Interactive C# code execution
                  v
                Result
```

#### 2. Script Module Compilation and Deployment Flow

```
ScriptDef[] Collection (Active)
        |
        | 1. script.Compile(envCoordinator)
        v
+------------------------------------+
|          Roslyn Compiler           |
|  - #r directives preprocessed      |
|  - MetadataReferences injected     |
|  - CSharpCompilation.Emit()        |
+-----------------+------------------+
                  |
                  +---------------------------+
                  | (Success)                 | (Failure)
                  v                           v
+------------------------------------+   +------------------------------------+
|    In-Memory Binary (byte[])       |   | Line and column errors             |
+-----------------+------------------+   | (Logged via Logger)                |
                  |                      +------------------------------------+
                  | 2. Collect byte array list
                  v
+------------------------------------+
|    IEnvCoordinator.UnloadDomain    |
|    - "Script" domain unloaded      |
+-----------------+------------------+
                  |
                  | 3. IEnvCoordinator.LoadDomain("Script", asms)
                  v
+------------------------------------+
|    IHDomainManager / ALC           |
|    - Loads in-memory assemblies    |
|    - [HModule] discovery           |
+-----------------+------------------+
                  |
                  | 4. Find commands with "init" role in "Script" domain
                  v
+------------------------------------+
|              IHGesCom              |
|  - gesCom.RunCommand(commandName)  |
|  - Widget/data initialization      |
+-----------------+------------------+
```

---

### Integration with the HorizonteNet Ecosystem

| HorizonteNet Component | Interaction from `Horizonte.Extensions.Interactive` |
|---|---|
| `IEnvCoordinator` | Main facade used to coordinate unloading and hot loading of the script domain (`LoadDomainAsync`, `UnloadDomainAsync`). |
| `IHAssemblySolver` | Resolves NuGet package names, downloads and locates physical `.dll` files from `#r` directives in scripts and notebooks. |
| `IHDomainManager` | Underlying management of the collectible `AssemblyLoadContext` identified as `"Script"`. |
| `IHGesCom` | Discovers and invokes commands decorated with `[HorizonteCommand]` and categorized with `[HorizonteRole("init")]` following domain loading. |
