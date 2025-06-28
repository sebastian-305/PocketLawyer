using System.Text.Json.Serialization;

namespace PocketLawyer.Infrastructure.AiServices.Gemini.Models;


public class GeminiContext
{
    [JsonPropertyName("cachedContents")]
    public CachedContents[] CachedContents { get; set; }
}

public class CachedContents
{

    [JsonPropertyName("name")]
    public string Name { get; set; }
    [JsonPropertyName("model")]
    public string Model { get; set; }
    [JsonPropertyName("createTime")]
    public DateTime CreateTime { get; set; }
    [JsonPropertyName("updateTime")]
    public DateTime UpdateTime { get; set; }
    [JsonPropertyName("expireTime")]
    public string ExpireTime { get; set; }
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }
    [JsonPropertyName("usageMetadata")]
    public Usagemetadata UsageMetadata { get; set; }

    public class Usagemetadata
    {
        [JsonPropertyName("totalTokenCount")]
        public int TotalTokenCount { get; set; }
    }

}