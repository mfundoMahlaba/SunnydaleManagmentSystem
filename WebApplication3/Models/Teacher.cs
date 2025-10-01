using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace WebApplication3.Models
{
    public class Teacher
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        [MinLength(13), MaxLength(13)]
        public string IdNumber { get; set; }
        [Required, StringLength(200)]
        public string FullName { get; set; }
        [MinLength(10), MaxLength(10)]
        public string PhoneNumber { get; set; }
        public string HomeAddress { get; set; }
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        [DataType(DataType.Password)]
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string FaceToken { get; set; }
    }
}