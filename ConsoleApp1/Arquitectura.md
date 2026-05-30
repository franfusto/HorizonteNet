# Arquitectura de Hagent

## 1. Principio de diseño

La arquitectura de Hagent se basa en una idea central:

> La configuración declara qué capacidades existen; la factoría las resuelve; `Hagent` las orquesta en tiempo de ejecución.

El flujo principal es:

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

La responsabilidad se reparte así:

```text
AgentDef describe el agente.
Configuration contiene servidores, clientes, RAGs y agentes.
HagentFactory valida y compone el runtime.
HagentFactoryContext mantiene el estado temporal de composición.
HagentFactoryException representa errores de configuración/composición.
Hagent ejecuta consultas.
HagentExecutionContext mantiene el estado de una consulta concreta.
IHagentChatClient abstrae el cliente conversacional.
IHagentTool abstrae capacidades opcionales.
IHagentImageToTextClient abstrae análisis de imágenes.
IHagentTextToImageClient abstrae generación de imágenes.
HagentFileCodec normaliza archivos.
ChatResponse transporta la respuesta final.
```

Regla principal:

```text
Hagent no debe saber cómo se resuelven servidores, clientes, RAGs o providers.
Hagent recibe dependencias ya resueltas por HagentFactory.
```

---

## 2. Modelo general de configuración

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

---

## 3. ServerDef

`ServerDef` representa un proveedor, servidor o endpoint.

Ejemplos conceptuales:

```text
OpenAI
Azure OpenAI
Ollama
LM Studio
vLLM
Servidor local
Proveedor compatible OpenAI
Proveedor custom
```

Responsabilidad:

```text
ServerDef = dónde se ejecuta o consume una capacidad.
```

Regla:

```text
ClientDef.Server debe apuntar a ServerDef.Id.
```

Ejemplo:

```text
ClientDef
 └── Server = "openai-main"

ServerDef
 └── Id = "openai-main"
```

---

## 4. ClientDef

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
```

Ejemplos:

```text
Chat principal      → ClientType.Chat
Embeddings RAG      → ClientType.Embedder
Análisis de imagen  → ClientType.ImageToText
Generación imagen   → ClientType.TextToImage
```

---

## 5. RaGdef

`RaGdef` representa una configuración RAG/vector store.

Responsabilidad:

```text
RaGdef = cómo recuperar contexto documental.
```

Regla:

```text
RaGdef.EmbedderClient debe apuntar a un ClientDef existente de tipo ClientType.Embedder.
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

En la primera versión real de `RagSearchTool`, `RaGdef` se usa en modo `InMemory` con búsqueda léxica simple. Aunque `EmbedderClient` ya se valida, todavía no se usan embeddings reales para calcular similitud vectorial.

---

## 6. AgentDef

`AgentDef` es la definición principal del agente.

La decisión de diseño más importante es:

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

---

## 7. AgentToolDef

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

Ejemplo RAG:

```text
AgentToolDef
 ├── Id = "tool-rag-search"
 ├── ToolType = RagSearch
 ├── RagId = "rag-memory-docs"
 ├── Enabled = true
 └── Settings
     └── autoUse = true
```

Ejemplo FileReader:

```text
AgentToolDef
 ├── Id = "tool-file-reader"
 ├── ToolType = FileReader
 ├── Enabled = true
 └── Settings
     ├── autoUse = true
     └── extractText = true
```

Ejemplo TextToImage:

```text
AgentToolDef
 ├── Id = "tool-text-to-image"
 ├── ToolType = TextToImage
 ├── ClientId = "openai-text-to-image-main"
 ├── Enabled = true
 └── Settings
     ├── autoUse = true
     ├── defaultMimeType = image/png
     └── size = 1024x1024
```

`Settings["autoUse"] = "true"` o `Settings["autoUse"] = "always"` permite que una tool real o stub pueda autoejecutarse durante `ExecuteAutoToolsAsync`, siempre que la tool lo soporte.

---

## 8. AgentFileOptions

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

Reglas mínimas implementadas en la factoría:

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

Validaciones funcionales opcionales:

