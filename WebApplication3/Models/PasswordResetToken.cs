using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        public string Email { get; set; }

        public string Token { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}