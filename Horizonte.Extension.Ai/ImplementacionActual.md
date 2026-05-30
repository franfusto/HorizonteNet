
## 2. Implementación actual, proveedores y pendientes

Esta sección describe el estado actual del proyecto, las implementaciones disponibles, los cambios introducidos para tener un flujo completo de pruebas y las capacidades pendientes.

---

### 2.1. Estado general actual

Actualmente Hagent dispone de un flujo funcional completo de pruebas:

```text
Configuration + AgentDef
    ↓
agentDef.GetHagent(configuration)
    ↓
HagentFactory.Create(...)
    ↓
Validación + resolución
    ↓
Creación de chat client
    ↓
Creación de tools
    ↓
Hagent.Query / Hagent.QueryAsync
    ↓
Procesamiento de archivos
    ↓
Tools automáticas
    ↓
Construcción de contexto textual
    ↓
Cliente chat stub o real
    ↓
ChatResponse
```

Ya se puede:

```text
Crear Hagent desde configuración.
Validar composición.
Resolver ServerDef y ClientDef.
Seleccionar implementación de chat.
Procesar archivos de entrada.
Ejecutar tools automáticas.
Extraer texto de archivos simples.
Preparar archivos de salida.
Recuperar contexto documental InMemory.
Analizar imágenes mediante cliente stub.
Generar imágenes placeholder mediante cliente stub.
Ejecutar CustomTool declarativa.
Construir contexto textual.
Llamar a un cliente chat stub o compatible real.
Construir ChatResponse.
Codificar archivos de salida.
Registrar tool calls del modelo para una fase posterior.
Probar un flujo completo con LM Studio vía endpoint OpenAI-compatible.
```

---

### 2.2. Dependencias actuales

El proyecto apunta a:

```text
TargetFramework: net10.0
Nullable: enable
```

Paquetes relevantes:

```text
Microsoft.Agents.AI
Microsoft.Agents.AI.OpenAI
Microsoft.Extensions.AI
Microsoft.Extensions.AI.OpenAI
Microsoft.SemanticKernel.Connectors.InMemory
OpenAI
```

Objetivo de estas dependencias:

```text
Microsoft.Extensions.AI permite adaptar proveedores reales a IChatClient.
OpenAI permite crear clientes OpenAI y OpenAI-compatible.
SemanticKernel/InMemory deja preparado terreno para RAG más avanzado.
```

---

### 2.3. Implementaciones actuales de IHagentChatClient

El contrato estable es:

```text
IHagentChatClient
    ↓
CompleteAsync(HagentChatRequest, CancellationToken)
    ↓
HagentChatResult
```

Implementaciones actuales o previstas en el flujo actual:

```text
StubHagentChatClient
MicrosoftExtensionsHagentChatClient
OpenAIHagentChatClient
OpenAICompatibleHagentChatClient
```

Arquitectura de adaptación:

```text
IHagentChatClient
    ↓
MicrosoftExtensionsHagentChatClient
    ↓
Microsoft.Extensions.AI.IChatClient
    ↓
Proveedor real
```

---

### 2.4. StubHagentChatClient

Responsabilidad:

```text
Permitir ejecutar el runtime sin proveedor real de chat.
```

Uso:

```text
Tests.
Desarrollo local.
Validación del flujo de HagentFactory.
Validación de Hagent.QueryAsync.
Validación de tools sin depender de red ni API keys.
```

Regla:

```text
Debe mantenerse siempre disponible como fallback controlado.
```

---

### 2.5. MicrosoftExtensionsHagentChatClient

Responsabilidad:

```text
Adaptar Microsoft.Extensions.AI.IChatClient al contrato IHagentChatClient.
```

Flujo:

```text
HagentChatRequest
    ↓
Conversión a Microsoft.Extensions.AI.ChatMessage
    ↓
ChatOptions
    ↓
IChatClient.GetResponseAsync
    ↓
HagentChatResult
```

Mapeo de roles:

```text
system    → ChatRole.System
assistant → ChatRole.Assistant
user      → ChatRole.User
context   → ChatRole.System con prefijo "Contexto adicional:"
otros     → ChatRole.User
```

Opciones incluidas:

