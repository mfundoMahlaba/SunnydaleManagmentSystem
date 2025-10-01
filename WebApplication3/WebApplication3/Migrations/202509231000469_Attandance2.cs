namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Attandance2 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Periods",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        StartTime = c.Time(nullable: false, precision: 7),
                        EndTime = c.Time(nullable: false, precision: 7),
                    })
                .PrimaryKey(t => t.Id);
            
            AddColumn("dbo.Attendances", "PeriodId", c => c.Int());
            AddColumn("dbo.Attendances", "MarkedByTeacherId", c => c.Int(nullable: false));
            AddColumn("dbo.Attendances", "MarkedAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.Attendances", "GradeId", c => c.Int(nullable: false));
            CreateIndex("dbo.Attendances", "PeriodId");
            CreateIndex("dbo.Attendances", "MarkedByTeacherId");
            AddForeignKey("dbo.Attendances", "MarkedByTeacherId", "dbo.Teachers", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Attendances", "PeriodId", "dbo.Periods", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Attendances", "PeriodId", "dbo.Periods");
            DropForeignKey("dbo.Attendances", "MarkedByTeacherId", "dbo.Teachers");
            DropIndex("dbo.Attendances", new[] { "MarkedByTeacherId" });
            DropIndex("dbo.Attendances", new[] { "PeriodId" });
            DropColumn("dbo.Attendances", "GradeId");
            DropColumn("dbo.Attendances", "MarkedAt");
            DropColumn("dbo.Attendances", "MarkedByTeacherId");
            DropColumn("dbo.Attendances", "PeriodId");
            DropTable("dbo.Periods");
        }
    }
}
