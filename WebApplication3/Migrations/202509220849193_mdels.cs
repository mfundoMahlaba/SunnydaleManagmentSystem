namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class mdels : DbMigration
    {
        public override void Up()
        {
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.StudentPayments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        TransactionId = c.String(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentDate = c.DateTime(nullable: false),
                        Status = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateIndex("dbo.StudentPayments", "StudentId");
            AddForeignKey("dbo.StudentPayments", "StudentId", "dbo.Students", "Id", cascadeDelete: true);
        }
    }
}
