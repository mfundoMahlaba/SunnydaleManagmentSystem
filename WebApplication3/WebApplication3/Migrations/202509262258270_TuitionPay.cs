namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class TuitionPay : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.TuitionPayments", "PaymentDate", c => c.DateTime(nullable: false));
            AddColumn("dbo.TuitionPayments", "Status", c => c.String(nullable: false, maxLength: 50));
            AddColumn("dbo.TuitionPayments", "StripePublishableKey", c => c.String(maxLength: 100));
            AddColumn("dbo.TuitionPayments", "TransactionId", c => c.String());
            DropColumn("dbo.TuitionPayments", "PaidAt");
            DropColumn("dbo.TuitionPayments", "StripePaymentIntentId");
        }
        
        public override void Down()
        {
            AddColumn("dbo.TuitionPayments", "StripePaymentIntentId", c => c.String());
            AddColumn("dbo.TuitionPayments", "PaidAt", c => c.DateTime(nullable: false));
            DropColumn("dbo.TuitionPayments", "TransactionId");
            DropColumn("dbo.TuitionPayments", "StripePublishableKey");
            DropColumn("dbo.TuitionPayments", "Status");
            DropColumn("dbo.TuitionPayments", "PaymentDate");
        }
    }
}
