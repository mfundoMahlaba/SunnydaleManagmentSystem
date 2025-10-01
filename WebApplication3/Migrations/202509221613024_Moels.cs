namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Moels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Marks", "Term", c => c.String());
            AddColumn("dbo.Marks", "AssessmentType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Marks", "AssessmentType");
            DropColumn("dbo.Marks", "Term");
        }
    }
}
