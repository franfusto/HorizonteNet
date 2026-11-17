### Arquitectura de Horizonte.Extension.AiWorkFlows

La biblioteca **Horizonte.Extension.AiWorkFlows** implementa la capa de orquestación de flujos de trabajo inteligentes y ejecución de grafos dirigidos para el ecosistema HorizonteNet. Integra las capacidades de agentes de inteligencia artificial y motores de flujos de trabajo basados en `Microsoft.Agents.AI.Workflows` con el sistema de comandos desacoplados (`IHGesCom`), gestión de credenciales (`IHCredManager`) y búsqueda semántica vectorial de herramientas mediante embeddings de OpenAI y `Microsoft.Extensions.VectorData`.

Este documento describe la arquitectura técnica del subsistema, los modelos de definición de flujos, el ciclo de vida y control de ejecución (pausa, reanudación, cancelación y puntos de control), el conjunto de herramientas expuestas a agentes de IA y la indexación semántica de comandos.

---

### Visión General de la Arquitectura

`Horizonte.Extension.AiWorkFlows` permite diseñar, compilar y ejecutar grafos de procesamiento donde cada nodo representa una acción o comando ejecutable en HorizonteNet. A través de este modelo, los flujos pueden coordinar llamadas a modelos de lenguaje (LLMs), invocar comandos síncronos o asíncronos del sistema, evaluar condiciones de ramificación dinámicas y gestionar de forma tolerante fallos, cancelaciones e interrupciones.

#### Esquema General de Componentes e Integración

```
+----------------------------------------------------------------------------------------------------+
|                                    DEFINICIÓN DEL FLUJO                                            |
|                  (WorkFlowDef: Nodos, Enlaces, Puertos, Condiciones, Procesadores)                  |
+--------------------------------------------------+-------------------------------------------------+
                                                   |
                                                   v
+----------------------------------------------------------------------------------------------------+
|                               MOTOR DE COMPILACIÓN Y CONSTRUCCIÓN                                  |
|                                                                                                    |
|    +------------------------------------------------------------------------------------------+    |
|    |                             Extensions.Build<T>()                                        |    |
|    |                                                                                          |    |
|    |   1. Localización del nodo inicial (Type == WorkFlowNodeTypes.In)                        |    |
|    |   2. Instanciación de ejecutores (HGesComExecutor<T> para cada nodo)                     |    |
|    |   3. Configuración de WorkflowBuilder y registro del nodo raíz                           |    |
|    |   4. Construcción de aristas (AddEdge) con condiciones evaluadas por IHGesCom            |    |
|    |   5. Asignación de nodos de salida (WithOutputFrom para nodos 'Out')                     |    |
|    +---------------------------------------------+--------------------------------------------+    |
+--------------------------------------------------|-------------------------------------------------+
                                                   v
+----------------------------------------------------------------------------------------------------+
|                                    PIPELINE DE EJECUCIÓN STREAMING                                 |
|                                                                                                    |
|    +-----------------------------+               +--------------------------------------------+    |
|    |     WorkFlowRunManager      | <-----------> |           InProcessExecution               |    |
|    |                             |               | (RunStreamingAsync / ResumeStreamingAsync) |    |
|    | - Control de Estado (Status)|               +---------------------+----------------------+    |
|    | - Solicitud de Pausa/Cancel |                                     |                           |
|    | - Checkpoint & Snapshots    |                                     v                           |
|    +-----------------------------+               +--------------------------------------------+    |
|                                                  |             HGesComExecutor<T>             |    |
|                                                  |                                            |    |
|                                                  |   - Verificación de señales de control     |    |
|                                                  |   - Ejecución: IHGesCom.RunCommandAsync()  |    |
|                                                  |   - Enrutamiento: Errores y Cancelaciones  |    |
|                                                  +---------------------+----------------------+    |
+------------------------------------------------------------------------|---------------------------+
                                                                         |
                                                                         v
+----------------------------------------------------------------------------------------------------+
|                                SUBSISTEMA DE HERRAMIENTAS Y AGENTES IA                             |
|                                                                                                    |
|  +----------------------------+  +----------------------------+  +-------------------------------+ |
|  |     AiAgentDef / Build     |  |      HCommandToolSet       |  |     CommandVectorManager      | |
|  |                            |  |                            |  |                               | |
|  | - Configuración del Agente |  | - RunCommand               |  | - Indexación de HCommands     | |
|  | - ChatClient / OpenAIClient|  | - QueryVectorStore         |  | - Embeddings (OpenAI)         | |
|  | - Asignación de AITools    |  | - GetSystemTime            |  | - VectorStore (InMemory)      | |
|  +----------------------------+  +----------------------------+  +-------------------------------+ |
+----------------------------------------------------------------------------------------------------+
```

