using System.ClientModel;
using Horizonte.Extension.Ai;
using Horizonte.Extension.Ai.Contracts;
using Horizonte.Extension.Ai.Definitions;
using Microsoft.Extensions.AI;
using OpenAI;
using OpenAI.Files;


namespace Horizonte.Extension.Ai;

class Program
{
   static async Task Main(string[] args)
{
    var configuration = GetConfigurationLmStudio();

    var agentDef = configuration.AgentDefs
        .First(agent => agent.Id == "lmstudio-agent");

    var hagent = agentDef.GetHagent(configuration);

    Console.WriteLine("Chat iniciado.");
    Console.WriteLine("Para terminar: /q");
    Console.WriteLine();

    while (true)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(">>>> ");
        Console.ResetColor();

        var input = Console.ReadLine();

        if (input is null)
        {
            break;
        }

        input = input.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            continue;
        }

        if (input.Equals("/q", StringComparison.OrdinalIgnoreCase))
        {
            break;
        }

        try
        {
            var response = await hagent.QueryAsync(new ChatQuery
            {
                Query = input
            });

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("Agente> ");
            Console.ResetColor();

            var hasContent = false;

            foreach (var item in response.ResponseItems ?? [])
            {
                if (string.IsNullOrWhiteSpace(item.Content))
                {
                    continue;
                }

                Console.WriteLine(item.Content);
                hasContent = true;
            }

            if (!hasContent)
            {
                Console.WriteLine("(sin respuesta)");
            }

            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Error: {ex.Message}");
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    Console.WriteLine();
    Console.WriteLine("Chat finalizado.");
}

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

