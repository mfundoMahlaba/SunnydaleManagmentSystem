namespace WebApplication3.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Sengane : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Parents",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FullName = c.String(nullable: false, maxLength: 100),
                        LinkedStudentID = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        PasswordHash = c.String(nullable: false),
                        PhoneNumber = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        FaceToken = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UniformCategories",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 80),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.UniformItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 120),
                        Description = c.String(maxLength: 1000),
                        Price = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Stock = c.Int(nullable: false),
                        IsActive = c.Boolean(nullable: false),
                        ImagePath = c.String(maxLength: 260),
                        CategoryId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UniformCategories", t => t.CategoryId, cascadeDelete: true)
                .Index(t => t.CategoryId);
            
            CreateTable(
                "dbo.UniformOrderItems",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Size = c.String(),
                        OrderId = c.Int(nullable: false),
                        UniformItemId = c.Int(nullable: false),
                        Quantity = c.Int(nullable: false),
                        UnitPrice = c.Decimal(nullable: false, precision: 18, scale: 2),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UniformOrders", t => t.OrderId, cascadeDelete: true)
                .ForeignKey("dbo.UniformItems", t => t.UniformItemId, cascadeDelete: true)
                .Index(t => t.OrderId)
                .Index(t => t.UniformItemId);
            
            CreateTable(
                "dbo.UniformOrders",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ParentId = c.Int(nullable: false),
                        OrderDate = c.DateTime(nullable: false),
                        Status = c.String(nullable: false, maxLength: 30),
                        TotalAmount = c.Decimal(nullable: false, precision: 18, scale: 2),
                        RecipientName = c.String(nullable: false, maxLength: 120),
                        RecipientEmail = c.String(nullable: false, maxLength: 120),
                        TransactionId = c.String(maxLength: 100),
                        PaidAmount = c.Decimal(precision: 18, scale: 2),
                        PaymentDate = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Parents", t => t.ParentId, cascadeDelete: true)
                .Index(t => t.ParentId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.UniformOrderItems", "UniformItemId", "dbo.UniformItems");
            DropForeignKey("dbo.UniformOrders", "ParentId", "dbo.Parents");
            DropForeignKey("dbo.UniformOrderItems", "OrderId", "dbo.UniformOrders");
            DropForeignKey("dbo.UniformItems", "CategoryId", "dbo.UniformCategories");
            DropIndex("dbo.UniformOrders", new[] { "ParentId" });
            DropIndex("dbo.UniformOrderItems", new[] { "UniformItemId" });
            DropIndex("dbo.UniformOrderItems", new[] { "OrderId" });
            DropIndex("dbo.UniformItems", new[] { "CategoryId" });
            DropTable("dbo.UniformOrders");
            DropTable("dbo.UniformOrderItems");
            DropTable("dbo.UniformItems");
            DropTable("dbo.UniformCategories");
            DropTable("dbo.Parents");
        }
    }
}