---

### Componentes y Responsabilidades

#### 1. Definición del Grafo de Flujo (`WorkFlowDef`)

Representa la estructura declarativa de un flujo de trabajo ejecutable, serializable a formatos de intercambio (e.g. JSON) para su persistencia o manipulación mediante diseñadores visuales.

- **`WorkFlowDef`**: Entidad raíz del flujo. Contiene el identificador único (`Id`), nombre descriptivo (`Name`), tipo de mensaje de datos que transita por los nodos (`MessageType`), manejadores globales de error (`FlowErrorProcessor`) y cancelación (`FlowCancelProcessor`), así como las colecciones de nodos (`Nodes`) y enlaces (`Links`).
- **`WorkFlowNode`**: Representa una estación de procesamiento individual dentro del grafo.
  - `Id`, `Name`: Identificación y descripción del nodo.
  - `Type`: Rol funcional del nodo (`In` para entrada de datos, `Node` para transformación/procesamiento intermedio, `Out` para emisión de resultados).
  - `PosX`, `PosY`: Coordenadas visuales de ubicación en el diseñador.
  - `InPorts`, `OutPorts`: Puertos de interconexión de entrada y salida (`NodePort`).
  - `CommandAction`: Clave del comando registrado en `IHGesCom` que implementa la lógica de negocio del nodo.
- **`WorkFlowLink`**: Conexión dirigida entre un puerto de salida de un nodo origen (`SourceNodeId`, `SourcePortId`) y un puerto de entrada de un nodo destino (`TargetNodeId`, `TargetPortId`).
- **`LinkCondition`**: Regla de evaluación condicional asociada a un enlace. Define el comando booleano de evaluación (`ConditionCommand`), el valor esperado (`ExpectedValue`) y el nombre descriptivo (`ConditionName`).
- **`WorkFlowNodeTypes`**: Enumeración que clasifica los nodos en `In`, `Node` y `Out`.

---

#### 2. Compilación y Ejecución del Grafo (`Extensions` y `HGesComExecutor<T>`)

Proporciona los mecanismos de transformación desde la definición abstracta (`WorkFlowDef`) hacia la estructura ejecutable del motor `Workflow`.

- **`Extensions.Build<T>`**:
  - Resuelve el servicio de comandos `IHGesCom` desde el contenedor de inyección de dependencias (`IServiceProvider`).
  - Valida la presencia obligatoria de al menos un nodo de entrada (`WorkFlowNodeTypes.In`).
  - Mapea cada nodo a una instancia dedicada de `HGesComExecutor<T>`.
  - Construye el grafo mediante `WorkflowBuilder`, añadiendo las aristas y acoplando los predicados condicionales que invocan `IHGesCom.RunCommand<bool>` con el mensaje actual.
  - Configura los nodos terminales (`WorkFlowNodeTypes.Out`) como salidas formales del flujo (`WithOutputFrom`).

