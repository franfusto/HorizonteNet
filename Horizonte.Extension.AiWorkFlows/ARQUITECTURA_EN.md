### Architecture of Horizonte.Extension.AiWorkFlows

The **Horizonte.Extension.AiWorkFlows** library implements the intelligent workflow orchestration and directed graph execution layer for the HorizonteNet ecosystem. It integrates the capabilities of artificial intelligence agents and workflow engines based on `Microsoft.Agents.AI.Workflows` with the decoupled command system (`IHGesCom`), credential management (`IHCredManager`), and semantic vector tool search using OpenAI embeddings and `Microsoft.Extensions.VectorData`.

This document describes the technical architecture of the subsystem, workflow definition models, execution lifecycle and control (pause, resume, cancellation, and checkpoints), the toolset exposed to AI agents, and semantic command indexing.

---

### Architecture Overview

`Horizonte.Extension.AiWorkFlows` allows designing, compiling, and executing processing graphs where each node represents an executable action or command in HorizonteNet. Through this model, workflows can coordinate calls to Large Language Models (LLMs), invoke synchronous or asynchronous system commands, evaluate dynamic branching conditions, and tolerantly manage failures, cancellations, and interruptions.

#### Component and Integration Overview

```
+----------------------------------------------------------------------------------------------------+
|                                    WORKFLOW DEFINITION                                             |
|                  (WorkFlowDef: Nodes, Links, Ports, Conditions, Processors)                         |
+--------------------------------------------------+-------------------------------------------------+
                                                   |
                                                   v
+----------------------------------------------------------------------------------------------------+
|                               COMPILATION AND BUILD ENGINE                                         |
|                                                                                                    |
|    +------------------------------------------------------------------------------------------+    |
|    |                             Extensions.Build<T>()                                        |    |
|    |                                                                                          |    |
|    |   1. Locate initial node (Type == WorkFlowNodeTypes.In)                                  |    |
|    |   2. Instantiate executors (HGesComExecutor<T> for each node)                            |    |
|    |   3. Configure WorkflowBuilder and register root node                                    |    |
|    |   4. Construct edges (AddEdge) with conditions evaluated by IHGesCom                     |    |
|    |   5. Assign output nodes (WithOutputFrom for 'Out' nodes)                                |    |
|    +---------------------------------------------+--------------------------------------------+    |
+--------------------------------------------------|-------------------------------------------------+
                                                   v
+----------------------------------------------------------------------------------------------------+
|                                  STREAMING EXECUTION PIPELINE                                      |
|                                                                                                    |
|    +-----------------------------+               +--------------------------------------------+    |
|    |     WorkFlowRunManager      | <-----------> |           InProcessExecution               |    |
|    |                             |               | (RunStreamingAsync / ResumeStreamingAsync) |    |
|    | - Status Control (Status)   |               +---------------------+----------------------+    |
|    | - Pause/Cancel Request      |                                     |                           |
|    | - Checkpoints & Snapshots   |                                     v                           |
|    +-----------------------------+               +--------------------------------------------+    |
|                                                  |             HGesComExecutor<T>             |    |
|                                                  |                                            |    |
|                                                  |   - Control signal verification            |    |
|                                                  |   - Execution: IHGesCom.RunCommandAsync()  |    |
|                                                  |   - Routing: Errors & Cancellations        |    |
|                                                  +---------------------+----------------------+    |
+------------------------------------------------------------------------|---------------------------+
                                                                         |
                                                                         v
+----------------------------------------------------------------------------------------------------+
|                                  AI AGENTS AND TOOLS SUBSYSTEM                                     |
|                                                                                                    |
|  +----------------------------+  +----------------------------+  +-------------------------------+ |
|  |     AiAgentDef / Build     |  |      HCommandToolSet       |  |     CommandVectorManager      | |
|  |                            |  |                            |  |                               | |
|  | - Agent Configuration      |  | - RunCommand               |  | - HCommands Indexing          | |
|  | - ChatClient / OpenAIClient|  | - QueryVectorStore         |  | - Embeddings (OpenAI)         | |
|  | - AITools Assignment       |  | - GetSystemTime            |  | - VectorStore (InMemory)      | |
|  +----------------------------+  +----------------------------+  +-------------------------------+ |
+----------------------------------------------------------------------------------------------------+
```

---

### Components and Responsibilities

#### 1. Workflow Graph Definition (`WorkFlowDef`)

Represents the declarative structure of an executable workflow, serializable to exchange formats (e.g., JSON) for persistence or manipulation via visual designers.

