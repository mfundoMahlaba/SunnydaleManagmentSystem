using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebApplication3.Models
{
    public class ParentSignupViewModel
    {
        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required]
        public string LinkedStudentID { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, DataType(DataType.Password)]
        public string Password { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
    }

    public class ParentProfileViewModel
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [StringLength(20)]
        public string PhoneNumber { get; set; }
    }

}
