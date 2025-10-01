namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Timetable1 : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.ClassTimetables", "GradeId", "dbo.Grades");
            DropIndex("dbo.ClassTimetables", new[] { "GradeId" });
            AddColumn("dbo.ClassTimetables", "ClassroomId", c => c.Int(nullable: false));
            CreateIndex("dbo.ClassTimetables", "ClassroomId");
            AddForeignKey("dbo.ClassTimetables", "ClassroomId", "dbo.Classrooms", "Id", cascadeDelete: true);
            DropColumn("dbo.ClassTimetables", "GradeId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.ClassTimetables", "GradeId", c => c.Int(nullable: false));
            DropForeignKey("dbo.ClassTimetables", "ClassroomId", "dbo.Classrooms");
            DropIndex("dbo.ClassTimetables", new[] { "ClassroomId" });
            DropColumn("dbo.ClassTimetables", "ClassroomId");
            CreateIndex("dbo.ClassTimetables", "GradeId");
            AddForeignKey("dbo.ClassTimetables", "GradeId", "dbo.Grades", "Id", cascadeDelete: true);
        }
    }
}
