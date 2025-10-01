using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class TuitionPaymentViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public string Grade { get; set; }

        public decimal Amount { get; set; }
        public bool HasPaid { get; set; }

        public string TransactionId { get; set; }
        public DateTime? PaymentDate { get; set; }

        public string StripePublishableKey { get; set; }
    }
    public class PaymentSuccessViewModel
    {
        public int StudentId { get; set; }
        public string FullName { get; set; }
        public decimal Amount { get; set; }
        public string TransactionId { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Status { get; set; }
    }


}
