
# Arquitectura de Hagent

## 1. Principio de diseño

La arquitectura se basa en una idea central:

> La configuración declara qué capacidades existen; la factoría las resuelve; `Hagent` las orquesta en tiempo de ejecución.

El flujo general es:

```text
Configuración declarativa
    ↓
AgentDef.GetHagent(configuration)
    ↓
Validación + resolución de dependencias
    ↓
Hagent runtime
    ↓
Query() / QueryAsync()
    ↓
Tools + chat client + file codec
    ↓
ChatResponse
```

Responsabilidades principales:

```text
AgentDef describe el agente.
GetHagent resuelve la configuración.
Hagent orquesta la ejecución.
IHagentTool ejecuta capacidades concretas.
IHagentChatClient habla con modelos conversacionales.
HagentFileCodec traduce archivos de entrada y salida.
ChatResponse transporta el resultado final.
```

El objetivo es evitar que `Hagent` tenga capacidades hardcodeadas. Las capacidades opcionales del agente se declaran mediante `AgentDef.Tools`.

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

Cada bloque tiene una responsabilidad concreta:

| Bloque | Responsabilidad |
|---|---|
| `ServerDef` | Define proveedores o endpoints externos/locales. |
| `ClientDef` | Define capacidades IA concretas servidas por un servidor. |
| `RAGdef` | Define configuraciones RAG/vector store. |
| `AgentDef` | Define agentes, prompt, cliente chat principal, tools y opciones. |

---

## 3. ServerDef

`ServerDef` representa un proveedor, servidor o endpoint.

Ejemplos:

- OpenAI.
- Ollama.
- Azure OpenAI.
- Servidor local.
- Proveedor compatible con OpenAI.
- Proveedor custom.

Responsabilidad:

```text
ServerDef = dónde se ejecuta o consume una capacidad.
```

Regla importante:

```text
ClientDef.Server debe apuntar a ServerDef.Id.
```

Ejemplo conceptual:

```text
ClientDef
 └── Server = "openai-main"

ServerDef
 └── Id = "openai-main"
```

---

## 4. ClientDef

`ClientDef` representa una capacidad IA concreta servida por un `ServerDef`.

Ejemplos de capacidades:

- Chat.
- Embeddings.
- Text-to-image.
- Image-to-text.
- OCR.
- Audio.
- Reranker.
- Custom.

Responsabilidad:

```text
ClientDef = qué modelo o capacidad concreta se quiere usar.
```

Reglas importantes:

```text
ClientDef.Server debe existir en Configuration.ServerDefs.
ClientDef.ClientType define el tipo de capacidad.
```

Ejemplos:

```text
Chat principal       → ClientType.Chat
Embeddings RAG      → ClientType.Embedder
Análisis de imagen  → ClientType.ImageToText
Generación imagen   → ClientType.TextToImage
```

---

## 5. RAGdef

`RAGdef` representa una configuración RAG o vector store.

Puede apuntar a distintos tipos de almacenamiento:

- InMemory.
- MongoDB.
- SQLite.
- Postgres.
- Qdrant.
- Redis.
- Azure AI Search.
- Elasticsearch.
- Custom.

Responsabilidad:

```text
RAGdef = cómo recuperar contexto documental.
```

Regla importante:

```text
RAGdef.EmbedderClient debe apuntar a un ClientDef.Id de tipo ClientType.Embedder.
```

Ejemplo conceptual:

```text
RAGdef
 ├── Id = "rag-memory-docs"
 ├── EmbedderClient = "openai-embedder-main"
 ├── TopK = 5
 ├── MinScore = 0.65
 └── CollectionName = "documents"
```

---

## 6. AgentDef

`AgentDef` es la definición principal del agente.

La decisión de diseño más importante es que `AgentDef` solo mantiene el cliente conversacional principal:

```text
AgentDef.ChatClientId
```

El resto de capacidades se expresan como tools:

```text
AgentDef.Tools
```

Esto evita que `AgentDef` crezca con propiedades específicas como:

```text
EmbedderClientId
RagId
TextToImageClientId
ImageToTextClientId
OcrClientId
DocumentExtractorClientId
...
```

Responsabilidad:

