namespace BotGoodMorningEvening.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Conv : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserResgistered", "activityId", c => c.String());
            AddColumn("dbo.UserResgistered", "conversationId", c => c.String());
            AddColumn("dbo.UserResgistered", "conversationName", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserResgistered", "conversationName");
            DropColumn("dbo.UserResgistered", "conversationId");
            DropColumn("dbo.UserResgistered", "activityId");
        }
    }
}
