using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WebApplication3.ViewModels
{
    public class AdminSignupViewModel
    {
        [MinLength(13), MaxLength(13)]
        public string IdNumber { get; set; }
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.EmailAddress)]
        [Compare("Email")]
        public string ConfirmEmail { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

    }
}