```text
AgentDef = qué agente quiero construir.
```

Regla importante:

```text
AgentDef.ChatClientId debe apuntar a un ClientDef.Id de tipo ClientType.Chat.
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

---

## 7. AgentToolDef

`AgentToolDef` es la pieza clave de extensibilidad.

Cada capacidad opcional del agente se declara como una tool.

Responsabilidad:

```text
AgentToolDef = qué herramienta puede usar el agente y cómo se resuelve.
```

Uso esperado:

```text
Tool RagSearch   → usa RagId
Tool ImageToText → usa ClientId
Tool TextToImage → usa ClientId
Tool FileReader  → usa FileOptions y Settings
Tool FileWriter  → usa FileOptions y Settings
Tool Custom      → usa Settings, ClientId opcional, etc.
```

Reglas importantes:

```text
Settings = opciones de comportamiento.
ClientId = referencia opcional a ClientDef.
RagId    = referencia opcional a RAGdef.
```

Ejemplo conceptual:

```text
AgentToolDef
 ├── Id = "tool-rag-search"
 ├── ToolType = RagSearch
 ├── RagId = "rag-memory-docs"
 ├── Enabled = true
 └── Settings
     └── autoUse = true
```

---

## 8. AgentFileOptions

`AgentFileOptions` define cómo el agente recibe y devuelve archivos.

Responsabilidad:

```text
AgentFileOptions = política de entrada/salida de archivos.
```

Actualmente el transporte previsto es mediante strings, usando contenido base64.

Formato esperado de un archivo transportado:

```json
{
  "fileName": "imagen.png",
  "mimeType": "image/png",
  "encoding": "base64",
  "content": "<base64-content>"
}
```

Aunque `ChatQuery.Files` sea actualmente:

```csharp
public string[]? Files { get; set; }
```

internamente el runtime no debería trabajar directamente con strings, sino con un modelo fuerte de archivo.

Flujo recomendado:

```text
ChatQuery.Files:string[]
    ↓
HagentFileCodec.DecodeInputFiles()
    ↓
HagentFile[]
    ↓
Tools / modelo / RAG / procesamiento
    ↓
HagentFile[]
    ↓
HagentFileCodec.EncodeOutputFiles()
    ↓
ChatResponse.ResponseFiles:string[]
```

Reglas importantes:

```text
AllowInputFiles controla si se aceptan archivos de entrada.
AllowOutputFiles controla si se pueden devolver archivos.
MaxInputFileSizeBytes limita el tamaño de entrada.
MaxOutputFileSizeBytes limita el tamaño de salida.
AllowedInputMimeTypes limita MIME types aceptados.
AllowedOutputMimeTypes limita MIME types generables.
AutoProcessImages permite analizar imágenes automáticamente.
AutoProcessDocuments permite extraer texto de documentos automáticamente.
```

---

## 9. AgentExecutionOptions

`AgentExecutionOptions` controla el ciclo de ejecución del agente.

Responsabilidad:

```text
AgentExecutionOptions = límites y comportamiento del runtime.
```

Uso esperado:

- Limitar turnos.
- Limitar llamadas a tools.
- Aplicar timeout global.
- Decidir si se invocan tools automáticamente.
- Decidir si se devuelven mensajes intermedios.
- Definir política ante errores de tools.

Mapeo recomendado:

| Opción | Uso en runtime |
|---|---|
| `MaxTurns` | Número máximo de llamadas al modelo chat. |
| `MaxToolCalls` | Número máximo total de tools ejecutadas. |
| `TimeoutSeconds` | Timeout global de la consulta. |
| `AutoInvokeTools` | Permite o bloquea invocación automática de tools. |
| `ReturnToolMessages` | Incluye trazas de tools en `ChatResponse`. |
| `ReturnIntermediateSteps` | Incluye pasos intermedios del razonamiento operativo. |

Si `AutoInvokeTools` es `false`, `MaxToolCalls` puede ignorarse o considerarse `0` a nivel de ejecución automática.

---

## 10. Factoría GetHagent

La extensión:

```csharp
agentDef.GetHagent(configuration)
```

debe construir un `Hagent` completo a partir de configuración declarativa.

Responsabilidad:

```text
GetHagent = validar, resolver y componer el runtime.
```

`GetHagent` no debería ejecutar consultas. Solo debería preparar el agente.

Flujo recomendado:

```text
1. Recibir AgentDef + Configuration.

