# Architecture of Hagent

Hagent is a declarative runtime for creating configurable agents, executing queries, processing files, invoking tools, and delegating conversational generation to interchangeable AI clients.

The core design principle is:

> Configuration declares what capabilities exist; the factory resolves them; `Hagent` orchestrates them at runtime.

---

## 1. Stable Architecture

This section describes architectural decisions that should be considered stable. The concrete implementation may evolve, but these rules must be maintained to preserve a clean, testable, and extensible architecture.

---

### 1.1. Composition Principle

The main execution flow is:

```text
Configuration + AgentDef
    ↓
agentDef.GetHagent(configuration)
    ↓
HagentFactory.Create(agentDef, configuration)
    ↓
Validation + resolution + composition
    ↓
Hagent
    ↓
Query() / QueryAsync()
    ↓
HagentExecutionContext
    ↓
Files + automatic tools + chat
    ↓
ChatResponse
```

Primary rule:

```text
Hagent must not know how servers, clients, RAGs, or providers are resolved.
Hagent receives dependencies already resolved by HagentFactory.
```

Responsibilities are divided as follows:

```text
Configuration contains servers, clients, RAGs, and agents.
AgentDef describes an agent.
ServerDef describes where a capability is hosted/served.
ClientDef describes which AI model or capability is used.
RaGdef describes how to retrieve document context.
AgentToolDef describes an optional agent tool.

HagentFactory validates and composes the runtime.
HagentFactoryContext holds temporary composition state.
HagentFactoryException represents configuration/composition errors.

Hagent executes queries.
HagentExecutionContext maintains state for a specific query.
HagentToolContext provides controlled context to tools.

IHagentChatClient abstracts the conversational client.
IHagentTool abstracts optional capabilities.
IHagentImageToTextClient abstracts image analysis.
IHagentTextToImageClient abstracts image generation.

HagentFileCodec normalizes files.
ChatResponse transports the final response.
```

---

### 1.2. Configuration Model

The global configuration is organized as follows:

```text
Configuration
 ├── ServerDefs
 ├── ClientDefs
 ├── RaGdefs
 └── AgentDefs
```

| Block | Responsibility |
|---|---|
| `ServerDef` | Defines providers, endpoints, or servers. |
| `ClientDef` | Defines a concrete AI capability served by a server. |
| `RaGdef` | Defines a RAG / vector store configuration. |
| `AgentDef` | Defines the agent, system prompt, primary chat client, tools, and options. |

Rule:

```text
Configuration is declarative.
Actual reference resolution belongs to HagentFactory.
```

---

### 1.3. ServerDef

`ServerDef` represents a provider, server, or endpoint.

Conceptual examples:

```text
OpenAI
Azure OpenAI
Ollama
LM Studio
vLLM
LocalAI
Local server
OpenAI-compatible provider
Custom provider
```

Responsibility:

```text
ServerDef = where a capability is executed or consumed.
```

Rules:

```text
ClientDef.Server must point to ServerDef.Id.
ServerDef.Provider allows inferring integration type.
ServerDef.Settings allows declaring implementation details without changing the model.
```

Conceptual example:

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

`ClientDef` represents a concrete AI capability.

Examples:

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

Responsibility:

```text
ClientDef = which model or capability to use.
```

Rules:

```text
ClientDef.Server must exist in Configuration.ServerDefs.
ClientDef.ClientType defines the capability type.
ClientDef.Settings can specify the concrete implementation.
```

Examples:

```text
Primary chat      → ClientType.Chat
RAG Embeddings    → ClientType.Embedder
Image analysis    → ClientType.ImageToText
Image generation  → ClientType.TextToImage
```

---

### 1.5. RaGdef

`RaGdef` represents a RAG / vector store configuration.

Responsibility:

```text
RaGdef = how to retrieve document context.
```

Rules:

```text
RaGdef.EmbedderClient must point to an existing ClientDef of type ClientType.Embedder.
The RAG tool decides how to use that definition at runtime.
```

Conceptual example:

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

In the initial implementation, `RagSearchTool` can use simple lexical search even if `EmbedderClient` is already validated. This keeps the contract ready for future vector RAG without blocking the current functional flow.

---

### 1.6. AgentDef

`AgentDef` is the primary agent definition.

Key decision:

```text
AgentDef only holds the primary conversational client via ChatClientId.
All other capabilities are declared as tools.
```

Conceptual structure:

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

Rule:

```text
AgentDef.ChatClientId must point to a ClientDef.Id of type ClientType.Chat.
```

Correct design:

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

Design to avoid:

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

Extensibility rule:

```text
New capabilities must enter as tools or specialized clients, not as new direct properties on AgentDef.
```

---

### 1.7. AgentToolDef

`AgentToolDef` declares an optional agent capability.

