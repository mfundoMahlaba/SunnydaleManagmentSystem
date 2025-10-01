namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddPaymentModel : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Students", "StripeCustomerId", c => c.String());
            AddColumn("dbo.TuitionPayments", "PaidAt", c => c.DateTime(nullable: false));
            AddColumn("dbo.TuitionPayments", "StripePaymentIntentId", c => c.String());
            DropColumn("dbo.TuitionPayments", "PaymentDate");
            DropColumn("dbo.TuitionPayments", "Status");
            DropColumn("dbo.TuitionPayments", "TransactionId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TuitionPayments", "TransactionId", c => c.String(maxLength: 100));
            AddColumn("dbo.TuitionPayments", "Status", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.TuitionPayments", "PaymentDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.TuitionPayments", "StripePaymentIntentId");
            DropColumn("dbo.TuitionPayments", "PaidAt");
            DropColumn("dbo.Students", "StripeCustomerId");
        }
    }
}
