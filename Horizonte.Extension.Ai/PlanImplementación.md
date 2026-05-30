## Plan de implementación para `Hagent`

Perfecto. Si ya están creadas las estructuras base, el siguiente paso debería ser implementar por capas, de forma incremental y testeable. La idea es evitar intentar conectar proveedores reales, RAG, tools y archivos a la vez. Primero dejamos el runtime funcionando con stubs, luego sustituimos piezas por implementaciones reales.

---

# 1. Objetivo de la primera implementación funcional

La primera versión debería permitir este flujo mínimo:

```text
Configuration + AgentDef
    ↓
agentDef.GetHagent(configuration)
    ↓
HagentFactory.Create(...)
    ↓
Validación + composición
    ↓
Hagent.QueryAsync(...)
    ↓
IHagentChatClient
    ↓
ChatResponse
```

Es decir:

- Crear un `Hagent` desde configuración.
- Validar referencias básicas.
- Resolver un cliente conversacional.
- Ejecutar una consulta.
- Devolver un `ChatResponse`.
- Dejar las tools preparadas, aunque inicialmente sean stubs.

---

# 2. Orden recomendado de implementación

## Fase 1 — Núcleo de composición

### 1.1. Implementar `HagentFactoryException`

Responsabilidad:

```text
Representar errores de configuración o composición.
```

Debe usarse para errores como:

- `AgentDef` nulo.
- `Configuration` nula.
- `ChatClientId` vacío.
- Cliente inexistente.
- Cliente no es de tipo `Chat`.
- Servidor inexistente.
- Tool mal configurada.

Resultado esperado:

```text
Los errores de creación del agente son claros y diferenciables de errores de ejecución.
```

---

### 1.2. Implementar `HagentFactoryContext`

Responsabilidad:

```text
Guardar el estado temporal durante la creación del agente.
```

Debe contener como mínimo:

```text
AgentDef
Configuration
ChatClientDef
ChatServerDef
IHagentChatClient
List<IHagentTool>
AgentFileOptions
AgentExecutionOptions
```

Resultado esperado:

```text
La factoría no trabaja con variables dispersas, sino con un contexto único.
```

---

### 1.3. Implementar `HagentFactoryExtensions.GetHagent`

Debe ser una extensión simple:

```text
agentDef.GetHagent(configuration)
    ↓
HagentFactory.Create(agentDef, configuration)
```

Regla importante:

```text
GetHagent no debe contener lógica de validación ni construcción.
```

Resultado esperado:

```text
API pública cómoda y limpia.
```

---

### 1.4. Implementar `HagentFactory.Create`

Esta es la pieza central de la primera fase.

Debe hacer:

```text
1. Validar AgentDef y Configuration.
2. Resolver ChatClientId.
3. Validar que el cliente sea Chat.
4. Resolver ServerDef del cliente.
5. Resolver FileOptions.
6. Resolver ExecutionOptions.
7. Crear IHagentChatClient.
8. Crear tools habilitadas.
9. Crear Hagent.
```

De momento, `CreateChatClient` puede devolver un cliente fake/stub.

Resultado esperado:

```text
HagentFactory ya compone un Hagent válido, aunque todavía no use proveedores reales.
```

---

# 3. Fase 2 — Contratos internos mínimos

## 2.1. Implementar `IHagentChatClient`

Debe ser una abstracción mínima para conversar con el modelo.

Contrato conceptual:

```text
SendAsync(...)
```

O algo equivalente según los modelos que ya tengáis definidos.

Debe permitir:

- Recibir prompt de sistema.
- Recibir mensaje del usuario.
- Recibir contexto adicional.
- Devolver texto de respuesta.
- En el futuro, devolver llamadas a tools.

Primera implementación recomendada:

```text
StubHagentChatClient
```

Que devuelva algo como:

```text
"Respuesta generada por StubHagentChatClient"
```

Esto permite probar todo el runtime sin depender todavía de OpenAI, Ollama u otro proveedor.

---

## 2.2. Implementar `IHagentTool`

Debe ser una interfaz común para todas las capacidades opcionales.

Contrato conceptual:

```text
Id
Name
ToolType
Enabled
CanHandle(...)
ShouldAutoInvokeAsync(...)
ExecuteAsync(...)
```

Primera implementación recomendada:

```text
StubHagentTool
```

Resultado esperado:

```text
La factoría puede crear tools y Hagent puede recorrerlas, aunque todavía no hagan trabajo real.
```

---

# 4. Fase 3 — Constructor y runtime de `Hagent`

