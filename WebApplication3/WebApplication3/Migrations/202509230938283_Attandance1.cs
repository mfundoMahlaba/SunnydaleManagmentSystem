namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Attandance1 : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Attendances", "SubjectId", c => c.Int());
            CreateIndex("dbo.Attendances", "SubjectId");
            AddForeignKey("dbo.Attendances", "SubjectId", "dbo.Subjects", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendances", "SubjectId", "dbo.Subjects");
            DropIndex("dbo.Attendances", new[] { "SubjectId" });
            DropColumn("dbo.Attendances", "SubjectId");
        }
    }
}