Responsibility:

```text
AgentToolDef = what tool the agent can use and how it is resolved.
```

Expected usage:

```text
RagSearch   → uses RagId
ImageToText → uses ClientId
TextToImage → uses ClientId
FileReader  → uses FileOptions and Settings
FileWriter  → uses FileOptions and Settings
Custom      → uses Settings
```

Conceptual fields:

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

Rules:

```text
An enabled tool must be validated by HagentFactory.
A tool can auto-execute if its implementation permits.
Settings["autoUse"] = "true" or "always" can enable auto-execution.
```

---

### 1.8. AgentFileOptions

`AgentFileOptions` defines the input/output file policy.

Responsibility:

```text
AgentFileOptions = rules regarding input and output files.
```

Relevant options:

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

Strict rules:

```text
MaxInputFileSizeBytes >= 0
MaxOutputFileSizeBytes >= 0
AllowedInputMimeTypes cannot be null
AllowedOutputMimeTypes cannot be null
Settings cannot be null
TransportEncoding cannot be empty
AllowedInputMimeTypes must not contain empty values
AllowedOutputMimeTypes must not contain empty values
```

Optional functional rules:

```text
AutoProcessImages = true
    → should have enabled ImageToText.

AutoProcessDocuments = true
    → should have FileReader or compatible document tool.

AllowOutputFiles = true
    → should have FileWriter or compatible output strategy.
```

To avoid blocking initial integrations, these functional validations can be activated via:

```text
AgentDef.Settings["validateFileOptionDependencies"] = "true"
```

---

### 1.9. AgentExecutionOptions

`AgentExecutionOptions` controls runtime limits and behavior.

Relevant options:

```text
MaxTurns
MaxToolCalls
TimeoutSeconds
AutoInvokeTools
ReturnToolMessages
ReturnIntermediateSteps
Settings
```

Minimum validations:

```text
MaxTurns >= 1
TimeoutSeconds > 0
MaxToolCalls >= 0
```

Runtime rules:

```text
If AutoInvokeTools = false:
    ExecuteAutoToolsAsync executes no automatic tools.

If MaxToolCalls = 0:
    ExecuteAutoToolsAsync also executes no automatic tools.

ReturnToolMessages = true:
    tool messages/traces are appended to ChatResponse.ResponseItems.
```

---

### 1.10. HagentFactory

`HagentFactory` is the single point of composition.

Responsibility:

```text
Validate configuration.
Resolve references.
Create chat client.
Create auxiliary tool clients.
Create tools.
Create Hagent.
```

Stable flow:

```text
1. Validate AgentDef is not null.
2. Validate Configuration is not null.
3. Resolve AgentDef.ChatClientId.
4. Validate that ClientDef is ClientType.Chat.
5. Resolve ServerDef associated with ClientDef.
6. Resolve AgentFileOptions.
7. Resolve AgentExecutionOptions.
8. Create HagentFactoryContext.
9. Validate AgentFileOptions.
10. Validate AgentExecutionOptions.
11. Create IHagentChatClient.
12. Resolve enabled tools.
13. Create Hagent.
```

Rule:

```text
If HagentFactory.Create returns a Hagent, its structural configuration must already be valid.
```

`HagentFactory` may select real or stub implementations, but `Hagent` must not be aware of that decision.

---

### 1.11. HagentFactoryContext

`HagentFactoryContext` holds temporary state during agent creation.

Contains:

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

Objective:

```text
Avoid scattered variables in HagentFactory.
Centralize dependencies resolved during composition.
```

---

### 1.12. HagentFactoryException

`HagentFactoryException` represents configuration or composition errors.

Objective:

```text
Differentiate agent creation errors from runtime execution errors.
```

Used for:

```text
AgentDef null.
Configuration null.
Empty ChatClientId.
Non-existent client.
Client not Chat.
Non-existent server.
Misconfigured tool.
Invalid options.
Non-existent RAG.
RAG not supported by real tool.
Non-existent tool client.
Tool client with incorrect ClientType.
Unsupported chat implementation.
Unresolvable provider.
```

---

### 1.13. IHagentChatClient

`IHagentChatClient` abstracts the conversational client.

Contract:

```text
CompleteAsync(HagentChatRequest request, CancellationToken cancellationToken)
    ↓
HagentChatResult
```

`HagentChatRequest` allows sending:

```text
Messages
Tools
Options
```

`HagentChatResult` returns:

```text
Text
ToolCalls
Metadata
```

Usage:

```text
Hagent builds HagentChatRequest.
IHagentChatClient returns HagentChatResult.
Hagent converts result.Text into ChatResponseItem.Response.
```

Rule:

```text
Hagent only knows IHagentChatClient.
It does not know OpenAI, Ollama, LM Studio, vLLM, or Microsoft.Extensions.AI.
```