```text
AutoProcessImages = true
    → debería existir ImageToText habilitada.

AutoProcessDocuments = true
    → debería existir FileReader o tool documental compatible.

AllowOutputFiles = true
    → debería existir FileWriter o estrategia de salida compatible.
```

Para no bloquear la integración inicial, estas validaciones funcionales se activan mediante configuración del agente:

```text
AgentDef.Settings["validateFileOptionDependencies"] = "true"
```

---

## 9. AgentExecutionOptions

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

Validaciones mínimas implementadas:

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

## 10. Factoría GetHagent

La API pública cómoda es:

```csharp
agentDef.GetHagent(configuration)
```

La extensión no contiene lógica de validación ni composición. Solo delega:

```csharp
public static Hagent GetHagent(this AgentDef agentDef, Configuration configuration)
{
    return HagentFactory.Create(agentDef, configuration);
}
```

Responsabilidad:

```text
GetHagent = punto de entrada cómodo.
HagentFactory.Create = validación, resolución y composición.
```

---

## 11. HagentFactory

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

Flujo actual:

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

`CreateTool` resuelve actualmente:

```text
AgentToolType.RagSearch   → RagSearchTool
AgentToolType.ImageToText → ImageToTextTool
AgentToolType.TextToImage → TextToImageTool
AgentToolType.FileReader  → FileReaderTool
AgentToolType.FileWriter  → FileWriterTool
AgentToolType.Custom      → CustomHagentTool
```

Clientes auxiliares creados por la factoría:

```text
ImageToTextTool
    ↓
IHagentImageToTextClient
    ↓
StubHagentImageToTextClient o cliente real futuro

TextToImageTool
    ↓
IHagentTextToImageClient
    ↓
StubHagentTextToImageClient o cliente real futuro
```

Errores de composición:

```text
HagentFactoryException
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
```

Regla:

```text
Si HagentFactory.Create devuelve un Hagent, su configuración estructural ya debe ser válida.
```

---

## 12. HagentFactoryContext

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

## 13. HagentFactoryException

`HagentFactoryException` representa errores de configuración o composición.

Objetivo:

```text
Diferenciar errores de creación del agente de errores de ejecución.
```

Ejemplos:

```text
El agente no define ChatClientId.
El cliente referenciado no existe.
El cliente no es de tipo Chat.
El servidor referenciado no existe.
La tool RAG no define RagId.
La tool RAG referencia un RAG inexistente.
La tool ImageToText no define ClientId.
La tool TextToImage usa un cliente de tipo incorrecto.
ExecutionOptions inválidas.
FileOptions inválidas.
```

---

## 14. IHagentChatClient

`IHagentChatClient` abstrae el cliente conversacional.

Contrato actual:

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

Uso actual:

```text
Hagent construye HagentChatRequest.
IHagentChatClient devuelve HagentChatResult.
Hagent convierte result.Text en ChatResponseItem.Response.
```

Implementación actual:

```text
StubHagentChatClient
```

Objetivo:

```text
Permitir ejecutar el runtime sin OpenAI, Ollama u otro proveedor real.
```

Implementaciones futuras:

```text
Microsoft.Extensions.AI
OpenAI
Azure OpenAI
OpenAI-compatible
Ollama
LM Studio
vLLM
```

---

## 15. Clientes auxiliares de IA

Además del chat principal, algunas tools usan clientes específicos.

### 15.1. IHagentImageToTextClient

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

`HagentImageToTextRequest` contiene:

```text
Prompt
Images
Options
```

`HagentImageToTextResult` contiene:

```text
Text
Metadata
```

Implementación actual:

```text
StubHagentImageToTextClient
```

Uso:

```text
ImageToTextTool
    ↓
IHagentImageToTextClient.DescribeAsync
    ↓
context.Items["image.descriptions"]
```

### 15.2. IHagentTextToImageClient

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

`HagentTextToImageRequest` contiene:

```text
Prompt
MimeType
Size
Options
```

`HagentTextToImageResult` contiene:

```text
Images
Metadata
```

Cada imagen generada contiene:

```text
FileName
MimeType
Content
Metadata
```

