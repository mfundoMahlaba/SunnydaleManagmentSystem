namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class model : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Students", "GradeId", "dbo.Grades");
            DropIndex("dbo.Students", new[] { "GradeId" });
            AlterColumn("dbo.Students", "GradeId", c => c.Int());
            CreateIndex("dbo.Students", "GradeId");
            AddForeignKey("dbo.Students", "GradeId", "dbo.Grades", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Students", "GradeId", "dbo.Grades");
            DropIndex("dbo.Students", new[] { "GradeId" });
            AlterColumn("dbo.Students", "GradeId", c => c.Int(nullable: false));
            CreateIndex("dbo.Students", "GradeId");
            AddForeignKey("dbo.Students", "GradeId", "dbo.Grades", "Id", cascadeDelete: true);
        }
    }
}
