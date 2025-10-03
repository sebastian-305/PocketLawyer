using Anthropic.SDK.Messaging;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Infrastructure.AiServices.Gemini;
using PocketLawyer.Infrastructure.AiServices.Gemini.Models;
using PocketLawyer.Options;
using System.ClientModel;
using System.Net.Http;
using System.Text.Json;

namespace PocketLawyer.Infrastructure.AiServices.Nebius;

public class NebiusService : IAiClient
{
    ILogger<NebiusService> _logger;
    ChatClient _client;

    public NebiusService(
        IOptions<NebiusOptions> options,
        ILogger<NebiusService> logger
        )
    {
        _logger = logger;
         _client = new(
             //model: "deepseek-ai/DeepSeek-V3-0324-fast",
             //model: "Qwen/Qwen3-235B-A22B-Thinking-2507",
             //model: "Qwen/Qwen3-32B-fast",
             model: options.Value.ModelId ?? "",
             credential: new ApiKeyCredential(options.Value.Key ?? ""),
             options: new OpenAIClientOptions()
             {
                 Endpoint = new Uri("https://api.studio.nebius.com/v1/")
             }
         );

    }


    public static System.BinaryData AnalyzerSchemaDefinition => GetAnalizerSchema();
    public static System.BinaryData PreAnalyzerSchemaDefinition => GetPreAnalizerSchema();

    public async Task<string> SendPrompt(string prompt, object? outputSchema)
    {
        _logger.LogDebug("Entering {Method}", nameof(SendPrompt));
    
        List<ChatMessage> messages =
        [
            new UserChatMessage(prompt),
         ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "Vertragsanalyse",
                jsonSchema: outputSchema as BinaryData,
                jsonSchemaIsStrict: true),
            Temperature= 0.1f,

            };
        ChatCompletion completion;
        try
        {
            completion = await _client.CompleteChatAsync(messages, options);
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in Nebius Request: {er}", ex.Message);
            return "Error";
        }

        return completion.Content[0].Text ?? "Error";
    }

    private static System.BinaryData GetAnalizerSchema()
    {
        return BinaryData.FromBytes("""
               {
                  "type": "object",
                  "properties": {
                    "fazit": {
                      "type": "string"
                    },
                    "nachteile_klauseln": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "problem": {
                            "type": "string"
                          },
                          "rechtsgrundlage": {
                            "type": "array",
                            "items": {
                              "type": "object",
                              "properties": {
                                "rechtsgrundlage": {
                                  "type": "string"
                                },
                                "wortlaut": {
                                  "type": "string",
                                  "enum": [
                                    ""
                                  ]
                                }
                              },
                              "required": [
                                "rechtsgrundlage",
                                "wortlaut"
                              ]
                            }
                          },
                          "titel": {
                            "type": "string"
                          },
                          "zitat": {
                            "type": "string"
                          }
                        },
                        "required": [
                          "problem",
                          "rechtsgrundlage",
                          "titel",
                          "zitat"
                        ]
                      }
                    },
                    "summary": {
                      "type": "object",
                      "properties": {
                        "arbeitgeber": {
                          "type": "string"
                        },
                        "arbeitsstunden": {
                          "type": "string"
                        },
                        "beginn": {
                          "type": "string"
                        },
                        "dauer": {
                          "type": "string"
                        },
                        "gehalt_brutto": {
                          "type": "string"
                        },
                        "probezeit": {
                          "type": "string"
                        },
                        "taetigkeit": {
                          "type": "string"
                        },
                        "ueberstunden": {
                          "type": "string"
                        },
                        "zuschlaege": {
                          "type": "string"
                        }
                      },
                      "required": [
                        "arbeitgeber",
                        "arbeitsstunden",
                        "beginn",
                        "dauer",
                        "gehalt_brutto",
                        "probezeit",
                        "taetigkeit",
                        "ueberstunden",
                        "zuschlaege"
                      ]
                    },
                    "unzulaessige_klauseln": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "problem": {
                            "type": "string"
                          },
                          "rechtsgrundlage": {
                            "type": "array",
                            "items": {
                              "type": "object",
                              "properties": {
                                "rechtsgrundlage": {
                                  "type": "string"
                                },
                                "wortlaut": {
                                  "type": "string",
                                  "enum": [
                                    ""
                                  ]
                                }
                              },
                              "required": [
                                "rechtsgrundlage",
                                "wortlaut"
                              ]
                            }
                          },
                          "titel": {
                            "type": "string"
                          },
                          "zitat": {
                            "type": "string"
                          }
                        },
                        "required": [
                          "problem",
                          "rechtsgrundlage",
                          "titel",
                          "zitat"
                        ]
                      }
                    },
                    "vorteilhafte_klauseln": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "titel": {
                            "type": "string"
                          },
                          "vorteil": {
                            "type": "string"
                          },
                          "zitat": {
                            "type": "string"
                          }
                        },
                        "required": [
                          "titel",
                          "vorteil",
                          "zitat"
                        ]
                      }
                    },
                    "widerspruechliche_klauseln": {
                      "type": "array",
                      "items": {
                        "type": "object",
                        "properties": {
                          "problem": {
                            "type": "string"
                          },
                          "rechtsgrundlage": {
                            "type": "array",
                            "items": {
                              "type": "object",
                              "properties": {
                                "rechtsgrundlage": {
                                  "type": "string"
                                },
                                "wortlaut": {
                                  "type": "string"
                                }
                              },
                              "required": [
                                "rechtsgrundlage",
                                "wortlaut"
                              ]
                            }
                          },
                          "titel": {
                            "type": "string"
                          },
                          "zitat_klausel_a": {
                            "type": "string"
                          },
                          "zitat_klausel_b": {
                            "type": "string"
                          }
                        },
                        "required": [
                          "problem",
                          "rechtsgrundlage",
                          "titel",
                          "zitat_klausel_a",
                          "zitat_klausel_b"
                        ]
                      }
                    }
                  },
                  "required": [
                    "fazit",
                    "summary"
                  ]
                }
            """u8.ToArray());
    }

    private static System.BinaryData GetPreAnalizerSchema()
    {
        return BinaryData.FromBytes("""
               {
                  "type": "object",
                  "properties": {
                    "isReadable": { "type": "boolean" },
                    "isContract": { "type": "boolean" },
                    "result":     { "type": "string" }
                  },
                  "required": ["isReadable", "isContract", "result"]
                }
            """u8.ToArray());
    }

}
