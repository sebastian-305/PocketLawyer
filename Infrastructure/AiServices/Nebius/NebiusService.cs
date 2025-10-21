using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using PocketLawyer.Application.Interfaces;
using PocketLawyer.Options;
using System.ClientModel;

namespace PocketLawyer.Infrastructure.AiServices.Nebius;

public class NebiusService : IAiClient
{
    ILogger<NebiusService> _logger;
    ChatClient _client;
    private readonly string _model;

    public NebiusService(
        IOptions<NebiusOptions> options,
        ILogger<NebiusService> logger
        )
    {
        _logger = logger;
        _model = options.Value.ModelId ?? "";
        _client = new(
            //model: "deepseek-ai/DeepSeek-V3-0324-fast",
            //model: "Qwen/Qwen3-235B-A22B-Thinking-2507",
            //model: "Qwen/Qwen3-32B-fast",
            model: _model,
            credential: new ApiKeyCredential(options.Value.Key ?? ""),
            options: new OpenAIClientOptions()
            {
                Endpoint = new Uri("https://api.studio.nebius.com/v1/"),
                NetworkTimeout = TimeSpan.FromMinutes(10),

            }
        );

    }

    public static System.BinaryData AnalyzerSchemaDefinition => GetAnalizerSchema();
    public static System.BinaryData RentAnalyzerSchemaDefinition => GetRentAnalizerSchema();
    public static System.BinaryData EmploymentAnalyzerSchemaDefinition => GetEmploymentAnalizerSchema();
    public static System.BinaryData PreAnalyzerSchemaDefinition => GetPreAnalizerSchema();

    public async Task<string> SendPrompt(string prompt, object? outputSchema)
    {
        _logger.LogDebug("Entering {Method}", nameof(SendPrompt));
        _logger.LogDebug("Model: {model}",_model);

        List<ChatMessage> messages =
        [
            ChatMessage.CreateSystemMessage(
                "Du bist ein erfahrener Rechtsanwalt, der sich auf das österreichische Recht spezialisiert hat."+
    "Antworte NUR als gültiges JSON nach Schema. Deutsch. " +   
    "Keine Wiederholungen über Abschnitte; " +
    "Längenlimits strikt einhalten."),
            new UserChatMessage(prompt),
         ];

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "Vertragsanalyse",
                jsonSchema: outputSchema as BinaryData,
                jsonSchemaIsStrict: true),
            Temperature = 0.0f,
            MaxOutputTokenCount = 700,
            TopP = 0.1f

        };
        ChatCompletion completion;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(4));
            completion = await _client.CompleteChatAsync(messages, options, cts.Token);
        }
        catch (ClientResultException ex)
        {
            var raw = ex.GetRawResponse(); // enthält Status, Headers, Content
            string body = raw?.Content?.ToString() ?? "(no body)";
            _logger.LogError(ex, "OpenAI 400: {Message}\nResponse body: {Body}\nHeaders: {Headers}",
                ex.Message, body, raw?.Headers);
            return "Error";
        }
        catch (Exception ex)
        {
            _logger.LogError("Error in Nebius Request: {er}", ex.Message);
            _logger.LogError("Content of completion: {comp}", ex.ToString());
            return "Error";
        }

        _logger.LogInformation("Nebius input Token:  {answ}", completion.Usage.InputTokenCount);
        _logger.LogInformation("Nebius output Token: {answ}", completion.Usage.OutputTokenCount);
        _logger.LogInformation("Nebius total Token:  {answ}", completion.Usage.TotalTokenCount);
        _logger.LogDebug(completion.Content[0].Text);

        return completion.Content[0].Text ?? "Error";
    }

    private static System.BinaryData GetRentAnalizerSchema()
    {
        return BinaryData.FromBytes("""
               {
                  "type": "object",
                  "properties": {
                    "fazit": {
                      "type": "string",
                        "maxLength":1500,
                       "description": "Zusammenfassung der Analyse und ob der Mietvertrag von einem Rechstgutachter angesehen werden sollte."
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
                    "keypoints": {
                      "type": "object",
                      "descritpion": "Stichworte, keine ganzen Sätze.",
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
                          "examples": ["01.01.2025, Anfang nächsten Monats"],
                          "description" : "Beginn der Miete"
                        },
                        "dauer": {
                          "type": "string",
                           "examples": ["3 Jahre, unbegrenzt"],
                           "description": "Vereinbarte Dauer des Mietvertrages"
                        },
                        "mindestdauer": {
                          "type": "string",
                           "description": "Mindestdauer des Mietvertrages"
                        },
                        "kuendigungsfrist": {
                          "type": "string",
                        "examples": ["3 Monate, 1 Monat"]

                        },
                        "miete_kalt": {
                          "type": "string",
                            "examples": ["500 Euro, 1.200 Euro"],
                          "description": "Kaltmiete in Euro"
                        },
                        "betriebskosten": {
                          "type": "string",
                            "examples": ["500 Euro, 1.200 Euro"]
                        }
                        ,
                        "quadratmeter": {
                          "type": "string",
                           "examples": ["50m², 120m²"]                           
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
                    "keypoints"
                  ]
                }
            """u8.ToArray());
    }
    private static System.BinaryData GetEmploymentAnalizerSchema()
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
    private static System.BinaryData GetAnalizerSchema()
    {
        return BinaryData.FromBytes("""
               {
                  "type": "object",
                  "properties": {
                    "fazit": {
                      "type": "string",
                        "maxLength":1500,
                       "description": "Zusammenfassung der Analyse und ob der Mietvertrag von einem Rechstgutachter angesehen werden sollte."
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
                    "keypoints": {
                      "type": "object",
                      "descritpion": "Stichworte, keine ganzen Sätze.",
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
                          "examples": ["01.01.2025, Anfang nächsten Monats"],
                          "description" : "Beginn der Miete"
                        },
                        "dauer": {
                          "type": "string",
                           "examples": ["3 Jahre, unbegrenzt"],
                           "description": "Vereinbarte Dauer des Mietvertrages"
                        },
                        "mindestdauer": {
                          "type": "string",
                           "description": "Mindestdauer des Mietvertrages"
                        },
                        "kuendigungsfrist": {
                          "type": "string",
                        "examples": ["3 Monate, 1 Monat"]

                        },
                        "miete_kalt": {
                          "type": "string",
                            "examples": ["500 Euro, 1.200 Euro"],
                          "description": "Kaltmiete in Euro"
                        },
                        "betriebskosten": {
                          "type": "string",
                            "examples": ["500 Euro, 1.200 Euro"]
                        }
                        ,
                        "quadratmeter": {
                          "type": "string",
                           "examples": ["50m², 120m²"]                           
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
                    "keypoints"
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
