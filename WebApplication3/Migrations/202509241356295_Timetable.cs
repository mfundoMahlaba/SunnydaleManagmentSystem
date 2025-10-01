namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Timetable : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ClassTimetables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        GradeId = c.Int(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        TeacherId = c.Int(nullable: false),
                        DayOfWeek = c.String(),
                        TimeSlot = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Grades", t => t.GradeId, cascadeDelete: true)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: false)
                .ForeignKey("dbo.Teachers", t => t.TeacherId, cascadeDelete: true)
                .Index(t => t.GradeId)
                .Index(t => t.SubjectId)
                .Index(t => t.TeacherId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ClassTimetables", "TeacherId", "dbo.Teachers");
            DropForeignKey("dbo.ClassTimetables", "SubjectId", "dbo.Subjects");
            DropForeignKey("dbo.ClassTimetables", "GradeId", "dbo.Grades");
            DropIndex("dbo.ClassTimetables", new[] { "TeacherId" });
            DropIndex("dbo.ClassTimetables", new[] { "SubjectId" });
            DropIndex("dbo.ClassTimetables", new[] { "GradeId" });
            DropTable("dbo.ClassTimetables");
        }
    }
}
