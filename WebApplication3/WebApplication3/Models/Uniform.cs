using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.UI.WebControls.WebParts;

namespace WebApplication3.Models
{
    public class UniformOrderItem
    {
        [Key]
        public int Id { get; set; }

        public string Size { get; set; }

        public int OrderId { get; set; }
        public virtual UniformOrder Order { get; set; }

        public int UniformItemId { get; set; }
        public virtual UniformItem UniformItem { get; set; }

        [Range(1, 1000)]
        public int Quantity { get; set; }

        [Column(TypeName = "decimal")]
        [Range(0, 999999)]
        public decimal UnitPrice { get; set; } // captured at purchase time

        [NotMapped]
        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class UniformOrder
    {
        [Key]
        public int Id { get; set; }

        public int ParentId { get; set; }       // Link to Parent
        public virtual Parent Parent { get; set; }

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required, StringLength(30)]
        public string Status { get; set; } = "Pending";
        // Pending, Confirmed, Cancelled, Fulfilled

        [Column(TypeName = "decimal")]
        [Range(0, 9999999)]
        public decimal TotalAmount { get; set; }

        // Snapshot of recipient info at order time
        [Required, StringLength(120)]
        public string RecipientName { get; set; }

        [Required, StringLength(120), EmailAddress]
        public string RecipientEmail { get; set; }

        public virtual ICollection<UniformOrderItem> Items { get; set; } = new List<UniformOrderItem>();

        // ✅ Payment Tracking
        [StringLength(100)]
        public string TransactionId { get; set; }   // From PayFast (pf_payment_id)

        [Column(TypeName = "decimal")]
        public decimal? PaidAmount { get; set; }    // Amount received

        public DateTime? PaymentDate { get; set; }  // When payment confirmed
    }

    public class UniformItem
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(120)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Column(TypeName = "decimal")]
        [Range(0, 999999)]
        public decimal Price { get; set; } // Configure precision in DbContext

        [Range(0, 100000)]
        public int Stock { get; set; }

        public bool IsActive { get; set; } = true;

        [StringLength(260)]
        public string ImagePath { get; set; } // ~/Upload/xxx.jpg

        // FK
        [Display(Name = "Category")]
        public int CategoryId { get; set; }
        public virtual UniformCategory Category { get; set; }
    }

    public class UniformCategory
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(80)]
        public string Name { get; set; }

        public virtual ICollection<UniformItem> Items { get; set; } = new List<UniformItem>();
    }

    public class CartItemVm
    {
        [Key]
        public int ItemId { get; set; }
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Size { get; set; } // New field
        public string ImagePath { get; set; }

        public decimal Subtotal => UnitPrice * Quantity;
    }

    public class CheckoutVm
    {
        [Required, StringLength(120)]
        public string RecipientName { get; set; }

        [Required, EmailAddress, StringLength(120)]
        public string RecipientEmail { get; set; }
    }

}