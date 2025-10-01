namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Streamenrollment : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.StudentSubjectEnrollments", "StreamId", c => c.Int());
            CreateIndex("dbo.StudentSubjectEnrollments", "StreamId");
            AddForeignKey("dbo.StudentSubjectEnrollments", "StreamId", "dbo.Streams", "Id");
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.StudentSubjectEnrollments", "StreamId", "dbo.Streams");
            DropIndex("dbo.StudentSubjectEnrollments", new[] { "StreamId" });
            DropColumn("dbo.StudentSubjectEnrollments", "StreamId");
        }
    }
}
