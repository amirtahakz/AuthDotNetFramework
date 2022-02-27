namespace Ui.Data.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddUserRefreshTokensTbl : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.UserRefreshTokens",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        UserId = c.String(),
                        RefreshToken = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        IsRemove = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.UserRefreshTokens");
        }
    }
}
