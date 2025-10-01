using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using WebApplication3.Migrations;

namespace WebApplication3.Models
{
    public class Student
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]

        [MinLength(13), MaxLength(13)]
        public string IDNumber { get; set; }

        [Required, StringLength(100)]

        public string FullName { get; set; }

        public string Gender { get; set; } 

        public string Race { get; set; }  
        
        public string Address { get; set; }

        public string PhoneNumber { get; set; }
        public string AlternativePhoneNumber { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }
        public int? GradeId { get; set; }
        public virtual Grade Grade { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        public virtual ICollection<TuitionPayment> TuitionPayments { get; set; }
        public string FaceToken { get; set; }


    }
}