## 3.1. Implementar constructor interno de `Hagent`

Debe recibir dependencias ya resueltas:

```text
AgentDef
IHagentChatClient
IReadOnlyList<IHagentTool>
AgentFileOptions
AgentExecutionOptions
```

Regla importante:

```text
Hagent no debe validar referencias globales.
```

Eso ya lo hace la factoría.

`Hagent` sí puede validar argumentos internos básicos, por ejemplo:

- `chatClient` no nulo.
- `tools` no nulo.
- `fileOptions` no nulo.
- `executionOptions` no nulo.

---

## 3.2. Implementar `QueryAsync`

Primera versión mínima:

```text
QueryAsync(ChatQuery query)
    ↓
Validar query
    ↓
Crear HagentExecutionContext
    ↓
Procesar archivos de entrada, inicialmente vacío/stub
    ↓
Ejecutar tools automáticas, inicialmente stub
    ↓
Llamar a IHagentChatClient
    ↓
Construir ChatResponse
```

Resultado esperado:

```text
Una consulta ya recorre el flujo real del agente.
```

---

## 3.3. Implementar `Query`

Debe ser un wrapper síncrono:

```text
Query(query)
    ↓
QueryAsync(query).GetAwaiter().GetResult()
```

Recomendación:

- Mantenerlo por comodidad.
- Documentar que la API principal es `QueryAsync`.

---

# 5. Fase 4 — Contexto de ejecución

## 4.1. Implementar `HagentExecutionContext`

Debe representar el estado de una consulta concreta.

Campos recomendados:

```text
ChatQuery OriginalQuery
AgentDef AgentDef
AgentFileOptions FileOptions
AgentExecutionOptions ExecutionOptions
List<HagentFile> InputFiles
List<HagentFile> OutputFiles
List<ChatResponseItem> ResponseItems
int TurnCount
int ToolCallCount
Dictionary<string, object?> Items
CancellationToken CancellationToken
```

Resultado esperado:

```text
Cada QueryAsync tiene su propio estado, sin contaminar otras consultas.
```

Esto es importante para que `Hagent` sea seguro en escenarios concurrentes.

---

# 6. Fase 5 — Validaciones de opciones

## 5.1. Validar `AgentExecutionOptions`

Reglas mínimas:

```text
MaxTurns >= 1
TimeoutSeconds > 0
MaxToolCalls >= 0
```

Además:

```text
Si AutoInvokeTools = false,
las tools automáticas no deben ejecutarse.
```

Resultado esperado:

```text
El runtime nunca recibe opciones de ejecución inválidas.
```

---

## 5.2. Validar `AgentFileOptions`

Reglas mínimas:

```text
MaxInputFileSizeBytes >= 0
MaxOutputFileSizeBytes >= 0
```

Reglas funcionales:

```text
Si AutoProcessImages = true,
debe existir una tool ImageToText habilitada.

Si AutoProcessDocuments = true,
debe existir FileReader o tool compatible.

Si AllowOutputFiles = true,
debe existir FileWriter o estrategia de salida.
```

Para la primera versión puedes separar:

```text
Validaciones estrictas iniciales:
- tamaños
- nulls
- MIME collections

Validaciones funcionales posteriores:
- dependencia entre AutoProcessImages y ImageToText
- dependencia entre AutoProcessDocuments y FileReader
```

Así no bloqueas la primera integración.

---

# 7. Fase 6 — Codec de archivos

## 6.1. Implementar `HagentFile`

Modelo interno fuerte:

```text
FileName
MimeType
Encoding
Content
Metadata
```

Preferiblemente `Content` debería ser binario:

```text
byte[]
```

No conviene mover base64 por todo el runtime.

---

## 6.2. Implementar `HagentFileEnvelope`

Modelo de transporte JSON:

```text
fileName
mimeType
encoding
content
```

Donde `content` será base64.

---

## 6.3. Implementar `HagentFileCodec`

Métodos mínimos:

```text
DecodeInputFiles(...)
EncodeOutputFiles(...)
ValidateInputFile(...)
ValidateOutputFile(...)
```

Debe encargarse de:

```text
Base64 → byte[]
byte[] → Base64
Validar tamaño
Validar MIME type
Serializar/deserializar JSON
```

Primera versión recomendada:

```text
Soportar solo encoding = "base64".
```

Resultado esperado:

```text
Hagent nunca manipula base64 directamente.
```

---

# 8. Fase 7 — Ejecución automática de tools

Implementar en `Hagent` un método interno parecido conceptualmente a:

```text
ExecuteAutoToolsAsync(context)
```

Debe hacer:

```text
Si AutoInvokeTools = false:
    no ejecutar nada.

Si AutoInvokeTools = true:
    recorrer tools habilitadas.
    preguntar ShouldAutoInvokeAsync.
    validar MaxToolCalls.
    ejecutar ExecuteAsync.
    guardar resultados en context.Items o ResponseItems.
```

Primera versión recomendada:

- Ejecutar solo tools stub.
- Incrementar `ToolCallCount`.
- Añadir trazas si `ReturnToolMessages = true`.

Resultado esperado:

```text
El runtime ya tiene la mecánica de tools aunque las capacidades sean básicas.
```

---

# 9. Fase 8 — Bucle chat/tools

Esta fase puede implementarse en dos niveles.

## Nivel 1 — Sin tool-calling del modelo

Primera versión:

```text
1. Ejecutar tools automáticas.
2. Construir contexto textual.
3. Llamar una vez al chat.
4. Devolver respuesta.
```

Esto es suficiente para:

- RAG previo.
- Archivos procesados previamente.
- Descripciones de imágenes previas.
- Tools automáticas.

---

## Nivel 2 — Tool-calling iterativo

Versión posterior:

```text
while TurnCount < MaxTurns:
    llamar al chat
    si respuesta final:
        terminar
    si pide tools:
        ejecutar tools
        añadir resultados
        continuar
```

Recomendación:

```text
No implementar tool-calling iterativo real hasta tener estable el flujo simple.
```

Así evitas complicar demasiado el primer corte.

---

# 10. Fase 9 — Tools reales progresivas

Una vez que el runtime funcione con stubs, implementar tools reales en este orden:

## 9.1. `FileReaderTool`

Prioridad alta porque no depende de proveedores IA.

Responsabilidad:

```text
Extraer texto básico de archivos soportados.
```

Primera versión:

```text
text/plain
application/json
text/markdown
```

Después:

```text
PDF
DOCX
HTML
```

---

## 9.2. `FileWriterTool`

Responsabilidad:

```text
Preparar archivos de salida.
```

Primera versión:

```text
Generar archivos text/plain o application/json.
```

---

## 9.3. `RagSearchTool`

Responsabilidad:

```text
Consultar contexto documental.
```

Primera versión recomendada:

```text
InMemory
```

Después:

```text
SQLite
Postgres
Qdrant
Azure AI Search
Elasticsearch
```

---

## 9.4. `ImageToTextTool`

Responsabilidad:

```text
Analizar imágenes.
```

Depende de cliente IA compatible.

---

## 9.5. `TextToImageTool`

Responsabilidad:

```text
Generar imágenes.
```

Debe devolver `HagentFile` en `OutputFiles`.

---

## 9.6. `CustomTool`

Responsabilidad:

```text
Extensión por Settings.
```

Recomendación:

```text
Implementarla al final, cuando el contrato IHagentTool esté estable.
```

---

# 11. Fase 10 — Clientes reales

El `IHagentChatClient` debería tener implementaciones progresivas.

Orden recomendado:

## 10.1. `StubHagentChatClient`

Para tests y desarrollo.

## 10.2. Cliente compatible con `Microsoft.Extensions.AI`

Dado que el proyecto ya apunta a .NET moderno y usa paquetes de IA de Microsoft, esta puede ser la integración más limpia.

Objetivo:

```text
IHagentChatClient
    ↓
Microsoft.Extensions.AI.IChatClient
```

## 10.3. Cliente OpenAI

Usando configuración de:

```text
ServerDef
ClientDef
```

## 10.4. Cliente compatible OpenAI genérico

Para servidores tipo:

```text
Ollama
LM Studio
vLLM
LocalAI
otros endpoints compatibles
```

---

# 12. Fase 11 — Testing

## 11.1. Tests de factoría

Casos mínimos:

```text
AgentDef null lanza HagentFactoryException.
Configuration null lanza HagentFactoryException.
ChatClientId vacío lanza excepción.
ChatClientId inexistente lanza excepción.
ChatClientId no Chat lanza excepción.
ServerDef inexistente lanza excepción.
Options inválidas lanzan excepción.
Agent válido crea Hagent.
```

---

## 11.2. Tests de runtime

Casos mínimos:

```text
QueryAsync devuelve ChatResponse.
Query llama a QueryAsync.
Timeout se respeta.
MaxTurns se respeta.
AutoInvokeTools false no ejecuta tools.
AutoInvokeTools true ejecuta tools automáticas.
ReturnToolMessages controla trazas.
ReturnIntermediateSteps controla pasos internos.
```

