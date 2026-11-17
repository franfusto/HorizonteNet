### Arquitectura de Horizonte.Extension.AspNetCore

La biblioteca **Horizonte.Extension.AspNetCore** proporciona la capa de integración entre el pipeline HTTP de ASP.NET Core y el subsistema modular de gestión de activos web de HorizonteNet. Su objetivo principal es resolver y servir eficientemente archivos estáticos y recursos web empaquetados (`_content/...`) provenientes de módulos y paquetes NuGet cargados dinámicamente en dominios aislados.

Este documento describe la arquitectura técnica del middleware, su ciclo de vida en el pipeline de procesamiento de solicitudes HTTP, la resolución contextual por dominio y su interacción con el registro central de archivos estáticos (`StaticFileRegistry`).

---

### Visión General de la Arquitectura

En aplicaciones modulares basadas en HorizonteNet, cada módulo o paquete NuGet puede incorporar sus propios activos web estáticos (hojas de estilo CSS, scripts JavaScript, imágenes, componentes Blazor/Razor o recursos estáticos de interfaz).

`Horizonte.Extension.AspNetCore` intercepta las solicitudes entrantes en el pipeline HTTP antes de que alcancen los proveedores de archivos por defecto, consultando al orquestador central `IEnvCoordinator` para resolver la ubicación física de los archivos asociados al dominio del ensamblado solicitante.

#### Esquema de Integración y Componentes

```
+----------------------------------------------------------------------------------------------------+
|                                    SOLICITUD HTTP ENTRANTE                                         |
|                               (GET / HEAD  /_content/Modulo/asset.js)                              |
+--------------------------------------------------+-------------------------------------------------+
                                                   |
                                                   v
+----------------------------------------------------------------------------------------------------+
|                                 PIPELINE DE ASP.NET CORE                                           |
|                                                                                                    |
|    +-----------------------------+                                                                 |
|    |      Routing / Auth         |                                                                 |
|    +--------------+--------------+                                                                 |
|                   v                                                                                |
|    +------------------------------------------------------------------------------------------+    |
|    |                        HorizonteStaticFileMiddelware                                     |    |
|    |                                                                                          |    |
|    |   1. Validación: Sin Endpoint Delegate asignado                                          |    |
|    |   2. Validación: Método HTTP permitido (GET / HEAD)                                      |    |
|    |   3. Validación: Coincidencia de prefijo de ruta (RequestPath)                           |    |
|    |   4. Resolución: Tipo MIME (IContentTypeProvider / FileExtensionContentTypeProvider)    |    |
|    |   5. Consulta: StaticFileRegistry.GetFile(subPath, domainName)                           |    |
|    |   6. Emisión: IHttpResponseBodyFeature.SendFileAsync()                                   |    |
|    +------------------------------+-----------------------------------------------------------+    |
|                                   | (Si no coincide o no se encuentra el archivo)                  |
|                                   v                                                                |
|    +------------------------------------------------------------------------------------------+    |
|    |      Next Middleware (UseStaticFiles estándar / Endpoints / Fallback)                    |    |
|    +------------------------------------------------------------------------------------------+    |
+--------------------------------------------------|-------------------------------------------------+
                                                   |
                                                   | Consulta de archivo y dominio
                                                   v
+----------------------------------------------------------------------------------------------------+
|                                   NÚCLEO DE HORIZONTENET                                           |
|                                                                                                    |
|  +----------------------------------------------------------------------------------------------+  |
|  |                                       IEnvCoordinator                                        |  |
|  |                                                                                              |  |
|  |   - AssembliesByDomain (Mapeo de ensamblados y dominios ALC activos)                        |  |
|  |   - StaticFileRegistry (Catálogo concurrente de rutas estáticas y wwwroot)                   |  |
|  +----------------------------------------------------------------------------------------------+  |
+----------------------------------------------------------------------------------------------------+
```

---