Implementación actual:

```text
StubHagentTextToImageClient
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

---

## 16. IHagentTool

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

Implementaciones actuales:

```text
StubHagentTool
FileReaderTool
FileWriterTool
RagSearchTool
ImageToTextTool
TextToImageTool
CustomHagentTool
```

---

## 17. Tools reales

### 17.1. FileReaderTool

Responsabilidad:

```text
Extraer texto básico de archivos soportados.
```

Primera versión soportada:

```text
text/plain
application/json
text/markdown
```

Flujo:

```text
InputFiles
    ↓
FileReaderTool
    ↓
extrae texto usando Encoding
    ↓
context.Items["file.extractedText"]
    ↓
BuildTextualContext
```

Reglas:

```text
Se autoejecuta si Settings["autoUse"] = "true".
Solo procesa MIME types soportados.
No manipula base64.
Trabaja con HagentFile.Content como byte[].
```

Pendiente:

```text
PDF
DOCX
HTML
OCR documental
```

---

### 17.2. FileWriterTool

Responsabilidad:

```text
Preparar archivos de salida.
```

Primera versión soportada:

```text
text/plain
application/json
```

Flujo:

```text
Settings / call.Arguments / context.Items
    ↓
FileWriterTool
    ↓
HagentToolResult.Files
    ↓
context.OutputFiles
    ↓
HagentFileCodec.EncodeOutputFiles
    ↓
ChatResponse.ResponseFiles
```

Fuentes de contenido:

```text
call.Arguments["content"]
Settings["content"]
Settings["sourceItemKey"]
context.UserQuery
```

Reglas:

```text
Respeta AgentFileOptions.AllowOutputFiles.
Respeta AgentFileOptions.AllowedOutputMimeTypes.
Devuelve HagentFile, no base64.
```

Limitación actual:

```text
Al ejecutarse antes del chat, todavía no puede escribir automáticamente la respuesta final del modelo.
```

Evolución futura:

```text
Post-tools después del chat.
Guardar chat.finalText en context.Items.
Generar archivos desde la respuesta final.
```

---

### 17.3. RagSearchTool

Responsabilidad:

```text
Consultar contexto documental.
```

Primera versión implementada:

```text
InMemory léxico
```

No usa todavía embeddings reales. Usa búsqueda por términos contra documentos configurados en `RaGdef.Settings`.

Fuentes de documentos soportadas:

```text
Settings["documents"] como JSON
Settings["document.{id}.title"]
Settings["document.{id}.text"]
Settings["documentText"]
Settings["documentTitle"]
```

Flujo:

```text
context.UserQuery
    ↓
RagSearchTool
    ↓
búsqueda léxica en documentos InMemory
    ↓
context.Items["rag.context"]
    ↓
BuildTextualContext
```

Reglas:

```text
Requiere AgentToolDef.RagId.
Requiere RaGdef existente.
Primera versión solo soporta RagType.InMemory.
Respeta TopK.
Respeta MinScore.
Respeta MaxContextItems.
```

Evolución futura:

```text
InMemory vectorial
SQLite
Postgres
Qdrant
Azure AI Search
Elasticsearch
```

---

### 17.4. ImageToTextTool

Responsabilidad:

```text
Analizar imágenes de entrada.
```

Depende de:

```text
IHagentImageToTextClient
```

Primera implementación de cliente:

```text
StubHagentImageToTextClient
```

MIME types soportados por defecto:

```text
image/png
image/jpeg
image/webp
```

Flujo:

```text
InputFiles
    ↓
ImageToTextTool
    ↓
IHagentImageToTextClient.DescribeAsync
    ↓
context.Items["image.descriptions"]
    ↓