---

### 1.14. Auxiliary AI Clients

In addition to primary chat, some tools use dedicated clients.

#### IHagentImageToTextClient

Responsibility:

```text
Abstract a client capable of analyzing images and returning text.
```

Conceptual contract:

```text
DescribeAsync(HagentImageToTextRequest request, CancellationToken cancellationToken)
    ↓
HagentImageToTextResult
```

Usage:

```text
ImageToTextTool
    ↓
IHagentImageToTextClient.DescribeAsync
    ↓
context.Items["image.descriptions"]
```

#### IHagentTextToImageClient

Responsibility:

```text
Abstract a client capable of generating images from text.
```

Conceptual contract:

```text
GenerateAsync(HagentTextToImageRequest request, CancellationToken cancellationToken)
    ↓
HagentTextToImageResult
```

Usage:

```text
TextToImageTool
    ↓
IHagentTextToImageClient.GenerateAsync
    ↓
HagentToolResult.Files
    ↓
context.OutputFiles
```

Rule:

```text
Multimodal tools must not know concrete providers.
They must depend on interfaces.
```

---

### 1.15. IHagentTool

`IHagentTool` abstracts an optional agent capability.

Contract:

```text
Id
Name
ToolType
Enabled
CanHandle(...)
ShouldAutoInvokeAsync(...)
ExecuteAsync(...)
```

Usage:

```text
ExecuteAutoToolsAsync iterates enabled tools.
Asks ShouldAutoInvokeAsync.
Executes ExecuteAsync if applicable.
Applies HagentToolResult to context.
```

Rule:

```text
Agent capabilities reside in interchangeable tools.
Hagent must not contain specific logic for each capability.
```

---

### 1.16. Hagent

`Hagent` is the runtime and orchestrator.

Responsibility:

```text
Receive queries.
Create context per query.
Process files.
Execute automatic tools.
Build textual context.
Call chat client.
Build ChatResponse.
```

It must not validate global references such as:

```text
ChatClientId exists.
ClientDef is Chat.
ServerDef exists.
RAG exists.
Embedder exists.
Tool points to valid client.
```

Those belong to `HagentFactory`.

It can validate internal arguments:

```text
agentDef not null.
chatClient not null.
tools not null.
fileOptions not null.
executionOptions not null.
query not null.
query.Query not empty.
```

Conceptual constructor:

```csharp
internal Hagent(
    AgentDef agentDef,
    IHagentChatClient chatClient,
    IReadOnlyList<IHagentTool> tools,
    AgentFileOptions fileOptions,
    AgentExecutionOptions executionOptions)
```

Concurrency rule:

```text
Hagent must be reusable across queries.
Each QueryAsync creates its own HagentExecutionContext.
```

---

### 1.17. Query and QueryAsync

The primary API is:

```csharp
Task<ChatResponse> QueryAsync(ChatQuery query, CancellationToken cancellationToken = default)
```

`Query` exists as a synchronous wrapper:

```text
Query(query)
    ↓
QueryAsync(query).GetAwaiter().GetResult()
```

Rule:

```text
The recommended primary API is QueryAsync.
```

Stable flow:

```text
1. Validate query.
2. Create linked CancellationTokenSource with timeout.
3. Create HagentExecutionContext.
4. Process input files.
5. Execute automatic tools.
6. Execute chat/tools pipeline.
7. Build ChatResponse.
```

---

### 1.18. HagentExecutionContext

`HagentExecutionContext` represents the state of a specific query.

Conceptual fields:

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

Objective:

```text
Each QueryAsync has its own isolated state.
Hagent can be reused concurrently without contaminating queries.
```

`Items` allows sharing data between phases:

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

`HagentToolContext` is provided to tools during execution.

Contains:

```text
AgentId
UserQuery
InputFiles
OutputFiles
FileOptions
ExecutionOptions
Items
```

Objective:

```text
Provide tool access to necessary operational state without directly exposing the full Hagent instance.
```

Usage by tool:

```text
FileReaderTool   → reads InputFiles and writes file.extractedText.
FileWriterTool   → generates files into HagentToolResult.Files.
RagSearchTool    → reads UserQuery and writes rag.context.
ImageToTextTool  → reads InputFiles and writes image.descriptions.
TextToImageTool  → reads UserQuery/Settings and returns images.
CustomHagentTool → reads/writes Items according to Settings.
```

---

### 1.20. File Model

#### HagentFile

`HagentFile` is the internal file model in memory.

```text
HagentFile
 ├── FileName
 ├── MimeType
 └── Bytes (byte[])
```

Rule:

```text
The runtime does not transport Base64 internally.
```

#### HagentFileEnvelope

`HagentFileEnvelope` is the JSON transport model.

Format:

```json
{
  "fileName": "image.png",
  "mimeType": "image/png",
  "encoding": "base64",
  "content": "<base64-content>"
}
```

Rule:

```text
Base64 exists only at the public boundary.
```

#### HagentFileCodec

`HagentFileCodec` converts between public transport and internal models.

Responsibilities:

```text
JSON → HagentFileEnvelope
HagentFileEnvelope → HagentFile
Base64 → byte[]
byte[] → Base64
Validate input file size
Validate output file size
Validate input MIME type
Validate output MIME type
Serialize output JSON
Deserialize input JSON
```

Rules:

```text
If files null or empty:
    DecodeInputFiles returns empty list.

If AllowInputFiles = false and files arrive:
    throw error.

If AllowOutputFiles = false and output files exist:
    throw error.

If AllowedInputMimeTypes is empty:
    allow any MIME.

If AllowedOutputMimeTypes is empty:
    allow any MIME.
```

---

### 1.21. Automatic Tool Execution

`ExecuteAutoToolsAsync` implements basic mechanics for automatic tools.

Flow:

```text
1. Verify cancellation token.
2. If AutoInvokeTools = false:
       do not execute tools.
3. If MaxToolCalls = 0:
       do not execute tools.
4. Create HagentToolContext.
5. Iterate enabled tools.
6. Validate MaxToolCalls.
7. Query ShouldAutoInvokeAsync.
8. Execute ExecuteAsync.
9. Increment ToolCallCount.
10. Apply HagentToolResult.
```

`ApplyToolResult` performs:

```text
Save result in Items["tool.{toolId}.result"].
Add result.Files to OutputFiles.
Add traces to ResponseItems if ReturnToolMessages = true.
```

---

### 1.22. Chat/Tools Loop

The design supports two tiers.

#### Tier 1: Simple Chat with Prior Context

Flow:

```text
1. ProcessInputFilesAsync.
2. ExecuteAutoToolsAsync.
3. BuildTextualContext.
4. BuildChatRequest.
5. Call IHagentChatClient once.
6. Add result.Text as ChatResponseItem.Response.
7. Record model ToolCalls if present, but do not execute them.
```

This tier supports:

```text
Prior RAG.
Pre-processed files.
Prior image descriptions.
Prior file generation.
Prior image generation.
Automatic tools.
Tool results injected as textual context.
```

#### Tier 2: Iterative Tool-Calling

Future design:

```text
while TurnCount < MaxTurns:
    call chat client
    if final response exists:
        terminate
    if tool calls exist:
        match compatible tools
        execute tools
        append results to context
        continue
```

Current rule:

```text
Tier 2 may be prepared, but must not complicate base flow until Tier 1 is fully stable.
```

---

### 1.23. ChatResponse

`ChatResponse` transports the final response.

Structure:

```text
ResponseItems
ResponseFiles
```

`ResponseItems` can contain:

```text
Think
Tool
Response
```

Usage:

```text
Response → final chat answer.
Tool     → tool execution traces if ReturnToolMessages = true.
Think    → reserved for future intermediate thinking steps.
```

Rule:

```text
BuildResponse guarantees that at least one ResponseItem of type Response exists, even if empty.
```

`ResponseFiles` is generated via:

```text
HagentFileCodec.EncodeOutputFiles(context.OutputFiles, fileOptions)
```

---

### 1.24. Immutable Architectural Decisions

#### Factory Composes, Hagent Executes

```text
HagentFactory:
    validates configuration
    resolves references
    creates dependencies
    creates Hagent

Hagent:
    executes queries
    creates context per query
    processes files
    executes tools
    calls chat client
    constructs response
```

#### AgentDef Does Not Expand Per Capability

```text
New capabilities must be added as tools,
not as new properties directly in AgentDef.
```

#### Files Are Normalized Early

```text
ChatQuery.Files:string[]
    ↓
HagentFileCodec
    ↓
HagentFile[]
```

After that point, the runtime works with `byte[]`, never Base64.

#### Tools Encapsulate Capabilities

`Hagent` must not contain provider-specific logic for:

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

It must delegate to:

```text
IHagentChatClient
IHagentTool
IHagentImageToTextClient
IHagentTextToImageClient
HagentFileCodec
```

#### Async-First

```text
The primary API is QueryAsync.
Query exists for convenience.
```

#### Declarative and Safe CustomTool

`CustomHagentTool` must not become arbitrary code execution.

Initial supported capabilities:

```text
echo
template
contextItem
setContextItem
```

Recommended future extensions:

```text
ICustomToolHandlerRegistry
Explicit handlers
No unrestricted reflection
No default OS system commands
Configurable security policies
Execution auditing
```

#### Interchangeable Real AI Clients

Correct design:

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
OpenAI / Azure / compatible provider / Stub
```
