namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Models2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TeacherSubjectAssignments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        TeacherId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        GradeId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grades", t => t.GradeId, cascadeDelete: true)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: false)
                .ForeignKey("dbo.Teachers", t => t.TeacherId, cascadeDelete: true)
                .Index(t => t.TeacherId)
                .Index(t => t.SubjectId)
                .Index(t => t.GradeId);
            
            AddColumn("dbo.Classrooms", "Capacity", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TeacherSubjectAssignments", "TeacherId", "dbo.Teachers");
            DropForeignKey("dbo.TeacherSubjectAssignments", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.TeacherSubjectAssignments", "GradeId", "dbo.Grades");
            DropIndex("dbo.TeacherSubjectAssignments", new[] { "GradeId" });
            DropIndex("dbo.TeacherSubjectAssignments", new[] { "SubjectId" });
            DropIndex("dbo.TeacherSubjectAssignments", new[] { "TeacherId" });
            DropColumn("dbo.Classrooms", "Capacity");
            DropTable("dbo.TeacherSubjectAssignments");
        }
    }
}