- **`WorkFlowDef`**: Root workflow entity. Contains the unique identifier (`Id`), descriptive name (`Name`), message data type transitioning through nodes (`MessageType`), global error handlers (`FlowErrorProcessor`) and cancellation handlers (`FlowCancelProcessor`), as well as collections of nodes (`Nodes`) and links (`Links`).
- **`WorkFlowNode`**: Represents an individual processing station within the graph.
  - `Id`, `Name`: Node identification and description.
  - `Type`: Functional role of the node (`In` for data input, `Node` for intermediate transformation/processing, `Out` for emitting results).
  - `PosX`, `PosY`: Visual designer location coordinates.
  - `InPorts`, `OutPorts`: Input and output interconnection ports (`NodePort`).
  - `CommandAction`: Registered command key in `IHGesCom` implementing the node's business logic.
- **`WorkFlowLink`**: Directed connection between an output port of a source node (`SourceNodeId`, `SourcePortId`) and an input port of a target node (`TargetNodeId`, `TargetPortId`).
- **`LinkCondition`**: Conditional evaluation rule associated with a link. Defines the boolean evaluation command (`ConditionCommand`), expected value (`ExpectedValue`), and descriptive name (`ConditionName`).
- **`WorkFlowNodeTypes`**: Enumeration categorizing nodes into `In`, `Node`, and `Out`.

---

#### 2. Graph Compilation and Execution (`Extensions` and `HGesComExecutor<T>`)

Provides the transformation mechanisms from abstract definition (`WorkFlowDef`) to the executable `Workflow` engine structure.

- **`Extensions.Build<T>`**:
  - Resolves the `IHGesCom` command service from the dependency injection container (`IServiceProvider`).
  - Validates mandatory presence of at least one input node (`WorkFlowNodeTypes.In`).
  - Maps each node to a dedicated `HGesComExecutor<T>` instance.
  - Constructs the graph via `WorkflowBuilder`, adding edges and binding conditional predicates that invoke `IHGesCom.RunCommand<bool>` with the current message.
  - Configures terminal nodes (`WorkFlowNodeTypes.Out`) as formal workflow outputs (`WithOutputFrom`).

- **`HGesComExecutor<T>`**:
  - Inherits from `Executor<T, T>` of the workflow engine.
  - Links the shared cancellation token with the node execution token.
  - Cooperatively evaluates pause requests issued by `WorkFlowRunManager` before and after command execution (`RequestHaltAsync`).
  - Invokes the command associated with the node passing message payload (`message`), execution context (`IWorkflowContext`), and cancellation token (`ct`).
  - **Exception and Cancellation Handling**: If an error or cancellation occurs and the workflow defines processing commands (`FlowErrorProcessor` or `FlowCancelProcessor`), the executor reroutes the payload to that command, yields the result via `context.YieldOutputAsync`, and requests controlled workflow halt (`RequestHaltAsync`).

---

#### 3. Monitoring, Lifecycle, and Persistence (`WorkFlowRunManager`)

Coordinates operational status, background execution, and checkpoint persistence for long-running workflows.

- **Execution States (`WorkFlowStatus`)**:
  - `Idle`: Initial or waiting state.
  - `Running`: Actively executing workflow.
  - `Pausing`: Transitioning to pause; the workflow completes the in-flight node before halting.
  - `Paused`: Workflow suspended persistently or in-memory at a valid checkpoint.
  - `Resuming`: Reactivation process from a checkpoint.
  - `Completed`: Successful completion of the workflow delivering final result.
  - `Cancelled`: Execution halted by cancellation request.
  - `Faulted`: Halted due to unrecoverable error during processing.

- **Snapshot Management (`WorkFlowSnapshot`)**:
  - Structure encapsulating execution state snapshot: `WorkflowId`, `InstanceId`, UTC timestamp, current state (`Status`), executing node (`CurrentNodeId`), last processed payload (`LastPayload`), and checkpoint information (`CheckpointInfo`).

- **Operational Capabilities of `WorkFlowRunManager`**:
  - **Cancellation and Pause Control**: Generates linked tokens (`CreateLinkedCts`), manages atomic pause requests (`RequestPause`), and forced cancellation (`Cancel`).
  - **Checkpoint Management**: Allows serializing and saving engine state (`SaveCheckpointAsync`, `LoadCheckpointAsync`) to local files (`DefaultCheckpointPath`) or custom storage via `CheckpointManager`.
  - **Event Monitoring**: Real-time notifications of status changes and workflow events (`NotifyEvent`), updating active node and data payload.

---

#### 4. AI Client and Agent Integration (`AiAgentDef`, `AiClientDef`, `AiExtensions`)

