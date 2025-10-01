namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModelsFace : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Teachers", "FaceToken", c => c.String());
            AddColumn("dbo.Students", "FaceToken", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Students", "FaceToken");
            DropColumn("dbo.Teachers", "FaceToken");
        }
    }
}
