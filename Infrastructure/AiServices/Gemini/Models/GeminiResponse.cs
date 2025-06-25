namespace PocketLawyer.Infrastructure.AiServices.Gemini.Models;

//File from LectureEvaluationAPI2 repository

public class GeminiResponse
{
    public Candidates[] candidates { get; set; }
    public UsageMetadata usageMetadata { get; set; }
    public string modelVersion { get; set; }
}

public class Candidates
{
    public Content content { get; set; }
    public string finishReason { get; set; }
    public double avgLogprobs { get; set; }
}

public class Content
{
    public Parts[] parts { get; set; }
    public string role { get; set; }
}

public class Parts
{
    public string text { get; set; }
}

public class UsageMetadata
{
    public int promptTokenCount { get; set; }
    public int candidatesTokenCount { get; set; }
    public int totalTokenCount { get; set; }
    public PromptTokensDetails[] promptTokensDetails { get; set; }
    public CandidatesTokensDetails[] candidatesTokensDetails { get; set; }
}

public class PromptTokensDetails
{
    public string modality { get; set; }
    public int tokenCount { get; set; }
}

public class CandidatesTokensDetails
{
    public string modality { get; set; }
    public int tokenCount { get; set; }
}
