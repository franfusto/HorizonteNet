*cambios por incluir:
hemos añadidido en ConfigSet las definiciones de las toos y en AgentDef hemos cambiado Tools por string[],
ahora aqui se guarda la lista de AgentToolDef.Id


# Arquitectura de Hagent

Hagent es un runtime declarativo para crear agentes configurables, ejecutar consultas, procesar archivos, invocar herramientas y delegar la generación conversacional en clientes de IA intercambiables.

La idea central del diseño es:

> La configuración declara qué capacidades existen; la factoría las resuelve; `Hagent` las orquesta en tiempo de ejecución.

---

## 1. Arquitectura estable

Esta sección describe las decisiones arquitectónicas que deben considerarse estables. La implementación concreta puede evolucionar, pero estas reglas deben mantenerse para conservar una arquitectura limpia, testeable y extensible.

---

### 1.1. Principio de composición

El flujo principal del sistema es:

```text
Configuration + AgentDef
    ↓
agentDef.GetHagent(configuration)
    ↓
HagentFactory.Create(agentDef, configuration)
    ↓
Validación + resolución + composición
    ↓
Hagent
    ↓
Query() / QueryAsync()
    ↓
HagentExecutionContext
    ↓
Archivos + tools automáticas + chat
    ↓
ChatResponse
```

Regla principal:

```text
Hagent no debe saber cómo se resuelven servidores, clientes, RAGs o providers.
Hagent recibe dependencias ya resueltas por HagentFactory.
```

La responsabilidad se reparte así:

```text
Configuration contiene servidores, clientes, RAGs y agentes.
AgentDef describe un agente.
ServerDef describe dónde se sirve una capacidad.
ClientDef describe qué modelo o capacidad IA se usa.
RaGdef describe cómo recuperar contexto documental.
AgentToolDef describe una herramienta opcional del agente.

HagentFactory valida y compone el runtime.
HagentFactoryContext mantiene el estado temporal de composición.
HagentFactoryException representa errores de configuración/composición.

Hagent ejecuta consultas.
HagentExecutionContext mantiene el estado de una consulta concreta.
HagentToolContext entrega contexto controlado a las tools.

IHagentChatClient abstrae el cliente conversacional.
IHagentTool abstrae capacidades opcionales.
IHagentImageToTextClient abstrae análisis de imágenes.
IHagentTextToImageClient abstrae generación de imágenes.

HagentFileCodec normaliza archivos.
ChatResponse transporta la respuesta final.
```

---

### 1.2. Modelo de configuración

La configuración global se organiza así:

```text
Configuration
 ├── ServerDefs
 ├── ClientDefs
 ├── RaGdefs
 └── AgentDefs
```

| Bloque | Responsabilidad |
|---|---|
| `ServerDef` | Define proveedores, endpoints o servidores. |
| `ClientDef` | Define una capacidad IA concreta servida por un servidor. |
| `RaGdef` | Define una configuración RAG/vector store. |
| `AgentDef` | Define el agente, prompt, cliente chat principal, tools y opciones. |

Regla:

```text
La configuración es declarativa.
La resolución real de referencias pertenece a HagentFactory.
```

---

### 1.3. ServerDef

`ServerDef` representa un proveedor, servidor o endpoint.

Ejemplos conceptuales:

```text
OpenAI
Azure OpenAI
Ollama
LM Studio
vLLM
LocalAI
Servidor local
Proveedor compatible OpenAI
Proveedor custom
```

Responsabilidad:

```text
ServerDef = dónde se ejecuta o consume una capacidad.
```

Reglas:

```text
ClientDef.Server debe apuntar a ServerDef.Id.
ServerDef.Provider permite inferir el tipo de integración.
ServerDef.Settings permite declarar detalles de implementación sin cambiar el modelo.
```

Ejemplo conceptual:

```text
ServerDef
 ├── Id = "lmstudio-local"
 ├── Provider = "LM Studio"
 ├── Url = "http://localhost:1234/v1"
 └── Settings
     ├── implementation = "openai-compatible"
     └── compatibility = "openai"
```

---

### 1.4. ClientDef

`ClientDef` representa una capacidad IA concreta.