BuildTextualContext
```

Reglas:

```text
Puede autoejecutarse si AutoProcessImages = true.
Puede autoejecutarse si Settings["autoUseForMimeTypes"] contiene MIME types compatibles.
Respeta los archivos ya normalizados por HagentFileCodec.
No manipula base64.
```

Evolución futura:

```text
Cliente OpenAI real
Cliente Azure OpenAI real
Cliente compatible OpenAI multimodal
OCR especializado
Extracción estructurada de imágenes
```

---

### 17.5. TextToImageTool

Responsabilidad:

```text
Generar imágenes desde texto.
```

Depende de:

```text
IHagentTextToImageClient
```

Primera implementación de cliente:

```text
StubHagentTextToImageClient
```

MIME types de salida soportados:

```text
image/png
image/jpeg
```

Flujo:

```text
Prompt
    ↓
TextToImageTool
    ↓
IHagentTextToImageClient.GenerateAsync
    ↓
HagentToolResult.Files
    ↓
context.OutputFiles
    ↓
HagentFileCodec.EncodeOutputFiles
    ↓
ChatResponse.ResponseFiles
```

Fuentes de prompt:

```text
call.Arguments["prompt"]
Settings["prompt"]
context.UserQuery
```

Reglas:

```text
Requiere ClientId de tipo ClientType.TextToImage.
Respeta AgentFileOptions.AllowOutputFiles.
Respeta AgentFileOptions.AllowedOutputMimeTypes.
Devuelve HagentFile en HagentToolResult.Files.
No devuelve base64 directamente.
```

Evolución futura:

```text
Cliente OpenAI real
Cliente Azure OpenAI real
Cliente compatible OpenAI
Soporte avanzado de size, quality, style y formatos.
```

---

### 17.6. CustomHagentTool

Responsabilidad:

```text
Extensión declarativa por Settings.
```

Primera versión segura:

```text
echo
template
contextItem
setContextItem
```

Acciones soportadas:

```text
echo
    Devuelve un texto desde Settings, argumentos o UserQuery.

template
    Renderiza una plantilla simple con tokens conocidos.

contextItem
    Lee un valor de context.Items.

setContextItem
    Guarda un valor en context.Items.
```

Tokens de plantilla soportados:

```text
{{agentId}}
{{query}}
{{toolId}}
{{toolName}}
{{nowUtc}}
{{item.{key}}}
```

Flujo:

```text
Settings["action"]
    ↓
CustomHagentTool
    ↓
acción declarativa conocida
    ↓
HagentToolResult
    ↓
context.Items["tool.{id}.result"]
```

Reglas:

```text
No ejecuta comandos del sistema.
No usa reflexión dinámica.
No carga assemblies.
No ejecuta scripts.
No hace HTTP arbitrario.
```

Evolución futura recomendada:

```text
ICustomToolHandlerRegistry
Registro explícito de handlers permitidos
Políticas de seguridad por agente
Auditoría de ejecución
```

---

## 18. Hagent

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

Constructor interno actual:

```csharp
internal Hagent(
    AgentDef agentDef,
    IHagentChatClient chatClient,
    IReadOnlyList<IHagentTool> tools,
    AgentFileOptions fileOptions,
    AgentExecutionOptions executionOptions)
```

El constructor copia las tools a un array para evitar depender de una colección mutable externa.

---

## 19. Query y QueryAsync

La API principal es:

```csharp
Task<ChatResponse> QueryAsync(ChatQuery query, CancellationToken cancellationToken = default)
```

`Query` es un wrapper síncrono:

```csharp
ChatResponse Query(ChatQuery query)
    ↓