- **`HGesComExecutor<T>`**:
  - Hereda de `Executor<T, T>` del motor de workflows.
  - Enlaza el token de cancelación compartido con el token de ejecución del nodo.
  - Evalúa cooperativamente peticiones de pausa solicitadas por `WorkFlowRunManager` antes y después de la ejecución del comando (`RequestHaltAsync`).
  - Invoca el comando asociado al nodo pasando la carga útil del mensaje (`message`), el contexto de ejecución (`IWorkflowContext`) y el token de cancelación (`ct`).
  - **Manejo de Excepciones y Cancelaciones**: Si ocurre un error o cancelación y el flujo tiene definidos comandos de procesamiento (`FlowErrorProcessor` o `FlowCancelProcessor`), el ejecutor redirige la carga útil a dicho comando, emite el resultado mediante `context.YieldOutputAsync` y solicita la detención controlada del flujo (`RequestHaltAsync`).

---

#### 3. Supervisión, Ciclo de Vida y Persistencia (`WorkFlowRunManager`)

Coordina el estado operativo, la ejecución en segundo plano y la persistencia de puntos de control (checkpoints) para flujos de larga duración.

- **Estados de Ejecución (`WorkFlowStatus`)**:
  - `Idle`: Estado inicial o en espera.
  - `Running`: Flujo en ejecución activa.
  - `Pausing`: Transición hacia la pausa; el flujo finaliza el nodo en curso antes de detenerse.
  - `Paused`: Flujo suspendido de forma persistente o en memoria en un punto de control válido.
  - `Resuming`: Proceso de reactivación desde un punto de control.
  - `Completed`: Finalización exitosa del flujo y entrega del resultado final.
  - `Cancelled`: Ejecución interrumpida por solicitud de cancelación.
  - `Faulted`: Detención por error no recuperable durante el procesamiento.

- **Manejo de Instantáneas (`WorkFlowSnapshot`)**:
  - Estructura que encapsula la instantánea del estado de ejecución: `WorkflowId`, `InstanceId`, marca temporal UTC, estado actual (`Status`), nodo en ejecución (`CurrentNodeId`), última carga útil procesada (`LastPayload`) e información del punto de control (`CheckpointInfo`).

- **Capacidades Operativas de `WorkFlowRunManager`**:
  - **Control de Cancelación y Pausa**: Genera tokens enlazados (`CreateLinkedCts`), gestiona solicitudes de pausa atómica (`RequestPause`) y cancelación forzada (`Cancel`).
  - **Gestión de Checkpoints**: Permite serializar y guardar el estado del motor (`SaveCheckpointAsync`, `LoadCheckpointAsync`) en archivos locales (`DefaultCheckpointPath`) o almacenamiento personalizado mediante `CheckpointManager`.
  - **Supervisión de Eventos**: Notifica en tiempo real cambios de estado y eventos del flujo (`NotifyEvent`), actualizando el nodo activo y la carga de datos.

---

#### 4. Integración de Clientes y Agentes de IA (`AiAgentDef`, `AiClientDef`, `AiExtensions`)

Facilita la instanciación de agentes inteligentes y la resolución de credenciales seguras para su uso dentro o fuera de los flujos.

- **`AiAgentDef`**: Modelo de configuración que especifica el identificador del agente (`Name`), modelo de chat LLM (`Model`), modelo de embeddings (`EmbedderModel`), instrucciones de sistema (`Instructions`) y la lista de herramientas disponibles (`Tools`).
- **`AiClientDef`**: Modelo de configuración para clientes de IA que define las claves de credenciales (`Endpoint`, `ApiKey`) para acceder a servicios compatibles con OpenAI / Azure OpenAI.
- **`AiExtensions`**:
  - `GetClient`: Resuelve las credenciales mediante `IHCredManager` y construye un cliente `OpenAIClient` autenticado.
  - `Build`: Instancia un agente `AIAgent` a partir de `AiAgentDef` utilizando el cliente de chat (`ChatClient.AsAIAgent`) configurado con sus instrucciones, nombre y herramientas (`AITool`).
  - `WhithTools`: Método fluido para asignar herramientas a la definición del agente.