### Componentes y Responsabilidades

#### 1. `HorizonteStaticFileMiddelware` (Middleware de Archivos Estáticos)

Middleware especializado en la entrega de archivos estáticos procedentes de módulos y librerías dinámicas gestionadas por HorizonteNet.

- **Responsabilidades**:
  - Interceptar solicitudes dirigidas a rutas de archivos estáticos registradas en la aplicación.
  - Asociar la ejecución del middleware al dominio del ensamblado registrador (`registeringAssembly`), permitiendo la resolución contextual de recursos por dominio o recurriendo al dominio por defecto.
  - Validar las condiciones de procesamiento HTTP (método, enrutamiento, tipo MIME y ruta).
  - Recuperar el archivo físico correspondiente mediante `IEnvCoordinator.StaticFileRegistry`.
  - Transmitir el archivo directamente a la respuesta HTTP utilizando optimizaciones de kernel/servidor mediante `IHttpResponseBodyFeature.SendFileAsync`.
  - Delegar el control al siguiente middleware en la cadena (`_next`) cuando la solicitud no corresponde a un archivo estático registrado.

- **Dependencias e Inyección de Servicios**:
  - `RequestDelegate`: Delegado hacia el siguiente eslabón del pipeline HTTP de ASP.NET Core.
  - `IWebHostEnvironment`: Entorno de alojamiento web utilizado para resolver el proveedor de archivos raíz de respaldo (`WebRootFileProvider`).
  - `IOptions<StaticFileOptions>`: Opciones de configuración de archivos estáticos (prefijo de ruta `RequestPath`, proveedor de tipo de contenido, manejo de tipos desconocidos y proveedor de archivos).
  - `ILogger<HorizonteStaticFileMiddelware>`: Registrador de eventos de diagnóstico y advertencias de entrega.
  - `IEnvCoordinator`: Coordinador del entorno de HorizonteNet que proporciona acceso al registro de archivos estáticos y al mapa de dominios.
  - `Assembly? registeringAssembly`: Ensamblado opcional que invoca el registro del middleware, utilizado para identificar unívocamente el dominio lógico/ALC de procedencia.

---

### Pipeline de Validación y Flujo de Procesamiento

El método de invocación `Invoke` aplica un pipeline estricto de validación paso a paso antes de intentar servir un archivo:

```
+-----------------------------------------+
|      Inicio de Solicitud HttpContext    |
+--------------------+--------------------+
                     |
                     v
+-----------------------------------------+       [NO]
|  ¿No existe delegado de endpoint?       | -------------> Delegar a _next(context)
|  (ValidateNoEndpointDelegate)           |
+--------------------+--------------------+
                     | [SÍ]
                     v
+-----------------------------------------+       [NO]
|  ¿Método HTTP es GET o HEAD?            | -------------> Delegar a _next(context)
|  (ValidateMethod)                       |
+--------------------+--------------------+
                     | [SÍ]
                     v
+-----------------------------------------+       [NO]
|  ¿Ruta coincide con prefijo matchUrl?   | -------------> Delegar a _next(context)
|  (ValidatePath -> out subPath)          |
+--------------------+--------------------+
                     | [SÍ]
                     v
+-----------------------------------------+       [NO]
|  ¿Tipo MIME soportado o permitido?      | -------------> Delegar a _next(context)
|  (LookupContentType -> out contentType) |
+--------------------+--------------------+
                     | [SÍ]
                     v
+-----------------------------------------+
|           TryServeStaticFile            |
|  - Consultar StaticFileRegistry         |
+--------------------+--------------------+
                     |
         +-----------+-----------+
         | (Existe)              | (No existe)
         v                       v
+-----------------------+ +-----------------------+
| Establecer Headers    | | Registrar Warning     |
| - ContentType         | | Delegar a _next()     |
| - ContentLength       | +-----------------------+
| SendFileAsync()       |
| Delegar a _next()     |
+-----------------------+
```