QueryAsync(query).GetAwaiter().GetResult()
```

Flujo actual de `QueryAsync`:

```text
1. Validar query.
2. Crear CancellationTokenSource enlazado con timeout.
3. Crear HagentExecutionContext.
4. ProcessInputFilesAsync.
5. ExecuteAutoToolsAsync.
6. ExecuteToolLoopAsync.
7. BuildResponse.
```

---

## 20. HagentExecutionContext

`HagentExecutionContext` representa el estado de una consulta concreta.

Campos actuales:

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

## 21. HagentToolContext

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

## 22. HagentFile

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

El runtime no transporta base64 internamente. Base64 solo existe en la frontera pública mediante `HagentFileEnvelope`.

Ejemplos de `Encoding` interno:

```text
utf-8
binary
```

---

## 23. HagentFileEnvelope

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

Campos:

```text
fileName
mimeType
encoding
content
```

Regla actual:

```text
Solo se soporta encoding = "base64" en la frontera pública.
```

---

## 24. HagentFileCodec

`HagentFileCodec` es responsable de convertir entre transporte público y modelo interno.

Métodos:

```text
DecodeInputFiles(...)
EncodeOutputFiles(...)
ValidateInputFile(...)
ValidateOutputFile(...)
```

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

Objetivo:

```text
Hagent nunca manipula base64 directamente.
Tools nunca deben devolver base64 directamente.
Tools deben devolver HagentFile.
```

---

## 25. Ejecución automática de tools

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

`ReturnToolMessages` controla si se añaden trazas tipo `Tool` a la respuesta.

Ejemplo de traza:

```text
Ejecutando tool automática 'Lectura de archivos'.
FileReaderTool extrajo texto de 1 archivo(s).
```

---

## 26. Bucle chat/tools — Nivel 1 implementado

Actualmente está implementado el **Nivel 1**, sin tool-calling iterativo real del modelo.

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

`BuildTextualContext` incluye:

```text
Archivos de entrada disponibles.
Items["rag.context"].
Items["image.descriptions"].
Items["file.extractedText"].
Resultados guardados como Items["tool.{id}.result"].
```

El request final al chat contiene:

```text
Mensaje system con SystemPrompt.
Mensaje context si existe contexto textual.
Mensaje user con la consulta original.
Descriptors de tools habilitadas.
Opciones de ejecución.
```

---

## 27. Tool-calling iterativo — Nivel 2 preparado

El **Nivel 2** todavía no ejecuta tool-calling iterativo real.

Si el modelo devuelve `ToolCalls`:

```text
Hagent las registra en context.Items["chat.toolCalls"].
Marca context.Items["chat.toolCalls.ignored"] = true.
Añade razón en context.Items["chat.toolCalls.ignoreReason"].
Opcionalmente añade trazas tipo Tool si ReturnToolMessages = true.
```

Motivo:

```text
El flujo simple debe mantenerse estable antes de introducir bucles iterativos.
```

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

Reglas futuras:

```text
MaxTurns limita llamadas al chat.
MaxToolCalls limita ejecuciones de tools.
CanHandle decide qué tool puede manejar una llamada.
Los resultados de tools se inyectan al contexto antes del siguiente turno.
```

---

## 28. ChatResponse

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

Uso actual:

```text
Response → respuesta final del chat.
Tool     → trazas de tools si ReturnToolMessages = true.
Think    → reservado para pasos intermedios futuros.
```

`BuildResponse` garantiza que exista al menos un `ResponseItem` tipo `Response`, aunque esté vacío.

`ResponseFiles` se genera mediante:

```text
HagentFileCodec.EncodeOutputFiles(context.OutputFiles, fileOptions)
```

---

## 29. Flujo completo actual

Flujo real actual:

```text
Usuario llama QueryAsync(ChatQuery)
    ↓
Validar query
    ↓
Crear HagentExecutionContext
    ↓
DecodeInputFiles
    ↓
InputFiles
    ↓
ExecuteAutoToolsAsync
    ├── respeta AutoInvokeTools
    ├── respeta MaxToolCalls
    ├── ejecuta tools reales con autoUse=true / condiciones propias
    ├── incrementa ToolCallCount
    └── guarda resultados en Items / ResponseItems / OutputFiles
    ↓
BuildTextualContext
    ├── InputFiles
    ├── rag.context
    ├── image.descriptions
    ├── file.extractedText
    └── tool.*.result
    ↓
BuildChatRequest
    ├── system
    ├── context
    ├── user
    ├── tool descriptors
    └── options
    ↓
IHagentChatClient.CompleteAsync
    ↓
HagentChatResult
    ├── Text
    ├── ToolCalls
    └── Metadata
    ↓
Guardar respuesta final
    ↓
Registrar ToolCalls si existen, sin ejecutarlas todavía
    ↓
EncodeOutputFiles
    ↓
