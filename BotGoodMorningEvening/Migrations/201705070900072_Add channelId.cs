namespace BotGoodMorningEvening.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddchannelId : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserResgistered", "ChannelId", c => c.String(maxLength: 50));
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserResgistered", "ChannelId");
        }
    }
}
