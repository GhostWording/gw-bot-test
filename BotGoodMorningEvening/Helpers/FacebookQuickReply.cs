using Newtonsoft.Json;

namespace BotGoodMorningEvening.Helpers
{
    public abstract class FacebookQuickReply
    {
        public FacebookQuickReply(string contentType)
        {
            this.ContentType = contentType;
        }

        [JsonProperty("content_type")]
        public string ContentType { get; set; }
    }
}