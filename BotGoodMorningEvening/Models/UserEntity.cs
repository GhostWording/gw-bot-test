using Microsoft.WindowsAzure.Storage.Table;

namespace BotGoodMorningEvening.Models
{
    public class UserEntity : TableEntity
    {
        public UserEntity( string partitionKey, string userId)
        {
            this.RowKey = userId;
            this.PartitionKey = partitionKey;
        }

        public UserEntity() { }

        public string UserName { get; set; }

        public string UserId { get; set; }

        public string BotName { get; set; }

        public string BotId { get; set; }

        public string ServiceURL { get; set; }

        public int? Gmtplus { get; set; }

        public string ChannelId { get; set; }

        public string ResumptionCookie { get; set; }

        public string CardsCache { get; set; }
    }
}