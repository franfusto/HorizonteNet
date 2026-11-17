### Architecture of Horizonte.Extension.AspNetCore

The **Horizonte.Extension.AspNetCore** library provides the integration layer between the ASP.NET Core HTTP pipeline and the HorizonteNet modular web asset management subsystem. Its main objective is to efficiently resolve and serve static files and packaged web resources (`_content/...`) originating from modules and NuGet packages dynamically loaded in isolated domains.

This document describes the technical architecture of the middleware, its lifecycle within the HTTP request processing pipeline, contextual resolution by domain, and its interaction with the central static file registry (`StaticFileRegistry`).

---

### Architecture Overview

In modular applications based on HorizonteNet, each module or NuGet package can include its own static web assets (CSS stylesheets, JavaScript scripts, images, Blazor/Razor components, or static UI resources).

`Horizonte.Extension.AspNetCore` intercepts incoming requests in the HTTP pipeline before they reach default file providers, querying the central orchestrator `IEnvCoordinator` to resolve the physical location of the files associated with the domain of the requesting assembly.

#### Integration Diagram and Components

```
+----------------------------------------------------------------------------------------------------+
|                                    INCOMING HTTP REQUEST                                           |
|                               (GET / HEAD  /_content/Module/asset.js)                              |
+--------------------------------------------------+-------------------------------------------------+
                                                   |
                                                   v
+----------------------------------------------------------------------------------------------------+
|                                 ASP.NET CORE PIPELINE                                              |
|                                                                                                    |
|    +-----------------------------+                                                                 |
|    |      Routing / Auth         |                                                                 |
|    +--------------+--------------+                                                                 |
|                   v                                                                                |
|    +------------------------------------------------------------------------------------------+    |
|    |                        HorizonteStaticFileMiddelware                                     |    |
|    |                                                                                          |    |
|    |   1. Validation: No Endpoint Delegate assigned                                          |    |
|    |   2. Validation: Allowed HTTP method (GET / HEAD)                                        |    |
|    |   3. Validation: Request path prefix match (RequestPath)                                 |    |
|    |   4. Resolution: MIME type (IContentTypeProvider / FileExtensionContentTypeProvider)    |    |
|    |   5. Query: StaticFileRegistry.GetFile(subPath, domainName)                              |    |
|    |   6. Emission: IHttpResponseBodyFeature.SendFileAsync()                                   |    |
|    +------------------------------+-----------------------------------------------------------+    |
|                                   | (If no match or file not found)                                |
|                                   v                                                                |
|    +------------------------------------------------------------------------------------------+    |
|    |      Next Middleware (standard UseStaticFiles / Endpoints / Fallback)                    |    |
|    +------------------------------------------------------------------------------------------+    |
+--------------------------------------------------|-------------------------------------------------+
                                                   |
                                                   | File and domain query
                                                   v
+----------------------------------------------------------------------------------------------------+
|                                   HORIZONTENET CORE                                                |
|                                                                                                    |
|  +----------------------------------------------------------------------------------------------+  |
|  |                                       IEnvCoordinator                                        |  |
|  |                                                                                              |  |
|  |   - AssembliesByDomain (Mapping of assemblies and active ALC domains)                        |  |
|  |   - StaticFileRegistry (Concurrent catalog of static paths and wwwroot)                      |  |
|  +----------------------------------------------------------------------------------------------+  |
+----------------------------------------------------------------------------------------------------+
```

---

### Components and Responsibilities

#### 1. `HorizonteStaticFileMiddelware` (Static Files Middleware)

Specialized middleware for serving static files originating from modules and dynamic libraries managed by HorizonteNet.

- **Responsibilities**:
  - Intercept requests directed to static file paths registered in the application.
  - Associate middleware execution with the domain of the registering assembly (`registeringAssembly`), allowing contextual resolution of resources by domain or falling back to the default domain.
  - Validate HTTP processing conditions (method, routing, MIME type, and path).
  - Retrieve the corresponding physical file through `IEnvCoordinator.StaticFileRegistry`.
  - Stream the file directly to the HTTP response using kernel/server optimizations via `IHttpResponseBodyFeature.SendFileAsync`.
  - Delegate control to the next middleware in the pipeline (`_next`) when the request does not correspond to a registered static file.

- **Dependencies and Service Injection**:
  - `RequestDelegate`: Delegate to the next link in the ASP.NET Core HTTP pipeline.
  - `IWebHostEnvironment`: Web hosting environment used to resolve the fallback root file provider (`WebRootFileProvider`).
  - `IOptions<StaticFileOptions>`: Static file configuration options (path prefix `RequestPath`, content type provider, unknown type handling, and file provider).
  - `ILogger<HorizonteStaticFileMiddelware>`: Logger for diagnostic events and delivery warnings.
  - `IEnvCoordinator`: HorizonteNet environment coordinator providing access to the static file registry and domain map.
  - `Assembly? registeringAssembly`: Optional assembly that triggers the middleware registration, used to uniquely identify the originating logical domain / ALC.

