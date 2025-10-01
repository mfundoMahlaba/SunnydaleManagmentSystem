using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using WebApplication3.Models;

namespace WebApplication3.ViewModels
{
    public class StudentDashboardViewModel
    {
        public int Id { get; set; }

        [Required]
        [MinLength(13), MaxLength(13)]
        public string IDNumber { get; set; }

        [Required, StringLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }
        
        [Required]
        public string GradeLevel { get; set; }
        public string LatestDocumentStatus { get; set; }

        public bool CanPayFees { get; set; }

        public bool HasPaid { get; set; }
        public List<EnrollmentDocument> EnrollmentDocuments { get; set; } = new List<EnrollmentDocument>();

    }
}