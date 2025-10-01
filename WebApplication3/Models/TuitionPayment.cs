using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication3.Models
{
    public class TuitionPayment
    {
        [Key]
        public int PaymentId { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual Student Student { get; set; }

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        public string Status { get; set; } // "Paid" or "Unpaid"

        [StringLength(100)]
        public string StripePublishableKey { get; set; }

        public string TransactionId { get; set; }








    }
}
