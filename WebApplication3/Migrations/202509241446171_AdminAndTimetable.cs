namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AdminAndTimetable : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Admins", "FaceToken", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Admins", "FaceToken");
        }
    }
}