---

### Validation Pipeline and Processing Flow

The `Invoke` method executes a strict step-by-step validation pipeline before attempting to serve a file:

```
+-----------------------------------------+
|      Incoming HttpContext Request       |
+--------------------+--------------------+
                     |
                     v
+-----------------------------------------+       [NO]
|  No endpoint delegate exists?           | -------------> Delegate to _next(context)
|  (ValidateNoEndpointDelegate)           |
+--------------------+--------------------+
                     | [YES]
                     v
+-----------------------------------------+       [NO]
|  HTTP method is GET or HEAD?            | -------------> Delegate to _next(context)
|  (ValidateMethod)                       |
+--------------------+--------------------+
                     | [YES]
                     v
+-----------------------------------------+       [NO]
|  Path matches prefix matchUrl?          | -------------> Delegate to _next(context)
|  (ValidatePath -> out subPath)          |
+--------------------+--------------------+
                     | [YES]
                     v
+-----------------------------------------+       [NO]
|  MIME type supported or allowed?        | -------------> Delegate to _next(context)
|  (LookupContentType -> out contentType) |
+--------------------+--------------------+
                     | [YES]
                     v
+-----------------------------------------+
|           TryServeStaticFile            |
|  - Query StaticFileRegistry             |
+--------------------+--------------------+
                     |
         +-----------+-----------+
         | (Exists)              | (Does not exist)
         v                       v
+-----------------------+ +-----------------------+
| Set Headers           | | Log Warning           |
| - ContentType         | | Delegate to _next()   |
| - ContentLength       | +-----------------------+
| SendFileAsync()       |
| Delegate to _next()   |
+-----------------------+
```

#### Detailed Validation Phases

1. **Endpoint Delegate Validation (`ValidateNoEndpointDelegate`)**:
   Verifies that the current request has not already been mapped to an endpoint with an execution delegate (`context.GetEndpoint()?.RequestDelegate is null`), ensuring that controller routes, APIs, or Razor pages take precedence and are not improperly intercepted.

2. **HTTP Method Validation (`ValidateMethod`)**:
   Restricts static content delivery strictly to safe verbs: `GET` and `HEAD`. Any other HTTP method is ignored and passed to the next middleware.

3. **Path Segment Validation (`ValidatePath`)**:
   Checks whether the request path (`context.Request.Path`) starts with the segment configured in `StaticFileOptions.RequestPath`. If matching, extracts the relative sub-path (`subPath`) for asset lookup.

4. **Content Type Lookup (`LookupContentType`)**:
   Uses the configured `IContentTypeProvider` (or `FileExtensionContentTypeProvider` by default) to resolve the MIME type from the file extension. If the extension is not recognized, evaluates whether `StaticFileOptions.ServeUnknownFileTypes` is enabled to apply `DefaultContentType`; otherwise, delivery is rejected.

5. **File Location and Delivery (`TryServeStaticFile`)**:
   - Queries `IEnvCoordinator.StaticFileRegistry.GetFile(subPath, _domainName)`.
   - If the physical file exists, sets the `Content-Type` and `Content-Length` response headers and transmits the content via `IHttpResponseBodyFeature.SendFileAsync`.
   - If the file does not exist in the registry, logs a warning and yields control to the next middleware in the pipeline.

---

### Contextual Resolution by Domain

HorizonteNet isolates modules and dependencies across different loading contexts (`AssemblyLoadContext`). To prevent collisions between static assets from different domains or versions, the middleware implements contextual resolution:

1. **Assembly Identification**: When registering the middleware via `app.UseMiddleware<HorizonteStaticFileMiddelware>(typeof(Worker).Assembly)`, the host component assembly is provided.
2. **Lookup in `AssembliesByDomain`**: The internal method `ResolveRegisteringAssemblyDomain` compares the assembly by object reference or exact `FullName` match against the assemblies registered in each domain within `IEnvCoordinator.AssembliesByDomain`.
3. **Lookup in `StaticFileRegistry`**: When requesting a file, the composite domain key (`{domain}::{normalized_path}`) guarantees that the exact asset version belonging to the corresponding module is served, maintaining fallback compatibility with global lookup if no domain is specified.

---

### Integration with the HorizonteNet Ecosystem

| Component | Role in Integration |
|---|---|
| `IEnvCoordinator` | Provides centralized access to the static asset registry and assembly mapping by domain. |
| `StaticFileRegistry` | In-memory catalog indexing relative paths and `wwwroot` / `staticwebassets` / `Content` folders of modules and NuGet packages. |
| `IHttpResponseBodyFeature` | Low-level ASP.NET Core interface used to asynchronously and efficiently stream files without intermediate buffer overhead. |
| `IWebHostEnvironment` | Supplies the web host root file provider in case fallback standard resolution is required. |
