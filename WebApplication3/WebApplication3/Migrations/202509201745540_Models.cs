namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Models : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Admins",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdNumber = c.String(maxLength: 13),
                        FullName = c.String(nullable: false, maxLength: 100),
                        Email = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                        PhoneNumber = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Classrooms",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        GradeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grades", t => t.GradeId, cascadeDelete: true)
                .Index(t => t.GradeId);
            
            CreateTable(
                "dbo.Grades",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GradeLevel = c.Int(nullable: false),
                        Description = c.String(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Subjects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        SubjectCode = c.String(),
                        StreamId = c.Int(),
                        GradeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grades", t => t.GradeId, cascadeDelete: true)
                .ForeignKey("dbo.Streams", t => t.StreamId)
                .Index(t => t.StreamId)
                .Index(t => t.GradeId);
            
            CreateTable(
                "dbo.Streams",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EnrollmentDocuments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        FileName = c.String(),
                        FilePath = c.String(nullable: false),
                        DocumentType = c.String(nullable: false),
                        Status = c.String(),
                        IsNotified = c.Boolean(nullable: false),
                        AdminComment = c.String(),
                        UploadedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.StudentId);
            
            CreateTable(
                "dbo.Students",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IDNumber = c.String(nullable: false, maxLength: 13),
                        FullName = c.String(nullable: false, maxLength: 100),
                        PhoneNumber = c.String(),
                        Email = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.EnrollmentSubjects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        EnrollmentRequestId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.GradeSubjects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Grade = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.PreviousResults",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        Grade = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        Percentage = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .Index(t => t.StudentId)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.StreamSubjects",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StreamId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Streams", t => t.StreamId, cascadeDelete: true)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .Index(t => t.StreamId)
                .Index(t => t.SubjectId);
            
            CreateTable(
                "dbo.Teachers",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        IdNumber = c.String(nullable: false, maxLength: 13),
                        FullName = c.String(nullable: false, maxLength: 200),
                        PhoneNumber = c.String(maxLength: 10),
                        HomeAddress = c.String(),
                        Email = c.String(),
                        PasswordHash = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StreamSubjects", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.StreamSubjects", "StreamId", "dbo.Streams");
            DropForeignKey("dbo.PreviousResults", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.PreviousResults", "StudentId", "dbo.Students");
            DropForeignKey("dbo.GradeSubjects", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.EnrollmentSubjects", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.EnrollmentDocuments", "StudentId", "dbo.Students");
            DropForeignKey("dbo.Subjects", "StreamId", "dbo.Streams");
            DropForeignKey("dbo.Subjects", "GradeId", "dbo.Grades");
            DropForeignKey("dbo.Classrooms", "GradeId", "dbo.Grades");
            DropIndex("dbo.StreamSubjects", new[] { "SubjectId" });
            DropIndex("dbo.StreamSubjects", new[] { "StreamId" });
            DropIndex("dbo.PreviousResults", new[] { "SubjectId" });
            DropIndex("dbo.PreviousResults", new[] { "StudentId" });
            DropIndex("dbo.GradeSubjects", new[] { "SubjectId" });
            DropIndex("dbo.EnrollmentSubjects", new[] { "SubjectId" });
            DropIndex("dbo.EnrollmentDocuments", new[] { "StudentId" });
            DropIndex("dbo.Subjects", new[] { "GradeId" });
            DropIndex("dbo.Subjects", new[] { "StreamId" });
            DropIndex("dbo.Classrooms", new[] { "GradeId" });
            DropTable("dbo.Teachers");
            DropTable("dbo.StreamSubjects");
            DropTable("dbo.PreviousResults");
            DropTable("dbo.GradeSubjects");
            DropTable("dbo.EnrollmentSubjects");
            DropTable("dbo.Students");
            DropTable("dbo.EnrollmentDocuments");
            DropTable("dbo.Streams");
            DropTable("dbo.Subjects");
            DropTable("dbo.Grades");
            DropTable("dbo.Classrooms");
            DropTable("dbo.Admins");
        }
    }
}