---

## 11.3. Tests de archivos

Casos mínimos:

```text
DecodeInputFiles decodifica base64 válido.
DecodeInputFiles rechaza JSON inválido.
DecodeInputFiles rechaza encoding no soportado.
DecodeInputFiles rechaza MIME no permitido.
DecodeInputFiles rechaza tamaño excedido.
EncodeOutputFiles genera JSON válido.
EncodeOutputFiles genera base64 válido.
```

---

## 11.4. Tests de tools

Casos mínimos:

```text
Tool deshabilitada no se ejecuta.
Tool incrementa ToolCallCount.
MaxToolCalls se respeta.
Errores de tools se manejan según política configurada.
```

---

# 13. Secuencia práctica de desarrollo

Yo lo implementaría exactamente en este orden:

```text
1. HagentFactoryException
2. HagentFactoryContext
3. HagentFactoryExtensions
4. HagentFactory.Create básico
5. IHagentChatClient
6. StubHagentChatClient
7. IHagentTool
8. StubHagentTool
9. Constructor interno de Hagent
10. HagentExecutionContext
11. QueryAsync mínimo
12. Query wrapper
13. ChatResponse final mínimo
14. Validaciones de ExecutionOptions
15. Validaciones de FileOptions
16. HagentFile
17. HagentFileEnvelope
18. HagentFileCodec
19. ExecuteAutoToolsAsync
20. FileReaderTool simple
21. FileWriterTool simple
22. RAG InMemory básico
23. Cliente IA real
24. ImageToTextTool
25. TextToImageTool
26. Tool-calling iterativo
27. Streaming futuro
```

---

# 14. Primer hito recomendado

El primer hito debería ser:

```text
Crear un Hagent desde configuración y ejecutar QueryAsync usando StubHagentChatClient.
```

Criterio de aceptación:

```text
agentDef.GetHagent(configuration)
    .QueryAsync(new ChatQuery { ... })
```

Debe devolver:

```text
ChatResponse
 ├── ResponseItems
 │   └── Response final
 └── ResponseFiles vacío
```

Sin tools reales todavía.

---

# 15. Segundo hito recomendado

Añadir tools stub y ejecución automática.

Criterio de aceptación:

```text
Si AgentDef.Tools contiene una tool habilitada con autoUse=true,
Hagent la ejecuta antes del chat.
```

Y si está activado:

```text
ReturnToolMessages = true
```

Entonces la respuesta debe incluir trazas tipo `Tool`.

---

# 16. Tercer hito recomendado

Añadir archivos básicos.

Criterio de aceptación:

```text
ChatQuery.Files:string[]
    ↓
HagentFileCodec.DecodeInputFiles()
    ↓
HagentExecutionContext.InputFiles
    ↓
FileReaderTool
    ↓
context.Items["file.extractedText"]
    ↓
IHagentChatClient
    ↓
ChatResponse
```

Primero solo para texto plano, JSON y Markdown.

---

# 17. Cuarto hito recomendado

Añadir RAG básico `InMemory`.

Criterio de aceptación:

```text
RagSearchTool
    ↓
consulta colección en memoria
    ↓
inyecta contexto documental
    ↓
chat responde usando ese contexto
```

No hace falta empezar con Qdrant, Postgres o Azure Search. Primero que la arquitectura respire; luego ya le ponemos turbo.

---

# 18. Recomendación general

No implementaría todavía:

```text
Streaming
Sesiones persistentes
Tool-calling iterativo complejo
Múltiples proveedores reales
RAG externo
OCR avanzado
PDF/DOCX avanzado
```

Primero cerraría este núcleo:

```text
Factory sólida
Runtime async
ChatClient stub
Tools stub
FileCodec básico
ChatResponse estable
Tests
```

Después cada capacidad real será mucho más fácil de enchufar.

---

## Resumen corto

El plan ideal es:

```text
1. Composición: HagentFactory.
2. Contratos: IHagentChatClient e IHagentTool.
3. Runtime: Hagent.QueryAsync.
4. Estado por consulta: HagentExecutionContext.
5. Archivos: HagentFileCodec.
6. Tools automáticas.
7. Tools reales simples.
8. Cliente IA real.
9. RAG real.
10. Tool-calling avanzado y streaming.
```

La clave es que la primera versión sea **funcional de punta a punta**, aunque internamente use stubs. Eso permite validar la arquitectura completa antes de invertir tiempo en proveedores concretos.

kotlin.Unit

