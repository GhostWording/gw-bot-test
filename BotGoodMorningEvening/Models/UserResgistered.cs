namespace BotGoodMorningEvening.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("UserResgistered")]
    public partial class UserResgistered
    {
        [Key]
        public Guid ID { get; set; }

        [StringLength(50)]
        public string UserName { get; set; }

        [StringLength(50)]
        public string UserId { get; set; }

        [StringLength(50)]
        public string BotName { get; set; }

        [StringLength(50)]
        public string BotId { get; set; }

        [StringLength(50)]
        public string ServiceURL { get; set; }

        public int? Gmtplus { get; set; }

        [StringLength(50)]
        public string ChannelId { get; set; }

        public string ResumptionCookie { get; set; }

        public string activityId { get; set; }

        public string conversationId { get; set; }

        public string conversationName { get; set; }

        public string CardsCache { get; set; }
    }
}
