namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Models1 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Enrollments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        Year = c.Int(nullable: false),
                        Grade = c.Int(nullable: false),
                        StreamId = c.Int(),
                        StudentType = c.String(nullable: false, maxLength: 20),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Streams", t => t.StreamId)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.StudentId)
                .Index(t => t.StreamId);
            
            AddColumn("dbo.EnrollmentSubjects", "EnrollmentId", c => c.Int(nullable: false));
            AddColumn("dbo.EnrollmentSubjects", "IsSelected", c => c.Boolean(nullable: false));
            CreateIndex("dbo.EnrollmentSubjects", "EnrollmentId");
            AddForeignKey("dbo.EnrollmentSubjects", "EnrollmentId", "dbo.Enrollments", "Id", cascadeDelete: true);
            DropColumn("dbo.EnrollmentSubjects", "EnrollmentRequestId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.EnrollmentSubjects", "EnrollmentRequestId", c => c.Int(nullable: false));
            DropForeignKey("dbo.Enrollments", "StudentId", "dbo.Students");
            DropForeignKey("dbo.Enrollments", "StreamId", "dbo.Streams");
            DropForeignKey("dbo.EnrollmentSubjects", "EnrollmentId", "dbo.Enrollments");
            DropIndex("dbo.EnrollmentSubjects", new[] { "EnrollmentId" });
            DropIndex("dbo.Enrollments", new[] { "StreamId" });
            DropIndex("dbo.Enrollments", new[] { "StudentId" });
            DropColumn("dbo.EnrollmentSubjects", "IsSelected");
            DropColumn("dbo.EnrollmentSubjects", "EnrollmentId");
            DropTable("dbo.Enrollments");
        }
    }
}
