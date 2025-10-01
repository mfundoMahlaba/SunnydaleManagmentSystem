namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Models3 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Marks",
                c => new
                    {
                        MarkId = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        GradeId = c.Int(nullable: false),
                        TeacherId = c.Int(nullable: false),
                        Score = c.Double(nullable: false),
                    })
                .PrimaryKey(t => t.MarkId)
                .ForeignKey("dbo.Grades", t => t.GradeId, cascadeDelete: false)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .Index(t => t.StudentId)
                .Index(t => t.SubjectId)
                .Index(t => t.GradeId);
            
            CreateTable(
                "dbo.StudentSubjectEnrollments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        GradeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grades", t => t.GradeId, cascadeDelete: false)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .Index(t => t.StudentId)
                .Index(t => t.SubjectId)
                .Index(t => t.GradeId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudentSubjectEnrollments", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.StudentSubjectEnrollments", "StudentId", "dbo.Students");
            DropForeignKey("dbo.StudentSubjectEnrollments", "GradeId", "dbo.Grades");
            DropForeignKey("dbo.Marks", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.Marks", "StudentId", "dbo.Students");
            DropForeignKey("dbo.Marks", "GradeId", "dbo.Grades");
            DropIndex("dbo.StudentSubjectEnrollments", new[] { "GradeId" });
            DropIndex("dbo.StudentSubjectEnrollments", new[] { "SubjectId" });
            DropIndex("dbo.StudentSubjectEnrollments", new[] { "StudentId" });
            DropIndex("dbo.Marks", new[] { "GradeId" });
            DropIndex("dbo.Marks", new[] { "SubjectId" });
            DropIndex("dbo.Marks", new[] { "StudentId" });
            DropTable("dbo.StudentSubjectEnrollments");
            DropTable("dbo.Marks");
        }
    }
}
