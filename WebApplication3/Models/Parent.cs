using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class Parent
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public string LinkedStudentID { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public string PhoneNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string FaceToken { get; set; }
    }
}
