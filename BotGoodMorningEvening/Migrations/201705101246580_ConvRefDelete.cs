namespace BotGoodMorningEvening.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ConvRefDelete : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.UserResgistered", "activityId");
            DropColumn("dbo.UserResgistered", "conversationId");
            DropColumn("dbo.UserResgistered", "conversationName");
        }
        
        public override void Down()
        {
            AddColumn("dbo.UserResgistered", "conversationName", c => c.String());
            AddColumn("dbo.UserResgistered", "conversationId", c => c.String());
            AddColumn("dbo.UserResgistered", "activityId", c => c.String());
        }
    }
}
