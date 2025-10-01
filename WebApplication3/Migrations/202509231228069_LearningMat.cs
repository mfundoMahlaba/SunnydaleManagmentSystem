namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class LearningMat : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.LearningMaterials",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Title = c.String(nullable: false),
                        Description = c.String(),
                        FilePath = c.String(nullable: false),
                        UploadedAt = c.DateTime(nullable: false),
                        SubjectId = c.Int(nullable: false),
                        UploadedByTeacherId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subjects", t => t.SubjectId, cascadeDelete: true)
                .ForeignKey("dbo.Teachers", t => t.UploadedByTeacherId, cascadeDelete: true)
                .Index(t => t.SubjectId)
                .Index(t => t.UploadedByTeacherId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.LearningMaterials", "UploadedByTeacherId", "dbo.Teachers");
            DropForeignKey("dbo.LearningMaterials", "SubjectId", "dbo.Subjects");
            DropIndex("dbo.LearningMaterials", new[] { "UploadedByTeacherId" });
            DropIndex("dbo.LearningMaterials", new[] { "SubjectId" });
            DropTable("dbo.LearningMaterials");
        }
    }
}