#### Fases de Validación Detalladas

1. **Validación de Delegado de Endpoint (`ValidateNoEndpointDelegate`)**:
   Verifica que la solicitud actual no haya sido asignada previamente a un endpoint con delegado de ejecución (`context.GetEndpoint()?.RequestDelegate is null`), asegurando que las rutas de controladores, APIs o páginas Razor tengan precedencia y no sean interceptadas indebidamente.

2. **Validación de Método HTTP (`ValidateMethod`)**:
   Restringe la entrega de contenido estático exclusivamente a los verbos seguros `GET` y `HEAD`. Cualquier otro método HTTP es descartado y remitido al siguiente middleware.

3. **Validación de Segmento de Ruta (`ValidatePath`)**:
   Comprueba si la ruta de la solicitud (`context.Request.Path`) comienza con el segmento configurado en `StaticFileOptions.RequestPath`. En caso afirmativo, extrae la subruta relativa (`subPath`) para la búsqueda del activo.

4. **Determinación del Tipo de Contenido (`LookupContentType`)**:
   Utiliza el proveedor `IContentTypeProvider` configurado (o `FileExtensionContentTypeProvider` por defecto) para resolver el tipo MIME a partir de la extensión del archivo. Si no se reconoce la extensión, se evalúa si `StaticFileOptions.ServeUnknownFileTypes` está habilitado para aplicar `DefaultContentType`; de lo contrario, se rechaza la entrega.

5. **Localización y Emisión del Archivo (`TryServeStaticFile`)**:
   - Consulta `IEnvCoordinator.StaticFileRegistry.GetFile(subPath, _domainName)`.
   - Si el archivo físico existe, se configuran las cabeceras `Content-Type` y `Content-Length` en la respuesta y se transmite el contenido mediante `IHttpResponseBodyFeature.SendFileAsync`.
   - Si el archivo no existe en el registro, se registra una advertencia en el log y se cede el control al siguiente middleware en el pipeline.

---

### Resolución Contextual por Dominio

HorizonteNet permite aislar módulos y dependencias en diferentes contextos de carga (`AssemblyLoadContext`). Para evitar colisiones entre activos estáticos de distintos dominios o versiones, el middleware implementa un mecanismo de resolución contextual:

1. **Identificación del Ensamblado**: Al registrar el middleware mediante `app.UseMiddleware<HorizonteStaticFileMiddelware>(typeof(Worker).Assembly)`, se suministra el ensamblado del componente que actúa como anfitrión.
2. **Búsqueda en `AssembliesByDomain`**: El método interno `ResolveRegisteringAssemblyDomain` compara el ensamblado por referencia de objeto o por coincidencia exacta de `FullName` contra los ensamblados registrados en cada dominio de `IEnvCoordinator.AssembliesByDomain`.
3. **Consulta en `StaticFileRegistry`**: Al solicitar un archivo, la clave compuesta de dominio (`{dominio}::{ruta_normalizada}`) garantiza que se sirva la versión exacta del activo perteneciente al módulo correspondiente, manteniendo compatibilidad con la búsqueda global si no se especifica dominio.

---

### Integración con el Ecosistema HorizonteNet

| Componente | Rol en la Integración |
|---|---|
| `IEnvCoordinator` | Provee acceso centralizado al registro de activos estáticos y a la asignación de ensamblados por dominio. |
| `StaticFileRegistry` | Catálogo en memoria que indexa rutas relativas y carpetas `wwwroot` / `staticwebassets` / `Content` de módulos y paquetes NuGet. |
| `IHttpResponseBodyFeature` | Interfaz de bajo nivel de ASP.NET Core utilizada para transmitir archivos de manera asíncrona y eficiente sin sobrecargar buffers intermedios. |
| `IWebHostEnvironment` | Suministra el proveedor de archivos raíz del host web en caso de requerir resolución estándar. |