2. Validar referencias:
   ├── AgentDef.ChatClientId existe.
   ├── ChatClientId es ClientType.Chat.
   ├── ClientDef.Server existe.
   └── Cada tool habilitada tiene sus referencias necesarias.

3. Construir el cliente chat principal.

4. Crear estructura interna de Hagent:
   ├── cliente chat
   ├── system prompt
   ├── opciones de ejecución
   ├── opciones de archivos
   └── tools resueltas

5. Recorrer AgentDef.Tools habilitadas.

6. Para cada tool:
   ├── RagSearch
   │   ├── resolver RAGdef
   │   ├── resolver embedder del RAG
   │   └── crear tool RAG
   │
   ├── ImageToText
   │   ├── resolver ClientDef image-to-text
   │   └── crear tool de análisis de imagen
   │
   ├── TextToImage
   │   ├── resolver ClientDef text-to-image
   │   └── crear tool de generación de imagen
   │
   ├── FileReader
   │   └── crear tool de lectura/decodificación
   │
   ├── FileWriter
   │   └── crear tool de salida/codificación
   │
   └── Custom
       └── resolver mediante Settings

7. Devolver Hagent listo para Query() / QueryAsync().
```

Regla de diseño:

```text
Si Hagent ha sido creado correctamente, su configuración ya debe ser válida.
```

Por tanto, las validaciones estructurales pertenecen principalmente a `GetHagent`, no al runtime.

---

## 11. Validaciones recomendadas

La factoría debe fallar pronto con mensajes claros.

### Validaciones generales

```text
AgentDef.ChatClientId no puede estar vacío.
AgentDef.ChatClientId debe existir en ClientDefs.
AgentDef.ChatClientId debe ser ClientType.Chat.
Cada ClientDef.Server debe existir en ServerDefs.
```

### Validaciones para RagSearch

```text
RagId es requerido.
RagId debe existir en RaGdefs.
RAGdef.EmbedderClient debe existir.
RAGdef.EmbedderClient debe ser ClientType.Embedder.
```

### Validaciones para ImageToText

```text
ClientId es requerido.
ClientId debe existir en ClientDefs.
ClientId debe ser ClientType.ImageToText.
```

### Validaciones para TextToImage

```text
ClientId es requerido.
ClientId debe existir en ClientDefs.
ClientId debe ser ClientType.TextToImage.
```

### Validaciones para archivos

```text
Si AutoProcessImages = true:
    debería existir una tool ImageToText habilitada.

Si AutoProcessDocuments = true:
    debería existir FileReader o una tool documental compatible.

Si AllowOutputFiles = true:
    debería existir FileWriter o una estrategia de serialización de salida.

Si AllowInputFiles = false:
    ChatQuery.Files debería rechazarse o ignorarse explícitamente.
```

### Validaciones para ejecución

```text
MaxTurns debería ser mayor o igual a 1.
TimeoutSeconds debería ser mayor que 0.
Si AutoInvokeTools = false:
    MaxToolCalls puede ignorarse o considerarse 0 para ejecución automática.
```

---

## 12. Hagent runtime

`Hagent` es la fachada pública del agente ya construido.

Responsabilidad:

```text
Hagent = orquestador de ejecución.
```

No debería saber cómo se implementa cada capacidad concreta. Para eso existen las tools y los clientes internos.

Estructura conceptual:

```text
Hagent
 ├── API pública estable
 │   ├── Query()
 │   └── QueryAsync()
 │
 ├── Orquestación interna
 │   ├── preparar contexto
 │   ├── procesar archivos de entrada
 │   ├── decidir tools automáticas
 │   ├── ejecutar bucle chat/tools
 │   └── construir ChatResponse
 │
 └── Dependencias resueltas por la factoría
     ├── cliente chat principal
     ├── lista de IHagentTool
     ├── opciones de ejecución
     ├── opciones de archivos
     └── prompt de sistema
