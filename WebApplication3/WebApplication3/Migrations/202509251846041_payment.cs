namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class payment : DbMigration
    {
        public override void Up()
        {
            DropColumn("dbo.Students", "StripeCustomerId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Students", "StripeCustomerId", c => c.String());
        }
    }
}
