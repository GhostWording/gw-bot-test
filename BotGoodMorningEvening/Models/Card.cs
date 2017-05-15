using System;

namespace BotGoodMorningEvening.Models
{
    [Serializable]
    public class Card
    {
        public string IntentionId { get; set; }
        public string IntentionSlug { get; set; }
        public string PrototypeId { get; set; }
        public string TextId { get; set; }
        public string Content { get; set; }
        public string ImageName { get; set; }
        public string ImageLink { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string ImageCardUrl { get; set; }
    }
}