```text
ModelId desde ClientDef.Model.
Temperature desde ClientDef.Temperature.
MaxOutputTokens desde ClientDef.MaxOutputTokens.
Options del request como AdditionalProperties.
ClientDef.Settings como AdditionalProperties con prefijo "client.".
```

Metadata devuelta:

```text
clientId
serverId
provider
model
adapter
```

Regla:

```text
Este adaptador no debe conocer detalles específicos de Hagent más allá del contrato de chat.
```

---

### 2.6. OpenAIHagentChatClient

Responsabilidad:

```text
Crear un IChatClient real para OpenAI usando ServerDef y ClientDef.
```

Fuente de configuración:

```text
ServerDef.Provider = "OpenAI"
ServerDef.Url = "https://api.openai.com/v1"
ServerDef.ApiKey = "env:OPENAI_API_KEY"
ClientDef.Model = modelo OpenAI
```

Resolución de API key:

```text
Si ApiKey empieza por "env:", se lee desde variable de entorno.
Si ApiKey es literal, se usa directamente.
Si falta ApiKey, se lanza error.
```

Regla de seguridad:

```text
No guardar API keys reales en código.
Usar siempre placeholders o variables de entorno.
```

Ejemplo:

```csharp
new ServerDef
{
    Id = "openai-main",
    Name = "OpenAI principal",
    Provider = "OpenAI",
    Url = "https://api.openai.com/v1",
    ApiKey = "env:OPENAI_API_KEY",
    TimeoutSeconds = 60,
    Settings = new Dictionary<string, string>
    {
        ["implementation"] = "openai"
    }
}
```

---

### 2.7. OpenAICompatibleHagentChatClient

Responsabilidad:

```text
Crear un IChatClient para servidores compatibles con la API de OpenAI.
```

Servidores objetivo:

```text
LM Studio
Ollama
vLLM
LocalAI
Otros endpoints compatibles OpenAI
```

Fuente de configuración:

```text
ServerDef.Provider = "LM Studio" | "Ollama" | "vLLM" | "LocalAI" | "OpenAI-Compatible"
ServerDef.Settings["compatibility"] = "openai"
ServerDef.Settings["implementation"] = "openai-compatible"
ClientDef.Settings["implementation"] = "openai-compatible"
```

Reglas:

```text
Si ServerDef.Url no termina en /v1, el cliente puede normalizarlo.
Si ApiKey está vacío, se puede usar un valor local dummy porque muchos servidores locales lo ignoran.
No exponer claves reales.
```

---

### 2.8. Selección de implementación de chat

`HagentFactory` selecciona la implementación mediante:

```text
ClientDef.Settings["implementation"]
ServerDef.Settings["implementation"]
ServerDef.Settings["chatClient"]
Provider inferido
Fallback a stub
```

Orden lógico:

```text
1. Si ClientDef.Settings["implementation"] existe:
       usar esa implementación.

2. Si ServerDef.Settings["implementation"] existe:
       usar esa implementación.

3. Si ServerDef.Settings["chatClient"] existe:
       usar ese valor.

4. Si Provider = "OpenAI":
       usar "openai".

5. Si Provider es compatible OpenAI:
       usar "openai-compatible".

6. Si nada aplica:
       usar "stub".
```

Implementaciones soportadas:

```text
stub
openai
openai-compatible
microsoft.extensions.ai
```

Providers considerados compatibles OpenAI:

```text
Ollama
LM Studio
vLLM
LocalAI
OpenAI-Compatible
```

También se considera compatible si:

```text
ServerDef.Settings["compatibility"] = "openai"
```

---

### 2.9. Ejemplo de configuración para LM Studio

LM Studio se integra como proveedor OpenAI-compatible.

Endpoint habitual:

```text
http://localhost:1234/v1
```

Requisito:

```text
LM Studio debe tener iniciado el servidor local.
Developer → Local Server → Start Server
```

Comprobación opcional:

```bash
curl http://localhost:1234/v1/models
```

Ejemplo completo:

