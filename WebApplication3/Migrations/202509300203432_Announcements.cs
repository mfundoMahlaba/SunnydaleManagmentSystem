namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Announcements : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Announcements",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        GradeId = c.Int(nullable: false),
                        Title = c.String(),
                        Message = c.String(),
                        SentAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grades", t => t.GradeId, cascadeDelete: true)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: false)
                .ForeignKey("dbo.Teachers", t => t.TeacherId, cascadeDelete: true)
                .Index(t => t.TeacherId)
                .Index(t => t.SubjectId)
                .Index(t => t.GradeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Announcements", "TeacherId", "dbo.Teachers");
            DropForeignKey("dbo.Announcements", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.Announcements", "GradeId", "dbo.Grades");
            DropIndex("dbo.Announcements", new[] { "GradeId" });
            DropIndex("dbo.Announcements", new[] { "SubjectId" });
            DropIndex("dbo.Announcements", new[] { "TeacherId" });
            DropTable("dbo.Announcements");
        }
    }
}
