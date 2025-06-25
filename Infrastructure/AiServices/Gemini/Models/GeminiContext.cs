namespace PocketLawyer.Infrastructure.AiServices.Gemini.Models;


public class GeminiContext
{
    public CachedContents[] cachedContents { get; set; }

    public class CachedContents
    {

        public string name { get; set; }
        public string model { get; set; }
        public DateTime createTime { get; set; }
        public DateTime updateTime { get; set; }
        public string expireTime { get; set; }
        public string displayName { get; set; }
        public Usagemetadata usageMetadata { get; set; }

        public class Usagemetadata
        {
            public int totalTokenCount { get; set; }
        }

    }




}