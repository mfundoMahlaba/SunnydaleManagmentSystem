namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Models4 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Classrooms", "Stream_Id", c => c.Int());
            CreateIndex("dbo.Classrooms", "Stream_Id");
            AddForeignKey("dbo.Classrooms", "Stream_Id", "dbo.Streams", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Classrooms", "Stream_Id", "dbo.Streams");
            DropIndex("dbo.Classrooms", new[] { "Stream_Id" });
            DropColumn("dbo.Classrooms", "Stream_Id");
        }
    }
}
