namespace BotGoodMorningEvening.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitDB : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserResgistered",
                c => new
                    {
                        ID = c.Guid(nullable: false),
                        UserName = c.String(maxLength: 50),
                        UserId = c.String(maxLength: 50),
                        BotName = c.String(maxLength: 50),
                        BotId = c.String(maxLength: 50),
                        ServiceURL = c.String(maxLength: 50),
                        Gmtplus = c.Int(),
                    })
                .PrimaryKey(t => t.ID);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserResgistered");
        }
    }
}