```

Diseño recomendado:

```text
Hagent
 ├── AgentDef
 ├── IHagentChatClient
 ├── IReadOnlyList<IHagentTool>
 ├── AgentFileOptions
 ├── AgentExecutionOptions
 ├── HagentFileCodec
 ├── Query()
 ├── QueryAsync()
 ├── ProcessInputFilesAsync()
 ├── ExecuteAutoToolsAsync()
 ├── ExecuteToolLoopAsync()
 └── BuildResponse()
```

---

## 13. Query y QueryAsync

La API principal recomendada es asíncrona:

```text
QueryAsync()
```

Motivo:

- Las llamadas a modelos son I/O.
- RAG puede depender de bases de datos o servicios externos.
- La generación de imágenes puede tardar.
- El procesamiento de archivos puede requerir operaciones asíncronas.
- El streaming futuro encaja mejor con una base async.

`Query()` puede existir como wrapper síncrono:

```text
Query()
 └── llama internamente a QueryAsync()
```

Flujo recomendado de `QueryAsync`:

```text
QueryAsync(query)
 ├── crear CancellationToken con timeout global
 ├── crear HagentExecutionContext
 ├── procesar archivos de entrada
 ├── ejecutar tools automáticas
 ├── ejecutar bucle chat/tools
 ├── construir ChatResponse
 └── devolver resultado
```

---

## 14. Contexto de ejecución

Cada consulta debería tener su propio contexto.

Responsabilidad:

```text
HagentExecutionContext = estado temporal de una llamada a QueryAsync.
```

Debe contener:

```text
HagentExecutionContext
 ├── ChatQuery original
 ├── AgentDef
 ├── AgentFileOptions
 ├── AgentExecutionOptions
 ├── InputFiles
 ├── OutputFiles
 ├── ResponseItems
 ├── TurnCount
 ├── ToolCallCount
 └── Items
```

`Items` permite compartir datos entre fases sin acoplar el runtime a casos concretos.

Ejemplos:

```text
Items["rag.context"]
Items["image.descriptions"]
Items["file.extractedText"]
Items["tool.errors"]
```

---

## 15. Tools internas

Las tools deben exponerse mediante una interfaz común.

Responsabilidad:

```text
IHagentTool = contrato ejecutable de una capacidad del agente.
```

Diseño conceptual:

```text
IHagentTool
 ├── Id
 ├── Name
 ├── ToolType
 ├── Enabled
 ├── CanHandle()
 ├── ShouldAutoInvokeAsync()
 └── ExecuteAsync()
```

Esto permite dos modos de uso.

### 15.1 Invocación automática

Ejemplos:

```text
AutoProcessImages = true
    ↓
Existe ImageToTextTool habilitada
    ↓
La imagen se analiza automáticamente
```

```text
RagSearch con Settings["autoUse"] = "true"
    ↓
Se consulta RAG antes o durante la llamada al chat
```

### 15.2 Invocación decidida por el modelo

Ejemplo:

```text
El modelo solicita una búsqueda documental
    ↓
Hagent crea un HagentToolCall
    ↓
Busca una IHagentTool compatible
    ↓
Ejecuta la tool
    ↓
Inyecta el resultado en el contexto
    ↓
Continúa el bucle
```

---

## 16. Tipos de tools esperados

### 16.1 RagSearch

Responsabilidad:

```text
Consultar un RAG/vector store y devolver contexto documental.
```

Usa:

```text
AgentToolDef.RagId
```

Salida esperada:

```text
Texto contextual
Metadatos
Scores
Referencias
```

---

### 16.2 ImageToText

Responsabilidad:

```text
Analizar imágenes de entrada y devolver descripción textual.
```

Usa:

```text
AgentToolDef.ClientId
```

Salida esperada:

```text
Descripción textual de una imagen
Metadatos del archivo analizado
```

---

### 16.3 TextToImage

Responsabilidad:

```text
Generar imágenes a partir de texto.
```

Usa:

```text
AgentToolDef.ClientId
```

Salida esperada:

```text
HagentFile con imagen generada
Texto opcional de confirmación
```

---

### 16.4 FileReader

Responsabilidad:

```text
Leer, decodificar o extraer texto de archivos.
```

Usa:

```text
AgentFileOptions
AgentToolDef.Settings
```

Salida esperada:

```text
Texto extraído
Metadatos del archivo
Errores de lectura si los hay
```

---

### 16.5 FileWriter

Responsabilidad:

```text
Preparar archivos de salida.
```

Usa:

```text
AgentFileOptions
AgentToolDef.Settings
```

Salida esperada:

```text
HagentFile listo para codificarse en ChatResponse.ResponseFiles
```

---

### 16.6 Custom

Responsabilidad:

```text
Permitir extensiones específicas sin modificar AgentDef ni Hagent.
```

Usa principalmente:

```text
AgentToolDef.Settings
```

Ejemplos de settings:

```text
toolSet
kind
handler
mode
```

---

## 17. Representación interna de archivos

Aunque el transporte público use strings, internamente se recomienda usar un modelo fuerte:

```text
HagentFile
 ├── FileName
 ├── MimeType
 ├── Encoding
 ├── Content
 └── Metadata