```csharp
private static Configuration GetConfigurationLmStudio()
{
    return new Configuration
    {
        ServerDefs =
        [
            new ServerDef
            {
                Id = "lmstudio-local",
                Name = "LM Studio local",
                Provider = "LM Studio",
                Url = "http://localhost:1234/v1",
                ApiKey = string.Empty,
                TimeoutSeconds = 120,
                Settings = new Dictionary<string, string>
                {
                    ["implementation"] = "openai-compatible",
                    ["compatibility"] = "openai"
                }
            }
        ],

        ClientDefs =
        [
            new ClientDef
            {
                Id = "lmstudio-chat-main",
                Name = "Chat LM Studio",
                ClientType = ClientType.Chat,
                Server = "lmstudio-local",
                Model = "local-model",
                Temperature = 0.2f,
                MaxOutputTokens = 2000,
                EnableStreaming = false,
                EnableTools = false,
                Settings = new Dictionary<string, string>
                {
                    ["implementation"] = "openai-compatible",
                    ["responseFormat"] = "text"
                }
            }
        ],

        RaGdefs = [],

        AgentDefs =
        [
            new AgentDef
            {
                Id = "lmstudio-agent",
                Name = "Agente LM Studio",
                Description = "Agente local usando LM Studio vía API compatible con OpenAI.",
                SystemPrompt = """
                               Eres un asistente local ejecutándose mediante LM Studio.
                               Responde en español, de forma clara y útil.
                               """,

                ChatClientId = "lmstudio-chat-main",

                Tools = [],

                FileOptions = new AgentFileOptions
                {
                    AllowInputFiles = false,
                    AllowOutputFiles = false,
                    TransportEncoding = "base64"
                },

                ExecutionOptions = new AgentExecutionOptions
                {
                    MaxTurns = 1,
                    MaxToolCalls = 0,
                    TimeoutSeconds = 120,
                    AutoInvokeTools = false,
                    ReturnToolMessages = false,
                    ReturnIntermediateSteps = false
                },

                Settings = new Dictionary<string, string>
                {
                    ["profile"] = "local-lmstudio"
                }
            }
        ]
    };
}
```

Uso:

```csharp
var configuration = GetConfigurationLmStudio();

var agentDef = configuration.AgentDefs
    .First(agent => agent.Id == "lmstudio-agent");

var hagent = agentDef.GetHagent(configuration);

var response = hagent.Query(new ChatQuery
{
    Query = "Explícame brevemente qué es un agente de IA."
});

foreach (var item in response.ResponseItems ?? [])
{
    Console.WriteLine(item.Content);
}
```

Notas:

```text
ClientDef.Model debe coincidir preferiblemente con el modelo cargado en LM Studio.
Puede obtenerse consultando /v1/models.
```

---

### 2.10. Ejemplo de configuración para Ollama

Ollama puede integrarse como OpenAI-compatible.

Endpoint habitual:

```text
http://localhost:11434/v1
```

Ejemplo:

```csharp
new ServerDef
{
    Id = "local-ollama",
    Name = "Servidor local Ollama",
    Provider = "Ollama",
    Url = "http://localhost:11434",
    ApiKey = string.Empty,
    TimeoutSeconds = 120,
    Settings = new Dictionary<string, string>
    {
        ["implementation"] = "openai-compatible",
        ["compatibility"] = "openai"
    }
}
```

Cliente:

```csharp
new ClientDef
{
    Id = "ollama-chat-main",
    Name = "Chat Ollama",
    ClientType = ClientType.Chat,
    Server = "local-ollama",
    Model = "llama3.1",
    Temperature = 0.2f,
    MaxOutputTokens = 2000,
    EnableStreaming = false,
    EnableTools = false,
    Settings = new Dictionary<string, string>
    {
        ["implementation"] = "openai-compatible"
    }
}
```

---

### 2.11. Tools implementadas actualmente

#### FileReaderTool

Responsabilidad:

```text
Extraer texto básico de archivos soportados.
```