Ejemplos:

```text
Chat
Embedder
ImageToText
TextToImage
OCR
Audio
Reranker
Custom
```

Responsabilidad:

```text
ClientDef = qué modelo o capacidad se quiere usar.
```

Reglas:

```text
ClientDef.Server debe existir en Configuration.ServerDefs.
ClientDef.ClientType define el tipo de capacidad.
ClientDef.Settings puede especificar la implementación concreta.
```

Ejemplos:

```text
Chat principal      → ClientType.Chat
Embeddings RAG      → ClientType.Embedder
Análisis de imagen  → ClientType.ImageToText
Generación imagen   → ClientType.TextToImage
```

---

### 1.5. RaGdef

`RaGdef` representa una configuración RAG/vector store.

Responsabilidad:

```text
RaGdef = cómo recuperar contexto documental.
```

Reglas:

```text
RaGdef.EmbedderClient debe apuntar a un ClientDef existente de tipo ClientType.Embedder.
La tool RAG decide cómo usar esa definición en runtime.
```

Ejemplo conceptual:

```text
RaGdef
 ├── Id = "rag-memory-docs"
 ├── RagType = InMemory
 ├── EmbedderClient = "openai-embedder-main"
 ├── TopK = 5
 ├── MinScore = 0.20
 ├── CollectionName = "documents"
 └── Settings
     └── documents = "[...]"
```

En la implementación inicial, `RagSearchTool` puede usar búsqueda léxica simple aunque `EmbedderClient` ya esté validado. Esto permite mantener el contrato preparado para RAG vectorial futuro sin bloquear el flujo funcional actual.

---

### 1.6. AgentDef

`AgentDef` es la definición principal del agente.

Decisión importante:

```text
AgentDef solo mantiene el cliente conversacional principal mediante ChatClientId.
El resto de capacidades se declaran como tools.
```

Estructura conceptual:

```text
AgentDef
 ├── Id
 ├── Name
 ├── Description
 ├── SystemPrompt
 ├── ChatClientId
 ├── Tools
 ├── FileOptions
 ├── ExecutionOptions
 └── Settings
```

Regla:

```text
AgentDef.ChatClientId debe apuntar a un ClientDef.Id de tipo ClientType.Chat.
```

Diseño correcto:

```text
AgentDef
 ├── ChatClientId
 └── Tools
     ├── RagSearch
     ├── ImageToText
     ├── TextToImage
     ├── FileReader
     ├── FileWriter
     └── Custom
```

Diseño que se evita:

```text
AgentDef
 ├── ChatClientId
 ├── RagId
 ├── EmbedderClientId
 ├── TextToImageClientId
 ├── ImageToTextClientId
 ├── OcrClientId
 └── ...
```

Regla de extensibilidad:

```text
Las nuevas capacidades deben entrar como tools o clientes especializados, no como nuevas propiedades directas de AgentDef.
```

---

### 1.7. AgentToolDef

`AgentToolDef` declara una capacidad opcional del agente.

Responsabilidad:

```text
AgentToolDef = qué herramienta puede usar el agente y cómo se resuelve.
```

Uso esperado:

```text
RagSearch   → usa RagId
ImageToText → usa ClientId
TextToImage → usa ClientId
FileReader  → usa FileOptions y Settings
FileWriter  → usa FileOptions y Settings
Custom      → usa Settings
```

Campos conceptuales:

```text
AgentToolDef
 ├── Id
 ├── Name
 ├── ToolType
 ├── Enabled
 ├── ClientId
 ├── RagId
 ├── TimeoutSeconds
 └── Settings
```

Reglas:

```text
Una tool habilitada debe ser validada por HagentFactory.
Una tool puede autoejecutarse si su implementación lo permite.
Settings["autoUse"] = "true" o "always" puede activar autoejecución.
```

---

### 1.8. AgentFileOptions

`AgentFileOptions` define la política de entrada/salida de archivos.

Responsabilidad:

```text
AgentFileOptions = reglas sobre archivos de entrada y salida.
```

Opciones relevantes:

```text
AllowInputFiles
AllowOutputFiles
TransportEncoding
MaxInputFileSizeBytes
MaxOutputFileSizeBytes
AllowedInputMimeTypes
AllowedOutputMimeTypes
AutoProcessImages
AutoProcessDocuments
Settings
```

Reglas estrictas:

```text
MaxInputFileSizeBytes >= 0
MaxOutputFileSizeBytes >= 0
AllowedInputMimeTypes no puede ser null
AllowedOutputMimeTypes no puede ser null
Settings no puede ser null
TransportEncoding no puede estar vacío
AllowedInputMimeTypes no debe contener valores vacíos
AllowedOutputMimeTypes no debe contener valores vacíos
```

Reglas funcionales opcionales:

```text
AutoProcessImages = true
    → debería existir ImageToText habilitada.

AutoProcessDocuments = true
    → debería existir FileReader o tool documental compatible.

AllowOutputFiles = true
    → debería existir FileWriter o estrategia de salida compatible.
```

Para no bloquear integraciones iniciales, estas validaciones funcionales pueden activarse mediante:

```text
AgentDef.Settings["validateFileOptionDependencies"] = "true"
```

---

### 1.9. AgentExecutionOptions

`AgentExecutionOptions` controla límites y comportamiento del runtime.

Opciones relevantes:

```text
MaxTurns
MaxToolCalls
TimeoutSeconds
AutoInvokeTools
ReturnToolMessages
ReturnIntermediateSteps
Settings
```

Validaciones mínimas:

```text
MaxTurns >= 1
TimeoutSeconds > 0
MaxToolCalls >= 0
```

Reglas runtime:

```text
Si AutoInvokeTools = false:
    ExecuteAutoToolsAsync no ejecuta ninguna tool automática.

Si MaxToolCalls = 0:
    ExecuteAutoToolsAsync tampoco ejecuta tools automáticas.

ReturnToolMessages = true:
    los mensajes/trazas de tools se añaden a ChatResponse.ResponseItems.
```

---

### 1.10. HagentFactory

`HagentFactory` es el punto único de composición.

Responsabilidad:

```text
Validar configuración.
Resolver referencias.
Crear cliente chat.
Crear clientes auxiliares de tools.
Crear tools.
Crear Hagent.
```

Flujo estable:

```text
1. Validar AgentDef no null.
2. Validar Configuration no null.
3. Resolver AgentDef.ChatClientId.
4. Validar que el ClientDef sea ClientType.Chat.
5. Resolver ServerDef asociado al ClientDef.
6. Resolver AgentFileOptions.
7. Resolver AgentExecutionOptions.
8. Crear HagentFactoryContext.
9. Validar AgentFileOptions.
10. Validar AgentExecutionOptions.
11. Crear IHagentChatClient.
12. Resolver tools habilitadas.
13. Crear Hagent.
```

Regla:

```text
Si HagentFactory.Create devuelve un Hagent, su configuración estructural ya debe ser válida.
```

`HagentFactory` puede seleccionar implementaciones reales o stub, pero `Hagent` no debe conocer esa decisión.

---

### 1.11. HagentFactoryContext

`HagentFactoryContext` guarda el estado temporal durante la creación del agente.

Contiene:

```text
AgentDef
Configuration
ChatClientDef
ChatServerDef
IHagentChatClient?
List<IHagentTool>
AgentFileOptions
AgentExecutionOptions
```

Objetivo:

```text
Evitar variables dispersas en HagentFactory.
Centralizar dependencias resueltas durante la composición.
```

---

### 1.12. HagentFactoryException

`HagentFactoryException` representa errores de configuración o composición.

Objetivo:

```text
Diferenciar errores de creación del agente de errores de ejecución.
```

Se usa para:

```text
AgentDef null.
Configuration null.
ChatClientId vacío.
Cliente inexistente.
Cliente no Chat.
Servidor inexistente.
Tool mal configurada.
Opciones inválidas.
RAG inexistente.
RAG no soportado por la tool real.
Cliente de tool inexistente.
Cliente de tool con ClientType incorrecto.
Implementación de chat no soportada.
Proveedor no resoluble.
```

---

### 1.13. IHagentChatClient

`IHagentChatClient` abstrae el cliente conversacional.

Contrato:

```text
CompleteAsync(HagentChatRequest request, CancellationToken cancellationToken)
    ↓
HagentChatResult
```

`HagentChatRequest` permite enviar:

```text
Messages
Tools
Options
```

`HagentChatResult` permite recibir:

```text
Text
ToolCalls
Metadata
```

Uso:

```text
Hagent construye HagentChatRequest.
IHagentChatClient devuelve HagentChatResult.
Hagent convierte result.Text en ChatResponseItem.Response.
```

Regla:

```text
Hagent solo conoce IHagentChatClient.
No conoce OpenAI, Ollama, LM Studio, vLLM ni Microsoft.Extensions.AI.
```

---

### 1.14. Clientes auxiliares de IA

Además del chat principal, algunas tools usan clientes específicos.

#### IHagentImageToTextClient

Responsabilidad:

```text
Abstraer un cliente capaz de analizar imágenes y devolver texto.
```

Contrato conceptual:

```text
DescribeAsync(HagentImageToTextRequest request, CancellationToken cancellationToken)
    ↓
HagentImageToTextResult
```

Uso:

```text
ImageToTextTool
    ↓
IHagentImageToTextClient.DescribeAsync
    ↓
context.Items["image.descriptions"]
```

#### IHagentTextToImageClient

Responsabilidad:

```text
Abstraer un cliente capaz de generar imágenes desde texto.
```

Contrato conceptual:

```text
GenerateAsync(HagentTextToImageRequest request, CancellationToken cancellationToken)
    ↓
HagentTextToImageResult
```

Uso:

```text
TextToImageTool
    ↓
IHagentTextToImageClient.GenerateAsync
    ↓
HagentToolResult.Files
    ↓
context.OutputFiles
```

Regla:

```text
Las tools multimodales no deben conocer proveedores concretos.
Deben depender de interfaces.
```

---

### 1.15. IHagentTool

`IHagentTool` abstrae una capacidad opcional del agente.

Contrato:

```text
Id
Name
ToolType
Enabled
CanHandle(...)
ShouldAutoInvokeAsync(...)
ExecuteAsync(...)
```

Uso:

```text
ExecuteAutoToolsAsync recorre tools habilitadas.
Pregunta ShouldAutoInvokeAsync.
Ejecuta ExecuteAsync si procede.
Aplica HagentToolResult al contexto.
```

Regla:

```text
Las capacidades del agente viven en tools intercambiables.
Hagent no debe contener lógica específica de cada capacidad.
```

---

### 1.16. Hagent

`Hagent` es el runtime/orquestador.

Responsabilidad:

```text
Recibir consultas.
Crear contexto por consulta.
Procesar archivos.
Ejecutar tools automáticas.
Construir contexto textual.
Llamar al chat.
Construir ChatResponse.
```

No debe validar referencias globales como:

```text
ChatClientId existe.
ClientDef es Chat.
ServerDef existe.
RAG existe.
Embedder existe.
Tool apunta a cliente válido.
```

Eso pertenece a `HagentFactory`.

Sí puede validar argumentos internos:

```text
agentDef no null.
chatClient no null.
tools no null.
fileOptions no null.
executionOptions no null.
query no null.
query.Query no vacío.
```

Constructor conceptual:

```csharp
internal Hagent(
    AgentDef agentDef,
    IHagentChatClient chatClient,
    IReadOnlyList<IHagentTool> tools,
    AgentFileOptions fileOptions,
    AgentExecutionOptions executionOptions)
```

Regla de concurrencia:

```text
Hagent debe poder reutilizarse entre consultas.
Cada QueryAsync crea su propio HagentExecutionContext.
```

---

### 1.17. Query y QueryAsync

La API principal es:

```csharp
Task<ChatResponse> QueryAsync(ChatQuery query, CancellationToken cancellationToken = default)
```

`Query` existe como wrapper síncrono:

```text
Query(query)
    ↓
QueryAsync(query).GetAwaiter().GetResult()
```

Regla:

```text
La API principal recomendada es QueryAsync.
```

Flujo estable:

