using System.Text.Json.Serialization;

namespace PocketLawyer.Infrastructure.AiServices.Gemini.Models;


public class GeminiResponse
{
    [JsonPropertyName("candidates")]
    public Candidates[] Candidates { get; set; }
    [JsonPropertyName("usageMetadata")]
    public UsageMetadata UsageMetadata { get; set; }
    [JsonPropertyName("modelVersion")]
    public string ModelVersion { get; set; }
}

public class Candidates
{
    [JsonPropertyName("content")]
    public Content Content { get; set; }
    [JsonPropertyName("finishReason")]
    public string FinishReason { get; set; }
    [JsonPropertyName("avgLogprobs")]
    public double AvgLogprobs { get; set; }
}

public class Content
{
    [JsonPropertyName("parts")]
    public Parts[] Parts { get; set; }
    [JsonPropertyName("role")]
    public string Role { get; set; }
}

public class Parts
{
    [JsonPropertyName("text")]
    public string Text { get; set; }
}

public class UsageMetadata
{
    [JsonPropertyName("promptTokenCount")]
    public int PromptTokenCount { get; set; }
    [JsonPropertyName("candidatesTokenCount")]
    public int CandidatesTokenCount { get; set; }
    [JsonPropertyName("totalTokenCount")]
    public int TotalTokenCount { get; set; }
    [JsonPropertyName("promptTokensDetails")]
    public PromptTokensDetails[] PromptTokensDetails { get; set; }
    [JsonPropertyName("candidatesTokensDetails")]
    public CandidatesTokensDetails[] CandidatesTokensDetails { get; set; }
}

public class PromptTokensDetails
{
    [JsonPropertyName("modality")]
    public string Modality { get; set; }
    [JsonPropertyName("tokenCount")]
    public int TokenCount { get; set; }
}

public class CandidatesTokensDetails
{
    [JsonPropertyName("modality")]
    public string Modality { get; set; }
    [JsonPropertyName("tokenCount")]
    public int TokenCount { get; set; }
}