MIME types soportados:

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
Respeta CancellationToken.
```

Pendiente:

```text
PDF
DOCX
HTML
OCR documental
```

---

#### FileWriterTool

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

Limitación actual:

```text
Al ejecutarse antes del chat, todavía no puede escribir automáticamente la respuesta final del modelo.
```

Pendiente:

```text
Post-tools después del chat.
Guardar chat.finalText en context.Items.
Generar archivos desde la respuesta final.
```

---

#### RagSearchTool

Responsabilidad:

```text
Consultar contexto documental.
```

Primera versión:

```text
InMemory léxico
```

Características actuales:

```text
No usa todavía embeddings reales.
Usa búsqueda por términos contra documentos configurados en RaGdef.Settings.
Respeta TopK.
Respeta MinScore.
Respeta MaxContextItems.
Guarda contexto en Items["rag.context"].
```

Fuentes de documentos soportadas:

```text
Settings["documents"] como JSON
Settings["document.{id}.title"]
Settings["document.{id}.text"]
Settings["documentText"]
Settings["documentTitle"]
```

Pendiente:

```text
InMemory vectorial.
Embeddings reales.
SQLite.
Postgres.
Qdrant.
Azure AI Search.
Elasticsearch.
```

---

#### ImageToTextTool

Responsabilidad:

```text
Analizar imágenes de entrada.
```

Depende de:

```text
IHagentImageToTextClient
```

Cliente actual:

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

Pendiente:

```text
Cliente OpenAI real.
Cliente Azure OpenAI real.
Cliente compatible OpenAI multimodal.
OCR especializado.
Extracción estructurada de imágenes.
```

---

#### TextToImageTool

Responsabilidad:

```text
Generar imágenes desde texto.
```

Depende de:

```text
IHagentTextToImageClient
```

Cliente actual:

```text
StubHagentTextToImageClient
```

El stub genera una imagen placeholder como bytes internos y devuelve archivos mediante:

```text
HagentToolResult.Files
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

Pendiente:

```text
Cliente OpenAI real.
Cliente Azure OpenAI real.
Cliente compatible OpenAI.
Soporte avanzado de size, quality, style y formatos.
```

---

#### CustomHagentTool

Responsabilidad:

```text
Extensión declarativa por Settings.
```

Acciones soportadas:

```text
echo
template
contextItem
setContextItem
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

Reglas de seguridad:

```text
No ejecuta comandos del sistema.
No usa reflexión dinámica.
No carga assemblies.
No ejecuta scripts.
No hace HTTP arbitrario.
```

Pendiente recomendado:

```text
ICustomToolHandlerRegistry.
Registro explícito de handlers permitidos.
Políticas de seguridad por agente.
Auditoría de ejecución.
```

---

### 2.12. Flujo completo actual de QueryAsync

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

### 2.13. BuildChatRequest actual

El request final al chat contiene:

```text
Mensaje system con SystemPrompt.
Mensaje context si existe contexto textual.
Mensaje user con la consulta original.
Descriptors de tools habilitadas.
Opciones de ejecución.
```

Opciones incluidas:

```text
agentId
agentName
maxTurns
maxToolCalls
autoInvokeTools
turnCount
toolCallCount
```

Regla:

```text
Los clientes reales pueden mapear estas opciones a metadata o AdditionalProperties.
```

---

### 2.14. Tool-calling iterativo actual

Estado:

```text
Preparado, pero no ejecutado.
```

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

Pendiente:

```text
Resolver tool por CanHandle.
Ejecutar tool calls del modelo.
Añadir resultados al contexto.
Volver a llamar al chat.
Respetar MaxTurns y MaxToolCalls.
```

---

### 2.15. Validaciones actuales

#### Validaciones de composición

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
Implementación de chat soportada.
```

#### Validaciones de runtime

```text
query no null.
query.Query no vacío.
dependencias internas no null.
CancellationToken respetado.
Timeout aplicado por QueryAsync.
MaxToolCalls respetado.
MaxTurns respetado antes de llamar al chat.
```

#### Validaciones de archivos

```text
JSON válido.
Encoding soportado.
Base64 válido.
Tamaño permitido.
MIME permitido.
AllowInputFiles.
AllowOutputFiles.
```

#### Validaciones de tools

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

### 2.16. Estado implementado

Estado actual:

```text
Implementado:
 ├── HagentFactoryException.
 ├── HagentFactoryContext.
 ├── HagentFactoryExtensions.GetHagent.
 ├── HagentFactory.Create.
 ├── Resolución de ChatClientId.
 ├── Validación de ClientType.Chat.
 ├── Validación de ServerDef.
 ├── Selección de implementación de chat.
 ├── Detección de provider OpenAI.
 ├── Detección de providers OpenAI-compatible.
 ├── IHagentChatClient.
 ├── StubHagentChatClient.
 ├── MicrosoftExtensionsHagentChatClient.
 ├── OpenAIHagentChatClient.
 ├── OpenAICompatibleHagentChatClient.
 ├── Configuración de ejemplo para LM Studio.
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

---

### 2.17. Pendiente técnico

Pendiente:

```text
Tests completos.
Cliente ImageToText real.
Cliente TextToImage real.
RAG vectorial real.
Embeddings reales.
PDF.
DOCX.
HTML.
Tool-calling iterativo real.
Post-tools después del chat.
Sesiones.
Streaming.
Observabilidad.
Logging estructurado.
Políticas avanzadas de seguridad para tools custom.
```

---

### 2.18. Roadmap recomendado

Orden recomendado a partir del estado actual:

```text
1. Tests del runtime actual con StubHagentChatClient.
2. Tests de MicrosoftExtensionsHagentChatClient con IChatClient fake.
3. Tests de selección de proveedor en HagentFactory.
4. Tests de OpenAICompatibleHagentChatClient con endpoint local.
5. Tests de configuración LM Studio.
6. Tests de HagentFileCodec.
7. Tests de ExecuteAutoToolsAsync.
8. Tests de FileReaderTool.
9. Tests de FileWriterTool.
10. Tests de RagSearchTool InMemory.
11. Tests de ImageToTextTool con stub.
12. Tests de TextToImageTool con stub.
13. Tests de CustomHagentTool.
14. Cliente ImageToText real.
15. Cliente TextToImage real.
16. RAG InMemory vectorial con embeddings.
17. Persistencia RAG: SQLite/Postgres/Qdrant/Azure AI Search/Elasticsearch.
18. Tool-calling iterativo real.
19. Post-tools después del chat.
20. Sesiones.
21. Streaming.
```

---

### 2.19. Testing recomendado

#### Tests de factoría

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
Provider OpenAI resuelve OpenAIHagentChatClient.
Provider LM Studio resuelve OpenAICompatibleHagentChatClient.
Provider Ollama resuelve OpenAICompatibleHagentChatClient.
implementation=stub resuelve StubHagentChatClient.
implementation desconocida lanza HagentFactoryException.
Agent válido crea Hagent.
```

#### Tests de runtime

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

#### Tests de archivos

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

#### Tests de FileReaderTool

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

#### Tests de FileWriterTool

```text
No genera archivos si AllowOutputFiles=false.
Genera text/plain.
Genera application/json.
Respeta AllowedOutputMimeTypes.
Devuelve HagentFile en HagentToolResult.Files.
No devuelve base64 directamente.
```

#### Tests de RagSearchTool

```text
No se autoejecuta si autoUse=false.
Se autoejecuta si autoUse=true y hay documentos.
Guarda Items["rag.context"].
Devuelve 0 hits si no hay coincidencias.
Respeta TopK.
Respeta MinScore.
Falla si RagType no es InMemory.
```

#### Tests de clientes reales

```text
MicrosoftExtensionsHagentChatClient convierte roles correctamente.
MicrosoftExtensionsHagentChatClient convierte context a system.
MicrosoftExtensionsHagentChatClient propaga ModelId.
MicrosoftExtensionsHagentChatClient propaga Temperature.
MicrosoftExtensionsHagentChatClient propaga MaxOutputTokens.
OpenAICompatibleHagentChatClient normaliza endpoint /v1.
OpenAICompatibleHagentChatClient acepta ApiKey vacía para servidores locales.
LM Studio responde usando configuración openai-compatible.
```

---

### 2.20. Resumen ejecutivo

El estado actual permite validar Hagent de extremo a extremo:

```text
Configuración declarativa.
Factoría sólida.
Runtime async.
Procesamiento de archivos.
Tools automáticas.
Tools reales simples.
Chat vía stub.
Chat vía Microsoft.Extensions.AI.
Chat vía OpenAI.
Chat vía OpenAI-compatible.
Ejemplo funcional para LM Studio.
Respuesta final en ChatResponse.
```

Principio final:

> `Hagent` debe ser pequeño, estable y orquestador.  
> Las capacidades deben vivir en tools intercambiables.  
> La configuración debe seguir siendo declarativa.  
> La factoría debe concentrar validación, resolución y composición.  
> El runtime debe ser async-first, stateless por consulta y extensible.  
> Los clientes reales deben ser adaptadores reemplazables detrás de `IHagentChatClient`.
```