```

Y un envelope de transporte:

```text
HagentFileEnvelope
 ├── fileName
 ├── mimeType
 ├── encoding
 └── content
```

`HagentFileCodec` debe encargarse de:

```text
Decodificar archivos de entrada.
Validar tamaños.
Validar MIME types.
Codificar archivos de salida.
Generar envelopes JSON.
Convertir contenido binario a base64.
Convertir base64 a contenido binario.
```

Regla de diseño:

```text
Hagent no debería contener lógica base64 directamente.
```

---

## 18. ChatResponse

`ChatResponse` es el contrato de salida del agente.

Responsabilidad:

```text
ChatResponse = transportar texto, trazas y archivos.
```

Estructura actual:

```text
ChatResponse
 ├── ResponseItems
 └── ResponseFiles
```

`ResponseItems` permite devolver distintos tipos de mensajes:

```text
Think    → pasos internos o intermedios.
Tool     → trazas de herramientas.
Response → respuesta final al usuario.
```

Uso recomendado:

```text
ReturnIntermediateSteps = true
    → incluir elementos Think.

ReturnToolMessages = true
    → incluir elementos Tool.

Respuesta final
    → incluir siempre al menos un elemento Response.
```

`ResponseFiles` contiene archivos codificados según `AgentFileOptions`.

---

## 19. Flujo completo de ejecución

Flujo recomendado para una consulta:

```text
Usuario llama a Hagent.QueryAsync(ChatQuery)
    ↓
Hagent crea HagentExecutionContext
    ↓
HagentFileCodec decodifica ChatQuery.Files
    ↓
Se validan archivos de entrada
    ↓
Se ejecutan tools automáticas si procede
    ├── FileReader
    ├── ImageToText
    └── RagSearch
    ↓
Se construyen mensajes para el chat
    ├── SystemPrompt
    ├── Query del usuario
    ├── Contexto documental
    ├── Texto extraído de archivos
    └── Descripciones de imágenes
    ↓
IHagentChatClient llama al modelo conversacional
    ↓
Si el modelo solicita tools
    ├── Hagent ejecuta tools compatibles
    ├── añade resultados al contexto
    └── repite hasta límites configurados
    ↓
Se genera respuesta final
    ↓
HagentFileCodec codifica archivos de salida
    ↓
Se devuelve ChatResponse
```

---

## 20. Sesiones y conversación

Inicialmente `Hagent` puede ser stateless:

```text
Cada llamada a QueryAsync es independiente.
```

Ventajas:

- Más simple.
- Más fácil de probar.
- Menos problemas de concurrencia.
- Menos estado mutable.

Diseño futuro recomendado:

```text
ChatQuery
 └── SessionId opcional
```

Y una abstracción:

```text
IHagentSessionStore
 ├── GetMessagesAsync()
 └── AppendMessagesAsync()
```

Recomendación:

```text
Versión inicial: stateless.
Versión futura: sesiones opcionales.
```

---

## 21. Streaming futuro

Aunque no se implemente todavía, la arquitectura debe permitir streaming.

API futura posible:

```text
StreamAsync(ChatQuery)
    → IAsyncEnumerable<HagentStreamEvent>