---

#### 5. Subsistema de Herramientas Semánticas y Vectoriales (`Tools`)

Proporciona integración entre los agentes de IA y los comandos registrados en el entorno HorizonteNet.

- **`HCommandToolSet`**: Expone funciones nativas (`AITool`) mediante `AIFunctionFactory` para que los LLMs puedan interactuar con el entorno:
  - `RunCommand`: Permite al modelo ejecutar dinámicamente comandos de HorizonteNet por nombre, soportando comandos síncronos (`RunCommandJson`) y asíncronos (`RunCommandJsonAsync`).
  - `QueryVectorStore`: Permite al modelo buscar comandos pertinentes mediante consultas en lenguaje natural en el almacén vectorial.
  - `GetSystemTime`: Proporciona la fecha y hora del sistema en formato ISO 8601.

- **`CommandVectorManager`**: Motor de búsqueda semántica y gestión de memoria vectorial:
  - Extrae y cataloga todos los comandos activos de `IHGesCom` (`GetCommandList`), serializando sus esquemas, descripciones y tipos de parámetros (`CommandDefinition`).
  - Mantiene un caché local (`cache_commands.csv`) con las definiciones serializadas y sus vectores de embedding correspondientes.
  - Valida la consistencia de caché frente a cambios en los comandos registrados (`ValidateCacheConsistencyAsync`).
  - Genera embeddings vectoriales de texto mediante el cliente OpenAI (`GenerateEmbeddingsFromTextAsync`).
  - Almacena y realiza búsquedas de similitud vectorial (`SearchCommandInVectorStoreAsync`) sobre una colección en memoria (`VectorStoreCollection<int, CommandVectorRecord>`) implementada con `Microsoft.SemanticKernel.Connectors.InMemory`.

---

### Flujo de Ejecución, Pausa y Reanudación

```
[Inicio de Flujo]
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
    | ¿Pausa solicitada?    |                                          |
    +-----------+-----------+                                          |
          [SÍ]  |  [NO]                                                |
          +-----+   |                                                  |
          |         v                                                  |
          |   +------------------------------------+                   |
          |   |  IHGesCom.RunCommandAsync(command) |                   |
          |   +-----------------+------------------+                   |
          |                     |                                      |
          |         +-----------+-----------+                          |
          |         | ¿Error o Cancelación? |                          |
          |         +-----------+-----------+                          |
          |               [SÍ]  |  [NO]                                |
          |               +-----+   |                                  |
          |               |         v                                  |
          |               |   +------------------------------------+   |
          |               |   | Siguiente nodo en grafo            | --+
          |               |   +------------------------------------+
          v               v
+-------------------------------+ +------------------------------------+
| context.RequestHaltAsync()    | | context.YieldOutputAsync(res)      |
| Estado -> Paused              | | Enrutamiento a Error/Cancel Cmd    |
| Guardar CheckpointInfo        | | Estado -> Faulted / Cancelled      |
+-------------------------------+ +------------------------------------+
```

---

### Integración con el Ecosistema HorizonteNet

| Componente de Horizonte | Rol en Horizonte.Extension.AiWorkFlows |
|---|---|
| `IHGesCom` | Ejecuta las acciones asociadas a cada nodo del grafo, evalúa condiciones de enlace y despacha herramientas invocadas por agentes de IA. |
| `IHCredManager` | Resuelve de forma segura y centralizada las credenciales (API Keys y Endpoints) requeridas por los clientes de IA. |
| `HCommand` | Estructura de metadatos de comandos utilizada por `CommandVectorManager` para extraer firmas, parámetros y generar índices semánticos. |
| `Microsoft.Agents.AI.Workflows` | Motor subyacente que gestiona el grafo de ejecución en streaming, la propagación de eventos y los puntos de control de ejecución. |
| `Microsoft.Extensions.VectorData` | Abstracción para el almacenamiento y consulta de registros vectoriales de comandos en memoria. |
