namespace BotGoodMorningEvening.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Addcardscachestorage : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.UserResgistered", "CardsCache", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.UserResgistered", "CardsCache");
        }
    }
}
