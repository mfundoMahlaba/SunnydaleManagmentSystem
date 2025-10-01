namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EnrollmentYear : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentSubjectEnrollments", "Year", c => c.Int(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentSubjectEnrollments", "Year");
        }
    }
}