ChatResponse
```

---

## 30. Stubs actuales

### StubHagentChatClient

Responsabilidad:

```text
Permitir ejecutar el runtime sin proveedor real de chat.
```

Resultado esperado:

```text
Devuelve una respuesta de prueba.
```

### StubHagentTool

Responsabilidad:

```text
Permitir probar ejecución automática genérica de tools.
```

Uso actual:

```text
Fallback para escenarios donde todavía no exista una tool real.
```

### StubHagentImageToTextClient

Responsabilidad:

```text
Permitir validar ImageToTextTool sin proveedor real de visión.
```

Resultado esperado:

```text
Devuelve una descripción stub de las imágenes recibidas.
```

### StubHagentTextToImageClient

Responsabilidad:

```text
Permitir validar TextToImageTool sin proveedor real de generación de imágenes.
```

Resultado esperado:

```text
Devuelve una imagen placeholder como HagentFile.
```

---

## 31. Validaciones actuales

### Validaciones de composición

En `HagentFactory`:

```text
AgentDef no null.
Configuration no null.
ChatClientId no vacío.
ClientDef existe.
ClientDef.ClientType = Chat.
ClientDef.Server no vacío.
ServerDef existe.
Tools habilitadas tienen referencias válidas.
RagSearch requiere RagId válido.
ImageToText requiere ClientId válido de tipo ImageToText.
TextToImage requiere ClientId válido de tipo TextToImage.
ExecutionOptions válidas.
FileOptions estrictas válidas.
FileOptions funcionales opcionales.
```

### Validaciones de runtime

En `Hagent`:

```text
query no null.
query.Query no vacío.
dependencias internas no null.
CancellationToken respetado.
Timeout aplicado por QueryAsync.
MaxToolCalls respetado.
MaxTurns respetado antes de llamar al chat.
```

### Validaciones de archivos

En `HagentFileCodec`:

```text
JSON válido.
Encoding soportado.
Base64 válido.
Tamaño permitido.
MIME permitido.
AllowInputFiles.
AllowOutputFiles.
```

### Validaciones de tools reales

```text
FileReaderTool:
    MIME type soportado.

FileWriterTool:
    AllowOutputFiles.
    AllowedOutputMimeTypes.
    MIME type soportado.

RagSearchTool:
    RagType.InMemory.
    documentos disponibles.
    TopK / MinScore.

ImageToTextTool:
    imágenes compatibles.
    cliente ImageToText resuelto.

TextToImageTool:
    AllowOutputFiles.
    MIME type de salida permitido.
    cliente TextToImage resuelto.
    prompt no vacío.

CustomHagentTool:
    acción declarativa conocida.
    sourceItemKey / targetItemKey cuando aplica.
```

---

## 32. Estado implementado

Estado actual:

```text
Implementado:
 ├── HagentFactoryException.
 ├── HagentFactoryContext.
 ├── HagentFactoryExtensions.GetHagent.
 ├── HagentFactory.Create.
 ├── Validación de ChatClientId.
 ├── Validación de ClientType.Chat.
 ├── Validación de ServerDef.
 ├── Validación básica de tools.
 ├── Validación de AgentExecutionOptions.
 ├── Validación estricta de AgentFileOptions.
 ├── Validación funcional opcional de AgentFileOptions.
 ├── IHagentChatClient.
 ├── StubHagentChatClient.
 ├── IHagentImageToTextClient.
 ├── StubHagentImageToTextClient.
 ├── IHagentTextToImageClient.
 ├── StubHagentTextToImageClient.
 ├── IHagentTool.
 ├── StubHagentTool.
 ├── FileReaderTool real.
 ├── FileWriterTool real.
 ├── RagSearchTool real InMemory léxico.
 ├── ImageToTextTool real con cliente stub.
 ├── TextToImageTool real con cliente stub.
 ├── CustomHagentTool declarativa.
 ├── Constructor interno de Hagent.
 ├── Query.
 ├── QueryAsync.
 ├── HagentExecutionContext.
 ├── HagentToolContext.
 ├── HagentFile.
 ├── HagentFileEnvelope.
 ├── HagentFileCodec.
 ├── DecodeInputFiles.
 ├── EncodeOutputFiles.
 ├── ExecuteAutoToolsAsync.
 ├── ApplyToolResult.
 ├── BuildTextualContext.
 ├── Bucle chat Nivel 1.
 └── Tool-calling Nivel 2 preparado pero no ejecutado.
