namespace BotGoodMorningEvening.Helpers
{
    public class FacebookChannelData
    {
        [Newtonsoft.Json.JsonProperty("quick_replies")]
        public FacebookTextQuickReply[] QuickReplies { get; set; }
    }
}