Facilitates instantiation of intelligent agents and resolution of secure credentials for use within or outside workflows.

- **`AiAgentDef`**: Configuration model specifying agent identifier (`Name`), LLM chat model (`Model`), embeddings model (`EmbedderModel`), system instructions (`Instructions`), and list of available tools (`Tools`).
- **`AiClientDef`**: Configuration model for AI clients defining credential keys (`Endpoint`, `ApiKey`) to access OpenAI / Azure OpenAI compatible services.
- **`AiExtensions`**:
  - `GetClient`: Resolves credentials via `IHCredManager` and builds an authenticated `OpenAIClient`.
  - `Build`: Instantiates an `AIAgent` from `AiAgentDef` using the chat client (`ChatClient.AsAIAgent`) configured with instructions, name, and tools (`AITool`).
  - `WhithTools`: Fluent method to assign tools to agent definition.

---

#### 5. Semantic and Vector Tools Subsystem (`Tools`)

Provides integration between AI agents and commands registered in the HorizonteNet environment.

- **`HCommandToolSet`**: Exposes native functions (`AITool`) via `AIFunctionFactory` for LLMs to interact with the environment:
  - `RunCommand`: Allows the model to dynamically execute HorizonteNet commands by name, supporting synchronous (`RunCommandJson`) and asynchronous (`RunCommandJsonAsync`) commands.
  - `QueryVectorStore`: Allows the model to find relevant commands via natural language queries in the vector store.
  - `GetSystemTime`: Returns current system date and time in ISO 8601 format.

- **`CommandVectorManager`**: Semantic search engine and vector memory management:
  - Extracts and catalogs all active `IHGesCom` commands (`GetCommandList`), serializing schemas, descriptions, and parameter types (`CommandDefinition`).
  - Maintains a local cache (`cache_commands.csv`) with serialized definitions and corresponding embedding vectors.
  - Validates cache consistency against changes in registered commands (`ValidateCacheConsistencyAsync`).
  - Generates text vector embeddings using OpenAI client (`GenerateEmbeddingsFromTextAsync`).
  - Stores and performs vector similarity search (`SearchCommandInVectorStoreAsync`) over an in-memory collection (`VectorStoreCollection<int, CommandVectorRecord>`) implemented with `Microsoft.SemanticKernel.Connectors.InMemory`.

---

### Execution, Pause, and Resume Flow

```
[Workflow Start]
       |
       v
+-------------------------------+
|  InProcessExecution.Run...    | <------------------------------------+
+---------------+---------------+                                      |
                |                                                      |
                v                                                      |
+-------------------------------+                                      |
|    HGesComExecutor.Handle     |                                      |
+---------------+---------------+                                      |
                |                                                      |
    +-----------+-----------+                                          |
    | Pause requested?      |                                          |
    +-----------+-----------+                                          |
          [YES] |  [NO]                                                |
          +-----+   |                                                  |
          |         v                                                  |
          |   +------------------------------------+                   |
          |   |  IHGesCom.RunCommandAsync(command) |                   |
          |   +-----------------+------------------+                   |
          |                     |                                      |
          |         +-----------+-----------+                          |
          |         | Error or Cancellation?|                          |
          |         +-----------+-----------+                          |
          |               [YES] |  [NO]                                |
          |               +-----+   |                                  |
          |               |         v                                  |
          |               |   +------------------------------------+   |
          |               |   | Next node in graph                 | --+
          |               |   +------------------------------------+
          v               v
+-------------------------------+ +------------------------------------+
| context.RequestHaltAsync()    | | context.YieldOutputAsync(res)      |
| State -> Paused               | | Routing to Error/Cancel Cmd        |
| Save CheckpointInfo           | | State -> Faulted / Cancelled       |
+-------------------------------+ +------------------------------------+
```

---

### Integration with the HorizonteNet Ecosystem

| Horizonte Component | Role in Horizonte.Extension.AiWorkFlows |
|---|---|
| `IHGesCom` | Executes actions associated with each graph node, evaluates link conditions, and dispatches tools invoked by AI agents. |
| `IHCredManager` | Resolves required AI client credentials (API Keys and Endpoints) securely and centrally. |
| `HCommand` | Command metadata structure used by `CommandVectorManager` to extract signatures, parameters, and generate semantic indices. |
| `Microsoft.Agents.AI.Workflows` | Underlying engine managing streaming execution graph, event propagation, and execution checkpoints. |
| `Microsoft.Extensions.VectorData` | Abstraction for in-memory storage and querying of command vector records. |
