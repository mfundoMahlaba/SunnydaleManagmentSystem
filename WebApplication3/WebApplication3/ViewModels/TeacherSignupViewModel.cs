using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class TeacherSignupViewModel
    {
        [MinLength(13), MaxLength(13)]
        public string IdNumber { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }
        [MinLength(10), MaxLength(10)]
        public string PhoneNumber { get; set; }
        public string HomeAddress { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [Compare("Password")]
        public string ConfirmPassword { get; set; }
    }

    public class TeacherProfileViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string FullName { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MinLength(10), MaxLength(10)]
        public string PhoneNumber { get; set; }

        public string HomeAddress { get; set; }
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }


}