using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication3.Attributes;

namespace WebApplication3.ViewModels
{
    public class StudentSignupViewModel
    {
        [Required]
        [MinLength(13), MaxLength(13)]
        [Display(Name = "ID Number")]
        public string IDNumber { get; set; }

        [Required, StringLength(300)]

        public string FullName { get; set; }
        [MinLength(10), MaxLength(10)]
        public string PhoneNumber { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }
        [Required, DataType(DataType.Password)]
        [System.Web.Mvc.Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "Gender")]
        public string Gender { get; set; }

        public List<SelectListItem> GenderOptions { get; set; }

        public string Address { get; set; }

        public string AlternativePhoneNumber { get; set; }
        [Required]
        [Display(Name = "Race")]
        public string Race { get; set; }

        public List<SelectListItem> RaceOptions { get; set; }

    }
}