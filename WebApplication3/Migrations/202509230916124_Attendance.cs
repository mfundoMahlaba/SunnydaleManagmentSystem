namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Attendance : DbMigration
    {
        public override void Up()
        {
            // Step 1: Add GradeId as nullable
            AddColumn("dbo.Students", "GradeId", c => c.Int());

            // Step 2: Populate GradeId for existing students (e.g., default to Grade 8)
            Sql("UPDATE dbo.Students SET GradeId = 1 WHERE GradeId IS NULL");

            // Step 3: Alter column to NOT NULL
            AlterColumn("dbo.Students", "GradeId", c => c.Int(nullable: false));

            // Step 4: Add FK and index
            CreateIndex("dbo.Students", "GradeId");
            AddForeignKey("dbo.Students", "GradeId", "dbo.Grades", "Id");
        }


        public override void Down()
        {
            DropForeignKey("dbo.Students", "GradeId", "dbo.Grades");
            DropIndex("dbo.Students", new[] { "GradeId" });
            DropColumn("dbo.Students", "GradeId");
        }
    }
}