```

Pendiente:

```text
Clientes IA reales.
Cliente chat real.
Cliente ImageToText real.
Cliente TextToImage real.
RAG vectorial real.
PDF.
DOCX.
HTML.
Tool-calling iterativo real.
Post-tools después del chat.
Sesiones.
Streaming.
Tests completos.
```

---

## 33. Roadmap recomendado actualizado

Orden recomendado a partir de ahora:

```text
1. Tests del runtime actual con StubHagentChatClient.
2. Tests de HagentFileCodec.
3. Tests de ExecuteAutoToolsAsync.
4. Tests de FileReaderTool.
5. Tests de FileWriterTool.
6. Tests de RagSearchTool InMemory.
7. Tests de ImageToTextTool con stub.
8. Tests de TextToImageTool con stub.
9. Tests de CustomHagentTool.
10. Cliente chat real mediante Microsoft.Extensions.AI.
11. Cliente OpenAI o compatible OpenAI.
12. Cliente ImageToText real.
13. Cliente TextToImage real.
14. RAG InMemory vectorial con embeddings.
15. Persistencia RAG: SQLite/Postgres/Qdrant/Azure AI Search/Elasticsearch.
16. Tool-calling iterativo real.
17. Post-tools después del chat.
18. Sesiones.
19. Streaming.
```

---

## 34. Testing recomendado

### Tests de factoría

```text
AgentDef null lanza HagentFactoryException.
Configuration null lanza HagentFactoryException.
ChatClientId vacío lanza HagentFactoryException.
Cliente inexistente lanza HagentFactoryException.
Cliente no Chat lanza HagentFactoryException.
Servidor inexistente lanza HagentFactoryException.
ExecutionOptions inválidas lanzan HagentFactoryException.
FileOptions inválidas lanzan HagentFactoryException.
RagSearch sin RagId lanza HagentFactoryException.
RagSearch con RagId inexistente lanza HagentFactoryException.
ImageToText sin ClientId lanza HagentFactoryException.
ImageToText con cliente incorrecto lanza HagentFactoryException.
TextToImage sin ClientId lanza HagentFactoryException.
TextToImage con cliente incorrecto lanza HagentFactoryException.
Agent válido crea Hagent.
```

### Tests de runtime

```text
QueryAsync devuelve ChatResponse.
Query delega en QueryAsync.
Query null lanza error.
Query vacía lanza error.
AutoInvokeTools false no ejecuta tools.
AutoInvokeTools true ejecuta tools autoUse=true.
MaxToolCalls se respeta.
ReturnToolMessages controla trazas.
TurnCount incrementa tras llamar al chat.
ToolCallCount incrementa tras ejecutar tools automáticas.
ToolCalls del modelo se registran pero no se ejecutan.
```

### Tests de archivos

```text
DecodeInputFiles con null devuelve vacío.
DecodeInputFiles decodifica JSON/base64 válido.
DecodeInputFiles rechaza JSON inválido.
DecodeInputFiles rechaza base64 inválido.
DecodeInputFiles rechaza encoding no soportado.
DecodeInputFiles rechaza tamaño excedido.
DecodeInputFiles rechaza MIME no permitido.
EncodeOutputFiles genera JSON válido.
EncodeOutputFiles genera base64 válido.
EncodeOutputFiles respeta AllowOutputFiles.
```

### Tests de FileReaderTool

```text
No se autoejecuta si autoUse=false.
No se autoejecuta sin archivos.
No se autoejecuta con MIME no soportado.
Extrae text/plain.
Extrae application/json.
Extrae text/markdown.
Guarda Items["file.extractedText"].
Respeta CancellationToken.
```

### Tests de FileWriterTool

```text
No genera archivos si AllowOutputFiles=false.
Genera text/plain.
Genera application/json.
Respeta AllowedOutputMimeTypes.
Devuelve HagentFile en HagentToolResult.Files.
No devuelve base64 directamente.
```

### Tests de RagSearchTool

```text
No se autoejecuta si autoUse=false.
Se autoejecuta si autoUse=true y hay documentos.
Guarda Items["rag.context"].
Devuelve 0 hits si no hay coincidencias.
Respeta TopK.
Respeta MinScore.
Falla si RagType no es InMemory.
```

### Tests de ImageToTextTool

```text
No se ejecuta sin imágenes.
Detecta image/png.
Detecta image/jpeg.
Detecta image/webp.
Llama a IHagentImageToTextClient.
Guarda Items["image.descriptions"].
Respeta AutoProcessImages.
Respeta autoUseForMimeTypes.
```

### Tests de TextToImageTool

```text
No genera si AllowOutputFiles=false.
Genera image/png.
Genera image/jpeg.
Respeta AllowedOutputMimeTypes.
Devuelve HagentFile en Files.
Guarda metadatos de generación.
Respeta prompt desde Settings.
Respeta prompt desde UserQuery.
```

### Tests de CustomHagentTool

```text
Acción echo devuelve texto.
Acción template reemplaza tokens.
Acción contextItem lee Items.
Acción setContextItem escribe Items.
Acción desconocida devuelve error controlado.
No ejecuta comandos ni código externo.
```

### Tests de chat Nivel 1

```text
BuildTextualContext incluye archivos.
BuildTextualContext incluye rag.context.
BuildTextualContext incluye image.descriptions.
BuildTextualContext incluye file.extractedText.
BuildTextualContext incluye tool results.
ExecuteToolLoopAsync llama una vez al chat.
ToolCalls del modelo se registran pero no se ejecutan.
```

---

## 35. Decisiones arquitectónicas importantes

### 35.1. La factoría compone, Hagent ejecuta

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

### 35.2. AgentDef no crece por capacidad

Las nuevas capacidades deben agregarse como tools, no como propiedades nuevas en `AgentDef`.

### 35.3. Los archivos se normalizan pronto

```text
ChatQuery.Files:string[]
    ↓
