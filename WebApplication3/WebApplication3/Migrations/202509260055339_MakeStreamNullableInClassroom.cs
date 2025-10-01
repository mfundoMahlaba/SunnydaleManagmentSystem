
namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class MakeStreamNullableInClassroom : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Classrooms", "StreamId", c => c.Int());
            CreateIndex("dbo.Classrooms", "StreamId");
            AddForeignKey("dbo.Classrooms", "StreamId", "dbo.Streams", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Classrooms", "StreamId", "dbo.Streams");
            DropIndex("dbo.Classrooms", new[] { "StreamId" });
            DropColumn("dbo.Classrooms", "StreamId");
        }
    }
}
