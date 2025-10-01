namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Paymnt : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TuitionPayments",
                c => new
                    {
                        PaymentId = c.Int(nullable: false, identity: true),
                        StudentId = c.Int(nullable: false),
                        Amount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        PaymentDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 50),
                        TransactionId = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.PaymentId)
                .ForeignKey("dbo.Students", t => t.StudentId, cascadeDelete: true)
                .Index(t => t.StudentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.TuitionPayments", "StudentId", "dbo.Students");
            DropIndex("dbo.TuitionPayments", new[] { "StudentId" });
            DropTable("dbo.TuitionPayments");
        }
    }
}