                // Debe coincidir con el modelo cargado en LM Studio.
                //Model = "qwen2.5-0.5b-instruct",
                Model = "google/gemma-4-e2b",

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
                    AutoInvokeTools = true,
                    ReturnToolMessages = true,
                    ReturnIntermediateSteps = true
                },

                Settings = new Dictionary<string, string>
                {
                    ["profile"] = "local-lmstudio"
                }
            }
        ]
    };
}
    private static Configuration GetConfigurationSample1()
    {
        return new Configuration
        {
            ServerDefs =
            [
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
                        ["organization"] = "<organization-id>",
                        ["project"] = "<project-id>"
                    }
                },

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
                        ["compatibility"] = "openai"
                    }
                }
            ],

            ClientDefs =
            [
                new ClientDef
                {
                    Id = "openai-chat-main",
                    Name = "Chat principal",
                    ClientType = ClientType.Chat,
                    Server = "openai-main",
                    Model = "gpt-4.1",
                    Temperature = 0.2f,
                    MaxOutputTokens = 4000,
                    EnableStreaming = false,
                    EnableTools = true,
                    Settings = new Dictionary<string, string>
                    {
                        ["responseFormat"] = "text"
                    }
                },

                new ClientDef
                {
                    Id = "openai-embedder-main",
                    Name = "Embeddings principal",
                    ClientType = ClientType.Embedder,
                    Server = "openai-main",
                    Model = "text-embedding-3-small",
                    EnableTools = false,
                    Settings = new Dictionary<string, string>
                    {
                        ["dimensions"] = "1536"
                    }
                },

                new ClientDef
                {
                    Id = "openai-image-to-text-main",
                    Name = "Visión principal",
                    ClientType = ClientType.ImageToText,
                    Server = "openai-main",
                    Model = "gpt-4.1",
                    Temperature = 0.0f,
                    MaxOutputTokens = 2000,
                    EnableTools = false,
                    Settings = new Dictionary<string, string>
                    {
                        ["detail"] = "auto"
                    }
                },

                new ClientDef
                {
                    Id = "openai-text-to-image-main",
                    Name = "Generador de imágenes",
                    ClientType = ClientType.TextToImage,
                    Server = "openai-main",
                    Model = "gpt-image-1",
                    EnableTools = false,
                    Settings = new Dictionary<string, string>
                    {
                        ["size"] = "1024x1024",
                        ["quality"] = "standard",
                        ["outputFormat"] = "png"
                    }
                }
            ],

            RaGdefs =
            [
                new RaGdef
                {
                    Id = "rag-memory-docs",
                    Name = "RAG documental en memoria",
                    RagType = RagType.InMemory,
                    EmbedderClient = "openai-embedder-main",
                    CollectionName = "documents",
                    Size = 1536,
                    TopK = 5,
                    MinScore = 0.65f,
                    ChunkSize = 1000,
                    ChunkOverlap = 150,
                    MaxContextItems = 8,
                    EnabledAsTool = true,
                    Settings = new Dictionary<string, string>
                    {
                        ["cache"] = "true",
                        ["cacheFile"] = "data/rag-cache.json"
                    }
                }
            ],

            AgentDefs =
            [
                new AgentDef
                {
                    Id = "default",
                    Name = "Agente principal",
                    Description = "Agente general con soporte para RAG, imágenes y archivos.",
                    SystemPrompt = """
                                   Eres un agente de asistencia técnica.
                                   Responde de forma clara, estructurada y útil.
                                   Si necesitas información documental, usa la herramienta RAG.
                                   Si recibes imágenes, analízalas cuando sea relevante.
                                   Si generas archivos, devuélvelos usando el formato configurado.
                                   """,

                    ChatClientId = "openai-chat-main",

                    Tools =
                    [
                        new AgentToolDef
                        {
                            Id = "tool-rag-search",
                            Name = "Búsqueda documental",
                            ToolType = AgentToolType.RagSearch,
                            RagId = "rag-memory-docs",
                            Enabled = true,
                            TimeoutSeconds = 30,
                            Settings = new Dictionary<string, string>
                            {
                                ["autoUse"] = "true",
                                ["description"] = "Busca información relevante en la base documental configurada."
                            }
                        },

                        new AgentToolDef
                        {
                            Id = "tool-image-to-text",
                            Name = "Análisis de imágenes",
                            ToolType = AgentToolType.ImageToText,
                            ClientId = "openai-image-to-text-main",
                            Enabled = true,
                            TimeoutSeconds = 60,
                            Settings = new Dictionary<string, string>
                            {
                                ["autoUseForMimeTypes"] = "image/png,image/jpeg,image/webp"
                            }
                        },


                        new AgentToolDef
                        {
                            Id = "tool-text-to-image",
                            Name = "Generación de imágenes",
                            ToolType = AgentToolType.TextToImage,
                            ClientId = "openai-text-to-image-main",
                            Enabled = true,
                            TimeoutSeconds = 90,
                            Settings = new Dictionary<string, string>
                            {
                                ["returnAsFile"] = "true",
                                ["defaultMimeType"] = "image/png"
                            }
                        },

                        new AgentToolDef
                        {
                            Id = "tool-file-reader",
                            Name = "Lectura de archivos",
                            ToolType = AgentToolType.FileReader,
                            Enabled = true,
                            TimeoutSeconds = 30,
                            Settings = new Dictionary<string, string>
                            {
                                ["decodeBase64"] = "true",
                                ["extractText"] = "true"
                            }
                        },

                        new AgentToolDef
                        {
                            Id = "tool-file-writer",
                            Name = "Generación de archivos",
                            ToolType = AgentToolType.FileWriter,
                            Enabled = true,
                            TimeoutSeconds = 30,
                            Settings = new Dictionary<string, string>
                            {
                                ["encodeBase64"] = "true"
                            }
                        },

                        new AgentToolDef
                        {
                            Id = "tool-system-commands",
                            Name = "Herramientas del sistema",
                            ToolType = AgentToolType.Custom,
                            Enabled = false,
                            TimeoutSeconds = 60,
                            Settings = new Dictionary<string, string>
                            {
                                ["toolSet"] = "HCommandToolSet",
                                ["allowCommandExecution"] = "false",
                                ["allowCommandSearch"] = "true"
                            }
                        }
                    ],

                    FileOptions = new AgentFileOptions
                    {
                        AllowInputFiles = true,
                        AllowOutputFiles = true,
                        TransportEncoding = "base64",
                        MaxInputFileSizeBytes = 10 * 1024 * 1024,
                        MaxOutputFileSizeBytes = 10 * 1024 * 1024,
                        AllowedInputMimeTypes =
                        [
                            "text/plain",
                            "text/markdown",
                            "application/json",
                            "application/pdf",
                            "image/png",
                            "image/jpeg",
                            "image/webp"
                        ],
                        AllowedOutputMimeTypes =
                        [
                            "text/plain",
                            "application/json",
                            "image/png",
                            "image/jpeg"
                        ],
                        AutoProcessImages = true,
                        AutoProcessDocuments = true,
                        Settings = new Dictionary<string, string>
                        {
                            ["fileEnvelope"] = "base64-json",
                            ["includeFileName"] = "true",
                            ["includeMimeType"] = "true"
                        }
                    },

                    ExecutionOptions = new AgentExecutionOptions
                    {
                        MaxTurns = 8,
                        MaxToolCalls = 5,
                        TimeoutSeconds = 120,
                        AutoInvokeTools = true,
                        ReturnToolMessages = true,
                        ReturnIntermediateSteps = false,
                        Settings = new Dictionary<string, string>
                        {
                            ["failOnToolError"] = "false",
                            ["toolErrorMode"] = "return-message",
                            ["defaultLanguage"] = "es"
                        }
                    },

                    Settings = new Dictionary<string, string>
                    {
                        ["profile"] = "general",
                        ["allowRag"] = "true",
                        ["allowImages"] = "true",
                        ["allowFileResponses"] = "true"
                    }
                }
            ]
        };
    }

    private static Configuration GetConfigurationSample2()
    {
        var newAgentDef = new AgentDef
        {
            Id = "simple-chat",
            Name = "Chat simple",
            Description = "Agente de conversación sin herramientas.",
            SystemPrompt = "Eres un asistente sencillo. Responde de forma breve y clara.",
            ChatClientId = "openai-chat-main",

            Tools = [],

            FileOptions = new AgentFileOptions
            {
                AllowInputFiles = false,
                AllowOutputFiles = false
            },

            ExecutionOptions = new AgentExecutionOptions
            {
                MaxTurns = 1,
                MaxToolCalls = 0,
                TimeoutSeconds = 60,
                AutoInvokeTools = false,
                ReturnToolMessages = false,
                ReturnIntermediateSteps = false
            }
        };

        var cfg = GetConfigurationSample1();
        cfg.AgentDefs = [newAgentDef];
        return cfg;
    }

    private static Configuration GetConfigurationSample3()
    {
        var agentdef = new AgentDef
        {
            Id = "docs-agent",
            Name = "Agente documental",
            Description = "Agente especializado en responder usando documentación interna.",
            SystemPrompt = """
                           Eres un agente documental.
                           Prioriza la información recuperada mediante RAG.
                           Si no encuentras información suficiente, dilo claramente.
                           No inventes datos que no estén respaldados por el contexto.
                           """,

            ChatClientId = "openai-chat-main",

            Tools =
            [
                new AgentToolDef
                {
                    Id = "tool-rag-search",
                    Name = "Búsqueda documental",
                    ToolType = AgentToolType.RagSearch,
                    RagId = "rag-memory-docs",
                    Enabled = true,
                    TimeoutSeconds = 30,
                    Settings = new Dictionary<string, string>
                    {
                        ["autoUse"] = "always",
                        ["injectResultsIntoPrompt"] = "true"
                    }
                }
            ],

            FileOptions = new AgentFileOptions
            {
                AllowInputFiles = true,
                AllowOutputFiles = false,
                TransportEncoding = "base64",
                AllowedInputMimeTypes =
                [
                    "text/plain",
                    "text/markdown",
                    "application/pdf",
                    "application/json"
                ],
                AutoProcessDocuments = true,
                AutoProcessImages = false
            },

            ExecutionOptions = new AgentExecutionOptions
            {
                MaxTurns = 4,
                MaxToolCalls = 3,
                TimeoutSeconds = 90,
                AutoInvokeTools = true,
                ReturnToolMessages = false,
                ReturnIntermediateSteps = false
            }
        };

        var cfg = GetConfigurationSample1();
        cfg.AgentDefs = [agentdef];
        return cfg;
    }


    private static Configuration GetConfigurationSample4()
    {
        var agentdef = new AgentDef
        {
            Id = "multimodal-agent",
            Name = "Agente multimodal",
            Description = "Agente capaz de analizar imágenes y generar archivos de salida.",
            SystemPrompt = """
                           Eres un agente multimodal.
                           Puedes analizar imágenes recibidas como archivos base64.
                           Puedes generar respuestas textuales y archivos cuando sea necesario.
                           """,

            ChatClientId = "openai-chat-main",

            Tools =
            [
                new AgentToolDef
                {
                    Id = "tool-image-to-text",
                    Name = "Análisis de imágenes",
                    ToolType = AgentToolType.ImageToText,
                    ClientId = "openai-image-to-text-main",
                    Enabled = true,
                    TimeoutSeconds = 60,
                    Settings = new Dictionary<string, string>
                    {
                        ["autoUseForMimeTypes"] = "image/png,image/jpeg,image/webp"
                    }
                },

                new AgentToolDef
                {
                    Id = "tool-text-to-image",
                    Name = "Generación de imágenes",
                    ToolType = AgentToolType.TextToImage,
                    ClientId = "openai-text-to-image-main",
                    Enabled = true,
                    TimeoutSeconds = 90,
                    Settings = new Dictionary<string, string>
                    {
                        ["returnAsFile"] = "true"
                    }
                },

                new AgentToolDef
                {
                    Id = "tool-file-writer",
                    Name = "Generación de archivos",
                    ToolType = AgentToolType.FileWriter,
                    Enabled = true,
                    TimeoutSeconds = 30
                }
            ],

            FileOptions = new AgentFileOptions
            {
                AllowInputFiles = true,
                AllowOutputFiles = true,
                TransportEncoding = "base64",
                MaxInputFileSizeBytes = 15 * 1024 * 1024,
                MaxOutputFileSizeBytes = 15 * 1024 * 1024,
                AllowedInputMimeTypes =
                [
                    "text/markdown",
                    "image/png",
                    "image/jpeg",
                    "image/webp"
                ],
                AllowedOutputMimeTypes =
                [
                    "image/png",
                    "image/jpeg",
                    "application/json",
                    "text/plain"
                ],
                AutoProcessImages = true,
                AutoProcessDocuments = false
            },

            ExecutionOptions = new AgentExecutionOptions
            {
                MaxTurns = 6,
                MaxToolCalls = 4,
                TimeoutSeconds = 180,
                AutoInvokeTools = true,
                ReturnToolMessages = true,
                ReturnIntermediateSteps = false
            }
        };

        var cfg = GetConfigurationSample1();
        cfg.AgentDefs = [agentdef];
        return cfg;
    }
}