```text
1. Validar query.
2. Crear CancellationTokenSource enlazado con timeout.
3. Crear HagentExecutionContext.
4. Procesar archivos de entrada.
5. Ejecutar tools automáticas.
6. Ejecutar flujo chat/tools.
7. Construir ChatResponse.
```

---

### 1.18. HagentExecutionContext

`HagentExecutionContext` representa el estado de una consulta concreta.

Campos conceptuales:

```text
OriginalQuery
AgentDef
FileOptions
ExecutionOptions
CancellationToken
InputFiles
OutputFiles
ResponseItems
TurnCount
ToolCallCount
Items
```

Objetivo:

```text
Cada QueryAsync tiene su propio estado.
Hagent puede reutilizarse concurrentemente sin contaminar consultas entre sí.
```

`Items` permite compartir datos entre fases:

```text
Items["rag.context"]
Items["rag.hits"]
Items["image.descriptions"]
Items["file.extractedText"]
Items["tool.{id}.result"]
Items["tool.{id}.generatedImages"]
Items["chat.lastResult"]
Items["chat.toolCalls"]
Items["chat.toolCalls.ignored"]
Items["chat.toolCalls.ignoreReason"]
Items["autoTools.skipped"]
Items["autoTools.skipReason"]
Items["autoTools.limitReached"]
```

---

### 1.19. HagentToolContext

`HagentToolContext` se entrega a las tools durante la ejecución.

Contiene:

```text
AgentId
UserQuery
InputFiles
OutputFiles
FileOptions
ExecutionOptions
Items
```

Objetivo:

```text
Dar a la tool acceso al estado operativo necesario sin exponer directamente todo Hagent.
```

Uso por tool:

```text
FileReaderTool   → lee InputFiles y escribe file.extractedText.
FileWriterTool   → genera archivos en HagentToolResult.Files.
RagSearchTool    → lee UserQuery y escribe rag.context.
ImageToTextTool  → lee InputFiles y escribe image.descriptions.
TextToImageTool  → lee UserQuery/Settings y devuelve imágenes.
CustomHagentTool → lee/escribe Items según Settings.
```

---

### 1.20. Modelo de archivos

#### HagentFile

`HagentFile` es el modelo interno fuerte de archivo.

Estructura:

```text
FileName
MimeType
Encoding
Content
Metadata
```

Regla importante:

```text
Content es byte[].
```

El runtime no transporta base64 internamente.

#### HagentFileEnvelope

`HagentFileEnvelope` es el modelo de transporte JSON.

Formato:

```json
{
  "fileName": "imagen.png",
  "mimeType": "image/png",
  "encoding": "base64",
  "content": "<base64-content>"
}
```

Regla:

```text
Base64 solo existe en la frontera pública.
```

#### HagentFileCodec

`HagentFileCodec` convierte entre transporte público y modelo interno.

Responsabilidades:

```text
JSON → HagentFileEnvelope
HagentFileEnvelope → HagentFile
Base64 → byte[]
byte[] → Base64
Validar tamaño de entrada
Validar tamaño de salida
Validar MIME type de entrada
Validar MIME type de salida
Serializar JSON de salida
Deserializar JSON de entrada
```

Reglas:

```text
Si files null o vacío:
    DecodeInputFiles devuelve lista vacía.

Si AllowInputFiles = false y llegan archivos:
    se lanza error.

Si AllowOutputFiles = false y hay archivos de salida:
    se lanza error.

Si AllowedInputMimeTypes está vacío:
    se permite cualquier MIME.

Si AllowedOutputMimeTypes está vacío:
    se permite cualquier MIME.
```

---

### 1.21. Ejecución automática de tools

`ExecuteAutoToolsAsync` implementa la mecánica básica de tools automáticas.

Flujo:

```text
1. Verificar cancellation token.
2. Si AutoInvokeTools = false:
       no ejecutar tools.
3. Si MaxToolCalls = 0:
       no ejecutar tools.
4. Crear HagentToolContext.
5. Recorrer tools habilitadas.
6. Validar MaxToolCalls.
7. Preguntar ShouldAutoInvokeAsync.
8. Ejecutar ExecuteAsync.
9. Incrementar ToolCallCount.
10. Aplicar HagentToolResult.
```

`ApplyToolResult` hace:

```text
Guardar result en Items["tool.{toolId}.result"].
Añadir result.Files a OutputFiles.
Añadir trazas a ResponseItems si ReturnToolMessages = true.
```

---

### 1.22. Bucle chat/tools

El diseño admite dos niveles.

#### Nivel 1: chat simple con contexto previo

Flujo:

```text
1. ProcessInputFilesAsync.
2. ExecuteAutoToolsAsync.
3. BuildTextualContext.
4. BuildChatRequest.
5. Llamar una vez a IHagentChatClient.
6. Añadir result.Text como ChatResponseItem.Response.
7. Registrar ToolCalls del modelo si existen, pero no ejecutarlas.
```

Este nivel soporta:

```text
RAG previo.
Archivos procesados previamente.
Descripciones de imágenes previas.
Generación de archivos previa.
Generación de imágenes previa.
Tools automáticas.
Resultados de tools inyectados como contexto textual.
```

#### Nivel 2: tool-calling iterativo

Diseño futuro:

```text
while TurnCount < MaxTurns:
    llamar al chat
    si hay respuesta final:
        terminar
    si hay tool calls:
        buscar tools compatibles
        ejecutar tools
        añadir resultados al contexto
        continuar
```

Regla actual:

```text
El nivel 2 puede estar preparado, pero no debe complicar el flujo base hasta que el nivel 1 esté estable.
```

---

### 1.23. ChatResponse

`ChatResponse` transporta la respuesta final.

Estructura:

```text
ResponseItems
ResponseFiles
```

`ResponseItems` puede contener:

```text
Think
Tool
Response
```

Uso:

```text
Response → respuesta final del chat.
Tool     → trazas de tools si ReturnToolMessages = true.
Think    → reservado para pasos intermedios futuros.
```

Regla:

```text
BuildResponse garantiza que exista al menos un ResponseItem tipo Response, aunque esté vacío.
```

`ResponseFiles` se genera mediante:

```text
HagentFileCodec.EncodeOutputFiles(context.OutputFiles, fileOptions)
```

---

### 1.24. Decisiones arquitectónicas inmutables

#### La factoría compone, Hagent ejecuta

```text
HagentFactory:
    valida configuración
    resuelve referencias
    crea dependencias
    crea Hagent

Hagent:
    ejecuta consultas
    crea contexto por query
    procesa archivos
    ejecuta tools
    llama al chat
    construye respuesta
```

#### AgentDef no crece por capacidad

```text
Las nuevas capacidades deben agregarse como tools,
no como propiedades nuevas en AgentDef.
```

#### Los archivos se normalizan pronto

```text
ChatQuery.Files:string[]
    ↓
HagentFileCodec
    ↓
HagentFile[]
```

Después de eso, el runtime trabaja con `byte[]`, no con base64.

#### Las tools encapsulan capacidades

`Hagent` no debe contener lógica específica de:

```text
OpenAI
Ollama
LM Studio
vLLM
Qdrant
PDF
OCR
Image generation
Embeddings
```

Debe delegar en:

```text
IHagentChatClient
IHagentTool
IHagentImageToTextClient
IHagentTextToImageClient
HagentFileCodec
```

#### Async-first

```text
La API principal es QueryAsync.
Query existe por comodidad.
```

#### CustomTool declarativa y segura

`CustomHagentTool` no debe convertirse en ejecución arbitraria de código.

Primera versión permitida:

```text
echo
template
contextItem
setContextItem
```

Futuro recomendado:

```text
ICustomToolHandlerRegistry
Handlers explícitos
Sin reflexión libre
Sin comandos del sistema por defecto
Políticas de seguridad configurables
Auditoría de ejecución
```

#### Clientes IA reales intercambiables

Diseño correcto:

```text
IHagentChatClient
 ├── Stub
 ├── Microsoft.Extensions.AI
 ├── OpenAI
 └── OpenAI-compatible

ImageToTextTool
    ↓
IHagentImageToTextClient
    ↓
OpenAI / Azure / Ollama / Stub

TextToImageTool
    ↓
IHagentTextToImageClient
    ↓
OpenAI / Azure / proveedor compatible / Stub
```

---
