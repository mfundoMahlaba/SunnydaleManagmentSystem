namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class StudentType : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentSubjectEnrollments", "StudentType", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.StudentSubjectEnrollments", "StudentType");
        }
    }
}
