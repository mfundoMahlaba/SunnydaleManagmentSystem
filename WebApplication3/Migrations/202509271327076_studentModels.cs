namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class studentModels : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Students", "Gender", c => c.String());
            AddColumn("dbo.Students", "Race", c => c.String());
            AddColumn("dbo.Students", "Address", c => c.String());
            AddColumn("dbo.Students", "AlternativePhoneNumber", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Students", "AlternativePhoneNumber");
            DropColumn("dbo.Students", "Address");
            DropColumn("dbo.Students", "Race");
            DropColumn("dbo.Students", "Gender");
        }
    }
}