HagentFileCodec
    ↓
HagentFile[]
```

Después de eso, el runtime trabaja con `byte[]`, no con base64.

### 35.4. Las tools encapsulan capacidades

`Hagent` no debe contener lógica específica de:

```text
OpenAI
Ollama
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

### 35.5. Async-first

La API principal es `QueryAsync`.

`Query` existe por comodidad, pero delega en `QueryAsync`.

### 35.6. Tool-calling iterativo se pospone

El nivel actual registra tool calls del modelo, pero no las ejecuta.

Esto evita complicar el primer corte y permite estabilizar:

```text
Factory
Runtime
Files
Tools automáticas
Tools reales simples
Chat simple
ChatResponse
```

### 35.7. CustomTool declarativa y segura

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
```

### 35.8. Clientes IA reales intercambiables

Las tools multimodales no deben conocer proveedores concretos.

Diseño correcto:

```text
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

## 36. Resumen ejecutivo

La arquitectura actual permite construir y ejecutar agentes declarativos con un runtime funcional de extremo a extremo.

Actualmente ya se puede:

```text
Crear Hagent desde configuración.
Validar composición.
Procesar archivos de entrada.
Ejecutar tools automáticas.
Extraer texto de archivos simples.
Preparar archivos de salida text/plain o application/json.
Recuperar contexto documental InMemory.
Analizar imágenes mediante cliente stub intercambiable.
Generar imágenes placeholder mediante cliente stub intercambiable.
Ejecutar CustomTool declarativa basada en Settings.
Construir contexto textual.
Llamar a un chat client stub.
Construir ChatResponse.
Codificar archivos de salida.
Registrar tool calls del modelo para una fase posterior.
```

Principio final:

> `Hagent` debe ser pequeño, estable y orquestador.  
> Las capacidades deben vivir en tools intercambiables.  
> La configuración debe seguir siendo declarativa.  
> La factoría debe concentrar validación, resolución y composición.  
> El runtime debe ser async-first, stateless por consulta y extensible.
````