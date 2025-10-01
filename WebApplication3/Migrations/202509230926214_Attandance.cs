namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Attandance : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Attendances",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        Date = c.DateTime(nullable: false),
                        IsPresent = c.Boolean(nullable: false),
                        Remarks = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.StudentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendances", "StudentId", "dbo.Students");
            DropIndex("dbo.Attendances", new[] { "StudentId" });
            DropTable("dbo.Attendances");
        }
    }
}