```

Eventos posibles:

```text
TextDelta
ToolStarted
ToolCompleted
FileGenerated
Warning
Error
Completed
```

Regla de diseño:

```text
No mezclar streaming dentro de ChatResponse todavía.
```

`ChatResponse` representa una respuesta final completa. El streaming debería tener su propio modelo de eventos.

---

## 22. Separación de responsabilidades

Resumen final:

| Pieza | Responsabilidad |
|---|---|
| `Configuration` | Contenedor global de definiciones. |
| `ServerDef` | Define proveedores/endpoints. |
| `ClientDef` | Define capacidades IA concretas. |
| `RAGdef` | Define recuperación documental. |
| `AgentDef` | Define el agente y sus tools. |
| `AgentToolDef` | Declara una capacidad opcional. |
| `GetHagent` | Valida y compone el runtime. |
| `Hagent` | Orquesta la ejecución. |
| `IHagentChatClient` | Abstrae el cliente conversacional. |
| `IHagentTool` | Ejecuta una capacidad concreta. |
| `HagentFileCodec` | Codifica/decodifica archivos. |
| `HagentExecutionContext` | Mantiene el estado de una consulta. |
| `ChatResponse` | Devuelve texto, trazas y archivos. |

---

## 23. Decisiones arquitectónicas importantes

### 23.1 AgentDef no debe crecer con cada capacidad

Incorrecto:

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

Correcto:

```text
AgentDef
 ├── ChatClientId
 └── Tools
     ├── RagSearch
     ├── ImageToText
     ├── TextToImage
     ├── FileReader
     └── FileWriter
```

---

### 23.2 GetHagent valida; Hagent ejecuta

`GetHagent` debe encargarse de:

```text
Validar referencias.
Resolver servidores.
Resolver clientes.
Resolver RAG.
Construir tools.
Crear Hagent.
```

`Hagent` debe encargarse de:

```text
Recibir consultas.
Crear contexto.
Procesar archivos.
Ejecutar tools.
Llamar al chat.
Construir respuesta.
```

---

### 23.3 Las tools encapsulan capacidades

`Hagent` no debería tener lógica específica de:

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
IHagentTool
IHagentChatClient
HagentFileCodec
```

---

### 23.4 El runtime debe ser async-first

La API recomendada es:

```text
QueryAsync()
```

`Query()` puede mantenerse por comodidad, pero internamente debería delegar en `QueryAsync()`.

---

### 23.5 Los archivos deben normalizarse pronto

El runtime no debería pasar strings base64 por todas partes.

Flujo correcto:

```text
Entrada pública string[]
    ↓
Modelo interno HagentFile[]
    ↓
Procesamiento
    ↓
Salida pública string[]
```

---

## 24. Objetivo de la primera implementación

La primera implementación no necesita cubrir todos los proveedores ni todas las tools reales.

Objetivo inicial recomendado:

```text
1. Constructor interno de Hagent.
2. GetHagent con validación básica.
3. IHagentChatClient mínimo.
4. IHagentTool mínimo.
5. HagentExecutionContext.
6. HagentFileCodec básico.
7. QueryAsync con flujo preparado.
8. ChatResponse con ResponseItems y ResponseFiles.
```

Las tools pueden empezar como stubs funcionales y crecer después.

---

## 25. Resumen ejecutivo

La arquitectura propuesta permite construir agentes declarativos y extensibles:

```text
La configuración define capacidades.
La factoría resuelve dependencias.
Hagent ejecuta consultas.
Las tools encapsulan acciones.
Los clientes abstraen proveedores.
El codec normaliza archivos.
ChatResponse devuelve texto y archivos.
```

Esta separación permite añadir nuevas capacidades sin modificar el núcleo del agente.

Ejemplos de futuras extensiones:

```text
Nueva tool OCR.
Nueva tool AudioToText.
Nuevo proveedor compatible OpenAI.
Nuevo vector store.
Nuevo sistema de sesiones.
Nuevo streaming.
Nueva política de seguridad.
```

Principio final:

> `Hagent` debe ser pequeño, estable y orquestador.  
> Las capacidades deben vivir en tools intercambiables.  
> La configuración debe seguir siendo declarativa.
```
