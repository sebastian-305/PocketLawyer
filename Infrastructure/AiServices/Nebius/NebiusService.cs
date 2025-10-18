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
            Temperature = 0.0f,

        };
        ChatCompletion completion;
        try
        {
            completion = await _client.CompleteChatAsync(messages, options);
        }
        catch (ClientResultException ex)
        {
            var raw = ex.GetRawResponse(); // enthält Status, Headers, Content
            string body = raw?.Content?.ToString() ?? "(no body)";
            _logger.LogError(ex, "OpenAI 400: {Message}\nResponse body: {Body}\nHeaders: {Headers}",
                ex.Message, body, raw?.Headers);
            throw; // oder sinnvoll behandeln
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in Nebius Request: {er}", ex.Message);
            _logger.LogError("Content of completion: {comp}", ex.ToString());
            return "Error";
        }

        _logger.LogDebug("Nebius input Token:  {answ}", completion.Usage.InputTokenCount);
        _logger.LogDebug("Nebius output Token:  {answ}", completion.Usage.OutputTokenCount);

        _logger.LogDebug("Nebius total Token:  {answ}", completion.Usage.TotalTokenCount);

        return completion.Content[0].Text ?? "Error";
    }

    private static System.BinaryData GetAnalizerSchema()
    {
        return BinaryData.FromBytes("""
               {
                  "type": "object",
                  "properties": {
                    "fazit": {
                      "type": "string",
                       "description": "Ein Fazit, dass die Analyse zusammenfasst und eine Aussage darüber macht, ob der Mietvertrag von einem Rechstgutachter angesehen werden sollte. Das Fazit besteht aus Maximal 100 Wörtern"
                    },
                    "nachteile_klauseln": {
                      "type": "array",
                       "description": "Klauseln zum Nachteil des Mieters bzw. der Mieterin",
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
                      "descritpion": "In der Zusammenfassung werden einzelne Punkte kurz und übersichtlich dargestellt, damit Schlüsselkomponenten des Mietvertrags leicht zu identifizieren sind.",
                      "properties": {
                        "vermieter": {
                          "type": "string",
                           "description": "Name des Vermiters"
                        },"mieter": {
                          "type": "string",
                            "description": "Name des Mieters"
                        },                       
                        "beginn": {
                          "type": "string",
                          "description" : "Beginn der Miete"
                        },
                        "dauer": {
                          "type": "string",
                           "description": "Vereinbarte Dauer des Mietvertrages"
                        },
                        "mindestdauer": {
                          "type": "string",
                           "description": "Mindestdauer des Mietvertrages"
                        },
                        "kuendigungsfrist": {
                          "type": "string"
                        },
                        "miete_kalt": {
                          "type": "string",
                          "description": "Kaltmiete in Euro"
                        },
                        "betriebskosten": {
                          "type": "string"
                        }
                        ,
                        "quadratmeter": {
                          "type": "string"
                        },
                        "zimmer": {
                          "type": "number"
                        },
                        "im_mietgegenstand_enthalten": {
                          "type": "string",
                           "description": "Bechreibung, was im Mietobjekt enthalten ist, bspw. Küche, Garage, Balkon etc."
                        }                
                      },
                    "required": [
                    "vermieter",
                    "mieter",
                    "beginn",
                    "dauer",
                    "mindestdauer",
                    "kuendigungsfrist",
                    "miete_kalt",
                    "betriebskosten",
                    "quadratmeter",
                    "zimmer",
                    "im_mietgegenstand_enthalten"
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
