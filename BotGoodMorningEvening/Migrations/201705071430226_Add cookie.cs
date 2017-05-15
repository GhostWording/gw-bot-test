namespace BotGoodMorningEvening.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addcookie : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserResgistered", "ResumptionCookie", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserResgistered", "ResumptionCookie");
        }
    }
}
